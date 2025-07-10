using System;
using System.IO;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public class OdataUriProvider : IOdataUriProvider
	{
		IMetadataProvider metadataProvider;

		public OdataUriProvider(IMetadataProvider metadataProvider)
		{
			Argument.NotNull(metadataProvider, nameof(metadataProvider));
			this.metadataProvider = metadataProvider;
		}

		public Uri GetUri(DbSource dbSource, Type entityType, Guid parentPK, DateTime? datetimeUTC = null)
		{
			Argument.NotNull(entityType, nameof(entityType));
			Argument.NotNull(parentPK, nameof(parentPK));
			var tablePrefix = entityType.GetTablePrefix();
			var previousPath = "";
			switch (dbSource)
			{
				case DbSource.RefDbRepoSafe:
					previousPath = Path.Combine(Constants.SafeUpdateServiceUri, $"{entityType.Name}Update/Default.GetWithOptimizedExpand()");
					previousPath += $"?$filter={tablePrefix}_PK eq {parentPK}";
					previousPath += $"&SystemVersionUTC={datetimeUTC ?? DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss.fffffffZ}";
					break;
				case DbSource.RefDbRepoStaging:
					previousPath = Path.Combine(Constants.StagingServiceUri, $"{entityType.Name}Update");
					previousPath += $"?$filter={tablePrefix}_PK eq {parentPK}";
					break;
			}
			var uri = OdataExpandHelper.Expand(entityType, metadataProvider, "");
			if (string.IsNullOrEmpty(uri))
			{
				uri = previousPath;
			}
			else
			{
				uri = $"{previousPath}&$expand={uri}";
			}
			return new Uri(uri);
		}
	}
}
