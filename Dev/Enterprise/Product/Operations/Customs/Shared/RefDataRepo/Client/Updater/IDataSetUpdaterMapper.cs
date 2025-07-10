using System.Collections.Generic;
using CargoWise.RefDbRepo.Client.Common;
using CargoWise.RefDbRepo.Common;
using Enterprise.Integration;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public interface IDataSetUpdaterMapper
	{
		IEnumerable<Customs.Shared.ISharedDataSetUpdater> GetDataSets();
		IDataSetUpdater GetDataSetUpdater(string updaterName);
		ISRDbDataSetUpdater GetSRDbDataSetUpdater(string updaterName);
		IDataSetUpdaterManager GetDataSetUpdaterManager(string updaterName);
		DataSetVersion GetDataSetVersion(string updaterName, string dataSetName);
	}
}
