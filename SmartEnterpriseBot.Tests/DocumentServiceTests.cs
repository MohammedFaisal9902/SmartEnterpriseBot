using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using SmartEnterpriseBot.Application.Interfaces;
using SmartEnterpriseBot.Domain.Entities;
using SmartEnterpriseBot.Domain.Enums;
using SmartEnterpriseBot.Infrastructure.Identity;
using SmartEnterpriseBot.Infrastructure.Services.Storage;
using System.Text;

namespace SmartEnterpriseBot.Tests
{
    public class DocumentServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<BlobContainerClient> _mockContainerClient;
        private readonly Mock<BlobClient> _mockBlobClient;
        private readonly Mock<ISearchIndexerService> _mockIndexer;
        private readonly Mock<ILogger<DocumentService>> _mockLogger;

        public DocumentServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _mockContainerClient = new Mock<BlobContainerClient>();
            _mockBlobClient = new Mock<BlobClient>();
            _mockIndexer = new Mock<ISearchIndexerService>();
            _mockLogger = new Mock<ILogger<DocumentService>>();

            _mockContainerClient
                .Setup(c => c.GetBlobClient(It.IsAny<string>()))
                .Returns(_mockBlobClient.Object);

            _mockBlobClient
                .Setup(b => b.Uri)
                .Returns(new Uri("https://test.blob.core.windows.net/container/test-file.pdf"));

            _mockBlobClient
                .Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<BlobUploadOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());

            _mockIndexer
                .Setup(x => x.UploadDocumentAsync(It.IsAny<string>(), It.IsAny<List<string>>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task UploadDocumentAsync_ShouldSaveToDatabase()
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.pdf");
            mockFile.Setup(f => f.OpenReadStream())
                   .Returns(() => new MemoryStream(Encoding.UTF8.GetBytes("test")));

            var service = new DocumentService(_mockContainerClient.Object, _context, _mockIndexer.Object, _mockLogger.Object);

            await service.UploadDocumentAsync(mockFile.Object, new List<Role> { Role.HR }, "TestUser", "Policy", "Test doc");

            var doc = await _context.DocumentMetadata.FirstOrDefaultAsync();
            Assert.NotNull(doc);
            Assert.Equal("test.pdf", doc.FileName);
        }

        [Fact]
        public async Task GetDocumentsByRoleAsync_ShouldReturnCorrectDocuments()
        {
            _context.DocumentMetadata.Add(new DocumentMetadata
            {
                FileName = "hr-doc.pdf",
                BlobUrl = "https://test.com/hr-doc.pdf",
                UploadedBy = "TestUser",
                DocumentType = "Policy",
                Description = "HR Document",
                AllowedRoles = new List<DocumentAllowedRole> { new() { Role = Role.HR } }
            });
            await _context.SaveChangesAsync();

            var service = new DocumentService(_mockContainerClient.Object, _context, _mockIndexer.Object, _mockLogger.Object);

            var result = await service.GetDocumentsByRoleAsync("HR");

            Assert.Single(result);
            Assert.Equal("hr-doc.pdf", result.First().FileName);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}