using System;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.CHReferenceData.Services
{
	public sealed class ApplicationConfig
	{
		const string JsonConfigFileName = "CargoWise.RefDbRepo.CHReferenceData.CmdLine.config.json";

		public static ApplicationConfig Instance => instance.Value;

		static readonly Lazy<ApplicationConfig> instance = new Lazy<ApplicationConfig>(() => new ApplicationConfig());

		ApplicationConfig()
		{
			var config = new ConfigurationBuilder().AddJsonFile(JsonConfigFileName).Build();

			OutputPath = config["OutputPath"];
			ExchangeRatesUrl = config["ExchangeRatesUrl"];
			CodeListsUrl = config["CodeListsUrl"];
			CustomsOfficesUrl = config["CustomsOfficesUrl"];
			TradeGroupsUrl = config["TradeGroupsUrl"];
			MasterDataUrl = config["MasterDataUrl"];
			PassarMasterDataUrl = config["PassarMasterDataUrl"];
			TariffStructureUrl = config["TariffStructureUrl"];
			KeyStructureExportUrl = config["KeyStructureExportUrl"];
			KeyStructureImportUrl = config["KeyStructureImportUrl"];
			PermitInformationExportUrl = config["PermitInformationExportUrl"];
			PermitInformationImportUrl = config["PermitInformationImportUrl"];
			PermitItemDetailsUrl = config["PermitItemDetailsUrl"];
			NonCustomsLawInformationImportUrl = config["NonCustomsLawInformationImportUrl"];
			NonCustomsLawInformationExportUrl = config["NonCustomsLawInformationExportUrl"];
			CustomsFacilitiesUrl = config["CustomsFacilitiesUrl"];
			KeyStructureAdditionalTaxesUrl = config["KeyStructureAdditionalTaxesUrl"];
			PassarCodeListsUrl = config["PassarCodeListsUrl"];
		}

		public string OutputPath { get; }
		public string ExchangeRatesUrl { get; }
		public string CodeListsUrl { get; }
		public string CustomsOfficesUrl { get; }
		public string TradeGroupsUrl { get; }
		public string MasterDataUrl { get; }
		public string PassarMasterDataUrl { get; }
		public string TariffStructureUrl { get; }
		public string KeyStructureExportUrl { get; }
		public string KeyStructureImportUrl { get; }
		public string PermitInformationExportUrl { get; }
		public string PermitInformationImportUrl { get; }
		public string PermitItemDetailsUrl { get; }
		public string NonCustomsLawInformationImportUrl { get; }
		public string NonCustomsLawInformationExportUrl { get; }
		public string CustomsFacilitiesUrl { get; }
		public string KeyStructureAdditionalTaxesUrl { get; }
		public string PassarCodeListsUrl { get; }
	}
}
