# Product Catalog Management System

LexisNexis Senior C#/Angular Developer Take-Home Assessment

A full-stack product catalog application demonstrating Clean Architecture, industry-grade testing, and documentation.

---

## Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 9 |
| Node.js | 24 |
| npm | 11 |
| Angular CLI | 19 (`npm install -g @angular/cli`) |

---

## Quick Start

### 1. Backend

```bash
cd backend

# Build all projects
dotnet build

# Run all tests (113 unit + 18 integration)
dotnet test

# Start the API (http://localhost:5000)
cd src/ProductCatalog.Api
dotnet run
```

### 2. Frontend

```bash
cd frontend

# Install dependencies
npm install

# Start the dev server (http://localhost:4200)
ng serve

# Run tests (33 specs)
ng test --watch=false --browsers=ChromeHeadless

# Production build
ng build
```

---

## Project Structure

```
Assessment/
├── backend/
│   ├── src/
│   │   ├── ProductCatalog.Domain/         # Entities, interfaces, exceptions (no dependencies)
│   │   ├── ProductCatalog.Application/    # DTOs, services, validation, LINQ extensions
│   │   ├── ProductCatalog.Infrastructure/ # EF Core, repositories, search engine, cache
│   │   └── ProductCatalog.Api/            # Controllers, middleware, serialization
│   └── tests/
│       ├── ProductCatalog.UnitTests/      # 113 unit tests (xUnit + Moq + FluentAssertions)
│       └── ProductCatalog.IntegrationTests/ # 18 integration tests (WebApplicationFactory)
└── frontend/
    └── src/app/
        ├── core/        # Models, services, interceptors
        ├── shared/      # Reusable components (search bar, category filter, dialogs)
        └── features/    # Smart page components (product list, product form, categories)
```

---

## API Endpoints

Base URL: `http://localhost:5000/api`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/products` | Paginated list with search, filter, price range |
| GET | `/products/{id}` | Get product by ID |
| POST | `/products` | Create product |
| PUT | `/products/{id}` | Update product (manual model binding) |
| DELETE | `/products/{id}` | Delete product |
| GET | `/products/search?q={query}` | Fuzzy full-text search |
| GET | `/categories` | Flat list of categories |
| GET | `/categories/tree` | Hierarchical tree (custom JSON) |
| POST | `/categories` | Create category |

### Query Parameters for GET /products

| Parameter | Type | Description |
|-----------|------|-------------|
| searchTerm | string | Filter by name (contains) |
| categoryId | int | Filter by category |
| minPrice | decimal | Minimum price |
| maxPrice | decimal | Maximum price |
| page | int | Page number (default: 1) |
| pageSize | int | Items per page (default: 10) |

---

## Running Tests

### Backend Unit Tests (113 tests)

```bash
cd backend
dotnet test tests/ProductCatalog.UnitTests/
```

Tests cover:
- `Domain/` — `IComparable<Product>` sorting
- `Application/` — Service business logic, validators, LINQ extensions
- `Infrastructure/` — Fuzzy matcher, search engine, in-memory repo, cache

### Backend Integration Tests (18 tests)

```bash
cd backend
dotnet test tests/ProductCatalog.IntegrationTests/
```

Tests cover all API endpoints end-to-end using `WebApplicationFactory<Program>`.

### Frontend Tests (33 specs)

```bash
cd frontend
ng test --watch=false --browsers=ChromeHeadless
```

Tests cover: `AppComponent`, `ProductListComponent`, `ProductFormComponent`, `ProductService`, `NotificationService`.

### All Tests

```bash
# Backend
cd backend && dotnet test

# Frontend
cd frontend && ng test --watch=false --browsers=ChromeHeadless
```

---

## Seed Data

The API starts with pre-seeded data:

- **8 categories** in a hierarchy: Electronics (Computers, Smartphones), Books (Technical, Fiction), Clothing, Home & Garden
- **18 products** across all categories, including one out-of-stock item

---

## Environment

Frontend connects to backend at `http://localhost:5000/api` (configured in `frontend/src/environments/environment.ts`).
