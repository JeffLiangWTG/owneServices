namespace Enterprise.Customs.Universal
{
	public class TvpConditionSelectionCriteria : TvpSelectionCriteria
	{
		public new const string QualifiedName = "dbo.TVP_ConditionSelectionCriteria_V2";

		public new class Columns : TvpSelectionCriteria.Columns
		{
			public const string IsImport = "IsImport";
			public const string IsExport = "IsExport";
			public const string ConditionClass = "ConditionClass";
			public const string ConditionType = "ConditionType";
		}
	}
}
