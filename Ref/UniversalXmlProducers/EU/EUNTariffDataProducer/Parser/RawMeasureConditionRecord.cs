using System;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class RawMeasureConditionRecord : IRawMeasureConditionRecord
	{
		public string TariffHeader { get; }
		public DateTime StartDate { get; }
		public string AdditionalCode { get; }
		public string OrderNumber { get; }
		public DateTime EndDate { get; }
		public string TradeGroup { get; }
		public string MeasureTypeId { get; }
		public string MeasureConditionCode { get; }
		public string CertificateTypeCode { get; }
		public string ConditionAmount { get; }
		public string MonetaryUnitCode { get; }
		public string MeasureUnit { get; }
		public string MeasureAction { get; }
		public string Comment { get; }
		public bool IsImport { get; }

		public RawMeasureConditionRecord(string tariffHeader, string additionalCode, string orderNumber, DateTime startDate,
			DateTime endDate, string tradeGroup, string measureTypeId, string measureConditionCode, string certificateTypeCode,
			string conditionAmount, string monetaryUnitCode, string measureUnit, string measureAction, string comment, bool isImport = true)
		{
			Argument.NotNullOrEmpty(tariffHeader, nameof(tariffHeader));
			Argument.NotNullOrEmpty(tradeGroup, nameof(tradeGroup));
			Argument.NotNullOrEmpty(measureTypeId, nameof(measureTypeId));
			Argument.NotNullOrEmpty(measureConditionCode, nameof(measureConditionCode));
			Argument.NotNullOrEmpty(measureAction, nameof(measureAction));

			TariffHeader = tariffHeader;
			AdditionalCode = additionalCode;
			OrderNumber = orderNumber;
			StartDate = startDate;
			EndDate = endDate;
			TradeGroup = tradeGroup;
			MeasureTypeId = measureTypeId;
			MeasureConditionCode = measureConditionCode;
			CertificateTypeCode = certificateTypeCode;
			ConditionAmount = conditionAmount;
			MonetaryUnitCode = monetaryUnitCode;
			MeasureUnit = measureUnit;
			MeasureAction = measureAction;
			Comment = comment;
			IsImport = isImport;
		}
	}
}
