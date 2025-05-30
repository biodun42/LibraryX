using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryX.Models
{    public class SavedBook
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string BookId { get; set; } = string.Empty; // External ID from Google Books or OpenLibrary

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Authors { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? ThumbnailUrl { get; set; }

        [StringLength(50)]
        public string? Source { get; set; } // "GoogleBooks" or "OpenLibrary"

        [Required]
        public DateTime SavedDate { get; set; }

        [StringLength(255)]
        public string? PreviewLink { get; set; }

        // Enhanced metadata properties
        [StringLength(255)]
        public string? Publisher { get; set; }

        public int? PageCount { get; set; }

        public int? PublishedYear { get; set; }        [StringLength(500)]
        public string? ISBN { get; set; }

        [StringLength(500)]
        
        // Navigation property
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
        public string? Categories { get; set; }

        [StringLength(500)]
        public string? BookUrl { get; set; }

        [StringLength(50)]
        public string? Language { get; set; }

        public double? AverageRating { get; set; }

        public int? RatingsCount { get; set; }    }
}
