using Microsoft.Extensions.Configuration;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public sealed class ApplicationConfig
	{
		public static IConfiguration Configuration
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

		public static ApplicationConfig Instance => instance.Value;
		public static Lazy<ApplicationConfig> LazyInstance => instance;
		static readonly Lazy<ApplicationConfig> instance = new Lazy<ApplicationConfig>(() => new ApplicationConfig());


		public static void ReLoad()
		{
			Instance.DeltaGBaseUrl = Configuration["DeltaGBaseUrl"];
			Instance.UCC6BaseUrl = Configuration["UCC6BaseUrl"];
			Instance.DownloadDirectory = Configuration["DownloadDirectory"];
			Instance.DropDirectory = Configuration["DropDirectory"];
			Instance.DownloadFileName = Configuration["DownloadFileName"];
			Instance.AirportFileName = Configuration["AirportFileName"];
			Instance.AdditionalCodesFileName = Configuration["AdditionalCodesFileName"];
			Instance.CustomsDestinationCodesFileName = Configuration["CustomsDestinationCodesFileName"];
			Instance.DeltaIEInvalidationMotivationReferenceCodesFileName = Configuration["DeltaIEInvalidationMotivationReferenceCodesFileName"];
			Instance.DeltaIEImportAdditionalReferenceCodesFileName = Configuration["DeltaIEImportAdditionalReferenceCodesFileName"];
			Instance.DeltaIEImportAdditionalReferenceCodesFRSpecifiedFileName = Configuration["DeltaIEImportAdditionalReferenceCodesFRSpecifiedFileName"];
			Instance.DeltaIEImportAdditionalInformationCodesFileName = Configuration["DeltaIEImportAdditionalInformationCodesFileName"];
			Instance.DeltaIEImportAdditionalInformationCodesFRSpecifiedFileName = Configuration["DeltaIEImportAdditionalInformationCodesFRSpecifiedFileName"];
			Instance.DeltaIEImportControlResultsTypeCodesFileName = Configuration["DeltaIEImportControlResultsTypeCodesFileName"];
			Instance.DeltaIEImportControlTypeTypeCodesFileName = Configuration["DeltaIEImportControlTypeTypeCodesFileName"];
			Instance.DeltaIEImportTypeOfDiscrepanciesTypeCodesFileName = Configuration["DeltaIEImportTypeOfDiscrepanciesTypeCodesFileName"];
			Instance.DeltaIEImportRiskAreaCodeTypeCodesFileName = Configuration["DeltaIEImportRiskAreaCodeTypeCodesFileName"];
			Instance.DeltaIEImportPreviousDocumentTypeCodesFileName = Configuration["DeltaIEImportPreviousDocumentTypeCodesFileName"];
			Instance.DeltaIEImportEntrySubStyleCodesFileName = Configuration["DeltaIEImportEntrySubStyleCodesFileName"];
			Instance.DeltaIEImportSupportingDocumentTypeCodesFileName = Configuration["DeltaIEImportSupportingDocumentTypeCodesFileName"];
			Instance.DeltaIEImportSupportingDocumentTypeCodesFRSpecifiedFileName = Configuration["DeltaIEImportSupportingDocumentTypeCodesFRSpecifiedFileName"];
			Instance.DeltaIEImportTransportDocumentTypeCodesFileName = Configuration["DeltaIEImportTransportDocumentTypeCodesFileName"];
			Instance.DeltaIEInwardProcessingConditionsListForAdhocAuthorisationsCodesFileName = Configuration["DeltaIEInwardProcessingConditionsListForAdhocAuthorisationsCodesFileName"];
			Instance.DeltaIELegalConditionsListForAdhocAuthorisationsCodesFileName = Configuration["DeltaIELegalConditionsListForAdhocAuthorisationsCodesFileName"];
			Instance.DeltaIEMotivationForRectificationRequestCodesFileName = Configuration["DeltaIEMotivationForRectificationRequestCodesFileName"];
			Instance.DeltaIETransactionNatureCodesFileName = Configuration["DeltaIETransactionNatureCodesFileName"];
			Instance.ConcessionFileName = Configuration["ConcessionFileName"];
			Instance.CurrenciesFileName = Configuration["CurrenciesFileName"];
			Instance.CurrencyPricesFileName = Configuration["CurrencyPricesFileName"];
			Instance.CustomsProcedureFileName = Configuration["CustomsProcedureFileName"];
			Instance.DocumentNatureFileName = Configuration["DocumentNatureFileName"];
			Instance.DocumentTypeFileName = Configuration["DocumentTypeFileName"];
			Instance.PntsCodeListsFileName = Configuration["PntsCodeListsFileName"];
			Instance.PreviousProcedureFileName = Configuration["PreviousProcedureFileName"];
			Instance.ProcedureFileName = Configuration["ProcedureFileName"];
			Instance.RateTypeFileName = Configuration["RateTypeFileName"];
			Instance.RateTypeWithMeasureTypeFileName = Configuration["RateTypeWithMeasureTypeFileName"];
			Instance.SpecialMentionFileName = Configuration["SpecialMentionFileName"];
			Instance.UnitOfMeasureFileName = Configuration["UnitOfMeasureFileName"];
			Instance.VATAdditionalCodesFileName = Configuration["VATAdditionalCodesFileName"];
			Instance.ZoneFileName = Configuration["ZoneFileName"];

			Instance.FRAdditionalCodesOutputFile = Configuration["FRAdditionalCodesOutputFile"];
			Instance.FRAirportOutputFile = Configuration["FRAirportOutputFile"];
			Instance.FRConditionTypesOutputFile = Configuration["FRConditionTypesOutputFile"];
			Instance.FRCustomsProcedureOutputFile = Configuration["FRCustomsProcedureOutputFile"];
			Instance.FRDeltaIECustomsProcedureOutputFile = Configuration["FRDeltaIECustomsProcedureOutputFile"];
			Instance.FRDocumentNatureOutputFile = Configuration["FRDocumentNatureOutputFile"];
			Instance.FRDocumentTypeOutputFile = Configuration["FRDocumentTypeOutputFile"];
			Instance.FRExchangeOutputFile = Configuration["FRExchangeOutputFile"];
			Instance.FRRateTypeOutputFile = Configuration["FRRateTypeOutputFile"];
			Instance.FRSpecialMentionOutputFile = Configuration["FRSpecialMentionOutputFile"];
			Instance.FRUnitOfMeasureOutputFile = Configuration["FRUnitOfMeasureOutputFile"];
			Instance.FRUnitOfMeasureTranslationOutputFile = Configuration["FRUnitOfMeasureTranslationOutputFile"];
			Instance.FRTradeGroupOutputFile = Configuration["FRTradeGroupOutputFile"];
			Instance.FRNationalRateCodeUsageOutputFile = Configuration["FRNationalRateCodeUsageOutputFile"];
			Instance.FRVATAdditionalCodesOutputFile = Configuration["FRVATAdditionalCodesOutputFile"];

			Instance.RITAWebServiceURL = Configuration["RITAWebServiceURL"];
			Instance.DownloadTimeoutInSeconds = Convert.ToInt16(Configuration["DownloadTimeoutInSeconds"], frCultureInfo);
			Instance.OutputDirectory = Configuration["OutputDirectory"];
			Instance.MinDelayBetweenRequestsToRITA = Convert.ToInt16(Configuration["MinDelayBetweenRequestsToRITA"], frCultureInfo);
			Instance.MaxAttemptsCountInCaseOfTechnicalError = Convert.ToInt16(Configuration["MaxAttemptsCountInCaseOfTechnicalError"], frCultureInfo);
			Instance.MinDelayBetweenAttemptsInCaseOfDownloadError = Convert.ToInt32(Configuration["MinDelayBetweenAttemptsInCaseOfDownloadError"], frCultureInfo);
			Instance.MaxAttemptsCountInCaseOfDownloadError = Convert.ToInt16(Configuration["MaxAttemptsCountInCaseOfDownloadError"], frCultureInfo);
			Instance.MinDelayBetweenAttemptsInCaseOfTechnicalError = Convert.ToInt32(Configuration["MinDelayBetweenAttemptsInCaseOfTechnicalError"], frCultureInfo);
			Instance.TemplateFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Configuration["TemplateFolder"]);
			Instance.WebServiceMeasureRequestTemplate = Path.Combine(Instance.TemplateFolder, Configuration["WebServiceMeasureRequestTemplate"]);
			Instance.WebServiceConditionRequestTemplate = Path.Combine(Instance.TemplateFolder, Configuration["WebServiceConditionRequestTemplate"]);
			Instance.WebServiceNomenclatureRequestTemplate = Path.Combine(Instance.TemplateFolder, Configuration["WebServiceNomenclatureRequestTemplate"]);
			Instance.StandardVatRate = Convert.ToDecimal(Configuration["StandardVatRate"], frCultureInfo);
			Instance.PetroleumVatRate = Convert.ToDecimal(Configuration["PetroleumVatRate"], frCultureInfo);
			Instance.HalfVatRate = Convert.ToDecimal(Configuration["HalfVatRate"], frCultureInfo);
			Instance.DOMStandardVatRate = Convert.ToDecimal(Configuration["DOMStandardVatRate"], frCultureInfo);
			Instance.ReducedVatRate = Convert.ToDecimal(Configuration["ReducedVatRate"], frCultureInfo);
			Instance.SuperReducedVatRate = Convert.ToDecimal(Configuration["SuperReducedVatRate"], frCultureInfo);
			Instance.DOMLiveStockVatRate = Convert.ToDecimal(Configuration["DOMLiveStockVatRate"], frCultureInfo);
			Instance.DOMPressVatRate = Convert.ToDecimal(Configuration["DOMPressVatRate"], frCultureInfo);
			Instance.CorsicaSuperReducedVatRate = Convert.ToDecimal(Configuration["CorsicaSuperReducedVatRate"], frCultureInfo);
			Instance.FreeVatRate = Convert.ToDecimal(Configuration["FreeVatRate"], frCultureInfo);
			Instance.IsRefDbServiceSecure = bool.Parse(Configuration["IsRefDbServiceSecure"]);
			Instance.RefDbServiceURI = Configuration["RefDbServiceURI"];

			Instance.EmailRecipients = Configuration["EmailRecipients"];
			Instance.EmailSender = Configuration["EmailSender"];
		}

		ApplicationConfig()
		{
			DeltaGBaseUrl = Configuration["DeltaGBaseUrl"];
			UCC6BaseUrl = Configuration["UCC6BaseUrl"];
			DownloadDirectory = Configuration["DownloadDirectory"];
			DropDirectory = Configuration["DropDirectory"];
			DownloadFileName = Configuration["DownloadFileName"];
			AirportFileName = Configuration["AirportFileName"];
			AdditionalCodesFileName = Configuration["AdditionalCodesFileName"];
			CustomsDestinationCodesFileName = Configuration["CustomsDestinationCodesFileName"];
			DeltaIEInvalidationMotivationReferenceCodesFileName = Configuration["DeltaIEInvalidationMotivationReferenceCodesFileName"];
			DeltaIEImportAdditionalReferenceCodesFileName = Configuration["DeltaIEImportAdditionalReferenceCodesFileName"];
			DeltaIEImportAdditionalReferenceCodesFRSpecifiedFileName = Configuration["DeltaIEImportAdditionalReferenceCodesFRSpecifiedFileName"];
			DeltaIEImportAdditionalInformationCodesFileName = Configuration["DeltaIEImportAdditionalInformationCodesFileName"];
			DeltaIEImportAdditionalInformationCodesFRSpecifiedFileName = Configuration["DeltaIEImportAdditionalInformationCodesFRSpecifiedFileName"];
			DeltaIEImportControlResultsTypeCodesFileName = Configuration["DeltaIEImportControlResultsTypeCodesFileName"];
			DeltaIEImportControlTypeTypeCodesFileName = Configuration["DeltaIEImportControlTypeTypeCodesFileName"];
			DeltaIEImportTypeOfDiscrepanciesTypeCodesFileName = Configuration["DeltaIEImportTypeOfDiscrepanciesTypeCodesFileName"];
			DeltaIEImportRiskAreaCodeTypeCodesFileName = Configuration["DeltaIEImportRiskAreaCodeTypeCodesFileName"];
			DeltaIEImportPreviousDocumentTypeCodesFileName = Configuration["DeltaIEImportPreviousDocumentTypeCodesFileName"];
			DeltaIEImportEntrySubStyleCodesFileName = Configuration["DeltaIEImportEntrySubStyleCodesFileName"];
			DeltaIEImportSupportingDocumentTypeCodesFileName = Configuration["DeltaIEImportSupportingDocumentTypeCodesFileName"];
			DeltaIEImportSupportingDocumentTypeCodesFRSpecifiedFileName = Configuration["DeltaIEImportSupportingDocumentTypeCodesFRSpecifiedFileName"];
			DeltaIEImportTransportDocumentTypeCodesFileName = Configuration["DeltaIEImportTransportDocumentTypeCodesFileName"];
			DeltaIEInwardProcessingConditionsListForAdhocAuthorisationsCodesFileName = Configuration["DeltaIEInwardProcessingConditionsListForAdhocAuthorisationsCodesFileName"];
			DeltaIELegalConditionsListForAdhocAuthorisationsCodesFileName = Configuration["DeltaIELegalConditionsListForAdhocAuthorisationsCodesFileName"];
			DeltaIEMotivationForRectificationRequestCodesFileName = Configuration["DeltaIEMotivationForRectificationRequestCodesFileName"];
			DeltaIETransactionNatureCodesFileName = Configuration["DeltaIETransactionNatureCodesFileName"];
			ConcessionFileName = Configuration["ConcessionFileName"];
			CurrenciesFileName = Configuration["CurrenciesFileName"];
			CurrencyPricesFileName = Configuration["CurrencyPricesFileName"];
			CustomsProcedureFileName = Configuration["CustomsProcedureFileName"];
			DocumentNatureFileName = Configuration["DocumentNatureFileName"];
			DocumentTypeFileName = Configuration["DocumentTypeFileName"];
			PntsCodeListsFileName = Configuration["PntsCodeListsFileName"];
			PreviousProcedureFileName = Configuration["PreviousProcedureFileName"];
			ProcedureFileName = Configuration["ProcedureFileName"];
			RateTypeFileName = Configuration["RateTypeFileName"];
			RateTypeWithMeasureTypeFileName = Configuration["RateTypeWithMeasureTypeFileName"];
			SpecialMentionFileName = Configuration["SpecialMentionFileName"];
			UnitOfMeasureFileName = Configuration["UnitOfMeasureFileName"];
			VATAdditionalCodesFileName = Configuration["VATAdditionalCodesFileName"];
			ZoneFileName = Configuration["ZoneFileName"];

			FRAdditionalCodesOutputFile = Configuration["FRAdditionalCodesOutputFile"];
			FRAirportOutputFile = Configuration["FRAirportOutputFile"];
			FRConditionTypesOutputFile = Configuration["FRConditionTypesOutputFile"];
			FRCustomsProcedureOutputFile = Configuration["FRCustomsProcedureOutputFile"];
			FRDeltaIECustomsProcedureOutputFile = Configuration["FRDeltaIECustomsProcedureOutputFile"];
			FRDocumentNatureOutputFile = Configuration["FRDocumentNatureOutputFile"];
			FRDocumentTypeOutputFile = Configuration["FRDocumentTypeOutputFile"];
			FRExchangeOutputFile = Configuration["FRExchangeOutputFile"];
			FRRateTypeOutputFile = Configuration["FRRateTypeOutputFile"];
			FRSpecialMentionOutputFile = Configuration["FRSpecialMentionOutputFile"];
			FRUnitOfMeasureOutputFile = Configuration["FRUnitOfMeasureOutputFile"];
			FRUnitOfMeasureTranslationOutputFile = Configuration["FRUnitOfMeasureTranslationOutputFile"];
			FRTradeGroupOutputFile = Configuration["FRTradeGroupOutputFile"];
			FRNationalRateCodeUsageOutputFile = Configuration["FRNationalRateCodeUsageOutputFile"];
			FRVATAdditionalCodesOutputFile = Configuration["FRVATAdditionalCodesOutputFile"];

			RITAWebServiceURL = Configuration["RITAWebServiceURL"];
			DownloadTimeoutInSeconds = Convert.ToInt16(Configuration["DownloadTimeoutInSeconds"], frCultureInfo);
			OutputDirectory = Configuration["OutputDirectory"];
			MinDelayBetweenRequestsToRITA = Convert.ToInt16(Configuration["MinDelayBetweenRequestsToRITA"], frCultureInfo);
			MaxAttemptsCountInCaseOfTechnicalError = Convert.ToInt16(Configuration["MaxAttemptsCountInCaseOfTechnicalError"], frCultureInfo);
			MinDelayBetweenAttemptsInCaseOfDownloadError = Convert.ToInt32(Configuration["MinDelayBetweenAttemptsInCaseOfDownloadError"], frCultureInfo);
			MaxAttemptsCountInCaseOfDownloadError = Convert.ToInt16(Configuration["MaxAttemptsCountInCaseOfDownloadError"], frCultureInfo);
			MinDelayBetweenAttemptsInCaseOfTechnicalError = Convert.ToInt32(Configuration["MinDelayBetweenAttemptsInCaseOfTechnicalError"], frCultureInfo);
			TemplateFolder = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Configuration["TemplateFolder"]);
			WebServiceMeasureRequestTemplate = Path.Combine(TemplateFolder, Configuration["WebServiceMeasureRequestTemplate"]);
			WebServiceConditionRequestTemplate = Path.Combine(TemplateFolder, Configuration["WebServiceConditionRequestTemplate"]);
			WebServiceNomenclatureRequestTemplate = Path.Combine(TemplateFolder, Configuration["WebServiceNomenclatureRequestTemplate"]);
			StandardVatRate = Convert.ToDecimal(Configuration["StandardVatRate"], frCultureInfo);
			PetroleumVatRate = Convert.ToDecimal(Configuration["PetroleumVatRate"], frCultureInfo);
			HalfVatRate = Convert.ToDecimal(Configuration["HalfVatRate"], frCultureInfo);
			DOMStandardVatRate = Convert.ToDecimal(Configuration["DOMStandardVatRate"], frCultureInfo);
			ReducedVatRate = Convert.ToDecimal(Configuration["ReducedVatRate"], frCultureInfo);
			SuperReducedVatRate = Convert.ToDecimal(Configuration["SuperReducedVatRate"], frCultureInfo);
			DOMLiveStockVatRate = Convert.ToDecimal(Configuration["DOMLiveStockVatRate"], frCultureInfo);
			DOMPressVatRate = Convert.ToDecimal(Configuration["DOMPressVatRate"], frCultureInfo);
			CorsicaSuperReducedVatRate = Convert.ToDecimal(Configuration["CorsicaSuperReducedVatRate"], frCultureInfo);
			FreeVatRate = Convert.ToDecimal(Configuration["FreeVatRate"], frCultureInfo);
			IsRefDbServiceSecure = bool.Parse(Configuration["IsRefDbServiceSecure"]);
			RefDbServiceURI = Configuration["RefDbServiceURI"];

			EmailRecipients = Configuration["EmailRecipients"];
			EmailSender = Configuration["EmailSender"];
			EmailSmtpServer = Configuration["EmailSmtpServer"];
			EmailUsername = Configuration["EmailUsername"];
			EmailCredentialsPassword = Configuration["EmailCredentialsPassword"];
			EmailSmtpPort = Convert.ToInt32(Configuration["EmailSmtpPort"], frCultureInfo);

			PermitDocumentTypes = Configuration["PermitDocumentTypes"];
			ODSDocumentTypes = Configuration["ODSDocumentTypes"];

			NationalRateCodesRequiringUsageTracking = Configuration["NationalRateCodesRequiringUsageTracking"];
		}


		static string JsonConfigFileName => "CargoWise.RefDbRepo.FRReferenceData.CmdLine.config.json";

		public static CultureInfo frCultureInfo => CultureInfo.GetCultureInfo("fr-FR");
		public string DeltaGBaseUrl { get; set; }
		public string UCC6BaseUrl { get; set; }
		public string DownloadDirectory { get; set; }
		public string DropDirectory { get; set; }
		public string DownloadFileName { get; set; }
		public string CurrenciesFileName { get; set; }
		public string CurrencyPricesFileName { get; set; }
		public string AirportFileName { get; set; }
		public string AdditionalCodesFileName { get; set; }
		public string CustomsDestinationCodesFileName { get; set; }
		public string DeltaIEInvalidationMotivationReferenceCodesFileName { get; set; }
		public string DeltaIEImportAdditionalReferenceCodesFileName { get; set; }
		public string DeltaIEImportAdditionalReferenceCodesFRSpecifiedFileName { get; set; }
		public string DeltaIEImportAdditionalInformationCodesFileName { get; set; }
		public string DeltaIEImportAdditionalInformationCodesFRSpecifiedFileName { get; set; }
		public string DeltaIEImportControlResultsTypeCodesFileName { get; set; }
		public string DeltaIEImportControlTypeTypeCodesFileName { get; set; }
		public string DeltaIEImportTypeOfDiscrepanciesTypeCodesFileName { get; set; }
		public string DeltaIEImportRiskAreaCodeTypeCodesFileName { get; set; }
		public string DeltaIEImportPreviousDocumentTypeCodesFileName { get; set; }
		public string DeltaIEImportEntrySubStyleCodesFileName { get; set; }
		public string DeltaIEImportSupportingDocumentTypeCodesFileName { get; set; }
		public string DeltaIEImportSupportingDocumentTypeCodesFRSpecifiedFileName { get; set; }
		public string DeltaIEImportTransportDocumentTypeCodesFileName { get; set; }
		public string DeltaIEInwardProcessingConditionsListForAdhocAuthorisationsCodesFileName { get; set; }
		public string DeltaIELegalConditionsListForAdhocAuthorisationsCodesFileName { get; set; }
		public string DeltaIEMotivationForRectificationRequestCodesFileName { get; set; }
		public string DeltaIETransactionNatureCodesFileName { get; set; }
		public string DocumentNatureFileName { get; set; }
		public string DocumentTypeFileName { get; set; }
		public string PntsCodeListsFileName { get; set; }
		public string RateTypeWithMeasureTypeFileName { get; set; }
		public string SpecialMentionFileName { get; set; }
		public string UnitOfMeasureFileName { get; set; }
		public string VATAdditionalCodesFileName { get; set; }
		public string ConcessionFileName { get; set; }
		public string CustomsProcedureFileName { get; set; }
		public string PreviousProcedureFileName { get; set; }
		public string ProcedureFileName { get; set; }
		public string RateTypeFileName { get; set; }
		public string ZoneFileName { get; set; }
		public string FRExchangeOutputFile { get; private set; }
		public string FRAirportOutputFile { get; private set; }
		public string FRAdditionalCodesOutputFile { get; private set; }
		public string FRConditionTypesOutputFile { get; private set; }
		public string FRDocumentNatureOutputFile { get; private set; }
		public string FRDocumentTypeOutputFile { get; private set; }
		public string FRSpecialMentionOutputFile { get; private set; }
		public string FRUnitOfMeasureOutputFile { get; private set; }
		public string FRUnitOfMeasureTranslationOutputFile { get; private set; }
		public string FRCustomsProcedureOutputFile { get; private set; }
		public string FRDeltaIECustomsProcedureOutputFile { get; private set; }
		public string FRTradeGroupOutputFile { get; private set; }
		public string FRRateTypeOutputFile { get; private set; }
		public string FRNationalRateCodeUsageOutputFile { get; private set; }
		public string FRVATAdditionalCodesOutputFile { get; private set; }
		public string RITAWebServiceURL { get; private set; }
		public int DownloadTimeoutInSeconds { get; private set; }
		public string OutputDirectory { get; set; }
		public int MinDelayBetweenRequestsToRITA { get; private set; }
		public int MaxAttemptsCountInCaseOfTechnicalError { get; set; }
		public int MinDelayBetweenAttemptsInCaseOfTechnicalError { get; set; }
		public int MinDelayBetweenAttemptsInCaseOfDownloadError { get; set; }
		public int MaxAttemptsCountInCaseOfDownloadError { get; set; }
		public string TemplateFolder { get; private set; }
		public string WebServiceMeasureRequestTemplate { get; private set; }
		public string WebServiceConditionRequestTemplate { get; private set; }
		public string WebServiceNomenclatureRequestTemplate { get; private set; }
		public string VATApplicabilitySchema { get; private set; }
		public decimal StandardVatRate { get; private set; }
		public decimal PetroleumVatRate { get; private set; }
		public decimal HalfVatRate { get; private set; }
		public decimal DOMStandardVatRate { get; private set; }
		public decimal ReducedVatRate { get; private set; }
		public decimal SuperReducedVatRate { get; private set; }
		public decimal DOMLiveStockVatRate { get; private set; }
		public decimal DOMPressVatRate { get; private set; }
		public decimal CorsicaSuperReducedVatRate { get; private set; }
		public decimal FreeVatRate { get; private set; }
		public bool IsRefDbServiceSecure { get; private set; }
		public string RefDbServiceURI { get; private set; }
		public string EmailRecipients { get; set; }
		public string EmailSender { get; private set; }
		public string EmailSmtpServer { get; }
		public string EmailUsername { get; }
		public string EmailCredentialsPassword { get; }
		public int EmailSmtpPort { get; }
		public string PermitDocumentTypes { get; }
		public string ODSDocumentTypes { get; }
		public string NationalRateCodesRequiringUsageTracking { get; }
	}
}
