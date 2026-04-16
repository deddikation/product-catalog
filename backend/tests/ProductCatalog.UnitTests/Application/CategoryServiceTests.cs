// =============================================================================
// File: CategoryServiceTests.cs
// Layer: Unit Tests — Application
// Purpose: Tests the CategoryService including tree building (req 9).
// =============================================================================

using Moq;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Services;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Exceptions;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.UnitTests.Application;

/// <summary>
/// Unit tests for CategoryService — validates flat listing, tree building,
/// and category creation with validation.
/// </summary>
public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repoMock = new();
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _service = new CategoryService(_repoMock.Object);
    }

    /// <summary>
    /// GetAllAsync should return all categories as flat DTOs.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Electronics", Description = "Devices" },
            new() { Id = 2, Name = "Books", Description = "Reading" }
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Electronics", result[0].Name);
    }

    /// <summary>
    /// GetTreeAsync should build a proper hierarchical tree structure (req 9).
    /// </summary>
    [Fact]
    public async Task GetTreeAsync_BuildsCorrectHierarchy()
    {
        // Arrange — flat list with parent-child relationships
        var categories = new List<Category>
        {
            new() { Id = 1, Name = "Electronics", Description = "Devices", ParentCategoryId = null },
            new() { Id = 2, Name = "Computers", Description = "PCs", ParentCategoryId = 1 },
            new() { Id = 3, Name = "Phones", Description = "Mobile", ParentCategoryId = 1 },
            new() { Id = 4, Name = "Books", Description = "Reading", ParentCategoryId = null }
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(categories);

        // Act
        var tree = await _service.GetTreeAsync();

        // Assert — should have 2 root nodes
        Assert.Equal(2, tree.Count);

        // Electronics should have 2 children
        var electronics = tree.First(t => t.Name == "Electronics");
        Assert.Equal(2, electronics.Children.Count);
        Assert.Contains(electronics.Children, c => c.Name == "Computers");
        Assert.Contains(electronics.Children, c => c.Name == "Phones");

        // Books should have no children
        var books = tree.First(t => t.Name == "Books");
        Assert.Empty(books.Children);
    }

    /// <summary>
    /// GetTreeAsync with empty repository should return empty list.
    /// </summary>
    [Fact]
    public async Task GetTreeAsync_EmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Category>());

        // Act
        var tree = await _service.GetTreeAsync();

        // Assert
        Assert.Empty(tree);
    }

    /// <summary>
    /// CreateAsync with valid data should create and return the category.
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidData_ReturnsCreatedCategory()
    {
        // Arrange
        var dto = new CreateCategoryDto("Electronics", "Devices", null);
        _repoMock.Setup(r => r.ExistsByNameAsync("Electronics")).ReturnsAsync(false);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Category>()))
            .ReturnsAsync((Category c) => { c.Id = 1; return c; });

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        Assert.Equal("Electronics", result.Name);
        Assert.Null(result.ParentCategoryId);
    }

    /// <summary>
    /// CreateAsync with duplicate name should throw DuplicateException.
    /// </summary>
    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsDuplicateException()
    {
        // Arrange
        var dto = new CreateCategoryDto("Electronics", "Devices", null);
        _repoMock.Setup(r => r.ExistsByNameAsync("Electronics")).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateException>(() => _service.CreateAsync(dto));
    }

    /// <summary>
    /// CreateAsync with non-existent parent should throw NotFoundException.
    /// </summary>
    [Fact]
    public async Task CreateAsync_NonExistentParent_ThrowsNotFoundException()
    {
        // Arrange
        var dto = new CreateCategoryDto("SubCategory", "Child", 999);
        _repoMock.Setup(r => r.ExistsByNameAsync("SubCategory")).ReturnsAsync(false);
        _repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateAsync(dto));
    }

    /// <summary>
    /// CreateAsync with invalid data should throw ValidationException.
    /// </summary>
    [Fact]
    public async Task CreateAsync_InvalidData_ThrowsValidationException()
    {
        // Arrange — empty name
        var dto = new CreateCategoryDto("", "Description", null);

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _service.CreateAsync(dto));
    }
}
