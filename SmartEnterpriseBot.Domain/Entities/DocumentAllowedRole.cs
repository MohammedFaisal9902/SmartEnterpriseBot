using SmartEnterpriseBot.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartEnterpriseBot.Domain.Entities
{
    public class DocumentAllowedRole
    {
        [Key]
        public Guid RecordId { get; set; } = Guid.NewGuid();

        public Guid DocumentId { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public Role Role { get; set; }

        [JsonIgnore]
        [ForeignKey(nameof(DocumentId))]
        public DocumentMetadata? Document { get; set; }
    }
}
