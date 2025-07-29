using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartEnterpriseBot.Application.Interfaces;
using SmartEnterpriseBot.Domain.Enums;
using SmartEnterpriseBot.Infrastructure.Identity;
using System.Text;

namespace SmartEnterpriseBot.Infrastructure.Services
{
    public class AiAnswerService : IAiAnswerService
    {
        private readonly ApplicationDbContext _context;
        private readonly OpenAIClient _openAIClient;
        private readonly SearchClient _searchClient;
        private readonly string _deploymentName;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AiAnswerService> _logger;

        public AiAnswerService(OpenAIClient openAIClient, SearchClient searchClient, string deploymentName, IMemoryCache cache, ILogger<AiAnswerService> logger, ApplicationDbContext context)
        {
            _openAIClient = openAIClient;
            _searchClient = searchClient;
            _deploymentName = deploymentName;
            _cache = cache;
            _logger = logger;
            _context = context;
        }

        public async Task<string> GetAnswerAsync(string question, Role userRole, string userId)
        {
            try
            {
                _logger.LogInformation("User {UserId} asked: '{Question}' with role: {UserRole}", userId, question, userRole);

                // 1️⃣ Check structured knowledge base (SQL)
                var structured = await GetStructuredAnswerAsync(question, userRole);
                if (!string.IsNullOrWhiteSpace(structured))
                {
                    _logger.LogInformation("Found a structured answer.");
                    return structured;
                }

                // 2️⃣ Search Azure Cognitive Search index for relevant documents
                _logger.LogInformation("No structured answer. Performing search over index.");
                var unstructured = await SearchAndAnswerFromIndexAsync(question, userRole);
                if (!string.IsNullOrWhiteSpace(unstructured))
                {
                    _logger.LogInformation("Generated answer based on search results.");
                    return unstructured;
                }

                // 3️⃣ Final fallback
                _logger.LogWarning("No answer found in both structured and unstructured sources.");
                return "I couldn't find relevant information to answer your question.";
            }
            catch (Exception)
            {
                return "Sorry, the system encountered an error while processing your request. Please try again later.";
            }
        }

        private async Task<string?> GetStructuredAnswerAsync(string question, Role userRole)
        {
            var cacheKey = $"structured_{question.GetHashCode()}_{userRole}";
            if (_cache.TryGetValue(cacheKey, out string? cached))
            {
                _logger.LogInformation("Structured cache hit.");
                return cached;
            }

            _logger.LogInformation("Structured cache miss. Querying database.");

            var knowledge = await _context.KnowledgeEntries
                .Where(k => k.AllowedRoles.Any(r => r.Role == userRole) || userRole == Role.Admin)
                .Where(k => k.Title.Contains(question) || k.Content.Contains(question))
                .FirstOrDefaultAsync();

            if (knowledge != null)
            {
                _cache.Set(cacheKey, knowledge.Content, TimeSpan.FromHours(1));
                return knowledge.Content;
            }

            _logger.LogInformation("No structured knowledge found.");
            return null;
        }

        private async Task<string?> SearchAndAnswerFromIndexAsync(string question, Role userRole)
        {
            var cacheKey = $"search_{question.GetHashCode()}_{userRole}";
            if (_cache.TryGetValue(cacheKey, out string? cached))
            {
                _logger.LogInformation("Search cache hit.");
                return cached;
            }

            var searchOptions = new SearchOptions
            {
                Size = 5,
                IncludeTotalCount = true,
                QueryType = SearchQueryType.Semantic,
            };

            if (!string.Equals(userRole.ToString(), "Admin", StringComparison.OrdinalIgnoreCase))
            {
                // Filter results by allowedRoles only if not Admin
                searchOptions.Filter = $"allowedRoles/any(r: r eq '{userRole}')";
            }

            var searchResults = await _searchClient.SearchAsync<SearchDocument>(question, searchOptions);


            var sb = new StringBuilder();
            await foreach (var result in searchResults.Value.GetResultsAsync())
            {
                if (result.Document.TryGetValue("content", out var content))
                {
                    sb.AppendLine(content.ToString());
                    sb.AppendLine("\n---\n");
                }
            }

            if (sb.Length == 0)
            {
                _logger.LogWarning("No search results for role {Role}", userRole);
                return null;
            }

            var prompt = $"Based on these relevant documents:\n\n{sb}\nQuestion: {question}";
            _logger.LogInformation("Sending prompt to OpenAI.");

            var chatOptions = new ChatCompletionsOptions
            {
                Messages =
                {
                    new ChatMessage(ChatRole.System, "You are a helpful enterprise assistant. Provide concise answers based on documents."),
                    new ChatMessage(ChatRole.User, prompt)
                },
                Temperature = 0.2f,
                MaxTokens = 500
            };

            var completion = await _openAIClient.GetChatCompletionsAsync(_deploymentName, chatOptions);
            var answer = completion.Value.Choices.FirstOrDefault()?.Message.Content?.Trim();

            if (!string.IsNullOrWhiteSpace(answer))
            {
                _cache.Set(cacheKey, answer, TimeSpan.FromMinutes(30));
                return answer;
            }

            _logger.LogError("OpenAI returned empty or null answer.");
            return null;
        }
    }
}
