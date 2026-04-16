// =============================================================================
// File: FuzzyMatcherTests.cs
// Layer: Unit Tests — Infrastructure
// Purpose: Tests the Levenshtein distance and fuzzy matching algorithms.
//          Verifies typo tolerance and similarity scoring accuracy.
// =============================================================================

using ProductCatalog.Infrastructure.Search;

namespace ProductCatalog.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for FuzzyMatcher — validates the Levenshtein distance algorithm
/// and the composite similarity scoring used by the search engine.
/// </summary>
public class FuzzyMatcherTests
{
    // =====================================================================
    // Levenshtein Distance Tests
    // =====================================================================

    /// <summary>
    /// Identical strings should have a distance of 0.
    /// </summary>
    [Fact]
    public void LevenshteinDistance_IdenticalStrings_ReturnsZero()
    {
        Assert.Equal(0, FuzzyMatcher.ComputeLevenshteinDistance("laptop", "laptop"));
    }

    /// <summary>
    /// Empty strings should have distance equal to the other string's length.
    /// </summary>
    [Theory]
    [InlineData("", "hello", 5)]
    [InlineData("hello", "", 5)]
    [InlineData("", "", 0)]
    public void LevenshteinDistance_EmptyStrings_ReturnsLength(string a, string b, int expected)
    {
        Assert.Equal(expected, FuzzyMatcher.ComputeLevenshteinDistance(a, b));
    }

    /// <summary>
    /// Known edit distances for common typos.
    /// </summary>
    [Theory]
    [InlineData("kitten", "sitting", 3)]   // Classic example
    [InlineData("laptop", "lptop", 1)]     // Missing character (assessment example)
    [InlineData("mouse", "moose", 1)]      // Character substitution
    [InlineData("abc", "def", 3)]          // Completely different
    public void LevenshteinDistance_KnownCases_ReturnsCorrectDistance(string a, string b, int expected)
    {
        Assert.Equal(expected, FuzzyMatcher.ComputeLevenshteinDistance(a, b));
    }

    /// <summary>
    /// Distance should be symmetric: d(a,b) == d(b,a).
    /// </summary>
    [Theory]
    [InlineData("hello", "world")]
    [InlineData("laptop", "lptop")]
    public void LevenshteinDistance_IsSymmetric(string a, string b)
    {
        var distAB = FuzzyMatcher.ComputeLevenshteinDistance(a, b);
        var distBA = FuzzyMatcher.ComputeLevenshteinDistance(b, a);
        Assert.Equal(distAB, distBA);
    }

    // =====================================================================
    // Similarity Score Tests
    // =====================================================================

    /// <summary>
    /// Identical strings should have similarity of 1.0.
    /// </summary>
    [Fact]
    public void Similarity_IdenticalStrings_ReturnsOne()
    {
        var score = FuzzyMatcher.CalculateSimilarity("laptop", "laptop");
        Assert.Equal(1.0, score);
    }

    /// <summary>
    /// Substring match should return high similarity (0.9).
    /// </summary>
    [Fact]
    public void Similarity_SubstringMatch_ReturnsHigh()
    {
        var score = FuzzyMatcher.CalculateSimilarity("laptop", "Laptop Pro 15");
        Assert.True(score >= 0.8, $"Expected >= 0.8 but got {score}");
    }

    /// <summary>
    /// Typo in search query (missing character) should still match.
    /// "lptop" should match "laptop" with reasonable similarity.
    /// </summary>
    [Fact]
    public void Similarity_TypoMissingCharacter_StillMatches()
    {
        var score = FuzzyMatcher.CalculateSimilarity("lptop", "laptop");
        Assert.True(score >= 0.7, $"Expected >= 0.7 for 'lptop' vs 'laptop' but got {score}");
    }

    /// <summary>
    /// Completely different strings should have low similarity.
    /// </summary>
    [Fact]
    public void Similarity_CompletelyDifferent_ReturnsLow()
    {
        var score = FuzzyMatcher.CalculateSimilarity("laptop", "xyz123");
        Assert.True(score < 0.5, $"Expected < 0.5 but got {score}");
    }

    /// <summary>
    /// Null or empty input should return 0.0.
    /// </summary>
    [Theory]
    [InlineData(null, "laptop")]
    [InlineData("laptop", null)]
    [InlineData("", "laptop")]
    [InlineData("laptop", "")]
    [InlineData("  ", "laptop")]
    public void Similarity_NullOrEmpty_ReturnsZero(string? source, string? target)
    {
        var score = FuzzyMatcher.CalculateSimilarity(source!, target!);
        Assert.Equal(0.0, score);
    }

    /// <summary>
    /// Case-insensitive matching: "LAPTOP" should match "laptop" perfectly.
    /// </summary>
    [Fact]
    public void Similarity_CaseInsensitive_MatchesPerfectly()
    {
        var score = FuzzyMatcher.CalculateSimilarity("LAPTOP", "laptop");
        Assert.Equal(1.0, score);
    }

    /// <summary>
    /// Multi-word query should match via token overlap.
    /// </summary>
    [Fact]
    public void Similarity_MultiWordQuery_MatchesTokens()
    {
        var score = FuzzyMatcher.CalculateSimilarity("wireless mouse", "Ergonomic wireless mouse");
        Assert.True(score >= 0.7, $"Expected >= 0.7 for multi-word match but got {score}");
    }
}
