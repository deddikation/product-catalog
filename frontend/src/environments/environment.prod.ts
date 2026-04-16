// =============================================================================
// File: environment.prod.ts
// Purpose: Production environment configuration.
//          apiUrl points to the deployed backend on Azure/AWS.
//          Angular CLI swaps this file for environment.ts during `ng build --configuration production`.
// =============================================================================

export const environment = {
  production: true,
  // Update this URL after deploying the backend:
  //   Azure: https://<your-app-name>.azurewebsites.net/api
  //   AWS:   http://<your-env>.elasticbeanstalk.com/api
  apiUrl: 'https://product-catalog-api-lx.azurewebsites.net/api'
};
