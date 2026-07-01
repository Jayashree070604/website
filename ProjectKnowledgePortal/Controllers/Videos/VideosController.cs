using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectKnowledgePortal.Helpers;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.ViewModels.Videos;

namespace ProjectKnowledgePortal.Controllers;

[Authorize]
public class VideosController : Controller
{
    private readonly IVideoService _videoService;
    private readonly IActivityLogService _activityLogService;

    public VideosController(IVideoService videoService, IActivityLogService activityLogService)
    {
        _videoService = videoService;
        _activityLogService = activityLogService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? searchText, int? projectId)
    {
        var videos = await _videoService.GetAllAsync(searchText, projectId);
        var projects = await _videoService.GetProjectsAsync();

        var viewModel = new VideoListViewModel
        {
            SearchText = searchText,
            ProjectId = projectId,
            Videos = videos.Select(x => new VideoListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                ProjectName = x.Project.Name,
                CategoryName = x.Category?.Name,
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
        var video = await _videoService.GetByIdAsync(id);
        if (video is null)
        {
            return NotFound();
        }

        var viewModel = new VideoDetailsViewModel
        {
            Id = video.Id,
            Name = video.Name,
            ProjectName = video.Project.Name,
            CategoryName = video.Category?.Name,
            ContentType = video.ContentType,
            FileSizeBytes = video.FileSizeBytes,
            UploadedAtUtc = video.UploadedAtUtc,
            UploadedByUserId = video.UploadedByUserId
        };

        return View(viewModel);
    }

    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Upload()
    {
        var viewModel = new VideoUploadViewModel
        {
            ProjectOptions = await BuildProjectOptionsAsync(),
            CategoryOptions = await BuildCategoryOptionsAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Upload(VideoUploadViewModel viewModel)
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

        var uploadResult = await _videoService.UploadAsync(
            viewModel.File!,
            viewModel.Name,
            viewModel.ProjectId,
            viewModel.CategoryId,
            currentUserId);

        if (!uploadResult.Success)
        {
            ModelState.AddModelError(string.Empty, uploadResult.Error ?? "Unable to upload video.");
            viewModel.ProjectOptions = await BuildProjectOptionsAsync();
            viewModel.CategoryOptions = await BuildCategoryOptionsAsync();
            return View(viewModel);
        }

        await LogActivityAsync("Video Uploaded", $"Video '{viewModel.Name ?? viewModel.File?.FileName}' uploaded.", viewModel.ProjectId, "Video");

        TempData["SuccessMessage"] = "Video uploaded successfully.";
        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous]
    public async Task<IActionResult> Stream(int id)
    {
        var fileResult = await _videoService.GetFileForAccessAsync(id);
        if (!fileResult.Success || string.IsNullOrWhiteSpace(fileResult.FullPath))
        {
            return NotFound(fileResult.Error);
        }

        return PhysicalFile(
            fileResult.FullPath,
            fileResult.ContentType ?? "video/mp4",
            enableRangeProcessing: true);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Download(int id)
    {
        var video = await _videoService.GetByIdAsync(id);
        if (video is not null)
        {
            await LogActivityAsync("Video Downloaded", $"Video '{video.Name}' downloaded.", video.ProjectId, "Video", video.Id.ToString());
        }

        var fileResult = await _videoService.GetFileForAccessAsync(id);
        if (!fileResult.Success || string.IsNullOrWhiteSpace(fileResult.FullPath))
        {
            return NotFound(fileResult.Error);
        }

        return PhysicalFile(
            fileResult.FullPath,
            fileResult.ContentType ?? "video/mp4",
            fileResult.DownloadName ?? "video.mp4",
            enableRangeProcessing: true);
    }

    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var video = await _videoService.GetByIdAsync(id);
        if (video is null)
        {
            return NotFound();
        }

        var viewModel = new VideoDeleteViewModel
        {
            Id = video.Id,
            Name = video.Name,
            ProjectName = video.Project.Name
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _videoService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        await LogActivityAsync("Video Deleted", $"Video with id '{id}' deleted.", entityType: "Video", entityId: id.ToString());

        TempData["SuccessMessage"] = "Video deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> BuildProjectOptionsAsync()
    {
        var projects = await _videoService.GetProjectsAsync();
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
        var categories = await _videoService.GetCategoriesAsync();
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
