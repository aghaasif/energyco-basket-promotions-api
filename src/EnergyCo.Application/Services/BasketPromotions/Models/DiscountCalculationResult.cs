namespace EnergyCo.Application.Services.BasketPromotions.Models;

public sealed record DiscountCalculationResult(
    decimal EligibleAmount,
    decimal DiscountAmount,
    IReadOnlyList<LineDiscount> LineDiscounts);
