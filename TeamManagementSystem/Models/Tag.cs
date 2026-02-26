namespace TeamManagementSystem.Web.Models;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
}

public class TaskTag
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = default!;
    public int TagId { get; set; }
    public Tag Tag { get; set; } = default!;
}