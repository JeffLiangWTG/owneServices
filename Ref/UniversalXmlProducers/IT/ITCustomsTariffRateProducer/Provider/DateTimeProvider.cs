using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.ITCustomsTariffRateProducer
{
	public sealed class DateTimeProvider : IDateTimeProvider
	{
		DateTime IDateTimeProvider.Now => DateTime.Now;
	}
}
