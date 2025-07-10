using System.Collections.Generic;
using System.IO;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.NLReferenceData.Services
{
	public class CommonExcelParser<T> : CommonExcelParserAbstract<T> where T : class, new()
	{
		public delegate T ExtractDataDelegate(XlsFile xlsFile, string key, int rowId);
		readonly ExtractDataDelegate Extractor;

		int sheetIndex;
		int startingRow;
		int keyColumn;

		protected override int SheetIndex => sheetIndex;
		protected override int StartingRow => startingRow;
		protected override int KeyColumn => keyColumn;

		public CommonExcelParser(ExtractDataDelegate extractor, int sheet = 1, int startRow = 2, int keyCol = 1)
		{
			Extractor = extractor;
			sheetIndex = sheet;
			startingRow = startRow;
			keyColumn = keyCol;
		}

		protected override T ExtractDataFromRow(XlsFile xlsFile, string key, int rowId)
		{
			return Extractor(xlsFile, key, rowId);
		}
	}

	public abstract class CommonExcelParserAbstract<T> where T : class
	{
		protected virtual bool ShouldProcessFile(XlsFile xlsFile) => true;
		protected abstract T ExtractDataFromRow(XlsFile xlsFile, string key, int rowId);

		protected virtual bool IsKeyValid(string key) => !string.IsNullOrWhiteSpace(key);

		protected virtual int SheetIndex => 1;
		protected virtual int StartingRow => 2;
		protected virtual int KeyColumn => 1;

		public IList<T> ReadXlsFile(IList<string> files)
		{
			var allExcelDatas = new List<T>();

			foreach (string file in files)
			{
				using (var reader = File.OpenRead(file))
				{
					var sheetIndex = SheetIndex;
					var startingRow = StartingRow;
					var lastRow = int.MaxValue;

					var xlsFile = new XlsFile(reader, false);
					xlsFile.SetSheetSelected(sheetIndex, true);

					if (ShouldProcessFile(xlsFile))
					{
						var rowCount = xlsFile.GetRowCount(xlsFile.ActiveSheet);
						var code = string.Empty;
						for (int rowId = startingRow; rowId <= rowCount && rowId <= lastRow; rowId++)
						{
							var key = xlsFile.GetStr(rowId, KeyColumn);
							if (IsKeyValid(key))
							{
								var rowData = ExtractDataFromRow(xlsFile, key, rowId);
								if (rowData != null)
								{
									allExcelDatas.Add(rowData);
								}
							}
						}
					}
					reader.Close();
				}
			}
			return allExcelDatas;
		}
	}

	public static class XlsFileExtensions
	{
		public static string GetStr(this XlsFile xls, int rowId, int colId) => xls.GetCellValue(rowId, colId)?.ToString().Trim() ?? string.Empty;
	}
}
