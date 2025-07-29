using Microsoft.AspNetCore.Identity;

namespace SmartEnterpriseBot.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public required string Department { get; set; }
        public required string FullName { get; set; }

    }
}
