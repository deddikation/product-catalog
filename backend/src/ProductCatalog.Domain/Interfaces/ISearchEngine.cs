// =============================================================================
// File: ISearchEngine.cs
// Layer: Domain
// Purpose: Generic search engine interface supporting fuzzy matching and
//          weighted multi-field search. Made generic for reusability (req 13).
// =============================================================================

using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.Domain.Interfaces;

/// <summary>
/// Generic search engine interface for performing fuzzy, weighted searches
/// across any entity type. Implementations must support:
/// - Fuzzy string matching (e.g., "lptop" matches "laptop")
/// - Multi-field search with configurable weights
/// - Scored result ranking
/// </summary>
/// <typeparam name="T">The entity type to search across.</typeparam>
public interface ISearchEngine<T> where T : class
{
    /// <summary>
    /// Searches through the given items using fuzzy matching and weighted scoring.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <param name="items">The collection of items to search through.</param>
    /// <returns>Search results ordered by relevance score (descending).</returns>
    IEnumerable<SearchResult<T>> Search(string query, IEnumerable<T> items);
}

/// <summary>
/// Represents a single search result with its relevance score and per-field breakdown.
/// </summary>
/// <typeparam name="T">The entity type of the matched item.</typeparam>
/// <param name="Item">The matched entity.</param>
/// <param name="Score">Overall relevance score (0.0 to 1.0, higher is better).</param>
/// <param name="FieldScores">Breakdown of scores per field for transparency.</param>
public record SearchResult<T>(T Item, double Score, Dictionary<string, double> FieldScores);
