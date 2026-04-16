// =============================================================================
// File: notification.service.ts
// Layer: Core — Services
// Purpose: Manages application-level toast notifications using RxJS BehaviorSubject.
//          Notifications auto-dismiss after a configurable timeout.
// =============================================================================

import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { Notification } from '../models/api-response.model';

/**
 * Service for displaying toast notifications to the user.
 * Uses a BehaviorSubject to maintain reactive state that components can subscribe to.
 * Notifications auto-dismiss after 4 seconds.
 */
@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  /** BehaviorSubject holding the current list of active notifications */
  private notificationsSubject = new BehaviorSubject<Notification[]>([]);

  /** Public observable for components to subscribe to notification changes */
  notifications$: Observable<Notification[]> = this.notificationsSubject.asObservable();

  /** Auto-dismiss timeout in milliseconds */
  private readonly dismissTimeout = 4000;

  /**
   * Shows a success notification with the given message.
   *
   * @param message - The message to display
   */
  success(message: string): void {
    this.addNotification(message, 'success');
  }

  /**
   * Shows an error notification with the given message.
   *
   * @param message - The error message to display
   */
  error(message: string): void {
    this.addNotification(message, 'error');
  }

  /**
   * Shows an informational notification with the given message.
   *
   * @param message - The info message to display
   */
  info(message: string): void {
    this.addNotification(message, 'info');
  }

  /**
   * Removes a specific notification by its ID.
   *
   * @param id - The ID of the notification to remove
   */
  dismiss(id: string): void {
    const current = this.notificationsSubject.value;
    this.notificationsSubject.next(current.filter(n => n.id !== id));
  }

  /**
   * Clears all active notifications.
   */
  clear(): void {
    this.notificationsSubject.next([]);
  }

  /**
   * Internal helper to create and add a notification.
   * Automatically schedules removal after the dismiss timeout.
   *
   * @param message - The notification message
   * @param type - The notification type (success/error/info)
   */
  private addNotification(message: string, type: 'success' | 'error' | 'info'): void {
    const notification: Notification = {
      id: crypto.randomUUID(),
      message,
      type,
      createdAt: new Date()
    };

    // Add notification to the current list
    const current = this.notificationsSubject.value;
    this.notificationsSubject.next([...current, notification]);

    // Auto-dismiss after timeout
    setTimeout(() => this.dismiss(notification.id), this.dismissTimeout);
  }
}
