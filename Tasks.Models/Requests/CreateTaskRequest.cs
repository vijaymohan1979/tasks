using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Tasks.Models.Requests;

/// <summary>
/// Request model for creating a new task.
/// </summary>
public class CreateTaskRequest
{
    /// <summary>
    /// The title of the task. Required, max 200 characters.
    /// </summary>
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Title must be between 1 and 200 characters.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Optional description providing additional details about the task.
    /// </summary>
    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// Task priority level. Higher values indicate higher priority.
    /// </summary>
    /// <example>5</example>
    [Range(-100, 100, ErrorMessage = "Priority must be between -100 and 100.")]
    public int Priority { get; set; } = 0;

    /// <summary>
    /// Optional due date for the task in UTC.
    /// </summary>
    public DateTime? DueDateUtc { get; set; }

    /// <summary>
    /// Display order for sorting tasks. Lower values appear first.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "SortOrder must be a non-negative integer.")]
    public int SortOrder { get; set; } = 0;
}
