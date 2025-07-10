using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public class TransformRefCusConditionToRefCusConditionApplicability : ISchemaMapper
{
	public (string originalName, string transformedName) NameMapper => (nameof(RefCusCondition), nameof(RefCusConditionApplicability));
	public (string originalTablePrefix, string transformedTablePrefix) TablePrefixMapper => (typeof(RefCusCondition).GetTablePrefix(), typeof(RefCusConditionApplicability).GetTablePrefix());
	public List<(string originalProperty, string transformedProperty)> PropertyMappers =>
	[
		(nameof(RefCusCondition.ZX1_AdditionalComment), nameof(RefCusConditionApplicability.S07_AdditionalComment)),
		(nameof(RefCusCondition.ZX1_ZY7_NKConditionCode), nameof(RefCusConditionApplicability.S07_ZY7_NKConditionCode)),
		(nameof(RefCusCondition.ZX1_Comment), nameof(RefCusConditionApplicability.S07_Comment)),
		(nameof(RefCusCondition.ZX1_ConditionValueTrueMeansStop), nameof(RefCusConditionApplicability.S07_ConditionValueTrueMeansStop)),
		(nameof(RefCusCondition.ZX1_IsExport), nameof(RefCusConditionApplicability.S07_IsExport)),
		(nameof(RefCusCondition.ZX1_IsImport), nameof(RefCusConditionApplicability.S07_IsImport)),
		(nameof(RefCusCondition.ZX1_StartDate), nameof(RefCusConditionApplicability.S07_StartDate)),
		(nameof(RefCusCondition.ZX1_EndDate), nameof(RefCusConditionApplicability.S07_EndDate)),
		(nameof(RefCusCondition.ZX1_LogicalANDWithinGroup), nameof(RefCusConditionApplicability.S07_LogicalANDWithinGroup)),
		(nameof(RefCusCondition.ZX1_Source), nameof(RefCusConditionApplicability.S07_Source)),
		(nameof(RefCusCondition.ZX1_ZX2_NKConditionType), nameof(RefCusConditionApplicability.S07_ZX2_NKConditionType)),
		(nameof(RefCusCondition.ZX1_ZX2_ZZZ_NKDataGrouping), nameof(RefCusConditionApplicability.S07_ZX2_ZZZ_NKDataGrouping)),
		(nameof(RefCusCondition.ZX1_ZZS_NKPreference), nameof(RefCusConditionApplicability.S07_ZZS_NKPreference)),
		(nameof(RefCusCondition.ZX1_ZZS_ZZZ_NKDataGrouping), nameof(RefCusConditionApplicability.S07_ZZS_ZZZ_NKDataGrouping)),
		(nameof(RefCusCondition.ZX1_ZZZ_NKDataGrouping), nameof(RefCusConditionApplicability.S07_ZZZ_NKDataGrouping)),
		(nameof(RefCusCondition.ZX1_Severity), nameof(RefCusConditionApplicability.S07_Severity)),
	];
}
