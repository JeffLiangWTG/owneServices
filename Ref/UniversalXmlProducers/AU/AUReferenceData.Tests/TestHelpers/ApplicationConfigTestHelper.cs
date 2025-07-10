using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.RefDbRepo.AUReferenceData.Services;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.AUReferenceData.Tests
{
	public static class ApplicationConfigTestHelper
	{
		static readonly Type ConfigType = typeof(ApplicationConfig);
		const string ConfigFieldName = "config";
		const string JsonConfigFileName = "CargoWise.RefDbRepo.AUReferenceData.CmdLine.config.json";

		public static void SetApplicationConfigValues(Dictionary<string, string> configValues)
		{
			var field = ConfigType.GetField(ConfigFieldName, BindingFlags.Static | BindingFlags.NonPublic);
			if (field == null)
			{
				throw new InvalidOperationException("Configuration field not found.");
			}

			if (field.GetValue(null) is not IConfiguration existingConfig)
			{
				existingConfig = new ConfigurationBuilder()
					.AddJsonFile(JsonConfigFileName)
					.Build();
			}

			var overrideConfig = new ConfigurationBuilder()
				.AddConfiguration(existingConfig)
				.AddInMemoryCollection(configValues)
				.Build();

			field.SetValue(null, overrideConfig);
		}

		public static void ResetApplicationConfig()
		{
			var field = ConfigType?.GetField(ConfigFieldName, BindingFlags.Static | BindingFlags.NonPublic);
			field?.SetValue(null, null);
		}
	}
}
