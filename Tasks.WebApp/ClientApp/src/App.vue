<script setup lang="ts">
  import { useAppState } from './App'
  import TaskCard from '@/components/TaskCard.vue'
  import TaskForm from '@/components/TaskForm.vue'
  import TaskFilter from '@/components/TaskFilter.vue'
  import './App.css'

  const {
    filter,
    showForm,
    editingTask,
    errorMessage,
    isLoading,
    isError,
    error,
    tasks,
    pagination,
    createMutation,
    updateMutation,
    openCreateForm,
    openEditForm,
    closeForm,
    handleSubmit,
    handleStatusChange,
    handleDelete,
    changePage,
    refetch
  } = useAppState()
</script>

<template>
  <div class="app">
    <header class="header">
      <h1>📋 Task Manager</h1>
      <button @click="openCreateForm" class="btn-add">+ Add Task</button>
    </header>

    <!-- Error Alert (only show when form is closed) -->
    <div v-if="errorMessage && !showForm" class="alert alert-error">
      {{ errorMessage }}
      <button @click="errorMessage = null" class="alert-close">×</button>
    </div>

    <!-- Filter -->
    <TaskFilter v-model="filter" />

    <!-- Loading State -->
    <div v-if="isLoading" class="state-loading">
      <div class="spinner"></div>
      <p>Loading tasks...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="isError" class="state-error">
      <p>❌ Failed to load tasks</p>
      <p class="error-detail">{{ (error as Error)?.message }}</p>
      <button @click="() => refetch()" class="btn-retry">Retry</button>
    </div>

    <!-- Empty State -->
    <div v-else-if="tasks.length === 0" class="state-empty">
      <p>📭 No tasks found</p>
      <p class="empty-hint">Create your first task to get started!</p>
    </div>

    <!-- Content with Pagination (above and below) -->
    <template v-else>
      <!-- Pagination Top -->
      <div v-if="pagination.totalPages > 1" class="pagination pagination-top">
        <button :disabled="!pagination.hasPrev"
                @click="changePage(pagination.page - 1)"
                class="btn-page">
          ← Previous
        </button>
        <span class="page-info">
          Page {{ pagination.page }} of {{ pagination.totalPages }} ({{ pagination.totalCount }} tasks)
        </span>
        <button :disabled="!pagination.hasNext"
                @click="changePage(pagination.page + 1)"
                class="btn-page">
          Next →
        </button>
      </div>

      <!-- Task List -->
      <div class="task-list">
        <TaskCard v-for="task in tasks"
                  :key="task.id"
                  :task="task"
                  @edit="openEditForm"
                  @delete="handleDelete"
                  @status-change="handleStatusChange" />
      </div>

      <!-- Pagination Bottom -->
      <div v-if="pagination.totalPages > 1" class="pagination">
        <button :disabled="!pagination.hasPrev"
                @click="changePage(pagination.page - 1)"
                class="btn-page">
          ← Previous
        </button>
        <span class="page-info">
          Page {{ pagination.page }} of {{ pagination.totalPages }} ({{ pagination.totalCount }} tasks)
        </span>
        <button :disabled="!pagination.hasNext"
                @click="changePage(pagination.page + 1)"
                class="btn-page">
          Next →
        </button>
      </div>
    </template>

    <!-- Modal Form -->
    <Teleport to="body">
      <div v-if="showForm" class="modal-overlay" @click.self="closeForm">
        <TaskForm :task="editingTask"
                  :is-submitting="createMutation.isPending.value || updateMutation.isPending.value"
                  :server-error="errorMessage"
                  @submit="handleSubmit"
                  @cancel="closeForm" />
      </div>
    </Teleport>
  </div>
</template>
