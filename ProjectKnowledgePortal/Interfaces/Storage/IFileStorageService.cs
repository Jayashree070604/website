namespace ProjectKnowledgePortal.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveAsync(string area, string fileName, Stream content, string? contentType, CancellationToken cancellationToken = default);
    Task<(bool Found, string? FullPath, Stream? Stream, string? ContentType)> OpenReadAsync(string area, string? storedPath, CancellationToken cancellationToken = default);
    Task DeleteAsync(string area, string? storedPath, CancellationToken cancellationToken = default);
}
