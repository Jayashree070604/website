using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectKnowledgePortal.Helpers;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.ViewModels.Scripts;

namespace ProjectKnowledgePortal.Controllers;

[Authorize]
public class ScriptsController : Controller
{
    private readonly IScriptService _scriptService;
    private readonly IActivityLogService _activityLogService;

    public ScriptsController(IScriptService scriptService, IActivityLogService activityLogService)
    {
        _scriptService = scriptService;
        _activityLogService = activityLogService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? searchText, int? projectId)
    {
        var scripts = await _scriptService.GetAllAsync(searchText, projectId);
        var projects = await _scriptService.GetProjectsAsync();

        var viewModel = new ScriptListViewModel
        {
            SearchText = searchText,
            ProjectId = projectId,
            Scripts = scripts.Select(x => new ScriptListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                ProjectName = x.Project.Name,
                CategoryName = x.Category?.Name,
                FileExtension = x.FileExtension,
                FileSizeBytes = x.FileSizeBytes,
                UploadedAtUtc = x.UploadedAtUtc
            }).ToList(),
            ProjectOptions = projects.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            }).ToList()
        };

        return View(viewModel);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var script = await _scriptService.GetByIdAsync(id);
        if (script is null)
        {
            return NotFound();
        }

        var viewModel = new ScriptDetailsViewModel
        {
            Id = script.Id,
            Name = script.Name,
            ProjectName = script.Project.Name,
            CategoryName = script.Category?.Name,
            FileExtension = script.FileExtension,
            ContentType = script.ContentType,
            FileSizeBytes = script.FileSizeBytes,
            UploadedAtUtc = script.UploadedAtUtc,
            UploadedByUserId = script.UploadedByUserId
        };

        return View(viewModel);
    }

    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Upload()
    {
        var viewModel = new ScriptUploadViewModel
        {
            ProjectOptions = await BuildProjectOptionsAsync(),
            CategoryOptions = await BuildCategoryOptionsAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Upload(ScriptUploadViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            viewModel.ProjectOptions = await BuildProjectOptionsAsync();
            viewModel.CategoryOptions = await BuildCategoryOptionsAsync();
            return View(viewModel);
        }

        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Forbid();
        }

        var uploadResult = await _scriptService.UploadAsync(
            viewModel.File!,
            viewModel.Name,
            viewModel.ProjectId,
            viewModel.CategoryId,
            currentUserId);

        if (!uploadResult.Success)
        {
            ModelState.AddModelError(string.Empty, uploadResult.Error ?? "Unable to upload script.");
            viewModel.ProjectOptions = await BuildProjectOptionsAsync();
            viewModel.CategoryOptions = await BuildCategoryOptionsAsync();
            return View(viewModel);
        }

        await LogActivityAsync("Script Uploaded", $"Script '{viewModel.Name ?? viewModel.File?.FileName}' uploaded.", viewModel.ProjectId, "Script");

        TempData["SuccessMessage"] = "Script uploaded successfully.";
        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous]
    public async Task<IActionResult> ViewFile(int id)
    {
        var fileResult = await _scriptService.GetFileForAccessAsync(id, forceDownload: false);
        if (!fileResult.Success || string.IsNullOrWhiteSpace(fileResult.FullPath))
        {
            return NotFound(fileResult.Error);
        }

        Response.Headers.ContentDisposition = fileResult.Inline
            ? $"inline; filename=\"{fileResult.DownloadName}\""
            : $"attachment; filename=\"{fileResult.DownloadName}\"";

        return PhysicalFile(fileResult.FullPath, fileResult.ContentType ?? "application/octet-stream");
    }

    [AllowAnonymous]
    public async Task<IActionResult> Download(int id)
    {
        var script = await _scriptService.GetByIdAsync(id);
        if (script is not null)
        {
            await LogActivityAsync("Script Downloaded", $"Script '{script.Name}' downloaded.", script.ProjectId, "Script", script.Id.ToString());
        }

        var fileResult = await _scriptService.GetFileForAccessAsync(id, forceDownload: true);
        if (!fileResult.Success || string.IsNullOrWhiteSpace(fileResult.FullPath))
        {
            return NotFound(fileResult.Error);
        }

        return PhysicalFile(
            fileResult.FullPath,
            fileResult.ContentType ?? "application/octet-stream",
            fileResult.DownloadName ?? "script");
    }

    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var script = await _scriptService.GetByIdAsync(id);
        if (script is null)
        {
            return NotFound();
        }

        var viewModel = new ScriptDeleteViewModel
        {
            Id = script.Id,
            Name = script.Name,
            ProjectName = script.Project.Name,
            FileExtension = script.FileExtension
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _scriptService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        await LogActivityAsync("Script Deleted", $"Script with id '{id}' deleted.", entityType: "Script", entityId: id.ToString());

        TempData["SuccessMessage"] = "Script deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> BuildProjectOptionsAsync()
    {
        var projects = await _scriptService.GetProjectsAsync();
        var options = new List<SelectListItem>
        {
            new() { Value = string.Empty, Text = "-- Select Project --" }
        };

        options.AddRange(projects.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.Name
        }));

        return options;
    }

    private async Task<List<SelectListItem>> BuildCategoryOptionsAsync()
    {
        var categories = await _scriptService.GetCategoriesAsync();
        var options = new List<SelectListItem>
        {
            new() { Value = string.Empty, Text = "-- Select Category --" }
        };

        options.AddRange(categories.Select(x => new SelectListItem
        {
            Value = x.Id.ToString(),
            Text = x.Name
        }));

        return options;
    }

    private Task LogActivityAsync(string activityType, string description, int? projectId = null, string? entityType = null, string? entityId = null)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        return _activityLogService.LogAsync(activityType, description, userId, projectId, entityType, entityId, ipAddress);
    }
}
