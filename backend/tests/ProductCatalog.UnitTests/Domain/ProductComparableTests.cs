// =============================================================================
// File: ProductComparableTests.cs
// Layer: Unit Tests — Domain
// Purpose: Tests the IComparable<Product> implementation (req 10).
//          Verifies sorting by Name → Price → CreatedAt.
// =============================================================================

using ProductCatalog.Domain.Entities;

namespace ProductCatalog.UnitTests.Domain;

/// <summary>
/// Unit tests for Product.CompareTo() — verifies the IComparable implementation
/// sorts products by Name (ascending), then Price (ascending), then CreatedAt (descending).
/// </summary>
public class ProductComparableTests
{
    /// <summary>
    /// Products with different names should sort alphabetically (case-insensitive).
    /// </summary>
    [Fact]
    public void CompareTo_DifferentNames_SortsAlphabetically()
    {
        // Arrange
        var productA = new Product { Name = "Apple", Price = 10m };
        var productB = new Product { Name = "Banana", Price = 10m };

        // Act & Assert — Apple comes before Banana
        Assert.True(productA.CompareTo(productB) < 0);
        Assert.True(productB.CompareTo(productA) > 0);
    }

    /// <summary>
    /// Products with same name should sort by price ascending.
    /// </summary>
    [Fact]
    public void CompareTo_SameNameDifferentPrice_SortsByPriceAscending()
    {
        // Arrange
        var cheap = new Product { Name = "Widget", Price = 9.99m, CreatedAt = DateTime.UtcNow };
        var expensive = new Product { Name = "Widget", Price = 19.99m, CreatedAt = DateTime.UtcNow };

        // Act & Assert — cheaper product comes first
        Assert.True(cheap.CompareTo(expensive) < 0);
        Assert.True(expensive.CompareTo(cheap) > 0);
    }

    /// <summary>
    /// Products with same name and price should sort by CreatedAt descending (newest first).
    /// </summary>
    [Fact]
    public void CompareTo_SameNameAndPrice_SortsByCreatedAtDescending()
    {
        // Arrange
        var older = new Product { Name = "Widget", Price = 10m, CreatedAt = DateTime.UtcNow.AddDays(-5) };
        var newer = new Product { Name = "Widget", Price = 10m, CreatedAt = DateTime.UtcNow };

        // Act & Assert — newer product comes first (descending CreatedAt)
        Assert.True(newer.CompareTo(older) < 0);
        Assert.True(older.CompareTo(newer) > 0);
    }

    /// <summary>
    /// Identical products should return 0 from CompareTo.
    /// </summary>
    [Fact]
    public void CompareTo_IdenticalProducts_ReturnsZero()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var product1 = new Product { Name = "Widget", Price = 10m, CreatedAt = now };
        var product2 = new Product { Name = "Widget", Price = 10m, CreatedAt = now };

        // Act & Assert
        Assert.Equal(0, product1.CompareTo(product2));
    }

    /// <summary>
    /// Comparing with null should return negative (non-null sorts before null).
    /// </summary>
    [Fact]
    public void CompareTo_Null_ReturnsNegative()
    {
        // Arrange
        var product = new Product { Name = "Widget", Price = 10m };

        // Act & Assert
        Assert.True(product.CompareTo(null) < 0);
    }

    /// <summary>
    /// Name comparison should be case-insensitive.
    /// </summary>
    [Fact]
    public void CompareTo_CaseInsensitiveNames_ReturnsZero()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var lower = new Product { Name = "widget", Price = 10m, CreatedAt = now };
        var upper = new Product { Name = "WIDGET", Price = 10m, CreatedAt = now };

        // Act & Assert — case should not affect ordering
        Assert.Equal(0, lower.CompareTo(upper));
    }

    /// <summary>
    /// Sorting a list of products should use the IComparable implementation.
    /// </summary>
    [Fact]
    public void Sort_ListOfProducts_UsesComparableImplementation()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var products = new List<Product>
        {
            new() { Name = "Banana", Price = 5m, CreatedAt = now },
            new() { Name = "Apple", Price = 10m, CreatedAt = now },
            new() { Name = "Apple", Price = 5m, CreatedAt = now },
            new() { Name = "Cherry", Price = 3m, CreatedAt = now }
        };

        // Act
        products.Sort();

        // Assert — sorted by Name then Price
        Assert.Equal("Apple", products[0].Name);
        Assert.Equal(5m, products[0].Price);
        Assert.Equal("Apple", products[1].Name);
        Assert.Equal(10m, products[1].Price);
        Assert.Equal("Banana", products[2].Name);
        Assert.Equal("Cherry", products[3].Name);
    }

    /// <summary>
    /// IsInStock should return true when Quantity > 0 and false when Quantity == 0.
    /// </summary>
    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(100, true)]
    public void IsInStock_ReturnsCorrectValue(int quantity, bool expected)
    {
        // Arrange
        var product = new Product { Quantity = quantity };

        // Act & Assert
        Assert.Equal(expected, product.IsInStock);
    }
}
