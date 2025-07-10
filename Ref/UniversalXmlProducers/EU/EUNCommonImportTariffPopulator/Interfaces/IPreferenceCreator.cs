using System.Collections.Generic;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public interface IPreferenceCreator
	{
		IEnumerable<string> Get(measure measure);
	}
}
