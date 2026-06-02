using EnergyCo.Application.Interfaces;
using EnergyCo.Domain.Promotions;
using EnergyCo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EnergyCo.Infrastructure.Repositories;

public sealed class PromotionRepository(
    EnergyCoDbContext dbContext,
    IMemoryCache cache) : IPromotionRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<IReadOnlyCollection<PointsPromotion>> GetActivePointsPromotionsAsync(
        DateOnly transactionDate,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"points-promotions:{transactionDate:yyyyMMdd}";

        return await cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                return await dbContext.PointsPromotions
                    .AsNoTracking()
                    .Where(promotion => promotion.StartDate <= transactionDate && transactionDate <= promotion.EndDate)
                    .ToArrayAsync(cancellationToken);
            }) ?? [];
    }

    public async Task<IReadOnlyCollection<DiscountPromotion>> GetActiveDiscountPromotionsAsync(
        DateOnly transactionDate,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"discount-promotions:{transactionDate:yyyyMMdd}";

        return await cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                return await dbContext.DiscountPromotions
                    .AsNoTracking()
                    .Where(promotion => promotion.StartDate <= transactionDate && transactionDate <= promotion.EndDate)
                    .ToArrayAsync(cancellationToken);
            }) ?? [];
    }

    public async Task<IReadOnlyDictionary<string, IReadOnlyCollection<string>>> GetDiscountPromotionProductIdsAsync(
        IReadOnlyCollection<string> discountPromotionIds,
        CancellationToken cancellationToken)
    {
        var normalizedPromotionIds = discountPromotionIds
            .Select(promotionId => promotionId.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalizedPromotionIds.Length == 0)
        {
            return new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase);
        }

        var cacheKey = $"discount-promotion-products:{string.Join("|", normalizedPromotionIds)}";

        return await cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                var mappings = await dbContext.DiscountPromotionProducts
                    .AsNoTracking()
                    .Where(mapping => normalizedPromotionIds.Contains(mapping.DiscountPromotionId))
                    .ToArrayAsync(cancellationToken);

                return mappings
                    .GroupBy(mapping => mapping.DiscountPromotionId, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        group => group.Key,
                        group => (IReadOnlyCollection<string>)group
                            .Select(mapping => mapping.ProductId)
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToArray(),
                        StringComparer.OrdinalIgnoreCase);
            }) ?? new Dictionary<string, IReadOnlyCollection<string>>(StringComparer.OrdinalIgnoreCase);
    }
}
