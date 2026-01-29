import axios, { type AxiosError } from 'axios'

import type {
  Task,
  PaginatedResponse,
  CreateTaskRequest,
  UpdateTaskRequest,
  UpdateTaskStatusRequest,
  TaskFilterRequest
} from '@/types/task'

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '/api',
  headers: {
    'Content-Type': 'application/json'
  }
})

// Error handling helper
export interface ApiError {
  title: string
  detail: string
  status: number
  errors?: Record<string, string[]>
}

// "never" is used to indicate that this function never returns normally.
// It will either throw an error or run indefinitely.
function handleError(error: AxiosError): never {
  const apiError: ApiError = {
    title: 'Error',
    detail: 'An unexpected error occurred',
    status: error.response?.status || 500
  }

  if (error.response?.data) {
    const data = error.response.data as Record<string, unknown>
    apiError.title = (data.title as string) || apiError.title
    apiError.detail = (data.detail as string) || apiError.detail
    apiError.errors = data.errors as Record<string, string[]>
  }

  throw apiError
}

// This is an object literal exported as a constant which creates a singleton object.
// This is sufficient when inheritance, construction, or multiple instances are not needed.
export const taskApi = {

  // A Promise is a JavaScript/TypeScript object that represents the eventual result of an asynchronous operation;
  // something that will complete in the future.

  // Promise<...> means the function will return something eventually
  // PaginatedResponse<Task> means the eventual result will be of this type

  // API calls take time because of network latency. In such cases, instead of blocking your app,
  // a Promise lets code continue running and delivers the result later.

  // The async keyword automatically wraps the return value in a Promise. The caller uses await
  // to pause until the Promise resolves and get the actual PaginatedResponse<Task> data.

  // GET /api/tasks
  async getTasks(filter?: TaskFilterRequest): Promise<PaginatedResponse<Task>> {
    try {
      const params = new URLSearchParams()
      if (filter) {
        Object.entries(filter).forEach(([key, value]) => {
          if (value !== null && value !== undefined) {
            params.append(key, String(value))
          }
        })
      }
      const response = await api.get<PaginatedResponse<Task>>('/tasks', { params })
      return response.data
    } catch (error) {
      handleError(error as AxiosError)
    }
  },

  // GET /api/tasks/:id
  async getTaskById(id: number): Promise<Task> {
    try {
      const response = await api.get<Task>(`/tasks/${id}`)
      return response.data
    } catch (error) {
      handleError(error as AxiosError)
    }
  },

  // POST /api/tasks
  async createTask(request: CreateTaskRequest): Promise<Task> {
    try {
      const response = await api.post<Task>('/tasks', request)
      return response.data
    } catch (error) {
      handleError(error as AxiosError)
    }
  },

  // PUT /api/tasks/:id
  async updateTask(request: UpdateTaskRequest): Promise<Task> {
    try {
      const response = await api.put<Task>(`/tasks/${request.id}`, request)
      return response.data
    } catch (error) {
      handleError(error as AxiosError)
    }
  },

  // PATCH /api/tasks/:id/status
  async updateTaskStatus(id: number, request: UpdateTaskStatusRequest): Promise<Task> {
    try {
      const response = await api.patch<Task>(`/tasks/${id}/status`, request)
      return response.data
    } catch (error) {
      handleError(error as AxiosError)
    }
  },

  // DELETE /api/tasks/:id
  async deleteTask(id: number): Promise<void> {
    try {
      await api.delete(`/tasks/${id}`)
    } catch (error) {
      handleError(error as AxiosError)
    }
  }
}
