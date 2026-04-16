// =============================================================================
// File: Product.cs
// Layer: Domain (innermost layer - no external dependencies)
// Purpose: Represents a product entity in the catalog system.
//          Implements IComparable<Product> for custom sorting support (req 10).
// =============================================================================

namespace ProductCatalog.Domain.Entities;

/// <summary>
/// Core domain entity representing a product in the catalog.
/// Products belong to a category and track inventory quantities.
/// Implements IComparable for natural ordering by Name, then Price, then CreatedAt.
/// </summary>
public class Product : IComparable<Product>
{
    /// <summary>Unique identifier for the product.</summary>
    public int Id { get; set; }

    /// <summary>Display name of the product.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Detailed description of the product.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Stock Keeping Unit — unique identifier for inventory tracking.</summary>
    public string SKU { get; set; } = string.Empty;

    /// <summary>Price of the product in the base currency.</summary>
    public decimal Price { get; set; }

    /// <summary>Current inventory quantity available.</summary>
    public int Quantity { get; set; }

    /// <summary>Foreign key to the product's category.</summary>
    public int CategoryId { get; set; }

    /// <summary>Navigation property to the associated category.</summary>
    public Category? Category { get; set; }

    /// <summary>Timestamp when the product was first created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Timestamp when the product was last updated.</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Compares products for natural ordering.
    /// Sort priority: Name (alphabetical) → Price (ascending) → CreatedAt (newest first).
    /// </summary>
    /// <param name="other">The product to compare against.</param>
    /// <returns>
    /// Negative if this product comes before <paramref name="other"/>,
    /// zero if equal, positive if this product comes after.
    /// </returns>
    public int CompareTo(Product? other)
    {
        // Null products sort to the end
        if (other is null) return -1;

        // Primary sort: alphabetical by name (case-insensitive)
        var nameComparison = string.Compare(Name, other.Name, StringComparison.OrdinalIgnoreCase);
        if (nameComparison != 0) return nameComparison;

        // Secondary sort: ascending by price
        var priceComparison = Price.CompareTo(other.Price);
        if (priceComparison != 0) return priceComparison;

        // Tertiary sort: newest first (descending by CreatedAt)
        return other.CreatedAt.CompareTo(CreatedAt);
    }

    /// <summary>
    /// Checks whether the product is currently in stock.
    /// </summary>
    public bool IsInStock => Quantity > 0;
}
