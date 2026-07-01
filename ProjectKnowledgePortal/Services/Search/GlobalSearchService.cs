using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Search;

namespace ProjectKnowledgePortal.Services;

public class GlobalSearchService : IGlobalSearchService
{
    private readonly IGlobalSearchRepository _globalSearchRepository;

    public GlobalSearchService(IGlobalSearchRepository globalSearchRepository)
    {
        _globalSearchRepository = globalSearchRepository;
    }

    public async Task<List<GlobalSearchItem>> SearchAsync(string? searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            return new List<GlobalSearchItem>();
        }

        return await _globalSearchRepository.SearchAsync(searchText.Trim());
    }
}
