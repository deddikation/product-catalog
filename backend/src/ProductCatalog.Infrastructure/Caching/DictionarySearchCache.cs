// =============================================================================
// File: DictionarySearchCache.cs
// Layer: Infrastructure
// Purpose: Simple caching layer using Dictionary<TKey, TValue> (req 8).
//          Implements TTL-based expiration for cached search results.
// =============================================================================

using System.Collections.Concurrent;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.Infrastructure.Caching;

/// <summary>
/// Dictionary-based cache implementation for search results (req 8).
///
/// Uses ConcurrentDictionary (which is part of .NET BCL) as the backing store
/// for thread-safe concurrent access. Each entry includes a timestamp for
/// TTL-based expiration.
///
/// Cache strategy:
/// - Entries expire after a configurable TTL (default: 5 minutes)
/// - Expired entries are lazily cleaned up on read
/// - Cache is fully cleared when data mutations occur
/// </summary>
public class DictionarySearchCache : ISearchCache
{
    /// <summary>
    /// Internal record storing a cached value with its creation timestamp.
    /// Used to determine if an entry has expired based on TTL.
    /// </summary>
    private record CacheEntry(object Value, DateTime CachedAt);

    /// <summary>
    /// The backing Dictionary store. Uses ConcurrentDictionary for thread safety.
    /// Keys are normalized search query strings; values include timestamps for TTL.
    /// </summary>
    private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    /// <summary>
    /// Time-to-live for cache entries. Entries older than this are considered expired.
    /// Default: 5 minutes — balances freshness with performance.
    /// </summary>
    private readonly TimeSpan _ttl;

    /// <summary>
    /// Constructs the cache with a configurable TTL.
    /// </summary>
    /// <param name="ttl">Time-to-live for entries. Defaults to 5 minutes if null.</param>
    public DictionarySearchCache(TimeSpan? ttl = null)
    {
        _ttl = ttl ?? TimeSpan.FromMinutes(5);
    }

    /// <inheritdoc />
    public bool TryGet<T>(string key, out T? value)
    {
        // Attempt to retrieve the entry from the dictionary
        if (_cache.TryGetValue(key, out var entry))
        {
            // Check if the entry has expired
            if (DateTime.UtcNow - entry.CachedAt < _ttl)
            {
                // Entry is still valid — cast and return
                value = (T)entry.Value;
                return true;
            }

            // Entry has expired — remove it lazily
            _cache.TryRemove(key, out _);
        }

        value = default;
        return false;
    }

    /// <inheritdoc />
    public void Set<T>(string key, T value)
    {
        if (value is null) return;

        // Store the value with current timestamp for TTL tracking
        var entry = new CacheEntry(value, DateTime.UtcNow);
        _cache.AddOrUpdate(key, entry, (_, _) => entry);
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        _cache.TryRemove(key, out _);
    }

    /// <inheritdoc />
    public void Clear()
    {
        // Clear all entries — called when data mutations invalidate cached results
        _cache.Clear();
    }
}
