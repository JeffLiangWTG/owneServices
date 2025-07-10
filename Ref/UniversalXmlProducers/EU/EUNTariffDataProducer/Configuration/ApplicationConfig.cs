using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

[assembly: InternalsVisibleTo("CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.Test")]

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public static class ApplicationConfig
	{
		const string JsonConfigFile = "CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer.config.json";
		static IConfiguration configuration;

		public static IConfiguration Config
		{
			get
			{
				if (configuration == null)
				{
					configuration = new ConfigurationBuilder().AddJsonFile(JsonConfigFile).Build();
				}
				return configuration;
			}
		}

		public static class ProgramFunctions
		{
			public const string Nomenclature = "NOMENCLATURE";
			public const string NomenclaturePlusDaily = "NOMENCLATURE_PLUS_DAILY";
			public const string TradeGroup = "TRADEGROUP";
			public const string Tariff = "TARIFF";
			public const string Daily = "DAILY";
			public const string AdditionalCodes = "ADDITIONALCODES";
		}

		public static string DailyZipFileName => "DailyTariff.zip";
		public static string DailyFileName => "Measures.xlsx";

		public static string MonthlyTariffDataSource => "EUN Monthly Tariffs";
		public static string SEAndDailyTariffDataSource => "EUN SE & Taric Daily Tariffs";
		public static string MonthlyTariffUXmlFile { get; private set; }
		public static string SEAndDailyTariffUXmlFile { get; private set; }
		public static string DailyTariffDownloadUrl { get; private set; }

		public static string NomenclatureDataSource => "EUN Nomenclature Groups";
		public static string ImportTariffCompositeKeyDataSource => "Import EUN Tariff CompositeKeys";
		public static string ImportTariffDataSource => "Import EUN Tariffs";
		public static string ExportTariffDataSource => "Export EUN Tariffs";
		public static string TradeGroupDataSource => "EUN Trade Groups";
		public static string AdditionalCodesDataSource => "EU Additional Codes";
		public static string DataGrouping => "EUN";
		public static string A00RateCode => "A00";
		public static string A20RateCode => "A20";
		public static string A30RateCode => "A30";
		public static string A35RateCode => "A35";
		public static string A40RateCode => "A40";
		public static string A45RateCode => "A45";
		public static string DutyRateType => "DTY";
		public static string ImportTariffType => "IMP";
		public static string MeasureConditionSource => "EU Taric";
		public static string NotApplicableForNoOtherCondition => "NA";
		public static string NoOtherConditionAppliedComment => "No other condition applies";
		public static string STypeCertificateConditionMeasureRateCode => "SEC";
		public static string EUTariffBaseUrl { get; private set; }
		public static string TariffCodeDetailsUrl { get; private set; }
		public static string EUReferenceDataBaseUrl { get; private set; }
		public static string ExportTariffsDownloadURL { get; private set; }
		public static string ExportTariffsUXmlFile { get; private set; }
		public static string DownloadsFolder { get; private set; }
		public static int DownloadTimeoutInSeconds { get; private set; }
		public static DateTime PublishDate { get; private set; }
		public static string ImportTariffUXmlFile { get; private set; }
		public static string ExportTariffUXmlFile { get; private set; }
		public static string ImportTariffCompositeKeyUXmlFile { get; private set; }
		public static string AdditionalCodesUXmlFile { get; private set; }
		public static string NomenclatureUXmlFile { get; private set; }
		public static string TradeGroupUXmlFile { get; private set; }
		public static string EUNomenclatureBaseUrl { get; private set; }
		public static string SectionDetailsUrl { get; private set; }
		public static string SEDailyFileBaseUrl { get; private set; }
		public static string SEDailyFileUrlForTest { get; private set; }
		public static int MonthsToSkipForTest { get; private set; }
		public static int WaitPageLoadingInSeconds { get; private set; }
		public static int MaxOfReloadLatestYearPage { get; private set; }
		public static string CircabcDownloadUrlPrefix { get; private set; }

		public static bool SkipNomenclatureDownloadIfExists { get; private set; }
		public static bool SkipImportDutyDownloadIfExists { get; private set; }

		public static bool SkipExportDutyDownloadIfExists { get; private set; }

		public static string MeasureConditionsCS01678500FilePath { get; private set; }

		public static HashSet<string> A00ValidMeasureTypeIds { get; private set; }

		public static HashSet<string> A20ValidMeasureTypeIds { get; private set; }

		public static HashSet<string> MeasureConditionValidMeasureTypeIds {get; private set;}

		public static HashSet<string> MeasureConditionValidMeasureActionCode { get; private set; }

		public static HashSet<string> TariffUOMValidMeasureTypeIds { get; private set; }

		public static HashSet<string> AntiDumpingMeasureTypeIds { get; private set; }
		
		public static HashSet<string> ValidMeasureTypeIdsForMeasureConditionInImportRate { get; private set; }

		public static HashSet<string> ValidMeasureTypeIdsForMeasureConditionInExportRate { get; private set; }

		public static void ConfigEnvironment()
		{
			EUTariffBaseUrl = GetSettingValue<string>("EUTariffBaseUrl") + "?p=1&n=-1&sort=modified_DESC";
			TariffCodeDetailsUrl = GetSettingValue<string>("TariffCodeDetailsUrl") + "?Lang=en&LangDescr=en&Domain=TARIC&SimDate={publishDate}&Taric={taric}&Offset={offset}";
			DownloadsFolder = GetSettingValue<string>("DownloadsFolder");
			EUReferenceDataBaseUrl = GetSettingValue<string>("EUReferenceDataBaseUrl") + "?p=1&n=-1&sort=modified_DESC";
			var downloadTime = GetSettingValue<string>("DownloadTimeoutInSeconds");
			DownloadTimeoutInSeconds = int.Parse(downloadTime, CultureInfo.InvariantCulture);
			NomenclatureUXmlFile = GetSettingValue<string>("NomenclatureUXmlFile");
			ImportTariffUXmlFile = GetSettingValue<string>("ImportTariffUXmlFile");
			ExportTariffUXmlFile = GetSettingValue<string>("ExportTariffUXmlFile");
			ImportTariffCompositeKeyUXmlFile = GetSettingValue<string>("ImportTariffCompositeKeyUXmlFile");
			TradeGroupUXmlFile = GetSettingValue<string>("TradeGroupUXmlFile");
			ExportTariffsDownloadURL = GetSettingValue<string>("ExportTariffsDownloadURL");
			ExportTariffsUXmlFile = GetSettingValue<string>("ExportTariffsUXmlFile");
			EUNomenclatureBaseUrl = GetSettingValue<string>("EUNomenclatureBaseUrl") + "?p=1&n=-1&sort=modified_DESC";
			SectionDetailsUrl = GetSettingValue<string>("SectionDetailsUrl") + "?Lang=en&SimDate={publishDate}&Expand=true";
			DailyTariffDownloadUrl = GetSettingValue<string>("DailyTariffDownloadUrl");
			MonthlyTariffUXmlFile = GetSettingValue<string>("MonthlyTariffUXmlFile");
			SEAndDailyTariffUXmlFile = GetSettingValue<string>("SEAndDailyTariffUXmlFile");
			AdditionalCodesUXmlFile = GetSettingValue<string>("AdditionalCodesUXmlFile");
			SEDailyFileBaseUrl = GetSettingValue<string>("SEDailyFileBaseUrl");
			SEDailyFileUrlForTest = GetSettingValue<string>("SEDailyFileUrlForTest");
			var monthsToSkipForTest = GetSettingValue<string>("MonthsToSkipForTest");
			MonthsToSkipForTest = !string.IsNullOrEmpty(monthsToSkipForTest) ? int.Parse(monthsToSkipForTest, CultureInfo.InvariantCulture) : 0;
			var waitLoadingSeconds = GetSettingValue<string>("WaitPageLoadingInSeconds");
			WaitPageLoadingInSeconds = !string.IsNullOrEmpty(waitLoadingSeconds) ? int.Parse(waitLoadingSeconds, CultureInfo.InvariantCulture) : 10;
			var maxOfReloadLatestYearPage = GetSettingValue<string>("MaxOfReloadLatestYearPage");
			MaxOfReloadLatestYearPage = !string.IsNullOrEmpty(maxOfReloadLatestYearPage) ? int.Parse(maxOfReloadLatestYearPage, CultureInfo.InvariantCulture) : 5;
			CircabcDownloadUrlPrefix = GetSettingValue<string>("CircabcDownloadUrlPrefix");
			SkipNomenclatureDownloadIfExists = bool.Parse(GetSettingValue<bool>("SkipNomenclatureDownloadIfExists"));
			SkipImportDutyDownloadIfExists = bool.Parse(GetSettingValue<bool>("SkipImportDutyDownloadIfExists"));
			SkipExportDutyDownloadIfExists = bool.Parse(GetSettingValue<bool>("SkipExportDutyDownloadIfExists"));
			MeasureConditionsCS01678500FilePath = GetSettingValue<string>("MeasureConditionsCS01678500FilePath");

			A00ValidMeasureTypeIds = GetSettingValueAsSet(nameof(A00ValidMeasureTypeIds));
			A20ValidMeasureTypeIds = GetSettingValueAsSet(nameof(A20ValidMeasureTypeIds));
			MeasureConditionValidMeasureTypeIds = GetSettingValueAsSet(nameof(MeasureConditionValidMeasureTypeIds));
			MeasureConditionValidMeasureActionCode = GetSettingValueAsSet(nameof(MeasureConditionValidMeasureActionCode));
			TariffUOMValidMeasureTypeIds = GetSettingValueAsSet(nameof(TariffUOMValidMeasureTypeIds));
			AntiDumpingMeasureTypeIds = GetSettingValueAsSet(nameof(AntiDumpingMeasureTypeIds));
			ValidMeasureTypeIdsForMeasureConditionInImportRate = GetSettingValueAsSet(nameof(ValidMeasureTypeIdsForMeasureConditionInImportRate));
			ValidMeasureTypeIdsForMeasureConditionInExportRate = GetSettingValueAsSet(nameof(ValidMeasureTypeIdsForMeasureConditionInExportRate));
		}

		internal static string GetSettingValue<T>(string key)
		{
			return Config[key] ?? default(T)?.ToString();
		}

		internal static HashSet<string> GetSettingValueAsSet(string key)
		{
			var configValue = Config[key] ?? string.Empty;
			return configValue
				.Split(",")
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(x => x.Trim())
				.ToHashSet();
		}

		public static IEnumerable<string> ValidMeasureTypeIds()
		{
			return A00ValidMeasureTypeIds.Concat(A20ValidMeasureTypeIds);
		}

		public static string GetRateCodeByMeasureTypeID(string measureTypeID)
		{
			if (A00ValidMeasureTypeIds.Contains(measureTypeID))
			{
				return A00RateCode;
			}
			if (A20ValidMeasureTypeIds.Contains(measureTypeID))
			{
				return A20RateCode;
			}
			if (AntiDumpingMeasureTypeIds.Contains(measureTypeID))
			{
				switch (measureTypeID)
				{
					case "551":
						return A35RateCode;
					case "552":
						return A30RateCode;
					case "553":
						return A45RateCode;
					case "554":
						return A40RateCode;
				}
			}
			return A00RateCode;
		}

		public static string GetRateTypeFromRateCode(string rateCode)
		{
			switch (rateCode)
			{
				case "A30":
				case "A35":
					return "ADD";
				case "A40":
				case "A45":
					return "CVD";
				case "SEC":
					return "SEC";
				default:
					return DutyRateType;
			}
		}

		public static IEnumerable<IWebFileInfo> GetExistingFileIfExists(string path, IEnumerable<string> fileNames, Regex pattern)
		{
			var absoluteDownloadsPath = Path.GetFullPath(path);
			var existingFiles = Directory.GetFiles(absoluteDownloadsPath).Select(x => Path.GetFileName(x));
			return existingFiles.Where(x => fileNames.Any(y => x.StartsWith(y, StringComparison.OrdinalIgnoreCase)) || (pattern != null && pattern.IsMatch(x)))
				.Select(x => new WebFileInfo(x, Path.Combine(absoluteDownloadsPath, x), File.GetLastWriteTime(Path.Combine(absoluteDownloadsPath, x))));
		}

		public static void SetPublishTime(DateTime publishDate)
		{
			PublishDate = publishDate;
		}

		public static void SetDownloadsFolder(string downloadsFolder)
		{
			DownloadsFolder = downloadsFolder;
		}

		public static void SetNomenclatureUXmlFile(string nomenclatureUXMLFile)
		{
			NomenclatureUXmlFile = nomenclatureUXMLFile;
		}

		public static IWebFileInfo GetMissingConditionsFile()
		{
			var configFilePath = ApplicationConfig.MeasureConditionsCS01678500FilePath;
			var fullFileName = Path.GetFullPath(configFilePath);
			var filePath = Path.GetDirectoryName(fullFileName);
			var fileName = Path.GetFileName(fullFileName);

			return new WebFileInfo(fileName, filePath, File.GetLastWriteTime(fullFileName));
		}
	}
}
