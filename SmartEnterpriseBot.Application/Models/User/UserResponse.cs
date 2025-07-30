using SmartEnterpriseBot.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEnterpriseBot.Application.Models.User
{
    public class UserResponse
    {
        public string Id { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string Department { get; set; } = default!;
        public string PhoneNumber { get; set; } = default!;
        public List<string> Roles { get; set; } = new List<string>();
    }
}
