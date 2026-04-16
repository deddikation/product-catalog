// =============================================================================
// File: CategoryValidator.cs
// Layer: Application
// Purpose: Validates category DTOs using C# pattern matching (req 5).
// =============================================================================

using ProductCatalog.Application.DTOs;

namespace ProductCatalog.Application.Validation;

/// <summary>
/// Static validator for category DTOs using C# pattern matching features.
/// </summary>
public static class CategoryValidator
{
    /// <summary>
    /// Validates a CreateCategoryDto using pattern matching switch expressions.
    /// </summary>
    /// <param name="dto">The category creation DTO to validate.</param>
    /// <returns>A tuple indicating validity and any error messages.</returns>
    public static (bool IsValid, List<string> Errors) Validate(CreateCategoryDto? dto)
    {
        var errors = new List<string>();

        // Pattern matching: null check
        if (dto is null)
        {
            errors.Add("Category data is required.");
            return (false, errors);
        }

        // Validate Name using property pattern matching
        var nameError = dto.Name switch
        {
            null or "" => "Category name is required.",
            { Length: > 100 } => "Category name must not exceed 100 characters.",
            _ => null
        };
        if (nameError is not null) errors.Add(nameError);

        // Validate Description using property pattern matching
        var descError = dto.Description switch
        {
            { Length: > 500 } => "Category description must not exceed 500 characters.",
            _ => null
        };
        if (descError is not null) errors.Add(descError);

        // Validate ParentCategoryId using relational pattern matching
        var parentError = dto.ParentCategoryId switch
        {
            < 0 => "ParentCategoryId must be a positive integer or null.",
            0 => "ParentCategoryId must be a positive integer or null.",
            _ => null
        };
        if (parentError is not null) errors.Add(parentError);

        return (errors.Count == 0, errors);
    }
}
