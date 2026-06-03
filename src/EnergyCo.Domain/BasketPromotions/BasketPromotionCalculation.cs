namespace EnergyCo.Domain.BasketPromotions;

public static class BasketPromotionCalculation
{
    public static decimal RoundMoney(this decimal amount) =>
        Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    public static int WholeDollars(this decimal amount) =>
        (int)Math.Floor(amount);
}
