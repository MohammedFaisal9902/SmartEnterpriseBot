using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SmartEnterpriseBot.Application.Interfaces;
using SmartEnterpriseBot.Domain.Entities;
using SmartEnterpriseBot.Domain.Enums;
using SmartEnterpriseBot.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;

namespace SmartEnterpriseBot.Infrastructure.Services.Storage
{
    public class DocumentService : IDocumentService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ApplicationDbContext _context;

        public DocumentService(BlobContainerClient containerClient, ApplicationDbContext context)
        {
            _containerClient = containerClient;
            _context = context;
        }

        public async Task<string> UploadDocumentAsync(IFormFile file, List<Role> allowedRoles, string uploadedBy, string type, string description)
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var blobClient = _containerClient.GetBlobClient(fileName);

            var metadata = new Dictionary<string, string>
            {
                { "uploadedBy", uploadedBy },
                { "type", type },
                { "description", description },
                { "roles", string.Join(",", allowedRoles.Select(r => r.ToString())) }
            };

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobUploadOptions
            {
                Metadata = metadata
            });

            var doc = new DocumentMetadata
            {
                FileName = file.FileName,
                BlobUrl = blobClient.Uri.ToString(),
                UploadedBy = uploadedBy,
                DocumentType = type,
                Description = description,
                AllowedRoles = allowedRoles.Select(roleStr => new DocumentAllowedRole
                {
                    Role = roleStr
                }).ToList()
            };

            _context.DocumentMetadata.Add(doc);
            await _context.SaveChangesAsync();

            return doc.BlobUrl;
        }


        public async Task<List<DocumentMetadata>> GetDocumentsByRoleAsync(string userRole)
        {
            var roleEnum = Enum.Parse<Role>(userRole);

            return await _context.DocumentMetadata
                .Include(d => d.AllowedRoles)
                .Where(d => d.AllowedRoles.Any(ar => ar.Role == roleEnum))
                .ToListAsync();
        }
    }
}
