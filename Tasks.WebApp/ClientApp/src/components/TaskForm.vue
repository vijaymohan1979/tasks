<script setup lang="ts">
  import type { Task, CreateTaskRequest, UpdateTaskRequest } from '@/types/task'
  import { useTaskForm } from './taskForm'

  interface Props {
    task?: Task | null
    isSubmitting?: boolean
    serverError?: string | null
  }

  const props = withDefaults(defineProps<Props>(), {
    task: null,
    isSubmitting: false,
    serverError: null
  })

  const emit = defineEmits<{
    submit: [data: CreateTaskRequest | UpdateTaskRequest]
    cancel: []
  }>()

  const {
    title,
    description,
    priority,
    dueDateUtc,
    sortOrder,
    status,
    errors,
    serverError,
    isEditing,
    hasChanges,
    TITLE_MAX_LENGTH,
    DESCRIPTION_MAX_LENGTH,
    PRIORITY_MIN,
    PRIORITY_MAX,
    handleSubmit,
    TaskStatus
  } = useTaskForm(props, emit)
</script>

<template>
  <form @submit.prevent="handleSubmit" class="task-form">
    <!-- Template remains unchanged -->
    <h2>{{ isEditing ? 'Edit Task' : 'Create Task' }}</h2>

    <!-- Server Error Alert -->
    <div v-if="serverError" class="alert alert-error">
      {{ serverError }}
    </div>

    <div class="form-group">
      <label for="title">Title *</label>
      <input id="title" v-model="title" type="text" :maxlength="TITLE_MAX_LENGTH" :disabled="isSubmitting"
             :class="{ error: errors.title }" placeholder="Enter task title" />
      <span v-if="errors.title" class="error-message">{{ errors.title }}</span>
      <span class="char-count">{{ title.length }}/{{ TITLE_MAX_LENGTH }}</span>
    </div>

    <div class="form-group">
      <label for="description">Description</label>
      <textarea id="description" v-model="description" :maxlength="DESCRIPTION_MAX_LENGTH" :disabled="isSubmitting"
                :class="{ error: errors.description }" placeholder="Enter task description (optional)" rows="4"></textarea>
      <span v-if="errors.description" class="error-message">{{ errors.description }}</span>
      <span class="char-count">{{ description.length }}/{{ DESCRIPTION_MAX_LENGTH }}</span>
    </div>

    <div class="form-row">
      <div class="form-group">
        <label for="priority">Priority</label>
        <input id="priority" v-model.number="priority" type="number" :min="PRIORITY_MIN" :max="PRIORITY_MAX"
               :disabled="isSubmitting" :class="{ error: errors.priority }" />
        <span v-if="errors.priority" class="error-message">{{ errors.priority }}</span>
      </div>

      <div class="form-group">
        <label for="sortOrder">Sort Order</label>
        <input id="sortOrder" v-model.number="sortOrder" type="number" min="0" :disabled="isSubmitting"
               :class="{ error: errors.sortOrder }" />
        <span v-if="errors.sortOrder" class="error-message">{{ errors.sortOrder }}</span>
      </div>
    </div>

    <div class="form-group">
      <label for="dueDate">Due Date</label>
      <input id="dueDate" v-model="dueDateUtc" type="datetime-local" :disabled="isSubmitting"
             :class="{ error: errors.dueDate }" />
      <span v-if="errors.dueDate" class="error-message">{{ errors.dueDate }}</span>
    </div>

    <div v-if="isEditing" class="form-group">
      <label for="status">Status</label>
      <select id="status" v-model="status" :disabled="isSubmitting">
        <option :value="TaskStatus.Todo">Todo</option>
        <option :value="TaskStatus.InProgress">In Progress</option>
        <option :value="TaskStatus.Done">Done</option>
      </select>
    </div>

    <div class="form-actions">
      <button type="button" @click="emit('cancel')" :disabled="isSubmitting" class="btn-secondary">
        Cancel
      </button>
      <button type="submit" :disabled="isSubmitting || !hasChanges" class="btn-primary">
        {{ isSubmitting ? 'Saving...' : isEditing ? 'Update Task' : 'Create Task' }}
      </button>
    </div>
  </form>
</template>

<style scoped src="./taskForm.css"></style>
