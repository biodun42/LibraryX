# 📚 LibraryX - Advanced Digital Library Platform 📚

![LibraryX Banner](https://via.placeholder.com/1200x400?text=LibraryX+-+Digital+Library+Platform)

_Elevating digital reading experiences through modern technology_

[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-9.0-blue.svg)](https://dotnet.microsoft.com/en-us/apps/aspnet)
[![TailwindCSS](https://img.shields.io/badge/TailwindCSS-v3.0-38bdf8.svg)](https://tailwindcss.com/)
[![SQLite](https://img.shields.io/badge/SQLite-3.0-003B57.svg)](https://www.sqlite.org/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT)

## 📚 Project Overview

**LibraryX** is a sophisticated, feature-rich digital library platform meticulously crafted with ASP.NET Core MVC architecture. This cutting-edge application provides seamless access to millions of books through strategic integrations with multiple industry-standard book APIs.

The platform serves as a comprehensive solution for modern readers, enabling users to:

- **Discover** new books through intelligent search algorithms
- **Save** favorites to personalized collections with custom organization systems
- **Download** content in various formats compatible with major e-readers
- **Manage** digital collections through an intuitive user interface
- **Subscribe** to premium tiers

The system leverages the extensive catalogs of both Google Books API and Open Library API, offering a unified search experience across disparate data sources while maintaining performance optimization and data consistency.

## 🚀 Key Features & Capabilities

### 📱 User Experience

- **Modern Interface**: Glass morphism design principles with subtle shadows, translucency effects, and fluid animations creating an immersive reading environment
- **Responsive Design**: Fully adaptive layout system automatically optimizing the interface across desktop, tablet, mobile and e-reader devices

### 🔍 Book Discovery

- **Dual API Integration**: Unified search system concurrently querying Google Books API and Open Library API with optimized request handling and response merging
- **Advanced Search**: Multi-faceted filtering system allowing complex queries by title, author, publisher, category, publication date, ratings, and more
- **Recommendation Engine**: Content suggestion algorithm based on user reading history, saved preferences, and trending titles

### 🔐 Security & Accounts

- **Identity Framework**: Enterprise-grade authentication system with role-based permission controls
- **User Preferences**: Persistent storage of reading preferences, UI settings, and personalization options
- **Cross-device Sync**: Secure synchronization of user libraries and reading progress across multiple devices

## 🛠️ Technology Stack

### Backend Architecture

- **Framework**: ASP.NET Core 9.0 MVC (Model View Controller) — Latest platform with enhanced performance metrics and security features
- **Database**: SQLite (library.db) — Lightweight, embedded database solution with zero configuration requirements
- **ORM**: Entity Framework Core — Object-relational mapping for efficient data access and manipulation
- **Authentication**: ASP.NET Core Identity — Comprehensive security framework with multi-factor authentication support

### Frontend Implementation

- **Styling**: TailwindCSS with custom utility classes — Flexible, utility-first CSS framework for rapid UI development
- **JavaScript**: Vanilla JS with modern ES6+ features — Optimized for performance without heavy framework overhead
- **Icons**: Font Awesome 5 — Comprehensive icon library with consistent visual language
- **Animations**: Custom CSS animations — Lightweight, hardware-accelerated transitions and effects

### External API Integrations

- **Google Books API**: RESTful service providing access to Google's vast book database

  - Endpoints utilized: volumes/search, volumes/{id}, volumes?q=subject:{subject}
  - Authentication: API key-based access
  - Rate limiting: 1,000 requests per day

- **Open Library API**: Open-source book metadata repository
  - Endpoints utilized: search.json, works/{key}.json, authors/{key}.json
  - Authentication: None required
  - Rate limiting: Best practice guidelines applied

## 📂 Project Architecture & Structure

### Controllers (Application Flow Coordination)

- **HomeController**: Central controller managing main application navigation and book discovery

  - **Routes & Functionality**:
    - `Index()`: Landing page with featured books and personalized recommendations
    - `About()`: Application information and team details
    - `Privacy()`: Privacy policy and data handling practices
    - `Contact()`: User support and feedback channels
    - `Catalog([parameters])`: Paginated book search with filtering
    - `BookDetail(id)`: Detailed Google Books API volume information
    - `OpenLibrary([parameters])`: Open Library catalog interface
    - `OpenLibraryDetail(key)`: Detailed Open Library work information
  - **Key Dependencies**: BookService, OpenLibraryService

- **UserLibraryController**: Manages personalized user collection functionality
  - **Routes & Functionality**:
    - `Index()`: Dashboard showing saved books, downloads, and subscription status
    - `RemoveSaved(id)`: Removes book from user's saved collection
    - `RemoveDownloaded(id)`: Removes book from user's downloaded collection
    - `ViewBookDetails(id, source)`: Displays comprehensive book information
    - `SubscribeToService(tier)`: Initiates premium subscription process
    - `CancelSubscription()`: Terminates active subscription
  - **Key Dependencies**: User authentication, database context

### Services (Business Logic & External Integration)

- **BookService**: Google Books API integration layer

  - **Implementation Details**:
    - Asynchronous HTTP client with retry policies
    - Response caching for frequently accessed resources
    - Deserialization to strongly-typed models
  - **Key Methods**:
    - `SearchBooksAsync(query, category, startIndex, maxResults)`: Executes parameterized search
    - `GetBookByIdAsync(id)`: Retrieves single book details
    - `GetRelatedBooksAsync(categories, title, author)`: Finds similar content
    - `GetNewArrivalsAsync(category, maxResults)`: Retrieves recently published books

- **OpenLibraryService**: Open Library API integration layer
  - **Implementation Details**:
    - Connection pooling for efficient resource utilization
    - Input sanitization and validation
    - Error handling with meaningful user feedback
  - **Key Methods**:
    - `SearchBooksAsync(query, subject, page, limit)`: Executes parameterized search
    - `GetFeaturedBooksAsync(subject, limit)`: Retrieves curated collections
    - `GetBookDetailsByKeyAsync(key)`: Retrieves comprehensive work information
    - `GetRelatedBooksAsync(subject, author, limit)`: Finds related content

### Models (Data Representations)

#### Core Models (Domain Entities)

- **ApplicationUser**: Extended identity user with application-specific properties

  ```csharp
  public class ApplicationUser : IdentityUser
  {
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public DateTime RegistrationDate { get; set; }
      public string ProfileImageUrl { get; set; }
      public bool EmailNotificationsEnabled { get; set; }

      public virtual ICollection<SavedBook> SavedBooks { get; set; }
      public virtual ICollection<DownloadedBook> DownloadedBooks { get; set; }
      public virtual UserSubscription Subscription { get; set; }
  }
  ```

- **Book**: Structured representation of Google Books API response

  ```csharp
  public class Book
  {
      public string Id { get; set; }
      public string Title { get; set; }
      public List<string> Authors { get; set; }
      public string Description { get; set; }
      public string Publisher { get; set; }
      public int? PageCount { get; set; }
      public string ThumbnailUrl { get; set; }
      public string PreviewLink { get; set; }
      public string InfoLink { get; set; }
      public string PublishedDate { get; set; }
      public List<string> Categories { get; set; }
      public string Language { get; set; }
      public double? AverageRating { get; set; }
      public int? RatingsCount { get; set; }
      public Dictionary<string, string> IndustryIdentifiers { get; set; }
  }
  ```

- **OpenLibraryBook**: Structured representation of Open Library API response

  ```csharp
  public class OpenLibraryBook
  {
      public string Key { get; set; }
      public string Title { get; set; }
      public List<Author> Authors { get; set; }
      public string Description { get; set; }
      public int? FirstPublishYear { get; set; }
      public List<string> Subjects { get; set; }
      public string CoverImageUrl { get; set; }
      public List<Edition> Editions { get; set; }

      public class Author
      {
          public string Key { get; set; }
          public string Name { get; set; }
      }

      public class Edition
      {
          public string Key { get; set; }
          public string Format { get; set; }
          public string PublishedDate { get; set; }
          public string Publisher { get; set; }
          public string ISBN { get; set; }
      }
  }
  ```

- **SavedBook**: Database entity for user's saved books

  ```csharp
  public class SavedBook
  {
      public int Id { get; set; }
      public string UserId { get; set; }
      public string BookId { get; set; }
      public string Title { get; set; }
      public string Authors { get; set; }
      public string Description { get; set; }
      public string ThumbnailUrl { get; set; }
      public string Source { get; set; }  // "GoogleBooks" or "OpenLibrary"
      public DateTime SavedDate { get; set; }
      public string PreviewLink { get; set; }
      public string Publisher { get; set; }
      public int? PageCount { get; set; }
      public string PublishedYear { get; set; }
      public string ISBN { get; set; }
      public string Categories { get; set; }
      public string BookUrl { get; set; }
      public string Language { get; set; }
      public double? AverageRating { get; set; }
      public int? RatingsCount { get; set; }

      public virtual ApplicationUser User { get; set; }
  }
  ```

- **DownloadedBook**: Database entity for user's downloaded books

  ```csharp
  public class DownloadedBook
  {
      public int Id { get; set; }
      public string UserId { get; set; }
      public string BookId { get; set; }
      public string Title { get; set; }
      public string Authors { get; set; }
      public string Format { get; set; }  // "PDF", "EPUB", etc.
      public string Source { get; set; }
      public DateTime DownloadDate { get; set; }
      public string ThumbnailUrl { get; set; }
      public string SourceType { get; set; }
      public string BookUrl { get; set; }
      public string CoverImageUrl { get; set; }

      public virtual ApplicationUser User { get; set; }
  }
  ```

- **UserSubscription**: Database entity for premium subscription details

  ```csharp
  public class UserSubscription
  {
      public int Id { get; set; }
      public string UserId { get; set; }
      public string SubscriptionType { get; set; }  // "Basic", "Premium", "Ultimate"
      public DateTime SubscriptionDate { get; set; }
      public DateTime ExpirationDate { get; set; }
      public bool IsActive { get; set; }
      public string SubscriptionDetails { get; set; } // JSON string with tier-specific details

      public virtual ApplicationUser User { get; set; }
  }
  ```

#### All The Pages (Presentation Layer)

The application features a comprehensive set of pages, each meticulously designed to fulfill specific user needs and enhance the overall user experience:

- **Register Page**: A streamlined user onboarding interface with progressive form validation, social authentication options, and responsive design. Implements client-side validation with immediate feedback and password strength indicators for enhanced security.

- **Login Page**: Secure authentication portal with multi-factor authentication support, "remember me" functionality, and secure password recovery workflow. Features intelligent error messaging and brute-force protection through incremental delays and CAPTCHA integration.

- **Profile Page**: Personalized user dashboard displaying account information, reading preferences, and subscription details. Includes avatar customization, notification preferences, and comprehensive account management tools with real-time updates via AJAX.

- **Home Page**: Dynamic landing page featuring a hero section with animated search functionality, personalized book recommendations based on user history, featured collections with cover art carousels, and trending titles updated in real-time. Implements lazy loading for optimal performance and skeleton loading states for enhanced perceived speed.

- **User Library Page**: Comprehensive personal collection management interface with distinct sections for saved and downloaded books. Features advanced sorting and filtering capabilities, custom collection organization through drag-and-drop functionality, reading progress tracking, and contextual action menus for efficient library management.

- **GoogleBooks Page**: Sophisticated catalog interface for Google Books API with intelligent search algorithms, advanced filtering options, and infinite scroll pagination. Includes toggle between grid/list views, search history tracking, and dynamic search suggestions with debounced input handling for optimal performance.

- **OpenLibrary Page**: Specialized interface for Open Library resources featuring subject-based browsing, author spotlight sections, historical publication timeline, and curated collection highlights. Implements content virtualization for handling large result sets efficiently.

- **GoogleBooks Book Details Page**: Immersive book exploration interface with high-resolution cover visualization, publisher metadata with typographic hierarchy, expandable table of contents, preview functionality integration, and related books recommendation engine using collaborative filtering algorithms.

- **OpenLibrary Book Details Page**: Comprehensive work information display with multiple editions comparison, author biography integration, subject classification visualization, and historical context information where available. Features semantic relationship mapping between related works and authors.

- **About Page**: Engaging company narrative with team profiles, mission statement, technology stack overview, and development roadmap. Implements parallax scrolling effects, animated statistics, and interactive timeline of platform evolution.

- **Privacy Page**: Transparent data handling policies presented in a user-friendly format with collapsible sections, visual iconography for key concepts, and contextual examples. Features interactive controls for managing user consent preferences and data export capabilities in compliance with GDPR and CCPA regulations.

- **Contact Page**: Advanced communication hub featuring:
  - **Multi-channel Support System**: Unified interface integrating email, live chat, and ticket-based support with real-time status tracking
  - **Intelligent Routing Algorithm**: Automated message categorization and priority assignment using natural language processing
  - **Interactive Contact Form**: Dynamic form fields that adapt based on inquiry type with smart validation and attachment support
  - **Scheduled Consultation System**: Integrated calendar for booking video consultations with support specialists
  - **Knowledge Base Integration**: Contextual suggestion engine recommending relevant help articles as users type their inquiries
  - **Geographic Support Localization**: Automatic routing to regional support teams based on user location
  - **Response Time Estimator**: AI-driven prediction of expected response times based on current support queue and inquiry complexity
  - **Accessibility Features**: Screen reader optimization, keyboard navigation support, and high-contrast mode support
  - **Feedback Loop System**: Post-resolution satisfaction surveys with actionable analytics dashboard for support quality improvement

#### View Models (Presentation Layer)

- **CatalogViewModel**: Structured data for book catalog presentation
- **BookDetailViewModel**: Comprehensive information for individual book display
- **OpenLibraryDetailViewModel**: Specialized structure for Open Library works
- **UserLibraryViewModel**: Composite model for user's library dashboard

### Views (User Interface)

The application leverages a sophisticated view hierarchy with partial views for reusability:

- **Home/Index**: Engaging landing page featuring:

  - Hero section with animated search box
  - Dynamic featured books carousel
  - Personalized recommendation section
  - Latest arrivals with cover art
  - Genre exploration blocks

- **Home/Catalog**: Google Books catalog implementing:

  - Infinite scroll pagination
  - Real-time search suggestions
  - Advanced filtering sidebar
  - Toggle between grid/list views
  - Book card hover effects

- **Home/BookDetail**: Detailed book information display:

  - High-resolution cover visualization
  - Publisher metadata with formatted typography
  - Table of contents expansion panel
  - Preview functionality integration
  - Related books recommendation carousel

- **Home/OpenLibrary**: Open Library catalog with:

  - Subject-based browsing interface
  - Author spotlight sections
  - Historical publication timeline
  - Collection highlights with custom illustrations

- **Home/OpenLibraryDetail**: Comprehensive Open Library work display:

  - Multiple editions comparison
  - Author biography integration
  - Subject classification visualization
  - External reference links with validation

- **UserLibrary/Index**: User's personalized collection portal:

  - Tab-based navigation between saved/downloaded content
  - Collection management tools with drag-drop organization
  - Subscription status with visual progress indicators
  - Download history with format filtering

- **Shared/\_Layout**: Master template implementing:
  - Responsive navigation with mobile optimization
  - Dynamic theme switching capability
  - User authentication state visualization
  - Accessibility controls and keyboard shortcuts
  - Smooth page transition effects

## 📊 Database Architecture & Schema

The application implements a carefully structured database design using SQLite with Entity Framework Core as the ORM. This approach provides excellent performance characteristics while maintaining zero external dependencies.

### Identity Framework Schema

ASP.NET Core Identity establishes a comprehensive security foundation with the following tables:

- **AspNetUsers**: Central user account repository

  - Primary Key: Id (string, GUID format)
  - Notable Columns: UserName, Email, PasswordHash, SecurityStamp, PhoneNumber
  - Extended Properties: FirstName, LastName, RegistrationDate, ProfileImageUrl, EmailNotificationsEnabled
  - Relationships: One-to-many with SavedBooks, DownloadedBooks; One-to-one with UserSubscription

- **AspNetRoles**: Role definitions for authorization

  - Primary Key: Id (string, GUID format)
  - Notable Columns: Name, NormalizedName, ConcurrencyStamp
  - Default Roles: "User", "Premium", "Administrator"

- **AspNetUserRoles**: Junction table linking users to roles

  - Composite Key: UserId, RoleId
  - Enforces role-based authorization throughout the application

- **AspNetUserClaims**: Stores additional user authorization claims

  - Primary Key: Id (integer)
  - Foreign Key: UserId
  - Notable Columns: ClaimType, ClaimValue

- **AspNetUserLogins**: External authentication providers

  - Composite Key: LoginProvider, ProviderKey
  - Foreign Key: UserId
  - Enables social media authentication options

- **AspNetUserTokens**: Authentication tokens for password resets, 2FA

  - Composite Key: UserId, LoginProvider, Name
  - Notable Columns: Value (encrypted token data)

- **AspNetRoleClaims**: Authorization rules associated with roles
  - Primary Key: Id (integer)
  - Foreign Key: RoleId
  - Notable Columns: ClaimType, ClaimValue

### Application-Specific Schema

Custom tables implementing the core functionality:

- **SavedBooks**: User's saved book collection

  - Primary Key: Id (integer, auto-increment)
  - Foreign Key: UserId → AspNetUsers.Id (CASCADE on delete)
  - Notable Columns:
    - BookId: External API identifier
    - Title, Authors: Core book metadata
    - Description: Full text with HTML support
    - ThumbnailUrl: Cover image URL (nullable)
    - Source: API source identifier
    - SavedDate: Timestamp of save action
    - PreviewLink: URL to content preview
    - Publisher, PageCount: Publishing metadata
    - PublishedYear: Extracted/normalized from PublishedDate
    - ISBN: International Standard Book Number
    - Categories: Comma-separated category list
    - BookUrl: Canonical URL to book resource
    - Language: ISO language code
    - AverageRating: Aggregate user rating (0-5 scale)
    - RatingsCount: Number of ratings submitted
  - Indexes: UserId, BookId (for efficient queries)

- **DownloadedBooks**: User's downloaded book collection

  - Primary Key: Id (integer, auto-increment)
  - Foreign Key: UserId → AspNetUsers.Id (CASCADE on delete)
  - Notable Columns:
    - BookId: External API identifier
    - Title, Authors: Core book metadata
    - Format: File format specification
    - Source: API source identifier
    - DownloadDate: Timestamp of download action
    - ThumbnailUrl: Cover image URL (nullable)
    - SourceType: Classification of content source
    - BookUrl: Canonical URL to book resource
    - CoverImageUrl: High-resolution cover image
  - Indexes: UserId, BookId, Format (for filtered queries)

- **UserSubscriptions**: Premium subscription management
  - Primary Key: Id (integer, auto-increment)
  - Foreign Key: UserId → AspNetUsers.Id (CASCADE on delete)
  - Notable Columns:
    - SubscriptionType: Tier classification
    - SubscriptionDate: Start of current subscription
    - ExpirationDate: End of current subscription
    - IsActive: Boolean subscription status
    - SubscriptionDetails: JSON-formatted tier benefits
  - Constraints: Unique UserId (one subscription per user)

### Entity Relationships

- **User-to-SavedBooks**: One-to-many relationship allowing users to maintain collections of unlimited size
- **User-to-DownloadedBooks**: One-to-many relationship tracking user's downloaded content
- **User-to-UserSubscription**: One-to-one relationship enforcing single subscription state
- **User-to-Roles**: Many-to-many relationship implemented through AspNetUserRoles junction table

## 🔄 API Integration Architecture

### Google Books API Integration

The application implements a sophisticated integration with Google Books API through the `BookService` class. This service provides programmatic access to Google's extensive book catalog with optimized request handling and response normalization.

#### Implementation Details

- **Base URL**: https://www.googleapis.com/books/v1/
- **Authentication**: API key included as query parameter
- **Rate Limiting**: Implements exponential backoff for 429 responses
- **Caching Strategy**: Memory cache with sliding expiration for frequent queries

#### Key Methods & Functionality

- **SearchBooksAsync**

  ```csharp
  public async Task<(List<Book> Books, int TotalItems)> SearchBooksAsync(
      string query,
      string category = null,
      int startIndex = 0,
      int maxResults = 10)
  {
      // Constructs search query with proper escaping and filtering
      // Handles pagination through startIndex parameter
      // Returns tuple with books and total count for pagination
  }
  ```

- **GetBookByIdAsync**

  ```csharp
  public async Task<Book> GetBookByIdAsync(string id)
  {
      // Retrieves detailed information about a specific volume
      // Includes additional metadata not available in search results
      // Implements null safety and fallback values for missing data
  }
  ```

- **GetRelatedBooksAsync**

  ```csharp
  public async Task<List<Book>> GetRelatedBooksAsync(
      List<string> categories,
      string title,
      string author,
      int maxResults = 5)
  {
      // Finds books with similar characteristics
      // Uses weighted algorithm prioritizing category matches
      // Excludes the original book from results
  }
  ```

- **GetNewArrivalsAsync**
  ```csharp
  public async Task<List<Book>> GetNewArrivalsAsync(
      string category = null,
      int maxResults = 10)
  {
      // Retrieves recently published books
      // Sorts by publication date
      // Optionally filters by category
  }
  ```

#### Error Handling & Resilience

- Implements circuit breaker pattern for handling API outages
- Graceful degradation when API limits are reached
- Comprehensive logging with contextual information
- User-friendly error messages mapped to technical issues

### Open Library API Integration

The application leverages the Open Library API through the `OpenLibraryService` class, providing access to this comprehensive open-source book database. The implementation focuses on data normalization and efficient resource utilization.

#### Implementation Details

- **Base URL**: https://openlibrary.org/
- **Authentication**: None required (public API)
- **Connection Management**: Persistent HttpClient with connection pooling
- **Response Handling**: Deserializes JSON to strongly-typed models with validation

#### Key Methods & Functionality

- **SearchBooksAsync**

  ```csharp
  public async Task<(List<OpenLibraryBook> Books, int TotalItems)> SearchBooksAsync(
      string query,
      string subject = null,
      int page = 1,
      int limit = 10)
  {
      // Executes search against Open Library search API
      // Normalizes pagination parameters
      // Returns tuple with books and total count
  }
  ```

- **GetFeaturedBooksAsync**

  ```csharp
  public async Task<List<OpenLibraryBook>> GetFeaturedBooksAsync(
      string subject = null,
      int limit = 10)
  {
      // Retrieves curated collections or trending books
      // Optionally filters by subject
      // Enhances results with additional metadata
  }
  ```

- **GetBookDetailsByKeyAsync**

  ```csharp
  public async Task<OpenLibraryBook> GetBookDetailsByKeyAsync(string key)
  {
      // Retrieves comprehensive details for a work
      // Includes author information and available editions
      // Processes and normalizes description formatting
  }
  ```

- **GetRelatedBooksAsync**
  ```csharp
  public async Task<List<OpenLibraryBook>> GetRelatedBooksAsync(
      string subject,
      string author,
      int limit = 5)
  {
      // Finds related works based on subject and author
      // Implements relevance ranking algorithm
      // Normalizes response data for consistency
  }
  ```

#### Data Transformation & Normalization

- Consistent author name formatting across API variations
- Structured parsing of publication dates with fallback strategies
- ISBN normalization and validation
- Image URL resolution with size variations

## 📱 User Interface Architecture

The user interface implements a modern design approach focusing on readability, accessibility, and responsive behavior. Key design principles include:

### Responsive Behavior Implementation

- **Mobile-First Approach**: Base styles target mobile devices
- **Breakpoint System**:
  - Small: 0-640px (phones)
  - Medium: 641-768px (tablets)
  - Large: 769-1024px (laptops)
  - Extra Large: 1025px+ (desktops)
- **Layout Transformation**: Grid-to-list view transitions
- **Navigation Adaptation**: Collapsible menu on smaller screens

### Component Architecture

- **Book Cards**: Reusable components with consistent metadata display
- **Search Interface**: Expandable input with integrated filters
- **Tab Navigation**: Context-aware navigation between related views
- **Modal Dialogs**: Lightweight overlay system for detail views

### Accessibility Implementation

- **ARIA Attributes**: Comprehensive screen reader support
- **Keyboard Navigation**: Full functionality without mouse
- **Color Contrast**: WCAG AA compliance (4.5:1 minimum ratio)
- **Focus Indicators**: Visible focus states for all interactive elements
- **Reduced Motion Options**: Respects user preference for animation reduction

## 🔐 Authentication & Authorization Architecture

The application implements ASP.NET Core Identity for robust security with customizations:

### Identity Configuration

```csharp
services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password complexity requirements
    options.Password.RequiredLength = 10;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;

    // User requirements
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    // SignIn requirements
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
```

### Authorization Policies

```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("RequireBasicSubscription", policy =>
        policy.RequireAssertion(context =>
            context.User.IsInRole("Premium") ||
            context.User.IsInRole("Administrator")));

    options.AddPolicy("RequirePremiumSubscription", policy =>
        policy.RequireRole("Premium", "Administrator"));

    options.AddPolicy("RequireAdministrator", policy =>
        policy.RequireRole("Administrator"));
});
```

### Security Headers

```csharp
app.UseHsts();
app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' https://cdn.jsdelivr.net; " +
        "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
        "font-src 'self' https://fonts.gstatic.com; " +
        "img-src 'self' https://covers.openlibrary.org https://books.google.com data:;");

    await next();
});
```

### Authentication Flow

1. **Registration Process**:

   - User submits registration form with required fields
   - System validates input and checks for existing email
   - Confirmation email sent with verification token
   - Account remains inactive until email verification
   - Default "User" role assigned automatically

2. **Login Process**:

   - Credentials validated against stored password hash
   - Failed attempts tracked for account lockout
   - Remember-me functionality with secure persistent cookies
   - Post-login redirect to intended destination or default page

3. **Password Management**:
   - Secure password reset with time-limited tokens
   - Password history to prevent reuse of recent passwords
   - Regular password expiration notifications
   - Secure password change verification

## 🌟 Premium Subscription System

The application implements a tiered subscription model providing enhanced features:

### Subscription Tiers

1. **Google Books Premium**

   - Access to premium Google Books catalog
   - Unlimited full-text downloads
   - Priority access to new releases
   - Advanced search & filtering options

2. **Open Library Premium**

   - Unlimited access to rare and historical books
   - Exclusive access to archive materials
   - Enhanced metadata and reading tools
   - Personal library organization features

### Implementation Architecture

```csharp
// UserLibraryController.cs
[HttpPost]
[Authorize]
public async Task<IActionResult> SubscribeToService(string tier)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var user = await _userManager.FindByIdAsync(userId);

    // Check if user already has a subscription
    var existingSubscription = await _dbContext.UserSubscriptions
        .FirstOrDefaultAsync(s => s.UserId == userId);

    if (existingSubscription != null)
    {
        // Update existing subscription
        existingSubscription.SubscriptionType = tier;
        existingSubscription.SubscriptionDate = DateTime.UtcNow;
        existingSubscription.ExpirationDate = DateTime.UtcNow.AddMonths(1);
        existingSubscription.IsActive = true;

        // Update subscription details based on tier
        existingSubscription.SubscriptionDetails = JsonSerializer.Serialize(
            GetSubscriptionDetails(tier));
    }
    else
    {
        // Create new subscription
        var subscription = new UserSubscription
        {
            UserId = userId,
            SubscriptionType = tier,
            SubscriptionDate = DateTime.UtcNow,
            ExpirationDate = DateTime.UtcNow.AddMonths(1),
            IsActive = true,
            SubscriptionDetails = JsonSerializer.Serialize(
                GetSubscriptionDetails(tier))
        };

        _dbContext.UserSubscriptions.Add(subscription);

        // Assign appropriate role based on tier
        if (tier == "Premium" || tier == "Ultimate")
        {
            await _userManager.AddToRoleAsync(user, "Premium");
        }
    }

    await _dbContext.SaveChangesAsync();

    return RedirectToAction("Index", "UserLibrary");
}
```

### Subscription Access Control

Feature access is controlled through policy-based authorization:

```csharp
// Example controller action with subscription requirement
[HttpGet]
[Authorize(Policy = "RequireBasicSubscription")]
public async Task<IActionResult> AdvancedSearch()
{
    // Implementation for premium users only
}
```

## 🚀 Getting Started Guide

### Prerequisites

Before installing LibraryX, ensure your development environment meets the following requirements:

- **.NET 9.0 SDK** or higher ([Download](https://dotnet.microsoft.com/download))
- **IDE**: Visual Studio 2022 ([Download](https://visualstudio.microsoft.com/vs/)) or Visual Studio Code ([Download](https://code.visualstudio.com/))
- **Database**: SQLite (included in project, no separate installation required)
- **API Keys**: Google Books API key (for production deployment)

### Installation Steps

1. **Clone the Repository**

   ```powershell
   git clone https://github.com/biodun42/LibraryX.git
   cd LibraryX
   ```

2. **Restore NuGet Packages**

   ```powershell
   dotnet restore
   ```

3. **Update Database**

   ```powershell
   dotnet ef database update
   ```

4. **Configure API Keys**

   - Create an `appsettings.Development.json` file in the project root
   - Add your API keys (for development environment)

   ```json
   {
     "ApiKey1": {
       "GoogleBooks": "https://www.googleapis.com/books/v1/volumes"
     },
     "ApiKey2": {
       "GoogleBooks": "https://openlibrary.org/search.json"
     }
   }
   ```

5. **Run the Application**

   ```powershell
   dotnet watch run
   ```

6. **Access the Application**
   - Open your browser and navigate to: https://localhost:5032
   - For first-time setup, register an administrator account using the registration form
   - Use the predefined seed data to explore the application features

### Development Configuration

The application uses environment-specific configuration files:

- `appsettings.json`: Base configuration shared across environments
- `appsettings.Development.json`: Development-specific settings (not committed to source control)
- `appsettings.Production.json`: Production-specific settings with secure values

Key configuration sections include:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=library.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "EmailSettings": {
    "SendGridKey": "[SENDGRID_KEY_HERE]",
    "SenderEmail": "noreply@libraryx.com",
    "SenderName": "LibraryX Support"
  },
  "FeatureManagement": {
    "EnableOpenLibraryIntegration": true,
    "EnableRecommendationEngine": true
  },
  "ApplicationSettings": {
    "MaxSearchResultsPerPage": 20,
    "DefaultSearchResultsPerPage": 10,
    "MaxDownloadsPerDay": {
      "Basic": 5,
      "Premium": 20,
      "Ultimate": 50
    }
  }
}
```

## 📄 License & Legal Information

This project is licensed under the MIT License - see the LICENSE file for details.

```
MIT License

Copyright (c) 2025 LibraryX Contributors

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

### API Usage Compliance

- Google Books API usage complies with [Google API Terms of Service](https://developers.google.com/terms)
- Open Library API usage complies with [Open Library Terms of Service](https://openlibrary.org/terms)

## 👥 Contributors

- [Biodun] - Initial work and maintenance

---

© 2025 LibraryX. All rights reserved.
