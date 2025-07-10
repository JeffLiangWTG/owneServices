using System.Collections.Generic;
using System.Text;
using CargoWise.RefDbRepo.KRReferenceData.Business;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class ProcessedRows
	{
		public ProcessedRows()
		{
			processedRows = new Dictionary<string, bool>();
		}

		readonly Dictionary<string, bool> processedRows;

		public bool IsAlreadyProcessed(IRow row, EntityConfiguration configuration)
		{
			return processedRows.ContainsKey(GetExcelColumnMappedValues(row, configuration));
		}

		public void RegisterProcessed(IRow row, EntityConfiguration configuration)
		{
			var key = GetExcelColumnMappedValues(row, configuration);
			if (!processedRows.ContainsKey(key))
			{
				processedRows.Add(key, true);
			}
		}

		static string GetExcelColumnMappedValues(IRow row, EntityConfiguration configuration)
		{
			var result = new StringBuilder();
			foreach (var mappingEntityType in configuration.EntityTypeExcelColumnMapping.EntityTypes)
			{
				foreach (var mappingProperty in mappingEntityType.Properties)
				{
					if (mappingProperty.ConstantValue == null)
					{
						var cell = row.GetCell(mappingProperty.ExcelColumn);
						cell?.SetCellType(CellType.String);
						result.Append(cell?.StringCellValue ?? string.Empty);
						result.Append(separator);
					}
				}
			}
			return result.ToString().TrimEnd(separator);
		}

		const char separator = ',';
	}
}
