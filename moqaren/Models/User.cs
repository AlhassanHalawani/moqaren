// Models/User.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace moqaren.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(50)]
        public string? City { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastLogin { get; set; }

        // Navigation properties
        public virtual ICollection<PriceAlert>? PriceAlerts { get; set; }
        public virtual ICollection<UserFavorite>? UserFavorites { get; set; }
    }
}

// Models/Category.cs
namespace moqaren.Models
{
    public class Category
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        // Navigation properties
        public virtual ICollection<Product>? Products { get; set; }
    }
}

// Models/Product.cs
namespace moqaren.Models
{
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductID { get; set; }

        [Required]
        public int CategoryID { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(100)]
        public string? Model { get; set; }

        [StringLength(500)]
        public string? ImageURL { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("CategoryID")]
        public virtual Category? Category { get; set; }
        public virtual ICollection<ProductPrice>? ProductPrices { get; set; }
        public virtual ICollection<PriceHistory>? PriceHistory { get; set; }
    }
}

// Models/Retailer.cs
namespace moqaren.Models
{
    public class Retailer
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RetailerID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Website { get; set; }

        [StringLength(500)]
        public string? Logo { get; set; }

        // Navigation properties
        public virtual ICollection<ProductPrice>? ProductPrices { get; set; }
    }
}

// Models/ProductPrice.cs
namespace moqaren.Models
{
    public class ProductPrice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PriceID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        public int RetailerID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [StringLength(500)]
        public string? URL { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("ProductID")]
        public virtual Product? Product { get; set; }

        [ForeignKey("RetailerID")]
        public virtual Retailer? Retailer { get; set; }
    }
}

// Models/PriceHistory.cs
namespace moqaren.Models
{
    public class PriceHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HistoryID { get; set; }

        [Required]
        public int ProductID { get; set; }

        [Required]
        public int RetailerID { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("ProductID")]
        public virtual Product? Product { get; set; }

        [ForeignKey("RetailerID")]
        public virtual Retailer? Retailer { get; set; }
    }
}

// Models/UserFavorite.cs
namespace moqaren.Models
{
    public class UserFavorite
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FavoriteID { get; set; }

        [Required]
        public int UserID { get; set; }

        [Required]
        public int ProductID { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product? Product { get; set; }
    }
}