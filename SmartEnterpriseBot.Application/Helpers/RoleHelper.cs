using SmartEnterpriseBot.Domain.Enums;

namespace SmartEnterpriseBot.Application.Helpers
{
    public static class RoleHelper
    {
        public static string GetRoleName(Role role)
        {
            return role.ToString();
        }

        public static List<string> GetAllRoleNames()
        {
            return Enum.GetNames(typeof(Role)).ToList();
        }
    }
}
