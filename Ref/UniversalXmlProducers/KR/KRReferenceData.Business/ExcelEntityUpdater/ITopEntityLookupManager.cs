using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;

namespace CargoWise.RefDbRepo.KRReferenceData.Business
{
	public interface ITopEntityLookupManager<T> where T : RefDataRepoModelEntityType
	{
		T GetTopEntity(Dictionary<string, T> topEntitiesByKey, KeyLookupDetails<T> keyDetails);
	}
}
