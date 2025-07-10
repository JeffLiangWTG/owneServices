using System;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.ReferenceDataSqlProducers
{
	public sealed class ApplicationConfig
	{
		const string JsonFileName = "CargoWise.RefDbRepo.ReferenceDataSqlProducers.RefCusConditionTypeLanguage.config.json";
		public static ApplicationConfig Instance => instance.Value;

		static readonly Lazy<ApplicationConfig> instance = new Lazy<ApplicationConfig>(() => new ApplicationConfig());

		public ApplicationConfig()
		{
			var configuration = new ConfigurationBuilder()
				.AddJsonFile(JsonFileName)
				.Build();

			OutputFile = configuration[nameof(OutputFile)];
			ParserFileName = configuration[nameof(ParserFileName)];
		}

		public string OutputFile { get; private set; }
		public string ParserFileName { get; private set; }
	}
}
