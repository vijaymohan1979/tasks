import { ref, computed } from 'vue'
import type { Task, TaskFilterRequest, CreateTaskRequest, UpdateTaskRequest } from '@/types/task'
import { TaskStatus } from '@/types/task'
import {
  useTasks,
  useCreateTask,
  useUpdateTask,
  useUpdateTaskStatus,
  useDeleteTask
} from '@/composables/useTasks'

// Filter state
export const filter = ref<TaskFilterRequest>({
  page: 1,
  pageSize: 10,
  sortBy: 'sortorder',
  sortDirection: 'asc'
})

// Query and mutations
export function useAppState() {
  const { data, isLoading, isError, error, refetch } = useTasks(filter)
  const createMutation = useCreateTask()
  const updateMutation = useUpdateTask(filter)
  const updateStatusMutation = useUpdateTaskStatus(filter)
  const deleteMutation = useDeleteTask()

  // UI state
  const showForm = ref(false)
  const editingTask = ref<Task | null>(null)
  const errorMessage = ref<string | null>(null)

  // Computed
  const tasks = computed(() => data.value?.items || [])
  const pagination = computed(() => ({
    page: data.value?.page || 1,
    pageSize: data.value?.pageSize || 10,
    totalPages: data.value?.totalPages || 0,
    totalCount: data.value?.totalCount || 0,
    hasNext: data.value?.hasNextPage || false,
    hasPrev: data.value?.hasPreviousPage || false
  }))

  // Handlers
  function openCreateForm() {
    editingTask.value = null
    showForm.value = true
    errorMessage.value = null
  }

  function openEditForm(task: Task) {
    editingTask.value = task
    showForm.value = true
    errorMessage.value = null
  }

  function closeForm() {
    showForm.value = false
    editingTask.value = null
    errorMessage.value = null
  }

  async function handleSubmit(formData: CreateTaskRequest | UpdateTaskRequest) {
    try {
      errorMessage.value = null
      if ('id' in formData) {
        await updateMutation.mutateAsync(formData)
      } else {
        await createMutation.mutateAsync(formData)
      }
      closeForm()
    } catch (err: unknown) {
      const apiErr = err as { detail?: string }
      errorMessage.value = apiErr.detail || 'An error occurred'
    }
  }

  async function handleStatusChange(task: Task, newStatus: TaskStatus) {
    try {
      errorMessage.value = null
      await updateStatusMutation.mutateAsync({
        id: task.id,
        request: {
          rowVersion: task.rowVersion,
          status: newStatus
        }
      })
    } catch (err: unknown) {
      const apiErr = err as { detail?: string }
      errorMessage.value = apiErr.detail || 'Failed to update status'
      refetch()
    }
  }

  async function handleDelete(task: Task) {
    if (!confirm(`Are you sure you want to delete "${task.title}"?`)) return

    try {
      errorMessage.value = null
      await deleteMutation.mutateAsync(task.id)
    } catch (err: unknown) {
      const apiErr = err as { detail?: string }
      errorMessage.value = apiErr.detail || 'Failed to delete task'
    }
  }

  function changePage(newPage: number) {
    filter.value = { ...filter.value, page: newPage }
  }

  return {
    // State
    filter,
    showForm,
    editingTask,
    errorMessage,
    isLoading,
    isError,
    error,
    // Computed
    tasks,
    pagination,
    // Mutations
    createMutation,
    updateMutation,
    // Handlers
    openCreateForm,
    openEditForm,
    closeForm,
    handleSubmit,
    handleStatusChange,
    handleDelete,
    changePage,
    refetch
  }
}
