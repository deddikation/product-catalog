// =============================================================================
// File: product-form.component.ts
// Layer: Features — Products
// Purpose: Smart form component for creating and editing products.
//          Uses Angular Reactive Forms with validation.
//          Detects edit vs create mode from the route parameter.
// =============================================================================

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ProductService } from '../../../core/services/product.service';
import { CategoryService } from '../../../core/services/category.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Category } from '../../../core/models/category.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';

/**
 * Product form component used for both creating and editing products.
 * Determines mode (create vs edit) by checking for an 'id' route parameter.
 *
 * Reactive form fields:
 * - name: required, maxlength 200
 * - description: maxlength 2000
 * - sku: required, min 2 chars, max 50 chars
 * - price: required, min 0
 * - quantity: required, min 0
 * - categoryId: required
 */
@Component({
  selector: 'app-product-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, LoadingSpinnerComponent],
  templateUrl: './product-form.component.html',
  styleUrls: ['./product-form.component.css']
})
export class ProductFormComponent implements OnInit {
  /** The reactive form group */
  form!: FormGroup;

  /** Whether we're in edit mode (true) or create mode (false) */
  isEditMode = false;

  /** The ID of the product being edited (null in create mode) */
  productId: number | null = null;

  /** Available categories for the dropdown */
  categories: Category[] = [];

  /** Whether the form is loading initial data */
  loading = false;

  /** Whether the form submission is in progress */
  submitting = false;

  /** Error message from the last failed operation */
  error: string | null = null;

  constructor(
    private fb: FormBuilder,
    private productService: ProductService,
    private categoryService: CategoryService,
    private notificationService: NotificationService,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadCategories();
    this.detectMode();
  }

  /**
   * Builds the reactive form with all validators.
   */
  private buildForm(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(2000)]],
      sku: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(50)]],
      price: [0, [Validators.required, Validators.min(0)]],
      quantity: [0, [Validators.required, Validators.min(0)]],
      categoryId: [null, [Validators.required]]
    });
  }

  /**
   * Loads categories for the dropdown.
   */
  private loadCategories(): void {
    this.categoryService.getCategories().subscribe({
      next: cats => this.categories = cats,
      error: () => this.categories = []
    });
  }

  /**
   * Detects whether we're in edit or create mode based on the route.
   * If an 'id' param is present, loads the existing product data.
   */
  private detectMode(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEditMode = true;
      this.productId = +id;
      this.loadProduct(this.productId);
    }
  }

  /**
   * Loads an existing product and patches the form.
   *
   * @param id - The product ID to load
   */
  private loadProduct(id: number): void {
    this.loading = true;
    this.productService.getProduct(id).subscribe({
      next: product => {
        this.form.patchValue({
          name: product.name,
          description: product.description,
          sku: product.sku,
          price: product.price,
          quantity: product.quantity,
          categoryId: product.categoryId
        });
        this.loading = false;
      },
      error: err => {
        this.error = err.message;
        this.loading = false;
        this.notificationService.error('Failed to load product for editing.');
      }
    });
  }

  /**
   * Handles form submission for both create and edit modes.
   * Shows validation errors if form is invalid.
   */
  onSubmit(): void {
    if (this.form.invalid) {
      // Mark all fields as touched to display validation messages
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;
    this.error = null;

    const value = this.form.value;

    if (this.isEditMode && this.productId) {
      // Update existing product
      this.productService.updateProduct(this.productId, value).subscribe({
        next: () => {
          this.notificationService.success('Product updated successfully.');
          this.router.navigate(['/products']);
        },
        error: err => {
          this.error = err.message;
          this.submitting = false;
        }
      });
    } else {
      // Create new product
      this.productService.createProduct(value).subscribe({
        next: () => {
          this.notificationService.success('Product created successfully.');
          this.router.navigate(['/products']);
        },
        error: err => {
          this.error = err.message;
          this.submitting = false;
        }
      });
    }
  }

  // -------------------------------------------------------------------------
  // Convenience getters for template form control access
  // -------------------------------------------------------------------------

  get nameControl() { return this.form.get('name'); }
  get descriptionControl() { return this.form.get('description'); }
  get skuControl() { return this.form.get('sku'); }
  get priceControl() { return this.form.get('price'); }
  get quantityControl() { return this.form.get('quantity'); }
  get categoryIdControl() { return this.form.get('categoryId'); }

  /** Returns the page title based on mode */
  get pageTitle(): string {
    return this.isEditMode ? 'Edit Product' : 'Add Product';
  }
}
