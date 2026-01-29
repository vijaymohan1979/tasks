using System.Net;
using Tasks.ApiTester.Clients;
using Tasks.ApiTester.Helpers;

namespace Tasks.ApiTester.Tests;

/// <summary>
/// Tests for input validation across all endpoints.
/// </summary>
public class ValidationTests : BaseTestSuite
{
    protected override string SectionName => "VALIDATION ERROR TESTS";

    public ValidationTests(TaskApiClient apiClient, TestRunner testRunner)
        : base(apiClient, testRunner) { }

    protected override async Task ExecuteTestsAsync()
    {
        // Create Task Validation
        await TestCreateValidation();

        // Update Task Validation
        await TestUpdateValidation();
    }

    private async Task TestCreateValidation()
    {
        ConsoleLogger.LogInfo("Testing Create Task validation rules...");

        await TestRunner.RunTestAsync("Create: Missing title returns 400", async () =>
        {
            var response = await ApiClient.CreateTaskRawAsync(new { Description = "No title provided" });
            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Create: Empty title returns 400", async () =>
        {
            var response = await ApiClient.CreateTaskRawAsync(new { Title = "" });
            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Create: Whitespace-only title returns 400", async () =>
        {
            var response = await ApiClient.CreateTaskRawAsync(new { Title = "   " });
            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Create: Title > 200 chars returns 400", async () =>
        {
            var response = await ApiClient.CreateTaskRawAsync(new { Title = new string('x', 201) });
            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Create: Description > 2000 chars returns 400", async () =>
        {
            var response = await ApiClient.CreateTaskRawAsync(new
            {
                Title = "Valid Title",
                Description = new string('d', 2001)
            });
            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Create: Priority > 100 returns 400", async () =>
        {
            var response = await ApiClient.CreateTaskRawAsync(new { Title = "Test", Priority = 101 });
            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Create: Priority < -100 returns 400", async () =>
        {
            var response = await ApiClient.CreateTaskRawAsync(new { Title = "Test", Priority = -101 });
            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Create: Negative SortOrder returns 400", async () =>
        {
            var response = await ApiClient.CreateTaskRawAsync(new { Title = "Test", SortOrder = -1 });
            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });
    }

    private async Task TestUpdateValidation()
    {
        ConsoleLogger.LogInfo("Testing Update Task validation rules...");

        var task = await ApiClient.CreateTaskAsync(new { Title = "Validation Test Task" });

        await TestRunner.RunTestAsync("Update: Empty title returns 400", async () =>
        {
            var current = await ApiClient.GetTaskByIdAsync(task!.Id);
            var response = await ApiClient.UpdateTaskRawAsync(current!.Id, new
            {
                Id = current.Id,
                RowVersion = current.RowVersion,
                Title = ""
            });

            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Update: Title > 200 chars returns 400", async () =>
        {
            var current = await ApiClient.GetTaskByIdAsync(task!.Id);
            var response = await ApiClient.UpdateTaskRawAsync(current!.Id, new
            {
                Id = current.Id,
                RowVersion = current.RowVersion,
                Title = new string('x', 201)
            });

            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Update: Missing RowVersion returns 400", async () =>
        {
            var current = await ApiClient.GetTaskByIdAsync(task!.Id);
            var response = await ApiClient.UpdateTaskRawAsync(current!.Id, new
            {
                Id = current.Id,
                Title = "No RowVersion"
            });

            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Update: Priority > 100 returns 400", async () =>
        {
            var current = await ApiClient.GetTaskByIdAsync(task!.Id);
            var response = await ApiClient.UpdateTaskRawAsync(current!.Id, new
            {
                Id = current.Id,
                RowVersion = current.RowVersion,
                Priority = 101
            });

            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Update: Priority < -100 returns 400", async () =>
        {
            var current = await ApiClient.GetTaskByIdAsync(task!.Id);
            var response = await ApiClient.UpdateTaskRawAsync(current!.Id, new
            {
                Id = current.Id,
                RowVersion = current.RowVersion,
                Priority = -101
            });

            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Update Status: Missing RowVersion returns 400", async () =>
        {
            var current = await ApiClient.GetTaskByIdAsync(task!.Id);
            var response = await ApiClient.UpdateTaskStatusRawAsync(current!.Id, new
            {
                Status = Models.TaskStatusResponse.Done
            });

            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });
    }
}