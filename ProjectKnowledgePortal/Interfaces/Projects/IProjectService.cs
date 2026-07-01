using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Interfaces;

public interface IProjectService
{
    Task<List<Project>> GetAllAsync(string? searchText);
    Task<Project?> GetByIdAsync(int id);
    Task<List<Category>> GetCategoriesAsync();
    Task<(bool Success, string? Error)> CreateAsync(Project project, string currentUserId);
    Task<(bool Success, string? Error)> UpdateAsync(Project project);
    Task<bool> DeleteAsync(int id);
}
