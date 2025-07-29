using Microsoft.AspNetCore.Identity;
using SmartEnterpriseBot.Domain.Enums;
using SmartEnterpriseBot.Infrastructure.Identity;

namespace SmartEnterpriseBot.API.DateSeeding
{
    public static class UserSeeder
    {
        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            const string adminEmail = "myapp.admin@gmail.com";
            const string adminPassword = "Admin@123";
            const string adminUserName = "Admin";

            // Ensure the Admin role exists
            if (!await roleManager.RoleExistsAsync(Role.Admin.ToString()))
            {
                await roleManager.CreateAsync(new IdentityRole(Role.Admin.ToString()));
            }

            // Check if the user already exists
            var existingUser = await userManager.FindByEmailAsync(adminEmail);
            if (existingUser == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    Department = "IT",
                    FullName = "System Admin"
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, Role.Admin.ToString());
                }
                else
                {
                    throw new Exception("Failed to seed admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
