using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer
{
	public interface IDateTimeProvider
	{
		DateTime Now { get; }
	}
}
