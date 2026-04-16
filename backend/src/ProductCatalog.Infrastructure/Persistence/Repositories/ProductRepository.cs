// =============================================================================
// File: ProductRepository.cs
// Layer: Infrastructure
// Purpose: Concrete product repository extending the generic EF Core base class.
//          Adds product-specific queries like SKU lookup and eager loading.
// =============================================================================

using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core-backed product repository. Extends the generic Repository&lt;Product&gt;
/// with product-specific data access operations.
/// </summary>
public class ProductRepository : Repository<Product>, IProductRepository
{
    /// <summary>
    /// Constructs the ProductRepository with the provided DbContext.
    /// </summary>
    /// <param name="context">The EF Core database context.</param>
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    /// <inheritdoc />
    public async Task<Product?> GetBySkuAsync(string sku)
    {
        // Case-insensitive SKU lookup
        return await _dbSet.FirstOrDefaultAsync(
            p => p.SKU.ToLower() == sku.ToLower());
    }

    /// <inheritdoc />
    public async Task<Product?> GetByIdWithCategoryAsync(int id)
    {
        // Return the product; Category navigation is populated by the service layer
        // using ICategoryRepository (pure in-memory) rather than EF Include,
        // because categories are not stored in the EF database.
        return await _dbSet.FirstOrDefaultAsync(p => p.Id == id);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Product>> GetAllWithCategoryAsync()
    {
        // Return all products; Category navigation is populated by the service layer.
        return await _dbSet.ToListAsync();
    }
}
