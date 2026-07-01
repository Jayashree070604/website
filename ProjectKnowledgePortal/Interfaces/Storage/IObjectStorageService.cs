namespace ProjectKnowledgePortal.Interfaces;

public interface IObjectStorageService
{
    bool IsEnabled { get; }

    Task<string> UploadAsync(string key, Stream content, string? contentType, CancellationToken cancellationToken = default);
    Task<(bool Found, Stream? Stream, string? ContentType)> OpenReadAsync(string key, CancellationToken cancellationToken = default);
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
