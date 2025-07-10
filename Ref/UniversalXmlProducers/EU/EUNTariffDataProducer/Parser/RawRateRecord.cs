using System;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class RawRateRecord : IRawRateRecord
	{
		public string TariffHeader { get; }
		public string AdditionalCode { get; }
		public string OrderNumber { get; }
		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public string Description { get; }
		public string Description2 { get; }
		public string LegalBase { get; }
		public string TradeGroup { get; }
		public string MeasureTypeId { get; }
		public string Rate { get; }
		public string ReductionIndicator { get; }
		public string RateCode { get; }
		public bool IsImport { get; }

		public RawRateRecord(string tariffHeader, string additionalCode, string orderNumber, DateTime startDate, DateTime endDate,
			string description, string description2, string legalBase, string tradeGroup, string measureTypeId, string rate,
			string reductionIndicator, bool isImport = true, string rateCode = null)
		{
			Argument.NotNullOrEmpty(tariffHeader, nameof(tariffHeader));
			Argument.NotNullOrEmpty(tradeGroup, nameof(tradeGroup));

			TariffHeader = tariffHeader;
			AdditionalCode = additionalCode;
			OrderNumber = orderNumber;
			StartDate = startDate;
			EndDate = endDate;
			Description = description;
			Description2 = description2;
			LegalBase = legalBase;
			TradeGroup = tradeGroup;
			MeasureTypeId = measureTypeId;
			Rate = rate;
			ReductionIndicator = reductionIndicator;
			RateCode = rateCode ?? ApplicationConfig.GetRateCodeByMeasureTypeID(measureTypeId);
			IsImport = isImport;
		}
	}
}
