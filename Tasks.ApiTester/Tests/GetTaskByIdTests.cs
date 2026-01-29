using System.Net;
using Tasks.ApiTester.Clients;
using Tasks.ApiTester.Helpers;
using Tasks.ApiTester.Models;

namespace Tasks.ApiTester.Tests;

/// <summary>
/// Tests for GET /api/tasks/{id} endpoint.
/// </summary>
public class GetTaskByIdTests : BaseTestSuite
{
    protected override string SectionName => "GET TASK BY ID TESTS (GET /api/tasks/{id})";

    public GetTaskByIdTests(TaskApiClient apiClient, TestRunner testRunner)
        : base(apiClient, testRunner) { }

    protected override async Task ExecuteTestsAsync()
    {
        // Create a task first for testing retrieval
        ConsoleLogger.LogInfo("Setting up test task for retrieval tests...");
        var createdTask = await ApiClient.CreateTaskAsync(new { Title = "Task for GetById Tests" });

        await TestRunner.RunTestAsync("Get existing task by ID", async () =>
        {
            var response = await ApiClient.GetTaskByIdAsync(createdTask!.Id);

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual(createdTask.Id, response!.Id, "IDs should match");
            Assertions.AreEqual(createdTask.Title, response.Title, "Titles should match");
        });

        await TestRunner.RunTestAsync("Get non-existent task returns 404 Not Found", async () =>
        {
            var response = await ApiClient.GetTaskByIdRawAsync(99999);
            Assertions.StatusCodeEquals(HttpStatusCode.NotFound, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Get task with ID = 0 returns 400 Bad Request", async () =>
        {
            var response = await ApiClient.GetTaskByIdRawAsync(0);
            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Get task with negative ID returns 400 Bad Request", async () =>
        {
            var response = await ApiClient.GetTaskByIdRawAsync(-1);
            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Retrieved task contains all expected fields", async () =>
        {
            var task = await ApiClient.CreateTaskAsync(new
            {
                Title = "Full Field Check",
                Description = "Test Description",
                Priority = 7,
                SortOrder = 5
            });

            var response = await ApiClient.GetTaskByIdAsync(task!.Id);

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual(task.Id, response!.Id, "Id should match");
            Assertions.AreEqual("Full Field Check", response.Title, "Title should match");
            Assertions.AreEqual("Test Description", response.Description, "Description should match");
            Assertions.AreEqual(7, response.Priority, "Priority should match");
            Assertions.AreEqual(5, response.SortOrder, "SortOrder should match");
            Assertions.AreEqual(TaskStatusResponse.Todo, response.Status, "Status should be Todo");
            Assertions.IsTrue(response.CreatedAtUtc > DateTime.MinValue, "CreatedAtUtc should be set");
            Assertions.IsTrue(response.UpdatedAtUtc > DateTime.MinValue, "UpdatedAtUtc should be set");
            Assertions.IsTrue(response.RowVersion >= 0, "RowVersion should be set");
        });
    }
}