using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LibraryX.Models
{
    public class BookSearchResult
    {
        public int TotalItems { get; set; }
        public List<Book> Items { get; set; } = new List<Book>();
    }    public class Book
    {
        public string? Id { get; set; }
        public VolumeInfo? VolumeInfo { get; set; }
        public SaleInfo? SaleInfo { get; set; }
        public AccessInfo? AccessInfo { get; set; }
        [JsonIgnore]
        public bool IsAvailable => !string.IsNullOrEmpty(AccessInfo?.WebReaderLink) ||
                                 SaleInfo?.Saleability == "FOR_SALE" ||
                                 AccessInfo?.Epub?.IsAvailable == true ||
                                 AccessInfo?.Pdf?.IsAvailable == true;
    }    public class VolumeInfo
    {
        public string? Title { get; set; }
        public List<string>? Authors { get; set; }
        public string? Publisher { get; set; }
        public string? PublishedDate { get; set; }
        public string? Description { get; set; }
        public int? PageCount { get; set; }
        public List<string>? Categories { get; set; }
        public double? AverageRating { get; set; }
        public int? RatingsCount { get; set; }
        public ImageLinks? ImageLinks { get; set; }
        public string? Language { get; set; }
        public string? PreviewLink { get; set; }
        public string? InfoLink { get; set; }
        public string? CanonicalVolumeLink { get; set; }
        public IndustryIdentifiers[]? IndustryIdentifiers { get; set; }
    }

    public class IndustryIdentifiers
    {
        public string? Type { get; set; }
        public string? Identifier { get; set; }
    }

    public class ImageLinks
    {
        public string? SmallThumbnail { get; set; }
        public string? Thumbnail { get; set; }
        public string? Small { get; set; }
        public string? Medium { get; set; }
        public string? Large { get; set; }
        public string? ExtraLarge { get; set; }

        [JsonIgnore]
        public string BestAvailableImage => ExtraLarge ?? Large ?? Medium ?? Small ?? Thumbnail ?? SmallThumbnail ?? "";
    }    public class SaleInfo
    {
        public string? Country { get; set; }
        public string? Saleability { get; set; }
        public bool IsEbook { get; set; }
    }

    public class AccessInfo
    {
        public string? Country { get; set; }
        public string? ViewAbility { get; set; }
        public bool Embeddable { get; set; }
        public bool PublicDomain { get; set; }
        public string? TextToSpeechPermission { get; set; }
        public Epub? Epub { get; set; }
        public Pdf? Pdf { get; set; }
        public string? WebReaderLink { get; set; }
        public string? AccessViewStatus { get; set; }
        public bool QuoteSharingAllowed { get; set; }
    }

    public class Epub
    {
        public bool IsAvailable { get; set; }
        public string? AcsTokenLink { get; set; }
    }

    public class Pdf
    {
        public bool IsAvailable { get; set; }
        public string? AcsTokenLink { get; set; }
    }
}