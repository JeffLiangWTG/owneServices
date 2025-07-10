using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public class TransformRefCusExcludedTradeGroupToRefCusExcludedTradeGroupNew : ISchemaMapper
{
	public (string originalName, string transformedName) NameMapper => (nameof(RefCusExcludedTradeGroup), nameof(RefCusExcludedTradeGroupNew));
	public (string originalTablePrefix, string transformedTablePrefix) TablePrefixMapper => (typeof(RefCusExcludedTradeGroup).GetTablePrefix(), typeof(RefCusExcludedTradeGroupNew).GetTablePrefix());

	public List<(string originalProperty, string transformedProperty)> PropertyMappers =>
	[
		(nameof(RefCusExcludedTradeGroup.ZZC_ZZA_NKTradeGroup), nameof(RefCusExcludedTradeGroupNew.S03_ZZA_NKTradeGroup)),
		(nameof(RefCusExcludedTradeGroup.ZZC_ZZA_ZZZ_NKDataGrouping), nameof(RefCusExcludedTradeGroupNew.S03_ZZA_ZZZ_NKDataGrouping)),
	];
}
