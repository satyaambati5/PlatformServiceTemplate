using Microsoft.AspNetCore.Identity;
using PlatformServiceTemplate.Application.Common.Interfaces;
using PlatformServiceTemplate.Application.Common.Models;
using PlatformServiceTemplate.Infrastructure.Identity;

namespace PlatformServiceTemplate.Infrastructure.Security;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    RoleManager<IdentityRole<Guid>> roleManager) : IIdentityService
{
    public async Task<(bool Succeeded, string[] Errors)> RegisterAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return (false, result.Errors.Select(x => x.Description).ToArray());
        }

        if (!await roleManager.RoleExistsAsync("User"))
        {
            var createRoleResult = await roleManager.CreateAsync(new IdentityRole<Guid>("User"));
            if (!createRoleResult.Succeeded)
            {
                return (false, createRoleResult.Errors.Select(x => x.Description).ToArray());
            }
        }

        var addRoleResult = await userManager.AddToRoleAsync(user, "User");
        if (!addRoleResult.Succeeded)
        {
            return (false, addRoleResult.Errors.Select(x => x.Description).ToArray());
        }

        return (true, []);
    }

    public async Task<AuthenticatedUser?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return new AuthenticatedUser(user.Id, user.Email!, user.UserName!, roles.ToArray());
    }

    public async Task<AuthenticatedUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return new AuthenticatedUser(user.Id, user.Email ?? string.Empty, user.UserName ?? string.Empty, roles.ToArray());
    }
}
