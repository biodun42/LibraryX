using System.Collections.Generic;

namespace LibraryX.Models
{
    public class BookDetailViewModel
    {
        // The book being displayed
        public Book Book { get; set; }
        
        // Related books (by same author, same category, etc.)
        public IEnumerable<Book> RelatedBooks { get; set; }
        
        // Similar academic or fiction books depending on the context
        public IEnumerable<Book> RecommendedBooks { get; set; }
        
        // For navigating between academic/fiction contexts
        public string ViewMode { get; set; } = "academic"; // "academic" or "fiction"
        
        // Original search query and category (for back navigation)
        public string SearchQuery { get; set; }
        public string SelectedCategory { get; set; }
        public int CurrentPage { get; set; }
    }
}
