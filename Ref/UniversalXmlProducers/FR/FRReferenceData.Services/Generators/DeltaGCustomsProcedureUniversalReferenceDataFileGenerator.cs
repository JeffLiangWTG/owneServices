namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public class DeltaGCustomsProcedureUniversalReferenceDataFileGenerator : CustomsProcedureUniversalReferenceDataFileGenerator
	{
		protected override string GetDefaultDataGrouping() => UniversalDataHelper.Constants.France;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1502:Avoid excessive complexity", Justification = "Just switch Case with lot of case.")]
		protected override string GetProcedureGroupCore(string procedureCode, string previousProcedureCode)
		{
			var group = HasEconomicImpact(procedureCode) || HasEconomicImpact(previousProcedureCode) ? $"{procedureCode}P" : procedureCode;
			return group;
		}

		public override string DataSource => "FR - Customs Procedures";
	}
}
