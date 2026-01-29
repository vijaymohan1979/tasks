namespace Tasks.ApiTester.Models;

/// <summary>
/// Generic wrapper for paginated API responses.
/// </summary>
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }

    public override string ToString() =>
        $"Page {Page}/{TotalPages} (PageSize={PageSize}, TotalCount={TotalCount}, ItemsReturned={Items.Count})";
}