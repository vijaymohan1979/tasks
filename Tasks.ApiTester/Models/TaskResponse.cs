namespace Tasks.ApiTester.Models;

/// <summary>
/// Response model representing a task from the API.
/// </summary>
public class TaskResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatusResponse Status { get; set; }
    public int Priority { get; set; }
    public DateTime? DueDateUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public int SortOrder { get; set; }
    public long RowVersion { get; set; }

    public override string ToString() =>
        $"Task[Id={Id}, Title='{Title}', Status={Status}, Priority={Priority}]";
}