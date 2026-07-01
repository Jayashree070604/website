namespace ProjectKnowledgePortal.ViewModels.Search;

public class SearchResultItemViewModel
{
    public string Module { get; set; } = string.Empty;
    public int EntityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? CategoryName { get; set; }
    public DateTime? CreatedAtUtc { get; set; }
    public string DetailsUrl { get; set; } = string.Empty;
}
