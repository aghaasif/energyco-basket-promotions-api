namespace EnergyCo.Application.Services.BasketPromotions.Models;

public sealed record BasketPromotionItem(string ProductId, decimal UnitPrice, int Quantity);
