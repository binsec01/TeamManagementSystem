# Team Management System

This repository contains a .NET 8 MVC application for managing organizations, teams, projects, tasks, notifications, and operational reports.

## Applications in this repository

- **`TeamManagementSystem`** — ASP.NET Core MVC web app (primary application).

For full setup, database configuration, and run instructions, see:

- [`TeamManagementSystem/README.md`](TeamManagementSystem/README.md)

## Quick start

```bash
dotnet restore
dotnet run --project TeamManagementSystem/TeamManagementSystem.csproj
```

By default, the app runs migrations at startup and seeds initial data when the database is empty.
