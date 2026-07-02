using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectKnowledgePortal.Helpers;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.ViewModels.ActivityLogs;

namespace ProjectKnowledgePortal.Controllers;

[Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
public class ActivityLogsController : Controller
{
    private static readonly TimeZoneInfo IndiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
    private readonly IActivityLogService _activityLogService;

    public ActivityLogsController(IActivityLogService activityLogService)
    {
        _activityLogService = activityLogService;
    }

    public async Task<IActionResult> Index(string? activityType, string? searchText)
    {
        var logs = await _activityLogService.GetAllAsync(activityType, searchText);

        var viewModel = new ActivityLogListViewModel
        {
            ActivityType = activityType,
            SearchText = searchText,
            Logs = logs.Select(x => new ActivityLogListItemViewModel
            {
                Id = x.Id,
                ActivityType = x.ActivityType,
                Description = x.Description,
                EntityType = x.EntityType,
                EntityId = x.EntityId,
                ProjectId = x.ProjectId,
                ProjectName = x.Project?.Name,
                PerformedByUserId = x.PerformedByUserId,
                IpAddress = x.IpAddress,
                CreatedAtUtc = TimeZoneInfo.ConvertTimeFromUtc(x.CreatedAtUtc, IndiaTimeZone)
            }).ToList()
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(long id)
    {
        var log = await _activityLogService.GetByIdAsync(id);
        if (log is null)
        {
            return NotFound();
        }

        var viewModel = new ActivityLogDetailsViewModel
        {
            Id = log.Id,
            ActivityType = log.ActivityType,
            Description = log.Description,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            ProjectId = log.ProjectId,
            ProjectName = log.Project?.Name,
            PerformedByUserId = log.PerformedByUserId,
            IpAddress = log.IpAddress,
            CreatedAtUtc = TimeZoneInfo.ConvertTimeFromUtc(log.CreatedAtUtc, IndiaTimeZone)
        };

        return View(viewModel);
    }
}
