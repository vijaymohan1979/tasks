using System;
using System.Collections.Generic;
using System.Text;

namespace Tasks.Services.Entities;

/// <summary>
/// Represents the result of a service operation with success/failure state and errors.
/// </summary>
public class ServiceResult
{
    public bool IsSuccess { get; protected set; }
    public List<string> Errors { get; protected set; } = [];
    public ServiceErrorType ErrorType { get; protected set; } = ServiceErrorType.None;

    public static ServiceResult Success() => new() { IsSuccess = true };

    public static ServiceResult Failure(List<string> errors, ServiceErrorType errorType = ServiceErrorType.Validation)
        => new() { IsSuccess = false, Errors = errors, ErrorType = errorType };

    public static ServiceResult Failure(string error, ServiceErrorType errorType = ServiceErrorType.Validation)
        => new() { IsSuccess = false, Errors = [error], ErrorType = errorType };

    public static ServiceResult NotFound(string message = "Resource not found.")
        => new() { IsSuccess = false, Errors = [message], ErrorType = ServiceErrorType.NotFound };

    public static ServiceResult Conflict(string message)
        => new() { IsSuccess = false, Errors = [message], ErrorType = ServiceErrorType.Conflict };
}
