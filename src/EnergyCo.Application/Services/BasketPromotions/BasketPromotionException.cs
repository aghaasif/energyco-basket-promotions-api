namespace EnergyCo.Application.Services.BasketPromotions;

public sealed class BasketPromotionException : Exception
{
    public BasketPromotionException(string message)
        : base(message)
    {
    }
}
