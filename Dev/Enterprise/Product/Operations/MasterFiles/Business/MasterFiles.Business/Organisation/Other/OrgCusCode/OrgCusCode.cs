using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance.Portugal;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.US.USAMS;

namespace Enterprise.MasterFiles.Business
{
	/* *************************************************
	 * PLEASE NOTE:
	 *    When you add a new code to ANY of the sub classes of OrgCusCode you must also make the following changes:
	 *    - If the code is a Primary Company/Business code:
	 *      + Create a country specific derived class of OrgCusCodeInfo.cs, e.g. BurkinaFasoOrgCusCodeInfo.cs, to get the codes that were previously got from OrgCodeList.
	 *      + In the country specific OrgCusCodeInfo class Implement IOrgCusCodeProvider.GetPrimaryCusCodes() and use OrgCusCodeFactory to instantiate a country specific file.
	 *      +  Update Enterprise.MasterFiles.Business.OrgCusCodeCountryFactory.GetOrgCusCodeInfo method with the country.
	 *    - Remove #region for the country in Enterprise.MasterFiles.Business.OrgCodeLists.cs.
	 *    - Update $\dev\Enterprise\Product\Core\DataTransfer\DataTransfer\DataFileDefinitions\Xml\Version1\Elements.xsd
	 *    - Run Custom Tool for Elements.xsd to build the Elements.cs (see properties of Elements.xsd).
	 *    - Update $\dev\Enterprise\Product\Core\DataTransfer\DataTransfer\DataAdapters\Organisations\OrgCusCodeXmlMappings.cs
	 *    - Unit test DataTransfer solution.
	 *    *************************************************
	 */
	[CodeProperty(OrgCusCode.Schema.OK_CustomsRegNo), DescriptionProperty("CompanyCodeAndPremisesAddresses")]
	[ProvideMetaDataProperty("ReadOnlySecurity", MetaDataTypes.ReadOnly)]
	[UniversalCopyWithExtendedEntities]
	[UniversalCopyIgnoreElement(OrgCusCodeSchema.Constants.OK_OA_PremisesAddress)]
	public class OrgCusCode : AutoOrgCusCode,
		IOrgCusCode,
		IOrgCusCodeForMatching,
		ICodeTypeCodeDescription,
		IEInvoicingEligibilityLiteRegistrationCode
	{
		#region Code Types Constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:Class is inherited and cannot be static")]
		public class CodeTypes
		{
			public const string AccountsPayableSuppliersReference = "APC";
			public const string CarrierCode = "CCC";    // Customs Carrier Code.
			public const string TruckCarrierCode = "CCT";    // Customs Truck Carrier Code.
			public const string CarrierPrincipalCode = "CAR";  // Shipping Company Carrier/Principal Code.
			public const string DEA = "DEA";
			public const string eNettRegistrationNumber = "ENE";
			public const string GSTCode = "GST";
			public const string GovBusinessCode = "GBR";
			public const string CorporationCode = "GCR";
			public const string ContainerChainCommunityCode = "CC1";
			public const string CustomsClientID = "CID";
			public const string CustomsCPPermitCode = "CPC";
			public const string TaxFileCode = "GTX";
			public const string CustomsClientCode = "CCD";
			public const string SupplierCode = "CSC";
			public const string ControlledPremisesID = "CCP";
			public const string DepotControlledPremisesID = "CPD";
			public const string TerminalControlledPremisesID = "CPT";
			public const string WarehouseControlledPremisesID = "CPW";
			public const string BrokerageRegistration = "CBR";
			public const string BrokerageSiteID = "CBS";
			public const string BrokeragePrinter = "CBP";
			public const string BondHolderCode = "BHR";
			public const string ManifestProviderID = "CMP";
			public const string LegacySystemCode = "LSC";
			public const string OneStopCode = "1ST";
			public const string RebateUserCode = "REB";
			public const string VATCode = "VAT";
			public const string TVACode = "TVA";
			public const string BuyerCode = "BYR";
			public const string DeliveranceCode = "DLV";
			public const string AgentCode = "AGT";
			public const string ReleaseAgentCode = "RAC";
			public const string UniversalNettingCode = "UNC";
			public const string UniversalOfficeCode = "UOC";
			public const string GlobalTrackingName = "GTN";
			public const string EDISiteID = "EID";
			public const string EUTracesID = "ETI";
			public const string PassportID = "PAS";
			public const string DriverLicenceID = "DRV";
			public const string MedicareID = "MED";
			public const string IdentityCardNumber = "IDN";
			public const string CompanyNumber = "CNO";
			public const string CompanyRegistrationNumber = "CRN";
			public const string OrganizationNumber = "ORG";
			public const string BusinessRegistrationNumber = "BRN";
			public const string TaxIDNumber = "TAX";
			public const string Kennitala = "KEN";
			public const string InntraCode = "INT";
			public const string ContainerManagementMessagingAgreedCode = "CMM";
			public const string SEPACreditorIdentifier = "SID";
			public const string PIMAAddress = "PIM";
			public const string ExternalDebtorAccountCode = "EDR";
			public const string ExternalCreditorAccountCode = "ECR";
			public const string CreditAgencyCode = "CAC";
			public const string IVA = "IVA";
			public const string GS1 = "GS1";
			public const string RegulatedAgentID = "RAI";
			public const string EHubOrganisationID = "HID";
			public const string RoadCarrierRegistration = "RCR";
			public const string TIR_TransportsInternationauxRoutiers = "TIR";
			public const string PalletTradingAccountChep = "CHP";
			public const string PalletTradingAccountLoscam = "LOS";
			public const string CargoWiseOneCarrierCode = "C1C";
			public const string DomesticCarrierCode = "DCC";
			public const string VGMRegistrationNumber = "VGM";
			public const string NorthAmericanIndustryClassificationSystem = "NAI";
			public const string StandardIndustrialClassification = "SIC";
			public const string JNP = "JNP";
			public const string WorldCargoAssociationNumber = "WCA";
			public const string CommercialAndGovernmentEntity = "CAG";
			public const string DataUniversalNumberingSystem = "DUN";
			public const string PortSystemNumber = "PSN";
			public const string PortServiceReference = "PSR";
			public const string CargoWiseRoadTransportProviderCode = "C1R";
			public const string NVOCCReference = "NVO";
			public const string CustomsOfficeForTransit = "CTR";
			public const string FDAEstablishmentIdentifier = "FEI";
			public const string BoleroTitleRegisterID = "TRI";
			public const string PersonalIdentificationCardNumber = "PIC";
			public const string SpecialEconomicZone = "SEZ";
		}

		#region All EUROPEAN UNION and EUROPEAN FREE TRADE ASSOCIATION COUNTRIES

		public static class EuropeanUnionSharedCodeTypes
		{
			public const string Eori = "EOR";
			public const string Turn = "TRN";
			public const string BTW = "BTW";
			public const string AuthorisedEconomicOperator = "AEO";
			public const string DefermentApprovalNumber = "DAN";
			public const string TraderExciseNumber = "TEN";
			public const string TraderID = "TID";
			public const string ConsigneeExemptNo = "CEN";
			public const string InwardProcessingReliefNumber = "IPR";
			public const string OutwardProcessingReliefNumber = "OPR";
			public const string RegisteredExporterNumber = "REX";
			public const string ImportOneStopShopVatRegistration = "IOS";
			public const string CustomsOfficeForExit = "CEX";
			public const string TrustedTrader = "TTD";
			public const string UKInternalMarketSchemeCode = "UKM";
		}

		public static class AustriaCodeTypes
		{
			public const string UID = "UID";
		}

		public static class SlovakiaCodeTypes
		{
			public const string DPH = "DPH";
		}

		public static class DenmarkCodeTypes
		{
			public const string CentralBusinessRegister = "CVR";
			public const string ProductionNumber = "PNR";
			public const string EANLocationNumber = "EAN";
		}

		public static class FranceCodeTypes
		{
			public const string TVA = "TVA";
			public const string NAF = "NAF";
			public const string Siret = "SRT";
			public const string Siren = "SRN";
			public const string ALT = "ALT";
			public const string IST = "IST";
			public const string CI5 = "CI5";
			public const string SON = "SON";
			public const string CIN = "CIN";
			public const string SOA = "SOA";
			public const string SOW = "SOW";
			public const string EoriBranchSuffix = "EBS";
			public const string ROU = "ROU";
			public const string SUF = "SUF";
		}

		public static class MoroccoCodeTypes
		{
			public const string ICE = "ICE";
		}

		public static class IrelandCodeTypes
		{
			public const string PYE = "PYE";
			public const string ITX = "ITX";
			public const string CGT = "CGT";
			public const string VatFreeAuthorisation = "VFA";
			public const string TraderAccountNumber = "TRA";
			public const string VatZeroRatedAct2010 = "VZR";
		}

		public static class LatviaCodeTypes
		{
			public const string PVN = "PVN";
		}

		public static class LithuaniaCodeTypes
		{
			public const string PVM = "PVM";
			public const string IMK = "IMK";
		}

		public static class NetherlandsCodeTypes
		{
			public const string ChamberOfCommerceNumber = "CCN";
			public const string FenexLocationCode = "FNL";
			public const string LFRVATNumberCode = "LFR";
			public const string GFRVATNumberCode = "GFR";
			public const string CargonautRegistationCode = "CGN";
		}

		public static class NorwayCodeTypes
		{
			public const string MVA = "MVA";
			public const string EMD = "EMD";
		}

		public static class PolandCodeTypes
		{
			public const string NIP = "NIP";
			public const string PTU = "PTU";
			public const string TIN = "TIN";
			public const string PES = "PES";
		}

		public static class SloveniaCodeTypes
		{
			public const string DDV = "DDV";
		}

		public static class SpainCodeTypes
		{
			public const string NIF = "NIF";
			public const string DNI = "DNI";
			public const string IGC = "IGC";
		}

		public static class LuxembourgCodeTypes
		{
			public const string TVA = "TVA";
		}

		public static class UnitedKingdomCodeTypes
		{
			public const string GemsCustomerCode = "GCC";
			public const string AirCargoAgentsListedNumber = "ACL";
			public const string EoriBranchSuffix = "EBS";  // allows the BR123 and AG123 statements to differ from the EORI suffix
			public const string CTOShed = "SHD";
			public const string CustomsComprehensiveGuarantee = "CCG";
		}

		#region EFTA members
		public static class IcelandCodeTypes
		{
			public const string VSK = "VSK";
			public const string Kennitala = "KEN";
			public const string CustomsOfficeCode = "COC";
		}
		#endregion

		public static class EuropeanUnionFriendsThirdCountry
		{
			// Codes issues by friends of hte EU, e.g. USA/CA/JP, to act as an EORI-like number
			public const string TCU = "TCU";
		}

		#endregion

		public static class SamoaCodeTypes
		{
			public const string GST = "GST";
		}

		public static class MadagascarCodeTypes
		{
			public const string TIN = "TIN";
			public const string NIS = "NIS";
		}

		public static class MaldivesCodeTypes
		{
			public const string TIN = "TIN";
		}

		public static class TongaCodeTypes
		{
			public const string TIN = "TIN";
		}

		public static class AustraliaCodeTypes
		{
			public const string AustralianBusinessNumber = "ABN";
			public const string ARN = "ARN";
			public const string Diplomat = "DIP";
			public const string eParcelMerchantLocationID = "EPL";
			public const string TraderIdentificationNumber = "TIN";
			public const string AEO = "AEO";
			public const string QuotaExporterNumber = "QEN";
			public const string ApprovedArrangementNumber = "AAN";
		}

		public static class IsraelCodeTypes
		{
			public const string AEO = "AEO";
		}

		public static class TaiwanCodeTypes
		{
			public const string AEO = "AEO";
			public const string TPC = "TPC";
			public const string PID = "PID";
			public const string PBR = "PBR";
			public const string MCI = "MCI";
			public const string PIG = "PIG";
			public const string EPZ = "EPZ";
			public const string CBF = "CBF";
			public const string FTZ = "FTZ";
			public const string FactoryRegistrationNumber = "FRI";
			public const string AgriculturalTechnologyPark = "ATP";
			public const string SciencePark = "SPK";
		}

		public static class IndonesiaCodeTypes
		{
			public const string PPN = "PPN";
			public const string PP2 = "PP2";
			public const string PP3 = "PP3";
			public const string NIT = "NIT";
		}

		public static class EgyptCodeTypes
		{
			public const string CommercialRegistrationNumber = "COM";
		}

		public static class ChadCodeTypes
		{
			public const string NIF = "NIF";
		}

		public static class CACodeTypes
		{
			public const string AuthorizationID = "AID";
			public const string BusinessNumberCustomsBroker = "BRB";
			public const string BusinessNumberForCorporateIncomeTax = "BRC";
			public const string BusinessNumberForImportExport = "BRM";
			public const string BusinessNumberForExport = "BRE";
			public const string BusinessNumberForPayrollDeductions = "BRP";
			public const string BusinessNumberForGoodsServicesHarmonizedSalesTax = "BRT";
			public const string BusinessNumberForLowValueShipments = "BRL";
			public const string BusinessNumberImporterCommercial = "CAI";
			public const string BusinessNumberImporterNonCommercial = "BNC";
			public const string CustomsOfficeCode = "COC";
			public const string CSAReferenceID = "CSA";
			public const string SocialInsuranceNumber = "SIN";
			public const string ExportLicenceNumber = "CAX";
			public const string QuebecSalesTaxID = "QST";
			public const string AccountSecurityCode = "ASC";
			public const string CFIAAccountNumber = "CFI";
			public const string WorldManufacturerIdentifier = "WMI";
			public const string NuclearSafetyCommissionLicenseNumber = "NSL";
			public const string SafeFoodForCanadiansLicense = "SFC";
			public const string ECCCAuthorizationNumber = "ECC";
		}

		public static class NZCodeTypes
		{
			public const string ApprovedTransitionalFacility = "ATF";
			public const string SecureExportPartner = "SEP";
			public const string RegistrationNumber = "RGN";
			public const string MAFCoverSheetQE = "MQE";
			public const string AEO = "AEO";
		}

		public static class UnitedArabEmiratesCodeTypes
		{
			public const string AEO = "AEO";
			public const string CBLSNumber = "CBL";
			public const string MPCINumber = "MPC";
			public const string IDNumber = "IDO";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1052:Class is inherited and cannot be static")]
		public class USACodeTypes
		{
			public const string ACEAssignedNumber = "ACE";
			public const string ABIRoutingCode = "ABR";
			public const string APHISAssignedNumber = "APH";
			public const string ForeignRegistrationNumber = "FRN";
			public const string EmployerIdentificationNumber = "EIN";
			public const string EncryptedConsigneeNumber = "ECN";
			public const string FIRMSCode = "FRM";
			public const string FreeAndSecureTradeCode = "FST";
			public const string ForeignProducerIdentifier = "FPI";
			public const string ForeignProducerIdentifierBeer = "FPB";
			public const string ForeignProducerIdentifierSpirits = "FPS";
			public const string ForeignProducerIdentifierWine = "FPW";
			public const string SocialSecurityNumber = "SSN";
			public const string CBPAssignedNumber = "CBN";
			public const string CBPAssignedSuretyCode = "SRT";
			public const string ManufacturerID = "MID";
			public const string FAAIndirectCarrierNumber = "FAA";
			public const string DeprecatedSpecialAddressNotification = "SAN";
			public const string CTPAT = "CTP";
			public const string DataUniversalNumberingSystemPlus4 = "DN4";
			public const string NMFCParticipant = "NMF";
			public const string AlcoholImportLicence = "ALC";
			public const string FoodFacilityRegistrationNumber = "PFR";
			public const string FederalMaritimeCommissionNumber = "FMC";
			public const string CustomsHouseBrokerLicenseNumber = "CHB";
			public const string ShipperRegistrationNumber = "SFR";
			public const string EntryFilerCode = "ENF";
			public const string CertifiedCargoScreening = "CCS";

			public const string TireManufacturerCode = "TMC";
			public const string GlazingManufacturerCode = "GMC";

			public const string DDTCRegistrationNumber = "DDT";
			public const string TTBPermitNumber = "TTB";
			public const string TTEPermitNumber = "TTE";
			public const string TTIRegistrationNumber = "TTI";
			public const string CPSCAccreditedLabId = "LAB";
			public const string AMSRegistrationNumber = "AMS";
			public const string DOTDepartmentOfTransportation = "DOT";
			public const string IFTPPermitNumber = "IFT";
			public const string ACASOriginatorCode = "ACA";

			public const string DepartmentOfDefenseActivityAddressCode = "DOD";
			public const string StandardCarrierAlphaCodeAir = "CCA";
			public const string CarrierPrefixCode = "CCP";
			public const string FWSeDecsAccountNumber = "FWE";
			public const string FDAForeignSellerRegistrationNumber = "FSR";

			public const string LegalEntityIdentifier = "LEI";
			public const string GlobalLocationNumber = "GLN";

			public const string AirAMSOriginatorCode = "AMO";
		}

		public static class HKCodeTypes
		{
			public const string KnownConsignorNumber = "KCN";
			public const string AEO = "AEO";
		}

		public static class RussiaCodeTypes
		{
			public const string OGRN = "OGR";
			public const string KPP = "KPP";
		}

		public static class AUQuarantineCodeTypes
		{
			public const string EXDOCExporterNumber = "EEN";
			public const string NEXDOCSExternalID = "NEI";
			public const string NEXDOCSExportNumber = "NEN";
			public const string EXDOCAMLCPerformanceExporterNumber = "EAP";
			public const string EXDOCEstablishmentNumber = "ESN";
			public const string EXDOCEDIUser = "EEU";
		}

		public static class IranCodeTypes
		{
			public const string AEO = "AEO";
		}

		public static class SingaporeCodeTypes
		{
			public const string QualifiedCompanyIdentificationCode = "QCI";
			public const string CentralRegistrationNumber = "CRN";
			public const string CentralProvidentFundNumber = "CPF";
			public const string UniqueEntityNumber = "UEN";
			public const string PartyStatusType = "PST";
			public const string InterbankGIRO = "IBG";
			public const string DirectDelivery = "DIR";
			public const string AEO = "AEO";
		}

		public static class ChinaCodeTypes
		{
			public const string BST = "BST";
			public const string CIQ = "CIQ";
			public const string VAG = "VAG";
			public const string VAS = "VAS";
			public const string ENP = "ENP";
			public const string USC = "USC";
			public const string NGB = "NGB";
			public const string AEO = "AEO";
			public const string MMR = "MMR";
			public const string SMR = "SMR";
		}

		public static class ThailandCodeTypes
		{
			public const string BID = "BID";
		}

		public static class JapanCodeTypes
		{
			public const string CON = "CON";
			public const string CIE = "CIE";
			public const string FSB = "FSB";
			public const string JAS = "JAS";
			public const string LPC = "LPC";
			public const string NUC = "NUC";
			public const string AAL = "AAL";
		}

		public static class MyanmarCodeTypes
		{
			public const string CMT = "CMT";
		}

		public static class AngolaCodeTypes
		{
			public const string NumeroDeIdentificacioFiscal = "NIF";
		}

		public static class PeruCodeTypes
		{
			public const string GovernmentTaxFileCode = "RUC";
			public const string DNI = "DNI";
		}

		public class GuamCodeTypes : USACodeTypes
		{
		}

		public class NorthernMarianaIslandsCodeTypes : USACodeTypes
		{
		}

		public static class SwissCodeTypes
		{
			public const string UID = "UID";
			public const string CAD = "CAD";
			public const string CAV = "CAV";
			public const string CTP = "CTP";
			public const string BID = "BID";
			public const string ASN = "ASN";
		}

		public static class GreeceCodeTypes
		{
			public const string AFM = "AFM";
			public const string DOY = "DOY";
		}

		public static class SriLankaCodeTypes
		{
			public const string SVATBusinessRegistrationNumber = "SVT";
		}

		public static class EthiopiaCodeTypes
		{
			public const string TaxIdentificationNumber = "TIN";
		}

		public static class ZambiaCodeTypes
		{
			public const string TaxIdentificationNumber = "TIN";
		}

		public static class VenezuelaCodeTypes
		{
			public const string RegistroUnicoDeInformacionFiscal = "RIF";
		}

		public static class NigeriaCodeTypes
		{
			public const string TaxIdentificationNumber = "TIN";
		}

		public static class PuertoRicoCodeTypes
		{
			public const string ImpuestoSobreVentasyUso = "IVU";
			public const string NumeroDeFianza = "TBN";
			public const string NRC = "NRC";
		}

		public static class GuatemalaCodeTypes
		{
			public const string NumeroDeIdentificacionTributaria = "NIT";
		}

		public static class EcuadorCodeTypes
		{
			public const string RUC = "RUC";
			public const string SRI = "SRI";
			public const string SRF = "SRF";
		}

		public static class ParaguayCodeTypes
		{
			public const string RUC = "RUC";
		}

		public static class NicaraguaCodeTypes
		{
			public const string RUC = "RUC";
		}

		public static class BoliviaCodeTypes
		{
			public const string NIT = "NIT";
		}

		public static class BosniaAndHerzegovinaCodeTypes
		{
			public const string PDV = "PDV";
			public const string IDB = "IDB";
			public const string JMB = "JMB";
		}

		public static class FrenchPolynesiaCodeTypes
		{
			public const string TAH = "TAH";
		}

		public static class HondurasCodeTypes
		{
			public const string RTN = "RTN";
		}

		public static class HungaryCodeTypes
		{
			public const string IDM = "IDM";
		}

		public static class AzerbaijanCodeTypes
		{
			public const string TIN = "TIN";
		}

		public static class KenyaCodeTypes
		{
			public const string PIN = "PIN";
		}

		public static class NewCaledoniaCodeTypes
		{
			public const string RDT = "RDT";
			public const string RID = "RID";
			public const string TGC = "TGC";
		}

		public static class TanzaniaCodeTypes
		{
			public const string VRN = "VRN";
			public const string TIN = "TIN";
		}

		public static class MaliCodeTypes
		{
			public const string NIF = "NIF";
		}

		public static class RomaniaCodeTypes
		{
			public const string CNP = "CNP";
			public const string CIF = "CIF";
			public const string TVA = "TVA";
		}

		public static class BangladeshCodeTypes
		{
			public const string AIN = "AIN";
			public const string BIN = "BIN";
		}

		public static class LebanonCodeTypes
		{
			public const string CRN = "CRN";
			public const string TIN = "TIN";
		}

		public static class RwandaCodeTypes
		{
			public const string TIN = "TIN";
		}

		public static class BarbadosCodeTypes
		{
			public const string TIN = "TIN";
		}

		public static class AlbaniaCodeTypes
		{
			public const string NIT = "NIT";
		}

		public static class SenegalCodeTypes
		{
			public const string NIN = "NIN";
		}

		public static class CoteDivoireCodeTypes
		{
			public const string NCC = "NCC";
			public const string NRC = "NRC";
		}

		public static class CameroonCodeTypes
		{
			public const string NIU = "NIU";
			public const string NRC = "NRC";
		}

		public static class MozambiqueCodeTypes
		{
			public const string NUI = "NUI";
			public const string GCR = "GCR";
		}

		public static class SouthAfricaCodeTypes
		{
			public const string CustomsDualProfileCode = "CDP";
			public const string RemoverUserCode = "REM";
			public const string CustomsApprovedExporter = "APE";
			public const string IDNumber = "IDO";
			public const string BillIssuer = "BIL";
			public const string TPT = "TPT";
			public const string TNP = "TNP";
			public const string BGV = "BGV";
		}

		public static class EquatorialGuineaCodeTypes
		{
			public const string NIF = "NIF";
		}

		public static class MalawiCodeTypes
		{
			public const string TIN = "TIN";
			public const string BRN = "BRN";
		}

		public static class NigerCodeTypes
		{
			public const string NIF = "NIF";
			public const string RCC = "RCC";
		}

		public static class PalauCodeTypes
		{
			public const string EIN = "EIN";
		}

		public static class CubaCodeTypes
		{
			public const string GCR = "GCR";
		}

		public static class GhanaCodeTypes
		{
			public const string GCR = "GCR";
			public const string TIN = "TIN";
		}

		public static class BelarusCodeTypes
		{
			public const string GCR = "GCR";
			public const string TIN = "TIN";
			public const string AEO = "AEO";
		}

		public static class SierraLeoneCodeTypes
		{
			public const string GCR = "GCR";
			public const string TIN = "TIN";
		}

		public static class ArmeniaCodeTypes
		{
			public const string TIN = "TIN";
		}

		public static class KiribatiCodeTypes
		{
			public const string TaxIdentificationNumber = "TIN";
		}

		public static class CuracaoCodeTypes
		{
			public const string CRB = "CRB";
			public const string CCR = "CCR";
		}

		public static class JamaicaCodeTypes
		{
			public const string GCT = "GCT";
			public const string TRN = "TRN";
		}

		public static class TrinidadAndTobagoCodeTypes
		{
			public const string BIR = "BIR";
		}

		public static class TogoCodeTypes
		{
			public const string NIF = "NIF";
			public const string NIC = "NIC";
		}

		public static class CroatiaCodeTypes
		{
			public const string OIB = "OIB";
		}

		public static class BeninCodeTypes
		{
			public const string NRC = "NRC";
		}

		public static class KosovoCodeTypes
		{
			public const string TVS = "TVS";
			public const string NFK = "NFK";
		}

		#endregion

		#region Loader

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public OrgCusCode LoadFromLegacyCode(ZString countryCode, ZString legacyCode)
			{
				return Factory.LoadTop1<OrgCusCode>(GetQuery(countryCode, OrgCusCode.CodeTypes.LegacySystemCode, legacyCode));
			}

			public OrgCusCode LoadRegardlesPremisesAddress(ZString codeType, ZString countryCode, ZGuid oK_OH)
			{
				ZQuery query = new ZQuery(OrgCusCodeSchema.OK_CodeType, codeType);
				query.AddToFilter(OrgCusCodeSchema.OK_OH, oK_OH);
				query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryCode);
				return Factory.LoadTop1<OrgCusCode>(query);
			}

			public OrgCusCode[] Load(ZString countryCode, ZString codeType, ZString regoNumber)
			{
				return Factory.Load<OrgCusCode>(GetQuery(countryCode, codeType, regoNumber));
			}

			public int GetDatabaseCount(ZString countryCode, ZString regoNumber, string[] codeTypes)
			{
				return Factory.GetDatabaseCount(typeof(OrgCusCode), GetQuery(countryCode, codeTypes, regoNumber));
			}

			public static ZQuery GetQuery(ZString countryCode, string codeType, ZString regoNumber)
			{
				return GetQuery(countryCode, new string[] { codeType }, regoNumber);
			}

			public static ZQuery GetQuery(ZString countryCode, string[] codeTypes, ZString regoNumber)
			{
				ZQuery result = new ZQuery(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryCode);
				result.AddToFilter(OrgCusCodeSchema.OK_CodeType, codeTypes);
				result.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, regoNumber);
				return result;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(OrgCusCode);
			}
		}

		#endregion

		public OrgCusCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static OrgCusCode Load(BusinessObjectFactory factory, ZString oK_CodeType, ZString oK_RN_NKCodeCountry, ZGuid oK_OH, ZGuid oK_OA_PremisesAddress)
		{
			ZQuery query = new ZQuery(OrgCusCodeSchema.OK_CodeType, oK_CodeType);
			query.AddToFilter(OrgCusCodeSchema.OK_OH, oK_OH);
			if (oK_OA_PremisesAddress.IsEmpty)
			{
				query.AddToFilter(OrgCusCodeSchema.OK_OA_PremisesAddress, null);
			}
			else
			{
				query.AddToFilter(OrgCusCodeSchema.OK_OA_PremisesAddress, oK_OA_PremisesAddress);
			}
			query.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, oK_RN_NKCodeCountry);
			return factory.LoadTop1<OrgCusCode>(query);
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (!IsDeleted && OK_CustomsRegNo.IsEmpty)
			{
				Delete();
			}
			base.OnFactorySavingBeforeTransactionCore();
		}

		public override void OnSaving()
		{
			if (IsInDatabase)
			{
				AddNewLogToOrgHeader(Events.EditedARecord);
			}
			else
			{
				AddNewLogToOrgHeader(Events.AddedARecordToTheSystem);
			}

			var originalCustomsRegNo = (ZString)OK_CustomsRegNoInfo.OriginalValue;
			if (!IsInDatabase || originalCustomsRegNo != OK_CustomsRegNo)
			{
				if (IsAMO)
				{
					USAirAMSOriginatorCodeChangedHandler.OnUpdateAction();
				}
			}
			else
			{
				var originalCodeType = (ZString)OK_CodeTypeInfo.OriginalValue;
				if (originalCodeType != OK_CodeType)
				{
					if ((IsAMO && !OK_CustomsRegNo.IsEmpty) || (originalCodeType == USACodeTypes.AirAMSOriginatorCode))
					{
						USAirAMSOriginatorCodeChangedHandler.OnUpdateAction();
					}
				}
			}

			base.OnSaving();
			InvalidateScreeningStatuses();
		}

		protected override void OnSavingForDelete()
		{
			if ((ZString)OK_CodeTypeInfo.OriginalValue == USACodeTypes.AirAMSOriginatorCode)
			{
				ObjectFactory.Get<IOriginatorCodeChangedHandler>("USAMS.IOriginatorCodeChangedHandler", Factory, (ZGuid)OK_OHInfo.OriginalValue).OnUpdateAction();
			}
			InvalidateScreeningStatuses(true);
			base.OnSavingForDelete();
		}

		void InvalidateScreeningStatuses(bool isBeingDeleted = false)
		{
			var ohPK = isBeingDeleted ? (ZGuid)OK_OHInfo.OriginalValue : OK_OH;
			var header = Factory.Load<OrgHeader>(ohPK);
			if (header != null && !header.IsDeleted && !header.IsBeingDeleted)
			{
				var shouldInvalidateScreeningStatuses = OK_RN_NKCodeCountryInfo.HasChanges ||
														 OK_CodeTypeInfo.HasChanges ||
														 OK_CustomsRegNoInfo.HasChanges ||
														(header.OH_ScreeningStatus != ScreeningStatusesList.Codes.Matched && !IsInDatabase) ||
														(header.OH_ScreeningStatus != ScreeningStatusesList.Codes.Clear && isBeingDeleted);

				if (shouldInvalidateScreeningStatuses)
				{
					header.InvalidateScreeningStatuses();
				}
			}
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		#endregion

		#region Logging

		void AddNewLogToOrgHeader(Event eventType)
		{
			Organisation?.Logs.AddNew(eventType, CustomLogSuffix);
		}

		ZString CustomLogSuffix
		{
			get
			{
				var logReference = (ZString)(NoResString)"Registration No.";
				logReference += " " + OK_CustomsRegNo + (OK_CustomsRegNoInfo.HasChanges ? "(" + OK_CustomsRegNoInfo.OriginalValue + ")" : "");

				if (CodeCountry != null)
				{
					logReference += (NoResString)" " + (NoResString)"Country:" + (NoResString)" " + OK_RN_NKCodeCountry;
				}

				logReference += (NoResString)" " + (NoResString)"Type:" + (NoResString)" " + OK_CodeType + (OK_CodeTypeInfo.HasChanges ? (NoResString)"(" + OK_CodeTypeInfo.OriginalValue + (NoResString)")" : (NoResString)"");

				return logReference;
			}
		}

		#endregion

		#region Properties

		#region OK_RN_NKCodeCountry

		[List("Lookups.CodeCountries")]
		public override ZString OK_RN_NKCodeCountry
		{
			get { return base.OK_RN_NKCodeCountry; }
			set
			{
				CheckMaximumLength(OK_RN_NKCodeCountryInfo, value);
				var oldValue = OK_RN_NKCodeCountry;
				base.OK_RN_NKCodeCountry = value;
				if (oldValue != OK_RN_NKCodeCountry)
				{
					if (OrgCusCodeValidity != null)
					{
						OrgCusCodeValidity.Delete();
					}

					if (VerificationSupported)
					{
						orgCusCodeValidity = null;
					}

					SyncBrazilRootCNPJFromCNPJIfNeeded(oldValue, OK_CodeType);
				}
			}
		}

		#endregion

		#region OK_CodeType
		[List("Lookups.OK_CodeType_List")]
		public override ZString OK_CodeType
		{
			get { return base.OK_CodeType; }
			set
			{
				var oldValue = OK_CodeType;
				base.OK_CodeType = value;

				if (Header != null)
				{
					Header.PatternMatchRequiresRegen = true;
					Header.FindDuplicates();
				}

				switch (OK_CodeType)
				{
					case CodeTypes.CarrierCode:
						OK_CustomsRegNo_MaxLength = OK_RN_NKCodeCountry == Constants.CountryCodes.UnitedArabEmirates ? 3 :
													OK_RN_NKCodeCountry == Constants.CountryCodes.UnitedStates ? 4 : 35;
						break;
					case CodeTypes.TruckCarrierCode:
						// Set max length to 4 when in US since this is max length of SCACs (Standard Carrier Alpha Code)
						OK_CustomsRegNo_MaxLength = OK_RN_NKCodeCountry == Constants.CountryCodes.UnitedStates ? 4 : 35;
						break;
					case CodeTypes.CargoWiseOneCarrierCode:
						OK_CustomsRegNo_MaxLength = 4;
						break;
					case IcelandCodeTypes.CustomsOfficeCode:
						OK_CustomsRegNo_MaxLength = 5;
						break;
					case AustraliaCodeTypes.Diplomat:
						OK_CustomsRegNo = OK_RN_NKCodeCountry;
						break;
					case SouthAfricaCodeTypes.CustomsApprovedExporter:
						OK_CustomsRegNo_MaxLength = 8;
						break;
					case GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber:
						OK_CustomsRegNo_MaxLength = 25;
						break;
					case CodeTypes.CargoWiseRoadTransportProviderCode:
						OK_CustomsRegNo_MaxLength = 15;
						break;
					case TurkeyOrgCusCodeInfo.OrgCusCodes.YFK:
						OK_CustomsRegNo_MaxLength = 13;
						break;
					case TurkeyOrgCusCodeInfo.OrgCusCodes.EOR:
						OK_CustomsRegNo_MaxLength = OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Turkey ? 17 : OK_CustomsRegNo_MaxLength;
						break;
					default:
						OK_CustomsRegNo_MaxLength = 254;
						break;
				}

				var upperCaseCRN = MakeSelectedUpperCase(OK_CustomsRegNo);
				if (OK_CustomsRegNo != upperCaseCRN && upperCaseCRN.Length <= OK_CustomsRegNo_MaxLength)
				{
					OK_CustomsRegNo = upperCaseCRN;
				}

				SyncCusCodeFromRefShippingLineIfNeeded();

				if (oldValue != OK_CodeType)
				{
					SyncBrazilRootCNPJFromCNPJIfNeeded(OK_RN_NKCodeCountry, oldValue);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateOK_OA_PremisesAddress();
					Validation.ValidateOK_CustomsRegNo();
					Validation.ValidateOK_RN_NKCodeCountry();
					Validation.ValidateOK_CodeType();
				}
			}
		}

		#endregion

		#region OK_OA_PremisesAddress
		[ReadOnlyMember(nameof(PremisesAddressReadOnly))]
		public override ZGuid OK_OA_PremisesAddress
		{
			get => base.OK_OA_PremisesAddress;
			set => base.OK_OA_PremisesAddress = value;
		}

		protected bool PremisesAddressReadOnly => GetPremisesAddressReadOnly(OK_CodeType);

		bool GetPremisesAddressReadOnly(ZString codeType)
		{
			switch (codeType)
			{
				case GermanyOrgCusCodeInfo.OrgCusCodes.IMA:
					return true;
				default:
					return false;
			}
		}

		#endregion OK_OA_PremisesAddress

		#region OK_CustomsRegNo
		[List("CustomsRegNoLookupList")]
		[ReadOnlyMember(nameof(OK_CustomsRegNo_ReadOnly))]
		public override ZString OK_CustomsRegNo
		{
			get { return base.OK_CustomsRegNo; }
			set
			{
				var oldValue = OK_CustomsRegNo;
				value = MakeSelectedUpperCase(value);
				var maxLength = OK_CustomsRegNo_MaxLength > 0 ? OK_CustomsRegNo_MaxLength : Schema.OK_CustomsRegNoMaxLength;
				base.OK_CustomsRegNo = value.TrimStart().SubstringSafe(0, maxLength);

				if (Header != null)
				{
					Header.PatternMatchRequiresRegen = true;
					Header.FindDuplicates();

					if (!IsValidationSuspended && OK_CodeType == OrgCusCode.CodeTypes.GS1)
					{
						Header.OrgFountains.MarkAsNeedingValidation();
					}
				}

				SyncCusCodeFromRefShippingLineIfNeeded();

				if (oldValue != OK_CustomsRegNo)
				{
					SyncBrazilRootCNPJFromCNPJIfNeeded(OK_RN_NKCodeCountry, OK_CodeType);
				}

				if (oldValue != OK_CustomsRegNo && VerificationStatus == OrgConstants.CusCodeValidityVerification.Verified.GetUnresolvedString())
				{
					MarkOrgCusCodeVerifiedOrUnVerified(false, ZString.Empty, ZString.Empty);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateOK_CustomsRegNo();
					Validation.ValidateOK_CodeType();
				}
			}
		}

		public bool IsAMO => OK_CodeType == USACodeTypes.AirAMSOriginatorCode;

		public IOriginatorCodeChangedHandler USAirAMSOriginatorCodeChangedHandler => fUSAirAMSOriginatorCodeChangedHandler ?? (fUSAirAMSOriginatorCodeChangedHandler = ObjectFactory.Get<IOriginatorCodeChangedHandler>("USAMS.IOriginatorCodeChangedHandler", Factory, OK_OH));
		IOriginatorCodeChangedHandler fUSAirAMSOriginatorCodeChangedHandler;

		#region BR Root CNPJ

		protected bool OK_CustomsRegNo_ReadOnly => IsBRRootCNPJ;

		bool IsBRRootCNPJ => OK_CodeType == BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ && OK_RN_NKCodeCountry == Constants.CountryCodes.Brazil;
		bool IsBRCNPJ => OK_CodeType == BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ && OK_RN_NKCodeCountry == Constants.CountryCodes.Brazil;

		void SyncBrazilRootCNPJFromCNPJIfNeeded(ZString oldCountryCode, ZString oldCodeType)
		{
			if (!IsCopying && Organisation != null && (IsBRCNPJOrRootCNPJ(oldCountryCode, oldCodeType) || IsBRCNPJOrRootCNPJ(OK_RN_NKCodeCountry, OK_CodeType)))
			{
				if (FindBRRootCNPJ() is OrgCusCode cusCodeRootCNPJ)
				{
					var cnpj = Organisation.CustomsCodes.GetCustomsRegNo(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, Constants.CountryCodes.Brazil);
					cusCodeRootCNPJ.OK_CustomsRegNo = cnpj.KeepNumericCharacters().SubstringSafe(0, 8);
				}
			}

			bool IsBRCNPJOrRootCNPJ(ZString countryCode, ZString codeType)
			{
				return countryCode == Constants.CountryCodes.Brazil
					&& (codeType == BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ || codeType == BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ);
			}
		}

		OrgCusCode FindBRRootCNPJ() => IsBRRootCNPJ ? this : Organisation?.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(BrazilOrgCusCodeInfo.OrgCusCodes.RootCNPJ, Constants.CountryCodes.Brazil);

		#endregion

		public ZString CustomsRegNoForDisplay
		{
			get
			{
				var customsRegNo = base.OK_CustomsRegNo;
				return AllowViewSocialSecurityNumber(OK_CodeType) ? customsRegNo : (ZString)SSNWithMask;
			}
		}

		const string SSNWithMask = "***-**-****";

		void SyncCusCodeFromRefShippingLineIfNeeded()
		{
			if ((OK_CodeType == CodeTypes.CarrierCode || OK_CodeType == CodeTypes.CargoWiseOneCarrierCode) && !OK_CustomsRegNo.IsEmpty && Organisation != null && !SyncCusCodeFromShippingLineLock.IsSync)
			{
				using (_ = new SyncCusCodeFromShippingLineLock())
				{
					if (OK_CodeType == CodeTypes.CarrierCode && OK_RN_NKCodeCountry == Constants.CountryCodes.UnitedStates)
					{
						var matchesExistingSCAC = Factory.LoadTop1<RefShippingLine>(new ZQuery(RefShippingLineSchema.RSL_StandardCarrierAlphaCode, OK_CustomsRegNo));

						if (matchesExistingSCAC != null)
						{
							var orgC1C = Organisation.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == CodeTypes.CargoWiseOneCarrierCode);

							if (!matchesExistingSCAC.RSL_CargoWiseOneCode.IsEmpty)
							{
								if (orgC1C == null)
								{
									var newC1C = Organisation.CustomsCodes.AddNew();
									newC1C.OK_RN_NKCodeCountry = OK_RN_NKCodeCountry;
									newC1C.OK_CustomsRegNo = matchesExistingSCAC.RSL_CargoWiseOneCode;
									newC1C.OK_CodeType = CodeTypes.CargoWiseOneCarrierCode;
								}
								else
								{
									orgC1C.OK_CustomsRegNo = matchesExistingSCAC.RSL_CargoWiseOneCode;
								}
							}

							SetOrgShippingInfo(matchesExistingSCAC);
						}
					}
					else if (OK_CodeType == CodeTypes.CargoWiseOneCarrierCode)
					{
						var matchesExistingC1C = Factory.LoadTop1<RefShippingLine>(new ZQuery(RefShippingLineSchema.RSL_CargoWiseOneCode, OK_CustomsRegNo));

						if (matchesExistingC1C != null)
						{
							var orgSCAC = Organisation.CustomsCodes.Cast<OrgCusCode>().FirstOrDefault(x => x.OK_CodeType == CodeTypes.CarrierCode && x.OK_RN_NKCodeCountry == Constants.CountryCodes.UnitedStates);

							if (!matchesExistingC1C.RSL_StandardCarrierAlphaCode.IsEmpty)
							{
								if (orgSCAC == null)
								{
									var newSCAC = Organisation.CustomsCodes.AddNew();
									newSCAC.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;
									newSCAC.OK_CustomsRegNo = matchesExistingC1C.RSL_StandardCarrierAlphaCode;
									newSCAC.OK_CodeType = CodeTypes.CarrierCode;
								}
								else
								{
									orgSCAC.OK_CustomsRegNo = matchesExistingC1C.RSL_StandardCarrierAlphaCode;
								}
							}
							else if (orgSCAC != null)
							{
								Organisation.CustomsCodes.RemoveAndDelete(orgSCAC);
							}

							SetOrgShippingInfo(matchesExistingC1C);
						}
					}
				}
			}
		}

		void SetOrgShippingInfo(RefShippingLine matchesExistingSCAC)
		{
			Organisation.OH_IsShippingLine = matchesExistingSCAC.RSL_IsShippingLine;
			Organisation.OH_IsSeaWholesaler = matchesExistingSCAC.RSL_IsNVO;

			if (!matchesExistingSCAC.RSL_IsShippingLine && !matchesExistingSCAC.RSL_IsNVO)
			{
				Organisation.OH_IsShippingLine = true;
			}

			if (Organisation.OH_RSL_ShippingLine != matchesExistingSCAC.PK)
			{
				Organisation.OH_RSL_ShippingLine = matchesExistingSCAC.PK;
			}

			if (!Organisation.OH_IsShippingProvider)
			{
				Organisation.OH_IsShippingProvider = true;
			}
		}

		class SyncCusCodeFromShippingLineLock : IDisposable
		{
			public SyncCusCodeFromShippingLineLock()
			{
				IsSync = true;
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
			internal static bool IsSync { get; private set; }

			public void Dispose()
			{
				IsSync = false;
			}
		}

		public int OK_CustomsRegNo_MaxLength
		{
			get { return oK_CustomsRegNo_MaxLength; }
			private set { oK_CustomsRegNo_MaxLength = value; }
		}
		int oK_CustomsRegNo_MaxLength = -1;

		ZString MakeSelectedUpperCase(ZString value)
		{
			ZString result = value;

			if (ShouldMakeUpperCase)
			{
				result = result.ToUpper();
			}

			return result;
		}

		#endregion

		#region PremisesAddress

		[UniversalCopyRelatedEntity(RelatedPropertyName = OrgCusCodeSchema.Constants.OK_OA_PremisesAddress, RelatedEntityTableName = OrgAddressSchema.Constants.TableName,
			DisableCopyMethodCopy = true, DisableCopyMethodLink = true, AllowCopyMethodLinkCopiedWhenLinkIsDisabled = true)]
		public override OrgAddress PremisesAddress
		{
			get { return base.PremisesAddress; }
		}

		#endregion

		#region CompanyCodeAndPremisesAddresses

		public ZString CompanyCodeAndPremisesAddresses
		{
			get
			{
				if (CompanyCodeAndPremisesAddressesCached == null)
				{
					CompanyCodeAndPremisesAddressesCached = new CachedProperty<ZString>(Factory, delegate
					{
						var header = Header;
						var companyCode = header == null ? ZString.Empty : header.OH_Code;
						var premisesAddress = PremisesAddress;
						var premisesDetails = premisesAddress == null ? ZString.Empty : premisesAddress.OA_Code;
						return ZString.Format("{0} ({1})", companyCode, premisesDetails);
					});
				}
				return CompanyCodeAndPremisesAddressesCached.Value;
			}
		}
		CachedProperty<ZString> CompanyCodeAndPremisesAddressesCached;

		#endregion

		#region IsNMFCParticipant

		public bool IsNMFCParticipant
		{
			get { return OK_CodeType == OrgCusCode.USACodeTypes.NMFCParticipant && OK_CustomsRegNo == OrgConstants.NMFCParticipantCodes.Code.Yes; }
		}

		#endregion

		#region For Grid Binding

		#region CustomsRegNoFieldType

		[MaxLength(3)]
		public ZString CustomsRegNoFieldType
		{
			get
			{
				var fieldType = OrgCusCodeCountryFactory.GetIOrgCusCodeLookupsProvider(OK_RN_NKCodeCountry)?.GetCustomsRegNoFieldType(OK_CodeType);
				if (fieldType != null)
				{
					return fieldType;
				}

				#region Instead of relying on below block please implement IOrgCusCodeLookupsProvider.GetCustomsRegNoFieldType() for the specific country and use OrgCusCodeCountryFactory to instantiate a country specific file.

				ZString result = nameof(FieldType.Text);
				if (IsNZSupplierCode || IsKRBondedAreaCodeList || IsKRIndustrialParkCodeList || IsCustomsOfficeOfExitList)
				{
					result = nameof(FieldType.TextCodeFindBox);
				}
				else if (IsNMFCode || IsSGPartyStatusType || IsDIRCode || IsCustomsSupervisingOfficeList)
				{
					result = nameof(FieldType.TextDropEdit);
				}
				return result;

				#endregion
			}
		}

		public ZPropertyInfo CustomsRegNoFieldTypeInfo
		{
			get { return GetZPropertyInfo(nameof(CustomsRegNoFieldType)); }
		}

		#endregion

		#region CustomsRegNoLookupList

		[CargoWise.Common.Testing.SuppressWeaklyTypedCollectionMessage]
		public IList CustomsRegNoLookupList
		{
			get
			{
				var lookList = OrgCusCodeCountryFactory.GetIOrgCusCodeLookupsProvider(OK_RN_NKCodeCountry)?.GetCustomsRegNoLookupList(OK_CodeType);
				if (lookList != null)
				{
					return lookList;
				}

				#region Instead of relying on below block please implement IOrgCusCodeLookupsProvider.GetCustomsRegNoLookupList() for the specific country and use OrgCusCodeCountryFactory to instantiate a country specific file.

				IList result = Lookups.NZCSupplier_List;
				if (IsNMFCode)
				{
					result = Lookups.NMFCParticipantList;
				}
				else if (IsSGPartyStatusType)
				{
					result = Lookups.SGPartyStatusTypeList;
				}
				else if (IsDIRCode)
				{
					result = Lookups.SGDirectDeliveryList;
				}
				else if (IsCustomsSupervisingOfficeList)
				{
					result = Lookups.CustomsSupervisingOfficeList;
				}
				else if (IsCustomsOfficeOfExitList)
				{
					result = Lookups.CustomsOfficeOfExitList;
				}
				else if (IsKRIndustrialParkCodeList)
				{
					result = Lookups.KRIndustrialParkCodeList;
				}
				else if (IsKRBondedAreaCodeList)
				{
					result = Lookups.KRBondedAreaCodeList;
				}
				else if (IsJPCustomsControlledPremisesCodeList)
				{
					result = Lookups.JPCustomsControlledPremisesCodeList;
				}
				else if (IsKRCustomsCarrierCodeList)
				{
					result = Lookups.KRCustomsCarrierCodeList;
				}

				return result;

				#endregion
			}
		}

		#endregion

		bool IsDIRCode
		{
			get { return (OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Singapore && OK_CodeType == OrgCusCode.SingaporeCodeTypes.DirectDelivery); }
		}

		bool IsNZSupplierCode
		{
			get { return (OK_RN_NKCodeCountry == Core.Constants.CountryCodes.NewZealand && OK_CodeType == CodeTypes.SupplierCode); }
		}

		internal bool IsApplicableToUSCustoms => Core.Constants.CountryCodes.IsUsaOrTerritory(OK_RN_NKCodeCountry) || Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(OK_RN_NKCodeCountry) == Core.Constants.CountryCodes.UnitedStates;

		bool IsNMFCode => IsApplicableToUSCustoms && OK_CodeType == USACodeTypes.NMFCParticipant;

		bool IsSGPartyStatusType
		{
			get { return (OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Singapore && OK_CodeType == OrgCusCode.SingaporeCodeTypes.PartyStatusType); }
		}

		bool IsCustomsSupervisingOfficeList
		{
			get { return (OK_RN_NKCodeCountry == Core.Constants.CountryCodes.UnitedKingdom && OK_CodeType == OrgCusCode.CodeTypes.CustomsClientCode); }
		}

		bool IsCustomsOfficeOfExitList
		{
			get { return (OK_RN_NKCodeCountry == Core.Constants.CountryCodes.UnitedKingdom && OK_CodeType == OrgCusCode.EuropeanUnionSharedCodeTypes.CustomsOfficeForExit); }
		}

		bool IsKRIndustrialParkCodeList
		{
			get { return OK_RN_NKCodeCountry == Core.Constants.CountryCodes.KoreaSouth && OK_CodeType == KoreaSouthComplianceInfo.CodeTypes.IndustrialParkCode; }
		}

		bool IsKRBondedAreaCodeList
		{
			get { return OK_RN_NKCodeCountry == Core.Constants.CountryCodes.KoreaSouth && OK_CodeType == CodeTypes.ControlledPremisesID; }
		}

		bool IsJPCustomsControlledPremisesCodeList => OK_RN_NKCodeCountry == Core.Constants.CountryCodes.Japan && OK_CodeType == OrgCusCode.CodeTypes.ControlledPremisesID;

		bool IsKRCustomsCarrierCodeList => OK_RN_NKCodeCountry == Core.Constants.CountryCodes.KoreaSouth && OK_CodeType == OrgCusCode.CodeTypes.CarrierCode;

		#endregion

		#region Premises Address

		public bool PremisesAddressIsAllowed => GetPremisesAddressIsAllowed(OK_CodeType, OK_RN_NKCodeCountry);
		public bool PremisesAddressIsRequired => new OrgCusCodePremiseAddressValidator().IsPremiseAddressRequired(OK_CodeType, OK_RN_NKCodeCountry);

		public static bool GetPremisesAddressIsAllowed(ZString codeType, ZString countryCode) => new OrgCusCodePremiseAddressValidator().IsPremiseAddressAllowed(codeType, countryCode);

		#endregion

		#region ShouldMakeUpperCase

		internal ZBool ShouldMakeUpperCase => GetShouldMakeUpperCase(OK_CodeType, OK_RN_NKCodeCountry);

		static ZBool GetShouldMakeUpperCase(ZString codeType, ZString countryCode)
		{
			switch (codeType)
			{
				case CodeTypes.ControlledPremisesID:
				case USACodeTypes.FIRMSCode:
				case USACodeTypes.TTBPermitNumber:
				case USACodeTypes.DDTCRegistrationNumber:
				case USACodeTypes.ACASOriginatorCode:
				case USACodeTypes.AirAMSOriginatorCode:
					return true;
			}

			if (countryCode == Constants.CountryCodes.UnitedStates)
			{
				switch (codeType)
				{
					case CodeTypes.CarrierCode:
					case CodeTypes.TruckCarrierCode:
						return true;
				}
			}
			else if (countryCode == Constants.CountryCodes.Taiwan)
			{
				switch (codeType)
				{
					case TaiwanCodeTypes.EPZ:
					case TaiwanCodeTypes.CBF:
					case TaiwanCodeTypes.FTZ:
					case CodeTypes.WarehouseControlledPremisesID:
						return true;
				}
			}
			else if (countryCode == Constants.CountryCodes.Norway)
			{
				switch (codeType)
				{
					case NorwayCodeTypes.MVA:
					case CodeTypes.GovBusinessCode:
						return true;
				}
			}
			else if (countryCode == Constants.CountryCodes.Turkey)
			{
				switch (codeType)
				{
					case CodeTypes.WarehouseControlledPremisesID:
					case CodeTypes.TerminalControlledPremisesID:
						return true;
				}
			}

			return false;
		}

		#endregion ShouldMakeUpperCase

		#region RequiresUniqueRegistrationNumber

		public ZBool RequiresUniqueRegistrationNumber => GetRequiresUniqueRegistrationNumber(OK_CodeType);

		static ZBool GetRequiresUniqueRegistrationNumber(ZString codeType)
		{
			switch (codeType)
			{
				case GermanyOrgCusCodeInfo.OrgCusCodes.EMCSWarehouseParticipantIdentificationNumber:
				case GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber:
				case GermanyOrgCusCodeInfo.OrgCusCodes.TaxOffice:
					return true;
				default:
					return false;
			}
		}

		#endregion

		#region Country Is

		public bool CountryIs(ZString code)
		{
			return OK_RN_NKCodeCountry == code && CodeCountry != null;
		}

		public bool CountryIsMemberOf(ZString groupName)
		{
			var codeCountry = CodeCountry;
			return codeCountry != null && codeCountry.RN_EconomicGrouping == groupName;
		}

		#endregion

		public OrgHeader Organisation
		{
			get { return fOrganisation ?? (fOrganisation = Factory.Load<OrgHeader>(OK_OH)); }
		}
		OrgHeader fOrganisation;

		#endregion

		#region OrgCusCodeValidity Properties

		public ZBool VerificationSupported => OK_RN_NKCodeCountry == Core.Constants.CountryCodes.UnitedStates || OK_RN_NKCodeCountry == Core.Constants.CountryCodes.UnitedKingdom;

		#region OrgCusCodeValidity

		public OrgCusCodeValidity OrgCusCodeValidity
		{
			get
			{
				if (ReferenceEquals(orgCusCodeValidity, null) || orgCusCodeValidity.IsDeleted)
				{
					if (VerificationSupported)
					{
						orgCusCodeValidity = Factory.LoadTop1<OrgCusCodeValidity>(new ZQuery(OrgCusCodeValiditySchema.OCV_OK_OrgCusCode, PK));

						if (orgCusCodeValidity == null && createNewOrgCusCodeValidity)
						{
							orgCusCodeValidity = (OrgCusCodeValidity)Factory.New(OrgCusCodeValidity.TypeDecider.GetTypeForCountryCode(OK_RN_NKCodeCountry));
							orgCusCodeValidity.OCV_OK_OrgCusCode = PK;
						}
					}

					if (orgCusCodeValidity == null || orgCusCodeValidity.IsDeleted)
					{
						orgCusCodeValidity = Factory.GetNull<OrgCusCodeValidity>();
					}

					if (orgCusCodeValidity != null)
					{
						RegisterEditableChildObject(orgCusCodeValidity);
					}
				}
				return orgCusCodeValidity;
			}
		}

		OrgCusCodeValidity orgCusCodeValidity;
		bool createNewOrgCusCodeValidity;

		public void MarkOrgCusCodeVerifiedOrUnVerified(ZBool verified, ZString verificationAuthority, ZString snapshotContext)
		{
			if (OrgCusCodeValidity.IsNull)
			{
				try
				{
					createNewOrgCusCodeValidity = true;
					orgCusCodeValidity = null;
					_ = OrgCusCodeValidity;
				}
				finally
				{
					createNewOrgCusCodeValidity = false;
				}
			}

			OrgCusCodeValidity.MarkVerifiedOrUnVerified(verified, verificationAuthority, snapshotContext);

			VerificationStatusInfo.RefreshBinding();
			LastVerifiedTimeInfo.RefreshBinding();
			VerificationAuthorityInfo.RefreshBinding();
		}

		#endregion

		#region Is Verified

		public ZBool IsVerified => OrgCusCodeValidity.IsVerified;

		#endregion

		#region Verification Status

		[ResourceStringData("Enterprise.MasterFiles.Business.OrgCusCode|VerificationStatus", Caption = "Verification Status")]
		[List(nameof(OrgCusCodeValidity) + "." + nameof(Business.OrgCusCodeValidity.Lookups) + "." + nameof(Business.OrgCusCodeValidity.Lookups.VerificationStatusList))]
		[MaxLength(12)]
		[ReadOnlyMember(nameof(OrgCusCodeValidity) + "." + nameof(Business.OrgCusCodeValidity.VerificationStatus_ReadOnly))]
		public ZString VerificationStatus
		{
			get => OrgCusCodeValidity.VerificationStatus;
			set
			{
				OrgCusCodeValidity.VerificationStatus = value;
				VerificationStatusInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo VerificationStatusInfo => GetWrappedZPropertyInfo(nameof(VerificationStatus), x => OrgCusCodeValidity.VerificationStatusInfo);

		#endregion

		#region Last Verified Time

		[ResourceStringData("Enterprise.MasterFiles.Business.OrgCusCode|LastVerifiedTime", Caption = "Last Verified Date")]
		[ReadOnlyMember(nameof(OrgCusCodeValidity) + "." + nameof(Business.OrgCusCodeValidity.LastVerifiedTime_ReadOnly))]
		public ZDateTime LastVerifiedTime
		{
			get => OrgCusCodeValidity.LastVerifiedTime;
			set
			{
				OrgCusCodeValidity.LastVerifiedTime = value;
				LastVerifiedTimeInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo LastVerifiedTimeInfo => GetWrappedZPropertyInfo(nameof(LastVerifiedTime), x => OrgCusCodeValidity.LastVerifiedTimeInfo);

		#endregion

		#region Verification Authority

		[ResourceStringData("Enterprise.MasterFiles.Business.OrgCusCode|VerificationAuthority", Caption = "Verification Authority")]
		[MaxLength(3)]
		[ReadOnlyMember(nameof(OrgCusCodeValidity) + "." + nameof(Business.OrgCusCodeValidity.VerificationAuthority_ReadOnly))]
		public ZString VerificationAuthority
		{
			get => OrgCusCodeValidity.VerificationAuthority;
			set
			{
				OrgCusCodeValidity.VerificationAuthority = value;
				VerificationAuthorityInfo.RefreshBinding();
			}
		}

		public ZString VerificationAuthorityFieldType => OrgCusCodeValidity.VerificationAuthorityFieldType;

		public ZPropertyInfo VerificationAuthorityInfo => GetWrappedZPropertyInfo(nameof(VerificationAuthority), x => OrgCusCodeValidity.VerificationAuthorityInfo);

		#endregion

		[ResourceStringData("Enterprise.MasterFiles.Business.OrgCusCode|SecuredCustomsRegNo", Caption = "Registration Number / Code")]
		[List("CustomsRegNoLookupList")]
		[MaxLength(OrgCusCode.Schema.OK_CustomsRegNoMaxLength)]
		public ZString SecuredCustomsRegNo
		{
			get { return IsInDatabase && IsDeniedSSNSecurity() ? ViewDeniedMessage : OK_CustomsRegNo; }
			set
			{
				OK_CustomsRegNo = value;
				SecuredCustomsRegNoInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SecuredCustomsRegNoInfo => GetWrappedZPropertyInfo(nameof(SecuredCustomsRegNo), x => OK_CustomsRegNoInfo);

		#region Snapshot Context For Display

		public ZString SnapshotContextForDisplay => OrgCusCodeValidity.SnapshotContextForDisplay;

		#endregion

		#endregion

		protected override OrgCusCodeValidation GetNewValidation()
		{
			OrgCusCodeValidation validation = null;

			if (CodeCountry != null && countrySpecificCusCodeValidations.ContainsKey(OK_RN_NKCodeCountry))
			{
				validation = (OrgCusCodeValidation)Activator.CreateInstance(countrySpecificCusCodeValidations[OK_RN_NKCodeCountry](), new object[] { this });
			}
			else if (Constants.CountryCodes.IsUsaOrTerritory(OK_RN_NKCodeCountry))
			{
				validation = (OrgCusCodeValidation)Activator.CreateInstance(countrySpecificCusCodeValidations[Constants.CountryCodes.UnitedStates](), new object[] { this });
			}
			else if (ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(OK_RN_NKCodeCountry))
			{
				validation = (OrgCusCodeValidation)Activator.CreateInstance(countrySpecificCusCodeValidations[Constants.CountryCodes.EuropeanUnion](), new object[] { this });
			}
			else
			{
				validation = base.GetNewValidation();
			}
			return validation;
		}

		readonly Dictionary<string, Func<Type>> countrySpecificCusCodeValidations = new Dictionary<string, Func<Type>>()
		{
			{ Constants.CountryCodes.UnitedStates, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.US.IOrgCusCodeValidation>(); } },
			{ Constants.CountryCodes.PuertoRico, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.US.IOrgCusCodeValidation>(); } },
			{ Constants.CountryCodes.SouthAfrica, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.ZA.IOrgCusCodeValidation>(); } },
			{ Constants.CountryCodes.Australia, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IOrgCusCodeValidation>(); } },
			{ Constants.CountryCodes.China, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.CN.IOrgCusCodeValidation>(); } },
			{ Constants.CountryCodes.Canada, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.CA.IOrgCusCodeValidation>(); } },
			{ Constants.CountryCodes.EuropeanUnion, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.EU.IOrgCusCodeValidation>(); } },
			{ Constants.CountryCodes.Germany, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.DE.IOrgCusCodeValidation>(); } },
			{ Constants.CountryCodes.NewZealand, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.NZ.IOrgCusCodeValidation>(); } },
			{ Constants.CountryCodes.Taiwan, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.TW.IOrgCusCodeValidation>(); } },
			{ Constants.CountryCodes.Spain, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.ES.IOrgCusCodeValidation>(); } },
			{ Constants.CountryCodes.Turkey, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.TR.IOrgCusCodeValidation>(); } },
			{ Constants.CountryCodes.Italy, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.IT.IOrgCusCodeValidation>(); } },
			{ Constants.CountryCodes.Netherlands, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.NL.IOrgCusCodeValidation>(); } },
			{ Constants.CountryCodes.Japan, () => ObjectFactory.GetType<Enterprise.Integration.Customs.JP.IOrgCusCodeValidation>() },
			{ Constants.CountryCodes.Norway, delegate { return ObjectFactory.GetType<Enterprise.Integration.Customs.NO.IOrgCusCodeValidation>(); } },
			{ Constants.CountryCodes.Brazil, () => ObjectFactory.GetType<Enterprise.Integration.Customs.BR.IOrgCusCodeValidation>() },
			{ Constants.CountryCodes.Poland, () => ObjectFactory.GetType<Enterprise.Integration.Customs.PL.IOrgCusCodeValidation>() },
			{ Constants.CountryCodes.UnitedArabEmirates, () => ObjectFactory.GetType<Enterprise.Integration.Customs.AE.IOrgCusCodeValidation>() },
			{ Constants.CountryCodes.Switzerland, () => ObjectFactory.GetType<Enterprise.Integration.Customs.CH.IOrgCusCodeValidation>() },
			{ Constants.CountryCodes.Liechtenstein, () => ObjectFactory.GetType<Enterprise.Integration.Customs.CH.IOrgCusCodeValidation>() },
			{ Constants.CountryCodes.KoreaSouth, () => ObjectFactory.GetType<Enterprise.Integration.Customs.KR.IOrgCusCodeValidation>() },
			{ Constants.CountryCodes.India, () => ObjectFactory.GetType<Enterprise.Integration.Customs.IN.IOrgCusCodeValidation>() },
		};

		#region IReadOnlySecurity Members

		protected bool GetReadOnlySecurity(PropertyDescriptor property)
		{
			return ShouldBeReadOnly() || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}

		bool ShouldBeReadOnly()
		{
			var header = Header;
			return IsInDatabase && (IsDeniedSecurity(header) || IsDeniedSSNSecurity() || IsReadOnlyAsEnrollmentRequirement());
		}

		bool IsDeniedSecurity(OrgHeader header)
		{
			return header != null && (header.GetFailingCheckpointWhenModifyRegistrationNumber(IsOriginalCompanyCodeTypePrimary) != null
						|| (!header.SecurityProvider.HasModifyReceivablesExternalDebtorSecurity && OK_CodeType == OrgCusCode.CodeTypes.ExternalDebtorAccountCode)
						|| (!header.SecurityProvider.HasModifyPayablesExternalCreditorSecurity && OK_CodeType == OrgCusCode.CodeTypes.ExternalCreditorAccountCode));
		}

		bool IsDeniedSSNSecurity()
		{
			return !AllowViewSocialSecurityNumber(base.OK_CodeType) && !OK_CodeTypeInfo.HasChanges;
		}

		bool IsReadOnlyAsEnrollmentRequirement()
		{
			return OK_CodeType == CodeTypes.BoleroTitleRegisterID
				&& Header.Logs.Find(log =>
					log.SL_SE_NKEvent == Events.MessageAccepted.Code
					&& log.SL_Reference.Contains((NoResString)"DEP=Bolero")
					&& log.SL_Reference.Contains((NoResString)"MST=Enrollment Request")
				).Any()
				&& !Env.CurrentUser.IsBatchProcessor;
		}

		#endregion

		#region IsPrimaryCompanyCode

		public ZBool IsCurrentCompanyCodeTypePrimary => IsCompanyCodeTypePrimary(OK_CodeType, CodeCountry);

		public ZBool IsOriginalCompanyCodeTypePrimary => IsCompanyCodeTypePrimary(OK_CodeTypeInfo.OriginalValue.ToString(), CodeCountry);

		public static ZBool IsCompanyCodeTypePrimary(string companyCodeType, RefCountry country)
		{
			if (country == null)
			{
				var primaryCodes = OrgCusCodeCountryFactory.GetFallbackIOrgCusCodeProvider().GetPrimaryCusCodes();
				return primaryCodes.Contains(companyCodeType);
			}
			else
			{
				var primaryCodes = OrgCusCodeCountryFactory.GetIOrgCusCodeProvider(country.Code).GetPrimaryCusCodes();
				return primaryCodes.Contains(companyCodeType);
			}
		}

		public ZPropertyInfo IsCurrentCompanyCodeTypePrimaryInfo
		{
			get { return GetZPropertyInfo(nameof(IsCurrentCompanyCodeTypePrimary)); }
		}

		public ZPropertyInfo IsOriginalCompanyCodeTypePrimaryInfo
		{
			get { return GetZPropertyInfo(nameof(IsOriginalCompanyCodeTypePrimary)); }
		}

		#endregion

		public override void Delete()
		{
			Header?.FindDuplicates();

			if (IsInDatabase)
			{
				AddNewLogToOrgHeader(Events.DeletedARecordInTheSystem);
			}

			if (OrgCusCodeValidity != null)
			{
				OrgCusCodeValidity.Delete();
			}

			if (IsBRCNPJ && FindBRRootCNPJ() is OrgCusCode cusCodeRootCNPJ)
			{
				cusCodeRootCNPJ.OK_CustomsRegNo = ZString.Empty;
			}

			base.Delete();
		}

		public override bool CanDelete
		{
			get
			{
				Organisation?.ClearValueCachedForValidation();
				return base.CanDelete && !IsRestricted;
			}
		}

		public bool IsRestricted => this.IsThereAnyTransactionForOrgWithRestrictedCusCode();

		ZString ViewDeniedMessage => Res.GetString("cd292400-8e29-45b1-9fd5-135a6f21ed82", "** View Denied due to Security Access **");

		public static bool AllowViewSocialSecurityNumber(string codeType)
		{
			return Env.Security.OrgDetailsViewPersonalInformation.IsAllowed || codeType != USACodeTypes.SocialSecurityNumber;
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (this.IsThereAnyTransactionForOrgWithRestrictedCusCode())
				{
					return ResString.GetMultilingualString("d06b9ef7-c3c9-4c46-b9db-64aa8ad34a75", "You cannot delete this Code.At least one transaction has been posted in a Portugal Login Company in this database using this Organization.");
				}
				else
				{
					return base.ReasonForNotAbleToDelete;
				}
			}
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			OK_CustomsRegNo = "1234567890";
		}
