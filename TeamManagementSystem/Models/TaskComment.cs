using TeamManagementSystem.Web.Models.Identity;

namespace TeamManagementSystem.Web.Models;

public class TaskComment
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = default!;

    public string AuthorId { get; set; } = default!;
    public ApplicationUser Author { get; set; } = default!;

    public string Body { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}