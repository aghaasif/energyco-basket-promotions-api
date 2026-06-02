using EnergyCo.Domain.Promotions;

namespace EnergyCo.Application.Interfaces;

public interface IPointsPromotionRepository
{
    Task<IReadOnlyCollection<PointsPromotion>> GetActiveAsync(
        DateTime transactionDateUtc,
        CancellationToken cancellationToken);
}
