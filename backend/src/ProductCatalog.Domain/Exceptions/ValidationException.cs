// =============================================================================
// File: ValidationException.cs
// Layer: Domain
// Purpose: Domain exception thrown when input validation fails.
//          Used by the exception handling middleware for HTTP 400 responses.
// =============================================================================

namespace ProductCatalog.Domain.Exceptions;

/// <summary>
/// Exception thrown when request validation fails.
/// The exception handling middleware maps this to HTTP 400 Bad Request.
/// </summary>
public class ValidationException : Exception
{
    /// <summary>Collection of individual validation error messages.</summary>
    public IReadOnlyList<string> Errors { get; }

    /// <summary>
    /// Creates a new ValidationException with a single error message.
    /// </summary>
    /// <param name="error">The validation error message.</param>
    public ValidationException(string error)
        : base(error)
    {
        Errors = new List<string> { error };
    }

    /// <summary>
    /// Creates a new ValidationException with multiple error messages.
    /// </summary>
    /// <param name="errors">The collection of validation error messages.</param>
    public ValidationException(IEnumerable<string> errors)
        : base(string.Join("; ", errors))
    {
        Errors = errors.ToList().AsReadOnly();
    }
}
