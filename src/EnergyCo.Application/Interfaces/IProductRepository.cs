using EnergyCo.Domain.Products;

namespace EnergyCo.Application.Interfaces;

public interface IProductRepository
{
    Task<IReadOnlyCollection<Product>> GetByIdsAsync(
        IReadOnlyCollection<string> productIds,
        CancellationToken cancellationToken);
}
