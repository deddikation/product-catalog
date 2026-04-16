// =============================================================================
// File: CategoryDto.cs
// Layer: Application
// Purpose: Record-type DTOs for category data transfer (req 4).
//          Includes a tree DTO for hierarchical category representation (req 9).
// =============================================================================

namespace ProductCatalog.Application.DTOs;

/// <summary>
/// Flat DTO representing a category without hierarchy information.
/// Used for the GET /api/categories flat list endpoint.
/// </summary>
/// <param name="Id">Unique category identifier.</param>
/// <param name="Name">Category display name.</param>
/// <param name="Description">Category description.</param>
/// <param name="ParentCategoryId">Parent category ID (null for root categories).</param>
public record CategoryDto(
    int Id,
    string Name,
    string Description,
    int? ParentCategoryId
);

/// <summary>
/// Hierarchical DTO representing a category tree node.
/// Used for the GET /api/categories/tree endpoint (req 9).
/// Each node contains its children, forming a recursive tree structure.
/// </summary>
/// <param name="Id">Unique category identifier.</param>
/// <param name="Name">Category display name.</param>
/// <param name="Description">Category description.</param>
/// <param name="Children">List of child category tree nodes.</param>
public record CategoryTreeDto(
    int Id,
    string Name,
    string Description,
    List<CategoryTreeDto> Children
);

/// <summary>
/// DTO for creating a new category.
/// </summary>
/// <param name="Name">Category name (required).</param>
/// <param name="Description">Category description.</param>
/// <param name="ParentCategoryId">Optional parent category ID (null for root).</param>
public record CreateCategoryDto(
    string Name,
    string Description,
    int? ParentCategoryId
);
