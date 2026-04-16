// =============================================================================
// File: error.interceptor.ts
// Layer: Core — Interceptors
// Purpose: Global HTTP error interceptor that converts HTTP errors into
//          user-friendly notifications and standardized error objects.
// =============================================================================

import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { NotificationService } from '../services/notification.service';

/**
 * Functional HTTP interceptor for global error handling.
 * Catches all HTTP errors and:
 * 1. Maps HTTP status codes to user-friendly messages
 * 2. Shows toast notifications via NotificationService
 * 3. Re-throws the error for component-level handling if needed
 *
 * Error mapping:
 * - 400 Bad Request → Validation error message from API
 * - 404 Not Found → "Resource not found"
 * - 409 Conflict → Duplicate/conflict message from API
 * - 500+ Server Error → "Server error" message
 * - Network errors → "Cannot connect to server"
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notificationService = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let userMessage: string;

      if (error.status === 0) {
        // Network error (no response from server)
        userMessage = 'Cannot connect to the server. Please check your connection.';
      } else {
        // Extract the error message from the API response if available
        const apiError = error.error?.error;
        userMessage = apiError ?? getDefaultMessage(error.status);
      }

      // Show error notification — but NOT for 404 on navigation
      // (components handle their own not-found states)
      if (error.status !== 404) {
        notificationService.error(userMessage);
      }

      // Re-throw with the user-friendly message for component handling
      return throwError(() => new Error(userMessage));
    })
  );
};

/**
 * Returns a default error message based on HTTP status code.
 *
 * @param status - The HTTP status code
 * @returns A user-friendly error message
 */
function getDefaultMessage(status: number): string {
  switch (true) {
    case status === 400: return 'Invalid request. Please check your input.';
    case status === 404: return 'The requested resource was not found.';
    case status === 409: return 'This record conflicts with an existing entry.';
    case status >= 500: return 'A server error occurred. Please try again later.';
    default: return `An error occurred (${status}). Please try again.`;
  }
}
