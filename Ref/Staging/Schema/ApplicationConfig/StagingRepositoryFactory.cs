using System.IO;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.Schema_New;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.Staging.ApplicationConfig
{
	public static class StagingRepositoryFactory
	{
		public static IStagingRepository GetStagingRepository()
		{
			var connectionString = DbConnectionStringManager.StagingConnectionString;
			if (string.IsNullOrEmpty(connectionString))
			{
				var configFile = Assembly.GetExecutingAssembly().GetName().Name + ".config.json";
				if (!File.Exists(configFile))
				{
					throw new FileNotFoundException($"Cannot find config file. Path: {configFile}");
				}
				var config = new ConfigurationBuilder().AddJsonFile(configFile).Build();
				connectionString = config.GetConnectionString("RefDbRepoStagingEntities");
			}
			return new StagingRepository(connectionString);
		}
	}
}
