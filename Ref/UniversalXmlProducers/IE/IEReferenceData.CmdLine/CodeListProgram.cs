using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Business;
using CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services;
using CargoWise.RefDbRepo.IEReferenceData.Services;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.CmdLine
{
	public static class CodeListProgram
	{
		public static void Run(string[] args, string outputFilePath, (ApplicationType applicationType, string downloadUrl)[] downloadData)
		{
			var (functionsToExclude, continueProcessing) = ValidateArguments(args);
			if (continueProcessing)
			{
				var functionsToRun = GetFunctionsToRun(outputFilePath, downloadData).ToHashSet();
				if (functionsToExclude.Any())
				{
					foreach (var functionToExclude in functionsToExclude)
					{
						functionsToRun.RemoveWhere(functionToRun => functionToRun.Key.Code == functionToExclude);
					}
				}

				Parallel.Invoke(functionsToRun.Select(x => x.Value).ToArray());
			}
		}

		static IRevenueCodeListDetails[] GetAllCodeLists() => new IRevenueCodeListDetails[]
		{
			new Services.AIS.AdditionalDeclarationTypesDetails(),
			new Services.AIS.AdditionalProcedureDetails(),
			new Services.AIS.AdditionalReference(),
			new Services.AIS.AdditionalReferenceNational(),
			new Services.AIS.AdditionalInformation(),
			new Services.AIS.AuthorisationCodeTypesDetails(),
			new Services.AIS.ErrorTypesDetails(),
			new Services.AIS.LocationIdentificationQualifiersDetails(),
			new Services.AIS.LegalBasisTypeDetails(),
			new Services.AIS.LocationOfGoodsDetails(),
			new Services.AIS.LocationTypesDetails(),
			new Services.AIS.NatureOfTransactionsDetails(),
			new Services.AIS.PreviousDocumentTypesDetails(),
			new Services.AIS.SupportingDocumentTypesDetails(),
			new Services.AIS.TransportDocumentTypeDetails(),
			new Services.AIS.UnitsOfMeasurementDetails(),
			new Services.AIS.CountryCodeDetails(),

			new Services.AISUCC5.AdditionalDeclarationTypesDetails(),
			new Services.AISUCC5.AdditionalFiscalRefRoleCodeDetails(),
			new Services.AISUCC5.AdditionalProcedureDetails(),
			new Services.AISUCC5.AdditionalInformation(),
			new Services.AISUCC5.AuthorisationCodeTypesDetails(),
			new Services.AISUCC5.ControlResultDetails(),
			new Services.AISUCC5.ErrorTypesDetails(),
			new Services.AISUCC5.KindOfPackagesDetails(),
			new Services.AISUCC5.LegalBasisTypeDetails(),
			new Services.AISUCC5.LocationIdentificationQualifiersDetails(),
			new Services.AISUCC5.LocationOfGoodsDetails(),
			new Services.AISUCC5.LocationTypesDetails(),
			new Services.AISUCC5.NatureOfTransactionsDetails(),
			new Services.AISUCC5.PreviousDocumentTypesDetails(),
			new Services.AISUCC5.SupportingDocumentTypesDetails(),
			new Services.AISUCC5.TransportDocumentTypeDetails(),
			new Services.AISUCC5.UnitsOfMeasurementDetails(),

			new Services.AES.AdditionalProcedureDetails(),
			new Services.AES.AdditionalReference(),
			new Services.AES.AdditionalReferenceNational(),
			new Services.AES.AddtionalInformation(),
			new Services.AES.BusinessRejectionType(),
			new Services.AES.CalculationOfTaxes(),
			new Services.AES.CountryCodesCommonTransitOutsideCommunity(),
			new Services.AES.CountryCodesCountryRegimeOTH(),
			new Services.AES.DestinationCountry(),
			new Services.AES.DiversionRejectionCode(),
			new Services.AES.ExitControlResultCode(),
			new Services.AES.FunctionalErrorCode(),
			new Services.AES.MeasurementUnitAndQualifier(),
			new Services.AES.NotificationType(),
			new Services.AES.PreviousDocumentType(),
			new Services.AES.RejectionReasonType(),
			new Services.AES.SpecificCircumstanceIndicator(),
			new Services.AES.SupportingDocument(),
			new Services.AES.TransportCharges(),
			new Services.AES.TransportDocument(),
			new Services.AES.TypeOfAlternativeEvidence(),
			new Services.AES.TypeOfControls(),

			new Services.NCTS.AdditionalInformation(),
			new Services.NCTS.AdditionalReference(),
			new Services.NCTS.AuthorisationTypeDeparture(),
			new Services.NCTS.AuthorisationTypeDestination(),
			new Services.NCTS.ControlType(),
			new Services.NCTS.CountryCodeFullList(),
			new Services.NCTS.DeclarationTypeAdditional(),
			new Services.NCTS.DocumentType(),
			new Services.NCTS.FunctionalErrorCode(),
			new Services.NCTS.GuaranteeType(),
			new Services.NCTS.IncidentCode(),
			new Services.NCTS.NotificationType(),
			new Services.NCTS.PreviousDocument(),
			new Services.NCTS.PreviousDocumentExport(),
			new Services.NCTS.PreviousDocumentExcise(),
			new Services.NCTS.QualifierOfIdentificationIncident(),
			new Services.NCTS.QueryIdentifier(),
			new Services.NCTS.RejectionCodeDepartureExport(),
			new Services.NCTS.RejectionCodeDestinationExit(),
			new Services.NCTS.SpecificCircumstanceIndicatorCode(),
			new Services.NCTS.SupportingDocumentType(),
			new Services.NCTS.TransportChargesMethodOfPayment(),
			new Services.NCTS.UNDangerousGoodsCode(),
			new Services.NCTS.XmlErrorCodesCode(),
		};

		public static Dictionary<(string DataGrouping, string Code), Action> GetFunctionsToRun(string outputFilePath, (ApplicationType applicationType, string downloadUrl)[] downloadData, bool forceLoadFromFilePath = false)
			=> GetFunctionsToRun(outputFilePath, GetAllCodeLists(), downloadData, forceLoadFromFilePath);

		public static Dictionary<(string DataGrouping, string Code), Action> GetFunctionsToRun(string outputFilePath, IRevenueCodeListDetails[] codeList, (ApplicationType applicationType, string downloadUrl)[] downloadData, bool forceLoadFromFilePath = false)
		{
			var downloader = new DownloadCodeLists(codeList);
			var (errors, extractedCodeLists) = downloader.Download(downloadData, forceLoadFromFilePath);

			if (string.IsNullOrEmpty(errors))
			{
				var result = extractedCodeLists.ToDictionary(x => x.Key, x => GetAction(x, outputFilePath, UCC5RefCusCodeTypesProcessor.GetDependingOn(x.Value)));
				new UCC5RefCusCodeTypesProcessor().Process(
					extractedCodeLists.Select(item => (item.Key.Code, item.Key.DataGrouping, item.Value)),
					codeList,
					outputFilePath
				);
				return result;
			}
			else
			{
				Console.Error.WriteLine(errors);
				return new Dictionary<(string, string), Action>();
			}

			Action GetAction(KeyValuePair<(string DataGrouping, string Code), IRevenueExtractedCodeList> extractedCodeList, string path, string dependingOn = null)
			{
				return () => ProduceXml(extractedCodeList.Key.Code, extractedCodeList.Value.CodeList, extractedCodeList.Value.VersionDate, extractedCodeList.Value.UpdateType, path, extractedCodeList.Value.DataGrouping, ((ICodeListAttribute)extractedCodeList.Value)?.IsCodeListAttributeNeeded ?? false, dependingOn);
			}

			void ProduceXml(string codeType, IEnumerable<IRevenueCodeDescriptionPair> codeDescriptionList, DateTime publicationDateTime, UpdateType updateType, string path, string dataGrouping, bool isCodeListAttributeNeeded, string dependingOn = null)
			{
				var producer = new RevenueRefCusCodeListProducer(codeType);
				Console.Error.WriteLine(producer.ConvertCodeListToXml(codeDescriptionList, publicationDateTime, updateType, path, dataGrouping, dependingOn, isCodeListAttributeNeeded));
			}
		}

		public static (IEnumerable<string> FunctionsToExclude, bool ValidArguments) ValidateArguments(string[] args)
		{
			var validArguments = true;
			var codeListsToExclude = Enumerable.Empty<string>();
			var validCodeLists = Constants.CodeListConstants.ValidCodeLists;
			var numberOfArguments = args.Length;

			if (numberOfArguments == 1 && args[0].ToUpperInvariant().StartsWith("-EXCLUDE:", StringComparison.Ordinal))
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
