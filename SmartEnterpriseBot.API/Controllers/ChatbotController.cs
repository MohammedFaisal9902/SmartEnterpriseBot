using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartEnterpriseBot.Application.Interfaces;
using SmartEnterpriseBot.Domain.Enums;
using System.Security.Claims;

namespace SmartEnterpriseBot.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatbotController : ControllerBase
    {
        private readonly IAiAnswerService _aiAnswerService;

        public ChatbotController(IAiAnswerService aiAnswerService)
        {
            _aiAnswerService = aiAnswerService;
        }

        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] string question)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Enum.TryParse<Role>(userRole, out var roleEnum))
                return BadRequest("Invalid role");

            var aiAnswer = await _aiAnswerService.GetAnswerAsync(question, roleEnum, userId);
            return Ok(aiAnswer);
        }
    }
}
