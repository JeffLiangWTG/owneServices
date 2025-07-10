using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IDailyStatementDutyAndTax
	{
		ZString EntryFilerCode { get; }
		ZString EntryNumber { get; }
		ZString EntryType { get; }
		ZString DistrictPortOfEntrySummary { get; }
		ZDecimal EstimatedDutyAmount { get; }
		ZDecimal EstimatedTaxAmount { get; }
		ZString DeferredTaxIndicator { get; }
		ZDecimal CountervailingDutyAmount { get; }
		ZDecimal AntidumpingDutyAmount { get; }
		ZString BrokerReferenceNumber { get; }
		ZString MessageType { get; }
	}
}
