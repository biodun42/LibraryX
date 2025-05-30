using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryX.Models
{
    public class UserSubscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SubscriptionType { get; set; } = string.Empty;

        [Required]
        public DateTime SubscriptionDate { get; set; }

        public DateTime ExpirationDate { get; set; }

        [Required]
        public bool IsActive { get; set; }

        [StringLength(500)]
        public string? SubscriptionDetails { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
