namespace EnergyCo.Domain.BasketPromotions;

public static class BasketPromotionCalculation
{
    public static decimal RoundMoney(decimal amount) =>
        Math.Round(amount, 2, MidpointRounding.AwayFromZero);

    public static int WholeDollars(decimal amount) =>
        (int)Math.Floor(amount);
}
