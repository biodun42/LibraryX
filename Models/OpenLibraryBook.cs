using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LibraryX.Models
{
    public class OpenLibrarySearchResult
    {
        [JsonPropertyName("numFound")]
        public int TotalItems { get; set; }
        
        [JsonPropertyName("start")]
        public int Start { get; set; }
        
        [JsonPropertyName("docs")]
        public List<OpenLibraryBook> Books { get; set; } = new List<OpenLibraryBook>();
    }

    public class OpenLibrarySearchResponse
    {
        [JsonPropertyName("docs")]
        public List<OpenLibraryBook> Docs { get; set; } = new List<OpenLibraryBook>();
        
        [JsonPropertyName("numFound")]
        public int NumFound { get; set; }
        
        [JsonPropertyName("start")]
        public int Start { get; set; }
        
        [JsonIgnore]
        public int NumPages => (NumFound + 99) / 100; // Assuming 100 items per page
    }

    public class OpenLibraryBook
    {
        [JsonPropertyName("key")]
        public string Key { get; set; }
        
        [JsonPropertyName("title")]
        public string Title { get; set; }
        
        [JsonPropertyName("author_name")]
        public List<string> Authors { get; set; } = new List<string>();
        
        [JsonPropertyName("first_publish_year")]
        public int? FirstPublishYear { get; set; }
        
        [JsonPropertyName("cover_i")]
        public int? CoverId { get; set; }
        
        [JsonPropertyName("isbn")]
        public List<string> ISBN { get; set; } = new List<string>();
        
        [JsonPropertyName("language")]
        public List<string> Language { get; set; } = new List<string>();
        
        [JsonPropertyName("publisher")]
        public List<string> Publisher { get; set; } = new List<string>();
        
        [JsonPropertyName("number_of_pages_median")]
        public int? NumberOfPages { get; set; }
        
        [JsonPropertyName("subject")]
        public List<string> Subjects { get; set; } = new List<string>();
        
        [JsonPropertyName("ratings_average")]
        public double? AverageRating { get; set; }
        
        [JsonPropertyName("ratings_count")]
        public int? RatingsCount { get; set; }
        
        [JsonPropertyName("ebook_access")]
        public string EbookAccess { get; set; }
        
        [JsonIgnore]
        public bool IsAvailable => !string.IsNullOrEmpty(EbookAccess) && EbookAccess != "no_ebook";
        
        [JsonIgnore]
        public string AuthorsText => Authors != null && Authors.Count > 0 ? string.Join(", ", Authors) : "Unknown Author";
        
        [JsonIgnore]
        public string CoverUrl => CoverId.HasValue 
            ? $"https://covers.openlibrary.org/b/id/{CoverId}-L.jpg" 
            : "https://via.placeholder.com/128x192?text=No+Cover";
        
        [JsonIgnore]
        public string SmallCoverUrl => CoverId.HasValue 
            ? $"https://covers.openlibrary.org/b/id/{CoverId}-M.jpg" 
            : "https://via.placeholder.com/128x192?text=No+Cover";
        
        [JsonIgnore]
        public string BookUrl => $"https://openlibrary.org{Key}";
    }
    
    public class OpenLibraryCatalogViewModel
    {
        public string SearchQuery { get; set; }
        public string SelectedCategory { get; set; }
        public OpenLibrarySearchResponse SearchResults { get; set; }
        public List<OpenLibraryBook> FeaturedBooks { get; set; }
        public List<OpenLibraryBook> NewArrivals { get; set; }
        public List<OpenLibraryBook> PopularBooks { get; set; }
        public List<OpenLibraryBook> FictionBooks { get; set; }
        public List<OpenLibraryBook> NonFictionBooks { get; set; }
        
        public string ViewMode { get; set; } = "all"; // "all", "fiction", "nonfiction"
        
        public int CurrentPage { get; set; }
        public int TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        
        public int TotalPages => (TotalItems + ItemsPerPage - 1) / ItemsPerPage;
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}
