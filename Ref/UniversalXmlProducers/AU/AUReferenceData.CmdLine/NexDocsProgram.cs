using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.AUReferenceData.Business;
using CargoWise.RefDbRepo.AUReferenceData.Services;

namespace CargoWise.RefDbRepo.AUReferenceData.CmdLine
{
	class NexDocsProgram
	{
		public static void Run(string[] args, string outputPath)
		{
			var (functionsToExclude, lastUpdated, continueProcessing) = ValidateArguments(args);
			if (continueProcessing)
			{
				var dateTimeProvider = new DateTimeProvider();
				var commodityCodeSetList = GetListCodeSets<CsvToCommodityTypeSetConverter>(NexDocConstants.CodeListTypes.ECM_COMMODITY_TYPE, lastUpdated).ToArray();
				var functionsToRun = new Dictionary<string, Func<string, INexDocCodeParser>>
				{
					{ NexDocConstants.CodeListTypes.COMMODITY_CUSCODELIST_ATTRIBUTE_NAME, (codeSetName) => new CommodityCusCodeListAttributeNameParser(commodityCodeSetList) },
					{ NexDocConstants.CodeListTypes.COMMODITY_CUSCODETYPE, (codeSetName) => new CommodityCusCodeTypeParser(commodityCodeSetList) },
					{ NexDocConstants.CodeListTypes.ECM_ADD_TEXT_FORMAT, (codeSetName) => new AdditionalTextFormatParser(GetListCodeSets<CsvToAdditionalTextSetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_ATTACHMENT_TYPE, (codeSetName) => new AttachmentTypeParser(GetListCodeSets<CsvToAttachmentTypeSetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_CN_CODE, (codeSetName) => new CombinedNomenclatureParser(GetListCodeSets<CsvToCombinedNomenclatureSetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_DECLARATION, (codeSetName) => new DeclarationStatementsParser(GetListCodeSets<CsvToDeclarationStatmentSetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_DOCUMENT_TYPE, (codeSetName) => new DocumentTypeParser(GetListCodeSets<CsvToDocumentTypeSetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_NATURE_OF_COMMODITY, (codeSetName) => new NatureOfCommodityParser(GetListCodeSets<CsvToNatureOfCommoditySetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_PACK_TYPE, (codeSetName) => new PackTypeParser(GetListCodeSets<CsvToPackTypeSetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_PACKAGE_TYPE, (codeSetName) => new PackageTypeParser(GetListCodeSets<CsvToPackageTypeSetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_PERMIT_TYPE, (codeSetName) => new PermitTypeParser(GetListCodeSets<CsvToPermitTypeSetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_PRESERVATION_TYPE, (codeSetName) => new PreservationTypeParser(GetListCodeSets<CsvToPreservationTypeSetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_PRINT_REGION, (codeSetName) => new PrintRegionParser(GetListCodeSets<CsvToPrintRegionSetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_PROCESS_CATEGORY, (codeSetName) => new ProcessCategoryParser(GetListCodeSets<CsvToProcessCategorySetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_PRODUCT_CATEGORY, (codeSetName) => new ProductCategoryParser(GetListCodeSets<CsvToProductCategorySetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_PRODUCT_CATEGORY_AHECC, (codeSetName) => new ProductCategoryAHECCParser(GetListCodeSets<CsvToProductCategoryAHECCSetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_PRODUCT_TYPE, (codeSetName) => new ProductTypeParser(GetListCodeSets<CsvToProductTypeSetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_SUPPLEMENTARY_CODE, (codeSetName) => new SupplementaryCodeParser(GetListCodeSets<CsvToSupplementaryCodeSetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_TREATMENT, (codeSetName) => new TreatmentParser(GetListCodeSets<CsvToTreatmentSetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_UNIT_OF_MEASUREMENT, (codeSetName) => new UnitOfMeasurementParser(GetListCodeSets<CsvToUnitOfMeasurementSetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_CODES, (codeSetName) => new NexDocCodesParser(GetListCodeSets<CsvToCodesSetConverter>(codeSetName, lastUpdated)) },
					{ NexDocConstants.CodeListTypes.ECM_ESTABLISHMENT_INDICATOR, (codeSetName) => new EstablishmentIndicatorParser(GetListCodeSets<CsvToEstablishmentIndicatorSetConverter>(codeSetName, lastUpdated)) }
				};

				if (functionsToExclude.Any())
				{
					foreach (var functionToExclude in functionsToExclude)
					{
						functionsToRun.Remove(functionToExclude);
					}
				}

				Parallel.Invoke(functionsToRun.Select(x => new Action(() => DownloadAndConvertCodesToXMLFile(x.Key, x.Value(x.Key), outputPath, dateTimeProvider))).ToArray());
			}
		}

		static IEnumerable<IListCodeSet> GetListCodeSets<T>(string codeSetName, DateTime lastUpdated)
			where T : CsvToItemCodeSetsConverter
		{
			IEnumerable<IListCodeSet> result = Enumerable.Empty<IListCodeSet>();

			var service = new NexDocCodeSetRESTService<T>(lastUpdated);
			var retryCount = 1;

			int maxAttempts = int.TryParse(ApplicationConfig.NexDocRESTReferenceDataMaxAttempts, out maxAttempts) ? maxAttempts : 1;
			int retryDelay = int.TryParse(ApplicationConfig.NexDocRESTReferenceDataRetryDelay, out retryDelay) ? retryDelay : (int)TimeSpan.FromSeconds(10).TotalMilliseconds;

			while (retryCount <= maxAttempts)
			{
				result = service.GetCodeSet(codeSetName);

				if (service.HasErrorNotification)
				{
					var errorMessage = $"[{retryCount}]Downloading {codeSetName} has encountered the following errors, {service.GetErrorNotification()}. It will run again in {retryDelay / 1000} seconds.\r\n";
					Console.WriteLine(errorMessage);
					retryCount++;

					Thread.Sleep(retryDelay);
				}
				else
				{
					Console.WriteLine($"Downloaded {codeSetName}.");
					break;
				}
			}

			if (retryCount > maxAttempts)
			{
				var errorMessage = $"Failed downloading {codeSetName} after {maxAttempts} attempts. It has encountered the following errors: {service.GetErrorNotification()}.";
				Console.Error.WriteLine(errorMessage);
			}

			return result;
		}

		static void DownloadAndConvertCodesToXMLFile(string codeSetName, INexDocCodeParser codeParser, string outputPath, DateTimeProvider dateTimeProvider)
		{
			var errorResult = codeParser.ConvertCodesToXMLFile(outputPath, dateTimeProvider);
			if (!string.IsNullOrEmpty(errorResult))
			{
				Console.WriteLine($"Downloading {codeSetName} completed with the following errors:\r\n{errorResult}");
			}
		}

		static (IEnumerable<string> FunctionsToExclude, DateTime LastUpdated, bool ValidArguments) ValidateArguments(string[] args)
		{
			var validArguments = true;
			var codeListsToExclude = Enumerable.Empty<string>();
			var lastUpdated = DateTime.MinValue;
			var validCodeLists = new HashSet<string>
			{
				NexDocConstants.CodeListTypes.COMMODITY_CUSCODELIST_ATTRIBUTE_NAME, NexDocConstants.CodeListTypes.COMMODITY_CUSCODETYPE, NexDocConstants.CodeListTypes.ECM_ADD_TEXT_FORMAT, NexDocConstants.CodeListTypes.ECM_ATTACHMENT_TYPE, NexDocConstants.CodeListTypes.ECM_CN_CODE,
				NexDocConstants.CodeListTypes.ECM_DECLARATION, NexDocConstants.CodeListTypes.ECM_DOCUMENT_TYPE, NexDocConstants.CodeListTypes.ECM_NATURE_OF_COMMODITY, NexDocConstants.CodeListTypes.ECM_PACK_TYPE, NexDocConstants.CodeListTypes.ECM_PACKAGE_TYPE,
				NexDocConstants.CodeListTypes.ECM_PERMIT_TYPE, NexDocConstants.CodeListTypes.ECM_PRESERVATION_TYPE, NexDocConstants.CodeListTypes.ECM_PRINT_REGION, NexDocConstants.CodeListTypes.ECM_PROCESS_CATEGORY, NexDocConstants.CodeListTypes.ECM_PRODUCT_CATEGORY,
				NexDocConstants.CodeListTypes.ECM_PRODUCT_CATEGORY_AHECC, NexDocConstants.CodeListTypes.ECM_PRODUCT_TYPE, NexDocConstants.CodeListTypes.ECM_SUPPLEMENTARY_CODE, NexDocConstants.CodeListTypes.ECM_TREATMENT, NexDocConstants.CodeListTypes.ECM_UNIT_OF_MEASUREMENT, NexDocConstants.CodeListTypes.ECM_CODES,
				NexDocConstants.CodeListTypes.ECM_ESTABLISHMENT_INDICATOR
			};

			args = args.Select(x => x.ToUpperInvariant()).ToArray();
			var numberOfArguements = args.Length;
			if (numberOfArguements > 0)
			{
				var excludeArgument = args.FirstOrDefault(x => x.StartsWith("-EXCLUDE:", StringComparison.InvariantCulture));
				var lastUpdatedArgument = args.FirstOrDefault(x => x.StartsWith("-LASTUPDATE=", StringComparison.InvariantCulture));
				var validArgumentCount = 0;
				if (excludeArgument != null)
				{
					validArgumentCount++;
					codeListsToExclude = excludeArgument.Remove(0, 9).Split(',');
					var invalidCodeListsToExclude = codeListsToExclude.Where(function => !validCodeLists.Contains(function));
					if (invalidCodeListsToExclude.Any())
					{
						Console.Error.WriteLine($"Invalid Code List Types to exclude entered: {string.Join(",", invalidCodeListsToExclude)}");
						validArguments = false;
					}
				}

				if (lastUpdatedArgument != null)
				{
					validArgumentCount++;
					var lastUpdatedAsString = lastUpdatedArgument.Substring(12);
					const string DateFormat = "yyyy-MM-dd";
					if (string.IsNullOrEmpty(lastUpdatedAsString) || !DateTime.TryParseExact(lastUpdatedAsString, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out lastUpdated))
					{
						Console.Error.WriteLine($"Invalid LastUpdated argument format. Valid argument should be in this format: -LASTUPDATE=yyyy-MM-dd");
						validArguments = false;
					}
				}

				if (validArguments && numberOfArguements != validArgumentCount)
				{
					Console.Error.WriteLine($"Invalid arguments entered; valid arguments are:\r\n-LASTUPDATE=yyyy-MM-dd\r\n-EXCLUDE:[functions]. Valid Code List Types are\r\n{string.Join("\r\n", validCodeLists)}");
					validArguments = false;
				}
			}

			return (codeListsToExclude, lastUpdated, validArguments);
		}
	}
}
