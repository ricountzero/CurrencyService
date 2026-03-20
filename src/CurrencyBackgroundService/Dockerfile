FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY nuget.config ./
COPY src/CurrencyBackgroundService/CurrencyBackgroundService.csproj src/CurrencyBackgroundService/
RUN dotnet restore src/CurrencyBackgroundService/CurrencyBackgroundService.csproj

COPY src/CurrencyBackgroundService/ src/CurrencyBackgroundService/
RUN dotnet publish src/CurrencyBackgroundService/CurrencyBackgroundService.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "CurrencyBackgroundService.dll"]
