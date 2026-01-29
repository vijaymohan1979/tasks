import { ref, watch } from 'vue'
import type { TaskFilterRequest } from '@/types/task'
import { TaskStatus } from '@/types/task'

export function useTaskFilter(
  props: { modelValue: TaskFilterRequest },
  emit: (event: 'update:modelValue', value: TaskFilterRequest) => void
) {
  // Local state
  const status = ref<TaskStatus | ''>('')
  const titleSearch = ref('')
  const sortBy = ref<TaskFilterRequest['sortBy']>('sortorder')
  const sortDirection = ref<TaskFilterRequest['sortDirection']>('asc')

  // Sync with prop
  watch(
    () => props.modelValue,
    (newValue) => {
      status.value = newValue.status || ''
      titleSearch.value = newValue.titleSearch || ''
      sortBy.value = newValue.sortBy || 'sortorder'
      sortDirection.value = newValue.sortDirection || 'asc'
    },
    { immediate: true }
  )

  function applyFilters() {
    emit('update:modelValue', {
      ...props.modelValue,
      status: status.value || undefined,
      titleSearch: titleSearch.value || undefined,
      sortBy: sortBy.value,
      sortDirection: sortDirection.value,
      page: 1 // Reset to first page on filter change
    })
  }

  function clearFilters() {
    status.value = ''
    titleSearch.value = ''
    sortBy.value = 'sortorder'
    sortDirection.value = 'asc'
    applyFilters()
  }

  return {
    status,
    titleSearch,
    sortBy,
    sortDirection,
    applyFilters,
    clearFilters,
    TaskStatus
  }
}
