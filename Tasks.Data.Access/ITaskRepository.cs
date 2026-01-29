using System;
using System.Collections.Generic;
using System.Text;
using Tasks.Models.Requests;
using Tasks.Models.Responses;

namespace Tasks.Data.Access;

/// <summary>
/// Repository contract for task data access operations.
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// Creates a new task.
    /// </summary>
    /// <throws>ArgumentException if validation fails</throws>
    Task<TaskResponse> CreateAsync(CreateTaskRequest createRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a task by ID.
    /// </summary>
    /// <returns>Task if found, null otherwise</returns>
    Task<TaskResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets tasks with optional filtering and sorting.
    /// </summary>
    Task<PaginatedResponse<TaskResponse>> GetAsync(TaskFilterRequest filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing task.
    /// </summary>
    /// <throws>ArgumentException if validation fails</throws>
    /// <throws>KeyNotFoundException if task not found</throws>
    Task<TaskResponse> UpdateAsync(UpdateTaskRequest updateRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a task by ID.
    /// </summary>
    /// <throws>KeyNotFoundException if task not found</throws>
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
