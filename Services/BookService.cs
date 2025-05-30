using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LibraryX.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LibraryX.Services
{
    public interface IBookService
    {
        Task<BookSearchResult> SearchBooksAsync(string query, string category = null, int startIndex = 0, int maxResults = 40);
        Task<Book?> GetBookByIdAsync(string id);
        Task<IEnumerable<Book>> GetNewArrivalsAsync(int count = 6);
        Task<IEnumerable<Book>> GetBooksBySubjectAsync(string subject, int count = 4);
        Task<IEnumerable<Book>> GetAcademicBooksAsync(string subject = null, int count = 8);
        Task<IEnumerable<Book>> GetFictionBooksAsync(string genre = null, int count = 8);
        Task<BookSearchResult> SearchAcademicBooksAsync(string query, string subject = null, int startIndex = 0, int maxResults = 40);
        Task<IEnumerable<Book>> GetComputerScienceBooks(int count = 4);
        Task<IEnumerable<Book>> GetRelatedBooksAsync(Book book, int count = 4);
    }

    public class BookService : IBookService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BookService> _logger;
        private readonly string _baseUrl = "https://www.googleapis.com/books/v1/volumes";

        public BookService(HttpClient httpClient, ILogger<BookService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }        public async Task<BookSearchResult> SearchBooksAsync(string query, string category = null, int startIndex = 0, int maxResults = 40)
        {
            try
            {
                // Build the query with filters
                var searchQuery = Uri.EscapeDataString(query);
                if (!string.IsNullOrEmpty(category))
                {
                    searchQuery = $"{searchQuery}+subject:\"{category}\"";
                }

                var url = $"{_baseUrl}?q={searchQuery}&startIndex={startIndex}&maxResults={maxResults}&filter=ebooks";
                
                // Add timeout to avoid long-running requests
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var response = await _httpClient.GetAsync(url, cts.Token);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API returned non-success status code: {StatusCode} for query: {Query}", 
                        response.StatusCode, query);
                    
                    // Return empty result set rather than throwing exception
                    return new BookSearchResult { Items = new List<Book>(), TotalItems = 0 };
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<BookSearchResult>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Handle null result
                if (result == null)
                {
                    _logger.LogWarning("API returned null result for query: {Query}", query);
                    return new BookSearchResult { Items = new List<Book>(), TotalItems = 0 };
                }

                // Ensure Items is not null
                if (result.Items == null)
                {
                    result.Items = new List<Book>();
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Request timeout for books query: {Query}", query);
                return new BookSearchResult { Items = new List<Book>(), TotalItems = 0 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching books with query: {Query}", query);
                // Return empty result set instead of throwing to improve UX
                return new BookSearchResult { Items = new List<Book>(), TotalItems = 0 };
            }
        }

        public async Task<Book?> GetBookByIdAsync(string id)
        {
            try
            {
                var url = $"{_baseUrl}/{id}";
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var response = await _httpClient.GetAsync(url, cts.Token);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("API returned non-success status code: {StatusCode} for book ID: {Id}", 
                        response.StatusCode, id);
                    return null;
                }

                var content = await response.Content.ReadAsStringAsync();
                var book = JsonSerializer.Deserialize<Book>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return book;
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Request timeout for book ID: {Id}", id);
                return null;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "HTTP error while getting book with id: {Id}", id);
                return null;
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON parsing error while getting book with id: {Id}", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while getting book with id: {Id}", id);
                return null;
            }
        }

        public async Task<IEnumerable<Book>> GetNewArrivalsAsync(int count = 6)
        {
            // Get recently published books
            var query = "subject:academic&orderBy=newest";
            var result = await SearchBooksAsync(query, null, 0, count);
            return result?.Items ?? new List<Book>();
        }        public async Task<IEnumerable<Book>> GetBooksBySubjectAsync(string subject, int count = 4)
        {
            if (string.IsNullOrEmpty(subject))
            {
                return await GetAcademicBooksAsync(string.Empty, count);
            }

            // Make the query more robust by searching in different ways
            // Use both subject: prefix and regular term search for more reliable results
            string query;
            
            // For computer science specifically, use the enhanced query we developed for computer science books
            if (subject.Contains("computer science", StringComparison.OrdinalIgnoreCase) || 
                subject.Contains("programming", StringComparison.OrdinalIgnoreCase))
            {
                query = "subject:\"computer science\" OR subject:programming OR subject:software";
            }
            else
            {
                query = $"subject:\"{subject}\" OR {subject}";  
            }
            
            // Log the query for debugging
            _logger.LogInformation("GetBooksBySubjectAsync query: {Query}", query);
            
            var result = await SearchBooksAsync(query, string.Empty, 0, count);
            
            // If we didn't get enough results, try a more generic search
            if ((result?.Items?.Count ?? 0) < count/2)
            {
                // Try a simpler query as fallback
                var fallbackQuery = subject;
                _logger.LogInformation("Using fallback query: {Query} for subject {Subject}", fallbackQuery, subject);
                result = await SearchBooksAsync(fallbackQuery, string.Empty, 0, count);
            }
            
            // Log the result for debugging
            _logger.LogInformation("GetBooksBySubjectAsync for '{Subject}', found: {Count} books", 
                subject, result?.Items?.Count ?? 0);
                
            return result?.Items ?? new List<Book>();
        }

        public async Task<IEnumerable<Book>> GetAcademicBooksAsync(string subject = null, int count = 8)
        {
            // Specific query to get academic textbooks and educational resources
            string query = "intitle:textbook OR intitle:handbook OR intitle:principles OR intitle:introduction";
            
            if (!string.IsNullOrEmpty(subject))
            {
                query += $"+subject:{subject}";
            }
            
            query += "+subject:\"academic\" OR subject:\"education\" OR subject:\"textbook\"";
            
            var result = await SearchBooksAsync(query, null, 0, count);
            return result?.Items ?? new List<Book>();
        }

        public async Task<IEnumerable<Book>> GetFictionBooksAsync(string genre = null, int count = 8)
        {
            // Query to get fiction books, optionally filtered by genre
            string query = "subject:fiction";
            
            if (!string.IsNullOrEmpty(genre))
            {
                query += $"+subject:{genre}";
            }
            
            var result = await SearchBooksAsync(query, null, 0, count);
            return result?.Items ?? new List<Book>();
        }

        public async Task<BookSearchResult> SearchAcademicBooksAsync(string query, string subject = null, int startIndex = 0, int maxResults = 40)
        {
            // Add academic filters to the query
            string searchQuery = query + " (textbook OR academic OR study OR university)";
            
            if (!string.IsNullOrEmpty(subject))
            {
                searchQuery += $" subject:{subject}";
            }
            
            return await SearchBooksAsync(searchQuery, null, startIndex, maxResults);
        }

        public async Task<IEnumerable<Book>> GetComputerScienceBooks(int count = 4)
        {
            // More specific query for computer science books
            var query = "subject:\"computer science\" OR subject:programming OR subject:software";
            
            _logger.LogInformation("Querying for computer science books with query: {Query}", query);
            
            var result = await SearchBooksAsync(query, null, 0, count);
            
            // Log results for debugging
            _logger.LogInformation("Computer science books query returned {Count} books", result?.Items?.Count ?? 0);
            
            return result?.Items ?? new List<Book>();
        }

        public async Task<IEnumerable<Book>> GetRelatedBooksAsync(Book book, int count = 4)
        {
            if (book == null)
            {
                return new List<Book>();
            }
            
            // Use a combination of strategies to find the most relevant related books
            
            // Strategy 1: Try to find books by the same author(s)
            string authorQuery = string.Empty;
            if (book.VolumeInfo?.Authors?.Any() == true)
            {
                authorQuery = $"inauthor:\"{book.VolumeInfo.Authors.First()}\"";
                _logger.LogInformation("Trying to find related books by author: {Author}", book.VolumeInfo.Authors.First());
                
                var authorResult = await SearchBooksAsync(authorQuery, string.Empty, 0, count);
                if (authorResult?.Items?.Count > 1) // If we found more than just the current book
                {
                    // Remove the current book from results
                    var filteredResults = authorResult.Items.Where(b => b.Id != book.Id).ToList();
                    if (filteredResults.Count > 0)
                    {
                        _logger.LogInformation("Found {Count} books by the same author", filteredResults.Count);
                        return filteredResults.Take(count);
                    }
                }
            }
            
            // Strategy 2: Try to find books in the same category
            if (book.VolumeInfo?.Categories?.Any() == true)
            {
                string categoryQuery = $"subject:\"{book.VolumeInfo.Categories.First()}\"";
                _logger.LogInformation("Trying to find related books by category: {Category}", book.VolumeInfo.Categories.First());
                
                var categoryResult = await SearchBooksAsync(categoryQuery, string.Empty, 0, count + 1);
                if (categoryResult?.Items?.Count > 1)
                {
                    var filteredResults = categoryResult.Items.Where(b => b.Id != book.Id).ToList();
                    if (filteredResults.Count > 0)
                    {
                        _logger.LogInformation("Found {Count} books in the same category", filteredResults.Count);
                        return filteredResults.Take(count);
                    }
                }
            }
            
            // Strategy 3: Use words from the title as a search query
            if (!string.IsNullOrEmpty(book.VolumeInfo?.Title))
            {
                // Extract meaningful words from the title and use them for search
                var titleWords = book.VolumeInfo.Title
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 3 && !new[] { "and", "the", "with", "from", "that", "this" }.Contains(w.ToLower()))
                    .Take(3);
                    
                if (titleWords.Any())
                {
                    string titleQuery = string.Join(" OR ", titleWords);
                    _logger.LogInformation("Trying to find related books using title keywords: {TitleWords}", titleQuery);
                    
                    var titleResult = await SearchBooksAsync(titleQuery, string.Empty, 0, count + 1);
                    if (titleResult?.Items?.Count > 1)
                    {
                        var filteredResults = titleResult.Items.Where(b => b.Id != book.Id).ToList();
                        if (filteredResults.Count > 0)
                        {
                            _logger.LogInformation("Found {Count} books using title keywords", filteredResults.Count);
                            return filteredResults.Take(count);
                        }
                    }
                }
            }
            
            // Fallback: If all else fails, return some general books
            _logger.LogInformation("No specific related books found, returning general recommendations");
            if (book.VolumeInfo?.Categories?.Any(c => c.Contains("fiction", StringComparison.OrdinalIgnoreCase)) == true)
            {
                return await GetFictionBooksAsync(string.Empty, count);
            }
            
            return await GetAcademicBooksAsync(string.Empty, count);
        }
    }
}