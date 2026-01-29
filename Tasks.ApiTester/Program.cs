using Tasks.ApiTester.Clients;
using Tasks.ApiTester.Helpers;
using Tasks.ApiTester.Tests;

namespace Tasks.ApiTester;

/// <summary>
/// Entry point for the Task Management API Test Suite.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        var baseUrl = args.Length > 0 ? args[0] : "http://localhost:5211/api/tasks";

        ConsoleLogger.PrintBanner("Task Management API Test Suite");
        ConsoleLogger.LogInfo($"Target API: {baseUrl}");
        ConsoleLogger.LogInfo($"Started at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine();

        var apiClient = new TaskApiClient(baseUrl);
        var testRunner = new TestRunner();

        try
        {
            // Verify API is reachable
            ConsoleLogger.LogInfo("Verifying API connectivity...");
            await apiClient.HealthCheckAsync();
            ConsoleLogger.LogSuccess("API is reachable!");
            Console.WriteLine();

            // Run all test suites
            await new CreateTaskTests(apiClient, testRunner).RunAllAsync();
            await new GetTaskByIdTests(apiClient, testRunner).RunAllAsync();
            await new GetTasksFilterTests(apiClient, testRunner).RunAllAsync();
            await new UpdateTaskTests(apiClient, testRunner).RunAllAsync();
            await new UpdateTaskStatusTests(apiClient, testRunner).RunAllAsync();
            await new DeleteTaskTests(apiClient, testRunner).RunAllAsync();
            await new ValidationTests(apiClient, testRunner).RunAllAsync();

            testRunner.PrintSummary();
        }
        catch (HttpRequestException ex)
        {
            ConsoleLogger.LogFatal($"Cannot connect to API at {baseUrl}");
            ConsoleLogger.LogError($"Ensure the API is running. Error: {ex.Message}");

            Console.WriteLine("Press ANY key to EXIT.");
            Console.ReadKey();
            Environment.Exit(1);
        }
        catch (Exception ex)
        {
            ConsoleLogger.LogFatal($"Unexpected error: {ex.Message}");
            ConsoleLogger.LogError(ex.StackTrace ?? "No stack trace available");

            Console.WriteLine("Press ANY key to EXIT.");
            Console.ReadKey();
            Environment.Exit(1);
        }

        Console.WriteLine("Press ANY key to EXIT.");
        Console.ReadKey();
        Environment.Exit(testRunner.FailedCount > 0 ? 1 : 0);
    }
}