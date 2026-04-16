// =============================================================================
// File: product-form.component.spec.ts
// Layer: Frontend Tests
// Purpose: Unit tests for the ProductFormComponent.
//          Verifies form validation, create/edit mode detection, and submission.
// =============================================================================

import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter, ActivatedRoute } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { of } from 'rxjs';

import { ProductFormComponent } from './product-form.component';
import { ProductService } from '../../../core/services/product.service';
import { CategoryService } from '../../../core/services/category.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Product } from '../../../core/models/product.model';

describe('ProductFormComponent', () => {
  let component: ProductFormComponent;
  let fixture: ComponentFixture<ProductFormComponent>;
  let productServiceSpy: jasmine.SpyObj<ProductService>;
  let categoryServiceSpy: jasmine.SpyObj<CategoryService>;
  let notificationServiceSpy: jasmine.SpyObj<NotificationService>;

  const mockCategories = [
    { id: 1, name: 'Electronics', description: 'Devices', parentCategoryId: null }
  ];

  beforeEach(async () => {
    productServiceSpy = jasmine.createSpyObj('ProductService', [
      'getProduct', 'createProduct', 'updateProduct'
    ]);
    categoryServiceSpy = jasmine.createSpyObj('CategoryService', ['getCategories']);
    notificationServiceSpy = jasmine.createSpyObj('NotificationService', ['success', 'error']);

    categoryServiceSpy.getCategories.and.returnValue(of(mockCategories));

    await TestBed.configureTestingModule({
      imports: [ProductFormComponent],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        { provide: ProductService, useValue: productServiceSpy },
        { provide: CategoryService, useValue: categoryServiceSpy },
        { provide: NotificationService, useValue: notificationServiceSpy },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: { get: () => null } } }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(ProductFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  /**
   * Component should create in create mode (no route id).
   */
  it('should create in create mode when no id param', () => {
    expect(component).toBeTruthy();
    expect(component.isEditMode).toBeFalse();
    expect(component.pageTitle).toBe('Add Product');
  });

  /**
   * Form should be invalid when empty.
   */
  it('should have invalid form when empty', () => {
    expect(component.form.valid).toBeFalse();
  });

  /**
   * Form should be valid with all required fields filled.
   */
  it('should have valid form when all required fields filled', () => {
    component.form.patchValue({
      name: 'Test Product',
      description: 'A description',
      sku: 'SKU-001',
      price: 9.99,
      quantity: 5,
      categoryId: 1
    });
    expect(component.form.valid).toBeTrue();
  });

  /**
   * Name field validation: required.
   */
  it('should show required error for empty name', () => {
    const control = component.nameControl!;
    control.setValue('');
    control.markAsTouched();
    expect(control.errors?.['required']).toBeTruthy();
  });

  /**
   * Price validation: must be >= 0.
   */
  it('should show error for negative price', () => {
    const control = component.priceControl!;
    control.setValue(-1);
    control.markAsTouched();
    expect(control.errors?.['min']).toBeTruthy();
  });

  /**
   * SKU validation: minimum 2 characters.
   */
  it('should show error for sku shorter than 2 chars', () => {
    const control = component.skuControl!;
    control.setValue('A');
    control.markAsTouched();
    expect(control.errors?.['minlength']).toBeTruthy();
  });

  /**
   * Submitting invalid form should mark all as touched but not call service.
   */
  it('should mark all as touched when submitting invalid form', () => {
    component.onSubmit();
    expect(component.nameControl?.touched).toBeTrue();
    expect(productServiceSpy.createProduct).not.toHaveBeenCalled();
  });

  /**
   * Categories should be loaded on init.
   */
  it('should load categories on init', () => {
    expect(component.categories.length).toBe(1);
    expect(component.categories[0].name).toBe('Electronics');
  });
});
