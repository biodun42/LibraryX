using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryX.Models
{
    public class DownloadedBook
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

        [StringLength(500)]
        public string? CoverImageUrl { get; set; }

        [Required]
        [StringLength(20)]
        public string Format { get; set; } = string.Empty; // "PDF", "EPUB", etc.

        [Required]
        [StringLength(50)]
        public string SourceType { get; set; } = string.Empty; // "GoogleBooks" or "OpenLibrary"
        
        // Make Source property derivable from SourceType if not explicitly set
        [StringLength(50)]
        public string Source 
        { 
            get => SourceType; 
            set => SourceType = value; 
        }

        [Required]
        public DateTime DownloadDate { get; set; }

        [StringLength(500)]
        public string? BookUrl { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
