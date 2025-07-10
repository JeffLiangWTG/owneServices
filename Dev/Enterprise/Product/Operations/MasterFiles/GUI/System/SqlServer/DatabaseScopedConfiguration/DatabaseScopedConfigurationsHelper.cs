using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	static class DatabaseScopedConfigurationsHelper
	{
		public static DatabaseScopedConfigurationCollection LoadConfigurations(DbConnection connection)
		{
			var configurations = new DatabaseScopedConfigurationCollection();
			LoadCore(connection, configurations, (id, name) =>
			{
				var configuration = new DatabaseScopedConfiguration(
					id,
					PresetDatabaseScopedConfiguration.PresetConfigurations.First(x => x.Name == name));
				configurations.Add(configuration);
				return configuration;
			});
			return configurations;
		}

		// Note, value returned by sys.database_scoped_configurations may have different type from value used in 'ALTER DATABASE SCOPED CONFIGURATION SET'.
		// e.g. PARAMETER_SNIFFING. Its value from sys.database_scoped_configurations is 1, but we can't configure PARAMETER_SNIFFING with 1.
		// Instead, we should use ON.
		public static void SaveProposedValues(AdminConnection connection, DatabaseScopedConfigurationCollection configurations)
		{
			if (!configurations.CanBeSavedPotentially)
			{
				throw new InvalidOperationException("No available database configuration can be saved");
			}

			var exceptions = new List<Exception>();

			foreach (var configuration in configurations)
			{
				if (!configuration.CanBeSavedPotentially)
				{
					continue;
				}

				if (int.TryParse(configuration.ProposedValue, out _))
				{
					WriteConfigurationHandlingSqlExceptionIfAnotherValueIsGiven(configuration, configuration.ProposedValue, null);
					continue;
				}

				if (configuration.ProposedValue.StartsWith("'") && configuration.ProposedValue.EndsWith("'"))
				{
					WriteConfigurationHandlingSqlExceptionIfAnotherValueIsGiven(configuration, configuration.ProposedValue, null);
					continue;
				}

				// No special character is found, we will try the value directly, or try it surrounded with '.
				if (((string)configuration.ProposedValue).All(x => char.IsLetter(x) || char.IsDigit(x) || x == '_'))
				{
					WriteConfigurationHandlingSqlExceptionIfAnotherValueIsGiven(configuration, configuration.ProposedValue, () => $"'{configuration.ProposedValue}'");
					continue;
				}

				// Once value contains special char such as semicolon, space, etc. it is seen as string content to avoid sql injection
				WriteConfigurationHandlingSqlExceptionIfAnotherValueIsGiven(configuration, $"'{configuration.ProposedValue}'", null);
			}

			if (exceptions.Count != 0)
			{
				throw new AggregateException($"Exception occurred in saving configuration of database [{connection.CurrentDatabase}]", exceptions); // to make caller aware of exception
			}

			void WriteConfigurationHandlingSqlExceptionIfAnotherValueIsGiven(DatabaseScopedConfiguration cfg, string cfgValue, Func<string> retryAnotherCfgValueFunc)
			{
				try
				{
					connection.ExecuteNonQuery($@"
ALTER DATABASE SCOPED CONFIGURATION
SET {cfg.Name} = {cfgValue}
");
					cfg.HasChanges = false;
				}
				catch (SqlException sqlException) when (!sqlException.IsCriticalException())
				{
					if (retryAnotherCfgValueFunc == null)
					{
						var error = Res.GetString(
							"9323B11D-7D98-4223-9DA1-5ACD66A11F6F",
							"Writing value [{0}] failed due to [{1}]. SQL error number = {2}.",
							cfg.ProposedValue,
							sqlException.Message,
							sqlException.Number);
						cfg.AddRowError(error);
						exceptions.Add(new InvalidOperationException(error, sqlException));
					}
					else
					{
						WriteConfigurationHandlingSqlExceptionIfAnotherValueIsGiven(cfg, retryAnotherCfgValueFunc(), null);
					}
				}
			}
		}

		public static void Reload(DbConnection connection, DatabaseScopedConfigurationCollection configurations)
		{
			LoadCore(connection, configurations, (id, name) => configurations.Find(id));
		}

		static void LoadCore(
			DbConnection connection,
			DatabaseScopedConfigurationCollection configurations,
			Func<int, string, DatabaseScopedConfiguration> configurationLocator)
		{
			var isDbReadOnly = !connection.IsDbWriteable(connection.CurrentDatabase);

			using (configurations.SuspendListChanged())
			{
				connection.ExecuteReader(
					(NoResString)@"
SELECT
	configuration_id,
	name,
	value,
	value_for_secondary,
	is_value_default
FROM sys.database_scoped_configurations
",
					(IDataRecord record) =>
					{
						var id = (int)record["configuration_id"];
						var name = (string)record["name"];

						var configuration = configurationLocator(id, name);
						configuration.IsDbReadOnly = isDbReadOnly;
						configuration.RefreshWithLatestDataFromDb(
							currentValueFromDatabase: GetSqlVariant(record["value"]),
							currentValueForSecondaryFromDatabase: GetSqlVariant(record["value_for_secondary"]),
							isValueDefault: (bool)record["is_value_default"]);

						object GetSqlVariant(object value)
						{
							return value == DBNull.Value ? null : value;
						}
					});
			}
		}
	}
}
