using System.Collections.Generic;

namespace LibraryX.Models
{
    public class OpenLibraryDetailViewModel
    {
        public OpenLibraryBook Book { get; set; }
        public List<OpenLibraryBook> RelatedBooks { get; set; } = new List<OpenLibraryBook>();
        public List<OpenLibraryBook> RecommendedBooks { get; set; } = new List<OpenLibraryBook>();
        public string ViewMode { get; set; } = "all";
        public string SearchQuery { get; set; }
        public string SelectedCategory { get; set; }
        public int CurrentPage { get; set; } = 1;
        
        // Additional OpenLibrary-specific properties
        public OpenLibraryBookDetails BookDetails { get; set; }
        public List<OpenLibraryAuthorDetails> AuthorDetails { get; set; } = new List<OpenLibraryAuthorDetails>();
    }
    
    public class OpenLibraryBookDetails
    {
        public string Key { get; set; }
        public string Title { get; set; }
        public List<string> Authors { get; set; } = new List<string>();
        public string Description { get; set; }
        public int? FirstPublishYear { get; set; }
        public int? Pages { get; set; }
        public List<string> Publishers { get; set; } = new List<string>(); 
        public List<string> Subjects { get; set; } = new List<string>();
        public List<string> SubjectPlaces { get; set; } = new List<string>();
        public List<string> SubjectTimes { get; set; } = new List<string>();
        public List<string> SubjectPeople { get; set; } = new List<string>();
        public string Language { get; set; }
        public string CoverImageUrl { get; set; }
        public string EbookAccess { get; set; }
        public int? CoverId { get; set; }
    }
    
    public class OpenLibraryAuthorDetails
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public string Bio { get; set; }
        public int? BirthDate { get; set; }
        public int? DeathDate { get; set; }
        public string PhotoUrl { get; set; }
    }
}
