
# TaskTracker

Lightweight task-tracking REST API (.NET 9), EF Core, JWT auth, and a background worker that marks overdue tasks.

## Prereqs
- .NET 9 SDK
- Windows: SQL Server LocalDB installed (for local run) or Docker Desktop (for compose)

## Setup & Run

### Option A — Docker (recommended)
```bash
docker compose up -d --build
```
- API: http://localhost:8080
- SQL Server exposed on 1433 (sa/Your_strong_password123)

### Option B — Local (LocalDB)
1. Ensure `TaskTracker.Api/appsettings.Development.json` has a valid LocalDB connection string.
2. Build solution:
   ```bash
   dotnet build TaskTracker.sln
   ```
3. Run API (applies migrations and seeds on startup):
   ```bash
   dotnet run --project TaskTracker.Api --urls "http://localhost:8080"
   ```
4. Run Worker (new shell):
   ```bash
   dotnet run --project TaskTracker.Worker
   ```

OpenAPI JSON is exposed at `/openapi/v1.json` in Development.

## Auth
- POST `/api/auth/register` { username, email, password }
- POST `/api/auth/login` { username, password } -> returns accessToken and refreshToken

Use `Authorization: Bearer <accessToken>` for `/api/tasks` endpoints.

## Tasks API
- GET `/api/tasks?status=New&dueBefore=2025-12-31&assignee=<guid>`
- GET `/api/tasks/{id}`
- POST `/api/tasks`
- PUT `/api/tasks/{id}`
- DELETE `/api/tasks/{id}`

## How to test/observe the scheduler (background worker)
1. Create a task due in the past:
   ```bash
   # Example body
   { "title": "Overdue demo", "description": "", "status": 0, "dueDate": "2024-01-01T00:00:00Z" }
   ```
2. The Worker runs hourly and marks non-completed past-due tasks as `Overdue`.
3. To observe quickly:
   - Docker: `docker compose logs -f worker`
   - Local: observe console logs of the Worker process

## Core design decisions
- Separate projects for `Domain`, `Infrastructure`, `Api`, and `Worker` for clear boundaries.
- EF Core with SQL Server; migrations included in `TaskTracker.Infrastructure`. Startup applies migrations and seeds via `DbInitializer.MigrateAndSeed`.
- JWT-based authentication with access token; refresh token stored on the user record.
- Background Worker as a hosted service scans tasks periodically for overdue status.

## Trade-offs
- Worker interval is hourly for simplicity; shorter intervals would cost more resources.
- Minimal auth flows (basic refresh token record) to keep scope tight; no rotation/blacklist.
- No pagination on `GET /api/tasks` in this iteration to focus on core flows.
- Basic error handling middleware; detailed problem details could be added.

## What to improve with more time
- Make the worker cadence configurable and add on-demand trigger endpoint for demos.
- Add pagination, sorting, and indexing for larger datasets.
- Harden authentication (refresh rotation, revocation, expiry policies).
- Introduce CQRS/mediator for larger-scale features and testing seams.
- Add Swagger UI for interactive docs in Development.

