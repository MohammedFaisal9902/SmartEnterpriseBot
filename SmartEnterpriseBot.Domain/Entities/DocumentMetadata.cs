using System.ComponentModel.DataAnnotations;

namespace SmartEnterpriseBot.Domain.Entities
{
    public class DocumentMetadata
    {
        public DocumentMetadata() {
            AllowedRoles = new HashSet<DocumentAllowedRole>();
        }
        [Key]
        public Guid RecordId { get; set; } = Guid.NewGuid();
        [Required]
        public string FileName { get; set; }
        [Required]
        public string BlobUrl { get; set; }
        public string UploadedBy { get; set; }
        public DateTime UploadDate { get; set; } = DateTime.UtcNow;
        public string DocumentType { get; set; }
        public string Description { get; set; }

        public virtual ICollection<DocumentAllowedRole> AllowedRoles { get; set; }
    }
}
