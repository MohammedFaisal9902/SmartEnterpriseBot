using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartEnterpriseBot.Application.Interfaces;
using SmartEnterpriseBot.Domain.Entities;
using SmartEnterpriseBot.Domain.Enums;
using System.Security.Claims;

namespace SmartEnterpriseBot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class KnowledgeController : ControllerBase
    {
        private readonly IKnowledgeService _knowledgeService;

        public KnowledgeController(IKnowledgeService knowledgeService)
        {
            _knowledgeService = knowledgeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;

            if (!Enum.TryParse<Role>(userRole, out var roleEnum))
                return BadRequest("Invalid role");

            var entries = await _knowledgeService.GetKnowledgeEntriesByRoleAsync(roleEnum, page, pageSize);
            return Ok(entries);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entry = await _knowledgeService.GetKnowledgeEntryByIdAsync(id);
            if (entry == null) return NotFound();
            return Ok(entry);
        }


        [HttpPost]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.HR)}")]
        public async Task<IActionResult> Add([FromBody] KnowledgeEntry entry, [FromQuery] List<Role> roles)
        {
            if (entry == null)
                return BadRequest("Entry cannot be null");

            entry.CreatedBy = User.Identity?.Name;
            var id = await _knowledgeService.AddKnowledgeEntryAsync(entry, roles);
            return Ok(new { Message = "Knowledge entry added", Id = id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = $"{nameof(Role.Admin)},{nameof(Role.HR)}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] KnowledgeEntry update, [FromQuery] List<Role> roles)
        {
            update.RecordId = id;
            var result = await _knowledgeService.UpdateKnowledgeEntryAsync(update, roles);
            if (!result) return NotFound();
            return Ok("Knowledge entry updated.");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = $"{nameof(Role.Admin)}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _knowledgeService.DeleteKnowledgeEntryAsync(id);
            if (!deleted) return NotFound();
            return Ok("Knowledge entry deleted.");
        }
    }
}
