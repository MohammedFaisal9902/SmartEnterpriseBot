using Azure.Search.Documents;
using Microsoft.Extensions.Logging;
using SmartEnterpriseBot.Application.Interfaces;
using SmartEnterpriseBot.Infrastructure.Models;

namespace SmartEnterpriseBot.Infrastructure.Services
{
    public class SearchIndexerService : ISearchIndexerService
    {
        private readonly SearchClient _searchClient;
        private readonly ILogger<SearchIndexerService> _logger;

        public SearchIndexerService(SearchClient searchClient, ILogger<SearchIndexerService> logger)
        {
            _searchClient = searchClient;
            _logger = logger;
        }

        public async Task<bool> UploadDocumentAsync(string content, List<string> allowedRoles)
        {
            var doc = new DocumentIndexDto
            {
                Content = content,
                AllowedRoles = allowedRoles
            };

            var batch = Azure.Search.Documents.Models.IndexDocumentsBatch.Create(
                Azure.Search.Documents.Models.IndexDocumentsAction.Upload(doc)
            );

            try
            {
                await _searchClient.IndexDocumentsAsync(batch);
                _logger.LogInformation("Document indexed successfully.");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Indexing failed.");
                return false;
            }
        }
    }
}
