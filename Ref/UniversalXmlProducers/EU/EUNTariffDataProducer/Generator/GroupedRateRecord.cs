using System;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class GroupedRateRecord : IGroupedRateRecord
	{
		public string MeasureTypeId { get; }
		public string AdditionalCode { get; }
		public string OrderNumber { get; }
		public string Rate { get; }
		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public string ReductionIndicator { get; }
		public string RateCode { get; }

		public GroupedRateRecord(string measureTypeId, string additionalCode, string ordernumber, string rate, DateTime startDate, DateTime endDate, string reductionIndicator, string rateCode)
		{
			Argument.NotNullOrEmpty(measureTypeId, nameof(measureTypeId));
			Argument.NotNullOrEmpty(rate, nameof(rate));

			this.MeasureTypeId = measureTypeId;
			this.AdditionalCode = additionalCode;
			this.OrderNumber = ordernumber;
			this.Rate = rate;
			this.StartDate = startDate;
			this.EndDate = endDate;
			this.ReductionIndicator = reductionIndicator;
			this.RateCode = rateCode;
		}
	}
}
