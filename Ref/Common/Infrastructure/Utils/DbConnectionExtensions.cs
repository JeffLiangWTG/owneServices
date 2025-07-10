using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public static class DbConnectionExtensions
	{
		public static void ChangeDatabaseIfNeeded(this IDbConnection connection, string dbName)
		{
			Argument.Argument.NotNull(connection, nameof(connection));
			Argument.Argument.NotNullOrEmpty(dbName, nameof(dbName));

			if (!dbName.Equals(connection.Database, StringComparison.OrdinalIgnoreCase))
			{
				((SqlConnection)connection).ChangeDatabase(dbName);
			}
		}

		public static void OpenIfNeeded(this IDbConnection connection)
		{
			Argument.Argument.NotNull(connection, nameof(connection));

			if (connection.State != ConnectionState.Open)
			{
				connection.Close();
				connection.Open();
			}
		}

		public static IDataReader ExecuteReader(this IDbConnection connection, string sql, params SqlParameter[] parameters)
		{
			using var command = connection.CreateCommand();
			command.CommandText = sql;
			if(command is SqlCommand sqlCommand && parameters is not null)
			{
				sqlCommand.Parameters.AddRange(parameters);
			}
			connection.OpenIfNeeded();
			return command.ExecuteReader(CommandBehavior.CloseConnection);
		}
	}
}