#endif

		#region IOrgCusCodeForMatching Members

		ZString IMatchingCusCode.OK_CodeTypeOriginalValue
		{
			get
			{
				return OK_CodeTypeInfo.OriginalValue.ToString();
			}
		}

		ZString IMatchingCusCode.OK_CustomsRegNoOriginalValue
		{
			get
			{
				return OK_CustomsRegNoInfo.OriginalValue.ToString();
			}
		}

		bool IMatchingCusCode.OK_CustomsRegNoHasChanges
		{
			get
			{
				return OK_CustomsRegNoInfo.HasChanges;
			}
		}

		bool IMatchingCusCode.OK_CodeTypeHasChanges
		{
			get
			{
				return OK_CodeTypeInfo.HasChanges;
			}
		}

		#endregion

		#region ICodeDescriptionWithCodeType

		string ICodeTypeCodeDescription.CodeType
		{
			get { return OK_CodeType; }
		}

		#endregion

		#region IEInvoicingEligibilityLiteRegistrationCode

		ZString IEInvoicingEligibilityLiteRegistrationCode.CountryCode => OK_RN_NKCodeCountry;

		ZString IEInvoicingEligibilityLiteRegistrationCode.CodeType => OK_CodeType;

		ZString IEInvoicingEligibilityLiteRegistrationCode.RegistrationNumber => OK_CustomsRegNo;

		#endregion
	}
}
