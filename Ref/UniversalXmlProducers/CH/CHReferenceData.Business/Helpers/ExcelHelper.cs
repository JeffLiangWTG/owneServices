using System;
using System.Globalization;
using System.Linq;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.CHReferenceData.Business
{
	internal static class ExcelHelper
	{
		internal static int FindColumn(this IRow row, params string[] titles)
		{
			if (row != null)
			{
				for (var columnIndex = 0; columnIndex < row.LastCellNum; columnIndex++)
				{
					var cell = row.GetCell(columnIndex);
					if (cell != null && cell.CellType == NPOI.SS.UserModel.CellType.String)
					{
						var cellValue = cell.GetStringValueSafe();
						if (titles.Any(x => x.Equals(cellValue, StringComparison.OrdinalIgnoreCase)))
						{
							return columnIndex;
						}
					}
				}
			}
			throw new InvalidOperationException($@"Column not found: {string.Join(" or ", titles.Select(x => '"' + x + '"'))} (Excel sheet ""({row?.Sheet.SheetName ?? "n/a"})"") ");
		}

		internal static int GetIntValueSafe(this ICell cell)
		{
			if (cell == null)
			{
				return 0;
			}
			switch (cell.CellType)
			{
				case CellType.Numeric:
					return (int)cell.NumericCellValue;
				case CellType.String:
					return int.TryParse(cell.StringCellValue, out var value) ? value : 0;
				default:
					return 0;
			}
		}

		internal static decimal GetDecimalValueSafe(this ICell cell)
		{
			if (cell == null)
			{
				return 0;
			}
			switch (cell.CellType)
			{
				case CellType.Numeric:
					return (decimal)cell.NumericCellValue;
				case CellType.String:
					return decimal.TryParse(cell.StringCellValue, out var value) ? value : 0;
				default:
					return 0;
			}
		}

		internal static string GetStringValueSafe(this ICell cell)
		{
			if (cell == null)
			{
				return string.Empty;
			}
			switch (cell.CellType)
			{
				case CellType.String:
					return cell.StringCellValue;
				case CellType.Numeric:
					return cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);
				default:
					return
						string.Empty;
			}
		}
	}
}
