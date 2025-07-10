using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.Customs.TW.MessageDefinitions.NX5105;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	[TestedType(typeof(NX5105MessageBuilder))]
	sealed class NX5105MessageBuilderTest : BaseTWMessageBuilderTest<INX5105Declaration, Declaration>
	{
		class Message : INX5105Declaration
		{
			public Message()
			{
				Packaging = new DeclarationPackaging();
				Applications = new List<IApplication>() { new Application(), new Application() };
				GoodsShipment = new GoodsShipment();
				CurrencyExchange = new CurrencyExchange();
				Importer = new PartyDetails();
			}
			public static bool TestForEmptyValue;

			public static bool TestForOverLength;

			public ZDate AcceptanceDateTime => new ZDate(2011, 05, 23);

			public ZString Authentication => TestForEmptyValue ? "" : "Authentication";

			public ZString ID => "ID";

			public ZDecimal InvoiceAmount => 11.333;

			public ZDecimal TotalGrossMassMeasure => 12.3333333;

			public ZInt TotalPackageQuantity => 1;

			public ZString AssociatedGovernmentProcedureCode => TestForEmptyValue ? "" : "AssociatedGovernmentProcedureCode";

			public ZString CombinedNote => TestForEmptyValue ? "" : "CombinedNote";

			public ZString TypeCode => "TypeCode";

			public IAdditionalInformation AdditionalInformation => new AdditionalInformation();

			public IPartyDetails Agent => new PartyDetails();

			public ITransportMeans BorderTransportMeans => new BorderTransportMeans();

			public ICurrencyExchange CurrencyExchange { get; set; }

			public IDutyTaxFee DutyTaxFee => new DutyTaxFee();

			public IGoodsShipment GoodsShipment { get; set; }

			public IEnumerable<ZString> GovernmentProcedureDescriptions => Message.TestForEmptyValue ? new List<ZString>() { "" } : new List<ZString>() { "GovernmentProcedureDescription", "GovernmentProcedureDescription" };

			public IPartyDetails Importer { get; set; }

			public IDeclarationPackaging Packaging { get; }

			public ZString RepresentativePersonName => "RepresentativePerson";

			public IEnumerable<IApplication> Applications { get; }
		}

		class AdditionalInformation : IAdditionalInformation
		{
			public ZInt CopyQuantity => 2;

			public ZString StatementCode => null;

			public ZString StatementDescription => null;

			public ZString ProcessNumber => null;

			public ZString Content => null;

			public ZString ApprovalID => null;

			public ZString DelProcessNumber => null;

			ZString IAdditionalInformation.PackingHouse => "Packing House";
		}

		class PartyDetails : IPartyDetails
		{
			public PartyDetails()
			{
				ID = Message.TestForEmptyValue ? "" : "ID";
				ChineseName = Message.TestForEmptyValue ? "" : "ChineseName";
				CustomsControlID = Message.TestForEmptyValue ? "" : "96944490";
				LPCOAuthorizedParty = new LPCOAuthorizedParty();
				Communications = new List<ICommunication> { new Communication(), new Communication() };
				Address = new Address();
				PaymentOnAccountBusinessID = null;
			}

			public ZString ID { get; set; }

			public ZString RoleCode => "RoleCode";

			public ZString SubBoxID => "SubBoxID";

			public ILPCOAuthorizedParty LPCOAuthorizedParty { get; set; }

			public ZString Name { get; set; }

			public ZString ChineseName { get; set; }

			public virtual ZString TypeCode { get; set; }

			public ZString CustomsControlID { get; set; }

			public ZString PaymentOnAccountBusinessID { get; set; }

			public IAddress Address { get; set; }

			public IEnumerable<ICommunication> Communications { get; set; }

			public ZString ContactName { get; set; }

			public ZString MainManufacturer => null;

			public ZString UndertakeCode => null;

			public ZString OwnerName => null;

			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;
		}

		class LPCOAuthorizedParty : ILPCOAuthorizedParty
		{
			public LPCOAuthorizedParty() : this(Message.TestForOverLength ? "REALLYLONGLPCOAUTHORIZEDPARTYIDREALLYLONGLPCOAUTHORIZEDPARTYID" : "ID", null, "Typ")
			{ }

			public LPCOAuthorizedParty(ZString id, ZString name, ZString typeCode)
			{
				ID = id;
				Name = name;
				TypeCode = typeCode;
			}

			public ZString ID
			{
				get => Message.TestForEmptyValue ? ZString.Empty : fID;
				set => fID = value;
			}
			ZString fID;

			public ZString Name { get; set; }

			public ZString TypeCode { get; set; }
		}

		class BorderTransportMeans : ITransportMeans
		{
			public ZDate ArrivalDateTime => new ZDate(2011, 05, 23);

			public ZString TypeCode => "TypeCode";

			public IEnumerable<ZString> ItineraryRoutingCountryCodes => new List<ZString>() { "Itinerary", "Itinerary" };

			public ZString ID => null;

			public ZString JourneyID => null;

			public ZString Registration => Message.TestForEmptyValue ? "" : "R";

			public ZString Name => null;

			public ZString CallSignID => null;
		}

		class CurrencyExchange : ICurrencyExchange
		{
			public CurrencyExchange()
			{
				CurrencyTypeCode = "CurrencyTypeCode";
				RateNumeric = 13.333333m;
			}
			public ZString CurrencyTypeCode { get; set; }

			public ZDecimal RateNumeric { get; set; }
		}

		class DutyTaxFee : IDutyTaxFee
		{
			public ZString DutyExemptionWaiverNote => Message.TestForEmptyValue ? "" : "DutyExemptionWaiverNote";

			public ZString DutyMemoPrinted => Message.TestForEmptyValue ? "" : "DutyMemoPrinted";

			public ZString DutyMethodCode => "DutyMethodCode";

			public ZDecimal TotalDutyTaxFeeAmount => 14.3;

			public ZString PaymentObligationGuaranteeReferenceID => "Payment";

			public ZDecimal TotalCashDutyTaxFeeAmount => ZDecimal.Zero;

			public ZDecimal TotalNonCashDutyTaxFeeAmount => ZDecimal.Zero;
		}

		class GoodsShipment : IGoodsShipment
		{
			public GoodsShipment()
			{
				Seller = new PartyDetails();
				Consignment = new Consignment();
			}
			public ZDateTime ExitDateTime => new ZDateTime(2011, 05, 23, 9, 30, 3);

			[DecimalPlaces(2)]
			public ZDecimal ItemChargeAmount => 15.333;

			public ZDecimal TotalCIFAmount => 16.3;

			public IEnumerable<IAdditionalDocument> AdditionalDocuments => new List<IAdditionalDocument>() { new AdditionalDocument(), new AdditionalDocument() };

			public IPartyDetails Consignee => new Consignee();

			public IConsignment Consignment { get; set; }

			public IPartyDetails Consignor => new Consignor();

			public ICustomsValuation CustomsValuation => new CustomsValuation();

			public ZString DeliveryDestinationName => "DeliveryDestination";

			public IEnumerable<IGoodsShipmentDutyTaxFee> DutyTaxFees => new List<IGoodsShipmentDutyTaxFee>() { new GoodsShipmentDutyTaxFee(), new GoodsShipmentDutyTaxFee() };

			public IEnumerable<IGovernmentAgencyGoodsItem> GovernmentAgencyGoodsItems => new List<IGovernmentAgencyGoodsItem>() { new GovernmentAgencyGoodsItem(), new GovernmentAgencyGoodsItem() };

			public IPartyDetails NotifyParty => new PartyDetails();

			public IPartyDetails Seller { get; set; }

			public ZString TradeTermsConditionCode => "TradeTerms";

			public ZString UCR => "UCR";

			public IPartyDetails Buyer => null;

			public IPartyDetails Exporter => null;

			public IEnumerable<IGoodsMeasure> GoodsMeasures => null;

			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;

			public IEnumerable<IAdditionalDeclaration> AdditionalDeclarations => null;
		}

		class AdditionalDocument : IAdditionalDocument
		{
			public AdditionalDocument()
			{
				ID = "ID";
			}

			public ZString ID { get; set; }

			ZString IAdditionalDocument.ID => Message.TestForEmptyValue ? ZString.Empty : ID;

			public ZString Content => Message.TestForEmptyValue ? "" : "Content";

			public ZString ImageFileFormat => Message.TestForEmptyValue ? "" : "ImageFileFormat";

			public ZString ImageFileName => Message.TestForEmptyValue ? "" : "ImageFileName";

			public ZLong SizeMeasure => Message.TestForEmptyValue ? 0 : 3;

			public ZString TypeCode => Message.TestForEmptyValue ? "" : "TypeCode";

			public ZString ResponsibleGovernmentAgency => Message.TestForEmptyValue ? "" : "ResponsibleGovernmentAgency";

			public ZInt SequenceNumeric => 0;

			ZDate IAdditionalDocument.SlaughterDateTime => new ZDate(2011, 05, 23);
		}

		class Address : IAddress
		{
			public Address()
			{
				Line = "Line";
				ChineseLine = "ChineseLine";
				CountryCode = "TW";
			}

			public ZString Line { get; set; }

			public ZString ChineseLine { get; set; }

			public ZString CountryCode { get; set; }

			public ZString CountrySubDivisionID => null;

			public ZString CountrySubDivisionName => null;
		}

		class Consignment : IConsignment
		{
			public Consignment()
			{
				BondedGoods = new BondedGoods();
			}

			public ZString ManifestSerialNumber => Message.TestForEmptyValue ? "" : "ManifestSerialNumber";

			public IEnumerable<IAdditionalInformation> AdditionalInformations => new List<IAdditionalInformation>() { new ConsignmentAdditionalInformation(), new ConsignmentAdditionalInformation() };

			public ZString ArrivalTransportMeansTypeCode => "ArrivalTransportMeans";

			public ITransportMeans BorderTransportMeans => new BorderTransportMeans();

			public IPartyDetails Carrier => new PartyDetails();

			public IConsignmentItem ConsignmentItem => new ConsignmentItem();

			public ZString GoodsLocation => "LoadingLocation";

			public ILocation LoadingLocation => new Location("LoadingLocation");

			public IEnumerable<ITransportContractDocument> TransportContractDocuments => new List<ITransportContractDocument>() { new TransportContractDocument(), new TransportContractDocument() };

			public IEnumerable<ITransportEquipment> TransportEquipments => new List<ITransportEquipment>() { new TransportEquipment(), new TransportEquipment() };

			public IBondedGoods BondedGoods { get; set; }

			public ZString ShippingOrderNumber => null;

			public ITransportMeans DepartureTransportMeans => null;

			public ZString TransitTransportMeansTypeCode => null;

			public IEnumerable<ZString> GoodsLocations => null;

			public ILocation UnloadingLocation => null;

			public ITransportContractDocument TransportContractDocument => null;

			public IGovernmentAgencyGoodsItem GovernmentAgencyGoodsItem => null;

			public ILocation TranshipmentLocation => null;

			public ILocation TransitDeparture => null;
		}

		class ConsignmentAdditionalInformation : IAdditionalInformation
		{
			public ZString StatementCode => "StatementCode";

			public ZString StatementDescription => "StatementDescription";

			public ZInt CopyQuantity => -1;

			public ZString ProcessNumber => null;

			public ZString Content => null;

			public ZString ApprovalID => null;

			public ZString ElectronicReceipt => null;

			public ZString DelProcessNumber => null;

			ZString IAdditionalInformation.PackingHouse => null;
		}

		class ConsignmentItem : IConsignmentItem
		{
			public ZString Split => Message.TestForEmptyValue ? "" : "Split";

			public ICommodity Commodity => null;

			public IGoodsMeasure GoodsMeasure => null;

			public IPackaging Packaging => null;

			public IEnumerable<ITransportContractDocument> TransportContractDocuments => null;

			public IOrigin Origin => null;

			public ZString AssociatedGovernmentProcedureCode => null;
		}

		class TransportContractDocument : ITransportContractDocument
		{
			public ZString ID => "ID";

			public ZString TypeCode => "TypeCode";

			public IPartyDetails Deconsolidator => null;
		}

		class TransportEquipment : ITransportEquipment
		{
			public ZString CharacteristicCode => "CharacteristicCode";

			public ZString ID => "ID";

			public ZString UsedCapacityCode => "UsedCapacityCode";

			public IEnumerable<ZString> Seals => new List<ZString>() { "Seal", "Seal" };
		}

		class BondedGoods : IBondedGoods
		{
			public BondedGoods()
			{
				BondedGoodsMonthlyReport = new BondedGoodsMonthlyReport();
			}

			public ZString AddDutyReasonCode => Message.TestForEmptyValue ? "" : "AddDutyReasonCode";

			public IEnumerable<IBondedGoodsInvoice> BondedGoodsInvoices => new List<IBondedGoodsInvoice>() { new BondedGoodsInvoice(), new BondedGoodsInvoice() };

			public IBondedGoodsMonthlyReport BondedGoodsMonthlyReport { get; set; }

			public IBondedParty InBondedParty => new BondedParty();

			public IBondedParty OutBondedParty => new BondedParty();

			public IEnumerable<IBondedParty> PreBondedParties => new List<IBondedParty>() { new BondedParty(), new BondedParty() };

			public ZString DocumentCode => null;

			public ZString Refundable => null;

			public IEnumerable<IBondedParty> BondedFactories => null;
		}

		class BondedGoodsInvoice : IBondedGoodsInvoice
		{
			public ZString ID => "ID";

			public ZDecimal ValueAmount => 1;
		}

		class BondedGoodsMonthlyReport : IBondedGoodsMonthlyReport
		{
			public BondedGoodsMonthlyReport()
			{
				MonthNumeric = -1;
				TraderReferenceID = "TraderReferenceID";
			}

			public ZInt MonthNumeric { get; set; }

			public ZString TraderReferenceID { get; set; }
		}

		class BondedParty : IBondedParty
		{
			public ZString BondedID => "BondedID";

			public ZString ID => "ID";

			public ZString TypeCode => "TypeCode";

			public ZString CustomsControlID => null;
		}

		class Consignee : PartyDetails
		{
			public override ZString TypeCode => Message.TestForEmptyValue ? "" : "TypeCode";
		}

		class Consignor : IPartyDetails
		{
			public ZString ID => "ID";

			public ZString Name => "Name";

			public ZString ChineseName => "ChineseName";

			public ZString TypeCode => Message.TestForEmptyValue ? "" : "TypeCode";

			public IAddress Address => new Address();

			public ZString CustomsControlID => null;

			public ZString PaymentOnAccountBusinessID => null;

			public ZString RoleCode => null;

			public ZString SubBoxID => null;

			public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

			public IEnumerable<ICommunication> Communications => null;

			public ZString ContactName => null;

			public ZString MainManufacturer => null;

			public ZString UndertakeCode => null;

			public ZString OwnerName => null;

			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;
		}

		class CustomsValuation : ICustomsValuation
		{
			public ZDecimal ExitToEntryChargeAmount => 17.333;

			public ZDecimal FreightChargeAmount => 18.333;

			public ZDecimal OtherChargeDeductionAmount => 19.3;

			public ZString PartyRelationshipCode => "PartyRelationshipCode";

			public ZDecimal OtherChargeAmount => 20.333;

			public ZDecimal OtherDeductionAmount => 21.333;

			public ZDecimal InvoiceAmount => ZDecimal.Zero;

			public ZDecimal TotalDutyTaxFeeAmount => ZDecimal.Zero;
		}

		class GoodsShipmentDutyTaxFee : IGoodsShipmentDutyTaxFee
		{
			public ZDecimal AdValoremTaxBaseAmount => 22.3;

			public ZString TypeCode => "TypeCode";
		}

		class GovernmentAgencyGoodsItem : IGovernmentAgencyGoodsItem
		{
			public ZInt SequenceNumeric => 5;

			public IEnumerable<IAdditionalDocument> AdditionalDocuments => new List<IAdditionalDocument>() { new AdditionalDocument(), new AdditionalDocument() };

			public IEnumerable<IAdditionalInformation> AdditionalInformations => new List<IAdditionalInformation>() { new GovernmentAgencyGoodsItemAdditionalInformation(), new GovernmentAgencyGoodsItemAdditionalInformation() };

			public ICommodity Commodity => new Commodity();

			public IGoodsMeasure GoodsMeasure => new GoodsMeasure();

			public IPartyDetails Manufacturer => new Manufacturer();

			public IOrigin Origin => new Origin();

			public IPackaging Packaging => new Packaging();

			public IPreviousDocument PreviousDocument => new PreviousDocument();

			public ILPCODetail ApprovalDocument => new ApprovalDocument();

			public ICommoditySpecification CommoditySpecification => new CommoditySpecification();

			public IGoodsLicensingStatisticalMeasure GoodsLicensingStatisticalMeasure => new GoodsLicensingStatisticalMeasure();

			public IGoodsStatisticalMeasure GoodsStatisticalMeasure => new GoodsStatisticalMeasure();

			public ILPCODetail MedicalInstrument => new MedicalInstrument();

			public IPreviousDocument PreBondedDocument => new PreBondedDocument();

			public IEnumerable<IShippingIdentification> ShippingIdentifications => new List<IShippingIdentification>() { new ShippingIdentification(), new ShippingIdentification() };

			public IGovernmentProcedure GovernmentProcedure => null;

			public ZDateTime ControlInspectionStartDateTime => ZDateTime.Empty;

			public ZString ExaminationPlace => null;

			public IEnumerable<ITransportEquipment> TransportEquipments => null;

			public IAdditionalDeclaration AdditionalDeclaration => null;

			public ZString CriteriaCode => null;

			public ZString PreferentialCriteria => null;

			public ZString ProducerCode => null;

			public ZString OtherCriteria => null;
		}

		class GovernmentAgencyGoodsItemAdditionalInformation : IAdditionalInformation
		{
			public ZString StatementCode => "StatementCode";

			public ZString StatementDescription => "StatementDescription";

			public ZInt CopyQuantity => -1;

			public ZString ProcessNumber => null;

			public ZString Content => null;

			public ZString ApprovalID => null;

			public ZString ElectronicReceipt => null;

			public ZString DelProcessNumber => null;

			ZString IAdditionalInformation.PackingHouse => null;
		}

		class Commodity : ICommodity
		{
			public Commodity()
			{
				CommercialCategorizationID = Message.TestForEmptyValue ? "" : "CommercialCategorizationID";
				Name = Message.TestForEmptyValue ? "" : "Name";
				Description = new string('A', 509) + "512HereOverMaxlength";
				ChineseDescription = Message.TestForEmptyValue ? "" : "ChineseDescription";
				Constituent = new Constituent();
			}
			public ZString CommercialCategorizationID { get; set; }

			public ZString Description { get; set; }

			public ZString GoodsGroupNameCode => Message.TestForEmptyValue ? "" : "GoodsGroupNameCode";

			public ZString Name { get; set; }

			public ZString BarCode => Message.TestForEmptyValue ? "" : "BarCode";

			public ZString ChineseDescription { get; set; }

			public ZString EnglishDescription => "EnglishDescription";

			public ZString CITESImportPermitID => Message.TestForEmptyValue ? "" : "CITESImportPermitID";

			public ZString FTATariffCode => Message.TestForEmptyValue ? "" : "FTATariffCode";

			public ZString SHTCImportPermitID => Message.TestForEmptyValue ? "" : "SHTCImportPermitID";

			public ZString TariffCodeExtensionCode => Message.TestForEmptyValue ? "" : "TariffCodeExtensionCode";

			public IEnumerable<IAdditionalDocument> AdditionalDocuments => new List<IAdditionalDocument>() { new AdditionalDocument(), new AdditionalDocument() };

			public IEnumerable<IClassification> Classifications => new List<IClassification>() { new Classification(), new Classification() };

			public ICommodityRelatedPackaging CommodityRelatedPackaging => new CommodityRelatedPackaging();

			public IConstituent Constituent { get; set; }

			public ICommodityDutyTaxFee DutyTaxFee => new CommodityDutyTaxFee();

			public IGovernmentProcedure GovernmentProcedure => new GovernmentProcedure();

			public IEnumerable<ZString> HandlingInstructionsCodes => Message.TestForEmptyValue ? new List<ZString>() : new List<ZString>() { "1", "2", "", "4", "5", "6", "7", "8", "9", "0" };

			public IInvoiceLine InvoiceLine => new InvoiceLine();

			public IPreviousDocument PreviousDocument => new PreviousDocument();

			public IEnumerable<ICommodityNumber> CommodityNumbers => new List<ICommodityNumber>() { new CommodityNumber(), new CommodityNumber() };

			public IEnumerable<IDutyOtherTaxFee> DutyOtherTaxFees => new List<IDutyOtherTaxFee>() { new DutyOtherTaxFee(), new DutyOtherTaxFee() };

			public IDutyTaxFeeAmount DutyTaxFeeAmount => new DutyTaxFeeAmount();

			public IDutyTaxFeeQuantity DutyTaxFeeQuantity => new DutyTaxFeeQuantity();

			public IFood Food => new Food();

			public IQuarantine Quarantine => new Quarantine();

			public IVehicle Vehicle => new Vehicle();

			public IWine Wine => new Wine();

			public ZString CargoDescription => null;

			public ZString BondedNoteCode => null;

			public IEnumerable<ZString> VehicleIDs => null;

			public IInvoice Invoice => null;

			public ZString PrintingTariffCode => null;

			IClassification ICommodity.Classification => null;
		}

		class Classification : IClassification
		{
			public ZString ID => "ID";

			public ZString IdentificationTypeCode => "IdentificationTypeCode";
		}

		class CommodityRelatedPackaging : ICommodityRelatedPackaging
		{
			public CommodityRelatedPackaging()
			{
				if (!Message.TestForEmptyValue)
				{
					PackingMethodDescription = "PackingMethodDescription";
					MaterialCode = "MaterialCode";
					Specification = "Specification";
				}
			}

			public ZString PackingMethodDescription { get; set; }

			public ZString MaterialCode { get; set; }

			public ZString Specification { get; set; }
		}

		class Constituent : IConstituent
		{
			public Constituent()
			{
				if (!Message.TestForEmptyValue)
				{
					ElementDescription = "ElementDescription";
					LevelID = "LevelID";
					Thickness = "Thickness";
				}
			}

			public ZString ElementDescription { get; set; }

			public ZString LevelID { get; set; }

			public ZString Thickness { get; set; }
		}

		class CommodityDutyTaxFee : ICommodityDutyTaxFee
		{
			public ZDecimal AdValoremTaxBaseAmount => 23.3;

			public ZString DutyRegimeCode => Message.TestForEmptyValue ? "" : "DutyRegimeCode";

			public ZDecimal SpecificTaxBaseQuantity => Message.TestForEmptyValue ? 0m : 24.3333333m;

			public ZDecimal PercentageNumeric => 25.33333;
		}

		class GovernmentProcedure : IGovernmentProcedure
		{
			public ZString CurrentCode => "CurrentCode";

			public ZString TransportTypeCode => null;

			public ZString Description => null;
		}

		class InvoiceLine : IInvoiceLine
		{
			public ZString ChargesTypeCode => "ChargesTypeCode";

			public ZString CurrencyTypeCode => "CurrencyTypeCode";

			public ZDecimal UnitPriceAmount => 26.3333333;

			public ZDecimal ItemChargeAmount => -1;

			public ZDecimal SubTotalAmount => ZDecimal.Zero;
		}

		class PreviousDocument : IPreviousDocument
		{
			public PreviousDocument() : this("ID", null, 0)
			{ }

			public PreviousDocument(ZString id, ZString functionalReferenceID, ZInt lineNumeric)
			{
				ID = id;
				FunctionalReferenceID = functionalReferenceID;
				LineNumeric = lineNumeric;
			}

			public ZString ID { get; set; }

			public ZString FunctionalReferenceID { get; set; }

			public ZInt LineNumeric { get; set; }
		}

		class CommodityNumber : ICommodityNumber
		{
			public ZString ID => "ID";

			public ZString IdentifierTypeCode => "IdentifierTypeCode";
		}

		class DutyOtherTaxFee : IDutyOtherTaxFee
		{
			public ZString MethodCode => "MethodCode";

			public ZDecimal TaxRateNumeric => 28.333333;

			public ZString TypeCode => "TypeCode";

			public ZString MethodOfCalculation => "MethodOfCalculation";

			public ZDecimal PercentageNumeric => ZDecimal.Zero;
		}

		class DutyTaxFeeAmount : IDutyTaxFeeAmount
		{
			public ZDecimal TaxRateNumeric => 30.333333;

			public ZDecimal PercentageNumeric => ZDecimal.Zero;
		}

		class DutyTaxFeeQuantity : IDutyTaxFeeQuantity
		{
			public ZString DutyUnitCode => "DutyUnitCode";

			public ZDecimal TaxRateNumeric => 32.33333;

			public ZDecimal PercentageNumeric => ZDecimal.Zero;
		}

		class Food : IFood
		{
			public Food(ZDecimal? phValue, ZDecimal? sterilizationValue, List<IFoodConstituent> constituents)
			{
				PHValueNumeric = phValue;
				SterilizationValueNumeric = sterilizationValue;
				Constituents = constituents;
			}

			public Food() : this(5.33m, 6.33m, new List<IFoodConstituent>() { new FoodConstituent(), new FoodConstituent() })
			{ }

			public ZDecimal? PHValueNumeric { get; }

			public ZDecimal? SterilizationValueNumeric { get; }

			public IEnumerable<IFoodConstituent> Constituents { get; }
		}

		class FoodConstituent : IFoodConstituent
		{
			public FoodConstituent(ZString elementName, ZDecimal elementPercentNumeric)
			{
				ElementName = elementName;
				ElementPercentNumeric = elementPercentNumeric;
			}

			public FoodConstituent() : this("ElementName", 1.11111m)
			{ }

			public ZString ElementName { get; }

			public ZDecimal ElementPercentNumeric { get; }
		}

		class Quarantine : IQuarantine
		{
			public Quarantine(bool testdAditionalDocumentEmpty = false, bool testAdditionalInformationEmpty = false, bool testPackingEmpty = false)
			{
				if (!Message.TestForEmptyValue)
				{
					ObjectFeature = "ObjectFeature";
					Treatment = "Treatment";
					Animal = new Animal();
				}

				additionalDocument = testdAditionalDocumentEmpty ? null : new[] { new AdditionalDocument(), new AdditionalDocument() };
				additionalInformation = testAdditionalInformationEmpty ? null : new[] { new AdditionalInformation(), new AdditionalInformation() };
				packing = testPackingEmpty ? null : new[] { new Packaging(), new Packaging() };
			}

			readonly IEnumerable<IAdditionalDocument> additionalDocument;
			readonly IEnumerable<IAdditionalInformation> additionalInformation;
			readonly IEnumerable<IPackaging> packing;

			public ZString ObjectFeature { get; set; }

			public ZString Treatment { get; set; }

			public IAnimal Animal { get; set; }

			IEnumerable<IAdditionalDocument> IQuarantine.AdditionalDocument => additionalDocument;

			IEnumerable<IAdditionalInformation> IQuarantine.AdditionalInformation => additionalInformation;

			IEnumerable<IPackaging> IQuarantine.Packing => packing;
		}

		class Animal : IAnimal
		{
			public Animal()
			{
				if (!Message.TestForEmptyValue)
				{
					AgeMonthNumeric = 7;
					AgeYearNumeric = 8;
					FemaleQuantity = 9;
					MaleQuantity = 10;
					MicrochipID = "MicrochipID";
					Vaccination = "Vaccination";
				}
			}

			public ZInt AgeMonthNumeric { get; set; }

			public ZInt AgeYearNumeric { get; set; }

			public ZInt FemaleQuantity { get; set; }

			public ZInt MaleQuantity { get; set; }

			public ZString MicrochipID { get; set; }

			public ZString Vaccination { get; set; }
		}

		class Vehicle : IVehicle
		{
			public ZString Catalyst => "Catalyst";

			public ZString ClassificationCode => "ClassificationCode";

			public ZInt CylinderQuantity => 11;

			public ZString Displace => "Displace";

			public ZInt DoorQuantity => 12;

			public ZString DrivingSide => "DrivingSide";

			public ZString FuelTypeCode => "FuelTypeCode";

			public ZInt ModelYearNumeric => 13;

			public ZInt SeatQuantity => 14;

			public ZString StatusCode => "StatusCode";

			public ZString TransmissionTypeCode => "TransmissionTypeCode";

			public IEnumerable<ZString> VehicleIDs => new List<ZString>() { "VehicleID", "VehicleID" };
		}

		class Wine : IWine
		{
			public Wine(ZInt ageNumeric, ZDecimal alcoholContentNumeric, ZDateTime bottledDate, ZDecimal coverLotNumberAmount, ZString geographicRegion, ZDecimal originalNonLotNumberAmount,
				ZDateTime productBestBeforeDateTime, ZDateTime productExpiryDateTime, ZDecimal removeLotNumberAmount, ZInt yearNumeric)
			{
				AgeNumeric = ageNumeric;
				AlcoholContentNumeric = alcoholContentNumeric;
				BottledDate = bottledDate;
				CoverLotNumberAmount = coverLotNumberAmount;
				GeographicRegion = geographicRegion;
				OriginalNonLotNumberAmount = originalNonLotNumberAmount;
				ProductBestBeforeDateTime = productBestBeforeDateTime;
				ProductExpiryDateTime = productExpiryDateTime;
				RemoveLotNumberAmount = removeLotNumberAmount;
				YearNumeric = yearNumeric;
			}

			public Wine() : this(15, 33.3333, Message.TestForEmptyValue ? ZDateTime.Empty : new ZDateTime(2011, 05, 24), 34.33333, "GeographicRegion", 35.33333, new ZDateTime(2011, 05, 23, 9, 30, 5), new ZDateTime(2011, 05, 23, 9, 30, 6), 36.33333, 16)
			{ }

			public ZInt AgeNumeric { get; }

			public ZDecimal AlcoholContentNumeric { get; }

			public ZDateTime BottledDate { get; }

			public ZDecimal CoverLotNumberAmount { get; }

			public ZString GeographicRegion { get; }

			public ZDecimal OriginalNonLotNumberAmount { get; }

			public ZDateTime ProductBestBeforeDateTime { get; }

			public ZDateTime ProductExpiryDateTime { get; }

			public ZDecimal RemoveLotNumberAmount { get; }

			public ZInt YearNumeric { get; }
		}

		class GoodsMeasure : IGoodsMeasure
		{
			public ZDecimal NetWeightMeasure => 37.3333333;

			public ZDecimal TariffQuantity => 38.33333;

			public ZString UnitCode => "UnitCode";

			ZString IGoodsMeasure.CustomUnitCode => ZString.Empty;
		}

		class Manufacturer : IPartyDetails
		{
			public ZString ID => "ID";

			public ZString Name => "Name";

			public IAddress Address => new Address();

			public ZString ContactName => "Contact";

			public ZString TypeCode => null;

			public ZString ChineseName => null;

			public ZString CustomsControlID => null;

			public ZString PaymentOnAccountBusinessID => null;

			public ZString RoleCode => null;

			public ZString SubBoxID => null;

			public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

			public IEnumerable<ICommunication> Communications => new List<ICommunication> { new Communication() };

			public ZString MainManufacturer => null;

			public ZString UndertakeCode => null;

			public ZString OwnerName => null;

			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;
		}

		class Communication : ICommunication
		{
			public Communication()
			{
				ID = "ID";
				TypeID = "TE";
			}
			public ZString ID { get; set; }

			public ZString TypeID { get; set; }
		}

		class Origin : IOrigin
		{
			public ZString CountryCode => "CountryCode";

			public IAdditionalDocument AdditionalDocument => new AdditionalDocument();
		}

		class Packaging : IPackaging
		{
			public ZDecimal QuantityQuantity => 18m;

			public ZString TypeCode => "TypeCode";

			public ZString MarksNumbers => null;

			public ZString PackagingMaterialDescription => null;

			public ZString Combination => null;

			ZDate IPackaging.PackingDateTime => new ZDate(2011, 05, 23);
		}

		class ApprovalDocument : ILPCODetail
		{
			public ApprovalDocument() : this("LPCOExemptionCode", "LPCOID", new LPCOAuthorizedParty())
			{ }

			public ApprovalDocument(ZString lpcoExemptionCode, ZString lpcoID, ILPCOAuthorizedParty lpcoAuthorizedParty)
			{
				LPCOExemptionCode = lpcoExemptionCode;
				LPCOID = lpcoID;
				LPCOAuthorizedParty = lpcoAuthorizedParty;
			}

			public ZString LPCOExemptionCode { get; set; }

			public ZString LPCOID { get; set; }

			public ILPCOAuthorizedParty LPCOAuthorizedParty { get; set; }
		}

		class CommoditySpecification : ICommoditySpecification
		{
			public ZString CharacteristicQualifierCode => "CharacteristicQualifierCode";

			public ZString ElementDescription => "ElementDescription";
		}

		class GoodsLicensingStatisticalMeasure : IGoodsLicensingStatisticalMeasure
		{
			public GoodsLicensingStatisticalMeasure()
			{
				if (!Message.TestForEmptyValue)
				{
					LicensingQuantity = 39.33333;
					StatisticalUnitCode = "StatisticalUnitCode";
					ResponsibleGovernmentAgency = "ResponsibleGovernmentAgency";
				}
			}

			public ZDecimal LicensingQuantity { get; set; }

			public ZString StatisticalUnitCode { get; set; }

			public ZString ResponsibleGovernmentAgency { get; set; }
		}

		class GoodsStatisticalMeasure : IGoodsStatisticalMeasure
		{
			public ZString StatisticalUnitCode => "StatisticalUnitCode";

			public ZDecimal TariffQuantity => 1;
		}

		class MedicalInstrument : ILPCODetail
		{
			public MedicalInstrument(ZString lpcoID, ILPCOAuthorizedParty lpcoAuthorizedParty)
			{
				LPCOID = lpcoID;
				LPCOAuthorizedParty = lpcoAuthorizedParty;
			}

			public MedicalInstrument() : this("LPCOID", new LPCOAuthorizedParty())
			{ }

			public ZString LPCOID { get; }

			public ILPCOAuthorizedParty LPCOAuthorizedParty { get; }

			public ZString LPCOExemptionCode => null;
		}

		class PreBondedDocument : IPreviousDocument
		{
			public ZString ID => "ID";

			public ZInt LineNumeric => 20;

			public ZString FunctionalReferenceID => null;
		}

		class ShippingIdentification : IShippingIdentification
		{
			public ShippingIdentification() : this("LotNumberID", new ZDateTime(2011, 05, 23, 9, 30, 7), 40.33333, Message.TestForEmptyValue ? ZDateTime.Empty : new ZDateTime(2011, 05, 25))
			{ }

			public ShippingIdentification(ZString lotNumberID, ZDateTime productBestBeforeDateTime, ZDecimal productLotNumberAmount, ZDateTime productManufacturedDate)
			{
				LotNumberID = lotNumberID;
				ProductBestBeforeDateTime = productBestBeforeDateTime;
				ProductLotNumberAmount = productLotNumberAmount;
				ProductManufacturedDate = productManufacturedDate;
			}

			public ZString LotNumberID { get; set; }

			public ZDateTime ProductBestBeforeDateTime { get; set; }

			public ZDecimal ProductLotNumberAmount { get; set; }

			public ZDateTime ProductManufacturedDate { get; set; }
		}

		class DeclarationPackaging : IDeclarationPackaging
		{
			public DeclarationPackaging()
			{
				MarksNumbers = "MarksNumbers";
			}

			public ZString MarksNumbers { get; set; }

			public ZString PackagingMaterialDescription => Message.TestForEmptyValue ? "" : "PackagingMaterialDescription";

			public ZString Combination => Message.TestForEmptyValue ? "" : "Combination";

			public ZString TypeCode => "TypeCode";
		}

		class Application : IApplication
		{
			public Application()
			{
				BankAccount = "BankAccount";
				ContactOffice = "ContactOffice";
				ResponsibleGovernmentAgency = "ResponsibleGovernmentAgency";
				Appointment = new Appointment();
				ApprovalAuthenticationInformation = "ApprovalAuthenticationInformation";
				AuthorizedInformation = new AuthorizedInformation();
				Declarer = new Declarer();
				ItemGroupReferenceSequenceNumerics = new List<ZInt>() { 1, 1 };
				Labels = new List<ILabel>() { new Label(), new Label() };
				LocalManufacturer = new LocalManufacturer();
				Wine = new ApplicationWine();
				Payment = new Payment() { MethodCode = "Payment" };
			}

			public ZString FunctionalReferenceID => "FunctionalReferenceID";

			public ZString ID => "ID";

			public ZString PurposeCode => "PurposeCode";

			public ZString TypeCode => "TypeCode";

			public IEnumerable<IAdditionalDocument> AdditionalDocuments => new List<IAdditionalDocument>() { new AdditionalDocument(), new AdditionalDocument() };

			public IApplicationAdditionalInformation AdditionalInformation => new ApplicationAdditionalInformation();

			public IPartyDetails Agent => new ApplicationAgent();

			public ZString BankAccount { get; set; }

			public ZString ContactOffice { get; set; }

			public ZString ResponsibleGovernmentAgency { get; set; }

			public IAppointment Appointment { get; set; }

			public ZString ApprovalAuthenticationInformation { get; set; }

			public IAuthorizedInformation AuthorizedInformation { get; set; }

			public IPartyDetails Declarer { get; set; }

			public IEnumerable<ZInt> ItemGroupReferenceSequenceNumerics { get; set; }

			public IEnumerable<ILabel> Labels { get; set; }

			public IPartyDetails LocalManufacturer { get; set; }

			public IApplicationWine Wine { get; set; }

			public IPayment Payment { get; set; }

			public IPartyDetails Applicant => null;
		}

		class Payment : IPayment
		{
			public ZString MethodCode { get; set; }

			public ZString ReferenceID { get; set; }
		}

		class ApplicationAdditionalInformation : IApplicationAdditionalInformation
		{
			public ApplicationAdditionalInformation()
			{
				AddressChineseLine = "AddressChineseLine";
			}

			public ZString StatementDescription => "StatementDescription";

			public ZString DeductionSample => "DeductionSample";

			public ZString ElectronicReceipt => "ElectronicReceipt";

			public ZString ProvedPaper => "ProvedPaper";

			public ZString ReturnSample => "ReturnSample";

			public ZString AddressChineseLine { get; set; }

			public ZString BulkApplicationID => null;

			public ZString BulkPortCode => null;
		}

		class ApplicationAgent : IPartyDetails
		{
			public ZString ID => "ID";

			public ZString Name => "Name";

			public ZString TypeCode => "TypeCode";

			public IAddress Address => new Address();

			public IEnumerable<ICommunication> Communications => new List<ICommunication>() { new Communication(), new Communication() };

			public ZString ChineseName => null;

			public ZString CustomsControlID => null;

			public ZString PaymentOnAccountBusinessID => null;

			public ZString RoleCode => null;

			public ZString SubBoxID => null;

			public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

			public ZString ContactName => null;

			public ZString MainManufacturer => null;

			public ZString UndertakeCode => null;

			public ZString OwnerName => null;

			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;
		}

		class Appointment : IAppointment
		{
			public Appointment()
			{
				ReservationDate = new ZDateTime(2011, 05, 26);
				ReservationPeriodCode = "ReservationPeriodCode";
			}
			public ZDateTime ReservationDate { get; set; }

			ZDateTime IAppointment.ReservationDate => Message.TestForEmptyValue ? ZDateTime.Empty : ReservationDate;

			public ZString ReservationPeriodCode { get; set; }
		}

		class AuthorizedInformation : IAuthorizedInformation
		{
			public AuthorizedInformation()
			{
				AuthorizedTypeCode = "AuthorizedTypeCode";
				AdditionalDocument = new AdditionalDocument();
			}

			public ZString AuthorizedTypeCode { get; set; }

			public AdditionalDocument AdditionalDocument { get; set; }

			IAdditionalDocument IAuthorizedInformation.AdditionalDocument => AdditionalDocument;
		}

		class Declarer : IPartyDetails
		{
			public Declarer()
			{
				ChineseName = "ChineseName";
				ID = "ID";
				Name = "Name";
				TypeCode = "TypeCode";
				Address = new Address();
				Communications = new List<Communication>() { new Communication(), new Communication(), new Communication() };
			}

			public ZString ChineseName { get; set; }

			public ZString ID { get; set; }

			public ZString Name { get; set; }

			public ZString TypeCode { get; set; }

			IAddress IPartyDetails.Address => Address;

			public Address Address { get; set; }

			IEnumerable<ICommunication> IPartyDetails.Communications => Communications;

			public List<Communication> Communications { get; set; }

			public ZString CustomsControlID => null;

			public ZString PaymentOnAccountBusinessID => null;

			public ZString RoleCode => null;

			public ZString SubBoxID => null;

			public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

			public ZString ContactName => null;

			public ZString MainManufacturer => null;

			public ZString UndertakeCode => null;

			public ZString OwnerName => null;

			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;
		}

		class Label : ILabel
		{
			public Label()
			{
				StatusNameCode = "Status";
				LabelDetails = new List<ILabelDetail>() { new LabelDetail(), new LabelDetail() };
			}

			public ZString StatusNameCode { get; set; }

			public IEnumerable<ILabelDetail> LabelDetails { get; set; }
		}

		class LabelDetail : ILabelDetail
		{
			public LabelDetail()
			{
				EndNumber = "EndNumber";
				StartNumber = "StartNumber";
				Track = "Track";
				Year = "Year";
			}

			public ZString EndNumber { get; set; }

			public ZString StartNumber { get; set; }

			public ZString Track { get; set; }

			public ZString Year { get; set; }
		}

		class LocalManufacturer : IPartyDetails
		{
			public LocalManufacturer()
			{
				ChineseName = "ChineseName";
				Address = new Address();
			}

			public ZString ChineseName { get; set; }

			public IAddress Address { get; set; }

			public ZString ID => null;

			public ZString Name => null;

			public ZString TypeCode => null;

			public ZString CustomsControlID => null;

			public ZString PaymentOnAccountBusinessID => null;

			public ZString RoleCode => null;

			public ZString SubBoxID => null;

			public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

			public IEnumerable<ICommunication> Communications { get; set; }

			public ZString ContactName => null;

			public ZString MainManufacturer => null;

			public ZString UndertakeCode => null;

			public ZString OwnerName => null;

			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;
		}

		class ApplicationWine : IApplicationWine
		{
			public ApplicationWine()
			{
				AdditionalDocuments = new List<AdditionalDocument>() { new AdditionalDocument(), new AdditionalDocument() };
				GovernmentProcedurePreviousCode = "GovernmentProcedurePreviousCode";
				PreviousDocument = new PreviousDocument();
			}

			public List<AdditionalDocument> AdditionalDocuments { get; set; }

			IEnumerable<IAdditionalDocument> IApplicationWine.AdditionalDocuments => AdditionalDocuments;

			public ZString GovernmentProcedurePreviousCode { get; set; }

			public PreviousDocument PreviousDocument { get; set; }

			IPreviousDocument IApplicationWine.PreviousDocument => PreviousDocument;
		}

		class Location : ILocation
		{
			public Location(ZString id)
			{
				ID = id;
			}

			public ZString ID { get; set; }

			public ZString Name => null;

			public ZDate LoadingDateTime => ZDate.Empty;

			public ZString EstimatedLoadingCode => null;
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclaration()
		{
			var message = new Message();
			var declaration = new NX5105MessageBuilder().PopulateDeclaration(message, MessageFunctionCode.Replace);
			NUnit.Framework.Assert.That(declaration.FunctionCode.Value, NUnit.Framework.Is.EqualTo("5"), "FunctionCode is 5");

			var xml = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			AssertXMLContains("<FunctionCode>5</FunctionCode>", xml);
			AssertXMLContains(GetExpectedMessageXML("NX5105_Replace.xml"), xml);
			CombineAssertions(() =>
			{
				AsserContainsXmlValueByType(xml, message, typeof(Message), IgnoreAssertPropertyNames);
				AsserContainsXmlByType(xml, declaration.GetType());
			});

			declaration = new NX5105MessageBuilder().PopulateDeclaration(message, MessageFunctionCode.Add);
			NUnit.Framework.Assert.That(declaration.FunctionCode.Value, NUnit.Framework.Is.EqualTo("9"), "FunctionCode is 9");

			xml = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			AssertXMLContains("<FunctionCode>9</FunctionCode>", xml);
			AssertXMLContains(GetExpectedMessageXML("NX5105_Add.xml"), xml);
			CombineAssertions(() =>
			{
				AsserContainsXmlValueByType(xml, message, typeof(Message), IgnoreAssertPropertyNames);
				AsserContainsXmlByType(xml, declaration.GetType());
			});
		}

		[ExpectNoExceptions]
		public override void TestSerializeToMessageString()
		{
			var expectedString = TWMessageBuilder.SerializeToMessageString(Declaration, "9");
			AssertXMLContains("<FunctionCode>9</FunctionCode>", expectedString);
			AsserContainsXmlValueByType(expectedString, Declaration, typeof(Message), IgnoreAssertPropertyNames);
		}

		IEnumerable<ZString> IgnoreAssertPropertyNames
		{
			get
			{
				yield return "Enterprise.Customs.TW.Messaging.MessageBuilders.Testing.NX5105MessageBuilderTest+Message.AcceptanceDateTime";
				yield return "Enterprise.Customs.TW.Messaging.ITransportMeans.ArrivalDateTime";
				yield return "Enterprise.Customs.TW.Messaging.IGoodsShipment.ExitDateTime";
			}
		}

		protected override ITWMessageBuilder CreateNewMessageBuilder()
		{
			return new NX5105MessageBuilder();
		}

		protected override INX5105Declaration CreateDataSource()
		{
			return new Message();
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclarationIfNodeValueIsEmpty()
		{
			Message.TestForEmptyValue = true;
			var message = new Message();
			NUnit.Framework.Assert.That(message.Authentication, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.AssociatedGovernmentProcedureCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.CombinedNote, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.DutyTaxFee.DutyExemptionWaiverNote, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.DutyTaxFee.DutyMemoPrinted, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.ManifestSerialNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.BorderTransportMeans.Registration, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.ConsignmentItem, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IConsignmentItem)));
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.ConsignmentItem.Split, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.BondedGoods.AddDutyReasonCode, NUnit.Framework.Is.EqualTo(ZString.Empty));

			var goodsItem = message.GoodsShipment.GovernmentAgencyGoodsItems.FirstOrDefault();
			NUnit.Framework.Assert.That(goodsItem.Commodity.CommercialCategorizationID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.GoodsGroupNameCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.Name, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.BarCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.ChineseDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.CITESImportPermitID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.FTATariffCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.SHTCImportPermitID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.TariffCodeExtensionCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.DutyTaxFee.DutyRegimeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(goodsItem.Commodity.DutyTaxFee.SpecificTaxBaseQuantity, NUnit.Framework.Is.EqualTo(ZDecimal.Zero));

			NUnit.Framework.Assert.That(message.GoodsShipment.Seller.ID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.GoodsShipment.Seller.ChineseName, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.GoodsShipment.Seller.CustomsControlID, NUnit.Framework.Is.EqualTo(ZString.Empty));

			NUnit.Framework.Assert.That(message.GovernmentProcedureDescriptions.Any(), NUnit.Framework.Is.True);

			NUnit.Framework.Assert.That(message.Packaging.PackagingMaterialDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.Packaging.Combination, NUnit.Framework.Is.EqualTo(ZString.Empty));

			NUnit.Framework.Assert.That(goodsItem.Commodity.Wine.BottledDate, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
			NUnit.Framework.Assert.That(goodsItem.ShippingIdentifications.FirstOrDefault().ProductManufacturedDate, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
			NUnit.Framework.Assert.That(message.Applications.FirstOrDefault().Appointment.ReservationDate, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));

			NUnit.Framework.Assert.That(message.Importer.CustomsControlID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignee.TypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignor.TypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));

			var builder = new NX5105MessageBuilder();
			var xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);

			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			var namespacePrefix = "a";
			var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Authentication", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:tw_AssociatedGovernmentProcedureCode", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:tw_CombinedNote", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:DutyTaxFee/a:tw_DutyExemptionWaiverNote", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:DutyTaxFee/a:tw_DutyMemoPrinted", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Agent", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Agent/a:LPCOAuthorizedParty", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_ManifestSerialNumber", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:BorderTransportMeans/a:tw_Registration", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:ConsignmentItem", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:ConsignmentItem/a:tw_Split", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_AddDutyReasonCode", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:CommercialCategorizationID", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:GoodsGroupNameCode", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:Name", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_BarCode", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_ChineseDescription", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_CITESImportPermitID", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_FTATariffCode", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_SHTCImportPermitID", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_TariffCodeExtensionCode", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:DutyTaxFee/a:DutyRegimeCode", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:DutyTaxFee/a:SpecificTaxBaseQuantity", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:Constituent/a:ElementDescription", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:Constituent/a:tw_LevelID", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:Constituent/a:tw_Thickness", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:ID", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:tw_ChineseName", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:tw_CustomsControlID", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GovernmentProcedure", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GovernmentProcedure/a:Description", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Packaging", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Packaging/a:PackagingMaterialDescription", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Packaging/a:tw_Combination", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_Wine/a:tw_BottledDate", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)), "goodsItem.Commodity.Wine.BottledDate - should be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:tw_ShippingIdentification/a:tw_ProductManufacturedDate", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)), "goodsItem.ShippingIdentifications.ProductManufacturedDate - should be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:tw_Application/a:tw_Appointment/a:tw_ReservationDate", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)), "message.Applications.ReservationDate - should be [null]");

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Importer/a:tw_CustomsControlID", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignee/a:tw_TypeCode", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignor/a:tw_TypeCode", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));

			Message.TestForEmptyValue = false;
			message = new Message();

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.Authentication, NUnit.Framework.Is.EqualTo("Authentication").Using(CustomComparers.TypeComparison), "Authentication");
				NUnit.Framework.Assert.That(message.AssociatedGovernmentProcedureCode, NUnit.Framework.Is.EqualTo("AssociatedGovernmentProcedureCode").Using(CustomComparers.TypeComparison), "AssociatedGovernmentProcedureCode");
				NUnit.Framework.Assert.That(message.CombinedNote, NUnit.Framework.Is.EqualTo("CombinedNote").Using(CustomComparers.TypeComparison), "CombinedNote");
				NUnit.Framework.Assert.That(message.DutyTaxFee.DutyExemptionWaiverNote, NUnit.Framework.Is.EqualTo("DutyExemptionWaiverNote").Using(CustomComparers.TypeComparison), "DutyTaxFee.DutyExemptionWaiverNote");
				NUnit.Framework.Assert.That(message.DutyTaxFee.DutyMemoPrinted, NUnit.Framework.Is.EqualTo("DutyMemoPrinted").Using(CustomComparers.TypeComparison), "DutyTaxFee.DutyMemoPrinted");
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.ManifestSerialNumber, NUnit.Framework.Is.EqualTo("ManifestSerialNumber").Using(CustomComparers.TypeComparison), "GoodsShipment.Consignment.ManifestSerialNumbe");
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.BorderTransportMeans.Registration, NUnit.Framework.Is.EqualTo("R").Using(CustomComparers.TypeComparison), "GoodsShipment.Consignment.BorderTransportMeans.Registration");
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.ConsignmentItem, NUnit.Framework.Is.Not.EqualTo(default(Enterprise.Customs.TW.Messaging.IConsignmentItem)));
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.ConsignmentItem.Split, NUnit.Framework.Is.EqualTo("Split").Using(CustomComparers.TypeComparison), "GoodsShipment.Consignment.ConsignmentItem.Split");
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.BondedGoods.AddDutyReasonCode, NUnit.Framework.Is.EqualTo("AddDutyReasonCode").Using(CustomComparers.TypeComparison), "GoodsShipment.Consignment.BondedGoods.AddDutyReasonCode");

				goodsItem = message.GoodsShipment.GovernmentAgencyGoodsItems.FirstOrDefault();
				NUnit.Framework.Assert.That(goodsItem.Commodity.CommercialCategorizationID, NUnit.Framework.Is.EqualTo("CommercialCategorizationID").Using(CustomComparers.TypeComparison), "goodsItem.Commodity.CommercialCategorizationID");
				NUnit.Framework.Assert.That(goodsItem.Commodity.GoodsGroupNameCode, NUnit.Framework.Is.EqualTo("GoodsGroupNameCode").Using(CustomComparers.TypeComparison), "goodsItem.Commodity.GoodsGroupNameCode");
				NUnit.Framework.Assert.That(goodsItem.Commodity.Name, NUnit.Framework.Is.EqualTo("Name").Using(CustomComparers.TypeComparison), "goodsItem.Commodity.Name");
				NUnit.Framework.Assert.That(goodsItem.Commodity.BarCode, NUnit.Framework.Is.EqualTo("BarCode").Using(CustomComparers.TypeComparison), "goodsItem.Commodity.BarCode");
				NUnit.Framework.Assert.That(goodsItem.Commodity.ChineseDescription, NUnit.Framework.Is.EqualTo("ChineseDescription").Using(CustomComparers.TypeComparison), "goodsItem.Commodity.ChineseDescription");
				NUnit.Framework.Assert.That(goodsItem.Commodity.CITESImportPermitID, NUnit.Framework.Is.EqualTo("CITESImportPermitID").Using(CustomComparers.TypeComparison), "goodsItem.Commodity.CITESImportPermitID");
				NUnit.Framework.Assert.That(goodsItem.Commodity.FTATariffCode, NUnit.Framework.Is.EqualTo("FTATariffCode").Using(CustomComparers.TypeComparison), "goodsItem.Commodity.FTATariffCode");
				NUnit.Framework.Assert.That(goodsItem.Commodity.SHTCImportPermitID, NUnit.Framework.Is.EqualTo("SHTCImportPermitID").Using(CustomComparers.TypeComparison), "goodsItem.Commodity.SHTCImportPermitID");
				NUnit.Framework.Assert.That(goodsItem.Commodity.TariffCodeExtensionCode, NUnit.Framework.Is.EqualTo("TariffCodeExtensionCode").Using(CustomComparers.TypeComparison), "goodsItem.Commodity.TariffCodeExtensionCode");
				NUnit.Framework.Assert.That(goodsItem.Commodity.DutyTaxFee.DutyRegimeCode, NUnit.Framework.Is.EqualTo("DutyRegimeCode").Using(CustomComparers.TypeComparison), "goodsItem.Commodity.DutyTaxFee.DutyRegimeCode");
				NUnit.Framework.Assert.That(goodsItem.Commodity.DutyTaxFee.SpecificTaxBaseQuantity, NUnit.Framework.Is.EqualTo(24.3333333m).Using(CustomComparers.TypeComparison), "goodsItem.Commodity.DutyTaxFee.SpecificTaxBaseQuantity");

				NUnit.Framework.Assert.That(message.GoodsShipment.Seller.ID, NUnit.Framework.Is.EqualTo("ID").Using(CustomComparers.TypeComparison), "message.GoodsShipment.Seller.ID");
				NUnit.Framework.Assert.That(message.GoodsShipment.Seller.ChineseName, NUnit.Framework.Is.EqualTo("ChineseName").Using(CustomComparers.TypeComparison), " message.GoodsShipment.Seller.ChineseName");
				NUnit.Framework.Assert.That(message.GoodsShipment.Seller.CustomsControlID, NUnit.Framework.Is.EqualTo("96944490").Using(CustomComparers.TypeComparison), "message.GoodsShipment.Seller.CustomsControlID");
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignee.TypeCode, NUnit.Framework.Is.EqualTo("TypeCode").Using(CustomComparers.TypeComparison), "message.GoodsShipment.Consignee.TypeCode");
				NUnit.Framework.Assert.That(message.GoodsShipment.Consignor.TypeCode, NUnit.Framework.Is.EqualTo("TypeCode").Using(CustomComparers.TypeComparison), "message.GoodsShipment.Seller.TypeCode");

				NUnit.Framework.Assert.That(message.GovernmentProcedureDescriptions.Any(), NUnit.Framework.Is.True);

				NUnit.Framework.Assert.That(message.Packaging.PackagingMaterialDescription, NUnit.Framework.Is.EqualTo("PackagingMaterialDescription").Using(CustomComparers.TypeComparison), "message.Packaging.PackagingMaterialDescription");
				NUnit.Framework.Assert.That(message.Packaging.Combination, NUnit.Framework.Is.EqualTo("Combination").Using(CustomComparers.TypeComparison), "message.Packaging.Combination");

				NUnit.Framework.Assert.That(goodsItem.Commodity.Wine.BottledDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2011, 05, 24)), "goodsItem.Commodity.Wine.BottledDate");
				NUnit.Framework.Assert.That(goodsItem.ShippingIdentifications.FirstOrDefault().ProductManufacturedDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2011, 05, 25)), "goodsItem.ShippingIdentifications.ProductManufacturedDate");
				NUnit.Framework.Assert.That(message.Applications.FirstOrDefault().Appointment.ReservationDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2011, 05, 26)), "message.Applications.ReservationDate");
			});

			xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);
			xmlDocument.LoadXml(xml);
			nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Authentication", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.Authentication - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:tw_AssociatedGovernmentProcedureCode", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.TwAssociatedGovernmentProcedureCode - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:tw_CombinedNote", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.TwCombinedNote - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:DutyTaxFee/a:tw_DutyExemptionWaiverNote", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.DutyTaxFee.TwDutyExemptionWaiverNote - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:DutyTaxFee/a:tw_DutyMemoPrinted", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.DutyTaxFee.TwDutyMemoPrinted - should not be [null]");

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.Consignment - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_ManifestSerialNumber", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.Consignment.TwManifestSerialNumber - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:BorderTransportMeans/a:tw_Registration", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.Consignment.BorderTransportMeans.TwRegistration - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:ConsignmentItem", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.Consignment.ConsignmentItem - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:ConsignmentItem/a:tw_Split", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.Consignment.ConsignmentItem.TwSplit - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_AddDutyReasonCode", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.Consignment.TwBondedGoods.TwAddDutyReasonCode - should not be [null]");

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:CommercialCategorizationID", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.CommercialCategorizationID - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:GoodsGroupNameCode", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.GoodsGroupNameCode - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:Name", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.Name - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_BarCode", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.TwBarCode - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_ChineseDescription", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.TwChineseDescription - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_CITESImportPermitID", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.TwCITESImportPermitID - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_FTATariffCode", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.TwFTATariffCode - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_SHTCImportPermitID", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.TwSHTCImportPermitID - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_TariffCodeExtensionCode", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.TwTariffCodeExtensionCode - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:DutyTaxFee/a:DutyRegimeCode", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.DutyRegimeCode - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:DutyTaxFee/a:SpecificTaxBaseQuantity", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.DutyTaxFee.SpecificTaxBaseQuantity - should not be [null]");

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:AdditionalDocument/a:tw_SequenceNumeric", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)), "$The Declaration.GoodsShipment.GovernmentAgencyGoodsItem.AdditionalDocument.TwSequenceNumeric node - should be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Origin/a:AdditionalDocument/a:tw_SequenceNumeric", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("0"), $"The Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Origin.AdditionalDocument.TwSequenceNumeric node");

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.Seller - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:ID", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.Seller.ID - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:tw_ChineseName", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.Seller.TwChineseName - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:tw_CustomsControlID", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GoodsShipment.Seller.TwCustomsControlID - should not be [null]");

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GovernmentProcedure", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GovernmentProcedure - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GovernmentProcedure/a:Description", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.GovernmentProcedure.Description - should not be [null]");

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Packaging", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.Packaging - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Packaging/a:PackagingMaterialDescription", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.Packaging.PackagingMaterialDescription - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Packaging/a:tw_Combination", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "Declaration.Packaging.TwCombination - should not be [null]");

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_Wine/a:tw_BottledDate", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "goodsItem.Commodity.Wine.BottledDate - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:tw_ShippingIdentification/a:tw_ProductManufacturedDate", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "goodsItem.ShippingIdentifications.ProductManufacturedDate - should not be [null]");
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:tw_Application/a:tw_Appointment/a:tw_ReservationDate", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "message.Applications.ReservationDate - should not be [null]");
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclarationIfNoteOverLength()
		{
			var oldTestForOverLength = Message.TestForOverLength;
			var oldTestForEmptyValue = Message.TestForEmptyValue;
			Message.TestForEmptyValue = false;
			Message.TestForOverLength = true;
			var message = new Message();
			var builder = new NX5105MessageBuilder();
			var xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			var namespacePrefix = "a";
			var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Agent/a:LPCOAuthorizedParty/a:ID", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("REALLYLONGLPCOAUTHOR"), "ID should have been trimmed to 20");

			Message.TestForEmptyValue = oldTestForEmptyValue;
			Message.TestForOverLength = oldTestForOverLength;
		}

		[ExpectNoExceptions]
		public void TestPopulateSellerLPCOAuthorizedParty()
		{
			var oldTestForOverLength = Message.TestForOverLength;
			var oldTestForEmptyValue = Message.TestForEmptyValue;
			Message.TestForEmptyValue = true;
			Message.TestForOverLength = false;
			var message = new Message();
			var builder = new NX5105MessageBuilder();
			var xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			var namespacePrefix = "a";
			var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:LPCOAuthorizedParty", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)), "Seller LPCOAuthorizedParty should be null - should be [null]");

			Message.TestForEmptyValue = false;
			xml = builder.SerializeToMessageString(new Message(), MessageFunctionCode.Add);
			xmlDocument.LoadXml(xml);
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:LPCOAuthorizedParty/a:ID", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("ID"), "Seller LPCOAuthorizedParty should be not null");
			Message.TestForEmptyValue = oldTestForEmptyValue;
			Message.TestForOverLength = oldTestForOverLength;
		}

		[ExpectNoExceptions]
		public void TestDateTimeFormat()
		{
			var message = new Message();
			var declaration = new NX5105MessageBuilder().PopulateDeclaration(message, MessageFunctionCode.Add);
			var xml = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			var namespacePrefix = "a";
			var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"].Value);

			foreach (var nodeName in DateTimeNodeNames)
			{
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode(nodeName, nameSpace).InnerText, NUnit.Framework.Is.EqualTo("2011-05-23"), $"The {nodeName} node value shoule be 'yyyy-MM-dd' after formatting");
			}
		}

		IEnumerable<ZString> DateTimeNodeNames
		{
			get
			{
				yield return "a:Declaration/a:AcceptanceDateTime";
				yield return "a:Declaration/a:BorderTransportMeans/a:ArrivalDateTime";
				yield return "a:Declaration/a:GoodsShipment/a:ExitDateTime";
			}
		}

		[ExpectNoExceptions]
		public void TestDeclarationPackagingMarksNumbersMaxLength()
		{
			var message = new Message();
			var packaging = message.Packaging as DeclarationPackaging;
			packaging.MarksNumbers = new ZString('A', 600);
			var declaration = new NX5105MessageBuilder().PopulateDeclaration(message, MessageFunctionCode.Add);
			NUnit.Framework.Assert.That(declaration.Packaging.MarksNumbers.Value, NUnit.Framework.Is.EqualTo(new string('A', 512)));
		}

		Tuple<XmlDocument, XmlNamespaceManager> GetXmlDocumentInformation(Message message)
		{
			var messageString = new NX5105MessageBuilder().SerializeToMessageString(message, MessageFunctionCode.Replace);
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(messageString);

			var namespacePrefix = "a";
			var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"].Value);
			return Tuple.Create(xmlDocument, nameSpace);
		}

		[ExpectNoExceptions]
		public void TestPopulateSellerLPCOAuthorizedPartyWithEffectiveCountries()
		{
			var message = new Message();
			var seller = message.GoodsShipment.Seller as PartyDetails;
			seller.ID = "125544";
			var address = seller.Address as Address;
			address.CountryCode = "US";
			var xmlDocumentInformation = GetXmlDocumentInformation(message);
			var xmlDocument = xmlDocumentInformation.Item1;
			var nameSpace = xmlDocumentInformation.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:LPCOAuthorizedParty", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)), "The LPCOAuthorizedParty node is empty - should be [null]");

			address.CountryCode = "TW";
			xmlDocumentInformation = GetXmlDocumentInformation(message);
			xmlDocument = xmlDocumentInformation.Item1;
			nameSpace = xmlDocumentInformation.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:LPCOAuthorizedParty", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "The LPCOAuthorizedParty node isn't empty - should not be [null]");
		}

		[ExpectNoExceptions]
		public void TestNodeForDeclaration_GoodsShipment_AdditionalDocument()
		{
			var message = new Message();
			var builder = new NX5105MessageBuilder();
			var xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);

			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			var namespacePrefix = "a";
			var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);

			CombineAssertions("Node of Declaration_GoodsShipment_AdditionalDocument values not null", () =>
			{
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:ID", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_Content", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_ImageFileFormat", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_ImageFileName", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_SizeMeasure", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:TypeCode", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));

				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:ID", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("ID"));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_Content", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("Content"));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_ImageFileFormat", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("ImageFileFormat"));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_ImageFileName", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("ImageFileName"));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_SizeMeasure", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("3"));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:TypeCode", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("TypeCode"));
			});

			Message.TestForEmptyValue = true;
			xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);
			xmlDocument.LoadXml(xml);
			nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);

			CombineAssertions("Node of Declaration_GoodsShipment_AdditionalDocument values null", () =>
			{
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:ID", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_Content", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_ImageFileFormat", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_ImageFileName", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_SizeMeasure", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:TypeCode", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));

				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_ImageFileFormat", nameSpace).InnerText, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_SizeMeasure", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("0"));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:TypeCode", nameSpace).InnerText, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));
			});

			Message.TestForEmptyValue = false;
		}

		[ExpectNoExceptions]
		public void TestPopulateConstituent()
		{
			var node = "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:Constituent";
			Message.TestForEmptyValue = true;
			var message = new Message();
			var constituent = message.GoodsShipment.GovernmentAgencyGoodsItems.First().Commodity.Constituent as Constituent;
			NUnit.Framework.Assert.That(constituent.ElementDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(constituent.LevelID, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(constituent.Thickness, NUnit.Framework.Is.EqualTo(ZString.Empty));
			AssertNodeNull(message, node);

			Message.TestForEmptyValue = false;
			message = new Message();
			constituent = message.GoodsShipment.GovernmentAgencyGoodsItems.First().Commodity.Constituent as Constituent;
			NUnit.Framework.Assert.That(constituent.ElementDescription, NUnit.Framework.Is.EqualTo("ElementDescription").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(constituent.LevelID, NUnit.Framework.Is.EqualTo("LevelID").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(constituent.Thickness, NUnit.Framework.Is.EqualTo("Thickness").Using(CustomComparers.TypeComparison));
			AssertChildNodesNotNull(message, node, new string[] { "ElementDescription", "tw_LevelID", "tw_Thickness" }, new string[] { "ElementDescription", "LevelID", "Thickness" });

			var builder = new NX5105MessageBuilder();
			var constituent2 = new Constituent();
			constituent2.ElementDescription = ZString.Empty;
			constituent2.LevelID = ZString.Empty;
			var commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			builder.PopulateConstituent(commodity, constituent2);
			var constituent3 = commodity.Constituent;
			NUnit.Framework.Assert.That(constituent3.ElementDescription, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentElementDescription)));
			NUnit.Framework.Assert.That(constituent3.TwLevelId, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentTwLevelId)));
			NUnit.Framework.Assert.That(constituent3.TwThickness.Value, NUnit.Framework.Is.EqualTo("Thickness"));

			constituent2.ElementDescription = "ElementDescription";
			constituent2.Thickness = ZString.Empty;
			commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			builder.PopulateConstituent(commodity, constituent2);
			constituent3 = commodity.Constituent;
			NUnit.Framework.Assert.That(constituent3.ElementDescription.Value, NUnit.Framework.Is.EqualTo("ElementDescription"));
			NUnit.Framework.Assert.That(constituent3.TwLevelId, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentTwLevelId)));
			NUnit.Framework.Assert.That(constituent3.TwThickness, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityConstituentTwThickness)));
		}

		[ExpectNoExceptions]
		public void TestPopulateCommodityRelatedPackaging()
		{
			var node = "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:CommodityRelatedPackaging";
			Message.TestForEmptyValue = true;
			var message = new Message();
			var commodityRelatedPackaging = message.GoodsShipment.GovernmentAgencyGoodsItems.First().Commodity.CommodityRelatedPackaging;
			NUnit.Framework.Assert.That(commodityRelatedPackaging.PackingMethodDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(commodityRelatedPackaging.MaterialCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(commodityRelatedPackaging.Specification, NUnit.Framework.Is.EqualTo(ZString.Empty));
			AssertNodeNull(message, node);

			Message.TestForEmptyValue = false;
			message = new Message();
			commodityRelatedPackaging = message.GoodsShipment.GovernmentAgencyGoodsItems.First().Commodity.CommodityRelatedPackaging;
			NUnit.Framework.Assert.That(commodityRelatedPackaging.PackingMethodDescription, NUnit.Framework.Is.EqualTo("PackingMethodDescription").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodityRelatedPackaging.MaterialCode, NUnit.Framework.Is.EqualTo("MaterialCode").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodityRelatedPackaging.Specification, NUnit.Framework.Is.EqualTo("Specification").Using(CustomComparers.TypeComparison));
			AssertChildNodesNotNull(message, node, new string[] { "PackingMethodDescription", "tw_MaterialCode", "tw_Specification" }, new string[] { "PackingMethodDescription", "MaterialCode", "Specification" });

			var builder = new NX5105MessageBuilder();
			var commodityRelatedPackaging2 = new CommodityRelatedPackaging();
			commodityRelatedPackaging2.PackingMethodDescription = ZString.Empty;
			commodityRelatedPackaging2.MaterialCode = ZString.Empty;
			var commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			builder.PopulateCommodityRelatedPackaging(commodity, commodityRelatedPackaging2);
			var commodityRelatedPackaging3 = commodity.CommodityRelatedPackaging;
			NUnit.Framework.Assert.That(commodityRelatedPackaging3.PackingMethodDescription, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackagingPackingMethodDescription)));
			NUnit.Framework.Assert.That(commodityRelatedPackaging3.TwMaterialCode, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackagingTwMaterialCode)));
			NUnit.Framework.Assert.That(commodityRelatedPackaging3.TwSpecification.Value, NUnit.Framework.Is.EqualTo("Specification"));

			commodityRelatedPackaging2.PackingMethodDescription = "PackingMethodDescription";
			commodityRelatedPackaging2.Specification = ZString.Empty;
			commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			builder.PopulateCommodityRelatedPackaging(commodity, commodityRelatedPackaging2);
			commodityRelatedPackaging3 = commodity.CommodityRelatedPackaging;
			NUnit.Framework.Assert.That(commodityRelatedPackaging3.PackingMethodDescription.Value, NUnit.Framework.Is.EqualTo("PackingMethodDescription"));
			NUnit.Framework.Assert.That(commodityRelatedPackaging3.TwMaterialCode, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackagingTwMaterialCode)));
			NUnit.Framework.Assert.That(commodityRelatedPackaging3.TwSpecification, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityCommodityRelatedPackagingTwSpecification)));
		}

		[ExpectNoExceptions]
		public void TestPopulateHandling()
		{
			var builder = new NX5105MessageBuilder();
			var commdity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			var handlings = new List<ZString> { "1", "2", "", "4", "5", "6", "7", "8", "9", "0" };
			builder.PopulateHandling(commdity, handlings);
			NUnit.Framework.Assert.That(commdity.Handling.Count, NUnit.Framework.Is.EqualTo(9));
			NUnit.Framework.Assert.That(commdity.Handling.FirstOrDefault(x => x.InstructionsCode.Value == "0"), NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityHandling)));
			for (int i = 0; i < 9; i++)
			{
				NUnit.Framework.Assert.That(commdity.Handling[i].InstructionsCode.Value, NUnit.Framework.Is.EqualTo(handlings[i]).Using(CustomComparers.TypeComparison));
			}

			commdity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			handlings = new List<ZString>();
			builder.PopulateHandling(commdity, handlings);
			NUnit.Framework.Assert.That(!commdity.Handling.Any(), NUnit.Framework.Is.True);

			commdity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			builder.PopulateHandling(commdity, null);
			NUnit.Framework.Assert.That(!commdity.Handling.Any(), NUnit.Framework.Is.True);
		}

		[ExpectNoExceptions]
		public void TestPopulateGoodsLicensingStatisticalMeasure()
		{
			var builder = new NX5105MessageBuilder();
			Message.TestForEmptyValue = true;
			var goodsLicensingStatisticalMeasure = new GoodsLicensingStatisticalMeasure();
			var goodsItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();

			builder.PopulateGoodsLicensingStatisticalMeasure(goodsItem, goodsLicensingStatisticalMeasure);
			NUnit.Framework.Assert.That(goodsItem.TwGoodsLicensingStatisticalMeasure, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsLicensingStatisticalMeasure)));

			builder.PopulateGoodsLicensingStatisticalMeasure(goodsItem, null);
			NUnit.Framework.Assert.That(goodsItem.TwGoodsLicensingStatisticalMeasure, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwGoodsLicensingStatisticalMeasure)));

			Message.TestForEmptyValue = false;
			goodsLicensingStatisticalMeasure = new GoodsLicensingStatisticalMeasure();
			builder.PopulateGoodsLicensingStatisticalMeasure(goodsItem, goodsLicensingStatisticalMeasure);
			var tw_GoodsLicensingStatisticalMeasure = goodsItem.TwGoodsLicensingStatisticalMeasure;
			NUnit.Framework.Assert.That(tw_GoodsLicensingStatisticalMeasure.TwLicensingQuantity.Value, NUnit.Framework.Is.EqualTo(39.3333m));
			NUnit.Framework.Assert.That(tw_GoodsLicensingStatisticalMeasure.TwStatisticalUnitCode.Value, NUnit.Framework.Is.EqualTo("StatisticalUnitCode"));
			NUnit.Framework.Assert.That(tw_GoodsLicensingStatisticalMeasure.ResponsibleGovernmentAgency.Id.Value, NUnit.Framework.Is.EqualTo("ResponsibleGovernmentAgency"));

			goodsLicensingStatisticalMeasure.LicensingQuantity = ZDecimal.Zero;
			goodsLicensingStatisticalMeasure.StatisticalUnitCode = ZString.Empty;
			builder.PopulateGoodsLicensingStatisticalMeasure(goodsItem, goodsLicensingStatisticalMeasure);
			tw_GoodsLicensingStatisticalMeasure = goodsItem.TwGoodsLicensingStatisticalMeasure;
			NUnit.Framework.Assert.That(tw_GoodsLicensingStatisticalMeasure.TwLicensingQuantity.Value, NUnit.Framework.Is.EqualTo(decimal.Zero));
			NUnit.Framework.Assert.That(tw_GoodsLicensingStatisticalMeasure.TwStatisticalUnitCode.Value, NUnit.Framework.Is.EqualTo(string.Empty));
			NUnit.Framework.Assert.That(tw_GoodsLicensingStatisticalMeasure.ResponsibleGovernmentAgency.Id.Value, NUnit.Framework.Is.EqualTo("ResponsibleGovernmentAgency"));
			goodsLicensingStatisticalMeasure.LicensingQuantity = 1m;
			goodsLicensingStatisticalMeasure.ResponsibleGovernmentAgency = ZString.Empty;
			builder.PopulateGoodsLicensingStatisticalMeasure(goodsItem, goodsLicensingStatisticalMeasure);
			tw_GoodsLicensingStatisticalMeasure = goodsItem.TwGoodsLicensingStatisticalMeasure;
			NUnit.Framework.Assert.That(tw_GoodsLicensingStatisticalMeasure.TwLicensingQuantity.Value, NUnit.Framework.Is.EqualTo(1m));
			NUnit.Framework.Assert.That(tw_GoodsLicensingStatisticalMeasure.TwStatisticalUnitCode.Value, NUnit.Framework.Is.EqualTo(string.Empty));
			NUnit.Framework.Assert.That(tw_GoodsLicensingStatisticalMeasure.ResponsibleGovernmentAgency.Id.Value, NUnit.Framework.Is.EqualTo(string.Empty));
		}

		[ExpectNoExceptions]
		public void TestPopulatePreviousDocument()
		{
			var builder = new NX5105MessageBuilder();
			var bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			var obj = new PreviousDocument(ZString.Empty, null, 0);
			builder.PopulatePreviousDocument(bo, obj);
			NUnit.Framework.Assert.That(bo.PreviousDocument, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityPreviousDocument)));

			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			obj = new PreviousDocument(new ZString('1', 15), null, 0);
			builder.PopulatePreviousDocument(bo, obj);
			NUnit.Framework.Assert.That(bo.PreviousDocument, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityPreviousDocument)));
			NUnit.Framework.Assert.That(bo.PreviousDocument.Id.Value, NUnit.Framework.Is.EqualTo(new ZString('1', 14)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateApprovalDocument()
		{
			var builder = new NX5105MessageBuilder();
			var bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
			var obj = new ApprovalDocument(ZString.Empty, ZString.Empty, null);
			builder.PopulateApprovalDocument(bo, obj);
			NUnit.Framework.Assert.That(bo.TwApprovalDocument, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocument)));

			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
			obj = new ApprovalDocument(new ZString('1', 2), ZString.Empty, null);
			builder.PopulateApprovalDocument(bo, obj);
			NUnit.Framework.Assert.That(bo.TwApprovalDocument, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocument)));
			NUnit.Framework.Assert.That(bo.TwApprovalDocument.TwLpcoExemptionCode.Value, NUnit.Framework.Is.EqualTo(new ZString('1', 1)).Using(CustomComparers.TypeComparison));

			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
			obj = new ApprovalDocument(ZString.Empty, new ZString('1', 15), null);
			builder.PopulateApprovalDocument(bo, obj);
			NUnit.Framework.Assert.That(bo.TwApprovalDocument, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocument)));
			NUnit.Framework.Assert.That(bo.TwApprovalDocument.TwLpcoid.Value, NUnit.Framework.Is.EqualTo(new ZString('1', 14)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateApprovalDocumentLPCOAuthorizedParty()
		{
			var builder = new NX5105MessageBuilder();
			var bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocument();
			var obj = new LPCOAuthorizedParty(ZString.Empty, ZString.Empty, ZString.Empty);
			builder.PopulateApprovalDocumentLPCOAuthorizedParty(bo, obj);
			NUnit.Framework.Assert.That(bo.LpcoAuthorizedParty, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocumentLpcoAuthorizedParty)));

			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocument();
			obj = new LPCOAuthorizedParty(new ZString('1', 15), ZString.Empty, ZString.Empty);
			builder.PopulateApprovalDocumentLPCOAuthorizedParty(bo, obj);
			NUnit.Framework.Assert.That(bo.LpcoAuthorizedParty, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocumentLpcoAuthorizedParty)));
			NUnit.Framework.Assert.That(bo.LpcoAuthorizedParty.Id.Value, NUnit.Framework.Is.EqualTo(new ZString('1', 14)).Using(CustomComparers.TypeComparison));

			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocument();
			obj = new LPCOAuthorizedParty(ZString.Empty, ZString.Empty, new ZString('1', 4));
			builder.PopulateApprovalDocumentLPCOAuthorizedParty(bo, obj);
			NUnit.Framework.Assert.That(bo.LpcoAuthorizedParty, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwApprovalDocumentLpcoAuthorizedParty)));
			NUnit.Framework.Assert.That(bo.LpcoAuthorizedParty.TwTypeCode.Value, NUnit.Framework.Is.EqualTo(new ZString('1', 3)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateShippingIdentification()
		{
			var builder = new NX5105MessageBuilder();
			var bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
			var obj = new List<IShippingIdentification>();
			for (int i = 0; i < 100; i++)
			{
				obj.Add(new ShippingIdentification("LotNumberID", new ZDateTime(2011, 05, 23), 44, new ZDateTime(2011, 05, 20)));
			}
			builder.PopulateShippingIdentification(bo, obj);
			NUnit.Framework.Assert.That(bo.TwShippingIdentification.Count, NUnit.Framework.Is.EqualTo(99));

			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
			obj = new List<IShippingIdentification>()
			{
				new ShippingIdentification(ZString.Empty, ZDateTime.Invalid, 0, ZDateTime.Invalid)
			};
			builder.PopulateShippingIdentification(bo, obj);
			NUnit.Framework.Assert.That(!bo.TwShippingIdentification.Any(), NUnit.Framework.Is.True);

			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
			obj = new List<IShippingIdentification>();
			builder.PopulateShippingIdentification(bo, obj);
			NUnit.Framework.Assert.That(!bo.TwShippingIdentification.Any(), NUnit.Framework.Is.True);

			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
			obj = null;
			builder.PopulateShippingIdentification(bo, obj);
			NUnit.Framework.Assert.That(!bo.TwShippingIdentification.Any(), NUnit.Framework.Is.True);

			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
			obj = new List<IShippingIdentification>()
			{
				new ShippingIdentification(new ZString('1', 151), ZDateTime.Invalid, 0, ZDateTime.Invalid)
			};
			builder.PopulateShippingIdentification(bo, obj);
			NUnit.Framework.Assert.That(bo.TwShippingIdentification[0].TwLotNumberId.Value, NUnit.Framework.Is.EqualTo(new ZString('1', 150)).Using(CustomComparers.TypeComparison));

			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
			obj = new List<IShippingIdentification>()
			{
				new ShippingIdentification(ZString.Empty, new ZDateTime(2020, 9, 24, 12, 12, 12), 0, ZDateTime.Invalid)
			};
			builder.PopulateShippingIdentification(bo, obj);
			NUnit.Framework.Assert.That(bo.TwShippingIdentification[0].TwProductBestBeforeDateTime, NUnit.Framework.Is.EqualTo("2020-09-24"));

			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
			obj = new List<IShippingIdentification>()
			{
				new ShippingIdentification(ZString.Empty, ZDateTime.Invalid, 1, ZDateTime.Invalid)
			};
			builder.PopulateShippingIdentification(bo, obj);
			NUnit.Framework.Assert.That(bo.TwShippingIdentification[0].TwProductLotNumberAmount.Value, NUnit.Framework.Is.EqualTo(1m));

			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
			obj = new List<IShippingIdentification>()
			{
				new ShippingIdentification(ZString.Empty, ZDateTime.Invalid, 0, new ZDateTime(2020, 9, 24, 12, 12, 12))
			};
			builder.PopulateShippingIdentification(bo, obj);
			NUnit.Framework.Assert.That(bo.TwShippingIdentification[0].TwProductManufacturedDateValueSpecified, NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(bo.TwShippingIdentification[0].TwProductManufacturedDate, NUnit.Framework.Is.EqualTo(new DateTime(2020, 9, 24, 12, 12, 12)));
		}

		#region MaxLength
		[ExpectNoExceptions]
		public void TestPopulateImporterMaxLength()
		{
			var obj = SetupPartyDetails();
			var messageBuiler = new NX5105MessageBuilder();
			var declaration = new Declaration();
			messageBuiler.PopulateImporter(declaration, obj);
			var importer = declaration.Importer;
			NUnit.Framework.Assert.That(importer.Name.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 80)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(importer.TwChineseName.Value, NUnit.Framework.Is.EqualTo(new ZString('C', 70)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(importer.Address.Line.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 120)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(importer.Address.TwChineseLine.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 100)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateConsigneeMaxLength()
		{
			var obj = SetupPartyDetails();
			var messageBuiler = new NX5105MessageBuilder();
			var goodsShipment = new DeclarationGoodsShipment();
			messageBuiler.PopulateConsignee(goodsShipment, obj);
			var consignee = goodsShipment.Consignee;
			NUnit.Framework.Assert.That(consignee.Name.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 80)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(consignee.TwChineseName.Value, NUnit.Framework.Is.EqualTo(new ZString('C', 70)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(consignee.Address.Line.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 120)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(consignee.Address.TwChineseLine.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 100)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateConsignorMaxLength()
		{
			var obj = SetupPartyDetails();
			var messageBuiler = new NX5105MessageBuilder();
			var goodsShipment = new DeclarationGoodsShipment();
			messageBuiler.PopulateConsignor(goodsShipment, obj);
			var consignor = goodsShipment.Consignor;

			NUnit.Framework.Assert.That(consignor.Name.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 80)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(consignor.TwChineseName.Value, NUnit.Framework.Is.EqualTo(new ZString('C', 70)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(consignor.Address.Line.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 120)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(consignor.Address.TwChineseLine.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 100)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateNotifyPartyMaxLength()
		{
			var obj = SetupPartyDetails();
			var messageBuiler = new NX5105MessageBuilder();
			var goodsShipment = new DeclarationGoodsShipment();
			messageBuiler.PopulateNotifyParty(goodsShipment, obj);
			var notifyParty = goodsShipment.NotifyParty;
			NUnit.Framework.Assert.That(notifyParty.Name.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 80)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(notifyParty.TwChineseName.Value, NUnit.Framework.Is.EqualTo(new ZString('C', 70)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(notifyParty.Address.Line.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 120)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(notifyParty.Address.TwChineseLine.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 100)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateSellerMaxLength()
		{
			var obj = SetupPartyDetails();
			var messageBuiler = new NX5105MessageBuilder();
			var goodsShipment = new DeclarationGoodsShipment();
			messageBuiler.PopulateSeller(goodsShipment, obj);
			var seller = goodsShipment.Seller;

			NUnit.Framework.Assert.That(seller.Id.Value, NUnit.Framework.Is.EqualTo(new ZString('Z', 14)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(seller.Name.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 80)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(seller.TwChineseName.Value, NUnit.Framework.Is.EqualTo(new ZString('C', 70)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(seller.TwCustomsControlId.Value, NUnit.Framework.Is.EqualTo(new ZString('Z', 8)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(seller.TwTypeCode.Value, NUnit.Framework.Is.EqualTo(new ZString('Z', 3)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(seller.Address.Line.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 120)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(seller.Address.TwChineseLine.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 100)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(seller.Communication.Id.Value, NUnit.Framework.Is.EqualTo(new ZString('Z', 20)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(seller.Contact.Name.Value, NUnit.Framework.Is.EqualTo(new ZString('Z', 70)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(seller.LpcoAuthorizedParty.Id.Value, NUnit.Framework.Is.EqualTo(new ZString('Z', 20)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateCommodityMaxLength()
		{
			var obj = SetupCommodityDetails();
			var messageBuiler = new NX5105MessageBuilder();
			var governmentAgencyGoodsItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
			messageBuiler.PopulateCommodity(governmentAgencyGoodsItem, obj);
			var commodity = governmentAgencyGoodsItem.Commodity;

			NUnit.Framework.Assert.That(commodity.Name.Value, NUnit.Framework.Is.EqualTo(new ZString('A', 50)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodity.Description.Value, NUnit.Framework.Is.EqualTo(new ZString('B', 512)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodity.TwChineseDescription.Value, NUnit.Framework.Is.EqualTo(new ZString('C', 512)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodity.CommercialCategorizationId.Value, NUnit.Framework.Is.EqualTo(new ZString('D', 80)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodity.Constituent.ElementDescription.Value, NUnit.Framework.Is.EqualTo(new ZString('E', 256)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclarationPackagingMaxLength()
		{
			var obj = new DeclarationPackaging();
			obj.MarksNumbers = new ZString('M', 1000);
			var messageBuiler = new NX5105MessageBuilder();
			var declaration = new Declaration();
			messageBuiler.PopulateDeclarationPackaging(declaration, obj);
			var packaging = declaration.Packaging;
			NUnit.Framework.Assert.That(packaging.MarksNumbers.Value, NUnit.Framework.Is.EqualTo(new ZString('M', 512)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateApplicationAdditionalInformationAddressMaxLength()
		{
			var obj = new ApplicationAdditionalInformation();
			obj.AddressChineseLine = new ZString('A', 200);
			var messageBuiler = new NX5105MessageBuilder();
			var additionalInformation = new DeclarationTwApplicationAdditionalInformation();
			messageBuiler.PopulateApplicationAdditionalInformationAddress(additionalInformation, obj);
			var address = additionalInformation.Address;
			NUnit.Framework.Assert.That(address.TwChineseLine.Value, NUnit.Framework.Is.EqualTo(new ZString('A', 100)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateApplicationAgentMaxLength()
		{
			var obj = SetupPartyDetails();
			var messageBuiler = new NX5105MessageBuilder();
			var application = new DeclarationTwApplication();
			messageBuiler.PopulateApplicationAgent(application, obj);
			var applicationAgent = application.Agent;

			NUnit.Framework.Assert.That(applicationAgent.Name.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 70)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(applicationAgent.Address.TwChineseLine.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 100)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclarerMaxLength()
		{
			var obj = SetupPartyDetails();
			var messageBuiler = new NX5105MessageBuilder();
			var application = new DeclarationTwApplication();
			messageBuiler.PopulateDeclarer(application, obj);
			var applicationDeclarer = application.TwDeclarer;

			NUnit.Framework.Assert.That(applicationDeclarer.TwChineseName.Value, NUnit.Framework.Is.EqualTo(new ZString('C', 70)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(applicationDeclarer.Address.TwChineseLine.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 100)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateWine()
		{
			var messageBuiler = new NX5105MessageBuilder();
			var obj = new Wine(9999, 12.5555, new ZDateTime(2020, 09, 25, 12, 34, 56), 12.34567, new ZString('1', 101), 12.44567, new ZDateTime(2020, 09, 30, 13, 34, 56), new ZDateTime(2020, 10, 01, 14, 34, 56), 12.54567, 7777);
			var bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();

			messageBuiler.PopulateWine(bo, obj);
			NUnit.Framework.Assert.That(bo.TwWine.TwAgeNumeric, NUnit.Framework.Is.EqualTo(9999m));
			NUnit.Framework.Assert.That(bo.TwWine.TwAlcoholContentNumeric, NUnit.Framework.Is.EqualTo(12.556m));
			NUnit.Framework.Assert.That(bo.TwWine.TwBottledDate, NUnit.Framework.Is.EqualTo(new DateTime(2020, 09, 25, 12, 34, 56)));
			NUnit.Framework.Assert.That(bo.TwWine.TwCoverLotNumberAmount.Value, NUnit.Framework.Is.EqualTo(12.3457m));
			NUnit.Framework.Assert.That(bo.TwWine.TwGeographicRegion.Value, NUnit.Framework.Is.EqualTo(new ZString('1', 100)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(bo.TwWine.TwOriginalNonLotNumberAmount.Value, NUnit.Framework.Is.EqualTo(12.4457m));
			NUnit.Framework.Assert.That(bo.TwWine.TwProductBestBeforeDateTime, NUnit.Framework.Is.EqualTo("2020-09-30"));
			NUnit.Framework.Assert.That(bo.TwWine.TwProductExpiryDateTime, NUnit.Framework.Is.EqualTo("2020-10-01"));
			NUnit.Framework.Assert.That(bo.TwWine.TwRemoveLotNumberAmount.Value, NUnit.Framework.Is.EqualTo(12.5457m));
			NUnit.Framework.Assert.That(bo.TwWine.TwYearNumeric, NUnit.Framework.Is.EqualTo(7777m));

			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			messageBuiler.PopulateWine(bo, null);
			NUnit.Framework.Assert.That(bo.TwWine, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwWine)));
		}

		[ExpectNoExceptions]
		public void TestPopulateFood()
		{
			var messageBuiler = new NX5105MessageBuilder();

			var obj = new Food(null, null, null);
			var bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			messageBuiler.PopulateFood(bo, obj);
			NUnit.Framework.Assert.That(bo.TwFood.TwPhValueNumericValueSpecified, NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(bo.TwFood.TwPhValueNumeric, NUnit.Framework.Is.Null);
			NUnit.Framework.Assert.That(bo.TwFood.TwSterilizationValueNumericValueSpecified, NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(bo.TwFood.TwSterilizationValueNumeric, NUnit.Framework.Is.Null);

			obj = new Food(7.5m, null, null);
			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			messageBuiler.PopulateFood(bo, obj);
			NUnit.Framework.Assert.That(bo.TwFood.TwPhValueNumericValueSpecified, NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(bo.TwFood.TwPhValueNumeric, NUnit.Framework.Is.EqualTo(7.5m));
			NUnit.Framework.Assert.That(bo.TwFood.TwSterilizationValueNumericValueSpecified, NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(bo.TwFood.TwSterilizationValueNumeric, NUnit.Framework.Is.Null);

			obj = new Food(null, 50.5m, null);
			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			messageBuiler.PopulateFood(bo, obj);
			NUnit.Framework.Assert.That(bo.TwFood.TwPhValueNumericValueSpecified, NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(bo.TwFood.TwPhValueNumeric, NUnit.Framework.Is.Null);
			NUnit.Framework.Assert.That(bo.TwFood.TwSterilizationValueNumericValueSpecified, NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(bo.TwFood.TwSterilizationValueNumeric, NUnit.Framework.Is.EqualTo(50.5m));
		}

		[ExpectNoExceptions]
		public void TestPopulateFoodConstituent()
		{
			var messageBuiler = new NX5105MessageBuilder();
			var obj = new List<FoodConstituent>()
			{
				new FoodConstituent(new ZString('1', 500), 123m),
				new FoodConstituent("element name 2", 234m),
			};
			var bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwFood();

			messageBuiler.PopulateFoodConstituent(bo, obj);
			NUnit.Framework.Assert.That(bo.Constituent.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(bo.Constituent[0].ElementName.Value, NUnit.Framework.Is.EqualTo(new ZString('1', 500)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(bo.Constituent[0].ElementPercentNumericValueSpecified, NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(bo.Constituent[0].ElementPercentNumeric, NUnit.Framework.Is.EqualTo(123m));
			NUnit.Framework.Assert.That(bo.Constituent[1].ElementName.Value, NUnit.Framework.Is.EqualTo("element name 2"));
			NUnit.Framework.Assert.That(bo.Constituent[1].ElementPercentNumericValueSpecified, NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(bo.Constituent[1].ElementPercentNumeric, NUnit.Framework.Is.EqualTo(234m));

			obj = new List<FoodConstituent>();
			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwFood();
			messageBuiler.PopulateFoodConstituent(bo, obj);
			NUnit.Framework.Assert.That(!bo.Constituent.Any(), NUnit.Framework.Is.True);

			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwFood();
			messageBuiler.PopulateFoodConstituent(bo, null);
			NUnit.Framework.Assert.That(!bo.Constituent.Any(), NUnit.Framework.Is.True);

			obj = new List<FoodConstituent>();
			for (int i = 0; i < 100; i++)
			{
				obj.Add(new FoodConstituent("element name", 123m));
			}
			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwFood();
			messageBuiler.PopulateFoodConstituent(bo, obj);
			NUnit.Framework.Assert.That(bo.Constituent.Count, NUnit.Framework.Is.EqualTo(99));
		}

		[ExpectNoExceptions]
		public void TestPopulateMedicalInstrument()
		{
			var messageBuiler = new NX5105MessageBuilder();
			var obj = new MedicalInstrument(new ZString('1', 15), null);
			var bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();

			messageBuiler.PopulateMedicalInstrument(bo, obj);
			NUnit.Framework.Assert.That(bo.TwMedicalInstrument.TwLpcoid.Value, NUnit.Framework.Is.EqualTo(new ZString('1', 14)).Using(CustomComparers.TypeComparison));

			obj = new MedicalInstrument(ZString.Empty, null);
			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
			messageBuiler.PopulateMedicalInstrument(bo, obj);
			NUnit.Framework.Assert.That(bo.TwMedicalInstrument, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrument)));

			obj = new MedicalInstrument(ZString.Empty, new LPCOAuthorizedParty("11", null, "22"));
			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
			messageBuiler.PopulateMedicalInstrument(bo, obj);
			NUnit.Framework.Assert.That(bo.TwMedicalInstrument, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrument)));
			NUnit.Framework.Assert.That(bo.TwMedicalInstrument.TwLpcoid, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrumentTwLpcoid)));
			NUnit.Framework.Assert.That(bo.TwMedicalInstrument.LpcoAuthorizedParty.Id.Value, NUnit.Framework.Is.EqualTo("11"));
			NUnit.Framework.Assert.That(bo.TwMedicalInstrument.LpcoAuthorizedParty.TwTypeCode.Value, NUnit.Framework.Is.EqualTo("22"));
		}

		[ExpectNoExceptions]
		public void TestPopulateMedicalInstrumentLPCOAuthorizedParty()
		{
			var messageBuiler = new NX5105MessageBuilder();
			var obj = new LPCOAuthorizedParty(new ZString('1', 15), null, new ZString('1', 4));
			var bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrument();

			messageBuiler.PopulateMedicalInstrumentLPCOAuthorizedParty(bo, obj);
			NUnit.Framework.Assert.That(bo.LpcoAuthorizedParty.Id.Value, NUnit.Framework.Is.EqualTo(new ZString('1', 14)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(bo.LpcoAuthorizedParty.TwTypeCode.Value, NUnit.Framework.Is.EqualTo(new ZString('1', 3)).Using(CustomComparers.TypeComparison));

			obj = new LPCOAuthorizedParty(ZString.Empty, null, ZString.Empty);
			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrument();
			messageBuiler.PopulateMedicalInstrumentLPCOAuthorizedParty(bo, obj);
			NUnit.Framework.Assert.That(bo.LpcoAuthorizedParty, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrumentLpcoAuthorizedParty)));

			obj = new LPCOAuthorizedParty("11", null, ZString.Empty);
			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrument();
			messageBuiler.PopulateMedicalInstrumentLPCOAuthorizedParty(bo, obj);
			NUnit.Framework.Assert.That(bo.LpcoAuthorizedParty.Id.Value, NUnit.Framework.Is.EqualTo("11"));
			NUnit.Framework.Assert.That(bo.LpcoAuthorizedParty.TwTypeCode, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrumentLpcoAuthorizedPartyTwTypeCode)));

			obj = new LPCOAuthorizedParty(ZString.Empty, null, "11");
			bo = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrument();
			messageBuiler.PopulateMedicalInstrumentLPCOAuthorizedParty(bo, obj);
			NUnit.Framework.Assert.That(bo.LpcoAuthorizedParty.Id, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemTwMedicalInstrumentLpcoAuthorizedPartyId)));
			NUnit.Framework.Assert.That(bo.LpcoAuthorizedParty.TwTypeCode.Value, NUnit.Framework.Is.EqualTo("11"));
		}

		PartyDetails SetupPartyDetails()
		{
			var result = new PartyDetails();
			result.ID = new ZString('Z', 20);
			result.Name = new ZString('N', 200);
			result.ChineseName = new ZString('C', 200);
			result.CustomsControlID = new ZString('Z', 10);
			result.TypeCode = new ZString('Z', 10);

			var address = new Address();
			address.ChineseLine = new ZString('N', 200);
			address.Line = new ZString('N', 200);
			result.Address = address;

			var communication = new Communication();
			communication.ID = new ZString('Z', 30);
			result.Communications = new List<ICommunication> { communication };

			result.ContactName = new ZString('Z', 80);
			result.LPCOAuthorizedParty = new LPCOAuthorizedParty(new ZString('Z', 30), "", "");

			return result;
		}

		Commodity SetupCommodityDetails()
		{
			var result = new Commodity();
			result.Name = new ZString('A', 100);
			result.Description = new ZString('B', 1000);
			result.ChineseDescription = new ZString('C', 1000);
			result.CommercialCategorizationID = new ZString('D', 100);

			var constituent = new Constituent();
			constituent.ElementDescription = new ZString('E', 1000);
			result.Constituent = constituent;
			return result;
		}
		#endregion

		#region PopulateBankAccount
		[ExpectNoExceptions]
		public void TestPopulateBankAccountMaxLength()
		{
			var obj = new ZString('A', 20);
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateBankAccount(item, obj);
			NUnit.Framework.Assert.That(item.BankAccount.Id.Value.Length, NUnit.Framework.Is.EqualTo(16));
		}

		[ExpectNoExceptions]
		public void TestPopulateBankAccountWithNode()
		{
			var obj = new ZString('A', 20);
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateBankAccount(item, ZString.Empty);
			NUnit.Framework.Assert.That(item.BankAccount, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationBankAccount)));
			messageBuiler.PopulateBankAccount(item, obj);
			NUnit.Framework.Assert.That(item.BankAccount.Id.Value, NUnit.Framework.Is.EqualTo(new ZString('A', 16)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateBankAccountInXml()
		{
			var node = "a:Declaration/a:tw_Application/a:BankAccount";
			var message = new Message();
			var item = message.Applications.First();
			NUnit.Framework.Assert.That(item.BankAccount, NUnit.Framework.Is.EqualTo("BankAccount").Using(CustomComparers.TypeComparison));
			AssertChildNodesNotNull(message, node, new string[] { "ID" }, new string[] { "BankAccount" });

			message.Applications.Cast<Application>().ToList().ForEach(x => x.BankAccount = ZString.Empty);
			NUnit.Framework.Assert.That(item.BankAccount, NUnit.Framework.Is.EqualTo(ZString.Empty));
			AssertNodeNull(message, node);
		}
		#endregion

		#region PopulateContactOffice
		[ExpectNoExceptions]
		public void TestPopulateContactOfficeMaxLength()
		{
			var obj = new ZString('A', 20);
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateContactOffice(item, obj);
			NUnit.Framework.Assert.That(item.ContactOffice.Id.Value.Length, NUnit.Framework.Is.EqualTo(17));
		}

		[ExpectNoExceptions]
		public void TestPopulateContactOfficeWithNode()
		{
			var obj = new ZString('A', 20);
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateContactOffice(item, ZString.Empty);
			NUnit.Framework.Assert.That(item.ContactOffice, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationContactOffice)));
			messageBuiler.PopulateContactOffice(item, obj);
			NUnit.Framework.Assert.That(item.ContactOffice.Id.Value, NUnit.Framework.Is.EqualTo(new ZString('A', 17)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateContactOfficeInXml()
		{
			var node = "a:Declaration/a:tw_Application/a:ContactOffice";
			var message = new Message();
			var item = message.Applications.First();
			NUnit.Framework.Assert.That(item.ContactOffice, NUnit.Framework.Is.EqualTo("ContactOffice").Using(CustomComparers.TypeComparison));
			AssertChildNodesNotNull(message, node, new string[] { "ID" }, new string[] { "ContactOffice" });

			message.Applications.Cast<Application>().ToList().ForEach(x => x.ContactOffice = ZString.Empty);
			NUnit.Framework.Assert.That(item.ContactOffice, NUnit.Framework.Is.EqualTo(ZString.Empty));
			AssertNodeNull(message, node);
		}
		#endregion

		#region PopulateApplicationPayment
		[ExpectNoExceptions]
		public void TestPopulateApplicationPaymentMaxLength()
		{
			var obj = new Application();
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateApplicationPayment(item, obj.Payment);
			NUnit.Framework.Assert.That(item.Payment.MethodCode.Value.Length, NUnit.Framework.Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestPopulateApplicationPaymentWithNode()
		{
			var obj = new Application();
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateApplicationPayment(item, null);
			NUnit.Framework.Assert.That(item.Payment, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationPayment)));
			messageBuiler.PopulateApplicationPayment(item, obj.Payment);
			NUnit.Framework.Assert.That(item.Payment.MethodCode.Value, NUnit.Framework.Is.EqualTo("Pa"));
		}

		[ExpectNoExceptions]
		public void TestPopulateApplicationPaymentInXml()
		{
			var node = "a:Declaration/a:tw_Application/a:Payment";
			var message = new Message();
			var item = message.Applications.First();
			NUnit.Framework.Assert.That(item.Payment.MethodCode, NUnit.Framework.Is.EqualTo("Payment").Using(CustomComparers.TypeComparison));
			AssertChildNodesNotNull(message, node, new string[] { "MethodCode" }, new string[] { "Pa" });

			message.Applications.Cast<Application>().ToList().ForEach(x => ((Payment)x.Payment).MethodCode = ZString.Empty);
			NUnit.Framework.Assert.That(item.Payment.MethodCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			AssertChildNodesNotNull(message, node, new string[] { "MethodCode" }, new string[] { "" });
		}
		#endregion

		#region PopulateApplicationResponsibleGovernmentAgency
		[ExpectNoExceptions]
		public void TestPopulateApplicationResponsibleGovernmentAgencyMaxLength()
		{
			var obj = new ZString('A', 20);
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateApplicationResponsibleGovernmentAgency(item, obj);
			NUnit.Framework.Assert.That(item.ResponsibleGovernmentAgency.Id.Value.Length, NUnit.Framework.Is.EqualTo(5));
		}

		[ExpectNoExceptions]
		public void TestPopulateApplicationResponsibleGovernmentAgencyWithNode()
		{
			var obj = new ZString('A', 20);
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateApplicationResponsibleGovernmentAgency(item, ZString.Empty);
			NUnit.Framework.Assert.That(item.ResponsibleGovernmentAgency.Id.Value, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));
			messageBuiler.PopulateApplicationResponsibleGovernmentAgency(item, obj);
			NUnit.Framework.Assert.That(item.ResponsibleGovernmentAgency.Id.Value, NUnit.Framework.Is.EqualTo(new ZString('A', 5)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateApplicationResponsibleGovernmentAgencyInXml()
		{
			var node = "a:Declaration/a:tw_Application/a:ResponsibleGovernmentAgency";
			var message = new Message();
			var item = message.Applications.First();
			NUnit.Framework.Assert.That(item.ResponsibleGovernmentAgency, NUnit.Framework.Is.EqualTo("ResponsibleGovernmentAgency").Using(CustomComparers.TypeComparison));
			AssertChildNodesNotNull(message, node, new string[] { "ID" }, new string[] { "Respo" });

			message.Applications.Cast<Application>().ToList().ForEach(x => x.ResponsibleGovernmentAgency = ZString.Empty);
			NUnit.Framework.Assert.That(item.ResponsibleGovernmentAgency, NUnit.Framework.Is.EqualTo(ZString.Empty));
			AssertChildNodesNotNull(message, node, new string[] { "ID" }, new string[] { "" });
		}
		#endregion

		#region PopulateAppointment
		[ExpectNoExceptions]
		public void TestPopulateAppointmentMaxLength()
		{
			var obj = new Appointment();
			obj.ReservationPeriodCode = new ZString('A', 20);
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateAppointment(item, obj);
			NUnit.Framework.Assert.That(item.TwAppointment.TwReservationPeriodCode.Value.Length, NUnit.Framework.Is.EqualTo(1));
		}

		[ExpectNoExceptions]
		public void TestPopulateAppointmentWithNode()
		{
			var obj = new Appointment();
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateAppointment(item, null);
			NUnit.Framework.Assert.That(item.TwAppointment, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwAppointment)));
			obj.ReservationDate = ZDateTime.Invalid;
			obj.ReservationPeriodCode = ZString.Empty;
			messageBuiler.PopulateAppointment(item, obj);
			NUnit.Framework.Assert.That(item.TwAppointment, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwAppointment)));
			obj.ReservationDate = ZDateTime.Empty;
			messageBuiler.PopulateAppointment(item, obj);
			NUnit.Framework.Assert.That(item.TwAppointment, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwAppointment)));

			obj.ReservationDate = ZDateTime.Invalid;
			obj.ReservationPeriodCode = "A";
			messageBuiler.PopulateAppointment(item, obj);
			NUnit.Framework.Assert.That(item.TwAppointment.TwReservationDateValueSpecified, NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(item.TwAppointment.TwReservationPeriodCode.Value, NUnit.Framework.Is.EqualTo("A"));

			obj.ReservationDate = new ZDateTime(2020, 09, 17, 16, 17, 22);
			obj.ReservationPeriodCode = ZString.Empty;
			item = new DeclarationTwApplication();
			messageBuiler.PopulateAppointment(item, obj);
			NUnit.Framework.Assert.That(item.TwAppointment.TwReservationDateValueSpecified, NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(item.TwAppointment.TwReservationDate, NUnit.Framework.Is.EqualTo(new DateTime(2020, 09, 17, 16, 17, 22)));
			NUnit.Framework.Assert.That(item.TwAppointment.TwReservationPeriodCode, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwAppointmentTwReservationPeriodCode)));

			obj.ReservationDate = new ZDateTime(2020, 09, 17, 16, 17, 22);
			obj.ReservationPeriodCode = "B";
			item = new DeclarationTwApplication();
			messageBuiler.PopulateAppointment(item, obj);
			NUnit.Framework.Assert.That(item.TwAppointment.TwReservationDateValueSpecified, NUnit.Framework.Is.EqualTo(true));
			NUnit.Framework.Assert.That(item.TwAppointment.TwReservationDate, NUnit.Framework.Is.EqualTo(new DateTime(2020, 09, 17, 16, 17, 22)));
			NUnit.Framework.Assert.That(item.TwAppointment.TwReservationPeriodCode.Value, NUnit.Framework.Is.EqualTo("B"));
		}

		[ExpectNoExceptions]
		public void TestPopulateAppointmentInXml()
		{
			var node = "a:Declaration/a:tw_Application/a:tw_Appointment";
			var message = new Message();
			var item = message.Applications.First().Appointment;
			NUnit.Framework.Assert.That(item.ReservationPeriodCode, NUnit.Framework.Is.EqualTo("ReservationPeriodCode").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(item.ReservationDate, NUnit.Framework.Is.EqualTo(new ZDateTime(2011, 05, 26)));
			AssertChildNodesNotNull(message, node, new string[] { "tw_ReservationDate", "tw_ReservationPeriodCode" }, new string[] { "2011-05-26", "R" });

			message.Applications.Select(x => x.Appointment).Cast<Appointment>().ToList().ForEach(y => { y.ReservationDate = ZDateTime.Empty; y.ReservationPeriodCode = ZString.Empty; });
			NUnit.Framework.Assert.That(item.ReservationPeriodCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			NUnit.Framework.Assert.That(item.ReservationDate, NUnit.Framework.Is.EqualTo(ZDateTime.Empty));
			AssertNodeNull(message, node);
		}
		#endregion

		#region PopulateApprovalAuthenticationInformation
		[ExpectNoExceptions]
		public void TestPopulateApprovalAuthenticationInformationMaxLength()
		{
			var obj = new ZString('A', 20);
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateApprovalAuthenticationInformation(item, obj);
			NUnit.Framework.Assert.That(item.TwApprovalAuthenticationInformation.TwId.Value.Length, NUnit.Framework.Is.EqualTo(6));
		}

		[ExpectNoExceptions]
		public void TestPopulateApprovalAuthenticationInformationWithNode()
		{
			var obj = new ZString('A', 20);
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateApprovalAuthenticationInformation(item, ZString.Empty);
			NUnit.Framework.Assert.That(item.TwApprovalAuthenticationInformation, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwApprovalAuthenticationInformation)));
			messageBuiler.PopulateApprovalAuthenticationInformation(item, obj);
			NUnit.Framework.Assert.That(item.TwApprovalAuthenticationInformation.TwId.Value, NUnit.Framework.Is.EqualTo(new ZString('A', 6)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateApprovalAuthenticationInformationInXml()
		{
			var node = "a:Declaration/a:tw_Application/a:tw_ApprovalAuthenticationInformation";
			var message = new Message();
			var item = message.Applications.First();
			NUnit.Framework.Assert.That(item.ApprovalAuthenticationInformation, NUnit.Framework.Is.EqualTo("ApprovalAuthenticationInformation").Using(CustomComparers.TypeComparison));
			AssertChildNodesNotNull(message, node, new string[] { "tw_ID" }, new string[] { "Approv" });

			message.Applications.Cast<Application>().ToList().ForEach(x => x.ApprovalAuthenticationInformation = ZString.Empty);
			NUnit.Framework.Assert.That(item.ApprovalAuthenticationInformation, NUnit.Framework.Is.EqualTo(ZString.Empty));
			AssertNodeNull(message, node);
		}
		#endregion

		#region PopulateAuthorizedInformation
		[ExpectNoExceptions]
		public void TestPopulateAuthorizedInformationMaxLengthIncludeChild()
		{
			var obj = new AuthorizedInformation();
			obj.AuthorizedTypeCode = new ZString('A', 20);
			obj.AdditionalDocument.ID = new ZString('A', 20);
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateAuthorizedInformation(item, obj);
			NUnit.Framework.Assert.That(item.TwAuthorizedInformation.TwAuthorizedTypeCode.Value.Length, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(item.TwAuthorizedInformation.AdditionalDocument.Id.Value.Length, NUnit.Framework.Is.EqualTo(10));
		}

		[ExpectNoExceptions]
		public void TestPopulateAuthorizedInformationWithNodeIncludeChild()
		{
			var obj = new AuthorizedInformation();
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateAuthorizedInformation(item, null);
			NUnit.Framework.Assert.That(item.TwAuthorizedInformation, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwAuthorizedInformation)));
			obj.AuthorizedTypeCode = ZString.Empty;
			messageBuiler.PopulateAuthorizedInformation(item, obj);
			NUnit.Framework.Assert.That(item.TwAuthorizedInformation, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwAuthorizedInformation)));
			obj.AdditionalDocument.ID = "213542";
			NUnit.Framework.Assert.That(item.TwAuthorizedInformation, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwAuthorizedInformation)));

			obj.AuthorizedTypeCode = "1";
			messageBuiler.PopulateAuthorizedInformation(item, obj);
			NUnit.Framework.Assert.That(item.TwAuthorizedInformation.TwAuthorizedTypeCode.Value, NUnit.Framework.Is.EqualTo("1"));
			NUnit.Framework.Assert.That(item.TwAuthorizedInformation.AdditionalDocument.Id.Value, NUnit.Framework.Is.EqualTo("213542"));

			item = new DeclarationTwApplication();
			obj.AdditionalDocument.ID = ZString.Empty;
			messageBuiler.PopulateAuthorizedInformation(item, obj);
			NUnit.Framework.Assert.That(item.TwAuthorizedInformation.TwAuthorizedTypeCode.Value, NUnit.Framework.Is.EqualTo("1"));
			NUnit.Framework.Assert.That(item.TwAuthorizedInformation.AdditionalDocument, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwAuthorizedInformationAdditionalDocument)));
		}

		[ExpectNoExceptions]
		public void TestPopulateAuthorizedInformationInXmlIncludeChild()
		{
			var node = "a:Declaration/a:tw_Application/a:tw_AuthorizedInformation";
			var chidNode = node + "/a:AdditionalDocument";
			var message = new Message();
			var item = message.Applications.First().AuthorizedInformation;
			NUnit.Framework.Assert.That(item.AuthorizedTypeCode, NUnit.Framework.Is.EqualTo("AuthorizedTypeCode").Using(CustomComparers.TypeComparison));
			AssertChildNodesNotNull(message, node, new string[] { "tw_AuthorizedTypeCode" }, new string[] { "A" });
			NUnit.Framework.Assert.That(item.AdditionalDocument.ID, NUnit.Framework.Is.EqualTo("ID").Using(CustomComparers.TypeComparison));
			AssertChildNodesNotNull(message, chidNode, new string[] { "ID" }, new string[] { "ID" });

			var items = message.Applications.Select(x => x.AuthorizedInformation).Cast<AuthorizedInformation>().ToList();
			items.ForEach(y => y.AuthorizedTypeCode = ZString.Empty);
			NUnit.Framework.Assert.That(item.AuthorizedTypeCode, NUnit.Framework.Is.EqualTo(ZString.Empty));
			AssertNodeNull(message, node);
		}
		#endregion

		#region PopulateDeclarer
		[ExpectNoExceptions]
		public void TestPopulateDeclarerMaxLengthIncludeChildren()
		{
			var obj = new Declarer();
			obj.ChineseName = new ZString('A', 110);
			obj.ID = new ZString('A', 110);
			obj.Name = new ZString('A', 110);
			obj.TypeCode = new ZString('A', 110);
			obj.Address.ChineseLine = new ZString('A', 110);
			obj.Communications[0].ID = new ZString('A', 110);
			obj.Communications[0].TypeID = "TE";
			obj.Communications[1].ID = new ZString('A', 110);
			obj.Communications[1].TypeID = "MA";
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateDeclarer(item, obj);
			NUnit.Framework.Assert.That(item.TwDeclarer.TwChineseName.Value.Length, NUnit.Framework.Is.EqualTo(70));
			NUnit.Framework.Assert.That(item.TwDeclarer.TwId.Value.Length, NUnit.Framework.Is.EqualTo(14));
			NUnit.Framework.Assert.That(item.TwDeclarer.TwName.Value.Length, NUnit.Framework.Is.EqualTo(80));
			NUnit.Framework.Assert.That(item.TwDeclarer.TwTypeCode.Value.Length, NUnit.Framework.Is.EqualTo(3));
			NUnit.Framework.Assert.That(item.TwDeclarer.Address.TwChineseLine.Value.Length, NUnit.Framework.Is.EqualTo(100));
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication[0].Id.Value.Length, NUnit.Framework.Is.EqualTo(20));
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication[0].TypeId.Value.Length, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication[1].Id.Value.Length, NUnit.Framework.Is.EqualTo(60));
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication[1].TypeId.Value.Length, NUnit.Framework.Is.EqualTo(2));
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclarerWithNodeIncludeChildren()
		{
			var obj = new Declarer();
			obj.ChineseName = ZString.Empty;
			obj.ID = ZString.Empty;
			obj.Name = ZString.Empty;
			obj.TypeCode = ZString.Empty;
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateDeclarer(item, null);
			NUnit.Framework.Assert.That(item.TwDeclarer, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwDeclarer)));
			messageBuiler.PopulateDeclarer(item, obj);
			NUnit.Framework.Assert.That(item.TwDeclarer, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwDeclarer)));

			obj.ChineseName = "ChineseName";
			obj.Address.ChineseLine = ZString.Empty;
			obj.Communications[0].ID = ZString.Empty;
			obj.Communications[0].TypeID = ZString.Empty;
			obj.Communications[1].ID = ZString.Empty;
			obj.Communications[1].TypeID = ZString.Empty;
			messageBuiler.PopulateDeclarer(item, obj);
			NUnit.Framework.Assert.That(item.TwDeclarer.TwChineseName.Value, NUnit.Framework.Is.EqualTo("ChineseName"));
			NUnit.Framework.Assert.That(item.TwDeclarer.TwId.Value, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(item.TwDeclarer.TwName.Value, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(item.TwDeclarer.TwTypeCode.Value, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(item.TwDeclarer.Address.TwChineseLine.Value, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication[0].Id.Value, NUnit.Framework.Is.EqualTo("ID"));
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication[0].TypeId.Value, NUnit.Framework.Is.EqualTo("TE"));
			obj.ChineseName = ZString.Empty;
			obj.ID = "ID";
			item = new DeclarationTwApplication();
			messageBuiler.PopulateDeclarer(item, obj);
			NUnit.Framework.Assert.That(item.TwDeclarer.TwChineseName.Value, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));

			SetupDeclarer(obj);
			item = new DeclarationTwApplication();
			messageBuiler.PopulateDeclarer(item, obj);
			NUnit.Framework.Assert.That(item.TwDeclarer.TwChineseName.Value, NUnit.Framework.Is.EqualTo("ChineseName"));
			NUnit.Framework.Assert.That(item.TwDeclarer.TwId.Value, NUnit.Framework.Is.EqualTo("ID"));
			NUnit.Framework.Assert.That(item.TwDeclarer.TwName.Value, NUnit.Framework.Is.EqualTo("Name"));
			NUnit.Framework.Assert.That(item.TwDeclarer.TwTypeCode.Value, NUnit.Framework.Is.EqualTo("TC"));
			NUnit.Framework.Assert.That(item.TwDeclarer.Address.TwChineseLine.Value, NUnit.Framework.Is.EqualTo("ChineseLine"));
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication[0].Id.Value, NUnit.Framework.Is.EqualTo("13925568211"));
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication[0].TypeId.Value, NUnit.Framework.Is.EqualTo("TE"));
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication[1].Id.Value, NUnit.Framework.Is.EqualTo("Mail@test.com"));
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication[1].TypeId.Value, NUnit.Framework.Is.EqualTo("MA"));

			obj.Communications = null;
			item = new DeclarationTwApplication();
			messageBuiler.PopulateDeclarer(item, obj);
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication[0].Id.Value, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication[0].TypeId.Value, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));
			obj.Communications = new List<Communication>();
			item = new DeclarationTwApplication();
			messageBuiler.PopulateDeclarer(item, obj);
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication.Count, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication[0].Id.Value, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(item.TwDeclarer.Communication[0].TypeId.Value, NUnit.Framework.Is.EqualTo(ZString.Empty).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclarerInXmlIncludeChildren()
		{
			var obj = new Declarer();
			SetupDeclarer(obj);
			obj.Communications[0].ID = "Mail@test.com";
			obj.Communications[0].TypeID = "MA";
			obj.Communications[2].ID = "Mail@test.com";
			obj.Communications[2].TypeID = "MA";
			var node = "a:Declaration/a:tw_Application/a:tw_Declarer";
			var nodeAddress = node + "/a:Address";
			var nodeCommunication = node + "/a:Communication";
			var message = new Message();
			var applications = message.Applications.Cast<Application>().ToList();
			applications.ForEach(x => x.Declarer = obj);
			AssertChildNodesNotNull(message, node, new string[] { "tw_ChineseName", "tw_ID", "tw_Name", "tw_TypeCode" }, new string[] { "ChineseName", "ID", "Name", "TC" });
			AssertChildNodesNotNull(message, nodeAddress, new string[] { "tw_ChineseLine" }, new string[] { "ChineseLine" });
			applications.ForEach(x => x.Declarer = obj);
			AssertChildNodesNotNull(message, nodeCommunication, new string[] { "ID", "TypeID" }, new string[] { "Mail@test.com", "MA" });

			obj.ChineseName = ZString.Empty;
			obj.ID = ZString.Empty;
			obj.Name = ZString.Empty;
			obj.TypeCode = ZString.Empty;
			AssertNodeNull(message, node);
			AssertNodeNull(message, nodeAddress);
			AssertNodeNull(message, nodeCommunication);
		}

		void SetupDeclarer(Declarer obj)
		{
			obj.ChineseName = "ChineseName";
			obj.ID = "ID";
			obj.Name = "Name";
			obj.TypeCode = "TC";
			obj.Address.ChineseLine = "ChineseLine";
			obj.Communications[0].ID = "13925568211";
			obj.Communications[0].TypeID = "TE";
			obj.Communications[1].ID = "Mail@test.com";
			obj.Communications[1].TypeID = "MA";
		}
		#endregion

		#region PopulateItemGroupReference
		[ExpectNoExceptions]
		public void TestPopulateItemGroupReferenceMaxLength()
		{
			var obj = new List<ZInt>(10001);
			for (int i = 0; i < obj.Capacity; i++)
			{
				obj.Add(i);
			}
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateItemGroupReference(item, obj);
			NUnit.Framework.Assert.That(item.TwItemGroupReference.Count, NUnit.Framework.Is.EqualTo(9999));
		}

		[ExpectNoExceptions]
		public void TestPopulateItemGroupReferenceWithNode()
		{
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateItemGroupReference(item, null);
			NUnit.Framework.Assert.That(item.TwItemGroupReference[0].TwSequenceNumeric, NUnit.Framework.Is.EqualTo(decimal.Zero));
			item = new DeclarationTwApplication();
			messageBuiler.PopulateItemGroupReference(item, Array.Empty<ZInt>());
			NUnit.Framework.Assert.That(item.TwItemGroupReference[0].TwSequenceNumeric, NUnit.Framework.Is.EqualTo(decimal.Zero));
			item = new DeclarationTwApplication();
			messageBuiler.PopulateItemGroupReference(item, new ZInt[] { 3 });
			NUnit.Framework.Assert.That(item.TwItemGroupReference[0].TwSequenceNumeric, NUnit.Framework.Is.EqualTo(3m));
		}

		[ExpectNoExceptions]
		public void TestPopulateItemGroupReferenceInXml()
		{
			var node = "a:Declaration/a:tw_Application/a:tw_ItemGroupReference";
			var message = new Message();
			var item = message.Applications.First();
			NUnit.Framework.Assert.That(item.ItemGroupReferenceSequenceNumerics.First(), NUnit.Framework.Is.EqualTo(1).Using(CustomComparers.TypeComparison));
			AssertChildNodesNotNull(message, node, new string[] { "tw_SequenceNumeric" }, new string[] { "1" });

			message.Applications.Cast<Application>().ToList().ForEach(x => x.ItemGroupReferenceSequenceNumerics = null);
			AssertChildNodesNotNull(message, node, new string[] { "tw_SequenceNumeric" }, new string[] { "0" });
		}
		#endregion

		#region PopulateLabel
		[ExpectNoExceptions]
		public void TestPopulateLabelMaxLength()
		{
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateLabel(item, SetupLabelWithFullValues());
			NUnit.Framework.Assert.That(item.TwLabel.Count, NUnit.Framework.Is.EqualTo(99));
			NUnit.Framework.Assert.That(item.TwLabel.First().Status.NameCode.Value.Length, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(item.TwLabel.First().TwLabelDetail.Count, NUnit.Framework.Is.EqualTo(99));
			var labelDetails = item.TwLabel.First().TwLabelDetail.First();
			NUnit.Framework.Assert.That(labelDetails.TwEndNumber.Value.Length, NUnit.Framework.Is.EqualTo(8));
			NUnit.Framework.Assert.That(labelDetails.TwStartNumber.Value.Length, NUnit.Framework.Is.EqualTo(8));
			NUnit.Framework.Assert.That(labelDetails.TwTrack.Value.Length, NUnit.Framework.Is.EqualTo(3));
			NUnit.Framework.Assert.That(labelDetails.TwYear.Value.Length, NUnit.Framework.Is.EqualTo(3));
		}

		[ExpectNoExceptions]
		public void TestPopulateLabelWithNode()
		{
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateLabel(item, null);
			NUnit.Framework.Assert.That(!item.TwLabel.Any(), NUnit.Framework.Is.True);

			var obj = SetupLabelWithFullValues();
			messageBuiler.PopulateLabel(item, obj);
			NUnit.Framework.Assert.That(item.TwLabel, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.ObjectModel.Collection<CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwLabel>)));
			NUnit.Framework.Assert.That(item.TwLabel.First().Status, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwLabelStatus)));
			NUnit.Framework.Assert.That(item.TwLabel.First().TwLabelDetail, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.ObjectModel.Collection<CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwLabelTwLabelDetail>)));

			obj = SetupLabelWithFullValues();
			item = new DeclarationTwApplication();
			var labelDetail = obj.First().LabelDetails.First() as LabelDetail;
			labelDetail.EndNumber = ZString.Empty;
			labelDetail.StartNumber = ZString.Empty;
			labelDetail.Track = ZString.Empty;
			labelDetail.Year = ZString.Empty;
			messageBuiler.PopulateLabel(item, obj);
			NUnit.Framework.Assert.That(!item.TwLabel.First().TwLabelDetail.Any(), NUnit.Framework.Is.True);
			item = new DeclarationTwApplication();
			labelDetail.EndNumber = "A";
			messageBuiler.PopulateLabel(item, obj);
			NUnit.Framework.Assert.That(item.TwLabel.First().TwLabelDetail, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.ObjectModel.Collection<CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwLabelTwLabelDetail>)));
			item = new DeclarationTwApplication();
			labelDetail.EndNumber = ZString.Empty;
			labelDetail.StartNumber = "A";
			messageBuiler.PopulateLabel(item, obj);
			NUnit.Framework.Assert.That(item.TwLabel.First().TwLabelDetail, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.ObjectModel.Collection<CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwLabelTwLabelDetail>)));
			item = new DeclarationTwApplication();
			labelDetail.StartNumber = ZString.Empty;
			labelDetail.Track = "A";
			messageBuiler.PopulateLabel(item, obj);
			NUnit.Framework.Assert.That(item.TwLabel.First().TwLabelDetail, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.ObjectModel.Collection<CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwLabelTwLabelDetail>)));
			item = new DeclarationTwApplication();
			labelDetail.Track = ZString.Empty;
			labelDetail.Year = "A";
			messageBuiler.PopulateLabel(item, obj);
			NUnit.Framework.Assert.That(item.TwLabel.First().TwLabelDetail, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.ObjectModel.Collection<CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwLabelTwLabelDetail>)));
		}

		[ExpectNoExceptions]
		public void TestPopulateLabelInXml()
		{
			var node = "a:Declaration/a:tw_Application/a:tw_Label";
			var childNodeStatus = node + "/a:Status";
			var childNodeLabelDetail = node + "/a:tw_LabelDetail";
			var message = new Message();
			var obj = SetupLabelWithFullValues();
			message.Applications.Cast<Application>().ToList().ForEach(x => x.Labels = obj);
			AssertChildNodesNotNull(message, childNodeStatus, new string[] { "NameCode" }, new string[] { "A" });
			AssertChildNodesNotNull(message, childNodeLabelDetail, new string[] { "tw_EndNumber", "tw_StartNumber", "tw_Track", "tw_Year" }, new string[] { "AAAAAAAA", "BBBBBBBB", "CCC", "DDD" });

			var labelDetail = obj.First().LabelDetails.First() as LabelDetail;
			labelDetail.EndNumber = ZString.Empty;
			labelDetail.StartNumber = ZString.Empty;
			labelDetail.Track = ZString.Empty;
			labelDetail.Year = ZString.Empty;
			message.Applications.Cast<Application>().ToList().ForEach(x => x.ItemGroupReferenceSequenceNumerics = null);
			AssertChildNodesNotNull(message, childNodeStatus, new string[] { "NameCode" }, new string[] { "A" });
			AssertNodeNull(message, childNodeLabelDetail);
		}

		List<Label> SetupLabelWithFullValues()
		{
			var labelDetails = new List<LabelDetail>(101);
			var labelDetail = new LabelDetail() { EndNumber = new ZString('A', 20), StartNumber = new ZString('B', 20), Track = new ZString('C', 20), Year = new ZString('D', 20) };
			for (int i = 0; i < labelDetails.Capacity; i++)
			{
				labelDetails.Add(labelDetail);
			}
			var labels = new List<Label>(101);
			var label = new Label() { LabelDetails = labelDetails, StatusNameCode = "AA" };
			for (int i = 0; i < labelDetails.Capacity; i++)
			{
				labels.Add(label);
			}
			return labels;
		}
		#endregion

		#region PopulateLocalManufacturer
		[ExpectNoExceptions]
		public void TestPopulateLocalManufacturerMaxLength()
		{
			var obj = SetupLocalManufacturerWithFullValues();
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateLocalManufacturer(item, obj);
			var localManufacturer = item.TwLocalManufacturer;
			NUnit.Framework.Assert.That(localManufacturer.TwChineseName.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 70)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(localManufacturer.Address.TwChineseLine.Value, NUnit.Framework.Is.EqualTo(new ZString('A', 100)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(localManufacturer.Communication.Id.Value, NUnit.Framework.Is.EqualTo(new ZString('0', 20)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateLocalManufacturerWithNode()
		{
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateLocalManufacturer(item, null);
			NUnit.Framework.Assert.That(item.TwLocalManufacturer, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwLocalManufacturer)));

			var obj = SetupLocalManufacturerWithFullValues();
			messageBuiler.PopulateLocalManufacturer(item, obj);
			NUnit.Framework.Assert.That(item.TwLocalManufacturer, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwLocalManufacturer)));
			NUnit.Framework.Assert.That(item.TwLocalManufacturer.Address, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwLocalManufacturerAddress)));
			NUnit.Framework.Assert.That(item.TwLocalManufacturer.Communication, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwLocalManufacturerCommunication)));

			item = new DeclarationTwApplication();
			obj.ChineseName = ZString.Empty;
			messageBuiler.PopulateLocalManufacturer(item, obj);
			NUnit.Framework.Assert.That(item.TwLocalManufacturer, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwLocalManufacturer)));

			obj = SetupLocalManufacturerWithFullValues();
			((Address)obj.Address).ChineseLine = ZString.Empty;
			obj.Communications = null;
			messageBuiler.PopulateLocalManufacturer(item, obj);
			NUnit.Framework.Assert.That(item.TwLocalManufacturer, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwLocalManufacturer)));
			NUnit.Framework.Assert.That(item.TwLocalManufacturer.Address, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwLocalManufacturerAddress)));
			NUnit.Framework.Assert.That(item.TwLocalManufacturer.Communication, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwLocalManufacturerCommunication)));
		}

		[ExpectNoExceptions]
		public void TestPopulateLocalManufacturerInXml()
		{
			var node = "a:Declaration/a:tw_Application/a:tw_LocalManufacturer";
			var childNodeAddress = node + "/a:Address";
			var childNodeCommunication = node + "/a:Communication";
			var message = new Message();
			var obj = SetupLocalManufacturerWithFullValues();
			message.Applications.Cast<Application>().ToList().ForEach(x => x.LocalManufacturer = obj);
			AssertChildNodesNotNull(message, node, new string[] { "tw_ChineseName" }, new string[] { new string('N', 70) });
			AssertChildNodesNotNull(message, childNodeAddress, new string[] { "tw_ChineseLine" }, new string[] { new string('A', 100) });
			AssertChildNodesNotNull(message, childNodeCommunication, new string[] { "ID" }, new string[] { new string('0', 20) });
		}

		LocalManufacturer SetupLocalManufacturerWithFullValues()
		{
			var result = new LocalManufacturer();
			result.ChineseName = new ZString('N', 80);
			result.Address = new Address() { ChineseLine = new ZString('A', 120) };
			result.Communications = new[] { new Communication() { ID = new ZString('0', 30) }, new Communication() { ID = new ZString('0', 30) } };
			return result;
		}
		#endregion

		#region PopulateApplicationWine
		[ExpectNoExceptions]
		public void TestPopulateApplicationWineMaxLength()
		{
			var obj = SetupApplicationWineWithFullValues();
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateApplicationWine(item, obj);
			NUnit.Framework.Assert.That(item.TwWine.AdditionalDocument.Count, NUnit.Framework.Is.EqualTo(10));
			NUnit.Framework.Assert.That(item.TwWine.AdditionalDocument.First().Id.Value.Length, NUnit.Framework.Is.EqualTo(14));
			NUnit.Framework.Assert.That(item.TwWine.GovernmentProcedure.PreviousCode.Value.Length, NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(item.TwWine.PreviousDocument.Id.Value.Length, NUnit.Framework.Is.EqualTo(14));
		}

		[ExpectNoExceptions]
		public void TestPopulateApplicationWineWithNode()
		{
			var messageBuiler = new NX5105MessageBuilder();
			var item = new DeclarationTwApplication();
			messageBuiler.PopulateApplicationWine(item, null);
			NUnit.Framework.Assert.That(item.TwWine, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwWine)));

			var obj = SetupApplicationWineWithFullValues();
			messageBuiler.PopulateApplicationWine(item, obj);
			NUnit.Framework.Assert.That(item.TwWine.AdditionalDocument.First(), NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwWineAdditionalDocument)));
			NUnit.Framework.Assert.That(item.TwWine.GovernmentProcedure, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwWineGovernmentProcedure)));
			NUnit.Framework.Assert.That(item.TwWine.PreviousDocument, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwWinePreviousDocument)));

			item = new DeclarationTwApplication();
			obj.AdditionalDocuments = null;
			obj.GovernmentProcedurePreviousCode = ZString.Empty;
			obj.PreviousDocument = null;
			messageBuiler.PopulateApplicationWine(item, obj);
			NUnit.Framework.Assert.That(item.TwWine, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwWine)));

			item = new DeclarationTwApplication();
			obj = SetupApplicationWineWithFullValues();
			obj.AdditionalDocuments = null;
			messageBuiler.PopulateApplicationWine(item, obj);
			NUnit.Framework.Assert.That(!item.TwWine.AdditionalDocument.Any(), NUnit.Framework.Is.True);
			NUnit.Framework.Assert.That(item.TwWine.GovernmentProcedure, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwWineGovernmentProcedure)));
			NUnit.Framework.Assert.That(item.TwWine.PreviousDocument, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwWinePreviousDocument)));
			item = new DeclarationTwApplication();
			obj = SetupApplicationWineWithFullValues();
			obj.GovernmentProcedurePreviousCode = ZString.Empty;
			messageBuiler.PopulateApplicationWine(item, obj);
			NUnit.Framework.Assert.That(item.TwWine.AdditionalDocument, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.ObjectModel.Collection<CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwWineAdditionalDocument>)));
			NUnit.Framework.Assert.That(item.TwWine.GovernmentProcedure, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwWineGovernmentProcedure)));
			NUnit.Framework.Assert.That(item.TwWine.PreviousDocument, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwWinePreviousDocument)));
			item = new DeclarationTwApplication();
			obj = SetupApplicationWineWithFullValues();
			obj.PreviousDocument = null;
			messageBuiler.PopulateApplicationWine(item, obj);
			NUnit.Framework.Assert.That(item.TwWine.AdditionalDocument, NUnit.Framework.Is.Not.EqualTo(default(System.Collections.ObjectModel.Collection<CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwWineAdditionalDocument>)));
			NUnit.Framework.Assert.That(item.TwWine.GovernmentProcedure, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwWineGovernmentProcedure)));
			NUnit.Framework.Assert.That(item.TwWine.PreviousDocument, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationTwApplicationTwWinePreviousDocument)));
		}

		[ExpectNoExceptions]
		public void TestPopulateApplicationWineInXml()
		{
			var node = "a:Declaration/a:tw_Application/a:tw_Wine";
			var childNodeAdditionalDocument = node + "/a:AdditionalDocument";
			var childNodeGovernmentProcedure = node + "/a:GovernmentProcedure";
			var childNodePreviousDocument = node + "/a:PreviousDocument";
			var message = new Message();
			var obj = SetupApplicationWineWithFullValues();
			message.Applications.Cast<Application>().ToList().ForEach(x => x.Wine = obj);
			AssertChildNodesNotNull(message, childNodeAdditionalDocument, new string[] { "ID" }, new string[] { new string('I', 14) });
			AssertChildNodesNotNull(message, childNodeGovernmentProcedure, new string[] { "PreviousCode" }, new string[] { "D" });
			AssertChildNodesNotNull(message, childNodePreviousDocument, new string[] { "ID" }, new string[] { new string('N', 14) });

			message.Applications.Cast<Application>().ToList().ForEach(x => x.Wine = null);
			AssertNodeNull(message, node);
		}

		ApplicationWine SetupApplicationWineWithFullValues()
		{
			var result = new ApplicationWine();
			var additionalDocuments = new List<AdditionalDocument>(12);
			var additionalDocument = new AdditionalDocument() { ID = new ZString('I', 20) };
			for (int i = 0; i < additionalDocuments.Capacity; i++)
			{
				additionalDocuments.Add(additionalDocument);
			}
			result.AdditionalDocuments = additionalDocuments;
			result.GovernmentProcedurePreviousCode = new ZString("DD");
			result.PreviousDocument.ID = new ZString('N', 20);
			return result;
		}
		#endregion

		#region Assert
		[ExpectNoExceptions]
		void AssertChildNodesNotNull(Message message, string parentNodePath, string[] childNodeNames, string[] expectChildNodeValues)
		{
			var builder = GetNX5105MessageBuilderXml(message);
			var xmlDocument = builder.Item1;
			var nameSpace = builder.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode(parentNodePath, nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			for (int i = 0; i < childNodeNames.Length; i++)
			{
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode(parentNodePath + "/a:" + childNodeNames[i], nameSpace).InnerText, NUnit.Framework.Is.EqualTo(expectChildNodeValues[i]));
			}
		}

		[ExpectNoExceptions]
		void AssertNodeNull(Message message, string nodePath)
		{
			var builder = GetNX5105MessageBuilderXml(message);
			var xmlDocument = builder.Item1;
			var nameSpace = builder.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode(nodePath, nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
		}

		Tuple<XmlDocument, XmlNamespaceManager> GetNX5105MessageBuilderXml(Message message)
		{
			var builder = new NX5105MessageBuilder();
			var xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);

			var result1 = new XmlDocument();
			result1.LoadXml(xml);
			var namespacePrefix = "a";
			var result2 = new XmlNamespaceManager(result1.NameTable);
			result2.AddNamespace(namespacePrefix, result1.DocumentElement.Attributes["xmlns"]?.Value);

			return new Tuple<XmlDocument, XmlNamespaceManager>(result1, result2);
		}
		#endregion

		[ExpectNoExceptions]
		public void TestPopulateQuarantine()
		{
			CombineAssertions("tw_Quarantine in xml", () =>
			{
				var node = "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_Quarantine";
				var nodeAnimal = node + "/a:tw_Animal";
				Message.TestForEmptyValue = true;
				var message = new Message();
				var quarantine = message.GoodsShipment.GovernmentAgencyGoodsItems.First().Commodity.Quarantine;
				var animal = quarantine.Animal;
				NUnit.Framework.Assert.That(quarantine.ObjectFeature, NUnit.Framework.Is.EqualTo(ZString.Empty));
				NUnit.Framework.Assert.That(quarantine.Treatment, NUnit.Framework.Is.EqualTo(ZString.Empty));
				NUnit.Framework.Assert.That(animal, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.TW.Messaging.IAnimal)));

				Message.TestForEmptyValue = false;
				message = new Message();
				quarantine = message.GoodsShipment.GovernmentAgencyGoodsItems.First().Commodity.Quarantine;
				animal = quarantine.Animal;
				NUnit.Framework.Assert.That(quarantine.ObjectFeature, NUnit.Framework.Is.EqualTo("ObjectFeature").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(quarantine.Treatment, NUnit.Framework.Is.EqualTo("Treatment").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(animal.AgeMonthNumeric, NUnit.Framework.Is.EqualTo(7).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(animal.AgeYearNumeric, NUnit.Framework.Is.EqualTo(8).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(animal.FemaleQuantity, NUnit.Framework.Is.EqualTo(9).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(animal.MaleQuantity, NUnit.Framework.Is.EqualTo(10).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(animal.MicrochipID, NUnit.Framework.Is.EqualTo("MicrochipID").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(animal.Vaccination, NUnit.Framework.Is.EqualTo("Vaccination").Using(CustomComparers.TypeComparison));
				AssertChildNodesNotNull(message, node, new string[] { "tw_ObjectFeature", "tw_Treatment" }, new string[] { "ObjectFeature", "Treatment" });
				AssertChildNodesNotNull(message, nodeAnimal, new string[] { "tw_AgeMonthNumeric", "tw_AgeYearNumeric", "tw_FemaleQuantity", "tw_MaleQuantity", "tw_MicrochipID", "tw_Vaccination" }, new string[] { "7", "8", "9", "10", "MicrochipID", "Vaccination" });
			});

			var builder = new NX5105MessageBuilder();
			var quarantine2 = new Quarantine();
			CombineAssertions("tw_Quarantine childnodes", () =>
			{
				quarantine2.ObjectFeature = ZString.Empty;
				quarantine2.Treatment = ZString.Empty;
				var commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
				builder.PopulateQuarantine(commodity, quarantine2);
				var quarantine3 = commodity.TwQuarantine;
				NUnit.Framework.Assert.That(quarantine3.TwObjectFeature, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwObjectFeature)));
				NUnit.Framework.Assert.That(quarantine3.TwTreatment, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwTreatment)));
				NUnit.Framework.Assert.That(quarantine3.TwAnimal, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimal)));

				quarantine2.ObjectFeature = "ObjectFeature";
				var animal2 = quarantine2.Animal as Animal;
				animal2.AgeMonthNumeric = ZInt.Zero;
				animal2.AgeYearNumeric = ZInt.Zero;
				animal2.FemaleQuantity = ZInt.Zero;
				animal2.MaleQuantity = ZInt.Zero;
				animal2.MicrochipID = ZString.Empty;
				animal2.Vaccination = ZString.Empty;
				commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
				builder.PopulateQuarantine(commodity, quarantine2);
				quarantine3 = commodity.TwQuarantine;
				NUnit.Framework.Assert.That(quarantine3.TwObjectFeature.Value, NUnit.Framework.Is.EqualTo("ObjectFeature"));
				NUnit.Framework.Assert.That(quarantine3.TwTreatment, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwTreatment)));
				NUnit.Framework.Assert.That(quarantine3.TwAnimal, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimal)));
			});

			CombineAssertions("tw_Animal childnodes", () =>
			{
				quarantine2.ObjectFeature = ZString.Empty;
				var animal2 = quarantine2.Animal as Animal;
				animal2.AgeMonthNumeric = 1;
				animal2.AgeYearNumeric = ZInt.Zero;
				animal2.FemaleQuantity = ZInt.Zero;
				animal2.MaleQuantity = ZInt.Zero;
				animal2.MicrochipID = ZString.Empty;
				animal2.Vaccination = ZString.Empty;
				var commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
				builder.PopulateQuarantine(commodity, quarantine2);
				var quarantine3 = commodity.TwQuarantine;
				NUnit.Framework.Assert.That(quarantine3.TwObjectFeature, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwObjectFeature)));
				NUnit.Framework.Assert.That(quarantine3.TwTreatment, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwTreatment)));
				var animal3 = quarantine3.TwAnimal;
				NUnit.Framework.Assert.That(animal3.TwAgeMonthNumeric, NUnit.Framework.Is.EqualTo(1m));
				NUnit.Framework.Assert.That(animal3.TwAgeMonthNumericValueSpecified, NUnit.Framework.Is.EqualTo(true));
				NUnit.Framework.Assert.That(animal3.TwAgeYearNumeric, NUnit.Framework.Is.Null);
				NUnit.Framework.Assert.That(animal3.TwAgeYearNumericValueSpecified, NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(animal3.TwFemaleQuantity, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimalTwFemaleQuantity)));
				NUnit.Framework.Assert.That(animal3.TwMaleQuantity, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimalTwMaleQuantity)));
				NUnit.Framework.Assert.That(animal3.TwMicrochipId, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimalTwMicrochipId)));
				NUnit.Framework.Assert.That(animal3.TwVaccination, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimalTwVaccination)));
				animal2.AgeMonthNumeric = 0;
				animal2.Vaccination = "Vaccination1";
				builder.PopulateQuarantine(commodity, quarantine2);
				quarantine3 = commodity.TwQuarantine;
				NUnit.Framework.Assert.That(quarantine3.TwObjectFeature, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwObjectFeature)));
				NUnit.Framework.Assert.That(quarantine3.TwTreatment, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwTreatment)));
				animal3 = quarantine3.TwAnimal;
				NUnit.Framework.Assert.That(animal3.TwAgeMonthNumeric, NUnit.Framework.Is.Null);
				NUnit.Framework.Assert.That(animal3.TwAgeMonthNumericValueSpecified, NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(animal3.TwAgeYearNumeric, NUnit.Framework.Is.Null);
				NUnit.Framework.Assert.That(animal3.TwAgeYearNumericValueSpecified, NUnit.Framework.Is.EqualTo(false));
				NUnit.Framework.Assert.That(animal3.TwFemaleQuantity, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimalTwFemaleQuantity)));
				NUnit.Framework.Assert.That(animal3.TwMaleQuantity, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimalTwMaleQuantity)));
				NUnit.Framework.Assert.That(animal3.TwMicrochipId, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantineTwAnimalTwMicrochipId)));
				NUnit.Framework.Assert.That(animal3.TwVaccination.Value, NUnit.Framework.Is.EqualTo("Vaccination1"));
			});
		}

		[ExpectNoExceptions]
		public void TestNodesNotPopulateWhenNodesNotEmpty()
		{
			var oldTestForEmptyValue = Message.TestForEmptyValue;
			Message.TestForEmptyValue = true;

			var builder = new NX5105MessageBuilder();
			var quarantine = new Quarantine(true, true, true);
			var commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
			builder.PopulateQuarantine(commodity, quarantine);

			CombineAssertions("tw_PackingHouse childnodes", () =>
			{
				NUnit.Framework.Assert.That(commodity.TwQuarantine, NUnit.Framework.Is.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantine)));

				quarantine = new Quarantine(false, true, true);
				commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
				builder.PopulateQuarantine(commodity, quarantine);
				NUnit.Framework.Assert.That(commodity.TwQuarantine, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantine)));

				quarantine = new Quarantine(true, true, false);
				commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
				builder.PopulateQuarantine(commodity, quarantine);
				NUnit.Framework.Assert.That(commodity.TwQuarantine, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantine)));

				quarantine = new Quarantine(true, false, true);
				commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity();
				builder.PopulateQuarantine(commodity, quarantine);
				NUnit.Framework.Assert.That(commodity.TwQuarantine, NUnit.Framework.Is.Not.EqualTo(default(CargoWise.Customs.TW.MessageDefinitions.NX5105.DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityTwQuarantine)));
			});

			Message.TestForEmptyValue = oldTestForEmptyValue;
		}

		[ExpectNoExceptions]
		public void TestPopulateImporterWithEmptyPaymentOnAccountBusinessID()
		{
			var obj = new PartyDetails();
			obj.PaymentOnAccountBusinessID = null;

			var message = new Message();
			message.Importer = obj;
			var builder = new NX5105MessageBuilder();
			var xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);

			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			var namespacePrefix = "a";
			var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Importer/a:tw_PaymentOnAccountBusinessID", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));

			obj.PaymentOnAccountBusinessID = "A";
			message.Importer = obj;
			xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);
			xmlDocument.LoadXml(xml);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Importer/a:tw_PaymentOnAccountBusinessID", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Importer/a:tw_PaymentOnAccountBusinessID", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("A"));
		}

		[ExpectNoExceptions]
		public void TestPopulateBondedGoodsMonthlyReport()
		{
			var goodsShipment = new GoodsShipment();
			var consignment = new Consignment();
			goodsShipment.Consignment = consignment;
			var bondedGoods = new BondedGoods();
			consignment.BondedGoods = bondedGoods;
			var bondedGoodsMonthlyReport = new BondedGoodsMonthlyReport();
			bondedGoods.BondedGoodsMonthlyReport = bondedGoodsMonthlyReport;
			bondedGoodsMonthlyReport.MonthNumeric = 1;
			bondedGoodsMonthlyReport.TraderReferenceID = "";

			var message = new Message();
			message.GoodsShipment = goodsShipment;
			var builder = new NX5105MessageBuilder();
			var xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);

			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			var namespacePrefix = "a";
			var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_TraderReferenceID", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_MonthNumeric", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_MonthNumeric", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("1"));

			bondedGoodsMonthlyReport.MonthNumeric = 0;
			bondedGoodsMonthlyReport.TraderReferenceID = "A";

			xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);
			xmlDocument.LoadXml(xml);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_TraderReferenceID", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_MonthNumeric", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_TraderReferenceID", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("A"));

			bondedGoodsMonthlyReport.MonthNumeric = 1;
			bondedGoodsMonthlyReport.TraderReferenceID = "A";

			xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);
			xmlDocument.LoadXml(xml);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_TraderReferenceID", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_MonthNumeric", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_TraderReferenceID", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("A"));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_MonthNumeric", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("1"));
		}

		[ExpectNoExceptions]
		public void TestPopulateImporterAddressChineseLineWhenNoLocalAddress()
		{
			var address = new Address();
			address.ChineseLine = "";

			var importer = new PartyDetails();
			importer.Address = address;

			var message = new Message();
			message.Importer = importer;
			var builder = new NX5105MessageBuilder();
			var xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);

			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			var namespacePrefix = "a";
			var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Importer/a:Address/a:tw_ChineseLine", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));

			address.ChineseLine = "A";
			xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);
			xmlDocument.LoadXml(xml);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Importer/a:Address/a:tw_ChineseLine", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:Importer/a:Address/a:tw_ChineseLine", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("A"));
		}

		[ExpectNoExceptions]
		public void TestPopulateSellerAddressChineseLineWhenNoLocalAddress()
		{
			var address = new Address();
			address.ChineseLine = "";

			var seller = new PartyDetails();
			seller.Address = address;

			var goodsShipment = new GoodsShipment();
			goodsShipment.Seller = seller;

			var message = new Message();
			message.GoodsShipment = goodsShipment;
			var builder = new NX5105MessageBuilder();
			var xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);

			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			var namespacePrefix = "a";
			var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:Address/a:tw_ChineseLine", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));

			address.ChineseLine = "A";
			xml = builder.SerializeToMessageString(message, MessageFunctionCode.Add);
			xmlDocument.LoadXml(xml);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"]?.Value);

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:Address/a:tw_ChineseLine", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Seller/a:Address/a:tw_ChineseLine", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("A"));
		}
	}
}
