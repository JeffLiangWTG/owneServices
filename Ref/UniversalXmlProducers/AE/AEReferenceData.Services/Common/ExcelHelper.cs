using System;
using System.Collections.Generic;
using FlexCel.XlsAdapter;

namespace CargoWise.RefDbRepo.AEReferenceData.Services;

public static class ExcelHelper
{
	public static XlsFile LoadExcel(string path)
	{
		var xls = new XlsFile();
		xls.Open(path);
		return xls;
	}

	public static List<T> GetDataFromExcel<T, TResult>(XlsFile xls, IDataConverter<T, TResult> converter)
	{
		xls.ActiveSheetByName = converter.SheetName;
		var excelColumns = converter.GetExcelColumns();
		var (columnIndexes, startRow) = GetColumnIndexes(xls, excelColumns);
		if (columnIndexes.Count != excelColumns.Count)
		{
			return new List<T>();
		}

		List<List<(string columnName, string value)>> rows = new List<List<(string columnName, string value)>>();
		for (int row = startRow; row <= xls.RowCount; row++)
		{
			var rowData = new List<(string columnName, string value)>();
			foreach (var column in columnIndexes)
			{
				var value = xls.GetStringFromCell(row, column.Value).Trim();
				rowData.Add((column.Key, value));
			}
			rows.Add(rowData);
		}
		return converter.ConvertExcelColumns(rows);
	}

	public static (Dictionary<string, int> columnsIndex, int startRow) GetColumnIndexes(XlsFile xls, List<string> excelColumns)
	{
		var columnIndexes = new Dictionary<string, int>();
		var startRow = 0;
		for (int row = 1; row <= xls.RowCount; row++)
		{
			int ColCountInRow = xls.ColCountInRow(row);
			for (int colIndex = 1; colIndex <= ColCountInRow; colIndex++)
			{
				var actualIndex = xls.ColFromIndex(row, colIndex);
				var cell = xls.GetStringFromCell(row, actualIndex).Trim();
				cell = cell.Replace("\r\n", "").Replace("\n", "").Replace("\r", "");
				if (excelColumns.Contains(cell))
				{
					columnIndexes[cell] = actualIndex;
					startRow = row + 1;
				}

				if (columnIndexes.Count == excelColumns.Count)
				{
					break;
				}
			}

			if (columnIndexes.Count == excelColumns.Count)
			{
				break;
			}

			if (columnIndexes.Count > 0 && columnIndexes.Count != excelColumns.Count)
			{
				Console.Error.WriteLine("Some columns are missing in the {0} sheet.", xls.ActiveSheetByName);
				return (new Dictionary<string, int>(), 0);
			}
		}
		return (columnIndexes, startRow);
	}
}
