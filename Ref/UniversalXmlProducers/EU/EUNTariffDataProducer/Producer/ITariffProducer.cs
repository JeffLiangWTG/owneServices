using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface ITariffProducer
	{
		void Run(IEnumerable<RefCusTariff> tariffs);
		void Run(IEnumerable<string> tariffHeaders, ICollection<RefCusTariff> tariffCollection);
	}
}
