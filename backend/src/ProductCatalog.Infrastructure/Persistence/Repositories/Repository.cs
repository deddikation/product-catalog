// =============================================================================
// File: Repository.cs
// Layer: Infrastructure
// Purpose: Generic repository base class using EF Core for persistence (req 1).
//          Provides standard CRUD operations that concrete repositories inherit.
// =============================================================================

using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.Infrastructure.Persistence.Repositories;

/// <summary>
/// Generic repository base class implementing IRepository&lt;T&gt; using EF Core (req 1).
/// Provides standard CRUD operations backed by the AppDbContext.
/// Concrete repositories extend this class to add entity-specific operations.
/// </summary>
/// <typeparam name="T">The entity type this repository manages.</typeparam>
public class Repository<T> : IRepository<T> where T : class
{
    /// <summary>The EF Core database context.</summary>
    protected readonly AppDbContext _context;

    /// <summary>The DbSet for the managed entity type.</summary>
    protected readonly DbSet<T> _dbSet;

    /// <summary>
    /// Constructs the repository with the provided DbContext.
    /// </summary>
    /// <param name="context">The EF Core database context.</param>
    public Repository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <inheritdoc />
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    /// <inheritdoc />
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    /// <inheritdoc />
    public virtual async Task<T> AddAsync(T entity)
    {
        var entry = await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entry.Entity;
    }

    /// <inheritdoc />
    public virtual async Task<T> UpdateAsync(T entity)
    {
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    /// <inheritdoc />
    public virtual async Task<bool> DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity is null) return false;

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public virtual IQueryable<T> Query()
    {
        return _dbSet.AsQueryable();
    }
}
