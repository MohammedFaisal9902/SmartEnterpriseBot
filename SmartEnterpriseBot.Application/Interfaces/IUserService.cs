using SmartEnterpriseBot.Application.Models.User;

namespace SmartEnterpriseBot.Application.Interfaces
{
    /// <summary>
    /// Interface for managing application users.
    /// Provides methods to retrieve, update, delete users, and manage passwords.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Retrieves a list of all users with their details and roles.
        /// </summary>
        /// <returns>A list of <see cref="UserResponse"/> representing all users.</returns>
        Task<List<UserResponse>> GetAllUsersAsync();

        /// <summary>
        /// Retrieves a specific user's details by their unique identifier.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>
        /// A <see cref="UserResponse"/> object representing the user, or null if the user does not exist.
        /// </returns>
        Task<UserResponse?> GetUserByIdAsync(string userId);

        /// <summary>
        /// Updates the details of an existing user.
        /// </summary>
        /// <param name="userId">The ID of the user to update.</param>
        /// <param name="request">An <see cref="UpdateUserRequest"/> containing the updated information.</param>
        /// <returns>True if the update was successful; otherwise, false.</returns>
        Task<bool> UpdateUserAsync(string userId, UpdateUserRequest request);

        /// <summary>
        /// Deletes a user from the system.
        /// </summary>
        /// <param name="userId">The ID of the user to delete.</param>
        /// <returns>True if the deletion was successful; otherwise, false.</returns>
        Task<bool> DeleteUserAsync(string userId);

        /// <summary>
        /// Changes a user's password.
        /// </summary>
        /// <param name="request">A <see cref="ChangePasswordRequest"/> containing email, old password, and new password.</param>
        /// <returns>True if the password change was successful; otherwise, false.</returns>
        Task<bool> ChangePasswordAsync(ChangePasswordRequest request);
    }
}
