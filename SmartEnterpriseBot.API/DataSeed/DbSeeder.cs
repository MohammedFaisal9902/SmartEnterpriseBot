using Microsoft.AspNetCore.Identity;
using SmartEnterpriseBot.API.DateSeeding;
using SmartEnterpriseBot.Infrastructure.Identity;

namespace SmartEnterpriseBot.API.DataSeed
{
    public static class DbSeeder
    {
        public static async Task SeedRolesAndAdminUserAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await RoleSeeder.SeedRolesAsync(roleManager);
            await UserSeeder.SeedAdminUserAsync(userManager, roleManager);
        }
    }
}
