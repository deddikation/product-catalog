// =============================================================================
// File: ProductFilterExtensionsTests.cs
// Layer: Unit Tests — Application
// Purpose: Tests the custom LINQ extension methods for product filtering (req 3).
// =============================================================================

using ProductCatalog.Application.Extensions;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.UnitTests.Application;

/// <summary>
/// Unit tests for ProductFilterExtensions — validates each custom LINQ
/// extension method works correctly for filtering, searching, and pagination.
/// </summary>
public class ProductFilterExtensionsTests
{
    /// <summary>Helper to create a test product list as IQueryable.</summary>
    private static IQueryable<Product> CreateTestProducts()
    {
        return new List<Product>
        {
            new() { Id = 1, Name = "Laptop Pro", Price = 1299.99m, Quantity = 10, CategoryId = 1 },
            new() { Id = 2, Name = "Wireless Mouse", Price = 29.99m, Quantity = 50, CategoryId = 1 },
            new() { Id = 3, Name = "C# Book", Price = 44.99m, Quantity = 30, CategoryId = 2 },
            new() { Id = 4, Name = "Running Shoes", Price = 79.99m, Quantity = 0, CategoryId = 3 },
            new() { Id = 5, Name = "Coffee Maker", Price = 69.99m, Quantity = 25, CategoryId = 4 }
        }.AsQueryable();
    }

    // =====================================================================
    // FilterByCategory Tests
    // =====================================================================

    /// <summary>
    /// FilterByCategory with a specific ID should return only matching products.
    /// </summary>
    [Fact]
    public void FilterByCategory_WithCategoryId_FiltersCorrectly()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act
        var result = products.FilterByCategory(1).ToList();

        // Assert — only category 1 products
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(1, p.CategoryId));
    }

    /// <summary>
    /// FilterByCategory with null should return all products.
    /// </summary>
    [Fact]
    public void FilterByCategory_WithNull_ReturnsAll()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act
        var result = products.FilterByCategory(null).ToList();

        // Assert
        Assert.Equal(5, result.Count);
    }

    // =====================================================================
    // SearchByName Tests
    // =====================================================================

    /// <summary>
    /// SearchByName should find products with matching substring (case-insensitive).
    /// </summary>
    [Fact]
    public void SearchByName_WithMatchingTerm_FindsProducts()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act
        var result = products.SearchByName("laptop").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Laptop Pro", result[0].Name);
    }

    /// <summary>
    /// SearchByName with null/empty should return all products.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SearchByName_WithNullOrEmpty_ReturnsAll(string? term)
    {
        // Arrange
        var products = CreateTestProducts();

        // Act
        var result = products.SearchByName(term).ToList();

        // Assert
        Assert.Equal(5, result.Count);
    }

    /// <summary>
    /// SearchByName should be case-insensitive.
    /// </summary>
    [Fact]
    public void SearchByName_CaseInsensitive_FindsProducts()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act
        var result = products.SearchByName("LAPTOP").ToList();

        // Assert
        Assert.Single(result);
    }

    // =====================================================================
    // InPriceRange Tests
    // =====================================================================

    /// <summary>
    /// InPriceRange with both bounds should filter correctly.
    /// </summary>
    [Fact]
    public void InPriceRange_WithBothBounds_FiltersCorrectly()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act — products between $30 and $80
        var result = products.InPriceRange(30m, 80m).ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, p => Assert.InRange(p.Price, 30m, 80m));
    }

    /// <summary>
    /// InPriceRange with null bounds should return all.
    /// </summary>
    [Fact]
    public void InPriceRange_WithNullBounds_ReturnsAll()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act
        var result = products.InPriceRange(null, null).ToList();

        // Assert
        Assert.Equal(5, result.Count);
    }

    // =====================================================================
    // InStock Tests
    // =====================================================================

    /// <summary>
    /// InStock should only return products with Quantity > 0.
    /// </summary>
    [Fact]
    public void InStock_ReturnsOnlyInStockProducts()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act
        var result = products.InStock().ToList();

        // Assert — Running Shoes has quantity 0, so 4 products should remain
        Assert.Equal(4, result.Count);
        Assert.All(result, p => Assert.True(p.Quantity > 0));
    }

    // =====================================================================
    // Paginate Tests
    // =====================================================================

    /// <summary>
    /// Paginate should return the correct page of results.
    /// </summary>
    [Fact]
    public void Paginate_ReturnsCorrectPage()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act — page 1 with 2 items per page
        var result = products.Paginate(1, 2).ToList();

        // Assert
        Assert.Equal(2, result.Count);
    }

    /// <summary>
    /// Paginate page 2 should skip the first page.
    /// </summary>
    [Fact]
    public void Paginate_Page2_SkipsFirstPage()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act — page 2 with 2 items per page (should get items 3-4)
        var result = products.Paginate(2, 2).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(3, result[0].Id);
    }

    /// <summary>
    /// Paginate with invalid page number should clamp to page 1.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Paginate_InvalidPage_ClampsToOne(int page)
    {
        // Arrange
        var products = CreateTestProducts();

        // Act
        var result = products.Paginate(page, 2).ToList();

        // Assert — should return first 2 items
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Id);
    }

    // =====================================================================
    // SortByDefault Tests
    // =====================================================================

    /// <summary>
    /// SortByDefault should sort by Name ascending as primary sort.
    /// </summary>
    [Fact]
    public void SortByDefault_SortsByNameAscending()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act
        var result = products.SortByDefault().ToList();

        // Assert — first product alphabetically should be "C# Book"
        Assert.Equal("C# Book", result[0].Name);
    }

    // =====================================================================
    // Chaining Tests
    // =====================================================================

    /// <summary>
    /// Extension methods should be chainable for composable queries.
    /// </summary>
    [Fact]
    public void Chaining_MultipleFilters_WorkCorrectly()
    {
        // Arrange
        var products = CreateTestProducts();

        // Act — chain multiple filters
        var result = products
            .FilterByCategory(1)
            .InPriceRange(20m, 100m)
            .InStock()
            .Paginate(1, 10)
            .ToList();

        // Assert — only "Wireless Mouse" (Category 1, $29.99, in stock)
        Assert.Single(result);
        Assert.Equal("Wireless Mouse", result[0].Name);
    }
}
