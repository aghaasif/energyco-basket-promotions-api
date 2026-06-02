using EnergyCo.Application.Interfaces;
using EnergyCo.Domain.Products;
using EnergyCo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EnergyCo.Infrastructure.Repositories;

public sealed class ProductRepository(
    EnergyCoDbContext dbContext) : IProductRepository
{
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

        return await dbContext.Products
            .AsNoTracking()
            .Where(product => normalizedProductIds.Contains(product.ProductId))
            .ToArrayAsync(cancellationToken);
    }
}
