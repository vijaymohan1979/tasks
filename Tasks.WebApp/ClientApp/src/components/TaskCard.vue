<script setup lang="ts">
  import type { Task } from '@/types/task'
  import { type Props, type Emits, formatDate, getStatusClass, getStatusLabel, TaskStatus } from './taskCard'

  defineProps<Props>()

  const emit = defineEmits<Emits>()
</script>

<template>
  <div class="task-card" :class="getStatusClass(task.status)">
    <div class="task-header">
      <h3 class="task-title">{{ task.title }}</h3>
      <span class="task-priority" :title="`Priority: ${task.priority}`">
        P{{ task.priority }}
      </span>
    </div>

    <p v-if="task.description" class="task-description">{{ task.description }}</p>

    <div class="task-meta">
      <span class="task-status" :class="getStatusClass(task.status)">
        {{ getStatusLabel(task.status) }}
      </span>
      <span v-if="task.dueDateUtc" class="task-due">
        Due: {{ formatDate(task.dueDateUtc) }}
      </span>
    </div>

    <div class="task-actions">
      <select :value="task.status"
              @change="emit('statusChange', task, ($event.target as HTMLSelectElement).value as TaskStatus)"
              class="status-select">
        <option :value="TaskStatus.Todo">To Do</option>
        <option :value="TaskStatus.InProgress">In Progress</option>
        <option :value="TaskStatus.Done">Done</option>
      </select>

      <button @click="emit('edit', task)" class="btn-icon" title="Edit task">✏️</button>
      <button @click="emit('delete', task)" class="btn-icon btn-delete" title="Delete task">
        🗑️
      </button>
    </div>
  </div>
</template>

<style scoped src="./taskCard.css"></style>
