using System;
using System.Collections.Generic;
using System.Text;
using Tasks.Models.Requests;

namespace Tasks.Data.Access.Caching;

/// <summary>
/// Provides caching for task count queries with bulk invalidation support.
/// </summary>
public interface ITaskCountCache
{
    /// <summary>
    /// Gets a cached count or creates it using the provided factory.
    /// </summary>
    Task<int> GetOrCreateAsync(
        TaskFilterRequest filter,
        Func<CancellationToken, Task<int>> countFactory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates all cached count entries.
    /// </summary>
    void InvalidateAll();
}
