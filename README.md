# Task Management Application

A full-stack task management application built with **ASP.NET 10 Web API** and **Vue 3** frontend, demonstrating clean architecture, modern development practices, and production-ready features.

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Running the Application](#running-the-application)
  - [Running Tests](#running-tests)
- [Project Structure](#project-structure)
- [API Documentation](#api-documentation)
- [Design Decisions & Thought Process](#design-decisions--thought-process)
- [Assumptions](#assumptions)
- [Trade-offs & Caveats](#trade-offs--caveats)
- [Scalability Considerations](#scalability-considerations)
- [Future Roadmap](#future-roadmap)

---

## Overview

This application provides a complete task management solution with the following features:

- **Create, Read, Update, Delete (CRUD)** tasks
- **Status Management**: Todo → In Progress → Done (with flexible transitions)
- **Filtering & Sorting** by status, priority, due date, and more
- **Pagination** for efficient data loading
- **Real-time UI updates** with optimistic mutations
- **Validation** on both frontend and backend

### Additional Documentation

- **[Startup Guide](docs/startup.md)** - Prerequisites installation and startup instructions
- **[Requirements and Design](docs/Requirements%20and%20Design.md)** - Detailed requirements, architecture decisions, and design rationale

---

## Architecture

The application follows a **layered architecture** with clear separation of concerns:

```
  Vue 3 Frontend (Components, Vue Query, TypeScript)
        |
        | HTTP/REST
        v
  Tasks.WebApp - API Controllers (thin facade)
        |
        v
  Tasks.Services - Business logic, validation, status rules
        |
        v
  Tasks.Data.Access - Repository, caching, DTO mapping
        |
        v
  Tasks.Data - EF Core entities, DbContext, SQLite
```

### Layer Responsibilities

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **API** | Tasks.WebApp | HTTP endpoints, request/response handling, Swagger docs |
| **Business** | Tasks.Services | Business rules, validation orchestration, status transitions |
| **Data Access** | Tasks.Data.Access | Repository pattern, caching, DTO mapping |
| **Data** | Tasks.Data | EF Core entities, DbContext, database schema |
| **Models** | Tasks.Models | Shared DTOs, request/response models, validators |

---

## Technology Stack

### Backend
- **.NET 10** / **ASP.NET Core Web API**
- **Entity Framework Core** with SQLite
- **Data Annotations** with custom validators for model validation
- **Swagger/OpenAPI** for API documentation
- **Memory Cache** for query count optimization

### Frontend
- **Vue 3** with Composition API
- **TypeScript** for type safety
- **TanStack Vue Query** for server state management (caching, invalidation)
- **Axios** for HTTP client
- **Vite** for fast development and building
- **Vitest** for unit testing

---

## Getting Started

For prerequisites, installation, running the application, and running tests, see the **[Startup Guide](docs/startup.md)**.

---

## Project Structure

```
Tasks.WebApp/                 - ASP.NET Core Web API + Vue frontend
  Controllers/
  ClientApp/src/
    components/               - TaskCard, TaskForm, TaskFilter
    composables/              - useTasks.ts (Vue Query)
    services/                 - taskApi.ts
    types/

Tasks.Services/               - Business logic layer
Tasks.Data.Access/            - Repository + caching
Tasks.Data/                   - EF Core + SQLite
Tasks.Models/                 - DTOs, validators
Tasks.ApiTester/              - API integration tests
docs/
```

---

## API Documentation

### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/tasks` | Create a new task |
| `GET` | `/api/tasks` | Get tasks (with filtering, sorting, pagination) |
| `GET` | `/api/tasks/{id}` | Get a specific task |
| `PUT` | `/api/tasks/{id}` | Update a task |
| `PATCH` | `/api/tasks/{id}/status` | Update task status only |
| `DELETE` | `/api/tasks/{id}` | Delete a task |

### Task Status Values

- `Todo` - Initial state for new tasks
- `InProgress` - Task is being worked on
- `Done` - Task is completed

### Example: Create Task

```http
POST /api/tasks
Content-Type: application/json

{
  "title": "Implement feature X",
  "description": "Detailed description here",
  "priority": 1,
  "dueDateUtc": "2026-02-15T00:00:00Z"
}
```

### Filtering & Sorting

```http
GET /api/tasks?status=InProgress&sortBy=priority&sortDirection=desc&page=1&pageSize=10
```

**Available Filter Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| `status` | string | Filter by status: `Todo`, `InProgress`, `Done` |
| `titleSearch` | string | Filter by title substring (case-insensitive) |
| `minPriority` | int | Minimum priority filter (inclusive) |
| `maxPriority` | int | Maximum priority filter (inclusive) |
| `sortBy` | string | Sort field: `priority`, `duedate`, `created`, `updated`, `title`, `sortorder` |
| `sortDirection` | string | Sort direction: `asc` or `desc` |
| `page` | int | Page number (1-based, default: 1) |
| `pageSize` | int | Items per page (1-100, default: 20) |

---

## Design Decisions & Thought Process

### 1. Layered Architecture
- **Why**: Separation of concerns enables independent testing, easier maintenance, and clear boundaries between components.
- **Trade-off**: Slightly more code, but significantly better maintainability and testability.

### 2. Repository Pattern with DTOs
- **Why**: Abstracts data access from business logic; DTOs prevent EF Core entities from leaking into higher layers.
- **Benefit**: Can swap database providers without changing service layer.

### 3. Vue Query for State Management
- **Why**: Handles caching, background refetching, and cache invalidation automatically.
- **Benefit**: Eliminates manual cache management; provides consistent loading/error states.

### 4. Service Result Pattern
- **Why**: Consistent error handling without exceptions for expected failures.
- **Benefit**: Clear distinction between validation errors, not-found, and system errors.

### 5. JSON String Enums
- **Why**: `"InProgress"` is more readable than `1` in API responses.
- **Benefit**: Self-documenting API; easier debugging; less error-prone integration.

---

## Assumptions

1. **No Authentication**: Authentication is out of scope for this MVP.
2. **Single User**: No multi-tenancy or user-specific task filtering.
3. **UTC Timestamps**: All dates stored and returned in UTC for consistency.
4. **SQLite Database**: Appropriate for demo/development; production would use SQL Server or PostgreSQL.
5. **Browser Support**: Modern browsers with ES2020+ support (Chrome, Firefox, Edge, Safari).

---

## Trade-offs & Caveats

| Decision | Trade-off |
|----------|-----------|
| SQLite | Simple setup but limited concurrency; not suitable for high-traffic production |
| Memory Cache | Fast but not distributed; cache invalidation on single instance only |
| Detailed DTOs | Type safety but more mapping code |
| Comprehensive Validation | User-friendly errors but validation logic in multiple places |

---

## Scalability Considerations

### Current Design Supports

- **Pagination**: Prevents loading entire datasets
- **Indexed Queries**: Title and status fields indexed for fast filtering
- **Caching**: Count queries cached to reduce database load
- **Async/Await**: Non-blocking I/O throughout the stack

### Production Scaling Recommendations

1. **Database**: Migrate to SQL Server or PostgreSQL for better concurrency
2. **Caching**: Replace memory cache with Redis for distributed caching
3. **API Gateway**: Add rate limiting and request throttling
4. **Load Balancing**: Deploy multiple API instances behind a load balancer
5. **Database Connection Pooling**: Configure appropriate pool sizes
6. **CDN**: Serve Vue frontend from CDN for global distribution

---

## Future Roadmap

### Phase 1: Authentication & Multi-User
- [ ] Add JWT authentication
- [ ] User registration and login
- [ ] User-specific task lists

### Phase 2: Enhanced Features
- [ ] Soft delete for data safety and recovery
- [ ] Task categories/tags
- [ ] Task assignments (assign to users)
- [ ] Due date reminders/notifications
- [ ] Task comments/activity log
- [ ] File attachments

### Phase 3: Collaboration
- [ ] Real-time updates (SignalR)
- [ ] Shared task lists/projects
- [ ] Team workspaces

### Phase 4: Advanced
- [ ] Recurring tasks
- [ ] Task dependencies
- [ ] Kanban board view
- [ ] Mobile app (MAUI or React Native)
- [ ] Calendar integration

---

## License

This project was created as a technical assessment demonstration.

---

## Author

Built with ❤️ for a technical assessment.

