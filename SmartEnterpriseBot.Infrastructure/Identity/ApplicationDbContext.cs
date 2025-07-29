using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartEnterpriseBot.Domain.Entities;

namespace SmartEnterpriseBot.Infrastructure.Identity
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<DocumentMetadata> DocumentMetadata { get; set; }
        public DbSet<DocumentAllowedRole> DocumentAllowedRoles { get; set; }
        public DbSet<KnowledgeEntry> KnowledgeEntries { get; set; }
        public DbSet<KnowledgeRole> KnowledgeRoles { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DocumentAllowedRole>()
                .HasOne(d => d.Document)
                .WithMany(r => r.AllowedRoles)
                .HasForeignKey(d => d.DocumentId);

            modelBuilder.Entity<KnowledgeRole>()
                .HasOne(d => d.KnowledgeEntry)
                .WithMany(r => r.AllowedRoles)
                .HasForeignKey(d => d.KnowledgeEntryId);
        }


    }
}
