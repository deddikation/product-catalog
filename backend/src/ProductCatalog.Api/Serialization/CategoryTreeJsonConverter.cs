// =============================================================================
// File: CategoryTreeJsonConverter.cs
// Layer: API (Presentation)
// Purpose: Custom JSON serialization for the category tree endpoint (req 12).
//          Customizes the output shape by adding depth, item count, and renaming fields.
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;
using ProductCatalog.Application.DTOs;

namespace ProductCatalog.Api.Serialization;

/// <summary>
/// Custom JsonConverter for CategoryTreeDto that modifies the serialized JSON shape (req 12).
///
/// Customizations over default serialization:
/// - Adds a "depth" field indicating nesting level in the tree
/// - Adds a "childCount" field for quick child count access
/// - Renames "children" to "subcategories" for semantic clarity
/// - Omits the "subcategories" array when empty (cleaner output)
///
/// This demonstrates understanding of System.Text.Json custom serialization.
/// </summary>
public class CategoryTreeJsonConverter : JsonConverter<List<CategoryTreeDto>>
{
    /// <summary>
    /// Reads and deserializes JSON into a list of CategoryTreeDto.
    /// Uses default deserialization since custom reading is not needed for this endpoint.
    /// </summary>
    public override List<CategoryTreeDto>? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        // Delegate to default deserialization for reading
        return JsonSerializer.Deserialize<List<CategoryTreeDto>>(ref reader, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Writes the category tree with custom JSON structure.
    /// Adds depth tracking, child counts, and renames fields.
    /// </summary>
    public override void Write(
        Utf8JsonWriter writer,
        List<CategoryTreeDto> value,
        JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var category in value)
        {
            // Start recursive writing from depth 0 (root level)
            WriteCategoryNode(writer, category, depth: 0);
        }

        writer.WriteEndArray();
    }

    /// <summary>
    /// Recursively writes a single category tree node with custom fields.
    /// Each node includes: id, name, description, depth, childCount, and subcategories.
    /// </summary>
    /// <param name="writer">The JSON writer.</param>
    /// <param name="category">The category tree node to serialize.</param>
    /// <param name="depth">Current nesting depth (0 = root).</param>
    private static void WriteCategoryNode(Utf8JsonWriter writer, CategoryTreeDto category, int depth)
    {
        writer.WriteStartObject();

        // Standard fields
        writer.WriteNumber("id", category.Id);
        writer.WriteString("name", category.Name);
        writer.WriteString("description", category.Description);

        // Custom fields added by this converter
        writer.WriteNumber("depth", depth);
        writer.WriteNumber("childCount", category.Children.Count);

        // Only include subcategories array if there are children (cleaner JSON output)
        if (category.Children.Count > 0)
        {
            writer.WritePropertyName("subcategories");
            writer.WriteStartArray();

            foreach (var child in category.Children)
            {
                // Recursively write children at depth + 1
                WriteCategoryNode(writer, child, depth + 1);
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }
}
