// =============================================================================
// File: confirm-dialog.component.ts
// Layer: Shared Components
// Purpose: Modal confirmation dialog for destructive actions like deletion.
//          Emits confirmed or cancelled events based on user choice.
// =============================================================================

import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Modal confirmation dialog component.
 * Displays a message asking the user to confirm or cancel an action.
 * Used for delete confirmations to prevent accidental data loss.
 */
@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (visible) {
      <div class="overlay" (click)="onCancel()">
        <div class="dialog" (click)="$event.stopPropagation()" role="dialog" aria-modal="true">
          <h3 class="dialog-title">Confirm Action</h3>
          <p class="dialog-message">{{ message }}</p>
          <div class="dialog-actions">
            <button class="btn btn-cancel" (click)="onCancel()">Cancel</button>
            <button class="btn btn-confirm" (click)="onConfirm()">Delete</button>
          </div>
        </div>
      </div>
    }
  `,
  styles: [`
    .overlay {
      position: fixed;
      top: 0; left: 0; right: 0; bottom: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      justify-content: center;
      align-items: center;
      z-index: 1000;
    }

    .dialog {
      background: white;
      padding: 24px;
      border-radius: 8px;
      max-width: 400px;
      width: 90%;
      box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
    }

    .dialog-title {
      margin: 0 0 12px;
      font-size: 18px;
      font-weight: 600;
      color: #333;
    }

    .dialog-message {
      margin: 0 0 20px;
      color: #666;
      line-height: 1.5;
    }

    .dialog-actions {
      display: flex;
      justify-content: flex-end;
      gap: 12px;
    }

    .btn {
      padding: 8px 20px;
      border: none;
      border-radius: 6px;
      font-size: 14px;
      cursor: pointer;
      transition: background 0.2s;
    }

    .btn-cancel {
      background: #f5f5f5;
      color: #333;
    }

    .btn-cancel:hover { background: #e8e8e8; }

    .btn-confirm {
      background: #e53935;
      color: white;
    }

    .btn-confirm:hover { background: #c62828; }
  `]
})
export class ConfirmDialogComponent {
  /** The confirmation message to display to the user */
  @Input() message = 'Are you sure you want to delete this item?';

  /** Controls whether the dialog is visible */
  @Input() visible = false;

  /** Emits when the user confirms the action */
  @Output() confirmed = new EventEmitter<void>();

  /** Emits when the user cancels the action */
  @Output() cancelled = new EventEmitter<void>();

  /** Handles confirm button click */
  onConfirm(): void {
    this.confirmed.emit();
  }

  /** Handles cancel button click or overlay click */
  onCancel(): void {
    this.cancelled.emit();
  }
}
