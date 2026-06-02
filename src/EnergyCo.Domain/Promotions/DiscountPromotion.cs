namespace EnergyCo.Domain.Promotions;

public sealed class DiscountPromotion
{
    public required string DiscountPromotionId { get; init; }

    public required string Name { get; init; }

    public DateTime StartDateUtc { get; init; }

    public DateTime EndDateUtc { get; init; }

    public decimal DiscountPercent { get; init; }

    public bool IsActiveOn(DateTime transactionDateUtc) =>
        StartDateUtc <= transactionDateUtc && transactionDateUtc < EndDateUtc;
}
