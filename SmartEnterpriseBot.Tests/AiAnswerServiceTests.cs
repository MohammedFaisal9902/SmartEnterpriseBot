using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using SmartEnterpriseBot.Domain.Entities;
using SmartEnterpriseBot.Domain.Enums;
using SmartEnterpriseBot.Infrastructure.Identity;
using SmartEnterpriseBot.Infrastructure.Services;

namespace SmartEnterpriseBot.Tests.Services;

public class AiAnswerServiceTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly Mock<ILogger<AiAnswerService>> _loggerMock;
    private readonly Mock<OpenAIClient> _openAIClientMock;
    private readonly Mock<SearchClient> _searchClientMock;
    private readonly AiAnswerService _service;
    private readonly string _deploymentName = "test-deployment";

    public AiAnswerServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _cache = new MemoryCache(new MemoryCacheOptions());
        _loggerMock = new Mock<ILogger<AiAnswerService>>();
        _openAIClientMock = new Mock<OpenAIClient>();
        _searchClientMock = new Mock<SearchClient>();

        _service = new AiAnswerService(
            _openAIClientMock.Object,
            _searchClientMock.Object,
            _deploymentName,
            _cache,
            _loggerMock.Object,
            _context);
    }

    [Fact]
    public async Task GetAnswerAsync_ShouldReturnStructuredAnswer_WhenFoundInDatabase()
    {
        var question = "Leave Policy";
        var userRole = Role.IT;
        var userId = "user1";
        var expectedContent = "Our leave policy allows 20 days annual leave.";
        var entryId = Guid.NewGuid();

        var entry = new KnowledgeEntry
        {
            RecordId = entryId,
            Title = "Leave Policy",
            Content = expectedContent,
            CreatedBy = "admin",
            UpdatedBy = "admin",
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow,
            AllowedRoles = new List<KnowledgeRole>
            {
                new KnowledgeRole
                {
                    Role = Role.IT,
                    KnowledgeEntryId = entryId
                }
            }
        };

        _context.KnowledgeEntries.Add(entry);
        await _context.SaveChangesAsync();

        var result = await _service.GetAnswerAsync(question, userRole, userId);

        Assert.NotNull(result);
        Assert.Equal(expectedContent, result);
    }

    [Fact]
    public async Task GetAnswerAsync_ShouldReturnCachedAnswer_WhenAnswerIsCached()
    {
        var question = "Cached Policy";
        var userRole = Role.IT;
        var userId = "user1";
        var cachedAnswer = "This is a cached answer";
        var cacheKey = $"structured_{question.GetHashCode()}_{userRole}";

        _cache.Set(cacheKey, cachedAnswer, TimeSpan.FromHours(1));

        var result = await _service.GetAnswerAsync(question, userRole, userId);

        Assert.Equal(cachedAnswer, result);
    }


    [Fact]
    public async Task GetAnswerAsync_ShouldReturnCachedUnstructuredAnswer_WhenCached()
    {
        var question = "Cached Unstructured";
        var userRole = Role.IT;
        var userId = "user1";
        var cachedAnswer = "This is a cached unstructured answer";
        var cacheKey = $"search_{question.GetHashCode()}_{userRole}";

        _cache.Set(cacheKey, cachedAnswer, TimeSpan.FromMinutes(30));

        var result = await _service.GetAnswerAsync(question, userRole, userId);

        Assert.Equal(cachedAnswer, result);
    }

    [Fact]
    public async Task GetAnswerAsync_ShouldReturnErrorMessage_WhenExceptionOccurs()
    {
        var question = "Error Question";
        var userRole = Role.IT;
        var userId = "user1";

        _context.Dispose(); // Trigger error

        var result = await _service.GetAnswerAsync(question, userRole, userId);

        Assert.Contains("encountered an error", result, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("try again later", result, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _cache?.Dispose();
    }
}
