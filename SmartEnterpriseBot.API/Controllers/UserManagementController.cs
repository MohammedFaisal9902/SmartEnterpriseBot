using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartEnterpriseBot.Application.Interfaces;
using SmartEnterpriseBot.Application.Models.User;
using SmartEnterpriseBot.Domain.Enums;

namespace SmartEnterpriseBot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.HR)}")]
    public class UserManagementController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserManagementController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return user == null ? NotFound() : Ok(user);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, UpdateUserRequest request)
        {
            var success = await _userService.UpdateUserAsync(id, request);
            return success ? Ok("User updated.") : NotFound("User not found.");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = nameof(Role.Admin))]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var success = await _userService.DeleteUserAsync(id);
            return success ? Ok("User deleted.") : NotFound("User not found.");
        }

        [HttpPost("change-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            var success = await _userService.ChangePasswordAsync(request);
            return success ? Ok("Password changed.") : BadRequest("Failed to change password.");
        }
    }
}
