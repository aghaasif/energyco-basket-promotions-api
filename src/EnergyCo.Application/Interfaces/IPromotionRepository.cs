using EnergyCo.Domain.Promotions;

namespace EnergyCo.Application.Interfaces;

public interface IPromotionRepository
{
    Task<IReadOnlyCollection<PointsPromotion>> GetActivePointsPromotionsAsync(
        DateOnly transactionDate,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<DiscountPromotion>> GetActiveDiscountPromotionsAsync(
        DateOnly transactionDate,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<string, IReadOnlyCollection<string>>> GetDiscountPromotionProductIdsAsync(
        IReadOnlyCollection<string> discountPromotionIds,
        CancellationToken cancellationToken);
}
