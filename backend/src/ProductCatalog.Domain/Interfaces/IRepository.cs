// =============================================================================
// File: IRepository.cs
// Layer: Domain (innermost layer - no external dependencies)
// Purpose: Generic repository interface providing a standard contract for
//          data access operations across all entity types (req 1).
// =============================================================================

namespace ProductCatalog.Domain.Interfaces;

/// <summary>
/// Generic repository interface defining standard CRUD operations.
/// Implementations may use EF Core, in-memory collections, or any other persistence mechanism.
/// </summary>
/// <typeparam name="T">The entity type this repository manages.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>Retrieves an entity by its unique identifier.</summary>
    /// <param name="id">The entity's unique identifier.</param>
    /// <returns>The entity if found; otherwise null.</returns>
    Task<T?> GetByIdAsync(int id);

    /// <summary>Retrieves all entities of type <typeparamref name="T"/>.</summary>
    /// <returns>An enumerable of all entities.</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>Adds a new entity to the repository.</summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>The added entity with any generated values (e.g., Id) populated.</returns>
    Task<T> AddAsync(T entity);

    /// <summary>Updates an existing entity in the repository.</summary>
    /// <param name="entity">The entity with updated values.</param>
    /// <returns>The updated entity.</returns>
    Task<T> UpdateAsync(T entity);

    /// <summary>Deletes an entity by its unique identifier.</summary>
    /// <param name="id">The entity's unique identifier.</param>
    /// <returns>True if the entity was found and deleted; false otherwise.</returns>
    Task<bool> DeleteAsync(int id);

    /// <summary>
    /// Returns a queryable for building custom queries against the data source.
    /// Enables use of LINQ extension methods for filtering and pagination.
    /// </summary>
    /// <returns>An IQueryable over the entity set.</returns>
    IQueryable<T> Query();
}
