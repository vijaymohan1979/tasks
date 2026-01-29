using System;
using System.Collections.Generic;
using System.Text;

namespace Tasks.Services.Entities;

/// <summary>
/// Represents the result of a service operation that returns data.
/// </summary>
public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; private set; }

    public static ServiceResult<T> Success(T data)
        => new() { IsSuccess = true, Data = data };

    public new static ServiceResult<T> Failure(List<string> errors, ServiceErrorType errorType = ServiceErrorType.Validation)
        => new() { IsSuccess = false, Errors = errors, ErrorType = errorType };

    public new static ServiceResult<T> Failure(string error, ServiceErrorType errorType = ServiceErrorType.Validation)
        => new() { IsSuccess = false, Errors = [error], ErrorType = errorType };

    public new static ServiceResult<T> NotFound(string message = "Resource not found.")
        => new() { IsSuccess = false, Errors = [message], ErrorType = ServiceErrorType.NotFound };

    public new static ServiceResult<T> Conflict(string message)
        => new() { IsSuccess = false, Errors = [message], ErrorType = ServiceErrorType.Conflict };
}
