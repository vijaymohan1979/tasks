using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tasks.Data.Access.Caching;
using Tasks.Data.Access.Mappers;
using Tasks.Models.Requests;
using Tasks.Models.Responses;
using Tasks.Models.Validators;

namespace Tasks.Data.Access;

/// <summary>
/// Implements task data access operations using Entity Framework Core.
/// Includes input validation and DTO mapping.
/// </summary>
public class TaskRepository : ITaskRepository
{
    private readonly TasksDbContext _context;
    private readonly ITaskCountCache _countCache;

    public TaskRepository(TasksDbContext context, ITaskCountCache countCache)
    {
        _context = context;
        _countCache = countCache;
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest createRequest, CancellationToken cancellationToken = default)
    {
        // Validate input
        var validationErrors = TaskModelValidator.ValidateCreate(createRequest);

        if (validationErrors.Any())
        {
            throw new ArgumentException(string.Join("; ", validationErrors));
        }

        // Map DTO to entity
        var entity = new TaskDbEntity
        {
            Title = createRequest.Title,
            Description = createRequest.Description,
            Priority = createRequest.Priority,
            DueDateUtc = createRequest.DueDateUtc,
            SortOrder = createRequest.SortOrder,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        // Persist
        _context.Tasks.Add(entity);
        
        if (await _context.SaveChangesAsync(cancellationToken) > 0)
        {
            // Invalidate count cache - new task affects counts
            _countCache.InvalidateAll();
        }

        // Map entity back to DTO
        return entity.ToTaskResponse();
    }

    public async Task<TaskResponse?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Validate input
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), id, "Id must be greater than 0.");
        }

        // Use AsNoTracking for read-only query - avoids change tracking overhead
        var entity = await _context.Tasks
                                   .AsNoTracking()
                                   .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        return entity == null ? null : entity.ToTaskResponse();
    }

    public async Task<PaginatedResponse<TaskResponse>> GetAsync(TaskFilterRequest filter, CancellationToken cancellationToken = default)
    {
        // Validate input
        var validationErrors = TaskModelValidator.ValidateFilter(filter);

        if (validationErrors.Any())
        {
            throw new ArgumentException(string.Join("; ", validationErrors));
        }

        // Build query with AsNoTracking early in the chain
        var query = _context.Tasks.AsNoTracking();

        // Apply filters
        if (filter.Status.HasValue)
        {
            query = query.Where(t => t.Status == filter.Status.ToDbStatus());
        }

        if (!string.IsNullOrWhiteSpace(filter.TitleSearch))
        {
            // Escape special LIKE characters for SQLite (using backslash as escape char)
            var escaped = filter.TitleSearch
                .Replace("\\", "\\\\")  // Escape the escape character first
                .Replace("%", "\\%")
                .Replace("_", "\\_");

            // Contains search - does NOT use index (documented tradeoff)
            // Acceptable for datasets < 10K rows
            // FUTURE TODO: Implement FTS5 (full-text search) for production scale
            query = query.Where(t => EF.Functions.Like(t.Title, $"%{escaped}%"));
        }

        if (filter.MinPriority.HasValue)
        {
            query = query.Where(t => t.Priority >= filter.MinPriority);
        }

        if (filter.MaxPriority.HasValue)
        {
            query = query.Where(t => t.Priority <= filter.MaxPriority);
        }

        // Get total count BEFORE sorting/paging (more efficient)
        // Use cached count to reduce DB load on frequent queries 
        var totalCount = await _countCache.GetOrCreateAsync(filter,
                                                            ct => query.CountAsync(ct),
                                                            cancellationToken);

        // Short-circuit if no results
        if (totalCount == 0)
        {
            return new PaginatedResponse<TaskResponse>
            {
                Items = [],
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = 0
            };
        }

        // Apply sorting
        IOrderedQueryable<TaskDbEntity> orderedQuery = filter.SortBy.ToLowerInvariant() switch
        {
            "priority" => filter.SortDirection == "desc"
                ? query.OrderByDescending(t => t.Priority)
                : query.OrderBy(t => t.Priority),
            "duedate" => filter.SortDirection == "desc"
                ? query.OrderByDescending(t => t.DueDateUtc)
                : query.OrderBy(t => t.DueDateUtc),
            "created" => filter.SortDirection == "desc"
                ? query.OrderByDescending(t => t.CreatedAtUtc)
                : query.OrderBy(t => t.CreatedAtUtc),
            "updated" => filter.SortDirection == "desc"
                ? query.OrderByDescending(t => t.UpdatedAtUtc)
                : query.OrderBy(t => t.UpdatedAtUtc),
            "title" => filter.SortDirection == "desc"
                ? query.OrderByDescending(t => t.Title)
                : query.OrderBy(t => t.Title),
            _ => filter.SortDirection == "desc"
                ? query.OrderByDescending(t => t.SortOrder)
                : query.OrderBy(t => t.SortOrder)
        };

        // Add tiebreaker for deterministic pagination
        // Without a tiebreaker, rows with identical sort values can appear in different orders across pages, causing:
        // duplicates (same item on different pages), missing rows (items skipped betweeb pages) and
        // inconsistent results (same query returns different results)
        orderedQuery = orderedQuery.ThenBy(t => t.Id);

        // Apply pagination
        var tasks = await orderedQuery
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<TaskResponse>
        {
            Items = tasks.Select(t => t.ToTaskResponse()).ToList(),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<TaskResponse> UpdateAsync(UpdateTaskRequest updateRequest, CancellationToken cancellationToken = default)
    {
        // Validate input
        var validationErrors = TaskModelValidator.ValidateUpdate(updateRequest);

        if (validationErrors.Any())
        {
            throw new ArgumentException(string.Join("; ", validationErrors));
        }

        // Get existing entity
        var entity = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == updateRequest.Id, cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException($"Task with Id {updateRequest.Id} not found.");
        }

        // Check RowVersion matches (optimistic concurrency)
        if (entity.RowVersion != updateRequest.RowVersion!.Value)
        {
            throw new DbUpdateConcurrencyException(
                $"Task with Id {updateRequest.Id} was modified by another user. " +
                $"Expected RowVersion: {updateRequest.RowVersion}, Current: {entity.RowVersion}");
        }

        // Track if update affects cached counts
        // The cached counts are per filter combination. When you update certain fields, a
        // task can move between filter buckets, making cached counts stale.
        var affectsCachedCounts = (updateRequest.Status.HasValue ||
                                   updateRequest.Priority.HasValue ||
                                   !string.IsNullOrWhiteSpace(updateRequest.Title));

        // Update only provided fields
        if (!string.IsNullOrWhiteSpace(updateRequest.Title))
        {
            entity.Title = updateRequest.Title;
        }

        if (updateRequest.Description != null)
        {
            entity.Description = updateRequest.Description;
        }

        if (updateRequest.Status.HasValue)
        {
            entity.Status = updateRequest.Status.Value.ToDbStatus();
        }

        // Handle CompletedAtUtc (controlled by service layer)
        if (updateRequest.CompletedAtUtc.HasValue)
        {
            entity.CompletedAtUtc = updateRequest.CompletedAtUtc.Value;
        }
        else if (updateRequest.ClearCompletedAtUtc)
        {
            entity.CompletedAtUtc = null;
        }

        if (updateRequest.Priority.HasValue)
        {
            entity.Priority = updateRequest.Priority.Value;
        }

        if (updateRequest.DueDateUtc.HasValue)
        {
            entity.DueDateUtc = updateRequest.DueDateUtc.Value;
        }

        if (updateRequest.SortOrder.HasValue)
        {
            entity.SortOrder = updateRequest.SortOrder.Value;
        }

        entity.UpdatedAtUtc = DateTime.UtcNow;
        entity.RowVersion++;  // Increment version for next update

        // Persist with concurrency handling
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Race condition: another update happened between our read and write
            throw new DbUpdateConcurrencyException(
                $"Task with Id {updateRequest.Id} was modified by another process during save.", ex);
        }

        // Invalidate count cache only if filter-relevant fields changed
        if (affectsCachedCounts)
        {
            _countCache.InvalidateAll();
        }

        return entity.ToTaskResponse();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        // Validate input
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(id), id, "Id must be greater than 0.");
        }

        // Use ExecuteDeleteAsync for single-query deletion (EF Core 7+)
        // This avoids fetching the entity before deleting
        var affectedRows = await _context.Tasks
                                         .Where(t => t.Id == id)
                                         .ExecuteDeleteAsync(cancellationToken);

        if (affectedRows == 0)
        {
            throw new KeyNotFoundException($"Task with Id {id} not found.");
        }

        // Invalidate count cache - deleted task affects counts
        _countCache.InvalidateAll();

        return affectedRows > 0;
    }

}
