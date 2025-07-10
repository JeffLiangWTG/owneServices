using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public static class ConfigurationProvider
	{
		public static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
					var configurationBuilder = new ConfigurationBuilder();
					configurationBuilder.AddJsonFile(Path.Combine(currentDirectory, JsonConfigFile));
					configuration = configurationBuilder.Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		public static string OIDCAuthority => Configuration[nameof(OIDCAuthority)];
		public static string OIDCAudience => Configuration[nameof(OIDCAudience)];
		public static string S2SAuthority => Configuration[nameof(S2SAuthority)];
		public static string S2SAudience => Configuration[nameof(S2SAudience)];
		public static bool DisableAuthentication
		{
			get
			{
#if DEBUG
				if (bool.TryParse(Configuration["DEBUG_DisableAuthentication"], out var disableAuthentication))
				{
					return disableAuthentication;
				}
#endif
				return false;
			}
		}
		public static string[] TablesNotUseBulkInsert
		{
			get
			{
				if (tablesNotUseBulkInsert == null)
				{
					var tableList = new List<string>();
					var dataSetsNotUseBulkInsert = Configuration["DataSetsNotUseBulkInsert"];
					var dataSets = string.IsNullOrEmpty(dataSetsNotUseBulkInsert) ? new string[0] : dataSetsNotUseBulkInsert.Split(',');
					foreach (var dataSet in dataSets)
					{
						var tables = DataSetStructureProvider.StructuredDataSets.FirstOrDefault(x => x[0] == dataSet);
						if (tables != null)
						{
							tableList.AddRange(tables);
						}
					}
					tablesNotUseBulkInsert = tableList.ToArray();
				}
				return tablesNotUseBulkInsert;
			}
		}
		static string[] tablesNotUseBulkInsert;

		public static string SafeConnectionString => DbConnectionStringManager.SafeConnectionString;

		public static IConfigurationSection MemoryUsageLogging => Configuration.GetSection("MemoryUsageLogging");

		const string JsonConfigFile = "CargoWise.RefDbRepo.NewSafeDataUpdateService.config.json";
	}
}
