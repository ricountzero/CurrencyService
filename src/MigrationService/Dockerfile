FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY nuget.config ./
COPY src/MigrationService/MigrationService.csproj src/MigrationService/
RUN dotnet restore src/MigrationService/MigrationService.csproj

COPY src/MigrationService/ src/MigrationService/
RUN dotnet publish src/MigrationService/MigrationService.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "MigrationService.dll"]
