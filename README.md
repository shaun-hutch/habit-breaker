# Habit Breaker

A Progressive Web App for tracking habits over time. Users create an account, choose a habit to work on, and log relevant information each day. The default habit duration is 66 days (the research-backed average time to form or break a habit), though this is configurable per user.

The initial focus is **food and drink intake**, with five meal slots per day (Breakfast, Morning Tea, Lunch, Dinner, Supper). Each meal entry has a free-text description and a Healthy / Unhealthy rating. The app works offline — entries are queued in the browser and synced automatically when the device reconnects.

**Tech stack:** .NET 10 ASP.NET Web API · Vue 3 + Vuetify · MS SQL Server 2022 · Docker

---

## Table of Contents

- [Running with Docker](#running-with-docker)
- [Local Development — Backend](#local-development--backend)
- [Local Development — Frontend](#local-development--frontend)
- [Development Database Setup](#development-database-setup)
- [Running Tests](#running-tests)
- [Environment Variables Reference](#environment-variables-reference)

---

## Running with Docker

The production setup runs two containers: `db` (SQL Server 2022) and `app` (ASP.NET API + Vue SPA).

**Prerequisites:** Docker Desktop (or Docker Engine + Compose v2)

### 1. Create your `.env` file

```bash
cp .env.example .env
```

Edit `.env` and fill in each value:

| Variable | Description |
|---|---|
| `SA_PASSWORD` | SQL Server SA password (min 8 chars, mixed case + digit + special) |
| `JWT_SECRET` | Random string, at least 32 characters |
| `JWT_ISSUER` | Token issuer label (default: `HabitBreaker`) |
| `JWT_AUDIENCE` | Token audience label (default: `HabitBreakerUsers`) |
| `JWT_EXPIRY_MINUTES` | Access token lifetime in minutes (default: `60`) |
| `VAPID_PUBLIC_KEY` | VAPID public key for Web Push |
| `VAPID_PRIVATE_KEY` | VAPID private key for Web Push |
| `VAPID_SUBJECT` | VAPID contact email, e.g. `mailto:admin@example.com` |
| `ALLOWED_ORIGINS` | CORS origin(s), e.g. `http://localhost` |

Generate VAPID keys with:

```bash
npx web-push generate-vapid-keys
```

### 2. Build and start

```bash
docker-compose up --build
```

The app will be available at **http://localhost**. The first startup takes longer as SQL Server initialises and the API runs EF migrations automatically before accepting traffic.

### Useful Docker commands

```bash
# Run in the background
docker-compose up -d --build

# View logs
docker-compose logs -f app
docker-compose logs -f db

# Stop and remove containers (data volume is preserved)
docker-compose down

# Stop and remove containers AND wipe the database volume
docker-compose down -v
```

---

## Local Development — Backend

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### 1. Set up the development database

See [Development Database Setup](#development-database-setup) below, then ensure `appsettings.json` (or `appsettings.Development.json`) has a connection string pointing to it:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=HabitBreaker;User Id=sa;Password=YourStr0ng!Password;TrustServerCertificate=True;"
}
```

### 2. Restore and run

```bash
cd src/backend
dotnet restore
dotnet run --project HabitBreaker.API
```

The API starts on **https://localhost:5001** (HTTP on **http://localhost:5000**). Swagger UI is available at `http://localhost:5000/swagger` in the Development environment.

### EF Migrations

To add a new migration after changing models:

```bash
cd src/backend
dotnet ef migrations add <MigrationName> --project HabitBreaker.API
```

To apply pending migrations to the development database manually:

```bash
dotnet ef database update --project HabitBreaker.API
```

> Migrations are also applied automatically at startup via `app.Services.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync()` in `Program.cs`.

---

## Local Development — Frontend

**Prerequisites:** [Node.js 20+](https://nodejs.org/)

### 1. Install dependencies

```bash
cd src/frontend
npm install
```

### 2. Configure the API URL

The dev server proxies `/api` to the backend automatically (configured in `vite.config.ts`). No `.env` changes are needed as long as the backend is running on `localhost:5000`.

If you need to override, create `src/frontend/.env.local`:

```
VITE_API_URL=http://localhost:5000
```

### 3. Start the dev server

```bash
npm run dev
```

The frontend is served at **http://localhost:5173** with hot module replacement enabled.

### Other frontend commands

```bash
# Type-check without building
npm run type-check

# Production build (outputs to dist/)
npm run build

# Preview the production build locally
npm run preview
```

---

## Development Database Setup

A local SQL Server instance is the easiest way to develop. The simplest option is to run just the `db` container from Docker Compose:

```bash
# Start only the database container
docker-compose up -d db
```

SQL Server will be available at `localhost:1433` with the SA password from your `.env` file.

The API applies EF migrations automatically on startup, creating all tables and seeding initial data (the "Better Eating" habit). No manual schema setup is required.

**Alternatively**, if you'd rather apply the schema manually against an existing SQL Server instance (e.g. bypassing migrations):

```bash
# Using sqlcmd (installed with SQL Server tools)
sqlcmd -S localhost,1433 -U sa -P 'YourStr0ng!Password' -i src/database/init.sql -C
```

The script (`src/database/init.sql`) creates the `HabitBreaker` database and all tables — it is a reference/backup and is not executed automatically by the container.

### Reset the development database

```bash
# Stop the db container and wipe the volume, then restart
docker-compose down -v
docker-compose up -d db
```

---

## Running Tests

### Backend unit tests

```bash
cd src/backend
dotnet test
```

Covers `AuthController`, `FoodLogController`, and `HabitsController` (14 tests).

### Frontend unit tests

```bash
cd src/frontend
npm run test:unit
```

Uses [Vitest](https://vitest.dev/) with jsdom. Covers the `auth` and `foodLog` Pinia stores.

### Frontend E2E tests (Playwright)

The E2E tests require the full stack to be running (backend + frontend dev server, or Docker).

```bash
# Install Playwright browsers (first time only)
cd src/frontend
npx playwright install --with-deps

# Run E2E tests
npm run test:e2e
```

Playwright tests cover: authentication flow, onboarding, food log entry, and history viewing.

---

## Environment Variables Reference

| Variable | Used by | Description |
|---|---|---|
| `SA_PASSWORD` | Docker / DB | SQL Server SA account password |
| `JWT_SECRET` | Backend | HMAC signing key for JWT tokens (≥32 chars) |
| `JWT_ISSUER` | Backend | JWT `iss` claim value |
| `JWT_AUDIENCE` | Backend | JWT `aud` claim value |
| `JWT_EXPIRY_MINUTES` | Backend | Access token lifetime |
| `VAPID_PUBLIC_KEY` | Backend + Frontend | Web Push VAPID public key |
| `VAPID_PRIVATE_KEY` | Backend | Web Push VAPID private key |
| `VAPID_SUBJECT` | Backend | Web Push contact URI |
| `ALLOWED_ORIGINS` | Backend | Comma-separated CORS origins |
| `VITE_API_URL` | Frontend (dev) | Overrides the API base URL in development |
