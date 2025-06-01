using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LibraryX.Data;
using LibraryX.Models;
using LibraryX.Services; // Add this for BookService
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
{
    // Allow login with username or email
    options.SignIn.RequireConfirmedAccount = false; 
    options.SignIn.RequireConfirmedEmail = false; 
    options.User.RequireUniqueEmail = true;
    
    // Password settings
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
    
    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
})
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configure cookie policy options
builder.Services.ConfigureApplicationCookie(options => 
{
    // Cookie settings
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.SlidingExpiration = true;
    
    // Enhanced cookie settings for cross-platform compatibility
    options.Cookie.SameSite = SameSiteMode.Lax;
    
    // Additional settings for mobile compatibility
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; 
    
    // Custom handling for sign-in failures
    options.Events.OnRedirectToLogin = context =>
    {
        // For API requests, return 401 instead of redirecting
        if (context.Request.Path.StartsWithSegments("/api") || context.Request.Headers["Accept"].ToString().Contains("application/json"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    
    // Custom handling for authentication failures to provide better error messages
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api") || context.Request.Headers["Accept"].ToString().Contains("application/json"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
        
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<IBookService, BookService>();
builder.Services.AddScoped<IBookService, BookService>();
// Register OpenLibrary service
builder.Services.AddHttpClient<IOpenLibraryService, OpenLibraryService>();
builder.Services.AddScoped<IOpenLibraryService, OpenLibraryService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Serve static files from wwwroot
app.UseRouting();

app.UseAuthentication(); // Add this to ensure authentication is configured
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
