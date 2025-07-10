using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Emcs;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Export;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Generic;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Import;
using CargoWise.RefDbRepo.DEReferenceData.Business.CodeLists.Ncts;
using CargoWise.RefDbRepo.DEReferenceData.Services;
using CargoWise.RefDbRepo.DEReferenceData.Services.CodeLists;

namespace CargoWise.RefDbRepo.DEReferenceData.CmdLine
{
	class CodeListsProgram
	{
		public static void Run(string[] args, string outputFilePath)
		{
			var (functionsToExclude, continueProcessing) = ValidateArguments(args);
			if (continueProcessing)
			{
				var importDownloadLinksList = new string[0];
				var exportDownloadLinksList = new string[0];
				var nctsDownloadLinksList = new string[0];
				var emcsDownloadLinks = new Dictionary<string, string>();

				var httpClient = new HttpClient();

				Parallel.Invoke(
					() => importDownloadLinksList = CodeListsDownloaderXML.GetDownloadLinks(httpClient, $"{ApplicationConfig.ImportCodeListsDownloadUrlPrimary}", $"{ApplicationConfig.ImportCodeListsDownloadUrlSecondary}"),
					() => exportDownloadLinksList = CodeListsDownloaderXML.GetDownloadLinks(httpClient, $"{ApplicationConfig.ExportCodeListsDownloadUrlPrimary}"),
					() => nctsDownloadLinksList = CodeListsDownloaderXML.GetDownloadLinks(httpClient, $"{ApplicationConfig.NctsCodeListsDownloadUrlPrimary}"),
					() => emcsDownloadLinks = CodeListsDownloaderTSV.GetEMCSDownloadLinks(httpClient, ApplicationConfig.EmcsCodeListsDownloadPageUrl)
				);

				var functionsToRun = GetFunctionsToRun(httpClient, outputFilePath, exportDownloadLinksList, importDownloadLinksList, nctsDownloadLinksList, emcsDownloadLinks);

				if (functionsToExclude.Any())
				{
					foreach (var functionToExclude in functionsToExclude)
					{
						functionsToRun.Remove(functionToExclude);
					}
				}

				Parallel.Invoke(functionsToRun.Select(x => x.Value).ToArray());

				httpClient.Dispose();
			}
		}

