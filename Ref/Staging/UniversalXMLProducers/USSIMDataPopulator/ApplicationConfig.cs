using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.USSIMDataPopulator
{
	public sealed class ApplicationConfig
	{
		const string JsonFileName = "CargoWise.RefDbRepo.UniversalXMLProducers.USSIMDataPopulator.config.json";

		public static ApplicationConfig Instance => instance.Value;

		static readonly Lazy<ApplicationConfig> instance = new Lazy<ApplicationConfig>(() => new ApplicationConfig());

		ApplicationConfig()
		{
			var configuration = new ConfigurationBuilder()
				.AddJsonFile(JsonFileName)
				.Build();

			FullDataUrl = configuration[nameof(FullDataUrl)];
			MandatoryCodesListUrl = configuration[nameof(MandatoryCodesListUrl)];

			ExportXMLFilename = configuration[nameof(ExportXMLFilename)];
			FullDataXLSFilepath = configuration[nameof(FullDataXLSFilepath)];
			MandatoryCodesListPDFFilepath = configuration[nameof(MandatoryCodesListPDFFilepath)];
			FullDataZipFilepath = configuration[nameof(FullDataZipFilepath)];

			ExportFileDir = configuration[nameof(ExportFileDir)];
			ExportXMLFilepath = Path.Combine(ExportFileDir, ExportXMLFilename);

			DefaultPublishedDate = new DateTime(2017, 1, 1);
			DefaultEndDate = new DateTime(2079, 6, 6, 23, 59, 00);
		}

		public string FullDataUrl { get; private set; }
		public string MandatoryCodesListUrl { get; private set; }

		public string ExportXMLFilename { get; private set; }

		public string ExportFileDir { get; private set; }
		public string ExportXMLFilepath { get; private set; }
		public string FullDataXLSFilepath { get; private set; }
		public string MandatoryCodesListPDFFilepath { get; private set; }
		public string FullDataZipFilepath { get; private set; }

		public static DateTime DefaultPublishedDate { get; private set; }
		public static DateTime DefaultEndDate { get; private set; }
	}
}
