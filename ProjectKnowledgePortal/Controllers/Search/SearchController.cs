using Microsoft.AspNetCore.Mvc;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.ViewModels.Search;

namespace ProjectKnowledgePortal.Controllers;

public class SearchController : Controller
{
    private readonly IGlobalSearchService _globalSearchService;

    public SearchController(IGlobalSearchService globalSearchService)
    {
        _globalSearchService = globalSearchService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? query)
    {
        var results = await _globalSearchService.SearchAsync(query);

        var grouped = results
            .GroupBy(x => x.Module)
            .OrderBy(x => x.Key)
            .Select(g => new SearchGroupViewModel
            {
                Module = g.Key,
                Items = g
                    .OrderByDescending(i => i.CreatedAtUtc)
                    .Select(i => new SearchResultItemViewModel
                    {
                        Module = i.Module,
                        EntityId = i.EntityId,
                        Title = i.Title,
                        Summary = i.Summary,
                        ProjectId = i.ProjectId,
                        ProjectName = i.ProjectName,
                        CategoryName = i.CategoryName,
                        CreatedAtUtc = i.CreatedAtUtc,
                        DetailsUrl = BuildDetailsUrl(i.Module, i.EntityId)
                    })
                    .ToList()
            })
            .ToList();

        var viewModel = new GlobalSearchViewModel
        {
            Query = query,
            TotalResults = grouped.Sum(x => x.Items.Count),
            Groups = grouped
        };

        return View(viewModel);
    }

    private string BuildDetailsUrl(string module, int entityId)
    {
        return module switch
        {
            "Projects" => Url.Action("Details", "Projects", new { id = entityId }) ?? "#",
            "Documents" => Url.Action("Details", "Documents", new { id = entityId }) ?? "#",
            "Videos" => Url.Action("Details", "Videos", new { id = entityId }) ?? "#",
            "Scripts" => Url.Action("Details", "Scripts", new { id = entityId }) ?? "#",
            "Images" => Url.Action("Details", "Images", new { id = entityId }) ?? "#",
            "Links" => Url.Action("Details", "Links", new { id = entityId }) ?? "#",
            "Categories" => Url.Action("Index", "Projects", new { categoryId = entityId }) ?? "#",
            _ => "#"
        };
    }
}
