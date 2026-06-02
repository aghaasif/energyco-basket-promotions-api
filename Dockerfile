FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY EnergyCo.slnx ./
COPY src/EnergyCo.Api/EnergyCo.Api.csproj src/EnergyCo.Api/
COPY src/EnergyCo.Application/EnergyCo.Application.csproj src/EnergyCo.Application/
COPY src/EnergyCo.Domain/EnergyCo.Domain.csproj src/EnergyCo.Domain/
COPY src/EnergyCo.Infrastructure/EnergyCo.Infrastructure.csproj src/EnergyCo.Infrastructure/
COPY tests/EnergyCo.Tests/EnergyCo.Tests.csproj tests/EnergyCo.Tests/
COPY tests/EnergyCo.Specs/EnergyCo.Specs.csproj tests/EnergyCo.Specs/

RUN dotnet restore EnergyCo.slnx

COPY . .

RUN dotnet publish src/EnergyCo.Api/EnergyCo.Api.csproj \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN mkdir -p /app/data

ENV ASPNETCORE_URLS=http://+:8080
ENV ConnectionStrings__Default="Data Source=/app/data/energyco.db"

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "EnergyCo.Api.dll"]
