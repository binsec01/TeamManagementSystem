namespace TeamManagementSystem.Web.Services.Interfaces;

public interface IActivityService
{
    Task LogAsync(string actorUserId, string entityType, string entityId, string action, string? detailsJson = null);
}
