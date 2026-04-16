// =============================================================================
// File: search-bar.component.ts
// Layer: Shared Components
// Purpose: Reusable search bar with debounced input.
//          Emits search queries with 300ms debounce to prevent excessive API calls.
// =============================================================================

import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged, takeUntil } from 'rxjs/operators';
import { CommonModule } from '@angular/common';

/**
 * Reusable search bar component with RxJS debouncing.
 * Emits the search term after 300ms of user inactivity to avoid
 * triggering API calls on every keystroke.
 */
@Component({
  selector: 'app-search-bar',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="search-bar">
      <input
        type="text"
        [formControl]="searchControl"
        [placeholder]="placeholder"
        class="search-input"
        aria-label="Search"
      />
      <span class="search-icon">🔍</span>
    </div>
  `,
  styles: [`
    .search-bar {
      position: relative;
      display: flex;
      align-items: center;
    }

    .search-input {
      width: 100%;
      padding: 8px 36px 8px 12px;
      border: 1px solid #ddd;
      border-radius: 6px;
      font-size: 14px;
      outline: none;
      transition: border-color 0.2s;
    }

    .search-input:focus {
      border-color: #4a90e2;
      box-shadow: 0 0 0 2px rgba(74, 144, 226, 0.2);
    }

    .search-icon {
      position: absolute;
      right: 10px;
      font-size: 14px;
      pointer-events: none;
    }
  `]
})
export class SearchBarComponent implements OnInit, OnDestroy {
  /** Placeholder text for the search input */
  @Input() placeholder = 'Search products...';

  /** Initial value for the search input */
  @Input() initialValue = '';

  /** Emits the debounced search term when the user types */
  @Output() searchChange = new EventEmitter<string>();

  /** Reactive form control for the search input */
  searchControl = new FormControl('');

  /** Subject used to clean up subscriptions on component destroy */
  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    // Set initial value if provided
    if (this.initialValue) {
      this.searchControl.setValue(this.initialValue);
    }

    // Subscribe to value changes with debounce
    this.searchControl.valueChanges.pipe(
      debounceTime(300),          // Wait 300ms after last keystroke
      distinctUntilChanged(),      // Only emit if value actually changed
      takeUntil(this.destroy$)     // Clean up on component destroy
    ).subscribe(value => {
      this.searchChange.emit(value ?? '');
    });
  }

  ngOnDestroy(): void {
    // Signal all subscriptions to complete
    this.destroy$.next();
    this.destroy$.complete();
  }
}
