namespace EnergyCo.Application.Services.BasketPromotions.Models;

public sealed record BasketPromotionResult(
    Guid CustomerId,
    string LoyaltyCard,
    DateTime TransactionDateUtc,
    decimal TotalAmount,
    AppliedDiscount Discount,
    decimal GrandTotal,
    EarnedPoints Points);
