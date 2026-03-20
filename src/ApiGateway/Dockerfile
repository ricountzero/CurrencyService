FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY nuget.config ./

COPY src/Shared/Auth/Shared.Auth.csproj src/Shared/Auth/
COPY src/ApiGateway/ApiGateway.csproj   src/ApiGateway/

RUN dotnet restore src/ApiGateway/ApiGateway.csproj

COPY src/Shared/   src/Shared/
COPY src/ApiGateway/ src/ApiGateway/

RUN dotnet publish src/ApiGateway/ApiGateway.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
