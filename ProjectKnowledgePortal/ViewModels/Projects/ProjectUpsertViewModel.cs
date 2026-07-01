using System.ComponentModel.DataAnnotations;

namespace ProjectKnowledgePortal.ViewModels.Projects;

public class ProjectUpsertViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    [Display(Name = "Project Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Project Code")]
    public string Code { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [StringLength(2000)]
    [Display(Name = "Team Members")]
    public string? TeamMembers { get; set; }
}
