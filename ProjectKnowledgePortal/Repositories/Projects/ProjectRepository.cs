using Microsoft.EntityFrameworkCore;
using ProjectKnowledgePortal.Data;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProjectRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Project>> GetAllAsync(string? searchText)
    {
        var query = _dbContext.Projects
            .Include(x => x.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim();
            query = query.Where(x => x.Name.Contains(term) || x.Code.Contains(term));
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<Project?> GetByIdAsync(int id)
    {
        return await _dbContext.Projects
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<bool> ExistsByCodeAsync(string code, int? excludingId = null)
    {
        var normalizedCode = code.Trim();

        return await _dbContext.Projects.AnyAsync(x =>
            x.Code == normalizedCode && (!excludingId.HasValue || x.Id != excludingId.Value));
    }

    public async Task AddAsync(Project project)
    {
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Project project)
    {
        _dbContext.Projects.Update(project);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Project project)
    {
        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync();
    }
}
