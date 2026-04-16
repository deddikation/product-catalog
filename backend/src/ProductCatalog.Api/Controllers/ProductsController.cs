// =============================================================================
// File: ProductsController.cs
// Layer: API (Presentation)
// Purpose: RESTful API controller for product CRUD operations and search.
//          Demonstrates manual model binding (req 11), pattern matching (req 5),
//          and DI injection of the ProductService (req 13).
// =============================================================================

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Services;

namespace ProductCatalog.Api.Controllers;

/// <summary>
/// API controller for product management operations.
///
/// Endpoints:
/// - GET    /api/products         — Paginated list with filtering and search
/// - GET    /api/products/{id}    — Get single product by ID
/// - POST   /api/products         — Create new product
/// - PUT    /api/products/{id}    — Update product (uses manual model binding - req 11)
/// - DELETE /api/products/{id}    — Delete product
/// - GET    /api/products/search  — Fuzzy search using ProductSearchEngine
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    /// <summary>Injected product service for business logic.</summary>
    private readonly IProductService _productService;

    /// <summary>
    /// Constructs the controller with the product service injected via DI (req 13).
    /// </summary>
    /// <param name="productService">The product service implementation.</param>
    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Retrieves a paginated list of products with optional filtering.
    /// Supports search by name, category filtering, price range, and pagination.
    ///
    /// GET /api/products?searchTerm=laptop&amp;categoryId=1&amp;page=1&amp;pageSize=10
    /// </summary>
    /// <param name="searchTerm">Optional text search filter.</param>
    /// <param name="categoryId">Optional category ID filter.</param>
    /// <param name="minPrice">Optional minimum price filter.</param>
    /// <param name="maxPrice">Optional maximum price filter.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Items per page (default: 10).</param>
    /// <returns>Paginated list of products wrapped in ApiResponse.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ProductDto>>>> GetProducts(
        [FromQuery] string? searchTerm = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var request = new ProductSearchRequest(searchTerm, categoryId, minPrice, maxPrice, page, pageSize);
        var result = await _productService.GetProductsAsync(request);
        return Ok(ApiResponse<PagedResult<ProductDto>>.Ok(result));
    }

    /// <summary>
    /// Retrieves a single product by its unique identifier.
    ///
    /// GET /api/products/{id}
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The product wrapped in ApiResponse, or 404 if not found.</returns>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        return Ok(ApiResponse<ProductDto>.Ok(product));
    }

    /// <summary>
    /// Creates a new product in the catalog.
    /// Uses standard [FromBody] model binding for POST operations.
    ///
    /// POST /api/products
    /// </summary>
    /// <param name="dto">The product creation data.</param>
    /// <returns>The created product with HTTP 201 Created.</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct(
        [FromBody] CreateProductDto dto)
    {
        var product = await _productService.CreateAsync(dto);
        return CreatedAtAction(
            nameof(GetProduct),
            new { id = product.Id },
            ApiResponse<ProductDto>.Ok(product));
    }

    /// <summary>
    /// Updates an existing product using MANUAL MODEL BINDING (req 11).
    ///
    /// Instead of using [FromBody], this action manually reads the request body
    /// as a raw stream and deserializes it using System.Text.Json.
    /// This demonstrates understanding of the model binding pipeline and
    /// how to bypass it when needed.
    ///
    /// PUT /api/products/{id}
    /// </summary>
    /// <param name="id">The product ID to update.</param>
    /// <returns>The updated product wrapped in ApiResponse.</returns>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(int id)
    {
        // =====================================================================
        // MANUAL MODEL BINDING (req 11)
        // Instead of [FromBody] attribute, we read the raw request body stream
        // and deserialize it manually using System.Text.Json.
        // =====================================================================

        // Step 1: Read the raw request body as a stream
        UpdateProductDto? dto;
        try
        {
            // Configure JSON deserialization options for camelCase property matching
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Manually deserialize the request body stream
            dto = await JsonSerializer.DeserializeAsync<UpdateProductDto>(
                Request.Body, jsonOptions);
        }
        catch (JsonException ex)
        {
            // Handle malformed JSON input
            return BadRequest(ApiResponse<ProductDto>.Fail($"Invalid JSON format: {ex.Message}"));
        }

        // Step 2: Validate the manually bound model
        if (dto is null)
        {
            return BadRequest(ApiResponse<ProductDto>.Fail("Request body is required."));
        }

        // Step 3: Proceed with the update using the manually bound DTO
        var product = await _productService.UpdateAsync(id, dto);
        return Ok(ApiResponse<ProductDto>.Ok(product));
    }

    /// <summary>
    /// Deletes a product from the catalog.
    ///
    /// DELETE /api/products/{id}
    /// </summary>
    /// <param name="id">The product ID to delete.</param>
    /// <returns>Success response or 404 if not found.</returns>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProduct(int id)
    {
        var result = await _productService.DeleteAsync(id);
        return Ok(ApiResponse<bool>.Ok(result));
    }

    /// <summary>
    /// Performs a fuzzy search across all products using the ProductSearchEngine.
    /// Supports typo tolerance (e.g., "lptop" matches "laptop").
    ///
    /// GET /api/products/search?q=laptop
    /// </summary>
    /// <param name="q">The search query string.</param>
    /// <returns>List of matching products ordered by relevance.</returns>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<ProductDto>>>> SearchProducts(
        [FromQuery] string q = "")
    {
        // Validate search query using pattern matching
        if (q is null or { Length: 0 })
        {
            return BadRequest(ApiResponse<List<ProductDto>>.Fail("Search query 'q' is required."));
        }

        var results = await _productService.SearchAsync(q);
        return Ok(ApiResponse<List<ProductDto>>.Ok(results));
    }
}
