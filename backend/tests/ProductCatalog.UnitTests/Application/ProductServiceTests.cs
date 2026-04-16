// =============================================================================
// File: ProductServiceTests.cs
// Layer: Unit Tests — Application
// Purpose: Tests the ProductService using mocked dependencies.
//          Validates business logic, error handling, and cache interactions.
// =============================================================================

using Moq;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Services;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Exceptions;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.UnitTests.Application;

/// <summary>
/// Unit tests for ProductService — uses Moq to mock all dependencies
/// and test business logic in isolation.
/// </summary>
public class ProductServiceTests
{
    // Mocked dependencies
    private readonly Mock<IProductRepository> _productRepoMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepoMock = new();
    private readonly Mock<ISearchEngine<Product>> _searchEngineMock = new();
    private readonly Mock<ISearchCache> _cacheMock = new();
    private readonly ProductService _service;

    /// <summary>
    /// Sets up the service with mocked dependencies for each test.
    /// </summary>
    public ProductServiceTests()
    {
        _service = new ProductService(
            _productRepoMock.Object,
            _categoryRepoMock.Object,
            _searchEngineMock.Object,
            _cacheMock.Object);
    }

    // =====================================================================
    // GetByIdAsync Tests
    // =====================================================================

    /// <summary>
    /// GetByIdAsync should return a ProductDto for an existing product.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ReturnsDto()
    {
        // Arrange
        var product = new Product
        {
            Id = 1, Name = "Laptop", Description = "A laptop", SKU = "LP01",
            Price = 999m, Quantity = 10, CategoryId = 1,
            Category = new Category { Id = 1, Name = "Electronics" }
        };
        _productRepoMock.Setup(r => r.GetByIdWithCategoryAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        Assert.Equal(1, result.Id);
        Assert.Equal("Laptop", result.Name);
        Assert.Equal("Electronics", result.CategoryName);
    }

    /// <summary>
    /// GetByIdAsync should throw NotFoundException for non-existent product.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_NonExistentProduct_ThrowsNotFoundException()
    {
        // Arrange
        _productRepoMock.Setup(r => r.GetByIdWithCategoryAsync(999))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByIdAsync(999));
    }

    // =====================================================================
    // CreateAsync Tests
    // =====================================================================

    /// <summary>
    /// CreateAsync with valid data should create and return the product.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidData_ReturnsCreatedProduct()
    {
        // Arrange
        var dto = new CreateProductDto("Laptop", "A laptop", "SKU001", 999m, 10, 1);
        var category = new Category { Id = 1, Name = "Electronics" };

        _productRepoMock.Setup(r => r.GetBySkuAsync("SKU001")).ReturnsAsync((Product?)null);
        _categoryRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(category);
        _productRepoMock.Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => { p.Id = 1; return p; });

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.Equal("Laptop", result.Name);
        Assert.Equal("Electronics", result.CategoryName);
        _cacheMock.Verify(c => c.Clear(), Times.Once); // Cache should be invalidated
    }

    /// <summary>
    /// CreateAsync with duplicate SKU should throw DuplicateException.
    /// </summary>
    [Fact]
    public async Task CreateAsync_DuplicateSku_ThrowsDuplicateException()
    {
        // Arrange
        var dto = new CreateProductDto("Laptop", "A laptop", "SKU001", 999m, 10, 1);
        _productRepoMock.Setup(r => r.GetBySkuAsync("SKU001"))
            .ReturnsAsync(new Product { Id = 99, SKU = "SKU001" });

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateException>(() => _service.CreateAsync(dto));
    }

    /// <summary>
    /// CreateAsync with invalid data should throw ValidationException.
    /// </summary>
    [Fact]
    public async Task CreateAsync_InvalidData_ThrowsValidationException()
    {
        // Arrange — empty name
        var dto = new CreateProductDto("", "Description", "SK", -5m, -1, 0);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(dto));
    }

    /// <summary>
    /// CreateAsync with non-existent category should throw NotFoundException.
    /// </summary>
    [Fact]
    public async Task CreateAsync_NonExistentCategory_ThrowsNotFoundException()
    {
        // Arrange
        var dto = new CreateProductDto("Laptop", "A laptop", "SKU001", 999m, 10, 999);
        _productRepoMock.Setup(r => r.GetBySkuAsync("SKU001")).ReturnsAsync((Product?)null);
        _categoryRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(dto));
    }

    // =====================================================================
    // DeleteAsync Tests
    // =====================================================================

    /// <summary>
    /// DeleteAsync should return true and clear cache on success.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ExistingProduct_ReturnsTrueAndClearsCache()
    {
        // Arrange
        _productRepoMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(new Product { Id = 1, Name = "Test" });
        _productRepoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _service.DeleteAsync(1);

        // Assert
        Assert.True(result);
        _cacheMock.Verify(c => c.Clear(), Times.Once);
    }

    /// <summary>
    /// DeleteAsync with non-existent product should throw NotFoundException.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_NonExistentProduct_ThrowsNotFoundException()
    {
        // Arrange
        _productRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(999));
    }

    // =====================================================================
    // SearchAsync Tests
    // =====================================================================

    /// <summary>
    /// SearchAsync should use the search engine and cache results.
    /// </summary>
    [Fact]
    public async Task SearchAsync_NewQuery_UsesEngineAndCaches()
    {
        // Arrange
        var products = new List<Product>
        {
            new() { Id = 1, Name = "Laptop", SKU = "LP01", Category = new Category { Name = "Electronics" } }
        };
        var searchResults = new List<SearchResult<Product>>
        {
            new(products[0], 0.95, new Dictionary<string, double> { { "Name", 0.95 } })
        };

        List<ProductDto>? cachedResult = null;
        _cacheMock.Setup(c => c.TryGet(It.IsAny<string>(), out cachedResult)).Returns(false);
        _productRepoMock.Setup(r => r.GetAllWithCategoryAsync()).ReturnsAsync(products);
        _searchEngineMock.Setup(e => e.Search("laptop", products)).Returns(searchResults);

        // Act
        var result = await _service.SearchAsync("laptop");

        // Assert
        Assert.Single(result);
        Assert.Equal("Laptop", result[0].Name);
        _cacheMock.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<List<ProductDto>>()), Times.Once);
    }
}
