// =============================================================================
// File: CustomWebApplicationFactory.cs
// Layer: Integration Tests
// Purpose: Configures the test web application host with seeded test data.
//          Uses WebApplicationFactory<Program> for end-to-end API testing.
// =============================================================================

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Domain.Interfaces;
using ProductCatalog.Infrastructure.Persistence;

namespace ProductCatalog.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory for integration tests.
/// Configures an isolated in-memory database per factory instance so that
/// multiple test classes can run in the same process without key conflicts.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    /// <summary>Unique name for this factory's EF Core InMemory database.</summary>
    private readonly string _dbName = $"ProductCatalogTest_{Guid.NewGuid()}";

    /// <summary>
    /// Replaces the default EF Core DbContext registration with one that uses a
    /// unique in-memory database name, preventing cross-test-class data collisions.
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the existing AppDbContext registration from DependencyInjection.cs
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Register a fresh in-memory database with a unique name per factory instance.
            // This prevents key conflicts when multiple test classes run concurrently.
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }

    /// <summary>
    /// No-op: Program.cs startup already seeds both categories and products.
    /// Kept for API compatibility but safe to call multiple times.
    /// </summary>
    public Task SeedTestDataAsync() => Task.CompletedTask;
}
