using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LibraryX.Models;
using Microsoft.Extensions.Logging;

namespace LibraryX.Services
{
    public interface IOpenLibraryService
    {
        Task<OpenLibrarySearchResponse> SearchBooksAsync(string query, string? category = null, int page = 1, int limit = 40);
        Task<List<OpenLibraryBook>> GetFeaturedBooksAsync(int count = 8);
        Task<List<OpenLibraryBook>> GetNewArrivalsAsync(int count = 8);
        Task<List<OpenLibraryBook>> GetPopularBooksAsync(int count = 8);
        Task<List<OpenLibraryBook>> GetBooksBySubjectAsync(string subject, int count = 8);
        Task<OpenLibraryBookDetails?> GetBookDetailsByKeyAsync(string key);
        Task<List<OpenLibraryBook>> GetRelatedBooksAsync(OpenLibraryBook book, int count = 4);
        Task<OpenLibraryAuthorDetails?> GetAuthorByKeyAsync(string authorKey);
    }

    public class OpenLibraryService : IOpenLibraryService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenLibraryService> _logger;
        private readonly string _baseUrl = "https://openlibrary.org/search.json";
        private readonly string _bookDetailsBaseUrl = "https://openlibrary.org";

        public OpenLibraryService(HttpClient httpClient, ILogger<OpenLibraryService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<OpenLibrarySearchResponse> SearchBooksAsync(string query, string? category = null, int page = 1, int limit = 40)
        {
            try
            {
                // Calculate offset from page number
                int offset = (page - 1) * limit;
                  // Build the query with filters
                string url = $"{_baseUrl}?offset={offset}&limit={limit}";
                
                // Add the query parameter if not empty
                if (!string.IsNullOrEmpty(query))
                {
                    url += $"&q={Uri.EscapeDataString(query)}";
                }
                else
                {
                    // If no query, use a default search term to get some results
                    url += "&q=popular";
                }
                
                // Add category/subject filter if provided
                if (!string.IsNullOrEmpty(category))
                {
                    url += $"&subject={Uri.EscapeDataString(category)}";
                }

                // Add sorting options
                url += "&sort=rating";
                
                // Add timeout to avoid long-running requests
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var response = await _httpClient.GetAsync(url, cts.Token);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("OpenLibrary API returned non-success status code: {StatusCode} for query: {Query}", 
                        response.StatusCode, query);
                    
                    // Return empty result set rather than throwing exception
                    return new OpenLibrarySearchResponse 
                    { 
                        Docs = new List<OpenLibraryBook>(), 
                        NumFound = 0, 
                        Start = 0 
                    };
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<OpenLibrarySearchResponse>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Handle null result
                if (result == null)
                {
                    _logger.LogWarning("OpenLibrary API returned null result for query: {Query}", query);
                    return new OpenLibrarySearchResponse 
                    { 
                        Docs = new List<OpenLibraryBook>(), 
                        NumFound = 0, 
                        Start = 0 
                    };
                }

                // Ensure Books is not null
                if (result.Docs == null)
                {
                    result.Docs = new List<OpenLibraryBook>();
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Request timeout for OpenLibrary books query: {Query}", query);
                return new OpenLibrarySearchResponse 
                { 
                    Docs = new List<OpenLibraryBook>(), 
                    NumFound = 0, 
                    Start = 0 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching OpenLibrary books with query: {Query}", query);
                // Return empty result set instead of throwing to improve UX
                return new OpenLibrarySearchResponse 
                { 
                    Docs = new List<OpenLibraryBook>(), 
                    NumFound = 0, 
                    Start = 0 
                };
            }
        }
        public async Task<List<OpenLibraryBook>> GetFeaturedBooksAsync(int count = 8)
        {
            // Get featured books - high-rated books
            var result = await SearchBooksAsync("literature", null, 1, count);
            return result?.Docs ?? new List<OpenLibraryBook>();
        }public async Task<List<OpenLibraryBook>> GetNewArrivalsAsync(int count = 8)
        {
            // Get recently published books
            var result = await SearchBooksAsync("first_publish_year:[2020 TO 2025]", null, 1, count);
            return result?.Docs ?? new List<OpenLibraryBook>();
        }        public async Task<List<OpenLibraryBook>> GetPopularBooksAsync(int count = 8)
        {
            // Get popular books based on ratings count
            var result = await SearchBooksAsync("bestsellers", null, 1, count);
            return result?.Docs ?? new List<OpenLibraryBook>();
        }        public async Task<List<OpenLibraryBook>> GetBooksBySubjectAsync(string subject, int count = 8)
        {
            if (string.IsNullOrEmpty(subject))
            {
                return await GetPopularBooksAsync(count);
            }

            try
            {
                // Use the subject as both a search term and a category filter to ensure we get relevant results
                var result = await SearchBooksAsync(subject, subject, 1, count);
                return result?.Docs ?? new List<OpenLibraryBook>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting books by subject: {Subject}", subject);
                return new List<OpenLibraryBook>();
            }
        }

        public async Task<OpenLibraryBookDetails?> GetBookDetailsByKeyAsync(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                {
                    return null;
                }
                
                // If key starts with "/works/", use as is, otherwise add it
                string bookKey = key.StartsWith("/works/") ? key : $"/works/{key}";
                
                // Construct the URL for book details
                string url = $"{_bookDetailsBaseUrl}{bookKey}.json";
                
                // Add timeout to avoid long-running requests
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var response = await _httpClient.GetAsync(url, cts.Token);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"OpenLibrary API returned non-success status code: {response.StatusCode} for book key: {bookKey}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var details = JsonSerializer.Deserialize<JsonElement>(content);
                
                var bookDetails = new OpenLibraryBookDetails
                {
                    Key = bookKey,
                    Title = details.TryGetProperty("title", out var title) ? title.GetString() ?? "Unknown Title" : "Unknown Title",
                    Description = details.TryGetProperty("description", out var description) 
                        ? (description.ValueKind == JsonValueKind.String ? description.GetString() : "No description available") 
                        : "No description available",
                    FirstPublishYear = details.TryGetProperty("first_publish_date", out var firstPublishDate) 
                        ? ParseYear(firstPublishDate.GetString() ?? "") 
                        : null,
                    CoverId = details.TryGetProperty("covers", out var covers) && covers.ValueKind == JsonValueKind.Array && covers.GetArrayLength() > 0 
                        ? covers[0].GetInt32() 
                        : null,
                    Subjects = GetStringListFromJsonElement(details, "subjects"),
                    SubjectPlaces = GetStringListFromJsonElement(details, "subject_places"),
                    SubjectTimes = GetStringListFromJsonElement(details, "subject_times"),
                    SubjectPeople = GetStringListFromJsonElement(details, "subject_people")
                };
                
                // Set cover image URL if cover ID is available
                if (bookDetails.CoverId.HasValue)
                {
                    bookDetails.CoverImageUrl = $"https://covers.openlibrary.org/b/id/{bookDetails.CoverId}-L.jpg";
                }
                
                // Get authors
                if (details.TryGetProperty("authors", out var authorsElement) && authorsElement.ValueKind == JsonValueKind.Array)
                {
                    bookDetails.Authors = new List<string>();
                    foreach (var author in authorsElement.EnumerateArray())
                    {
                        if (author.TryGetProperty("author", out var authorRef) && 
                            authorRef.TryGetProperty("key", out var authorKey))
                        {
                            var authorDetails = await GetAuthorByKeyAsync(authorKey.GetString() ?? "");
                            if (authorDetails != null && !string.IsNullOrEmpty(authorDetails.Name))
                            {
                                bookDetails.Authors.Add(authorDetails.Name);
                            }
                        }
                    }
                }
                
                return bookDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting book details for key: {key}");
                return null;
            }
        }        public async Task<List<OpenLibraryBook>> GetRelatedBooksAsync(OpenLibraryBook book, int count = 4)
        {
            try
            {
                if (book == null)
                {
                    _logger.LogWarning("Null book object passed to GetRelatedBooksAsync");
                    return await GetPopularBooksAsync(count);
                }
                
                // Strategy: Get books by similar subjects or from the same author
                string subject = book.Subjects?.FirstOrDefault() ?? string.Empty;
                string author = book.Authors?.FirstOrDefault() ?? string.Empty;
                
                List<OpenLibraryBook> relatedBooks = new List<OpenLibraryBook>();
                
                // Try to find related books by subject first
                if (!string.IsNullOrEmpty(subject))
                {
                    var subjectBooks = await GetBooksBySubjectAsync(subject, count);
                    if (subjectBooks?.Any() == true)
                    {
                        relatedBooks.AddRange(subjectBooks.Where(b => b?.Key != null && b.Key != book.Key));
                    }
                }
                
                // If we don't have enough books, try by author
                if (relatedBooks.Count < count && !string.IsNullOrEmpty(author))
                {
                    var result = await SearchBooksAsync($"author:{author}", null, 1, count);
                    if (result?.Docs?.Any() == true)
                    {
                        relatedBooks.AddRange(result.Docs.Where(b => b?.Key != null && b.Key != book.Key));
                    }
                }
                
                // If we still don't have enough, get popular books
                if (relatedBooks.Count < count)
                {
                    var popularBooks = await GetPopularBooksAsync(count);
                    if (popularBooks?.Any() == true)
                    {
                        relatedBooks.AddRange(popularBooks.Where(b => b?.Key != null && b.Key != book.Key));
                    }
                }
                
                return relatedBooks.Distinct().Take(count).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting related books");
                return new List<OpenLibraryBook>();
            }
        }

        public async Task<OpenLibraryAuthorDetails?> GetAuthorByKeyAsync(string authorKey)
        {
            try
            {
                if (string.IsNullOrEmpty(authorKey))
                {
                    return null;
                }
                
                // If key doesn't start with a slash, add it
                string key = authorKey.StartsWith("/") ? authorKey : $"/{authorKey}";
                
                // Construct the URL for author details
                string url = $"{_bookDetailsBaseUrl}{key}.json";
                
                // Add timeout to avoid long-running requests
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var response = await _httpClient.GetAsync(url, cts.Token);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"OpenLibrary API returned non-success status code: {response.StatusCode} for author key: {authorKey}");
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var details = JsonSerializer.Deserialize<JsonElement>(content);
                  var authorDetails = new OpenLibraryAuthorDetails
                {
                    Key = key,
                    Name = details.TryGetProperty("name", out var name) ? name.GetString() ?? "Unknown Author" : "Unknown Author",
                    Bio = details.TryGetProperty("bio", out var bio) 
                        ? (bio.ValueKind == JsonValueKind.String ? bio.GetString() ?? "" : "") 
                        : "",
                    BirthDate = details.TryGetProperty("birth_date", out var birthDate) 
                        ? ParseYear(birthDate.GetString() ?? "") 
                        : null,
                    DeathDate = details.TryGetProperty("death_date", out var deathDate) 
                        ? ParseYear(deathDate.GetString() ?? "") 
                        : null
                };
                
                // Get author photo if available
                if (details.TryGetProperty("photos", out var photos) && photos.ValueKind == JsonValueKind.Array && photos.GetArrayLength() > 0)
                {
                    try
                    {
                        var photoId = photos[0].GetInt32();
                        authorDetails.PhotoUrl = $"https://covers.openlibrary.org/a/id/{photoId}-M.jpg";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to extract author photo ID");
                        // Set default placeholder image
                        authorDetails.PhotoUrl = "https://via.placeholder.com/150?text=No+Photo";
                    }
                }
                
                return authorDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting author details for key: {authorKey}");
                return null;
            }
        }
        
        // Helper methods
        private int? ParseYear(string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return null;
                
            // Try to extract a 4-digit year from the date string
            var match = System.Text.RegularExpressions.Regex.Match(dateString, @"\b\d{4}\b");
            if (match.Success && int.TryParse(match.Value, out int year))
                return year;
                
            return null;
        }
          private List<string> GetStringListFromJsonElement(JsonElement element, string propertyName)
        {
            var result = new List<string>();
            
            if (element.TryGetProperty(propertyName, out var jsonArray) && jsonArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in jsonArray.EnumerateArray())
                {
                    if (item.ValueKind == JsonValueKind.String)
                    {
                        string? value = item.GetString();
                        if (!string.IsNullOrEmpty(value))
                        {
                            result.Add(value);
                        }
                    }
                }
            }
            
            return result;
        }
    }
}
