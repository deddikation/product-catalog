// =============================================================================
// File: loading-spinner.component.ts
// Layer: Shared Components
// Purpose: Reusable loading spinner that shows/hides based on an input flag.
// =============================================================================

import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Simple loading spinner overlay component.
 * Shown when @Input() loading is true.
 */
@Component({
  selector: 'app-loading-spinner',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (loading) {
      <div class="spinner-overlay" role="status" aria-label="Loading">
        <div class="spinner"></div>
        <span class="sr-only">Loading...</span>
      </div>
    }
  `,
  styles: [`
    .spinner-overlay {
      display: flex;
      justify-content: center;
      align-items: center;
      padding: 40px;
    }

    .spinner {
      width: 40px;
      height: 40px;
      border: 3px solid #f3f3f3;
      border-top: 3px solid #4a90e2;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
      0% { transform: rotate(0deg); }
      100% { transform: rotate(360deg); }
    }

    .sr-only {
      position: absolute;
      width: 1px;
      height: 1px;
      padding: 0;
      margin: -1px;
      overflow: hidden;
      clip: rect(0, 0, 0, 0);
      border: 0;
    }
  `]
})
export class LoadingSpinnerComponent {
  /** Controls whether the spinner is shown */
  @Input() loading = false;
}
