using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.JPReferenceData.Services
{
	public static class AppConfig
	{
		public static class Shared
		{
			public static string OutputDirectory => Config["shared:outputDirectory"];
		}

		public static class ExchangeRate
		{
			public static string Url => Config["exchangeRate:url"];
			public static string PdfFileUrlPrefix => Config["exchangeRate:pdfFileUrlPrefix"];
		}

		public static class Customs
		{
			public static string BaseUrl => Config["customs:baseUrl"];

			public static class CodeLists
			{
				public static string BaseUrl => Config["customs:codeLists:baseUrl"];
				public static string FSBTxtFileDownloadUrl => Config["customs:codeLists:fsbTxtFileDownloadUrl"];
			}
		}

		public static class NACCS
		{
			public static string BaseUrl => Config["naccs:shared:baseUrl"];

			public static class CodeLists
			{
				public static string BaseUrl => Config["naccs:codeLists:baseUrl"];
				public static string CarrierBaseUrl => Config["naccs:codeLists:carrierBaseUrl"];
				public static string ConsumptionTaxExemptionReductionCodeCsvFileDownloadUrl => Config["naccs:codeLists:consumptionTaxExemptionReductionCodeCsvFileDownloadUrl"];
				public static string JPNACCSTariffHomePageDownloadUrl => Config["naccs:codeLists:jPNACCSTariffHomePageDownloadUrl"];
				public static string JPNACCSExportTariffHomePageDownloadUrl => Config["naccs:codeLists:jPNACCSExportTariffHomePageDownloadUrl"];
				public static string JPNACCSTariffPath => Config["naccs:codeLists:jPNACCSTariffPath"];
				public static string JPNACCSTariffNeedParallel => Config["jPNACCSTariffNeedParallel"];
				public static string JPNACCS98TariffDownloadUrl => Config["naccs:codeLists:jPNACCS98TariffDownloadUrl"];
				public static string JPNACCS99TariffDownloadUrl => Config["naccs:codeLists:jPNACCS99TariffDownloadUrl"];
				public static string DutyExemptionRefundCodeCsvFileDownloadUrl => Config["naccs:codeLists:dutyExemptionRefundCodeCsvFileDownloadUrl"];
				public static string ExportApprovalCertificateCsvFileDownloadUrl => Config["naccs:codeLists:exportApprovalCertificateTypeCsvFileDownloadUrl"];
				public static string BeforePermitApplicationReasonCodeCsvDownloadUrl => Config["naccs:codeLists:beforePermitApplicationReasonCodeCsvFileDownloadUrl"];
				public static string ImportTradeControlOrdinanceAppendixCsvFileDownloadUrl => Config["naccs:codeLists:importTradeControlOrdinanceAppendixCsvFileDownloadUrl"];
				public static string CustomsOfficesFileDownloadUrl => Config["naccs:codeLists:customsOfficesFileDownloadUrl"];
				public static string CustomsOfficeDepartmentsFileDownloadParentUrl => Config["naccs:codeLists:customsOfficeDepartmentsFileDownloadParentUrl"];
				public static string CertificateOfOriginTypeCodeFileDownloadUrl => Config["naccs:codeLists:certificateOfOriginTypeCodeFileDownloadUrl"];
				public static string ConsumptionTaxExemptionCodeExportFileDownloadParentUrl => Config["naccs:codeLists:consumptionTaxExemptionCodeExportFileDownloadParentUrl"];
				public static string UnitOfMeasurementCsvFileDownloadUrl => Config["naccs:codeLists:unitOfMeasurementCsvFileDownloadUrl"];
				public static string BondedAreaCodeHomePageUrl => Config["naccs:codeLists:bondedAreaCodeHomePageUrl"];
				public static string BondedAreaCodeCsvFileDownloadUrl => Config["naccs:codeLists:bondedAreaCodeCsvFileDownloadUrl"];
				public static string DutyExemptionCodeCsvFileDownloadUrl => Config["naccs:codeLists:dutyExemptionCodeCsvFileDownloadUrl"];
				public static string ImportApprovalCertificateNumberFileDownloadUrl => Config["naccs:codeLists:importApprovalCertificateNumberFileDownloadUrl"];
				public static string OtherLawCodeCsvFileDownloadUrl => Config["naccs:codeLists:otherLawCodeCsvFileDownloadUrl"];
				public static string SpecialCargoCodeCsvFileDownloadUrl => Config["naccs:codeLists:specialCargoCodeCsvFileDownloadUrl"];
				public static string SpecialCargoCodeEngDescFileName => Config["specialCargoCodeEngDescFileName"];
				public static string SpecialCargoCodeEngDescPublicationTime => Config["specialCargoCodeEngDescPublicationTime"];
				public static string ResultCodeZipDownloadUrl => Config["naccs:codeLists:resultCodeZipDownloadUrl"];
				public static string ResultCodeHomePageUrl => Config["naccs:codeLists:resultCodeHomePageUrl"];
				public static string ExportTradeControlOrdinanceAppendixCsvFileDownloadUrl => Config["naccs:codeLists:exportTradeControlOrdinanceAppendixCsvFileDownloadUrl"];
				public static string MSXDocumentTypeCsvFileDownloadUrl => Config["naccs:codeLists:msxDocumentTypeCsvFileDownloadUrl"];
				public static string UNLOCOAndIATACsvFileDownloadUrl => Config["naccs:codeLists:uNLOCOAndIATACsvFileDownloadUrl"];
				public static string ContainerTypeCsvFileDownloadUrl => Config["naccs:codeLists:containerTypeCsvFileDownloadUrl"];
				public static string ContainerLengthAndHeightCsvFileDownloadUrl => Config["naccs:codeLists:containerLengthAndHeightDownloadUrl"];
				public static string SeaCarrierCsvFileDownloadUrl => Config["naccs:codeLists:seaCarrierCsvFileDownloadUrl"];
				public static string PackageTypeCsvFileDownloadUrl => Config["naccs:codeLists:packageTypeCsvFileDownloadUrl"];
				public static string SeaVesselCsvFileDownloadUrl => Config["naccs:codeLists:seaVesselCsvFileDownloadUrl"];
			}
		}

		const string JsonConfigFileName = "CargoWise.RefDbRepo.JPReferenceData.CmdLine.config.json";
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
	}
}
