
using System.Collections.Generic;
using TeamManagementSystem.Web.Models;

namespace TeamManagementSystem.Web.Services;

public static class OnboardingTemplates
{
    public static List<TaskItem> CreateTemplateTasks(string templateKey, int projectId, string createdByUserId)
    {
        var tasks = new List<TaskItem>();
        switch (templateKey.ToLowerInvariant())
        {
            case "research":
                tasks.Add(new TaskItem
                {
                    ProjectId = projectId,
                    Title = "Capture initial research questions",
                    Description = "List out the key questions this research should answer.",
                    Status = TeamManagementSystem.Web.Models.TaskStatus.Backlog,
                    Priority = TaskPriority.Medium,
                    CreatedById = createdByUserId
                });
                tasks.Add(new TaskItem
                {
                    ProjectId = projectId,
                    Title = "Collect background materials",
                    Description = "Gather existing docs, reports, and links.",
                    Status = TeamManagementSystem.Web.Models.TaskStatus.InProgress,
                    Priority = TaskPriority.Medium,
                    CreatedById = createdByUserId
                });
                break;

            case "development":
                tasks.Add(new TaskItem
                {
                    ProjectId = projectId,
                    Title = "Define MVP scope",
                    Description = "List core features for the first iteration.",
                    Status = TeamManagementSystem.Web.Models.TaskStatus.Backlog,
                    Priority = TaskPriority.High,
                    CreatedById = createdByUserId
                });
                tasks.Add(new TaskItem
                {
                    ProjectId = projectId,
                    Title = "Implement first user story",
                    Description = "Pick a small but valuable piece of functionality.",
                    Status = TeamManagementSystem.Web.Models.TaskStatus.InProgress,
                    Priority = TaskPriority.High,
                    CreatedById = createdByUserId
                });
                break;
        }

        return tasks;
    }
}

