using EnergyCo.Application.Services.BasketPromotions.Calculators;
using EnergyCo.Application.Services.BasketPromotions.Models;
using EnergyCo.Domain.BasketPromotions;
using EnergyCo.Domain.Products;
using EnergyCo.Domain.Promotions;

namespace EnergyCo.Tests;

public sealed class PointsCalculatorTests
{
    [Fact]
    public void Calculate_selects_best_points_promotion_per_line()
    {
        var calculator = new PointsCalculator();
        var basketItems = new[]
        {
            new BasketItem("PRD01", 1.20m, 10),
            new BasketItem("PRD04", 2.30m, 2)
        };
        var products = new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase)
        {
            ["PRD01"] = new()
            {
                ProductId = "PRD01",
                Name = "Vortex 95",
                Category = ProductCategory.Fuel,
                UnitPrice = 1.20m
            },
            ["PRD04"] = new()
            {
                ProductId = "PRD04",
                Name = "Coffee",
                Category = ProductCategory.Shop,
                UnitPrice = 2.30m
            }
        };
        var promotions = new[]
        {
            PointsPromotion("PP000", null, 1),
            PointsPromotion("PP002", ProductCategory.Fuel, 3),
            PointsPromotion("PP003", ProductCategory.Shop, 4)
        };

        var result = calculator.Calculate(
            basketItems,
            products,
            promotions,
            [LineDiscount("PRD01", 12m), LineDiscount("PRD04", 4.60m)]);

        Assert.Equal(52, result.Points);
        Assert.Equal(16.60m, result.QualifyingAmount);
        Assert.Equal("PP002", result.LinePoints[0].PromotionId);
        Assert.Equal(36, result.LinePoints[0].Points);
        Assert.Equal("PP003", result.LinePoints[1].PromotionId);
        Assert.Equal(16, result.LinePoints[1].Points);
    }

    [Fact]
    public void Calculate_uses_post_discount_amount_when_configured()
    {
        var calculator = new PointsCalculator();
        var basketItems = new[]
        {
            new BasketItem("PRD01", 5m, 1)
        };
        var products = new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase)
        {
            ["PRD01"] = new()
            {
                ProductId = "PRD01",
                Name = "Vortex 95",
                Category = ProductCategory.Fuel,
                UnitPrice = 5m
            }
        };
        var promotions = new[]
        {
            PointsPromotion("PP001", null, 2, PointsCalculationBasis.PostDiscount)
        };

        var result = calculator.Calculate(basketItems, products, promotions, [LineDiscount("PRD01", 5m, 1m)]);

        Assert.Equal(4m, result.QualifyingAmount);
        Assert.Equal(8, result.Points);
        Assert.Equal("PP001", result.LinePoints[0].PromotionId);
        Assert.Equal(4m, result.LinePoints[0].QualifyingAmount);
    }

    private static PointsPromotion PointsPromotion(
        string promotionId,
        ProductCategory? category,
        int pointsPerDollar,
        PointsCalculationBasis basis = PointsCalculationBasis.PostDiscount) =>
        new()
        {
            PointsPromotionId = promotionId,
            Name = promotionId,
            StartDateUtc = DateTime.MinValue,
            EndDateUtc = DateTime.MaxValue,
            Category = category,
            PointsPerDollar = pointsPerDollar,
            CalculationBasis = basis
        };

    private static LineDiscount LineDiscount(
        string productId,
        decimal lineTotal,
        decimal discountAmount = 0m) =>
        new(
            productId,
            lineTotal,
            null,
            null,
            0m,
            discountAmount,
            lineTotal - discountAmount);
}
