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

    /// <summary>
    /// Gets the best active product discount promotions for the given transaction date. 
    /// If multiple promotions are active for a product, the one with the highest discount percent is returned. 
    /// Promotions are cached by day to optimize performance for transactions occurring on the same day.
    /// </summary>
    /// <param name="transactionDateUtc">Transaction date (UTC)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A collection of the best active product discount promotions for the given transaction date.</returns>
    public async Task<IReadOnlyCollection<ProductDiscountPromotion>> GetBestActiveProductDiscountsAsync(
        DateTime transactionDateUtc,
        CancellationToken cancellationToken)
    {
        var utcDayStart = transactionDateUtc.Date;
        var utcDayEnd = utcDayStart.AddDays(1);
        var cacheKey = $"discount-promotions:products:{utcDayStart:yyyyMMdd}";

        // Cache the discount promotions for the day to avoid hitting the database multiple times for transactions on the same day.
        var dayDiscountCandidates = await cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                return await (
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
            }) ?? [];


        // Filter the cached promotions to those active at the transaction time, then select the promotion that
        // gives the maximum discount for each product.
        return dayDiscountCandidates
            .Where(discount => discount.StartDateUtc <= transactionDateUtc && transactionDateUtc < discount.EndDateUtc)
            .GroupBy(discount => discount.ProductId, StringComparer.OrdinalIgnoreCase)
            .Select(group => group
                .OrderByDescending(discount => discount.DiscountPercent)
                .ThenBy(discount => discount.DiscountPromotionId, StringComparer.OrdinalIgnoreCase)
                .First())
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
