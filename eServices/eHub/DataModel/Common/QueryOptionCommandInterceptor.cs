using System;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;

namespace CargoWise.eHub.DataModel.Common
{
	public class QueryOptionCommandInterceptor : DbCommandInterceptor, IDisposable
	{
		public QueryOptionCommandInterceptor(DbContext context, string option)
		{
			Context = context;
			Option = option;
			DbInterception.Add(this);
		}

		public DbContext Context { get; }
		public string Option { get; }

		public void Dispose()
		{
			DbInterception.Remove(this);
		}

		public override void ReaderExecuting(DbCommand command, DbCommandInterceptionContext<DbDataReader> interceptionContext)
		{
			if (interceptionContext.DbContexts.Contains(Context))
			{
				command.CommandText += $" OPTION({Option})";
			}
		}
	}
}
