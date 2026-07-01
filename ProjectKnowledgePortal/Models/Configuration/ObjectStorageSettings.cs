namespace ProjectKnowledgePortal.Models.Configuration;

public class ObjectStorageSettings
{
    public const string SectionName = "ObjectStorage";

    public bool Enabled { get; set; }
    public string ServiceUrl { get; set; } = string.Empty;
    public string Region { get; set; } = "us-east-1";
    public string BucketName { get; set; } = string.Empty;
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public bool ForcePathStyle { get; set; } = true;
    public string KeyPrefix { get; set; } = "pkp";
}
