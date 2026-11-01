using FCG02.CatalogAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FCG02.CatalogAPI.Infrastructure.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Game> Games => Set<Game>();
    public DbSet<UserLibrary> UserLibraries => Set<UserLibrary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Category).HasConversion<string>();
            entity.HasIndex(e => new { e.Category, e.IsActive });
            
            // Map Tags to string using JSON conversion for SQL Server
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new System.Collections.Generic.List<string>()
                );
        });

        modelBuilder.Entity<UserLibrary>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            
            // Map the private field _items
            entity.OwnsMany(e => e.Items, item =>
            {
                item.ToTable("LibraryItems");
                item.HasKey("Id"); // Shadow property
                item.Property<int>("Id").ValueGeneratedOnAdd();
                item.Property(i => i.GameId).IsRequired();
                item.Property(i => i.GameTitle).IsRequired().HasMaxLength(150);
                item.Property(i => i.PricePaid).HasPrecision(18, 2);
                item.WithOwner().HasForeignKey("UserLibraryId");
            });
            entity.Navigation(e => e.Items).HasField("_items").UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }
}