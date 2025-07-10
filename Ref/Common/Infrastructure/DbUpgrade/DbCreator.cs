using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public class DbCreator : IDbCreator
	{
		public DbCreator(IDbConnection connection)
		{
			Argument.Argument.NotNull(connection, nameof(connection));
			this.connection = connection;
		}

		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public void ExcuteDbScript(string dbName, string sqlScript, IDbTransaction transaction = null, bool retryConnection = false)
		{
			Argument.Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.Argument.NotNull(sqlScript, nameof(sqlScript));
			if (retryConnection)
			{
				connection.OpenIfNeeded();
			}
			connection.ChangeDatabaseIfNeeded(dbName);
			var builder = new StringBuilder();
			var reader = new StringReader(sqlScript);
			string line = null;
			while ((line = reader.ReadLine()) != null)
			{
				if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
				{
					ExecuteNonQuery(builder.ToString(), transaction);
					builder.Clear();
				}
				else
				{
					builder.AppendLine(line);
				}
			}
			var sqlText = builder.ToString().Trim();
			if (!string.IsNullOrEmpty(sqlText))
			{
				ExecuteNonQuery(sqlText, transaction);
			}
		}

		static bool IsSqlCmd(string sqlText)
		{
			return string.IsNullOrEmpty(sqlText) || sqlText.StartsWith(":", StringComparison.OrdinalIgnoreCase) || sqlText.Contains(":setvar") || sqlText.Contains("$(DatabaseName)") || sqlText.Contains("SET NOEXEC ON");
		}

		void ExecuteNonQuery(string sqlText, IDbTransaction transaction)
		{
			if (!IsSqlCmd(sqlText))
			{
				connection.ExecuteNonQuery(sqlText, transaction);
			}
		}

		[SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
		public void CreateDatabase(string dbName, string filePath, string collation = null)
		{
			connection.ChangeDatabaseIfNeeded("master");
			var builder = new StringBuilder($@"CREATE DATABASE [{dbName}]
CONTAINMENT = NONE");
			if (!string.IsNullOrEmpty(filePath))
			{
				builder.AppendLine(CultureInfo.InvariantCulture, $@"
ON PRIMARY
( NAME = N'{dbName}', FILENAME = N'{filePath}\{dbName}.mdf' , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'{dbName}_log', FILENAME = N'{filePath}\{dbName}.ldf' , MAXSIZE = 2048GB , FILEGROWTH = 10% )");
			}
			if (!string.IsNullOrEmpty(collation))
			{
				builder.AppendLine(CultureInfo.InvariantCulture, $" COLLATE {collation}");
			}

			using var cmd = connection.CreateCommand();
			Console.WriteLine($"Creating database {dbName} {(!string.IsNullOrEmpty(filePath) ? $"with file path: {filePath}" : "")} {(!string.IsNullOrEmpty(collation) ? $"and collation: {collation}" : "using the server's default collation")}");
			cmd.CommandText = builder.ToString();
			cmd.ExecuteNonQuery();
		}

		public void DropDatabase(string dbName)
		{
			connection.OpenIfNeeded();
			const string sqlText = """
									USE [master];
									IF db_id(@dbName) IS NOT NULL
									BEGIN
										DECLARE @sql NVARCHAR(MAX) = 
											'ALTER DATABASE [' + @dbName + '] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [' + @dbName + ']';
										EXEC(@sql);
									END
									""";
			using var cmd = connection.CreateCommand();
			Console.WriteLine($"Dropping database {dbName}");
			cmd.CommandText = sqlText;
			cmd.Parameters.Add(new SqlParameter("@dbName", dbName));
			cmd.ExecuteNonQuery();
		}

		public void ForceDropDatabase(string dbName)
		{
			connection.OpenIfNeeded();
			const string sqlText = """
									USE [master]
									IF db_id(@dbName) IS NOT NULL
									BEGIN
										DECLARE @sql NVARCHAR(MAX) = 
											'ALTER DATABASE [' + @dbName + '] SET OFFLINE WITH ROLLBACK IMMEDIATE; DROP DATABASE [' + @dbName + ']';
										EXEC(@sql);
									END
									""";
			using var cmd = connection.CreateCommand();
			Console.WriteLine($"Force Dropping database {dbName}");
			cmd.CommandText = sqlText;
			cmd.Parameters.Add(new SqlParameter("@dbName", dbName));
			cmd.ExecuteNonQuery();
		}

		public void Deploy(string dacpacPath, string dbName, string connectionString)
		{
			Argument.Argument.NotNullOrEmpty(dacpacPath, nameof(dacpacPath));
			Argument.Argument.NotNullOrEmpty(dbName, nameof(dbName));
			Argument.Argument.NotNullOrEmpty(connectionString, nameof(connectionString));

			Console.WriteLine($"Deploying database {dbName}");
			var dacService = new DacServices(connectionString);
			var dacpack = DacPackage.Load(dacpacPath);
			dacService.Deploy(dacpack, dbName, true, new DacDeployOptions
			{
				ExcludeObjectTypes = [ObjectType.RoleMembership, ObjectType.Logins, ObjectType.DatabaseOptions],
				RegisterDataTierApplication = false,
				ScriptDatabaseOptions = false,
				AllowIncompatiblePlatform = true,
				ScriptDatabaseCompatibility = true,
				ScriptDatabaseCollation = false,
				CompareUsingTargetCollation = true
			});
		}

		public string GetExtendedProperty(string dbName, string propertyKey)
		{
			connection.OpenIfNeeded();
			string sqlText = FormattableString.Invariant($@"
SELECT value FROM {dbName}.sys.extended_properties WHERE class = 0 AND name = '{propertyKey}'");
			var objectValue = connection.ExecuteScalar(sqlText);
			var result = (objectValue == null || objectValue == DBNull.Value) ? null : objectValue.ToString();
			return result;
		}

		readonly IDbConnection connection;
	}
}
