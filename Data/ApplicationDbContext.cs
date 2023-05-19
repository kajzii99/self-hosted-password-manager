using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SelfHostedPasswordManager.Models;


namespace SelfHostedPasswordManager.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public DbSet<Credential> Credentials { get; set; }

        private DbContextOptions<ApplicationDbContext> options;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> _options) : base(_options)
        {
            options = _options;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=ManagerDatabase.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Credential>().ToTable("Credentials");
            base.OnModelCreating(builder);
        }
    }
}