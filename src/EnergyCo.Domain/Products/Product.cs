namespace EnergyCo.Domain.Products;

public sealed class Product
{
    public required string ProductId { get; init; }

    public required string Name { get; init; }

    public ProductCategory Category { get; init; }

    public decimal UnitPrice { get; init; }
}
