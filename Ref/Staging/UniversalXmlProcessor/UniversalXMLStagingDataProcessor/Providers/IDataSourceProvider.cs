using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	public interface IDataSourceProvider
	{
		bool IsAutoSchema { get; }
		bool IsFullUpdate { get; }
		bool IsDeletionType { get; }
		bool InclusiveEndDate { get; }
		string AppName { get; }
		IEnumerable<Dependency> Dependencies { get; }
		UpdateType GetUpdateType();
	}
}
