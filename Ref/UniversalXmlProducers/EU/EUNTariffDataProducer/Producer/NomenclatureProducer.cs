using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common.CompositeKey;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class NomenclatureProducer : BaseProducer, INomenclatureProducer
	{
		readonly INomenclatureParser nomenclatureParser;
		readonly IDeclarableCodeParser declarableCodeParser;
		readonly IXmlProducer<RefCusNomenclatureGroup> xmlProducer;
		protected internal const string nomenclatureLinkRegex = @"nomenclature[ ]+([A-Z]{2}).*[.]?(.xlsx|.xls)";
		protected internal const string nomenclatureMatchingFilenameRegex = @"nomenclature[ ]+([A-Z]{2}).*[.]?(xlsx|xls)?";

		public NomenclatureProducer()
		{
			nomenclatureParser = new NomenclatureParser();
			declarableCodeParser = new DeclarableCodeParser();
			xmlProducer = new NomenclatureXmlProducer();
		}

		public override IEnumerable<IWebFileInfo> LocateWebFiles()
		{
			IEnumerable<IWebFileInfo> webFileInfos;
			var searchPatternRegex = new Regex(nomenclatureLinkRegex, RegexOptions.IgnoreCase);

			if (ApplicationConfig.SkipNomenclatureDownloadIfExists)
			{
				return ApplicationConfig.GetExistingFileIfExists(ApplicationConfig.DownloadsFolder, FilesToLocate, searchPatternRegex);
			}

			using (var pageNavigator = GetNewWebDriverHelper())
			{
				var webFileLocator = GetNewWebFileLocator(pageNavigator, FilesToLocate);
				var basePage = EUNLibraryBasePage;

				webFileInfos = webFileLocator
					.GetLocationOfLatestFiles(basePage, takeLatestOfDuplicatedFiles: true)
					.Concat(webFileLocator.GetLocationOfSpecificFiles(basePage, new string[] { }, searchPatternRegex))
					.ToList();

				var additionalLanguages = webFileLocator.GetLocationOfSpecificFiles(basePage, new[] { "07 - July", "01 - January" }, searchPatternRegex).ToList();

				Func<IWebFileInfo, string> extractLanguageCode = webFileInfo => new Regex(nomenclatureMatchingFilenameRegex, RegexOptions.IgnoreCase).Match(webFileInfo.FileName).Groups[1].Value;
				Action<IWebFileInfo> removeEarlierVersionDuplicates = webFileInfo =>
				{
					additionalLanguages.RemoveAll(x => extractLanguageCode(x) == extractLanguageCode(webFileInfo));
				};
				Array.ForEach(webFileInfos.ToArray(), removeEarlierVersionDuplicates);
				webFileInfos = webFileInfos.Concat(additionalLanguages);
			}

			return webFileInfos;
		}

		public ICompositeKeyGeneratorResult Run(bool produceXml = true)
		{
			var files = LocateWebFiles().ToList();
			var error = files.FirstOrDefault(x => x.Exception != null);
			if (error != null)
			{
				xmlProducer.InitializeWriter(DateTime.Today);
				var errorMessage = error.Exception.Message;
				xmlProducer.ReportDataSourceError(errorMessage);
				throw error.Exception;
			}

			var filesDownloadedSuccessfully = DownloadFiles(files);
			if (!filesDownloadedSuccessfully)
			{
				throw new InvalidOperationException("Failure downloading files.");
			}

			var result = ParseNomenclatureData(files);

			if (produceXml)
			{
				xmlProducer.InitializeWriter(PublishTime);
				xmlProducer.ExportToXml(result.NomenclatureGroups);
			}

			return result;
		}

		public virtual ICompositeKeyGeneratorResult ParseNomenclatureData(IEnumerable<IWebFileInfo> files)
		{
			var parsedRecords = GetNomenclatureRecords(files);
			var treeGenerator = GetNewCompositeKeyTreeGenerator();
			var rootNode = treeGenerator.GenerateTree(parsedRecords);

			var compositeKeyGenerator = new CompositeKeyGenerator();
			return compositeKeyGenerator.GenerateCompositeKeys(rootNode);
		}

		protected virtual ICompositeKeyTreeGenerator GetNewCompositeKeyTreeGenerator() => new CompositeKeyTreeGenerator(GetNewChapterToSectionMapper());

		protected virtual IChapterToSectionMapper GetNewChapterToSectionMapper() => new ChapterToSectionMapper();

		protected virtual IEnumerable<INomenclatureRecord> GetNomenclatureRecords(IEnumerable<IWebFileInfo> webFileInfos)
		{
			Argument.NotNull(webFileInfos, nameof(webFileInfos));

			var rawNomenclatureRecords = new List<IRawNomenclatureRecord>();
			var rawDeclarableCodeRecords = new List<IRawDeclarableCodeRecord>();

			foreach (var webFileInfo in webFileInfos)
			{
				var filePath = Path.Combine(ApplicationConfig.DownloadsFolder, webFileInfo.FileName);

				if (webFileInfo.FileName.ToUpperInvariant().Contains("NOMENCLATURE"))
				{
					rawNomenclatureRecords.AddRange(nomenclatureParser.Parse(filePath));
				}
				else if (webFileInfo.FileName.ToUpperInvariant().Contains("DECLARABLE"))
				{
					rawDeclarableCodeRecords.AddRange(declarableCodeParser.Parse(filePath));
				}
			}

			var (rawNomenclaturesRecords, rawDeclarableRecords) = FinalizeNomenclatureAndDeclarableFileParsing(rawNomenclatureRecords, rawDeclarableCodeRecords);

			var rawNomenclatureRecordGroupedByTariffDictionary = rawNomenclaturesRecords
				.GroupBy(x => x.TariffHeader)
				.ToDictionary(x => x.Key, x => x.ToList());
			var rawDeclarableCodeRecordDictionary = rawDeclarableRecords.ToDictionary(x => x.TariffHeader, x => x);

			return GenerateNomenclatureRecords(rawNomenclatureRecordGroupedByTariffDictionary, rawDeclarableCodeRecordDictionary);
		}

		protected virtual (List<IRawNomenclatureRecord> nomenclatures, List<IRawDeclarableCodeRecord> declarables) FinalizeNomenclatureAndDeclarableFileParsing(
			List<IRawNomenclatureRecord> rawNomenclatureRecords,
			List<IRawDeclarableCodeRecord> rawDeclarableCodeRecords)
		{
			return (rawNomenclatureRecords, rawDeclarableCodeRecords);
		}

		protected static List<INomenclatureRecord> GenerateNomenclatureRecords(Dictionary<string, List<IRawNomenclatureRecord>> rawNomenclature, Dictionary<string, IRawDeclarableCodeRecord> rawDeclarableCodes)
		{
			var result = new List<INomenclatureRecord>();
			foreach (var tariff in rawNomenclature)
			{
				var englishNomenclature = tariff.Value.FirstOrDefault(x => x.Language == "EN");
				if (englishNomenclature != null)
				{
					rawDeclarableCodes.TryGetValue(englishNomenclature.TariffHeader, out var rawDeclarableCodeRecord);
					var languages = GetAdditionalLanguages(tariff.Value.Where(x => x.Language != "EN"));

					result.Add(new NomenclatureRecord(
					englishNomenclature.TariffHeader,
					rawDeclarableCodeRecord?.DeclarableStartDate ?? englishNomenclature.StartDate,
					rawDeclarableCodeRecord?.EndDate ?? englishNomenclature.EndDate,
					englishNomenclature.HierarchyPosition,
					englishNomenclature.Level,
					englishNomenclature.Description,
					languages,
					rawDeclarableCodeRecord?.IsLeaf ?? false));
				}
			}
			return result;
		}

		static List<(string language, string description)> GetAdditionalLanguages(IEnumerable<IRawNomenclatureRecord> additionalLanguages)
		{
			Argument.NotNull(additionalLanguages, nameof(additionalLanguages));
			var result = new List<(string language, string description)>();
			foreach (var languageRecord in additionalLanguages)
			{
				result.Add((languageRecord.Language, languageRecord.Description));
			}
			return result;
		}

		public override IEnumerable<string> FilesToLocate => new[]
		{
			"Nomenclature EN",
			"Declarable codes"
		};

		public override string EUNLibraryBasePage => ApplicationConfig.EUNomenclatureBaseUrl;
	}
}
