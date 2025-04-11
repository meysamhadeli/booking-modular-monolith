using BuildingBlocks.EFCore;
using Identity.Identity.Constants;
using Identity.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Data;

public class IdentityDataSeeder : IDataSeeder
{
    private readonly RoleManager<IdentityRole<long>> _roleManager;
    private readonly IdentityContext _identityContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityDataSeeder(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<long>> roleManager,
        IdentityContext identityContext
    )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _identityContext = identityContext;
    }

    public async Task SeedAllAsync()
    {
        if (await _identityContext.Database.CanConnectAsync())
        {
            await SeedRoles();
            await SeedUsers();
        }
    }

    private async Task SeedRoles()
    {
        if (!await _identityContext.Roles.AnyAsync())
        {
            if (await _roleManager.RoleExistsAsync(Constants.Role.Admin) == false)
            {
                await _roleManager.CreateAsync(new IdentityRole<long>(Constants.Role.Admin));
                await _identityContext.SaveChangesAsync();
            }

            if (await _roleManager.RoleExistsAsync(Constants.Role.User) == false)
            {
                await _roleManager.CreateAsync(new IdentityRole<long>(Constants.Role.User));
                await _identityContext.SaveChangesAsync();
            }
        }
    }

    private async Task SeedUsers()
    {
        if (!await _identityContext.Users.AnyAsync())
        {
            if (await _userManager.FindByNameAsync("meysamh") == null)
            {
                var user = new ApplicationUser
                           {
                               FirstName = "Meysam",
                               LastName = "Hadeli",
                               UserName = "meysamh",
                               Email = "meysam@test.com",
                               SecurityStamp = Guid.NewGuid().ToString(),
                               PassPortNumber = String.Empty
                           };

                var result = await _userManager.CreateAsync(user, "Admin@123456");

                if (result.Succeeded) await _userManager.AddToRoleAsync(user, Constants.Role.Admin);

                await _identityContext.SaveChangesAsync();
            }

            if (await _userManager.FindByNameAsync("meysamh2") == null)
            {
                var user = new ApplicationUser
                           {
                               FirstName = "Meysam",
                               LastName = "Hadeli",
                               UserName = "meysamh2",
                               Email = "meysam2@test.com",
                               SecurityStamp = Guid.NewGuid().ToString(),
                               PassPortNumber = String.Empty
                           };

                var result = await _userManager.CreateAsync(user, "User@123456");

                if (result.Succeeded) await _userManager.AddToRoleAsync(user, Constants.Role.User);

                await _identityContext.SaveChangesAsync();
            }
        }
    }
}
