namespace CargoWise.RefDbRepo.NZReferenceData.Business
{
	public interface IBuilder<TProcessingData>
	{
		bool Build(IDataRepo dataRepo, BuildersFilePath[] filePaths, TProcessingData processingData);
	}

	public class BuildersFilePath
	{
		public BuildersFilePath(string filePath, BuilderFilePathSymbol symbol = BuilderFilePathSymbol.Default)
		{
			FilePath = filePath;
			Symbol = symbol;
		}

		public string FilePath { get; }

		public BuilderFilePathSymbol Symbol { get; }
	}

	public enum BuilderFilePathSymbol
	{
		Default,
		ConcessionDetail,
		ConcessionOverride,
		ConcessionRate,
		ConcessionToTariff,
		ConsolidatedListOfApprovalsJson,
		Levy,
		LevyFormula,
		PublicationTime,
		TariffDetail,
		TariffOverride
	}
}
