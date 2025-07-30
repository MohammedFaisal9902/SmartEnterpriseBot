using Microsoft.AspNetCore.Identity;
using SmartEnterpriseBot.Application.Interfaces;
using SmartEnterpriseBot.Application.Models.User;
using SmartEnterpriseBot.Infrastructure.Identity;

namespace SmartEnterpriseBot.Infrastructure.Servicees
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<UserResponse>> GetAllUsersAsync()
        {
            var users = _userManager.Users.ToList();
            var userResponses = new List<UserResponse>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userResponses.Add(new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email!,
                    FullName = user.FullName,
                    Department = user.Department,
                    PhoneNumber = user.PhoneNumber!,
                    Roles = roles.ToList()
                });
            }

            return userResponses;
        }

        public async Task<UserResponse?> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            return new UserResponse
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName,
                Department = user.Department,
                PhoneNumber = user.PhoneNumber!,
                Roles = roles.ToList()
            };
        }

        public async Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            user.Department = request.Department;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<bool> ChangePasswordAsync(ChangePasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null) return false;

            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            return result.Succeeded;
        }
    }
}
