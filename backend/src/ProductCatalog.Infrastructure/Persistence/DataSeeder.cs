// =============================================================================
// File: DataSeeder.cs
// Layer: Infrastructure
// Purpose: Seeds the database with sample categories and products for
//          demonstration and testing. Creates a realistic category hierarchy.
// =============================================================================

using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;

namespace ProductCatalog.Infrastructure.Persistence;

/// <summary>
/// Static utility class for seeding initial data into the application.
/// Creates a hierarchical category structure and diverse product catalog
/// for demonstration purposes.
/// </summary>
public static class DataSeeder
{
    /// <summary>
    /// Seeds the EF Core database with sample products.
    /// Products reference categories by ID, which are managed by the in-memory repo.
    /// </summary>
    /// <param name="context">The EF Core database context.</param>
    public static void SeedProducts(AppDbContext context)
    {
        // Only seed if the database is empty
        if (context.Products.Any()) return;

        var products = new List<Product>
        {
            // Electronics (CategoryId: 1)
            new() { Id = 1, Name = "Laptop Pro 15", Description = "High-performance 15-inch laptop with 16GB RAM and 512GB SSD", SKU = "ELEC-LP15", Price = 1299.99m, Quantity = 25, CategoryId = 2, CreatedAt = DateTime.UtcNow.AddDays(-30), UpdatedAt = DateTime.UtcNow.AddDays(-5) },
            new() { Id = 2, Name = "Wireless Mouse", Description = "Ergonomic wireless mouse with 2.4GHz connectivity", SKU = "ELEC-WM01", Price = 29.99m, Quantity = 150, CategoryId = 2, CreatedAt = DateTime.UtcNow.AddDays(-25), UpdatedAt = DateTime.UtcNow.AddDays(-3) },
            new() { Id = 3, Name = "Mechanical Keyboard", Description = "RGB mechanical keyboard with Cherry MX switches", SKU = "ELEC-MK01", Price = 89.99m, Quantity = 75, CategoryId = 2, CreatedAt = DateTime.UtcNow.AddDays(-20), UpdatedAt = DateTime.UtcNow },
            new() { Id = 4, Name = "4K Monitor 27\"", Description = "Ultra-HD 27-inch IPS monitor with HDR support", SKU = "ELEC-MN27", Price = 449.99m, Quantity = 30, CategoryId = 2, CreatedAt = DateTime.UtcNow.AddDays(-15), UpdatedAt = DateTime.UtcNow },
            new() { Id = 5, Name = "USB-C Hub", Description = "7-in-1 USB-C hub with HDMI, USB 3.0, and SD card reader", SKU = "ELEC-HB01", Price = 49.99m, Quantity = 200, CategoryId = 2, CreatedAt = DateTime.UtcNow.AddDays(-10), UpdatedAt = DateTime.UtcNow },

            // Smartphones (CategoryId: 3)
            new() { Id = 6, Name = "SmartPhone X12", Description = "Flagship smartphone with 6.7-inch OLED display", SKU = "PHON-X12", Price = 999.99m, Quantity = 50, CategoryId = 3, CreatedAt = DateTime.UtcNow.AddDays(-28), UpdatedAt = DateTime.UtcNow.AddDays(-2) },
            new() { Id = 7, Name = "Phone Case Clear", Description = "Transparent protective case for SmartPhone X12", SKU = "PHON-CS01", Price = 19.99m, Quantity = 300, CategoryId = 3, CreatedAt = DateTime.UtcNow.AddDays(-20), UpdatedAt = DateTime.UtcNow },

            // Books (CategoryId: 4)
            new() { Id = 8, Name = "C# in Depth", Description = "Comprehensive guide to C# programming language features", SKU = "BOOK-CS01", Price = 44.99m, Quantity = 60, CategoryId = 5, CreatedAt = DateTime.UtcNow.AddDays(-45), UpdatedAt = DateTime.UtcNow.AddDays(-10) },
            new() { Id = 9, Name = "Clean Architecture", Description = "A guide to software structure and design by Robert C. Martin", SKU = "BOOK-CA01", Price = 39.99m, Quantity = 45, CategoryId = 5, CreatedAt = DateTime.UtcNow.AddDays(-40), UpdatedAt = DateTime.UtcNow.AddDays(-8) },
            new() { Id = 10, Name = "Angular Development", Description = "Building modern web applications with Angular", SKU = "BOOK-AD01", Price = 49.99m, Quantity = 35, CategoryId = 5, CreatedAt = DateTime.UtcNow.AddDays(-35), UpdatedAt = DateTime.UtcNow },

            // Fiction (CategoryId: 6)
            new() { Id = 11, Name = "The Great Novel", Description = "A bestselling fiction novel with compelling characters", SKU = "BOOK-GN01", Price = 14.99m, Quantity = 120, CategoryId = 6, CreatedAt = DateTime.UtcNow.AddDays(-50), UpdatedAt = DateTime.UtcNow },

            // Clothing (CategoryId: 7)
            new() { Id = 12, Name = "Cotton T-Shirt", Description = "Premium cotton t-shirt available in multiple colors", SKU = "CLTH-TS01", Price = 24.99m, Quantity = 500, CategoryId = 7, CreatedAt = DateTime.UtcNow.AddDays(-60), UpdatedAt = DateTime.UtcNow },
            new() { Id = 13, Name = "Denim Jeans", Description = "Classic fit denim jeans with stretch comfort", SKU = "CLTH-DJ01", Price = 59.99m, Quantity = 200, CategoryId = 7, CreatedAt = DateTime.UtcNow.AddDays(-55), UpdatedAt = DateTime.UtcNow },
            new() { Id = 14, Name = "Running Shoes", Description = "Lightweight running shoes with cushioned sole", SKU = "CLTH-RS01", Price = 79.99m, Quantity = 100, CategoryId = 7, CreatedAt = DateTime.UtcNow.AddDays(-30), UpdatedAt = DateTime.UtcNow },

            // Home & Garden (CategoryId: 8)
            new() { Id = 15, Name = "Coffee Maker", Description = "12-cup programmable coffee maker with thermal carafe", SKU = "HOME-CM01", Price = 69.99m, Quantity = 80, CategoryId = 8, CreatedAt = DateTime.UtcNow.AddDays(-22), UpdatedAt = DateTime.UtcNow },
            new() { Id = 16, Name = "LED Desk Lamp", Description = "Adjustable LED desk lamp with 5 brightness levels", SKU = "HOME-DL01", Price = 34.99m, Quantity = 150, CategoryId = 8, CreatedAt = DateTime.UtcNow.AddDays(-18), UpdatedAt = DateTime.UtcNow },
            new() { Id = 17, Name = "Garden Tool Set", Description = "5-piece stainless steel garden tool set", SKU = "HOME-GT01", Price = 42.99m, Quantity = 65, CategoryId = 8, CreatedAt = DateTime.UtcNow.AddDays(-12), UpdatedAt = DateTime.UtcNow },

            // Out of stock item for testing
            new() { Id = 18, Name = "Limited Edition Watch", Description = "Collector's edition watch - currently sold out", SKU = "CLTH-LW01", Price = 299.99m, Quantity = 0, CategoryId = 7, CreatedAt = DateTime.UtcNow.AddDays(-90), UpdatedAt = DateTime.UtcNow.AddDays(-1) }
        };

        context.Products.AddRange(products);
        context.SaveChanges();
    }

