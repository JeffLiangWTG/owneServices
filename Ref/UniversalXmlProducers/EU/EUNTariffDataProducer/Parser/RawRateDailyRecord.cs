using System;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.EUNTariffDataProducer
{
	public class RawRateDailyRecord : IRawRateDailyRecord
	{
		public string TariffHeader { get; }
		public string AdditionalCode { get; }
		public string OrderNumber { get; }
		public DateTime StartDate { get; }
		public DateTime EndDate { get; }
		public string Description => string.Empty;
		public string Description2 => string.Empty;
		public string LegalBase { get; }
		public string TradeGroup { get; }
		public string MeasureTypeId { get; }
		public string Rate { get; }
		public string ReductionIndicator { get; }
		public string RateCode { get; }
		public bool IsImport { get; }
		public string OperationType { get; }
		public int SequenceNumber { get; }
		public string FileName { get; }

		public RawRateDailyRecord(string tariffHeader, string additionalCode, string orderNumber, DateTime startDate, DateTime endDate,
			string legalBase, string tradeGroup, string measureTypeId, string rate,
			string reductionIndicator, bool isImport,
			string operationType, int sequenceNumber,string fileName)
		{
			Argument.NotNullOrEmpty(tariffHeader, nameof(tariffHeader));
			Argument.NotNullOrEmpty(tradeGroup, nameof(tradeGroup));

			TariffHeader = tariffHeader;
			AdditionalCode = additionalCode;
			OrderNumber = orderNumber;
			StartDate = startDate;
			EndDate = endDate;
			LegalBase = legalBase;
			TradeGroup = tradeGroup;
			MeasureTypeId = measureTypeId;
			Rate = rate;
			ReductionIndicator = reductionIndicator;
			RateCode = ApplicationConfig.GetRateCodeByMeasureTypeID(measureTypeId);
			IsImport = isImport;
			OperationType = operationType;
			SequenceNumber = sequenceNumber;
			FileName = fileName;
		}
	}
}
