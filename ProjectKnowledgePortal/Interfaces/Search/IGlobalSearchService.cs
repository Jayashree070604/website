using ProjectKnowledgePortal.Models.Search;

namespace ProjectKnowledgePortal.Interfaces;

public interface IGlobalSearchService
{
    Task<List<GlobalSearchItem>> SearchAsync(string? searchText);
}
