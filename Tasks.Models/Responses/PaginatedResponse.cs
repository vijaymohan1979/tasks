using System;
using System.Collections.Generic;
using System.Text;

namespace Tasks.Models.Responses;

/// <summary>
/// Generic wrapper for paginated results.
/// </summary>
/// <typeparam name="T">The type of items in the result set.</typeparam>
public class PaginatedResponse<T>
{
    /// <summary>
    /// The list of items for the current page.
    /// </summary>
    public List<T> Items { get; set; } = [];

    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    /// <summary>
    /// Indicates whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Indicates whether there is a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
}
