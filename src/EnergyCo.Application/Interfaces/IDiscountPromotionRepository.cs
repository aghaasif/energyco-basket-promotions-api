namespace EnergyCo.Application.Interfaces;

public interface IDiscountPromotionRepository
{
    Task<IReadOnlyCollection<ProductDiscountPromotion>> GetBestActiveProductDiscountsAsync(
        DateTime transactionDateUtc,
        CancellationToken cancellationToken);
}

public sealed record ProductDiscountPromotion(
    string ProductId,
    string DiscountPromotionId,
    string PromotionName,
    decimal DiscountPercent,
    DateTime EndDateUtc);
