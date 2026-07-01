namespace ProjectKnowledgePortal.Interfaces;

public interface IStoragePathService
{
    string GetDirectory(string area);
    string BuildRelativePath(string area, string fileName);
    bool TryResolvePath(string area, string? relativePath, out string fullPath);
}
