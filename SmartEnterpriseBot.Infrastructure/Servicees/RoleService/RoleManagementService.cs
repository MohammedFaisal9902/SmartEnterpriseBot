using Microsoft.AspNetCore.Identity;
using SmartEnterpriseBot.Application.Interfaces;
using SmartEnterpriseBot.Application.Models;
using SmartEnterpriseBot.Infrastructure.Identity;

namespace SmartEnterpriseBot.Infrastructure.Servicees.RoleService
{
    public class RoleManagementService : IRoleManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleManagementService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> ManageUserRolesAsync(ManageUserRolesRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null) return false;

            var currentRoles = await _userManager.GetRolesAsync(user);

            var rolesToAdd = request.RolesToAdd.Select(c => c.ToString()).Except(currentRoles).ToList();
            var rolesToRemove = request.RolesToRemove.Select(c => c.ToString()).Intersect(currentRoles).ToList();

            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
            if (!addResult.Succeeded) return false;

            var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
            return removeResult.Succeeded;
        }
    }
}
