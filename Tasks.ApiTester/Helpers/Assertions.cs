using System.Net;

namespace Tasks.ApiTester.Helpers;

/// <summary>
/// Custom assertion helpers for API testing.
/// </summary>
public static class Assertions
{
    public static void IsTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new AssertionException(message);
        }

        ConsoleLogger.LogDebug($"  [PASS] Assert: {message}");
    }

    public static void IsFalse(bool condition, string message)
    {
        if (condition)
        {
            throw new AssertionException(message);
        }

        ConsoleLogger.LogDebug($"  [PASS] Assert: {message}");
    }

    public static void AreEqual<T>(T expected, T actual, string message)
    {
        if (!Equals(expected, actual))
        {
            throw new AssertionException($"{message} - Expected: '{expected}', Actual: '{actual}'");
        }

        ConsoleLogger.LogDebug($"  [PASS] Assert: {message} (Value: {actual})");
    }

    public static void IsNotNull<T>(T? value, string message) where T : class
    {
        if (value is null)
        {
            throw new AssertionException($"{message} - Value was null");
        }

        ConsoleLogger.LogDebug($"  [PASS] Assert: {message}");
    }

    public static void IsNull<T>(T? value, string message) where T : class
    {
        if (value is not null)
        {
            throw new AssertionException($"{message} - Expected null but got: {value}");
        }

        ConsoleLogger.LogDebug($"  [PASS] Assert: {message}");
    }

    public static void IsGreaterThan(int value, int threshold, string message)
    {
        if (value <= threshold)
        {
            throw new AssertionException($"{message} - Expected > {threshold}, Actual: {value}");
        }

        ConsoleLogger.LogDebug($"  [PASS] Assert: {message} (Value: {value})");
    }

    public static void StatusCodeEquals(HttpStatusCode expected, HttpStatusCode actual)
    {
        if (expected != actual)
        {
            throw new AssertionException($"Expected status code {(int)expected} {expected}, but got {(int)actual} {actual}");
        }

        ConsoleLogger.LogDebug($"  [PASS] Assert: Status code is {(int)actual} {actual}");
    }

    public static void CollectionIsNotEmpty<T>(ICollection<T> collection, string message)
    {
        if (collection.Count == 0)
        {
            throw new AssertionException($"{message} - Collection was empty");
        }

        ConsoleLogger.LogDebug($"  [PASS] Assert: {message} (Count: {collection.Count})");
    }

    public static void AllMatch<T>(IEnumerable<T> items, Func<T, bool> predicate, string message)
    {
        var list = items.ToList();

        if (!list.All(predicate))
        {
            var failCount = list.Count(i => !predicate(i));
            throw new AssertionException($"{message} - {failCount} item(s) did not match");
        }

        ConsoleLogger.LogDebug($"  [PASS] Assert: {message} (Checked {list.Count} items)");
    }
}

/// <summary>
/// Custom exception for test assertions.
/// </summary>
public class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
}