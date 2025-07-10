using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.NZReferenceData.Services
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
					configuration = configurationBuilder.AddJsonFile(JsonConfigFileName)
						.Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		const string JsonConfigFileName = "CargoWise.RefDbRepo.NZReferenceData.CmdLine.config.json";

		public static int ActiveDateMonthOffset =>
			int.TryParse(Configuration["ActiveDateMonthOffset"], out var months) ? months : 60;

		public static string OutputDirectory => Configuration["OutputPath"];

		public static string VesselListUrl => Configuration["VesselListURL"];

		public static string SupplierListUrl => Configuration["SupplierListUrl"];

		public static string SupplierPageUrl => Configuration["SupplierPageUrl"];

		public static string TariffDataListUrl => Configuration["TariffDataListUrl"];

		public static string ConcessionDataListUrl => Configuration["ConcessionDataListUrl"];

		public static string SeleniumFtpAddress => Configuration["SeleniumFtpAddress"];

		public static string SeleniumFtpDownloadsNZAddress => Configuration["SeleniumFtpDownloadsNZAddress"];

		public static string ConcessionOverrideFileName => Configuration[nameof(ConcessionOverrideFileName)];

		public static string ConsolidatedListOfApprovalsFileName => Configuration["ConsolidatedListOfApprovalsFileName"];

		public static string EmailSender => Configuration[nameof(EmailSender)];
		public static string EmailSmtpServer => Configuration[nameof(EmailSmtpServer)];
		public static string EmailUsername => Configuration[nameof(EmailUsername)];
		public static string EmailCredentialsPassword => Configuration[nameof(EmailCredentialsPassword)];
		public static string EmailSmtpPort => Configuration[nameof(EmailSmtpPort)];
		public static string EmailGroups => Configuration[nameof(EmailGroups)];

		public static string TariffOverrideFileName => Configuration[nameof(TariffOverrideFileName)];
	}
}
