// =============================================================================
// File: app.routes.ts
// Purpose: Application routing configuration.
//          Lazy loading is intentionally omitted to keep the assessment
//          straightforward; all components are eagerly imported.
// =============================================================================

import { Routes } from '@angular/router';
import { ProductListComponent } from './features/products/product-list/product-list.component';
import { ProductFormComponent } from './features/products/product-form/product-form.component';
import { CategoryManagementComponent } from './features/categories/category-management/category-management.component';

/**
 * Application route definitions.
 * - /products        → Product listing with search and filters
 * - /products/new    → Create product form
 * - /products/:id/edit → Edit product form (same component, different mode)
 * - /categories      → Category management with tree view
 */
export const routes: Routes = [
  { path: '', redirectTo: 'products', pathMatch: 'full' },
  { path: 'products', component: ProductListComponent },
  { path: 'products/new', component: ProductFormComponent },
  { path: 'products/:id/edit', component: ProductFormComponent },
  { path: 'categories', component: CategoryManagementComponent },
  { path: '**', redirectTo: 'products' }
];
