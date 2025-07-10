using System.ComponentModel.DataAnnotations.Schema;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	[NotMapped]
	[NonPersistentObject]
	public class RefCusConditionWithoutApplicability:RefCusCondition
	{
		public RefCusConditionWithoutApplicability(RefCusCondition condition)
		{
			ZX1_ZX2_NKConditionType = condition.ZX1_ZX2_NKConditionType;
			ZX1_ZZ1_Tariff = condition.ZX1_ZZ1_Tariff;
			ZX1_ZZ5_Nomenclature = condition.ZX1_ZZ5_Nomenclature;
			ZX1_StartDate = condition.ZX1_StartDate;
			ZX1_EndDate = condition.ZX1_EndDate;
			ZX1_Source = condition.ZX1_Source;
			ZX1_Comment = condition.ZX1_Comment;
			ZX1_IsImport = condition.ZX1_IsImport;
			ZX1_IsExport = condition.ZX1_IsExport;
			ZX1_ConditionValueTrueMeansStop = condition.ZX1_ConditionValueTrueMeansStop;
			ZX1_ZZS_NKPreference = condition.ZX1_ZZS_NKPreference;
			ZX1_ZZZ_NKDataGrouping = condition.ZX1_ZZZ_NKDataGrouping;
			ZX1_LogicalANDWithinGroup = condition.ZX1_LogicalANDWithinGroup;
			ZX1_ZY7_NKConditionCode = condition.ZX1_ZY7_NKConditionCode;
			ZX1_AdditionalComment = condition.ZX1_AdditionalComment;
			ZX1_ZX2_ZZZ_NKDataGrouping = condition.ZX1_ZX2_ZZZ_NKDataGrouping;
			ZX1_ZZS_ZZZ_NKDataGrouping = condition.ZX1_ZZS_ZZZ_NKDataGrouping;
			ZX1_Severity = condition.ZX1_Severity;

			RefCusConditionValues = condition.RefCusConditionValues;
			RefCusConditionLanguages = condition.RefCusConditionLanguages;
			ZX1_ZZ1_TariffNavigation = condition.ZX1_ZZ1_TariffNavigation;
			ZX1_ZZ5_NomenclatureNavigation = condition.ZX1_ZZ5_NomenclatureNavigation;
		}
	}
}
