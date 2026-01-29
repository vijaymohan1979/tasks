using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tasks.Services.Extensions;

/// <summary>
/// Extension methods for registering service layer dependencies.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds task service layer dependencies to the DI container.
    /// </summary>
    public static IServiceCollection AddTaskServices(this IServiceCollection services)
    {
        services.AddScoped<ITaskService, TaskService>();
        return services;
    }
}
