# Solution Notes — Product Catalog Management System

## Architecture Overview

The solution uses **Clean Architecture** on both the backend and frontend, with dependency flow strictly pointing inward (toward the domain).

---

## Backend Design Decisions

### Layer Structure

| Layer | Project | Depends On |
|-------|---------|-----------|
| Domain | `ProductCatalog.Domain` | Nothing |
| Application | `ProductCatalog.Application` | Domain |
| Infrastructure | `ProductCatalog.Infrastructure` | Domain + Application |
| Presentation | `ProductCatalog.Api` | All |

This ensures business logic is never polluted by framework concerns. The `ProductService` knows nothing about HTTP, EF Core, or JSON.

---

### Requirement Coverage

#### 1. Generic Repository Pattern (`IRepository<T>`)

`Domain/Interfaces/IRepository.cs` defines a generic interface with `GetByIdAsync`, `GetAllAsync`, `AddAsync`, `UpdateAsync`, `DeleteAsync`, `Query()`. The `Infrastructure/Persistence/Repositories/Repository.cs` provides the EF Core base implementation. Concrete repositories (`ProductRepository`) extend it and add entity-specific methods.

#### 2. In-Memory Collections (No EF for Categories)

`Infrastructure/InMemory/CategoryRepository.cs` uses `List<Category>` + `Dictionary<int, Category>` with `lock` for thread safety. Zero EF dependencies. This is registered as a **Singleton** in DI because it owns its state — scoped registration would lose data between requests.

#### 3. Custom LINQ Extension Methods

`Application/Extensions/ProductFilterExtensions.cs` provides chainable extensions on `IQueryable<Product>`:
- `FilterByCategory` — skip if null
- `SearchByName` — case-insensitive `Contains`
- `InPriceRange` — lower/upper bound filtering
- `InStock` — quantity > 0
- `Paginate` — clamps page/pageSize to valid ranges
- `SortByDefault` — mirrors `IComparable<Product>` logic for EF

#### 4. Record Types for DTOs

All types in `Application/DTOs/` are C# 9 `record` types. Records provide:
- Structural equality (two DTOs with same values are equal)
- Immutability (init-only properties)
- Concise syntax (`record ProductDto(int Id, string Name, ...)`)

`PagedResult<T>` uses computed properties (`TotalPages`, `HasNextPage`, `HasPreviousPage`) derived from the constructor parameters.

#### 5. Pattern Matching for Validation

`Application/Validation/ProductValidator.cs` uses switch expressions with property and relational patterns:

```csharp
var error = value switch {
    null or "" when minLength > 0 => $"{fieldName} is required.",
    { Length: var len } when len < minLength => $"...",
    { Length: var len } when len > maxLength => $"...",
    _ => null
};
```

`ExceptionHandlingMiddleware` uses pattern matching to map domain exceptions to HTTP status codes.

#### 6. Nullable Reference Types

All `.csproj` files enable `<Nullable>enable</Nullable>`. All nullable properties use `?` annotations. The compiler enforces null-safety across the solution.

#### 7. Custom Middleware (From Scratch)

Both `RequestLoggingMiddleware` and `ExceptionHandlingMiddleware` are implemented as classes taking `RequestDelegate` in their constructor — no `IMiddleware` interface, no built-in helpers. They call `await _next(context)` directly inside a try/catch for exception handling.

#### 8. Dictionary-Based Caching

`Infrastructure/Caching/DictionarySearchCache.cs` uses `ConcurrentDictionary<string, CacheEntry>` where `CacheEntry` is a `record` with a `CachedAt` timestamp. TTL is checked lazily on `TryGet` — expired entries are removed and treated as cache misses. This avoids a background sweep thread.

#### 9. Category Tree Hierarchy

`CategoryRepository` stores categories flat. `CategoryService.GetTreeAsync` builds the tree in O(n) using a Dictionary lookup:
1. First pass: index all categories by ID
2. Second pass: for each category with a parent, add it to parent's `Children` list
3. Final pass: collect root categories (no parent) as tree roots

#### 10. `IComparable<Product>`

`Product.CompareTo` sorts by Name ascending → Price ascending → CreatedAt descending (newest first). This gives a stable, predictable default sort. The `SortByDefault` LINQ extension mirrors this logic as an `OrderBy`/`ThenBy`/`ThenByDescending` chain for EF queries.

#### 11. Manual Model Binding on PUT

The `PUT /api/products/{id}` action reads the request body manually:

```csharp
dto = await JsonSerializer.DeserializeAsync<UpdateProductDto>(Request.Body, jsonOptions);
```

This contrasts with standard `[FromBody]` used on POST and demonstrates understanding of how ASP.NET Core model binding works at a lower level.

#### 12. Custom JSON Serialization

`CategoryTreeJsonConverter` (a `JsonConverter<List<CategoryTreeDto>>`) adds two computed fields to every tree node:
- `depth` — how deep in the tree (0 = root)
- `childCount` — number of direct children

