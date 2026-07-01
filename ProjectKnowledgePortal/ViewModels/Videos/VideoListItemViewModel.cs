namespace ProjectKnowledgePortal.ViewModels.Videos;

public class VideoListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAtUtc { get; set; }
}
