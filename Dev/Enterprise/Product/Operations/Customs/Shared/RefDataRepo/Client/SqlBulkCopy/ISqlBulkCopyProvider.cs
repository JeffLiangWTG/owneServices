using CargoWise.Data.Providers.Common;
using SqlBulkCopyOptions = CargoWise.Data.Providers.Common.SqlBulkCopyOptions;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public interface ISqlBulkCopyProvider
	{
		ISqlBulkCopy GetSqlBulkCopy(System.Data.Common.DbConnection connection, SqlBulkCopyOptions options, System.Data.Common.DbTransaction transaction);
	}
}
