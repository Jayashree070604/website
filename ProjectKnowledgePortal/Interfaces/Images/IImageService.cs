using Microsoft.AspNetCore.Http;
using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Interfaces;

public interface IImageService
{
    Task<List<ProjectImage>> GetAllAsync(string? searchText, int? projectId);
    Task<ProjectImage?> GetByIdAsync(int id);
    Task<List<Project>> GetProjectsAsync();
    Task<List<Category>> GetCategoriesAsync();
    Task<(bool Success, string? Error)> UploadAsync(IFormFile file, string? imageName, int projectId, int? categoryId, string uploadedByUserId);
    Task<(bool Success, string? Error, string? FullPath, string? ContentType, string? DownloadName)> GetFileForAccessAsync(int id);
    Task<bool> DeleteAsync(int id);
}
