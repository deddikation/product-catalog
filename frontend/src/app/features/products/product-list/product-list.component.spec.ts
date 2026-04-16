// =============================================================================
// File: product-list.component.spec.ts
// Layer: Frontend Tests
// Purpose: Unit tests for the ProductListComponent.
//          Mocks ProductService and CategoryService, verifies rendering
//          and user interaction flows.
// =============================================================================

import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { of, throwError } from 'rxjs';

import { ProductListComponent } from './product-list.component';
import { ProductService } from '../../../core/services/product.service';
import { NotificationService } from '../../../core/services/notification.service';
import { PagedResult } from '../../../core/models/api-response.model';
import { Product } from '../../../core/models/product.model';

/** Helper: creates a mock product */
function makeProduct(id: number, name: string): Product {
  return {
    id, name,
    description: 'Test description',
    sku: `SKU-${id}`,
    price: 9.99,
    quantity: 10,
    categoryId: 1,
    categoryName: 'Electronics',
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString()
  };
}

/** Helper: creates a mock PagedResult */
function makePagedResult(items: Product[]): PagedResult<Product> {
  return {
    items,
    totalCount: items.length,
    page: 1,
    pageSize: 10,
    totalPages: 1,
    hasNextPage: false,
    hasPreviousPage: false
  };
}

describe('ProductListComponent', () => {
  let component: ProductListComponent;
  let fixture: ComponentFixture<ProductListComponent>;
  let productServiceSpy: jasmine.SpyObj<ProductService>;
  let notificationServiceSpy: jasmine.SpyObj<NotificationService>;

  beforeEach(async () => {
    // Create spy objects for dependencies
    productServiceSpy = jasmine.createSpyObj('ProductService', [
      'getProducts', 'deleteProduct'
    ]);
    notificationServiceSpy = jasmine.createSpyObj('NotificationService', [
      'success', 'error'
    ]);

    // Default mock: return two products
    const mockProducts = [makeProduct(1, 'Laptop'), makeProduct(2, 'Mouse')];
    productServiceSpy.getProducts.and.returnValue(of(makePagedResult(mockProducts)));

    await TestBed.configureTestingModule({
      imports: [ProductListComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        { provide: ProductService, useValue: productServiceSpy },
        { provide: NotificationService, useValue: notificationServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  /**
   * Component should create successfully.
   */
  it('should create', () => {
    expect(component).toBeTruthy();
  });

  /**
   * Should call getProducts on init and populate the products array.
   * ngOnInit must be called inside the fakeAsync zone so that debounceTime
   * timers are tracked by tick().
   */
  it('should load products on init', fakeAsync(() => {
    component.ngOnDestroy();  // tear down the beforeEach subscription
    component.ngOnInit();     // re-init inside fakeAsync zone
    tick(200);                // advance past the 100ms debounce
    fixture.detectChanges();

    expect(productServiceSpy.getProducts).toHaveBeenCalled();
    expect(component.products.length).toBe(2);
    expect(component.products[0].name).toBe('Laptop');
  }));

  /**
   * Should not be loading after products are fetched.
   */
  it('should set loading to false after products load', fakeAsync(() => {
    tick(200);
    fixture.detectChanges();

    expect(component.loading).toBeFalse();
  }));

  /**
   * Should display error message when service fails.
   */
  it('should display error when getProducts fails', fakeAsync(() => {
    productServiceSpy.getProducts.and.returnValue(throwError(() => new Error('Network error')));
    component.ngOnInit();

    tick(200);
    fixture.detectChanges();

    expect(component.error).toBe('Network error');
  }));

  /**
   * Search input should reset page to 1.
   */
  it('should reset page to 1 when searching', () => {
    component.onSearch('laptop');
    expect(component.currentPage).toBe(1);
  });

  /**
   * Category filter change should reset page to 1.
   */
  it('should reset page to 1 when category changes', () => {
    component.onCategoryChange(1);
    expect(component.currentPage).toBe(1);
  });

  /**
   * confirmDelete should set the delete target and show dialog.
   */
  it('should show confirm dialog when delete is clicked', () => {
    const product = makeProduct(1, 'Laptop');
    component.confirmDelete(product);

    expect(component.showDeleteConfirm).toBeTrue();
    expect(component.deleteTargetId).toBe(1);
    expect(component.deleteTargetName).toBe('Laptop');
  });

  /**
   * onDeleteCancelled should hide the dialog.
   */
  it('should hide confirm dialog when cancelled', () => {
    component.showDeleteConfirm = true;
    component.onDeleteCancelled();

    expect(component.showDeleteConfirm).toBeFalse();
    expect(component.deleteTargetId).toBeNull();
  });

  /**
   * onDeleteConfirmed should call deleteProduct and show success notification.
   */
  it('should delete product and show success notification on confirm', fakeAsync(() => {
    productServiceSpy.deleteProduct.and.returnValue(of(undefined));
    component.deleteTargetId = 1;
    component.deleteTargetName = 'Laptop';

    component.onDeleteConfirmed();
    tick(200);

    expect(productServiceSpy.deleteProduct).toHaveBeenCalledWith(1);
    expect(notificationServiceSpy.success).toHaveBeenCalled();
  }));

  /**
   * Pagination: goToPage should update the current page.
   */
  it('should navigate to the requested page', () => {
    component.goToPage(2);
    expect(component.currentPage).toBe(2);
  });
});
