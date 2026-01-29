// beforeEach - runs setup code before every test
import { describe, it, expect, vi, beforeEach } from 'vitest'
// Import the code being tested
import { useTaskForm } from '../TaskForm'
// Import type definitions
import { TaskStatus, type Task } from '@/types/task'

// Create a test group named "useTaskForm" - all tests inside { } relate to this function
describe('useTaskForm', () => {
  // The variable emit holds a fake function.
  // 'emit' is how child components send messages to parent components
  //
  // When form validation PASSES, the code internally does something like:
  // emit('submit', { title: 'My Task', ... })
  // emit(<event name>, <event data/ payload>)
  //
  // We are thereby mocking this.
  let emit: ReturnType<typeof vi.fn>

  beforeEach(() => {
    // Before each test runs, create a fresh fake emit function (for test isolation)
    // that records all calls. 
    emit = vi.fn()
  })

  // A nested group specifically for validation-related tests
  describe('validation', () => {
    // Test Case 1
    it('should fail validation when title is empty', () => {
      // Create test input.
      //  props are parameters passed to components
      //  task: null means we're creating a NEW task
      const props = { task: null }
      // Calls the function we're testing, passing our fake props and emit
      const form = useTaskForm(props, emit)

      // Set the title field to invalid, empty string
      form.title.value = ''
      // Simulate clicking the "Submit" button
      form.handleSubmit()

      // We expect an error message to appear for the title field
      expect(form.errors.value.title).toBe('Title is required.')

      // We expect that no submit event was emitted due to validation failure
      expect(emit).not.toHaveBeenCalled()
    })

    it('should fail validation when title is only whitespace', () => {
      const props = { task: null }
      const form = useTaskForm(props, emit)

      // Whitespace validation
      form.title.value = '   '
      form.handleSubmit()

      expect(form.errors.value.title).toBe('Title is required.')
      expect(emit).not.toHaveBeenCalled()
    })

    it('should fail validation when title exceeds max length', () => {
      const props = { task: null }
      const form = useTaskForm(props, emit)

      // Length Validation
      //  Create a string of 201 "a" characters (thereby exceeding the 200-character limit)
      form.title.value = 'a'.repeat(201)
      form.handleSubmit()

      expect(form.errors.value.title).toBe('Title must be between 1 and 200 characters.')
      expect(emit).not.toHaveBeenCalled()
    })

    it('should fail validation when description exceeds max length', () => {
      const props = { task: null }
      const form = useTaskForm(props, emit)

      form.title.value = 'Valid Title'
      // Length Validation
      form.description.value = 'a'.repeat(2001)
      form.handleSubmit()

      expect(form.errors.value.description).toBe('Description cannot exceed 2000 characters.')
      expect(emit).not.toHaveBeenCalled()
    })

    it('should fail validation when priority is below minimum', () => {
      const props = { task: null }
      const form = useTaskForm(props, emit)

      form.title.value = 'Valid Title'
      form.priority.value = -101
      form.handleSubmit()

      expect(form.errors.value.priority).toBe('Priority must be between -100 and 100.')
      expect(emit).not.toHaveBeenCalled()
    })

    it('should fail validation when priority exceeds maximum', () => {
      const props = { task: null }
      const form = useTaskForm(props, emit)

      form.title.value = 'Valid Title'
      form.priority.value = 101
      form.handleSubmit()

      expect(form.errors.value.priority).toBe('Priority must be between -100 and 100.')
      expect(emit).not.toHaveBeenCalled()
    })

    it('should fail validation when sort order is negative', () => {
      const props = { task: null }
      const form = useTaskForm(props, emit)

      form.title.value = 'Valid Title'
      form.sortOrder.value = -1
      form.handleSubmit()

      expect(form.errors.value.sortOrder).toBe('Sort order must be a non-negative integer.')
      expect(emit).not.toHaveBeenCalled()
    })

    it('should pass validation with valid data', () => {
      const props = { task: null }
      const form = useTaskForm(props, emit)

      form.title.value = 'Valid Title'
      form.description.value = 'Valid description'
      form.priority.value = 50
      form.sortOrder.value = 1
      form.handleSubmit()

      // Object.keys() gets all property names from an object as an array
      // We expect zero errors (validation passed)
      expect(Object.keys(form.errors.value).length).toBe(0)

      // We expect the submit event to have been emitted with correct data
      expect(emit).toHaveBeenCalledWith('submit', expect.objectContaining({
        title: 'Valid Title',
        description: 'Valid description',
        priority: 50,
        sortOrder: 1
      }))
    })
  })

  describe('create mode', () => {
    it('should be in create mode when no task is provided', () => {
      const props = { task: null }
      const form = useTaskForm(props, emit)

      expect(form.isEditing.value).toBe(false)
    })

    it('should emit create request with correct data', () => {
      const props = { task: null }
      const form = useTaskForm(props, emit)

      form.title.value = 'New Task'
      form.description.value = 'Task description'
      form.priority.value = 10
      form.sortOrder.value = 5
      form.handleSubmit()

      expect(emit).toHaveBeenCalledWith('submit', {
        title: 'New Task',
        description: 'Task description',
        priority: 10,
        dueDateUtc: null,
        sortOrder: 5
      })
    })

    it('should emit null for empty description', () => {
      const props = { task: null }
      const form = useTaskForm(props, emit)

      form.title.value = 'New Task'
      form.description.value = ''
      form.handleSubmit()

      expect(emit).toHaveBeenCalledWith('submit', expect.objectContaining({
        description: null
      }))
    })
  })

  const existingTask: Task = {
    id: 1,
    title: 'Existing Task',
    description: 'Existing description',
    status: TaskStatus.InProgress,
    priority: 25,
    dueDateUtc: null,
    createdAtUtc: '2024-01-01T00:00:00Z',
    updatedAtUtc: '2024-01-02T00:00:00Z',
    completedAtUtc: null,
    sortOrder: 10,
    rowVersion: 5
  }

  describe('edit mode', () => {

    it('should be in edit mode when task is provided', () => {
      const props = { task: existingTask }
      const form = useTaskForm(props, emit)

      expect(form.isEditing.value).toBe(true)
    })

    it('should populate form fields with task data', () => {
      const props = { task: existingTask }
      const form = useTaskForm(props, emit)

      expect(form.title.value).toBe('Existing Task')
      expect(form.description.value).toBe('Existing description')
      expect(form.priority.value).toBe(25)
      expect(form.sortOrder.value).toBe(10)
      expect(form.status.value).toBe(TaskStatus.InProgress)
    })

    it('should emit update request with only changed fields', () => {
      const props = { task: existingTask }
      const form = useTaskForm(props, emit)

      form.title.value = 'Updated Task'
      form.handleSubmit()

      // Should only include id, rowVersion, and the changed field (title)
      expect(emit).toHaveBeenCalledWith('submit', {
        id: 1,
        rowVersion: 5,
        title: 'Updated Task'
      })
    })

    it('should include all changed fields in update request', () => {
      const props = { task: existingTask }
      const form = useTaskForm(props, emit)

      form.title.value = 'Updated Task'
      form.priority.value = 99
      form.status.value = TaskStatus.Done
      form.handleSubmit()

      expect(emit).toHaveBeenCalledWith('submit', {
        id: 1,
        rowVersion: 5,
        title: 'Updated Task',
        priority: 99,
        status: TaskStatus.Done
      })
    })
  })

  describe('serverError', () => {
    it('should expose server error from props', () => {
      const props = { task: null, serverError: 'Server validation failed' }
      const form = useTaskForm(props, emit)

      expect(form.serverError.value).toBe('Server validation failed')
    })

    it('should expose null when no server error', () => {
      const props = { task: null, serverError: null }
      const form = useTaskForm(props, emit)

      expect(form.serverError.value).toBeNull()
    })
  })

  describe('constants', () => {
    it('should expose validation constants', () => {
      const props = { task: null }
      const form = useTaskForm(props, emit)

      expect(form.TITLE_MAX_LENGTH).toBe(200)
      expect(form.DESCRIPTION_MAX_LENGTH).toBe(2000)
      expect(form.PRIORITY_MIN).toBe(-100)
      expect(form.PRIORITY_MAX).toBe(100)
    })
  })

  describe('hasChanges', () => {
    it('should be false in create mode with empty title', () => {
      const props = { task: null }
      const form = useTaskForm(props, emit)

      expect(form.hasChanges.value).toBe(false)
    })

    it('should be true in create mode when title is provided', () => {
      const props = { task: null }
      const form = useTaskForm(props, emit)

      form.title.value = 'New Task'
      expect(form.hasChanges.value).toBe(true)
    })

    it('should be false in edit mode when no changes made', () => {
      const props = { task: existingTask }
      const form = useTaskForm(props, emit)

      expect(form.hasChanges.value).toBe(false)
    })

    it('should be true in edit mode when title changed', () => {
      const props = { task: existingTask }
      const form = useTaskForm(props, emit)

      form.title.value = 'Changed Title'
      expect(form.hasChanges.value).toBe(true)
    })

    it('should emit cancel when no changes on submit in edit mode', () => {
      const props = { task: existingTask }
      const form = useTaskForm(props, emit)

      // No changes made, button should be disabled, but if somehow submitted:
      form.handleSubmit()

      // With button disabled, this shouldn't happen, but the logic should handle it
      expect(emit).not.toHaveBeenCalledWith('submit', expect.anything())
    })
  })

})
