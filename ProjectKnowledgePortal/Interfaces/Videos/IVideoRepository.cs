using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Interfaces;

public interface IVideoRepository
{
    Task<List<Video>> GetAllAsync(string? searchText, int? projectId);
    Task<Video?> GetByIdAsync(int id);
    Task<List<Project>> GetProjectsAsync();
    Task<List<Category>> GetCategoriesAsync();
    Task<bool> ProjectExistsAsync(int projectId);
    Task<bool> CategoryExistsAsync(int categoryId);
    Task AddAsync(Video video);
    Task DeleteAsync(Video video);
}
