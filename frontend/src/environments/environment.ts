// =============================================================================
// File: environment.ts
// Purpose: Development environment configuration.
//          The API URL points to the local ASP.NET Core backend.
// =============================================================================

export const environment = {
  production: false,
  /** Base URL for the backend API (ASP.NET Core running locally) */
  apiUrl: 'http://localhost:5279/api'
};
