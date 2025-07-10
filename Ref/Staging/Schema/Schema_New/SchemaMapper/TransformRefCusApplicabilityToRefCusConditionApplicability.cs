using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public class TransformRefCusApplicabilityToRefCusConditionApplicability : ISchemaMapper
{
	public (string originalName, string transformedName) NameMapper => (nameof(RefCusApplicability), nameof(RefCusConditionApplicability));

	public (string originalTablePrefix, string transformedTablePrefix) TablePrefixMapper => (typeof(RefCusApplicability).GetTablePrefix(), typeof(RefCusConditionApplicability).GetTablePrefix());

	public List<(string originalProperty, string transformedProperty)> PropertyMappers =>
	[
		(nameof(RefCusApplicability.ZZT_StartDate), nameof(RefCusConditionApplicability.S07_StartDate)),
		(nameof(RefCusApplicability.ZZT_EndDate), nameof(RefCusConditionApplicability.S07_EndDate)),
		(nameof(RefCusApplicability.ZZT_ZZA_NKTradeGroup), nameof(RefCusConditionApplicability.S07_ZZA_NKTradeGroup)),
		(nameof(RefCusApplicability.ZZT_ZZA_ZZZ_NKDataGrouping), nameof(RefCusConditionApplicability.S07_ZZA_ZZZ_NKDataGrouping)),
		(nameof(RefCusApplicability.ZZT_AdditionalCode), nameof(RefCusConditionApplicability.S07_AdditionalCode)),
		(nameof(RefCusApplicability.ZZT_OrderNumber), nameof(RefCusConditionApplicability.S07_OrderNumber)),
		(nameof(RefCusApplicability.ZZT_ZZA_NKSecondTradeGroup), nameof(RefCusConditionApplicability.S07_ZZA_NKSecondTradeGroup)),
		(nameof(RefCusApplicability.ZZT_ZZA_ZZZ_NKSecondDataGrouping), nameof(RefCusConditionApplicability.S07_ZZA_ZZZ_NKSecondDataGrouping)),
	];
}
