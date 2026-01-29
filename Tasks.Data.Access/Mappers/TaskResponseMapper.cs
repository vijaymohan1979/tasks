using System;
using System.Collections.Generic;
using System.Text;
using Tasks.Data;
using Tasks.Models.Responses;

namespace Tasks.Data.Access.Mappers;

public static class TaskResponseMapper
{
    public static TaskResponse ToTaskResponse(this TaskDbEntity entity) =>
        new TaskResponse
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            Status = entity.Status.ToStatusResponse(),
            Priority = entity.Priority,
            DueDateUtc = entity.DueDateUtc,
            CreatedAtUtc = entity.CreatedAtUtc,
            UpdatedAtUtc = entity.UpdatedAtUtc,
            CompletedAtUtc = entity.CompletedAtUtc,
            SortOrder = entity.SortOrder,
            RowVersion = entity.RowVersion
        };

}
