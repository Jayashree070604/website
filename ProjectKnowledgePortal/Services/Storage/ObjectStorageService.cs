using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.Models.Configuration;

namespace ProjectKnowledgePortal.Services;

public class ObjectStorageService : IObjectStorageService
{
    private readonly ObjectStorageSettings _settings;
    private readonly IAmazonS3? _s3Client;

    public ObjectStorageService(IOptions<ObjectStorageSettings> options)
    {
        _settings = options.Value;

        if (!IsEnabled)
        {
            return;
        }

        var config = new AmazonS3Config
        {
            ServiceURL = _settings.ServiceUrl,
            ForcePathStyle = _settings.ForcePathStyle,
            AuthenticationRegion = _settings.Region
        };

        var credentials = new BasicAWSCredentials(_settings.AccessKey, _settings.SecretKey);
        _s3Client = new AmazonS3Client(credentials, config);
    }

    public bool IsEnabled => _settings.Enabled
                             && !string.IsNullOrWhiteSpace(_settings.ServiceUrl)
                             && !string.IsNullOrWhiteSpace(_settings.BucketName)
                             && !string.IsNullOrWhiteSpace(_settings.AccessKey)
                             && !string.IsNullOrWhiteSpace(_settings.SecretKey);

    public async Task<string> UploadAsync(string key, Stream content, string? contentType, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled || _s3Client is null)
        {
            throw new InvalidOperationException("Object storage is not configured.");
        }

        var putRequest = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            InputStream = content,
            AutoCloseStream = false,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
        };

        await _s3Client.PutObjectAsync(putRequest, cancellationToken);
        return key;
    }

    public async Task<(bool Found, Stream? Stream, string? ContentType)> OpenReadAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled || _s3Client is null)
        {
            return (false, null, null);
        }

        try
        {
            var response = await _s3Client.GetObjectAsync(_settings.BucketName, key, cancellationToken);
            return (true, response.ResponseStream, response.Headers.ContentType);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound || ex.ErrorCode == "NoSuchKey")
        {
            return (false, null, null);
        }
    }

    public async Task DeleteAsync(string key, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled || _s3Client is null)
        {
            return;
        }

        await _s3Client.DeleteObjectAsync(_settings.BucketName, key, cancellationToken);
    }
}
