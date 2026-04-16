// =============================================================================
// File: NotFoundException.cs
// Layer: Domain
// Purpose: Domain exception thrown when a requested entity cannot be found.
//          Used by the exception handling middleware for HTTP 404 responses.
// =============================================================================

namespace ProductCatalog.Domain.Exceptions;

/// <summary>
/// Exception thrown when a requested resource does not exist in the system.
/// The exception handling middleware maps this to HTTP 404 Not Found.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>The name of the entity type that was not found.</summary>
    public string EntityName { get; }

    /// <summary>The identifier that was searched for.</summary>
    public object EntityId { get; }

    /// <summary>
    /// Creates a new NotFoundException for a specific entity and ID.
    /// </summary>
    /// <param name="entityName">Name of the entity type (e.g., "Product").</param>
    /// <param name="entityId">The ID that was not found.</param>
    public NotFoundException(string entityName, object entityId)
        : base($"{entityName} with ID '{entityId}' was not found.")
    {
        EntityName = entityName;
        EntityId = entityId;
    }
}
