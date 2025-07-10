using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.Common
{
	public static class CommonApplicationConfig
	{
		public static string SecurityProtocols => Config[nameof(SecurityProtocols)];
		public static string SeleniumServerURL => Config[nameof(SeleniumServerURL)];
		public static string SeleniumTimeOutInSeconds => Config[nameof(SeleniumTimeOutInSeconds)];

		public static void AddJsonFile(string path)
		{
			jsonConfigFiles.Add(path);
			_config = null;
		}

		static List<string> jsonConfigFiles = new List<string> { "CargoWise.RefDbRepo.UniversalXMLProducers.Common.config.json" };

		static IConfiguration Config
		{
			get
			{
				if (_config == null)
				{
					var builder = new ConfigurationBuilder();
					foreach(var jsonConfigFile in jsonConfigFiles)
					{
						builder.AddJsonFile(jsonConfigFile);
					}
					_config= builder.Build();
				}
				return _config;
			}
		}
		static IConfiguration _config;
	}
}
