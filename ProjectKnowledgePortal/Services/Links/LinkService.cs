using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Services;

public class LinkService : ILinkService
{
    private readonly ILinkRepository _linkRepository;

    public LinkService(ILinkRepository linkRepository)
    {
        _linkRepository = linkRepository;
    }

    public Task<List<Link>> GetAllAsync(string? searchText, int? projectId, int? categoryId)
    {
        return _linkRepository.GetAllAsync(searchText, projectId, categoryId);
    }

    public Task<Link?> GetByIdAsync(int id)
    {
        return _linkRepository.GetByIdAsync(id);
    }

    public Task<List<Project>> GetProjectsAsync()
    {
        return _linkRepository.GetProjectsAsync();
    }

    public Task<List<Category>> GetCategoriesAsync()
    {
        return _linkRepository.GetCategoriesAsync();
    }

    public async Task<(bool Success, string? Error)> CreateAsync(Link link, string createdByUserId)
    {
        var validation = await ValidateLinkAsync(link);
        if (!validation.Success)
        {
            return validation;
        }

        link.Title = link.Title.Trim();
        link.Url = link.Url.Trim();
        link.CreatedByUserId = createdByUserId;
        link.CreatedAtUtc = DateTime.UtcNow;

        await _linkRepository.AddAsync(link);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Link link)
    {
        var existing = await _linkRepository.GetByIdAsync(link.Id);
        if (existing is null)
        {
            return (false, "Link not found.");
        }

        var validation = await ValidateLinkAsync(link);
        if (!validation.Success)
        {
            return validation;
        }

        existing.Title = link.Title.Trim();
        existing.Url = link.Url.Trim();
        existing.Description = link.Description;
        existing.ProjectId = link.ProjectId;
        existing.CategoryId = link.CategoryId;

        await _linkRepository.UpdateAsync(existing);
        return (true, null);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existing = await _linkRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return false;
        }

        await _linkRepository.DeleteAsync(existing);
        return true;
    }

    private async Task<(bool Success, string? Error)> ValidateLinkAsync(Link link)
    {
        if (string.IsNullOrWhiteSpace(link.Title))
        {
            return (false, "Link title is required.");
        }

        if (string.IsNullOrWhiteSpace(link.Url))
        {
            return (false, "URL is required.");
        }

        var urlText = link.Url.Trim();
        if (!Uri.TryCreate(urlText, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return (false, "Please provide a valid absolute HTTP/HTTPS URL.");
        }

        if (!await _linkRepository.ProjectExistsAsync(link.ProjectId))
        {
            return (false, "Selected project does not exist.");
        }

        if (link.CategoryId.HasValue && !await _linkRepository.CategoryExistsAsync(link.CategoryId.Value))
        {
            return (false, "Selected category does not exist.");
        }

        return (true, null);
    }
}
