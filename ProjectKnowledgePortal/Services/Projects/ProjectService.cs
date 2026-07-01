using Microsoft.EntityFrameworkCore;
using ProjectKnowledgePortal.Data;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly ApplicationDbContext _dbContext;

    public ProjectService(IProjectRepository projectRepository, ApplicationDbContext dbContext)
    {
        _projectRepository = projectRepository;
        _dbContext = dbContext;
    }

    public Task<List<Project>> GetAllAsync(string? searchText)
    {
        return _projectRepository.GetAllAsync(searchText);
    }

    public Task<Project?> GetByIdAsync(int id)
    {
        return _projectRepository.GetByIdAsync(id);
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _dbContext.Categories
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<(bool Success, string? Error)> CreateAsync(Project project, string currentUserId)
    {
        project.Name = project.Name.Trim();
        project.Code = project.Code.Trim();

        if (await _projectRepository.ExistsByCodeAsync(project.Code))
        {
            return (false, "Project code already exists.");
        }

        if (project.CategoryId.HasValue)
        {
            var categoryExists = await _dbContext.Categories.AnyAsync(x => x.Id == project.CategoryId.Value);
            if (!categoryExists)
            {
                return (false, "Selected category does not exist.");
            }
        }

        project.CreatedByUserId = currentUserId;
        project.CreatedAtUtc = DateTime.UtcNow;

        await _projectRepository.AddAsync(project);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Project project)
    {
        var existingProject = await _projectRepository.GetByIdAsync(project.Id);
        if (existingProject is null)
        {
            return (false, "Project was not found.");
        }

        var code = project.Code.Trim();
        if (await _projectRepository.ExistsByCodeAsync(code, project.Id))
        {
            return (false, "Project code already exists.");
        }

        if (project.CategoryId.HasValue)
        {
            var categoryExists = await _dbContext.Categories.AnyAsync(x => x.Id == project.CategoryId.Value);
            if (!categoryExists)
            {
                return (false, "Selected category does not exist.");
            }
        }

        existingProject.Name = project.Name.Trim();
        existingProject.Code = code;
        existingProject.Description = project.Description;
        existingProject.TeamMembers = project.TeamMembers;
        existingProject.CategoryId = project.CategoryId;
        existingProject.UpdatedAtUtc = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(existingProject);
        return (true, null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project is null)
        {
            return false;
        }

        await _projectRepository.DeleteAsync(project);
        return true;
    }
}
