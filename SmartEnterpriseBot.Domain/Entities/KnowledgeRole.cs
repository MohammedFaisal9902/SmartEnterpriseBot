using SmartEnterpriseBot.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartEnterpriseBot.Domain.Entities
{
    public class KnowledgeRole
    {
        [Key]
        public Guid RecordId { get; set; } = Guid.NewGuid();

        public Role Role { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public Guid KnowledgeEntryId { get; set; }

        [JsonIgnore]
        [ForeignKey("KnowledgeEntryId")]
        public KnowledgeEntry? KnowledgeEntry { get; set; }
    }
}
