using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace moqaren.Models
{
    public class PriceAlert
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AlertID { get; set; }

        [Required]
        [Display(Name = "User")]
        public int UserID { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductID { get; set; }

        [Required]
        [Display(Name = "Target Price")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal TargetPrice { get; set; }

        [Required]
        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        [Required]
        [Display(Name = "Created Date")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Last Notification")]
        [DataType(DataType.DateTime)]
        public DateTime? LastNotificationSent { get; set; }

        [Display(Name = "Notification Count")]
        public int NotificationCount { get; set; } = 0;

        // Navigation properties
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        [ForeignKey("ProductID")]
        public virtual Product? Product { get; set; }
    }
}