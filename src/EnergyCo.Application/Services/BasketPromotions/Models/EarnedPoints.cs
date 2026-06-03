namespace EnergyCo.Application.Services.BasketPromotions.Models;

public sealed record EarnedPoints(
    decimal QualifyingAmount,
    int Points,
    IReadOnlyList<LinePoints> LinePoints);
