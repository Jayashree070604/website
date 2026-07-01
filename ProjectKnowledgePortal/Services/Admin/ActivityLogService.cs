using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly IActivityLogRepository _activityLogRepository;

    public ActivityLogService(IActivityLogRepository activityLogRepository)
    {
        _activityLogRepository = activityLogRepository;
    }

    public Task<List<ActivityLog>> GetAllAsync(string? activityType, string? searchText)
    {
        return _activityLogRepository.GetAllAsync(activityType, searchText);
    }

    public Task<ActivityLog?> GetByIdAsync(long id)
    {
        return _activityLogRepository.GetByIdAsync(id);
    }

    public async Task LogAsync(
        string activityType,
        string description,
        string? performedByUserId,
        int? projectId = null,
        string? entityType = null,
        string? entityId = null,
        string? ipAddress = null)
    {
        if (string.IsNullOrWhiteSpace(activityType) || string.IsNullOrWhiteSpace(description))
        {
            return;
        }

        var log = new ActivityLog
        {
            ActivityType = activityType.Trim(),
            Description = description.Trim(),
            ProjectId = projectId,
            EntityType = string.IsNullOrWhiteSpace(entityType) ? null : entityType.Trim(),
            EntityId = string.IsNullOrWhiteSpace(entityId) ? null : entityId.Trim(),
            PerformedByUserId = string.IsNullOrWhiteSpace(performedByUserId) ? null : performedByUserId.Trim(),
            IpAddress = string.IsNullOrWhiteSpace(ipAddress) ? null : ipAddress.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        await _activityLogRepository.AddAsync(log);
    }
}
