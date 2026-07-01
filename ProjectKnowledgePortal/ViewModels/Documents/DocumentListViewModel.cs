using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjectKnowledgePortal.ViewModels.Documents;

public class DocumentListViewModel
{
    public string? SearchText { get; set; }
    public int? ProjectId { get; set; }

    public List<SelectListItem> ProjectOptions { get; set; } = new();
    public List<DocumentListItemViewModel> Documents { get; set; } = new();
}
