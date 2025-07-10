using System.Collections.Generic;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IPreferenceMapper
	{
		IEnumerable<RefCusRate> SetPreferenceOnRates(IEnumerable<RefCusRate> originalRates);
		IEnumerable<RefCusCondition> SetPreferenceOnConditions(IEnumerable<RefCusCondition> originalConditions);
	}
}
