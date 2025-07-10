using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class PreferenceRefCusMapDataUpdater : IAdditionalDataUpdater<RefCusMap>
	{
		public static bool IsDataRowValid(IRow row, EntityConfiguration configuration)
		{
			var result = false;
			var columnIndex = configuration.EntityTypeExcelColumnMapping.EntityTypes.SelectMany(x => x.Properties.Where(y => y.Name == nameof(RefCusMap.ZZM_CustomsValue))).FirstOrDefault()?.ExcelColumn ?? -1;
			if (columnIndex != -1)
			{
				var cell = row.GetCell(columnIndex);
				cell?.SetCellType(CellType.String);
				var cellValue = cell?.StringCellValue ?? string.Empty;
				result = !string.IsNullOrEmpty(cellValue);
			}
			return result;
		}

		bool IAdditionalDataUpdater<RefCusMap>.IsDataRowValid(IRow row, EntityConfiguration configuration) => IsDataRowValid(row, configuration);
		void IAdditionalDataUpdater<RefCusMap>.UpdateAdditionally(RefCusMap cusMap, IRow row, EntityConfiguration configuration) { }
		void IAdditionalDataUpdater<RefCusMap>.UpdateRule(RefCusMap cusMap, Rule rule) { }
		void IAdditionalDataUpdater<RefCusMap>.RegisterUpdated(IRow row, EntityConfiguration configuration) { }
	}
}
