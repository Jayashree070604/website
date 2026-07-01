using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectKnowledgePortal.Helpers;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.ViewModels.Documents;

namespace ProjectKnowledgePortal.Controllers;

[Authorize]
public class DocumentsController : Controller
{
    private readonly IDocumentService _documentService;
    private readonly IActivityLogService _activityLogService;

    public DocumentsController(IDocumentService documentService, IActivityLogService activityLogService)
    {
        _documentService = documentService;
        _activityLogService = activityLogService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? searchText, int? projectId)
    {
        var documents = await _documentService.GetAllAsync(searchText, projectId);
        var projects = await _documentService.GetProjectsAsync();

        var viewModel = new DocumentListViewModel
        {
            SearchText = searchText,
            ProjectId = projectId,
            Documents = documents.Select(x => new DocumentListItemViewModel
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
        var document = await _documentService.GetByIdAsync(id);
        if (document is null)
        {
            return NotFound();
        }

        var viewModel = new DocumentDetailsViewModel
        {
            Id = document.Id,
            Name = document.Name,
            ProjectName = document.Project.Name,
            CategoryName = document.Category?.Name,
            FilePath = document.FilePath,
            FileExtension = document.FileExtension,
            ContentType = document.ContentType,
            FileSizeBytes = document.FileSizeBytes,
            UploadedAtUtc = document.UploadedAtUtc,
            UploadedByUserId = document.UploadedByUserId
        };

        return View(viewModel);
    }

    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Upload()
    {
        var viewModel = new DocumentUploadViewModel
        {
            ProjectOptions = await BuildProjectOptionsAsync(),
            CategoryOptions = await BuildCategoryOptionsAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Upload(DocumentUploadViewModel viewModel)
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

        var uploadResult = await _documentService.UploadAsync(
            viewModel.File!,
            viewModel.Name,
            viewModel.ProjectId,
            viewModel.CategoryId,
            currentUserId);

        if (!uploadResult.Success)
        {
            ModelState.AddModelError(string.Empty, uploadResult.Error ?? "Unable to upload document.");
            viewModel.ProjectOptions = await BuildProjectOptionsAsync();
            viewModel.CategoryOptions = await BuildCategoryOptionsAsync();
            return View(viewModel);
        }

        await LogActivityAsync("Document Uploaded", $"Document '{viewModel.Name ?? viewModel.File?.FileName}' uploaded.", viewModel.ProjectId, "Document");

        TempData["SuccessMessage"] = "Document uploaded successfully.";
        return RedirectToAction(nameof(Index));
    }

    [AllowAnonymous]
    public async Task<IActionResult> ViewFile(int id)
    {
        var fileResult = await _documentService.GetFileForAccessAsync(id, forceDownload: false);
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
        var document = await _documentService.GetByIdAsync(id);
        if (document is not null)
        {
            await LogActivityAsync("Document Downloaded", $"Document '{document.Name}' downloaded.", document.ProjectId, "Document", document.Id.ToString());
        }

        var fileResult = await _documentService.GetFileForAccessAsync(id, forceDownload: true);
        if (!fileResult.Success || string.IsNullOrWhiteSpace(fileResult.FullPath))
        {
            return NotFound(fileResult.Error);
        }

        return PhysicalFile(
            fileResult.FullPath,
            fileResult.ContentType ?? "application/octet-stream",
            fileResult.DownloadName ?? "document");
    }

    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var document = await _documentService.GetByIdAsync(id);
        if (document is null)
        {
            return NotFound();
        }

        var viewModel = new DocumentDeleteViewModel
        {
            Id = document.Id,
            Name = document.Name,
            ProjectName = document.Project.Name,
            FileExtension = document.FileExtension
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _documentService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        await LogActivityAsync("Document Deleted", $"Document with id '{id}' deleted.", entityType: "Document", entityId: id.ToString());

        TempData["SuccessMessage"] = "Document deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> BuildProjectOptionsAsync()
    {
        var projects = await _documentService.GetProjectsAsync();
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
        var categories = await _documentService.GetCategoriesAsync();
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
