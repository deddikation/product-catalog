// =============================================================================
// File: ICategoryRepository.cs
// Layer: Domain
// Purpose: Extends the generic repository with category-specific operations
//          such as retrieving child categories and tree structures (req 9).
// =============================================================================

using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Domain.Interfaces;

/// <summary>
/// Category-specific repository interface extending the generic IRepository.
/// Adds operations for hierarchical category tree retrieval.
/// </summary>
public interface ICategoryRepository : IRepository<Category>
{
    /// <summary>
    /// Retrieves all child categories for a given parent category.
    /// </summary>
    /// <param name="parentId">The parent category's ID.</param>
    /// <returns>An enumerable of child categories.</returns>
    Task<IEnumerable<Category>> GetChildrenAsync(int parentId);

    /// <summary>
    /// Retrieves all root-level categories (those with no parent).
    /// </summary>
    /// <returns>An enumerable of root categories.</returns>
    Task<IEnumerable<Category>> GetRootCategoriesAsync();

    /// <summary>
    /// Checks whether a category with the given name already exists.
    /// </summary>
    /// <param name="name">The category name to check.</param>
    /// <returns>True if a category with that name exists.</returns>
    Task<bool> ExistsByNameAsync(string name);
}
