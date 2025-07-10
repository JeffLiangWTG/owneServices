using System.Collections.Generic;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;
namespace CargoWise.RefDbRepo.KRReferenceData.Business.OGA
{
	public class SteelNomenclaturesAdditionalDataUpdater : IAdditionalDataUpdater<RefCusNomenclatureGroup>
	{
		public SteelNomenclaturesAdditionalDataUpdater()
		{
		}
		const string RefCusConditionValueCommentUS = "미국으로 수출하는 다음의 것에 대한 수출승인에 관한 권한을 대외무역법 시행령 제 91 조 제 7 항 제 1 호에 따라 한국철강협회장에게 위탁한다 .";
		const string RefCusConditionValueCommentUSAndEU = "미국 또는 유럽연합으로 수출하는 다음의 것에 대한 수출승인에 관한 권한을 대외무역법 시행령 제 91 조 제 7 항 제 1 호에 따라 한국철강협회장에게 위탁한다 .";
		const string RefCusConditionValueUS = "US";
		const string RefCusConditionValueEU = "EU";

		public static bool IsDataRowValid(IRow row, EntityConfiguration configuration)
		{
			var result = false;
			var columnIndex = configuration.EntityTypeExcelColumnMapping.EntityTypes.SelectMany(x => x.Properties.Where(y => y.Name == nameof(RefCusConditionValue.ZX3_Value))).FirstOrDefault()?.ExcelColumn ?? -1;
			if (columnIndex != -1)
			{
				var cell = row.GetCell(columnIndex);
				cell?.SetCellType(CellType.String);
				var cellValue = cell?.StringCellValue ?? string.Empty;
				result = cellValue == RefCusConditionValueCommentUS || cellValue == RefCusConditionValueCommentUSAndEU;
			}
			return result;
		}

		public static void UpdateAdditionally(RefCusNomenclatureGroup dataEntity)
		{
			if (dataEntity.RefCusConditions == null)
			{
				return;
			}
			foreach (var condition in dataEntity.RefCusConditions)
			{
				var conditionValue = condition.RefCusConditionValues.First();
				if (conditionValue.ZX3_Value == RefCusConditionValueCommentUS)
				{
					conditionValue.ZX3_Value = RefCusConditionValueUS;
				}
				else if (conditionValue.ZX3_Value == RefCusConditionValueCommentUSAndEU)
				{
					condition.RefCusConditionValues = new RefCusConditionValue[] { new RefCusConditionValue { ZX3_Value = RefCusConditionValueUS }, new RefCusConditionValue { ZX3_Value = RefCusConditionValueEU } };
				}
				else
				{
					conditionValue.ZX3_Value = "";
				}
			}
		}

		bool IAdditionalDataUpdater<RefCusNomenclatureGroup>.IsDataRowValid(IRow row, EntityConfiguration configuration) => IsDataRowValid(row, configuration);
		void IAdditionalDataUpdater<RefCusNomenclatureGroup>.UpdateAdditionally(RefCusNomenclatureGroup dataEntity, IRow row, EntityConfiguration configuration) => UpdateAdditionally(dataEntity);
		void IAdditionalDataUpdater<RefCusNomenclatureGroup>.UpdateRule(RefCusNomenclatureGroup dataEntity, Rule rule) { }
		void IAdditionalDataUpdater<RefCusNomenclatureGroup>.RegisterUpdated(IRow row, EntityConfiguration configuration) { }
	}
}
