using SmartEnterpriseBot.Domain.Enums;
namespace SmartEnterpriseBot.Application.Models
{
    public class ManageUserRolesRequest
    {
        public ManageUserRolesRequest() { 
           RolesToAdd = new List<Role>();
           RolesToRemove = new List<Role>();
        }

        public required string UserId { get; set; }
        public List<Role> RolesToAdd { get; set; }
        public List<Role> RolesToRemove { get; set; }
    }
}
