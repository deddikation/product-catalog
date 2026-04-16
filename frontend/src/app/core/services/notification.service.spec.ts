// =============================================================================
// File: notification.service.spec.ts
// Layer: Frontend Tests
// Purpose: Unit tests for the NotificationService.
//          Validates add/dismiss/clear and auto-dismiss behaviour.
// =============================================================================

import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { NotificationService } from './notification.service';

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [NotificationService] });
    service = TestBed.inject(NotificationService);
  });

  /**
   * Service should be created.
   */
  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  /**
   * success() should add a success notification.
   */
  it('should add a success notification', done => {
    service.success('Operation successful');
    service.notifications$.subscribe(notifications => {
      const successNote = notifications.find(n => n.type === 'success');
      expect(successNote).toBeDefined();
      expect(successNote?.message).toBe('Operation successful');
      done();
    });
  });

  /**
   * error() should add an error notification.
   */
  it('should add an error notification', done => {
    service.error('Something went wrong');
    service.notifications$.subscribe(notifications => {
      const errorNote = notifications.find(n => n.type === 'error');
      expect(errorNote).toBeDefined();
      expect(errorNote?.message).toBe('Something went wrong');
      done();
    });
  });

  /**
   * dismiss() should remove a specific notification by ID.
   */
  it('should dismiss a notification by id', done => {
    service.success('To be dismissed');
    service.notifications$.subscribe(notifications => {
      if (notifications.length > 0) {
        const id = notifications[0].id;
        service.dismiss(id);
        service.notifications$.subscribe(updated => {
          expect(updated.find(n => n.id === id)).toBeUndefined();
          done();
        });
      }
    });
  });

  /**
   * clear() should remove all notifications.
   */
  it('should clear all notifications', done => {
    service.success('Note 1');
    service.error('Note 2');
    service.clear();
    service.notifications$.subscribe(notifications => {
      expect(notifications.length).toBe(0);
      done();
    });
  });

  /**
   * Notifications should auto-dismiss after 4 seconds.
   */
  it('should auto-dismiss after timeout', fakeAsync(() => {
    service.success('Auto-dismiss me');

    let count = 0;
    service.notifications$.subscribe(n => count = n.length);

    expect(count).toBe(1);

    // Advance time past the 4000ms dismiss timeout
    tick(4100);

    expect(count).toBe(0);
  }));
});
