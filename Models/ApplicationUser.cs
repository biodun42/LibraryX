using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace LibraryX.Models
{
    public class ApplicationUser : IdentityUser
    {        // Add new fields to the user model
        public string FullName { get; set; } = string.Empty;

        public string? CreatedDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        public string? LastLoginDate { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

        

        // Navigation properties for related entities
        public virtual ICollection<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
        public virtual ICollection<SavedBook> SavedBooks { get; set; } = new List<SavedBook>();
        public virtual ICollection<DownloadedBook> DownloadedBooks { get; set; } = new List<DownloadedBook>();

    }
}
