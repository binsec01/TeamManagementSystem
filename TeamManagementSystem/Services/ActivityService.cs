using TeamManagementSystem.Web.Data;
using TeamManagementSystem.Web.Models;
using TeamManagementSystem.Web.Services.Interfaces;

namespace TeamManagementSystem.Web.Services;

public class ActivityService : IActivityService
{
    private readonly AppDbContext _db;

    public ActivityService(AppDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(string actorUserId, string entityType, string entityId, string action, string? detailsJson = null)
    {
        _db.ActivityLogs.Add(new ActivityLog
        {
            ActorUserId = actorUserId,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            DetailsJson = detailsJson
        });
        await _db.SaveChangesAsync();
    }
}
