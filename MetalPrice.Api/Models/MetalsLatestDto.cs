namespace MetalPrice.Api.Models;

public sealed record MetalsLatestDto(
    string BaseCurrency,
    DateTimeOffset TimestampUtc,
    decimal GoldPerOunce,
    decimal SilverPerOunce,
    string Unit);
