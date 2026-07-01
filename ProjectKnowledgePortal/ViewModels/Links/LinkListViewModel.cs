using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjectKnowledgePortal.ViewModels.Links;

public class LinkListViewModel
{
    public string? SearchText { get; set; }
    public int? ProjectId { get; set; }
    public int? CategoryId { get; set; }

    public List<SelectListItem> ProjectOptions { get; set; } = new();
    public List<SelectListItem> CategoryOptions { get; set; } = new();
    public List<LinkListItemViewModel> Links { get; set; } = new();
}
