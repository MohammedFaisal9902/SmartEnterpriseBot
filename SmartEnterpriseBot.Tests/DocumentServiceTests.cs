using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using SmartEnterpriseBot.Domain.Entities;
using SmartEnterpriseBot.Domain.Enums;
using SmartEnterpriseBot.Infrastructure.Identity;
using SmartEnterpriseBot.Infrastructure.Services.Storage;
using System.IO;
using System.Text;
using Xunit;

namespace SmartEnterpriseBot.Tests
{
    public class DocumentServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<BlobContainerClient> _mockContainerClient;
        private readonly Mock<BlobClient> _mockBlobClient;

        public DocumentServiceTests()
        {
            // Create unique database for each test instance
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // Setup blob mocks
            _mockContainerClient = new Mock<BlobContainerClient>();
            _mockBlobClient = new Mock<BlobClient>();

            // Setup mock behavior
            _mockContainerClient
                .Setup(c => c.GetBlobClient(It.IsAny<string>()))
                .Returns(_mockBlobClient.Object);

            _mockBlobClient
                .Setup(b => b.Uri)
                .Returns(new Uri("https://test.blob.core.windows.net/container/test-file.pdf"));

            _mockBlobClient
                .Setup(b => b.UploadAsync(It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Mock.Of<Azure.Response<BlobContentInfo>>()));
        }

        [Fact]
        public async Task UploadDocumentAsync_ShouldUploadAndSaveMetadata_Simple()
        {
            var fileMock = CreateMockFile("sample.txt", "Sample content");
            var roles = new List<Role> { Role.HR };
            var uploadedBy = "TestUser";
            var docType = "Policy";
            var description = "Company HR Policy";

            var service = new DocumentService(_mockContainerClient.Object, _context);

            var blobUrl = await service.UploadDocumentAsync(fileMock.Object, roles, uploadedBy, docType, description);

            Assert.NotNull(blobUrl);
            Assert.Contains(".blob.core.windows.net", blobUrl);

            var doc = await _context.DocumentMetadata.Include(d => d.AllowedRoles).FirstOrDefaultAsync();
            Assert.NotNull(doc);
            Assert.Equal("sample.txt", doc.FileName);
            Assert.Equal(uploadedBy, doc.UploadedBy);
            Assert.Equal(docType, doc.DocumentType);
            Assert.Equal(description, doc.Description);
            Assert.Single(doc.AllowedRoles);
            Assert.Equal(Role.HR, doc.AllowedRoles.First().Role);
        }



        [Fact]
        public async Task GetDocumentsByRoleAsync_ShouldReturnFilteredDocuments()
        {
            // Arrange
            var service = new DocumentService(_mockContainerClient.Object, _context);

            var doc1 = new DocumentMetadata
            {
                RecordId = Guid.NewGuid(),
                FileName = "hr-file.pdf",
                BlobUrl = "https://test.blob.core.windows.net/hr-file.pdf",
                UploadedBy = "Admin",
                DocumentType = "PDF",
                Description = "HR Document",
                UploadDate = DateTime.UtcNow,
                AllowedRoles = new List<DocumentAllowedRole>
                {
                    new DocumentAllowedRole { Role = Role.HR }
                }
            };

            var doc2 = new DocumentMetadata
            {
                RecordId = Guid.NewGuid(),
                FileName = "it-file.pdf",
                BlobUrl = "https://test.blob.core.windows.net/it-file.pdf",
                UploadedBy = "Admin",
                DocumentType = "PDF",
                Description = "IT Document",
                UploadDate = DateTime.UtcNow,
                AllowedRoles = new List<DocumentAllowedRole>
                {
                    new DocumentAllowedRole { Role = Role.IT }
                }
            };

            _context.DocumentMetadata.AddRange(doc1, doc2);
            await _context.SaveChangesAsync();

            // Act
            var hrResults = await service.GetDocumentsByRoleAsync("HR");
            var itResults = await service.GetDocumentsByRoleAsync("IT");

            // Assert
            Assert.Single(hrResults);
            Assert.Equal("hr-file.pdf", hrResults.First().FileName);

            Assert.Single(itResults);
            Assert.Equal("it-file.pdf", itResults.First().FileName);
        }

        [Fact]
        public async Task GetDocumentsByRoleAsync_WithNoDocuments_ShouldReturnEmptyList()
        {
            // Arrange
            var service = new DocumentService(_mockContainerClient.Object, _context);

            // Act
            var results = await service.GetDocumentsByRoleAsync("HR");

            // Assert
            Assert.Empty(results);
        }

        [Fact]
        public async Task GetDocumentsByRoleAsync_WithMultipleRoles_ShouldReturnCorrectDocuments()
        {
            // Arrange
            var service = new DocumentService(_mockContainerClient.Object, _context);

            var doc = new DocumentMetadata
            {
                RecordId = Guid.NewGuid(),
                FileName = "multi-role-file.pdf",
                BlobUrl = "https://test.blob.core.windows.net/multi-role-file.pdf",
                UploadedBy = "Admin",
                DocumentType = "PDF",
                Description = "Multi-role Document",
                UploadDate = DateTime.UtcNow,
                AllowedRoles = new List<DocumentAllowedRole>
                {
                    new DocumentAllowedRole { Role = Role.HR },
                    new DocumentAllowedRole { Role = Role.IT }
                }
            };

            _context.DocumentMetadata.Add(doc);
            await _context.SaveChangesAsync();

            // Act
            var hrResults = await service.GetDocumentsByRoleAsync("HR");
            var itResults = await service.GetDocumentsByRoleAsync("IT");

            // Assert
            Assert.Single(hrResults);
            Assert.Single(itResults);
            Assert.Equal("multi-role-file.pdf", hrResults.First().FileName);
            Assert.Equal("multi-role-file.pdf", itResults.First().FileName);
        }

        [Theory]
        [InlineData("HR")]
        [InlineData("IT")]
        [InlineData("Admin")]
        public async Task GetDocumentsByRoleAsync_WithValidRoles_ShouldNotThrow(string role)
        {
            // Arrange
            var service = new DocumentService(_mockContainerClient.Object, _context);

            // Act & Assert
            var results = await service.GetDocumentsByRoleAsync(role);
            Assert.NotNull(results);
        }

        private Mock<IFormFile> CreateMockFile(string fileName, string content)
        {
            var fileMock = new Mock<IFormFile>();
            var bytes = Encoding.UTF8.GetBytes(content);
            var ms = new MemoryStream(bytes);

            fileMock.Setup(f => f.OpenReadStream()).Returns(() => {
                ms.Position = 0; // Reset stream position
                return ms;
            });
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(bytes.Length);
            fileMock.Setup(f => f.ContentType).Returns("text/plain");

            return fileMock;
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}