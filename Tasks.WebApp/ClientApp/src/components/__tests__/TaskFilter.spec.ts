import { describe, it, expect, vi, beforeEach } from 'vitest'
import { useTaskFilter } from '../TaskFilter'
import { TaskStatus, type TaskFilterRequest } from '@/types/task'

describe('useTaskFilter', () => {
  let emit: ReturnType<typeof vi.fn>
  const defaultFilter: TaskFilterRequest = {
    status: undefined,
    titleSearch: undefined,
    sortBy: 'sortorder',
    sortDirection: 'asc',
    page: 1,
    pageSize: 10
  }

  beforeEach(() => {
    emit = vi.fn()
  })

  describe('initialization', () => {
    it('should initialize with default values when filter is empty', () => {
      const props = { modelValue: defaultFilter }
      const filter = useTaskFilter(props, emit)

      expect(filter.status.value).toBe('')
      expect(filter.titleSearch.value).toBe('')
      expect(filter.sortBy.value).toBe('sortorder')
      expect(filter.sortDirection.value).toBe('asc')
    })

    it('should initialize with provided filter values', () => {
      const props = {
        modelValue: {
          ...defaultFilter,
          status: TaskStatus.Done,
          titleSearch: 'search term',
          sortBy: 'priority' as const,
          sortDirection: 'desc' as const
        }
      }
      const filter = useTaskFilter(props, emit)

      expect(filter.status.value).toBe(TaskStatus.Done)
      expect(filter.titleSearch.value).toBe('search term')
      expect(filter.sortBy.value).toBe('priority')
      expect(filter.sortDirection.value).toBe('desc')
    })
  })

  describe('applyFilters', () => {
    it('should emit updated filter with status', () => {
      const props = { modelValue: defaultFilter }
      const filter = useTaskFilter(props, emit)

      filter.status.value = TaskStatus.InProgress
      filter.applyFilters()

      expect(emit).toHaveBeenCalledWith('update:modelValue', expect.objectContaining({
        status: TaskStatus.InProgress,
        page: 1
      }))
    })

    it('should emit updated filter with title search', () => {
      const props = { modelValue: defaultFilter }
      const filter = useTaskFilter(props, emit)

      filter.titleSearch.value = 'test search'
      filter.applyFilters()

      expect(emit).toHaveBeenCalledWith('update:modelValue', expect.objectContaining({
        titleSearch: 'test search',
        page: 1
      }))
    })

    it('should emit updated filter with sort options', () => {
      const props = { modelValue: defaultFilter }
      const filter = useTaskFilter(props, emit)

      filter.sortBy.value = 'duedate'
      filter.sortDirection.value = 'desc'
      filter.applyFilters()

      expect(emit).toHaveBeenCalledWith('update:modelValue', expect.objectContaining({
        sortBy: 'duedate',
        sortDirection: 'desc',
        page: 1
      }))
    })

    it('should reset page to 1 when applying filters', () => {
      const props = {
        modelValue: {
          ...defaultFilter,
          page: 5
        }
      }
      const filter = useTaskFilter(props, emit)

      filter.applyFilters()

      expect(emit).toHaveBeenCalledWith('update:modelValue', expect.objectContaining({
        page: 1
      }))
    })

    it('should emit undefined for empty status', () => {
      const props = { modelValue: defaultFilter }
      const filter = useTaskFilter(props, emit)

      filter.status.value = ''
      filter.applyFilters()

      expect(emit).toHaveBeenCalledWith('update:modelValue', expect.objectContaining({
        status: undefined
      }))
    })

    it('should emit undefined for empty title search', () => {
      const props = { modelValue: defaultFilter }
      const filter = useTaskFilter(props, emit)

      filter.titleSearch.value = ''
      filter.applyFilters()

      expect(emit).toHaveBeenCalledWith('update:modelValue', expect.objectContaining({
        titleSearch: undefined
      }))
    })
  })

  describe('clearFilters', () => {
    it('should reset all filter values to defaults', () => {
      const props = {
        modelValue: {
          ...defaultFilter,
          status: TaskStatus.Done,
          titleSearch: 'search term',
          sortBy: 'priority' as const,
          sortDirection: 'desc' as const
        }
      }
      const filter = useTaskFilter(props, emit)

      filter.clearFilters()

      expect(filter.status.value).toBe('')
      expect(filter.titleSearch.value).toBe('')
      expect(filter.sortBy.value).toBe('sortorder')
      expect(filter.sortDirection.value).toBe('asc')
    })

    it('should emit cleared filter values', () => {
      const props = {
        modelValue: {
          ...defaultFilter,
          status: TaskStatus.Done,
          titleSearch: 'search term'
        }
      }
      const filter = useTaskFilter(props, emit)

      filter.clearFilters()

      expect(emit).toHaveBeenCalledWith('update:modelValue', expect.objectContaining({
        status: undefined,
        titleSearch: undefined,
        sortBy: 'sortorder',
        sortDirection: 'asc',
        page: 1
      }))
    })
  })

  describe('TaskStatus enum exposure', () => {
    it('should expose TaskStatus enum for template usage', () => {
      const props = { modelValue: defaultFilter }
      const filter = useTaskFilter(props, emit)

      expect(filter.TaskStatus).toBe(TaskStatus)
      expect(filter.TaskStatus.Todo).toBe('Todo')
      expect(filter.TaskStatus.InProgress).toBe('InProgress')
      expect(filter.TaskStatus.Done).toBe('Done')
    })
  })
})
