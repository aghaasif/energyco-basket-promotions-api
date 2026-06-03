namespace EnergyCo.Application.Services.BasketPromotions.Models;

public sealed record LineDiscount(
    string ProductId,
    decimal LineTotal,
    string? PromotionId,
    string? PromotionName,
    decimal DiscountPercent,
    decimal DiscountAmount,
    decimal DiscountedLineTotal);
