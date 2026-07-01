using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Configuration;
using Microsoft.Extensions.Options;

namespace ProjectKnowledgePortal.Services;

public class FileStorageService : IFileStorageService
{
    private const string ObjectStoragePrefix = "s3:";

    private readonly IStoragePathService _storagePathService;
    private readonly IObjectStorageService _objectStorageService;
    private readonly ObjectStorageSettings _settings;

    public FileStorageService(
        IStoragePathService storagePathService,
        IObjectStorageService objectStorageService,
        IOptions<ObjectStorageSettings> objectStorageSettings)
    {
        _storagePathService = storagePathService;
        _objectStorageService = objectStorageService;
        _settings = objectStorageSettings.Value;
    }

    public async Task<string> SaveAsync(string area, string fileName, Stream content, string? contentType, CancellationToken cancellationToken = default)
    {
        if (_objectStorageService.IsEnabled)
        {
            var objectKey = BuildObjectKey(area, fileName);
            await _objectStorageService.UploadAsync(objectKey, content, contentType, cancellationToken);
            return ObjectStoragePrefix + objectKey;
        }

        var directory = _storagePathService.GetDirectory(area);
        var fullPath = Path.Combine(directory, fileName);

        await using var output = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        await content.CopyToAsync(output, cancellationToken);

        return _storagePathService.BuildRelativePath(area, fileName);
    }

    public async Task<(bool Found, string? FullPath, Stream? Stream, string? ContentType)> OpenReadAsync(string area, string? storedPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
        {
            return (false, null, null, null);
        }

        if (storedPath.StartsWith(ObjectStoragePrefix, StringComparison.OrdinalIgnoreCase))
        {
            var key = storedPath[ObjectStoragePrefix.Length..];
            var result = await _objectStorageService.OpenReadAsync(key, cancellationToken);
            return (result.Found, null, result.Stream, result.ContentType);
        }

        if (!_storagePathService.TryResolvePath(area, storedPath, out var requestedPath))
        {
            return (false, null, null, null);
        }

        if (!File.Exists(requestedPath))
        {
            return (false, null, null, null);
        }

        return (true, requestedPath, null, null);
    }

    public async Task DeleteAsync(string area, string? storedPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(storedPath))
        {
            return;
        }

        if (storedPath.StartsWith(ObjectStoragePrefix, StringComparison.OrdinalIgnoreCase))
        {
            var key = storedPath[ObjectStoragePrefix.Length..];
            await _objectStorageService.DeleteAsync(key, cancellationToken);
            return;
        }

        if (_storagePathService.TryResolvePath(area, storedPath, out var requestedPath) && File.Exists(requestedPath))
        {
            File.Delete(requestedPath);
        }
    }

    private string BuildObjectKey(string area, string fileName)
    {
        var keyPrefix = (_settings.KeyPrefix ?? string.Empty).Trim().Trim('/');
        var segments = new List<string>();

        if (!string.IsNullOrWhiteSpace(keyPrefix))
        {
            segments.Add(keyPrefix);
        }

        segments.Add("uploads");
        segments.Add(area.Trim().ToLowerInvariant());
        segments.Add(fileName);

        return string.Join('/', segments);
    }
}
