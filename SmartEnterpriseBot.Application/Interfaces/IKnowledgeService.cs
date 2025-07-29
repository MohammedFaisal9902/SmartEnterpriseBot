using SmartEnterpriseBot.Domain.Entities;
using SmartEnterpriseBot.Domain.Enums;

namespace SmartEnterpriseBot.Application.Interfaces
{
    public interface IKnowledgeService
    {
        Task<Guid> AddKnowledgeEntryAsync(KnowledgeEntry entry, List<Role> allowedRoles);

        Task<KnowledgeEntry?> GetKnowledgeEntryByIdAsync(Guid id);

        Task<bool> UpdateKnowledgeEntryAsync(KnowledgeEntry entry, List<Role> allowedRoles);

        Task<bool> DeleteKnowledgeEntryAsync(Guid id);

        Task<List<KnowledgeEntry>> GetKnowledgeEntriesByRoleAsync(Role role, int page = 1, int pageSize = 10);
    }
}
