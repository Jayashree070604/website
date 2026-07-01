using Microsoft.EntityFrameworkCore;
using ProjectKnowledgePortal.Data;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Repositories;

public class DocumentRepository : IDocumentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public DocumentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Document>> GetAllAsync(string? searchText, int? projectId)
    {
        var query = _dbContext.Documents
            .Include(x => x.Project)
            .Include(x => x.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var term = searchText.Trim();
            query = query.Where(x => x.Name.Contains(term));
        }

        if (projectId.HasValue)
        {
            query = query.Where(x => x.ProjectId == projectId.Value);
        }

        return await query
            .OrderByDescending(x => x.UploadedAtUtc)
            .ToListAsync();
    }

    public async Task<Document?> GetByIdAsync(int id)
    {
        return await _dbContext.Documents
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

    public async Task AddAsync(Document document)
    {
        _dbContext.Documents.Add(document);
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Document document)
    {
        _dbContext.Documents.Remove(document);
        await _dbContext.SaveChangesAsync();
    }
}
