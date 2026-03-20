FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY nuget.config ./

COPY src/Shared/Auth/Shared.Auth.csproj                               src/Shared/Auth/
COPY src/FinanceService/Domain/FinanceService.Domain.csproj           src/FinanceService/Domain/
COPY src/FinanceService/Application/FinanceService.Application.csproj src/FinanceService/Application/
COPY src/FinanceService/Infrastructure/FinanceService.Infrastructure.csproj src/FinanceService/Infrastructure/
COPY src/FinanceService/API/FinanceService.API.csproj                 src/FinanceService/API/

RUN dotnet restore src/FinanceService/API/FinanceService.API.csproj

COPY src/Shared/         src/Shared/
COPY src/FinanceService/ src/FinanceService/

RUN dotnet publish src/FinanceService/API/FinanceService.API.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "FinanceService.API.dll"]
