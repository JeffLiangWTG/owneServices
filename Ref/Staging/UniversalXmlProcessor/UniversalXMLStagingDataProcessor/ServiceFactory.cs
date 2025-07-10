using System;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.Staging.Schema_New;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "<Pending>")]
	public class ServiceFactory : IServiceFactory
	{
		public ServiceFactory(string safeUpdateServiceUri, bool inclusiveEndDate, IAccessTokenProvider accessTokenProvider)
		{
			Argument.NotNullOrEmpty(safeUpdateServiceUri, nameof(safeUpdateServiceUri));
			this.safeUpdateServiceUri = safeUpdateServiceUri;
			OverlappingCalculator = new OverlappingCalculator(inclusiveEndDate);
			this.accessTokenProvider = accessTokenProvider;
		}

		readonly string safeUpdateServiceUri;
		readonly IAccessTokenProvider accessTokenProvider;

		public IOverlappingCalculator OverlappingCalculator { get; }

		public ISafeDataProvider GetSafeDataProvider(ICacheProvider cacheProvider)
		{
			Argument.NotNull(cacheProvider, nameof(cacheProvider));
			return new SafeDataProvider(new SafeRepository(new Uri(safeUpdateServiceUri), false, accessTokenProvider), cacheProvider, OverlappingCalculator);
		}

		public IStagingDataWrapperFactory GetStagingDataWrapperFactory(ISafeDataProvider safeDataProvider, IMetadataProvider metadataProvider, IStagingDataProvider stagingProvider)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			Argument.NotNull(stagingProvider, nameof(stagingProvider));
			return new StagingDataWrapperFactory(safeDataProvider, metadataProvider, stagingProvider, OverlappingCalculator);
		}

		public ISafeObjectUpdater GetSafeObjectUpdater(ISafeDataProvider safeDataProvider, IMetadataProvider metadataProvider, bool isDeletionType)
		{
			Argument.NotNull(safeDataProvider, nameof(safeDataProvider));
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			return new SafeObjectUpdater(safeDataProvider, metadataProvider, OverlappingCalculator, isDeletionType);
		}

		public IStagingDataProvider GetStagingDataProvider(bool autoDetectChanges = false, int? timeOut = null)
		{
			var repo = new StagingRepository(Constants.ConnectionStrings, autoDetectChanges, [new SqlDurationInterceptor()]) { CommandTimeout = timeOut };
			return new StagingDataProvider(repo);
		}
	}
}
