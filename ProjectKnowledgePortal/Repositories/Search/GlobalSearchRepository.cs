using Microsoft.EntityFrameworkCore;
using ProjectKnowledgePortal.Data;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Search;

namespace ProjectKnowledgePortal.Repositories;

public class GlobalSearchRepository : IGlobalSearchRepository
{
    private readonly ApplicationDbContext _dbContext;

    public GlobalSearchRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<GlobalSearchItem>> SearchAsync(string searchText, int maxPerModule = 30)
    {
        var term = searchText.Trim();
        var results = new List<GlobalSearchItem>();

        var projects = await _dbContext.Projects
            .Include(x => x.Category)
            .Where(x =>
                x.Name.Contains(term) ||
                x.Code.Contains(term) ||
                (x.Description != null && x.Description.Contains(term)) ||
                (x.TeamMembers != null && x.TeamMembers.Contains(term)))
            .OrderBy(x => x.Name)
            .Take(maxPerModule)
            .Select(x => new GlobalSearchItem
            {
                Module = "Projects",
                EntityId = x.Id,
                Title = x.Name,
                Summary = x.Code,
                ProjectId = x.Id,
                ProjectName = x.Name,
                CategoryId = x.CategoryId,
                CategoryName = x.Category != null ? x.Category.Name : null,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        results.AddRange(projects);

        var documents = await _dbContext.Documents
            .Include(x => x.Project)
            .Include(x => x.Category)
            .Where(x => x.Name.Contains(term))
            .OrderByDescending(x => x.UploadedAtUtc)
            .Take(maxPerModule)
            .Select(x => new GlobalSearchItem
            {
                Module = "Documents",
                EntityId = x.Id,
                Title = x.Name,
                Summary = x.FileExtension,
                ProjectId = x.ProjectId,
                ProjectName = x.Project.Name,
                CategoryId = x.CategoryId,
                CategoryName = x.Category != null ? x.Category.Name : null,
                CreatedAtUtc = x.UploadedAtUtc
            })
            .ToListAsync();

        results.AddRange(documents);

        var videos = await _dbContext.Videos
            .Include(x => x.Project)
            .Include(x => x.Category)
            .Where(x => x.Name.Contains(term))
            .OrderByDescending(x => x.UploadedAtUtc)
            .Take(maxPerModule)
            .Select(x => new GlobalSearchItem
            {
                Module = "Videos",
                EntityId = x.Id,
                Title = x.Name,
                Summary = "MP4",
                ProjectId = x.ProjectId,
                ProjectName = x.Project.Name,
                CategoryId = x.CategoryId,
                CategoryName = x.Category != null ? x.Category.Name : null,
                CreatedAtUtc = x.UploadedAtUtc
            })
            .ToListAsync();

        results.AddRange(videos);

        var scripts = await _dbContext.Scripts
            .Include(x => x.Project)
            .Include(x => x.Category)
            .Where(x => x.Name.Contains(term))
            .OrderByDescending(x => x.UploadedAtUtc)
            .Take(maxPerModule)
            .Select(x => new GlobalSearchItem
            {
                Module = "Scripts",
                EntityId = x.Id,
                Title = x.Name,
                Summary = x.FileExtension,
                ProjectId = x.ProjectId,
                ProjectName = x.Project.Name,
                CategoryId = x.CategoryId,
                CategoryName = x.Category != null ? x.Category.Name : null,
                CreatedAtUtc = x.UploadedAtUtc
            })
            .ToListAsync();

        results.AddRange(scripts);

        var images = await _dbContext.Images
            .Include(x => x.Project)
            .Include(x => x.Category)
            .Where(x => x.Name.Contains(term))
            .OrderByDescending(x => x.UploadedAtUtc)
            .Take(maxPerModule)
            .Select(x => new GlobalSearchItem
            {
                Module = "Images",
                EntityId = x.Id,
                Title = x.Name,
                Summary = x.ContentType,
                ProjectId = x.ProjectId,
                ProjectName = x.Project.Name,
                CategoryId = x.CategoryId,
                CategoryName = x.Category != null ? x.Category.Name : null,
                CreatedAtUtc = x.UploadedAtUtc
            })
            .ToListAsync();

        results.AddRange(images);

        var links = await _dbContext.Links
            .Include(x => x.Project)
            .Include(x => x.Category)
            .Where(x =>
                x.Title.Contains(term) ||
                x.Url.Contains(term) ||
                (x.Description != null && x.Description.Contains(term)))
            .OrderByDescending(x => x.CreatedAtUtc)
            .Take(maxPerModule)
            .Select(x => new GlobalSearchItem
            {
                Module = "Links",
                EntityId = x.Id,
                Title = x.Title,
                Summary = x.Url,
                ProjectId = x.ProjectId,
                ProjectName = x.Project.Name,
                CategoryId = x.CategoryId,
                CategoryName = x.Category != null ? x.Category.Name : null,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        results.AddRange(links);

        var categories = await _dbContext.Categories
            .Where(x =>
                x.Name.Contains(term) ||
                (x.Description != null && x.Description.Contains(term)))
            .OrderBy(x => x.Name)
            .Take(maxPerModule)
            .Select(x => new GlobalSearchItem
            {
                Module = "Categories",
                EntityId = x.Id,
                Title = x.Name,
                Summary = x.Description,
                CategoryId = x.Id,
                CategoryName = x.Name,
                CreatedAtUtc = x.CreatedAtUtc
            })
            .ToListAsync();

        results.AddRange(categories);

        return results;
    }
}
