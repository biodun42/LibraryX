using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LibraryX.Data;
using LibraryX.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LibraryX.Controllers
{
    [Authorize]
    public class UserLibraryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserLibraryController> _logger;

        public UserLibraryController(
            ApplicationDbContext context,
            ILogger<UserLibraryController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int pageSize = 20;
            
            // Get saved books with pagination
            var savedBooks = await _context.SavedBooks
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.SavedDate)
                .ToListAsync();
                
            // Get downloaded books with pagination
            var downloadedBooks = await _context.DownloadedBooks
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.DownloadDate)
                .ToListAsync();

            // Ensure no null BookUrl values are present
            foreach (var download in downloadedBooks)
            {
                download.BookUrl ??= "";
            }

            // Get user's subscription information
            var subscriptions = await _context.UserSubscriptions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();
            
            var model = new UserLibraryViewModel
            {
                SavedBooks = savedBooks,
                DownloadedBooks = downloadedBooks,
                Subscriptions = subscriptions,
                CurrentPage = page,
                ItemsPerPage = pageSize,
                TotalSavedItems = savedBooks.Count,
                TotalDownloadedItems = downloadedBooks.Count
            };
            
            return View(model);
        }

        // Remove saved book
        [HttpPost]
        public async Task<IActionResult> RemoveSaved(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var savedBook = await _context.SavedBooks
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
                
            if (savedBook == null)
            {
                TempData["ErrorMessage"] = "Book not found in your library.";
                return RedirectToAction("Index");
            }
            
            _context.SavedBooks.Remove(savedBook);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Book removed from your library.";
            return RedirectToAction("Index");
        }

        // Remove downloaded book
        [HttpPost]
        public async Task<IActionResult> RemoveDownloaded(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var downloadedBook = await _context.DownloadedBooks
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
                
            if (downloadedBook == null)
            {
                TempData["ErrorMessage"] = "Downloaded book not found.";
                return RedirectToAction("Index");
            }
            
            _context.DownloadedBooks.Remove(downloadedBook);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Downloaded book removed successfully.";
            return RedirectToAction("Index");
        }

        // View book details action
        public IActionResult ViewBookDetails(string bookId, string source)
        {
            if (source?.Equals("GoogleBooks", StringComparison.OrdinalIgnoreCase) == true)
            {
                return RedirectToAction("BookDetail", "Home", new { id = bookId });
            }
            else if (source?.Equals("OpenLibrary", StringComparison.OrdinalIgnoreCase) == true)
            {
                return RedirectToAction("OpenLibraryDetail", "Home", new { key = bookId.Replace("/works/", "") });
            }
            
            return RedirectToAction("Index");
        }

        // Subscribe to a service
        [HttpPost]
        public async Task<IActionResult> SubscribeToService(string subscriptionType)
        {
            if (string.IsNullOrEmpty(subscriptionType))
            {
                TempData["ErrorMessage"] = "Subscription information is missing";
                return RedirectToAction("Index");
            }            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge(); // Redirect to login if userId is null
            }

            // Check if user already has an active subscription of this type
            var existingSubscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.SubscriptionType == subscriptionType && s.IsActive);

            if (existingSubscription != null)
            {
                TempData["Message"] = $"You already have an active {subscriptionType} subscription.";
                return RedirectToAction("Index");
            }

            // Create new subscription
            var subscription = new UserSubscription
            {
                UserId = userId,
                SubscriptionType = subscriptionType,
                SubscriptionDate = DateTime.Now,
                ExpirationDate = DateTime.Now.AddYears(1),
                IsActive = true,
                SubscriptionDetails = GetSubscriptionDetails(subscriptionType)
            };            _context.UserSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Successfully subscribed to {subscriptionType}!";
            return RedirectToAction("Index", new { subscribed = true, type = subscriptionType });
        }        // Cancel a subscription
        [HttpPost]
        public async Task<IActionResult> CancelSubscription(string subscriptionType)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge(); // Redirect to login if userId is null
            }
            
            var subscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(s => s.SubscriptionType == subscriptionType && s.UserId == userId && s.IsActive);
                
            if (subscription == null)
            {
                TempData["ErrorMessage"] = "Active subscription not found.";
                return RedirectToAction("Index");
            }
            
            // Instead of deleting, just set it to inactive
            subscription.IsActive = false;
            await _context.SaveChangesAsync();
              TempData["SuccessMessage"] = $"Your {subscription.SubscriptionType} subscription has been cancelled.";
            return RedirectToAction("Index");        }
        
        // Renew a subscription
        [HttpPost]
        public async Task<IActionResult> RenewSubscription(string subscriptionType)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge(); // Redirect to login if userId is null
            }
            
            var subscription = await _context.UserSubscriptions
                .FirstOrDefaultAsync(s => s.SubscriptionType == subscriptionType && s.UserId == userId);
                
            if (subscription == null)
            {
                // If no subscription exists, create a new one instead
                return RedirectToAction("SubscribeToService", new { subscriptionType });
            }

            // Renew the subscription for another year            subscription.IsActive = true;
            subscription.SubscriptionDate = DateTime.Now;
            subscription.ExpirationDate = DateTime.Now.AddYears(1);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = $"Your {subscription.SubscriptionType} subscription has been renewed for one year.";
            return RedirectToAction("Index", new { subscribed = true, type = subscription.SubscriptionType, renewed = true });
        }

        private string GetSubscriptionDetails(string subscriptionType)
        {
            return subscriptionType switch
            {
                "Basic Plan" => "Access to standard library features with basic downloads and storage limits.",
                "Premium Plan" => "Enhanced access with unlimited downloads and premium features.",
                "Academic Plan" => "Access to academic resources, journals, and research papers.",
                _ => $"Annual subscription to {subscriptionType}"
            };
        }
    }
}
