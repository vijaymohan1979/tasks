using System;
using System.Collections.Generic;
using System.Text;
using Tasks.Models.Responses;

namespace Tasks.Data.Access.Mappers;

public static class TaskStatusMapper
{
    public static TaskDbStatus ToDbStatus(this TaskStatusResponse status) =>
        status switch
        {
            TaskStatusResponse.Todo => TaskDbStatus.Todo,
            TaskStatusResponse.InProgress => TaskDbStatus.InProgress,
            TaskStatusResponse.Done => TaskDbStatus.Done,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown task status.")
        };

    public static TaskDbStatus? ToDbStatus(this TaskStatusResponse? status) =>
        status.HasValue ? ToDbStatus(status.Value) : (TaskDbStatus?) null;

    public static TaskStatusResponse ToStatusResponse(this TaskDbStatus status) =>
        status switch
        {
            TaskDbStatus.Todo => TaskStatusResponse.Todo,
            TaskDbStatus.InProgress => TaskStatusResponse.InProgress,
            TaskDbStatus.Done => TaskStatusResponse.Done,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown task status.")
        };

    public static TaskStatusResponse? ToStatusResponse(this TaskDbStatus? status) =>
        status.HasValue ? ToStatusResponse(status.Value) : (TaskStatusResponse?) null;

}