using System;
using Microsoft.Data.SqlClient;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Tools.Common
{
	public static class DBHelper
	{
		public static void DropAndCreateNewDatabase(string connectionString, string dbName, Func<string> GetRestoreScript)
		{
			connectionString = connectionString.Replace(dbName, "master");
			using (var conn = new SqlConnection(connectionString))
			{
				conn.Open();
				var dbCreator = new DbCreator(conn);
				dbCreator.DropDatabase(dbName);
				dbCreator.CreateDatabase(dbName, string.Empty);
				if (GetRestoreScript != null)
				{
					dbCreator.ExcuteDbScript("master", GetRestoreScript());
				}
				dbCreator.ExcuteDbScript(dbName, "ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = ON");
				dbCreator.ExcuteDbScript(dbName, @"
if not exists(select * from sys.database_principals where name = 'refdbreporeader')
BEGIN
CREATE USER [refdbreporeader] WITHOUT LOGIN
END");
				dbCreator.ExcuteDbScript(dbName, @"
if not exists(select * from sys.database_principals where name = 'refdbrepowriter')
BEGIN
	CREATE USER [refdbrepowriter] WITHOUT LOGIN
END");
			}
		}

		public static void DropAndCreateNewDatabase(string connectionString)
		{
			var dbName = GetDbName(connectionString);
			DropAndCreateNewDatabase(connectionString, dbName, null);
		}

		public static void DropDatabase(string connectionString, string dbName)
		{
			using (var conn = new SqlConnection(connectionString))
			{
				conn.Open();
				var dbCreator = new DbCreator(conn);
				dbCreator.DropDatabase(dbName);
			}
		}

		public static string GetDbName(string connectionString)
		{
			var splitConn = connectionString.Split(';');
			var catalog = splitConn[1];
			var splitCatalog = catalog.Split('=');
			return splitCatalog[1];
		}
	}
}
