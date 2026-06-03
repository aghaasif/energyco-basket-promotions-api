using EnergyCo.Application.Interfaces;
using EnergyCo.Application.Services.BasketPromotions.Calculators;
using EnergyCo.Domain.BasketPromotions;

namespace EnergyCo.Tests;

public sealed class DiscountCalculatorTests
{
    [Fact]
    public void Calculate_applies_best_product_discounts_and_returns_line_discounts()
    {
        var calculator = new DiscountCalculator();
        var basketItems = new[]
        {
            new BasketItem("PRD01", 10m, 1),
            new BasketItem("PRD02", 5m, 2)
        };
        var productDiscounts = new[]
        {
            new ProductDiscountPromotion("PRD01", "DP002", "Better Promo", 30m, DateTime.UtcNow.AddDays(1)),
            new ProductDiscountPromotion("PRD01", "DP003", "Worse Promo", 10m, DateTime.UtcNow.AddDays(1)),
            new ProductDiscountPromotion("PRD02", "DP001", "Fuel Promo", 20m, DateTime.UtcNow.AddDays(1))
        };

        var result = calculator.Calculate(basketItems, productDiscounts);

        Assert.Equal(5m, result.DiscountAmount);
        Assert.Equal(20m, result.EligibleAmount);
        Assert.Equal(2, result.LineDiscounts.Count);
        Assert.Equal("PRD01", result.LineDiscounts[0].ProductId);
        Assert.Equal("DP002", result.LineDiscounts[0].PromotionId);
        Assert.Equal(10m, result.LineDiscounts[0].LineTotal);
        Assert.Equal(3m, result.LineDiscounts[0].DiscountAmount);
        Assert.Equal(7m, result.LineDiscounts[0].DiscountedLineTotal);
        Assert.Equal("PRD02", result.LineDiscounts[1].ProductId);
        Assert.Equal("DP001", result.LineDiscounts[1].PromotionId);
        Assert.Equal(10m, result.LineDiscounts[1].LineTotal);
        Assert.Equal(2m, result.LineDiscounts[1].DiscountAmount);
        Assert.Equal(8m, result.LineDiscounts[1].DiscountedLineTotal);
    }
}
