# TeamManagementSystem Web Application

`TeamManagementSystem` is an ASP.NET Core MVC web application for collaborative work management across organizations, teams, projects, and tasks.

## What the application includes

- **Identity & authentication** with ASP.NET Core Identity (register/login, role support).
- **Organization and team management** with workspace membership roles.
- **Project and task management** including board/list views, status workflow, priorities, comments, and attachments.
- **Notifications and activity tracking** for user-facing updates and auditing.
- **Reports** for workload, project progress, and activity.

## Tech stack

- .NET 8 (`net8.0`)
- ASP.NET Core MVC
- Entity Framework Core 8
- SQL Server provider (`Microsoft.EntityFrameworkCore.SqlServer`)
- ASP.NET Core Identity with Entity Framework stores

## Project structure

Key folders:

- `Controllers/` — MVC endpoints (account, organizations, teams, projects, tasks, reports, etc.)
- `Data/` — `AppDbContext`, migrations, initializer, and seed logic
- `Models/` — domain and identity models
- `Services/` — business services (authorization, activity, notifications, file storage, task query)
- `Views/` — Razor UI
- `wwwroot/` — static assets and local upload storage

## Prerequisites

- .NET SDK 8.0+
- SQL Server-compatible database instance:
  - SQL Server LocalDB (default on many Windows dev setups), or
  - SQL Server (local/remote), or
  - SQL Server in Docker

Optional but useful:

- `dotnet-ef` CLI tool for migration commands:

```bash
dotnet tool install --global dotnet-ef
```

## Installation

From repository root:

```bash
dotnet restore
dotnet build TeamManagementSystem/TeamManagementSystem.csproj
```

## Database setup and connection

The app reads the connection string from `ConnectionStrings:DefaultConnection` in `appsettings.json`.

### 1) Configure connection string

Default value:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=TeamManagementDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

For local/remote SQL Server, set an override via environment variable:

**Linux/macOS (bash):**

```bash
export ConnectionStrings__DefaultConnection='Server=localhost,1433;Database=TeamManagementDB;User Id=sa;Password=Your_password123;TrustServerCertificate=True;MultipleActiveResultSets=true'
```

**Windows PowerShell:**

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=localhost,1433;Database=TeamManagementDB;User Id=sa;Password=Your_password123;TrustServerCertificate=True;MultipleActiveResultSets=true'
```

> Tip: for persistent local settings, put the connection string in `appsettings.Development.json` (avoid committing secrets).

### 2) Create/update database schema

This project applies migrations automatically at startup using `DbInitializer.InitializeAsync(app)`.

So the minimal flow is simply:

```bash
dotnet run --project TeamManagementSystem/TeamManagementSystem.csproj
```

If you prefer explicit migration execution, run:

```bash
dotnet ef database update --project TeamManagementSystem/TeamManagementSystem.csproj
```

### 3) Seed data

When the database is empty, startup seeding creates:

- Roles: `Admin`, `TeamLead`, `Member`, `Client`
- Default organization and team
- Admin user:
  - Email: `admin@tms.local`
  - Password: `Admin@123`

Change seeded credentials in `Data/Seed/SeedData.cs` for non-local environments.

## Run the application

From repo root:

```bash
dotnet run --project TeamManagementSystem/TeamManagementSystem.csproj
```

Default local URLs from launch settings:

- `http://localhost:5249`
- `https://localhost:7298`

## Common development commands

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build TeamManagementSystem/TeamManagementSystem.csproj

# Run
dotnet run --project TeamManagementSystem/TeamManagementSystem.csproj

# Apply migrations manually
dotnet ef database update --project TeamManagementSystem/TeamManagementSystem.csproj
```

## Notes

- Local file uploads are stored under `wwwroot/uploads` via the configured `FileStorage` options.
- The app includes an audit middleware to persist activity logs for authenticated actions.
