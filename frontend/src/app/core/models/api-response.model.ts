// =============================================================================
// File: api-response.model.ts
// Layer: Core — Models
// Purpose: TypeScript interfaces for the standard API response envelope
//          and pagination metadata.
// =============================================================================

/**
 * Standard API response envelope wrapping all backend responses.
 * Maps to the backend ApiResponse<T> record.
 */
export interface ApiResponse<T> {
  /** Whether the operation was successful */
  success: boolean;
  /** Response data payload (null on failure) */
  data: T | null;
  /** Error message (null on success) */
  error: string | null;
}

/**
 * Paginated result wrapper containing items and pagination metadata.
 * Maps to the backend PagedResult<T> record.
 */
export interface PagedResult<T> {
  /** The items on the current page */
  items: T[];
  /** Total number of items across all pages */
  totalCount: number;
  /** Current page number (1-based) */
  page: number;
  /** Items per page */
  pageSize: number;
  /** Total number of pages */
  totalPages: number;
  /** Whether there is a next page */
  hasNextPage: boolean;
  /** Whether there is a previous page */
  hasPreviousPage: boolean;
}

/**
 * Notification message for displaying user feedback.
 * Used by the NotificationService.
 */
export interface Notification {
  /** Unique identifier for the notification */
  id: string;
  /** The message to display */
  message: string;
  /** Type of notification (controls styling) */
  type: 'success' | 'error' | 'info';
  /** When the notification was created */
  createdAt: Date;
}
