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

        var activeProductDiscounts = await discountPromotionRepository.GetBestActiveProductDiscountsAsync(
            command.TransactionDateUtc,
            cancellationToken);

        var discountOutcome = CalculateProductDiscounts(
            basketItems,
            activeProductDiscounts);

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

    private static DiscountOutcome CalculateProductDiscounts(
        IReadOnlyCollection<BasketItem> basketItems,
        IReadOnlyCollection<ProductDiscountPromotion> activeProductDiscounts)
    {
        if (activeProductDiscounts.Count == 0)
        {
            return new DiscountOutcome(NoDiscount, EmptyLineDiscounts(basketItems));
        }

        var discountByProductId = activeProductDiscounts.ToDictionary(
            discount => discount.ProductId,
            StringComparer.OrdinalIgnoreCase);
        var lineDiscounts = new List<decimal>(basketItems.Count);
        var eligibleAmount = 0m;
        var discountAmount = 0m;
        var appliedPromotions = new Dictionary<string, ProductDiscountPromotion>(StringComparer.OrdinalIgnoreCase);

        foreach (var item in basketItems)
        {
            var lineDiscount = 0m;

            if (discountByProductId.TryGetValue(item.ProductId, out var discount))
            {
                eligibleAmount += item.LineTotal;
                lineDiscount = BasketPromotionCalculation.RoundMoney(item.LineTotal * discount.DiscountPercent / 100m);
                discountAmount += lineDiscount;
                appliedPromotions.TryAdd(discount.DiscountPromotionId, discount);
            }

            lineDiscounts.Add(lineDiscount);
        }

        discountAmount = BasketPromotionCalculation.RoundMoney(discountAmount);

        return new DiscountOutcome(
            new AppliedDiscount(
                AppliedPromotionId(appliedPromotions.Values),
                AppliedPromotionName(appliedPromotions.Values),
                AppliedDiscountPercent(appliedPromotions.Values),
                BasketPromotionCalculation.RoundMoney(eligibleAmount),
                discountAmount),
            lineDiscounts);
    }

    private static string? AppliedPromotionId(IReadOnlyCollection<ProductDiscountPromotion> promotions) =>
        promotions.Count == 0
            ? null
            : promotions.Count == 1
                ? promotions.First().DiscountPromotionId
                : "Multiple";

    private static string? AppliedPromotionName(IReadOnlyCollection<ProductDiscountPromotion> promotions) =>
        promotions.Count == 0
            ? null
            : promotions.Count == 1
                ? promotions.First().PromotionName
                : "Multiple promotions";

    private static decimal AppliedDiscountPercent(IReadOnlyCollection<ProductDiscountPromotion> promotions) =>
        promotions.Count == 1 ? promotions.First().DiscountPercent : 0m;

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
        IReadOnlyList<decimal> LineDiscounts);
}
