using System;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;

namespace Enterprise.MasterFiles.GUI.Test
{
	static class DatabaseScopedConfigurationExtensions
	{
		public static DatabaseScopedConfiguration FindByName(this DatabaseScopedConfigurationCollection collection, string name)
		{
			return collection.Find(x => x.Name == name).Single();
		}

		public static void MakeChangeToProposedValueAsUIDoes(this DatabaseScopedConfiguration config, string value)
		{
			config.HasChanges = true;     // UI framework does it first to make sure event fired below can make button enabled
			config.ProposedValue = value; // call RefreshBinding, which will fire ValueChanged event
		}

		public static IDisposable CreateConfigurationProtectionScope<T>(string name)
		{
			var connection = Db.NewAdminConnection(Db.DatabaseName);
			var originalValue = ReadConfiguration<T>(connection, name);
			return new DisposableAction(() =>
			{
				WriteConfiguration(connection, name, originalValue.ToString());
				connection.Dispose();
			});
		}

		public static TDbValue ReadConfiguration<TDbValue>(DbConnection connection, string cfgName)
		{
			return connection.ExecuteScalar<TDbValue>(
				@"
SELECT
	value
	FROM sys.database_scoped_configurations
WHERE name = @name
",
				cmd =>
				{
					cmd.AddParameter("@name", SqlDbType.NVarChar, 60, cfgName);
				});
		}

		public static object ReadConfiguration(DbConnection connection, string cfgName)
		{
			return connection.ExecuteScalar(
				@"
SELECT
	value
	FROM sys.database_scoped_configurations
WHERE name = @name
",
				cmd =>
				{
					cmd.AddParameter("@name", SqlDbType.NVarChar, 60, cfgName);
				});
		}

		public static void WriteConfiguration(DbConnection connection, string cfgName, string newCfgValue)
		{
			connection.ExecuteNonQuery($@"
ALTER DATABASE SCOPED CONFIGURATION
SET {cfgName} = {newCfgValue}
");
		}
	}
}
