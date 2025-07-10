using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.SEReferenceData.Services;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNCommonImportTariffPopulator
{
	public interface IRateCreator
	{
		IEnumerable<RefCusRate> Get(measure measure, string rateCode, string rateType, IEnumerable<string> preferences);
	}
}
