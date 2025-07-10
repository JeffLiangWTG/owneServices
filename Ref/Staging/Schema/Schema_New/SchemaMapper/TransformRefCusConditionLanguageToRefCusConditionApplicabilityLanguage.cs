using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.Schema_New;

public class TransformRefCusConditionLanguageToRefCusConditionApplicabilityLanguage : ISchemaMapper
{
	public (string originalName, string transformedName) NameMapper => (nameof(RefCusConditionLanguage), nameof(RefCusConditionApplicabilityLanguage));
	public (string originalTablePrefix, string transformedTablePrefix) TablePrefixMapper => (typeof(RefCusConditionLanguage).GetTablePrefix(), typeof(RefCusConditionApplicabilityLanguage).GetTablePrefix());
	public List<(string originalProperty, string transformedProperty)> PropertyMappers =>
	[
		(nameof(RefCusConditionLanguage.ZXJ_ZX6_NKLanguage), nameof(RefCusConditionApplicabilityLanguage.S09_ZX6_NKLanguage)),
		(nameof(RefCusConditionLanguage.ZXJ_Comment), nameof(RefCusConditionApplicabilityLanguage.S09_Comment)),
		(nameof(RefCusConditionLanguage.ZXJ_Source), nameof(RefCusConditionApplicabilityLanguage.S09_Source)),
		(nameof(RefCusConditionLanguage.ZXJ_AdditionalComment), nameof(RefCusConditionApplicabilityLanguage.S09_AdditionalComment)),
	];
}
