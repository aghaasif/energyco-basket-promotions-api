using EnergyCo.Application.Interfaces;
using EnergyCo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EnergyCo.Infrastructure.Repositories;

public sealed class DiscountPromotionRepository(
    EnergyCoDbContext dbContext,
    IMemoryCache cache) : IDiscountPromotionRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<IReadOnlyCollection<ProductDiscountPromotion>> GetBestActiveProductDiscountsAsync(
        DateTime transactionDateUtc,
        CancellationToken cancellationToken)
    {
        var utcDayStart = transactionDateUtc.Date;
        var utcDayEnd = utcDayStart.AddDays(1);
        var cacheKey = $"discount-promotions:products:{utcDayStart:yyyyMMdd}";

        var dayDiscounts = await cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                var rows = await (
                    from promotion in dbContext.DiscountPromotions.AsNoTracking()
                    join mapping in dbContext.DiscountPromotionProducts.AsNoTracking()
                        on promotion.DiscountPromotionId equals mapping.DiscountPromotionId
                    where promotion.StartDateUtc < utcDayEnd && utcDayStart < promotion.EndDateUtc
                    select new ProductDiscountPromotionRow(
                        mapping.ProductId,
                        promotion.DiscountPromotionId,
                        promotion.Name,
                        promotion.DiscountPercent,
                        promotion.StartDateUtc,
                        promotion.EndDateUtc))
                    .ToArrayAsync(cancellationToken);

                return rows
                    .GroupBy(row => row.ProductId, StringComparer.OrdinalIgnoreCase)
                    .Select(group => group
                        .OrderByDescending(row => row.DiscountPercent)
                        .ThenBy(row => row.EndDateUtc)
                        .ThenBy(row => row.DiscountPromotionId, StringComparer.OrdinalIgnoreCase)
                        .First())
                    .ToArray();
            }) ?? [];

        return dayDiscounts
            .Where(discount => discount.StartDateUtc <= transactionDateUtc && transactionDateUtc < discount.EndDateUtc)
            .Select(discount => new ProductDiscountPromotion(
                discount.ProductId,
                discount.DiscountPromotionId,
                discount.PromotionName,
                discount.DiscountPercent,
                discount.EndDateUtc))
            .ToArray();
    }

    private sealed record ProductDiscountPromotionRow(
        string ProductId,
        string DiscountPromotionId,
        string PromotionName,
        decimal DiscountPercent,
        DateTime StartDateUtc,
        DateTime EndDateUtc);
}
