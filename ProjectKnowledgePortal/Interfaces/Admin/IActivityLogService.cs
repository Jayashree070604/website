using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Interfaces;

public interface IActivityLogService
{
    Task<List<ActivityLog>> GetAllAsync(string? activityType, string? searchText);
    Task<ActivityLog?> GetByIdAsync(long id);
    Task LogAsync(string activityType, string description, string? performedByUserId, int? projectId = null, string? entityType = null, string? entityId = null, string? ipAddress = null);
}
