using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjectKnowledgePortal.ViewModels.Scripts;

public class ScriptListViewModel
{
    public string? SearchText { get; set; }
    public int? ProjectId { get; set; }

    public List<SelectListItem> ProjectOptions { get; set; } = new();
    public List<ScriptListItemViewModel> Scripts { get; set; } = new();
}
