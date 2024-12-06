using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace moqaren.Models
{
    public class MoqarenContext : DbContext
    {
        public MoqarenContext(DbContextOptions<MoqarenContext> options)
            : base(options)
        {
            // Initialize DbSets
            Users = Set<User>();
            Categories = Set<Category>();
            Products = Set<Product>();
            Retailers = Set<Retailer>();
            ProductPrices = Set<ProductPrice>();
            PriceHistory = Set<PriceHistory>();
            PriceAlerts = Set<PriceAlert>();
            UserFavorites = Set<UserFavorite>();
        }

        // DbSet Properties
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Retailer> Retailers { get; set; }
        public DbSet<ProductPrice> ProductPrices { get; set; }
        public DbSet<PriceHistory> PriceHistory { get; set; }
        public DbSet<PriceAlert> PriceAlerts { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserID);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(50);
                entity.Property(e => e.City).HasMaxLength(50);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
            });

            // Category Configuration
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.CategoryID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(200);

                entity.HasMany(c => c.Products)
                    .WithOne(p => p.Category)
                    .HasForeignKey(p => p.CategoryID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Product Configuration
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.ProductID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Brand).HasMaxLength(100);
                entity.Property(e => e.Model).HasMaxLength(100);
                entity.Property(e => e.ImageURL).HasMaxLength(500);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(p => p.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(p => p.CategoryID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.ProductPrices)
                    .WithOne(pp => pp.Product)
                    .HasForeignKey(pp => pp.ProductID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(p => p.PriceHistory)
                    .WithOne(ph => ph.Product)
                    .HasForeignKey(ph => ph.ProductID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Retailer Configuration
            modelBuilder.Entity<Retailer>(entity =>
            {
                entity.HasKey(e => e.RetailerID);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Website).HasMaxLength(200);
                entity.Property(e => e.Logo).HasMaxLength(500);

                entity.HasMany(r => r.ProductPrices)
                    .WithOne(pp => pp.Retailer)
                    .HasForeignKey(pp => pp.RetailerID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ProductPrice Configuration
            modelBuilder.Entity<ProductPrice>(entity =>
            {
                entity.HasKey(e => e.PriceID);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.URL).HasMaxLength(500);
                entity.Property(e => e.LastUpdated).HasDefaultValueSql("GETDATE()");

                entity.HasOne(pp => pp.Product)
                    .WithMany(p => p.ProductPrices)
                    .HasForeignKey(pp => pp.ProductID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pp => pp.Retailer)
                    .WithMany(r => r.ProductPrices)
                    .HasForeignKey(pp => pp.RetailerID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PriceHistory Configuration
            modelBuilder.Entity<PriceHistory>(entity =>
            {
                entity.HasKey(e => e.HistoryID);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RecordedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(ph => ph.Product)
                    .WithMany(p => p.PriceHistory)
                    .HasForeignKey(ph => ph.ProductID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ph => ph.Retailer)
                    .WithMany()
                    .HasForeignKey(ph => ph.RetailerID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PriceAlert Configuration
            modelBuilder.Entity<PriceAlert>(entity =>
            {
                entity.HasKey(e => e.AlertID);
                entity.Property(e => e.TargetPrice).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(pa => pa.User)
                    .WithMany(u => u.PriceAlerts)
                    .HasForeignKey(pa => pa.UserID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pa => pa.Product)
                    .WithMany()
                    .HasForeignKey(pa => pa.ProductID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // UserFavorite Configuration
            modelBuilder.Entity<UserFavorite>(entity =>
            {
                entity.HasKey(e => e.FavoriteID);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");

                entity.HasOne(uf => uf.User)
                    .WithMany(u => u.UserFavorites)
                    .HasForeignKey(uf => uf.UserID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(uf => uf.Product)
                    .WithMany()
                    .HasForeignKey(uf => uf.ProductID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Add a unique constraint to prevent duplicate favorites
                entity.HasIndex(uf => new { uf.UserID, uf.ProductID }).IsUnique();
            });
        }
    }
}