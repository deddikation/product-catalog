// =============================================================================
// File: ICategoryService.cs
// Layer: Application
// Purpose: Interface defining the category service contract.
// =============================================================================

using ProductCatalog.Application.DTOs;

namespace ProductCatalog.Application.Services;

/// <summary>
/// Service interface for category operations including flat list
/// retrieval, hierarchical tree building, and category creation.
/// </summary>
public interface ICategoryService
{
    /// <summary>
    /// Retrieves all categories as a flat list.
    /// </summary>
    /// <returns>A list of all category DTOs.</returns>
    Task<List<CategoryDto>> GetAllAsync();

    /// <summary>
    /// Builds and returns the full category tree structure (req 9).
    /// Root categories are top-level nodes with children nested recursively.
    /// </summary>
    /// <returns>A list of root-level category tree DTOs with nested children.</returns>
    Task<List<CategoryTreeDto>> GetTreeAsync();

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="dto">The category creation data.</param>
    /// <returns>The created category DTO.</returns>
    /// <exception cref="Domain.Exceptions.ValidationException">Thrown if validation fails.</exception>
    /// <exception cref="Domain.Exceptions.DuplicateException">Thrown if name already exists.</exception>
    Task<CategoryDto> CreateAsync(CreateCategoryDto dto);
}
