namespace ProjectKnowledgePortal.ViewModels.Links;

public class LinkListItemViewModel
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
