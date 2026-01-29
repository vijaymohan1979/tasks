using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Tasks.Models.Requests;

namespace Tasks.Models.Validators;

/// <summary>
/// Validates task DTOs before persistence.
/// This ensures consistent validation across all data access operations.
/// </summary>
public class TaskModelValidator
{
    /// <summary>
    /// Validates a CreateTaskRequest.
    /// </summary>
    /// <returns>List of validation errors. Empty if valid.</returns>
    public static List<string> ValidateCreate(CreateTaskRequest dto)
    {
        var errors = new List<string>();

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        errors.AddRange(results.Select(r => r.ErrorMessage ?? "Unknown error"));

        // Custom business logic validation
        if (dto.DueDateUtc.HasValue && dto.DueDateUtc < DateTime.UtcNow)
        {
            errors.Add("DueDateUtc cannot be in the past.");
        }

        return errors;
    }

    /// <summary>
    /// Validates an UpdateTaskRequest.
    /// </summary>
    /// <returns>List of validation errors. Empty if valid.</returns>
    public static List<string> ValidateUpdate(UpdateTaskRequest dto)
    {
        var errors = new List<string>();

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        errors.AddRange(results.Select(r => r.ErrorMessage ?? "Unknown error"));

        // At least one field must be provided for update
        if (string.IsNullOrWhiteSpace(dto.Title) &&
            string.IsNullOrWhiteSpace(dto.Description) &&
            !dto.Status.HasValue &&
            !dto.Priority.HasValue &&
            !dto.DueDateUtc.HasValue &&
            !dto.SortOrder.HasValue)
        {
            errors.Add("At least one field must be provided for update.");
        }

        if (dto.DueDateUtc.HasValue && dto.DueDateUtc < DateTime.UtcNow)
        {
            errors.Add("DueDateUtc cannot be in the past.");
        }

        return errors;
    }

    /// <summary>
    /// Validates a TaskFilterRequest.
    /// </summary>
    /// <returns>List of validation errors. Empty if valid.</returns>
    public static List<string> ValidateFilter(TaskFilterRequest dto)
    {
        var errors = new List<string>();

        var context = new ValidationContext(dto);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

        errors.AddRange(results.Select(r => r.ErrorMessage ?? "Unknown error"));

        return errors;
    }
}
