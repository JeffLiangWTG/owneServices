using System;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class GroupedMeasureConditionKey : IGroupedMeasureConditionKey
	{
		public string MeasureTypeId { get; }
		public string AdditionalCode { get; }
		public string OrderNumber { get; }
		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public string TradeGroup { get; }
		public string MeasureConditionCode { get; }
		public string Comment { get; }
		public bool IsImport { get; }

		public GroupedMeasureConditionKey(string measureTypeId, string additionalCode, string orderNumber, DateTime startDate, DateTime endDate, string tradeGroup, string measureConditionCode, string comment, bool isImport = true)
		{
			Argument.NotNullOrEmpty(measureTypeId, nameof(measureTypeId));
			Argument.NotNullOrEmpty(tradeGroup, nameof(tradeGroup));
			Argument.NotNullOrEmpty(measureConditionCode, nameof(measureConditionCode));

			this.MeasureTypeId = measureTypeId;
			this.AdditionalCode = additionalCode;
			this.OrderNumber = orderNumber;
			this.StartDate = startDate;
			this.EndDate = endDate;
			this.TradeGroup = tradeGroup;
			this.MeasureConditionCode = measureConditionCode;
			this.Comment = comment;
			IsImport = isImport;
		}
	}
}
