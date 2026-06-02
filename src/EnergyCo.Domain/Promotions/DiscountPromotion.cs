namespace EnergyCo.Domain.Promotions;

public sealed class DiscountPromotion
{
    public required string DiscountPromotionId { get; init; }

    public required string Name { get; init; }

    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }

    public decimal DiscountPercent { get; init; }

    public bool IsActiveOn(DateOnly transactionDate) =>
        StartDate <= transactionDate && transactionDate <= EndDate;
}
