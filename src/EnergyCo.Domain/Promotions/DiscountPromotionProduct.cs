namespace EnergyCo.Domain.Promotions;

public sealed class DiscountPromotionProduct
{
    public required string DiscountPromotionId { get; init; }

    public required string ProductId { get; init; }
}
