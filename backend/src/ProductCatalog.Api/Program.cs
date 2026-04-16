// =============================================================================
// File: Program.cs
// Layer: API (Presentation) — Application entry point
// Purpose: Configures the ASP.NET Core application including:
//          - DI container registration (req 13)
//          - Custom middleware pipeline (req 7)
//          - CORS for Angular frontend
//          - Database seeding with sample data
// =============================================================================

using ProductCatalog.Api.Middleware;
using ProductCatalog.Domain.Interfaces;
using ProductCatalog.Infrastructure;
using ProductCatalog.Infrastructure.Persistence;

// =========================================================================
// Build Phase — Configure services and the DI container
// =========================================================================
var builder = WebApplication.CreateBuilder(args);

// Register all infrastructure services (repos, search engine, cache, services)
// This single call registers everything following clean architecture DI pattern (req 13)
builder.Services.AddInfrastructure();

// Add controller support with camelCase JSON serialization
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy =
            System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Configure CORS to allow the Angular frontend (http://localhost:4200)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// =========================================================================
// App Phase — Configure the HTTP request pipeline
// =========================================================================
var app = builder.Build();

// Custom middleware — registered in correct order (req 7):
// 1. Exception handling wraps everything (outermost)
// 2. Request logging captures timing for all requests
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

// Enable CORS before routing
app.UseCors();

// Map controller routes
app.MapControllers();

// =========================================================================
// Seed Phase — Populate initial data
// =========================================================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Seed categories into the in-memory repository
    var categoryRepo = services.GetRequiredService<ICategoryRepository>();
    await DataSeeder.SeedCategoriesAsync(categoryRepo);

    // Seed products into the EF Core InMemory database
    var dbContext = services.GetRequiredService<AppDbContext>();
    DataSeeder.SeedProducts(dbContext);
}

// =========================================================================
// Run the application
// =========================================================================
app.Run();

// Make Program class accessible for integration tests using WebApplicationFactory
/// <summary>
/// Partial class declaration to make the auto-generated Program class
/// accessible for integration testing with WebApplicationFactory.
/// </summary>
public partial class Program { }
