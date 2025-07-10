using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;
using static CargoWise.RefDbRepo.KRReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class WCONomenclatureAdditionalDataUpdater : IAdditionalDataUpdater<RefCusNomenclatureGroup>
	{
		public static bool IsDataRowValid(IRow row, EntityConfiguration configuration)
		{
			var result = false;
			var columnIndex = configuration.EntityTypeExcelColumnMapping.EntityTypes.SelectMany(x => x.Properties.Where(y => y.Name == nameof(RefCusNomenclatureGroup.ZZ5_Value))).FirstOrDefault()?.ExcelColumn ?? -1;
			if (columnIndex != -1)
			{
				var cell = row.GetCell(columnIndex);
				cell?.SetCellType(CellType.String);
				var cellValue = cell?.StringCellValue ?? string.Empty;
				if (!string.IsNullOrEmpty(cellValue))
				{
					if (cellValue.Length <= Constants.WCONomenclatureMaxLength)
					{
						if (cellValue.Length == Constants.WCONomenclatureMaxLength)
						{
							RemoveEndingZerosIfNecessary(cell, cellValue, NomenclaturePadZerosToRemove.TwoZeros);
						}
						result = true;
					}
					else if (cellValue.Length == Constants.TariffLength)
					{
						result = RemoveEndingZerosIfNecessary(cell, cellValue, NomenclaturePadZerosToRemove.SixZeros);
						if (!result)
						{
							result = RemoveEndingZerosIfNecessary(cell, cellValue, NomenclaturePadZerosToRemove.FourZeros);
						}
					}
				}
			}
			return result;
		}

		static bool RemoveEndingZerosIfNecessary(ICell cell, string currentCellValue, string padStr)
		{
			var removed = false;
			if (currentCellValue.EndsWith(padStr, System.StringComparison.Ordinal))
			{
				cell.SetCellValue(currentCellValue.Remove(currentCellValue.LastIndexOf(padStr, System.StringComparison.Ordinal)));
				removed = true;
			}
			return removed;
		}

		bool IAdditionalDataUpdater<RefCusNomenclatureGroup>.IsDataRowValid(IRow row, EntityConfiguration configuration) => IsDataRowValid(row, configuration);
		void IAdditionalDataUpdater<RefCusNomenclatureGroup>.UpdateAdditionally(RefCusNomenclatureGroup dataEntity, IRow row, EntityConfiguration configuration) { }
		void IAdditionalDataUpdater<RefCusNomenclatureGroup>.UpdateRule(RefCusNomenclatureGroup dataEntity, Rule rule) { }
		void IAdditionalDataUpdater<RefCusNomenclatureGroup>.RegisterUpdated(IRow row, EntityConfiguration configuration) { }
	}
}
