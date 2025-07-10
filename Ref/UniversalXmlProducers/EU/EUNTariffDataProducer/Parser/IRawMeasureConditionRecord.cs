using System;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public interface IRawMeasureConditionRecord : IExcelDataRecord
	{
		string AdditionalCode { get; }
		string OrderNumber { get; }
		DateTime EndDate { get; }
		string TradeGroup { get; }
		string MeasureTypeId { get; }
		string MeasureConditionCode { get; }
		string CertificateTypeCode { get; }
		string ConditionAmount { get; }
		string MonetaryUnitCode { get; }
		string MeasureUnit { get; }
		string MeasureAction { get; }
		string Comment { get; }
		bool IsImport { get; }
	}
}
