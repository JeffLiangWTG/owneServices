using System.Collections.Generic;
using System.IO;
using CargoWise.RefDbRepo.EUReferenceData.Business;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.EUReferenceData.CustomsMeursing.Business
{
	public static class ExcelParser
	{
		public static IList<CustomsMeursingData> ReadXlsFileIntoResults(string fileName)
		{
			using (var reader = File.OpenRead(fileName))
			{
				var sheetIndex = 1;
				var startingRow = 2;
				var lastRow = int.MaxValue;

				var allExcelDatas = new List<CustomsMeursingData>();
				var xlsFile = new XlsFile(reader, false);
				xlsFile.SetSheetSelected(sheetIndex, true);
				var rowCount = xlsFile.GetRowCount(xlsFile.ActiveSheet);

				for (int rowId = startingRow; rowId <= rowCount && rowId <= lastRow; rowId++)
				{
					var excelData = new CustomsMeursingData
					{
						GoodsNomItemID = xlsFile.GetCellValue(rowId, 1)?.ToString(),
						TariffCode = xlsFile.GetCellValue(rowId, 2)?.ToString(),
						OrdNumb = xlsFile.GetCellValue(rowId, 3)?.ToString(),
						DatStart = Utils.GetFromOADate(xlsFile.GetCellValue(rowId, 4)),
						DatEnd = Utils.GetFromOADate(xlsFile.GetCellValue(rowId, 5)),
						RedInd = xlsFile.GetCellValue(rowId, 6)?.ToString(),
						Description = xlsFile.GetCellValue(rowId, 7)?.ToString(),
						Description2 = xlsFile.GetCellValue(rowId, 8)?.ToString(),
						Decode = xlsFile.GetCellValue(rowId, 9)?.ToString(),
						DutyCondFull = xlsFile.GetCellValue(rowId, 10)?.ToString(),
						GeogrAreaID = xlsFile.GetCellValue(rowId, 11)?.ToString(),
						MeasTypID = xlsFile.GetCellValue(rowId, 12)?.ToString()
					};
					allExcelDatas.Add(excelData);
				}
				return allExcelDatas;
			}
		}

		public static IList<GeographicalAreaCompositionData> ReadGeographicalXlsFileIntoResults(string fileName)
		{
			using (var reader = File.OpenRead(fileName))
			{
				var sheetIndex = 1;
				var startingRow = 2;
				var lastRow = int.MaxValue;

				var allExcelDatas = new List<GeographicalAreaCompositionData>();
				var xlsFile = new XlsFile(reader, false);
				xlsFile.SetSheetSelected(sheetIndex, true);
				var rowCount = xlsFile.GetRowCount(xlsFile.ActiveSheet);

				for (int rowId = startingRow; rowId <= rowCount && rowId <= lastRow; rowId++)
				{
					var excelData = new GeographicalAreaCompositionData
					{
						CountryGroup = xlsFile.GetCellValue(rowId, 1)?.ToString(),
						GroupAbbreviation = xlsFile.GetCellValue(rowId, 4)?.ToString(),
						MemberCountry = xlsFile.GetCellValue(rowId, 6)?.ToString(),
					};
					allExcelDatas.Add(excelData);
				}
				return allExcelDatas;
			}
		}
	}
}
