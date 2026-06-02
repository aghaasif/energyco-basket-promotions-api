namespace EnergyCo.Application.Services.BasketPromotions.Models;

public sealed record BasketPromotionCommand(
    Guid CustomerId,
    string LoyaltyCard,
    DateOnly TransactionDate,
    IReadOnlyList<BasketPromotionItem> Basket);
