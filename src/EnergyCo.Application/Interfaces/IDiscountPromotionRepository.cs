using EnergyCo.Domain.Promotions;

namespace EnergyCo.Application.Interfaces;

public interface IDiscountPromotionRepository
{
    Task<IReadOnlyCollection<DiscountPromotion>> GetActiveAsync(
        DateTime transactionDateUtc,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<string, IReadOnlyCollection<string>>> GetEligibleProductIdsAsync(
        IReadOnlyCollection<string> discountPromotionIds,
        CancellationToken cancellationToken);
}
