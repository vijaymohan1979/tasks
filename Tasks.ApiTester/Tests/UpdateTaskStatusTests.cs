using System.Net;
using Tasks.ApiTester.Clients;
using Tasks.ApiTester.Helpers;
using Tasks.ApiTester.Models;

namespace Tasks.ApiTester.Tests;

/// <summary>
/// Tests for PATCH /api/tasks/{id}/status endpoint.
/// </summary>
public class UpdateTaskStatusTests : BaseTestSuite
{
    protected override string SectionName => "UPDATE TASK STATUS TESTS (PATCH /api/tasks/{id}/status)";

    public UpdateTaskStatusTests(TaskApiClient apiClient, TestRunner testRunner)
        : base(apiClient, testRunner) { }

    protected override async Task ExecuteTestsAsync()
    {
        await TestRunner.RunTestAsync("Update status: Todo -> InProgress", async () =>
        {
            var task = await ApiClient.CreateTaskAsync(new { Title = "Status Test: Todo to InProgress" });
            Assertions.AreEqual(TaskStatusResponse.Todo, task!.Status, "Initial status should be Todo");

            var request = new { RowVersion = task.RowVersion, Status = TaskStatusResponse.InProgress };
            var response = await ApiClient.UpdateTaskStatusAsync(task.Id, request);

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual(TaskStatusResponse.InProgress, response!.Status, "Status should be InProgress");
        });

        await TestRunner.RunTestAsync("Update status: InProgress -> Done", async () =>
        {
            var task = await ApiClient.CreateTaskAsync(new { Title = "Status Test: InProgress to Done" });

            var inProgress = await ApiClient.UpdateTaskStatusAsync(
                                                task!.Id,
                                                new { RowVersion = task.RowVersion, Status = TaskStatusResponse.InProgress });

            var request = new { RowVersion = inProgress!.RowVersion, Status = TaskStatusResponse.Done };
            var response = await ApiClient.UpdateTaskStatusAsync(task.Id, request);

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual(TaskStatusResponse.Done, response!.Status, "Status should be Done");
        });

        await TestRunner.RunTestAsync("Update status: Done -> Todo (reset)", async () =>
        {
            var task = await ApiClient.CreateTaskAsync(new { Title = "Status Test: Done to Todo" });

            var inProgress = await ApiClient.UpdateTaskStatusAsync(
                                                task!.Id,
                                                new { RowVersion = task.RowVersion, Status = TaskStatusResponse.InProgress });

            var done = await ApiClient.UpdateTaskStatusAsync(
                                            task.Id,
                                            new { RowVersion = inProgress!.RowVersion, Status = TaskStatusResponse.Done });

            var request = new { RowVersion = done!.RowVersion, Status = TaskStatusResponse.Todo };
            var response = await ApiClient.UpdateTaskStatusAsync(task.Id, request);

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual(TaskStatusResponse.Todo, response!.Status, "Status should be Todo");
        });

        await TestRunner.RunTestAsync("Update status: Todo -> Done (skip InProgress)", async () =>
        {
            var task = await ApiClient.CreateTaskAsync(new { Title = "Status Test: Todo to Done directly" });

            var request = new { RowVersion = task!.RowVersion, Status = TaskStatusResponse.Done };
            var response = await ApiClient.UpdateTaskStatusAsync(task.Id, request);

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual(TaskStatusResponse.Done, response!.Status, "Status should be Done");
        });

        await TestRunner.RunTestAsync("CompletedAtUtc is set when status becomes Done", async () =>
        {
            var task = await ApiClient.CreateTaskAsync(new { Title = "CompletedAt Test" });
            Assertions.IsNull(task!.CompletedAtUtc?.ToString(), "CompletedAtUtc should be null initially");

            var request = new { RowVersion = task.RowVersion, Status = TaskStatusResponse.Done };
            var response = await ApiClient.UpdateTaskStatusAsync(task.Id, request);

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.IsNotNull(response!.CompletedAtUtc?.ToString(), "CompletedAtUtc should be set when Done");
        });

        await TestRunner.RunTestAsync("Update status with stale RowVersion returns 409 Conflict", async () =>
        {
            var task = await ApiClient.CreateTaskAsync(new { Title = "Status Conflict Test" });

            // First update
            await ApiClient.UpdateTaskStatusAsync(
                                task!.Id,
                                new { RowVersion = task.RowVersion, Status = TaskStatusResponse.InProgress });

            // Second update with stale RowVersion
            var response = await ApiClient.UpdateTaskStatusRawAsync(
                                                task.Id,
                                                new { RowVersion = task.RowVersion, Status = TaskStatusResponse.Done });

            Assertions.StatusCodeEquals(HttpStatusCode.Conflict, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Update status of non-existent task returns 404", async () =>
        {
            var response = await ApiClient.UpdateTaskStatusRawAsync(
                                                99999,
                                                new { RowVersion = 1L, Status = TaskStatusResponse.Done });

            Assertions.StatusCodeEquals(HttpStatusCode.NotFound, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Update status with invalid ID returns 400", async () =>
        {
            var response = await ApiClient.UpdateTaskStatusRawAsync(
                                                0,
                                                new { RowVersion = 1L, Status = TaskStatusResponse.Done });

            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });
    }
}