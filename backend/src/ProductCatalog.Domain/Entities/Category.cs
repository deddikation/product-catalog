// =============================================================================
// File: Category.cs
// Layer: Domain (innermost layer - no external dependencies)
// Purpose: Represents a product category that supports hierarchical nesting.
//          ParentCategoryId is nullable to allow root-level categories (req 9).
// =============================================================================

namespace ProductCatalog.Domain.Entities;

/// <summary>
/// Core domain entity representing a product category.
/// Categories form a tree structure via ParentCategoryId — root categories have null parent.
/// </summary>
public class Category
{
    /// <summary>Unique identifier for the category.</summary>
    public int Id { get; set; }

    /// <summary>Display name of the category.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description explaining the category's scope.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key to the parent category. Null indicates a root-level category.
    /// </summary>
    public int? ParentCategoryId { get; set; }

    /// <summary>Navigation property to the parent category (null for root categories).</summary>
    public Category? ParentCategory { get; set; }

    /// <summary>Navigation property to child categories forming the subtree.</summary>
    public List<Category> SubCategories { get; set; } = new();

    /// <summary>Navigation property to products assigned to this category.</summary>
    public List<Product> Products { get; set; } = new();
}
