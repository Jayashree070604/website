using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Interfaces;

public interface ILinkService
{
    Task<List<Link>> GetAllAsync(string? searchText, int? projectId, int? categoryId);
    Task<Link?> GetByIdAsync(int id);
    Task<List<Project>> GetProjectsAsync();
    Task<List<Category>> GetCategoriesAsync();
    Task<(bool Success, string? Error)> CreateAsync(Link link, string createdByUserId);
    Task<(bool Success, string? Error)> UpdateAsync(Link link);
    Task<bool> DeleteAsync(int id);
}
