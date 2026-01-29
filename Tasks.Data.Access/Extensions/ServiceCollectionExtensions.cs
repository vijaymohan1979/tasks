using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using Tasks.Data.Access.Caching;

namespace Tasks.Data.Access.Extensions;

/// <summary>
/// Extension methods for registering Data.Access layer services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Task data access services to the dependency injection container.
    /// </summary>
    public static IServiceCollection AddTaskDataAccess(this IServiceCollection services)
    {
        // Register cache as singleton - manages shared invalidation state
        services.AddSingleton<ITaskCountCache, TaskCountCache>();

        // Register repository as scoped - aligns with DbContext lifetime
        services.AddScoped<ITaskRepository, TaskRepository>();

        return services;
    }
}
