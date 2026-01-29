using Tasks.ApiTester.Clients;
using Tasks.ApiTester.Helpers;

namespace Tasks.ApiTester.Tests;

/// <summary>
/// Base class for all test suites with common functionality.
/// </summary>
public abstract class BaseTestSuite
{
    protected readonly TaskApiClient ApiClient;
    protected readonly TestRunner TestRunner;

    protected BaseTestSuite(TaskApiClient apiClient, TestRunner testRunner)
    {
        ApiClient = apiClient;
        TestRunner = testRunner;
    }

    /// <summary>
    /// Override to provide the section name for logging.
    /// </summary>
    protected abstract string SectionName { get; }

    /// <summary>
    /// Runs all tests in this suite.
    /// </summary>
    public async Task RunAllAsync()
    {
        ConsoleLogger.PrintSection(SectionName);
        await ExecuteTestsAsync();
    }

    /// <summary>
    /// Override to implement the actual tests.
    /// </summary>
    protected abstract Task ExecuteTestsAsync();
}