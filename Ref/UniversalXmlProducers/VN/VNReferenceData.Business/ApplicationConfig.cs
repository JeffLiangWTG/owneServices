using System.Globalization;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.VNReferenceData.Business
{
	public static class ApplicationConfig
	{
		public static string XmlOutputFolder => Config["XmlOutputFolder"];
		public static string CodeListDownloadUrlPrefix => Config["CodeListDownloadUrlPrefix"] ?? "https://files.customs.gov.vn";
		public static string CodeListMetadataUrl => Config["CodeListMetadataUrl"];
		public static string CombinationTabCategoryId => Config["CombinationTabCategoryId"] ?? "23";

		#region CUSOF

		public static string CustomsOfficeOutputFileName =>
			Config["CustomsOfficeOutputFileName"] ?? "RefCusCodeList_VN_CUSOF.xml";

		public static int CustomsOfficeMetadataId => !string.IsNullOrWhiteSpace(Config["CustomsOfficeMetadataId"])
			? int.Parse(Config["CustomsOfficeMetadataId"], CultureInfo.InvariantCulture)
			: 40;

		#endregion


		const string DefaultJsonConfigFileName = "CargoWise.RefDbRepo.VNReferenceData.CmdLine.config.json";
		static IConfiguration config;

		static IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					config = configurationBuilder.AddJsonFile(DefaultJsonConfigFileName).Build();
				}

				return config;
			}
		}
	}
}
