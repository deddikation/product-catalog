// =============================================================================
// File: ApiResponse.cs
// Layer: Application
// Purpose: Standard API response envelope (req 4).
//          Provides consistent response structure across all endpoints.
// =============================================================================

namespace ProductCatalog.Application.DTOs;

/// <summary>
/// Standard envelope for all API responses, providing a consistent structure
/// with success/failure indication and optional error details.
/// </summary>
/// <typeparam name="T">The type of data payload.</typeparam>
/// <param name="Success">Whether the operation completed successfully.</param>
/// <param name="Data">The response data payload (null on failure).</param>
/// <param name="Error">Error message (null on success).</param>
public record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Error
)
{
    /// <summary>
    /// Creates a successful response with the given data.
    /// </summary>
    /// <param name="data">The data payload.</param>
    /// <returns>A success ApiResponse wrapping the data.</returns>
    public static ApiResponse<T> Ok(T data) => new(true, data, null);

    /// <summary>
    /// Creates a failure response with the given error message.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failure ApiResponse with the error.</returns>
    public static ApiResponse<T> Fail(string error) => new(false, default, error);
}
