using System;
using System.Data;
using System.Globalization;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Eval;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace CargoWise.RefDbRepo.TaiwanReferenceData
{
	public static class ExcelHelper
	{
		public static string GetValueAsString(object obj)
		{
			return obj == null ? string.Empty : obj.ToString();
		}

		public static DataTable BytesToDataTable(string format, byte[] data, int sheetIndex, int headerRowIndex, bool needHeader)
		{
			IWorkbook workbook;
			using (var stream = new MemoryStream(data))
			{
				if (format == ExcelFileFormat.Xlsx)
				{
					workbook = new XSSFWorkbook(stream);
				}
				else
				{
					workbook = new HSSFWorkbook(stream);
				}
			}
			var sheet = workbook.GetSheetAt(sheetIndex);
			var table = ToDataTable(sheet, headerRowIndex, needHeader);
			table.TableName = sheet.SheetName;
			return table;
		}

		public static DataTable ExcelToDataTable(string fileName, int sheetIndex, int headerRowIndex, bool needHeader)
		{
			IWorkbook workbook;
			using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				if (fileName.ToLower().EndsWith(ExcelFileFormat.Xlsx))
				{
					workbook = new XSSFWorkbook(file);
				}
				else
				{
					workbook = new HSSFWorkbook(file);
				}
			}
			var sheet = workbook.GetSheetAt(sheetIndex);
			var table = ToDataTable(sheet, headerRowIndex, needHeader);
			table.TableName = sheet.SheetName;
			return table;
		}

		static DataTable ToDataTable(ISheet sheet, int headerRowIndex, bool needHeader)
		{
			var table = CreateTable(sheet, headerRowIndex, needHeader, out var cellCount);
			FillTableData(sheet, headerRowIndex, table, cellCount);
			return table;
		}

		static DataTable CreateTable(ISheet sheet, int headerRowIndex, bool needHeader, out int cellCount)
		{
			var table = new DataTable();
			IRow headerRow;
			if (headerRowIndex < 0 || !needHeader)
			{
				headerRow = sheet.GetRow(0);
				cellCount = headerRow.LastCellNum;

				for (int i = headerRow.FirstCellNum; i <= cellCount; i++)
				{
					table.Columns.Add(new DataColumn(Convert.ToString(i, CultureInfo.InvariantCulture)));
				}
			}
			else
			{
				headerRow = sheet.GetRow(headerRowIndex);
				cellCount = headerRow.LastCellNum;

				for (int i = headerRow.FirstCellNum; i <= cellCount; i++)
				{
					string columnName;
					if (headerRow.GetCell(i) == null)
					{
						columnName = Convert.ToString(i, CultureInfo.InvariantCulture);
					}
					else
					{
						columnName = headerRow.GetCell(i).ToString().Trim();
					}
					if (string.IsNullOrEmpty(columnName) || table.Columns.IndexOf(columnName) > 0)
					{
						columnName = "Column" + i;
					}
					table.Columns.Add(new DataColumn(columnName));
				}
			}
			return table;
		}

		static void FillTableData(ISheet sheet, int headerRowIndex, DataTable table, int cellCount)
		{
			for (int i = headerRowIndex + 1; i <= sheet.LastRowNum; i++)
			{
				var row = sheet.GetRow(i);
				if (row == null)
				{
					row = sheet.CreateRow(i);
				}

				var dataRow = table.NewRow();
				for (int cellIndex = row.FirstCellNum; cellIndex >= 0 && cellIndex <= cellCount; cellIndex++)
				{
					if (row.GetCell(cellIndex) != null)
					{
						SwitchCellType(row, dataRow, cellIndex);
					}
				}
				table.Rows.Add(dataRow);
			}
		}

		static void SwitchCellType(IRow row, DataRow dataRow, int index)
		{
			switch (row.GetCell(index).CellType)
			{
				case CellType.String:
					dataRow[index] = row.GetCell(index).StringCellValue;
					break;
				case CellType.Numeric:
					if (DateUtil.IsCellDateFormatted(row.GetCell(index)))
					{
						dataRow[index] = DateTime.FromOADate(row.GetCell(index).NumericCellValue).ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
					}
					else
					{
						dataRow[index] = Convert.ToDecimal(row.GetCell(index).NumericCellValue, CultureInfo.InvariantCulture);
					}
					break;
				case CellType.Boolean:
					dataRow[index] = Convert.ToString(row.GetCell(index).BooleanCellValue, CultureInfo.InvariantCulture);
					break;
				case CellType.Error:
					dataRow[index] = ErrorEval.GetText(row.GetCell(index).ErrorCellValue);
					break;
				case CellType.Formula:
					switch (row.GetCell(index).CachedFormulaResultType)
					{
						case CellType.String:
							dataRow[index] = row.GetCell(index).StringCellValue;
							break;
						case CellType.Numeric:
							dataRow[index] = Convert.ToString(row.GetCell(index).NumericCellValue, CultureInfo.InvariantCulture);
							break;
						case CellType.Boolean:
							dataRow[index] = Convert.ToString(row.GetCell(index).BooleanCellValue, CultureInfo.InvariantCulture);
							break;
						case CellType.Error:
							dataRow[index] = ErrorEval.GetText(row.GetCell(index).ErrorCellValue);
							break;
						default:
							dataRow[index] = "";
							break;
					}
					break;
				default:
					dataRow[index] = "";
					break;
			}
		}
	}

	public static class ExcelFileFormat
	{
		public const string Xlsx = "xlsx";
		public const string Ods = "ods";
	}
}
