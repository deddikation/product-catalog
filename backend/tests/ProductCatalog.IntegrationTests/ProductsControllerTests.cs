// =============================================================================
// File: ProductsControllerTests.cs
// Layer: Integration Tests
// Purpose: End-to-end tests for the Products API endpoints.
//          Tests the full request pipeline including middleware, validation,
//          and data persistence.
// =============================================================================

using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using ProductCatalog.Application.DTOs;

namespace ProductCatalog.IntegrationTests;

/// <summary>
/// Integration tests for the /api/products endpoints.
/// Tests the full HTTP request lifecycle from controller to data layer.
/// </summary>
public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ProductsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        // Seed data synchronously for test setup
        factory.SeedTestDataAsync().GetAwaiter().GetResult();
    }

    // =====================================================================
    // GET /api/products Tests
    // =====================================================================

    /// <summary>
    /// GET /api/products should return HTTP 200 with paginated results.
    /// </summary>
    [Fact]
    public async Task GetProducts_ReturnsOkWithPagedResults()
    {
        // Act
        var response = await _client.GetAsync("/api/products");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PagedResult<ProductDto>>>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Items.Count > 0);
    }

    /// <summary>
    /// GET /api/products with pagination should return the correct page.
    /// </summary>
    [Fact]
    public async Task GetProducts_WithPagination_ReturnsCorrectPage()
    {
        // Act
        var response = await _client.GetAsync("/api/products?page=1&pageSize=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PagedResult<ProductDto>>>(content, _jsonOptions);

        Assert.NotNull(result?.Data);
        Assert.True(result.Data.Items.Count <= 5);
        Assert.Equal(1, result.Data.Page);
        Assert.Equal(5, result.Data.PageSize);
    }

    /// <summary>
    /// GET /api/products with category filter should return filtered results.
    /// </summary>
    [Fact]
    public async Task GetProducts_WithCategoryFilter_ReturnsFilteredResults()
    {
        // Act — filter by category ID 2 (Computers)
        var response = await _client.GetAsync("/api/products?categoryId=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<PagedResult<ProductDto>>>(content, _jsonOptions);

        Assert.NotNull(result?.Data);
        Assert.All(result.Data.Items, p => Assert.Equal(2, p.CategoryId));
    }

    // =====================================================================
    // GET /api/products/{id} Tests
    // =====================================================================

    /// <summary>
    /// GET /api/products/{id} for an existing product should return HTTP 200.
    /// </summary>
    [Fact]
    public async Task GetProduct_ExistingId_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/products/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(content, _jsonOptions);

        Assert.NotNull(result?.Data);
        Assert.Equal(1, result.Data.Id);
    }

    /// <summary>
    /// GET /api/products/{id} for non-existent product should return HTTP 404.
    /// </summary>
    [Fact]
    public async Task GetProduct_NonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/products/9999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // =====================================================================
    // POST /api/products Tests
    // =====================================================================

    /// <summary>
    /// POST /api/products with valid data should return HTTP 201 Created.
    /// </summary>
    [Fact]
    public async Task CreateProduct_ValidData_ReturnsCreated()
    {
        // Arrange
        var dto = new CreateProductDto(
            "Test Product", "Test description", $"TEST-{Guid.NewGuid():N}"[..10],
            49.99m, 25, 1);

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(content, _jsonOptions);

        Assert.NotNull(result?.Data);
        Assert.Equal("Test Product", result.Data.Name);
        Assert.True(result.Data.Id > 0);
    }

    /// <summary>
    /// POST /api/products with invalid data should return HTTP 400.
    /// </summary>
    [Fact]
    public async Task CreateProduct_InvalidData_ReturnsBadRequest()
    {
        // Arrange — empty name and negative price
        var dto = new CreateProductDto("", "Description", "SK", -5m, -1, 0);

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // =====================================================================
    // PUT /api/products/{id} Tests (manual model binding)
    // =====================================================================

    /// <summary>
    /// PUT /api/products/{id} with valid data using manual body reading should return HTTP 200.
    /// This tests the manual model binding implementation (req 11).
    /// </summary>
    [Fact]
    public async Task UpdateProduct_ValidData_ReturnsOk()
    {
        // Arrange — first create a product to update
        var createDto = new CreateProductDto(
            "Update Me", "Original desc", $"UPD-{Guid.NewGuid():N}"[..8],
            99.99m, 10, 1);
        var createResponse = await _client.PostAsJsonAsync("/api/products", createDto);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(createContent, _jsonOptions);

        // Build update DTO
        var updateDto = new UpdateProductDto("Updated Name", "Updated desc", created!.Data!.SKU, 129.99m, 20, 1);
        var json = JsonSerializer.Serialize(updateDto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act — PUT with manual JSON body (no [FromBody] on the controller)
        var response = await _client.PutAsync($"/api/products/{created.Data.Id}", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(responseContent, _jsonOptions);

        Assert.Equal("Updated Name", result?.Data?.Name);
        Assert.Equal(129.99m, result?.Data?.Price);
    }

    // =====================================================================
    // DELETE /api/products/{id} Tests
    // =====================================================================

    /// <summary>
    /// DELETE /api/products/{id} for an existing product should return HTTP 200.
    /// </summary>
    [Fact]
    public async Task DeleteProduct_ExistingProduct_ReturnsOk()
    {
        // Arrange — create a product to delete
        var createDto = new CreateProductDto(
            "To Delete", "Will be deleted", $"DEL-{Guid.NewGuid():N}"[..8],
            9.99m, 1, 1);
        var createResponse = await _client.PostAsJsonAsync("/api/products", createDto);
        var createContent = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<ApiResponse<ProductDto>>(createContent, _jsonOptions);

        // Act
        var response = await _client.DeleteAsync($"/api/products/{created!.Data!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Verify product is gone
        var getResponse = await _client.GetAsync($"/api/products/{created.Data.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    // =====================================================================
    // GET /api/products/search Tests
    // =====================================================================

    /// <summary>
    /// GET /api/products/search should return matching products.
    /// </summary>
    [Fact]
    public async Task SearchProducts_WithQuery_ReturnsResults()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?q=laptop");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<ProductDto>>>(content, _jsonOptions);

        Assert.NotNull(result?.Data);
        Assert.True(result.Data.Count > 0);
    }

    /// <summary>
    /// GET /api/products/search without query should return HTTP 400.
    /// </summary>
    [Fact]
    public async Task SearchProducts_EmptyQuery_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync("/api/products/search?q=");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
