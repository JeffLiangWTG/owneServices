using System;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.CAReferenceData.Services
{
	public sealed class ApplicationConfig
	{
		public static ApplicationConfig Instance => instance.Value;

		static readonly Lazy<ApplicationConfig> instance = new Lazy<ApplicationConfig>(() => new ApplicationConfig());

		const string JsonFileName = "CargoWise.RefDbRepo.CAReferenceData.CmdLine.config.json";

		public ApplicationConfig()
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile(JsonFileName)
				.Build();

			DownloadFileRetryTimes = ParseDownloadFileRetryTimes(config[nameof(DownloadFileRetryTimes)]);
		}

		public int DownloadFileRetryTimes { get; private set; }

		private static int ParseDownloadFileRetryTimes(string times)
		{
			int retryTimes = int.TryParse(times, out retryTimes)
				? retryTimes : 1;
			return retryTimes;
		}
	}
}
