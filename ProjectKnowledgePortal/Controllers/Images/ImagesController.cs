using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectKnowledgePortal.Helpers;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.ViewModels.Images;

namespace ProjectKnowledgePortal.Controllers;

[Authorize]
public class ImagesController : Controller
{
    private readonly IImageService _imageService;
    private readonly IActivityLogService _activityLogService;

    public ImagesController(IImageService imageService, IActivityLogService activityLogService)
    {
        _imageService = imageService;
        _activityLogService = activityLogService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? searchText, int? projectId)
    {
        var images = await _imageService.GetAllAsync(searchText, projectId);
        var projects = await _imageService.GetProjectsAsync();

        var viewModel = new ImageListViewModel
        {
            SearchText = searchText,
            ProjectId = projectId,
            Images = images.Select(x => new ImageListItemViewModel
            {
                Id = x.Id,
                Name = x.Name,
                ProjectName = x.Project.Name,
                CategoryName = x.Category?.Name,
                ImageUrl = Url.Action(nameof(ViewFile), new { id = x.Id }) ?? string.Empty,
                ContentType = x.ContentType,
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
        var image = await _imageService.GetByIdAsync(id);
        if (image is null)
        {
            return NotFound();
        }

        var viewModel = new ImageDetailsViewModel
        {
            Id = image.Id,
            Name = image.Name,
            ProjectName = image.Project.Name,
            CategoryName = image.Category?.Name,
            ImageUrl = Url.Action(nameof(ViewFile), new { id = image.Id }) ?? string.Empty,
            ContentType = image.ContentType,
            FileSizeBytes = image.FileSizeBytes,
            UploadedAtUtc = image.UploadedAtUtc,
            UploadedByUserId = image.UploadedByUserId
        };

        return View(viewModel);
    }

    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Upload()
    {
        var viewModel = new ImageUploadViewModel
        {
            ProjectOptions = await BuildProjectOptionsAsync(),
            CategoryOptions = await BuildCategoryOptionsAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Upload(ImageUploadViewModel viewModel)
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

        var uploadResult = await _imageService.UploadAsync(
            viewModel.File!,
            viewModel.Name,
            viewModel.ProjectId,
            viewModel.CategoryId,
            currentUserId);

        if (!uploadResult.Success)
        {
            ModelState.AddModelError(string.Empty, uploadResult.Error ?? "Unable to upload image.");
            viewModel.ProjectOptions = await BuildProjectOptionsAsync();
            viewModel.CategoryOptions = await BuildCategoryOptionsAsync();
            return View(viewModel);
        }

        await LogActivityAsync("Image Uploaded", $"Image '{viewModel.Name ?? viewModel.File?.FileName}' uploaded.", viewModel.ProjectId, "Image");

        TempData["SuccessMessage"] = "Image uploaded successfully.";
        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous]
    public async Task<IActionResult> ViewFile(int id)
    {
        var fileResult = await _imageService.GetFileForAccessAsync(id);
        if (!fileResult.Success || string.IsNullOrWhiteSpace(fileResult.FullPath))
        {
            return NotFound(fileResult.Error);
        }

        return PhysicalFile(fileResult.FullPath, fileResult.ContentType ?? "application/octet-stream");
    }

    [AllowAnonymous]
    public async Task<IActionResult> Download(int id)
    {
        var image = await _imageService.GetByIdAsync(id);
        if (image is not null)
        {
            await LogActivityAsync("Image Downloaded", $"Image '{image.Name}' downloaded.", image.ProjectId, "Image", image.Id.ToString());
        }

        var fileResult = await _imageService.GetFileForAccessAsync(id);
        if (!fileResult.Success || string.IsNullOrWhiteSpace(fileResult.FullPath))
        {
            return NotFound(fileResult.Error);
        }

        return PhysicalFile(
            fileResult.FullPath,
            fileResult.ContentType ?? "application/octet-stream",
            fileResult.DownloadName ?? "image");
    }

    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var image = await _imageService.GetByIdAsync(id);
        if (image is null)
        {
            return NotFound();
        }

        var viewModel = new ImageDeleteViewModel
        {
            Id = image.Id,
            Name = image.Name,
            ProjectName = image.Project.Name
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _imageService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        await LogActivityAsync("Image Deleted", $"Image with id '{id}' deleted.", entityType: "Image", entityId: id.ToString());

        TempData["SuccessMessage"] = "Image deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> BuildProjectOptionsAsync()
    {
        var projects = await _imageService.GetProjectsAsync();
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
        var categories = await _imageService.GetCategoriesAsync();
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
