// =============================================================================
// File: category-management.component.ts
// Layer: Features — Categories
// Purpose: Category management page with tree view and add form.
// =============================================================================

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CategoryService } from '../../../core/services/category.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Category, CategoryTree } from '../../../core/models/category.model';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';

/**
 * Category management page.
 * Displays the category hierarchy as a tree and provides a form to add new categories.
 */
@Component({
  selector: 'app-category-management',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LoadingSpinnerComponent],
  templateUrl: './category-management.component.html',
  styleUrls: ['./category-management.component.css']
})
export class CategoryManagementComponent implements OnInit {
  /** Hierarchical tree of categories */
  categoryTree: CategoryTree[] = [];

  /** Flat list of categories for the parent dropdown */
  flatCategories: Category[] = [];

  /** Form for adding a new category */
  form!: FormGroup;

  /** Whether the tree is loading */
  loading = false;

  /** Whether the form is being submitted */
  submitting = false;

  /** Error message */
  error: string | null = null;

  constructor(
    private categoryService: CategoryService,
    private notificationService: NotificationService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadData();
  }

  /** Builds the category creation form */
  private buildForm(): void {
    this.form = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['', [Validators.maxLength(500)]],
      parentCategoryId: [null]
    });
  }

  /** Loads both the tree and flat category list */
  private loadData(): void {
    this.loading = true;

    // Load tree for display
    this.categoryService.getCategoryTree().subscribe({
      next: tree => {
        this.categoryTree = tree;
        this.loading = false;
      },
      error: err => {
        this.error = err.message;
        this.loading = false;
      }
    });

    // Load flat list for parent dropdown
    this.categoryService.getCategories().subscribe({
      next: cats => this.flatCategories = cats,
      error: () => {}
    });
  }

  /** Handles form submission */
  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting = true;
    const value = this.form.value;

    this.categoryService.createCategory({
      name: value.name,
      description: value.description,
      parentCategoryId: value.parentCategoryId ?? null
    }).subscribe({
      next: () => {
        this.notificationService.success(`Category "${value.name}" created successfully.`);
        this.form.reset();
        this.loadData(); // Refresh tree
        this.submitting = false;
      },
      error: err => {
        this.notificationService.error(err.message);
        this.submitting = false;
      }
    });
  }

  get nameControl() { return this.form.get('name'); }
}
