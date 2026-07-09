namespace ProjectKnowledgePortal.ViewModels.ActivityLogs;

public class ActivityLogListItemViewModel
{
    public long Id { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public int? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? PerformedByUser { get; set; }
    public string? IpAddress { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
