using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Tasks.Models.Responses;

namespace Tasks.Models.Requests;

/// <summary>
/// Request model for filtering, sorting, and paginating tasks.
/// </summary>
public class TaskFilterRequest
{
    /// <summary>
    /// Filter by task status. Null means all statuses.
    /// </summary>
    public TaskStatusResponse? Status { get; set; }

    /// <summary>
    /// Filter by title substring (case-insensitive).
    /// </summary>
    public string? TitleSearch { get; set; }

    /// <summary>
    /// Minimum priority filter (inclusive). Null means no minimum.
    /// </summary>
    public int? MinPriority { get; set; }

    /// <summary>
    /// Maximum priority filter (inclusive). Null means no maximum.
    /// </summary>
    public int? MaxPriority { get; set; }

    /// <summary>
    /// Field to sort by. Valid values: "priority", "duedate", "created", "updated", "title", "sortorder".
    /// </summary>
    [RegularExpression("^(priority|duedate|created|updated|title|sortorder)$",
        ErrorMessage = "Invalid sort field.")]
    public string SortBy { get; set; } = "sortorder";

    /// <summary>
    /// Sort direction. Valid values: "asc" or "desc".
    /// </summary>
    [RegularExpression("^(asc|desc)$", ErrorMessage = "Sort direction must be 'asc' or 'desc'.")]
    public string SortDirection { get; set; } = "asc";

    /// <summary>
    /// Page number for pagination (1-based).
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1.")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 20;
}
