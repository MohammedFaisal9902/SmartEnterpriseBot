using Microsoft.AspNetCore.Http;
using SmartEnterpriseBot.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace SmartEnterpriseBot.Application.Models.Documents
{
    public class UploadDocumentRequest
    {
        [Required]
        public IFormFile File { get; set; }

        [Required]
        public List<Role> AllowedRoles { get; set; }

        [Required]
        public string DocumentType { get; set; }

        public string? Description { get; set; }
    }
}
