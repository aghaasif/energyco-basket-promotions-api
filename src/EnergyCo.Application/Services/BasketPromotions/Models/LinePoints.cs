using EnergyCo.Domain.Promotions;

namespace EnergyCo.Application.Services.BasketPromotions.Models;

public sealed record LinePoints(
    string ProductId,
    string? PromotionId,
    string? PromotionName,
    decimal QualifyingAmount,
    int PointsPerDollar,
    PointsCalculationBasis CalculationBasis,
    int Points);
