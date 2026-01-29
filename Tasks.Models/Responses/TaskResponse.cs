using System;
using System.Collections.Generic;
using System.Text;

namespace Tasks.Models.Responses;

/// <summary>
/// Response model representing a task.
/// </summary>
public class TaskResponse
{
    /// <summary>
    /// The unique identifier of the task.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The title of the task.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of the task.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The current status of the task.
    /// </summary>
    public TaskStatusResponse Status { get; set; }

    /// <summary>
    /// The priority level of the task.
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// The due date of the task in UTC, if set.
    /// </summary>
    public DateTime? DueDateUtc { get; set; }

    /// <summary>
    /// The date and time when the task was created in UTC.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>
    /// The date and time when the task was last updated in UTC.
    /// </summary>
    public DateTime UpdatedAtUtc { get; set; }

    /// <summary>
    /// The date and time when the task was completed in UTC, if applicable.
    /// </summary>
    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// The sort order for display purposes.
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// Version number for optimistic concurrency control.
    /// </summary>
    public long RowVersion { get; set; }
}