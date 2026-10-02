/*
File role: ApiResponse.cs
Layer: Shared
Purpose: Standard response envelope used by API controllers to return consistent success/data/message payloads without leaking ASP.NET types into core layers.
Business flow: Application result -> ApiResponse<T> -> HTTP response.
Design note: This public reference keeps dependencies directed inward so AI/cloud/storage providers can be replaced without changing core use-case logic.
*/

namespace URTech.EnterpriseRAG.Shared.Common;

/*
Standard API envelope used by the public reference application.
It keeps success/error responses consistent without coupling the core layers
with ASP.NET Core response types.
*/
public sealed record ApiResponse<T>(bool Success, T? Data, string? Message)
{
    public static ApiResponse<T> Ok(T data, string? message = null) =>
        new(true, data, message);

    public static ApiResponse<T> Fail(string message) =>
        new(false, default, message);
}
