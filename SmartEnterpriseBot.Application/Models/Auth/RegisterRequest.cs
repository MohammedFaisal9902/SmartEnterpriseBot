using SmartEnterpriseBot.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEnterpriseBot.Application.Models.Auth
{
    public class RegisterRequest
    {
        public string Email { get; set; }
        public string UserName { get; set; }           
        public string FullName { get; set; }            
        public string? PhoneNumber { get; set; }      
        public string Department { get; set; }   
        public Role Role { get; set; }
        public string Password { get; set; }
    }
}

