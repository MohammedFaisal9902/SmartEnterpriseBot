using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartEnterpriseBot.Application.Interfaces;
using SmartEnterpriseBot.Application.Models;
using SmartEnterpriseBot.Domain.Enums;

namespace SmartEnterpriseBot.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.HR)}")]
    public class RoleManagementController : ControllerBase
    {
        private readonly IRoleManagementService _roleService;

        public RoleManagementController(IRoleManagementService roleService)
        {
            _roleService = roleService;
        }
        [HttpPost("manage")]
        public async Task<IActionResult> ManageRoles([FromBody] ManageUserRolesRequest request)
        {
            var success = await _roleService.ManageUserRolesAsync(request);
            return success ? Ok("Roles updated successfully.") :  BadRequest("Failed to update user roles.");
        }

    }
}
