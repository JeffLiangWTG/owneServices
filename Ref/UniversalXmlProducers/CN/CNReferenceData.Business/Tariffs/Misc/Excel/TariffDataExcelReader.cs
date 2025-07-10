using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using FlexCel.XlsAdapter;
using Columns = CargoWise.RefDbRepo.CNReferenceData.Business.ExcelReadConfiguration.ColumnConstants;

namespace CargoWise.RefDbRepo.CNReferenceData.Business
{
	public class TariffDataExcelReader
	{
		public TariffDataExcelReader(DateTime? effectiveDate)
		{
			SourceFolder = GlobalOption.Instance.Setting.GetFullExcelSourcePath();
			EffectiveDate = effectiveDate;
			AdditionalElementHelper = new AdditionalElementHelper();
		}
		public string SourceFolder { get; private set; }

		readonly DateTime? EffectiveDate;

		public RefCusTariff[] OutputRefCusTariffs { get; private set; }
		public AdditionalElementHelper AdditionalElementHelper { get; }

		public void ReadFromExcels()
		{
			AdditionalElementHelper.Clear();

			var tariffDataList = ReadFromExcelFile(ExcelReadConfiguration.Tariff).Where(x => !string.IsNullOrEmpty(x.Properties[Columns.Tariff.NormalRate.Name])).ToList();
			tariffDataList.ForEach(x => { x.Key = x.Key.PadRight(10, '0'); x.Properties[Columns.Tariff.TariffCode.Name] = x.Key; });

			var ciqDataList = ReadFromExcelFile(ExcelReadConfiguration.CIQ).ToList();
			var ciqDataDict = ciqDataList.GroupBy(row => row.Key).ToDictionary(group => group.Key, group => group.Select(row => row));
			var rateDataList = ReadFromExcelFile(ExcelReadConfiguration.DutyRate).ToList();
			var rateDataDict = rateDataList.ToDictionary(row => row.Key);
			var exciseDataList = ReadFromExcelFile(ExcelReadConfiguration.ExciseRate).ToList();
			var exciseDataDict = exciseDataList.ToDictionary(row => row.Key);
			var expDutyDataList = ReadFromExcelFile(ExcelReadConfiguration.ExportDutyRate).ToList();
			var expDutyDataDict = expDutyDataList.ToDictionary(row => row.Key);
			var usaDutyDataList = ReadFromExcelFile(ExcelReadConfiguration.UsaAddRate).ToList();
			var usaDutyDataDict = usaDutyDataList.GroupBy(row => row.Key).ToDictionary(group => group.Key, group => group.Select(row => row));

			var reader = new ExcelTariffParser(EffectiveDate, GlobalOption.Instance.MaxSmallDateTime, AdditionalElementHelper);

			var refCusTariffs = new List<RefCusTariff>();
			foreach (var tariff in tariffDataList)
			{
				refCusTariffs.AddRange(reader.CreateTariffs(tariff,
					ciqDataDict.GetSafe(tariff.Key),
					rateDataDict.GetSafe(tariff.Key),
					exciseDataDict.GetSafe(tariff.Key),
					expDutyDataDict.GetSafe(tariff.Key),
					usaDutyDataDict.GetSafe(tariff.Key)));
			}
			OutputRefCusTariffs = refCusTariffs.ToArray();

			Log(ExcelReadConfiguration.Tariff, tariffDataList);
			Log(ExcelReadConfiguration.CIQ, ciqDataList);
			Log(ExcelReadConfiguration.DutyRate, rateDataList);
			Log(ExcelReadConfiguration.ExciseRate, exciseDataList);
			Log(ExcelReadConfiguration.ExportDutyRate, expDutyDataList);
			Log(ExcelReadConfiguration.UsaAddRate, usaDutyDataList);
		}

		static void Log(ExcelReadConfiguration configuration, IEnumerable<RowData> rows)
		{
			GlobalOption.Instance.Log.Info($"{configuration.SourceFile}: {rows.Count()} {configuration.Description} loaded.");
			var unexportedData = rows.Where(x => !x.Exported);
			if (unexportedData.Any())
			{
				GlobalOption.Instance.Log.Info($"\tMissing Data: {string.Join(", ", unexportedData.Select(x => x.Key).ToArray())}");
			}
		}

		protected virtual XlsFile GetXlsFile(ExcelReadConfiguration readConfig)
		{
			var inputXlsFile = Directory.GetFiles(SourceFolder, "*.xlsx").FirstOrDefault(path => path.FitsConfig(readConfig));
			if (inputXlsFile == null)
			{
				GlobalOption.Instance.Log.Error($"Cannot find excel file for {readConfig.Description}, File Index: {readConfig.FileIndex}.");
				return null;
			}
			else
			{
				return new XlsFile(inputXlsFile, false);
			}
		}

		IEnumerable<RowData> ReadFromExcelFile(ExcelReadConfiguration readConfig)
		{
			var xlsFile = GetXlsFile(readConfig);
			if (xlsFile != null)
			{
				readConfig.SourceFile = xlsFile.ActiveFileName;
				xlsFile.SetSheetSelected(readConfig.SheetIndex, true);
				xlsFile.ActiveSheet = readConfig.SheetIndex;
				var rowCount = xlsFile.GetRowCount(xlsFile.ActiveSheet);

				for (int rowId = readConfig.StartingRow; rowId <= rowCount && rowId <= readConfig.LastRow; rowId++)
				{
					var key = xlsFile.GetCellValue(rowId, readConfig.KeyColumnIndex)?.ToString();
					if (!string.IsNullOrEmpty(key) && Regex.IsMatch(key, readConfig.KeyRegex))
					{
						var data = new RowData() { Key = key, SourceRowIndex = rowId };

						foreach (var (ColIndex, Name, PreOperation) in readConfig.Columns)
						{
							string readedValue = string.Empty;
							if (ColIndex > 0)
							{
								var cellValue = xlsFile.GetCellValue(rowId, ColIndex);
								if (cellValue == null)
								{
									readedValue = string.Empty;
								}
								else if (PreOperation == null)
								{
									readedValue = cellValue.ToString();
								}
								else
								{
									readedValue = PreOperation.Invoke(cellValue.ToString());
								}
							}
							data.Properties.Add(Name, readedValue);
						}

						yield return data;
					}
				}
			}
		}
	}
}
