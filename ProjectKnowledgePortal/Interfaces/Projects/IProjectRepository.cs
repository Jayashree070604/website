using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Interfaces;

public interface IProjectRepository
{
    Task<List<Project>> GetAllAsync(string? searchText);
    Task<Project?> GetByIdAsync(int id);
    Task<bool> ExistsByCodeAsync(string code, int? excludingId = null);
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task DeleteAsync(Project project);
}
