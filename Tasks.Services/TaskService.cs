using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Tasks.Data.Access;
using Tasks.Models.Requests;
using Tasks.Models.Responses;
using Tasks.Models.Validators;
using Tasks.Services.Entities;

namespace Tasks.Services;

/// <summary>
/// Business logic layer for task operations.
/// Handles validation, business rules, and orchestrates data access.
/// </summary>
public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;
    private readonly ILogger<TaskService> _logger;

    /// <summary>
    /// Valid task status transitions. Defined once as immutable business rules.
    /// </summary>
    private static readonly Dictionary<TaskStatusResponse, TaskStatusResponse[]> _validStatusTransitions = new()
    {
        [TaskStatusResponse.Todo] = [TaskStatusResponse.InProgress, TaskStatusResponse.Done],
        [TaskStatusResponse.InProgress] = [TaskStatusResponse.Todo, TaskStatusResponse.Done],
        [TaskStatusResponse.Done] = [TaskStatusResponse.Todo, TaskStatusResponse.InProgress]
    };

    public TaskService(ITaskRepository repository, ILogger<TaskService> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<ServiceResult<TaskResponse>> CreateTaskAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate request
        var validationErrors = TaskModelValidator.ValidateCreate(request);

        if (validationErrors.Count > 0)
        {
            _logger.LogWarning("Task creation validation failed: {Errors}", string.Join(", ", validationErrors));
            return ServiceResult<TaskResponse>.Failure(validationErrors, ServiceErrorType.Validation);
        }

        try
        {
            var task = await _repository.CreateAsync(request, cancellationToken);
            _logger.LogInformation("Task created successfully with ID {TaskId}", task.Id);

            return ServiceResult<TaskResponse>.Success(task);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Task creation failed due to validation error");
            return ServiceResult<TaskResponse>.Failure(ex.Message, ServiceErrorType.Validation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating task");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<TaskResponse>> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return ServiceResult<TaskResponse>.Failure("Task ID must be a positive integer.", ServiceErrorType.Validation);
        }

        try
        {
            var task = await _repository.GetByIdAsync(id, cancellationToken);

            if (task is null)
            {
                _logger.LogWarning("Task with ID {TaskId} not found", id);
                return ServiceResult<TaskResponse>.NotFound($"Task with ID {id} was not found.");
            }

            return ServiceResult<TaskResponse>.Success(task);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            // Defense-in-depth (DID): repository also validates ID
            // DID is a security and software design principle where you implement multiple layers of
            // protection, so if one layer fails, others still catch the problem.
            _logger.LogWarning(ex, "Get task failed due to invalid ID {TaskId}", id);
            return ServiceResult<TaskResponse>.Failure(ex.Message, ServiceErrorType.Validation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task with ID {TaskId}", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<PaginatedResponse<TaskResponse>>> GetTasksAsync(TaskFilterRequest filter, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filter);

        // Validate filter
        var validationErrors = TaskModelValidator.ValidateFilter(filter);

        if (validationErrors.Count > 0)
        {
            _logger.LogWarning("Task filter validation failed: {Errors}", string.Join(", ", validationErrors));
            return ServiceResult<PaginatedResponse<TaskResponse>>.Failure(validationErrors, ServiceErrorType.Validation);
        }

        try
        {
            var result = await _repository.GetAsync(filter, cancellationToken);
            _logger.LogDebug("Retrieved {Count} tasks (Page {Page}/{TotalPages})",
                result.Items.Count, result.Page, result.TotalPages);

            return ServiceResult<PaginatedResponse<TaskResponse>>.Success(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Get tasks failed due to validation error");
            return ServiceResult<PaginatedResponse<TaskResponse>>.Failure(ex.Message, ServiceErrorType.Validation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks with filter");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<TaskResponse>> UpdateTaskAsync(UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Validate request
        var validationErrors = TaskModelValidator.ValidateUpdate(request);

        if (validationErrors.Count > 0)
        {
            _logger.LogWarning("Task update validation failed for ID {TaskId}: {Errors}",
                request.Id, string.Join(", ", validationErrors));
            return ServiceResult<TaskResponse>.Failure(validationErrors, ServiceErrorType.Validation);
        }

        // Validate status transition if status is being changed
        if (request.Status.HasValue)
        {
            var existingTask = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (existingTask is null)
            {
                return ServiceResult<TaskResponse>.NotFound($"Task with ID {request.Id} was not found.");
            }

            var transitionResult = ValidateStatusTransition(existingTask.Status, request.Status.Value);

            if (!transitionResult.IsSuccess)
            {
                return ServiceResult<TaskResponse>.Failure(transitionResult.Errors, ServiceErrorType.BusinessRule);
            }

            // Apply business rule: manage CompletedAtUtc based on status
            if (request.Status.Value == TaskStatusResponse.Done)
            {
                // Auto-set CompletedAtUtc when marking as Done (if not already completed)
                if (!existingTask.CompletedAtUtc.HasValue)
                {
                    request.CompletedAtUtc = DateTime.UtcNow;
                }
            }
            else
            {
                // Clear CompletedAtUtc only if reverting FROM Done (had a completion timestamp)
                if (existingTask.CompletedAtUtc.HasValue)
                {
                    request.ClearCompletedAtUtc = true;
                }
            }
        }

        try
        {
            var task = await _repository.UpdateAsync(request, cancellationToken);
            _logger.LogInformation("Task {TaskId} updated successfully", task.Id);

            return ServiceResult<TaskResponse>.Success(task);
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Task with ID {TaskId} not found for update", request.Id);
            return ServiceResult<TaskResponse>.NotFound($"Task with ID {request.Id} was not found.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating task {TaskId}", request.Id);
            return ServiceResult<TaskResponse>.Conflict("The task was modified by another user. Please refresh and try again.");
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Task update failed due to validation error");
            return ServiceResult<TaskResponse>.Failure(ex.Message, ServiceErrorType.Validation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error updating task {TaskId}", request.Id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ServiceResult<TaskResponse>> UpdateTaskStatusAsync(int id, long rowVersion, TaskStatusResponse newStatus, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return ServiceResult<TaskResponse>.Failure("Task ID must be a positive integer.", ServiceErrorType.Validation);
        }

        var updateRequest = new UpdateTaskRequest
        {
            Id = id,
            RowVersion = rowVersion,
            Status = newStatus
        };

        return await UpdateTaskAsync(updateRequest, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ServiceResult> DeleteTaskAsync(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return ServiceResult.Failure("Task ID must be a positive integer.", ServiceErrorType.Validation);
        }

        try
        {
            var deleted = await _repository.DeleteAsync(id, cancellationToken);

            if (!deleted)
            {
                return ServiceResult.NotFound($"Task with ID {id} was not found.");
            }

            _logger.LogInformation("Task {TaskId} deleted successfully", id);
            return ServiceResult.Success();
        }
        catch (KeyNotFoundException)
        {
            _logger.LogWarning("Task with ID {TaskId} not found for deletion", id);
            return ServiceResult.NotFound($"Task with ID {id} was not found.");
        }
        catch (ArgumentOutOfRangeException ex)
        {
            _logger.LogWarning(ex, "Delete task failed due to invalid ID");
            return ServiceResult.Failure(ex.Message, ServiceErrorType.Validation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            throw;
        }
    }

    /// <summary>
    /// Validates task status transitions based on business rules.
    /// </summary>
    private static ServiceResult ValidateStatusTransition(TaskStatusResponse currentStatus, TaskStatusResponse newStatus)
    {
        // Same status is always allowed (no-op)
        if (currentStatus == newStatus)
        {
            return ServiceResult.Success();
        }

        if (_validStatusTransitions.TryGetValue(currentStatus, out var allowedStatuses) &&
            allowedStatuses.Contains(newStatus))
        {
            return ServiceResult.Success();
        }

        return ServiceResult.Failure(
            $"Invalid status transition from '{currentStatus}' to '{newStatus}'.",
            ServiceErrorType.BusinessRule);
    }
}
