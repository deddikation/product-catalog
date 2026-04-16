// =============================================================================
// File: CategoryService.cs
// Layer: Application
// Purpose: Implements category business logic including hierarchical tree
//          construction from flat category data (req 9).
// =============================================================================

using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Mapping;
using ProductCatalog.Application.Validation;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Exceptions;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.Application.Services;

/// <summary>
/// Application service for category operations. Handles flat listing,
/// hierarchical tree construction, and category creation with validation.
/// </summary>
public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    /// <summary>
    /// Constructs the CategoryService with the category repository injected.
    /// </summary>
    /// <param name="categoryRepository">Repository for category persistence.</param>
    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    /// <inheritdoc />
    public async Task<List<CategoryDto>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(c => c.ToDto()).ToList();
    }

    /// <inheritdoc />
    public async Task<List<CategoryTreeDto>> GetTreeAsync()
    {
        // Retrieve all categories as a flat list
        var allCategories = (await _categoryRepository.GetAllAsync()).ToList();

        // Build lookup dictionary for O(1) parent resolution
        var lookup = allCategories.ToDictionary(c => c.Id);

        // Build tree by assigning children to their parents
        // Step 1: Initialize SubCategories lists (clear any stale navigation data)
        foreach (var category in allCategories)
        {
            category.SubCategories = new List<Category>();
        }

        // Step 2: Link children to parents
        foreach (var category in allCategories)
        {
            if (category.ParentCategoryId.HasValue &&
                lookup.TryGetValue(category.ParentCategoryId.Value, out var parent))
            {
                parent.SubCategories.Add(category);
            }
        }

        // Step 3: Root categories are those with no parent — map to tree DTOs
        var rootCategories = allCategories
            .Where(c => c.ParentCategoryId is null)
            .Select(c => c.ToTreeDto())
            .ToList();

        return rootCategories;
    }

    /// <inheritdoc />
    public async Task<CategoryDto> CreateAsync(CreateCategoryDto dto)
    {
        // Validate using pattern matching (req 5)
        var (isValid, errors) = CategoryValidator.Validate(dto);
        if (!isValid)
            throw new Domain.Exceptions.ValidationException(errors);

        // Check for duplicate name
        var exists = await _categoryRepository.ExistsByNameAsync(dto.Name);
        if (exists)
            throw new DuplicateException("Name", dto.Name);

        // Verify parent category exists if specified
        if (dto.ParentCategoryId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(dto.ParentCategoryId.Value);
            if (parent is null)
                throw new NotFoundException(nameof(Category), dto.ParentCategoryId.Value);
        }

        // Map and persist
        var category = dto.ToEntity();
        var created = await _categoryRepository.AddAsync(category);

        return created.ToDto();
    }
}
