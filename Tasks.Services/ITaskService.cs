using System;
using System.Collections.Generic;
using System.Text;
using Tasks.Models.Requests;
using Tasks.Models.Responses;
using Tasks.Services.Entities;

namespace Tasks.Services;

/// <summary>
/// Service contract for task business operations.
/// Controllers should use this interface, not the repository directly.
/// </summary>
public interface ITaskService
{
    /// <summary>
    /// Creates a new task after validation.
    /// </summary>
    Task<ServiceResult<TaskResponse>> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a task by ID.
    /// </summary>
    Task<ServiceResult<TaskResponse>> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks with filtering, sorting, and pagination.
    /// </summary>
    Task<ServiceResult<PaginatedResponse<TaskResponse>>> GetTasksAsync(TaskFilterRequest filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing task after validation.
    /// </summary>
    Task<ServiceResult<TaskResponse>> UpdateTaskAsync(UpdateTaskRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the status of a task (convenience method for status transitions).
    /// </summary>
    Task<ServiceResult<TaskResponse>> UpdateTaskStatusAsync(int id, long rowVersion, TaskStatusResponse newStatus, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a task by ID.
    /// </summary>
    Task<ServiceResult> DeleteTaskAsync(int id, CancellationToken cancellationToken = default);
}
