using SmartEnterpriseBot.Domain.Entities;
using SmartEnterpriseBot.Domain.Enums;

namespace SmartEnterpriseBot.Application.Interfaces
{
    /// <summary>
    /// Provides an abstraction for managing knowledge entries with role-based access.
    /// </summary>
    public interface IKnowledgeService
    {
        /// <summary>
        /// Adds a new knowledge entry and associates it with the specified allowed roles.
        /// </summary>
        /// <param name="entry">The knowledge entry to add.</param>
        /// <returns>The unique identifier of the newly added knowledge entry.</returns>
        Task<Guid> AddKnowledgeEntryAsync(KnowledgeEntry entry);

        /// <summary>
        /// Retrieves a knowledge entry by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the knowledge entry.</param>
        /// <returns>The knowledge entry if found; otherwise, null.</returns>
        Task<KnowledgeEntry?> GetKnowledgeEntryByIdAsync(Guid id);

        /// <summary>
        /// Updates an existing knowledge entry and updates its associated allowed roles.
        /// </summary>
        /// <param name="entry">The updated knowledge entry.</param>
        /// <param name="allowedRoles">The updated list of roles allowed to access the entry.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        Task<bool> UpdateKnowledgeEntryAsync(KnowledgeEntry entry);

        /// <summary>
        /// Deletes a knowledge entry by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the knowledge entry to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        Task<bool> DeleteKnowledgeEntryAsync(Guid id);

        /// <summary>
        /// Retrieves a paginated list of knowledge entries that are accessible to the specified role.
        /// </summary>
        /// <param name="role">The role used to filter accessible knowledge entries.</param>
        /// <param name="page">The page number for pagination (default is 1).</param>
        /// <param name="pageSize">The number of items per page (default is 10).</param>
        /// <returns>A list of knowledge entries accessible by the specified role.</returns>
        Task<List<KnowledgeEntry>> GetKnowledgeEntriesByRoleAsync(Role role, int page = 1, int pageSize = 10);
    }
}
