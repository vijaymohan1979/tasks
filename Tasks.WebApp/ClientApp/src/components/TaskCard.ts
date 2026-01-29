import type { Task } from '@/types/task'
import { TaskStatus } from '@/types/task'

export interface Props {
  task: Task
}

export interface Emits {
  edit: [task: Task]
  delete: [task: Task]
  statusChange: [task: Task, newStatus: TaskStatus]
}

export function formatDate(dateString: string | null): string {
  if (!dateString) return '-'
  // Server returns UTC dates without 'Z' suffix - normalize to ensure UTC parsing
  const normalizedUtc = dateString.endsWith('Z') ? dateString : dateString + 'Z'

  return new Date(normalizedUtc).toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  })
}

export function getStatusClass(status: TaskStatus): string {
  switch (status) {
    case TaskStatus.Todo:
      return 'status-todo'
    case TaskStatus.InProgress:
      return 'status-progress'
    case TaskStatus.Done:
      return 'status-done'
    default:
      return ''
  }
}

export function getStatusLabel(status: TaskStatus): string {
  switch (status) {
    case TaskStatus.Todo:
      return 'To Do'
    case TaskStatus.InProgress:
      return 'In Progress'
    case TaskStatus.Done:
      return 'Done'
    default:
      return status
  }
}

export { TaskStatus }
