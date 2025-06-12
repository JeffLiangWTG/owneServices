using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OcmPoc.Utils.Config;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer
{
	public class MessageHistoryContextDesignTimeFactory : IDesignTimeDbContextFactory<MessageHistoryContext>
	{
		public MessageHistoryContext CreateDbContext(string[] args)
		{
			return new MessageHistoryContext(
				new DbContextOptionsBuilder<MessageHistoryContext>()
					.UseSqlServer(new SqlServerConfig().ToConnectionString())
					.Options);
		}
	}
}
