using System;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public interface IOdataUriProvider
	{
		Uri GetUri(DbSource dbSource,Type entityType, Guid parentPK, DateTime? datetimeUTC = null);
	}

	public enum DbSource
	{
		RefDbRepoSafe,
		RefDbRepoStaging,
	}
}
