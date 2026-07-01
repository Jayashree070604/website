using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjectKnowledgePortal.ViewModels.Links;

public class LinkUpsertViewModel
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    [Url]
    [Display(Name = "URL")]
    public string Url { get; set; } = string.Empty;

    [StringLength(2000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required]
    [Display(Name = "Project")]
    public int ProjectId { get; set; }

    [Display(Name = "Category")]
    public int? CategoryId { get; set; }

    public List<SelectListItem> ProjectOptions { get; set; } = new();
    public List<SelectListItem> CategoryOptions { get; set; } = new();
}
