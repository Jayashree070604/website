namespace ProjectKnowledgePortal.ViewModels.Projects;

public class ProjectDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? TeamMembers { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
}
