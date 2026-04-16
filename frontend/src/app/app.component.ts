// =============================================================================
// File: app.component.ts
// Layer: App Shell
// Purpose: Root application component. Renders the nav bar, notification
//          toasts, and the router outlet for page components.
// =============================================================================

import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { NotificationService } from './core/services/notification.service';
import { Notification } from './core/models/api-response.model';
import { Observable } from 'rxjs';

/**
 * Root application component.
 * Provides the navigation bar and a notification toast container.
 * All page content is rendered via <router-outlet>.
 */
@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  /** Stream of active notifications from the notification service */
  notifications$!: Observable<Notification[]>;

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.notifications$ = this.notificationService.notifications$;
  }

  /**
   * Dismisses a notification by its ID.
   * @param id - The notification ID to dismiss
   */
  dismissNotification(id: string): void {
    this.notificationService.dismiss(id);
  }
}
