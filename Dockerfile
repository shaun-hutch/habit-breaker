# ─── Stage 1: Build Vue frontend ──────────────────────────────────────────────
FROM node:22-alpine AS frontend-build
WORKDIR /app/frontend

COPY src/frontend/package*.json ./
RUN npm ci

COPY src/frontend/ ./
RUN npm run build

# ─── Stage 2: Build .NET backend ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build
WORKDIR /app/backend

COPY src/backend/HabitBreaker.sln ./
COPY src/backend/HabitBreaker.API/HabitBreaker.API.csproj ./HabitBreaker.API/
COPY src/backend/HabitBreaker.Tests/HabitBreaker.Tests.csproj ./HabitBreaker.Tests/

RUN dotnet restore

COPY src/backend/ ./
RUN dotnet publish HabitBreaker.API/HabitBreaker.API.csproj -c Release -o /app/publish --no-restore

# ─── Stage 3: Runtime ─────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=backend-build /app/publish ./
COPY --from=frontend-build /app/frontend/dist ./wwwroot/

EXPOSE 80
ENTRYPOINT ["dotnet", "HabitBreaker.API.dll"]
