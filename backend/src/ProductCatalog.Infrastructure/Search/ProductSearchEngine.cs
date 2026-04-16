// =============================================================================
// File: ProductSearchEngine.cs
// Layer: Infrastructure
// Purpose: Core C# Challenge — implements an efficient in-memory search engine
//          using ONLY .NET BCL (no external NuGet packages).
//          Features: fuzzy matching, weighted multi-field search, generics.
// =============================================================================

using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;
using ProductCatalog.Domain.ValueObjects;

namespace ProductCatalog.Infrastructure.Search;

/// <summary>
/// In-memory search engine for products using fuzzy matching and weighted scoring.
///
/// Core features (all implemented with pure .NET BCL):
/// - Fuzzy matching via Levenshtein distance (e.g., "lptop" matches "laptop")
/// - Multi-field search with configurable weights (Name=1.0, SKU=0.8, Description=0.5)
/// - Relevance scoring combining substring matching, token overlap, and edit distance
/// - Generic interface for reusability with any entity type
///
/// Scoring algorithm:
/// 1. For each product, extract searchable fields with weights
/// 2. For each field, compute similarity score using FuzzyMatcher
/// 3. Multiply each field score by its weight
/// 4. Final score = sum(fieldScore * weight) / sum(weights)
/// 5. Filter results above the minimum score threshold
/// 6. Sort by score descending
/// </summary>
public class ProductSearchEngine : ISearchEngine<Product>
{
    /// <summary>Minimum score threshold for a result to be included (0.0 to 1.0).</summary>
    private const double MinScoreThreshold = 0.3;

    /// <summary>
    /// Configuration defining how to extract searchable fields from a Product.
    /// Each field has a name, value extractor, and relative importance weight.
    /// </summary>
    private readonly SearchConfiguration<Product> _configuration;

    /// <summary>
    /// Constructs the ProductSearchEngine with default field configuration.
    /// Fields and their weights:
    /// - Name (1.0): Highest weight — product names are the primary search target
    /// - SKU (0.8): High weight — SKUs are precise identifiers
    /// - Description (0.5): Lower weight — descriptions provide context but are less specific
    /// </summary>
    public ProductSearchEngine()
    {
        _configuration = new SearchConfiguration<Product>(product => new List<SearchField>
        {
            new("Name", product.Name ?? string.Empty, 1.0),
            new("SKU", product.SKU ?? string.Empty, 0.8),
            new("Description", product.Description ?? string.Empty, 0.5)
        });
    }

    /// <inheritdoc />
    /// <remarks>
    /// Algorithm overview:
    /// 1. Validate input — return empty for null/whitespace queries
    /// 2. Extract searchable fields from each product using the configuration
    /// 3. Score each product against the query using fuzzy matching
    /// 4. Filter out low-scoring results below the threshold
    /// 5. Sort by score descending for best-match-first ordering
    /// </remarks>
    public IEnumerable<SearchResult<Product>> Search(string query, IEnumerable<Product> items)
    {
        // Guard: empty or null queries return no results
        if (string.IsNullOrWhiteSpace(query))
            return Enumerable.Empty<SearchResult<Product>>();

        var normalizedQuery = query.Trim();
        var results = new List<SearchResult<Product>>();

        foreach (var item in items)
        {
            // Extract searchable fields from the product
            var fields = _configuration.FieldExtractor(item).ToList();

            // Calculate per-field scores using FuzzyMatcher
            var fieldScores = new Dictionary<string, double>();
            var weightedScoreSum = 0.0;
            var totalWeight = 0.0;

            foreach (var field in fields)
            {
                // Skip empty field values
                if (string.IsNullOrWhiteSpace(field.Value))
                {
                    fieldScores[field.FieldName] = 0.0;
                    continue;
                }

                // Compute similarity between query and this field's value
                var similarity = FuzzyMatcher.CalculateSimilarity(normalizedQuery, field.Value);

                fieldScores[field.FieldName] = similarity;

                // Only accumulate weight for fields with a positive score so that
                // unrelated fields don't dilute a strong single-field match.
                if (similarity > 0.0)
                {
                    weightedScoreSum += similarity * field.Weight;
                    totalWeight += field.Weight;
                }
            }

            // Calculate final weighted average score across matched fields only.
            // This ensures an exact name match (score=1.0, weight=1.0) is not
            // penalised by unrelated fields that scored 0.
            var finalScore = totalWeight > 0 ? weightedScoreSum / totalWeight : 0.0;

            // Only include results above the minimum score threshold
            if (finalScore >= MinScoreThreshold)
            {
                results.Add(new SearchResult<Product>(item, finalScore, fieldScores));
            }
        }

        // Sort by score descending — best matches first
        return results.OrderByDescending(r => r.Score);
    }
}
