namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public interface IServiceFactory
	{
		ISafeDataProvider GetSafeDataProvider(ICacheProvider cacheProvider);
		IStagingDataWrapperFactory GetStagingDataWrapperFactory(ISafeDataProvider safeDataProvider, IMetadataProvider metadataProvider, IStagingDataProvider stagingProvider);
		ISafeObjectUpdater GetSafeObjectUpdater(ISafeDataProvider safeDataProvider, IMetadataProvider metadataProvider, bool isDeletionType);
		IStagingDataProvider GetStagingDataProvider(bool autoDetectChanges = false, int? timeOut = null);
		IOverlappingCalculator OverlappingCalculator { get; }
	}
}
