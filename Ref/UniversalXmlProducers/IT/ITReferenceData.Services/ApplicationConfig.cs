using System;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.ITReferenceData.Services
{
	public static class ApplicationConfig
	{
		static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configuration = configurationBuilder.AddJsonFile(JsonFileName).Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		public static string OutputDirectory => Configuration["OutputFilePath"];
		public static string NationalSupportingDocumentsUrl => Configuration["NationalSupportingDocumentsUrl"];
		public static string TaricServletUrl => Configuration["TaricServletUrl"];
		public static string MisureServletUrl => Configuration["MisureServletUrl"];
		public static string EuropeanSupportingDocumentsUrl => Configuration["EuropeanSupportingDocumentsUrl"];
		public static string AdditionalCodesUrl => Configuration["AdditionalCodesUrl"];
		public static string FetchUrl => Configuration["FetchURL"];
		public static string DailyRateUrl => Configuration["DailyRateURL"];
		public static string RefDataRepoUrl => Configuration["RefDataRepoUrl"];
		public static Uri RefDataRepoUri => new(Configuration["RefDataRepoUrl"]);
		public static Uri TaricServletUri => new(Configuration["TaricServletUrl"]);
		public static Uri MisureServletUri => new(Configuration["MisureServletUrl"]);

		const string JsonFileName = "CargoWise.RefDbRepo.ITReferenceData.CmdLine.config.json";
	}
}
