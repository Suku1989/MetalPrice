var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMemoryCache();

builder.Services.AddOptions<MetalPrice.Api.Options.MetalpriceApiOptions>()
    .Bind(builder.Configuration.GetSection(MetalPrice.Api.Options.MetalpriceApiOptions.SectionName))
    .ValidateDataAnnotations();

builder.Services.AddHttpClient<MetalPrice.Api.Services.MetalpriceApiClient>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ui", policy =>
    {
        var configuredOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>();

        var origins = (configuredOrigins ?? [])
            .Where(o => !string.IsNullOrWhiteSpace(o))
            .Select(o => o.Trim())
            .ToArray();

        // Only allow localhost by default during development.
        if (origins.Length == 0 && builder.Environment.IsDevelopment())
        {
            origins = ["http://localhost:5173"]; // Vite dev server
        }

        if (origins.Length > 0)
        {
            policy.WithOrigins(origins);
        }

        policy
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("ui");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapGet("/api/metals/latest", async (
        MetalPrice.Api.Services.MetalpriceApiClient metalsApi,
        string? baseCurrency,
        CancellationToken cancellationToken) =>
    {
        try
        {
            var result = await metalsApi.GetGoldAndSilverPricesAsync(baseCurrency, cancellationToken);
            return Results.Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(title: "Configuration error", detail: ex.Message, statusCode: StatusCodes.Status500InternalServerError);
        }
        catch (HttpRequestException ex)
        {
            return Results.Problem(title: "Upstream API error", detail: ex.Message, statusCode: StatusCodes.Status502BadGateway);
        }
    })
    .WithName("GetLatestMetals");

app.Run();

