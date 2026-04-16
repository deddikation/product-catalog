// =============================================================================
// File: DuplicateException.cs
// Layer: Domain
// Purpose: Domain exception thrown when a uniqueness constraint is violated
//          (e.g., duplicate SKU). Maps to HTTP 409 Conflict.
// =============================================================================

namespace ProductCatalog.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to create or update an entity
/// that would violate a uniqueness constraint (e.g., duplicate SKU).
/// The exception handling middleware maps this to HTTP 409 Conflict.
/// </summary>
public class DuplicateException : Exception
{
    /// <summary>The name of the field that has a duplicate value.</summary>
    public string FieldName { get; }

    /// <summary>The duplicate value that caused the conflict.</summary>
    public object FieldValue { get; }

    /// <summary>
    /// Creates a new DuplicateException for a specific field and value.
    /// </summary>
    /// <param name="fieldName">Name of the field with the duplicate (e.g., "SKU").</param>
    /// <param name="fieldValue">The duplicate value.</param>
    public DuplicateException(string fieldName, object fieldValue)
        : base($"A record with {fieldName} '{fieldValue}' already exists.")
    {
        FieldName = fieldName;
        FieldValue = fieldValue;
    }
}
