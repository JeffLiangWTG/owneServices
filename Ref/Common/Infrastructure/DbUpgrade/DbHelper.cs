using System.Data;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public static class DbHelper
	{
		public static int ExecuteNonQuery(IDbTransaction trans, string sql, int? commandTimeOut = null)
		{
			Argument.Argument.NotNull(trans, nameof(trans));
			Argument.Argument.NotNullOrEmpty(sql, nameof(sql));

			using (var cmd = CreateCommand(trans, sql, commandTimeOut))
			{
				return cmd.ExecuteNonQuery();
			}
		}

		public static object ExecuteScalar(IDbTransaction trans, string sql, int? commandTimeOut = null)
		{
			Argument.Argument.NotNull(trans, nameof(trans));
			Argument.Argument.NotNullOrEmpty(sql, nameof(sql));

			using (var cmd = CreateCommand(trans, sql, commandTimeOut))
			{
				return cmd.ExecuteScalar();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public static IDbCommand CreateCommand(IDbTransaction trans, string sql, int? commandTimeOut = null)
		{
			Argument.Argument.NotNull(trans, nameof(trans));
			Argument.Argument.NotNullOrEmpty(sql, nameof(sql));

			var result = trans.Connection?.CreateCommand();
			result.Connection = trans.Connection;
			result.Transaction = trans;
			result.CommandText = sql;
			if (commandTimeOut.HasValue)
			{
				result.CommandTimeout = commandTimeOut.Value;
			}
			return result;
		}

		public static int SetSystemVersioningOn(IDbTransaction trans, string tableName)
		{
			Argument.Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.Argument.NotNull(trans, nameof(trans));
			var sql = $"ALTER TABLE {tableName} SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.{tableName}History))";
			using (var cmd = CreateCommand(trans, sql))
			{
				return cmd.ExecuteNonQuery();
			}
		}

		public static int SetSystemVersioningOff(IDbTransaction trans, string tableName)
		{
			Argument.Argument.NotNullOrEmpty(tableName, nameof(tableName));
			Argument.Argument.NotNull(trans, nameof(trans));
			var sql = $@"IF OBJECTPROPERTY(OBJECT_ID('{tableName}'), 'TableTemporalType') = 2
ALTER TABLE {tableName} SET (SYSTEM_VERSIONING = OFF)";
			using (var cmd = CreateCommand(trans, sql))
			{
				return cmd.ExecuteNonQuery();
			}
		}
	}
}
