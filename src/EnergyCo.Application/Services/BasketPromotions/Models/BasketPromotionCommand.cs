namespace EnergyCo.Application.Services.BasketPromotions.Models;

public sealed record BasketPromotionCommand(
    Guid CustomerId,
    string LoyaltyCard,
    DateTime TransactionDateUtc,
    IReadOnlyList<BasketPromotionItem> Basket);
