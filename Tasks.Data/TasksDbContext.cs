using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tasks.Data;

public class TasksDbContext : DbContext
{

    public TasksDbContext(DbContextOptions<TasksDbContext> options) : base(options) { }

    public DbSet<TaskDbEntity> Tasks => Set<TaskDbEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var task = modelBuilder.Entity<TaskDbEntity>();

        task.ToTable("Tasks");

        task.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        task.Property(p => p.Title)
            .HasMaxLength(200)
            .IsRequired()
            .UseCollation("NOCASE"); // Enables case-insensitive index usage

        task.Property(p => p.RowVersion)
            .IsConcurrencyToken();

        // Single column indexes for filtering and sorting
        task.HasIndex(p => p.Status);
        task.HasIndex(p => p.Priority);
        task.HasIndex(p => p.DueDateUtc);
        task.HasIndex(p => p.SortOrder);
        task.HasIndex(p => p.Title);

        //Indexes for sorting columns
        task.HasIndex(p => p.CreatedAtUtc);
        task.HasIndex(p => p.UpdatedAtUtc);
        task.HasIndex(p => p.CompletedAtUtc); // Useful for future "completed tasks" queries

        // Composite indexes for common filter + sort patterns
        // Status filtering with various sort orders
        task.HasIndex(p => new { p.Status, p.Priority });
        task.HasIndex(p => new { p.Status, p.DueDateUtc });
        task.HasIndex(p => new { p.Status, p.SortOrder });
        task.HasIndex(p => new { p.Status, p.CreatedAtUtc });
        task.HasIndex(p => new { p.Status, p.UpdatedAtUtc });
        task.HasIndex(p => new { p.Status, p.Title });

        // Priority range filtering with sorting
        task.HasIndex(p => new { p.Priority, p.SortOrder });
        task.HasIndex(p => new { p.Priority, p.DueDateUtc });
    }

}
