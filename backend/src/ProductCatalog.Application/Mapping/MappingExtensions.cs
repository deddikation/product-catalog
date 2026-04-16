// =============================================================================
// File: MappingExtensions.cs
// Layer: Application
// Purpose: Extension methods for mapping between domain entities and DTOs.
//          Keeps mapping logic centralized and avoids external mapping libraries.
// =============================================================================

using ProductCatalog.Application.DTOs;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.Mapping;

/// <summary>
/// Static extension methods for converting between domain entities and DTOs.
/// Centralizes all mapping logic to maintain single responsibility and
/// avoid scattering conversion code across services.
/// </summary>
public static class MappingExtensions
{
    // =========================================================================
    // Product Mappings
    // =========================================================================

    /// <summary>
    /// Maps a Product entity to its read-only DTO representation.
    /// </summary>
    /// <param name="product">The product entity to map.</param>
    /// <returns>A ProductDto with all fields populated.</returns>
    public static ProductDto ToDto(this Product product) => new(
        Id: product.Id,
        Name: product.Name,
        Description: product.Description,
        SKU: product.SKU,
        Price: product.Price,
        Quantity: product.Quantity,
        CategoryId: product.CategoryId,
        CategoryName: product.Category?.Name,
        CreatedAt: product.CreatedAt,
        UpdatedAt: product.UpdatedAt
    );

    /// <summary>
    /// Maps a CreateProductDto to a new Product entity.
    /// Sets CreatedAt and UpdatedAt to the current UTC time.
    /// </summary>
    /// <param name="dto">The creation DTO.</param>
    /// <returns>A new Product entity ready for persistence.</returns>
    public static Product ToEntity(this CreateProductDto dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        SKU = dto.SKU,
        Price = dto.Price,
        Quantity = dto.Quantity,
        CategoryId = dto.CategoryId,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    /// <summary>
    /// Applies values from an UpdateProductDto onto an existing Product entity.
    /// Updates the UpdatedAt timestamp automatically.
    /// </summary>
    /// <param name="dto">The update DTO with new values.</param>
    /// <param name="existing">The existing product entity to update.</param>
    /// <returns>The updated product entity.</returns>
    public static Product ApplyTo(this UpdateProductDto dto, Product existing)
    {
        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.SKU = dto.SKU;
        existing.Price = dto.Price;
        existing.Quantity = dto.Quantity;
        existing.CategoryId = dto.CategoryId;
        existing.UpdatedAt = DateTime.UtcNow;
        return existing;
    }

    // =========================================================================
    // Category Mappings
    // =========================================================================

    /// <summary>
    /// Maps a Category entity to its flat DTO representation.
    /// </summary>
    /// <param name="category">The category entity to map.</param>
    /// <returns>A flat CategoryDto without children.</returns>
    public static CategoryDto ToDto(this Category category) => new(
        Id: category.Id,
        Name: category.Name,
        Description: category.Description,
        ParentCategoryId: category.ParentCategoryId
    );

    /// <summary>
    /// Maps a Category entity to a hierarchical tree DTO.
    /// Recursively maps all subcategories into the Children list.
    /// </summary>
    /// <param name="category">The category entity with SubCategories populated.</param>
    /// <returns>A CategoryTreeDto with nested children.</returns>
    public static CategoryTreeDto ToTreeDto(this Category category) => new(
        Id: category.Id,
        Name: category.Name,
        Description: category.Description,
        Children: category.SubCategories.Select(c => c.ToTreeDto()).ToList()
    );

    /// <summary>
    /// Maps a CreateCategoryDto to a new Category entity.
    /// </summary>
    /// <param name="dto">The creation DTO.</param>
    /// <returns>A new Category entity ready for persistence.</returns>
    public static Category ToEntity(this CreateCategoryDto dto) => new()
    {
        Name = dto.Name,
        Description = dto.Description,
        ParentCategoryId = dto.ParentCategoryId
    };
}
