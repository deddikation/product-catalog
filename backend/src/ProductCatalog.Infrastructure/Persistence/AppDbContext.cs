// =============================================================================
// File: AppDbContext.cs
// Layer: Infrastructure
// Purpose: Entity Framework Core DbContext configured with InMemory database.
//          Defines entity relationships, indexes, and constraints.
// =============================================================================

using Microsoft.EntityFrameworkCore;
using ProductCatalog.Domain.Entities;

namespace ProductCatalog.Infrastructure.Persistence;

/// <summary>
/// EF Core database context for the Product Catalog system.
/// Configured to use InMemory database for development/assessment purposes.
/// Defines the schema including relationships, indexes, and constraints.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>Products table.</summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>Categories table — note: only used for EF schema definition.
    /// Actual category persistence uses the in-memory CategoryRepository (req 2).</summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>
    /// Constructs the DbContext with the provided options (typically InMemory).
    /// </summary>
    /// <param name="options">EF Core configuration options.</param>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configures entity relationships, indexes, and constraints.
    /// </summary>
    /// <param name="modelBuilder">The model builder for Fluent API configuration.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // =====================================================================
        // Product Configuration
        // =====================================================================
        modelBuilder.Entity<Product>(entity =>
        {
            // Primary key
            entity.HasKey(p => p.Id);

            // Required fields with max lengths
            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Description)
                .HasMaxLength(2000);

            entity.Property(p => p.SKU)
                .IsRequired()
                .HasMaxLength(50);

            // Unique index on SKU for fast lookups and constraint enforcement
            entity.HasIndex(p => p.SKU)
                .IsUnique();

            // Price precision configuration
            entity.Property(p => p.Price)
                .HasPrecision(18, 2);

            // Index on Name for search performance
            entity.HasIndex(p => p.Name);

            // Relationship: Product belongs to Category
            entity.HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // =====================================================================
        // Category Configuration
        // =====================================================================
        modelBuilder.Entity<Category>(entity =>
        {
            // Primary key
            entity.HasKey(c => c.Id);

            // Required fields
            entity.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Description)
                .HasMaxLength(500);

            // Self-referencing relationship: Category has optional parent
            entity.HasOne(c => c.ParentCategory)
                .WithMany(c => c.SubCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired(false);
        });
    }
}
