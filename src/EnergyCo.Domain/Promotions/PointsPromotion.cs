using EnergyCo.Domain.Products;

namespace EnergyCo.Domain.Promotions;

public sealed class PointsPromotion
{
    public required string PointsPromotionId { get; init; }

    public required string Name { get; init; }

    public DateOnly StartDate { get; init; }

    public DateOnly EndDate { get; init; }

    public ProductCategory? Category { get; init; }

    public int PointsPerDollar { get; init; }

    public PointsCalculationBasis CalculationBasis { get; init; } = PointsCalculationBasis.PreDiscount;

    public bool IsActiveOn(DateOnly transactionDate) =>
        StartDate <= transactionDate && transactionDate <= EndDate;
}
