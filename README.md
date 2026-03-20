# CurrencyService

A microservices solution built with .NET 8, PostgreSQL, JWT authentication, Clean Architecture, and CQRS.

## Solution Structure

```
CurrencyService.sln
├── src/
│   ├── MigrationService                — DB migration service (creates tables)
│   ├── CurrencyBackgroundService       — Background service, fetches exchange rates from CBR
│   ├── UserService/
│   │   ├── Domain                      — Entities and repository interfaces
│   │   ├── Application                 — CQRS commands/queries (MediatR)
│   │   ├── Infrastructure              — Repository implementations (Npgsql), BCrypt
│   │   └── API                         — ASP.NET Core Web API, Swagger
│   ├── FinanceService/
│   │   ├── Domain
│   │   ├── Application
│   │   ├── Infrastructure
│   │   └── API
│   ├── ApiGateway                      — Ocelot API Gateway
│   └── Shared/Auth                     — Shared JWT utilities
└── tests/
    ├── UserService.Tests               — xUnit + Moq + FluentAssertions
    └── FinanceService.Tests
```

## Prerequisites

- .NET 8 SDK
- Docker & Docker Compose
- Visual Studio 2022 (or `dotnet` CLI)

## Running with Docker Compose

```bash
docker-compose up --build
```

> **Note:** If you have a local PostgreSQL instance already running on port 5432, the Docker container's port is not exposed to the host — services communicate via the internal Docker network.

> ```

Once all services are up, the following endpoints are available:

| Service        | URL                           |
|----------------|-------------------------------|
| API Gateway    | http://localhost:5000          |
| UserService    | http://localhost:5001/swagger  |
| FinanceService | http://localhost:5002/swagger  |

## Running Locally (without Docker)

1. Start PostgreSQL and verify the connection string in each `appsettings.json`.
2. Run the migration service:
   ```bash
   dotnet run --project src/MigrationService
   ```
3. Start the currency background service:
   ```bash
   dotnet run --project src/CurrencyBackgroundService
   ```
4. Start UserService:
   ```bash
   dotnet run --project src/UserService/API
   ```
5. Start FinanceService:
   ```bash
   dotnet run --project src/FinanceService/API
   ```
6. Start the API Gateway:
   ```bash
   dotnet run --project src/ApiGateway
   ```

## Running Tests

```bash
dotnet test
```

## API Reference

All endpoints are accessible through the Gateway on port **5000**.

### Authentication

| Method | URL                  | Description              |
|--------|----------------------|--------------------------|
| POST   | /api/auth/register   | Register a new user      |
| POST   | /api/auth/login      | Obtain a JWT token       |
| POST   | /api/auth/logout     | Logout (requires JWT)    |

### Exchange Rates

All currency endpoints require `Authorization: Bearer <token>` header.

| Method | URL                                   | Description                              |
|--------|---------------------------------------|------------------------------------------|
| GET    | /api/currency                         | Get all exchange rates                   |
| GET    | /api/currency/my                      | Get rates for the user's favourites      |
| POST   | /api/currency/favorites/{currencyId}  | Add a currency to favourites             |
| DELETE | /api/currency/favorites/{currencyId}  | Remove a currency from favourites        |

## Quick Test (curl)

```bash
# Register
curl -s -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name": "alice", "password": "secret123"}'

# Login and save token
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"name": "alice", "password": "secret123"}' | jq -r '.token')

# Get all rates
curl -s http://localhost:5000/api/currency \
  -H "Authorization: Bearer $TOKEN" | jq '.[0:5]'

# Add USD to favourites
CURRENCY_ID=$(curl -s http://localhost:5000/api/currency \
  -H "Authorization: Bearer $TOKEN" | jq -r '.[] | select(.name == "USD") | .id')

curl -s -X POST "http://localhost:5000/api/currency/favorites/$CURRENCY_ID" \
  -H "Authorization: Bearer $TOKEN"

# Get favourite rates only
curl -s http://localhost:5000/api/currency/my \
  -H "Authorization: Bearer $TOKEN" | jq
```

## Tech Stack

| Technology                           | Purpose                 |
|--------------------------------------|-------------------------|
| ASP.NET Core 8                       | Web API                 |
| MediatR 12                           | CQRS                    |
| Npgsql 8                             | PostgreSQL driver       |
| BCrypt.Net-Next                      | Password hashing        |
| JWT Bearer                           | Authentication          |
| Ocelot                               | API Gateway             |
| xUnit + Moq + FluentAssertions       | Unit testing            |
| Docker Compose                       | Container orchestration |
