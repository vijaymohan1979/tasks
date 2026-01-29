using System.Net;
using Tasks.ApiTester.Clients;
using Tasks.ApiTester.Helpers;
using Tasks.ApiTester.Models;

namespace Tasks.ApiTester.Tests;

/// <summary>
/// Tests for POST /api/tasks endpoint.
/// </summary>
public class CreateTaskTests : BaseTestSuite
{
    protected override string SectionName => "CREATE TASK TESTS (POST /api/tasks)";

    public CreateTaskTests(TaskApiClient apiClient, TestRunner testRunner)
        : base(apiClient, testRunner) { }

    protected override async Task ExecuteTestsAsync()
    {
        await TestRunner.RunTestAsync("Create task with minimal fields (Title only)", async () =>
        {
            var request = new { Title = "Minimal Task" };
            var response = await ApiClient.CreateTaskAsync(request);

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.IsGreaterThan(response!.Id, 0, "Task should have a valid ID");
            Assertions.AreEqual("Minimal Task", response.Title, "Title should match");
            Assertions.AreEqual(TaskStatusResponse.Todo, response.Status, "Default status should be Todo");
            Assertions.AreEqual(0, response.Priority, "Default priority should be 0");
            Assertions.AreEqual(0, response.SortOrder, "Default sort order should be 0");
        });

        await TestRunner.RunTestAsync("Create task with all fields populated", async () =>
        {
            var dueDate = DateTime.UtcNow.AddDays(7);
            var request = new
            {
                Title = "Complete Task",
                Description = "This is a detailed description of the task",
                Priority = 5,
                DueDateUtc = dueDate,
                SortOrder = 10
            };
            var response = await ApiClient.CreateTaskAsync(request);

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual("Complete Task", response!.Title, "Title should match");
            Assertions.AreEqual("This is a detailed description of the task", response.Description, "Description should match");
            Assertions.AreEqual(5, response.Priority, "Priority should be 5");
            Assertions.AreEqual(10, response.SortOrder, "SortOrder should be 10");
            Assertions.IsNotNull(response.DueDateUtc?.ToString(), "DueDate should be set");
        });

        await TestRunner.RunTestAsync("Create task with maximum priority (100)", async () =>
        {
            var request = new { Title = "High Priority Task", Priority = 100 };
            var response = await ApiClient.CreateTaskAsync(request);

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual(100, response!.Priority, "Max priority (100) should be accepted");
        });

        await TestRunner.RunTestAsync("Create task with minimum priority (-100)", async () =>
        {
            var request = new { Title = "Low Priority Task", Priority = -100 };
            var response = await ApiClient.CreateTaskAsync(request);

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual(-100, response!.Priority, "Min priority (-100) should be accepted");
        });

        await TestRunner.RunTestAsync("Create task with maximum title length (200 chars)", async () =>
        {
            var longTitle = new string('A', 200);
            var request = new { Title = longTitle };
            var response = await ApiClient.CreateTaskAsync(request);

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual(200, response!.Title.Length, "Title should be 200 characters");
        });

        await TestRunner.RunTestAsync("Create task with maximum description length (2000 chars)", async () =>
        {
            var longDescription = new string('D', 2000);
            var request = new { Title = "Task with Long Description", Description = longDescription };
            var response = await ApiClient.CreateTaskAsync(request);

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.AreEqual(2000, response!.Description!.Length, "Description should be 2000 characters");
        });

        await TestRunner.RunTestAsync("Create task with null description", async () =>
        {
            var request = new { Title = "Task with Null Description", Description = (string?)null };
            var response = await ApiClient.CreateTaskAsync(request);

            Assertions.IsNotNull(response, "Response should not be null");
            Assertions.IsNull(response!.Description, "Description should be null");
        });
    }
}