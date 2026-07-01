using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectKnowledgePortal.Helpers;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Domain;
using ProjectKnowledgePortal.ViewModels.Projects;

namespace ProjectKnowledgePortal.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly IProjectService _projectService;
    private readonly IActivityLogService _activityLogService;

    public ProjectsController(IProjectService projectService, IActivityLogService activityLogService)
    {
        _projectService = projectService;
        _activityLogService = activityLogService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? searchText)
    {
        var projects = await _projectService.GetAllAsync(searchText);

        var viewModel = new ProjectListViewModel
        {
            SearchText = searchText,
            Projects = projects.Select(x => new ProjectListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                CreatedAtUtc = x.CreatedAtUtc
            }).ToList()
        };

        return View(viewModel);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project is null)
        {
            return NotFound();
        }

        var viewModel = new ProjectDetailsViewModel
        {
            Id = project.Id,
            Name = project.Name,
            Code = project.Code,
            Description = project.Description,
            TeamMembers = project.TeamMembers,
            CreatedAtUtc = project.CreatedAtUtc,
            UpdatedAtUtc = project.UpdatedAtUtc,
            CreatedByUserId = project.CreatedByUserId
        };

        return View(viewModel);
    }

    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Create()
    {
        return View(new ProjectUpsertViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Create(ProjectUpsertViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Forbid();
        }

        var project = new Project
        {
            Name = viewModel.Name,
            Code = viewModel.Code,
            Description = viewModel.Description,
            TeamMembers = viewModel.TeamMembers
        };

        var result = await _projectService.CreateAsync(project, currentUserId);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Unable to create project.");
            return View(viewModel);
        }

        await LogActivityAsync(
            activityType: "Project Created",
            description: $"Project '{project.Name}' ({project.Code}) created.",
            projectId: project.Id,
            entityType: "Project",
            entityId: project.Id.ToString());

        TempData["SuccessMessage"] = "Project created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project is null)
        {
            return NotFound();
        }

        var viewModel = new ProjectUpsertViewModel
        {
            Id = project.Id,
            Name = project.Name,
            Code = project.Code,
            Description = project.Description,
            TeamMembers = project.TeamMembers
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Edit(ProjectUpsertViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var project = new Project
        {
            Id = viewModel.Id,
            Name = viewModel.Name,
            Code = viewModel.Code,
            Description = viewModel.Description,
            TeamMembers = viewModel.TeamMembers
        };

        var result = await _projectService.UpdateAsync(project);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Unable to update project.");
            return View(viewModel);
        }

        await LogActivityAsync(
            activityType: "Project Updated",
            description: $"Project '{project.Name}' ({project.Code}) updated.",
            projectId: project.Id,
            entityType: "Project",
            entityId: project.Id.ToString());

        TempData["SuccessMessage"] = "Project updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleConstants.SuperAdmin)]
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _projectService.GetByIdAsync(id);
        if (project is null)
        {
            return NotFound();
        }

        var viewModel = new ProjectDeleteViewModel
        {
            Id = project.Id,
            Name = project.Name,
            Code = project.Code
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.SuperAdmin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _projectService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        await LogActivityAsync(
            activityType: "Project Deleted",
            description: $"Project with id '{id}' deleted.",
            projectId: id,
            entityType: "Project",
            entityId: id.ToString());

        TempData["SuccessMessage"] = "Project deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private Task LogActivityAsync(string activityType, string description, int? projectId = null, string? entityType = null, string? entityId = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        return _activityLogService.LogAsync(activityType, description, userId, projectId, entityType, entityId, ipAddress);
    }
}
