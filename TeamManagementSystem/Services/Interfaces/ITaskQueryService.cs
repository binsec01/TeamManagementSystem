using TeamManagementSystem.Web.Models;
using TeamManagementSystem.Web.ViewModels.Tasks;

namespace TeamManagementSystem.Web.Services.Interfaces;

public interface ITaskQueryService
{
    Task<TaskListVm> GetTaskListAsync(int projectId, string? statusFilter, string? assigneeIdFilter, string? search);
    Task<TaskBoardVm> GetTaskBoardAsync(int projectId);
}
