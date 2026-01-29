using System.Net;
using Tasks.ApiTester.Clients;
using Tasks.ApiTester.Helpers;
using Tasks.ApiTester.Models;

namespace Tasks.ApiTester.Tests;

/// <summary>
/// Tests for PUT /api/tasks/{id} endpoint.
/// </summary>
public class UpdateTaskTests : BaseTestSuite
{
    protected override string SectionName => "UPDATE TASK TESTS (PUT /api/tasks/{id})";

    public UpdateTaskTests(TaskApiClient apiClient, TestRunner testRunner)
        : base(apiClient, testRunner) { }

    protected override async Task ExecuteTestsAsync()
    {
        // Create a task for testing updates
        ConsoleLogger.LogInfo("Setting up test task for update tests...");

        var createdTask = await ApiClient.CreateTaskAsync(new
        {
            Title = "Original Title",
            Description = "Original Description",
            Priority = 1,
            SortOrder = 1
        });

        await TestRunner.RunTestAsync("Update task title only", async () =>
        {
            var current = await ApiClient.GetTaskByIdAsync(createdTask!.Id);
            
            var request = new
            {
                Id = current!.Id,
                RowVersion = current.RowVersion,
                Title = "Updated Title"
            };

            var response = await ApiClient.UpdateTaskAsync(current.Id, request);

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual("Updated Title", response!.Title, "Title should be updated");
        });

        await TestRunner.RunTestAsync("Update task description only", async () =>
        {
            var current = await ApiClient.GetTaskByIdAsync(createdTask!.Id);

            var request = new
            {
                Id = current!.Id,
                RowVersion = current.RowVersion,
                Description = "New Description"
            };

            var response = await ApiClient.UpdateTaskAsync(current.Id, request);

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual("New Description", response!.Description, "Description should be updated");
        });

        await TestRunner.RunTestAsync("Update multiple fields at once", async () =>
        {
            var current = await ApiClient.GetTaskByIdAsync(createdTask!.Id);

            var request = new
            {
                Id = current!.Id,
                RowVersion = current.RowVersion,
                Title = "Multi-Update Title",
                Description = "Multi-Update Description",
                Priority = 50,
                Status = TaskStatusResponse.InProgress,
                SortOrder = 99
            };

            var response = await ApiClient.UpdateTaskAsync(current.Id, request);

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual("Multi-Update Title", response!.Title, "Title should be updated");
            Assertions.AreEqual("Multi-Update Description", response.Description, "Description should be updated");
            Assertions.AreEqual(50, response.Priority, "Priority should be updated");
            Assertions.AreEqual(TaskStatusResponse.InProgress, response.Status, "Status should be updated");
            Assertions.AreEqual(99, response.SortOrder, "SortOrder should be updated");
        });

        await TestRunner.RunTestAsync("Update increments RowVersion", async () =>
        {
            var taskForVersion = await ApiClient.CreateTaskAsync(new { Title = "Version Test" });
            var originalVersion = taskForVersion!.RowVersion;

            var request = new
            {
                Id = taskForVersion.Id,
                RowVersion = taskForVersion.RowVersion,
                Title = "Version Updated"
            };

            var updated = await ApiClient.UpdateTaskAsync(taskForVersion.Id, request);

            Assertions.IsNotNull(updated, "Response should not be null");
            Assertions.IsTrue(updated!.RowVersion > originalVersion, "RowVersion should increment after update");
        });

        await TestRunner.RunTestAsync("Update with stale RowVersion returns 409 Conflict", async () =>
        {
            var taskForConflict = await ApiClient.CreateTaskAsync(new { Title = "Conflict Test" });

            // First update to change RowVersion
            var firstRequest = new { Id = taskForConflict!.Id, RowVersion = taskForConflict.RowVersion, Title = "First Update" };
            await ApiClient.UpdateTaskAsync(taskForConflict.Id, firstRequest);

            // Second update with stale RowVersion
            var staleRequest = new { Id = taskForConflict.Id, RowVersion = taskForConflict.RowVersion, Title = "Stale Update" };
            var response = await ApiClient.UpdateTaskRawAsync(taskForConflict.Id, staleRequest);

            if (response.StatusCode != HttpStatusCode.Conflict)
            {
                var body = await response.Content.ReadAsStringAsync();
                ConsoleLogger.LogError($"Response body: {body}");
            }

            Assertions.StatusCodeEquals(HttpStatusCode.Conflict, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Update with mismatched route/body ID returns 400", async () =>
        {
            var current = await ApiClient.GetTaskByIdAsync(createdTask!.Id);
            var request = new { Id = 99999, RowVersion = current!.RowVersion, Title = "Mismatch Test" };
            var response = await ApiClient.UpdateTaskRawAsync(current.Id, request);

            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Update non-existent task returns 404", async () =>
        {
            var request = new { Id = 99999, RowVersion = 1L, Title = "Ghost Task" };
            var response = await ApiClient.UpdateTaskRawAsync(99999, request);

            Assertions.StatusCodeEquals(HttpStatusCode.NotFound, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Update with invalid ID (0) returns 400", async () =>
        {
            var request = new { Id = 0, RowVersion = 1L, Title = "Invalid ID" };
            var response = await ApiClient.UpdateTaskRawAsync(0, request);

            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Update sets UpdatedAtUtc timestamp", async () =>
        {
            var taskForTimestamp = await ApiClient.CreateTaskAsync(new { Title = "Timestamp Test" });
            var originalUpdated = taskForTimestamp!.UpdatedAtUtc;

            await Task.Delay(100); // Small delay to ensure timestamp difference

            var request = new { Id = taskForTimestamp.Id, RowVersion = taskForTimestamp.RowVersion, Title = "Timestamp Updated" };
            var updated = await ApiClient.UpdateTaskAsync(taskForTimestamp.Id, request);

            Assertions.IsNotNull(updated, "Response should not be null");
            Assertions.IsTrue(updated!.UpdatedAtUtc > originalUpdated, "UpdatedAtUtc should be updated");
        });
    }
}