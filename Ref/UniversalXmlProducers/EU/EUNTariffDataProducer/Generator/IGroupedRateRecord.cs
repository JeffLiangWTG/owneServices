using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IGroupedRateRecord
	{
		string MeasureTypeId { get; }
		string AdditionalCode { get; }
		string OrderNumber { get; }
		string Rate { get; }
		DateTime StartDate { get; }
		DateTime EndDate { get; }
		string ReductionIndicator { get; }
		string RateCode { get; }
	}
}
