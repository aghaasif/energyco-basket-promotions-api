using EnergyCo.Domain.Promotions;
using EnergyCo.Infrastructure.Persistence;
using EnergyCo.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EnergyCo.Tests;

public sealed class DiscountPromotionRepositoryTests
{
    [Fact]
    public async Task GetBestActiveProductDiscountsAsync_filters_by_transaction_time_before_selecting_best_discount()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<EnergyCoDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var dbContext = new EnergyCoDbContext(options))
        {
            await dbContext.Database.EnsureCreatedAsync();
            dbContext.DiscountPromotions.AddRange(
                new DiscountPromotion
                {
                    DiscountPromotionId = "DP001",
                    Name = "Morning Discount",
                    StartDateUtc = new DateTime(2020, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                    EndDateUtc = new DateTime(2020, 1, 15, 12, 0, 0, DateTimeKind.Utc),
                    DiscountPercent = 50m
                },
                new DiscountPromotion
                {
                    DiscountPromotionId = "DP002",
                    Name = "Afternoon Discount",
                    StartDateUtc = new DateTime(2020, 1, 15, 12, 0, 0, DateTimeKind.Utc),
                    EndDateUtc = new DateTime(2020, 1, 16, 0, 0, 0, DateTimeKind.Utc),
                    DiscountPercent = 20m
                });
            dbContext.DiscountPromotionProducts.AddRange(
                new DiscountPromotionProduct { DiscountPromotionId = "DP001", ProductId = "PRD01" },
                new DiscountPromotionProduct { DiscountPromotionId = "DP002", ProductId = "PRD01" });
            await dbContext.SaveChangesAsync();
        }

        await using var queryContext = new EnergyCoDbContext(options);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var repository = new DiscountPromotionRepository(queryContext, cache);

        var discounts = await repository.GetBestActiveProductDiscountsAsync(
            new DateTime(2020, 1, 15, 13, 0, 0, DateTimeKind.Utc),
            CancellationToken.None);

        var discount = Assert.Single(discounts);
        Assert.Equal("DP002", discount.DiscountPromotionId);
        Assert.Equal(20m, discount.DiscountPercent);
    }
}
