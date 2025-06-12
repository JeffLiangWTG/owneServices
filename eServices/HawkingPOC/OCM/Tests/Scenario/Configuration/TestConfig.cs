using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace OcmPoc.Tests.Scenario.Configuration
{
	static class TestConfig
	{
		static readonly TestConfiguration config;

		static TestConfig()
		{
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Path.GetDirectoryName(typeof(TestConfig).Assembly.Location))
				.AddJsonFile("TestConfiguration.json")
				.Build();

			config = new TestConfiguration();

			configuration.Bind(config);
		}

		public static FileSystemConfig ProviderA => config.ProviderA;
		public static FileSystemConfig ProviderB => config.ProviderB;
		public static ApiConfig CW1 => config.CW1;

		public static string ToJson()
		{
			return JsonConvert.SerializeObject(config);
		}
	}
}
