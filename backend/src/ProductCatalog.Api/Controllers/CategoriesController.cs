// =============================================================================
// File: CategoriesController.cs
// Layer: API (Presentation)
// Purpose: RESTful API controller for category management.
//          Includes flat list, hierarchical tree (with custom JSON - req 12), and create.
// =============================================================================

using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.Api.Serialization;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Application.Services;

namespace ProductCatalog.Api.Controllers;

/// <summary>
/// API controller for category management operations.
///
/// Endpoints:
/// - GET  /api/categories      — Flat list of all categories
/// - GET  /api/categories/tree — Hierarchical tree with custom JSON serialization (req 12)
/// - POST /api/categories      — Create new category
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    /// <summary>Injected category service for business logic.</summary>
    private readonly ICategoryService _categoryService;

    /// <summary>
    /// Constructs the controller with the category service injected via DI.
    /// </summary>
    /// <param name="categoryService">The category service implementation.</param>
    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Retrieves all categories as a flat list.
    ///
    /// GET /api/categories
    /// </summary>
    /// <returns>Flat list of all categories wrapped in ApiResponse.</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories()
    {
        var categories = await _categoryService.GetAllAsync();
        return Ok(ApiResponse<List<CategoryDto>>.Ok(categories));
    }

    /// <summary>
    /// Retrieves categories as a hierarchical tree structure (req 9).
    /// Uses custom JSON serialization (req 12) via CategoryTreeJsonConverter
    /// which adds depth, childCount, and renames children to subcategories.
    ///
    /// GET /api/categories/tree
    /// </summary>
    /// <returns>Hierarchical category tree with custom JSON format.</returns>
    [HttpGet("tree")]
    public async Task<IActionResult> GetCategoryTree()
    {
        var tree = await _categoryService.GetTreeAsync();

        // =====================================================================
        // CUSTOM JSON SERIALIZATION (req 12)
        // Instead of returning the default serialized CategoryTreeDto,
        // we use our custom CategoryTreeJsonConverter to produce a
        // modified JSON shape with depth, childCount, and renamed fields.
        // =====================================================================
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new CategoryTreeJsonConverter() }
        };

        var json = JsonSerializer.Serialize(tree, jsonOptions);
        return Content(json, "application/json");
    }

    /// <summary>
    /// Creates a new category.
    ///
    /// POST /api/categories
    /// </summary>
    /// <param name="dto">The category creation data.</param>
    /// <returns>The created category wrapped in ApiResponse.</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory(
        [FromBody] CreateCategoryDto dto)
    {
        var category = await _categoryService.CreateAsync(dto);
        return CreatedAtAction(
            nameof(GetCategories),
            ApiResponse<CategoryDto>.Ok(category));
    }
}
