// =============================================================================
// File: RequestLoggingMiddleware.cs
// Layer: API (Presentation)
// Purpose: Custom middleware built from scratch — NOT using framework helpers (req 7).
//          Logs HTTP request method, path, status code, and elapsed time.
// =============================================================================

using System.Diagnostics;

namespace ProductCatalog.Api.Middleware;

/// <summary>
/// Custom request logging middleware built from scratch using the RequestDelegate pattern (req 7).
///
/// This middleware:
/// 1. Captures the start time before the request enters the pipeline
/// 2. Passes the request to the next middleware via _next(context)
/// 3. Logs the HTTP method, path, response status code, and elapsed milliseconds
///
/// NOT using UseHttpLogging() or any framework helper — fully hand-written.
/// </summary>
public class RequestLoggingMiddleware
{
    /// <summary>Reference to the next middleware in the pipeline.</summary>
    private readonly RequestDelegate _next;

    /// <summary>Logger instance for writing structured log entries.</summary>
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    /// <summary>
    /// Constructs the middleware with the next delegate and logger.
    /// ASP.NET Core's DI container automatically resolves these parameters.
    /// </summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="logger">Logger for writing request log entries.</param>
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware, logging request details and elapsed time.
    /// This method is called by ASP.NET Core for each HTTP request.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        // Capture request details before passing to next middleware
        var method = context.Request.Method;
        var path = context.Request.Path;
        var queryString = context.Request.QueryString;

        // Start timing the request processing
        var stopwatch = Stopwatch.StartNew();

        // Pass request to the next middleware in the pipeline
        await _next(context);

        // Stop timing after the response has been generated
        stopwatch.Stop();

        // Log the request details with response information
        var statusCode = context.Response.StatusCode;
        var elapsed = stopwatch.ElapsedMilliseconds;

        _logger.LogInformation(
            "HTTP {Method} {Path}{QueryString} responded {StatusCode} in {Elapsed}ms",
            method, path, queryString, statusCode, elapsed);
    }
}
