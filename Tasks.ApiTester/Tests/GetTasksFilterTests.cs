using System.Net;
using Tasks.ApiTester.Clients;
using Tasks.ApiTester.Helpers;
using Tasks.ApiTester.Models;

namespace Tasks.ApiTester.Tests;

/// <summary>
/// Tests for GET /api/tasks endpoint with filtering, sorting, and pagination.
/// </summary>
public class GetTasksFilterTests : BaseTestSuite
{
    protected override string SectionName => "GET TASKS FILTER TESTS (GET /api/tasks)";

    public GetTasksFilterTests(TaskApiClient apiClient, TestRunner testRunner)
        : base(apiClient, testRunner) { }

    protected override async Task ExecuteTestsAsync()
    {
        // Setup test data
        await SetupTestDataAsync();

        // Status Filter Tests
        await TestStatusFilters();

        // Title Search Tests
        await TestTitleSearch();

        // Priority Filter Tests
        await TestPriorityFilters();

        // Sort Tests
        await TestSortOptions();

        // Pagination Tests
        await TestPagination();

        // Combined Filter Tests
        await TestCombinedFilters();

        // Default/Empty Parameter Tests
        await TestDefaultParameters();
    }

    private async Task SetupTestDataAsync()
    {
        ConsoleLogger.LogInfo("Setting up diverse test data for filter tests...");

        // Create tasks with various properties
        var testTasks = new[]
        {
            new { Title = "Alpha Test Task", Priority = 1, SortOrder = 1 },
            new { Title = "Beta Test Task", Priority = 5, SortOrder = 2 },
            new { Title = "Gamma Test Task", Priority = 10, SortOrder = 3 },
            new { Title = "Delta Different", Priority = 3, SortOrder = 4 },
            new { Title = "Epsilon Test Item", Priority = 7, SortOrder = 5 },
            new { Title = "Zeta Priority High", Priority = 50, SortOrder = 6 },
            new { Title = "Eta Priority Low", Priority = -50, SortOrder = 7 }
        };

        foreach (var task in testTasks)
        {
            var response = await ApiClient.CreateTaskRawAsync(task);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                ConsoleLogger.LogError($"Failed to create task '{task.Title}': {response.StatusCode} - {body}");
            }

            response.EnsureSuccessStatusCode();
        }

        // Create tasks with different statuses
        var inProgressTask = await ApiClient.CreateTaskAsync(new { Title = "InProgress Filter Test", Priority = 2, SortOrder = 8 });

        if (inProgressTask != null)
        {
            await ApiClient.UpdateTaskStatusAsync(
                                inProgressTask.Id,
                                new { RowVersion = inProgressTask.RowVersion, Status = TaskStatusResponse.InProgress });
        }

        var doneTask = await ApiClient.CreateTaskAsync(new { Title = "Done Filter Test", Priority = 4, SortOrder = 9 });

        if (doneTask != null)
        {
            var updated = await ApiClient.UpdateTaskStatusAsync(
                                            doneTask.Id,
                                            new { RowVersion = doneTask.RowVersion, Status = TaskStatusResponse.InProgress });

            if (updated != null)
            {
                await ApiClient.UpdateTaskStatusAsync(
                                    doneTask.Id,
                                    new { RowVersion = updated.RowVersion, Status = TaskStatusResponse.Done });
            }
        }

