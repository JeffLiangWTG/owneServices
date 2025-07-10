using CargoWise.Data.Providers.Common;
using CargoWise.Data.SqlServer;
using SqlBulkCopyOptions = CargoWise.Data.Providers.Common.SqlBulkCopyOptions;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class SqlBulkCopyProvider : ISqlBulkCopyProvider
	{
		public ISqlBulkCopy GetSqlBulkCopy(System.Data.Common.DbConnection connection, SqlBulkCopyOptions options, System.Data.Common.DbTransaction transaction)
		{
			return new SqlServerBulkCopy(connection, options, transaction);
		}
	}
}
