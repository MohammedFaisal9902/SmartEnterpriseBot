using Microsoft.AspNetCore.Identity;
using Moq;
using SmartEnterpriseBot.Application.Models.User;
using SmartEnterpriseBot.Infrastructure.Identity;
using SmartEnterpriseBot.Infrastructure.Servicees;
using Xunit;

namespace SmartEnterpriseBot.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _userService = new UserService(_userManagerMock.Object);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnMappedUsers()
        {
            var users = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", Email = "user1@example.com", FullName = "User 1", Department = "IT", PhoneNumber = "123" },
                new ApplicationUser { Id = "2", Email = "user2@example.com", FullName = "User 2", Department = "HR", PhoneNumber = "456" }
            }.AsQueryable();

            _userManagerMock.Setup(m => m.Users).Returns(users);
            _userManagerMock.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new List<string> { "Admin" });

            var result = await _userService.GetAllUsersAsync();

            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Contains("Admin", r.Roles));
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenExists()
        {
            var user = new ApplicationUser
            {
                Id = "1",
                Email = "test@example.com",
                FullName = "Test User",
                Department = "HR",
                PhoneNumber = "999"
            };

            _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "HR" });

            var result = await _userService.GetUserByIdAsync("1");

            Assert.NotNull(result);
            Assert.Equal("test@example.com", result?.Email);
            Assert.Contains("HR", result?.Roles!);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync((ApplicationUser?)null);

            var result = await _userService.GetUserByIdAsync("1");

            Assert.Null(result);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdate_WhenUserExists()
        {
            var user = new ApplicationUser { Id = "1", FullName = "Old Name", Department = "IT"};
            var request = new UpdateUserRequest
            {
                FullName = "New Name",
                Department = "IT",
                PhoneNumber = "123"
            };

            _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _userService.UpdateUserAsync("1", request);

            Assert.True(result);
            Assert.Equal("New Name", user.FullName);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync((ApplicationUser?)null);

            var request = new UpdateUserRequest();
            var result = await _userService.UpdateUserAsync("1", request);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldDelete_WhenUserExists()
        {
            var user = new ApplicationUser { Id = "1", FullName = "Old Name", Department = "IT" };
            _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _userService.DeleteUserAsync("1");

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            _userManagerMock.Setup(m => m.FindByIdAsync("1")).ReturnsAsync((ApplicationUser?)null);

            var result = await _userService.DeleteUserAsync("1");

            Assert.False(result);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldChangePassword_WhenUserExists()
        {
            var user = new ApplicationUser { Email = "user@example.com", FullName = "Old Name", Department = "IT" };
            _userManagerMock.Setup(m => m.FindByEmailAsync("user@example.com")).ReturnsAsync(user);
            _userManagerMock.Setup(m => m.ChangePasswordAsync(user, "old", "new")).ReturnsAsync(IdentityResult.Success);

            var result = await _userService.ChangePasswordAsync(new ChangePasswordRequest
            {
                Email = "user@example.com",
                OldPassword = "old",
                NewPassword = "new"
            });

            Assert.True(result);
        }

        [Fact]
        public async Task ChangePasswordAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            _userManagerMock.Setup(m => m.FindByEmailAsync("user@example.com")).ReturnsAsync((ApplicationUser?)null);

            var result = await _userService.ChangePasswordAsync(new ChangePasswordRequest
            {
                Email = "user@example.com",
                OldPassword = "old",
                NewPassword = "new"
            });

            Assert.False(result);
        }
    }
}
