using System.ComponentModel.DataAnnotations;
using Tasks.Models.Responses;

namespace Tasks.Models.Requests;

/// <summary>
/// Request model for updating only the status of a task.
/// </summary>
public class UpdateTaskStatusRequest
{
    /// <summary>
    /// Required for optimistic concurrency control.
    /// Must match the current RowVersion in the database.
    /// </summary>
    [Required(ErrorMessage = "RowVersion is required for concurrency control.")]
    public long? RowVersion { get; set; }

    /// <summary>
    /// The new status for the task.
    /// </summary>
    [Required(ErrorMessage = "Status is required.")]
    [EnumDataType(typeof(TaskStatusResponse), ErrorMessage = "Invalid task status.")]
    public TaskStatusResponse Status { get; set; }
}
