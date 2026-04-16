// =============================================================================
// File: PagedResult.cs
// Layer: Application
// Purpose: Generic record type for paginated API responses (req 4).
//          Wraps a page of items with metadata for client-side pagination.
// =============================================================================

namespace ProductCatalog.Application.DTOs;

/// <summary>
/// Generic paginated result wrapper. Contains a page of items
/// along with pagination metadata for building pagination controls.
/// </summary>
/// <typeparam name="T">The type of items in the page.</typeparam>
/// <param name="Items">The items on the current page.</param>
/// <param name="TotalCount">Total number of items across all pages.</param>
/// <param name="Page">Current page number (1-based).</param>
/// <param name="PageSize">Number of items per page.</param>
public record PagedResult<T>(
    List<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    /// <summary>Total number of pages based on TotalCount and PageSize.</summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>Whether there is a next page available.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Whether there is a previous page available.</summary>
    public bool HasPreviousPage => Page > 1;
}
