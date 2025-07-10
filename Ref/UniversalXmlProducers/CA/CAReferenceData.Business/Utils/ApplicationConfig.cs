using System;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.CAReferenceData.Business
{
	public static class ApplicationConfig
	{
		static IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					config = new ConfigurationBuilder().AddJsonFile(jsonConfigFile).Build();
				}
				return config;
			}
		}
		static string jsonConfigFile = "CargoWise.RefDbRepo.CAReferenceData.CmdLine.config.json";
		static IConfiguration config;

		public static string OutputPath => Config[nameof(OutputPath)];
		public static string CACDocumentTypesFileName => Config[nameof(CACDocumentTypesFileName)];
		public static string CACDocumentTypesPublicationTime => Config[nameof(CACDocumentTypesPublicationTime)];
		public static string CATariffPGAConditionPatchFileName => Config[nameof(CATariffPGAConditionPatchFileName)];
		public static string ExchangeRateAPI => Config[nameof(ExchangeRateAPI)];
		public static string CBSAExchangeRatesFileName => Config[nameof(CBSAExchangeRatesFileName)];

		public static string TEMP => Config[nameof(TEMP)];

		public static string CustomsOfficeGenericSublocationCodeFilename => Config[nameof(CustomsOfficeGenericSublocationCodeFilename)];
		public static string CustomsOfficeGenericSublocationCodeUrl => Config[nameof(CustomsOfficeGenericSublocationCodeUrl)];
		public static string SufferanceWarehouseOperatorAndSublocationCodeFilename => Config[nameof(SufferanceWarehouseOperatorAndSublocationCodeFilename)];
		public static string SufferanceWarehouseOperatorAndSublocationCodeUrl => Config[nameof(SufferanceWarehouseOperatorAndSublocationCodeUrl)];

		public static string CBSABaseUrl => Config[nameof(CBSABaseUrl)];
		public static string CATariffDetailUrl => Config[nameof(CATariffDetailUrl)];
		public static string EntityMatcherUri => Config[nameof(EntityMatcherUri)];
		public static string TTCodeFileName => Config[nameof(TTCodeFileName)];
		public static string CustomsTariffAPI => Config[nameof(CustomsTariffAPI)];
		public static string CustomsHarmonizedFilename => Config[nameof(CustomsHarmonizedFilename)];
		public static string TTCodePublicationTime => Config[nameof(TTCodePublicationTime)];
		public static string GSTCodePatchFileName => Config[nameof(GSTCodePatchFileName)];
		public static string CAConditionDetailUrl => Config[nameof(CAConditionDetailUrl)];
		public static string USPortOfExitMappingFileName => Config[nameof(USPortOfExitMappingFileName)];
		public static string CAOfficeCodeUrl => Config[nameof(CAOfficeCodeUrl)];
		public static string CAOfficeCodesFilename => Config[nameof(CAOfficeCodesFilename)];
		public static DateTime CAOfficeCodePublicationTime => DateTime.TryParse(Config[nameof(CAOfficeCodePublicationTime)], out var publicationTime) ? publicationTime : DateTime.MinValue;

		public static string SimaUrl => Config[nameof(SimaUrl)];
		public static string SimaBaseUrl => Config[nameof(SimaBaseUrl)];
		public static string CASIMAFilename => Config[nameof(CASIMAFilename)];

		public static string TariffDbUrl => Config[nameof(TariffDbUrl)];
		public static string SurtaxUrl => Config[nameof(SurtaxUrl)];
		public static string OutputFile => Config[nameof(OutputFile)];

		public static string CBSAEServiceUrl => Config[nameof(CBSABaseUrl)] + "/eservices/";
		public static string CBSAErrorCodes => Config[nameof(CBSAErrorCodes)];

		public static string InspectionCanadaCA => Config[nameof(InspectionCanadaCA)];
		public static string AIRSMiscellaneousCodes => Config[nameof(AIRSMiscellaneousCodes)];
		public static string CFIAAIRSMiscCodesFilename => Config[nameof(CFIAAIRSMiscCodesFilename)];

		public static string CFIAAIRSRegistrationTypesFilename => Config[nameof(CFIAAIRSRegistrationTypesFilename)];

		public static string EmailRecipients => Config[nameof(EmailRecipients)];
		public static string EmailSender => Config[nameof(EmailSender)];
		public static string EmailSmtpServer => Config[nameof(EmailSmtpServer)];
		public static string EmailUsername => Config[nameof(EmailUsername)];
		public static string EmailCredentialsPassword => Config[nameof(EmailCredentialsPassword)];
		public static int EmailSmtpPort => int.TryParse(Config[nameof(EmailSmtpPort)], out var result) ? result : 25;

#if DEBUG
		public static void SetConfigFileForTest(string path)
		{
			jsonConfigFile = path;
			config = null;
		}
#endif
	}
}
