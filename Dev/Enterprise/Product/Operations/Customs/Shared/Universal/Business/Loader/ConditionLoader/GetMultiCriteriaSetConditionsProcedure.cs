namespace Enterprise.Customs.Universal
{
	public static class GetMultiCriteriaSetConditionsProcedure
	{
		public const string QualifiedName = "dbo.GetMultiCriteriaSetConditions";

		public static class Parameters
		{
			public const string CriteriaTvp = "@ConditionCriteriaTvp";
			public const string AdditionalCodesTvp = "@AdditionalCodesTvp";
			public const string SecondTradeGroupTvp = "@SecondTradeGroupTvp";
		}

		public static class Columns
		{
			public const string CriteriaId = "CriteriaId";
			public const string DataPk = "ConditionPk";
		}
	}
}
