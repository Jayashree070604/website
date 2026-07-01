namespace ProjectKnowledgePortal.ViewModels.Search;

public class GlobalSearchViewModel
{
    public string? Query { get; set; }
    public int TotalResults { get; set; }
    public List<SearchGroupViewModel> Groups { get; set; } = new();
}
