using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business
{
	/// <summary>
	/// CusEntryHeader 
	/// </summary>
	public interface IInBondQPHeader : IMessageAttacheeWithCBPSenderReference
	{
		ZString EntryType { get; }
		ZString InBondNumber { get; }
		ZString InbondCarrierSCAC { get; }
		ZString USDestination { get; }
		ZString ForeignDestination { get; }
		ZDecimal Value { get; }
		ZString InBondCarrierID { get; }
		ZBool FTZIndicator { get; }
		ZBool BTAIndicator { get; }
		ZBool IsAir { get; }

		ZString ImportingCarrierSCAC { get; }
		ZString ImportTransportMode { get; }
		ZString ImportingCarrierCountryCode { get; }
		ZString ImportingConveyanceName { get; }
		ZString ImportingCarrierVoyageNumber { get; }
		ZString PortOfUnlading { get; }
		ZDateTime ETAatUnlading { get; }
		ZString FTZFirmsCode { get; }
		ZString InbondCarrierSCACOrFirms { get; }

		IEnumerable<IInBondBillDetails> Bills { get; }

		IInBondBillDetails FindBillMatchingSeqNo(ZString sequenceNumber);
		IInBondBillDetails FindBillMatchingNumber(ZString masterBillNumber, ZString houseBillNumber);
		IInBondContainer FindContainer(ZString containerNumber, ZString billNumber);
		ZString JobNumber { get; }
	}

	public interface IInBondQPBill
	{
		IInBondQPHeader Header { get; }
	}

	public interface IIMessageAttacheeWithDisposition : IMessageAttachee
	{
		void UpdateDispositionInformation(ZString dispositionCode, ZDateTime dispositionDate);
		void MarkPreviousDispositionsInactive(ZDateTime dispositionDate);
	}

	/// <summary>
	/// Bill 
	/// </summary>
	public interface IInBondBillDetails : IIMessageAttacheeWithDisposition
	{
		ZString SequenceNumber { get; set; }
		ZString MasterBillIssuerSCAC { get; }
		ZString MasterBillNumber { get; }
		ZInt InBondQuantity { get; }
		ZString PreviousITNumber { get; }
		ZString PreviousITType { get; }
		ZBool IsDetailedInBond { get; }

		IEnumerable<ZString> SecondaryNotifyParties { get; }
		IEnumerable<IInBondBillReferenceNumber> RefNumbers { get; }

		ZString ForeignLadingPortLocalCode { get; }
		ZInt ManifestQuantity { get; }
		ZString ManifestUQ { get; }

		ZDecimal WeightInWholeNumber { get; }
		ZString WeightUQ { get; }
		ZDecimal VolumeInWholeNumber { get; }
		ZString VolumeUQ { get; }

		ZDecimal GoodsValueInLocalCurrency { get; }
		ZString PlaceOfPreReceipt { get; }

		JobDocAddress ForeignShipperAddress { get; }
		JobDocAddress ConsigneeAddress { get; }
		JobDocAddress NotifyPartyAddress { get; }
		ZString HouseBillNumber { get; }
		ZString HouseBillIssuerCode { get; }

		IEnumerable<IInBondLineDetailsHeader> LineDetailsHeaders { get; }
	}

	/// <summary>
	/// Bill 
	/// </summary>
	public interface IInBondBillReferenceNumber
	{
		ZString Qualifier { get; }
		ZString ReferenceIdentifier { get; }
	}

	public interface IInBondLineDetailsHeader
	{
		IInBondContainer Container { get; }

		IEnumerable<IInBondTariffLineDetails> TariffLines { get; }

		IEnumerable<IHazardousMaterial> HazardousLines { get; }
	}

	public interface IInBondContainer : IIMessageAttacheeWithDisposition
	{
		ZString ContainerNumber { get; }
		ZString SealNumber1 { get; }
		ZString SealNumber2 { get; }
		ZInt PieceCount { get; }
		ZString ContainerDescriptionCode { get; }
	}

	public interface IInBondTariffLineClassificationDetails
	{
		ZString TariffNumber { get; }
		ZDecimal CustomsValue { get; }
		ZDecimal NetWeight { get; }
		ZString NetWeightUQ { get; }
	}

	public interface IInBondTariffLineDetails : IInBondTariffLineClassificationDetails
	{
		ZDecimal PieceCount { get; }
		ZString ManifestUnitCode { get; }
		ZString CargoDescription { get; }
		ZString MarksAndNumbers { get; }

		IEnumerable<IInBondTariffLineClassificationDetails> MultiClassifications { get; }
	}

	/// <summary>
	/// Package
	/// </summary>
	public interface IInBondContainerMarksAndNumbers
	{
		ZString MarksAndNumbers { get; }
	}
}
