using System.ComponentModel.DataAnnotations;

namespace MetalPrice.Api.Options;

public sealed class MetalpriceApiOptions
{
    public const string SectionName = "MetalpriceApi";

    /// <summary>
    /// MetalpriceAPI key. Prefer setting via environment variable METALPRICE_API_KEY.
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// Base URL for MetalpriceAPI (no trailing slash).
    /// Example: https://api.metalpriceapi.com/v1
    /// </summary>
    [Required]
    public string BaseUrl { get; init; } = "https://api.metalpriceapi.com/v1";

    /// <summary>
    /// Default base currency used for pricing (e.g. USD).
    /// </summary>
    [Required]
    [MinLength(3)]
    public string BaseCurrency { get; init; } = "USD";

    /// <summary>
    /// Cache duration in seconds for the latest prices.
    /// </summary>
    [Range(1, 3600)]
    public int CacheSeconds { get; init; } = 10;
}
