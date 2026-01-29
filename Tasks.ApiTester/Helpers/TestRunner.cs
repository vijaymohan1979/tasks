namespace Tasks.ApiTester.Helpers;

/// <summary>
/// Test execution framework with result tracking.
/// </summary>
public class TestRunner
{

    // These properties won't be thread safe if tests are run in parallel.
    // If we start doing that, use Interlocked.Increment

    public int PassedCount { get; private set; }
    public int FailedCount { get; private set; }
    public int SkippedCount { get; private set; }

    private readonly List<(string Name, bool Passed, string? Error)> _results = [];

    /// <summary>
    /// Runs a test and tracks the result.
    /// </summary>
    public async Task RunTestAsync(string testName, Func<Task> test)
    {
        ConsoleLogger.LogTestStart(testName);

        try
        {
            await test();
            ConsoleLogger.LogTestPass();
            PassedCount++;
            _results.Add((testName, true, null));
        }
        catch (Exception ex)
        {
            ConsoleLogger.LogTestFail(ex.Message);
            FailedCount++;
            _results.Add((testName, false, ex.Message));
        }
    }

    /// <summary>
    /// Runs a test that returns a value (for chaining).
    /// </summary>
    public async Task<T?> RunTestAsync<T>(string testName, Func<Task<T>> test)
    {
        ConsoleLogger.LogTestStart(testName);

        try
        {
            var result = await test();
            ConsoleLogger.LogTestPass();
            PassedCount++;
            _results.Add((testName, true, null));
            return result;
        }
        catch (Exception ex)
        {
            ConsoleLogger.LogTestFail(ex.Message);
            FailedCount++;
            _results.Add((testName, false, ex.Message));
            return default;
        }
    }

    /// <summary>
    /// Skips a test with a reason.
    /// </summary>
    public void SkipTest(string testName, string reason)
    {
        ConsoleLogger.LogTestStart(testName);
        ConsoleLogger.LogTestSkip(reason);
        SkippedCount++;
        _results.Add((testName, true, $"SKIPPED: {reason}"));
    }

    /// <summary>
    /// Prints the final test summary.
    /// </summary>
    public void PrintSummary()
    {
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("|" + new string('═', 58) + "|");
        Console.WriteLine("|" + "TEST SUMMARY".PadLeft(35).PadRight(58) + "|");
        Console.WriteLine("|" + new string('═', 58) + "|");
        Console.ResetColor();

        Console.Write("|  ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"Passed:  {PassedCount,4}");
        Console.ResetColor();
        Console.WriteLine("".PadRight(44) + "|");

        Console.Write("|  ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write($"Failed:  {FailedCount,4}");
        Console.ResetColor();
        Console.WriteLine("".PadRight(44) + "|");

        Console.Write("|  ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Skipped: {SkippedCount,4}");
        Console.ResetColor();
        Console.WriteLine("".PadRight(44) + "|");

        Console.Write("|  ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"Total:   {PassedCount + FailedCount + SkippedCount,4}");
        Console.ResetColor();
        Console.WriteLine("".PadRight(44) + "|");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("|" + new string('═', 58) + "|");
        Console.ResetColor();

        // Print failed tests if any
        if (FailedCount > 0)
        {
            Console.WriteLine();
            ConsoleLogger.LogError("Failed tests:");
            foreach (var (name, passed, error) in _results.Where(r => !r.Passed))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  • {name}");
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine($"    {error}");
                Console.ResetColor();
            }
        }

        Console.WriteLine();
        var resultText = FailedCount == 0 ? "All tests passed! [PASS]" : $"Test run completed with {FailedCount} failure(s)";
        var resultColor = FailedCount == 0 ? ConsoleColor.Green : ConsoleColor.Red;
        Console.ForegroundColor = resultColor;
        Console.WriteLine(resultText);
        Console.ResetColor();
    }
}