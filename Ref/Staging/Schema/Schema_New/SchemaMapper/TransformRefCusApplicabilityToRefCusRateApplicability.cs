using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public class TransformRefCusApplicabilityToRefCusRateApplicability : ISchemaMapper
{
	public (string originalName, string transformedName) NameMapper => (nameof(RefCusApplicability), nameof(RefCusRateApplicability));
	public (string originalTablePrefix, string transformedTablePrefix) TablePrefixMapper => (typeof(RefCusApplicability).GetTablePrefix(), typeof(RefCusRateApplicability).GetTablePrefix());
	public List<(string originalProperty, string transformedProperty)> PropertyMappers =>
	[
		(nameof(RefCusApplicability.ZZT_StartDate), nameof(RefCusRateApplicability.S01_StartDate)),
		(nameof(RefCusApplicability.ZZT_EndDate), nameof(RefCusRateApplicability.S01_EndDate)),
		(nameof(RefCusApplicability.ZZT_ZZA_NKTradeGroup), nameof(RefCusRateApplicability.S01_ZZA_NKTradeGroup)),
		(nameof(RefCusApplicability.ZZT_ZZA_ZZZ_NKDataGrouping), nameof(RefCusRateApplicability.S01_ZZA_ZZZ_NKDataGrouping)),
		(nameof(RefCusApplicability.ZZT_AdditionalCode), nameof(RefCusRateApplicability.S01_AdditionalCode)),
		(nameof(RefCusApplicability.ZZT_OrderNumber), nameof(RefCusRateApplicability.S01_OrderNumber)),
		(nameof(RefCusApplicability.ZZT_ZZA_NKSecondTradeGroup), nameof(RefCusRateApplicability.S01_ZZA_NKSecondTradeGroup)),
		(nameof(RefCusApplicability.ZZT_ZZA_ZZZ_NKSecondDataGrouping), nameof(RefCusRateApplicability.S01_ZZA_ZZZ_NKSecondDataGrouping)),
	];
}
