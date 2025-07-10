namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public interface INonPersistentObjectTransformer
	{
		SafeObjectUpdaterResult[] TransformNonPersistentObjects(SafeObjectUpdaterResult[] updaterResults);
	}
}
