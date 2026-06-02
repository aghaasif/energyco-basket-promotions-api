using EnergyCo.Application.Interfaces;
using EnergyCo.Domain.Products;
using EnergyCo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace EnergyCo.Infrastructure.Repositories;

public sealed class ProductRepository(
    EnergyCoDbContext dbContext,
    IMemoryCache cache) : IProductRepository
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public async Task<IReadOnlyCollection<Product>> GetByIdsAsync(
        IReadOnlyCollection<string> productIds,
        CancellationToken cancellationToken)
    {
        var normalizedProductIds = productIds
            .Select(productId => productId.Trim().ToUpperInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalizedProductIds.Length == 0)
        {
            return [];
        }

        var cacheKey = $"products:{string.Join("|", normalizedProductIds)}";

        return await cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                return await dbContext.Products
                    .AsNoTracking()
                    .Where(product => normalizedProductIds.Contains(product.ProductId))
                    .ToArrayAsync(cancellationToken);
            }) ?? [];
    }
}
