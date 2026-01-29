// Matches Tasks.Models.Responses.TaskStatusResponse
export enum TaskStatus {
  Todo = 'Todo',
  InProgress = 'InProgress',
  Done = 'Done'
}

// Matches Tasks.Models.Responses.TaskResponse
export interface Task {
  id: number
  title: string
  description: string | null
  status: TaskStatus
  priority: number
  dueDateUtc: string | null
  createdAtUtc: string
  updatedAtUtc: string
  completedAtUtc: string | null
  sortOrder: number
  rowVersion: number
}

// Matches Tasks.Models.Responses.PaginatedResponse<T>
export interface PaginatedResponse<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

// Matches Tasks.Models.Requests.CreateTaskRequest
export interface CreateTaskRequest {
  title: string
  description?: string | null
  priority?: number
  dueDateUtc?: string | null
  sortOrder?: number
}

// Matches Tasks.Models.Requests.UpdateTaskRequest
export interface UpdateTaskRequest {
  id: number
  rowVersion: number
  title?: string | null
  description?: string | null
  status?: TaskStatus | null
  priority?: number | null
  dueDateUtc?: string | null
  sortOrder?: number | null
}

// Matches Tasks.Models.Requests.UpdateTaskStatusRequest
export interface UpdateTaskStatusRequest {
  rowVersion: number
  status: TaskStatus
}

// Matches Tasks.Models.Requests.TaskFilterRequest
export interface TaskFilterRequest {
  status?: TaskStatus | null
  titleSearch?: string | null
  minPriority?: number | null
  maxPriority?: number | null
  sortBy?: 'priority' | 'duedate' | 'created' | 'updated' | 'title' | 'sortorder'
  sortDirection?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}
