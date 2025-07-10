using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public class TransformRefCusRateUOMToRefCusRateApplicabilityUOM : ISchemaMapper
{
	public (string originalName, string transformedName) NameMapper => (nameof(RefCusRateUOM), nameof(RefCusRateApplicabilityUOM));
	public (string originalTablePrefix, string transformedTablePrefix) TablePrefixMapper => (typeof(RefCusRateUOM).GetTablePrefix(), typeof(RefCusRateApplicabilityUOM).GetTablePrefix());

	public List<(string originalProperty, string transformedProperty)> PropertyMappers =>
	[
		(nameof(RefCusRateUOM.ZXG_UOM), nameof(RefCusRateApplicabilityUOM.S02_UOM)),
	];
}
