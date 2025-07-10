using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.GBReferenceData.Services
{
	public static class ConfigurationProvider
	{
		public static string JsonConfigFile
		{
			get
			{
				return jsonConfigFile ?? (jsonConfigFile = "CargoWise.RefDbRepo.GBReferenceData.CmdLine.config.json");
			}
			set
			{
				jsonConfigFile = value;
			}
		}
		static string jsonConfigFile;

		public static IConfiguration Configuration
		{
			get
			{
				if (configuration == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					configuration = configurationBuilder.AddJsonFile(JsonConfigFile).Build();
				}
				return configuration;
			}
		}
		static IConfiguration configuration;

		public static string SupportingDocumentCodeNationalUrl => Configuration["CdsStandingDataSupportingDocumentCodeNationalUrl"];
		public static string SupportingDocumentCodeUnionUrl => Configuration["CdsStandingDataSupportingDocumentCodeUnionUrl"];
		public static string SupportingDocumentCodeStatusUrl => Configuration["CdsStandingDataSupportingDocumentCodeStatusUrl"];
		public static string AdditionalInformationUrl => Configuration["CdsStandingDataAdditionalInformationUrl"];
		public static string ChiefHarmonisedDeclarationCodeUrl => Configuration["ChiefHarmonisedDeclarationCodeUrl"];
		public static string OutputDirectory => Configuration["OutputPath"];
		public static string UKOfficeCodesLandingPageUrl => Configuration["UKOfficeCodesLandingPageUrl"];
		public static string UKOfficeCodesDataAnchorRegex => Configuration["UKOfficeCodesDataAnchorRegex"];
		public static string TariffAuthorisationUrl => Configuration["TariffAuthorisationUrl"];
		public static string TariffDailyUrl => Configuration["TariffDailyUrl"];
		public static string TariffMonthlyUrl => Configuration["TariffMonthlyUrl"];
		public static string TariffAnnualUrl => Configuration["TariffAnnualUrl"];
		public static string TariffFileExclusionList => Configuration["TariffFileExclusionList"];
		public static string TariffClientId => Configuration["TariffClientId"];
		public static string TariffClientSecret => Configuration["TariffClientSecret"];
		public static string TariffClientScope => Configuration["TariffClientScope"];
		public static string GvmsReferenceDataUrl => Configuration["GvmsReferenceDataUrl"];
		public static string RefDbServiceURI => Configuration["RefDbServiceURI"];
		public static bool IsRefDbServiceSecure => bool.Parse(Configuration["IsRefDbServiceSecure"]);
		public static string ExchangeRateBaseUrl => Configuration["ExchangeRateBaseUrl"];
		public static string ExchangeRateMonthFormat => Configuration["ExchangeRateMonthFormat"];
		public static string CDSErrorCodeUrl => Configuration["CDSErrorCodeUrl"];
		public static string CDSErrorCodeAnchorText => Configuration["CDSErrorCodeAnchorText"];
		public static int TariffHistoricalPeriod => int.Parse(Configuration["TariffHistoricalPeriod"], CultureInfo.CurrentCulture);
		public static bool TariffUseMostRecentAnnualFileOnly => bool.Parse(Configuration["TariffUseMostRecentAnnualFileOnly"]);
		public static string TariffUseSpecifiedAnnualFileOnly => Configuration["TariffUseSpecifiedAnnualFileOnly"];
	}
}
