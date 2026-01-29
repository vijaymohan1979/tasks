// 'ref', 'computed', and 'watch' are Vue "Composition API" functions.
// - ref: Creates a "reactive" variable. When its value changes, Vue automatically
//        updates any part of the UI that displays it. Think of it as a smart variable
//        that notifies the screen when it changes.
// - computed: Creates a value that automatically recalculates when its dependencies change.
//             Like an Excel formula that updates when referenced cells change.
// - watch: Monitors a value and runs code whenever that value changes.
//          Like setting up an alarm that goes off when something specific happens.
import { ref, computed, watch } from 'vue'

// 'type' imports bring in TypeScript type definitions (not actual code).
import type { Task, CreateTaskRequest, UpdateTaskRequest } from '@/types/task'
import { TaskStatus } from '@/types/task'

// This interface describes what "props" (properties) this form component can receive.
interface Props {
  //  Optional 'task' prop for edit mode; null/undefined means create mode.
  task?: Task | null
  // Optional flag to indicate if the form is currently submitting.
  isSubmitting?: boolean
  // Optional server error message to display in the form.
  serverError?: string | null
}

// MAIN FUNCTION - THE "COMPOSABLE"

// This is a Vue "composable" - a function that encapsulates reusable reactive logic.
//
// Parameters:
// - props: The input data passed to this form (like the task being edited)
// - emit: A function to send events back to the parent component.
//         Events are how child components communicate with parents in Vue.
//         The type annotation describes what events can be emitted and their data.
export function useTaskForm(props: Props, emit: (event: 'submit' | 'cancel', data?: CreateTaskRequest | UpdateTaskRequest) => void) {

  // FORM STATE

  const title = ref('')
  const description = ref('')
  const priority = ref(0)
  const dueDateUtc = ref<string | null>(null)
  const sortOrder = ref(0)
  const status = ref<TaskStatus>(TaskStatus.Todo)

  // VALIDATION STATE

  const errors = ref<Record<string, string>>({})

  // CONSTANTS FOR VALIDATION

  // Validation rules matching backend
  const TITLE_MAX_LENGTH = 200
  const DESCRIPTION_MAX_LENGTH = 2000
  const PRIORITY_MIN = -100
  const PRIORITY_MAX = 100

  // COMPUTED PROPERTY

  // computed() creates a value that automatically recalculates.
  // !!props.task is JavaScript to convert any value to true/false:
  // - First ! converts the value to boolean and inverts it
  // - Second ! inverts it back
  // - Result: null/undefined becomes false, any object becomes true
  //
  // So isEditing is true when we have an existing task (edit mode),
  // and false when creating a new task.
  const isEditing = computed(() => !!props.task)

  // Converts a UTC date string to local datetime-local input format (YYYY-MM-DDTHH:mm)
  function toLocalDateTimeString(utcDateString: string): string {
    // Server returns UTC dates without 'Z' suffix - normalize to ensure UTC parsing
    const normalizedUtc = utcDateString.endsWith('Z') ? utcDateString : utcDateString + 'Z'
    const date = new Date(normalizedUtc)
    const year = date.getFullYear()

    // .padStart(2, '0') is a JavaScript string method that ensures the string is at least 2 characters
    // long by adding '0' characters to the beginning if needed.
    //
    // This ensures months like January (1) become '01' instead of '1', which is required for the
    // datetime - local input format(YYYY-MM-DDTHH:mm)
    const month = String(date.getMonth() + 1).padStart(2, '0')
    const day = String(date.getDate()).padStart(2, '0')
    const hours = String(date.getHours()).padStart(2, '0')
    const minutes = String(date.getMinutes()).padStart(2, '0')
    return `${year}-${month}-${day}T${hours}:${minutes}`
  }

  // Helper to normalize ISO date strings to minute precision for comparison
  function normalizeToMinute(isoString: string | null): string | null {
    if (!isoString) return null
    return isoString.substring(0, 16)
  }

  // Tracks whether the form has unsaved changes (for edit mode)
  // In create mode, check if required fields are filled
  const hasChanges = computed(() => {
    if (!isEditing.value) {
      // For create mode, enable save if title is provided
      return title.value.trim().length > 0
    }

    if (!props.task) return false

    // Check each field for changes
    if (title.value !== props.task.title) return true
    if ((description.value || null) !== props.task.description) return true
    if (priority.value !== props.task.priority) return true
    if (sortOrder.value !== props.task.sortOrder) return true
    if (status.value !== props.task.status) return true

    // Compare due dates at minute precision
    const newDueDateUtc = dueDateUtc.value ? new Date(dueDateUtc.value).toISOString() : null
    const existingDueDateUtc = props.task.dueDateUtc
      ? new Date(props.task.dueDateUtc.endsWith('Z') ? props.task.dueDateUtc : props.task.dueDateUtc + 'Z').toISOString()
      : null

    if (normalizeToMinute(newDueDateUtc) !== normalizeToMinute(existingDueDateUtc)) return true

    return false
  })

  // WATCHER

  // watch() monitors a value and runs code when it changes.
  // Watch for task changes (edit mode)
  //
  // Arg 1: () => props.task
  //   This is a "getter function" that returns the value to watch.
  //   We wrap props.task in a function because Vue needs to track it reactively.
  //
  // Arg 2: (newTask) => { ... }
  //   This is the "callback function" that runs when the watched value changes.
  //   'newTask' is the new value after the change.
  //
  // Arg 3: { immediate: true }
  //   This is an options object. 'immediate: true' means run the callback
  //   immediately when the watcher is created, not just on future changes.
  //   This ensures the form is populated if we start with an existing task.
  watch(
    () => props.task,
    (newTask) => {
      // If we have a task, populate the form fields with its values.
      if (newTask) {
        title.value = newTask.title
        // The || '' is the "logical OR" operator used as a fallback.
        // If newTask.description is null/undefined, use empty string instead.
        description.value = newTask.description || ''
        priority.value = newTask.priority
        dueDateUtc.value = newTask.dueDateUtc
          // Convert UTC to local datetime-local format
          ? toLocalDateTimeString(newTask.dueDateUtc)
          : null
        sortOrder.value = newTask.sortOrder
        status.value = newTask.status
      } else {
        // If no task provided (creating new), reset the form to defaults.
        resetForm()
      }
    },
    { immediate: true }
  )

  // VALIDATION FUNCTION

  function validate(): boolean {
    // Clear any previous errors
    errors.value = {}

    // Title Validation

    if (!title.value.trim()) {
      // If title is empty/whitespace, add an error message.
      errors.value.title = 'Title is required.'
    } else if (title.value.length > TITLE_MAX_LENGTH) {
      errors.value.title = `Title must be between 1 and ${TITLE_MAX_LENGTH} characters.`
    }

    // Description Validation

    if (description.value && description.value.length > DESCRIPTION_MAX_LENGTH) {
      errors.value.description = `Description cannot exceed ${DESCRIPTION_MAX_LENGTH} characters.`
    }

    // Priority Validation

    if (priority.value < PRIORITY_MIN || priority.value > PRIORITY_MAX) {
      errors.value.priority = `Priority must be between ${PRIORITY_MIN} and ${PRIORITY_MAX}.`
    }

    // Sort Order Validation

    if (sortOrder.value < 0) {
      errors.value.sortOrder = 'Sort order must be a non-negative integer.'
    }

    // Due Date Validation

    if (dueDateUtc.value) {
      const selectedDate = new Date(dueDateUtc.value)
      if (selectedDate < new Date()) {
        errors.value.dueDate = 'Due date cannot be in the past.'
      }
    }

    // Return Validation Result

    // Object.keys() returns an array of all property names in an object.
    // Example: Object.keys({a: 1, b: 2}) returns ['a', 'b']
    // 
    // If there are 0 errors, validation passed (return true).
    // If there are any errors, validation failed (return false).
    return Object.keys(errors.value).length === 0
  }

  // FORM SUBMISSION HANDLER

  // This function runs when the user clicks the submit button.
  function handleSubmit() {
    if (!validate()) return

    if (isEditing.value && props.task) {
      // Start with required fields
      const updateRequest: UpdateTaskRequest = {
        id: props.task.id,
        rowVersion: props.task.rowVersion
      }

      // Only include fields that have changed
      if (title.value !== props.task.title) {
        updateRequest.title = title.value
      }
      if ((description.value || null) !== props.task.description) {
        updateRequest.description = description.value || null
      }
      if (priority.value !== props.task.priority) {
        updateRequest.priority = priority.value
      }
      if (sortOrder.value !== props.task.sortOrder) {
        updateRequest.sortOrder = sortOrder.value
      }
      if (status.value !== props.task.status) {
        updateRequest.status = status.value
      }

      // Handle due date comparison - normalize formats AND truncate to minute precision
      const newDueDateUtc = dueDateUtc.value ? new Date(dueDateUtc.value).toISOString() : null
      const existingDueDateUtc = props.task.dueDateUtc
        ? new Date(props.task.dueDateUtc.endsWith('Z') ? props.task.dueDateUtc : props.task.dueDateUtc + 'Z').toISOString()
        : null

      if (normalizeToMinute(newDueDateUtc) !== normalizeToMinute(existingDueDateUtc)) {
        updateRequest.dueDateUtc = newDueDateUtc
      }

      // Guard: Don't submit if no fields actually changed (defensive check)
      // The button should be disabled, but handle this case anyway because
      // this ensures that even if handleSubmit() is called directly (e.g., in tests or via keyboard),
      // it won't emit a submit event when there are no changes.
      if (Object.keys(updateRequest).length <= 2) {
        return // Only id and rowVersion - no actual changes
      }

      emit('submit', updateRequest)
    } else {
      // Creating a new task (not editing).
      // (no id, rowVersion, or status needed for new tasks).
      const createRequest: CreateTaskRequest = {
        title: title.value,
        description: description.value || null,
        priority: priority.value,
        dueDateUtc: dueDateUtc.value ? new Date(dueDateUtc.value).toISOString() : null,
        sortOrder: sortOrder.value
      }

      emit('submit', createRequest)
    }
  }

  // RESET FUNCTION

  // Resets all form fields to their default/empty values.
  // Called when switching from edit mode to create mode,
  // or when the user wants to clear the form.
  function resetForm() {
    title.value = ''
    description.value = ''
    priority.value = 0
    dueDateUtc.value = null
    sortOrder.value = 0
    status.value = TaskStatus.Todo
    errors.value = {} // Clear any validation errors too
  }

  // RETURN STATEMENT

  // The return statement specifies what this composable "exposes" to components.
  // Any component using useTaskForm() will receive this object.
  //
  // This uses the JavaScript shorthand property syntax:
  // { title } is the same as { title: title }
  // When property name matches variable name, you can omit the colon and value.
  return {
    // Reactive form field values (refs)
    title,
    description,
    priority,
    dueDateUtc,
    sortOrder,
    status,

    // Validation errors object
    errors,

    // Server error passed from parent
    serverError: computed(() => props.serverError),

    // Computed property indicating edit vs create mode
    isEditing,

    // Computed property indicating if form has unsaved changes
    hasChanges,

    // Constants for validation (so the template can display limits to users)
    TITLE_MAX_LENGTH,
    DESCRIPTION_MAX_LENGTH,
    PRIORITY_MIN,
    PRIORITY_MAX,
    handleSubmit,
    TaskStatus
  }
}
