using SmartEnterpriseBot.Application.Models;

namespace SmartEnterpriseBot.Application.Interfaces
{
    /// <summary>
    /// Service interface for managing user roles.
    /// </summary>
    public interface IRoleManagementService
    {
        /// <summary>
        /// Adds and/or removes roles from a specified user.
        /// </summary>
        /// <param name="request">The role management request containing the user ID, roles to add, and roles to remove.</param>
        /// <returns>
        /// A boolean indicating whether the role management operation was successful.
        /// Returns false if the user is not found or if any add/remove operation fails.
        /// </returns>
        Task<bool> ManageUserRolesAsync(ManageUserRolesRequest request);
    }
}
