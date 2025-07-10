using System;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.TRReferenceData.Services
{
	public static class ApplicationConfig
	{
		public static string OutputPath => Configuration[nameof(OutputPath)];

		public static string ExchangeRatesURL => Configuration[nameof(ExchangeRatesURL)];

		public static string ResPath => Configuration[nameof(ResPath)];

		public static string RefDbServiceURI => Configuration[nameof(RefDbServiceURI)];

		public static string IsRefDbServiceSecure => Configuration[nameof(IsRefDbServiceSecure)];

		static readonly Lazy<IConfiguration> ConfigurationLazy = new Lazy<IConfiguration>(() => new ConfigurationBuilder().AddJsonFile(JsonConfigFile).Build());
		public static IConfiguration Configuration => ConfigurationLazy.Value;
		const string JsonConfigFile = "CargoWise.RefDbRepo.TRReferenceData.CmdLine.config.json";
	}
}

