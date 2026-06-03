using EnergyCo.Domain.Products;

namespace EnergyCo.Domain.Promotions;

public sealed class PointsPromotion
{
    public required string PointsPromotionId { get; init; }

    public required string Name { get; init; }

    public DateTime StartDateUtc { get; init; }

    public DateTime EndDateUtc { get; init; }

    public ProductCategory? Category { get; init; }

    public int PointsPerDollar { get; init; }

    public PointsCalculationBasis CalculationBasis { get; init; } = PointsCalculationBasis.PostDiscount;

    public bool IsActiveOn(DateTime transactionDateUtc) =>
        StartDateUtc <= transactionDateUtc && transactionDateUtc < EndDateUtc;
}
