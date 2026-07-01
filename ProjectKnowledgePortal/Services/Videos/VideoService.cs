using Microsoft.AspNetCore.Http;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Services;

public class VideoService : IVideoService
{
    private const long MaxFileSizeBytes = 10L * 1024 * 1024 * 1024;

    private readonly IVideoRepository _videoRepository;
    private readonly IFileStorageService _fileStorageService;

    public VideoService(IVideoRepository videoRepository, IFileStorageService fileStorageService)
    {
        _videoRepository = videoRepository;
        _fileStorageService = fileStorageService;
    }

    public Task<List<Video>> GetAllAsync(string? searchText, int? projectId)
    {
        return _videoRepository.GetAllAsync(searchText, projectId);
    }

    public Task<Video?> GetByIdAsync(int id)
    {
        return _videoRepository.GetByIdAsync(id);
    }

    public Task<List<Project>> GetProjectsAsync()
    {
        return _videoRepository.GetProjectsAsync();
    }

    public Task<List<Category>> GetCategoriesAsync()
    {
        return _videoRepository.GetCategoriesAsync();
    }

    public async Task<(bool Success, string? Error)> UploadAsync(IFormFile file, string? videoName, int projectId, int? categoryId, string uploadedByUserId)
    {
        if (file is null || file.Length == 0)
        {
            return (false, "Please select a video file.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return (false, "File size exceeds 10 GB limit.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase))
        {
            return (false, "Invalid file type. Only MP4 is allowed.");
        }

        if (!await _videoRepository.ProjectExistsAsync(projectId))
        {
            return (false, "Selected project does not exist.");
        }

        if (categoryId.HasValue && !await _videoRepository.CategoryExistsAsync(categoryId.Value))
        {
            return (false, "Selected category does not exist.");
        }

        var storedFileName = $"{Guid.NewGuid():N}.mp4";
        await using var input = file.OpenReadStream();
        var relativePath = await _fileStorageService.SaveAsync("videos", storedFileName, input, "video/mp4");

        var video = new Video
        {
            ProjectId = projectId,
            CategoryId = categoryId,
            Name = string.IsNullOrWhiteSpace(videoName)
                ? Path.GetFileNameWithoutExtension(file.FileName)
                : videoName.Trim(),
            FilePath = relativePath,
            ContentType = "video/mp4",
            FileSizeBytes = file.Length,
            UploadedAtUtc = DateTime.UtcNow,
            UploadedByUserId = uploadedByUserId
        };

        await _videoRepository.AddAsync(video);
        return (true, null);
    }

    public async Task<(bool Success, string? Error, string? FullPath, string? ContentType, string? DownloadName)> GetFileForAccessAsync(int id)
    {
        var video = await _videoRepository.GetByIdAsync(id);
        if (video is null)
        {
            return (false, "Video not found.", null, null, null);
        }

        var storageResult = await _fileStorageService.OpenReadAsync("videos", video.FilePath);
        if (!storageResult.Found)
        {
            return (false, "Invalid video path.", null, null, null);
        }

        string requestedPath;
        if (!string.IsNullOrWhiteSpace(storageResult.FullPath))
        {
            requestedPath = storageResult.FullPath;
        }
        else if (storageResult.Stream is not null)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"pkp-video-{Guid.NewGuid():N}.mp4");
            await using (var tempOutput = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                await storageResult.Stream.CopyToAsync(tempOutput);
            }

            await storageResult.Stream.DisposeAsync();
            requestedPath = tempPath;
        }
        else
        {
            return (false, "Video file is missing from storage.", null, null, null);
        }

        return (
            true,
            null,
            requestedPath,
            string.IsNullOrWhiteSpace(video.ContentType) ? "video/mp4" : video.ContentType,
            $"{video.Name}.mp4"
        );
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var video = await _videoRepository.GetByIdAsync(id);
        if (video is null)
        {
            return false;
        }

        await _fileStorageService.DeleteAsync("videos", video.FilePath);

        await _videoRepository.DeleteAsync(video);
        return true;
    }
}
