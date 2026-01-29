using System;
using System.Collections.Generic;
using System.Text;

namespace Tasks.Models.Responses;

/// <summary>
/// Represents the status of a task.
/// </summary>
public enum TaskStatusResponse
{
    /// <summary>
    /// Task is pending and has not been started.
    /// </summary>
    Todo = 0,
    /// <summary>
    /// Task is currently being worked on.
    /// </summary>
    InProgress = 1,
    /// <summary>
    /// Task has been completed.
    /// </summary>
    Done = 2
}
