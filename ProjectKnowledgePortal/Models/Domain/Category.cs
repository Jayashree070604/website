using System.ComponentModel.DataAnnotations;

namespace ProjectKnowledgePortal.Models.Domain;

public class Category
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [StringLength(450)]
    public string? CreatedByUserId { get; set; }

    public ICollection<Project> Projects { get; set; } = new List<Project>();

    public ICollection<Document> Documents { get; set; } = new List<Document>();

    public ICollection<Video> Videos { get; set; } = new List<Video>();

    public ICollection<Script> Scripts { get; set; } = new List<Script>();

    public ICollection<ProjectImage> Images { get; set; } = new List<ProjectImage>();

    public ICollection<Link> Links { get; set; } = new List<Link>();
}
