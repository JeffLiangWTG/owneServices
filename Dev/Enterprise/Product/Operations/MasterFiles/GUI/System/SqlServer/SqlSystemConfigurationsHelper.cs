using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using static System.FormattableString;

namespace Enterprise.MasterFiles.GUI
{
	public class SqlSystemConfigurationsHelper
	{
		[ThreadStatic]
		static SqlSystemConfigurationsHelper instance;

		internal static SqlSystemConfigurationsHelper Instance
		{
			get
			{
				return instance ?? (instance = new SqlSystemConfigurationsHelper(
					new Dictionary<int, string>()
					{
						{ RecoveryIntervalConfigId, "1" },
						{ AllowUpdatesConfigId, "0" },
						{ ClrEnabledConfigId, "1" },
						{ TextSizeConfigId, "-1" },

						// The ability to return result sets from triggers will be removed in a future version of SQL Server.
						// Avoid returning result sets from triggers in new development work, and plan to modify applications that currently do this.
						// To prevent triggers from returning result sets, change the disallow results from triggers option to a value of 1.
						// The default setting for the disallow results from triggers option will be set to 1 in a future version of SQL Server.
						// See https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/disallow-results-from-triggers-server-configuration-option?view=sql-server-ver16
						{ DisallowResultsFromTriggersId, "1" },
					},
					new int[]
					{
						RecoveryIntervalConfigId,
						AllowUpdatesConfigId,
						ClrEnabledConfigId
					}));
			}
		}

		internal SqlSystemConfigurationsHelper(Dictionary<int, string> recommendedValues, int[] blockedConfigurationIds)
		{
			RecommendedValues = recommendedValues;
			BlockedConfigurationIds = blockedConfigurationIds;
			EnforcedConfigurationIds = new int[]
			{
				RecoveryIntervalConfigId,
				AllowUpdatesConfigId,
				ClrEnabledConfigId
			};
		}

		internal static bool DenyUserEdit => EnvProxy.IsHostedWithCargowise || !GlbStaff.CurrentUser.GS_IsController;

		internal SqlSystemConfigurationsCollection LoadSqlServerConfigurations(DbConnection connection, string whereClause = "")
		{
			var collection = new SqlSystemConfigurationsCollection();

			connection.ExecuteReader(
				Invariant($@"
select
    configuration_id
    , name
    , is_dynamic
    , minimum
    , maximum
    , value
    , value_in_use
    --, proposed_value = ''
    , is_advanced
    , description
from
    sys.configurations {whereClause}
"),
				cmd => { },
				record =>
				{
					var configuration = new SqlSystemConfiguration((int)record["configuration_id"]);
					collection.Add(configuration);

					configuration.Name = (string)record["name"];
					configuration.ItemDescription = (string)record["description"];
					configuration.ConfiguredValue = (int)record["value"];
					configuration.ValueInUse = (int)record["value_in_use"];
					configuration.MinValue = (int)record["minimum"];
					configuration.MaxValue = (int)record["maximum"];
					configuration.IsDynamic = (bool)record["is_dynamic"];
					configuration.IsAdvanced = (bool)record["is_advanced"];

					configuration.ProposedValue = configuration.ConfiguredValue;
					configuration.HasRecommendedValue = false;
				}
			);

			return collection;
		}

		internal void ApplyProposedValues(AdminConnection adminConnection, IEnumerable<SqlSystemConfiguration> proposedConfigurations)
		{
			foreach (var config in proposedConfigurations)
			{
				try
				{
					DataUtils.SetServerConfigOption(adminConnection, config.Name, Invariant($"{config.ProposedValue}"), config.IsAdvanced);
				}
				catch (Exception ex)
				{
					throw new ApplicationException(config.ToString(), ex);
				}

				config.ConfiguredValue = config.ProposedValue;
			}
		}

		internal void LoadPersistedValuesFromRegistry(IEnumerable<SqlSystemConfiguration> configurations)
		{
			var systemConfigurations = configurations.ToArray();
			var persistedValues = SystemDataRegistry.Instance.SqlSystemConfigurations.Value;

			if (persistedValues != null)
			{
				foreach (var persistedConfig in persistedValues.Cast<PersistedSqlConfiguration>())
				{
					var config = systemConfigurations.FirstOrDefault(x => x.ConfigurationId == persistedConfig.ConfigurationId);
					if (config != null && !EnforcedConfigurationIds.Contains(config.ConfigurationId))
					{
						config.SetPersistedConfig(persistedConfig);
					}
				}
			}
		}

		internal void SaveConfigurationsToRegistry(IEnumerable<SqlSystemConfiguration> configurations)
		{
			var valueSaveToRegistry = new PersistedSqlConfigurationsCollection();
			foreach (var item in configurations)
			{
				valueSaveToRegistry.Add(item.PersistedConfig);
			}

			SystemDataRegistry.Instance.SqlSystemConfigurations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, valueSaveToRegistry);
		}

		internal void ComputeOptimalValues(IEnumerable<SqlSystemConfiguration> configurations)
		{
			using (var connection = Db.NewAdminConnection())
			{
				foreach (var configuration in configurations)
				{
					if (RecommendedValues.TryGetValue(configuration.ConfigurationId, out var value))
					{
						configuration.RecommendedValue = value;

						if (EnforcedConfigurationIds.Contains(configuration.ConfigurationId))
						{
							configuration.ProposedValueText = configuration.RecommendedValue;
						}
					}
				}
			}
		}

		// a list of configuration ids that must use the recommended values as computed by the scripts <see cref="OverridableScripts" />
		internal Dictionary<int, string> RecommendedValues { get; }

		internal int[] EnforcedConfigurationIds { get; }

		// a list of configuration ids that restricted users from editing their values
		internal int[] BlockedConfigurationIds { get; }

		internal const int RecoveryIntervalConfigId = 101;
		internal const int AllowUpdatesConfigId = 102;
		internal const int ClrEnabledConfigId = 1562;
		internal const int TextSizeConfigId = 1536;
		const int DisallowResultsFromTriggersId = 114;
	}
}
