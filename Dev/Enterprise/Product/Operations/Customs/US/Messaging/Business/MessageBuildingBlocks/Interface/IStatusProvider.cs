using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks
{
	public interface IInBondStatusProvider
	{
		ZString InBondStatus { get; }
		ZDate InBondArrivalDate { get; }
		ZDate InBondExportDate { get; }
		ZString InbondEntryType { get; }
	}

	public interface IDispositionDetailProvider
	{
		ZString DispositionCode { get; }
		ZDateTime DispositionDateTime { get; }
		ZString NarrativeMessage { get; }
		ZString ReleaseOrigin { get; }
		ZDateTime ReleaseDateTime { get; }
		ZString DocumentType { get; }
	}

	public interface IReleaseDetailProvider : IDispositionDetailProvider
	{
		ZInt Sequence { get; }
		ZInt Quantity { get; }
	}

	public interface IPGADispositionProvider
	{
		ZString GovernmentAgencyProgramCode { get; }
		ZString BeginningCBPLineNo { get; }
		ZString PGALineDispositionCode { get; }
		ZString ReviewReasonCode { get; }
		ZDateTime DispositionDateTime { get; }
		ZString OtherAgencyQuotaIdentifier { get; }
		ZString EntryDispositionCode { get; }
		ZString NarrativeMessage { get; }
		ZString EntryLineDispositionCode { get; }
		ZString BeginningOGALineNo { get; }
		ZString RangeIndicator { get; }
		ZString EndingCBPLineNo { get; }
		ZString EndingOGALineNo { get; }
		ZString DocumentTypeCode { get; }
		ZString PGAEntryHoldType { get; }
		ZString BeginningTariffPosition { get; }
		ZString EndingTariffPosition { get; }
		ZString PGAProcessingGroupVersion { get; }
	}

	public interface IReferenceDataProvider
	{
		ZString ReferenceIdentifierQualifier { get; }
		ZString ReferenceIdentifier { get; }
	}

	public interface IPGADispositionDetailProvider
	{
		ZString ReferenceIDQualifier { get; }
		ZString ReferenceID { get; }
		ZDateTime ReceiptDateTime { get; }
		IEnumerable<ZString> SubReasonCodes { get; }
	}

	public interface IPGADispositionComments
	{
		ZString CommentsToTradeFromPGA { get; }
	}

	public interface IManifestInfoProvider
	{
		ZString InBondNumber { get; }
		ZString MasterBillNumber { get; }
		ZString HouseBillNumber { get; }
		ZString SubHouseBillNumber { get; }
		ZDecimal ManifestQuantity { get; }
		ZString Unit { get; }
		ZString IssuerCodeOfMasterBillNumber { get; }
		ZString IssuerCodeOfHouseBillNumber { get; }
		ZString BillOfLadingType { get; }
		ZString ImporterSecurityFilingIndicator { get; }
		ZString ModeOfTransportationCode { get; }
	}
}
