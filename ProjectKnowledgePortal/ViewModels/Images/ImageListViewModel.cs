using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjectKnowledgePortal.ViewModels.Images;

public class ImageListViewModel
{
    public string? SearchText { get; set; }
    public int? ProjectId { get; set; }

    public List<SelectListItem> ProjectOptions { get; set; } = new();
    public List<ImageListItemViewModel> Images { get; set; } = new();
}
