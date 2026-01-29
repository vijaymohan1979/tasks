namespace Tasks.ApiTester.Helpers;

/// <summary>
/// Centralized console logging with color-coded output.
/// </summary>
public static class ConsoleLogger
{

    // The lock is needed because Console is not thread-safe when multiple
    // threads write simultaneously. Otherwise, output can become interleaved
    // and corrupted
    private static readonly object _lock = new();
    public static bool VerboseMode { get; set; } = false;

    public static void PrintBanner(string title)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine();
            Console.WriteLine("|" + new string('═', 58) + "|");
            Console.WriteLine("|" + title.PadLeft((58 + title.Length) / 2).PadRight(58) + "|");
            Console.WriteLine("|" + new string('═', 58) + "|");
            Console.ResetColor();
        }
    }

    public static void PrintSection(string title)
    {
        lock (_lock)
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"> {title}");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(new string('─', 55));
            Console.ResetColor();
        }
    }

    public static void LogInfo(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("INFO  ");
            Console.ResetColor();
            Console.WriteLine(message);
        }
    }

    public static void LogDebug(string message)
    {
        if (!VerboseMode) return;

        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
            Console.Write("DEBUG ");
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }

    public static void LogSuccess(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("OK    ");
            Console.ResetColor();
            Console.WriteLine(message);
        }
    }

    public static void LogWarning(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("WARN  ");
            Console.ResetColor();
            Console.WriteLine(message);
        }
    }

    public static void LogError(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERROR ");
            Console.ResetColor();
            Console.WriteLine(message);
        }
    }

    public static void LogFatal(string message)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.Write("FATAL ");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" {message}");
            Console.ResetColor();
        }
    }

    public static void LogTestStart(string testName)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{DateTime.Now:HH:mm:ss}] ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.Write("TEST  ");
            Console.ResetColor();
            Console.Write($"{testName}... ");
        }
    }

    public static void LogTestPass()
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(" PASSED");
            Console.ResetColor();
        }
    }

    public static void LogTestFail(string reason)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($" FAILED");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"           └─ {reason}");
            Console.ResetColor();
        }
    }

    public static void LogTestSkip(string reason)
    {
        lock (_lock)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($" SKIPPED: {reason}");
            Console.ResetColor();
        }
    }
}