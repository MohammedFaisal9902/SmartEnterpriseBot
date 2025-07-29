using Azure;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartEnterpriseBot.Application.Interfaces;
using SmartEnterpriseBot.Domain.Entities;
using SmartEnterpriseBot.Domain.Enums;
using SmartEnterpriseBot.Infrastructure.Identity;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;

namespace SmartEnterpriseBot.Infrastructure.Services.Storage
{
    public class DocumentService : IDocumentService
    {
        private readonly BlobContainerClient _containerClient;
        private readonly ApplicationDbContext _context;
        private readonly ISearchIndexerService _searchIndexerService;
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(
            BlobContainerClient containerClient,
            ApplicationDbContext context,
            ISearchIndexerService searchIndexerService,
            ILogger<DocumentService> logger)
        {
            _containerClient = containerClient;
            _context = context;
            _searchIndexerService = searchIndexerService;
            _logger = logger;
        }

        public async Task<string> UploadDocumentAsync(IFormFile file, List<Role> allowedRoles, string uploadedBy, string type, string description)
        {
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var blobClient = _containerClient.GetBlobClient(fileName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new Azure.Storage.Blobs.Models.BlobUploadOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "uploadedBy", uploadedBy },
                    { "type", type },
                    { "description", description },
                    { "roles", string.Join(",", allowedRoles.Select(r => r.ToString())) }
                }
            });

            var doc = new DocumentMetadata
            {
                FileName = file.FileName,
                BlobUrl = blobClient.Uri.ToString(),
                UploadedBy = uploadedBy,
                DocumentType = type,
                Description = description,
                AllowedRoles = allowedRoles.Select(role => new DocumentAllowedRole
                {
                    Role = role
                }).ToList()
            };

            _context.DocumentMetadata.Add(doc);
            await _context.SaveChangesAsync();

            var textContent = ExtractTextFromPdf(file);
            if (!string.IsNullOrWhiteSpace(textContent))
            {
                await _searchIndexerService.UploadDocumentAsync(
                    textContent,
                    allowedRoles.Select(r => r.ToString()).ToList()
                );
            }

            return doc.BlobUrl;
        }

        private string ExtractTextFromPdf(IFormFile file)
        {
            try
            {
                using var reader = PdfDocument.Open(file.OpenReadStream());
                var sb = new System.Text.StringBuilder();

                foreach (Page page in reader.GetPages())
                {
                    sb.AppendLine(page.Text);
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "PDF extraction failed.");
                return string.Empty;
            }
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
