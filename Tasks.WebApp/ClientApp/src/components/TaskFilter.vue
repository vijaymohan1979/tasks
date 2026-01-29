<script setup lang="ts">
  import type { TaskFilterRequest } from '@/types/task'
  import { useTaskFilter } from './taskFilter'

  interface Props {
    modelValue: TaskFilterRequest
  }

  const props = defineProps<Props>()

  const emit = defineEmits<{
    'update:modelValue': [value: TaskFilterRequest]
  }>()

  const {
    status,
    titleSearch,
    sortBy,
    sortDirection,
    applyFilters,
    clearFilters,
    TaskStatus
  } = useTaskFilter(props, emit)
</script>

<template>
  <div class="task-filter">
    <div class="filter-row">
      <div class="filter-group">
        <label for="search">Search</label>
        <input id="search"
               v-model="titleSearch"
               type="text"
               placeholder="Search by title..."
               @keyup.enter="applyFilters" />
      </div>

      <div class="filter-group">
        <label for="status">Status</label>
        <select id="status" v-model="status">
          <option value="">All</option>
          <option :value="TaskStatus.Todo">To Do</option>
          <option :value="TaskStatus.InProgress">In Progress</option>
          <option :value="TaskStatus.Done">Done</option>
        </select>
      </div>

      <div class="filter-group">
        <label for="sortBy">Sort By</label>
        <select id="sortBy" v-model="sortBy">
          <option value="sortorder">Sort Order</option>
          <option value="priority">Priority</option>
          <option value="duedate">Due Date</option>
          <option value="created">Created</option>
          <option value="updated">Updated</option>
          <option value="title">Title</option>
        </select>
      </div>

      <div class="filter-group">
        <label for="sortDir">Direction</label>
        <select id="sortDir" v-model="sortDirection">
          <option value="asc">Ascending</option>
          <option value="desc">Descending</option>
        </select>
      </div>
    </div>

    <div class="filter-actions">
      <button @click="clearFilters" class="btn-secondary">Clear</button>
      <button @click="applyFilters" class="btn-primary">Apply Filters</button>
    </div>
  </div>
</template>

<style scoped src="./taskFilter.css"></style>
