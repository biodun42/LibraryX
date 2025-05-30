using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using LibraryX.Models;
using LibraryX.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LibraryX.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;

namespace LibraryX.Controllers
{
    public class HomeController : Controller
    {        private readonly ILogger<HomeController> _logger;
        private readonly IBookService _bookService;
        private readonly IOpenLibraryService _openLibraryService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly SignInManager<ApplicationUser> _signInManager;
        
        public HomeController(ILogger<HomeController> logger, 
                             IBookService bookService, 
                             IOpenLibraryService openLibraryService,
                             UserManager<ApplicationUser> userManager,
                             ApplicationDbContext context,
                             SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _bookService = bookService;
            _openLibraryService = openLibraryService;
            _userManager = userManager;
            _context = context;
            _signInManager = signInManager;
        }
        
        public async Task<IActionResult> Index()
        {
            var featuredBooks = new Dictionary<string, IEnumerable<Book>>();
            featuredBooks["NewArrivals"] = await _bookService.GetNewArrivalsAsync(6);
            featuredBooks["AcademicHighlights"] = await _bookService.GetAcademicBooksAsync(count: 4);
            featuredBooks["FictionHighlights"] = await _bookService.GetFictionBooksAsync(count: 4);
            
            // Add user-specific data for authenticated users
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                if (userId != null)
                {
                    // Calculate books due soon (books downloaded in the last 14 days)
                    var currentDate = DateTime.UtcNow;
                    var booksDueSoon = await _context.DownloadedBooks
                        .Where(db => db.UserId == userId && 
                                    db.DownloadDate.AddDays(14) > currentDate)
                        .CountAsync();
                    
                    // Count saved books (reading list)
                    var savedBooksCount = await _context.SavedBooks
                        .Where(sb => sb.UserId == userId)
                        .CountAsync();
                    
                    // Get subscription status
                    var activeSubscription = await _context.UserSubscriptions
                        .Where(us => us.UserId == userId && us.IsActive && us.ExpirationDate > currentDate)
                        .FirstOrDefaultAsync();
                    
                    // Pass data to the view using ViewBag
                    ViewBag.UserLibraryStats = new
                    {
                        BooksDueSoon = booksDueSoon,
                        ReadingListCount = savedBooksCount,
                        HasActiveSubscription = activeSubscription != null,
                        SubscriptionType = activeSubscription?.SubscriptionType,
                        SubscriptionExpiresOn = activeSubscription?.ExpirationDate
                    };
                      // Get recommended books based on saved and downloaded books
                    // For simplicity, we'll just get a few books that the user hasn't downloaded yet
                    var userDownloadedBookIds = await _context.DownloadedBooks
                        .Where(db => db.UserId == userId)
                        .Select(db => db.BookId)
                        .ToListAsync();
                    
                    var recommendationsCount = 3; // Number of recommendations to show
                    var recommendedBooks = await _context.SavedBooks
                        .Where(sb => sb.UserId == userId && !userDownloadedBookIds.Contains(sb.BookId))
                        .OrderByDescending(sb => sb.SavedDate)
                        .Take(recommendationsCount)
                        .ToListAsync();
                    
                    // Get most recent activity (combinations of saved and downloaded books)
                    var recentDownloads = await _context.DownloadedBooks
                        .Where(db => db.UserId == userId)
                        .OrderByDescending(db => db.DownloadDate)
                        .Take(1)
                        .ToListAsync();
                        
                    var recentSaves = await _context.SavedBooks
                        .Where(sb => sb.UserId == userId)
                        .OrderByDescending(sb => sb.SavedDate)
                        .Take(1)
                        .ToListAsync();
                    
                    // Combine and sort to get most recent activity
                    var recentActivity = new List<object>();
                    if (recentDownloads.Any())
                    {
                        var download = recentDownloads.First();
                        recentActivity.Add(new { 
                            Type = "download",
                            Date = download.DownloadDate,
                            Title = download.Title,
                            BookId = download.BookId,
                            Format = download.Format
                        });
                    }
                    
                    if (recentSaves.Any())
                    {
                        var save = recentSaves.First();
                        recentActivity.Add(new { 
                            Type = "save", 
                            Date = save.SavedDate,
                            Title = save.Title,
                            BookId = save.BookId
                        });
                    }
                      // Sort by date, most recent first
                    var mostRecentActivity = recentActivity.Count > 0 ? 
                        recentActivity.OrderByDescending(a => ((dynamic)a).Date).FirstOrDefault() : 
                        null;
                    
                    ViewBag.RecommendedBooks = recommendedBooks;
                    ViewBag.RecommendationsCount = recommendedBooks.Count;
                    ViewBag.MostRecentActivity = mostRecentActivity;
                }
            }
            
