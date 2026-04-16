// =============================================================================
// File: ProductService.cs
// Layer: Application
// Purpose: Implements product business logic, orchestrating the repository,
//          search engine, and caching layer. Demonstrates DI pattern (req 13).
// =============================================================================

using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Extensions;
using ProductCatalog.Application.Mapping;
using ProductCatalog.Application.Validation;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Exceptions;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.Application.Services;

/// <summary>
/// Application service for product operations. Orchestrates:
/// - IProductRepository for CRUD persistence
/// - ISearchEngine&lt;Product&gt; for fuzzy search (req 13 — DI injected)
/// - ISearchCache for caching search results (req 8)
///
/// All dependencies are injected via constructor injection following the DI pattern.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISearchEngine<Product> _searchEngine;
    private readonly ISearchCache _searchCache;

    /// <summary>
    /// Constructs the ProductService with all required dependencies injected.
    /// </summary>
    /// <param name="productRepository">Repository for product persistence.</param>
    /// <param name="categoryRepository">Repository for category lookups.</param>
    /// <param name="searchEngine">Search engine for fuzzy product search (req 13).</param>
    /// <param name="searchCache">Cache for search result memoization (req 8).</param>
    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ISearchEngine<Product> searchEngine,
        ISearchCache searchCache)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _searchEngine = searchEngine;
        _searchCache = searchCache;
    }

    /// <inheritdoc />
    public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductSearchRequest request)
    {
        // Build query using custom LINQ extension methods (req 3)
        var query = _productRepository.Query()
            .FilterByCategory(request.CategoryId)
            .SearchByName(request.SearchTerm)
            .InPriceRange(request.MinPrice, request.MaxPrice)
            .SortByDefault();

        // Get total count before pagination for metadata
        var totalCount = query.Count();

        // Apply pagination using custom extension method
        var items = query
            .Paginate(request.Page, request.PageSize)
            .ToList();

        // Map entities to DTOs — need to load category names
        var productDtos = new List<ProductDto>();
        foreach (var product in items)
        {
            // Load category if not already included
            if (product.Category is null && product.CategoryId > 0)
            {
                product.Category = (await _categoryRepository.GetByIdAsync(product.CategoryId));
            }
            productDtos.Add(product.ToDto());
        }

        return new PagedResult<ProductDto>(productDtos, totalCount, request.Page, request.PageSize);
    }

    /// <inheritdoc />
    public async Task<ProductDto> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdWithCategoryAsync(id);

        // Throw domain exception if not found — middleware converts to HTTP 404
        if (product is null)
            throw new NotFoundException(nameof(Product), id);

        // Populate Category from the in-memory ICategoryRepository (req 2),
        // since categories are not stored in the EF database.
        if (product.Category is null && product.CategoryId > 0)
            product.Category = await _categoryRepository.GetByIdAsync(product.CategoryId);

        return product.ToDto();
    }

    /// <inheritdoc />
    public async Task<ProductDto> CreateAsync(CreateProductDto dto)
    {
        // Validate using pattern matching (req 5)
        var (isValid, errors) = ProductValidator.Validate(dto);
        if (!isValid)
            throw new Domain.Exceptions.ValidationException(errors);

        // Check for duplicate SKU
        var existingProduct = await _productRepository.GetBySkuAsync(dto.SKU);
        if (existingProduct is not null)
            throw new DuplicateException("SKU", dto.SKU);

        // Verify category exists
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category is null)
            throw new NotFoundException(nameof(Category), dto.CategoryId);

        // Map DTO to entity and persist
        var product = dto.ToEntity();
        var created = await _productRepository.AddAsync(product);

        // Invalidate search cache since data has changed
        _searchCache.Clear();

        // Reload with category for complete DTO
        created.Category = category;
        return created.ToDto();
    }

    /// <inheritdoc />
    public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto)
    {
        // Validate using pattern matching (req 5)
        var (isValid, errors) = ProductValidator.Validate(dto);
        if (!isValid)
            throw new Domain.Exceptions.ValidationException(errors);

        // Verify product exists
        var existing = await _productRepository.GetByIdAsync(id);
        if (existing is null)
            throw new NotFoundException(nameof(Product), id);

        // Check for SKU conflict with other products
        var skuConflict = await _productRepository.GetBySkuAsync(dto.SKU);
        if (skuConflict is not null && skuConflict.Id != id)
            throw new DuplicateException("SKU", dto.SKU);

        // Verify category exists
        var category = await _categoryRepository.GetByIdAsync(dto.CategoryId);
        if (category is null)
            throw new NotFoundException(nameof(Category), dto.CategoryId);

        // Apply updates and persist
        dto.ApplyTo(existing);
        var updated = await _productRepository.UpdateAsync(existing);

        // Invalidate search cache
        _searchCache.Clear();

        updated.Category = category;
        return updated.ToDto();
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(int id)
    {
        // Verify product exists before deletion
        var existing = await _productRepository.GetByIdAsync(id);
        if (existing is null)
            throw new NotFoundException(nameof(Product), id);

        var result = await _productRepository.DeleteAsync(id);

        // Invalidate search cache
        _searchCache.Clear();

        return result;
    }

    /// <inheritdoc />
    public async Task<List<ProductDto>> SearchAsync(string query)
    {
        // Normalize query for consistent cache keys
        var cacheKey = $"search:{query.Trim().ToLowerInvariant()}";

        // Check cache first (req 8)
        if (_searchCache.TryGet<List<ProductDto>>(cacheKey, out var cachedResults) && cachedResults is not null)
        {
            return cachedResults;
        }

        // Load all products; populate Category from ICategoryRepository (req 2)
        // since categories are not stored in the EF database.
        var allProducts = (await _productRepository.GetAllWithCategoryAsync()).ToList();
        foreach (var p in allProducts.Where(p => p.Category is null && p.CategoryId > 0))
            p.Category = await _categoryRepository.GetByIdAsync(p.CategoryId);

        // Execute fuzzy search using the ProductSearchEngine (req 13 — DI injected)
        var searchResults = _searchEngine.Search(query, allProducts);

        // Map results to DTOs, ordered by relevance score
        var results = searchResults
            .Select(r => r.Item.ToDto())
            .ToList();

        // Cache the results for future identical queries
        _searchCache.Set(cacheKey, results);

        return results;
    }
}
