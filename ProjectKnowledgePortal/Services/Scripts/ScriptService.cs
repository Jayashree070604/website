using Microsoft.AspNetCore.Http;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Services;

public class ScriptService : IScriptService
{
    private const long MaxFileSizeBytes = 25 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions =
    [
        ".txt",
        ".pdf",
        ".docx"
    ];

    private static readonly Dictionary<string, string> ContentTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        [".txt"] = "text/plain",
        [".pdf"] = "application/pdf",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    private readonly IScriptRepository _scriptRepository;
    private readonly IFileStorageService _fileStorageService;

    public ScriptService(IScriptRepository scriptRepository, IFileStorageService fileStorageService)
    {
        _scriptRepository = scriptRepository;
        _fileStorageService = fileStorageService;
    }

    public Task<List<Script>> GetAllAsync(string? searchText, int? projectId)
    {
        return _scriptRepository.GetAllAsync(searchText, projectId);
    }

    public Task<Script?> GetByIdAsync(int id)
    {
        return _scriptRepository.GetByIdAsync(id);
    }

    public Task<List<Project>> GetProjectsAsync()
    {
        return _scriptRepository.GetProjectsAsync();
    }

    public Task<List<Category>> GetCategoriesAsync()
    {
        return _scriptRepository.GetCategoriesAsync();
    }

    public async Task<(bool Success, string? Error)> UploadAsync(IFormFile file, string? scriptName, int projectId, int? categoryId, string uploadedByUserId)
    {
        if (file is null || file.Length == 0)
        {
            return (false, "Please select a script file.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return (false, "File size exceeds 25 MB limit.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return (false, "Invalid file type. Allowed types: TXT, PDF, DOCX.");
        }

        if (!await _scriptRepository.ProjectExistsAsync(projectId))
        {
            return (false, "Selected project does not exist.");
        }

        if (categoryId.HasValue && !await _scriptRepository.CategoryExistsAsync(categoryId.Value))
        {
            return (false, "Selected category does not exist.");
        }

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        await using var input = file.OpenReadStream();
        var relativePath = await _fileStorageService.SaveAsync("scripts", storedFileName, input, file.ContentType);
        var resolvedContentType = ContentTypeMap.TryGetValue(extension, out var mappedType)
            ? mappedType
            : file.ContentType;

        var script = new Script
        {
            ProjectId = projectId,
            CategoryId = categoryId,
            Name = string.IsNullOrWhiteSpace(scriptName)
                ? Path.GetFileNameWithoutExtension(file.FileName)
                : scriptName.Trim(),
            FilePath = relativePath,
            FileExtension = extension,
            ContentType = string.IsNullOrWhiteSpace(resolvedContentType) ? "application/octet-stream" : resolvedContentType,
            FileSizeBytes = file.Length,
            UploadedAtUtc = DateTime.UtcNow,
            UploadedByUserId = uploadedByUserId
        };

        await _scriptRepository.AddAsync(script);
        return (true, null);
    }

    public async Task<(bool Success, string? Error, string? FullPath, string? ContentType, string? DownloadName, bool Inline)> GetFileForAccessAsync(int id, bool forceDownload)
    {
        var script = await _scriptRepository.GetByIdAsync(id);
        if (script is null)
        {
            return (false, "Script not found.", null, null, null, false);
        }

        var storageResult = await _fileStorageService.OpenReadAsync("scripts", script.FilePath);
        if (!storageResult.Found)
        {
            return (false, "Invalid script path.", null, null, null, false);
        }

        string requestedPath;
        if (!string.IsNullOrWhiteSpace(storageResult.FullPath))
        {
            requestedPath = storageResult.FullPath;
        }
        else if (storageResult.Stream is not null)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"pkp-script-{Guid.NewGuid():N}{script.FileExtension}");
            await using (var tempOutput = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                await storageResult.Stream.CopyToAsync(tempOutput);
            }

            await storageResult.Stream.DisposeAsync();
            requestedPath = tempPath;
        }
        else
        {
            return (false, "Script file is missing from storage.", null, null, null, false);
        }

        var isInlineType = script.FileExtension.Equals(".txt", StringComparison.OrdinalIgnoreCase)
            || script.FileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase);

        return (
            true,
            null,
            requestedPath,
            string.IsNullOrWhiteSpace(script.ContentType) ? "application/octet-stream" : script.ContentType,
            $"{script.Name}{script.FileExtension}",
            !forceDownload && isInlineType
        );
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var script = await _scriptRepository.GetByIdAsync(id);
        if (script is null)
        {
            return false;
        }

        await _fileStorageService.DeleteAsync("scripts", script.FilePath);

        await _scriptRepository.DeleteAsync(script);
        return true;
    }
}
