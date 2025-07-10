using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.SG.V4.Business
{
	#region DEC

	public interface ISGCUSDEC
	{
		#region ZBool

		ZBool IsDG { get; }
		ZBool IsAir { get; }
		ZBool IsContainerised { get; }
		ZBool IsExport { get; }
		ZBool HasOutwardTransport { get; }
		ZBool IsForStorage { get; }
		ZBool IsImport { get; }
		ZBool HasInwardTransport { get; }
		ZBool HasLiquorOrTobacco { get; }
		ZBool IsSea { get; }
		ZBool IsShortPayment { get; }
		ZBool IsExempted { get; }
		ZBool IsRecoveryPayment { get; }
		ZBool IsReleasedInLicensedPremiseExclBWCY { get; }
		ZBool IsStorageInFTZ { get; }
		ZBool IsStoredInLicensedPremise { get; }
		ZBool IsSeaStoreDeclaration { get; }
		ZBool IsInwardDeclaration { get; }
		ZBool IsOutwardDeclaration { get; }
		ZBool IsTranshipmentDeclaration { get; }

		#endregion

		#region ZDate

		ZDate ArrivalDate { get; }
		ZDate DepartureDate { get; }
		ZDate StartDateOfBlanket { get; }

		#endregion

		#region ZDecimal

		ZDecimal TotalDutyPayable { get; }
		ZDecimal TotalExcisePayable { get; }
		ZDecimal TotalOtherTaxPayable { get; }
		ZDecimal TotalGSTPayable { get; }
		ZDecimal TotalPayable { get; }
		ZDecimal TotalCustomsValue { get; }
		ZDecimal TotalGrossWeight { get; }
		ZDecimal TotalOuterPack { get; }

		#endregion

		#region ZInt

		ZInt InwardTransportCode { get; }
		ZInt NumberOfCrew { get; }
		ZInt NumberOfRequestsForUpdate { get; }
		ZInt OutwardTransportCode { get; }
		ZInt OutwardVesselNRT { get; }
		ZInt VoyageDuration { get; }

		#endregion

		#region ZString

		ZString BGIndicator { get; }
		ZString CargoPackingType { get; }
		ZString ClaimantCode { get; }
		ZString ClaimantName { get; }
		ZString CountryOfFinalDestination { get; }
		ZString DeclarationType { get; }
		ZString DeclarantId { get; }
		ZString FinalPortOfCall { get; }
		ZString InwardHouseBill { get; }
		ZString InwardMasterBill { get; }
		ZString InwardJourneyIdentifier { get; }
		ZString InwardTransportIdentifier { get; }
		ZString InwardVesselType { get; }
		ZString JobNumber { get; }
		ZString NextPortOfCall { get; }
		ZString OutwardHouseBill { get; }
		ZString OutwardMasterBill { get; }
		ZString OutwardJourneyIdentifier { get; }
		ZString OutwardTransportIdentifier { get; }
		ZString OutwardVesselNationality { get; }
		ZString OutwardVesselType { get; }
		ZString PlaceOfStorage { get; }
		ZString PortOfDischarge { get; }
		ZString PortOfLoading { get; }
		ZString PreviousPermitNumber { get; }
		ZString PermitNoToUpdateOrCancel { get; }
		ZString ReplacementPermitNumber { get; }
		ZString SupplyIndicator { get; }
		ZString TotalGrossWeightUnitOfQty { get; }
		ZString TotalOuterPackUnitOfQty { get; }
		ZString TowingVesselName { get; }
		ZString TowingVesselVoyageNo { get; }

		#endregion

		#region IOrganisation

		IOrganisation Claimant { get; }
		IOrganisation Importer { get; }
		IOrganisation Exporter { get; }
		IOrganisation FreightForwarder { get; }
		IOrganisation InwardCarrierAgent { get; }
		IOrganisation OutwardCarrierAgent { get; }
		IOrganisation HandlingAgent { get; }
		IOrganisation Consignee { get; }
		IOrganisation EndUser { get; }
		IOrganisation Manufacturer { get; }

		#endregion

		#region ISGPlace

		ISGCPlace InwardVesselBerth { get; }
		ISGCPlace OutwardVesselBerth { get; }
		ISGCPlace PlaceOfReceipt { get; }
		ISGCPlace PlaceOfRelease { get; }

		#endregion

		#region IEnumerable

		IEnumerable<ZString> AdditionalRecipients { get; }
		IEnumerable<ICusContainer> Containers { get; }
		IEnumerable<ICusDocument> LicencesAndDocuments { get; } // not more than 5
		IEnumerable<ICusInvoice> Invoices { get; }
		IEnumerable<ICusItem> Items { get; }
		IEnumerable<ICusCPC> CPCs { get; } // not more than 5
		IEnumerable<ZString> TradersRemarksForMessage { get; } // not more than 5

		#endregion

		#region Other

		ICusAgentInfo Declarant { get; }
		IAdditionalMessageInformation AdditionalMessageInformation { get; }
		IContainerSequenceStore GetPreviousMessageContainerSequence();

		#endregion
	}

	#endregion

	#region IPT

	public interface IIPTDEC : ISGCUSDEC
	{
	}

	public interface IIPTUPD : IIPTDEC
	{
	}

	#endregion

	#region INP

	public interface IINPDEC : ISGCUSDEC
	{
		ZBool IsTemporaryConsignment { get; }
		ZBool Is2bStoredBWCY { get; }
		ZBool Is2bStoredCfw { get; }
		ZBool Is2bStoredC2y { get; }
		ZBool GoodsImportedUnderMESorBWS { get; }

		ZDate StartDateOfTemporaryImport { get; }
		ZDate EndDateOfTemporaryImport { get; }
	}

	public interface IINPUPD : IINPDEC
	{
	}

	#endregion

	#region TNP

	public interface ITNPDEC : ISGCUSDEC
	{
		ZDate StartDateOfCargoRemoval { get; }
		ZBool Is2bStoredBWCY { get; }
	}

	public interface ITNPUPD : ITNPDEC
	{
	}

	#endregion

	#region OUT

	public interface IOUTDEC : ISGCUSDEC
	{
		ITCODEC CO { get; }

		ZBool IsReceiptInLicensedPremise { get; }
	}

	public interface IOUTUPD : IOUTDEC
	{
		ZString CertificateNumber { get; }
	}

	#endregion

	#region TCODEC

	public interface ITCODEC : ISGCUSDEC
	{
		ZString AdditionalInformation { get; }
		ZString ApplicationProductType { get; }
		ZString DonorCountryCode { get; }

		ZInt YearOfEntry { get; }

		ZBool SendInvoiceDetails { get; }

		ZInt PercCommContent1 { get; }
		ZInt NumberOfCopies1 { get; }
		ZInt NumberOfCopies2 { get; }

		ZString CertificateType1 { get; }
		ZString CertificateType2 { get; }
		ZString CurrencyCode { get; }
		ZString AdditionalDetails1 { get; }
		ZString TransportDetails1 { get; }

		IEnumerable<ICusCertItem> CertItems { get; }
	}

	#endregion

	public interface ICustomsDec : ISGCUSDEC, IINPDEC, IIPTDEC, IIPTUPD, IOUTDEC, ITCODEC, ITNPDEC, IINPUPD, IOUTUPD, ITNPUPD
	{
		CusEntryHeader.EntryTypes EntryType { get; }
	}

	public interface ICusAttachment
	{
		ZGuid UniqueIdentifier { get; }
		ZString FileName { get; }
		ZString DocType { get; }
	}

	public interface ICusContainer
	{
		ZDecimal ContainerWeight { get; } //length=3

		ZInt ContainerSize { get; } //length =2

		ZString ContainerNumber { get; }
		ZString ContainerType { get; } // FCL or LCL
		ZString ContainerWeightUnit { get; } //length=3
		ZString SealNumber { get; } // if not exists then Empty
	}

	public interface ICusDocument
	{
		ZString LicenceNumber { get; }
	}

	public interface IOrganisation
	{
		ZString UEN { get; }
		ZString Name { get; }
		IAddress Address { get; }
	}

	public interface IAddress
	{
		ZString FullAddress { get; }
		ZString FullAddressWithoutAdditionalInfo { get; }
		ZString City { get; }
		ZString PostCode { get; }
		ZString CountryCode { get; }
		ZString SubdivisionCode { get; }
		ZString SubdivisionName { get; }
	}

	public interface ICusAgentInfo
	{
		ZString EntityIdentifier { get; }
		ZString Name { get; }
		ZString Code { get; }
		ZString Passport { get; }
		ZString Phone { get; }
	}

	public interface ICusProductCode
	{
		ZString ProductCode { get; }
		ZDecimal ProductCodeQty { get; }
		ZString ProductCodeUnitType { get; }
	}

	public interface ISGCPlace
	{
		ZString Code { get; }
		ZString Type { get; }
		ZString NameAndAddress { get; }
		ZBool AddressRequired { get; }
		ZBool IsNonSystemNonLicenced { get; }
	}

	public interface IContainerSequenceStore
	{
		int GetSequenceNumber(string containerId);
		int HighestSequenceNumber { get; }
	}

	public interface ICusCPC
	{
		ZString APCCodeName { get; }
		IEnumerable<ICusProcessingCodes> PCOccurrences { get; } // not more than 5
	}

	public interface ICusProcessingCodes
	{
		ZString ProcessingCode1 { get; }
		ZString ProcessingCode2 { get; }
		ZString ProcessingCode3 { get; }
	}
}
