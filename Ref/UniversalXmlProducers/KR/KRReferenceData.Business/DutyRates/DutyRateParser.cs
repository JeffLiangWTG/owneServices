using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class DutyRateParser : BaseEntityConfigurationParser
	{
		public DutyRateParser(string configFilePath, string dataFilePath, string preferenceDataFileInputPath, string preferenceConfigFilePath) : base(configFilePath, dataFilePath)
		{
			PreferenceDataFile = preferenceDataFileInputPath;
			PreferenceConfigFile = preferenceConfigFilePath;
		}

		protected override void ExportToXmlFilesCore(string outputFilePath, DateTime publicationDate, EntityConfiguration configuration, IWorkbook workbook, string suffix)
		{
			var (preferenceConfiguration, preferenceWorkbook) = GetEntityConfigurationAndWorkbook(PreferenceDataFile, PreferenceConfigFile);
			var commonOutputFileName = Path.GetFileNameWithoutExtension(outputFilePath);
			var outputDirectoryPath = Path.GetDirectoryName(outputFilePath);
			var tariffs = new ExcelEntityUpdater<RefCusTariff>(configuration, AdditionalDataUpdater, new DutyRateTopEntityLookupManager()).Update(workbook);
			var writerConfiguration = EntityConfigurationManager.GetWriterConfiguration(configuration);
			for (int i = 0; i < 10; i++)
			{
				var outputFile = $"{outputDirectoryPath}\\{commonOutputFileName}_{publicationDate.Year}_{i}.xml";
				var entities = tariffs.Where(item => item.ZZ1_TariffCode.StartsWith(i.ToString(CultureInfo.CurrentCulture), StringComparison.Ordinal))?.ToArray();
				Helper.ExportToXMLFile($"{Constants.DataSources.DutyRates} {i}", outputFile, writerConfiguration, publicationDate, entities);
			}
		}

		protected static Dictionary<string, string> PopulateTradeGroups((EntityConfiguration, IWorkbook) preferenceData)
		{
			var configuration = preferenceData.Item1;
			var workbook = preferenceData.Item2;
			var result = new Dictionary<string, string>();
			var sheetIndex = configuration.EntityTypeExcelColumnMapping.SheetIndex;
			var startRow = configuration.EntityTypeExcelColumnMapping.StartRow;
			var sheet = workbook.GetSheetAt(sheetIndex);

			var rowIndex = startRow;
			IRow row;
			var keyColumn = configuration.EntityTypeExcelColumnMapping.EntityTypes.SelectMany(x => x.Properties.Where(y => y.Name == nameof(RefCusPreference.ZZS_Preference))).FirstOrDefault()?.ExcelColumn ?? -1;
			var valueColumn = configuration.EntityTypeExcelColumnMapping.EntityTypes.SelectMany(x => x.Properties.Where(y => y.Name == "TradeGroup")).FirstOrDefault()?.ExcelColumn ?? -1;
			while ((row = sheet.GetRow(rowIndex++)) != null)
			{
				var cell = row.GetCell(keyColumn);
				cell?.SetCellType(CellType.String);
				var keyCell = cell?.StringCellValue ?? string.Empty;

				cell = row.GetCell(valueColumn);
				cell?.SetCellType(CellType.String);
				var valueCell = cell?.StringCellValue ?? string.Empty;
				if (!string.IsNullOrEmpty(valueCell) && !string.IsNullOrEmpty(valueCell))
				{
					result.Add(keyCell, valueCell);
				}
			}
			return result;
		}

		protected virtual DutyRateAdditionalDataUpdater AdditionalDataUpdater => new DutyRateAdditionalDataUpdater(PopulateTradeGroups(GetEntityConfigurationAndWorkbook(PreferenceDataFile, PreferenceConfigFile)));

		string PreferenceDataFile { get; }
		string PreferenceConfigFile { get; }
	}
}
