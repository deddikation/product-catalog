// =============================================================================
// File: DictionarySearchCacheTests.cs
// Layer: Unit Tests — Infrastructure
// Purpose: Tests the Dictionary-based search cache (req 8).
//          Validates TTL expiration, cache hits/misses, and invalidation.
// =============================================================================

using ProductCatalog.Infrastructure.Caching;

namespace ProductCatalog.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for DictionarySearchCache — validates the Dictionary-based
/// caching layer with TTL expiration and cache invalidation.
/// </summary>
public class DictionarySearchCacheTests
{
    /// <summary>
    /// Set and TryGet should return the cached value when not expired.
    /// </summary>
    [Fact]
    public void TryGet_AfterSet_ReturnsCachedValue()
    {
        // Arrange
        var cache = new DictionarySearchCache();
        var data = new List<string> { "result1", "result2" };

        // Act
        cache.Set("key1", data);
        var found = cache.TryGet<List<string>>("key1", out var result);

        // Assert
        Assert.True(found);
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    /// <summary>
    /// TryGet for a non-existent key should return false.
    /// </summary>
    [Fact]
    public void TryGet_NonExistentKey_ReturnsFalse()
    {
        // Arrange
        var cache = new DictionarySearchCache();

        // Act
        var found = cache.TryGet<string>("missing", out var result);

        // Assert
        Assert.False(found);
        Assert.Null(result);
    }

    /// <summary>
    /// Expired entries should not be returned by TryGet.
    /// </summary>
    [Fact]
    public async Task TryGet_ExpiredEntry_ReturnsFalse()
    {
        // Arrange — cache with 100ms TTL
        var cache = new DictionarySearchCache(TimeSpan.FromMilliseconds(100));
        cache.Set("key1", "value1");

        // Act — wait for expiration
        await Task.Delay(150);
        var found = cache.TryGet<string>("key1", out var result);

        // Assert
        Assert.False(found);
        Assert.Null(result);
    }

    /// <summary>
    /// Remove should delete a specific cache entry.
    /// </summary>
    [Fact]
    public void Remove_ExistingKey_DeletesEntry()
    {
        // Arrange
        var cache = new DictionarySearchCache();
        cache.Set("key1", "value1");

        // Act
        cache.Remove("key1");
        var found = cache.TryGet<string>("key1", out _);

        // Assert
        Assert.False(found);
    }

    /// <summary>
    /// Clear should remove all cache entries.
    /// </summary>
    [Fact]
    public void Clear_RemovesAllEntries()
    {
        // Arrange
        var cache = new DictionarySearchCache();
        cache.Set("key1", "value1");
        cache.Set("key2", "value2");
        cache.Set("key3", "value3");

        // Act
        cache.Clear();

        // Assert
        Assert.False(cache.TryGet<string>("key1", out _));
        Assert.False(cache.TryGet<string>("key2", out _));
        Assert.False(cache.TryGet<string>("key3", out _));
    }

    /// <summary>
    /// Set with same key should overwrite the previous entry.
    /// </summary>
    [Fact]
    public void Set_SameKey_OverwritesPrevious()
    {
        // Arrange
        var cache = new DictionarySearchCache();
        cache.Set("key1", "original");

        // Act
        cache.Set("key1", "updated");
        cache.TryGet<string>("key1", out var result);

        // Assert
        Assert.Equal("updated", result);
    }

    /// <summary>
    /// Cache should handle different value types.
    /// </summary>
    [Fact]
    public void Set_DifferentTypes_WorkCorrectly()
    {
        // Arrange
        var cache = new DictionarySearchCache();

        // Act
        cache.Set("string_key", "hello");
        cache.Set("int_key", 42);
        cache.Set("list_key", new List<int> { 1, 2, 3 });

        // Assert
        cache.TryGet<string>("string_key", out var stringResult);
        cache.TryGet<int>("int_key", out var intResult);
        cache.TryGet<List<int>>("list_key", out var listResult);

        Assert.Equal("hello", stringResult);
        Assert.Equal(42, intResult);
        Assert.NotNull(listResult);
        Assert.Equal(3, listResult.Count);
    }

    /// <summary>
    /// Set with null value should be ignored (no entry created).
    /// </summary>
    [Fact]
    public void Set_NullValue_IsIgnored()
    {
        // Arrange
        var cache = new DictionarySearchCache();

        // Act
        cache.Set<string?>("key1", null);
        var found = cache.TryGet<string>("key1", out _);

        // Assert
        Assert.False(found);
    }

    /// <summary>
    /// Non-expired entries should be returned successfully.
    /// </summary>
    [Fact]
    public async Task TryGet_NotExpired_ReturnsTrue()
    {
        // Arrange — cache with 5 second TTL
        var cache = new DictionarySearchCache(TimeSpan.FromSeconds(5));
        cache.Set("key1", "value1");

        // Act — check immediately (well within TTL)
        await Task.Delay(10);
        var found = cache.TryGet<string>("key1", out var result);

        // Assert
        Assert.True(found);
        Assert.Equal("value1", result);
    }
}
