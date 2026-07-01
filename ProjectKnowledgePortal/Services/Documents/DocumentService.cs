using Microsoft.AspNetCore.Http;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Domain;

namespace ProjectKnowledgePortal.Services;

public class DocumentService : IDocumentService
{
    private const long MaxFileSizeBytes = 25 * 1024 * 1024;

    private static readonly HashSet<string> AllowedExtensions =
    [
        ".pdf",
        ".docx",
        ".pptx",
        ".xlsx",
        ".txt"
    ];

    private static readonly Dictionary<string, string> ContentTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = "application/pdf",
        [".docx"] = "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        [".pptx"] = "application/vnd.openxmlformats-officedocument.presentationml.presentation",
        [".xlsx"] = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        [".txt"] = "text/plain"
    };

    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;

    public DocumentService(IDocumentRepository documentRepository, IFileStorageService fileStorageService)
    {
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
    }

    public Task<List<Document>> GetAllAsync(string? searchText, int? projectId)
    {
        return _documentRepository.GetAllAsync(searchText, projectId);
    }

    public Task<Document?> GetByIdAsync(int id)
    {
        return _documentRepository.GetByIdAsync(id);
    }

    public Task<List<Project>> GetProjectsAsync()
    {
        return _documentRepository.GetProjectsAsync();
    }

    public Task<List<Category>> GetCategoriesAsync()
    {
        return _documentRepository.GetCategoriesAsync();
    }

    public async Task<(bool Success, string? Error)> UploadAsync(IFormFile file, string? documentName, int projectId, int? categoryId, string uploadedByUserId)
    {
        if (file is null || file.Length == 0)
        {
            return (false, "Please select a document file.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return (false, "File size exceeds 25 MB limit.");
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            return (false, "Invalid file type. Allowed types: PDF, DOCX, PPTX, XLSX, TXT.");
        }

        if (!await _documentRepository.ProjectExistsAsync(projectId))
        {
            return (false, "Selected project does not exist.");
        }

        if (categoryId.HasValue && !await _documentRepository.CategoryExistsAsync(categoryId.Value))
        {
            return (false, "Selected category does not exist.");
        }

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        await using var input = file.OpenReadStream();
        var relativePath = await _fileStorageService.SaveAsync("documents", storedFileName, input, file.ContentType);
        var resolvedContentType = ContentTypeMap.TryGetValue(extension, out var mappedType)
            ? mappedType
            : file.ContentType;

        var document = new Document
        {
            ProjectId = projectId,
            CategoryId = categoryId,
            Name = string.IsNullOrWhiteSpace(documentName)
                ? Path.GetFileNameWithoutExtension(file.FileName)
                : documentName.Trim(),
            FilePath = relativePath,
            FileExtension = extension,
            ContentType = string.IsNullOrWhiteSpace(resolvedContentType) ? "application/octet-stream" : resolvedContentType,
            FileSizeBytes = file.Length,
            UploadedAtUtc = DateTime.UtcNow,
            UploadedByUserId = uploadedByUserId
        };

        await _documentRepository.AddAsync(document);
        return (true, null);
    }

    public async Task<(bool Success, string? Error, string? FullPath, string? ContentType, string? DownloadName, bool Inline)> GetFileForAccessAsync(int id, bool forceDownload)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        if (document is null)
        {
            return (false, "Document not found.", null, null, null, false);
        }

        var storageResult = await _fileStorageService.OpenReadAsync("documents", document.FilePath);
        if (!storageResult.Found)
        {
            return (false, "Invalid document path.", null, null, null, false);
        }

        string requestedPath;
        if (!string.IsNullOrWhiteSpace(storageResult.FullPath))
        {
            requestedPath = storageResult.FullPath;
        }
        else if (storageResult.Stream is not null)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"pkp-doc-{Guid.NewGuid():N}{document.FileExtension}");
            await using (var tempOutput = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                await storageResult.Stream.CopyToAsync(tempOutput);
            }

            await storageResult.Stream.DisposeAsync();
            requestedPath = tempPath;
        }
        else
        {
            return (false, "Document file is missing from storage.", null, null, null, false);
        }

        var isInlineType = document.FileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)
            || document.FileExtension.Equals(".txt", StringComparison.OrdinalIgnoreCase);

        return (
            true,
            null,
            requestedPath,
            string.IsNullOrWhiteSpace(document.ContentType) ? "application/octet-stream" : document.ContentType,
            $"{document.Name}{document.FileExtension}",
            !forceDownload && isInlineType
        );
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        if (document is null)
        {
            return false;
        }

        await _fileStorageService.DeleteAsync("documents", document.FilePath);

        await _documentRepository.DeleteAsync(document);
        return true;
    }
}
