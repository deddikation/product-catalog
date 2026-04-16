// =============================================================================
// File: IProductRepository.cs
// Layer: Domain
// Purpose: Extends the generic repository with product-specific data access
//          operations such as paged queries and SKU lookups.
// =============================================================================

using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Domain.Interfaces;

/// <summary>
/// Product-specific repository interface extending the generic IRepository.
/// Adds operations for pagination, search, and SKU-based lookups.
/// </summary>
public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Retrieves a product by its unique SKU identifier.
    /// </summary>
    /// <param name="sku">The Stock Keeping Unit to search for.</param>
    /// <returns>The matching product, or null if not found.</returns>
    Task<Product?> GetBySkuAsync(string sku);

    /// <summary>
    /// Retrieves a product by ID including its category navigation property.
    /// </summary>
    /// <param name="id">The product's unique identifier.</param>
    /// <returns>The product with category loaded, or null if not found.</returns>
    Task<Product?> GetByIdWithCategoryAsync(int id);

    /// <summary>
    /// Retrieves all products with their category navigation property loaded.
    /// </summary>
    /// <returns>An enumerable of products with categories.</returns>
    Task<IEnumerable<Product>> GetAllWithCategoryAsync();
}
