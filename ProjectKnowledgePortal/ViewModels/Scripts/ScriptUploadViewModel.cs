using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ProjectKnowledgePortal.ViewModels.Scripts;

public class ScriptUploadViewModel
{
    [Display(Name = "Script Name")]
    [StringLength(200)]
    public string? Name { get; set; }

    [Required]
    [Display(Name = "Project")]
    public int ProjectId { get; set; }

    [Display(Name = "Category")]
    public int? CategoryId { get; set; }

    [Required]
    [Display(Name = "Script File")]
    public IFormFile? File { get; set; }

    public List<SelectListItem> ProjectOptions { get; set; } = new();
    public List<SelectListItem> CategoryOptions { get; set; } = new();
}
