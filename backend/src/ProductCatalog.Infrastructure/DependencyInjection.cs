// =============================================================================
// File: DependencyInjection.cs
// Layer: Infrastructure
// Purpose: Registers all infrastructure services in the DI container (req 13).
//          Demonstrates proper DI pattern for the ProductSearchEngine and
//          all repositories and caching services.
// =============================================================================

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Application.Services;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;
using ProductCatalog.Infrastructure.Caching;
using ProductCatalog.Infrastructure.InMemory;
using ProductCatalog.Infrastructure.Persistence;
using ProductCatalog.Infrastructure.Persistence.Repositories;
using ProductCatalog.Infrastructure.Search;

namespace ProductCatalog.Infrastructure;

/// <summary>
/// Extension methods for registering infrastructure layer services in the DI container.
/// Follows the clean architecture pattern where each layer provides its own registration.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all infrastructure services including:
    /// - EF Core InMemory database context
    /// - Repository implementations (EF-based and in-memory)
    /// - ProductSearchEngine (req 13 — DI registered as singleton)
    /// - Search cache (Dictionary-based)
    /// - Application services
    /// </summary>
    /// <param name="services">The service collection to register with.</param>
    /// <param name="databaseName">Name for the InMemory database instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string databaseName = "ProductCatalog")
    {
        // =====================================================================
        // Database Context — EF Core with InMemory provider
        // =====================================================================
        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(databaseName));

        // =====================================================================
        // Repositories
        // =====================================================================

        // Product repository — EF Core backed (extends generic Repository<T>)
        services.AddScoped<IProductRepository, ProductRepository>();

        // Category repository — pure in-memory using List + Dictionary (req 2)
        // Registered as Singleton since it maintains its own state without EF
        services.AddSingleton<ICategoryRepository, CategoryRepository>();

        // =====================================================================
        // Search Engine — registered as Singleton for reusability (req 13)
        // ProductSearchEngine uses only .NET BCL — no external NuGet packages
        // =====================================================================
        services.AddSingleton<ISearchEngine<Product>, ProductSearchEngine>();

        // =====================================================================
        // Caching — Dictionary-based search cache (req 8)
        // Singleton to maintain cache state across requests
        // =====================================================================
        services.AddSingleton<ISearchCache, DictionarySearchCache>();

        // =====================================================================
        // Application Services
        // =====================================================================
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();

        return services;
    }
}
