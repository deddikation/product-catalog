// =============================================================================
// File: CategoryRepository.cs
// Layer: Infrastructure
// Purpose: Pure in-memory category repository using List<Category> and
//          Dictionary<int, Category> — NO Entity Framework (req 2).
//          Demonstrates working with core collections for data persistence.
// =============================================================================

using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.Infrastructure.InMemory;

/// <summary>
/// In-memory category repository that uses pure .NET collections instead of EF Core (req 2).
///
/// Backing stores:
/// - List&lt;Category&gt; for ordered enumeration and LINQ queries
/// - Dictionary&lt;int, Category&gt; for O(1) lookups by ID
///
/// Thread-safe using lock for concurrent access protection.
/// This implementation satisfies the requirement to have at least one repository
/// using pure in-memory collections (List, Dictionary) instead of Entity Framework.
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    /// <summary>Ordered list of all categories for enumeration.</summary>
    private readonly List<Category> _categories = new();

    /// <summary>Dictionary index for O(1) lookups by category ID.</summary>
    private readonly Dictionary<int, Category> _categoryIndex = new();

    /// <summary>Lock object for thread-safe concurrent access.</summary>
    private readonly object _lock = new();

    /// <summary>Auto-incrementing ID counter for new categories.</summary>
    private int _nextId = 1;

    /// <inheritdoc />
    public Task<Category?> GetByIdAsync(int id)
    {
        lock (_lock)
        {
            // O(1) lookup using the dictionary index
            _categoryIndex.TryGetValue(id, out var category);
            return Task.FromResult(category);
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<Category>> GetAllAsync()
    {
        lock (_lock)
        {
            // Return a copy of the list to prevent external modification
            var snapshot = _categories.ToList();
            return Task.FromResult<IEnumerable<Category>>(snapshot);
        }
    }

    /// <inheritdoc />
    public Task<Category> AddAsync(Category entity)
    {
        lock (_lock)
        {
            // Assign ID if not pre-assigned (for seeding, IDs may be pre-set)
            if (entity.Id == 0)
            {
                entity.Id = _nextId++;
            }
            else
            {
                // Ensure _nextId stays ahead of manually assigned IDs
                if (entity.Id >= _nextId)
                {
                    _nextId = entity.Id + 1;
                }
            }

            // Add to both backing stores
            _categories.Add(entity);
            _categoryIndex[entity.Id] = entity;

            return Task.FromResult(entity);
        }
    }

    /// <inheritdoc />
    public Task<Category> UpdateAsync(Category entity)
    {
        lock (_lock)
        {
            // Find and replace the existing entry
            var index = _categories.FindIndex(c => c.Id == entity.Id);
            if (index >= 0)
            {
                _categories[index] = entity;
                _categoryIndex[entity.Id] = entity;
            }

            return Task.FromResult(entity);
        }
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(int id)
    {
        lock (_lock)
        {
            // Remove from both backing stores
            var removed = _categoryIndex.Remove(id);
            if (removed)
            {
                _categories.RemoveAll(c => c.Id == id);
            }

            return Task.FromResult(removed);
        }
    }

    /// <inheritdoc />
    public IQueryable<Category> Query()
    {
        lock (_lock)
        {
            // Return an IQueryable snapshot for LINQ compatibility
            return _categories.ToList().AsQueryable();
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<Category>> GetChildrenAsync(int parentId)
    {
        lock (_lock)
        {
            // Filter categories whose parent matches the given ID
            var children = _categories
                .Where(c => c.ParentCategoryId == parentId)
                .ToList();

            return Task.FromResult<IEnumerable<Category>>(children);
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<Category>> GetRootCategoriesAsync()
    {
        lock (_lock)
        {
            // Root categories have no parent (ParentCategoryId is null)
            var roots = _categories
                .Where(c => c.ParentCategoryId is null)
                .ToList();

            return Task.FromResult<IEnumerable<Category>>(roots);
        }
    }

    /// <inheritdoc />
    public Task<bool> ExistsByNameAsync(string name)
    {
        lock (_lock)
        {
            // Case-insensitive name comparison
            var exists = _categories
                .Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            return Task.FromResult(exists);
        }
    }
}
