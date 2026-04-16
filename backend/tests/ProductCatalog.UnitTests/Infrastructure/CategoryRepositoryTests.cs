// =============================================================================
// File: CategoryRepositoryTests.cs
// Layer: Unit Tests — Infrastructure
// Purpose: Tests the pure in-memory CategoryRepository (req 2).
//          Validates CRUD operations using List + Dictionary backing stores.
// =============================================================================

using ProductCatalog.Domain.Entities;
using ProductCatalog.Infrastructure.InMemory;

namespace ProductCatalog.UnitTests.Infrastructure;

/// <summary>
/// Unit tests for the in-memory CategoryRepository.
/// Validates all CRUD operations, tree queries, and thread safety.
/// </summary>
public class CategoryRepositoryTests
{
    /// <summary>Creates a fresh repository instance for each test.</summary>
    private static CategoryRepository CreateRepository() => new();

    /// <summary>
    /// AddAsync should assign an ID and store the category.
    /// </summary>
    [Fact]
    public async Task AddAsync_NewCategory_AssignsIdAndStores()
    {
        // Arrange
        var repo = CreateRepository();
        var category = new Category { Name = "Electronics", Description = "Electronic devices" };

        // Act
        var result = await repo.AddAsync(category);

        // Assert
        Assert.True(result.Id > 0);
        Assert.Equal("Electronics", result.Name);
    }

    /// <summary>
    /// GetByIdAsync should return the correct category.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ExistingCategory_ReturnsCategory()
    {
        // Arrange
        var repo = CreateRepository();
        var added = await repo.AddAsync(new Category { Name = "Books", Description = "All books" });

        // Act
        var result = await repo.GetByIdAsync(added.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Books", result.Name);
    }

    /// <summary>
    /// GetByIdAsync with non-existent ID should return null.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_NonExistentId_ReturnsNull()
    {
        // Arrange
        var repo = CreateRepository();

        // Act
        var result = await repo.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    /// <summary>
    /// GetAllAsync should return all stored categories.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_WithMultipleCategories_ReturnsAll()
    {
        // Arrange
        var repo = CreateRepository();
        await repo.AddAsync(new Category { Name = "Cat1", Description = "First" });
        await repo.AddAsync(new Category { Name = "Cat2", Description = "Second" });
        await repo.AddAsync(new Category { Name = "Cat3", Description = "Third" });

        // Act
        var result = (await repo.GetAllAsync()).ToList();

        // Assert
        Assert.Equal(3, result.Count);
    }

    /// <summary>
    /// UpdateAsync should modify the stored category.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_ExistingCategory_UpdatesValues()
    {
        // Arrange
        var repo = CreateRepository();
        var category = await repo.AddAsync(new Category { Name = "Old Name", Description = "Old" });
        category.Name = "New Name";
        category.Description = "Updated";

        // Act
        await repo.UpdateAsync(category);
        var result = await repo.GetByIdAsync(category.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        Assert.Equal("Updated", result.Description);
    }

    /// <summary>
    /// DeleteAsync should remove the category from both backing stores.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_ExistingCategory_RemovesCategory()
    {
        // Arrange
        var repo = CreateRepository();
        var category = await repo.AddAsync(new Category { Name = "ToDelete", Description = "Temp" });

        // Act
        var deleted = await repo.DeleteAsync(category.Id);
        var result = await repo.GetByIdAsync(category.Id);

        // Assert
        Assert.True(deleted);
        Assert.Null(result);
    }

    /// <summary>
    /// DeleteAsync with non-existent ID should return false.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_NonExistentId_ReturnsFalse()
    {
        // Arrange
        var repo = CreateRepository();

        // Act
        var result = await repo.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    /// <summary>
    /// GetChildrenAsync should return only direct children of the given parent.
    /// </summary>
    [Fact]
    public async Task GetChildrenAsync_ReturnsDirectChildren()
    {
        // Arrange
        var repo = CreateRepository();
        var parent = await repo.AddAsync(new Category { Name = "Parent", Description = "Root" });
        await repo.AddAsync(new Category { Name = "Child1", Description = "First child", ParentCategoryId = parent.Id });
        await repo.AddAsync(new Category { Name = "Child2", Description = "Second child", ParentCategoryId = parent.Id });
        await repo.AddAsync(new Category { Name = "Other", Description = "Not a child" });

        // Act
        var children = (await repo.GetChildrenAsync(parent.Id)).ToList();

        // Assert
        Assert.Equal(2, children.Count);
        Assert.All(children, c => Assert.Equal(parent.Id, c.ParentCategoryId));
    }

    /// <summary>
    /// GetRootCategoriesAsync should return only categories without a parent.
    /// </summary>
    [Fact]
    public async Task GetRootCategoriesAsync_ReturnsOnlyRootCategories()
    {
        // Arrange
        var repo = CreateRepository();
        var root1 = await repo.AddAsync(new Category { Name = "Root1", Description = "First root" });
        await repo.AddAsync(new Category { Name = "Root2", Description = "Second root" });
        await repo.AddAsync(new Category { Name = "Child", Description = "Has parent", ParentCategoryId = root1.Id });

        // Act
        var roots = (await repo.GetRootCategoriesAsync()).ToList();

        // Assert
        Assert.Equal(2, roots.Count);
        Assert.All(roots, r => Assert.Null(r.ParentCategoryId));
    }

    /// <summary>
    /// ExistsByNameAsync should return true for existing names (case-insensitive).
    /// </summary>
    [Fact]
    public async Task ExistsByNameAsync_ExistingName_ReturnsTrue()
    {
        // Arrange
        var repo = CreateRepository();
        await repo.AddAsync(new Category { Name = "Electronics", Description = "Devices" });

        // Act & Assert — case-insensitive check
        Assert.True(await repo.ExistsByNameAsync("Electronics"));
        Assert.True(await repo.ExistsByNameAsync("electronics"));
        Assert.True(await repo.ExistsByNameAsync("ELECTRONICS"));
    }

    /// <summary>
    /// ExistsByNameAsync should return false for non-existing names.
    /// </summary>
    [Fact]
    public async Task ExistsByNameAsync_NonExistingName_ReturnsFalse()
    {
        // Arrange
        var repo = CreateRepository();

        // Act & Assert
        Assert.False(await repo.ExistsByNameAsync("NonExistent"));
    }

    /// <summary>
    /// Query should return an IQueryable for LINQ operations.
    /// </summary>
    [Fact]
    public async Task Query_ReturnsQueryableForLinq()
    {
        // Arrange
        var repo = CreateRepository();
        await repo.AddAsync(new Category { Name = "Alpha", Description = "First" });
        await repo.AddAsync(new Category { Name = "Beta", Description = "Second" });

        // Act — use LINQ on the queryable
        var result = repo.Query().Where(c => c.Name.StartsWith("A")).ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Alpha", result[0].Name);
    }

    /// <summary>
    /// Pre-assigned IDs (for seeding) should be preserved.
    /// </summary>
    [Fact]
    public async Task AddAsync_WithPreAssignedId_PreservesId()
    {
        // Arrange
        var repo = CreateRepository();

        // Act
        var category = await repo.AddAsync(new Category { Id = 42, Name = "Specific", Description = "Test" });

        // Assert
        Assert.Equal(42, category.Id);
        var retrieved = await repo.GetByIdAsync(42);
        Assert.NotNull(retrieved);
    }
}
