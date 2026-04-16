// =============================================================================
// File: FuzzyMatcher.cs
// Layer: Infrastructure
// Purpose: Implements fuzzy string matching using Levenshtein distance algorithm.
//          Uses ONLY .NET BCL — no external NuGet packages (core challenge).
//          Supports typo tolerance (e.g., "lptop" matches "laptop").
// =============================================================================

namespace ProductCatalog.Infrastructure.Search;

/// <summary>
/// Static utility class for fuzzy string matching using the Levenshtein distance algorithm.
/// Provides similarity scoring between strings to support typo-tolerant search.
///
/// Implementation uses only .NET Base Class Library — no external dependencies.
///
/// Algorithm: Levenshtein distance measures the minimum number of single-character
/// edits (insertions, deletions, substitutions) needed to transform one string into another.
/// The distance is normalized to a 0.0–1.0 similarity score.
/// </summary>
public static class FuzzyMatcher
{
    /// <summary>
    /// Computes the similarity score between two strings using multiple matching strategies:
    /// 1. Exact match bonus (highest score)
    /// 2. Substring containment bonus
    /// 3. Levenshtein distance-based similarity
    /// 4. Token overlap for multi-word queries
    /// </summary>
    /// <param name="source">The search query string.</param>
    /// <param name="target">The target string to match against.</param>
    /// <returns>Similarity score from 0.0 (no match) to 1.0 (perfect match).</returns>
    public static double CalculateSimilarity(string source, string target)
    {
        // Handle null/empty edge cases
        if (string.IsNullOrWhiteSpace(source) || string.IsNullOrWhiteSpace(target))
            return 0.0;

        // Normalize both strings: lowercase and trim whitespace
        var normalizedSource = source.Trim().ToLowerInvariant();
        var normalizedTarget = target.Trim().ToLowerInvariant();

        // Strategy 1: Exact match — return perfect score
        if (normalizedSource == normalizedTarget)
            return 1.0;

        // Strategy 2: Substring containment — strong signal of relevance
        if (normalizedTarget.Contains(normalizedSource))
            return 0.9;

        // Strategy 3: Token overlap — handles multi-word queries
        var tokenScore = CalculateTokenOverlap(normalizedSource, normalizedTarget);

        // Strategy 4: Levenshtein distance-based similarity
        var levenshteinScore = CalculateLevenshteinSimilarity(normalizedSource, normalizedTarget);

        // Return the best score from all strategies
        // This ensures both fuzzy character matching and word-level matching work
        return Math.Max(tokenScore, levenshteinScore);
    }

    /// <summary>
    /// Computes the Levenshtein distance between two strings.
    /// Uses a space-optimized dynamic programming approach with two rows
    /// instead of a full matrix, reducing memory from O(m*n) to O(min(m,n)).
    /// </summary>
    /// <param name="source">First string.</param>
    /// <param name="target">Second string.</param>
    /// <returns>The minimum number of edits to transform source into target.</returns>
    public static int ComputeLevenshteinDistance(string source, string target)
    {
        // Edge cases: one or both strings are empty
        if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target)) return source.Length;

        var sourceLen = source.Length;
        var targetLen = target.Length;

        // Optimization: ensure we iterate over the shorter string for our "row"
        // This reduces memory usage to O(min(m,n))
        if (sourceLen > targetLen)
        {
            (source, target) = (target, source);
            (sourceLen, targetLen) = (targetLen, sourceLen);
        }

        // Two-row DP approach: previousRow and currentRow
        var previousRow = new int[sourceLen + 1];
        var currentRow = new int[sourceLen + 1];

        // Initialize first row: distance from empty string
        for (var i = 0; i <= sourceLen; i++)
        {
            previousRow[i] = i;
        }

        // Fill in the DP table row by row
        for (var j = 1; j <= targetLen; j++)
        {
            currentRow[0] = j; // Distance from empty source to first j chars of target

            for (var i = 1; i <= sourceLen; i++)
            {
                // Cost is 0 if characters match, 1 if substitution needed
                var cost = source[i - 1] == target[j - 1] ? 0 : 1;

                // Minimum of three operations:
                // - Deletion:    currentRow[i-1] + 1
                // - Insertion:   previousRow[i] + 1
                // - Substitution: previousRow[i-1] + cost
                currentRow[i] = Math.Min(
                    Math.Min(currentRow[i - 1] + 1, previousRow[i] + 1),
                    previousRow[i - 1] + cost
                );
            }

            // Swap rows: current becomes previous for next iteration
            (previousRow, currentRow) = (currentRow, previousRow);
        }

        // Result is in previousRow after the final swap
        return previousRow[sourceLen];
    }

    /// <summary>
    /// Calculates a normalized similarity score from Levenshtein distance.
    /// Score ranges from 0.0 (completely different) to 1.0 (identical).
    /// </summary>
    /// <param name="source">First string (already normalized).</param>
    /// <param name="target">Second string (already normalized).</param>
    /// <returns>Normalized similarity score.</returns>
    private static double CalculateLevenshteinSimilarity(string source, string target)
    {
        var distance = ComputeLevenshteinDistance(source, target);
        var maxLength = Math.Max(source.Length, target.Length);

        // Avoid division by zero
        if (maxLength == 0) return 1.0;

        // Normalize: 1.0 means identical, 0.0 means completely different
        return 1.0 - ((double)distance / maxLength);
    }

    /// <summary>
    /// Calculates the overlap between tokens (words) in the source and target strings.
    /// For each source token, finds the best fuzzy match among target tokens.
    /// Returns the average best-match score across all source tokens.
    ///
    /// This handles multi-word queries like "wireless mouse" matching "Ergonomic wireless mouse".
    /// </summary>
    /// <param name="source">The search query (normalized).</param>
    /// <param name="target">The target text to match against (normalized).</param>
    /// <returns>Average token overlap score (0.0 to 1.0).</returns>
    private static double CalculateTokenOverlap(string source, string target)
    {
        // Split both strings into tokens (words)
        var sourceTokens = source.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var targetTokens = target.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (sourceTokens.Length == 0 || targetTokens.Length == 0)
            return 0.0;

        var totalScore = 0.0;

        // For each query token, find its best match among target tokens
        foreach (var sourceToken in sourceTokens)
        {
            var bestTokenScore = 0.0;

            foreach (var targetToken in targetTokens)
            {
                // Check for substring containment first (fast path)
                if (targetToken.Contains(sourceToken))
                {
                    bestTokenScore = 0.9;
                    break; // Can't do better than containment (except exact)
                }

                // Fall back to Levenshtein similarity
                var similarity = CalculateLevenshteinSimilarity(sourceToken, targetToken);
                bestTokenScore = Math.Max(bestTokenScore, similarity);
            }

            totalScore += bestTokenScore;
        }

        // Average score across all source tokens
        return totalScore / sourceTokens.Length;
    }
}
