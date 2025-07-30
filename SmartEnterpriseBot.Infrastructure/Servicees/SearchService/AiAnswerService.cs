using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SmartEnterpriseBot.Application.Interfaces;
using SmartEnterpriseBot.Domain.Enums;
using SmartEnterpriseBot.Infrastructure.Identity;
using System.Text;

namespace SmartEnterpriseBot.Infrastructure.Servicees.SearchService
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

                var structured = await GetStructuredAnswerAsync(question, userRole);
                if (!string.IsNullOrWhiteSpace(structured))
                {
                    _logger.LogInformation("Found a structured answer.");
                    return structured;
                }

                _logger.LogInformation("No structured answer. Performing search over index.");
                var unstructured = await SearchAndAnswerFromIndexAsync(question, userRole);
                if (!string.IsNullOrWhiteSpace(unstructured))
                {
                    _logger.LogInformation("Generated answer based on search results.");
                    return unstructured;
                }

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

            _logger.LogInformation("Answer not found in cache. Querying database.");

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

            _logger.LogInformation("Starting search for question: '{Question}' with role: {Role}", question, userRole);

            var searchOptions = new SearchOptions
            {
                Size = 5,
                IncludeTotalCount = true,
                Select = { "content", "allowedRoles" },
            };

            if (!string.Equals(userRole.ToString(), "Admin", StringComparison.OrdinalIgnoreCase))
            {
                var filter = $"allowedRoles/any(r: r eq '{userRole}')";
                searchOptions.Filter = filter;
                _logger.LogInformation("Applying filter: {Filter}", filter);
            }
            else
            {
                _logger.LogInformation("Admin role - no filter applied");
            }

            try
            {
                var searchResults = await _searchClient.SearchAsync<SearchDocument>(question, searchOptions);

                _logger.LogInformation("Search completed. Total results: {TotalCount}", searchResults.Value.TotalCount);

                var sb = new StringBuilder();
                int resultCount = 0;

                await foreach (var result in searchResults.Value.GetResultsAsync())
                {
                    resultCount++;
                    _logger.LogInformation("Processing result {Count}, Score: {Score}", resultCount, result.Score);

                    _logger.LogInformation("Available fields: {Fields}", string.Join(", ", result.Document.Keys));

                    if (result.Document.TryGetValue("content", out var content))
                    {
                        _logger.LogInformation("Found content, length: {Length}", content?.ToString()?.Length ?? 0);
                        sb.AppendLine(content?.ToString());
                        sb.AppendLine("\n---\n");
                    }
                    else
                    {
                        _logger.LogWarning("No 'content' field found in result {Count}", resultCount);
                    }
                }

                _logger.LogInformation("Total results processed: {Count}, Content length: {ContentLength}",
                    resultCount, sb.Length);

                if (sb.Length == 0)
                {
                    _logger.LogWarning("No search results found for role {Role} and question '{Question}'", userRole, question);

                    _logger.LogInformation("Attempting search without role filter for debugging...");
                    var debugOptions = new SearchOptions { Size = 3 };
                    var debugResults = await _searchClient.SearchAsync<SearchDocument>(question, debugOptions);

                    var debugCount = 0;
                    await foreach (var result in debugResults.Value.GetResultsAsync())
                    {
                        debugCount++;
                        _logger.LogInformation("Debug result {Count}: Fields available: {Fields}",
                            debugCount, string.Join(", ", result.Document.Keys));
                    }

                    return null;
                }

                var prompt = $"Based on these relevant documents:\n\n{sb}\nQuestion: {question}\n\nPlease provide a concise and helpful answer based only on the information provided above.";
                _logger.LogInformation("Sending prompt to OpenAI. Prompt length: {Length}", prompt.Length);

                var chatOptions = new ChatCompletionsOptions
                {
                    Messages =
                    {
                          new ChatMessage(ChatRole.System, "You are a helpful enterprise assistant. Provide concise answers based only on the documents provided. If the documents don't contain relevant information, say so."),
                          new ChatMessage(ChatRole.User, prompt)
                    },
                    Temperature = 0.2f,
                    MaxTokens = 800
                };

                var completion = await _openAIClient.GetChatCompletionsAsync(_deploymentName, chatOptions);
                var answer = completion.Value.Choices.FirstOrDefault()?.Message.Content?.Trim();

                _logger.LogInformation("OpenAI response length: {Length}", answer?.Length ?? 0);

                if (!string.IsNullOrWhiteSpace(answer))
                {
                    _cache.Set(cacheKey, answer, TimeSpan.FromMinutes(30));
                    return answer;
                }

                _logger.LogError("OpenAI returned empty or null answer.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during search operation");
                throw;
            }
        }
    }
}
