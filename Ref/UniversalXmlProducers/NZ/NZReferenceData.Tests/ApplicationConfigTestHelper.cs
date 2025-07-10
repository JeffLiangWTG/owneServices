using System.Collections.Generic;
using System;
using System.Reflection;
using CargoWise.RefDbRepo.NZReferenceData.Services;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.NZReferenceData.Tests
{
	public static class ApplicationConfigTestHelper
	{
		static readonly Type ConfigType = typeof(ApplicationConfig);
		const string ConfigFieldName = "configuration";
		const string JsonConfigFileName = "CargoWise.RefDbRepo.NZReferenceData.CmdLine.config.json";

		public static void SetApplicationConfigValue(string key, string value)
		{
			var field = ConfigType.GetField(ConfigFieldName, BindingFlags.Static | BindingFlags.NonPublic);
			if (field == null)
			{
				throw new InvalidOperationException("configuration field not found.");
			}

			if (field.GetValue(null) is not IConfiguration existingConfig)
			{
				existingConfig = new ConfigurationBuilder()
					.AddJsonFile(JsonConfigFileName)
					.Build();
			}

			var overrideConfig = new ConfigurationBuilder()
				.AddConfiguration(existingConfig)
				.AddInMemoryCollection([new KeyValuePair<string, string>(key, value)])
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
