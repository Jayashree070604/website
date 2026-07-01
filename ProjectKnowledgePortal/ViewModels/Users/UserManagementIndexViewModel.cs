namespace ProjectKnowledgePortal.ViewModels.Users;

public class UserManagementIndexViewModel
{
    public List<UserListItemViewModel> Users { get; set; } = new();
    public List<string> AvailableRoles { get; set; } = new();
}
