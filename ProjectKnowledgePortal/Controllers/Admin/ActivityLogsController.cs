using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<IdentityUser> _userManager;

    public ActivityLogsController(IActivityLogService activityLogService, UserManager<IdentityUser> userManager)
    {
        _activityLogService = activityLogService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? activityType, string? searchText)
    {
        var logs = await _activityLogService.GetAllAsync(activityType, searchText);
        var userIds = logs
            .Select(x => x.PerformedByUserId)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct()
            .ToList();

        var users = _userManager.Users
            .Where(x => userIds.Contains(x.Id))
            .ToDictionary(x => x.Id, x => x.Email ?? x.UserName ?? x.Id);

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
                PerformedByUser = x.PerformedByUserId is not null && users.TryGetValue(x.PerformedByUserId, out var userEmail)
                    ? userEmail
                    : x.PerformedByUserId,
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

        string? performedByUser = null;
        if (!string.IsNullOrWhiteSpace(log.PerformedByUserId))
        {
            var user = await _userManager.FindByIdAsync(log.PerformedByUserId);
            performedByUser = user?.Email ?? user?.UserName ?? log.PerformedByUserId;
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
            PerformedByUser = performedByUser,
            IpAddress = log.IpAddress,
            CreatedAtUtc = TimeZoneInfo.ConvertTimeFromUtc(log.CreatedAtUtc, IndiaTimeZone)
        };

        return View(viewModel);
    }
}
