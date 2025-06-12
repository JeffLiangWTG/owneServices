using System.Configuration;
using Hawking.Elk.EhubArchiveMessages.Model;
using Microsoft.EntityFrameworkCore;

namespace Hawking.Elk.EhubArchiveMessages.Data.Context
{
    public class EhubClientDbContext : DbContext
    {
        const string DefaultConnectionString = "EhubClientDbContext";

        public EhubClientDbContext(DbContextOptions options)
            : base(DefaultDbContextOptions(options))
        {
        }

        static DbContextOptions DefaultDbContextOptions(DbContextOptions options)
        {
            if (options == null)
            {
                return
                    SqlServerDbContextOptionsExtensions
                    .UseSqlServer(
                        new DbContextOptionsBuilder(),
                        ConfigurationManager.ConnectionStrings[DefaultConnectionString].ConnectionString).Options;
            }

            return options;
        }

        public virtual DbSet<EhubClient> EhubClients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var config = modelBuilder.Entity<EhubClient>();

            config.ToTable("eHubClient");
            config.Property(e => e.TrackingId).HasColumnName("CC_PK");
        }
    }
}
