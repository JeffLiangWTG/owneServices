using System;
using System.Diagnostics;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.MasterFiles.GUI
{
	[DebuggerDisplay("{Code}")]
	sealed class DatabaseScopedConfigurationContainer : NonPersistentBusinessObject, ICodeDescription
	{
		public AdminConnection Connection { get; }
		readonly string dbName;

		public DatabaseScopedConfigurationCollection Configurations => lazyConfigurations.Value;
		readonly Lazy<DatabaseScopedConfigurationCollection> lazyConfigurations;
		public DatabaseScopedConfigurationContainer(AdminConnection connection, string dbName)
		{
			this.Connection = connection;
			this.dbName = dbName;

			lazyConfigurations = new Lazy<DatabaseScopedConfigurationCollection>(() =>
			{
				using (((ICurrentDbControl)connection).UseDatabase(dbName))
				{
					var configs = DatabaseScopedConfigurationsHelper.LoadConfigurations(connection);
					RegisterEditableChildObject(configs);
					return configs;
				}
			});
		}

		public string Code => dbName;
		public string Description => dbName;

		public bool CanBeSavedPotentially => lazyConfigurations.IsValueCreated && lazyConfigurations.Value.CanBeSavedPotentially;

		public void Save()
		{
			using (((ICurrentDbControl)Connection).UseDatabase(dbName))
			{
				try
				{
					DatabaseScopedConfigurationsHelper.SaveProposedValues(Connection, lazyConfigurations.Value);
					DatabaseScopedConfigurationsHelper.Reload(Connection, lazyConfigurations.Value);
				}
				catch (AggregateException)
				{
					// Some updates can succeed, some can not.
					// Then even after exception occurs, we need to refresh values so that those successful updates can be seen by user in UI.
					DatabaseScopedConfigurationsHelper.Reload(Connection, lazyConfigurations.Value);
					throw;
				}
			}
		}
	}
}
