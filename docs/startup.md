# Startup Guide

This guide covers setting up the Task Management application on a fresh Windows machine.

---

## Prerequisites

### 1. Install Git

**Option A: Direct Download (Recommended)**

1. Download from: https://git-scm.com
2. Run the installer with default options
3. Restart your terminal after installation

**Option B: Using winget**

```bash
winget install Git.Git
```

**Verify installation:**
```bash
git --version
```

### 2. Install .NET 10 SDK

Download and install from: https://dotnet.microsoft.com/download/dotnet/10.0

Verify installation:
```bash
dotnet --version
```

### 3. Install Node.js and npm

**Option A: Direct Download (Recommended)**

1. Download the LTS installer from: https://nodejs.org/
2. Run the installer and follow the prompts
3. Ensure "Add to PATH" is checked during installation
4. Restart your terminal after installation

**Option B: Using winget**

```bash
winget install OpenJS.NodeJS.LTS
```

**Option C: Using Chocolatey**

```bash
choco install nodejs-lts
```

**Verify installation:**
```bash
node --version
npm --version
```

---

## Setup

### 1. Clone the Repository

```bash
git clone https://github.com/vijaymohan1979/tasks.git
```

### 2. Restore Backend Dependencies

```bash
cd "<your-solution-path>\Tasks.WebApp"
dotnet restore
```

### 3. Install Frontend Dependencies

```bash
cd "<your-solution-path>\Tasks.WebApp\ClientApp"
npm install
```

---

## Running the Application

### Start the Backend API (Terminal 1)

```bash
cd "<your-solution-path>\Tasks.WebApp"
dotnet run
```

### Start the Vue Frontend (Terminal 2)

```bash
cd "<your-solution-path>\Tasks.WebApp\ClientApp"
npm run dev
```

### Access the Application

| Service | URL |
|---------|-----|
| **Frontend** | http://localhost:5173 |
| **Backend API** | http://localhost:5211 |
| **Swagger Docs** | http://localhost:5211/swagger |

---

## Running Tests

### Backend API Tests

Ensure the API is running first, then in a new terminal:

```bash
cd "<your-solution-path>\Tasks.ApiTester"
dotnet run
```

The test suite covers:
- Task creation (valid/invalid inputs)
- Task retrieval (by ID, with filters)
- Task updates (fields and status)
- Task deletion
- Validation error handling
- Edge cases

### Frontend Unit Tests

```bash
cd "<your-solution-path>\Tasks.WebApp\ClientApp"
npm run test:unit -- --run
```

The frontend test suite includes:
- **TaskForm.spec.ts** - Form validation, create/edit mode, field population
- **TaskFilter.spec.ts** - Filter initialization, apply/clear filters, sort options
- **useTasks.spec.ts** - Query key generation and cache hierarchy
- **App.spec.ts** - Application mounting and rendering
