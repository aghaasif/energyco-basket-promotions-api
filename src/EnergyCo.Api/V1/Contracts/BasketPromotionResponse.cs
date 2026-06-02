namespace EnergyCo.Api.V1.Contracts;

public sealed record BasketPromotionResponse(
    Guid CustomerId,
    string LoyaltyCard,
    DateOnly TransactionDate,
    string TotalAmount,
    string DiscountApplied,
    string GrandTotal,
    string PointsEarned);
