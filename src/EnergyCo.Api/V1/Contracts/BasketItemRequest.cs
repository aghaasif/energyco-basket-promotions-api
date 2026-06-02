using System.ComponentModel.DataAnnotations;

namespace EnergyCo.Api.V1.Contracts;

public sealed record BasketItemRequest(
    [property: Required]
    [property: MinLength(1)]
    string ProductId,

    [property: Range(typeof(decimal), "0.01", "999999999")]
    decimal UnitPrice,

    [property: Range(1, int.MaxValue)]
    int Quantity);
