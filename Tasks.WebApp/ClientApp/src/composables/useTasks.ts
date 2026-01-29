/**
 * useTasks.ts - Task Data Management Composable
 * 
 * This file contains "composables" - reusable functions that encapsulate
 * data-fetching logic for Vue components. Think of composables as utility
 * functions that manage server communication and caching automatically.
 * 
 * Key Concepts:
 * - Query: A read operation (fetching data from server)
 * - Mutation: A write operation (creating/updating/deleting data)
 * - Cache: Temporary storage of fetched data to avoid repeated server calls
 * - Invalidation: Marking cached data as outdated so it gets refetched
 */

/**
 * Import hooks from TanStack Query (a data-fetching library):
 * - useQuery: Hook for fetching/reading data
 * - useMutation: Hook for creating/updating/deleting data
 * - useQueryClient: Hook to access the cache manager
 */
import { useQuery, useMutation, useQueryClient } from '@tanstack/vue-query'

/**
 * Import the API service that makes actual HTTP calls to the backend.
 * - taskApi: Object containing methods like getTasks(), createTask(), etc.
 * - ApiError: TypeScript type defining the shape of error responses
 * 
 * The "type" keyword means we're only importing the type definition,
 * not actual runtime code. This is a TypeScript optimization.
 */
import { taskApi, type ApiError } from '@/services/taskApi'

import type {
  Task,
  PaginatedResponse,
  CreateTaskRequest,
  UpdateTaskRequest,
  UpdateTaskStatusRequest,
  TaskFilterRequest
} from '@/types/task'

/**
 * Import Vue 3 reactivity utilities:
 * - computed: Creates a value that auto-updates when its dependencies change
 * - Ref: TypeScript type for reactive references (values Vue can track)
 */
import { computed, type Ref } from 'vue'

/**
 * Query keys are unique identifiers used to:
 * 1. Store data in the cache (like a dictionary key)
 * 2. Find and invalidate related cached data when it becomes stale
 * 
 * We organize keys hierarchically so we can invalidate at different levels:
 * - Invalidate ['tasks'] → clears ALL task-related cache
 * - Invalidate ['tasks', 'list'] → clears only task lists, keeps individual tasks
 * - Invalidate ['tasks', 'detail', 5] → clears only task with id=5
 * 
 * "as const" is TypeScript syntax that makes the array:
 * 1. Readonly (can't be modified)
 * 2. Literally typed (exactly ['tasks'], not just string[])
 */
export const taskKeys = {
  /**
   * Base key for all task-related queries.
   * Result: ['tasks']
   */
  all: ['tasks'] as const,

  /**
   * Key for all task list queries.
   * The "..." spread operator expands the array: [...['tasks']] becomes ['tasks']
   * Then we add 'list' to create: ['tasks', 'list']
   * 
   * This is a function (not a value) because we need to call it fresh each time
   * to ensure we get a new array instance.
   */
  lists: () => [...taskKeys.all, 'list'] as const,

  /**
   * Key for a specific filtered task list.
   * Includes the filter object so different filters have different cache entries.
   * Example result: ['tasks', 'list', { status: 'active', page: 1 }]
   * 
   * @param filter - The filter criteria being used
   */
  list: (filter: TaskFilterRequest) => [...taskKeys.lists(), filter] as const,

  /**
   * Key for all individual task detail queries.
   * Result: ['tasks', 'detail']
   */
  details: () => [...taskKeys.all, 'detail'] as const,

  /**
   * Key for a specific task by ID.
   * Example result: ['tasks', 'detail', 42]
   * 
   * @param id - The task's unique identifier
   */
  detail: (id: number) => [...taskKeys.details(), id] as const
}

// QUERY COMPOSABLES - For reading/fetching data

/**
 * Composable for fetching a filtered list of tasks.
 */
export function useTasks(filter: Ref<TaskFilterRequest>) {
  /**
   * useQuery is the TanStack Query hook for fetching data.
   * It handles loading states, caching, and refetching automatically.
   */
  return useQuery({
    // queryKey: Unique identifier for this cached data.
    // "computed()" makes this reactive - when filter.value changes,
    // the key changes, and TanStack Query fetches data for the new key.
    queryKey: computed(() => taskKeys.list(filter.value)),
    // queryFn: The function that actually fetches the data.
    // This is called when:
    // 1. Data is not in cache
    // 2. Cached data is stale(older than staleTime)
    // 3. Query is manually invalidated
    queryFn: () => taskApi.getTasks(filter.value),
    // staleTime: How long (in milliseconds) cached data is considered fresh.
    // - During this time, cached data is returned immediately(no refetch)
    // - After this time, data is refetched in the background
    staleTime: 30000 // 30 seconds
  })
}

// Composable Function for fetching a single task by its ID.
export function useTask(id: Ref<number>) {
  return useQuery({
    // each task is cached separately
    queryKey: computed(() => taskKeys.detail(id.value)),
    queryFn: () => taskApi.getTaskById(id.value),
    /**
     * enabled: Controls whether the query should run.
     * 
     * Here we only fetch if id > 0 (a valid ID).
     * This prevents fetching when:
     * - id is 0 (no task selected)
     * - id is negative (invalid)
     * 
     * Using computed() makes this reactive - if id changes from 0 to 5,
     * the query automatically enables and fetches.
     */
    enabled: computed(() => id.value > 0)
  })
}

// MUTATION COMPOSABLES - For creating/updating/deleting data

/**
 * Composable Function for creating a new task.
 * 
 * Unlike queries (which run automatically), mutations must be triggered
 * manually by calling .mutate() or .mutateAsync().
 */
