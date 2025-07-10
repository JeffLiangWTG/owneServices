using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IAENQJC
	{
		ZString EntrySummaryControlStatus { get; }
		ZString EntrySummaryStatusCode { get; }
		ZDate EntrySummaryStatusDate { get; }
		ZString LateFilingStatusCode { get; }
		ZString ReleaseStatusCode { get; }
		ZDate ReleaseDate { get; }
		ZString CollectionStatusCode { get; }
		ZDate CollectionDate { get; }
		ZDate ExtensionSuspensionDate { get; }
		ZDate ExtensionSuspensionNoticeDate { get; }
		ZString CensusHeaderStatusCode { get; }
		ZString InvoiceStatusCode { get; }
		ZString ProtestStatusCode { get; }
		ZString QuotaStatusCode { get; }
		ZString TradeAgreementReconciliationFilerCode { get; }
		ZString TradeAgreementReconciliationEntryNumber { get; }
		ZString OtherReconciliationFilerCode { get; }
		ZString OtherReconciliationEntryNumber { get; }
		ZString ExtensionSuspensionStatusCode1 { get; }
		ZString ExtensionSuspensionStatusCode2 { get; }
		ZString ExtensionSuspensionStatusCode3 { get; }
		ZString ExtensionSuspensionStatusCode4 { get; }
	}
}
