using SmartEnterpriseBot.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEnterpriseBot.Application.Models.User
{
    public class ChangeUserRoleRequest
    {
        public required string UserId { get; set; }
        public required Role NewRole { get; set; }
    }
}
