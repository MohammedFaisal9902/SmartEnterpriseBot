using Microsoft.EntityFrameworkCore;
using SmartEnterpriseBot.Domain.Entities;
using SmartEnterpriseBot.Domain.Enums;
using SmartEnterpriseBot.Infrastructure.Identity;
using SmartEnterpriseBot.Infrastructure.Servicees;

namespace SmartEnterpriseBot.Tests
{
    public class KnowledgeServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly KnowledgeService _service;

        public KnowledgeServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new KnowledgeService(_context);
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task AddKnowledgeEntryAsync_ShouldAddEntry()
        {
            var entry = new KnowledgeEntry
            {
                Title = "Test Title",
                Content = "Test Content",
                CreatedBy = "TestUser",
                UpdatedBy = "TestUser",
                AllowedRoles = new List<KnowledgeRole>
                {
                   new KnowledgeRole { Role = Role.IT }
                }
            };

            var roles = new List<Role> { Role.IT };

            var id = await _service.AddKnowledgeEntryAsync(entry, roles);

            var result = await _context.KnowledgeEntries
                .Include(k => k.AllowedRoles)
                .FirstOrDefaultAsync(e => e.RecordId == id);

            Assert.NotNull(result);
            Assert.Equal("Test Title", result.Title);
            Assert.Single(result.AllowedRoles);
            Assert.Equal(Role.IT, result.AllowedRoles.First().Role);
        }


        [Fact]
        public async Task UpdateKnowledgeEntryAsync_ShouldUpdateEntry()
        {
            var recordId = Guid.NewGuid();

            var entry = new KnowledgeEntry
            {
                RecordId = recordId,
                Title = "Old Title",
                Content = "Old Content",
                CreatedBy = "User1",
                UpdatedBy = "User1",
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow,
                AllowedRoles = new List<KnowledgeRole>
                {
                    new KnowledgeRole { Role = Role.IT }
                }
            };

            _context.KnowledgeEntries.Add(entry);
            await _context.SaveChangesAsync();

            var updatedEntry = new KnowledgeEntry
            {
                RecordId = recordId,
                Title = "New Title",
                Content = "New Content",
                UpdatedBy = "User2"
            };

            var newRoles = new List<Role> { Role.HR };

            var result = await _service.UpdateKnowledgeEntryAsync(updatedEntry, newRoles);

            Assert.True(result);

            var updated = await _context.KnowledgeEntries
                .Include(e => e.AllowedRoles)
                .FirstOrDefaultAsync(e => e.RecordId == recordId);

            Assert.NotNull(updated);
            Assert.Equal("New Title", updated.Title);
            Assert.Equal("New Content", updated.Content);
            Assert.Equal("User2", updated.UpdatedBy);
            Assert.Single(updated.AllowedRoles);
            Assert.Equal(Role.HR, updated.AllowedRoles.First().Role);
        }

        [Fact]
        public async Task DeleteKnowledgeEntryAsync_ShouldDeleteEntry()
        {
            var entry = new KnowledgeEntry
            {
                Title = "ToDelete",
                Content = "Content",
                CreatedBy = "User",
                UpdatedBy = "User",
                AllowedRoles = new List<KnowledgeRole> { new KnowledgeRole { Role = Role.IT } }
            };
            _context.KnowledgeEntries.Add(entry);
            await _context.SaveChangesAsync();

            var result = await _service.DeleteKnowledgeEntryAsync(entry.RecordId);

            var deleted = await _context.KnowledgeEntries.FindAsync(entry.RecordId);
            Assert.True(result);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task GetKnowledgeEntriesByRoleAsync_ShouldReturnCorrectEntries()
        {
            _context.KnowledgeEntries.AddRange(
                new KnowledgeEntry
                {
                    Title = "IT Doc",
                    Content = "Info",
                    CreatedBy = "User",
                    UpdatedBy = "User",
                    AllowedRoles = new List<KnowledgeRole> { new KnowledgeRole { Role = Role.IT } }
                },
                new KnowledgeEntry
                {
                    Title = "HR Doc",
                    Content = "Info",
                    CreatedBy = "User",
                    UpdatedBy = "User",
                    AllowedRoles = new List<KnowledgeRole> { new KnowledgeRole { Role = Role.HR } }
                });
            await _context.SaveChangesAsync();

            var results = await _service.GetKnowledgeEntriesByRoleAsync(Role.IT);
            Assert.Single(results);
            Assert.Equal("IT Doc", results.First().Title);
        }

        [Fact]
        public async Task GetKnowledgeEntryByIdAsync_ShouldReturnCorrectEntry()
        {
            var entry = new KnowledgeEntry
            {
                Title = "Find Me",
                Content = "Important",
                CreatedBy = "User",
                UpdatedBy = "User",
                AllowedRoles = new List<KnowledgeRole> { new KnowledgeRole { Role = Role.IT } }
            };
            _context.KnowledgeEntries.Add(entry);
            await _context.SaveChangesAsync();

            var result = await _service.GetKnowledgeEntryByIdAsync(entry.RecordId);

            Assert.NotNull(result);
            Assert.Equal("Find Me", result.Title);
        }
    }
}
