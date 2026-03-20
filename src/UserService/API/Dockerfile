FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY nuget.config ./

# Copy csproj files for restore layer caching
COPY src/Shared/Auth/Shared.Auth.csproj                           src/Shared/Auth/
COPY src/UserService/Domain/UserService.Domain.csproj             src/UserService/Domain/
COPY src/UserService/Application/UserService.Application.csproj   src/UserService/Application/
COPY src/UserService/Infrastructure/UserService.Infrastructure.csproj src/UserService/Infrastructure/
COPY src/UserService/API/UserService.API.csproj                   src/UserService/API/

RUN dotnet restore src/UserService/API/UserService.API.csproj

# Copy all source
COPY src/Shared/      src/Shared/
COPY src/UserService/ src/UserService/

RUN dotnet publish src/UserService/API/UserService.API.csproj -c Release -o /out --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "UserService.API.dll"]
