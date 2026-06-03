using EnergyCo.Application.Interfaces;
using EnergyCo.Application.Services.BasketPromotions.Models;
using EnergyCo.Domain.BasketPromotions;

namespace EnergyCo.Application.Services.BasketPromotions.Calculators;

/// <summary>
/// Calculator for product discount promotions. It picks the best discount per product and applies it to the relevant basket items.
/// </summary>
public sealed class DiscountCalculator
{
    public DiscountCalculationResult Calculate(
        IReadOnlyCollection<BasketItem> basketItems,
        IReadOnlyCollection<ProductDiscountPromotion> activeProductDiscounts)
    {
        if (activeProductDiscounts.Count == 0)
        {
            return new DiscountCalculationResult(0m, 0m, EmptyLineDiscounts(basketItems));
        }

        // Pick the best discount per product; promotion id is only a deterministic tie-breaker.
        var discountByProductId = activeProductDiscounts
            .GroupBy(discount => discount.ProductId, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(discount => discount.DiscountPercent)
                    .ThenBy(discount => discount.DiscountPromotionId, StringComparer.OrdinalIgnoreCase)
                    .First(),
                StringComparer.OrdinalIgnoreCase);

        var lineDiscounts = new List<LineDiscount>(basketItems.Count);
        var eligibleAmount = 0m;
        var discountAmount = 0m;

        // Keep discounts at line level so post-discount points can use the correct amount.
        foreach (var item in basketItems)
        {
            ProductDiscountPromotion? appliedPromotion = null;
            var lineDiscountAmount = 0m;

            if (discountByProductId.TryGetValue(item.ProductId, out var discount))
            {
                appliedPromotion = discount;
                eligibleAmount += item.LineTotal;
                lineDiscountAmount = (item.LineTotal * discount.DiscountPercent / 100m).RoundMoney();
                discountAmount += lineDiscountAmount;
            }

            lineDiscounts.Add(new LineDiscount(
                item.ProductId,
                item.LineTotal.RoundMoney(),
                appliedPromotion?.DiscountPromotionId,
                appliedPromotion?.PromotionName,
                appliedPromotion?.DiscountPercent ?? 0m,
                lineDiscountAmount,
                (item.LineTotal - lineDiscountAmount).RoundMoney()));
        }

        discountAmount = discountAmount.RoundMoney();

        return new DiscountCalculationResult(
            eligibleAmount.RoundMoney(),
            discountAmount,
            lineDiscounts);
    }

    private static IReadOnlyList<LineDiscount> EmptyLineDiscounts(
        IReadOnlyCollection<BasketItem> basketItems) =>
        basketItems
            .Select(item => new LineDiscount(
                item.ProductId,
                item.LineTotal.RoundMoney(),
                null,
                null,
                0m,
                0m,
                item.LineTotal.RoundMoney()))
            .ToArray();
}
