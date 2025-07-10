using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IRawRateRecord : IExcelDataRecord
	{
		string AdditionalCode { get; }
		string OrderNumber { get; }
		DateTime EndDate { get; }
		string Description { get; }
		string Description2 { get; }
		string LegalBase { get; }
		string TradeGroup { get; }
		string MeasureTypeId { get; }
		string Rate { get; }
		string ReductionIndicator { get; }
		string RateCode { get; }
		bool IsImport { get;}
	}
}
