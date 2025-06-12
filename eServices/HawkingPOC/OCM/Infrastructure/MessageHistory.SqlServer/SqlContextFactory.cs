using Microsoft.EntityFrameworkCore;
using OcmPoc.Utils.Config;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer
{
	public class SqlContextFactory : IContextFactory<MessageHistoryContext>
	{
		readonly DbContextOptions<MessageHistoryContext> contextOptions;

		public SqlContextFactory(SqlServerConfig config)
		{
			contextOptions = new DbContextOptionsBuilder<MessageHistoryContext>()
				.UseSqlServer(config.ToConnectionString())
				.Options;
		}

		public MessageHistoryContext Create()
		{
			return new MessageHistoryContext(contextOptions);
		}
	}
}
