namespace EnergyCo.Application.Services.BasketPromotions.Models;

public sealed record AppliedDiscount(
    string? PromotionId,
    string? PromotionName,
    decimal DiscountPercent,
    decimal EligibleAmount,
    decimal Amount);
