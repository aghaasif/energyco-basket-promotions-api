using EnergyCo.Application.Services.BasketPromotions.Models;
using EnergyCo.Domain.BasketPromotions;
using EnergyCo.Domain.Products;
using EnergyCo.Domain.Promotions;

namespace EnergyCo.Application.Services.BasketPromotions.Calculators;

/// <summary>
/// Calculates earned points for a basket based on active points promotions and the discount outcome (because points 
/// promotions calculation can be based on pre or post-discount spend depending on its classification).
/// </summary>
public sealed class PointsCalculator
{
    public EarnedPoints Calculate(
        IReadOnlyList<BasketItem> basketItems,
        IReadOnlyDictionary<string, Product> products,
        IReadOnlyCollection<PointsPromotion> activePromotions,
        IReadOnlyList<LineDiscount> lineDiscounts)
    {
        if (activePromotions.Count == 0)
        {
            return new EarnedPoints(0m, 0, EmptyLinePoints(basketItems));
        }

        // Points promotions are evaluated per line so different products can win different promotions.
        var linePoints = basketItems
            .Select((item, index) => CalculateBestLinePoints(item, products[item.ProductId], activePromotions, lineDiscounts[index]))
            .ToArray();

        return new EarnedPoints(
            linePoints.Sum(points => points.QualifyingAmount).RoundMoney(),
            linePoints.Sum(points => points.Points),
            linePoints);
    }

    private static LinePoints CalculateBestLinePoints(
        BasketItem item,
        Product product,
        IReadOnlyCollection<PointsPromotion> activePromotions,
        LineDiscount lineDiscount)
    {
        // Choose the best eligible promotion for this line; promotion id keeps tied results stable.
        return activePromotions
            .Where(promotion => promotion.Category is null || promotion.Category == product.Category)
            .Select(promotion => CalculateLinePoints(item, promotion, lineDiscount))
            .OrderByDescending(points => points.Points)
            .ThenBy(points => points.PromotionId, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault()
            ?? NoLinePoints(item);
    }

    private static LinePoints CalculateLinePoints(
        BasketItem item,
        PointsPromotion promotion,
        LineDiscount lineDiscount)
    {
        // Post-discount promotions use the already-calculated discounted line total.
        var qualifyingAmount = (promotion.CalculationBasis == PointsCalculationBasis.PostDiscount
            ? lineDiscount.DiscountedLineTotal
            : lineDiscount.LineTotal).RoundMoney();

        return new LinePoints(
            item.ProductId,
            promotion.PointsPromotionId,
            promotion.Name,
            qualifyingAmount,
            promotion.PointsPerDollar,
            promotion.CalculationBasis,
            qualifyingAmount.WholeDollars() * promotion.PointsPerDollar);
    }

    private static IReadOnlyList<LinePoints> EmptyLinePoints(IReadOnlyCollection<BasketItem> basketItems) =>
        basketItems.Select(NoLinePoints).ToArray();

    private static LinePoints NoLinePoints(BasketItem item) =>
        new(item.ProductId, null, null, 0m, 0, PointsCalculationBasis.PreDiscount, 0);
}
