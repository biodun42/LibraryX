using System.Collections.Generic;

namespace LibraryX.Models
{
    public class UserLibraryViewModel
    {
        public List<SavedBook> SavedBooks { get; set; } = new List<SavedBook>();
        public List<DownloadedBook> DownloadedBooks { get; set; } = new List<DownloadedBook>();
        public List<UserSubscription> Subscriptions { get; set; } = new List<UserSubscription>();
        
        // Pagination properties
        public int CurrentPage { get; set; } = 1;
        public int ItemsPerPage { get; set; } = 20;
        public int TotalSavedItems { get; set; }
        public int TotalDownloadedItems { get; set; }
    }
}
