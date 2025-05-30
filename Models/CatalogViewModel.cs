using System.Collections.Generic;

namespace LibraryX.Models
{    public class CatalogViewModel
    {
        public string SearchQuery { get; set; }
        public string SelectedCategory { get; set; }
        public BookSearchResult SearchResults { get; set; }
        public IEnumerable<Book> NewArrivals { get; set; }
        public IEnumerable<Book> ComputerScienceBooks { get; set; }
        public IEnumerable<Book> EngineeringBooks { get; set; }
        public IEnumerable<Book> MedicineBooks { get; set; }
        
        // Academic and Fiction categories
        public IEnumerable<Book> AcademicBooks { get; set; }
        public IEnumerable<Book> FictionBooks { get; set; }
        public IEnumerable<Book> LiteratureBooks { get; set; }
        public IEnumerable<Book> ResearchBooks { get; set; }
        
        // View mode - to toggle between academic and fiction view
        public string ViewMode { get; set; } = "academic"; // "academic" or "fiction"
        
        public int CurrentPage { get; set; }
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        
        public int TotalPages => (TotalItems + ItemsPerPage - 1) / ItemsPerPage;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}