// =============================================================================
// File: category-filter.component.ts
// Layer: Shared Components
// Purpose: Dropdown for filtering by category. Loads categories from the API
//          and emits the selected category ID when changed.
// =============================================================================

import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CategoryService } from '../../../core/services/category.service';
import { Category } from '../../../core/models/category.model';

/**
 * Standalone dropdown component for category filtering.
 * Loads all categories from the CategoryService and emits
 * the selected category ID (or null for "All Categories").
 */
@Component({
  selector: 'app-category-filter',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="category-filter">
      <select
        [(ngModel)]="selectedCategoryId"
        (ngModelChange)="onCategoryChange($event)"
        class="category-select"
        aria-label="Filter by category"
      >
        <option [ngValue]="null">All Categories</option>
        @for (category of categories; track category.id) {
          <option [ngValue]="category.id">{{ category.name }}</option>
        }
      </select>
    </div>
  `,
  styles: [`
    .category-select {
      padding: 8px 12px;
      border: 1px solid #ddd;
      border-radius: 6px;
      font-size: 14px;
      background: white;
      cursor: pointer;
      outline: none;
      min-width: 160px;
      transition: border-color 0.2s;
    }

    .category-select:focus {
      border-color: #4a90e2;
      box-shadow: 0 0 0 2px rgba(74, 144, 226, 0.2);
    }
  `]
})
export class CategoryFilterComponent implements OnInit {
  /** Currently selected category ID (null = all categories) */
  @Input() selectedCategoryId: number | null = null;

  /** Emits the selected category ID, or null for "all" */
  @Output() categoryChange = new EventEmitter<number | null>();

  /** Flat list of categories loaded from the service */
  categories: Category[] = [];

  constructor(private categoryService: CategoryService) {}

  ngOnInit(): void {
    // Load categories when the component initialises
    this.categoryService.getCategories().subscribe({
      next: categories => this.categories = categories,
      error: () => this.categories = [] // Silently fail — filter just won't work
    });
  }

  /**
   * Handles category selection changes and emits the new value.
   *
   * @param value - The selected category ID or null
   */
  onCategoryChange(value: number | null): void {
    this.categoryChange.emit(value);
  }
}
