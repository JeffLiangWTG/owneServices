using System;
using System.Globalization;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.TRReferenceData.Services
{
	public static class NPOIExtensions
	{
		public static string GetTextOrMerged(this ICell cell)
		{
			var result = cell.ValueToString();
			if (result != null && string.IsNullOrWhiteSpace(result))
			{
				result = cell.GetMergedText();
			}
			return result?.Trim();
		}

		public static string GetMergedText(this ICell cell)
		{
			var sheet = cell.Sheet;
			var rowIndex = cell.RowIndex;
			var columnIndex = cell.ColumnIndex;

			foreach (var range in sheet.MergedRegions)
			{
				if (rowIndex >= range.FirstRow && rowIndex <= range.LastRow && columnIndex >= range.FirstColumn && columnIndex <= range.LastColumn)
				{
					var startRow = sheet.GetRow(range.FirstRow);
					var startCell = startRow.GetCell(range.FirstColumn);

					return startCell.ValueToString();
				}
			}

			return string.Empty;
		}

		public static string ToSearchKey(this ICell cell) => cell.ValueToString().ToSearchKey();

		public static string ValueToString(this ICell cell, string format = null) => cell == null ? null : ValueToString(cell.CellType, cell, format);

		static string ValueToString(CellType type, ICell cell, string format = null, bool strict = false)
		{
			var result = string.Empty;
			switch (type)
			{
				case CellType.String:
					result = cell.StringCellValue;
					break;
				case CellType.Numeric:
					if (DateUtil.IsCellDateFormatted(cell))
					{
						var dateCellvalue= cell.DateCellValue;
						result = format == null ? dateCellvalue.ToString("o") : dateCellvalue.ToString(format, CultureInfo.InvariantCulture);
					}
					else
					{
						var numericCellValue = cell.NumericCellValue;
						if (!strict)
						{
							numericCellValue = Math.Round(numericCellValue, 4);
						}
						result = format == null ? numericCellValue.ToString(CultureInfo.InvariantCulture) : numericCellValue.ToString(format, CultureInfo.InvariantCulture);
					}
					break;
				case CellType.Boolean:
					result = cell.BooleanCellValue.ToString();
					break;
				case CellType.Formula:
					result = ValueToString(cell.CachedFormulaResultType, cell);
					break;
			}
			return result.Trim();
		}

		public static bool IsEmpty(this ICell cell) => cell.CellType == CellType.Blank || cell.CellType == CellType.String && string.IsNullOrWhiteSpace(cell.StringCellValue);
	}
}
