// =============================================================================
// File: IProductService.cs
// Layer: Application
// Purpose: Interface defining the product service contract.
//          Orchestrates repository, search engine, and caching operations.
// =============================================================================

using ProductCatalog.Application.DTOs;

namespace ProductCatalog.Application.Services;

/// <summary>
/// Service interface for product operations. Acts as the application layer
/// orchestrator between controllers and the domain/infrastructure layers.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves a paginated, filtered list of products.
    /// Supports search by name, category filtering, price range, and pagination.
    /// </summary>
    /// <param name="request">Search/filter/pagination parameters.</param>
    /// <returns>A paged result of product DTOs.</returns>
    Task<PagedResult<ProductDto>> GetProductsAsync(ProductSearchRequest request);

    /// <summary>
    /// Retrieves a single product by its unique identifier.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The product DTO.</returns>
    /// <exception cref="Domain.Exceptions.NotFoundException">Thrown if the product doesn't exist.</exception>
    Task<ProductDto> GetByIdAsync(int id);

    /// <summary>
    /// Creates a new product in the catalog.
    /// </summary>
    /// <param name="dto">The product creation data.</param>
    /// <returns>The created product DTO with generated ID and timestamps.</returns>
    /// <exception cref="Domain.Exceptions.ValidationException">Thrown if validation fails.</exception>
    /// <exception cref="Domain.Exceptions.DuplicateException">Thrown if SKU already exists.</exception>
    Task<ProductDto> CreateAsync(CreateProductDto dto);

    /// <summary>
    /// Updates an existing product's information.
    /// </summary>
    /// <param name="id">The ID of the product to update.</param>
    /// <param name="dto">The updated product data.</param>
    /// <returns>The updated product DTO.</returns>
    /// <exception cref="Domain.Exceptions.NotFoundException">Thrown if the product doesn't exist.</exception>
    /// <exception cref="Domain.Exceptions.ValidationException">Thrown if validation fails.</exception>
    Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto);

    /// <summary>
    /// Deletes a product from the catalog.
    /// </summary>
    /// <param name="id">The ID of the product to delete.</param>
    /// <returns>True if the product was found and deleted.</returns>
    /// <exception cref="Domain.Exceptions.NotFoundException">Thrown if the product doesn't exist.</exception>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Performs a fuzzy search across products using the ProductSearchEngine.
    /// Results are cached to improve performance on repeated queries.
    /// </summary>
    /// <param name="query">The search query string.</param>
    /// <returns>A list of matching product DTOs ordered by relevance.</returns>
    Task<List<ProductDto>> SearchAsync(string query);
}
