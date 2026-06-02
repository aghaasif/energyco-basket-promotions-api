# EnergyCo Basket Promotions API

A .NET 10 backend API for calculating basket discounts and loyalty points from configurable promotions.

## Tech Stack

- .NET 10 LTS
- ASP.NET Core Minimal APIs
- Header-based API versioning
- Scalar API reference UI
- SQLite with EF Core migrations
- In-memory caching for reference data
- xUnit unit tests
- Reqnroll BDD integration test
- Single Docker container

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
  "pointsEarned": "16"
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

- Basket line totals use the request `unitPrice` and `quantity`.
- Product catalogue data is used for product existence, category, and promotion eligibility.
- Promotion date ranges are inclusive.
- If multiple discount promotions are eligible, the API chooses the one that gives the customer the highest discount.
- If multiple points promotions are eligible, the API chooses the one that gives the customer the highest points.
- Discount promotions do not stack with other discount promotions.
- Points promotions do not stack with other points promotions.
- One discount promotion and one points promotion can apply to the same basket.
- Duplicate discount-product mappings are de-duplicated.
- Points are calculated by flooring qualifying spend to whole dollars before multiplying by points per dollar.
- Money is calculated with `decimal` and rounded to two decimal places.
- Date/time values are normalized to UTC.
- Promotion source dates are interpreted as UTC calendar dates. End dates are inclusive in the supplied source data and represented internally as exclusive UTC end instants.

## Assumptions

- Authentication and authorisation are out of scope.
- `customerId` is read from the payload only. In production, identity would come from trusted authentication claims.
- The supplied sample response appears inconsistent with the supplied basket, prices, and promotion dates, so this implementation calculates from the rules and reference data.
- The duplicate `DP001 -> PRD02` discount-product mapping is treated as a data quality issue.
- For demonstration, the duplicate mapping is replaced with `DP002 -> PRD04`.
- Default loyalty points are calculated on the pre-discount amount, with support in the domain model for post-discount calculation.
- Request date/time values without an explicit timezone offset are treated as UTC.
- No secrets are required for this sample.

## Scalability, Reliability, And Performance

- The API, application logic, domain rules, and persistence are separated into distinct projects.
- EF Core queries use `AsNoTracking()` for read-only reference data.
- Basket products are loaded in one batched query by distinct product IDs.
- Active promotions are queried by transaction date.
- Product IDs, promotion date ranges, and promotion-product mappings are indexed.
- Reference data is cached with `IMemoryCache` to reduce repeated SQLite reads.
- Basket-specific quote responses are not cached.
- Request validation and business validation return structured problem responses.
- Basic fixed-window rate limiting is applied to the basket promotion calculation endpoint.

In a larger deployment, the same application boundary could use a server database such as SQL Server or PostgreSQL. Hot read paths could move to read replicas, a distributed cache such as Redis, or compiled EF queries after profiling shows a need.

## Production Migration Notes

Startup migrations are used here to make the project easy to run during review. In production, migrations should run as a separate deployment step or job before API rollout.

For breaking schema changes, prefer an expand/contract approach:

- add compatible schema first
- backfill data separately
- deploy application changes
- verify behaviour in production-like environments
- remove old schema only after callers have moved

Riskier releases should include backups, migration-duration monitoring, post-deployment validation, and rollback-forward planning.
