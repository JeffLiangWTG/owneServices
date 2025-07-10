using System;
using Microsoft.Extensions.Configuration;

namespace CargoWise.RefDbRepo.EUReferenceData.Services
{
	public sealed class ApplicationConfig
	{
		public static ApplicationConfig Instance => instance.Value;

		static readonly Lazy<ApplicationConfig> instance = new Lazy<ApplicationConfig>(() => new ApplicationConfig());



		ApplicationConfig()
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile(JsonFileName)
				.Build();

			OutputDirectory = config["OutputPath"];
			DownloadPageUrl = config["OfficeCodesDownloadPageUrl"];
			CUSNumberEntryPointUrl = config["CUSNumberEntryPointUrl"];
			CUSNumberListUrlPattern = config["CUSNumberListUrlPattern"];
			CUSNumberSoapServiceUrl = config["CUSNumberSoapServiceUrl"];
			CUSNumberSoapServiceNumberOfItemsPerCall = int.TryParse(config["CUSNumberSoapServiceNumberOfItemsPerCall"], out var itemsPerCall) ? itemsPerCall : 10;
			CUSNumberPagesPerBatch = int.TryParse(config["CUSNumberPagesPerBatch"], out var pagesPerBatch) ? pagesPerBatch : 20;
			RDEntryFileLinkPattern = config["OfficeCodesRDEntryFileLinkPattern"];

			CustomsMeursingDownloadDir = config["CustomsMeursingDownloadDir"];
			CircabcServiceUrlTemplate = config["CircabcServiceUrlTemplate"];
			CirabcDutiesAndRelatedRootId = config["CirabcDutiesAndRelatedRootId"];
			CircabcDownloadUrlTemplate = config["CircabcDownloadUrlTemplate"];

			TransportChargesMethodOfPaymentUrl = config["TransportChargesMethodOfPaymentUrl"];
			DocumentTypeCommonDownloadUrl = config["DocumentTypeCommonDownloadUrl"];
			KindOfPackagesDownloadUrl = config["KindOfPackagesDownloadUrl"];
			AdditionalSupplyChainActorRoleCodeUrl = config["AdditionalSupplyChainActorRoleCodeUrl"];
			TypeOfGoodsUrl = config["TypeOfGoodsUrl"];
			TypeOfMeansOfTransportUrl = config["TypeOfMeansOfTransportUrl"];
			AdditionalInformationCodeSubsetUrl = config["AdditionalInformationCodeSubsetUrl"];
			StateSubsetUrl = config["StateSubsetUrl"];
			ICS2FunctionalErrorCodesUrl = config["ICS2FunctionalErrorCodesUrl"];
			ICS2HRCMScreeningMethodUrl = config["ICS2HRCMScreeningMethodUrl"];
			ICS2CountryCodeICS2MSUrl = config["ICS2CountryCodeICS2MSUrl"];
			ICS2UnLocodeExtendedUrl = config["ICS2UnLocodeExtendedUrl"];

			AESAdditionalInformationUrl = config["AESAdditionalInformationUrl"];
			AESAdditionalReferenceUrl = config["AESAdditionalReferenceUrl"];
			AESTransportDocumentTypeUrl = config["AESTransportDocumentTypeUrl"];
			AESPreviousDocumentTypeUrl = config["AESPreviousDocumentTypeUrl"];
			AESNationalityUrl = config["AESNationalityUrl"];
			NctsCodeListDownloadUrl = config["UCCCodeListDownloadUrl"];
			AuthorisationTypeUrl = config["AuthorisationTypeUrl"];

			CCIPreviousDocumentTypeUrl = config["CCIPreviousDocumentTypeUrl"];
			CCIMethodOfPaymentUrl = config["CCIMethodOfPaymentUrl"];

			RefCusCodeListAdditionalTranslationsPath = config["RefCusCodeListAdditionalTranslationsPath"];
		}

		public string OutputDirectory { get; }

		public string DownloadPageUrl { get; }

		public string CUSNumberEntryPointUrl { get; }

		public string CUSNumberListUrlPattern { get; }

		public string CUSNumberSoapServiceUrl { get; }

		public int CUSNumberSoapServiceNumberOfItemsPerCall { get; }

		public int CUSNumberPagesPerBatch { get; }

		public string RDEntryFileLinkPattern { get; }

		public string CustomsMeursingDownloadDir { get; }
		public string CircabcServiceUrlTemplate { get; }
		public string CirabcDutiesAndRelatedRootId { get; }
		public string CircabcDownloadUrlTemplate { get; }

		public string TransportChargesMethodOfPaymentUrl { get; }
		public string DocumentTypeCommonDownloadUrl { get; }
		public string KindOfPackagesDownloadUrl { get; }
		public string AdditionalSupplyChainActorRoleCodeUrl { get; }
		public string TypeOfGoodsUrl { get; }
		public string TypeOfMeansOfTransportUrl { get; }
		public string StateSubsetUrl { get; }
		public string AdditionalInformationCodeSubsetUrl { get; }
		public string ICS2FunctionalErrorCodesUrl { get; }
		public string ICS2HRCMScreeningMethodUrl { get; }
		public string ICS2CountryCodeICS2MSUrl { get; }
		public string ICS2UnLocodeExtendedUrl { get; }

		public string AESAdditionalInformationUrl { get; }
		public string AESAdditionalReferenceUrl { get; }
		public string AESTransportDocumentTypeUrl { get; }
		public string AESPreviousDocumentTypeUrl { get; }
		public string AESNationalityUrl { get; }

		public string NctsCodeListDownloadUrl { get; }

		public string AuthorisationTypeUrl { get; }

		public string CCIPreviousDocumentTypeUrl { get; }
		public string CCIMethodOfPaymentUrl { get; }

		public string RefCusCodeListAdditionalTranslationsPath { get; }

		const string JsonFileName = "CargoWise.RefDbRepo.EUReferenceData.CmdLine.Config.json";
	}
}