            return View(featuredBooks);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = 
                HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            
            var exception = exceptionHandlerPathFeature?.Error;
            
            var errorViewModel = new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            };
            
            if (exception != null)
            {
                // Log the exception details
                _logger.LogError(exception, "Error occurred on path: {Path}", exceptionHandlerPathFeature?.Path);
                
                // Try to provide more specific information for authentication errors
                if (exception.Message.Contains("authentication") || 
                    exception.Message.Contains("sign in") ||
                    exception.Message.Contains("identity") ||
                    exception.Message.Contains("login"))
                {
                    errorViewModel.ErrorTitle = "Authentication Error";
                    errorViewModel.ErrorMessage = "There was a problem with your sign in. Please try logging in again.";
                    errorViewModel.SuggestedAction = "Sign Out and Sign In";
                    
                    // Clear the existing identity cookie to help resolve auth issues
                    HttpContext.Response.Cookies.Delete(".AspNetCore.Identity.Application");
                }
            }
            
            return View(errorViewModel);
        }
        
        [AllowAnonymous]
        public IActionResult AuthError()
        {
            // Clear identity cookies
            foreach (var cookie in Request.Cookies.Keys)
            {
                if (cookie.StartsWith(".AspNetCore."))
                {
                    Response.Cookies.Delete(cookie);
                }
            }
            
            var errorViewModel = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                ErrorTitle = "Authentication Error",
                ErrorMessage = "Your authentication session has expired or is invalid. Please sign in again.",
                SuggestedAction = "Sign In"
            };
            
            return View("Error", errorViewModel);
        }
        
        [AllowAnonymous]
        public IActionResult CheckAuth()
        {
            var model = new Dictionary<string, object>();
            
            // Check if the user is authenticated
            model["IsAuthenticated"] = User.Identity?.IsAuthenticated ?? false;
            
            // Get the authentication type if authenticated
            if (User.Identity?.IsAuthenticated == true)
            {
                model["AuthType"] = User.Identity?.AuthenticationType ?? "Unknown";
                model["UserName"] = User.Identity?.Name ?? "Unknown";
                model["UserId"] = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Unknown";
                model["Claims"] = User.Claims.Select(c => new { Type = c.Type, Value = c.Value }).ToList();
                model["Cookies"] = Request.Cookies.Select(c => c.Key).ToList();
            }
            else
            {
                model["AuthErrorMessage"] = "User is not authenticated";
                model["Cookies"] = Request.Cookies.Select(c => c.Key).ToList();
            }
            
            return Json(model);
        }
        
        public async Task<IActionResult> BookDetail(string id, string viewMode = "academic", string searchQuery = "", string category = "", int page = 1)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Catalog");
            }

            try
            {
                // Get the book details
                var book = await _bookService.GetBookByIdAsync(id);
                
                if (book == null)
                {
                    TempData["ErrorMessage"] = "Book details could not be found. The book may no longer be available.";
                    return RedirectToAction("Catalog", new { query = searchQuery, category = category, page = page, viewMode = viewMode });
                }

                // Check if the user is authenticated - we'll pass this to the view
                ViewData["IsAuthenticated"] = User.Identity?.IsAuthenticated ?? false;
                
                // Check book status for authenticated users
                bool isBookSaved = false;
                bool isBookDownloaded = false;
                SavedBook? savedBookInfo = null;
                DownloadedBook? downloadedBookInfo = null;
                
                if (User.Identity?.IsAuthenticated == true)
                {
                    // Use UserId from claims instead of getting user from UserManager
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userId != null)
                    {
                        isBookSaved = await IsBookSavedAsync(id, userId);
                        isBookDownloaded = await IsBookDownloadedAsync(id, userId);
                        savedBookInfo = await GetSavedBookAsync(id, userId);
                        downloadedBookInfo = await GetDownloadedBookAsync(id, userId);
                    }
                }
                
                ViewData["IsBookSaved"] = isBookSaved;
                ViewData["IsBookDownloaded"] = isBookDownloaded;
                ViewData["SavedBookInfo"] = savedBookInfo;
                ViewData["DownloadedBookInfo"] = downloadedBookInfo;
                
                // Get related books based on our improved method
                var relatedBooks = await _bookService.GetRelatedBooksAsync(book, 6);
                
                // Get recommended books based on view mode
                IEnumerable<Book> recommendedBooks;
                if (viewMode == "academic")
                {
                    // Custom recommendations based on academic discipline
                    string recommendationSubject = "research methodology";
                    
                    // Try to detect academic discipline from categories or title
                    if (book.VolumeInfo?.Categories?.Any(c => c.Contains("comput", StringComparison.OrdinalIgnoreCase)) == true ||
                        book.VolumeInfo?.Title?.Contains("programming", StringComparison.OrdinalIgnoreCase) == true ||
                        book.VolumeInfo?.Title?.Contains("computer", StringComparison.OrdinalIgnoreCase) == true)
                    {
                        recommendationSubject = "computer science";
                    }
                    else if (book.VolumeInfo?.Categories?.Any(c => c.Contains("engineer", StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        recommendationSubject = "engineering";
                    }
                    else if (book.VolumeInfo?.Categories?.Any(c => c.Contains("medic", StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        recommendationSubject = "medicine";
                    }
                    else if (book.VolumeInfo?.Categories?.Any(c => c.Contains("business", StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        recommendationSubject = "business";
                    }
                    
                    // Log for debugging
                    _logger.LogInformation("Finding recommended books with subject: {Subject}", recommendationSubject);
                    
                    // Use GetBooksBySubjectAsync for more reliable results
                    recommendedBooks = await _bookService.GetBooksBySubjectAsync(recommendationSubject, 4);
                }
                else // fiction
                {
                    // Get recommended fiction books based on detected genre
                    string recommendedGenre = "literature";
                    if (book.VolumeInfo?.Categories?.Any(c => c.Contains("fantasy", StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        recommendedGenre = "fantasy";
                    }
                    else if (book.VolumeInfo?.Categories?.Any(c => c.Contains("mystery", StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        recommendedGenre = "mystery";
                    }
                    else if (book.VolumeInfo?.Categories?.Any(c => c.Contains("romance", StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        recommendedGenre = "romance";
                    }
                    else if (book.VolumeInfo?.Categories?.Any(c => c.Contains("science fiction", StringComparison.OrdinalIgnoreCase)) == true)
                    {
                        recommendedGenre = "science fiction";
                    }
                    
                    // Log for debugging
                    _logger.LogInformation("Finding recommended fiction books with genre: {Genre}", recommendedGenre);
                    
                    recommendedBooks = await _bookService.GetFictionBooksAsync(recommendedGenre, 4);
                }
                
                // Log results for debugging
                _logger.LogInformation("RelatedBooks count: {RelatedCount}, RecommendedBooks count: {RecommendedCount}", 
                    relatedBooks?.Count() ?? 0, recommendedBooks?.Count() ?? 0);
                
                // Create the view model
                var viewModel = new BookDetailViewModel
                {
                    Book = book,
                    RelatedBooks = relatedBooks ?? Array.Empty<Book>(),
                    RecommendedBooks = recommendedBooks ?? Array.Empty<Book>(),
                    ViewMode = viewMode,
                    SearchQuery = searchQuery,
                    SelectedCategory = category,
                    CurrentPage = page
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving book details for ID: {Id}", id);
                TempData["ErrorMessage"] = "Unable to retrieve book details. Please try again later.";
                return RedirectToAction("Catalog", new { query = searchQuery, category = category, page = page, viewMode = viewMode });
            }
        }        public async Task<IActionResult> Catalog(string query = "", string category = "", int page = 1, string viewMode = "academic")
        {
            int maxResultsPerPage = 40;
            int startIndex = (page - 1) * maxResultsPerPage;
            
            try 
            {
                // Only load what's immediately necessary based on the current mode
                var tasks = new List<Task>();
                BookSearchResult? searchResults = null;
                IEnumerable<Book>? newArrivals = null;
                IEnumerable<Book>? computerScienceBooks = null;
                IEnumerable<Book>? engineeringBooks = null;
                IEnumerable<Book>? medicineBooks = null;
                IEnumerable<Book>? fictionBooks = null;
                IEnumerable<Book>? literatureBooks = null;
                IEnumerable<Book>? researchBooks = null;
                IEnumerable<Book>? academicBooks = null;
                
                // Search task is always needed
                Task searchTask;
                
                // Use different search methods based on view mode
                if (viewMode == "academic")
                {
                    // If no search query, default to showing academic books
                    string searchQuery = string.IsNullOrEmpty(query) ? "university textbooks" : query;
                    searchTask = Task.Run(async () => 
                    {
                        searchResults = await _bookService.SearchAcademicBooksAsync(searchQuery, category, startIndex, maxResultsPerPage);
                    });
                    
                    // Load academic-specific content in parallel
                    var newArrivalsTask = Task.Run(async () => 
                    {
                        newArrivals = await _bookService.GetNewArrivalsAsync();
                    });
                    
                    var computerScienceTask = Task.Run(async () => 
                    {
                        computerScienceBooks = await _bookService.GetComputerScienceBooks();
                    });
                    
                    var engineeringTask = Task.Run(async () => 
                    {
                        engineeringBooks = await _bookService.GetBooksBySubjectAsync("engineering");
                    });
                    
                    var medicineTask = Task.Run(async () => 
                    {
                        medicineBooks = await _bookService.GetBooksBySubjectAsync("medicine");
                    });
                    
                    var researchTask = Task.Run(async () => 
                    {
                        researchBooks = await _bookService.GetAcademicBooksAsync("research methodology");
                    });
                    
                    var academicTask = Task.Run(async () => 
                    {
                        academicBooks = await _bookService.GetAcademicBooksAsync();
                    });
                    
                    tasks.AddRange(new[] { searchTask, newArrivalsTask, computerScienceTask, engineeringTask, 
                                          medicineTask, researchTask, academicTask });
                }
                else // fiction view mode
                {
                    // For fiction mode, search for fiction books
                    string searchQuery = string.IsNullOrEmpty(query) ? "fiction" : query;
                    searchTask = Task.Run(async () => 
                    {
                        searchResults = await _bookService.SearchBooksAsync(searchQuery, string.IsNullOrEmpty(category) ? "fiction" : category, startIndex, maxResultsPerPage);
                    });
                    
                    // Load fiction-specific content in parallel
                    var newArrivalsTask = Task.Run(async () => 
                    {
                        newArrivals = await _bookService.GetNewArrivalsAsync();
                    });
                    
                    var fictionTask = Task.Run(async () => 
                    {
                        fictionBooks = await _bookService.GetFictionBooksAsync();
                    });
                    
                    var literatureTask = Task.Run(async () => 
                    {
                        literatureBooks = await _bookService.GetFictionBooksAsync("literature");
                    });
                    
                    tasks.AddRange(new[] { searchTask, newArrivalsTask, fictionTask, literatureTask });
                }
                
                // Wait for all tasks to complete
                await Task.WhenAll(tasks);
                
                var viewModel = new CatalogViewModel
                {
                    SearchQuery = query,
                    SelectedCategory = category,
                    SearchResults = searchResults ?? new BookSearchResult { Items = new List<Book>(), TotalItems = 0 },
                    NewArrivals = newArrivals ?? Array.Empty<Book>(),
                    ComputerScienceBooks = computerScienceBooks ?? Array.Empty<Book>(),
                    EngineeringBooks = engineeringBooks ?? Array.Empty<Book>(),
                    MedicineBooks = medicineBooks ?? Array.Empty<Book>(),
                    
                    // Add new properties
                    AcademicBooks = academicBooks ?? Array.Empty<Book>(),
                    FictionBooks = fictionBooks ?? Array.Empty<Book>(),
                    LiteratureBooks = literatureBooks ?? Array.Empty<Book>(),
                    ResearchBooks = researchBooks ?? Array.Empty<Book>(),
                    ViewMode = viewMode,
                    
                    CurrentPage = page,
                    TotalItems = searchResults?.TotalItems ?? 0,
                    ItemsPerPage = maxResultsPerPage
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading catalog with query: {Query}, category: {Category}, viewMode: {ViewMode}", 
                    query, category, viewMode);
                
                TempData["ErrorMessage"] = "There was a problem loading the catalog. Please try again later.";
                
                // Return a minimal view model to prevent the view from crashing
                return View(new CatalogViewModel
                {
                    SearchQuery = query,
                    SelectedCategory = category,
                    SearchResults = new BookSearchResult { Items = new List<Book>(), TotalItems = 0 },
                    NewArrivals = Array.Empty<Book>(),
                    ComputerScienceBooks = Array.Empty<Book>(),
                    EngineeringBooks = Array.Empty<Book>(),
                    MedicineBooks = Array.Empty<Book>(),
                    AcademicBooks = Array.Empty<Book>(),
                    FictionBooks = Array.Empty<Book>(),
                    LiteratureBooks = Array.Empty<Book>(),
                    ResearchBooks = Array.Empty<Book>(),
                    ViewMode = viewMode,
                    CurrentPage = page,
                    ItemsPerPage = maxResultsPerPage
                });
            }
        }
        public async Task<IActionResult> OpenLibrary(string query = "", string category = "", int page = 1, string viewMode = "all")
        {
            int maxResultsPerPage = 40;
            
            try 
            {                // Only load what's immediately necessary based on the current mode
                var tasks = new List<Task>();
                OpenLibrarySearchResponse? searchResults = null;
                List<OpenLibraryBook>? featuredBooks = null;
                List<OpenLibraryBook>? newArrivals = null;
                List<OpenLibraryBook>? popularBooks = null;
                List<OpenLibraryBook>? fictionBooks = null;
                List<OpenLibraryBook>? nonFictionBooks = null;
                
                // Search task is always needed
                Task searchTask;
                
                // Default search query if empty
                string searchQuery = string.IsNullOrEmpty(query) ? "" : query;
                
                if (viewMode == "fiction")
                {
                    category = string.IsNullOrEmpty(category) ? "fiction" : category;
                }
                else if (viewMode == "nonfiction")
                {
                    category = string.IsNullOrEmpty(category) ? "non-fiction" : category;
                }
                
                searchTask = Task.Run(async () => 
                {
                    searchResults = await _openLibraryService.SearchBooksAsync(searchQuery, category, page, maxResultsPerPage);
                });
                
                // Load featured books
                var featuredBooksTask = Task.Run(async () => 
                {
                    featuredBooks = await _openLibraryService.GetFeaturedBooksAsync();
                });
                
                // Load new arrivals
                var newArrivalsTask = Task.Run(async () => 
                {
                    newArrivals = await _openLibraryService.GetNewArrivalsAsync();
                });
                
                // Load popular books
                var popularBooksTask = Task.Run(async () => 
                {
                    popularBooks = await _openLibraryService.GetPopularBooksAsync();
                });
                
                // Load fiction books (for categories section)
                var fictionBooksTask = Task.Run(async () => 
                {
                    fictionBooks = await _openLibraryService.GetBooksBySubjectAsync("fiction");
                });
                
                // Load non-fiction books (for categories section)
                var nonFictionBooksTask = Task.Run(async () => 
                {
                    nonFictionBooks = await _openLibraryService.GetBooksBySubjectAsync("non-fiction");
                });
                
                tasks.AddRange(new[] { searchTask, featuredBooksTask, newArrivalsTask, popularBooksTask, fictionBooksTask, nonFictionBooksTask });
                
                // Wait for all tasks to complete
                await Task.WhenAll(tasks);
                
                var viewModel = new OpenLibraryCatalogViewModel
                {
                    SearchQuery = query,
                    SelectedCategory = category,
                    SearchResults = searchResults ?? new OpenLibrarySearchResponse 
                    { 
                        Docs = new List<OpenLibraryBook>(), 
                        NumFound = 0, 
                        Start = 0 
                    },
                    FeaturedBooks = featuredBooks ?? new List<OpenLibraryBook>(),
                    NewArrivals = newArrivals ?? new List<OpenLibraryBook>(),
                    PopularBooks = popularBooks ?? new List<OpenLibraryBook>(),
                    FictionBooks = fictionBooks ?? new List<OpenLibraryBook>(),
                    NonFictionBooks = nonFictionBooks ?? new List<OpenLibraryBook>(),
                    ViewMode = viewMode,
                    CurrentPage = page,
                    TotalItems = searchResults?.NumFound ?? 0,
                    ItemsPerPage = maxResultsPerPage
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading OpenLibrary catalog with query: {Query}, category: {Category}, viewMode: {ViewMode}", 
                    query, category, viewMode);
                
                TempData["ErrorMessage"] = "There was a problem loading the catalog. Please try again later.";
                
                // Return a minimal view model to prevent the view from crashing
                return View(new OpenLibraryCatalogViewModel
                {
                    SearchQuery = query,
                    SelectedCategory = category,
                    SearchResults = new OpenLibrarySearchResponse 
                    { 
                        Docs = new List<OpenLibraryBook>(), 
                        NumFound = 0, 
                        Start = 0 
                    },
                    FeaturedBooks = new List<OpenLibraryBook>(),
                    NewArrivals = new List<OpenLibraryBook>(),
                    PopularBooks = new List<OpenLibraryBook>(),
                    FictionBooks = new List<OpenLibraryBook>(),
                    NonFictionBooks = new List<OpenLibraryBook>(),
                    ViewMode = viewMode,
                    CurrentPage = page,
                    ItemsPerPage = maxResultsPerPage
                });
            }
        }
        public async Task<IActionResult> OpenLibraryDetail(string key, string viewMode = "all", string searchQuery = "", string category = "", int page = 1)
        {
            if (string.IsNullOrEmpty(key))
            {
                return RedirectToAction("OpenLibrary");
            }

            try
            {
                // Get the book details
                var bookDetails = await _openLibraryService.GetBookDetailsByKeyAsync(key);
                
                if (bookDetails == null)
                {
                    TempData["ErrorMessage"] = "Book details could not be found. The book may no longer be available.";
                    return RedirectToAction("OpenLibrary", new { query = searchQuery, category = category, page = page, viewMode = viewMode });
                }

                // Check if the user is authenticated - we'll pass this to the view
                ViewData["IsAuthenticated"] = User.Identity?.IsAuthenticated ?? false;
                
                // Check book status for authenticated users
                bool isBookSaved = false;
                bool isBookDownloaded = false;
                SavedBook? savedBookInfo = null;
                DownloadedBook? downloadedBookInfo = null;
                
                if (User.Identity?.IsAuthenticated == true)
                {
                    // Use UserId from claims instead of getting user from UserManager
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    if (userId != null)
                    {
                        // For OpenLibrary, we need to check if the key has /works/ prefix
                        string bookId = key.StartsWith("/works/") ? key : $"/works/{key}";
                        isBookSaved = await IsBookSavedAsync(bookId, userId);
                        isBookDownloaded = await IsBookDownloadedAsync(bookId, userId);
                        savedBookInfo = await GetSavedBookAsync(bookId, userId);
                        downloadedBookInfo = await GetDownloadedBookAsync(bookId, userId);
                    }
                }
                
                ViewData["IsBookSaved"] = isBookSaved;
                ViewData["IsBookDownloaded"] = isBookDownloaded;
                ViewData["SavedBookInfo"] = savedBookInfo;
                ViewData["DownloadedBookInfo"] = downloadedBookInfo;
                
                // Create an OpenLibraryBook from the details for related books search
                var book = new OpenLibraryBook
                {
                    Key = bookDetails.Key,
                    Title = bookDetails.Title,
                    Authors = bookDetails.Authors,
                    Subjects = bookDetails.Subjects
                };
                
                // Get related books
                var relatedBooks = await _openLibraryService.GetRelatedBooksAsync(book, 8);
                
                // Get recommended books based on subjects
                List<OpenLibraryBook> recommendedBooks;
                string recommendationSubject = bookDetails.Subjects?.FirstOrDefault() ?? "fiction";
                recommendedBooks = await _openLibraryService.GetBooksBySubjectAsync(recommendationSubject, 4);
                
                // Create the view model
                var viewModel = new OpenLibraryDetailViewModel
                {
                    Book = book,
                    BookDetails = bookDetails,
                    RelatedBooks = relatedBooks ?? new List<OpenLibraryBook>(),
                    RecommendedBooks = recommendedBooks ?? new List<OpenLibraryBook>(),
                    ViewMode = viewMode,
                    SearchQuery = searchQuery,
                    SelectedCategory = category,
                    CurrentPage = page
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving OpenLibrary book details for key: {Key}", key);
                TempData["ErrorMessage"] = "Unable to retrieve book details. Please try again later.";
                return RedirectToAction("OpenLibrary", new { query = searchQuery, category = category, page = page, viewMode = viewMode });
            }
        }        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SaveBook(string bookId, string title, string authors, 
            string description, string thumbnailUrl, string source, string previewLink,
            string? publisher = null, int? pageCount = null, int? publishedYear = null, 
            string? isbn = null, string? categories = null, string? bookUrl = null, 
            string? language = null, double? averageRating = null, int? ratingsCount = null)
        {
            if (string.IsNullOrEmpty(bookId) || string.IsNullOrEmpty(title))
            {
                return BadRequest("Book information is incomplete");
            }

            // Use UserId from claims instead of getting user from UserManager
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }
            
            // Ensure userId is not null for database operations
            userId ??= string.Empty;

            // Provide a default thumbnail if missing
            if (string.IsNullOrEmpty(thumbnailUrl))
            {
                thumbnailUrl = "/images/no-cover.png";
            }

            // Check if the book is already saved by this user
            var existingSavedBook = await _context.SavedBooks
                .FirstOrDefaultAsync(b => b.BookId == bookId && b.UserId == userId);

            if (existingSavedBook != null)
            {
                TempData["Message"] = "This book is already in your saved library.";
                // If source is GoogleBooks or OpenLibrary, redirect accordingly
                if (source == "GoogleBooks")
                {
                    return RedirectToAction("BookDetail", new { id = bookId });
                }
                else
                {
                    return RedirectToAction("OpenLibraryDetail", new { key = bookId.Replace("/works/", "") });
                }
            }

            var savedBook = new SavedBook
            {
                UserId = userId,
                BookId = bookId,
                Title = title,
                Authors = authors,
                Description = description != null ? (description.Length > 1000 ? description.Substring(0, 997) + "..." : description) : null,
                ThumbnailUrl = thumbnailUrl,
                Source = source,
                SavedDate = DateTime.Now,
                PreviewLink = previewLink,
                // Enhanced metadata
                Publisher = publisher,
                PageCount = pageCount,
                PublishedYear = publishedYear,
                ISBN = isbn,
                Categories = categories,
                BookUrl = bookUrl,
                Language = language,
                AverageRating = averageRating,
                RatingsCount = ratingsCount
            };

            _context.SavedBooks.Add(savedBook);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Book saved to your library successfully!";

            // Redirect back to the book detail page
            if (source == "GoogleBooks")
            {
                return RedirectToAction("BookDetail", new { id = bookId });
            }
            else
            {
                return RedirectToAction("OpenLibraryDetail", new { key = bookId.Replace("/works/", "") });
            }
        }        [Authorize]
        [HttpPost]
        public async Task<IActionResult> DownloadBook(string bookId, string title, string authors, 
            string coverImageUrl, string format, string sourceType, string bookUrl, string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = returnUrl ?? Url.Action("Index", "Home") });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }
            
            // Ensure userId is not null for database operations
            userId ??= string.Empty;

            // Check if book already downloaded
            var existingDownload = await _context.DownloadedBooks
                .FirstOrDefaultAsync(b => b.BookId == bookId && b.UserId == userId && b.Format == format);

            if (existingDownload == null)
            {
                // Save the download record
                var downloadedBook = new DownloadedBook
                {
                    BookId = bookId,
                    Title = title,
                    Authors = authors,
                    CoverImageUrl = coverImageUrl,
                    Format = format,
                    SourceType = sourceType,
                    DownloadDate = DateTime.Now,
                    UserId = userId,
                    BookUrl = bookUrl
                };

                _context.DownloadedBooks.Add(downloadedBook);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Book '{title}' successfully downloaded in {format} format.";
            }
            else
            {
                TempData["InfoMessage"] = $"You have already downloaded '{title}' in {format} format.";
            }

            // Redirect back to details or provided return URL
            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else if (sourceType == "OpenLibrary")
            {
                return RedirectToAction("OpenLibraryDetail", new { key = bookId.Replace("/works/", "") });
            }
            else
            {
                return RedirectToAction("BookDetail", new { id = bookId });
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SubscribeToService(string subscriptionType)
        {
            if (string.IsNullOrEmpty(subscriptionType))
            {
                return BadRequest("Subscription information is missing");
            }

            // Use UserId from claims instead of getting user from UserManager
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }
            
            // Ensure userId is not null for database operations
            userId ??= string.Empty;

            // Check if user already has an active subscription of this type
            var existingSubscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.SubscriptionType == subscriptionType && s.IsActive);

            if (existingSubscription != null)
            {
                TempData["Message"] = $"You already have an active {subscriptionType} subscription.";
                return RedirectToAction("Index", "UserLibrary");
            }

            var subscription = new UserSubscription
            {
                UserId = userId,
                SubscriptionType = subscriptionType,
                SubscriptionDate = DateTime.Now,
                ExpirationDate = DateTime.Now.AddYears(1),
                IsActive = true,
                SubscriptionDetails = $"Annual subscription to {subscriptionType}"
            };

            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Successfully subscribed to {subscriptionType}!";
            return RedirectToAction("Index", "UserLibrary");
        }

        private async Task<bool> IsBookSavedAsync(string bookId, string userId)
        {
            return await _context.SavedBooks
                .AnyAsync(b => b.BookId == bookId && b.UserId == userId);
        }

        private async Task<bool> IsBookDownloadedAsync(string bookId, string userId)
        {
            return await _context.DownloadedBooks
                .AnyAsync(b => b.BookId == bookId && b.UserId == userId);
        }

        private async Task<SavedBook?> GetSavedBookAsync(string bookId, string userId)
        {
            return await _context.SavedBooks
                .FirstOrDefaultAsync(b => b.BookId == bookId && b.UserId == userId);
        }

        private async Task<DownloadedBook?> GetDownloadedBookAsync(string bookId, string userId)
        {
            return await _context.DownloadedBooks
                .FirstOrDefaultAsync(b => b.BookId == bookId && b.UserId == userId);
        }
    }
}
