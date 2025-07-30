using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Moq;
using SmartEnterpriseBot.Infrastructure.Models;
using SmartEnterpriseBot.Infrastructure.Services;
using Xunit;
using Microsoft.Extensions.Logging;

namespace SmartEnterpriseBot.Tests
{
    public class SearchIndexerServiceTests
    {
        private readonly Mock<SearchClient> _mockSearchClient;
        private readonly SearchIndexerService _searchIndexerService;

        public SearchIndexerServiceTests()
        {
            _mockSearchClient = new Mock<SearchClient>();
            var loggerMock = new Mock<ILogger<SearchIndexerService>>();
            _searchIndexerService = new SearchIndexerService(_mockSearchClient.Object, loggerMock.Object);
        }

        [Fact]
        public async Task UploadDocumentAsync_ReturnsTrue_WhenIndexingSucceeds()
        {
            _mockSearchClient
                .Setup(client => client.IndexDocumentsAsync(It.IsAny<IndexDocumentsBatch<DocumentIndexDto>>(), It.IsAny<IndexDocumentsOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Mock.Of<Response<IndexDocumentsResult>>());

            var content = "Test document content";
            var roles = new List<string> { "Admin", "HR" };

            var result = await _searchIndexerService.UploadDocumentAsync(content, roles);

            Assert.True(result);
        }

        [Fact]
        public async Task UploadDocumentAsync_ReturnsFalse_WhenExceptionThrown()
        {
            _mockSearchClient
                .Setup(client => client.IndexDocumentsAsync(It.IsAny<IndexDocumentsBatch<DocumentIndexDto>>(), It.IsAny<IndexDocumentsOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Simulated failure"));

            var content = "Failing document";
            var roles = new List<string> { "User" };

            var result = await _searchIndexerService.UploadDocumentAsync(content, roles);

            Assert.False(result);
        }
    }
}