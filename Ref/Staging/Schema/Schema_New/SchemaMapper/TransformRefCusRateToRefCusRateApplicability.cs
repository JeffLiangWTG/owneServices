using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public class TransformRefCusRateToRefCusRateApplicability : ISchemaMapper
{
	public (string originalName, string transformedName) NameMapper => (nameof(RefCusRate), nameof(RefCusRateApplicability));
	public (string originalTablePrefix, string transformedTablePrefix) TablePrefixMapper => (typeof(RefCusRate).GetTablePrefix(), typeof(RefCusRateApplicability).GetTablePrefix());

	public List<(string originalProperty, string transformedProperty)> PropertyMappers =>
	[
		(nameof(RefCusRate.ZZ2_StartDate), nameof(RefCusRateApplicability.S01_StartDate)),
		(nameof(RefCusRate.ZZ2_EndDate), nameof(RefCusRateApplicability.S01_EndDate)),
		(nameof(RefCusRate.ZZ2_ZY1_NKRateCode), nameof(RefCusRateApplicability.S01_ZY1_NKRateCode)),
		(nameof(RefCusRate.ZZ2_ZY1_ZZR_NKRateType), nameof(RefCusRateApplicability.S01_ZY1_ZZR_NKRateType)),
		(nameof(RefCusRate.ZZ2_ZY1_ZZR_ZZZ_NKDataGrouping), nameof(RefCusRateApplicability.S01_ZY1_ZZR_ZZZ_NKDataGrouping)),
		(nameof(RefCusRate.ZZ2_RateFormula), nameof(RefCusRateApplicability.S01_RateFormula)),
		(nameof(RefCusRate.ZZ2_ZZS_NKPreference), nameof(RefCusRateApplicability.S01_ZZS_NKPreference)),
		(nameof(RefCusRate.ZZ2_ZZS_ZZZ_NKDataGrouping), nameof(RefCusRateApplicability.S01_ZZS_ZZZ_NKDataGrouping)),
		(nameof(RefCusRate.ZZ2_SelectorFormula), nameof(RefCusRateApplicability.S01_SelectorFormula)),
		(nameof(RefCusRate.ZZ2_ZZZ_NKDataGrouping), nameof(RefCusRateApplicability.S01_ZZZ_NKDataGrouping)),
		(nameof(RefCusRate.ZZ2_RateFormulaDerivedFrom), nameof(RefCusRateApplicability.S01_RateFormulaDerivedFrom)),
		(nameof(RefCusRate.ZZ2_RX_NKCurrencyOverride), nameof(RefCusRateApplicability.S01_RX_NKCurrencyOverride)),
	];
}
