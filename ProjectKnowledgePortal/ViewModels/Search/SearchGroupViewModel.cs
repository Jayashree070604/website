namespace ProjectKnowledgePortal.ViewModels.Search;

public class SearchGroupViewModel
{
    public string Module { get; set; } = string.Empty;
    public List<SearchResultItemViewModel> Items { get; set; } = new();
}
