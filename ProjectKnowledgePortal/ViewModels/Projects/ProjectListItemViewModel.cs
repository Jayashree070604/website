namespace ProjectKnowledgePortal.ViewModels.Projects;

public class ProjectListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
