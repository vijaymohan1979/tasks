# Take Home Test Requirements

The technical assessment is to help anyone understand my:

- **Coding Abilities**
- **Architectural Thinking**
- **Problem-solving approach**

The high-level requirements are to build a task management app with an API and front-end components.

---

## Goals

We are designing for:

- Clean, well-structured code
- Thoughtful architectural decisions
- Good communication between frontend and backend
- Production-ready features and considerations
- Clear documentation and setup instructions

---

## Requirements

Build a small task management API and frontend. Add any features required for a Production MVP.

### Assumptions

With a reduced scope, I will start with the following assumptions:

- No authentication
- **Add Task**
- **Update Task**
- **Delete Task**
- **Mark Task as:**
  - `TODO` (initial state)
  - `IN PROGRESS`
  - `DONE`
- **View Tasks**
  - Filtering
  - Sorting

---

## Evaluations

### Backend API Design (using .NET Core)

- Will use **ASP.NET 10 Web API** with REST

### Data Structure Design (using SQLite)

| Layer | Description |
|-------|-------------|
| **Database Schema** | SQLite database structure |
| **EF-Core Entities** | Entity Framework Core data models |
| **DTO Layer** | Request/Response contracts used to communicate between API, service, and data layers |
| **DAL Layer** | Uses EF-Core and returns DTOs to the backend service layer |

**Design Principles:**

- Separation of concerns, modularity
- Loose coupling between layers, high cohesion within a layer

### Frontend Component Design (using Vue)

#### Separation of Concerns

- Separate logic from presentation

#### User Input Validation

- Required fields
- Lengths
- Valid Ranges

#### Data Management with Vue Query

- Caching
- Invalidation
- Error states

#### State Handling

Handle the following states clearly:

- **Loading**
- **Error**
- **Empty**

#### Backend Contract Matching

- Route
- Field Name
- Data shape

### Communication Between Frontend and Backend

| Aspect | Implementation |
|--------|----------------|
| **API Calls** | Simple REST calls to the Controller hosting a REST API |
| **Documentation** | Swagger for API visualization & easy testability |
| **Architecture** | API is a Façade, actual logic in the backend service layer |
| **Data Transfer** | DTOs for requests and responses |
| **Error Handling** | Consistent, structured responses |

**Service Layer Responsibilities:**

- Input validation
- Separation of concerns, modularity
- Loose coupling between layers, high cohesion within a layer

### Clean Code

#### Guidelines

