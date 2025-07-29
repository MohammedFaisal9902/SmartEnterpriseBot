using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartEnterpriseBot.Application.Interfaces
{
    /// <summary>
    /// Provides functionality for indexing document content to enable search capabilities,
    /// along with access control based on user roles.
    /// </summary>
    public interface ISearchIndexerService
    {
        /// <summary>
        /// Uploads a document's content to the search index with associated role-based access control.
        /// </summary>
        /// <param name="content">The plain text content of the document to be indexed.</param>
        /// <param name="allowedRoles">A list of roles that are permitted to access the indexed content.</param>
        /// <returns>True if the indexing was successful; otherwise, false.</returns>
        Task<bool> UploadDocumentAsync(string content, List<string> allowedRoles);
    }
}
