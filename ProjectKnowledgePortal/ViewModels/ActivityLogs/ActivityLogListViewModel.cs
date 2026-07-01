namespace ProjectKnowledgePortal.ViewModels.ActivityLogs;

public class ActivityLogListViewModel
{
    public string? ActivityType { get; set; }
    public string? SearchText { get; set; }
    public List<ActivityLogListItemViewModel> Logs { get; set; } = new();
}
