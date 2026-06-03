# EnergyCo Basket Promotions API

A .NET 10 backend API for calculating basket discounts and loyalty points from configurable promotions.

## Tech Stack

- .NET 10 LTS
- ASP.NET Core Minimal APIs
- Scalar API UI based on OpenApi specs
- SQLite with EF Core migrations
- In-memory caching for selective reference data
- xUnit unit tests
- Reqnroll BDD integration test
- Single Docker container for isolated runs

## Run With Docker

Prerequisite:

- Docker

Build and run:

```bash
docker build -t energyco-api .
docker run --rm -p 8080:8080 energyco-api
```

Open Scalar:

```text
http://localhost:8080/scalar/v1
```

The API listens on:

```text
http://localhost:8080
```

## Run Locally

Prerequisite:

- .NET 10 SDK

Commands:

```bash
dotnet restore EnergyCo.slnx
dotnet build EnergyCo.slnx
dotnet test EnergyCo.slnx
dotnet run --project src/EnergyCo.Api/EnergyCo.Api.csproj
```

The API applies EF Core migrations and seeds reference data on startup for reviewer convenience. A local SQLite database file named `energyco.db` is created if it does not already exist.

## API

### Calculate Basket Promotions

```http
POST /api/basket-promotions
api-version: 1.0
Content-Type: application/json
```

Sample request:

```json
{
  "customerId": "8e4e8991-aaee-495b-9f24-52d5d0e509c5",
  "loyaltyCard": "CTX0000001",
  "transactionDate": "2020-03-10T00:00:00Z",
  "basket": [
    {
      "productId": "PRD04",
      "unitPrice": 2.30,
      "quantity": 2
    },
    {
      "productId": "PRD01",
      "unitPrice": 1.20,
      "quantity": 3
    }
  ]
}
```

Sample response:

```json
{
  "customerId": "8e4e8991-aaee-495b-9f24-52d5d0e509c5",
  "loyaltyCard": "CTX0000001",
  "transactionDate": "2020-03-10T00:00:00.0000000+00:00",
  "totalAmount": "8.20",
  "discountApplied": "0.69",
  "grandTotal": "7.51",
  "pointsEarned": "15"
}
```

Health endpoints:

```text
GET /health/live
GET /health/ready
```

OpenAPI document:

```text
GET /openapi/v1.json
```

## Calculation Rules

- Basket line totals use the request `unitPrice` and `quantity` instead of UnitPrice in Products table
- If multiple active discount promotions apply to the same product, the API chooses the promotion that gives the highest product-level discount.
- Points promotions are evaluated per basket line.
- If multiple points promotions are eligible for the same basket line, the API chooses the promotion that gives the customer the highest points for that line.
- Since the points are currently calculated per basket line, qualifying spend is floored to whole dollars at line level before multiplying by points per dollar. This can award fewer points than grouping lines by winning points promotion and flooring the grouped qualifying spend once.
- A base points promotion runs indefinitely and awards 1 point per whole dollar when no better points promotion applies.
- Date/time values are normalized to UTC and Promotion source dates are interpreted as UTC calendar dates.

## Assumptions

- Authentication and authorisation are out of scope.
- The duplicate `DP001 -> PRD02` discount-product mapping is treated as a data quality issue.
- For demonstration, the duplicate mapping is replaced with `DP002 -> PRD04`.
- Default loyalty points are calculated on post-discount amounts. Points promotions can be configured to calculate from either pre-discount or post-discount amounts on per promotion basis in the database table.
- Request date/time values without an explicit timezone offset are treated as UTC.

## Scalability, Reliability, And Performance

- The API, application logic, domain rules, and persistence are separated into distinct projects following Clean Architecture.
- EF Core queries use `AsNoTracking()` for read-only reference data.
- Basket products are loaded in one batched query by distinct product IDs.
- Active promotions are queried by transaction date and cached.
- Product IDs, promotion date ranges, and promotion-product mappings are indexed.
- Active points/discount promotion reference data is cached with `IMemoryCache` to reduce repeated SQLite reads.
- Request validation and business validation return structured problem responses.
- Basic fixed-window rate limiting is applied to the basket promotion calculation endpoint.
- Health check endpoints added to expose lightweight liveness and readiness probes.
- Database connection retries were considered for reliability but not implemented as of now.

## Disclaimer

AI assistance was used during this project to help refine parts of the code, tests and documentation. All design decisions, business logic, code organisation, performance and scalability elements were planned and executed independently.
