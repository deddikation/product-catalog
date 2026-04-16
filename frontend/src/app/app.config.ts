// =============================================================================
// File: app.config.ts
// Purpose: Application-level providers configuration.
//          Registers HttpClient, router, and the global error interceptor.
// =============================================================================

import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { errorInterceptor } from './core/interceptors/error.interceptor';

/**
 * Root application configuration.
 * - provideRouter: registers all application routes
 * - provideHttpClient: enables HttpClient with the global error interceptor
 */
export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    // Register HttpClient with the global error interceptor
    provideHttpClient(withInterceptors([errorInterceptor]))
  ]
};
