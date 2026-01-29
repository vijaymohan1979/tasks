using Microsoft.AspNetCore.Mvc;
using Tasks.Services.Entities;

namespace Tasks.WebApp.Extensions;

/// <summary>
/// Extension methods for mapping <see cref="ServiceResult"/> to <see cref="IActionResult"/>.
/// </summary>
public static class ServiceResultExtensions
{
    /// <summary>
    /// Maps a <see cref="ServiceResult{T}"/> to an appropriate <see cref="IActionResult"/>.
    /// </summary>
    public static IActionResult ToActionResult<T>(
        this ControllerBase controller,
        ServiceResult<T> result,
        Func<IActionResult> onSuccess)
    {
        return result.IsSuccess
            ? onSuccess()
            : controller.MapErrorToActionResult(result);
    }

    /// <summary>
    /// Maps a <see cref="ServiceResult"/> (non-generic) to an appropriate <see cref="IActionResult"/>.
    /// Returns 204 No Content on success.
    /// </summary>
    public static IActionResult ToActionResult(
        this ControllerBase controller,
        ServiceResult result)
    {
        return result.IsSuccess
            ? new NoContentResult()
            : controller.MapErrorToActionResult(result);
    }

    /// <summary>
    /// Maps service errors to appropriate HTTP responses using <see cref="ProblemDetails"/>.
    /// </summary>
    public static IActionResult MapErrorToActionResult(
        this ControllerBase controller,
        ServiceResult result)
    {
        var errorMessage = string.Join("; ", result.Errors);

        return result.ErrorType switch
        {
            ServiceErrorType.Validation => controller.Problem(
                detail: errorMessage,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Validation Error"),

            ServiceErrorType.NotFound => controller.Problem(
                detail: errorMessage,
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found"),

            ServiceErrorType.Conflict => controller.Problem(
                detail: errorMessage,
                statusCode: StatusCodes.Status409Conflict,
                title: "Conflict"),

            ServiceErrorType.BusinessRule => controller.Problem(
                detail: errorMessage,
                statusCode: StatusCodes.Status422UnprocessableEntity,
                title: "Business Rule Violation"),

            _ => controller.Problem(
                detail: errorMessage,
                statusCode: StatusCodes.Status500InternalServerError,
                title: "Internal Server Error")
        };
    }

}
