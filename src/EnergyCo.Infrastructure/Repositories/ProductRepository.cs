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
    private const string ProductCatalogueCacheKey = "products:catalogue";
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

        var productCatalogue = await cache.GetOrCreateAsync(
            ProductCatalogueCacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;

                var products = await dbContext.Products
                    .AsNoTracking()
                    .ToArrayAsync(cancellationToken);

                return products.ToDictionary(
                    product => product.ProductId,
                    StringComparer.OrdinalIgnoreCase);
            }) ?? new Dictionary<string, Product>(StringComparer.OrdinalIgnoreCase);

        return normalizedProductIds
            .Where(productCatalogue.ContainsKey)
            .Select(productId => productCatalogue[productId])
            .ToArray();
    }
}
