using Microsoft.EntityFrameworkCore;
using ProjectKnowledgePortal.Data;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Repositories;

public class LinkRepository : ILinkRepository
{
    private readonly ApplicationDbContext _dbContext;

    public LinkRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Link>> GetAllAsync(string? searchText, int? projectId, int? categoryId)
    {
        var query = _dbContext.Links
            .Include(x => x.Project)
            .Include(x => x.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim();
            query = query.Where(x => x.Title.Contains(term) || x.Url.Contains(term));
        }

        if (projectId.HasValue)
        {
            query = query.Where(x => x.ProjectId == projectId.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();
    }

    public async Task<Link?> GetByIdAsync(int id)
    {
        return await _dbContext.Links
            .Include(x => x.Project)
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Project>> GetProjectsAsync()
    {
        return await _dbContext.Projects
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _dbContext.Categories
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public Task<bool> ProjectExistsAsync(int projectId)
    {
        return _dbContext.Projects.AnyAsync(x => x.Id == projectId);
    }

    public Task<bool> CategoryExistsAsync(int categoryId)
    {
        return _dbContext.Categories.AnyAsync(x => x.Id == categoryId);
    }

    public async Task AddAsync(Link link)
    {
        _dbContext.Links.Add(link);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Link link)
    {
        _dbContext.Links.Update(link);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Link link)
    {
        _dbContext.Links.Remove(link);
        await _dbContext.SaveChangesAsync();
    }
}
