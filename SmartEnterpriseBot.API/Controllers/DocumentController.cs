using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartEnterpriseBot.Application.Interfaces;
using SmartEnterpriseBot.Application.Models.Documents;
using SmartEnterpriseBot.Domain.Enums;
using System.Security.Claims;

namespace SmartEnterpriseBot.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : ControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        [HttpPost("upload/File")]
        [Authorize(Roles = nameof(Role.Admin))]
        public async Task<IActionResult> UploadDocument([FromForm] UploadDocumentRequest request)
        {
            var uploadedBy = User?.Identity?.Name ?? "Unknown";
            var blobUrl = await _documentService.UploadDocumentAsync(request.File, request.AllowedRoles, uploadedBy, request.DocumentType, request.Description);

            return Ok(new { Message = "Upload successful", Url = blobUrl });
        }

        [HttpGet("list")]
        [Authorize]
        public async Task<IActionResult> List()
        {
            var role = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? "General";
            var docs = await _documentService.GetDocumentsByRoleAsync(role);
            return Ok(docs);
        }
    }
}
