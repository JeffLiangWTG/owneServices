using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public class TransformRefCusConditionValueToRefCusConditionApplicabilityValue : ISchemaMapper
{
	public (string originalName, string transformedName) NameMapper => (nameof(RefCusConditionValue), nameof(RefCusConditionApplicabilityValue));
	public (string originalTablePrefix, string transformedTablePrefix) TablePrefixMapper => (typeof(RefCusConditionValue).GetTablePrefix(), typeof(RefCusConditionApplicabilityValue).GetTablePrefix());
	public List<(string originalProperty, string transformedProperty)> PropertyMappers =>
	[
		(nameof(RefCusConditionValue.ZX3_Value), nameof(RefCusConditionApplicabilityValue.S08_Value)),
		(nameof(RefCusConditionValue.ZX3_LogicalORWithinGroup), nameof(RefCusConditionApplicabilityValue.S08_LogicalORWithinGroup)),
		(nameof(RefCusConditionValue.ZX3_ZX4_ZZZ_NKDataGrouping), nameof(RefCusConditionApplicabilityValue.S08_ZX4_ZZZ_NKDataGrouping)),
		(nameof(RefCusConditionValue.ZX3_ZX4_NKValueType), nameof(RefCusConditionApplicabilityValue.S08_ZX4_NKValueType)),
	];
}
