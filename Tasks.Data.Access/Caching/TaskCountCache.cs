using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using Tasks.Models.Requests;

namespace Tasks.Data.Access.Caching;

/// <summary>
/// Manages caching for task count queries with token-based bulk invalidation.
/// </summary>
public class TaskCountCache : ITaskCountCache
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration;

    private CancellationTokenSource _resetTokenSource = new();
    private readonly Lock _resetLock = new();

    public TaskCountCache(IMemoryCache cache, TimeSpan? cacheDuration = null)
    {
        _cache = cache;
        _cacheDuration = cacheDuration ?? TimeSpan.FromSeconds(30);
    }

    /// <summary>
    /// Gets a cached count or creates it using the provided factory.
    /// </summary>
    public async Task<int> GetOrCreateAsync(
        TaskFilterRequest filter,
        Func<CancellationToken, Task<int>> countFactory,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(filter);

        if (_cache.TryGetValue(cacheKey, out int cachedCount))
        {
            return cachedCount;
        }

        var count = await countFactory(cancellationToken);

        var options = new MemoryCacheEntryOptions()
                            .SetAbsoluteExpiration(_cacheDuration)
                            .AddExpirationToken(new CancellationChangeToken(_resetTokenSource.Token));

        _cache.Set(cacheKey, count, options);

        return count;
    }

    /// <summary>
    /// Invalidates all cached count entries.
    /// </summary>
    public void InvalidateAll()
    {
        lock (_resetLock)
        {
            _resetTokenSource.Cancel();
            _resetTokenSource.Dispose();
            _resetTokenSource = new CancellationTokenSource();
        }
    }

    private static string BuildCacheKey(TaskFilterRequest filter)
    {
        // Include only filters that affect count (not sorting/paging)
        return $"TaskCount:" +
               $"Status={filter.Status?.ToString() ?? "all"}|" +
               $"Title={filter.TitleSearch ?? ""}|" +
               $"MinPri={filter.MinPriority?.ToString() ?? ""}|" +
               $"MaxPri={filter.MaxPriority?.ToString() ?? ""}";
    }
}
