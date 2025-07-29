namespace SmartEnterpriseBot.Infrastructure.Models
{
    public class DocumentIndexDto
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Content { get; set; } = string.Empty;
        public List<string> AllowedRoles { get; set; } = new();
    }
}
