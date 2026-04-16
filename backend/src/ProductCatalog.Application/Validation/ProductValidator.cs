// =============================================================================
// File: ProductValidator.cs
// Layer: Application
// Purpose: Validates product DTOs using C# pattern matching (req 5).
//          Demonstrates switch expressions with property patterns, relational
//          patterns, and logical patterns for comprehensive input validation.
// =============================================================================

using ProductCatalog.Application.DTOs;

namespace ProductCatalog.Application.Validation;

/// <summary>
/// Static validator for product DTOs using C# pattern matching features.
/// Returns a tuple of (IsValid, ErrorMessages) for each validation operation.
/// </summary>
public static class ProductValidator
{
    /// <summary>
    /// Validates a CreateProductDto using pattern matching switch expressions.
    /// Checks all required fields, value constraints, and format rules.
    /// </summary>
    /// <param name="dto">The product creation DTO to validate.</param>
    /// <returns>A tuple indicating validity and any error messages.</returns>
    public static (bool IsValid, List<string> Errors) Validate(CreateProductDto? dto)
    {
        var errors = new List<string>();

        // Pattern matching: check for null input
        if (dto is null)
        {
            errors.Add("Product data is required.");
            return (false, errors);
        }

        // Pattern matching with property patterns and relational patterns (req 5)
        // Each rule uses a switch expression to evaluate a specific validation concern
        ValidateField(dto.Name, "Name", 1, 200, errors);
        ValidateField(dto.Description, "Description", 0, 2000, errors);
        ValidateSku(dto.SKU, errors);
        ValidatePrice(dto.Price, errors);
        ValidateQuantity(dto.Quantity, errors);
        ValidateCategoryId(dto.CategoryId, errors);

        return (errors.Count == 0, errors);
    }

    /// <summary>
    /// Validates an UpdateProductDto using the same rules as creation.
    /// </summary>
    /// <param name="dto">The product update DTO to validate.</param>
    /// <returns>A tuple indicating validity and any error messages.</returns>
    public static (bool IsValid, List<string> Errors) Validate(UpdateProductDto? dto)
    {
        // Convert to CreateProductDto for shared validation logic using pattern matching
        return dto switch
        {
            null => (false, new List<string> { "Product data is required." }),
            _ => Validate(new CreateProductDto(
                dto.Name, dto.Description, dto.SKU,
                dto.Price, dto.Quantity, dto.CategoryId))
        };
    }

    /// <summary>
    /// Validates a text field using pattern matching with relational patterns.
    /// Demonstrates C# property patterns and relational patterns (> < >= <=).
    /// </summary>
    /// <param name="value">The field value to validate.</param>
    /// <param name="fieldName">Name of the field for error messages.</param>
    /// <param name="minLength">Minimum allowed length.</param>
    /// <param name="maxLength">Maximum allowed length.</param>
    /// <param name="errors">Error list to append to.</param>
    private static void ValidateField(string? value, string fieldName, int minLength, int maxLength, List<string> errors)
    {
        // Pattern matching with property patterns and relational patterns
        var error = value switch
        {
            null or "" when minLength > 0 => $"{fieldName} is required.",
            { Length: var len } when len < minLength => $"{fieldName} must be at least {minLength} characters.",
            { Length: var len } when len > maxLength => $"{fieldName} must not exceed {maxLength} characters.",
            _ => null
        };

        if (error is not null) errors.Add(error);
    }

    /// <summary>
    /// Validates SKU format using pattern matching.
    /// SKU must be non-empty and between 2-50 characters.
    /// </summary>
    private static void ValidateSku(string? sku, List<string> errors)
    {
        var error = sku switch
        {
            null or "" => "SKU is required.",
            { Length: < 2 } => "SKU must be at least 2 characters.",
            { Length: > 50 } => "SKU must not exceed 50 characters.",
            _ => null
        };

        if (error is not null) errors.Add(error);
    }

    /// <summary>
    /// Validates price using relational pattern matching.
    /// Price must be non-negative and not exceed a reasonable maximum.
    /// </summary>
    private static void ValidatePrice(decimal price, List<string> errors)
    {
        var error = price switch
        {
            < 0 => "Price must be non-negative.",
            > 999_999.99m => "Price must not exceed 999,999.99.",
            _ => null
        };

        if (error is not null) errors.Add(error);
    }

    /// <summary>
    /// Validates quantity using relational pattern matching.
    /// Quantity must be non-negative.
    /// </summary>
    private static void ValidateQuantity(int quantity, List<string> errors)
    {
        var error = quantity switch
        {
            < 0 => "Quantity must be non-negative.",
            > 999_999 => "Quantity must not exceed 999,999.",
            _ => null
        };

        if (error is not null) errors.Add(error);
    }

    /// <summary>
    /// Validates CategoryId using relational pattern matching.
    /// Must be a positive integer referencing an existing category.
    /// </summary>
    private static void ValidateCategoryId(int categoryId, List<string> errors)
    {
        var error = categoryId switch
        {
            <= 0 => "CategoryId must be a positive integer.",
            _ => null
        };

        if (error is not null) errors.Add(error);
    }
}
