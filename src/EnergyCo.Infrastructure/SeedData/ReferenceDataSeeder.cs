using EnergyCo.Domain.Products;
using EnergyCo.Domain.Promotions;
using EnergyCo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnergyCo.Infrastructure.SeedData;

public static class ReferenceDataSeeder
{
    public static async Task SeedAsync(EnergyCoDbContext dbContext, CancellationToken cancellationToken = default)
    {
        await UpsertProductsAsync(dbContext, cancellationToken);
        await UpsertPointsPromotionsAsync(dbContext, cancellationToken);
        await UpsertDiscountPromotionsAsync(dbContext, cancellationToken);
        await UpsertDiscountPromotionProductsAsync(dbContext, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task UpsertProductsAsync(EnergyCoDbContext dbContext, CancellationToken cancellationToken)
    {
        var products = new[]
        {
            new Product { ProductId = "PRD01", Name = "Vortex 95", Category = ProductCategory.Fuel, UnitPrice = 1.2m },
            new Product { ProductId = "PRD02", Name = "Vortex 98", Category = ProductCategory.Fuel, UnitPrice = 1.3m },
            new Product { ProductId = "PRD03", Name = "Diesel Fuel", Category = ProductCategory.Fuel, UnitPrice = 1.1m },
            new Product { ProductId = "PRD04", Name = "Twix 55g", Category = ProductCategory.Shop, UnitPrice = 2.3m },
            new Product { ProductId = "PRD05", Name = "Mars 72g", Category = ProductCategory.Shop, UnitPrice = 5.1m },
            new Product { ProductId = "PRD06", Name = "SNICKERS 72G", Category = ProductCategory.Shop, UnitPrice = 3.4m },
            new Product { ProductId = "PRD07", Name = "Bounty 3", Category = ProductCategory.Shop, UnitPrice = 6.9m },
            new Product { ProductId = "PRD08", Name = "Snickers 50g", Category = ProductCategory.Shop, UnitPrice = 4.0m }
        };

        foreach (var product in products)
        {
            var existing = await dbContext.Products.FindAsync([product.ProductId], cancellationToken);
            if (existing is null)
            {
                dbContext.Products.Add(product);
                continue;
            }

            dbContext.Entry(existing).CurrentValues.SetValues(product);
        }
    }

    private static async Task UpsertPointsPromotionsAsync(EnergyCoDbContext dbContext, CancellationToken cancellationToken)
    {
        var promotions = new[]
        {
            new PointsPromotion
            {
                PointsPromotionId = "PP000",
                Name = "Base Points",
                StartDateUtc = DateTime.MinValue,
                EndDateUtc = DateTime.MaxValue,
                Category = null,
                PointsPerDollar = 1,
                CalculationBasis = PointsCalculationBasis.PostDiscount
            },
            new PointsPromotion
            {
                PointsPromotionId = "PP001",
                Name = "New Year Promo",
                StartDateUtc = UtcStart(2020, 1, 1),
                EndDateUtc = UtcExclusiveEnd(2020, 1, 30),
                Category = null,
                PointsPerDollar = 2,
                CalculationBasis = PointsCalculationBasis.PostDiscount
            },
            new PointsPromotion
            {
                PointsPromotionId = "PP002",
                Name = "Fuel Promo",
                StartDateUtc = UtcStart(2020, 2, 5),
                EndDateUtc = UtcExclusiveEnd(2020, 2, 15),
                Category = ProductCategory.Fuel,
                PointsPerDollar = 3,
                CalculationBasis = PointsCalculationBasis.PostDiscount
            },
            new PointsPromotion
            {
                PointsPromotionId = "PP003",
                Name = "Shop Promo",
                StartDateUtc = UtcStart(2020, 3, 1),
                EndDateUtc = UtcExclusiveEnd(2020, 3, 20),
                Category = ProductCategory.Shop,
                PointsPerDollar = 4,
                CalculationBasis = PointsCalculationBasis.PostDiscount
            }
        };

        foreach (var promotion in promotions)
        {
            var existing = await dbContext.PointsPromotions.FindAsync([promotion.PointsPromotionId], cancellationToken);
            if (existing is null)
            {
                dbContext.PointsPromotions.Add(promotion);
                continue;
            }

            dbContext.Entry(existing).CurrentValues.SetValues(promotion);
        }
    }

    private static async Task UpsertDiscountPromotionsAsync(EnergyCoDbContext dbContext, CancellationToken cancellationToken)
    {
        var promotions = new[]
        {
            new DiscountPromotion
            {
                DiscountPromotionId = "DP001",
                Name = "Fuel Discount Promo",
                StartDateUtc = UtcStart(2020, 1, 1),
                EndDateUtc = UtcExclusiveEnd(2020, 2, 15),
                DiscountPercent = 20m
            },
            new DiscountPromotion
            {
                DiscountPromotionId = "DP002",
                Name = "Happy Promo",
                StartDateUtc = UtcStart(2020, 3, 2),
                EndDateUtc = UtcExclusiveEnd(2020, 3, 20),
                DiscountPercent = 15m
            }
        };

        foreach (var promotion in promotions)
        {
            var existing = await dbContext.DiscountPromotions.FindAsync([promotion.DiscountPromotionId], cancellationToken);
            if (existing is null)
            {
                dbContext.DiscountPromotions.Add(promotion);
                continue;
            }

            dbContext.Entry(existing).CurrentValues.SetValues(promotion);
        }
    }

    private static async Task UpsertDiscountPromotionProductsAsync(EnergyCoDbContext dbContext, CancellationToken cancellationToken)
    {
        var mappings = new[]
        {
            new DiscountPromotionProduct { DiscountPromotionId = "DP001", ProductId = "PRD02" },
            new DiscountPromotionProduct { DiscountPromotionId = "DP002", ProductId = "PRD04" }
        };

        foreach (var mapping in mappings)
        {
            var existing = await dbContext.DiscountPromotionProducts.FindAsync(
                [mapping.DiscountPromotionId, mapping.ProductId],
                cancellationToken);

            if (existing is null)
            {
                dbContext.DiscountPromotionProducts.Add(mapping);
            }
        }
    }

    private static DateTime UtcStart(int year, int month, int day) =>
        new(year, month, day, 0, 0, 0, DateTimeKind.Utc);

    private static DateTime UtcExclusiveEnd(int year, int month, int day) =>
        UtcStart(year, month, day).AddDays(1);
}
