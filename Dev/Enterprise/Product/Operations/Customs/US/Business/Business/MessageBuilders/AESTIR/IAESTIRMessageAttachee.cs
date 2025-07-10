using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.MessageBuilders
{
	public interface IAESTIRMessageAttachee : IMessageAttachee
	{
		// SC1 Record
		ZString RelatedCompanyIndicator { get; }
		ZString ModeOfTransportationCodeMOT { get; }
		ZString CountryOfUltimateDestinationCode { get; }
		ZString USStateOfOriginCode { get; }
		ZString CarrierIDSCACIATA { get; }
		ZString ShipmentReferenceNumber { get; }
		ZString ConveyanceNameCarrierName { get; }
		ZString FilingOptionIndicator { get; }
		ZString AEIFilingType { get; }
		ZString PortOfUnladingCode { get; }
		ZString PortOfExportationCode { get; }
		ZDate EstimatedDateOfExport { get; }
		ZString HazardousMaterialIndicatorHAZMAT { get; }

		// SC2 Record
		ZString InbondCode { get; }
		ZString EntryNumber { get; }
		ZString ForeignTradeZoneIdentifier { get; }
		ZString RoutedExportTransactionIndicator { get; }
		ZString OriginalITN { get; }

		// SC3 Records
		IEnumerable<IAESTIRTransportationDetail> TransportationDetails { get; }

		// Parties
		IAESTIRParty USPPI { get; }
		ZString USPPIIRSNumber { get; }
		ZString USPPIIRSIDType { get; }

		IAESTIRParty ForwardingAgent { get; }

		IAESTIRParty UltimateConsignee { get; }
		ZString UltimateConsigneeType { get; }
		ZBool IsSoldEnRoute { get; }
		ZString CityOfFirstPortOfCall { get; }
		ZString CountryOfFirstPortOfCall { get; }

		IAESTIRParty IntermediateConsignee { get; }

		// Commodity Line Items
		IEnumerable<IAESTIRCommodityLineItem> CommodityLineItems { get; }
	}

	public interface IAESTIRTransportationDetail
	{
		// SC3 Record
		ZString EquipmentNumber { get; }
		ZString SealNumber { get; }
		ZString TransportationReferenceNumber { get; }
	}

	public interface IAESTIRParty
	{
		// N01 Record
		ZString PartyID { get; }
		ZString PartyIDType { get; }
		ZString PartyName { get; }
		ZString ContactFirstName { get; }
		ZString ContactMiddleInitial { get; }
		ZString ContactLastName { get; }

		// N02 Record
		ZString AddressLine1 { get; }
		ZString AddressLine2 { get; }
		ZString ContactPhoneNumber { get; }

		// N03 Record
		ZString City { get; }
		ZString StateCode { get; }
		ZString CountryCode { get; }
		ZString PostalCode { get; }
	}

	public interface IAESTIRCommodityLineItem
	{
		// CL1 Record
		ZString ExportInformationCode { get; }
		ZInt LineNumber { get; }
		ZString CommodityDescription { get; }
		ZDecimal LicenseValue { get; }
		ZString LicenseCodeLicenseExemptionCode { get; }
		ZString ForeignDomesticOriginIndicator { get; }

		// CL2 Record
		ZString ScheduleBHTSNumber { get; }
		ZString UnitOfMeasure1 { get; }
		ZDecimal Quantity1 { get; }
		ZDecimal ValueOfGoods { get; }
		ZString UnitOfMeasure2 { get; }
		ZDecimal Quantity2 { get; }
		ZDecimal ShippingWeight { get; }
		ZString ExportControlClassificationNumberECCN { get; }
		ZString ExportLicenseNumberCFRCitationAuthorizationSymbolKCP { get; }

		// ODT Record
		ZString DDTCITARExemptionNumber { get; }
		ZString DDTCRegistrationNumber { get; }
		ZString DDTCSignificantMilitaryEquipmentSMEIndicator { get; }
		ZString DDTCEligiblePartyCertificationIndicator { get; }
		ZString DDTCUSMLCategoryCode { get; }
		ZString DDTCUnitOfMeasureCode { get; }
		ZDecimal DDTCQuantity { get; }
		ZString DDTCCommodityJurisdictionNumber { get; }

		// Used Vehicles
		IEnumerable<IAESTIRUsedVehicle> UsedVehicles { get; }

		//PGA Record
		ZString AMSIndicator { get; }
		ZString EPAIndicator { get; }
		ZString NMFSIndicator { get; }
		ZString ATFIndicator { get; }
		ZString DEAIndicator { get; }
		ZString FWSIndicator { get; }
		ZString TTBIndicator { get; }

		IAESAMS ExportAMS { get; }
		IAESEPA ExportEPA { get; }
		IAESATF ExportATF { get; }
		IAESFWS ExportFWS { get; }

		IEnumerable<IAESNMFS> ExportNMFSLines { get; }
		IEnumerable<IAESDEA> ExportDEALines { get; }
		IEnumerable<IAESTTB> ExportTTBLines { get; }
	}

	public interface IAESTIRUsedVehicle
	{
		// EV1 Record
		ZString VehicleIdentificationNumberVINProductID { get; }
		ZString VehicleIDQualifier { get; }
		ZString VehicleTitleNumber { get; }
		ZString VehicleTitleStateCode { get; }
	}

	public interface IAESAMS
	{
		ZString ExportCertificateNo { get; }
	}

	public interface IAESEPA
	{
		ZString EPAConsentNumber { get; }
		ZString HazWasteManifestTrackingNumber { get; }
		ZDecimal EPANetQuantity { get; }
		ZString EPANetQuantityUQ { get; }
	}

	public interface IAESNMFS
	{
		ZString Description { get; }
		ZString ProgramCode { get; }
		ZString ProcessingTypeCode { get; }
		ZString DocumentType { get; }
		ZString DocumentNumber { get; }
		ZString SourceCountry { get; }
		ZString HarvestingCountry { get; }
		ZString GeographicLocation { get; }
		ZString PermitNumber { get; }
		ZDecimal Quantity { get; }
		ZString UQ { get; }
		ZString CatchDocumentNumber { get; }
		ZString ReExportNumber { get; }
		ZString DocumentImageSent { get; }
	}

	public interface IAESATF
	{
		ZString FFLNumber { get; }
		ZString FFLExemptionCode { get; }
		ZString PermitNumber { get; }
		ZString PermitExemptionCode { get; }
		ZDecimal Quantity { get; }
		ZString CategoryCode { get; }
		ZString Description { get; }
	}

	public interface IAESDEA
	{
		ZString DrugCode { get; }
		ZDecimal Quantity { get; }
		ZString UnitOfMeasure { get; }
		ZString TransactionType { get; }
		ZString PermitNumber { get; }
		ZString RegistrationNumber { get; }
	}

	public interface IAESFWS
	{
		ZString EDecsConfirmation { get; }
		ZString TaxonomicSerialNumber { get; }
		ZString PurposeCode { get; }
		ZString DescriptionCode { get; }
		ZString SpeciesOrigin { get; }
		ZString SourceCode { get; }
		ZString ExemptionCertification { get; }
		ZString WildlifeCategoryCode { get; }
		ZString StateCode { get; }
		ZString CommercialDescription { get; }
	}

	public interface IAESTTB
	{
		ZString IRCNumber { get; }
		ZDate Date { get; }
		ZString SerialNumber { get; }
		ZString Disclaimer { get; }
	}
}
