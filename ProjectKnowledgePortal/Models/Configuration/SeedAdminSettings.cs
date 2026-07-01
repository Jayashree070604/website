namespace ProjectKnowledgePortal.Models.Configuration;

public class SeedAdminSettings
{
    public const string SectionName = "SeedSuperAdmin";

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = "Super";
    public string LastName { get; set; } = "Admin";
}
