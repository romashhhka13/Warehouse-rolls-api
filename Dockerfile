# Restore
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore
WORKDIR /src

COPY ["WarehouseRolls.Api/WarehouseRolls.Api.csproj", "WarehouseRolls.Api/"]
RUN dotnet restore "WarehouseRolls.Api/WarehouseRolls.Api.csproj"

# Build
FROM restore AS build
WORKDIR /src
COPY . .
RUN dotnet build "WarehouseRolls.Api/WarehouseRolls.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
WORKDIR /src
RUN dotnet publish "WarehouseRolls.Api/WarehouseRolls.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

RUN useradd -m -u 1000 dotnetuser
USER dotnetuser

ENV ASPNETCORE_URLS=http://0.0.0.0:80
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "WarehouseRolls.Api.dll"]
