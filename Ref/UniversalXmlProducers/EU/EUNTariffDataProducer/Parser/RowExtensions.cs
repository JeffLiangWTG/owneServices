using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	internal static class RowExtensions
	{
		internal static string GetStringValue(this IRow row, int columnIndex)
		{
			var column = row.GetCell(columnIndex);

			if (column == null)
			{
				return null;
			}

			if (column.CellType == CellType.String && !string.IsNullOrEmpty(column.StringCellValue))
			{
				return column.StringCellValue;
			}

			return column.CellType == CellType.Numeric ? $"{column.NumericCellValue}" : null;
		}

		internal static double GetNumericValue(this IRow row, int columnIndex)
		{
			var column = row.GetCell(columnIndex);

			return column?.NumericCellValue ?? 0;
		}
	}
}
