# SkillForge

*Standardized Skill Assessment & Mock Interview Platform*

## Architecture

SkillForge follows **.NET 8 Clean Architecture** with clear separation of concerns:

| Layer | Project | Responsibility |
| --- | --- | --- |
| Domain | `SkillForge.Core` | Entities, enums, interfaces, domain exceptions (no external dependencies) |
| Application | `SkillForge.Application` | Use cases, validation, application services |
| Infrastructure | `SkillForge.Infrastructure` | EF Core / PostgreSQL, JWT auth, external integrations |
| API | `SkillForge.Api` | ASP.NET Core Web API + Swagger |
| Web | `SkillForge.Web` | Blazor WebAssembly client |

**Stack:** .NET 8 · PostgreSQL · Blazor WebAssembly · FluentValidation · EF Core (Npgsql)

```
src/
├── SkillForge.Core/
├── SkillForge.Application/
├── SkillForge.Infrastructure/
├── SkillForge.Api/
└── SkillForge.Web/
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/) 14+ (local or remote)
- [Docker](https://www.docker.com/) (optional, for running PostgreSQL in a container)

## Getting Started

```bash
# Restore and build
dotnet restore SkillForge.sln
dotnet build SkillForge.sln

# Run the API (migrates + seeds on startup by default)
dotnet run --project src/SkillForge.Api --urls http://127.0.0.1:43124

# Run the Blazor WebAssembly client
dotnet run --project src/SkillForge.Web
```

Swagger UI: `http://127.0.0.1:43124/swagger`

### Connection string & JWT

Edit `src/SkillForge.Api/appsettings.json` (or override via environment variables):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=skillforge;Username=skillforge;Password=skillforge"
  },
  "Jwt": {
    "Issuer": "SkillForge",
    "Audience": "SkillForge",
    "Key": "SkillForge_Dev_SigningKey_ChangeInProduction_32+chars!",
    "ExpirationMinutes": 120
  },
  "Database": {
    "MigrateOnStartup": true
  }
}
```

Environment variable overrides:

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=skillforge;Username=skillforge;Password=skillforge"
export Jwt__Key="YOUR_LONG_SECRET_AT_LEAST_32_CHARS!!!!"
```

### Seeded admin

| Email | Password |
| --- | --- |
| `admin@skillforge.local` | `Admin@SkillForge2026!` |

Auth endpoints: `POST /api/auth/register`, `POST /api/auth/login`

### Optional: PostgreSQL via Docker

```bash
docker run --name skillforge-pg \
  -e POSTGRES_USER=skillforge \
  -e POSTGRES_PASSWORD=skillforge \
  -e POSTGRES_DB=skillforge \
  -p 5432:5432 \
  -d postgres:16
```

## CI/CD

GitHub Actions workflows live under `.github/workflows/`:

| Workflow | Trigger | Purpose |
| --- | --- | --- |
| `ci.yml` | push / PR to `main` | Restore + Release build of the full solution |
| `deploy-pages.yml` | push to `main` | Publish `SkillForge.Web` (Blazor WASM) to GitHub Pages |

### GitHub Pages

After Pages is enabled (Settings → Pages → Source: **GitHub Actions**), the client is available at:

**https://mohamadjavad77.github.io/SkillForge/**

> Note: GitHub Pages hosts the **Blazor WebAssembly** static client only. The API (`SkillForge.Api`) and PostgreSQL need a separate host (Azure, Fly.io, Docker VPS, etc.).

## License

Proprietary — all rights reserved.
