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

# Run the API
dotnet run --project src/SkillForge.Api

# Run the Blazor WebAssembly client
dotnet run --project src/SkillForge.Web
```

Configure the PostgreSQL connection string in `src/SkillForge.Api/appsettings.json` (or via user secrets / environment variables) before running the API against a database.

### Optional: PostgreSQL via Docker

```bash
docker run --name skillforge-pg \
  -e POSTGRES_USER=skillforge \
  -e POSTGRES_PASSWORD=skillforge \
  -e POSTGRES_DB=skillforge \
  -p 5432:5432 \
  -d postgres:16
```

## License

Proprietary — all rights reserved.
