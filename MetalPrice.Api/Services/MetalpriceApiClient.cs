using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MetalPrice.Api.Models;
using MetalPrice.Api.Options;

namespace MetalPrice.Api.Services;

public sealed class MetalpriceApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly MetalpriceApiOptions _options;

    public MetalpriceApiClient(HttpClient http, IMemoryCache cache, IOptions<MetalpriceApiOptions> options)
    {
        _http = http;
        _cache = cache;
        _options = options.Value;

        _http.BaseAddress = new Uri(_options.BaseUrl.TrimEnd('/') + "/");
    }

    public async Task<MetalsLatestDto> GetGoldAndSilverPricesAsync(string? baseCurrency, CancellationToken cancellationToken)
    {
        var apiKey =
            Environment.GetEnvironmentVariable("METALPRICE_API_KEY")?.Trim()
            ?? Environment.GetEnvironmentVariable("METALS_API_KEY")?.Trim() // fallback from previous provider
            ?? _options.ApiKey?.Trim();

        if (string.IsNullOrWhiteSpace(apiKey) || string.Equals(apiKey, "REPLACE_ME", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Missing API key. Set METALPRICE_API_KEY (recommended) or configure MetalpriceApi:ApiKey in appsettings.");
        }

        var effectiveBase = string.IsNullOrWhiteSpace(baseCurrency) ? _options.BaseCurrency : baseCurrency.Trim().ToUpperInvariant();
        var cacheKey = $"metalprice-latest:{effectiveBase}";

        if (_cache.TryGetValue(cacheKey, out MetalsLatestDto? cached) && cached is not null)
        {
            return cached;
        }

        // MetalpriceAPI:  https://api.metalpriceapi.com/v1/latest?api_key=...&base=USD&currencies=XAU,XAG
        // Rates are "metal per 1 base currency" (e.g., 1 USD = 0.00053853 XAU). To get "USD per oz", invert.
        var url = $"latest?api_key={Uri.EscapeDataString(apiKey)}&base={Uri.EscapeDataString(effectiveBase)}&currencies=XAU,XAG";

        using var response = await _http.GetAsync(url, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (response.StatusCode is not HttpStatusCode.OK)
        {
            throw new HttpRequestException(
                $"MetalpriceAPI returned {(int)response.StatusCode} {response.ReasonPhrase}. Body: {body}");
        }

        var parsed = JsonSerializer.Deserialize<MetalpriceLatestResponse>(body, JsonOptions)
            ?? throw new InvalidOperationException("MetalpriceAPI response was empty or invalid JSON.");

        if (parsed.Success == false)
        {
            var errorMessage = parsed.Error?.Info ?? "Unknown MetalpriceAPI error.";
            throw new InvalidOperationException(errorMessage);
        }

        if (parsed.Rates is null
            || !parsed.Rates.TryGetValue("XAU", out var xauRate)
            || !parsed.Rates.TryGetValue("XAG", out var xagRate))
        {
            throw new InvalidOperationException("MetalpriceAPI response missing XAU/XAG rates.");
        }

        if (xauRate <= 0 || xagRate <= 0)
        {
            throw new InvalidOperationException("MetalpriceAPI returned non-positive rates.");
        }

        var goldPerOunce = 1m / xauRate;
        var silverPerOunce = 1m / xagRate;

        var timestampUtc = parsed.Timestamp.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(parsed.Timestamp.Value)
            : DateTimeOffset.UtcNow;

        var dto = new MetalsLatestDto(
            BaseCurrency: effectiveBase,
            TimestampUtc: timestampUtc,
            GoldPerOunce: decimal.Round(goldPerOunce, 2),
            SilverPerOunce: decimal.Round(silverPerOunce, 2),
            Unit: $"{effectiveBase} per oz");

        _cache.Set(cacheKey, dto, TimeSpan.FromSeconds(_options.CacheSeconds));
        return dto;
    }

    private sealed record MetalpriceLatestResponse(
        bool? Success,
        long? Timestamp,
        string? Base,
        Dictionary<string, decimal>? Rates,
        MetalpriceApiError? Error);

    private sealed record MetalpriceApiError(int? Code, string? Info);
}
