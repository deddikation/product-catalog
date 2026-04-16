// =============================================================================
// File: app.component.spec.ts
// Layer: Frontend Tests
// Purpose: Unit tests for the root AppComponent.
//          Verifies the component creates successfully and wires the
//          notification service for toast display.
// =============================================================================

import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AppComponent } from './app.component';
import { NotificationService } from './core/services/notification.service';

describe('AppComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppComponent],
      providers: [
        provideRouter([]),
        NotificationService
      ]
    }).compileComponents();
  });

  /**
   * The root component should be created successfully.
   */
  it('should create the app', () => {
    const fixture = TestBed.createComponent(AppComponent);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  /**
   * On init, notifications$ should be assigned from the NotificationService.
   */
  it('should initialise notifications$ on init', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const app = fixture.componentInstance;
    expect(app.notifications$).toBeDefined();
  });

  /**
   * dismissNotification should delegate to the NotificationService.
   */
  it('should call dismiss on the NotificationService', () => {
    const fixture = TestBed.createComponent(AppComponent);
    fixture.detectChanges();
    const app = fixture.componentInstance;
    const service = TestBed.inject(NotificationService);
    spyOn(service, 'dismiss');

    app.dismissNotification('test-id');

    expect(service.dismiss).toHaveBeenCalledWith('test-id');
  });
});
