using EnergyCo.Domain.Promotions;

namespace EnergyCo.Application.Services.BasketPromotions.Models;

public sealed record EarnedPoints(
    string? PromotionId,
    string? PromotionName,
    decimal QualifyingAmount,
    int PointsPerDollar,
    PointsCalculationBasis CalculationBasis,
    int Points);
