// =============================================================================
// File: ProductSearchEngineTests.cs
// Layer: Unit Tests — Infrastructure
// Purpose: Tests the ProductSearchEngine — the core C# challenge.
//          Validates fuzzy matching, weighted scoring, and result ranking.
// =============================================================================

using ProductCatalog.Domain.Entities;
using ProductCatalog.Infrastructure.Search;

namespace ProductCatalog.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for ProductSearchEngine — verifies the in-memory fuzzy search
/// algorithm works correctly with weighted scoring across multiple fields.
/// </summary>
public class ProductSearchEngineTests
{
    /// <summary>Reusable search engine instance for tests.</summary>
    private readonly ProductSearchEngine _engine = new();

    /// <summary>Creates a standard set of test products.</summary>
    private static List<Product> CreateTestProducts() => new()
    {
        new() { Id = 1, Name = "Laptop Pro 15", Description = "High-performance laptop with 16GB RAM", SKU = "ELEC-LP15" },
        new() { Id = 2, Name = "Wireless Mouse", Description = "Ergonomic wireless mouse", SKU = "ELEC-WM01" },
        new() { Id = 3, Name = "Mechanical Keyboard", Description = "RGB mechanical keyboard", SKU = "ELEC-MK01" },
        new() { Id = 4, Name = "C# in Depth", Description = "Comprehensive C# programming guide", SKU = "BOOK-CS01" },
        new() { Id = 5, Name = "Running Shoes", Description = "Lightweight running shoes", SKU = "CLTH-RS01" }
    };

    /// <summary>
    /// Exact name match should return the product with highest score.
    /// </summary>
    [Fact]
    public void Search_ExactNameMatch_ReturnsHighestScore()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act
        var results = _engine.Search("Laptop Pro 15", products).ToList();

        // Assert — first result should be the exact match
        Assert.NotEmpty(results);
        Assert.Equal(1, results[0].Item.Id);
        // Score reflects multi-field weighted average — name match dominates but
        // description also scores (contains "laptop"), so threshold is 0.65+
        Assert.True(results[0].Score > 0.65, $"Expected score > 0.65 but got {results[0].Score}");
    }

    /// <summary>
    /// Fuzzy matching should find "laptop" when querying "lptop" (missing 'a').
    /// This is the specific example from the assessment requirements.
    /// </summary>
    [Fact]
    public void Search_FuzzyMatch_LptopMatchesLaptop()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act
        var results = _engine.Search("lptop", products).ToList();

        // Assert — should find "Laptop Pro 15"
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.Item.Name.Contains("Laptop"));
    }

    /// <summary>
    /// Name matches should score higher than description-only matches
    /// due to the weight configuration (Name=1.0 > Description=0.5).
    /// </summary>
    [Fact]
    public void Search_NameWeight_HigherThanDescription()
    {
        // Arrange — two products where "mouse" appears in name vs description
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Wireless Mouse", Description = "A computer peripheral", SKU = "A001" },
            new() { Id = 2, Name = "Computer Peripheral", Description = "Works like a mouse", SKU = "A002" }
        };

        // Act
        var results = _engine.Search("mouse", products).ToList();

        // Assert — product with "mouse" in name should rank higher
        Assert.True(results.Count >= 2);
        Assert.Equal(1, results[0].Item.Id);
    }

    /// <summary>
    /// SKU search should find products by their SKU code.
    /// </summary>
    [Fact]
    public void Search_BySku_FindsProduct()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act
        var results = _engine.Search("ELEC-WM01", products).ToList();

        // Assert — should find Wireless Mouse by SKU
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.Item.SKU == "ELEC-WM01");
    }

    /// <summary>
    /// Empty query should return no results.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Search_EmptyQuery_ReturnsEmpty(string? query)
    {
        // Arrange
        var products = CreateTestProducts();

        // Act
        var results = _engine.Search(query!, products).ToList();

        // Assert
        Assert.Empty(results);
    }

    /// <summary>
    /// Search results should be ordered by score descending.
    /// </summary>
    [Fact]
    public void Search_Results_OrderedByScoreDescending()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act
        var results = _engine.Search("laptop", products).ToList();

        // Assert — scores should be in descending order
        for (var i = 1; i < results.Count; i++)
        {
            Assert.True(results[i - 1].Score >= results[i].Score,
                $"Results not sorted: score[{i - 1}]={results[i - 1].Score} < score[{i}]={results[i].Score}");
        }
    }

    /// <summary>
    /// Each result should include per-field score breakdown.
    /// </summary>
    [Fact]
    public void Search_Results_IncludeFieldScores()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act
        var results = _engine.Search("laptop", products).ToList();

        // Assert — first result should have field scores for Name, SKU, Description
        Assert.NotEmpty(results);
        Assert.True(results[0].FieldScores.ContainsKey("Name"));
        Assert.True(results[0].FieldScores.ContainsKey("SKU"));
        Assert.True(results[0].FieldScores.ContainsKey("Description"));
    }

    /// <summary>
    /// Search with no matching products should return empty.
    /// </summary>
    [Fact]
    public void Search_NoMatches_ReturnsEmpty()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act
        var results = _engine.Search("xyzzynonexistent123", products).ToList();

        // Assert
        Assert.Empty(results);
    }

    /// <summary>
    /// Search should handle empty product list gracefully.
    /// </summary>
    [Fact]
    public void Search_EmptyProductList_ReturnsEmpty()
    {
        // Act
        var results = _engine.Search("laptop", new List<Product>()).ToList();

        // Assert
        Assert.Empty(results);
    }

    /// <summary>
    /// Case-insensitive search: "LAPTOP" should match "laptop".
    /// </summary>
    [Fact]
    public void Search_CaseInsensitive_MatchesRegardlessOfCase()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act
        var results = _engine.Search("LAPTOP", products).ToList();

        // Assert
        Assert.NotEmpty(results);
        Assert.Contains(results, r => r.Item.Name.Contains("Laptop"));
    }
}
