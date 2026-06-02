namespace EnergyCo.Application.Services.BasketPromotions.Models;

public sealed record BasketPromotionResult(
    Guid CustomerId,
    string LoyaltyCard,
    DateOnly TransactionDate,
    decimal TotalAmount,
    AppliedDiscount Discount,
    decimal GrandTotal,
    EarnedPoints Points);
