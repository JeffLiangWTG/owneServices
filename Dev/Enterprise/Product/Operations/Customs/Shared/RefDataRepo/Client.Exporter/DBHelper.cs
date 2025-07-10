using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Common;
using CargoWise.RefDbRepo.Client.Common;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.RefDataRepo.Ent.Client.Exporter
{
	public class DBHelper : IDBHelper
	{
		public DBHelper(IDbConnection conn)
		{
			Argument.NotNull(conn, nameof(conn));
			this.conn = conn;
		}
		readonly IDbConnection conn;

		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public IEnumerable<IDataRow> GetRecords(Type storageType, string column, IEnumerable<Guid> values, IDbTransaction transaction = null)
		{
			var tableName = Globals.IsTest ? SharedSQLBuilder.GetTableNameForSRDbOffline(storageType) : SharedSQLBuilder.GetTableName(storageType);
			var sqlText = $"SELECT * FROM dbo.{tableName} WHERE {column} IN ({string.Join(",", values.Select(x => $"'{x}'"))})";
			using (var cmd = conn.CreateCommand())
			{
				cmd.CommandText = sqlText;
				cmd.Transaction = transaction;
				using (var reader = cmd.ExecuteReader())
				{
					var dataTable = new DataTable();
					dataTable.Load(reader);
					foreach (DataRow row in dataTable.Rows)
					{
						yield return new DataRowAdapter(row);
					}
				}
			}
		}
	}
}
