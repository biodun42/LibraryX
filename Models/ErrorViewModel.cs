namespace LibraryX.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    
    // Additional properties for improved error handling
    public string? ErrorTitle { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SuggestedAction { get; set; }
    public bool HasCustomError => !string.IsNullOrEmpty(ErrorMessage);
}
