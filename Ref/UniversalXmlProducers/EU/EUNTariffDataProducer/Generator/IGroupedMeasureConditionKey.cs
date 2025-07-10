using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IGroupedMeasureConditionKey
	{
		string MeasureTypeId { get; }
		string AdditionalCode { get; }
		string OrderNumber { get; }
		DateTime StartDate { get; }
		DateTime EndDate { get; }
		string TradeGroup { get; }
		string MeasureConditionCode { get; }
		string Comment { get; }
		bool IsImport { get; }
	}
}
