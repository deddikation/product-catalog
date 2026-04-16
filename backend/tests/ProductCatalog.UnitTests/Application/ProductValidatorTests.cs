// =============================================================================
// File: ProductValidatorTests.cs
// Layer: Unit Tests — Application
// Purpose: Tests the pattern matching-based product validation (req 5).
//          Covers all validation rules and boundary values.
// =============================================================================

using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Validation;

namespace ProductCatalog.UnitTests.Application;

/// <summary>
/// Unit tests for ProductValidator — validates all pattern matching rules
/// for product creation and update DTOs.
/// </summary>
public class ProductValidatorTests
{
    /// <summary>
    /// A valid CreateProductDto should pass all validation.
    /// </summary>
    [Fact]
    public void Validate_ValidProduct_ReturnsValid()
    {
        // Arrange
        var dto = new CreateProductDto("Laptop", "A nice laptop", "SKU001", 999.99m, 10, 1);

        // Act
        var (isValid, errors) = ProductValidator.Validate(dto);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errors);
    }

    /// <summary>
    /// Null DTO should fail validation.
    /// </summary>
    [Fact]
    public void Validate_NullDto_ReturnsError()
    {
        // Act
        var (isValid, errors) = ProductValidator.Validate((CreateProductDto?)null);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("required"));
    }

    /// <summary>
    /// Empty name should fail validation.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptyName_ReturnsError(string? name)
    {
        // Arrange
        var dto = new CreateProductDto(name!, "Description", "SKU001", 10m, 5, 1);

        // Act
        var (isValid, errors) = ProductValidator.Validate(dto);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("Name"));
    }

    /// <summary>
    /// Name exceeding 200 characters should fail.
    /// </summary>
    [Fact]
    public void Validate_NameTooLong_ReturnsError()
    {
        // Arrange
        var longName = new string('A', 201);
        var dto = new CreateProductDto(longName, "Description", "SKU001", 10m, 5, 1);

        // Act
        var (isValid, errors) = ProductValidator.Validate(dto);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("Name") && e.Contains("200"));
    }

    /// <summary>
    /// Empty SKU should fail validation.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_EmptySku_ReturnsError(string? sku)
    {
        // Arrange
        var dto = new CreateProductDto("Laptop", "Description", sku!, 10m, 5, 1);

        // Act
        var (isValid, errors) = ProductValidator.Validate(dto);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("SKU"));
    }

    /// <summary>
    /// SKU with 1 character (below minimum of 2) should fail.
    /// </summary>
    [Fact]
    public void Validate_SkuTooShort_ReturnsError()
    {
        // Arrange
        var dto = new CreateProductDto("Laptop", "Description", "A", 10m, 5, 1);

        // Act
        var (isValid, errors) = ProductValidator.Validate(dto);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("SKU") && e.Contains("2"));
    }

    /// <summary>
    /// Negative price should fail validation.
    /// </summary>
    [Fact]
    public void Validate_NegativePrice_ReturnsError()
    {
        // Arrange
        var dto = new CreateProductDto("Laptop", "Description", "SKU001", -1m, 5, 1);

        // Act
        var (isValid, errors) = ProductValidator.Validate(dto);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("Price") && e.Contains("non-negative"));
    }

    /// <summary>
    /// Zero price should be valid (free items).
    /// </summary>
    [Fact]
    public void Validate_ZeroPrice_IsValid()
    {
        // Arrange
        var dto = new CreateProductDto("Free Item", "Description", "SKU001", 0m, 5, 1);

        // Act
        var (isValid, errors) = ProductValidator.Validate(dto);

        // Assert
        Assert.True(isValid);
    }

    /// <summary>
    /// Negative quantity should fail validation.
    /// </summary>
    [Fact]
    public void Validate_NegativeQuantity_ReturnsError()
    {
        // Arrange
        var dto = new CreateProductDto("Laptop", "Description", "SKU001", 10m, -1, 1);

        // Act
        var (isValid, errors) = ProductValidator.Validate(dto);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("Quantity"));
    }

    /// <summary>
    /// CategoryId of 0 or negative should fail validation.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_InvalidCategoryId_ReturnsError(int categoryId)
    {
        // Arrange
        var dto = new CreateProductDto("Laptop", "Description", "SKU001", 10m, 5, categoryId);

        // Act
        var (isValid, errors) = ProductValidator.Validate(dto);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("CategoryId"));
    }

    /// <summary>
    /// Multiple validation errors should all be returned at once.
    /// </summary>
    [Fact]
    public void Validate_MultipleErrors_ReturnsAllErrors()
    {
        // Arrange — empty name, empty SKU, negative price, negative quantity, invalid category
        var dto = new CreateProductDto("", "Description", "", -5m, -1, 0);

        // Act
        var (isValid, errors) = ProductValidator.Validate(dto);

        // Assert — should have multiple errors
        Assert.False(isValid);
        Assert.True(errors.Count >= 4, $"Expected at least 4 errors but got {errors.Count}");
    }

    /// <summary>
    /// UpdateProductDto validation should use the same rules.
    /// </summary>
    [Fact]
    public void Validate_ValidUpdateDto_ReturnsValid()
    {
        // Arrange
        var dto = new UpdateProductDto("Updated Laptop", "Updated description", "SKU002", 1099.99m, 20, 2);

        // Act
        var (isValid, errors) = ProductValidator.Validate(dto);

        // Assert
        Assert.True(isValid);
        Assert.Empty(errors);
    }

    /// <summary>
    /// Null UpdateProductDto should fail validation.
    /// </summary>
    [Fact]
    public void Validate_NullUpdateDto_ReturnsError()
    {
        // Act
        var (isValid, errors) = ProductValidator.Validate((UpdateProductDto?)null);

        // Assert
        Assert.False(isValid);
        Assert.Contains(errors, e => e.Contains("required"));
    }
}
