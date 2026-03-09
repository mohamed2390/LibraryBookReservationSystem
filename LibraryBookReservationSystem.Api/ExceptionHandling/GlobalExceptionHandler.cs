using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;


namespace LibraryBookReservationSystem.Application.ExceptionHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var env = httpContext.RequestServices.GetService<IWebHostEnvironment>();

        ProblemDetails problemDetails;
        int status;
      
      if (exception is KeyNotFoundException) _logger.LogWarning(exception, "Resource not found: {Message}", exception.Message);
      else if (exception is InvalidOperationException) _logger.LogWarning(exception, "Invalid operation: {Message}", exception.Message);
      else _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

      if (exception is KeyNotFoundException)
        {
            status = StatusCodes.Status404NotFound;
            problemDetails = new ProblemDetails
            {
                Title = "Not Found",
                Detail = exception.Message,
                Status = status,
                Instance = httpContext.Request.Path
            };
        }
        else if (exception is InvalidOperationException)
        {
            status = StatusCodes.Status400BadRequest;
            problemDetails = new ProblemDetails
            {
                Title = "Invalid Operation",
                Detail = exception.Message,
                Status = status,
                Instance = httpContext.Request.Path
            };
        }
        else
        {
            status = StatusCodes.Status500InternalServerError;
         _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
         problemDetails = new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred while processing your request." + exception.Message,
                Status = status,            
                Instance = httpContext.Request.Path
            };
        }

        // Show detailed error + TraceId and stack only in Development
        if (env?.IsDevelopment() == true)
        {
            problemDetails.Detail = exception.Message;
            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = status;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true; // Exception is fully handled
    }
}
