using Microsoft.EntityFrameworkCore;
using ProjectKnowledgePortal.Data;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Repositories;

public class ActivityLogRepository : IActivityLogRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ActivityLogRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<ActivityLog>> GetAllAsync(string? activityType, string? searchText)
    {
        var query = _dbContext.ActivityLogs
            .Include(x => x.Project)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(activityType))
        {
            var type = activityType.Trim();
            query = query.Where(x => x.ActivityType == type);
        }

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim();
            query = query.Where(x =>
                x.Description.Contains(term) ||
                (x.EntityType != null && x.EntityType.Contains(term)) ||
                (x.EntityId != null && x.EntityId.Contains(term)) ||
                (x.PerformedByUserId != null && x.PerformedByUserId.Contains(term)));
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<ActivityLog?> GetByIdAsync(long id)
    {
        return await _dbContext.ActivityLogs
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(ActivityLog activityLog)
    {
        _dbContext.ActivityLogs.Add(activityLog);
        await _dbContext.SaveChangesAsync();
    }
}
