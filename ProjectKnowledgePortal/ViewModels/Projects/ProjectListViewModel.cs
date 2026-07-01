namespace ProjectKnowledgePortal.ViewModels.Projects;

public class ProjectListViewModel
{
    public string? SearchText { get; set; }

    public List<ProjectListItemViewModel> Projects { get; set; } = new();
}
