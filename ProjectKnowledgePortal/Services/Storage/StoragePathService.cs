using Microsoft.Extensions.Options;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Configuration;

namespace ProjectKnowledgePortal.Services;

public class StoragePathService : IStoragePathService
{
    private readonly string _uploadsRoot;

    public StoragePathService(IWebHostEnvironment webHostEnvironment, IOptions<StorageSettings> storageSettings)
    {
        var configuredRoot = storageSettings.Value.UploadsRootPath?.Trim();
        _uploadsRoot = string.IsNullOrWhiteSpace(configuredRoot)
            ? Path.Combine(webHostEnvironment.WebRootPath, "uploads")
            : Path.GetFullPath(configuredRoot);
    }

    public string GetDirectory(string area)
    {
        var safeArea = NormalizeArea(area);
        var directory = Path.Combine(_uploadsRoot, safeArea);
        Directory.CreateDirectory(directory);
        return directory;
    }

    public string BuildRelativePath(string area, string fileName)
    {
        var safeArea = NormalizeArea(area);
        return Path.Combine("uploads", safeArea, fileName).Replace('\\', '/');
    }

    public bool TryResolvePath(string area, string? relativePath, out string fullPath)
    {
        fullPath = string.Empty;

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            return false;
        }

        var safeArea = NormalizeArea(area);
        var areaDirectory = Path.GetFullPath(Path.Combine(_uploadsRoot, safeArea));
        var fileName = Path.GetFileName(relativePath);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return false;
        }

        var candidatePath = Path.GetFullPath(Path.Combine(areaDirectory, fileName));
        if (!candidatePath.StartsWith(areaDirectory, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        fullPath = candidatePath;
        return true;
    }

    private static string NormalizeArea(string area)
    {
        if (string.IsNullOrWhiteSpace(area))
        {
            throw new ArgumentException("Storage area is required.", nameof(area));
        }

        return area.Trim().ToLowerInvariant();
    }
}
