using Microsoft.AspNetCore.Http;
using SmartEnterpriseBot.Domain.Entities;
using SmartEnterpriseBot.Domain.Enums;

namespace SmartEnterpriseBot.Application.Interfaces
{
    public interface IDocumentService
    {
        Task<string> UploadDocumentAsync(IFormFile file, List<Role> allowedRoles, string uploadedBy, string type, string description);
        Task<List<DocumentMetadata>> GetDocumentsByRoleAsync(string userRole);
    }
}
