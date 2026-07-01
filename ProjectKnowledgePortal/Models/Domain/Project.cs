using System.ComponentModel.DataAnnotations;

namespace ProjectKnowledgePortal.Models.Domain;

public class Project
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(2000)]
    public string? TeamMembers { get; set; }

    public bool IsArchived { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }

    [Required]
    [StringLength(450)]
    public string CreatedByUserId { get; set; } = string.Empty;

    public int? CategoryId { get; set; }

    public Category? Category { get; set; }

    public ICollection<Document> Documents { get; set; } = new List<Document>();

    public ICollection<Video> Videos { get; set; } = new List<Video>();

    public ICollection<Script> Scripts { get; set; } = new List<Script>();

    public ICollection<ProjectImage> Images { get; set; } = new List<ProjectImage>();

    public ICollection<Link> Links { get; set; } = new List<Link>();

    public ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
}
