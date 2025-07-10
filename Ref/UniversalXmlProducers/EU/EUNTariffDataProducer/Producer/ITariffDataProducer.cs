using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface ITariffDataProducer
	{
		IEnumerable<RefCusTariff> LoadAllTariffRates();
		IEnumerable<RefCusTariff> LoadTariffRates(IEnumerable<IWebTariffHeader> tariffHeaders);
	}
}
