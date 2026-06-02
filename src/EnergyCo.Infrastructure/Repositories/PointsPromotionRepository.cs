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
        var utcDayStart = transactionDateUtc.Date;
        var utcDayEnd = utcDayStart.AddDays(1);
        var cacheKey = $"points-promotions:{utcDayStart:yyyyMMdd}";

        var dayPromotions = await cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                return await dbContext.PointsPromotions
                    .AsNoTracking()
                    .Where(promotion => promotion.StartDateUtc < utcDayEnd && utcDayStart < promotion.EndDateUtc)
                    .ToArrayAsync(cancellationToken);
            }) ?? [];

        return dayPromotions
            .Where(promotion => promotion.IsActiveOn(transactionDateUtc))
            .ToArray();
    }
}
