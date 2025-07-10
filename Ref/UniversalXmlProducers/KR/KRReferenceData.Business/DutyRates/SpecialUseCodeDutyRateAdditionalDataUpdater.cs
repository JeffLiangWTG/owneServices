using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.KRReferenceData.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public class SpecialUseCodeDutyRateAdditionalDataUpdater : IAdditionalDataUpdater<RefCusTariff>
	{
		public static bool IsDataRowValid(IRow row, EntityConfiguration configuration)
		{
			var result = false;
			var tariffColumnIndex = configuration.EntityTypeExcelColumnMapping.EntityTypes.SelectMany(x => x.Properties.Where(y => y.Name == nameof(RefCusTariff.ZZ1_TariffCode))).FirstOrDefault()?.ExcelColumn ?? -1;
			var specialUseCodeColumnIndex = configuration.EntityTypeExcelColumnMapping.EntityTypes.SelectMany(x => x.Properties.Where(y => y.Name == SpecialUseCodeColumnName)).FirstOrDefault()?.ExcelColumn ?? -1;
			if (tariffColumnIndex != -1 && specialUseCodeColumnIndex != -1)
			{
				var tariffCellValue = GetColumnValueFromRow(tariffColumnIndex, row);
				var specialUseCodeCellValue = GetColumnValueFromRow(specialUseCodeColumnIndex, row);

				result = tariffCellValue.Length == Constants.TariffLength && !string.IsNullOrEmpty(specialUseCodeCellValue);
			}
			return result;
		}

		const string SpecialUseCodeColumnName = "SpecialUseCode";

		public void UpdateAdditionally(RefCusTariff tariff, IRow row, EntityConfiguration configuration)
		{
			var conditionEntityType = configuration.EntityTypeExcelColumnMapping.EntityTypes.FirstOrDefault(x => x.Name == nameof(RefCusCondition));

			var preference = string.Empty;
			var specialUseCode = string.Empty;
			foreach (var property in conditionEntityType.Properties)
			{
				if (property.Name == nameof(RefCusCondition.ZX1_ZZS_NKPreference))
				{
					preference = GetColumnValueFromRow(property.ExcelColumn, row);
				}
				else if (property.Name == SpecialUseCodeColumnName)
				{
					specialUseCode = GetColumnValueFromRow(property.ExcelColumn, row);
				}
			}
			var startDate = Helper.GetDateFromRow(row, conditionEntityType, nameof(RefCusCondition.ZX1_StartDate));
			var currentRefCusCondition = tariff.RefCusConditions.LastOrDefault(x => x.ZX1_ZZS_NKPreference == preference && x.ZX1_StartDate == startDate.Date);
			if (currentRefCusCondition != null)
			{
				currentRefCusCondition.RefCusConditionValues = new RefCusConditionValue[]
				{
					new RefCusConditionValue { ZX3_Value = specialUseCode }
				};
			}
		}

		static string GetColumnValueFromRow(int columnIndex, IRow row)
		{
			var cell = row.GetCell(columnIndex);
			cell?.SetCellType(CellType.String);
			return cell?.StringCellValue ?? string.Empty;
		}

		void IAdditionalDataUpdater<RefCusTariff>.UpdateAdditionally(RefCusTariff dataEntity, IRow row, EntityConfiguration configuration) => UpdateAdditionally(dataEntity, row, configuration);
		void IAdditionalDataUpdater<RefCusTariff>.UpdateRule(RefCusTariff dataEntity, Rule rule) { }
		bool IAdditionalDataUpdater<RefCusTariff>.IsDataRowValid(IRow row, EntityConfiguration configuration) => IsDataRowValid(row, configuration);
		void IAdditionalDataUpdater<RefCusTariff>.RegisterUpdated(IRow row, EntityConfiguration configuration) { }
		protected virtual ISafeRepository SafeRepository => new SafeRepository(new Uri(ApplicationConfig.SafeDataUpdateUri));
		protected virtual RefDataEntityLoader RefDataEntityLoader => new RefDataEntityLoader(SafeRepository);
	}
}
