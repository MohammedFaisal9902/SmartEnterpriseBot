using Microsoft.AspNetCore.Identity;
using Moq;
using SmartEnterpriseBot.Application.Interfaces;
using SmartEnterpriseBot.Application.Models;
using SmartEnterpriseBot.Domain.Enums;
using SmartEnterpriseBot.Infrastructure.Identity;
using SmartEnterpriseBot.Infrastructure.Servicees.RoleService;

namespace SmartEnterpriseBot.Tests
{
    public class RoleManagementServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly IRoleManagementService _roleService;

        public RoleManagementServiceTests()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _roleService = new RoleManagementService(_userManagerMock.Object);
        }

        [Fact]
        public async Task ManageUserRolesAsync_ShouldAddAndRemoveRolesCorrectly()
        {
            var user = new ApplicationUser { Id = "123", UserName = "testuser", Department = "Admin", FullName = "Admin User" };

            _userManagerMock.Setup(m => m.FindByIdAsync("123"))
                .ReturnsAsync(user);

            _userManagerMock.Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin", "HR" });

            _userManagerMock.Setup(m => m.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock.Setup(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            var request = new ManageUserRolesRequest
            {
                UserId = "123",
                RolesToAdd = new List<Role> { Role.IT },
                RolesToRemove = new List<Role> { Role.HR }
            };

            var result = await _roleService.ManageUserRolesAsync(request);

            Assert.True(result);

            _userManagerMock.Verify(m => m.AddToRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("IT"))), Times.Once);
            _userManagerMock.Verify(m => m.RemoveFromRolesAsync(user, It.Is<IEnumerable<string>>(r => r.Contains("HR"))), Times.Once);
        }

        [Fact]
        public async Task ManageUserRolesAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            _userManagerMock.Setup(m => m.FindByIdAsync("invalid-id"))
                .ReturnsAsync((ApplicationUser)null!);

            var request = new ManageUserRolesRequest
            {
                UserId = "invalid-id",
                RolesToAdd = new List<Role> { Role.IT },
                RolesToRemove = new List<Role> { Role.Admin }
            };

            var result = await _roleService.ManageUserRolesAsync(request);

            Assert.False(result);
        }

        [Fact]
        public async Task ManageUserRolesAsync_ShouldReturnFalse_WhenAddFails()
        {
            var user = new ApplicationUser {Id = "123", UserName = "testuser", Department = "Test Department", FullName= "Test User" };

            _userManagerMock.Setup(m => m.FindByIdAsync("123"))
                .ReturnsAsync(user);

            _userManagerMock.Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(new List<string>());

            _userManagerMock.Setup(m => m.AddToRolesAsync(user, It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Add failed" }));

            var request = new ManageUserRolesRequest
            {
                UserId = "123",
                RolesToAdd = new List<Role> { Role.HR },
                RolesToRemove = new List<Role>()
            };

            var result = await _roleService.ManageUserRolesAsync(request);

            Assert.False(result);
        }
    }
}
