namespace CargoWise.RefDbRepo.Staging.Schema_New;

public static class SchemaMapperHelper
{
	public static ISchemaMapper[] GetSchemaMappers_RateAndApplicabilityGroup()
	{
		return
		[
			new TransformRefCusRateToRefCusRateApplicability(),
			new TransformRefCusApplicabilityToRefCusRateApplicability(),
			new TransformRefCusRateUOMToRefCusRateApplicabilityUOM(),
			new TransformRefCusExcludedTradeGroupToRefCusExcludedTradeGroupNew(),
		];
	}

	public static ISchemaMapper[] GetSchemaMappers_ConditionAndApplicabilityGroup()
	{
		return
		[
			new TransformRefCusConditionToRefCusConditionApplicability(),
			new TransformRefCusApplicabilityToRefCusConditionApplicability(),
			new TransformRefCusExcludedTradeGroupToRefCusExcludedTradeGroupNew(),
			new TransformRefCusConditionValueToRefCusConditionApplicabilityValue(),
			new TransformRefCusConditionLanguageToRefCusConditionApplicabilityLanguage(),
		];
	}

	public static (string originalName, string transformedName) RateWithoutApplicabilityNameMapper => (nameof(RefCusRate), nameof(RefCusRateWithoutApplicability));
	public static (string originalName, string transformedName) ConditionWithoutApplicabilityNameMapper => (nameof(RefCusCondition), nameof(RefCusConditionWithoutApplicability));
}
