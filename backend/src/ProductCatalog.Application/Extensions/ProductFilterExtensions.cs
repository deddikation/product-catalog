// =============================================================================
// File: ProductFilterExtensions.cs
// Layer: Application
// Purpose: Custom LINQ extension methods for product filtering (req 3).
//          Provides composable query building for product searches.
// =============================================================================

using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Application.Extensions;

/// <summary>
/// Custom LINQ extension methods for filtering, searching, and paginating
/// product queries. These extensions enable composable query building:
///
///   products.FilterByCategory(5).SearchByName("laptop").InPriceRange(100, 500).Paginate(1, 10)
///
/// Each method is designed to be chainable and handles null/empty parameters gracefully.
/// </summary>
public static class ProductFilterExtensions
{
    /// <summary>
    /// Filters products by category ID. Returns all products if categoryId is null.
    /// </summary>
    /// <param name="query">The product queryable to filter.</param>
    /// <param name="categoryId">The category ID to filter by, or null for all categories.</param>
    /// <returns>Filtered queryable containing only products in the specified category.</returns>
    public static IQueryable<Product> FilterByCategory(this IQueryable<Product> query, int? categoryId)
    {
        // Skip filtering if no category specified — returns all products
        if (categoryId is null) return query;

        return query.Where(p => p.CategoryId == categoryId.Value);
    }

    /// <summary>
    /// Searches products by name using case-insensitive substring matching.
    /// Returns all products if the search term is null or empty.
    /// </summary>
    /// <param name="query">The product queryable to search.</param>
    /// <param name="searchTerm">The text to search for in product names.</param>
    /// <returns>Filtered queryable containing only products whose names match.</returns>
    public static IQueryable<Product> SearchByName(this IQueryable<Product> query, string? searchTerm)
    {
        // Skip search if no term provided — returns all products
        if (string.IsNullOrWhiteSpace(searchTerm)) return query;

        var normalizedTerm = searchTerm.Trim().ToLower();
        return query.Where(p => p.Name.ToLower().Contains(normalizedTerm));
    }

    /// <summary>
    /// Filters products within a price range. Either bound can be null for an open range.
    /// </summary>
    /// <param name="query">The product queryable to filter.</param>
    /// <param name="minPrice">Minimum price (inclusive), or null for no lower bound.</param>
    /// <param name="maxPrice">Maximum price (inclusive), or null for no upper bound.</param>
    /// <returns>Filtered queryable containing only products within the price range.</returns>
    public static IQueryable<Product> InPriceRange(this IQueryable<Product> query, decimal? minPrice, decimal? maxPrice)
    {
        // Apply lower bound if specified
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        // Apply upper bound if specified
        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        return query;
    }

    /// <summary>
    /// Filters to only products that are currently in stock (Quantity > 0).
    /// </summary>
    /// <param name="query">The product queryable to filter.</param>
    /// <returns>Filtered queryable containing only in-stock products.</returns>
    public static IQueryable<Product> InStock(this IQueryable<Product> query)
    {
        return query.Where(p => p.Quantity > 0);
    }

    /// <summary>
    /// Applies pagination to the query using skip/take pattern.
    /// Page numbers are 1-based for API consumer friendliness.
    /// </summary>
    /// <param name="query">The product queryable to paginate.</param>
    /// <param name="page">Page number (1-based). Values less than 1 are treated as 1.</param>
    /// <param name="pageSize">Number of items per page. Clamped between 1 and 100.</param>
    /// <returns>Paginated queryable for the requested page.</returns>
    public static IQueryable<Product> Paginate(this IQueryable<Product> query, int page, int pageSize)
    {
        // Clamp page and pageSize to valid ranges
        var safePage = Math.Max(1, page);
        var safePageSize = Math.Clamp(pageSize, 1, 100);

        return query
            .Skip((safePage - 1) * safePageSize)
            .Take(safePageSize);
    }

    /// <summary>
    /// Sorts products using their IComparable implementation (req 10).
    /// Falls back to ordering by Name for IQueryable compatibility.
    /// </summary>
    /// <param name="query">The product queryable to sort.</param>
    /// <returns>Sorted queryable using the product's natural ordering.</returns>
    public static IQueryable<Product> SortByDefault(this IQueryable<Product> query)
    {
        // IQueryable can't directly use IComparable, so we mirror the sort logic:
        // Primary: Name ascending, Secondary: Price ascending, Tertiary: CreatedAt descending
        return query
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Price)
            .ThenByDescending(p => p.CreatedAt);
    }
}
