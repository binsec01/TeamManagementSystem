using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Http;
using TeamManagementSystem.Web.Services.Interfaces;

namespace TeamManagementSystem.Web.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IActivityService? activityService)
    {
        if (activityService == null)
        {
            await _next(context);
            return;
        }

        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var path = context.Request.Path.Value ?? "";
        var method = context.Request.Method;

        var sw = Stopwatch.StartNew();
        await _next(context);
        sw.Stop();

        if (!string.IsNullOrEmpty(userId) && (method == "POST" || method == "PUT" || method == "DELETE"))
        {
            try
            {
                var action = method switch
                {
                    "POST" => "Create",
                    "PUT" => "Update",
                    "DELETE" => "Delete",
                    _ => method
                };
                await activityService.LogAsync(userId, "HttpRequest", path, action, $"{{\"Method\":\"{method}\",\"StatusCode\":{context.Response.StatusCode}}}");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Audit log failed for {Path}", path);
            }
        }
    }
}
