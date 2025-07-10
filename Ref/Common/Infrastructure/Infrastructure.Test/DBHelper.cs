using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.Common.DbUpgrade;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Data.SqlClient;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.Common.Infrastructure.Test
{
	public static class DBHelper
	{
		static void CleanOldDatabases(SqlConnection connection, List<string> databases)
		{
			connection.ChangeDatabase("master");
			foreach (var olddb in databases)
			{
				new DbCreator(connection).DropDatabase(olddb);
			}
		}

		public static void CleanOldDatabases(SqlConnection connection, string dbNamePrefix, string dbName)
		{
			var oldDatabases = new List<string>();
			using (var cmd = new SqlCommand($@"SELECT name FROM sys.databases
WHERE (name LIKE '@dbNamePrefix\_%' ESCAPE '\'
AND create_date < dateadd(minute, -15, getdate()))
OR name = '@dbName'", connection))
			{
				cmd.Parameters.AddWithValue("@dbNamePrefix", dbNamePrefix);
				cmd.Parameters.AddWithValue("@dbName", dbName);
				using (var reader = cmd.ExecuteReader())
				{
					while (reader.Read())
					{
						oldDatabases.Add((string)reader["name"]);
					}
				}
			}
			CleanOldDatabases(connection, oldDatabases);
		}

		public static void TearDown(SqlConnection connection, string tmpPath, string dbName)
		{
			connection.OpenIfNeeded();
			var dbCreator = new DbCreator(connection);
			try
			{
				dbCreator.DropDatabase(dbName);
				
			} catch (SqlException)
			{
				dbCreator.ForceDropDatabase(dbName);
			}
			if (Directory.Exists(tmpPath))
			{
				Directory.Delete(tmpPath, true);
			}
		}

		public static void Setup(SqlConnection connection, string dbName, string dbNamePrefix)
		{
			connection.OpenIfNeeded();
			var dbCreator = new DbCreator(connection);
			CleanOldDatabases(connection, dbNamePrefix, dbName);
			dbCreator.DropDatabase(dbName);
		}

		public static void CreateDatabase(SqlConnection connection, string dbName, string tmpPath, string collation = null)
		{
			if (!string.IsNullOrEmpty(tmpPath))
			{
				Directory.CreateDirectory(tmpPath);
			}
			var dbCreator = new DbCreator(connection);
			dbCreator.CreateDatabase(dbName, tmpPath, collation);
		}

		public static bool CheckIfDbExist(SqlConnection connection, string dbName)
		{
			connection.OpenIfNeeded();
			using var cmd = connection.CreateCommand();
			cmd.CommandText = "SELECT CASE WHEN EXISTS(SELECT 1 FROM sys.databases WHERE name = @dbName) THEN 1 ELSE 0 END";
			cmd.Parameters.Add(new SqlParameter("@dbName", dbName));
			return (int) cmd.ExecuteScalar()! == 1;
		}

		public static string GetExtendedProperty(SqlConnection connection, string dbName, string propertyKey)
		{
			var dbCreator = new DbCreator(connection);
			return dbCreator.GetExtendedProperty(dbName, propertyKey);
		}

		public static void CreateLogin(SqlConnection connection, string dbName, string userId, string password, string[] memberships)
		{
			var sqlText = new StringBuilder($@"
IF EXISTS (SELECT * FROM sys.syslogins WHERE name = N'{userId}')
	DROP LOGIN {userId}
GO
CREATE LOGIN [{userId}] WITH PASSWORD = '{password}'
GO
IF EXISTS (SELECT * FROM sys.database_principals WHERE name = N'{userId}')
	DROP USER {userId}
GO
CREATE USER {userId} FOR LOGIN {userId}
GO
");
			Array.ForEach(memberships, x => sqlText.Append(CultureInfo.InvariantCulture, $@"
EXEC sp_addrolemember '{x}', '{userId}'
GO
"));
			new DbCreator(connection).ExcuteDbScript(dbName, sqlText.ToString());
		}

		public static void CreateSynonym(SqlConnection connection, string safeDbName, string stagingDbName)
		{
			var sql = $@"
IF EXISTS (SELECT name FROM sys.synonyms WHERE name = 'RefStagingDatabase_DataProcessingResult')
BEGIN
	DROP SYNONYM [dbo].[RefStagingDatabase_DataProcessingResult];
END
CREATE SYNONYM [dbo].[RefStagingDatabase_DataProcessingResult] FOR [{stagingDbName}].[dbo].[DataProcessingResult];
";
			new DbCreator(connection).ExcuteDbScript(safeDbName, sql, retryConnection: true);
		}

		public static void CreateSynonym(SqlConnection connection, string dbName)
		{
			const string sql = @"
IF EXISTS (SELECT name FROM sys.synonyms WHERE name = 'RefStagingDatabase_DataProcessingResult')
BEGIN
	DROP SYNONYM [dbo].[RefStagingDatabase_DataProcessingResult];
END
IF EXISTS (
	SELECT * FROM INFORMATION_SCHEMA.TABLES 
	WHERE TABLE_NAME = 'DataProcessingResult_Test' AND TABLE_SCHEMA = 'dbo'
)
BEGIN
	DROP TABLE [dbo].[DataProcessingResult_Test];
END
BEGIN
	CREATE TABLE [dbo].[DataProcessingResult_Test] (DPR_ParentPK UNIQUEIDENTIFIER NOT NULL)
	CREATE SYNONYM [dbo].[RefStagingDatabase_DataProcessingResult] FOR [dbo].[DataProcessingResult_Test];
END
";
			new DbCreator(connection).ExcuteDbScript(dbName, sql, retryConnection: true);
		}

		public static void Deploy(SqlConnection connection, string dacPacFilePath, string dbName, string connectionString)
		{
			connection.OpenIfNeeded();
			var dbCreator = new DbCreator(connection);
			dbCreator.Deploy(dacPacFilePath, dbName, connectionString);
		}

		public static void SetTestDbExtendedProperties(SqlConnection connection, string dacPacFileHash, string dbName)
		{
			connection.OpenIfNeeded();
			connection.ChangeDatabaseIfNeeded(dbName);
			var sqlText = FormattableString.Invariant($@"
EXEC sys.sp_addextendedproperty @name = '{TestDbExtendedProperties.DacPacFileModelHash}', @value = @value
");
			using var cmd = connection.CreateCommand();
			cmd.CommandText = sqlText;
			cmd.Parameters.Add(new SqlParameter("@value", dacPacFileHash));
			cmd.ExecuteNonQuery();
		}

		public static void ExecuteNonQuery(SqlConnection conn, string script, SqlTransaction trans = null)
		{
			using var command = conn.CreateCommand();
			command.Transaction = trans;
			command.CommandText = script;
			command.ExecuteNonQuery();
		}

		public static void AssertThrowsSqlException(SqlConnection conn, string script, string expectedExceptionMessage)
		{
			Exception exception = null;
			try
			{
				using var command = conn.CreateCommand();
				command.CommandText = script;
				command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				exception = ex;
			}
			Assert.That(exception, Is.Not.Null);
			Assert.That(exception.GetBaseException().Message, Is.EqualTo(expectedExceptionMessage));
		}

		public static void AssertDoesNotThrowsSqlException(SqlConnection conn, string script)
		{
			Exception exception = null;
			try
			{
				ExecuteNonQuery(conn, script);
			}
			catch (Exception ex)
			{
				exception = ex;
			}

			Assert.That(exception, Is.Null);
		}
	}
}
