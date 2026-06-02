namespace EnergyCo.Domain.BasketPromotions;

public sealed record BasketItem(string ProductId, decimal UnitPrice, int Quantity)
{
    public decimal LineTotal => UnitPrice * Quantity;
}
