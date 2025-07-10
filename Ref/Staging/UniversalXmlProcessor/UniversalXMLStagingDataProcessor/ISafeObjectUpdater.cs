namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public interface ISafeObjectUpdater
	{
		SafeObjectUpdaterResult[] Update(IStagingDataWrapper[] wrappers);
	}
}
