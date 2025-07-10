using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.KRReferenceData.Services
{
	public static class ApplicationConfig
	{
		const string JsonConfigFileName = "CargoWise.RefDbRepo.KRReferenceData.CmdLine.config.json";
		public static IConfiguration Config
		{
			get
			{
				if (config == null)
				{
					var configurationBuilder = new ConfigurationBuilder();
					config = configurationBuilder.AddJsonFile(JsonConfigFileName).Build();
				}
				return config;
			}
		}
		static IConfiguration config;

		public static string OutputDirectory => Config[nameof(OutputDirectory)];
		public static string ExchangeRatesStartURL => Config[nameof(ExchangeRatesStartURL)];
		public static string TariffConfigFileInputPath => Config[nameof(TariffConfigFileInputPath)];
		public static string TariffDataFileInputPath => Config[nameof(TariffDataFileInputPath)];
		public static string NonGAReasonExportConfigFileInputPath => Config[nameof(NonGAReasonExportConfigFileInputPath)];
		public static string NonGAReasonExportDataFileInputPath => Config[nameof(NonGAReasonExportDataFileInputPath)];
		public static string NonGAReasonImportConfigFileInputPath => Config[nameof(NonGAReasonImportConfigFileInputPath)];
		public static string NonGAReasonImportDataFileInputPath => Config[nameof(NonGAReasonImportDataFileInputPath)];
		public static string NonGAReasonPublicationDate => Config[nameof(NonGAReasonPublicationDate)];
		public static string PreferenceConfigFileInputPath => Config[nameof(PreferenceConfigFileInputPath)];
		public static string PreferenceDataFileInputPath => Config[nameof(PreferenceDataFileInputPath)];
		public static string PreferenceRefCusMapConfigFileInputPath => Config[nameof(PreferenceRefCusMapConfigFileInputPath)];
		public static string PreferenceRefCusMapDataFileInputPath => Config[nameof(PreferenceRefCusMapDataFileInputPath)];
		public static string ExportFTATypeConfigFilePath => Config[nameof(ExportFTATypeConfigFilePath)];
		public static string ExportFTATypeDataFilePath => Config[nameof(ExportFTATypeDataFilePath)];
		public static string WCONomenclatureDataFileInputPathPDF => Config[nameof(WCONomenclatureDataFileInputPathPDF)];
		public static string WCONomenclatureDataFileInputPathXLS => Config[nameof(WCONomenclatureDataFileInputPathXLS)];
		public static string WCONomenclatureConfigFileInputPath => Config[nameof(WCONomenclatureConfigFileInputPath)];
		public static string KRNomenclatureConfigFileInputPath => Config[nameof(KRNomenclatureConfigFileInputPath)];
		public static string KRNomenclatureDataFileInputPath => Config[nameof(KRNomenclatureDataFileInputPath)];
		public static string SafeDataUpdateUri => Config[nameof(SafeDataUpdateUri)];
		public static string WCOPublicationDate => Config[nameof(WCOPublicationDate)];
		public static string HSCodePublicationDate => Config[nameof(HSCodePublicationDate)];
		public static string OGAPublicationDate => Config[nameof(OGAPublicationDate)];
		public static string OGAImportConfigFileInputPath => Config[nameof(OGAImportConfigFileInputPath)];
		public static string OGAExportConfigFileInputPath => Config[nameof(OGAExportConfigFileInputPath)];
		public static string OGAImportDataFileInputPath => Config[nameof(OGAImportDataFileInputPath)];
		public static string OGAExportDataFileInputPath => Config[nameof(OGAExportDataFileInputPath)];
		public static string TradeGroupConfigFilePath => Config[nameof(TradeGroupConfigFilePath)];
		public static string TradeGroupDataFilePath => Config[nameof(TradeGroupDataFilePath)];
		public static string SteelNomenclaturesConfigFileInputPath => Config[nameof(SteelNomenclaturesConfigFileInputPath)];
		public static string SteelNomenclaturesDataFileInputPath => Config[nameof(SteelNomenclaturesDataFileInputPath)];
		public static string SteelNomenclaturesPublicationDate => Config[nameof(SteelNomenclaturesPublicationDate)];
		public static string SimpleDrawbackPublicationDate => Config[nameof(SimpleDrawbackPublicationDate)];
		public static string SimpleDrawbackConfigFileInputPath => Config[nameof(SimpleDrawbackConfigFileInputPath)];
		public static string SimpleDrawbackDataFileInputPath => Config[nameof(SimpleDrawbackDataFileInputPath)];
		public static string CustomsOfficeConfigFilePath => Config[nameof(CustomsOfficeConfigFilePath)];
		public static string CustomsOfficeDataFilePath => Config[nameof(CustomsOfficeDataFilePath)];
		public static string CustomsDepartmentConfigFilePath => Config[nameof(CustomsDepartmentConfigFilePath)];
		public static string CustomsDepartmentDataFilePath => Config[nameof(CustomsDepartmentDataFilePath)];
		public static string CustomsOfficeDepartmentPublicationDate => Config[nameof(CustomsOfficeDepartmentPublicationDate)];
		public static string ForwarderIDsPublicationDate => Config[nameof(ForwarderIDsPublicationDate)];
		public static string ForwarderIDsConfigFilePath => Config[nameof(ForwarderIDsConfigFilePath)];
		public static string ForwarderIDsDataFilePath => Config[nameof(ForwarderIDsDataFilePath)];
		public static string OtherGovernmentConfigFilePath => Config[nameof(OtherGovernmentConfigFilePath)];
		public static string OtherGovernmentDataFilePath => Config[nameof(OtherGovernmentDataFilePath)];
		public static string OtherGovernmentPublicationDate => Config[nameof(OtherGovernmentPublicationDate)];
		public static string ExpressDeliveryServiceIDConfigFileInputPath => Config[nameof(ExpressDeliveryServiceIDConfigFileInputPath)];
		public static string ExpressDeliveryServiceIDDataFileInputPath => Config[nameof(ExpressDeliveryServiceIDDataFileInputPath)];
		public static string BrandCodesConfigFilePath => Config[nameof(BrandCodesConfigFilePath)];
		public static string BrandCodesDataFilePath => Config[nameof(BrandCodesDataFilePath)];
		public static string BrandCodesPublicationDate => Config[nameof(BrandCodesPublicationDate)];
		public static string DutyRateConfigFileInputPath => Config[nameof(DutyRateConfigFileInputPath)];
		public static string SpecialUseCodeDutyRateConfigFileInputPath => Config[nameof(SpecialUseCodeDutyRateConfigFileInputPath)];
		public static string DutyRateDataFileInputPath => Config[nameof(DutyRateDataFileInputPath)];
		public static string DutyRatePublicationDate => Config[nameof(DutyRatePublicationDate)];
		public static string DutyReductionExemptionConfigFileInputPath => Config[nameof(DutyReductionExemptionConfigFileInputPath)];
		public static string DutyReductionExemptionDataFileInputPath => Config[nameof(DutyReductionExemptionDataFileInputPath)];
		public static string DutyReductionExemptionPublicationDate => Config[nameof(DutyReductionExemptionPublicationDate)];
		public static string DomesticTaxExemptionConfigFileInputPath => Config[nameof(DomesticTaxExemptionConfigFileInputPath)];
		public static string DomesticTaxExemptionDataFileInputPath => Config[nameof(DomesticTaxExemptionDataFileInputPath)];
		public static string DomesticTaxExemptionPublicationDate => Config[nameof(DomesticTaxExemptionPublicationDate)];
		public static string DomesticTaxRatesConfigFileInputPath => Config[nameof(DomesticTaxRatesConfigFileInputPath)];
		public static string DomesticTaxRatesDataFileInputPath => Config[nameof(DomesticTaxRatesDataFileInputPath)];
		public static string DomesticTaxRatesPublicationDate => Config[nameof(DomesticTaxRatesPublicationDate)];
		public static string HSExtensionCodeConfigFileMainInputPath => Config[nameof(HSExtensionCodeConfigFileMainInputPath)];
		public static string HSExtensionCodeConfigFileSubInputPath => Config[nameof(HSExtensionCodeConfigFileSubInputPath)];
		public static string HSExtensionCodeDataFileInputPath => Config[nameof(HSExtensionCodeDataFileInputPath)];
		public static string HSExtensionCodePublicationDate => Config[nameof(HSExtensionCodePublicationDate)];
		public static string InstalmentCodesConfigFileInputPath => Config[nameof(InstalmentCodesConfigFileInputPath)];
		public static string InstalmentCodesDataFileInputPath => Config[nameof(InstalmentCodesDataFileInputPath)];
		public static string InstalmentCodesPublicationDate => Config[nameof(InstalmentCodesPublicationDate)];
		public static string AdditionalPaymentReasonsConfigFilePath => Config[nameof(AdditionalPaymentReasonsConfigFilePath)];
		public static string AdditionalPaymentReasonsDataFilePath => Config[nameof(AdditionalPaymentReasonsDataFilePath)];
		public static string AdditionalPaymentReasonsPublicationDate => Config[nameof(AdditionalPaymentReasonsPublicationDate)];
		public static string TaxOfficeConfigFileInputPath => Config[nameof(TaxOfficeConfigFileInputPath)];
		public static string TaxOfficeDataFileInputPath => Config[nameof(TaxOfficeDataFileInputPath)];
		public static string TaxOfficePublicationDate => Config[nameof(TaxOfficePublicationDate)];
		public static string OGARegulationCategoryConfigFilePath => Config[nameof(OGARegulationCategoryConfigFilePath)];
		public static string OGARegulationCategoryPublicationDate => Config[nameof(OGARegulationCategoryPublicationDate)];
		public static string OGARegulationCategoryDataFilePath => Config[nameof(OGARegulationCategoryDataFilePath)];
	}
}
