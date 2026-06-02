using System.Threading.RateLimiting;
using Asp.Versioning;
using EnergyCo.Api;
using EnergyCo.Api.Json;
using EnergyCo.Api.V1.Endpoints;
using EnergyCo.Application;
using EnergyCo.Infrastructure;
using EnergyCo.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new UtcDateTimeOffsetJsonConverter());
});
builder.Services.AddValidation();
builder.Services.AddMemoryCache();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = false;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new HeaderApiVersionReader("api-version");
    });

builder.Services.AddHealthChecks()
    .AddDbContextCheck<EnergyCoDbContext>("sqlite");

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("basket-promotions", limiter =>
    {
        limiter.PermitLimit = builder.Configuration.GetValue("RateLimiting:BasketPromotions:PermitLimit", 60);
        limiter.Window = TimeSpan.FromSeconds(builder.Configuration.GetValue("RateLimiting:BasketPromotions:WindowSeconds", 60));
        limiter.QueueLimit = 0;
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

var app = builder.Build();

await app.ApplyDatabaseMigrationsAsync();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseRateLimiter();

var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1, 0))
    .ReportApiVersions()
    .Build();

app.MapGroup("/api")
    .WithApiVersionSet(versionSet)
    .MapBasketPromotionEndpoints();

app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions { Predicate = _ => false });

app.MapHealthChecks("/health/ready");

app.Run();

public partial class Program;
