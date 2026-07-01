using System.ComponentModel.DataAnnotations;

namespace ProjectKnowledgePortal.Models.Domain;

public class Script
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public int? CategoryId { get; set; }

    public Category? Category { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string FileExtension { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string ContentType { get; set; } = string.Empty;

    [Range(0, long.MaxValue)]
    public long FileSizeBytes { get; set; }

    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(450)]
    public string UploadedByUserId { get; set; } = string.Empty;
}
