using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ProjectKnowledgePortal.Helpers;
using ProjectKnowledgePortal.Models.Configuration;

namespace ProjectKnowledgePortal.Data;

public class IdentityDataSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SeedAdminSettings _seedAdminSettings;
    private readonly ILogger<IdentityDataSeeder> _logger;

    public IdentityDataSeeder(
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager,
        IOptions<SeedAdminSettings> seedAdminOptions,
        ILogger<IdentityDataSeeder> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _seedAdminSettings = seedAdminOptions.Value;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        await SeedRolesAsync();
        await SeedSuperAdminAsync();
    }

    private async Task SeedRolesAsync()
    {
        foreach (var role in RoleConstants.AllRoles)
        {
            if (await _roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var createRoleResult = await _roleManager.CreateAsync(new IdentityRole(role));
            if (!createRoleResult.Succeeded)
            {
                var errors = string.Join(", ", createRoleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Unable to create role '{role}'. Errors: {errors}");
            }
        }
    }

    private async Task SeedSuperAdminAsync()
    {
        if (string.IsNullOrWhiteSpace(_seedAdminSettings.Email) || string.IsNullOrWhiteSpace(_seedAdminSettings.Password))
        {
            _logger.LogWarning("Super Admin seeding skipped because email or password is missing in configuration section '{SectionName}'.", SeedAdminSettings.SectionName);
            return;
        }

        var existingUser = await _userManager.FindByEmailAsync(_seedAdminSettings.Email);
        if (existingUser is null)
        {
            existingUser = new IdentityUser
            {
                UserName = _seedAdminSettings.Email,
                Email = _seedAdminSettings.Email,
                EmailConfirmed = true
            };

            var createUserResult = await _userManager.CreateAsync(existingUser, _seedAdminSettings.Password);
            if (!createUserResult.Succeeded)
            {
                var errors = string.Join(", ", createUserResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Unable to create Super Admin user '{_seedAdminSettings.Email}'. Errors: {errors}");
            }
        }
        else
        {
            var passwordToken = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
            var resetResult = await _userManager.ResetPasswordAsync(existingUser, passwordToken, _seedAdminSettings.Password);
            if (!resetResult.Succeeded)
            {
                var errors = string.Join(", ", resetResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Unable to reset Super Admin password for '{_seedAdminSettings.Email}'. Errors: {errors}");
            }

            existingUser.EmailConfirmed = true;
            existingUser.UserName = _seedAdminSettings.Email;
            existingUser.Email = _seedAdminSettings.Email;

            var updateUserResult = await _userManager.UpdateAsync(existingUser);
            if (!updateUserResult.Succeeded)
            {
                var errors = string.Join(", ", updateUserResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Unable to update Super Admin account '{_seedAdminSettings.Email}'. Errors: {errors}");
            }
        }

        if (!await _userManager.IsInRoleAsync(existingUser, RoleConstants.SuperAdmin))
        {
            var addRoleResult = await _userManager.AddToRoleAsync(existingUser, RoleConstants.SuperAdmin);
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Unable to add Super Admin role to '{_seedAdminSettings.Email}'. Errors: {errors}");
            }
        }

        await EnsureClaimAsync(existingUser, ClaimTypes.GivenName, _seedAdminSettings.FirstName);
        await EnsureClaimAsync(existingUser, ClaimTypes.Surname, _seedAdminSettings.LastName);
    }

    private async Task EnsureClaimAsync(IdentityUser user, string claimType, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        var claims = await _userManager.GetClaimsAsync(user);
        var existingClaim = claims.FirstOrDefault(c => c.Type == claimType);

        if (existingClaim is null)
        {
            var addClaimResult = await _userManager.AddClaimAsync(user, new Claim(claimType, value));
            if (!addClaimResult.Succeeded)
            {
                var errors = string.Join(", ", addClaimResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Unable to add claim '{claimType}' to '{user.Email}'. Errors: {errors}");
            }

            return;
        }

        if (existingClaim.Value == value)
        {
            return;
        }

        var replaceClaimResult = await _userManager.ReplaceClaimAsync(user, existingClaim, new Claim(claimType, value));
        if (!replaceClaimResult.Succeeded)
        {
            var errors = string.Join(", ", replaceClaimResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Unable to replace claim '{claimType}' for '{user.Email}'. Errors: {errors}");
        }
    }
}
