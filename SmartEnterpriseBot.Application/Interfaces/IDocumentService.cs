using Microsoft.AspNetCore.Http;
using SmartEnterpriseBot.Domain.Entities;
using SmartEnterpriseBot.Domain.Enums;

namespace SmartEnterpriseBot.Application.Interfaces
{
    /// <summary>
    /// Provides functionality for uploading and retrieving documents with role-based access control.
    /// </summary>
    public interface IDocumentService
    {
        /// <summary>
        /// Uploads a document to the system along with associated metadata and role-based access permissions.
        /// </summary>
        /// <param name="file">The document file to upload.</param>
        /// <param name="allowedRoles">A list of roles that are allowed to access this document.</param>
        /// <param name="uploadedBy">The identifier (e.g., username or user ID) of the person uploading the document.</param>
        /// <param name="type">The type/category of the document.</param>
        /// <param name="description">A description of the document.</param>
        /// <returns>The URI or identifier of the uploaded document.</returns>
        Task<string> UploadDocumentAsync(IFormFile file, List<Role> allowedRoles, string uploadedBy, string type, string description);

        /// <summary>
        /// Retrieves a list of documents that are accessible by the specified user role.
        /// </summary>
        /// <param name="userRole">The role of the user requesting access to documents.</param>
        /// <returns>A list of document metadata accessible to the user role.</returns>
        Task<List<DocumentMetadata>> GetDocumentsByRoleAsync(string userRole);
    }
}
