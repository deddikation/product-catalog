// =============================================================================
// File: ProductDto.cs
// Layer: Application
// Purpose: Record-type DTOs for product data transfer (req 4).
//          Records provide immutability, value equality, and concise syntax.
// =============================================================================

namespace ProductCatalog.Application.DTOs;

/// <summary>
/// Read-only DTO representing a product returned to API consumers.
/// Includes the resolved category name for display purposes.
/// </summary>
/// <param name="Id">Unique product identifier.</param>
/// <param name="Name">Product display name.</param>
/// <param name="Description">Product description.</param>
/// <param name="SKU">Stock Keeping Unit identifier.</param>
/// <param name="Price">Product price in base currency.</param>
/// <param name="Quantity">Current inventory quantity.</param>
/// <param name="CategoryId">Associated category identifier.</param>
/// <param name="CategoryName">Resolved category name (for display).</param>
/// <param name="CreatedAt">When the product was created.</param>
/// <param name="UpdatedAt">When the product was last modified.</param>
public record ProductDto(
    int Id,
    string Name,
    string Description,
    string SKU,
    decimal Price,
    int Quantity,
    int CategoryId,
    string? CategoryName,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

/// <summary>
/// DTO for creating a new product. Excludes server-generated fields (Id, timestamps).
/// </summary>
/// <param name="Name">Product display name (required).</param>
/// <param name="Description">Product description.</param>
/// <param name="SKU">Stock Keeping Unit (required, must be unique).</param>
/// <param name="Price">Product price (must be >= 0).</param>
/// <param name="Quantity">Initial inventory quantity (must be >= 0).</param>
/// <param name="CategoryId">Category to assign the product to.</param>
public record CreateProductDto(
    string Name,
    string Description,
    string SKU,
    decimal Price,
    int Quantity,
    int CategoryId
);

/// <summary>
/// DTO for updating an existing product. Same shape as CreateProductDto
/// but semantically distinct for clarity in controller actions.
/// </summary>
/// <param name="Name">Updated product name.</param>
/// <param name="Description">Updated description.</param>
/// <param name="SKU">Updated SKU (must remain unique).</param>
/// <param name="Price">Updated price.</param>
/// <param name="Quantity">Updated inventory quantity.</param>
/// <param name="CategoryId">Updated category assignment.</param>
public record UpdateProductDto(
    string Name,
    string Description,
    string SKU,
    decimal Price,
    int Quantity,
    int CategoryId
);

/// <summary>
/// DTO encapsulating product search/filter/pagination parameters.
/// Used to pass query parameters from the controller to the service layer.
/// </summary>
/// <param name="SearchTerm">Optional text search query (fuzzy matched).</param>
/// <param name="CategoryId">Optional category filter.</param>
/// <param name="MinPrice">Optional minimum price filter.</param>
/// <param name="MaxPrice">Optional maximum price filter.</param>
/// <param name="Page">Page number (1-based, defaults to 1).</param>
/// <param name="PageSize">Items per page (defaults to 10).</param>
public record ProductSearchRequest(
    string? SearchTerm = null,
    int? CategoryId = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    int Page = 1,
    int PageSize = 10
);
