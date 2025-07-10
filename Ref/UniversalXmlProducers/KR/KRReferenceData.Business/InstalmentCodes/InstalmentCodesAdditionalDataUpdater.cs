using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NPOI.SS.UserModel;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public sealed class InstalmentCodesAdditionalDataUpdater : IAdditionalDataUpdater<RefCusCodeList>
	{
		bool IsDataRowValid(IRow row, EntityConfiguration configuration)
		{
			var monthsValue = GetInstalmentCellValue(row, configuration, Constants.InstalmentMonthsSequence, nameof(RefCusCodeListAttribute.ZZE_Value));
			var frequencyValue = GetInstalmentCellValue(row, configuration, Constants.InstalmentFrequencySequence, nameof(RefCusCodeListAttribute.ZZE_Value));

			return (int.TryParse(monthsValue, out int months) ? months : 0) > 0 || (int.TryParse(frequencyValue, out int frequency) ? frequency : 0) > 0;
		}

		void UpdateAdditionally(RefCusCodeList dataEntity, IRow row, EntityConfiguration configuration)
		{
			var postClearanceProcedureValue = GetInstalmentCellValue(row, configuration, Constants.InstalmentPostClearanceProcedureSequence, Constants.InstalmentPostClearanceProcedure);

			if (postClearanceProcedureValue == Constants.Yes)
			{
				var refCusCodeListAttribute = new RefCusCodeListAttribute
				{
					ZZE_Value = postClearanceProcedureValue,
					ZZE_ZXE_NKName = Constants.InstalmentPostClearanceProcedureName
				};
				dataEntity.RefCusCodeListAttributes = dataEntity.RefCusCodeListAttributes.Append(refCusCodeListAttribute).ToArray();
			}
		}

		bool IAdditionalDataUpdater<RefCusCodeList>.IsDataRowValid(IRow row, EntityConfiguration configuration) => IsDataRowValid(row, configuration);
		void IAdditionalDataUpdater<RefCusCodeList>.UpdateAdditionally(RefCusCodeList dataEntity, IRow row, EntityConfiguration configuration) => UpdateAdditionally(dataEntity, row, configuration);
		void IAdditionalDataUpdater<RefCusCodeList>.UpdateRule(RefCusCodeList dataEntity, Rule rule) { }
		void IAdditionalDataUpdater<RefCusCodeList>.RegisterUpdated(IRow row, EntityConfiguration configuration) { }

		string GetInstalmentCellValue(IRow row, EntityConfiguration configuration, int sequence, string nameOfValue)
		{
			var result = string.Empty;
			var refCusCodeListAttributes = configuration.EntityTypeExcelColumnMapping.EntityTypes.Where(x => x.Name == nameof(RefCusCodeListAttribute));
			var index = refCusCodeListAttributes.FirstOrDefault(x => x.Sequence == sequence)?.Properties.FirstOrDefault(x => x.Name == nameOfValue)?.ExcelColumn ?? -1;
			
			if (index != -1)
			{
				var cell = row.GetCell(index);
				cell?.SetCellType(CellType.String);
				result = cell?.StringCellValue ?? string.Empty;
			}
			return result;
		}

		static class Constants
		{
			public const int InstalmentMonthsSequence = 1;
			public const int InstalmentFrequencySequence = 2;
			public const int InstalmentPostClearanceProcedureSequence = 3;
			public const string InstalmentPostClearanceProcedureName = "ISP";
			public const string InstalmentPostClearanceProcedure = "InstalmentPostClearanceProcedure";
			public const string Yes = "Y";
		}
	}
}
