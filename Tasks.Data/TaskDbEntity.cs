using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Tasks.Data;

/// <summary>
/// This class implements the requirements for: CRUD, status changes (Todo, In Progress, Done), filtering and sorting.
/// Priority, DueDate and SortOrder support typical ordering needs (priority first, due date, and custom sort order).
/// Timestamps support auditing. 
/// </summary>
public class TaskDbEntity
{
    public int Id { get; set; }

    /// <summary>
    /// This is a short name, indexed for search.
    /// </summary>
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public TaskDbStatus Status { get; set; } = TaskDbStatus.Todo;

    /// <summary>
    /// The priority of the task, where a higher number indicates a higher priority.
    /// </summary>
    /// <remarks>
    /// 0 = normal priority, positive numbers indicate higher priority, negative numbers indicate lower priority.
    /// </remarks>
    public int Priority { get; set; } = 0;

    public DateTime? DueDateUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAtUtc { get; set; }

    /// <summary>
    /// Helps implement stable, custom ordering
    /// </summary>
    public int SortOrder { get; set; } = 0;

    /// <summary>
    /// Gets or sets a value indicating whether the entity has been marked as deleted.
    /// </summary>
    /// <remarks>
    /// This is a soft delete flag. This avoids accidental data loss and simplifies undo/ recover
    /// scenarios.
    /// </remarks>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Gets or sets the version number used for concurrency control in the database.
    /// </summary>
    /// <remarks>
    /// Optimistic concurrency is used to detect and prevent conflicting updates in scenarios where
    /// multiple users may attempt to modify the same record concurrently. SQLite does not have a 
    /// built-in RowVersion column like SQL Server, so we use a long integer that is incremented by
    /// EF Core on each update.
    /// 
    /// The [ConcurrencyCheck] attribute tells EF Core to include this property in the WHERE clause
    /// during updates.
    /// </remarks>
    [ConcurrencyCheck]
    public long RowVersion { get; set; }
}
