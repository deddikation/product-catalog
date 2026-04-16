// =============================================================================
// File: ExceptionHandlingMiddleware.cs
// Layer: API (Presentation)
// Purpose: Custom exception handling middleware built from scratch (req 7).
//          Uses pattern matching (req 5) to map domain exceptions to HTTP status codes.
//          NOT using UseExceptionHandler() or any framework helper.
// =============================================================================

using System.Text.Json;
using ProductCatalog.Application.DTOs;
using ProductCatalog.Domain.Exceptions;

namespace ProductCatalog.Api.Middleware;

/// <summary>
/// Custom exception handling middleware built from scratch using the RequestDelegate pattern (req 7).
///
/// This middleware wraps the entire pipeline in a try-catch and converts
/// unhandled exceptions into structured JSON error responses.
///
/// Uses C# pattern matching (req 5) to map exception types to HTTP status codes:
/// - NotFoundException → 404 Not Found
/// - ValidationException → 400 Bad Request
/// - DuplicateException → 409 Conflict
/// - All others → 500 Internal Server Error
///
/// NOT using UseExceptionHandler(), UseStatusCodePages(), or any framework helper.
/// </summary>
public class ExceptionHandlingMiddleware
{
    /// <summary>Reference to the next middleware in the pipeline.</summary>
    private readonly RequestDelegate _next;

    /// <summary>Logger for recording exception details.</summary>
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    /// <summary>
    /// Constructs the middleware with the next delegate and logger.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="logger">Logger for exception logging.</param>
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware, catching any unhandled exceptions and converting
    /// them into structured JSON error responses with appropriate HTTP status codes.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Pass request to the next middleware — if no exception, this returns normally
            await _next(context);
        }
        catch (Exception ex)
        {
            // Log the exception with full details for debugging
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);

            // Use pattern matching to determine HTTP status code and user-facing message (req 5)
            var (statusCode, message) = ex switch
            {
                NotFoundException notFound => (StatusCodes.Status404NotFound, notFound.Message),
                Domain.Exceptions.ValidationException validation => (StatusCodes.Status400BadRequest, validation.Message),
                DuplicateException duplicate => (StatusCodes.Status409Conflict, duplicate.Message),
                ArgumentException argument => (StatusCodes.Status400BadRequest, argument.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred. Please try again later.")
            };

            // Build structured error response
            var response = ApiResponse<object>.Fail(message);

            // Write the JSON error response directly to the response body
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}
