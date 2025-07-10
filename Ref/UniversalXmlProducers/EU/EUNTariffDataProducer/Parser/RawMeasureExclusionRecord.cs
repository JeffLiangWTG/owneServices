using System;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class RawMeasureExclusionRecord : IRawMeasureExclusionRecord
	{
		public string TariffHeader { get; }
		public string AdditionalCode { get; }
		public string OrderNumber { get; }
		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public string Description { get; }
		public string Description2 { get; }
		public string TradeGroup { get; }
		public string MeasureTypeId { get; }
		public string ExcludedTradeGroup { get; }

		public RawMeasureExclusionRecord(string tariffHeader, string additionalCode, string orderNumber, DateTime startDate, DateTime endDate,
			string description, string description2, string tradeGroup, string measureTypeId, string excludedTradeGroup)
		{
			Argument.NotNullOrEmpty(tariffHeader, nameof(tariffHeader));
			Argument.NotNullOrEmpty(tradeGroup, nameof(tradeGroup));
			Argument.NotNullOrEmpty(excludedTradeGroup, nameof(excludedTradeGroup));

			TariffHeader = tariffHeader;
			AdditionalCode = additionalCode;
			OrderNumber = orderNumber;
			StartDate = startDate;
			EndDate = endDate;
			Description = description;
			Description2 = description2;
			TradeGroup = tradeGroup;
			MeasureTypeId = measureTypeId;
			ExcludedTradeGroup = excludedTradeGroup;
		}
	}
}
