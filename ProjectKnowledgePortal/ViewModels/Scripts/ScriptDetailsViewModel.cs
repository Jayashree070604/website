namespace ProjectKnowledgePortal.ViewModels.Scripts;

public class ScriptDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string FileExtension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAtUtc { get; set; }
    public string UploadedByUserId { get; set; } = string.Empty;
}
