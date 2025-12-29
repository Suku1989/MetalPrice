var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMemoryCache();

builder.Services.AddOptions<MetalPrice.Api.Options.MetalpriceApiOptions>()
    .Bind(builder.Configuration.GetSection(MetalPrice.Api.Options.MetalpriceApiOptions.SectionName))
    .ValidateDataAnnotations();

builder.Services.AddHttpClient<MetalPrice.Api.Services.MetalpriceApiClient>();

// Dev-only CORS for the React dev server (Vite default: http://localhost:5173)
builder.Services.AddCors(options =>
{
    options.AddPolicy("dev", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("dev");
}

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

