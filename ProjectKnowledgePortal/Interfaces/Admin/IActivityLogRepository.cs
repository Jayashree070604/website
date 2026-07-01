using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Interfaces;

public interface IActivityLogRepository
{
    Task<List<ActivityLog>> GetAllAsync(string? activityType, string? searchText);
    Task<ActivityLog?> GetByIdAsync(long id);
    Task AddAsync(ActivityLog activityLog);
}