- **Single Responsibility**: Each class/component has one clear purpose
- **DRY (Don't Repeat Yourself)**: Shared validators, mappers, and query keys
- **Meaningful Names**: Descriptive method and variable names (`CreateTaskAsync`, `taskKeys.list()`)
- **Small Functions**: Methods focused on single tasks with clear inputs/outputs
- **Consistent Formatting**: Standard code style across all projects
- **XML Documentation**: Public APIs documented with `<summary>` comments
- **Error Handling**: Explicit error types with `ServiceResult<T>` pattern
- **Immutability**: Prefer readonly collections; consider records for future refactoring
- **Dependency Injection**: All dependencies injected via constructor
- **Guard Clauses**: Early validation with `ArgumentNullException.ThrowIfNull()`

### Architecture Structure and Thought Process

Detailed in this document.

---

## Design

Different projects within the solution have been designed as shown below:

| Project | Description |
|---------|-------------|
| **Tasks.Models** | Shared Contracts (DTOs) |
| **Tasks.Data** | EF Core Infrastructure |
| **Tasks.Data.Access** | DAL/ Repository layer |
| **Tasks.Services** | Business Logic |
| **Tasks.WebApp** | API Layer |

### Key Design Decisions

#### Repository Pattern

| Decision | Rationale |
|----------|-----------|
| **Interface-based (ITaskRepository)** | Abstraction enables testing and future swaps |
| **Single Responsibility** | Each method handles one operation (CRUD, filtering, sorting) |
| **Async/await** | Scalability |
| **CancellationToken** | Graceful shutdowns, request timeouts |

#### Data Transfer Objects (DTOs)

Contracts are first-class citizens. DTOs represent the business domain, not infrastructure.

| Layer Boundary | DTO Type | Direction |
|----------------|----------|-----------|
| API → Service | `CreateTaskRequest`, `UpdateTaskRequest` | Inbound |
| Service → API | `TaskResponse` | Outbound |
| Service → Repository | `TaskFilterRequest` | Query specs |

#### Validation Strategy

- Shared and centralized within `Tasks.Models.Validators`
- Declarative (`[Required]`, `[StringLength]`) + custom logic
- Applied in DAL before persistence
- Same rules across API, Service & DAL

#### Filtering & Sorting

- `TaskFilterRequest` encapsulates all query parameters
- Supports sorting by: priority, duedate, created, updated, title & sortorder
- Page-based (1-indexed) pagination, max 100 items per page
- The indexing strategy covers the most common queries

#### Concurrency & Timestamps

- `CreatedAtUtc` – immutable, set on creation
- `UpdatedAtUtc` – updated on every modification
- `CompletedAtUtc` – auto-set when marking Done
- `RowVersion` – optimistic locking

#### Dependency Inversion Graph

```
WebApp Controller → ITaskService → ITaskRepository → Tasks.Models (shared across all layers) → DTOs, Validators, Enums
```

Each layer depends on abstractions, not implementations. Easy to mock, test and refactor.

### ASP.NET Controller

The `TasksController` will be a façade which delegates all business logic to `ITaskService`, following these principles:

- **RESTful API** with standard HTTP verbs (GET, POST, PUT, PATCH, DELETE) with resource-based URLs
  - POST returns 201 with location header
  - DELETE returns 204
- **Input Validation**
  - Layer 1: Data Annotations (ModelState)
  - Service-layer validation
- **Routing**
  - `{id:int}` prevents invalid route matching
- **Consistent Response Mapping**
- **Structured Error Responses** using ProblemDetails
- **Concurrency Support**
  - RowVersion required on PUT/PATCH for optimistic locking
- **Async/CancellationToken support**
- **Swagger Documentation**

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/tasks` | Create a task |
| GET | `/api/tasks/{id}` | Get task by ID |
| GET | `/api/tasks` | Get tasks (filtered, sorted, paginated) |
| PUT | `/api/tasks/{id}` | Update a task. PUT is typically used to replace an entire resource. |
| PATCH | `/api/tasks/{id}/status` | Update task status only. PATCH is typically used for the partial update of a resource. |
| DELETE | `/api/tasks/{id}` | Delete a task |

### Vue Based Front-end

#### Axios

We are using Axios, which is a popular JavaScript/TypeScript HTTP client library used to make HTTP requests from both browsers and Node.js. 

Key features include:

- Returns promises, and works with async/await
- Transforms data before sending or after receiving using request/response interceptors
- Converts JSON responses automatically
- Provides detailed error information using `AxiosError`
- Allows the creation of pre-configured clients (like the API client)

#### TanStack Query (Vue Query)

We are using TanStack Query (formerly known as React Query) which is a data-fetching and server state management library for JavaScript/TypeScript applications. It works with React, Vue, Solid, and Svelte.

**Core Concepts:**

- **Queries** (GET data)
- **Mutations** (create/update/delete data)
- **Query Keys** (unique identifiers for queries)
- **Query Client** (manages cache and state)

Using this framework reduces boilerplate code because we do not need to implement manual loading or error state management.

---

## Testing

| Type | Project | Coverage |
|------|---------|----------|
| **Backend API Integration Tests** | `Tasks.ApiTester` | CRUD operations, validation, filtering, status transitions, edge cases |
| **Frontend Unit Tests** | `ClientApp` (Vitest) | Component rendering, composables, service layer |

### Backend Test Suite (`Tasks.ApiTester`)

| Test Class | Coverage |
|------------|----------|
| `CreateTaskTests` | Valid/invalid task creation, required fields, field lengths |
| `GetTaskByIdTests` | Existing tasks, non-existent IDs, invalid IDs |
| `GetTasksFilterTests` | Status filtering, pagination, sorting, empty results |
| `UpdateTaskTests` | Field updates, partial updates, validation |
| `UpdateTaskStatusTests` | Valid transitions, invalid transitions, concurrent updates |
| `DeleteTaskTests` | Delete operations, idempotency, non-existent tasks |
| `ValidationTests` | Boundary conditions, error message format |

### Frontend Tests

- Component rendering tests (TaskForm, TaskFilter)
- Vue Query composable tests (useTasks)
- Application component tests (App)

### Running the API Tester

`Tasks.ApiTester` is a console application that can be used to test the APIs within the app.

It can be run using the following commands from the `tasks\Tasks.ApiTester` folder:

```bash
# Default (http://localhost:5211)
dotnet run --project Tasks.ApiTester

# Custom URL
dotnet run --project Tasks.ApiTester http://localhost:5211/api/tasks
```

---

## Other Considerations

### Trade-offs

| Decision | Benefit | Trade-off |
|----------|---------|-----------|
| **SQLite Database** | Zero configuration, portable, easy setup | Limited concurrency, not suitable for high-traffic production |
| **Memory Cache** | Fast access, simple implementation | Not distributed; cache invalidation limited to single instance |
| **Detailed DTOs** | Type safety, layer isolation, clear contracts | More mapping code between layers |
| **Comprehensive Validation** | User-friendly error messages, data integrity | Validation logic duplicated in frontend and backend |
| **Service Result Pattern** | Explicit error handling without exceptions | Additional wrapper types, more verbose code |
| **Layered Architecture** | Separation of concerns, testability, maintainability | Slightly more boilerplate code |
| **Vue Query for State** | Automatic caching, background refetch, cache invalidation | Learning curve, additional dependency |

### Caveats

- **No Authentication**: This MVP does not include user authentication or authorization
- **Single Instance**: Memory caching strategy assumes single server deployment
- **SQLite Limitations**: Not suitable for concurrent write-heavy workloads
- **UTC Only**: All timestamps are stored and returned in UTC; clients must handle timezone conversion
- **Browser Support**: Requires modern browsers
- **No Real-time Updates**: Changes from other sources require manual refresh or polling

### Scalability

#### Current Design Supports

- ✅ **Pagination** - Prevents loading entire datasets; configurable page sizes
- ✅ **Indexed Queries** - Title and status fields indexed for fast filtering
- ✅ **Count Caching** - Expensive count queries cached to reduce database load
- ✅ **Async/Await** - Non-blocking I/O throughout the entire stack
- ✅ **AsNoTracking** - Read queries don't use EF Core change tracking (performance)
- ✅ **Cancellation Tokens** - Proper request cancellation support

#### Production Scaling Recommendations

| Area | Recommendation |
|------|----------------|
| **Database** | Migrate to SQL Server or PostgreSQL for better concurrency and features |
| **Caching** | Replace `IMemoryCache` with Redis for distributed caching across instances |
| **API Gateway** | Add rate limiting, request throttling, and API versioning |
| **Load Balancing** | Deploy multiple API instances behind a load balancer (Azure App Service, K8s) |
| **Connection Pooling** | Configure appropriate database connection pool sizes |
| **CDN** | Serve Vue frontend static assets from CDN for global distribution |
| **Monitoring** | Add Application Insights or similar APM for observability |
| **Message Queue** | Use Azure Service Bus or RabbitMQ for async processing of heavy operations |

### Future Roadmap Items

#### Phase 1: Authentication & Multi-User
- [ ] JWT authentication with refresh tokens
- [ ] User registration and login
- [ ] User-specific task lists (multi-tenancy)
- [ ] Role-based access control (RBAC)

#### Phase 2: Enhanced Features
- [ ] Soft delete for data safety and recovery
- [ ] Task categories/tags with filtering
- [ ] Task assignments (assign to other users)
- [ ] Due date reminders and email notifications
- [ ] Task comments and activity log
- [ ] File attachments (Azure Blob Storage)
- [ ] Bulk operations (delete, status change)

#### Phase 3: Collaboration
- [ ] Real-time updates using SignalR
- [ ] Shared task lists and projects
- [ ] Team workspaces
- [ ] @mentions and notifications
- [ ] Activity feed

#### Phase 4: Advanced Features
- [ ] Recurring tasks (daily, weekly, monthly)
- [ ] Task dependencies and blocking relationships
- [ ] Kanban board view with drag-and-drop
- [ ] Calendar view integration
- [ ] Mobile app (MAUI or React Native)
- [ ] Offline support with sync
- [ ] Export/Import (CSV, JSON)
- [ ] API webhooks for integrations

---

## Submission Checklist

- [✔️] Complete test
- [✔️] Submit via GitHub repo link
- [✔️] Include a comprehensive `README.md` with:
  - [✔️] **Setup steps**
    - How to run
    - How to test
  - [✔️] **Thought process**
  - [✔️] **Explanation Notes**
  - [✔️] **Assumptions**
  - [✔️] **Scalability**
  - [✔️] **Future roadmap**
