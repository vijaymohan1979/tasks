// Import testing utilities from Vitest (a test runner)
//   describe - Groups related tests together under a label
//   it - Defines a single test case
//   expect - Makes assertions
//   vi - A utility to create mocks of functions
import { describe, it, expect, vi } from 'vitest'

// Import mount from Vue's testing library.
// This function creates a "virtual" version of a Vue component
// so we can test it without opening a browser.
import { mount } from '@vue/test-utils'

// Import tools for handling data fetching.
//   VueQueryPlugin helps Vue components fetch and cache data from APIs.
//   QueryClient manages that cached data.
import { VueQueryPlugin, QueryClient } from '@tanstack/vue-query'

// Imports the main App.vue component that we want to test.
import App from '../App.vue'

// Mock the taskApi service to avoid actual HTTP calls
//   vi.mock() - replaces real code with fake versions for testing
vi.mock('@/services/taskApi', () => ({
  // Define mock API methods
  taskApi: {
    // A fake function (vi.fn()) that immediately returns a fake
    // "empty" response with pagination info.
    //
    // mockResolvedValue means it pretends to be an async function that succeeds.
    getTasks: vi.fn().mockResolvedValue({
      items: [],
      page: 1,
      pageSize: 10,
      totalCount: 0,
      totalPages: 0,
      hasNextPage: false,
      hasPreviousPage: false
    }),

    // Creates additional fake functions for other API operations.
    // These don't return anything specific because this test doesn't use them. 
    getTaskById: vi.fn(),
    createTask: vi.fn(),
    updateTask: vi.fn(),
    updateTaskStatus: vi.fn(),
    deleteTask: vi.fn()
  }
}))

// Start a test group named "App". All tests inside this block relate to testing the App component.
//  'describe()' - groups related tests together under a label
describe('App', () => {
  // Defines a single test.
  //  'it()' - defines one specific test
  //  The text describes what we're testing (that the app successfully shows the "Task Manager" heading).
  it('mounts and renders the task manager heading', () => {
    // Creates a QueryClient with retry: false.
    //  This tells the data - fetching library not to retry failed requests
    //  during tests(makes tests faster and more predictable).
    const queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false
        }
      }
    })

    // Mount (create) the App component for testing
    //  'mount()' - creates a testable instance of a Vue component
    //  The wrapper variable lets us inspect and interact with the component.
    const wrapper = mount(App, {
      // The global.plugins option installs the VueQueryPlugin so the component can fetch data
      global: {
        plugins: [[VueQueryPlugin, { queryClient }]]
      }
    })

    // The actual assertion
    //  expect() - checks if something is true
    //  wrapper.text() = Gets all visible text from the component
    //  Checks if it contains the words "Task Manager"
    //  If not found, the test fails
    expect(wrapper.text()).toContain('Task Manager')
  })
})
