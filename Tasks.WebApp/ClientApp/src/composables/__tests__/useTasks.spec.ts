import { describe, it, expect, vi, beforeEach } from 'vitest'
import { taskKeys } from '../useTasks'
import { TaskStatus, type TaskFilterRequest } from '@/types/task'

// Mock TanStack Query hooks
vi.mock('@tanstack/vue-query', () => ({
  useQuery: vi.fn(),
  useMutation: vi.fn(),
  useQueryClient: vi.fn(() => ({
    invalidateQueries: vi.fn(),
    setQueryData: vi.fn(),
    removeQueries: vi.fn()
  }))
}))

// Mock the taskApi service
vi.mock('@/services/taskApi', () => ({
  taskApi: {
    getTasks: vi.fn(),
    getTaskById: vi.fn(),
    createTask: vi.fn(),
    updateTask: vi.fn(),
    updateTaskStatus: vi.fn(),
    deleteTask: vi.fn()
  }
}))

describe('taskKeys', () => {
  describe('all', () => {
    it('should return base tasks key', () => {
      expect(taskKeys.all).toEqual(['tasks'])
    })
  })

  describe('lists', () => {
    it('should return key for all task lists', () => {
      expect(taskKeys.lists()).toEqual(['tasks', 'list'])
    })

    it('should return a new array instance on each call', () => {
      const first = taskKeys.lists()
      const second = taskKeys.lists()
      expect(first).not.toBe(second)
      expect(first).toEqual(second)
    })
  })

  describe('list', () => {
    it('should return key for specific filtered list', () => {
      const filter: TaskFilterRequest = {
        status: TaskStatus.Todo,
        page: 1
      }
      expect(taskKeys.list(filter)).toEqual(['tasks', 'list', filter])
    })

    it('should include filter object in key', () => {
      const filter: TaskFilterRequest = {
        titleSearch: 'test',
        sortBy: 'priority',
        sortDirection: 'desc'
      }
      const key = taskKeys.list(filter)
      expect(key[2]).toBe(filter)
    })

    it('should create different keys for different filters', () => {
      const filter1: TaskFilterRequest = { status: TaskStatus.Todo }
      const filter2: TaskFilterRequest = { status: TaskStatus.Done }

      const key1 = taskKeys.list(filter1)
      const key2 = taskKeys.list(filter2)

      expect(key1).not.toEqual(key2)
    })
  })

  describe('details', () => {
    it('should return key for all task details', () => {
      expect(taskKeys.details()).toEqual(['tasks', 'detail'])
    })

    it('should return a new array instance on each call', () => {
      const first = taskKeys.details()
      const second = taskKeys.details()
      expect(first).not.toBe(second)
      expect(first).toEqual(second)
    })
  })

  describe('detail', () => {
    it('should return key for specific task by id', () => {
      expect(taskKeys.detail(42)).toEqual(['tasks', 'detail', 42])
    })

    it('should work with different ids', () => {
      expect(taskKeys.detail(1)).toEqual(['tasks', 'detail', 1])
      expect(taskKeys.detail(100)).toEqual(['tasks', 'detail', 100])
      expect(taskKeys.detail(0)).toEqual(['tasks', 'detail', 0])
    })

    it('should create different keys for different ids', () => {
      const key1 = taskKeys.detail(1)
      const key2 = taskKeys.detail(2)

      expect(key1).not.toEqual(key2)
    })
  })

  describe('key hierarchy', () => {
    it('should have consistent hierarchical structure', () => {
      // All keys should start with 'tasks'
      expect(taskKeys.all[0]).toBe('tasks')
      expect(taskKeys.lists()[0]).toBe('tasks')
      expect(taskKeys.list({})[0]).toBe('tasks')
      expect(taskKeys.details()[0]).toBe('tasks')
      expect(taskKeys.detail(1)[0]).toBe('tasks')
    })

    it('should allow invalidation at different levels', () => {
      // Base key ['tasks'] should be prefix of all other keys
      const baseKey = taskKeys.all

      const listKey = taskKeys.list({ status: TaskStatus.Todo })
      const detailKey = taskKeys.detail(1)

      // List keys start with base
      expect(listKey.slice(0, 1)).toEqual(baseKey)

      // Detail keys start with base
      expect(detailKey.slice(0, 1)).toEqual(baseKey)
    })
  })
})
