// =============================================================================
// File: product-list.component.ts
// Layer: Features — Products
// Purpose: Smart component for the product listing page.
//          Combines search, category filter, pagination, and delete with RxJS.
// =============================================================================

import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { Subject, BehaviorSubject, combineLatest } from 'rxjs';
import { switchMap, takeUntil, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { ProductService } from '../../../core/services/product.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Product } from '../../../core/models/product.model';
import { PagedResult } from '../../../core/models/api-response.model';
import { SearchBarComponent } from '../../../shared/components/search-bar/search-bar.component';
import { CategoryFilterComponent } from '../../../shared/components/category-filter/category-filter.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { ConfirmDialogComponent } from '../../../shared/components/confirm-dialog/confirm-dialog.component';

/**
 * Product list page component.
 * Displays products in a table with search, filtering, pagination, and CRUD actions.
 *
 * Uses RxJS combineLatest to reactively respond to changes in:
 * - Search query (with debounce)
 * - Selected category filter
 * - Current page number
 */
@Component({
  selector: 'app-product-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    SearchBarComponent,
    CategoryFilterComponent,
    LoadingSpinnerComponent,
    ConfirmDialogComponent
  ],
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.css']
})
export class ProductListComponent implements OnInit, OnDestroy {
  /** Current page of products */
  products: Product[] = [];

  /** Pagination metadata */
  pagedResult: PagedResult<Product> | null = null;

  /** Whether products are loading */
  loading = false;

  /** Error message to display */
  error: string | null = null;

  /** ID of product pending deletion */
  deleteTargetId: number | null = null;

  /** Name of product pending deletion (for confirmation message) */
  deleteTargetName = '';

  /** Whether the delete confirmation dialog is visible */
  showDeleteConfirm = false;

  // -----------------------------------------------------------------------
  // RxJS state subjects
  // -----------------------------------------------------------------------

  /** BehaviorSubject for the current search term */
  private searchTerm$ = new BehaviorSubject<string>('');

  /** BehaviorSubject for the selected category ID */
  private categoryId$ = new BehaviorSubject<number | null>(null);

  /** BehaviorSubject for the current page */
  private page$ = new BehaviorSubject<number>(1);

  /** Subject for cleaning up subscriptions */
  private destroy$ = new Subject<void>();

  /** Items per page */
  readonly pageSize = 10;

  constructor(
    private productService: ProductService,
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Combine search, filter, and page streams — reload products whenever any changes
    combineLatest([this.searchTerm$, this.categoryId$, this.page$]).pipe(
      debounceTime(100),         // Small debounce to batch rapid changes
      distinctUntilChanged(),
      switchMap(([searchTerm, categoryId, page]) => {
        this.loading = true;
        this.error = null;
        return this.productService.getProducts({
          searchTerm: searchTerm || undefined,
          categoryId: categoryId ?? undefined,
          page,
          pageSize: this.pageSize
        });
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: result => {
        this.pagedResult = result;
        this.products = result.items;
        this.loading = false;
      },
      error: err => {
        this.error = err.message;
        this.loading = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // -----------------------------------------------------------------------
  // Event handlers
  // -----------------------------------------------------------------------

  /** Handles search term changes from the search bar component */
  onSearch(term: string): void {
    this.searchTerm$.next(term);
    this.page$.next(1); // Reset to page 1 on new search
  }

  /** Handles category filter changes */
  onCategoryChange(categoryId: number | null): void {
    this.categoryId$.next(categoryId);
    this.page$.next(1); // Reset to page 1 on filter change
  }

  /** Navigates to the edit form for a product */
  editProduct(id: number): void {
    this.router.navigate(['/products', id, 'edit']);
  }

  /** Shows the delete confirmation dialog */
  confirmDelete(product: Product): void {
    this.deleteTargetId = product.id;
    this.deleteTargetName = product.name;
    this.showDeleteConfirm = true;
  }

  /** Executes the deletion after user confirmation */
  onDeleteConfirmed(): void {
    if (!this.deleteTargetId) return;

    const id = this.deleteTargetId;
    this.showDeleteConfirm = false;

    this.productService.deleteProduct(id).subscribe({
      next: () => {
        this.notificationService.success(`"${this.deleteTargetName}" deleted successfully.`);
        this.refreshProducts();
      },
      error: err => {
        this.notificationService.error(`Failed to delete: ${err.message}`);
      }
    });

    this.deleteTargetId = null;
  }

  /** Cancels the delete dialog */
  onDeleteCancelled(): void {
    this.showDeleteConfirm = false;
    this.deleteTargetId = null;
  }

  // -----------------------------------------------------------------------
  // Pagination
  // -----------------------------------------------------------------------

  /** Navigates to a specific page */
  goToPage(page: number): void {
    this.page$.next(page);
  }

  /** Returns an array of page numbers for the pagination controls */
  get pageNumbers(): number[] {
    if (!this.pagedResult) return [];
    return Array.from({ length: this.pagedResult.totalPages }, (_, i) => i + 1);
  }

  /** Gets the current page number */
  get currentPage(): number {
    return this.page$.value;
  }

  // -----------------------------------------------------------------------
  // Helpers
  // -----------------------------------------------------------------------

  /** Refreshes the current page of products */
  private refreshProducts(): void {
    // Trigger a reload by pushing the same values through
    const current = this.page$.value;
    this.page$.next(current);
  }
}
