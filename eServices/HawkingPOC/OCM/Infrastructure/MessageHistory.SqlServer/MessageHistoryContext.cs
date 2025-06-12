using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OcmPoc.Infrastructure.MessageHistory.SqlServer.Configuration;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer
{
	public class MessageHistoryContext : DbContext
	{
		public MessageHistoryContext(DbContextOptions<MessageHistoryContext> options)
			: base(options)
		{
		}

		public DbSet<MessageFlow> Flows { get; set; }
		public DbSet<Conversation> Conversations { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
		}

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			var now = DateTime.UtcNow;
			foreach (var entry in ChangeTracker.Entries<BaseEntity>())
			{
				if (entry.State == EntityState.Added)
				{
					entry.Entity.Created = now;
					entry.Entity.LastUpdate = now;
				}

				if (entry.State == EntityState.Modified)
				{
					entry.Entity.LastUpdate = now;
				}
			}

			return base.SaveChangesAsync(cancellationToken);
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder
				.ApplyConfiguration(new ConversationConfiguration())
				.ApplyConfiguration(new MessageFlowConfiguration())
				.ApplyConfiguration(new MessageEventConfiguration());
		}
	}
}
