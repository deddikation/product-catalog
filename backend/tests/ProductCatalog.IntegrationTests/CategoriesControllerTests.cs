// =============================================================================
// File: CategoriesControllerTests.cs
// Layer: Integration Tests
// Purpose: End-to-end tests for the Categories API endpoints.
//          Tests flat listing, tree structure, and category creation.
// =============================================================================

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ProductCatalog.Application.DTOs;

namespace ProductCatalog.IntegrationTests;

/// <summary>
/// Integration tests for the /api/categories endpoints.
/// Tests the full HTTP request lifecycle for category management.
/// </summary>
public class CategoriesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public CategoriesControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
        factory.SeedTestDataAsync().GetAwaiter().GetResult();
    }

    // =====================================================================
    // GET /api/categories Tests
    // =====================================================================

    /// <summary>
    /// GET /api/categories should return HTTP 200 with all categories as flat list.
    /// </summary>
    [Fact]
    public async Task GetCategories_ReturnsOkWithFlatList()
    {
        // Act
        var response = await _client.GetAsync("/api/categories");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<CategoryDto>>>(content, _jsonOptions);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Count > 0);
    }

    /// <summary>
    /// GET /api/categories should return categories with correct structure.
    /// </summary>
    [Fact]
    public async Task GetCategories_ReturnsCorrectCategoryStructure()
    {
        // Act
        var response = await _client.GetAsync("/api/categories");
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<List<CategoryDto>>>(content, _jsonOptions);

        // Assert — verify root categories have null ParentCategoryId
        Assert.NotNull(result?.Data);
        var rootCategories = result.Data.Where(c => c.ParentCategoryId == null).ToList();
        Assert.True(rootCategories.Count > 0, "Should have at least one root category");
    }

    // =====================================================================
    // GET /api/categories/tree Tests
    // =====================================================================

    /// <summary>
    /// GET /api/categories/tree should return HTTP 200 with custom JSON (req 12).
    /// </summary>
    [Fact]
    public async Task GetCategoryTree_ReturnsOkWithCustomJson()
    {
        // Act
        var response = await _client.GetAsync("/api/categories/tree");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);

        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);
        Assert.NotEqual("[]", content.Trim());
    }

    /// <summary>
    /// GET /api/categories/tree should include custom fields added by CategoryTreeJsonConverter.
    /// Verifies that "depth" and "childCount" fields are present (req 12 — custom JSON).
    /// </summary>
    [Fact]
    public async Task GetCategoryTree_HasCustomJsonFields()
    {
        // Act
        var response = await _client.GetAsync("/api/categories/tree");
        var content = await response.Content.ReadAsStringAsync();

        // Parse as raw JsonDocument to check custom fields
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        Assert.True(root.GetArrayLength() > 0);

        // Check that the custom serializer added "depth" and "childCount" fields
        var firstNode = root[0];
        Assert.True(firstNode.TryGetProperty("depth", out _), "Should have 'depth' field");
        Assert.True(firstNode.TryGetProperty("childCount", out _), "Should have 'childCount' field");
        Assert.True(firstNode.TryGetProperty("id", out _), "Should have 'id' field");
        Assert.True(firstNode.TryGetProperty("name", out _), "Should have 'name' field");
    }

    /// <summary>
    /// GET /api/categories/tree should nest children under parent nodes.
    /// </summary>
    [Fact]
    public async Task GetCategoryTree_NestedChildrenUnderParent()
    {
        // Act
        var response = await _client.GetAsync("/api/categories/tree");
        var content = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        // Find a node that has subcategories
        var hasSubcategories = false;
        foreach (var node in root.EnumerateArray())
        {
            if (node.TryGetProperty("subcategories", out var subs) &&
                subs.ValueKind == JsonValueKind.Array &&
                subs.GetArrayLength() > 0)
            {
                hasSubcategories = true;

                // Verify children have depth = 1
                var firstChild = subs[0];
                Assert.True(firstChild.TryGetProperty("depth", out var depth));
                Assert.Equal(1, depth.GetInt32());
                break;
            }
        }

        Assert.True(hasSubcategories, "Tree should contain at least one parent with children");
    }

    // =====================================================================
    // POST /api/categories Tests
    // =====================================================================

    /// <summary>
    /// POST /api/categories with valid data should return HTTP 201 Created.
    /// </summary>
    [Fact]
    public async Task CreateCategory_ValidData_ReturnsCreated()
    {
        // Arrange
        var dto = new CreateCategoryDto($"NewCat-{Guid.NewGuid():N}"[..15], "A new category", null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/categories", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ApiResponse<CategoryDto>>(content, _jsonOptions);

        Assert.NotNull(result?.Data);
        Assert.True(result.Data.Id > 0);
    }

    /// <summary>
    /// POST /api/categories with invalid data should return HTTP 400.
    /// </summary>
    [Fact]
    public async Task CreateCategory_InvalidData_ReturnsBadRequest()
    {
        // Arrange — empty name
        var dto = new CreateCategoryDto("", "Description", null);

        // Act
        var response = await _client.PostAsJsonAsync("/api/categories", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
