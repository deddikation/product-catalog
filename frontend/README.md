# Frontend

This project was generated using [Angular CLI](https://github.com/angular/angular-cli) version 19.2.11.

## Development server

To start a local development server, run:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Karma](https://karma-runner.github.io) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

Claude Code Prompt : 

Build a full-stack Product Catalog Management System using Clean Architecture.

## Backend: ASP.NET Core Web API (.NET 9)

### Architecture — 4 layers, strict inward dependency flow:
- ProductCatalog.Domain — entities, interfaces, exceptions (zero dependencies)
- ProductCatalog.Application — DTOs (C# records), services, validation, LINQ extensions
- ProductCatalog.Infrastructure — EF Core InMemory, repositories, search, cache
- ProductCatalog.Api — controllers, middleware, serialization, Program.cs

### Domain Layer
- Product entity: Id, Name, Description, SKU, Price (decimal), Quantity (int), CategoryId,
  CreatedAt, UpdatedAt, Category (nav), IsInStock (computed). Implements IComparable<Product>
  sorting Name asc → Price asc → CreatedAt desc.
- Category entity: Id, Name, Description, ParentCategoryId (nullable), SubCategories, Products.
- Generic IRepository<T>: GetByIdAsync, GetAllAsync, AddAsync, UpdateAsync, DeleteAsync, Query().
- IProductRepository extends IRepository<Product>: GetBySkuAsync, GetByIdWithCategoryAsync,
  GetAllWithCategoryAsync.
- ICategoryRepository extends IRepository<Category>: GetChildrenAsync, GetRootCategoriesAsync,
  ExistsByNameAsync.
- ISearchEngine<T>: Search(string query, IEnumerable<T>) → IEnumerable<SearchResult<T>>.
  SearchResult<T> is a record (Item, Score, FieldScores).
- ISearchCache: TryGet<T>, Set<T>, Remove, Clear.
- Domain exceptions: NotFoundException, ValidationException, DuplicateException.

### Application Layer
- All DTOs are C# 9 records: ProductDto, CreateProductDto, UpdateProductDto,
  ProductSearchRequest, CategoryDto, CategoryTreeDto(with List<CategoryTreeDto> Children),
  CreateCategoryDto, PagedResult<T>(Items, TotalCount, Page, PageSize + computed props),
  ApiResponse<T> with static Ok()/Fail() factories.
- MappingExtensions: static extension methods ToDto(), ToEntity(), ApplyTo() for Product/Category.
- ProductValidator and CategoryValidator using switch expression pattern matching:
  { Length: var len } when len < min => error
- ProductFilterExtensions on IQueryable<Product>: FilterByCategory, SearchByName, InPriceRange,
  InStock, Paginate (clamp bounds), SortByDefault (mirrors IComparable logic).
- ProductService: GetProductsAsync (LINQ chain + category lazy-load from ICategoryRepository),
  GetByIdAsync (load from repo then populate Category from ICategoryRepository),
  CreateAsync (validate → check duplicate SKU → verify category → persist),
  UpdateAsync, DeleteAsync, SearchAsync (cache-first → GetAllWithCategoryAsync →
  search engine → cache result). All mutations clear cache.
- CategoryService: GetAllAsync, GetTreeAsync (flat list → Dictionary lookup to build tree in O(n)
  → map roots to CategoryTreeDto), CreateAsync.

### Infrastructure Layer
- AppDbContext: EF InMemory, Products + Categories DbSets, Fluent API (unique SKU index,
  Product→Category FK Restrict, self-referencing Category hierarchy).
- Generic Repository<T>: FindAsync, AddAsync+SaveChangesAsync, Update+SaveChangesAsync,
  Remove+SaveChangesAsync, AsQueryable().
- ProductRepository extends Repository<Product>: case-insensitive SKU lookup,
  GetByIdWithCategoryAsync and GetAllWithCategoryAsync WITHOUT Include (categories not in EF —
  service layer populates via ICategoryRepository).
- CategoryRepository: pure in-memory, NO EF. List<Category> + Dictionary<int,Category> index,
  lock for thread safety, auto-increment _nextId (supports pre-assigned IDs for seeding).
  Registered as Singleton in DI.
- FuzzyMatcher (pure .NET BCL, no NuGet): CalculateSimilarity tries exact (1.0) →
  substring (0.9) → token overlap → Levenshtein normalized. Space-optimized 2-row DP
  for Levenshtein.
- ProductSearchEngine: weights Name=1.0, SKU=0.8, Description=0.5. Final score =
  weightedSum / sumOfWeightsForNonZeroFields (only count matched fields in denominator).
  MinScoreThreshold=0.3. Results ordered by score desc.
- DictionarySearchCache: ConcurrentDictionary<string, CacheEntry> where
  record CacheEntry(object Value, DateTime CachedAt). TTL=5min, lazy expiry on TryGet.
- DataSeeder: SeedCategoriesAsync (in-memory repo, guard if any()) seeds 8 categories
  in hierarchy (Electronics→Computers/Smartphones, Books→Technical/Fiction, Clothing,
  Home & Garden). SeedProducts (EF, guard if any()) seeds 18 products with explicit IDs.
- DependencyInjection.AddInfrastructure(string databaseName="ProductCatalog"):
  DbContext InMemory scoped, IProductRepository→ProductRepository scoped,
  ICategoryRepository→CategoryRepository singleton, ISearchEngine<Product>→ProductSearchEngine
  singleton, ISearchCache→DictionarySearchCache singleton, IProductService/ICategoryService scoped.

### API Layer
- RequestLoggingMiddleware: Stopwatch, await _next, log "HTTP METHOD PATH responded CODE in Xms".
- ExceptionHandlingMiddleware: try/catch around _next, pattern match exceptions to status codes:
  NotFoundException→404, ValidationException→400, DuplicateException→409, ArgumentException→400, _→500.
- CategoryTreeJsonConverter (JsonConverter<List<CategoryTreeDto>>): renames children→subcategories,
  adds depth (int) and childCount (int) fields, recurses WriteCategoryNode(writer, node, depth).
- ProductsController: GET list (query params: searchTerm, categoryId, minPrice, maxPrice, page,
  pageSize), GET/{id}, POST [FromBody], PUT/{id} MANUAL MODEL BINDING via
  JsonSerializer.DeserializeAsync<UpdateProductDto>(Request.Body, options), DELETE/{id},
  GET /search?q= with pattern match guard (q is null or {Length:0} → 400).
- CategoriesController: GET flat, GET tree (apply CategoryTreeJsonConverter via
  JsonSerializerOptions then return Content(json,"application/json")), POST.
- Program.cs: AddInfrastructure(), AddControllers with camelCase JSON, CORS AllowAny,
  UseMiddleware<ExceptionHandlingMiddleware> then <RequestLoggingMiddleware>,
  seed categories then products, public partial class Program {}.

### Tests (xUnit + Moq + FluentAssertions)
Unit tests (113): IComparable<Product> sorting, ProductValidator boundary values,
ProductFilterExtensions chaining, ProductService (mock all deps, test cache miss flow,
duplicate SKU, missing category), CategoryService (tree building, duplicate name),
FuzzyMatcher (known Levenshtein distances, symmetry), ProductSearchEngine (exact match,
"lptop"→"laptop" fuzzy, field weight ordering), CategoryRepository CRUD + LINQ,
DictionarySearchCache TTL expiry.

Integration tests (18, WebApplicationFactory<Program>): CustomWebApplicationFactory replaces
DbContext with unique Guid-named InMemory DB per factory instance (prevents cross-class conflicts).
SeedTestDataAsync() is a no-op (Program.cs startup seeding is sufficient).
Tests: GET list paginated, GET by id 200/404, POST 201/400, PUT manual binding 200,
DELETE+verify 404, GET search 200/400, GET categories flat/tree (verify depth+childCount fields),
POST category 201/400.

---

## Frontend: Angular 19 Standalone Components

### Structure
core/models/ — Product, CreateProduct, UpdateProduct, ProductQueryParams,
  Category, CategoryTree(depth,childCount,subcategories?), ApiResponse<T>, PagedResult<T>,
  Notification interfaces.
core/services/ — ProductService (all methods return Observable, map to extract data or throw),
  CategoryService (getCategoryTree returns CategoryTree[] directly, not wrapped),
  NotificationService (BehaviorSubject<Notification[]>, auto-dismiss 4s via setTimeout,
  crypto.randomUUID() IDs).
core/interceptors/ — errorInterceptor as HttpInterceptorFn: extract message from
  HttpErrorResponse, show notification except 404s.
shared/components/ — SearchBarComponent (FormControl + debounceTime(300) + distinctUntilChanged),
  CategoryFilterComponent (FormsModule ngModel, loads on ngOnInit),
  LoadingSpinnerComponent (@keyframes spin),
  ConfirmDialogComponent (overlay click cancels, stopPropagation on dialog).
features/products/product-list/ — BehaviorSubject searchTerm$/categoryId$/page$,
  combineLatest+debounceTime(100)+switchMap to load products, delete flow with confirmation dialog.
features/products/product-form/ — FormBuilder Validators.required/maxLength/minLength/min,
  detectMode() reads route :id param, patchValue for edit mode.
features/categories/category-management/ — ng-template #treeNode + ngTemplateOutlet for
  recursive tree, loads both tree and flat list.
app.routes.ts — '' redirects to 'products', '/products', '/products/new',
  '/products/:id/edit', '/categories', '**' → 'products'.
app.config.ts — provideHttpClient(withInterceptors([errorInterceptor])), provideRouter.
environments/environment.ts — apiUrl: 'http://localhost:5000/api'.

### Styling
styles.css — global reset, body (font, background #f4f6f8), shared .btn variants
  (primary/secondary/danger), .form-control, .badge, .card, .alert utilities.
app.component.css — navbar (dark blue #1a237e, sticky, box-shadow) and toast animations.
Each feature component has its own component CSS file.

### Tests (Jasmine + karma, 33 specs)
AppComponent: creates, notifications$ defined, dismissNotification delegates to service.
ProductListComponent (fakeAsync): re-call ngOnInit INSIDE fakeAsync to track debounce timers;
  test load, error state, search/category resets page, delete confirmation flow, goToPage.
ProductFormComponent: create mode, edit mode patchValue, invalid submit markAllAsTouched,
  each validator rule, categories loaded on init.
ProductService: HttpTestingController verifies URLs/methods/params for all 5 endpoints.
NotificationService: success/error/dismiss/clear, fakeAsync auto-dismiss tick(4100).

---

## Key constraints
- Pure .NET BCL only for fuzzy search (no NuGet packages for the algorithm)
- No AutoMapper, no MediatR
- No Swagger/OpenAPI
- All DTOs are C# records
- Nullable reference types enabled in all projects
- Comments on every class, method, and significant block
- Clean Architecture: no layer references anything outside its allowed dependencies