using Microsoft.AspNetCore.Http;
using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Interfaces;

public interface IDocumentService
{
    Task<List<Document>> GetAllAsync(string? searchText, int? projectId);
    Task<Document?> GetByIdAsync(int id);
    Task<List<Project>> GetProjectsAsync();
    Task<List<Category>> GetCategoriesAsync();
    Task<(bool Success, string? Error)> UploadAsync(IFormFile file, string? documentName, int projectId, int? categoryId, string uploadedByUserId);
    Task<(bool Success, string? Error, string? FullPath, string? ContentType, string? DownloadName, bool Inline)> GetFileForAccessAsync(int id, bool forceDownload);
    Task<bool> DeleteAsync(int id);
}
