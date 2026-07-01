using ProjectKnowledgePortal.Models.Search;

namespace ProjectKnowledgePortal.Interfaces;

public interface IGlobalSearchRepository
{
    Task<List<GlobalSearchItem>> SearchAsync(string searchText, int maxPerModule = 30);
}
