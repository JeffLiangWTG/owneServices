using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class ExportFTATypeDataUpdater : IAdditionalDataUpdater<RefCusCodeList>
	{
		const int EXFTA_ZZDCodeLength = 3;

		public static bool IsDataRowValid(IRow row, EntityConfiguration configuration)
		{
			var result = false;
			var columnIndex = configuration.EntityTypeExcelColumnMapping.EntityTypes.SelectMany(x => x.Properties.Where(y => y.Name == nameof(RefCusCodeList.ZZD_Code))).FirstOrDefault()?.ExcelColumn ?? -1;
			if (columnIndex != -1)
			{
				var cell = row.GetCell(columnIndex);
				cell?.SetCellType(CellType.String);
				var cellValue = cell?.StringCellValue ?? string.Empty;
				var isNumeric = int.TryParse(cellValue, out _);
				result = isNumeric && cellValue.Length == EXFTA_ZZDCodeLength;
			}
			return result;
		}

		public static void UpdateAdditionally(RefCusCodeList cusCodeList)
		{
			if (cusCodeList != null && !string.IsNullOrEmpty(cusCodeList.ZZD_ZZK_NKCodeType))
			{
				if (cusCodeList.RefCusCodeListAttributes != null)
				{
					var codeTypeDataField = cusCodeList.ZZD_ZZK_NKCodeType;
					var refCusCodeListAttribute = cusCodeList.RefCusCodeListAttributes[0];

					if (codeTypeDataField == Constants.CodeTypes.EXFTA)
					{
						refCusCodeListAttribute.ZZE_ZXE_NKName = Constants.CodeListAttributeNames.FTATradeGroup;
					}
					else if (codeTypeDataField == Constants.CodeTypes.EXTPF)
					{
						refCusCodeListAttribute.ZZE_ZXE_NKName = Constants.CodeListAttributeNames.TradePreferenceTradeGroup;
					}
				}
			}
		}

		bool IAdditionalDataUpdater<RefCusCodeList>.IsDataRowValid(IRow row, EntityConfiguration configuration) => IsDataRowValid(row, configuration);
		void IAdditionalDataUpdater<RefCusCodeList>.UpdateAdditionally(RefCusCodeList cusCodeList, IRow row, EntityConfiguration configuration) => UpdateAdditionally(cusCodeList);
		void IAdditionalDataUpdater<RefCusCodeList>.UpdateRule(RefCusCodeList cusCodeList, Rule rule) { }
		void IAdditionalDataUpdater<RefCusCodeList>.RegisterUpdated(IRow row, EntityConfiguration configuration) { }
	}
}
