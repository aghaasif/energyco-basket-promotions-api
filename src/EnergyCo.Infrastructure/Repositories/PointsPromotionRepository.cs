using EnergyCo.Application.Interfaces;
using EnergyCo.Domain.Promotions;
using EnergyCo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EnergyCo.Infrastructure.Repositories;

public sealed class PointsPromotionRepository(
    EnergyCoDbContext dbContext,
    IMemoryCache cache) : IPointsPromotionRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<IReadOnlyCollection<PointsPromotion>> GetActiveAsync(
        DateTime transactionDateUtc,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"points-promotions:{transactionDateUtc:yyyyMMddHHmmss}";

        return await cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                return await dbContext.PointsPromotions
                    .AsNoTracking()
                    .Where(promotion => promotion.StartDateUtc <= transactionDateUtc && transactionDateUtc < promotion.EndDateUtc)
                    .ToArrayAsync(cancellationToken);
            }) ?? [];
    }
}
