namespace Enterprise.Customs.Universal;

public class TvpTariffAdditionalCodeSelectionCriteria : TvpSelectionCriteria
{
	public new const string QualifiedName = "dbo.TVP_TariffAdditionalCodeSelectionCriteria";

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Column string")]
	public new class Columns : TvpSelectionCriteria.Columns
	{
		public const string Category = "Category";
	}
}