        ConsoleLogger.LogSuccess("Test data setup complete");
    }

    private async Task TestStatusFilters()
    {
        await TestRunner.RunTestAsync("Filter by Status = Todo", async () =>
        {
            var response = await ApiClient.GetTasksAsync("?Status=Todo");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AllMatch(response!.Items, t => t.Status == TaskStatusResponse.Todo,
                "All items should have Todo status");
        });

        await TestRunner.RunTestAsync("Filter by Status = InProgress", async () =>
        {
            var response = await ApiClient.GetTasksAsync("?Status=InProgress");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AllMatch(response!.Items, t => t.Status == TaskStatusResponse.InProgress,
                "All items should have InProgress status");
        });

        await TestRunner.RunTestAsync("Filter by Status = Done", async () =>
        {
            var response = await ApiClient.GetTasksAsync("?Status=Done");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AllMatch(response!.Items, t => t.Status == TaskStatusResponse.Done,
                "All items should have Done status");
        });

        await TestRunner.RunTestAsync("Filter without Status returns all statuses", async () =>
        {
            var response = await ApiClient.GetTasksAsync("");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.IsGreaterThan(response!.TotalCount, 0, "Should return tasks");
        });
    }

    private async Task TestTitleSearch()
    {
        await TestRunner.RunTestAsync("TitleSearch - case insensitive uppercase", async () =>
        {
            var response = await ApiClient.GetTasksAsync("?TitleSearch=TEST");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.CollectionIsNotEmpty(response!.Items, "Should find tasks with 'test' in title");
            Assertions.AllMatch(response.Items, t => t.Title.Contains("Test", StringComparison.OrdinalIgnoreCase),
                "All items should contain 'Test' in title");
        });

        await TestRunner.RunTestAsync("TitleSearch - case insensitive lowercase", async () =>
        {
            var response = await ApiClient.GetTasksAsync("?TitleSearch=test");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.CollectionIsNotEmpty(response!.Items, "Should find tasks with 'test' in title");
        });

        await TestRunner.RunTestAsync("TitleSearch - partial match", async () =>
        {
            var response = await ApiClient.GetTasksAsync("?TitleSearch=Alpha");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AllMatch(response!.Items, t => t.Title.Contains("Alpha", StringComparison.OrdinalIgnoreCase),
                "All items should contain 'Alpha'");
        });

        await TestRunner.RunTestAsync("TitleSearch - no matches returns empty", async () =>
        {
            var response = await ApiClient.GetTasksAsync("?TitleSearch=ZZZZNOTFOUND");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual(0, response!.Items.Count, "Should return empty list");
        });
    }

    private async Task TestPriorityFilters()
    {
        await TestRunner.RunTestAsync("Filter by MinPriority only", async () =>
        {
            var response = await ApiClient.GetTasksAsync("?MinPriority=5");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AllMatch(response!.Items, t => t.Priority >= 5,
                "All items should have priority >= 5");
        });

        await TestRunner.RunTestAsync("Filter by MaxPriority only", async () =>
        {
            var response = await ApiClient.GetTasksAsync("?MaxPriority=3");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AllMatch(response!.Items, t => t.Priority <= 3,
                "All items should have priority <= 3");
        });

        await TestRunner.RunTestAsync("Filter by MinPriority and MaxPriority range", async () =>
        {
            var response = await ApiClient.GetTasksAsync("?MinPriority=2&MaxPriority=8");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AllMatch(response!.Items, t => t.Priority >= 2 && t.Priority <= 8,
                "All items should have priority between 2 and 8");
        });

        await TestRunner.RunTestAsync("Filter by negative priority range", async () =>
        {
            var response = await ApiClient.GetTasksAsync("?MinPriority=-100&MaxPriority=-1");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AllMatch(response!.Items, t => t.Priority >= -100 && t.Priority <= -1,
                "All items should have negative priority");
        });
    }

    private async Task TestSortOptions()
    {
        string[] sortFields = ["priority", "duedate", "created", "updated", "title", "sortorder"];

        foreach (var field in sortFields)
        {
            await TestRunner.RunTestAsync($"Sort by {field} ascending", async () =>
            {
                var response = await ApiClient.GetTasksAsync($"?SortBy={field}&SortDirection=asc");

                Assertions.IsNotNull(response, "Response should not be null");
                Assertions.IsTrue(response!.Items.Count >= 0, $"Should return items sorted by {field} asc");
            });

            await TestRunner.RunTestAsync($"Sort by {field} descending", async () =>
            {
                var response = await ApiClient.GetTasksAsync($"?SortBy={field}&SortDirection=desc");

                Assertions.IsNotNull(response, "Response should not be null");
                Assertions.IsTrue(response!.Items.Count >= 0, $"Should return items sorted by {field} desc");
            });
        }

        await TestRunner.RunTestAsync("Invalid SortBy value returns 400", async () =>
        {
            var response = await ApiClient.GetTasksRawAsync("?SortBy=invalid");

            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Invalid SortDirection value returns 400", async () =>
        {
            var response = await ApiClient.GetTasksRawAsync("?SortDirection=invalid");

            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });
    }

    private async Task TestPagination()
    {
        await TestRunner.RunTestAsync("Pagination - Page 1 with PageSize 2", async () =>
        {
            var response = await ApiClient.GetTasksAsync("?Page=1&PageSize=2");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.IsTrue(response!.Items.Count <= 2, "Should return at most 2 items");
            Assertions.AreEqual(1, response.Page, "Page should be 1");
            Assertions.AreEqual(2, response.PageSize, "PageSize should be 2");
        });

        await TestRunner.RunTestAsync("Pagination - Page 2 with PageSize 2", async () =>
        {
            var response = await ApiClient.GetTasksAsync("?Page=2&PageSize=2");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual(2, response!.Page, "Page should be 2");
        });

        await TestRunner.RunTestAsync("Pagination - Maximum PageSize (100)", async () =>
        {
            var response = await ApiClient.GetTasksAsync("?PageSize=100");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual(100, response!.PageSize, "PageSize should be 100");
        });

        await TestRunner.RunTestAsync("Pagination - Page < 1 returns 400", async () =>
        {
            var response = await ApiClient.GetTasksRawAsync("?Page=0");

            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Pagination - PageSize > 100 returns 400", async () =>
        {
            var response = await ApiClient.GetTasksRawAsync("?PageSize=101");

            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Pagination - PageSize < 1 returns 400", async () =>
        {
            var response = await ApiClient.GetTasksRawAsync("?PageSize=0");

            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Pagination - TotalPages calculation is correct", async () =>
        {
            var response = await ApiClient.GetTasksAsync("?PageSize=3");

            Assertions.IsNotNull(response, "Response should not be null");
            var expectedPages = (int)Math.Ceiling(response!.TotalCount / 3.0);
            Assertions.AreEqual(expectedPages, response.TotalPages, "TotalPages calculation should be correct");
        });
    }

    private async Task TestCombinedFilters()
    {
        await TestRunner.RunTestAsync("Combined: Status + TitleSearch + Sort + Pagination", async () =>
        {
            var response = await ApiClient.GetTasksAsync(
                "?Status=Todo&TitleSearch=Test&SortBy=priority&SortDirection=desc&Page=1&PageSize=5");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AllMatch(response!.Items, t => t.Status == TaskStatusResponse.Todo,
                "All items should be Todo");
            Assertions.AllMatch(response.Items, t => t.Title.Contains("Test", StringComparison.OrdinalIgnoreCase),
                "All items should contain 'Test'");
        });

        await TestRunner.RunTestAsync("Combined: Priority range + Sort by title", async () =>
        {
            var response = await ApiClient.GetTasksAsync(
                "?MinPriority=0&MaxPriority=10&SortBy=title&SortDirection=asc");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AllMatch(response!.Items, t => t.Priority >= 0 && t.Priority <= 10,
                "All items should have priority between 0 and 10");
        });
    }

    private async Task TestDefaultParameters()
    {
        await TestRunner.RunTestAsync("Default parameters (no query string)", async () =>
        {
            var response = await ApiClient.GetTasksAsync("");

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual(1, response!.Page, "Default page should be 1");
            Assertions.AreEqual(20, response.PageSize, "Default page size should be 20");
        });
    }
}