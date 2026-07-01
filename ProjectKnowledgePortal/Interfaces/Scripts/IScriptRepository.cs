using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Interfaces;

public interface IScriptRepository
{
    Task<List<Script>> GetAllAsync(string? searchText, int? projectId);
    Task<Script?> GetByIdAsync(int id);
    Task<List<Project>> GetProjectsAsync();
    Task<List<Category>> GetCategoriesAsync();
    Task<bool> ProjectExistsAsync(int projectId);
    Task<bool> CategoryExistsAsync(int categoryId);
    Task AddAsync(Script script);
    Task DeleteAsync(Script script);
}
