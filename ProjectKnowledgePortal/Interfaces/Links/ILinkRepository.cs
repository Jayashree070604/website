using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Interfaces;

public interface ILinkRepository
{
    Task<List<Link>> GetAllAsync(string? searchText, int? projectId, int? categoryId);
    Task<Link?> GetByIdAsync(int id);
    Task<List<Project>> GetProjectsAsync();
    Task<List<Category>> GetCategoriesAsync();
    Task<bool> ProjectExistsAsync(int projectId);
    Task<bool> CategoryExistsAsync(int categoryId);
    Task AddAsync(Link link);
    Task UpdateAsync(Link link);
    Task DeleteAsync(Link link);
}
