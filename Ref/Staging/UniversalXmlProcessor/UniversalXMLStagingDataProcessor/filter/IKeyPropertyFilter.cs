using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor
{
	interface IKeyPropertyFilter
	{
		IEnumerable<IStagingDataWrapper> GetFilteredEntities(IEnumerable<IStagingDataWrapper> entities);

		IEnumerable<object> GetFilteredSafeObjects(IEnumerable<object> safeObjects);
	}
}
