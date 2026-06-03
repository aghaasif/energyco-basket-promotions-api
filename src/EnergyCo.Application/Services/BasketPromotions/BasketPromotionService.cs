using EnergyCo.Application.Interfaces;
using EnergyCo.Application.Services.BasketPromotions.Calculators;
using EnergyCo.Application.Services.BasketPromotions.Models;
using EnergyCo.Domain.BasketPromotions;
using EnergyCo.Domain.Products;

namespace EnergyCo.Application.Services.BasketPromotions;

public sealed class BasketPromotionService(
    IProductRepository productRepository,
    IPointsPromotionRepository pointsPromotionRepository,
    IDiscountPromotionRepository discountPromotionRepository,
    DiscountCalculator discountCalculator,
    PointsCalculator pointsCalculator) : IBasketPromotionService
{
    public async Task<BasketPromotionResult> CalculateAsync(
        BasketPromotionCommand command,
        CancellationToken cancellationToken)
    {
        var basketItems = command.Basket
            .Select(item => new BasketItem(
                item.ProductId.Trim().ToUpperInvariant(),
                item.UnitPrice,
                item.Quantity))
            .ToArray();

        // Load products to ensure they exist and to get necessary details (e.g. Category) for promotion calculations.
        var products = await LoadProductsAsync(basketItems, cancellationToken);
        var totalAmount = basketItems.Sum(item => item.LineTotal).RoundMoney();

        // Load 'best' active discounts for the transaction date i.e. if multiple discounts apply to
        // the same product(s), only the one that gives the maximum discount will be returned.
        var activeProductDiscounts = await discountPromotionRepository.GetBestActiveProductDiscountsAsync(
            command.TransactionDateUtc,
            cancellationToken);

        var discountOutcome = discountCalculator.Calculate(
            basketItems,
            activeProductDiscounts);


        // Load active points promotions for the transaction date.
        var activePointsPromotions = await pointsPromotionRepository.GetActiveAsync(
            command.TransactionDateUtc,
            cancellationToken);

        // Calculate points based on the basket items by choosing the promotion candidates that give maximum
        // points per line. Points calculation also considers the discount outcome because based on promotion classification,
        // points promotions may apply on either pre-discount or post-discount spend.
        var points = pointsCalculator.Calculate(
            basketItems,
            products,
            activePointsPromotions,
            discountOutcome.LineDiscounts); // Points need the discount outcome because
                                            // some promotions calculate from post-discount spend.

        var grandTotal = (totalAmount - discountOutcome.DiscountAmount).RoundMoney();

        return new BasketPromotionResult(
            command.CustomerId,
            command.LoyaltyCard.Trim(),
            command.TransactionDateUtc,
            totalAmount,
            new AppliedDiscount(discountOutcome.EligibleAmount, discountOutcome.DiscountAmount),
            grandTotal,
            points);
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
}
