using Microsoft.EntityFrameworkCore;
using TeamManagementSystem.Web.Data;
using TeamManagementSystem.Web.Models;
using TeamManagementSystem.Web.Services.Interfaces;
using TeamManagementSystem.Web.ViewModels.Tasks;
using TaskStatusEnum = TeamManagementSystem.Web.Models.TaskStatus;

namespace TeamManagementSystem.Web.Services;

public class TaskQueryService : ITaskQueryService
{
    private readonly AppDbContext _db;

    public TaskQueryService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<TaskListVm> GetTaskListAsync(int projectId, string? statusFilter, string? assigneeIdFilter, string? search)
    {
        var project = await _db.Projects.AsNoTracking().FirstAsync(p => p.Id == projectId);
        var query = _db.Tasks
            .AsNoTracking()
            .Include(t => t.Assignee)
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .Where(t => t.ProjectId == projectId);

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<TaskStatusEnum>(statusFilter, out var status))
            query = query.Where(t => t.Status == status);
        if (!string.IsNullOrWhiteSpace(assigneeIdFilter))
            query = query.Where(t => t.AssigneeId == assigneeIdFilter);
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Title.Contains(search) || (t.Description != null && t.Description.Contains(search)));

        var tasks = await query.OrderBy(t => t.Status).ThenBy(t => t.DueDateUtc).ToListAsync();
        var list = new TaskListVm
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            Tasks = tasks.Select(t => new TaskRowVm
            {
                Id = t.Id,
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                DueDateUtc = t.DueDateUtc,
                AssigneeName = t.Assignee?.FullName ?? t.Assignee?.Email,
                TagNames = t.TaskTags.Select(tt => tt.Tag.Name).ToList()
            }).ToList(),
            FilterStatus = statusFilter,
            FilterAssigneeId = assigneeIdFilter,
            Search = search
        };
        return list;
    }

    public async Task<TaskBoardVm> GetTaskBoardAsync(int projectId)
    {
        var project = await _db.Projects.AsNoTracking().FirstAsync(p => p.Id == projectId);
        var tasks = await _db.Tasks
            .AsNoTracking()
            .Include(t => t.Assignee)
            .Include(t => t.TaskTags)
            .ThenInclude(tt => tt.Tag)
            .Where(t => t.ProjectId == projectId)
            .ToListAsync();

        var columns = Enum.GetValues<TaskStatusEnum>().ToDictionary(s => s, _ => new List<TaskCardVm>());
        foreach (var t in tasks)
        {
            columns[t.Status].Add(new TaskCardVm
            {
                Id = t.Id,
                Title = t.Title,
                Priority = t.Priority,
                DueDateUtc = t.DueDateUtc,
                AssigneeName = t.Assignee?.FullName ?? t.Assignee?.Email,
                AssigneeId = t.AssigneeId,
                TagNames = t.TaskTags.Select(tt => tt.Tag.Name).ToList()
            });
        }

        return new TaskBoardVm
        {
            ProjectId = projectId,
            ProjectName = project.Name,
            Columns = columns
        };
    }
}
