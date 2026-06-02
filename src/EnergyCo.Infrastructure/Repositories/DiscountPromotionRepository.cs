using EnergyCo.Application.Interfaces;
using EnergyCo.Domain.Promotions;
using EnergyCo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EnergyCo.Infrastructure.Repositories;

public sealed class DiscountPromotionRepository(
    EnergyCoDbContext dbContext,
    IMemoryCache cache) : IDiscountPromotionRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<IReadOnlyCollection<DiscountPromotion>> GetActiveAsync(
        DateTime transactionDateUtc,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"discount-promotions:{transactionDateUtc:yyyyMMddHHmmss}";

        return await cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                return await dbContext.DiscountPromotions
                    .AsNoTracking()
                    .Where(promotion => promotion.StartDateUtc <= transactionDateUtc && transactionDateUtc < promotion.EndDateUtc)
                    .ToArrayAsync(cancellationToken);
            }) ?? [];
    }

    public async Task<IReadOnlyDictionary<string, IReadOnlyCollection<string>>> GetEligibleProductIdsAsync(
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
