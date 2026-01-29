using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Tasks.Models.Responses;

namespace Tasks.Models.Requests;

/// <summary>
/// Request model for updating an existing task.
/// </summary>
public class UpdateTaskRequest
{
    /// <summary>
    /// The unique identifier of the task to update.
    /// </summary>
    [Required(ErrorMessage = "Id is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Id must be a positive integer.")]
    public int Id { get; set; }

    /// <summary>
    /// Required for optimistic concurrency control.
    /// Must match the current RowVersion in the database.
    /// </summary>
    [Required(ErrorMessage = "RowVersion is required for concurrency control.")]
    public long? RowVersion { get; set; }

    /// <summary>
    /// The updated title of the task.
    /// </summary>
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters.")]
    public string? Title { get; set; }

    /// <summary>
    /// The updated description of the task.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// The updated status of the task.
    /// </summary>
    public TaskStatusResponse? Status { get; set; }

    /// <summary>
    /// The updated priority level of the task.
    /// </summary>
    [Range(-100, 100, ErrorMessage = "Priority must be between -100 and 100.")]
    public int? Priority { get; set; }

    /// <summary>
    /// The updated due date in UTC.
    /// </summary>
    public DateTime? DueDateUtc { get; set; }

    /// <summary>
    /// The updated sort order for display purposes.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "SortOrder must be a non-negative integer.")]
    public int? SortOrder { get; set; }

    /// <summary>
    /// The timestamp when the task was completed (UTC).
    /// </summary>
    /// <remarks>
    /// Automatically managed by the service layer based on status changes.
    /// </remarks>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// Indicates whether CompletedAtUtc should be explicitly cleared. 
    /// </summary>
    /// <remarks>
    /// Needed because null could mean "don't change" vs "set to null".
    /// </remarks>
    public bool ClearCompletedAtUtc { get; set; }
}
