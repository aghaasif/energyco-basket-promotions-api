using System.Net.Http.Json;
using EnergyCo.Api.V1.Contracts;
using EnergyCo.Specs.Support;
using Reqnroll;

namespace EnergyCo.Specs.StepDefinitions;

[Binding]
public sealed class BasketPromotionSteps : IDisposable
{
    private readonly TestApiFactory _factory = new();
    private BasketPromotionRequest? _request;
    private BasketPromotionResponse? _response;

    [Given("a customer has a basket with eligible fuel and shop products")]
    public void GivenACustomerHasABasketWithEligibleFuelAndShopProducts()
    {
        _request = new BasketPromotionRequest(
            Guid.Parse("8e4e8991-aaee-495b-9f24-52d5d0e509c5"),
            "CTX0000001",
            new DateTimeOffset(2020, 3, 10, 0, 0, 0, TimeSpan.Zero),
            [
                new BasketItemRequest("PRD04", 2.30m, 2),
                new BasketItemRequest("PRD01", 1.20m, 3)
            ]);
    }

    [When("the basket promotions are calculated")]
    public async Task WhenTheBasketPromotionsAreCalculated()
    {
        using var client = _factory.CreateClient();
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/basket-promotions")
        {
            Content = JsonContent.Create(_request)
        };

        httpRequest.Headers.Add("api-version", "1.0");

        using var response = await client.SendAsync(httpRequest);
        response.EnsureSuccessStatusCode();

        _response = await response.Content.ReadFromJsonAsync<BasketPromotionResponse>();
    }

    [Then("the response includes the calculated promotion outcome")]
    public void ThenTheResponseIncludesTheCalculatedPromotionOutcome()
    {
        Assert.NotNull(_response);
        Assert.Equal("8.20", _response.TotalAmount);
        Assert.Equal("0.69", _response.DiscountApplied);
        Assert.Equal("7.51", _response.GrandTotal);
        Assert.Equal("15", _response.PointsEarned);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