		static Dictionary<string, Action> GetFunctionsToRun(HttpClient httpClient, string outputFilePath, string[] exportDownloadLinksList, string[] importDownloadLinksList, string[] nctsDownloadLinksList, Dictionary<string, string> emcsDownloadLinks)
		{
			var importExportDownloadLinksListXML = importDownloadLinksList.Concat(exportDownloadLinksList).ToArray();

			return new Dictionary<string, Action>
			{
				{ nameof(CodeListsConstants.Generic.CodeTypes.CO15_ORIGIN_COUNTRY_LIST), () => ParseXML(new GenericOriginCountryCodesCO15Transformer(importExportDownloadLinksListXML)) },
				{ nameof(CodeListsConstants.Generic.CodeTypes.CO17_DESTINATION_COUNTRY_LIST), () => ParseXML(new GenericDestinationCountryCO17Transformer(importExportDownloadLinksListXML)) },
				{ nameof(CodeListsConstants.Generic.CodeTypes.EU15_ORIGIN_COUNTRY_LIST), () => ParseXML(new GenericOriginCountryCodesEftaEU15Transformer(importExportDownloadLinksListXML)) },
				{ nameof(CodeListsConstants.Generic.CodeTypes.EX15_ORIGIN_COUNTRY_LIST), () => ParseXML(new ExportCountryCodesCommunityEX15Transformer(exportDownloadLinksList)) },
				{ nameof(CodeListsConstants.Generic.CodeTypes.EX17_DESTINATION_COUNTRY_LIST), () => ParseXML(new ExportCountryEligibleForExportEX17Transformer(exportDownloadLinksList)) },
				{ nameof(CodeListsConstants.Generic.CodeTypes.IM15_ORIGIN_COUNTRY_LIST), () => ParseXML(new ImportCountryListIM15Transformer(importDownloadLinksList)) },
				{ nameof(CodeListsConstants.Generic.CodeTypes.IM17_DESTINATION_COUNTRY_LIST), () => ParseXML(new ImportCountryCodesCommunityIM17Transformer(exportDownloadLinksList)) },
				{ nameof(CodeListsConstants.Import.CodeTypes.IMPORT_A2055_ECONOMIC_CONDITIONS), () => ParseXML(new ImportEconomicConditionsA2055Transformer(importDownloadLinksList)) },
				{ nameof(CodeListsConstants.Import.CodeTypes.IMPORT_MOP_METHOD_OF_PAYMENT), () => ParseXML(new ImportMethodOfPaymentMOPTransformer(importDownloadLinksList)) },
				{ nameof(CodeListsConstants.Import.CodeTypes.IMPORT_TRNAT_TRANSACTION_NATURE), () => ParseXML(new ImportNatureOfTransactionTRNATTransformer(importDownloadLinksList)) },
				{ nameof(CodeListsConstants.Import.CodeTypes.IMPORT_DC44I_TARIC_CODES_AND_CERTIFICATES), () => ParseXML(new ImportTaricCodesAndCertificatesDC44ITransformer(importDownloadLinksList)) },
				{ nameof(CodeListsConstants.Import.CodeTypes.IMPORT_EUIAT_DEPARTURE_AIRPORTS), () => ParseXML(new ImportDepartureAirportsEUIATTransformer(importDownloadLinksList)) },
				{ nameof(CodeListsConstants.Import.CodeTypes.IMPORT_CUSUQ_CUSTOMS_DECLARATION_UNITS_OF_QUANTITY), () => ParseXML(new ImportTaricUnitsOfQuantityCUSUQTransformer(importDownloadLinksList)) },
				{ nameof(CodeListsConstants.Import.CodeTypes.IMPORT_C0754_TRANSPORT_DOCUMENT_TYPES), () => ParseXML(new ImportTransportDocumentTypesC0754Transformer(importDownloadLinksList)) },
				{ nameof(CodeListsConstants.Export.CodeTypes.EXPORT_I0139_PRODUCT_NUMBERS_OILS_AND_GASES), () => ParseXML(new ExportProductNumbersOilsAndGasesI0139Transformer(exportDownloadLinksList)) },
				{ nameof(CodeListsConstants.Export.CodeTypes.EXPORT_I0809_COUNTRY_LIST_REIMPORT), () => ParseXML(new ExportCountryListReImportI0809Transformer(exportDownloadLinksList)) },
				{ nameof(CodeListsConstants.Export.CodeTypes.EXPORT_I0810_COUNTRY_LIST_SPECIFIC_SANCTIONS), () => ParseXML(new ExportCountryListSpecificSanctionsI0810Transformer(exportDownloadLinksList)) },
				{ nameof(CodeListsConstants.Export.CodeTypes.EXPORT_I0812_COUNTRY_LIST_CO_REEXPORT), () => ParseXML(new ExportCountryListCoReExportI0812Transformer(exportDownloadLinksList)) },
				{ nameof(CodeListsConstants.Export.CodeTypes.EXPORT_DC44E_DOCUMENTS), () => ParseXML(new ExportDocumentsDC44ETransformer(exportDownloadLinksList)) },
				{ nameof(CodeListsConstants.Export.CodeTypes.EXPORT_CURRE_CURRENCY), () => ParseXML(new ExportCurrencyCURRETransformer(exportDownloadLinksList)) },
				{ nameof(CodeListsConstants.Export.CodeTypes.EXPORT_TD44E_TRANSPORT_DOCUMENT), () => ParseXML(new ExportTransportDocumentsTD44ETransformer(exportDownloadLinksList)) },
				{ nameof(CodeListsConstants.Export.CodeTypes.EXPORT_DC40E_PREVIOUS_DOCUMENTS), () => ParseXML(new ExportPreviousDocumentsDC40ETransformer(exportDownloadLinksList)) },
				{ nameof(CodeListsConstants.Export.CodeTypes.EXPORT_AR44E_ADDITIONAL_REFERENCES), () => ParseXML(new ExportAdditionalReferencesAR44ETransformer(exportDownloadLinksList)) },
				{ nameof(CodeListsConstants.Export.CodeTypes.EXPORT_AI44E_ADDITIONAL_INFORMATION), () => ParseXML(new ExportAdditionalInformationAI44ETransformer(exportDownloadLinksList)) },
				{ nameof(CodeListsConstants.Export.CodeTypes.EXPORT_AI44X_ADDITIONAL_INFORMATION), () => ParseXML(new ExportAdditionalInformationOnExitAI44XTransformer(exportDownloadLinksList)) },
				{ nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_NC008_COUNTRY_CODES_FULL_LIST), () => ParseXML(new NctsCountryCodesFullListNC008Transformer(nctsDownloadLinksList)) },
				{ nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_C0009_COUNTRY_CODES_COMMON_TRANSIT), () => ParseXML(new NctsCountryCodesCommonTransitC0009Transformer(nctsDownloadLinksList)) },
				{ nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_NC010_COUNTRY_CODES_COMMUNITY), () => ParseXML(new NctsCountryCodesCommunityNC010Transformer(nctsDownloadLinksList)) },
				{ nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_DC44N_SUPPORTING_DOCUMENTS), () => ParseXML(new NctsDocumentsDC44NTransformer(nctsDownloadLinksList)) },
				{ nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_DC40N_PREVIOUS_DOCUMENTS), () => ParseXML(new NctsPreviousDocumentsDC40NTransformer(nctsDownloadLinksList)) },
				{ nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_TD44N_TRANSPORT_DOCUMENTS), () => ParseXML(new NctsTransportDocumentsTD44NTransformer(nctsDownloadLinksList)) },
				{ nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_AR44N_ADDITIONAL_REFERENCES), () => ParseXML(new NctsAdditionalReferencesAR44NTransformer(nctsDownloadLinksList)) },
				{ nameof(CodeListsConstants.Ncts.CodeTypes.NCTS_AI44N_ADDITIONAL_INFORMATION), () => ParseXML(new NctsAdditionalInformationAI44NTransformer(nctsDownloadLinksList)) },
				{ nameof(CodeListsConstants.Emcs.CodeTypes.EMCS_EMCMS_MEMBER_STATES), () => ParseTSV(new EmcsMemberStatesEMCMSTransformer(emcsDownloadLinks)) },
				{ nameof(CodeListsConstants.Emcs.CodeTypes.EMCS_EMCPK_PACK_TYPES), () => ParseTSV(new EmcsPackTypesEMCPKTransformer(emcsDownloadLinks)) },
				{ nameof(CodeListsConstants.Emcs.CodeTypes.EMCS_EPC_PRODUCT_CODES), () => ParseTSV(new EmcsProductCodesEPCTransformer(emcsDownloadLinks)) },
				{ nameof(CodeListsConstants.Emcs.CodeTypes.EMCS_EMCCN_CN_CODES), () => ParseTSV(new EmcsCnCodesEMCCNTransformer(emcsDownloadLinks)) }
			};

			void ParseXML<T, K>(ManyToOneCodeListsParserXML<T, K> parser) where T : RefDataRepoModelEntityType where K : IKeyValues
			{
				Program.PrintErrorMessage(parser.DownloadAndConvertToRefCusCodeListXML(httpClient, outputFilePath).Result);
			}

			void ParseTSV<T, K>(CodeListsParserTSV<T, K> parser) where T : RefDataRepoModelEntityType where K : IKeyValues
			{
				Program.PrintErrorMessage(parser.DownloadAndConvertToRefCusCodeListXML(httpClient, outputFilePath, new DateTimeProvider()));
			}
		}

		static (IEnumerable<string> FunctionsToExclude, bool ValidArguments) ValidateArguments(string[] args)
		{
			var validArguments = true;
			var codeListsToExclude = Enumerable.Empty<string>();
			var validCodeLists = CodeListsConstants.ValidCodeLists;
			var numberOfArguments = args.Length;

			if (numberOfArguments == 1 && args[0].StartsWith("-EXCLUDE:", StringComparison.InvariantCultureIgnoreCase))
			{
				codeListsToExclude = args[0].Remove(0, 9).Split(',');
				var invalidCodeListsToExclude = codeListsToExclude.Where(function => !validCodeLists.Contains(function));
				if (invalidCodeListsToExclude.Any())
				{
					Console.Error.WriteLine($"Invalid CodeList Types to exclude entered: {string.Join(",", invalidCodeListsToExclude)}");
					validArguments = false;
				}
			}
			else if (numberOfArguments > 0)
			{
				Console.Error.WriteLine($"Invalid arguments entered -EXCLUDE:[functions]. Valid Code List Types are {string.Join(",", validCodeLists)}");
				validArguments = false;
			}

			return (codeListsToExclude, validArguments);
		}
	}
}