    /// <summary>
    /// Seeds the in-memory category repository with sample hierarchical categories.
    /// Creates a tree structure: Electronics > Computers/Smartphones, Books > Technical/Fiction, etc.
    /// </summary>
    /// <param name="categoryRepository">The in-memory category repository.</param>
    public static async Task SeedCategoriesAsync(ICategoryRepository categoryRepository)
    {
        // Only seed if repository is empty
        var existing = await categoryRepository.GetAllAsync();
        if (existing.Any()) return;

        // Root categories
        var electronics = new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories" };
        var books = new Category { Id = 4, Name = "Books", Description = "Physical and digital books" };
        var clothing = new Category { Id = 7, Name = "Clothing", Description = "Apparel and footwear" };
        var home = new Category { Id = 8, Name = "Home & Garden", Description = "Home improvement and garden supplies" };

        // Child categories under Electronics
        var computers = new Category { Id = 2, Name = "Computers", Description = "Laptops, desktops, and accessories", ParentCategoryId = 1 };
        var smartphones = new Category { Id = 3, Name = "Smartphones", Description = "Mobile phones and accessories", ParentCategoryId = 1 };

        // Child categories under Books
        var technical = new Category { Id = 5, Name = "Technical", Description = "Programming and technology books", ParentCategoryId = 4 };
        var fiction = new Category { Id = 6, Name = "Fiction", Description = "Novels and fiction literature", ParentCategoryId = 4 };

        // Seed in order: parents first, then children
        await categoryRepository.AddAsync(electronics);
        await categoryRepository.AddAsync(computers);
        await categoryRepository.AddAsync(smartphones);
        await categoryRepository.AddAsync(books);
        await categoryRepository.AddAsync(technical);
        await categoryRepository.AddAsync(fiction);
        await categoryRepository.AddAsync(clothing);
        await categoryRepository.AddAsync(home);
    }
}
