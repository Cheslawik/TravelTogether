# TravelTogether

[![CI](https://github.com/Cheslawik/TravelTogether/actions/workflows/ci.yml/badge.svg)](https://github.com/Cheslawik/TravelTogether/actions/workflows/ci.yml)

REST API for splitting shared travel expenses, in the spirit of Tricount or Splitwise.
A group adds what each person paid during a trip, and the API works out each person's balance
and the shortest list of transfers that settles everyone.

Built with **.NET 10**, **ASP.NET Core**, **EF Core** and **PostgreSQL**, following Clean Architecture.

## Features

- **Accounts:** registration and login with JWT bearer tokens. Each user sees only their own trips.
- **Trips:** create, list, view, update and delete. Every trip has one currency (ISO 4217, e.g. `EUR`).
- **Participants:** add, rename and remove the people on a trip. Participants don't need an account.
  Someone who already appears in an expense can't be removed.
- **Expenses:** add, view, update and delete. Each expense is split between some or all participants:
  - `Equal`: everyone pays the same amount;
  - `Shares`: proportional to weights, e.g. `2:1:1` when one person had a room alone.
- **Exact money arithmetic:** amounts are split in whole cents and the parts always add up to the total
  (100.00 / 3 = 33.34 + 33.33 + 33.33).
- **Balances:** what each person paid, their share of the costs and their net balance.
- **Settle up:** a short list of "who pays whom" transfers that clears all debts.
- Validation with FluentValidation and consistent error responses
  ([RFC 9457 problem details](https://www.rfc-editor.org/rfc/rfc9457)).
- Swagger UI with JWT support, a `/health` endpoint, rate limiting on the auth endpoints.
- Docker + docker-compose, GitHub Actions CI, 64 unit and integration tests.

## How debts are settled

Example: Anna, Ben and Carl spend a long weekend in Lisbon.

| Expense   | Amount | Paid by | Split                          |
|-----------|-------:|---------|--------------------------------|
| Apartment | 300.00 | Anna    | Shares 2:1:1 (Anna had a room alone) |
| Dinner    |  90.00 | Ben     | Equal between all three        |
| Taxi      |  20.00 | Carl    | Equal between Ben and Carl     |

**1. Balances** (`GET /api/trips/{id}/balances`). Net = what you paid minus your share:

| Participant | Paid   | Share (apartment + dinner + taxi) | Net         |
|-------------|-------:|-----------------------------------|------------:|
| Anna        | 300.00 | 150 + 30 + 0 = 180.00             | **+120.00** |
| Ben         |  90.00 | 75 + 30 + 10 = 115.00             | **−25.00**  |
| Carl        |  20.00 | 75 + 30 + 10 = 115.00             | **−95.00**  |

The net balances always add up to zero.

**2. Transfers** (`GET /api/trips/{id}/settlement`):

```json
{
  "currency": "EUR",
  "transfers": [
    { "fromName": "Carl", "toName": "Anna", "amount": 95.00 },
    { "fromName": "Ben",  "toName": "Anna", "amount": 25.00 }
  ]
}
```

Settled expense by expense, this would take five payments. For example, Ben owes Anna 75 for the
apartment while Anna owes Ben 30 for the dinner. The settlement needs only two.

**The algorithm** ([`DebtSimplifier`](src/TravelTogether.Domain/Settlements/DebtSimplifier.cs)):

1. Only net balances matter, not who paid for whom. If Anna owes Ben 10 and Ben owes Carl 10,
   Anna pays Carl 10 directly.
2. First, a debtor and a creditor with exactly the same amount are paired, so one transfer settles both.
3. Then, greedily, the person who owes the most pays the person who is owed the most, as much as possible.
   Each step settles at least one person, so there are **at most N − 1 transfers** for N people.

Finding the *absolute* minimum number of transfers is NP-hard in general, so this is a heuristic.
It gives the optimum, or close to it, for realistic group sizes. Unit tests cover the typical cases,
and a randomized test (500 random groups) checks that the transfers always settle everyone within N − 1 steps.

## Tech stack

| Area           | Choice |
|----------------|--------|
| Runtime        | .NET 10, ASP.NET Core Web API (controllers) |
| Data           | EF Core 10, PostgreSQL 17 (Npgsql), code-first migrations |
| Auth           | JWT bearer tokens, PBKDF2 password hashing (ASP.NET Core Identity hasher) |
| Validation     | FluentValidation |
| API docs       | Swagger / OpenAPI (Swashbuckle) |
| Tests          | xUnit, `WebApplicationFactory`, SQLite in-memory for integration tests |
| Infrastructure | Docker, docker-compose, GitHub Actions |

## Architecture

```
src/
  TravelTogether.Domain          entities, business rules, splitting and settlement logic. No dependencies.
  TravelTogether.Application     use-case services, DTOs, validators, abstractions (repositories, current user, tokens).
  TravelTogether.Infrastructure  EF Core DbContext, configurations, migrations, repositories, JWT, password hashing.
  TravelTogether.WebApi          controllers, authentication, error handling, Swagger, composition root.
tests/
  TravelTogether.UnitTests        domain and validator tests.
  TravelTogether.IntegrationTests the real HTTP pipeline end to end.
```

Dependencies point inward: `WebApi → Infrastructure → Application → Domain`.
The domain knows nothing about EF Core, HTTP or JSON.

Design decisions:

- **`Trip` is an aggregate root.** Participants and expenses are changed only through `Trip` methods,
  so rules like "the payer must belong to the trip" or "a participant used in an expense can't be removed"
  are always checked. Entities have private setters and no public parameterless constructors.
- **Rich domain, thin services.** Splitting and settlement are pure, deterministic domain code, so they are easy to unit test.
  Application services load the aggregate, call it and save it.
- **Explicit mapping and plain services instead of AutoMapper and MediatR.** Both libraries now have commercial licences,
  and for a project this size, explicit code is easier to read and debug.
- **One error format.** A global `IExceptionHandler` maps exceptions to status codes:

  | Exception | Status |
  |-----------|--------|
  | FluentValidation `ValidationException` | `400` with field errors |
  | `InvalidCredentialsException` | `401` |
  | `NotFoundException` | `404` (also for another user's trip, so ids aren't leaked) |
  | `ConflictException` | `409` (e.g. email already registered) |
  | `DomainException` | `422` (business rule violated) |
  | anything else | `500`, logged, without a stack trace in the response |

- **Secure by default.** A fallback authorization policy requires a token on every endpoint;
  only `/api/auth/*` and `/health` are anonymous.

## Getting started

### With Docker (recommended)

```bash
docker compose up --build
```

- Swagger UI: http://localhost:8080/swagger
- Health check: http://localhost:8080/health

Compose starts PostgreSQL and the API, and the API applies migrations on startup.
The JWT signing key in `docker-compose.yml` is a demo value: set `JWT_SIGNING_KEY` for anything real.

### Locally

Requirements: [.NET 10 SDK](https://dotnet.microsoft.com/download) and PostgreSQL
(for example `docker compose up db`).

```bash
dotnet run --project src/TravelTogether.WebApi
```

Swagger opens at http://localhost:5146/swagger. In `Development` the database is migrated automatically
and a development-only JWT key from `appsettings.Development.json` is used.
[`TravelTogether.WebApi.http`](src/TravelTogether.WebApi/TravelTogether.WebApi.http) contains a ready request flow
(register → login → trip → participants → expense → settlement).

### Using the API in Swagger

1. `POST /api/auth/register` with an email, a password (8+ characters, a letter and a digit) and a display name.
2. Copy `accessToken` from the response, click **Authorize** and paste it.
3. Create a trip, add participants, add expenses, then call `GET /api/trips/{tripId}/settlement`.

### Tests

```bash
dotnet test
```

The integration tests start the API in memory with SQLite, so they need neither Docker nor PostgreSQL.

### Migrations

```bash
dotnet tool restore
dotnet ef migrations add <Name> --project src/TravelTogether.Infrastructure --output-dir Persistence/Migrations
dotnet ef database update --project src/TravelTogether.Infrastructure
```

CI fails if the model has changes that aren't covered by a migration.

## Configuration

| Key | Default | Description |
|-----|---------|-------------|
| `ConnectionStrings:Database` | local PostgreSQL | Npgsql connection string |
| `Jwt:SigningKey` | *(empty: required)* | HMAC key, at least 32 characters |
| `Jwt:Issuer` / `Jwt:Audience` | `TravelTogether` | Token issuer and audience |
| `Jwt:AccessTokenLifetimeMinutes` | `60` | Access token lifetime |
| `Database:ApplyMigrationsOnStartup` | `false` (`true` in Development and compose) | Run EF migrations at startup |
| `Swagger:Enabled` | `false` (always on in Development) | Expose Swagger outside Development |
| `RateLimiting:AuthPermitsPerMinute` | `10` | Login/register attempts per IP per minute |

Settings can be overridden with environment variables, e.g. `Jwt__SigningKey`.

## API

All endpoints except auth and health require `Authorization: Bearer <token>`.

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/auth/register` | Create an account, returns a token |
| `POST` | `/api/auth/login` | Log in, returns a token |
| `GET` | `/api/trips` | List my trips |
| `POST` | `/api/trips` | Create a trip |
| `GET` | `/api/trips/{tripId}` | Trip details with participants and total spent |
| `PUT` | `/api/trips/{tripId}` | Update a trip |
| `DELETE` | `/api/trips/{tripId}` | Delete a trip with everything in it |
| `GET` | `/api/trips/{tripId}/participants` | List participants |
| `POST` | `/api/trips/{tripId}/participants` | Add a participant |
| `PUT` | `/api/trips/{tripId}/participants/{participantId}` | Update a participant |
| `DELETE` | `/api/trips/{tripId}/participants/{participantId}` | Remove a participant |
| `GET` | `/api/trips/{tripId}/expenses` | List expenses |
| `POST` | `/api/trips/{tripId}/expenses` | Add an expense |
| `GET` | `/api/trips/{tripId}/expenses/{expenseId}` | Expense with its split |
| `PUT` | `/api/trips/{tripId}/expenses/{expenseId}` | Update an expense |
| `DELETE` | `/api/trips/{tripId}/expenses/{expenseId}` | Delete an expense |
| `GET` | `/api/trips/{tripId}/balances` | Paid, share and net balance per participant |
| `GET` | `/api/trips/{tripId}/settlement` | Transfers that settle all debts |
| `GET` | `/health` | Health check, including the database |

Example: add an expense split by shares.

```http
POST /api/trips/{tripId}/expenses
Content-Type: application/json
Authorization: Bearer <token>

{
  "title": "Apartment",
  "amount": 300.00,
  "date": "2026-08-01",
  "paidByParticipantId": "<anna-id>",
  "splitType": "Shares",
  "splits": [
    { "participantId": "<anna-id>", "shares": 2 },
    { "participantId": "<ben-id>",  "shares": 1 },
    { "participantId": "<carl-id>", "shares": 1 }
  ]
}
```

For `"splitType": "Equal"` the `shares` field can be omitted.

## Possible next steps

Not implemented yet:

- inviting other users to a trip (today a trip is visible only to its owner);
- multiple currencies within one trip, with exchange rates;
- refresh tokens;
- pagination of trip and expense lists;
- recording that a transfer was paid;
- integration tests against real PostgreSQL with Testcontainers.
