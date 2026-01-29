using Microsoft.AspNetCore.Mvc;
using Tasks.Models.Requests;
using Tasks.Models.Responses;
using Tasks.Services;
using Tasks.Services.Entities;
using Tasks.WebApp.Extensions;

namespace Tasks.WebApp.Controllers;

/// <summary>
/// REST API controller for task management operations.
/// Acts as a thin facade, delegating business logic to <see cref="ITaskService"/>.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService ?? throw new ArgumentNullException(nameof(taskService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new task.
    /// </summary>
    /// <param name="request">The task creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created task.</returns>
    /// <response code="201">Task created successfully.</response>
    /// <response code="400">Validation errors in the request.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTaskAsync(
        [FromBody] CreateTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _taskService.CreateTaskAsync(request, cancellationToken);

        return this.ToActionResult(result, 
                                   () => CreatedAtAction(
                                            nameof(GetTaskByIdAsync),
                                            new { id = result.Data!.Id },
                                            result.Data));
    }

    /// <summary>
    /// Gets a task by its ID.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The task if found.</returns>
    /// <response code="200">Task found.</response>
    /// <response code="400">Invalid ID provided.</response>
    /// <response code="404">Task not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTaskByIdAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return Problem(
                detail: "Task ID must be a positive integer.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid ID");
        }

        var result = await _taskService.GetTaskByIdAsync(id, cancellationToken);

        return this.ToActionResult(result, () => Ok(result.Data));
    }

    /// <summary>
    /// Gets tasks with optional filtering, sorting, and pagination.
    /// </summary>
    /// <param name="filter">Filter, sort, and pagination parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Paginated list of tasks.</returns>
    /// <response code="200">Tasks retrieved successfully.</response>
    /// <response code="400">Invalid filter parameters.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<TaskResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTasksAsync(
        [FromQuery] TaskFilterRequest filter,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _taskService.GetTasksAsync(filter, cancellationToken);

        return this.ToActionResult(result, () => Ok(result.Data));
    }

    /// <summary>
    /// Updates an existing task.
    /// </summary>
    /// <param name="id">The task ID from the route.</param>
    /// <param name="request">The task update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    /// <response code="200">Task updated successfully.</response>
    /// <response code="400">Validation errors or ID mismatch.</response>
    /// <response code="404">Task not found.</response>
    /// <response code="409">Concurrency conflict (RowVersion mismatch).</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateTaskAsync(
        int id,
        [FromBody] UpdateTaskRequest request,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return Problem(
                detail: "Task ID must be a positive integer.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid ID");
        }

        if (request.Id != id)
        {
            ModelState.AddModelError(nameof(request.Id), "Route ID must match request body ID.");
            return ValidationProblem(ModelState);
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _taskService.UpdateTaskAsync(request, cancellationToken);

        return this.ToActionResult(result, () => Ok(result.Data));
    }

    /// <summary>
    /// Updates only the status of a task.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="request">The status update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated task.</returns>
    /// <response code="200">Task status updated successfully.</response>
    /// <response code="400">Invalid status or transition not allowed.</response>
    /// <response code="404">Task not found.</response>
    /// <response code="409">Concurrency conflict (RowVersion mismatch).</response>
    [HttpPatch("{id:int}/status")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateTaskStatusAsync(
        int id,
        [FromBody] UpdateTaskStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return Problem(
                detail: "Task ID must be a positive integer.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid ID");
        }

        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var result = await _taskService.UpdateTaskStatusAsync(id, request.RowVersion!.Value, request.Status, cancellationToken);

        return this.ToActionResult(result, () => Ok(result.Data));
    }

    /// <summary>
    /// Deletes a task by its ID.
    /// </summary>
    /// <param name="id">The task ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">Task deleted successfully.</response>
    /// <response code="400">Invalid ID provided.</response>
    /// <response code="404">Task not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTaskAsync(int id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return Problem(
                detail: "Task ID must be a positive integer.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid ID");
        }

        var result = await _taskService.DeleteTaskAsync(id, cancellationToken);

        return this.ToActionResult(result);
    }



}
