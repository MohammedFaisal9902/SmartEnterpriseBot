using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEnterpriseBot.Application.Models.User
{
    public class UpdateUserRequest
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Department { get; set; }
    }
}
