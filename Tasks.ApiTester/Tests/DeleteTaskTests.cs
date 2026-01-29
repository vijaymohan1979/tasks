using System.Net;
using Tasks.ApiTester.Clients;
using Tasks.ApiTester.Helpers;

namespace Tasks.ApiTester.Tests;

/// <summary>
/// Tests for DELETE /api/tasks/{id} endpoint.
/// </summary>
public class DeleteTaskTests : BaseTestSuite
{
    protected override string SectionName => "DELETE TASK TESTS (DELETE /api/tasks/{id})";

    public DeleteTaskTests(TaskApiClient apiClient, TestRunner testRunner)
        : base(apiClient, testRunner) { }

    protected override async Task ExecuteTestsAsync()
    {
        await TestRunner.RunTestAsync("Delete existing task returns 204 No Content", async () =>
        {
            var task = await ApiClient.CreateTaskAsync(new { Title = "Task to Delete" });
            var response = await ApiClient.DeleteTaskAsync(task!.Id);

            Assertions.StatusCodeEquals(HttpStatusCode.NoContent, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Deleted task is no longer retrievable (404)", async () =>
        {
            var task = await ApiClient.CreateTaskAsync(new { Title = "Task to Delete and Verify" });
            await ApiClient.DeleteTaskAsync(task!.Id);

            var response = await ApiClient.GetTaskByIdRawAsync(task.Id);
            Assertions.StatusCodeEquals(HttpStatusCode.NotFound, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Delete non-existent task returns 404", async () =>
        {
            var response = await ApiClient.DeleteTaskAsync(99999);
            Assertions.StatusCodeEquals(HttpStatusCode.NotFound, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Delete with ID = 0 returns 400", async () =>
        {
            var response = await ApiClient.DeleteTaskAsync(0);
            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Delete with negative ID returns 400", async () =>
        {
            var response = await ApiClient.DeleteTaskAsync(-1);
            Assertions.StatusCodeEquals(HttpStatusCode.BadRequest, response.StatusCode);
        });

        await TestRunner.RunTestAsync("Delete same task twice - second returns 404", async () =>
        {
            var task = await ApiClient.CreateTaskAsync(new { Title = "Double Delete Test" });

            var firstDelete = await ApiClient.DeleteTaskAsync(task!.Id);
            Assertions.StatusCodeEquals(HttpStatusCode.NoContent, firstDelete.StatusCode);

            var secondDelete = await ApiClient.DeleteTaskAsync(task.Id);
            Assertions.StatusCodeEquals(HttpStatusCode.NotFound, secondDelete.StatusCode);
        });
    }
}