using Microsoft.EntityFrameworkCore;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer
{
	public interface IContextFactory<TContext>
		where TContext : DbContext
    {
		TContext Create();
    }
}
