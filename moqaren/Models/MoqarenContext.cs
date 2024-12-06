using Microsoft.EntityFrameworkCore;

namespace moqaren.Models
{
    public class MoqarenContext : DbContext
    {
        public MoqarenContext(DbContextOptions<MoqarenContext> options)
            : base(options)
        {
        }

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

            // Configure relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryID);

            modelBuilder.Entity<ProductPrice>()
                .HasOne(pp => pp.Product)
                .WithMany(p => p.ProductPrices)
                .HasForeignKey(pp => pp.ProductID);

            modelBuilder.Entity<PriceHistory>()
                .HasOne(ph => ph.Product)
                .WithMany(p => p.PriceHistory)
                .HasForeignKey(ph => ph.ProductID);

            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.User)
                .WithMany()
                .HasForeignKey(uf => uf.UserID);

            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.Product)
                .WithMany()
                .HasForeignKey(uf => uf.ProductID);
        }
    }
}