using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	static class ConnectionExtensions
	{
		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public static int ExecuteNonQuery(this IDbConnection connection, string sqlText, IDbTransaction transaction = null, int timeoutDurationInSeconds = 3600)
		{
			Argument.Argument.NotNull(connection, nameof(connection));
			using (var cmd = connection.CreateCommand())
			{
				cmd.Transaction = transaction;
				cmd.CommandText = sqlText;
				cmd.CommandTimeout = timeoutDurationInSeconds;
				return cmd.ExecuteNonQuery();
			}
		}

		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public static object ExecuteScalar(this IDbConnection connection, string sqlText, IDbTransaction transaction = null)
		{
			Argument.Argument.NotNull(connection, nameof(connection));
			using (var cmd = connection.CreateCommand())
			{
				cmd.Transaction = transaction;
				cmd.CommandText = sqlText;
				return cmd.ExecuteScalar();
			}
		}
	}
}
