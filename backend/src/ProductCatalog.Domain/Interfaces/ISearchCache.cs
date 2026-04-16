// =============================================================================
// File: ISearchCache.cs
// Layer: Domain
// Purpose: Interface for a simple caching layer used to cache search results.
//          Implementation uses Dictionary<TKey, TValue> (req 8).
// =============================================================================

namespace ProductCatalog.Domain.Interfaces;

/// <summary>
/// Interface for a simple key-value cache with TTL-based expiration.
/// Used to cache search results and avoid repeated expensive computations.
/// </summary>
public interface ISearchCache
{
    /// <summary>
    /// Attempts to retrieve a cached value by key.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The cached value if found and not expired.</param>
    /// <returns>True if a valid (non-expired) cache entry was found.</returns>
    bool TryGet<T>(string key, out T? value);

    /// <summary>
    /// Stores a value in the cache with the configured TTL.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    void Set<T>(string key, T value);

    /// <summary>
    /// Removes a specific entry from the cache.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    void Remove(string key);

    /// <summary>
    /// Clears all entries from the cache. Used when data mutations
    /// invalidate cached search results.
    /// </summary>
    void Clear();
}
