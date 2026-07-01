using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjectKnowledgePortal.ViewModels.Videos;

public class VideoUploadViewModel
{
    [Display(Name = "Video Name")]
    [StringLength(200)]
    public string? Name { get; set; }

    [Required]
    [Display(Name = "Project")]
    public int ProjectId { get; set; }

    [Display(Name = "Category")]
    public int? CategoryId { get; set; }

    [Required]
    [Display(Name = "Video File")]
    public IFormFile? File { get; set; }

    public List<SelectListItem> ProjectOptions { get; set; } = new();
    public List<SelectListItem> CategoryOptions { get; set; } = new();
}
