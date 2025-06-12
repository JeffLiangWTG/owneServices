using CargoWise.eHub.DataModel.Comparer.DBInfo;
using System.Collections.Generic;
using System.Data.Entity;

namespace CargoWise.eHub.DataModel.Comparer
{
	public interface IDbInfoFactory
	{
		IList<ITableInfo> GetAllEfTablesWithColInfo(DbContext context, string[] filterTables);

		IList<ITableInfo> GetSQLTablesWithColInfo(string connectionString, string[] filterTables);
	}
}
