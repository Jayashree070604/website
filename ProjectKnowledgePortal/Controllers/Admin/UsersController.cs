using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectKnowledgePortal.Helpers;
using ProjectKnowledgePortal.Interfaces;
using ProjectKnowledgePortal.ViewModels.Users;

namespace ProjectKnowledgePortal.Controllers;

[Authorize(Roles = RoleConstants.SuperAdmin)]
public class UsersController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IActivityLogService _activityLogService;

    public UsersController(
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IActivityLogService activityLogService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _activityLogService = activityLogService;
    }

    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.OrderBy(x => x.Email).ToList();
        var userItems = new List<UserListItemViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            userItems.Add(new UserListItemViewModel
            {
                Id = user.Id,
                Email = user.Email ?? user.UserName ?? string.Empty,
                Roles = roles.ToList()
            });
        }

        var viewModel = new UserManagementIndexViewModel
        {
            Users = userItems,
            AvailableRoles = RoleConstants.AllRoles.ToList()
        };

        return View(viewModel);
    }

    public IActionResult Create()
    {
        var viewModel = new CreateUserViewModel
        {
            AvailableRoles = RoleConstants.AllRoles.ToList(),
            Role = RoleConstants.User
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel viewModel)
    {
        viewModel.AvailableRoles = RoleConstants.AllRoles.ToList();

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        if (!RoleConstants.AllRoles.Contains(viewModel.Role))
        {
            ModelState.AddModelError(nameof(viewModel.Role), "Invalid role selected.");
            return View(viewModel);
        }

        var existing = await _userManager.FindByEmailAsync(viewModel.Email);
        if (existing is not null)
        {
            ModelState.AddModelError(nameof(viewModel.Email), "Email already exists.");
            return View(viewModel);
        }

        var user = new IdentityUser
        {
            UserName = viewModel.Email,
            Email = viewModel.Email,
            EmailConfirmed = true
        };

        var createResult = await _userManager.CreateAsync(user, viewModel.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(viewModel);
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, viewModel.Role);
        if (!addRoleResult.Succeeded)
        {
            foreach (var error in addRoleResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(viewModel);
        }

        await LogActivityAsync("User Created", $"User '{viewModel.Email}' created with role '{viewModel.Role}'.", entityType: "User", entityId: user.Id);

        TempData["SuccessMessage"] = "User created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRole(string userId, string role)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(role) || !RoleConstants.AllRoles.Contains(role))
        {
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Unable to update user role.";
                return RedirectToAction(nameof(Index));
            }
        }

        var addRoleResult = await _userManager.AddToRoleAsync(user, role);
        if (!addRoleResult.Succeeded)
        {
            TempData["ErrorMessage"] = "Unable to assign selected role.";
            return RedirectToAction(nameof(Index));
        }

        await LogActivityAsync("Role Changed", $"Role for user '{user.Email}' set to '{role}'.", entityType: "User", entityId: user.Id);

        TempData["SuccessMessage"] = "Role updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return RedirectToAction(nameof(Index));
        }

        var currentUserId = _userManager.GetUserId(User);
        if (string.Equals(currentUserId, userId, StringComparison.Ordinal))
        {
            TempData["ErrorMessage"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            TempData["ErrorMessage"] = "Unable to delete user.";
            return RedirectToAction(nameof(Index));
        }

        await LogActivityAsync("User Deleted", $"User '{user.Email}' deleted.", entityType: "User", entityId: user.Id);

        TempData["SuccessMessage"] = "User deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> ResetPassword(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new ResetPasswordViewModel
        {
            UserId = user.Id,
            Email = user.Email ?? user.UserName ?? string.Empty
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var user = await _userManager.FindByIdAsync(viewModel.UserId);
        if (user is null)
        {
            return RedirectToAction(nameof(Index));
        }

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, resetToken, viewModel.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            viewModel.Email = user.Email ?? user.UserName ?? string.Empty;
            return View(viewModel);
        }

        await LogActivityAsync("Password Reset", $"Password reset for user '{user.Email}'.", entityType: "User", entityId: user.Id);

        TempData["SuccessMessage"] = "Password reset successfully.";
        return RedirectToAction(nameof(Index));
    }

    private Task LogActivityAsync(string activityType, string description, int? projectId = null, string? entityType = null, string? entityId = null)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        return _activityLogService.LogAsync(activityType, description, userId, projectId, entityType, entityId, ipAddress);
    }
}
