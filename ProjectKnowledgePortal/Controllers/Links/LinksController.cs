using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ProjectKnowledgePortal.Helpers;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Domain;
using ProjectKnowledgePortal.ViewModels.Links;

namespace ProjectKnowledgePortal.Controllers;

[Authorize]
public class LinksController : Controller
{
    private readonly ILinkService _linkService;
    private readonly IActivityLogService _activityLogService;

    public LinksController(ILinkService linkService, IActivityLogService activityLogService)
    {
        _linkService = linkService;
        _activityLogService = activityLogService;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? searchText, int? projectId, int? categoryId)
    {
        var links = await _linkService.GetAllAsync(searchText, projectId, categoryId);

        var viewModel = new LinkListViewModel
        {
            SearchText = searchText,
            ProjectId = projectId,
            CategoryId = categoryId,
            Links = links.Select(x => new LinkListItemViewModel
            {
                Id = x.Id,
                Title = x.Title,
                Url = x.Url,
                ProjectName = x.Project.Name,
                CategoryName = x.Category?.Name,
                CreatedAtUtc = x.CreatedAtUtc
            }).ToList(),
            ProjectOptions = await BuildProjectOptionsAsync(),
            CategoryOptions = await BuildCategoryOptionsAsync()
        };

        return View(viewModel);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var link = await _linkService.GetByIdAsync(id);
        if (link is null)
        {
            return NotFound();
        }

        var viewModel = new LinkDetailsViewModel
        {
            Id = link.Id,
            Title = link.Title,
            Url = link.Url,
            Description = link.Description,
            ProjectName = link.Project.Name,
            CategoryName = link.Category?.Name,
            CreatedAtUtc = link.CreatedAtUtc,
            CreatedByUserId = link.CreatedByUserId
        };

        return View(viewModel);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Open(int id)
    {
        var link = await _linkService.GetByIdAsync(id);
        if (link is null)
        {
            return NotFound();
        }

        if (!Uri.TryCreate(link.Url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return BadRequest("Invalid link URL.");
        }

        await LogActivityAsync("Link Opened", $"Link '{link.Title}' opened.", link.ProjectId, "Link", link.Id.ToString());

        return Redirect(uri.ToString());
    }

    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Create()
    {
        var viewModel = new LinkUpsertViewModel
        {
            ProjectOptions = await BuildProjectOptionsAsync(),
            CategoryOptions = await BuildCategoryOptionsAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Create(LinkUpsertViewModel viewModel)
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

        var link = new Link
        {
            Title = viewModel.Title,
            Url = viewModel.Url,
            Description = viewModel.Description,
            ProjectId = viewModel.ProjectId,
            CategoryId = viewModel.CategoryId
        };

        var result = await _linkService.CreateAsync(link, currentUserId);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Unable to create link.");
            viewModel.ProjectOptions = await BuildProjectOptionsAsync();
            viewModel.CategoryOptions = await BuildCategoryOptionsAsync();
            return View(viewModel);
        }

        await LogActivityAsync("Link Created", $"Link '{link.Title}' created.", link.ProjectId, "Link", link.Id.ToString());

        TempData["SuccessMessage"] = "Link created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Edit(int id)
    {
        var link = await _linkService.GetByIdAsync(id);
        if (link is null)
        {
            return NotFound();
        }

        var viewModel = new LinkUpsertViewModel
        {
            Id = link.Id,
            Title = link.Title,
            Url = link.Url,
            Description = link.Description,
            ProjectId = link.ProjectId,
            CategoryId = link.CategoryId,
            ProjectOptions = await BuildProjectOptionsAsync(),
            CategoryOptions = await BuildCategoryOptionsAsync()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Edit(LinkUpsertViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            viewModel.ProjectOptions = await BuildProjectOptionsAsync();
            viewModel.CategoryOptions = await BuildCategoryOptionsAsync();
            return View(viewModel);
        }

        var link = new Link
        {
            Id = viewModel.Id,
            Title = viewModel.Title,
            Url = viewModel.Url,
            Description = viewModel.Description,
            ProjectId = viewModel.ProjectId,
            CategoryId = viewModel.CategoryId
        };

        var result = await _linkService.UpdateAsync(link);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Unable to update link.");
            viewModel.ProjectOptions = await BuildProjectOptionsAsync();
            viewModel.CategoryOptions = await BuildCategoryOptionsAsync();
            return View(viewModel);
        }

        await LogActivityAsync("Link Updated", $"Link '{link.Title}' updated.", link.ProjectId, "Link", link.Id.ToString());

        TempData["SuccessMessage"] = "Link updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> Delete(int id)
    {
        var link = await _linkService.GetByIdAsync(id);
        if (link is null)
        {
            return NotFound();
        }

        var viewModel = new LinkDeleteViewModel
        {
            Id = link.Id,
            Title = link.Title,
            Url = link.Url,
            ProjectName = link.Project.Name
        };

        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = RoleConstants.SuperAdmin + "," + RoleConstants.Admin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var deleted = await _linkService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound();
        }

        await LogActivityAsync("Link Deleted", $"Link with id '{id}' deleted.", entityType: "Link", entityId: id.ToString());

        TempData["SuccessMessage"] = "Link deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<List<SelectListItem>> BuildProjectOptionsAsync()
    {
        var projects = await _linkService.GetProjectsAsync();
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
        var categories = await _linkService.GetCategoriesAsync();
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
