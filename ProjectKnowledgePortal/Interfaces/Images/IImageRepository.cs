using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Interfaces;

public interface IImageRepository
{
    Task<List<ProjectImage>> GetAllAsync(string? searchText, int? projectId);
    Task<ProjectImage?> GetByIdAsync(int id);
    Task<List<Project>> GetProjectsAsync();
    Task<List<Category>> GetCategoriesAsync();
    Task<bool> ProjectExistsAsync(int projectId);
    Task<bool> CategoryExistsAsync(int categoryId);
    Task AddAsync(ProjectImage image);
    Task DeleteAsync(ProjectImage image);
}