export function useCreateTask() {
  const queryClient = useQueryClient()

  return useMutation<Task, ApiError, CreateTaskRequest>({
    mutationFn: (request) => taskApi.createTask(request),
    onSuccess: () => {
      // Invalidate all task lists to refetch
      queryClient.invalidateQueries({ queryKey: taskKeys.lists() })
    }
  })
}

// Composable Function for updating an existing task.
export function useUpdateTask(currentFilter?: Ref<TaskFilterRequest>) {
  // Get access to the "query client" - this is the cache manager.
  const queryClient = useQueryClient()

  /**
   * useMutation hook with three generic type parameters:
   * 
   * useMutation<TData, TError, TVariables>
   *   - TData (Task): Type of successful response (the created task)
   *   - TError (ApiError): Type of error response
   *   - TVariables (CreateTaskRequest): Type of input data
   * 
   */
  return useMutation<Task, ApiError, UpdateTaskRequest>({
    // mutationFn: The function that actually calls the server.
    mutationFn: (request) => taskApi.updateTask(request),
    // onSuccess: This function runs AFTER the server responds successfully.
    onSuccess: (updatedTask, request) => {
      // STEP 1: Update the cache for this specific task.
      queryClient.setQueryData(taskKeys.detail(updatedTask.id), updatedTask)

      // STEP 2: Decide whether we need to refetch the task list from the server,
      // or if we can just update the local cache directly.

      // Get the current filter settings.
      const filter = currentFilter?.value

      // STEP 3: Check if what we changed could affect filtering or sorting.
      //
      // If ANY of these is true, we need to refetch from the server.

      const affectsFilterOrSort =
        // Condition 1: Did we change status AND is there a status filter active?
        (request.status !== undefined && filter?.status) ||
        // Condition 2: Did we change priority AND is priority involved in filtering/sorting?
        (request.priority !== undefined && (filter?.minPriority || filter?.maxPriority || filter?.sortBy === 'priority')) ||
        // Condition 3: Did we change title AND is title involved in filtering/sorting?
        (request.title !== undefined && (filter?.titleSearch || filter?.sortBy === 'title')) ||
        // Condition 4: Did we change due date AND are we sorting by due date?
        (request.dueDateUtc !== undefined && filter?.sortBy === 'duedate') ||
        // Condition 5: Did we change sort order AND are we sorting by sort order?
        (request.sortOrder !== undefined && filter?.sortBy === 'sortorder')

      // STEP 4: Either refetch from server OR update cache directly.
      
      if (affectsFilterOrSort) {
        // The change DOES affect filtering/sorting.
        //
        // invalidateQueries marks all cached task lists as "stale" (outdated),
        // which triggers an automatic refetch from the server.
        //console.log('useUpdateTask:', { affectsFilterOrSort, action: 'invalidateQueries', request, ...(filter && { filter }) })
        queryClient.invalidateQueries({ queryKey: taskKeys.lists() })
      } else {
        //console.log('useUpdateTask:', { affectsFilterOrSort, action: 'NOT invalidateQueries', request, ...(filter && { filter }) })
        // The change does NOT affect filtering/sorting.
        // Update in-place without refetching
        queryClient.setQueriesData<PaginatedResponse<Task>>(
          // Find all cache entries whose key starts with ['tasks', 'list']
          { queryKey: taskKeys.lists() },
          // This function receives the old cached data and returns the new data.
          // It's called for EACH matching cache entry.
          (oldData) => {
            // If there's no cached data, return undefined (do nothing)
            if (!oldData) return oldData

            // Create a NEW object with the updated task.
            return {
              // Copy all properties: page, pageSize, totalCount, etc.
              ...oldData,
              // Replace the items array with a new array where we've swapped
              // out the old task for the updated task.
              items: oldData.items.map(task =>
                task.id === updatedTask.id ? updatedTask : task
              )
            }
          }
        )
      }
    }
  })
}

// Composable Function for updating only the status of a task.
//   This function is similar to useUpdateTask, but specifically for changing
//   just the status of a task (Todo → InProgress → Done).
//   It's simpler because we only need to check if status filtering is active.
export function useUpdateTaskStatus(currentFilter?: Ref<TaskFilterRequest>) {
  const queryClient = useQueryClient()

  return useMutation<Task, ApiError, { id: number; request: UpdateTaskStatusRequest }>({
    mutationFn: ({ id, request }) => taskApi.updateTaskStatus(id, request),
    onSuccess: (updatedTask) => {
      queryClient.setQueryData(taskKeys.detail(updatedTask.id), updatedTask)

      // For status changes, we only need to check ONE thing:
      // Is there a status filter active?

      // Only invalidate if status filter is active (task might no longer match filter)
      if (currentFilter?.value?.status) {
        // Status filter IS active - must refetch from server
        queryClient.invalidateQueries({ queryKey: taskKeys.lists() })
      } else {
        // Status filter is NOT active - safe to update cache directly.
        // Update in-place without refetching
        queryClient.setQueriesData<PaginatedResponse<Task>>(
          { queryKey: taskKeys.lists() },
          (oldData) => {
            if (!oldData) return oldData
            return {
              ...oldData,
              items: oldData.items.map(task =>
                task.id === updatedTask.id ? updatedTask : task
              )
            }
          }
        )
      }
    }
  })
}

// Composable Function for deleting a task.
export function useDeleteTask() {
  const queryClient = useQueryClient()

  return useMutation<void, ApiError, number>({
    mutationFn: (id) => taskApi.deleteTask(id),
    onSuccess: (_, id) => {
      queryClient.removeQueries({ queryKey: taskKeys.detail(id) })
      queryClient.invalidateQueries({ queryKey: taskKeys.lists() })
    }
  })
}
