using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business
{
	public interface IPGAContactDetails
	{
		ZString Name { get; }
		ZString PhoneNumber { get; }
		ZString EmailAddress { get; }
		ZString Fax { get; }
		IAddressDetails CompanyAddress { get; }
	}

	public interface IPGAContactDetailsWithID : IPGAContactDetails
	{
		ZString IDType { get; }
		ZString IDNumber { get; }
	}

	public interface ICustomsBrokerDetails
	{
		IAddressDetails Address { get; }
		ZString ContactName { get; }
		ZString ContactPhone { get; }
		ZString ContactEmail { get; }
	}

	public interface IGovernmentAgenciesIndicators
	{
		ZString ODSIndicator { get; }
		ZString ODSDisclaimReason { get; }

		ZString FSISIndicator { get; }
		ZString FSISDisclaimReason { get; }

		ZString VNEIndicator { get; }
		ZString VNEDisclaimReason { get; }

		ZString PSTIndicator { get; }
		ZString PSTDisclaimReason { get; }
		ZString PSTDisclaimProgram { get; }

		ZString NMFS370Indicator { get; }
		ZString NMFS370DisclaimReason { get; }
		ZString NMFSAMRIndicator { get; }
		ZString NMFSAMRDisclaimReason { get; }
		ZString NMFSHMSIndicator { get; }
		ZString NMFSHMSDisclaimReason { get; }
		ZString NMFSSIMIndicator { get; }
		ZString NMFSCOAIndicator { get; }

		ZString ACEFDAIndicator { get; }
		ZString ACEFDADisclaimReason { get; }

		ZString TSCAIndicator { get; }
		ZString TSCADisclaimReason { get; }

		ZString AMSIndicator { get; }
		ZString AMSDisclaimReason { get; }
		ZString AMSDisclaimProgram { get; }

		ZString NOPIndicator { get; }
		ZString NOPDisclaimReason { get; }

		ZString NHTSAIndicator { get; }
		ZString NHTSADisclaimReason { get; }

		ZString ATFIndicator { get; }

		ZString TTBIndicator { get; }
		ZString TTBDisclaimReason { get; }

		ZString OMCIndicator { get; }
		ZString OMCDisclaimReason { get; }

		ZString LaceyActIndicator { get; }
		ZString LaceyActDisclaimReason { get; }

		ZString APHISIndicator { get; }
		ZString APHISDisclaimReason { get; }

		ZString FWSIndicator { get; }
		ZString FWSDisclaimReason { get; }

		ZString DDTCIndicator { get; }

		ZString CPSCIndicator { get; }
		ZString CPSCDisclaimReason { get; }
		ZString DEAIndicator { get; }
		ZString DEADisclaimReason { get; }
		ZString HFCIndicator { get; }
		ZString HFCDisclaimReason { get; }
	}

	public interface IGovernmentAgencies : IGovernmentAgenciesIndicators
	{
		ZString CommercialDescription { get; }

		IEnumerable<IFSISLine> FSISLines { get; }
		IEnumerable<IVNEData> EPA_VNELines { get; }
		IEnumerable<IPSTData> EPA_PSTLines { get; }
		IEnumerable<IHFCHeader> EPA_HFCHeaders { get; }
		IEnumerable<INMFSLine> NMFS370Lines { get; }
		IEnumerable<INMFSLine> NMFSAMRLines { get; }
		IEnumerable<INMFSLine> NMFSHMSLines { get; }
		IEnumerable<INMFSLine> NMFSSIMLines { get; }
		IEnumerable<INMFSLine> NMFSCOALines { get; }
		IEnumerable<INHTSAHeader> NHTSALines { get; }
		IEnumerable<IAPHISHeader> APHISHeaders { get; }
		IDDTCData DDTCData { get; }
		IEnumerable<IFDAData> FDALines { get; }
		IEnumerable<IAMSData> AMSLines { get; }
		ITSCAData EPA_TSCAData { get; }
		IPGADataCorrection TSCADataCorrection { get; }
		IPGADataCorrection ODSDataCorrection { get; }

		IEnumerable<ILaceyActCommon> LaceyActData { get; }
		IEnumerable<IATFData> ATFLines { get; }
		IEnumerable<IOMCHeader> OMCHeaders { get; }

		IEnumerable<IFWSHeader> FWSHeaders { get; }

		IEnumerable<ITTBLine> TTBLines { get; }

		IEnumerable<ICPSCHeader> CPSCHeaders { get; }
		IEnumerable<IDEAHeader> DEAHeaders { get; }

		ZBool ShouldIncludePGAInMessage(ZBool isCertified, ZString pgaCode);
	}

	public interface ICPSCHeader : IPGADataCorrection
	{
		//PG01
		ZInt LineNo { get; set; }
		//Government Agency Code = CPS
		//Government Agency Program Code = CPS
		ZString ProcessingCode { get; }
		ZString ProductIDType { get; }
		ZString ProductID { get; }
		ZString IntendedUseCode { get; }
		ZString IntendedUseDescription { get; }

		//PG02
		ZString SKUProductCode { get; }
		ZString ProductCode { get; }
		ZString ProductCodeVersionNumber { get; }

		//PG07
		ZString BrandName { get; }
		ZString ProductName { get; }
		ZString ModelNumber { get; }
		ZString SerialNumber { get; }
		ZString RegisteredNumber { get; }
		ZString AltenateID { get; }
		ZString ManufacturerMonthAndYear { get; }

		//PG10
		ZString ModelColor { get; }
		ZString ModelDescription { get; }
		ZString ModelStyle { get; }

		//PG14
		ZString ReferenceNumber { get; }

		// PG19, PG20 and PG21
		IPGAContactDetails Manufacturer { get; }
		ZString ManufacturerEntityIdentificationCode { get; }
		ZString ManufacturerRegistryID { get; }
		IPGAContactDetails CertifyingEntity { get; }
		IPGAContactDetails ContactPoint { get; }

		//PG22
		ZString CertificateExists { get; }

		//PG19
		ZBool NoLabTestingRequired { get; }

		//PG25
		IEnumerable<ICPSCLot> Lots { get; }

		IEnumerable<ICPSCRulesAndLabs> RulesAndLabs { get; }

		//PG60
		ZString RuleCodes { get; }
	}

	public interface ICPSCLot
	{
		ZString Number { get; }
		ZString NumberType { get; }
		ZDateTime StartDate { get; }
		ZDateTime EndDate { get; }
	}

	public interface ICPSCRulesAndLabs
	{
		//PG19-21
		IPGAContactDetails SafetyTestLocation { get; }
		//PG19
		ZString CPSCAccreditedLabID { get; }
		//PG30
		ZDateTime PreviousInspectionDate { get; }
		//PG60
		ZString RuleCodes { get; }
		CPSCReportCollection ReportAndLabs { get; }
	}

	public interface IFWSHeader : IPGADataCorrection
	{
		// PG01
		ZInt LineNo { get; set; }
		// Government Agency Code = FWS
		ZString ProcessingCode { get; }
		ZBool IsDocSubmitted { get; }
		ZString ProductType { get; }
		ZString ProductNumber { get; }
		ZString IntendedUseCode { get; }

		// PG05
		ZString ScientificGenusName { get; }
		ZString ScientificSpeciesName { get; }
		ZString ScientificSubSpeciesName { get; }
		ZString ScientificSpeciesCode { get; }
		ZString FWSDescriptionCode { get; }

		// Second PG05 when hybrid
		ZString Scientific2GenusName { get; }
		ZString Scientific2SpeciesName { get; }
		ZString Scientific2SubSpeciesName { get; }

		// PG06
		// Source Type Code = 267
		ZString SourceCountryCode { get; }

		// PG10
		ZString CommodityQualifierCode { get; }
		ZString Hybrid { get; }

		// PG13 and PG14
		IEnumerable<IFWSLicense> Licenses { get; }

		// PG17
		ZString CommoditySpecificName { get; }
		ZString CommodityGeneralName { get; }
		ZString IsLiveVenomous { get; }
		ZShort CartonQty { get; }

		// PG19, PG20 and PG21
		IPGAContactDetails FWSImporter { get; }
		ZString FWSImporterFWE { get; }
		IPGAContactDetails FWSForeignExporter { get; }
		ZString FWSForeignExporterDUNS { get; }
		ICustomsBrokerDetails ContactDetails { get; }
		ICustomsBrokerDetails BrokerDetails { get; }
		ZString FilerAccountNumber { get; }

		// PG22
		ZString DeclarationCode { get; }
		ZDate CertifySignatureDate { get; set; }

		// PG24
		// Remarks Type Code = GEN
		ZString RemarksText { get; }

		// PG25
		ZDecimal PGALineValue { get; }

		// PG27
		IEnumerable<ZString> ContainerNumbers { get; }

		// PG29
		ZString NetCommodityUQ { get; }
		ZDecimal NetCommodityQty { get; }

		// PG30
		ZString FIRMS { get; }
		ZDate ArrivalDate { get; }
	}

	public interface IOMCHeader : IPGADataCorrection
	{
		//PG01
		ZInt LineNo { get; set; }
		ZBool ElectronicImageSubmitted { get; }

		//PG06
		ZString SourceCountry { get; }
		ZDateTime DepartureDate { get; }

		//PG19-21
		IPGAContactDetails Exporter { get; }
		IPGAContactDetails ResponsibleGovernmentOfficial { get; }
		IEnumerable<IPGAContactDetails> AquacultureFacilities { get; }
		ICustomsBrokerDetails ExporterPGAContactInformation { get; }
		ICustomsBrokerDetails GovOfficialPGAContactInformation { get; }

		//PG22
		ZString ConformanceDeclaration { get; }
		ZDateTime ExporterCertificationDate { get; }
		ZDateTime OfficialCertificationDate { get; }

		//PG29
		ZDecimal NetWeight { get; }
		ZString NetWeightUQ { get; }
	}

	public interface IFWSLicense
	{
		// PG14
		ZString Type { get; }
		ZString Number { get; }
	}

	public interface IAPHISHeader : IPGADataCorrection
	{
		// PG01
		ZInt LineNo { get; set; }
		ZString ProgramType { get; }
		ZBool IsDocSubmitted { get; }
		ZString ProcessingCode { get; }
		ZString IntendedUseCode { get; }
		ZString IntendedUseDescription { get; }

		// PG02
		ZString ProductCodeQualifier { get; }
		ZString ProductCodeNumber { get; }
		ZString StockKeepingUnitNumber { get; }

		// PG05
		ZString ScientificGenusName { get; }
		ZString ScientificSpeciesName { get; }
		ZString ScientificSubSpeciesName { get; }

		// PG06
		IEnumerable<ISource> Sources { get; }

		// PG10
		ZString CategoryTypeCode { get; }
		ZString CategoryCode { get; }
		IEnumerable<IAPHISProductCharacteristic> ProductCharacteristics { get; }

		IEnumerable<IAPHISProductComponent> ProductComponents { get; }

		// PG13 and PG14
		IEnumerable<IAPHISLicense> Licenses { get; }

		// PG17
		ZString CommoditySpecificName { get; }

		// PG19, PG20 and PG21
		ICustomsBrokerDetails BrokerDetails { get; }
		IPGAContactDetails ImporterDetails { get; }
		IAddressDetails UltimateCosigneeDetails { get; }
		RegistrationNumber UltimateCosigneeRegistrationNumber { get; }
		ZString BrokerFilerCode { get; }
		IAddressDetails ApplicantDetails { get; }
		PGAEntityIdentificationCodeAndNumberDetails ApplicantAPHISAssignedNumber { get; }
		IAddressDetails CropGrowerDetails { get; }
		RegistrationNumber CropGrowerDetailsRegistrationNumber { get; }
		IAddressDetails ShipperDetails { get; }
		PGAEntityIdentificationCodeAndNumberDetails ShipperCBPAssignedNumber { get; }
		IAddressDetails PermittedDetails { get; }
		PGAEntityIdentificationCodeAndNumberDetails PermittedAPHISAssignedNumber { get; }
		IAddressDetails USDAAPHISGrower { get; }

		//PG24
		ZString ReMarks { get; }

		// PG26
		IEnumerable<FDAQtyUQPair> OrderedQtyUQs { get; }

		// PG27
		IEnumerable<IContainerDetail> Containers { get; }

		// PG30
		IEnumerable<IAPHISInspection> Inspections { get; }
		ZString ArrivalLocation { get; }
		ZDateTime ArrivalDate { get; }

		// PG32
		IEnumerable<IAPHISRouting> Routings { get; }
	}

	public interface IAPHISProductCharacteristic
	{
		// PG07 and PG08
		IEnumerable<IAPHISIdentity> Identities { get; }

		IEnumerable<IAPHISCharacteristic> Characteristics { get; }
	}

	public interface IAPHISProductComponent : IAPHISProductCharacteristic
	{
		ZString Origin { get; }
		ZString SpecificName { get; }
		ZString GeneralName { get; }
		IAPHISCharacteristic Component { get; }
		ZDate ProcessingStartDate { get; }
		ZDate ProcessingEndDate { get; }
		ZString ProcessingTypeCode { get; }
		ZString ProcessingDescription { get; }
		ZString GeographicLocation { get; }
		ZString Genus { get; }
		ZString Species { get; }
		ZString Variety { get; }
		ZString SourceType { get; }
		ZString CountryCode { get; }
	}

	public interface IAPHISRouting
	{
		// PG32
		ZString RoutingType { get; }
		ZString RoutingCountry { get; }
		ZString RoutingState { get; }
	}

	public interface IAPHISInspection
	{
		// PG30
		ZString InspectionTestingStatus { get; }
		ZDate InspectionDate { get; }
		ZString InspectionLocationQualifier { get; }
		ZString InspectionLocation { get; }
	}

	public interface ISource
	{
		// PG06
		ZString SourceTypeCode { get; }
		ZString CountryCode { get; }
		ZString GeographicLocation { get; }
		ZDate ProcessingStartDate { get; }
		ZDate ProcessingEndDate { get; }
		ZString ProcessingTypeCode { get; }
		ZString ProcessingDescription { get; }
	}

	public interface IAPHISLicense : ILicense
	{
		// PG13
		ZString Location { get; }
		ZString LocationDescription { get; }

		// PG14
		ZDecimal Quantity { get; }
		ZString UnitOfMeasure { get; }
	}

	public interface IAPHISCharacteristic
	{
		// PG10
		ZString CommodityQualifierCode { get; }
		ZString CommodityCharacteristicQualifier { get; }
		ZString CommodityCharacteristicDescription { get; }
	}

	public interface IAPHISIdentity
	{
		// PG07 and PG08
		ZString IdentityType { get; }
		IEnumerable<INumberRange> Numbers { get; }
	}

	public interface INumberRange
	{
		ZString StartNumber { get; }
		ZString EndNumber { get; }
	}

	public interface ITSCAData
	{
		ZDate CertifySignatureDate { get; set; }
		ZString DeclarationCertificate { get; }
		ZString TSCACertificationCode { get; }
		ZString ContactName { get; }
		ZString ContactPhone { get; }
		ZString ContactEmail { get; }
		ZInt TSCALineNumber { get; set; }
		ZInt ODSLineNumber { get; set; }
	}

	public interface ITTBLine : IPGADataCorrection
	{
		ZInt LineNo { get; set; }
		ZString ProgramCode { get; }
		ZString ProcessingCode { get; }
		ZString PermitNumber { get; }
		ZString ExemptionCode { get; }
		ZString NumberForIRC { get; }
		IAddressDetails Consignee { get; }
		ZString ConsigneeEIN { get; }
		ZDecimal QuantityInPCS { get; }

		IEnumerable<ITTBCOLAAndCertificate> COLAAndCertificates { get; }
		IEnumerable<ITTBCigar> Cigars { get; }
	}

	public interface ITTBCOLAAndCertificate
	{
		ZString COLA { get; }
		ZString ExemptionCode { get; }
		ZString ForeignCertificateCountry { get; }
	}

	public interface ITTBCigar
	{
		ZBool IsSmall { get; }
		ZInt Quantity { get; }
		ZDecimal UnitPrice { get; }
	}

	public interface IDDTCData : IPGADataCorrection
	{
		ZString LicenseType { get; }
		ZString LicenseNumber { get; }
		ZString ExemptionCode { get; }
		ZString RegistrationNumber { get; }
		ZDateTime AnticipatedArrivalDate { get; }
	}

	public interface IATFData : IPGADataCorrection
	{
		ZInt LineNumber { get; set; }
		ZDecimal Quantity { get; }
		ZString CategoryCode { get; }
		ZString ExtendedDescription { get; }
		ZString FFLNumber { get; }
		ZString FFLExemptionCode { get; }
		ZString FELNumber { get; }
		ZString FELExemptionCode { get; }
		ZString PermitNumber { get; }
		ZString PermitExemptionCode { get; }
		ZString AECANumber { get; }
		ZString AECAExemptionCode { get; }
		ZString Model { get; }
		ZString CaliberGaugeSize { get; }
		ZDecimal BarrelLength { get; }
		ZDecimal OverallLength { get; }
		ZDateTime ArrivalDate { get; }
		ZString ArrivalLocation { get; }
		ZString ExportCountry { get; }
		IAddressDetails ManufacturerAddress { get; }
	}

	public interface IVNEData : IPGADataCorrection
	{
		ZInt LineNo { get; set; }
		ZString DocumentIdentifier { get; }

		IPGAContactDetails OwnerContactDetails { get; }
		IPGAContactDetails ImporterContactDetails { get; }
		IPGAContactDetails StorageLocationContactDetails { get; }
		ICustomsBrokerDetails ContactDetails { get; }

		ZString NAICNo { get; }
		ZString StateOfIssue { get; }
		ZString GeneralRemarks { get; }
		ZString IndustryCode { get; }
		ZString ImportCode { get; }
		ZString BondExemptionCode { get; }
		ZString ExemptionRemarks { get; }
		ZString MaxEnginePower { get; }
		ZString MaxEnginePowerUQ { get; }
		ZString BondPolicyNo { get; }
		ZString CBPBondNumber { get; }
		ZString VehiclesExemptionNumber { get; }
		ZString EPARegistrationNumber { get; }
		ZString CertOfConformity { get; }
		ZDate CertOfConformityExpiryDate { get; }
		ZString ElectronicImageSubmitted { get; }

		ZString BodyType { get; }
		ZString BodyCode { get; }
		ZString BodyDescription { get; }

		ZString DriverSide { get; }
		ZString MilitaryEq { get; }
		ZString ModelYear { get; }
		ZString CertifyingIndividual { get; }
		ZDate CertifySignatureDate { get; set; }
		ZString DeclarationCertificate { get; }

		IEnumerable<IVNEDetails> VNEDetails { get; }
	}

	public interface IVNEDetails
	{
		ZString Model { get; }
		ZString BuildMonth { get; }
		ZString BuildYear { get; }
		ZString IdentityNumberQualifier { get; }
		ZString IdentityNumber { get; }
		ZString VehicleManufacturer { get; }

		ZString EngineNumber { get; }
		ZString EngineBuildDate { get; }
		ZString EngineManufacturer { get; }
		ZString EngineModel { get; }

		ZString ManufactureDateType { get; }
		ZString BuildDateExplanation { get; }

		IEnumerable<VNEAdditionalNumbers> EngineAdditionalNumbers { get; }
		IEnumerable<VNEAdditionalNumbers> VehicleAdditionalNumbers { get; }
	}

	public interface VNEAdditionalNumbers
	{
		ZString NumberType { get; }
		ZString Number { get; }
	}

	public interface IFSISLine : IPGADataCorrection
	{
		ZInt LineNumber { get; set; }
		ZBool IsElectronicallyCertificated { get; }

		ZString CountryOfOrigin { get; }

		ZString IntendedUseCode { get; }

		ZString ProductID { get; }
		ZString ProductIDQualifier { get; }

		ZString CertificateIssuerCountry { get; }
		ZString HealthCertifcateNumber { get; }

		ZString ExportingEstNo { get; }

		ZDateTime ScheduleInspectionDate { get; }
		ZString ImportingEstNo { get; }

		IEnumerable<ZString> SealNumbers { get; }
		IEnumerable<IFSISLot> Lots { get; }

		IPGAContactDetails Importer { get; }
		IPGAContactDetails Consignee { get; }
		ICustomsBrokerDetails ContactDetails { get; }
		ICustomsBrokerDetails Broker { get; }

		ZDate CertifySignatureDate { get; set; }
		ZString DeclarationCertificate { get; }
		ZString CertifyingIndividual { get; }
	}

	public interface IFSISLot
	{
		ZString LotNumber { get; }
		ZDateTime StartDate { get; }
		ZDateTime EndDate { get; }

		ZString ProducingEstNo { get; }
		ZString SourceEstNo { get; }
		ZString SourceCountry { get; }
		ZString Species { get; }
		ZString ProductQualifierCode { get; }
		ZString ProductCharacteristic { get; }

		ZString ShippingMarks { get; }
		ZInt Quantity1 { get; }
		ZInt Quantity2 { get; }
		ZString UQ1 { get; }
		ZString UQ2 { get; }

		ZDecimal NetWeightInLB { get; }
	}

	public interface IPSTData : IPGADataCorrection
	{
		ZInt LineNo { get; set; }
		ZString IntendedUseCode { get; }
		ZString IntendedUseDescription { get; }
		ZString ProductType { get; }
		ZString ReasonCode { get; }
		ZString ReasonRemarks { get; }
		ZString BrandName { get; }
		ZString LPCONumber { get; }
		ZString ProducerEstNo { get; }
		ZString ProducerEstForNo { get; }
		ICustomsBrokerDetails BrokerDetails { get; }
		IPGAContactDetails ImporterDetails { get; }
		IPGAContactDetails CarrierDetails { get; }
		IPGAContactDetails ShipperDetails { get; }
		IPGAContactDetails ExaminationLocationDetails { get; }
		ZDecimal Quantity1 { get; }
		ZString UQ1 { get; }
		ZDecimal Quantity2 { get; }
		ZString UQ2 { get; }
		ZDecimal Quantity3 { get; }
		ZString UQ3 { get; }
		ZDecimal Quantity4 { get; }
		ZString UQ4 { get; }
		ZDecimal Quantity5 { get; }
		ZString UQ5 { get; }
		ZDecimal Quantity6 { get; }
		ZString UQ6 { get; }
		ZDecimal NetWeight { get; }
		ZString WeightUQ { get; }
		ZString CertifyingIndividual { get; }
		ZDate CertifySignatureDate { get; set; }
		ZString DeclarationCertificate { get; }
		ZString NotifyParty { get; }
		ZBool ConfidentialInfoIncluded { get; }
		ZString ConfidentialityRemarks { get; }
		ZBool IsPSTLabelsSent { get; }

		IEnumerable<IPSTLine> Lines { get; }
	}

	public interface IPSTLine
	{
		ZString LPCOType { get; }
		ZString LPCONumber { get; }
		ZString NameOfActiveIngredient { get; }
		ZDecimal ActiveIngredientPercentage { get; }
	}

	public interface INMFSLine : IPGADataCorrection
	{
		ZInt LineNo { get; set; }
		ZString ProgramCode { get; }
		ZBool ContainsYellowfinTuna { get; }
		ZString ToothfishState { get; }
		ZBool ElectronicImageSubmitted { get; }
		ZString AMLRPermitNumber { get; }
		ZString PreApprovalIssuedNumber { get; }
		ZString IFTPPermitNumber { get; }
		ZString OtherAuthorizationNumber { get; }
		ZString AuthorizationType { get; }
		ZDecimal PreApprovalIssuedQuantity { get; }
		ZString eBCDNumber { get; }
		ZBool Confidential { get; }
		ZString SpeciesCode { get; }
		ZDecimal NetWeight { get; }
		ZString NetWeightUQ { get; }
		IEnumerable<INMFSDocument> DocumentDetails { get; }
		IEnumerable<INMFSHarvestingDetail> HarvestingDetails { get; }
	}

	public interface INMFSHarvestingDetail
	{
		ZString SourceTypeCode { get; }
		ZString CountryCode { get; }
		ZString GeographicLocation { get; }
		ZString ProcessingTypeCode { get; }
		ZDate ProcessingStartDate { get; }
		ZString ProcessingDescription { get; }
		ZString ContactPartyType { get; }
		ZBool ContainsYellowfinTuna { get; }
		ZInt NumberOfVessels { get; }
		ZString FirstLandingCountry { get; }
		IEnumerable<ZString> HarvestingVessels { get; }
		IEnumerable<INMFSVessel> Vessels { get; }
		IPGAContactDetails ContactPartyDetails { get; }
	}

	public interface INMFSVessel
	{
		ZString HarvestedCountry { get; }
		ZString HarvestedVessel { get; }
		ZString TranshipmentPlace { get; }
		ZString FirstLandingCountry { get; }
		ZDecimal NetWeight { get; }
		ZString NetWeightUQ { get; }
	}

	public interface INMFSDocument
	{
		ZString DocumentIdentifier { get; }
		ZString DocumentNumber { get; }
	}

	public interface IFDAData : IPGADataCorrection
	{
		ZInt LineNo { get; set; }
		ZString ProgramCode { get; }
		ZString ProcessingCode { get; }
		ZString IntendedUseCode { get; }
		ZString IntendedUseDescription { get; }
		ZString BrandName { get; }
		ZString ItemIdentityNumberQualifier { get; }
		ZString ItemIdentityNumber { get; }
		ZString Description { get; }
		ZString ProductCode { get; }

		ZString SourceCountry { get; }
		ZString ProductionGrowthCountry { get; }
		ZString ProductionGrowthCountryQualifier { get; }
		ZString RefusalCountry { get; }
		ZString ShipmentCountry { get; }

		IEnumerable<IConstituentElement> ProductConstituentElements { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IEnumerable<KeyValuePair<ZString, ZString>> AffirmationOfCompliance { get; }

		IEnumerable<IFDALot> Lots { get; }
		IEnumerable<ZString> ContainerNumbers { get; }
		IEnumerable<IFDALicense> Licenses { get; }

		OrgCusCodeForFDA DeliveryPartyNumber { get; }
		IPGAContactDetails DeliveryPartyContact { get; }
		ZString DeliveryPartyRoleCode { get; }

		OrgCusCodeForFDA ManufacturerNumber { get; }
		IPGAContactDetails ManufacturerContact { get; }
		ZString FirmType { get; }

		OrgCusCodeForFDA ShipperNumber { get; }
		IPGAContactDetails ShipperContact { get; }

		OrgCusCodeForFDA FDAImporterNumber { get; }
		IPGAContactDetails FDAImporterContact { get; }

		OrgCusCodeForFDA FSVPImporterNumber { get; }
		IPGAContactDetails FSVPImporterContact { get; }

		OrgCusCodeForFDA ProducerNumber { get; }
		IPGAContactDetails ProducerContact { get; }
		ZString InitialImporterRoleCode { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		IEnumerable<KeyValuePair<IPGAContactDetails, OrgCusCodeForFDA>> ActiveIngredientProducers { get; }

		OrgCusCodeForFDA SubmitterNumber { get; }
		IPGAContactDetails SubmitterContact { get; }
		ZString SubmitterType { get; }
		bool IsSubmitterRelevant { get; }

		OrgCusCodeForFDA TransmitterNumber { get; }
		ICustomsBrokerDetails Transmitter { get; }
		bool IsTransmitterRelevant { get; }

		OrgCusCodeForFDA OwnerNumber { get; }
		IPGAContactDetails OwnerContact { get; }

		OrgCusCodeForFDA LocationOfGoodsNumber { get; }
		IPGAContactDetails LocationOfGoodsContact { get; }

		ZDate InspectionDate { get; }
		ZString InspectionTime { get; }
		ZString ArrivalLocation { get; }

		List<FDAQtyUQPair> OrderedQtyUQs { get; }

		ZString CanDimensions1 { get; }
		ZString CanDimensions2 { get; }
		ZString CanDimensions3 { get; }

		ZString PackageTrackingNumberCode { get; }
		ZString PackageTrackingNumber { get; }
		ZString Remarks { get; }
		ZString PNConfirmationNumber { get; }
		ZDecimal PGALineValue { get; }
		ZDecimal UnitValue { get; }
		ZString GoodsFromFTZ { get; }
	}

	public interface IACEPriorNoticeLine : IFDAData, IPGADataCorrection
	{
		ZString CommercialDescription { get; }
	}

	public interface IFDALot
	{
		ZString TemperatureQualifier { get; }
		ZString DegreeType { get; }
		ZString LocationOfTemperatureRecording { get; }

		ZString NumberQualifier { get; }
		ZString Number { get; }

		ZDate ProductionStartDate { get; }
		ZDate ProductionEndDate { get; }
		ZString Temperature { get; }
		ZString NegativeTemperatureIndicator { get; }
	}

	public interface IFDALicense
	{
		ZString Issuer { get; }
		ZString CountryCode { get; }
		ZString StateCode { get; }
		ZString StateDescription { get; }
		ZString Number { get; }
	}

	public interface IAMSData : IPGADataCorrection
	{
		ZInt LineNumber { get; set; }
		ZString Program { get; }
		ZString IntendedUseCode { get; }
		ZString IntendedUseCodeDescription { get; }
		ZString CommercialDescription { get; }
		ZDateTime EstimatedDate { get; }
		IPGAContactDetailsWithID Importer { get; }
		IPGAContactDetailsWithID Exporter { get; }
		IPGAContactDetailsWithID CertifyingBody { get; }
		IPGAContactDetailsWithID UltimateConsignee { get; }
		ICustomsBrokerDetails Broker { get; }

		IPGAContactDetailsWithID Consignee { get; }
		IEnumerable<IAMSLine> AMSLinesDetails { get; }
		IEnumerable<ILotCode> AMSLotCodes { get; }
		IEnumerable<IContainerDetail> Containers { get; }

		ZBool IsElecImageSubmitted { get; }
		ZString CerType { get; }
		ZString CerNumber { get; }
		ZBool USDAOrganicStandard { get; }
		ZBool EquivalentOrganicStandard { get; }
		ZString RemarkText { get; }
		ZString DateType { get; }
		ZDateTime Date { get; }
		ZDecimal NetWeight { get; }
		ZString NetWeightUQ { get; }
	}

	public interface IAMSLine
	{
		IPGAContactDetailsWithID Applicant { get; }
		IPGAContactDetailsWithID GoodsLocation { get; }
		IPGAContactDetailsWithID FinalHandler { get; }
		IPGAContactDetailsWithID CertifyingFinalHandler { get; }
		ZDateTime InspecDateTime { get; }
		ZString InspecRemarks { get; }
		ZDecimal Packages { get; }
		ZString PackagesUQ { get; }
		ZDecimal QtyPerPackage { get; }
		ZString QtyPerPackageUQ { get; }
		ZDecimal PackageWeight { get; }
		ZString PackageWeightUQ { get; }
		ZBool IsDocSubmitted { get; }
		ZString Location { get; }
		ZString Party { get; }
		ZString CertNumber { get; }
		ZDate IssueDate { get; }
		ZDecimal Weight { get; }
		ZString WeightUQ { get; }
		ZString AuthorizationNumber { get; }
		ZDecimal NetWeight { get; }
		ZString NetWeightUQ { get; }
		ZString ProductNumber { get; }
		ZDecimal OuterPackage { get; }
		ZString OuterPackageUQ { get; }
		ZDecimal InnerPackage { get; }
		ZString InnerPackageUQ { get; }
		ZDecimal InnerAmount { get; }
		ZString InnerAmountUQ { get; }
		ZDecimal TotalWeight { get; }
		ZString TotalWeightUQ { get; }
		ZDecimal TotalQuantity { get; }
		ZString TotalQuantityUQ { get; }
		ZDecimal InnerWeight { get; }
		ZString InnerWeightUQ { get; }
		ZString PermitNumber { get; }
		ZString InspectionLocation { get; }
		ZString CertType { get; }
		IEnumerable<ILotCode> AMSLineLotCodes { get; }
		ZString LotNumberQualifier { get; }
		ZString LotNumber { get; }
		ZString ProductLabel { get; }
	}

	public interface ILotCode
	{
		ZString Code { get; }
		ZString Value { get; }
	}

	public interface INHTSAHeader : IPGADataCorrection
	{
		ZInt LineNumber { get; set; }
		ZString ProgramCode { get; }
		ZBool ElectronicImageSubmitted { get; }

		ICustomsBrokerDetails ContactDetails { get; }
		IPGAContactDetails ConsigneeDetails { get; }
		IPGAContactDetails OwnerDetails { get; }
		IPGAContactDetailsWithID FabricatingManufacturerDetails { get; }
		IPGAContactDetails ImporterDetails { get; }
		IPGAContactDetails OriginalVehicleMFRDetails { get; }
		IPGAContactDetails RetailerDetails { get; }

		ZString BoxNumber { get; }
		ZString EmbassyNationality { get; }
		ZString DocumentType { get; }
		ZString DocumentNationality { get; }
		ZString DocumentNumber { get; }
		ZString DOTSuretyCode { get; }
		ZString DOTBondSerialNumber { get; }
		ZString DOTBondType { get; }
		ZInt DOTBondAmount { get; }
		IEnumerable<INHTSADetails> Details { get; }
		IEnumerable<INHTSADocument> Documents { get; }
		ZString CertifyingIndividual { get; }
		ZBool IsTMCCodeRequired { get; }
		ZBool IsGMCCodeRequired { get; }
		ZString IntendedUseCode { get; }
		ZString IntendedUseDesc { get; }
		ZDate CertifySignatureDate { get; set; }
		ZString DeclarationCertificate { get; }
	}

	public interface INHTSADetails
	{
		ZString BrandName { get; }
		ZString Model { get; }
		ZString YearOfManufacturer { get; }
		ZString MonthOfManufacturer { get; }
		ZString NumberType { get; }
		ZString Number { get; }
		ZString CategoryType { get; }
		ZString CategoryCode { get; }
		ZString DriveSide { get; }
		ZString ModelYear { get; }

		IEnumerable<INHTSAAdditionalNumber> AdditionalNumbers { get; }
		IEnumerable<INHTSAPermitAndLicense> PermitAndLicenses { get; }

		IEnumerable<IEnumerable<INHTSAAdditionalNumber>> OtherAdditionalNumbers { get; }
	}

	public interface INHTSAAdditionalNumber
	{
		ZString NumberType { get; }
		ZString Number { get; }
	}

	public interface INHTSAPermitAndLicense
	{
		ZString TransactionType { get; }
		ZString LPCOType { get; }
		ZString LPCONumber { get; }
		ZString DateType { get; }
		ZDate LPCODate { get; }
		ZDecimal LPCOQuantity { get; }
		ZString UnitOfMeasure { get; }
	}

	public interface INHTSADocument
	{
		ZString DocumentType { get; }
		ZString OwnerCode { get; }
	}

	public interface ILaceyActCommon : MessageBuilders.IOGALine, IPGADataCorrection
	{
		ZString CommercialDescription { get; }
		ZInt PGALineItemNumber { get; set; }
		IPGAContactDetails ImporterContactDetails { get; }

		//PG04
		IEnumerable<IConstituentElement> ConstituentElements { get; }

		//this is for legacy Lacey Act implementation
		//PG15/16 are duplicates of the PG05/PG06 records
		//if there are multiple PG04 records, since there are multiple components, PG15 and PG16 records are used
		ZBool ShouldSendPG15PG16Records { get; }

		ICustomsBrokerDetails ContactDetails { get; }
		ZString CertifyingIndividual { get; }
		//PG22
		ZDate CertifySignatureDate { get; set; }
		ZString DeclarationCertificate { get; }

		//PG25
		ZDecimal PGALineValue { get; }

		//PG27
		IEnumerable<IContainerNumber> ContainerNumbers { get; }

		ZBool UnknownBreakdown { get; }
		ZBool UnknownBreakdownTotal { get; }

		ZString NameOfConstituent { get; }
		ZDecimal QuantityOfConstituent { get; }
		ZString UnitOfMeasure { get; }
		IEnumerable<ILaceyCountry> CountryCodes { get; }
	}

	public interface IConstituentElement
	{
		ZString Name { get; }
		ZDecimal Quantity { get; }
		ZString UnitOfMeasure { get; }
		ZDecimal Percent { get; }
		ZString CountryCode { get; }

		PGAScientificDataCollection ScientificData { get; }
		ZString GenusName { get; }
		ZString SpeciesName { get; }
	}

	public interface ILaceyCountry
	{
		ZString CountryCode { get; }
	}

	public interface IScientificData
	{
		ZString GenusName { get; }
		ZString SpeciesName { get; }
		ZString CountryCode { get; }
		ZString SubSpeciesName { get; }
		ZString SpeciesCode { get; }
	}

	public interface IContainerNumber
	{
		ZString ContainerEquipmentID { get; }
	}

	public interface IContainerDetail
	{
		// PG27
		ZString ContainerEquipmentID { get; }
		ZShort ContainerLength { get; }
		ZBool IsRefrigerated { get; }
	}

	public interface ILicense
	{
		ZString TransactionType { get; }
		ZString Type { get; }
		ZString Number { get; }
		ZString DateQualifier { get; }
		ZDate Date { get; }
	}

	public interface IDEAHeader : IPGADataCorrection
	{
		//PG01
		ZInt LineNo { get; set; }

		//PG06
		ZString CountryOfShipment { get; }

		// PG22
		ZString FormID { get; }

		//PG14
		ZString PermitNumber { get; }

		//PG19
		ZString RegistrationNumber { get; }

		//PG30
		ZDate ArrivalDate { get; }

		//PG02 and PG04
		IEnumerable<IDEAConstituent> Constituents { get; }
	}

	public interface IDEAConstituent
	{
		//PG02
		ZString ProductCode { get; }

		//PG04
		ZDecimal Weight { get; }
		ZString WeightUQ { get; }
	}

	public interface IHFCHeader : IPGADataCorrection
	{
		//PG01
		ZInt LineNo { get; set; }
		ZBool ElectronicImageSubmitted { get; }

		//PG02 and PG04
		IEnumerable<IHFCDetail> HFCDetails { get; }

		//PG07
		ZString ASHRAENumber { get; }

		// PG19, PG20 and PG21
		ZString CertifyingIndividual { get; }

		IPGAContactDetails Importer { get; }

		IPGAContactDetails Consignee { get; }

		ICustomsBrokerDetails CustomsBroker { get; }

		// PG27
		IEnumerable<IContainerDetail> CusContainers { get; }

		// PG29
		ZDecimal NetWeight { get; }
	}

	public interface IHFCDetail
	{
		//PG02
		ZString ProductCode { get; }

		//PG04
		ZString NameOfActiveIngredient { get; }
		ZDecimal ActiveIngredientPercentage { get; }
	}

	public static class IVNEDetailsExtensionMethod
	{
		public static bool IsVehicleAndEngine(this IVNEDetails details)
		{
			return !details.IdentityNumber.IsEmpty && !details.EngineNumber.IsEmpty;
		}

		public static bool IsVehicleOnly(this IVNEDetails details)
		{
			return details.EngineNumber.IsEmpty && details.EngineBuildDate.IsEmpty && details.EngineManufacturer.IsEmpty && details.EngineModel.IsEmpty;
		}

		public static bool IsEngineOnly(this IVNEDetails details)
		{
			return details.BuildMonth.IsEmpty && details.BuildYear.IsEmpty && details.IdentityNumber.IsEmpty && details.IdentityNumberQualifier.IsEmpty && details.VehicleManufacturer.IsEmpty;
		}

		public static bool AllEngineDetailsEntered(this IVNEDetails details)
		{
			return !details.EngineNumber.IsEmpty && !details.EngineBuildDate.IsEmpty && !details.EngineManufacturer.IsEmpty && !details.EngineModel.IsEmpty;
		}
	}

	public static class IGovernmentAgenciesCommonExtensionMethods
	{
		public static bool HasAnyPGADataToBeDeclaredOrDisclaimed(this IGovernmentAgenciesIndicators line)
		{
			return OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.ODSIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.PSTIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.VNEIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.FSISIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.NMFS370Indicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.NMFSAMRIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.NMFSHMSIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.NMFSSIMIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.NMFSCOAIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.ACEFDAIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.TSCAIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.TTBIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.NHTSAIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.AMSIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.NOPIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.LaceyActIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.APHISIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.FWSIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.OMCIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.DDTCIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.ATFIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.DEAIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.CPSCIndicator)
				|| OGAIndicatorList.IsToBeDeclaredOrDisclaimed(line.HFCIndicator);
		}
	}

	public class PGAIndicatorsInvoiceLineWrapper : IGovernmentAgenciesIndicators
	{
		public PGAIndicatorsInvoiceLineWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		readonly JobComInvoiceLine invoiceLine;

		ZString IGovernmentAgenciesIndicators.ODSIndicator
		{
			get { return invoiceLine.US_ODSInd; }
		}

		ZString IGovernmentAgenciesIndicators.FSISIndicator
		{
			get { return invoiceLine.US_FSISInd; }
		}

		ZString IGovernmentAgenciesIndicators.VNEIndicator
		{
			get { return invoiceLine.US_VNEInd; }
		}

		ZString IGovernmentAgenciesIndicators.PSTIndicator
		{
			get { return invoiceLine.US_PSTIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.NMFS370Indicator
		{
			get { return invoiceLine.US_NMFS370Ind; }
		}

		ZString IGovernmentAgenciesIndicators.NMFS370DisclaimReason
		{
			get { return invoiceLine.US_NMFS370DisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSAMRIndicator
		{
			get { return invoiceLine.US_NMFSAMRInd; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSAMRDisclaimReason
		{
			get { return invoiceLine.US_NMFSAMRDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSHMSIndicator
		{
			get { return invoiceLine.US_NMFSHMSInd; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSHMSDisclaimReason
		{
			get { return invoiceLine.US_NMFSHMSDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSSIMIndicator
		{
			get { return invoiceLine.US_NMFSSIMPInd; }
		}

		ZString IGovernmentAgenciesIndicators.NMFSCOAIndicator
		{
			get { return invoiceLine.US_NMFSCOAInd; }
		}

		ZString IGovernmentAgenciesIndicators.ACEFDAIndicator
		{
			get { return invoiceLine.US_FDAIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.ACEFDADisclaimReason
		{
			get { return invoiceLine.US_FDADisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.TSCAIndicator
		{
			get { return invoiceLine.US_TSCAInd; }
		}

		ZString IGovernmentAgenciesIndicators.ATFIndicator
		{
			get { return invoiceLine.US_ATFInd; }
		}

		ZString IGovernmentAgenciesIndicators.NHTSAIndicator
		{
			get { return invoiceLine.US_NHTSAIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.ODSDisclaimReason
		{
			get { return invoiceLine.US_ODSDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.FSISDisclaimReason
		{
			get { return invoiceLine.US_FSISDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.VNEDisclaimReason
		{
			get { return invoiceLine.US_VNEDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.PSTDisclaimReason
		{
			get { return invoiceLine.US_PSTDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.PSTDisclaimProgram
		{
			get { return invoiceLine.US_PSTDisclaimProgram; }
		}

		ZString IGovernmentAgenciesIndicators.TSCADisclaimReason
		{
			get { return invoiceLine.US_TSCADisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.LaceyActIndicator
		{
			get { return invoiceLine.US_LaceyIndicator; }
		}

		ZString IGovernmentAgenciesIndicators.LaceyActDisclaimReason
		{
			get { return invoiceLine.US_LaceyDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.NHTSADisclaimReason
		{
			get { return invoiceLine.US_NHTDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.OMCIndicator
		{
			get { return invoiceLine.US_OMCInd; }
		}

		ZString IGovernmentAgenciesIndicators.OMCDisclaimReason
		{
			get { return invoiceLine.US_OMCDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.TTBIndicator
		{
			get { return invoiceLine.US_TTBInd; }
		}

		ZString IGovernmentAgenciesIndicators.TTBDisclaimReason
		{
			get { return invoiceLine.US_TTBDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.CPSCIndicator
		{
			get { return invoiceLine.US_CPSCInd; }
		}

		ZString IGovernmentAgenciesIndicators.CPSCDisclaimReason
		{
			get { return invoiceLine.US_CPSCDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.APHISIndicator
		{
			get { return invoiceLine.US_APHISInd; }
		}

		ZString IGovernmentAgenciesIndicators.APHISDisclaimReason
		{
			get { return invoiceLine.US_APHISDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.FWSIndicator
		{
			get { return invoiceLine.US_FWSInd; }
		}

		ZString IGovernmentAgenciesIndicators.FWSDisclaimReason
		{
			get { return invoiceLine.US_FWSDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.AMSDisclaimReason
		{
			get { return invoiceLine.US_AMSDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.AMSIndicator
		{
			get { return invoiceLine.US_AMSInd; }
		}

		ZString IGovernmentAgenciesIndicators.AMSDisclaimProgram
		{
			get { return invoiceLine.US_AMSDisclaimProgram; }
		}

		ZString IGovernmentAgenciesIndicators.NOPIndicator
		{
			get { return invoiceLine.US_NOPInd; }
		}

		ZString IGovernmentAgenciesIndicators.NOPDisclaimReason
		{
			get { return invoiceLine.US_NOPDisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.DDTCIndicator
		{
			get { return invoiceLine.US_DDTCInd; }
		}

		ZString IGovernmentAgenciesIndicators.DEAIndicator
		{
			get { return invoiceLine.US_DEAInd; }
		}

		ZString IGovernmentAgenciesIndicators.DEADisclaimReason
		{
			get { return invoiceLine.US_DEADisclaimReason; }
		}

		ZString IGovernmentAgenciesIndicators.HFCIndicator
		{
			get { return invoiceLine.US_HFCInd; }
		}

		ZString IGovernmentAgenciesIndicators.HFCDisclaimReason
		{
			get { return invoiceLine.US_HFCDisclaimReason; }
		}
	}
}
