using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEnterpriseBot.Domain.Entities
{
    public class KnowledgeEntry
    {
        public KnowledgeEntry() {

            AllowedRoles = new HashSet<KnowledgeRole>();
        }

        [Key]
        public Guid RecordId { get; set; } = Guid.NewGuid();
        public string Title { get; set; }
        public string Content { get; set; }
        public string UpdatedBy { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
        public virtual ICollection<KnowledgeRole> AllowedRoles { get; set; }
    }

}
