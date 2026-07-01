using System.ComponentModel.DataAnnotations;

namespace ProjectKnowledgePortal.Models.Domain;

public class ActivityLog
{
    public long Id { get; set; }

    public int? ProjectId { get; set; }

    public Project? Project { get; set; }

    [Required]
    [StringLength(100)]
    public string ActivityType { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [StringLength(100)]
    public string? EntityType { get; set; }

    [StringLength(100)]
    public string? EntityId { get; set; }

    [StringLength(450)]
    public string? PerformedByUserId { get; set; }

    [StringLength(100)]
    public string? IpAddress { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
