namespace Enterprise.Customs.Universal
{
	public static class GetMultiCriteriaSetTariffAdditionalCodesProcedure
	{
		public const string QualifiedName = "dbo.GetMultiCriteriaSetTariffAdditionalCodes";

		public static class Parameters
		{
			public const string CriteriaTvp = "@TariffAdditionalCodeTvp";
		}

		public static class Columns
		{
			public const string CriteriaId = "CriteriaId";
			public const string DataPk = "TariffAdditionalCodePk";
		}
	}
}
