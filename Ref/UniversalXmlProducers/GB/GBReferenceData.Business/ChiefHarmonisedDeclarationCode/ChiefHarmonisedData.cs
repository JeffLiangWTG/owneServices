namespace CargoWise.RefDbRepo.GBReferenceData.Business.ChiefHarmonisedDeclarationCode
{
	public class ChiefHarmonisedData
	{
		public ChiefHarmonisedData(string code, string importExport, string level, string description, string details)
		{
			Code = code;
			ImportExport = importExport;
			Level = level;
			Description = description;
			Details = details;
		}

		internal readonly string Code;

		internal readonly string ImportExport;

		internal readonly string Level;

		internal readonly string Description;

		internal readonly string Details;
	}
}
