namespace TeamManagementSystem.Web.ViewModels.Reports;

public class ActivityReportVm
{
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = default!;
    public DateTime? FromUtc { get; set; }
    public DateTime? ToUtc { get; set; }
    public List<ActivityRowVm> Activities { get; set; } = new();
}

public class ActivityRowVm
{
    public int Id { get; set; }
    public string ActorUserId { get; set; } = default!;
    public string? ActorName { get; set; }
    public string EntityType { get; set; } = default!;
    public string EntityId { get; set; } = default!;
    public string Action { get; set; } = default!;
    public string? DetailsJson { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
