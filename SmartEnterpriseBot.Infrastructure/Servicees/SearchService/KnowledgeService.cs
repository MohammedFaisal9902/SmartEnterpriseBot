using Microsoft.EntityFrameworkCore;
using SmartEnterpriseBot.Application.Interfaces;
using SmartEnterpriseBot.Domain.Entities;
using SmartEnterpriseBot.Domain.Enums;
using SmartEnterpriseBot.Infrastructure.Identity;

namespace SmartEnterpriseBot.Infrastructure.Servicees.SearchService
{
    public class KnowledgeService : IKnowledgeService
    {
        private readonly ApplicationDbContext _context;

        public KnowledgeService(ApplicationDbContext applicationDbContext) {
            _context = applicationDbContext;
        }

        public async Task<Guid> AddKnowledgeEntryAsync(KnowledgeEntry data)
        {
            var entry = new KnowledgeEntry
            {
                Title = data.Title,
                Content = data.Content,
                CreatedBy = data.CreatedBy,
                UpdatedBy = data.UpdatedBy,
                AllowedRoles = data.AllowedRoles,
            };

            _context.KnowledgeEntries.Add(entry);
            await _context.SaveChangesAsync();
            return entry.RecordId;
        }

        public async Task<bool> UpdateKnowledgeEntryAsync(KnowledgeEntry update)
        {
            var entry = await _context.KnowledgeEntries
                .Include(e => e.AllowedRoles)
                .FirstOrDefaultAsync(e => e.RecordId == update.RecordId);

            if (entry == null)
                return false;

            entry.Title = update.Title;
            entry.Content = update.Content;
            entry.UpdatedDate = DateTime.UtcNow;
            entry.UpdatedBy = update.UpdatedBy;

            if (entry.AllowedRoles.Any())
            {
                _context.KnowledgeRoles.RemoveRange(entry.AllowedRoles);
            }

            var newKnowledgeRoles = update.AllowedRoles.Select(r => new KnowledgeRole
            {
                Role = r.Role,
                KnowledgeEntry = entry
            }).ToList();

            _context.KnowledgeRoles.AddRange(newKnowledgeRoles);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteKnowledgeEntryAsync(Guid recordId)
        {
            var entry = await _context.KnowledgeEntries
                .Include(e => e.AllowedRoles)
                .FirstOrDefaultAsync(e => e.RecordId == recordId);

            if (entry == null)
                return false;

            _context.KnowledgeEntries.Remove(entry);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<KnowledgeEntry>> GetKnowledgeEntriesByRoleAsync(Role role, int page = 1, int pageSize = 10)
        {
            var query = _context.KnowledgeEntries
                .Include(e => e.AllowedRoles)
                .AsQueryable();

            if (role != Role.Admin)
            {
                query = query.Where(e => e.AllowedRoles.Any(ar => ar.Role == role));
            }

            return await query
                .OrderByDescending(e => e.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }


        public async Task<KnowledgeEntry?> GetKnowledgeEntryByIdAsync(Guid id)
        {
            return await _context.KnowledgeEntries
                .Include(e => e.AllowedRoles)
                .FirstOrDefaultAsync(e => e.RecordId == id);
        }

    }
}
