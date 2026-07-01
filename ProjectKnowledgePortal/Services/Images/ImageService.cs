using Microsoft.AspNetCore.Http;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Services;

public class ImageService : IImageService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions =
    [
        ".jpg",
        ".jpeg",
        ".png",
        ".gif",
        ".webp"
    ];

    private static readonly Dictionary<string, string> ContentTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".png"] = "image/png",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp"
    };

    private readonly IImageRepository _imageRepository;
    private readonly IFileStorageService _fileStorageService;

    public ImageService(IImageRepository imageRepository, IFileStorageService fileStorageService)
    {
        _imageRepository = imageRepository;
        _fileStorageService = fileStorageService;
    }

    public Task<List<ProjectImage>> GetAllAsync(string? searchText, int? projectId)
    {
        return _imageRepository.GetAllAsync(searchText, projectId);
    }

    public Task<ProjectImage?> GetByIdAsync(int id)
    {
        return _imageRepository.GetByIdAsync(id);
    }

    public Task<List<Project>> GetProjectsAsync()
    {
        return _imageRepository.GetProjectsAsync();
    }

    public Task<List<Category>> GetCategoriesAsync()
    {
        return _imageRepository.GetCategoriesAsync();
    }

    public async Task<(bool Success, string? Error)> UploadAsync(IFormFile file, string? imageName, int projectId, int? categoryId, string uploadedByUserId)
    {
        if (file is null || file.Length == 0)
        {
            return (false, "Please select an image file.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return (false, "File size exceeds 10 MB limit.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return (false, "Invalid file type. Allowed types: JPG, JPEG, PNG, GIF, WEBP.");
        }

        if (!await _imageRepository.ProjectExistsAsync(projectId))
        {
            return (false, "Selected project does not exist.");
        }

        if (categoryId.HasValue && !await _imageRepository.CategoryExistsAsync(categoryId.Value))
        {
            return (false, "Selected category does not exist.");
        }

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        await using var input = file.OpenReadStream();
        var relativePath = await _fileStorageService.SaveAsync("images", storedFileName, input, file.ContentType);
        var resolvedContentType = ContentTypeMap.TryGetValue(extension, out var mappedType)
            ? mappedType
            : file.ContentType;

        var image = new ProjectImage
        {
            ProjectId = projectId,
            CategoryId = categoryId,
            Name = string.IsNullOrWhiteSpace(imageName)
                ? Path.GetFileNameWithoutExtension(file.FileName)
                : imageName.Trim(),
            FilePath = relativePath,
            ContentType = string.IsNullOrWhiteSpace(resolvedContentType) ? "application/octet-stream" : resolvedContentType,
            FileSizeBytes = file.Length,
            UploadedAtUtc = DateTime.UtcNow,
            UploadedByUserId = uploadedByUserId
        };

        await _imageRepository.AddAsync(image);
        return (true, null);
    }

    public async Task<(bool Success, string? Error, string? FullPath, string? ContentType, string? DownloadName)> GetFileForAccessAsync(int id)
    {
        var image = await _imageRepository.GetByIdAsync(id);
        if (image is null)
        {
            return (false, "Image not found.", null, null, null);
        }

        var storageResult = await _fileStorageService.OpenReadAsync("images", image.FilePath);
        if (!storageResult.Found)
        {
            return (false, "Invalid image path.", null, null, null);
        }

        string requestedPath;
        if (!string.IsNullOrWhiteSpace(storageResult.FullPath))
        {
            requestedPath = storageResult.FullPath;
        }
        else if (storageResult.Stream is not null)
        {
            var fileExtension = Path.GetExtension(image.FilePath);
            var tempPath = Path.Combine(Path.GetTempPath(), $"pkp-image-{Guid.NewGuid():N}{fileExtension}");
            await using (var tempOutput = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                await storageResult.Stream.CopyToAsync(tempOutput);
            }

            await storageResult.Stream.DisposeAsync();
            requestedPath = tempPath;
        }
        else
        {
            return (false, "Image file is missing from storage.", null, null, null);
        }

        var extension = Path.GetExtension(requestedPath);

        return (
            true,
            null,
            requestedPath,
            string.IsNullOrWhiteSpace(image.ContentType) ? "application/octet-stream" : image.ContentType,
            $"{image.Name}{extension}"
        );
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var image = await _imageRepository.GetByIdAsync(id);
        if (image is null)
        {
            return false;
        }

        await _fileStorageService.DeleteAsync("images", image.FilePath);

        await _imageRepository.DeleteAsync(image);
        return true;
    }
}
