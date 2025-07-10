using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	static class OrgCusCodeListHelpers
	{
		public static void InsertDefaultOrgCusCodes(CodeDescriptionPairList list)
		{
			// Default list
			list.AddPair(OrgCusCode.CodeTypes.GovBusinessCode, Res.GetString("Organisation|CustomsCodes|GovBusinessCode", "Government Business Code"));
			list.AddPair(OrgCusCode.CodeTypes.CorporationCode, Res.GetString("Organisation|CustomsCodes|CorporationCode", "Government Corporation Code"));
			list.AddPair(OrgCusCode.CodeTypes.TaxFileCode, Res.GetString("Organisation|CustomsCodes|TaxFileCode", "Government Tax File Code"));
			list.AddPair(OrgCusCode.CodeTypes.CustomsClientCode, Res.GetString("Organisation|CustomsCodes|CustomsClientCode", "Customs Client Code"));
			list.AddPair(OrgCusCode.CodeTypes.SupplierCode, Res.GetString("Organisation|CustomsCodes|SupplierCode", "Customs Supplier Code"));
			list.AddPair(OrgCusCode.CodeTypes.ControlledPremisesID, Res.GetString("Organisation|CustomsCodes|ControlledPremisesID", "Customs Controlled Premises Code"));
			list.AddPair(OrgCusCode.CodeTypes.BrokerageRegistration, Res.GetString("Organisation|CustomsCodes|BrokerageRegistration", "Customs Brokerage Registration Code"));
			list.AddPair(OrgCusCode.CodeTypes.BrokerageSiteID, Res.GetString("Organisation|CustomsCodes|BrokerageSiteID", "Customs Brokerage Site Code"));
			list.AddPair(OrgCusCode.CodeTypes.BrokeragePrinter, Res.GetString("Organisation|CustomsCodes|BrokeragePrinter", "Customs Brokerage Printer Code"));
			list.AddPair(OrgCusCode.CodeTypes.CarrierCode, Res.GetString("Organisation|CustomsCodes|CarrierCode", "Customs Carrier Code"));
			list.AddPair(OrgCusCode.CodeTypes.ManifestProviderID, Res.GetString("Organisation|CustomsCodes|ManifestProviderID", "Customs Manifest Provider Code"));
			list.AddPair(OrgCusCode.CodeTypes.LegacySystemCode, Res.GetString("Organisation|CustomsCodes|LegacySystemCode", "Legacy System Code"));
			list.AddPair(OrgCusCode.CodeTypes.DeliveranceCode, Res.GetString("Organisation|CustomsCodes|DeliveranceCode", "Deliverance System Code"));
			list.AddPair(OrgCusCode.CodeTypes.PassportID, Res.GetString("Organisation|CustomsCodes|PassportID", "Passport Number"));
			list.AddPair(OrgCusCode.CodeTypes.DriverLicenceID, Res.GetString("Organisation|CustomsCodes|DriverLicenceID", "Driver's License Number"));
			list.AddPair(OrgCusCode.CodeTypes.ContainerChainCommunityCode, Res.GetString("Organisation|CustomsCodes|ContainerChainCommunityCode", "Container Chain community code"));

			// The extra codes added afterwards, effectively default
			list.AddPair(OrgCusCode.CodeTypes.UniversalNettingCode, Res.GetString("OrgCusCode.CodeTypes.UniversalNettingCode", "Universal Netting Code"));
			list.AddPair(OrgCusCode.CodeTypes.UniversalOfficeCode, Res.GetString("OrgCusCode.CodeTypes.UniversalOfficeCode", "Universal Office Code"));
			list.AddPair(OrgCusCode.CodeTypes.GlobalTrackingName, Res.GetString("OrgCusCode.CodeTypes.GlobalTrackingName", "Global Tracking Name"));
			list.AddPair(OrgCusCode.CodeTypes.EDISiteID, Res.GetString("OrgCusCode.CodeTypes.EDISiteID", "EDI Site ID"));
			list.AddPair(OrgCusCode.CodeTypes.AccountsPayableSuppliersReference, Res.GetString("OrgCusCode.CodeTypes.AccountsPayableSuppliersReference", "Accounts Payable Suppliers Reference"));
			list.AddPair(OrgCusCode.CodeTypes.CarrierPrincipalCode, Res.GetString("OrgCusCode.CodeTypes.CarrierPrincipalCode", "Shipping Company Carrier/Principal Code"));
			list.AddPair(OrgCusCode.CodeTypes.InntraCode, Res.GetString("OrgCusCode.CodeTypes.InntraCode", "INTTRA Code"));
			list.AddPair(OrgCusCode.CodeTypes.ContainerManagementMessagingAgreedCode, Res.GetString("OrgCusCode.CodeTypes.ContainerManagementMessagingAgreedCode", "Container Management Messaging Agreed Code"));
			list.AddPair(OrgCusCode.CodeTypes.SEPACreditorIdentifier, Res.GetString("OrgCusCode.CodeTypes.SEPACreditorIdentifier", "SEPA Creditor Identifier"));
			list.AddPair(OrgCusCode.CodeTypes.PIMAAddress, Res.GetString("OrgCusCode.CodeTypes.PIMAAddress", "Participant Identification and Messaging Address"));
			list.AddPair(OrgCusCode.CodeTypes.ExternalDebtorAccountCode, Res.GetString("OrgCusCode.CodeTypes.ExternalDebtorAccountCode", "External Debtor Account"));
			list.AddPair(OrgCusCode.CodeTypes.ExternalCreditorAccountCode, Res.GetString("OrgCusCode.CodeTypes.ExternalCreditorAccountCode", "External Creditor Account"));
			list.AddPair(OrgCusCode.CodeTypes.CreditAgencyCode, Res.GetString("OrgCusCode.CodeTypes.CreditAgencyCode", "Credit Agency Code"));
			list.AddPair(OrgCusCode.CodeTypes.GS1, Res.GetString("OrgCusCode.CodeTypes.GS1", "GS1 Company Prefix"));
			list.AddPair(OrgCusCode.CodeTypes.RegulatedAgentID, Res.GetString("OrgCusCode.CodeTypes.RegulatedAgentID", "Regulated Agent Unique Identifier"));
			list.AddPair(OrgCusCode.CodeTypes.EHubOrganisationID, Res.GetString("7164c4f7-507f-4688-bc9c-b2aabf43c418", "E-Hub Organization Code"));
			list.AddPair(OrgCusCode.CodeTypes.RoadCarrierRegistration, Res.GetString("AB9550F0-FD66-4B8F-ABCB-08E868D98206", "Road Carrier Registration Number"));
			list.AddPair(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, Res.GetString("12345678-FD66-4B8F-ABCB-08E868D98206", "TIR Carnet holder ID number"));
			list.AddPair(OrgCusCode.CodeTypes.PalletTradingAccountChep, Res.GetString("20e50924-a6cd-4a36-8fac-c464017b3538", "Pallet Trading Account Number - CHEP"));
			list.AddPair(OrgCusCode.CodeTypes.PalletTradingAccountLoscam, Res.GetString("0124202c-3793-4ebe-8a05-8e3ce4674555", "Pallet Trading Account Number - LOSCAM"));
			list.AddPair(OrgCusCode.CodeTypes.CargoWiseOneCarrierCode, Res.GetString("f1472d0f-bdcc-4e57-b9fd-39de7184cddf", "{0} Carrier Code", "CargoWise"));
			list.AddPair(OrgCusCode.CodeTypes.DomesticCarrierCode, Res.GetString("OrgCusCode.CodeTypes.DomesticCarrierCode", "Domestic Carrier Code"));
			list.AddPair(OrgCusCode.CodeTypes.NorthAmericanIndustryClassificationSystem, Res.GetString("E96D88CD-5E16-4371-B74F-11FDEB6828AC", "North American Industry Classification System (NAICS)"));
			list.AddPair(OrgCusCode.CodeTypes.StandardIndustrialClassification, Res.GetString("D71FFD76-B76A-441A-86E3-058B2B2F9373", "Standard Industrial Classification"));
			list.AddPair(OrgCusCode.CodeTypes.JNP, Res.GetString("e78d0361-ee48-46f4-8c9e-2733c0afa370", "Operational Job Number Prefix for Carrier/Agent"));
			list.AddPair(OrgCusCode.CodeTypes.WorldCargoAssociationNumber, Res.GetString("D6AE0AD6-E704-402D-B86B-AF2E1D5323F3", "World Cargo Association Number"));
			list.AddPair(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, Res.GetString("OrgCusCode.CodeTypes.DataUniversalNumberingSystem", "DUNS Data Universal Numbering System (Dun & Bradstreet)"));
			list.AddPair(OrgCusCode.CodeTypes.PortSystemNumber, Res.GetString("0B281EC1-115B-4E9B-AC15-4BF0112AA674", "Port System Number"));
			list.AddPair(OrgCusCode.CodeTypes.PortServiceReference, Res.GetString("D0926813-4398-4B52-82BF-907E61CCAB44", "Port Service Reference"));
			list.AddPair(OrgCusCode.CodeTypes.CargoWiseRoadTransportProviderCode, Res.GetString("5c2f1d3c-022e-49c5-92b3-35f4af901eff", "CargoWise Road Transport Provider Code"));
			list.AddPair(OrgCusCode.CodeTypes.NVOCCReference, Res.GetString("74C9362D-BA35-4EAA-A381-9FA1977ADEF7", "NVOCC Reference"));
			list.AddPair(OrgCusCode.CodeTypes.BoleroTitleRegisterID, Res.GetString("C59680C2-355C-47B4-BA57-488CC5039AAD", "Bolero Entity Identifier"));
			list.AddPair(OrgCusCode.CodeTypes.SpecialEconomicZone, Res.GetString("OrgCusCode.CodeTypes.SpecialEconomicZone", "Special Economic Zone"));
		}

		public static void InsertUsaOrTerritoryCodes(CodeDescriptionPairList list)
		{
			list.AddPair(OrgCusCode.USACodeTypes.ABIRoutingCode, Res.GetString("OrgCusCode.USACodeTypes.ABIRoutingCode", "ABI Routing Code"));
			list.AddPair(OrgCusCode.USACodeTypes.ACEAssignedNumber, Res.GetString("OrgCusCode.USACodeTypes.ACEAssignedNumber", "ACE Assigned Number"));
			list.AddPair(OrgCusCode.USACodeTypes.APHISAssignedNumber, Res.GetString("OrgCusCode.USACodeTypes.APHISAssignedNumber", "APHIS Establishment Number"));
			list.AddPair(OrgCusCode.USACodeTypes.FreeAndSecureTradeCode, Res.GetString("OrgCusCode.USACodeTypes.FreeAndSecureTradeCode", "FAST (Free and Secure Trade)"));
			list.AddPair(OrgCusCode.USACodeTypes.ForeignRegistrationNumber, Res.GetString("OrgCusCode.USACodeTypes.ForeignRegistrationNumber", "Foreign Registration Number"));
			list.AddPair(OrgCusCode.USACodeTypes.EncryptedConsigneeNumber, Res.GetString("OrgCusCode.USACodeTypes.EncryptedConsigneeNumber", "Encrypted Consignee Number"));
			list.OverridePair(OrgCusCode.CodeTypes.CarrierCode, Res.GetString("OrgCusCode.CodeTypes.CarrierCodeStandardUS", "Standard Carrier Alpha Code (Sea)"));
			list.AddPair(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, Res.GetString("OrgCusCode.CodeTypes.FDAEstablishmentIdentifier", "FDA Establishment Identifier"));
			list.AddPair(OrgCusCode.USACodeTypes.FIRMSCode, Res.GetString("12cfc9f2-8bdd-4612-8ecb-32cf100df7e3", "FIRMS Code"));
			list.AddPair(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, Res.GetString("OrgCusCode.USACodeTypes.EmployerIdentificationNumber", "Employer Identification Number"));
			list.AddPair(OrgCusCode.USACodeTypes.DataUniversalNumberingSystemPlus4, Res.GetString("OrgCusCode.USACodeTypes.DataUniversalNumberingSystemPlus4", "DUNS Data Universal Numbering System + 4 (Dun & Bradstreet)"));
			list.AddPair(OrgCusCode.USACodeTypes.SocialSecurityNumber, Res.GetString("OrgCusCode.USACodeTypes.SocialSecurityNumber", "Social Security Number"));
			list.AddPair(OrgCusCode.USACodeTypes.CBPAssignedNumber, Res.GetString("OrgCusCode.USACodeTypes.CBPAssignedNumber", "CBP Assigned Number"));
			list.AddPair(OrgCusCode.USACodeTypes.CBPAssignedSuretyCode, Res.GetString("OrgCusCode.USACodeTypes.CBPAssignedSuretyCode", "CBP Assigned Surety Code"));
			list.AddPair(OrgCusCode.USACodeTypes.ManufacturerID, Res.GetString("OrgCusCode.USACodeTypes.ManufacturerID", "Supplier/Manufacturer ID Number"));
			list.AddPair(OrgCusCode.USACodeTypes.FAAIndirectCarrierNumber, Res.GetString("OrgCusCode.USACodeTypes.FAAIndirectCarrierNumber", "FAA Indirect Air Carrier Number"));
			list.AddPair(OrgCusCode.USACodeTypes.CTPAT, Res.GetString("OrgCusCode.USACodeTypes.CTPAT", "Customs-Trade Partnership Against Terrorism Status Verification Interface"));
			list.AddPair(OrgCusCode.USACodeTypes.NMFCParticipant, Res.GetString("OrgCusCode.USACodeTypes.NMFCParticipant", "NMFC Participant"));
			list.AddPair(OrgCusCode.USACodeTypes.AlcoholImportLicence, Res.GetString("OrgCusCode.USACodeTypes.AlcoholImportLicence", "Alcohol Import License"));
			list.AddPair(OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber, Res.GetString("OrgCusCode.USACodeTypes.FoodFacilityRegistrationNumber", "Food Facility Registration Number"));
			list.AddPair(OrgCusCode.USACodeTypes.ForeignProducerIdentifier, Res.GetString("OrgCusCode.USACodeTypes.ForeignProducerIdentifier", "Foreign Producer Identifier"));
			list.AddPair(OrgCusCode.USACodeTypes.ForeignProducerIdentifierBeer, Res.GetString("OrgCusCode.USACodeTypes.ForeignProducerIdentifierBeer", "Foreign Producer Identifier - Beer"));
			list.AddPair(OrgCusCode.USACodeTypes.ForeignProducerIdentifierSpirits, Res.GetString("OrgCusCode.USACodeTypes.ForeignProducerIdentifierSpirits", "Foreign Producer Identifier - Spirits"));
			list.AddPair(OrgCusCode.USACodeTypes.ForeignProducerIdentifierWine, Res.GetString("OrgCusCode.USACodeTypes.ForeignProducerIdentifierWine", "Foreign Producer Identifier - Wine"));
			list.AddPair(OrgCusCode.USACodeTypes.FederalMaritimeCommissionNumber, Res.GetString("OrgCusCode.USACodeTypes.FederalMaritimeCommissionNumber", "Federal Maritime Commission Number"));
			list.AddPair(OrgCusCode.USACodeTypes.CustomsHouseBrokerLicenseNumber, Res.GetString("OrgCusCode.USACodeTypes.CustomsHouseBrokerLicenseNumber", "Customs House Broker License Number"));
			list.AddPair(OrgCusCode.USACodeTypes.ShipperRegistrationNumber, Res.GetString("OrgCusCode.USACodeTypes.ShipperRegistrationNumber", "Shipper Registration Number"));
			list.AddPair(OrgCusCode.USACodeTypes.EntryFilerCode, Res.GetString("OrgCusCode.USACodeTypes.EntryFilerCode", "Entry Filer Code"));
			list.AddPair(OrgCusCode.USACodeTypes.CertifiedCargoScreening, Res.GetString("485cafd0-cbe1-4305-845f-7d4bb6bbf3ad", "Certified Cargo Screening"));
			list.AddPair(OrgCusCode.USACodeTypes.DDTCRegistrationNumber, Res.GetString("C2B23B90-356C-4647-8A03-120BF75A228B", "DDTC Registration Number"));
			list.AddPair(OrgCusCode.USACodeTypes.TTBPermitNumber, Res.GetString("69A05846-D033-4536-A0E7-3623A6685729", "TTB Import Permit Number"));
			list.AddPair(OrgCusCode.USACodeTypes.TTIRegistrationNumber, Res.GetString("0A2DC5CC-9EDA-4A4D-91F9-38DF74C6C057", "TTB IRC Registration Number"));
			list.AddPair(OrgCusCode.USACodeTypes.TTEPermitNumber, Res.GetString("BB2BEC8B-E8CD-469A-9C84-A95540F0F861", "TTB Export Permit Number"));
			list.AddPair(OrgCusCode.USACodeTypes.CPSCAccreditedLabId, Res.GetString("F2F41761-A11D-4A9D-BB43-EC5DBD37D170", "CPSC Accredited Lab ID"));
			list.AddPair(OrgCusCode.CodeTypes.DEA, Res.GetString("70F634ED-0990-4620-BE1A-8A526D5647C6", "DEA Registration Number"));
			list.AddPair(OrgCusCode.USACodeTypes.IFTPPermitNumber, Res.GetString("88E6A04F-7D14-4C10-857E-C247986C2BFA", "IFTP Permit No\\Auth. To Fish No (NMFS)"));
			list.AddPair(OrgCusCode.USACodeTypes.LegalEntityIdentifier, Res.GetString("BAAE5710-5308-4E9C-837D-978B9BAAF527", "GLEIF (Global Legal Entity Identifier Foundation Number)"));
			list.AddPair(OrgCusCode.USACodeTypes.GlobalLocationNumber, Res.GetString("1F330BA3-16C4-41CF-B07C-C55A75C282E8", "GS1 (Global Location Number)"));
		}
	}
}
