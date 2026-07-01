using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjectKnowledgePortal.ViewModels.Videos;

public class VideoListViewModel
{
    public string? SearchText { get; set; }
    public int? ProjectId { get; set; }

    public List<SelectListItem> ProjectOptions { get; set; } = new();
    public List<VideoListItemViewModel> Videos { get; set; } = new();
}
