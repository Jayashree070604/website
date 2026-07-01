using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Interfaces;

public interface IDocumentRepository
{
    Task<List<Document>> GetAllAsync(string? searchText, int? projectId);
    Task<Document?> GetByIdAsync(int id);
    Task<List<Project>> GetProjectsAsync();
    Task<List<Category>> GetCategoriesAsync();
    Task<bool> ProjectExistsAsync(int projectId);
    Task<bool> CategoryExistsAsync(int categoryId);
    Task AddAsync(Document document);
    Task DeleteAsync(Document document);
}
