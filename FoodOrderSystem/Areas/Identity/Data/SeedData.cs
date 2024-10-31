using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using FoodOrderSystem.Areas.Identity.Data; // For ApplicationUser

public class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = { "Admin", "Customer" };

        // Create roles if they don't exist
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // Create an admin user
        var adminUser = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@gmail.com",
            FullName = "Admin User"
        };

        var user = await userManager.FindByEmailAsync(adminUser.Email);
        if (user == null)
        {
            var createAdmin = await userManager.CreateAsync(adminUser, "Admin@123");
            if (createAdmin.Succeeded)
            {
                // Assign the admin user to Admin role
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
