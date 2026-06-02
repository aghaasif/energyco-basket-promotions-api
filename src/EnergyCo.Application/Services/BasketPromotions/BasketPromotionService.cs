using EnergyCo.Application.Interfaces;
using EnergyCo.Application.Services.BasketPromotions.Models;
using EnergyCo.Domain.BasketPromotions;
using EnergyCo.Domain.Products;
using EnergyCo.Domain.Promotions;

namespace EnergyCo.Application.Services.BasketPromotions;

public sealed class BasketPromotionService(
    IProductRepository productRepository,
    IPointsPromotionRepository pointsPromotionRepository,
    IDiscountPromotionRepository discountPromotionRepository) : IBasketPromotionService
{
    private static readonly AppliedDiscount NoDiscount = new(null, null, 0m, 0m, 0m);

    public async Task<BasketPromotionResult> CalculateAsync(
        BasketPromotionCommand command,
        CancellationToken cancellationToken)
    {
        Validate(command);

        var basketItems = command.Basket
            .Select(item => new BasketItem(
                NormalizeProductId(item.ProductId),
                item.UnitPrice,
                item.Quantity))
            .ToArray();

        var products = await LoadProductsAsync(basketItems, cancellationToken);
        var totalAmount = BasketPromotionCalculation.RoundMoney(basketItems.Sum(item => item.LineTotal));

        var activeDiscountPromotions = await discountPromotionRepository.GetActiveAsync(
            command.TransactionDateUtc,
            cancellationToken);

        var discountOutcome = await CalculateBestDiscountAsync(
            basketItems,
            activeDiscountPromotions,
            cancellationToken);

        var activePointsPromotions = await pointsPromotionRepository.GetActiveAsync(
            command.TransactionDateUtc,
            cancellationToken);

        var points = CalculateBestPoints(
            basketItems,
            products,
            activePointsPromotions,
            discountOutcome.LineDiscounts);

        var grandTotal = BasketPromotionCalculation.RoundMoney(totalAmount - discountOutcome.Discount.Amount);

        return new BasketPromotionResult(
            command.CustomerId,
            command.LoyaltyCard.Trim(),
            command.TransactionDateUtc,
            totalAmount,
            discountOutcome.Discount,
            grandTotal,
            points);
    }

    private static void Validate(BasketPromotionCommand command)
    {
        if (command.CustomerId == Guid.Empty)
        {
            throw new BasketPromotionException("CustomerId is required.");
        }

        if (string.IsNullOrWhiteSpace(command.LoyaltyCard))
        {
            throw new BasketPromotionException("LoyaltyCard is required.");
        }

        if (command.Basket.Count == 0)
        {
            throw new BasketPromotionException("Basket must contain at least one item.");
        }

        foreach (var item in command.Basket)
        {
            if (string.IsNullOrWhiteSpace(item.ProductId))
            {
                throw new BasketPromotionException("Basket item ProductId is required.");
            }

            if (item.UnitPrice <= 0)
            {
                throw new BasketPromotionException($"UnitPrice for product '{item.ProductId}' must be greater than zero.");
            }

            if (item.Quantity <= 0)
            {
                throw new BasketPromotionException($"Quantity for product '{item.ProductId}' must be greater than zero.");
            }
        }
    }

    private async Task<IReadOnlyDictionary<string, Product>> LoadProductsAsync(
        IReadOnlyCollection<BasketItem> basketItems,
        CancellationToken cancellationToken)
    {
        var requestedProductIds = basketItems
            .Select(item => item.ProductId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var products = await productRepository.GetByIdsAsync(requestedProductIds, cancellationToken);
        var productMap = products.ToDictionary(product => product.ProductId, StringComparer.OrdinalIgnoreCase);

        var missingProductIds = requestedProductIds
            .Where(productId => !productMap.ContainsKey(productId))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (missingProductIds.Length > 0)
        {
            throw new BasketPromotionException($"Unknown product id(s): {string.Join(", ", missingProductIds)}.");
        }

        return productMap;
    }

    private async Task<DiscountOutcome> CalculateBestDiscountAsync(
        IReadOnlyCollection<BasketItem> basketItems,
        IReadOnlyCollection<DiscountPromotion> activePromotions,
        CancellationToken cancellationToken)
    {
        if (activePromotions.Count == 0)
        {
            return new DiscountOutcome(NoDiscount, EmptyLineDiscounts(basketItems));
        }

        var promotionIds = activePromotions
            .Select(promotion => promotion.DiscountPromotionId)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var productMappings = await discountPromotionRepository.GetEligibleProductIdsAsync(
            promotionIds,
            cancellationToken);

        var candidates = activePromotions
            .Select(promotion => CalculateDiscountCandidate(basketItems, promotion, productMappings))
            .Where(candidate => candidate.Discount.Amount > 0)
            .OrderByDescending(candidate => candidate.Discount.Amount)
            .ThenBy(candidate => candidate.Promotion!.EndDateUtc)
            .ThenBy(candidate => candidate.Promotion!.DiscountPromotionId, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return candidates.FirstOrDefault() ?? new DiscountOutcome(NoDiscount, EmptyLineDiscounts(basketItems));
    }

    private static DiscountOutcome CalculateDiscountCandidate(
        IReadOnlyCollection<BasketItem> basketItems,
        DiscountPromotion promotion,
        IReadOnlyDictionary<string, IReadOnlyCollection<string>> productMappings)
    {
        if (!productMappings.TryGetValue(promotion.DiscountPromotionId, out var eligibleProductIds) ||
            eligibleProductIds.Count == 0)
        {
            return new DiscountOutcome(NoDiscount, EmptyLineDiscounts(basketItems), promotion);
        }

        var eligibleProducts = eligibleProductIds.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var lineDiscounts = new List<decimal>(basketItems.Count);
        var eligibleAmount = 0m;
        var discountAmount = 0m;

        foreach (var item in basketItems)
        {
            var lineDiscount = 0m;

            if (eligibleProducts.Contains(item.ProductId))
            {
                eligibleAmount += item.LineTotal;
                lineDiscount = BasketPromotionCalculation.RoundMoney(item.LineTotal * promotion.DiscountPercent / 100m);
                discountAmount += lineDiscount;
            }

            lineDiscounts.Add(lineDiscount);
        }

        discountAmount = BasketPromotionCalculation.RoundMoney(discountAmount);

        return new DiscountOutcome(
            new AppliedDiscount(
                promotion.DiscountPromotionId,
                promotion.Name,
                promotion.DiscountPercent,
                BasketPromotionCalculation.RoundMoney(eligibleAmount),
                discountAmount),
            lineDiscounts,
            promotion);
    }

    private static EarnedPoints CalculateBestPoints(
        IReadOnlyList<BasketItem> basketItems,
        IReadOnlyDictionary<string, Product> products,
        IReadOnlyCollection<PointsPromotion> activePromotions,
        IReadOnlyList<decimal> lineDiscounts)
    {
        if (activePromotions.Count == 0)
        {
            return new EarnedPoints(null, null, 0m, 0, PointsCalculationBasis.PreDiscount, 0);
        }

        return activePromotions
            .Select(promotion => CalculatePointsCandidate(basketItems, products, promotion, lineDiscounts))
            .OrderByDescending(points => points.Points)
            .ThenBy(points => activePromotions.Single(promotion => promotion.PointsPromotionId == points.PromotionId).EndDateUtc)
            .ThenBy(points => points.PromotionId, StringComparer.OrdinalIgnoreCase)
            .First();
    }

    private static EarnedPoints CalculatePointsCandidate(
        IReadOnlyList<BasketItem> basketItems,
        IReadOnlyDictionary<string, Product> products,
        PointsPromotion promotion,
        IReadOnlyList<decimal> lineDiscounts)
    {
        var qualifyingAmount = 0m;

        for (var index = 0; index < basketItems.Count; index++)
        {
            var item = basketItems[index];
            var product = products[item.ProductId];
            if (promotion.Category is not null && promotion.Category != product.Category)
            {
                continue;
            }

            var lineAmount = promotion.CalculationBasis == PointsCalculationBasis.PostDiscount
                ? item.LineTotal - lineDiscounts[index]
                : item.LineTotal;

            qualifyingAmount += lineAmount;
        }

        qualifyingAmount = BasketPromotionCalculation.RoundMoney(qualifyingAmount);
        var points = BasketPromotionCalculation.WholeDollars(qualifyingAmount) * promotion.PointsPerDollar;

        return new EarnedPoints(
            promotion.PointsPromotionId,
            promotion.Name,
            qualifyingAmount,
            promotion.PointsPerDollar,
            promotion.CalculationBasis,
            points);
    }

    private static IReadOnlyList<decimal> EmptyLineDiscounts(
        IReadOnlyCollection<BasketItem> basketItems) =>
        Enumerable.Repeat(0m, basketItems.Count).ToArray();

    private static string NormalizeProductId(string productId) =>
        productId.Trim().ToUpperInvariant();

    private sealed record DiscountOutcome(
        AppliedDiscount Discount,
        IReadOnlyList<decimal> LineDiscounts,
        DiscountPromotion? Promotion = null);
}
