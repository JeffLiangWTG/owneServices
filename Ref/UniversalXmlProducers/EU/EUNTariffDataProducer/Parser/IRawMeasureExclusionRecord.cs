using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IRawMeasureExclusionRecord : IExcelDataRecord
	{
		string AdditionalCode { get; }
		string OrderNumber { get; }
		DateTime EndDate { get; }
		string Description { get; }
		string Description2 { get; }
		string TradeGroup { get; }
		string MeasureTypeId { get; }
		string ExcludedTradeGroup { get; }
	}
}