It also renames the children array from `children` to `subcategories`. These fields are computed during serialization without modifying the `CategoryTreeDto` record.

#### 13. DI Registration for ProductSearchEngine

`Infrastructure/DependencyInjection.cs` provides an `AddInfrastructure()` extension method. `ProductSearchEngine` is registered as **Singleton** (one instance reused across all requests — safe because it's stateless). `DictionarySearchCache` is also Singleton (it must persist state across requests). `ProductService` and `CategoryService` are Scoped.

---

## Fuzzy Search Engine

`Infrastructure/Search/ProductSearchEngine.cs` is the core algorithmic challenge. It implements weighted multi-field fuzzy matching using only .NET BCL.

**Scoring algorithm:**
1. Extract `Name` (weight 1.0), `SKU` (weight 0.8), `Description` (weight 0.5) from each product
2. For each field, compute similarity using `FuzzyMatcher.CalculateSimilarity`
3. `finalScore = Σ(fieldScore × weight) / Σ(weights for matched fields only)`
   - Denominator uses only fields that scored > 0, preventing unrelated fields from diluting a strong single-field match

**FuzzyMatcher strategies (in priority order):**
1. Exact match → 1.0
2. Substring match → 0.9 (e.g., "laptop" in "laptop pro 15")
3. Token overlap → weighted by shared word count
4. Levenshtein distance → normalized by max string length (e.g., "lptop"→"laptop" ≈ 0.83)

**Levenshtein implementation:** Space-optimized 2-row DP algorithm, O(m × n) time, O(min(m, n)) space.

---

## Frontend Design Decisions

### Standalone Components (Angular 19)

All components use `standalone: true` with explicit `imports: []`. No NgModules anywhere. This is Angular's modern recommended approach — each component is self-describing.

### RxJS Reactive Data Loading

`ProductListComponent` uses `combineLatest([searchTerm$, categoryId$, page$])` with `debounceTime(100)` + `switchMap`. This means:
- Any change to search, filter, or page triggers a reload
- `switchMap` cancels in-flight requests if parameters change rapidly
- `debounceTime(100)` batches simultaneous changes (e.g., changing category also resets page — both changes fire in one HTTP call)

### Functional HTTP Interceptor

`errorInterceptor` is an `HttpInterceptorFn` (Angular 14+ functional style) registered via `provideHttpClient(withInterceptors([errorInterceptor]))`. It extracts the error message from `HttpErrorResponse` and shows a notification, except for 404s (those are handled individually by components).

### Notification System

`NotificationService` maintains a `BehaviorSubject<Notification[]>`. Notifications are auto-dismissed after 4 seconds via `setTimeout`. Each notification has a `crypto.randomUUID()` ID for targeted dismissal. The `AppComponent` subscribes to `notifications$` and renders toasts in the app shell.

---

## Testing Strategy

### Backend Unit Tests (113 tests)

- **Domain:** `IComparable<Product>` — sort stability, null comparison, case-insensitivity
- **Application services:** Moq mocks for all dependencies; test both happy path and every error condition (not found, duplicate SKU, invalid input, bad category)
- **Validators:** Every rule tested at boundary values
- **LINQ extensions:** Each extension in isolation and in composition
- **Search engine:** Exact match, fuzzy "lptop"→"laptop", field weight comparison, empty queries, result ordering
- **Fuzzy matcher:** Known Levenshtein distances, symmetry property, similarity thresholds
- **Category repository:** Full CRUD, tree queries, thread safety with pre-assigned IDs
- **Cache:** Hit/miss, TTL expiry, type safety

### Backend Integration Tests (18 tests)

`CustomWebApplicationFactory` wraps `WebApplicationFactory<Program>` with a unique in-memory database name per test class (preventing cross-class data conflicts). Tests cover the full HTTP pipeline including middleware, controller binding, and service interaction.

### Frontend Tests (33 specs)

- **Services:** `HttpTestingController` verifies correct URLs, methods, and query parameters
- **Components:** Jasmine spies for all service dependencies; tests for form validation rules, component state machine (loading/error/data), and user interactions (search resets page, delete confirmation flow)
- **`fakeAsync`/`tick`:** Used for debounce testing; `ngOnInit` is called explicitly inside `fakeAsync` blocks to ensure RxJS timers are tracked by `tick()`

---

## Trade-offs and Constraints

| Decision | Rationale |
|----------|-----------|
| EF Core InMemory (not SQLite) | Meets assessment requirement for in-memory storage; simpler setup |
| Categories NOT in EF | Assessment explicitly requires in-memory collections for categories (req 2) |
| No AutoMapper | Explicit mapping extensions (`MappingExtensions.cs`) keep the mapping visible and debuggable |
| No MediatR | Overkill for a CRUD API; adds indirection without benefit at this scale |
| No Swagger/OpenAPI | Not part of assessment requirements |
| `--no-openapi` in `dotnet new webapi` | Keeps the project lean; API is documented here and in controller comments |
