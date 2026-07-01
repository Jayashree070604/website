using System.ComponentModel.DataAnnotations;

namespace ProjectKnowledgePortal.Models.Domain;

public class Link
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    public int? CategoryId { get; set; }

    public Category? Category { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    [Url]
    public string Url { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(450)]
    public string CreatedByUserId { get; set; } = string.Empty;
}
