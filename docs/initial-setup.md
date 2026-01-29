# Initial Setup Instructions

## Prerequisites

Run all commands using **PowerShell** from the solution root where the `.slnx` file is located.

---

## Database Setup

### 1. Navigate to Solution Root

```powershell
# Navigate to the solution root directory where the .slnx file is located
cd <your-solution-path>
```

### 2. Create Initial Migration

Creates a migration file in `Tasks.Data/Migrations/` based on your DbContext.

```powershell
dotnet ef migrations add InitialCreate --project Tasks.Data --startup-project Tasks.WebApp
```

| Flag | Purpose |
|------|---------|
| `--project Tasks.Data` | Tells EF where the DbContext and entities live (the data layer) |
| `--startup-project Tasks.WebApp` | Tells EF which project's `Program.cs` and configuration to use (connection strings, DI setup) |

> **Undo:** To remove a migration, run `dotnet ef migrations remove`

### 3. Apply Migration to Database

Applies the migration to the SQLite database using the connection string from `Tasks.WebApp/appsettings.json`.

```powershell
dotnet ef database update --project Tasks.Data --startup-project Tasks.WebApp
```

### 4. Future Schema Changes

When you make changes to the database schema, run:

```powershell
# Replace "AddPerformanceIndexes" with a descriptive name for your migration
dotnet ef migrations add AddPerformanceIndexes --project Tasks.Data --startup-project Tasks.WebApp
dotnet ef database update --project Tasks.Data --startup-project Tasks.WebApp
```

### 5. Viewing the Database (Optional)

To browse and query the SQLite database directly, download **DB Browser for SQLite** from:
https://sqlitebrowser.org/

The database file is located at: `Tasks.WebApp/tasks.db`

---

## Vue Frontend Setup

### 1. Create the Vue App

```powershell
cd "<your-solution-path>\Tasks.WebApp"
npm create vue@latest ClientApp
```

### 2. Features Included

The project was created with the following features:

- TypeScript
- Vitest (unit testing)
- ESLint (error prevention)
- Prettier (code formatting)

> **Note:** No experimental features were selected.

### 3. Post-Install Steps

```powershell
cd "<your-solution-path>\Tasks.WebApp\ClientApp"
npm install
npm run format
```

### 4. Build for Production

To create the `wwwroot` folder within the project:

```powershell
npm run build
```

---

## Next Steps

See [startup.md](startup.md) for commands to run the backend and frontend projects.
