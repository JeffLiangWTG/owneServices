using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.Customs.TW.MessageDefinitions.N5203;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	[TestedType(typeof(N5203MessageBuilder))]
	sealed class N5203MessageBuilderTest : BaseTWMessageBuilderTest<IN5203Declaration, Declaration>
	{
		public class Message : IN5203Declaration
		{
			public Message()
			{
				DutyTaxFee = new DutyTaxFee();
				GoodsShipment = new GoodsShipment();
				AdditionalDocuments = new List<IAdditionalDocument>() { new AdditionalDocument(), new AdditionalDocument() };
				Packaging = new DeclarationPackaging();
			}
			public static bool NodeValueIsEmpty;
			public ZDate AcceptanceDateTime => new ZDate(2011, 05, 23);
			public ZString Authentication => Message.NodeValueIsEmpty ? "" : "Authentication";
			public ZString ID => "MessageID";
			public ZString FunctionCode => "5";
			public ZDecimal InvoiceAmount => 11.333;
			public ZDecimal TotalGrossMassMeasure => 12.3333333;
			public ZInt TotalPackageQuantity => 1;
			public ZString AssociatedGovernmentProcedureCode => NodeValueIsEmpty ? "" : "AssociatedGovernmentProcedureCode";
			public ZString TypeCode => "TypeCode";
			public IEnumerable<IAdditionalDocument> AdditionalDocuments { get; }
			public IAdditionalInformation AdditionalInformation => new AdditionalInformation();
			public IPartyDetails Agent => new PartyDetails();
			public ITransportMeans BorderTransportMeans => new BorderTransportMeans();
			public ICurrencyExchange CurrencyExchange => new CurrencyExchange();

			IDutyTaxFee IN5203Declaration.DutyTaxFee => DutyTaxFee;
			internal readonly IDutyTaxFee DutyTaxFee;

			IGoodsShipment IN5203Declaration.GoodsShipment => GoodsShipment;
			internal readonly GoodsShipment GoodsShipment;

			public IEnumerable<ZString> GovernmentProcedureDescriptions => Message.NodeValueIsEmpty ? new List<ZString>() { "" } : new List<ZString>() { "GovernmentProcedureDescription", "GovernmentProcedureDescription" };
			public IDeclarationPackaging Packaging { get; }
			public ZString RepresentativePersonName => "RepresentativePerson";
		}

		class AdditionalDocument : IAdditionalDocument
		{
			public ZString ID = Message.NodeValueIsEmpty ? "" : "AdditionalDocumentID";
			public ZString Content = null;
			public ZString ImageFileFormat = null;
			public ZString ImageFileName = null;
			public ZInt SequenceNumeric = Message.NodeValueIsEmpty ? 0 : 1;
			public ZLong SizeMeasure = 0;
			public ZString TypeCode = null;
			public ZString ResponsibleGovernmentAgency => "ZString";

			ZString IAdditionalDocument.ID => ID;

			ZString IAdditionalDocument.Content => Content;

			ZString IAdditionalDocument.ImageFileFormat => ImageFileFormat;

			ZString IAdditionalDocument.ImageFileName => ImageFileName;

			ZInt IAdditionalDocument.SequenceNumeric => SequenceNumeric;

			ZLong IAdditionalDocument.SizeMeasure => SizeMeasure;

			ZString IAdditionalDocument.TypeCode => TypeCode;

			ZDate IAdditionalDocument.SlaughterDateTime => ZDate.Empty;
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

			ZString IAdditionalInformation.PackingHouse => null;
		}

		class PartyDetails : IPartyDetails
		{
			public PartyDetails()
			{
				ID = Message.NodeValueIsEmpty ? "" : "96989317";
				LPCOAuthorizedParty = new LPCOAuthorizedParty();
				TypeCode = Message.NodeValueIsEmpty ? "" : "CDF";
				CustomsControlID = Message.NodeValueIsEmpty ? "" : "96944490";
				PaymentOnAccountBusinessID = Message.NodeValueIsEmpty ? "" : "52889317";
				Address = new Address();
			}

			public ZString ID { get; set; }
			public ZString RoleCode => "RoleCode";
			public ZString SubBoxID => "SubBoxID";
			public ILPCOAuthorizedParty LPCOAuthorizedParty { get; set; }
			public ZString Name { get; set; }
			public ZString ChineseName { get; set; }
			public ZString TypeCode { get; set; }
			public ZString CustomsControlID { get; set; }
			public ZString PaymentOnAccountBusinessID { get; set; }
			public IAddress Address { get; set; }
			public IEnumerable<ICommunication> Communications => null;
			public ZString ContactName => null;
			public ZString MainManufacturer => null;
			public ZString UndertakeCode => null;
			public ZString OwnerName => null;
			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;
		}

		class LPCOAuthorizedParty : ILPCOAuthorizedParty
		{
			public LPCOAuthorizedParty()
			{
				ID = Message.NodeValueIsEmpty ? "" : "022347689402";
			}
			public ZString ID { get; set; }
			public ZString Name => null;
			public ZString TypeCode => null;
		}

		class BorderTransportMeans : ITransportMeans
		{
			public ZString TypeCode => "TypeCode";
			public IEnumerable<ZString> ItineraryRoutingCountryCodes => new List<ZString>() { "Itinerary", "Itinerary" };
			public ZDate ArrivalDateTime => ZDate.Empty;
			public ZString ID => null;
			public ZString JourneyID => null;
			public ZString Registration => Message.NodeValueIsEmpty ? "" : "Registration";
			public ZString Name => null;
			public ZString CallSignID => Message.NodeValueIsEmpty ? "" : "CallSignID";
		}

		class CurrencyExchange : ICurrencyExchange
		{
			public ZString CurrencyTypeCode => "CurrencyTypeCode";
			public ZDecimal RateNumeric => 13.333333;
		}

		class DutyTaxFee : IDutyTaxFee
		{
			public ZString DutyMethodCode => Message.NodeValueIsEmpty ? "" : "DutyMethodCode";
			public ZString DutyExemptionWaiverNote => null;
			public ZString DutyMemoPrinted => null;
			public ZDecimal TotalDutyTaxFeeAmount => -1;
			public ZString PaymentObligationGuaranteeReferenceID => null;
			public ZDecimal TotalCashDutyTaxFeeAmount => ZDecimal.Zero;
			public ZDecimal TotalNonCashDutyTaxFeeAmount => ZDecimal.Zero;
		}

		internal class GoodsShipment : IGoodsShipment
		{
			public GoodsShipment()
			{
				GovernmentAgencyGoodsItems = new List<GovernmentAgencyGoodsItem>() { new GovernmentAgencyGoodsItem(), new GovernmentAgencyGoodsItem() };
				CustomsValuation = new CustomsValuation();
				Consignment = new Consignment();
				AdditionalDocuments = new List<IAdditionalDocument>() { new AdditionalDocument(), new AdditionalDocument() };
				Buyer = new PartyDetails();
			}
			[DecimalPlaces(0)]
			public ZDecimal ItemChargeAmount => 14.3;
			public IEnumerable<IAdditionalDocument> AdditionalDocuments { get; }
			public IPartyDetails Buyer { get; }
			public IPartyDetails Consignee => new PartyDetails();

			IConsignment IGoodsShipment.Consignment => Consignment;
			internal readonly Consignment Consignment;

			public IPartyDetails Consignor => new Consignor();

			ICustomsValuation IGoodsShipment.CustomsValuation => CustomsValuation;
			internal readonly CustomsValuation CustomsValuation;

			public ZString DeliveryDestinationName => "DeliveryDestination";
			public IPartyDetails Exporter => new PartyDetails();

			IEnumerable<IGovernmentAgencyGoodsItem> IGoodsShipment.GovernmentAgencyGoodsItems => GovernmentAgencyGoodsItems;
			internal readonly IEnumerable<GovernmentAgencyGoodsItem> GovernmentAgencyGoodsItems;

			public IPartyDetails NotifyParty => new PartyDetails();
			public ZString TradeTermsConditionCode => "TradeTerms";
			public ZString UCR => Message.NodeValueIsEmpty ? "" : "UCR";
			public ZDateTime ExitDateTime => ZDateTime.Empty;
			public ZDecimal TotalCIFAmount => -1.3;
			public IEnumerable<IGoodsShipmentDutyTaxFee> DutyTaxFees => null;
			public IPartyDetails Seller => null;
			public IEnumerable<IGoodsMeasure> GoodsMeasures => null;
			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;

			public IEnumerable<IAdditionalDeclaration> AdditionalDeclarations => null;
		}

		class Address : IAddress
		{
			public Address()
			{
				Line = Message.NodeValueIsEmpty ? "" : "Line";
				CountryCode = "TW";
			}

			public ZString CountryCode { get; set; }
			public ZString Line { get; set; }
			public ZString ChineseLine { get; set; }
			public ZString CountrySubDivisionID => null;
			public ZString CountrySubDivisionName => null;
		}

		internal class Consignment : IConsignment
		{
			public Consignment()
			{
				BondedGoods = new BondedGoods();
			}

			public ZString ShippingOrderNumber => Message.NodeValueIsEmpty ? "" : "ShippingOrderNumber";
			public IEnumerable<IAdditionalInformation> AdditionalInformations => new List<IAdditionalInformation>() { new ConsignmentAdditionalInformation(), new ConsignmentAdditionalInformation() };
			public ITransportMeans BorderTransportMeans => new BorderTransportMeans();
			public IPartyDetails Carrier => new PartyDetails();
			public ITransportMeans DepartureTransportMeans => new DepartureTransportMeans();
			public IEnumerable<ZString> GoodsLocations => Message.NodeValueIsEmpty ? new List<ZString>() { "", "" } : new List<ZString>() { "GoodsLocation", "GoodsLocation" };
			public ILocation LoadingLocation => new Location(Message.NodeValueIsEmpty ? "" : "LoadingLocation");
			public IEnumerable<ITransportContractDocument> TransportContractDocuments => new List<ITransportContractDocument>() { new TransportContractDocument(), new TransportContractDocument() };
			public IEnumerable<ITransportEquipment> TransportEquipments => new List<ITransportEquipment>() { new TransportEquipment(), new TransportEquipment() };

			IBondedGoods IConsignment.BondedGoods => BondedGoods;
			internal readonly BondedGoods BondedGoods;

			public ILocation UnloadingLocation => new Location("UnloadingLocation");
			public ZString ManifestSerialNumber => null;
			public ZString ArrivalTransportMeansTypeCode => null;
			public IConsignmentItem ConsignmentItem => null;
			public ZString GoodsLocation => null;
			public ZString TransitTransportMeansTypeCode => null;
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

		class DepartureTransportMeans : ITransportMeans
		{
			public ZString ID => Message.NodeValueIsEmpty ? "" : "DepartureTransportMeansID";
			public ZString Name => Message.NodeValueIsEmpty ? "" : "Name";
			public ZString TypeCode => "TypeCode";
			public ZString JourneyID => null;
			public ZString Registration => null;
			public ZDate ArrivalDateTime => ZDate.Empty;
			public IEnumerable<ZString> ItineraryRoutingCountryCodes => null;
			public ZString CallSignID => null;
		}

		class TransportContractDocument : ITransportContractDocument
		{
			public ZString ID => "TransportContractDocumentID";
			public ZString TypeCode => "TypeCode";
			public IPartyDetails Deconsolidator => null;
		}

		class TransportEquipment : ITransportEquipment
		{
			public ZString CharacteristicCode => "CharacteristicCode";
			public ZString ID => "TransportEquipmentID";
			public ZString UsedCapacityCode => "UsedCapacityCode";
			public IEnumerable<ZString> Seals => new List<ZString>() { "SealID", "SealID" };
		}

		internal class BondedGoods : IBondedGoods
		{
			public BondedGoods()
			{
				BondedGoodsMonthlyReport = new BondedGoodsMonthlyReport();
			}

			public ZString DocumentCode => Message.NodeValueIsEmpty ? "" : "DocumentCode";
			public ZString Refundable => "Refundable";
			public IEnumerable<IBondedParty> BondedFactories => new List<IBondedParty>() { new BondedParty(), new BondedParty() };
			public IEnumerable<IBondedGoodsInvoice> BondedGoodsInvoices => new List<IBondedGoodsInvoice>() { new BondedGoodsInvoice(), new BondedGoodsInvoice() };

			IBondedGoodsMonthlyReport IBondedGoods.BondedGoodsMonthlyReport => BondedGoodsMonthlyReport;
			internal readonly BondedGoodsMonthlyReport BondedGoodsMonthlyReport;

			public IBondedParty InBondedParty => new BondedParty();
			public IBondedParty OutBondedParty => new BondedParty();
			public ZString AddDutyReasonCode => null;
			public IEnumerable<IBondedParty> PreBondedParties => null;
		}

		class BondedGoodsInvoice : IBondedGoodsInvoice
		{
			public ZString ID => "BondedGoodsInvoiceID";
			public ZDecimal ValueAmount => 1;
		}

		internal class BondedGoodsMonthlyReport : IBondedGoodsMonthlyReport
		{
			ZInt IBondedGoodsMonthlyReport.MonthNumeric => MonthNumeric;
			internal ZInt MonthNumeric = Message.NodeValueIsEmpty ? 0 : 4;

			ZString IBondedGoodsMonthlyReport.TraderReferenceID => TraderReferenceID;
			internal ZString TraderReferenceID = Message.NodeValueIsEmpty ? "" : "TraderReferenceID";
		}

		class BondedParty : IBondedParty
		{
			public ZString BondedID => "BondedID";
			public ZString ID => "BondedPartyID";
			public ZString TypeCode => "TypeCode";
			public ZString CustomsControlID => "CustomsControlID";
		}

		class Consignor : IPartyDetails
		{
			public ZString ID => "ConsignorID";
			public ZString Name => "ConsignorName";
			public ZString ChineseName => "ConsignorChineseName";
			public ZString TypeCode => Message.NodeValueIsEmpty ? "" : "ConsignorTypeCode";
			public IAddress Address => new Address();
			public ZString CustomsControlID => "CustomsControlID";
			public ZString PaymentOnAccountBusinessID => null;
			public ZString RoleCode => null;
			public ZString SubBoxID => null;
			public ILPCOAuthorizedParty LPCOAuthorizedParty => new LPCOAuthorizedParty();
			public IEnumerable<ICommunication> Communications => null;
			public ZString ContactName => null;
			public ZString MainManufacturer => null;
			public ZString UndertakeCode => null;
			public ZString OwnerName => null;
			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;
		}

		public class CustomsValuation : ICustomsValuation
		{
			ZDecimal ICustomsValuation.ExitToEntryChargeAmount => ExitToEntryChargeAmount;
			internal ZDecimal ExitToEntryChargeAmount = Message.NodeValueIsEmpty ? 0 : 15.333;

			ZDecimal ICustomsValuation.FreightChargeAmount => FreightChargeAmount;
			internal ZDecimal FreightChargeAmount = Message.NodeValueIsEmpty ? 0 : 16.333;

			ZDecimal ICustomsValuation.OtherChargeAmount => OtherChargeAmount;
			internal ZDecimal OtherChargeAmount = Message.NodeValueIsEmpty ? 0 : 17.333;

			ZDecimal ICustomsValuation.OtherDeductionAmount => OtherDeductionAmount;
			internal ZDecimal OtherDeductionAmount = Message.NodeValueIsEmpty ? 0 : 18.333;

			public ZDecimal OtherChargeDeductionAmount => -1.3;
			public ZString PartyRelationshipCode => null;

			public ZDecimal InvoiceAmount => ZDecimal.Zero;
			public ZDecimal TotalDutyTaxFeeAmount => ZDecimal.Zero;
		}

		internal class GovernmentAgencyGoodsItem : IGovernmentAgencyGoodsItem
		{
			public GovernmentAgencyGoodsItem()
			{
				Commodity = new Commodity();
				PreviousDocument = new PreviousDocument();
				GoodsStatisticalMeasure = new GoodsStatisticalMeasure();
			}
			public ZInt SequenceNumeric => 5;
			public IEnumerable<IAdditionalDocument> AdditionalDocuments => new List<IAdditionalDocument>() { new AdditionalDocument(), new AdditionalDocument() };
			public IEnumerable<IAdditionalInformation> AdditionalInformations => new List<IAdditionalInformation>() { new GovernmentAgencyGoodsItemAdditionalInformation(), new GovernmentAgencyGoodsItemAdditionalInformation() };

			ICommodity IGovernmentAgencyGoodsItem.Commodity => Commodity;
			internal readonly Commodity Commodity;

			public IGoodsMeasure GoodsMeasure => new GoodsMeasure();
			public IGovernmentProcedure GovernmentProcedure => new GovernmentProcedure();
			public IPartyDetails Manufacturer => new Manufacturer();
			public IOrigin Origin => new Origin();
			public IPackaging Packaging => new Packaging();

			IPreviousDocument IGovernmentAgencyGoodsItem.PreviousDocument => PreviousDocument;
			internal PreviousDocument PreviousDocument;

			IGoodsStatisticalMeasure IGovernmentAgencyGoodsItem.GoodsStatisticalMeasure => GoodsStatisticalMeasure;
			internal GoodsStatisticalMeasure GoodsStatisticalMeasure;

			public IPreviousDocument PreBondedDocument => new PreBondedDocument();
			public ILPCODetail ApprovalDocument => null;
			public ICommoditySpecification CommoditySpecification => null;
			public IGoodsLicensingStatisticalMeasure GoodsLicensingStatisticalMeasure => null;
			public ILPCODetail MedicalInstrument => null;
			public IEnumerable<IShippingIdentification> ShippingIdentifications => null;
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

		internal class Commodity : ICommodity
		{
			public Commodity()
			{
				InvoiceLine = new InvoiceLine();
				DutyOtherTaxFees = new List<DutyOtherTaxFee>() { new DutyOtherTaxFee(), new DutyOtherTaxFee() };
				Name = "Name";
				Description = new string('A', 509) + "512HereOverMaxlength";
				CommercialCategorizationID = Message.NodeValueIsEmpty ? "" : "CommercialCategorizationID";
				Constituent = new Constituent();
			}
			public ZString CommercialCategorizationID { get; set; }
			public ZString Description { get; set; }
			public ZString Name { get; set; }
			public ZString BondedNoteCode => Message.NodeValueIsEmpty ? "" : "BondedNoteCode";
			public IEnumerable<IAdditionalDocument> AdditionalDocuments => new List<IAdditionalDocument>() { new AdditionalDocument(), new AdditionalDocument() };
			public IEnumerable<IClassification> Classifications => new List<IClassification>() { new Classification(), new Classification() };
			public IConstituent Constituent { get; set; }
			IInvoiceLine ICommodity.InvoiceLine => InvoiceLine;
			internal readonly InvoiceLine InvoiceLine;
			public IEnumerable<ICommodityNumber> CommodityNumbers => new List<ICommodityNumber>() { new CommodityNumber(), new CommodityNumber() };

			IEnumerable<IDutyOtherTaxFee> ICommodity.DutyOtherTaxFees => DutyOtherTaxFees;
			internal readonly IEnumerable<DutyOtherTaxFee> DutyOtherTaxFees;

			public IEnumerable<ZString> VehicleIDs => new List<ZString>() { "VehicleID", "VehicleID" };
			public ZString GoodsGroupNameCode => null;
			public ZString BarCode => null;
			public ZString ChineseDescription => null;
			public ZString EnglishDescription => null;
			public ZString CITESImportPermitID => null;
			public ZString FTATariffCode => null;
			public ZString SHTCImportPermitID => null;
			public ZString TariffCodeExtensionCode => null;
			public ICommodityRelatedPackaging CommodityRelatedPackaging => null;
			public ICommodityDutyTaxFee DutyTaxFee => null;
			public IGovernmentProcedure GovernmentProcedure => null;
			public IEnumerable<ZString> HandlingInstructionsCodes => null;
			public IPreviousDocument PreviousDocument => null;
			public IDutyTaxFeeAmount DutyTaxFeeAmount => null;
			public IDutyTaxFeeQuantity DutyTaxFeeQuantity => null;
			public IFood Food => null;
			public IQuarantine Quarantine => null;
			public IVehicle Vehicle => null;
			public IWine Wine => null;
			public ZString CargoDescription => null;

			IClassification ICommodity.Classification => null;

			public IInvoice Invoice => null;

			public ZString PrintingTariffCode => null;
		}

		class Classification : IClassification
		{
			public ZString ID => "ClassificationID";
			public ZString IdentificationTypeCode => "IdentificationTypeCode";
		}

		class Constituent : IConstituent
		{
			public Constituent()
			{
				ElementDescription = Message.NodeValueIsEmpty ? "" : "ElementDescription";
			}

			public ZString ElementDescription { get; set; }
			public ZString LevelID => null;
			public ZString Thickness => null;
		}

		internal class InvoiceLine : IInvoiceLine
		{
			ZDecimal IInvoiceLine.ItemChargeAmount => ItemChargeAmount;
			internal ZDecimal ItemChargeAmount = 1;
			public ZDecimal UnitPriceAmount => 1;
			public ZString ChargesTypeCode => null;
			public ZString CurrencyTypeCode => null;
			public ZDecimal SubTotalAmount => ZDecimal.Zero;
		}

		class CommodityNumber : ICommodityNumber
		{
			public ZString ID => "CommodityNumberID";
			public ZString IdentifierTypeCode => "IdentifierTypeCode";
		}

		public class DutyOtherTaxFee : IDutyOtherTaxFee
		{
			ZDecimal IDutyOtherTaxFee.TaxRateNumeric => TaxRateNumeric;
			internal ZDecimal TaxRateNumeric = Message.NodeValueIsEmpty ? 0 : 19.333333;

			ZString IDutyOtherTaxFee.TypeCode => TypeCode;
			internal ZString TypeCode = Message.NodeValueIsEmpty ? "" : "TypeCode";

			public ZString MethodCode => null;

			public ZString MethodOfCalculation => null;

			public ZDecimal PercentageNumeric => ZDecimal.Zero;
		}

		class GoodsMeasure : IGoodsMeasure
		{
			public ZDecimal NetWeightMeasure => 20.3333333;
			public ZDecimal TariffQuantity => 21.33333;
			public ZString UnitCode => "UnitCode";
			ZString IGoodsMeasure.CustomUnitCode => ZString.Empty;
		}

		class GovernmentProcedure : IGovernmentProcedure
		{
			public ZString CurrentCode => "CurrentCode";
			public ZString TransportTypeCode => null;
			public ZString Description => null;
		}

		class Manufacturer : IPartyDetails
		{
			public ZString ID => Message.NodeValueIsEmpty ? "" : "ManufacturerID";
			public ZString Name => Message.NodeValueIsEmpty ? "" : "ManufacturerName";
			public ZString TypeCode => Message.NodeValueIsEmpty ? "" : "ManufacturerTypeCode";
			public IAddress Address => null;
			public ZString ContactName => null;
			public ZString ChineseName => null;
			public ZString CustomsControlID => null;
			public ZString PaymentOnAccountBusinessID => null;
			public ZString RoleCode => null;
			public ZString SubBoxID => null;
			public ILPCOAuthorizedParty LPCOAuthorizedParty => null;
			public IEnumerable<ICommunication> Communications => null;
			public ZString MainManufacturer => null;
			public ZString UndertakeCode => null;
			public ZString OwnerName => null;
			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;
		}

		class Origin : IOrigin
		{
			public ZString CountryCode => Message.NodeValueIsEmpty ? "" : "CountryCode";
			public IAdditionalDocument AdditionalDocument => new AdditionalDocument();
		}

		class Packaging : IPackaging
		{
			public ZDecimal QuantityQuantity => 8m;
			public ZString TypeCode => "TypeCode";
			public ZString MarksNumbers => null;
			public ZString PackagingMaterialDescription => null;
			public ZString Combination => null;

			ZDate IPackaging.PackingDateTime => ZDate.Empty;
		}

		public class PreviousDocument : IPreviousDocument
		{
			ZString IPreviousDocument.ID => ID;
			internal ZString ID = Message.NodeValueIsEmpty ? "" : "PreviousDocumentID";

			ZInt IPreviousDocument.LineNumeric => LineNumeric;
			internal ZInt LineNumeric = Message.NodeValueIsEmpty ? 0 : 9;

			public ZString FunctionalReferenceID => null;
		}

		internal class GoodsStatisticalMeasure : IGoodsStatisticalMeasure
		{
			ZString IGoodsStatisticalMeasure.StatisticalUnitCode => StatisticalUnitCode;
			internal ZString StatisticalUnitCode = Message.NodeValueIsEmpty ? "" : "StatisticalUnitCode";

			ZDecimal IGoodsStatisticalMeasure.TariffQuantity => TariffQuantity;
			internal ZDecimal TariffQuantity = Message.NodeValueIsEmpty ? 0 : 22.33333;
		}

		class PreBondedDocument : IPreviousDocument
		{
			public ZString ID => "PreBondedDocumentID";
			public ZInt LineNumeric => 10;
			public ZString FunctionalReferenceID => null;
		}

		class DeclarationPackaging : IDeclarationPackaging
		{
			public DeclarationPackaging()
			{
				MarksNumbers = "MarksNumbers";
			}

			public ZString MarksNumbers { get; set; }
			public ZString PackagingMaterialDescription => Message.NodeValueIsEmpty ? "" : "PackagingMaterialDescription";
			public ZString Combination => Message.NodeValueIsEmpty ? "" : "Combination";
			public ZString TypeCode => "TypeCode";
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
			var declaration = new N5203MessageBuilder().PopulateDeclaration(message, MessageFunctionCode.Replace);
			NUnit.Framework.Assert.That(declaration.FunctionCode.Value, NUnit.Framework.Is.EqualTo("5"), "FunctionCode is 5");

			var xml = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			AssertXMLContains("<FunctionCode>5</FunctionCode>", xml);
			AssertXMLContains(GetExpectedMessageXML("N5203_Replace.xml"), xml);
			CombineAssertions(() =>
			{
				AsserContainsXmlValueByType(xml, message, typeof(Message));
				AsserContainsXmlByType(xml, declaration.GetType());
			});

			declaration = new N5203MessageBuilder().PopulateDeclaration(message, MessageFunctionCode.Add);
			NUnit.Framework.Assert.That(declaration.FunctionCode.Value, NUnit.Framework.Is.EqualTo("9"), "FunctionCode is 9");

			xml = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			AssertXMLContains("<FunctionCode>9</FunctionCode>", xml);
			AssertXMLContains(GetExpectedMessageXML("N5203_Add.xml"), xml);
			CombineAssertions(() =>
			{
				AsserContainsXmlValueByType(xml, message, typeof(Message));
				AsserContainsXmlByType(xml, declaration.GetType());
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateXml()
		{
			var message = new Message();
			var xml = new N5203MessageBuilder().PopulateXml(message, MessageFunctionCode.Add);
			AssertXMLContains("<FunctionCode>9</FunctionCode>", xml);
			AssertXMLContains(GetExpectedMessageXML("N5203_Add.xml"), xml);
			AsserContainsXmlValueByType(xml, message, typeof(Message));
		}

		[ExpectNoExceptions]
		public void TestDeclarationPackagingMarksNumbersMaxLength()
		{
			var message = new Message();
			var packaging = message.Packaging as DeclarationPackaging;
			packaging.MarksNumbers = new ZString('A', 600);
			var declaration = new N5203MessageBuilder().PopulateDeclaration(message, MessageFunctionCode.Add);
			NUnit.Framework.Assert.That(declaration.Packaging.MarksNumbers.Value, NUnit.Framework.Is.EqualTo(new string('A', 512)));
		}

		[ExpectNoExceptions]
		public void TestPopulateBuyerLPCOAuthorizedParty()
		{
			var message = new Message();
			var buyer = message.GoodsShipment.Buyer as PartyDetails;
			buyer.ID = "125544";
			var address = buyer.Address as Address;
			address.CountryCode = "US";
			var xmlDocumentInformation = GetXmlDocumentInformation(message);
			var xmlDocument = xmlDocumentInformation.Item1;
			var nameSpace = xmlDocumentInformation.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Buyer/a:LPCOAuthorizedParty", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)), "The LPCOAuthorizedParty node is empty - should be [null]");

			address.CountryCode = "TW";
			xmlDocumentInformation = GetXmlDocumentInformation(message);
			xmlDocument = xmlDocumentInformation.Item1;
			nameSpace = xmlDocumentInformation.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Buyer/a:LPCOAuthorizedParty", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "The LPCOAuthorizedParty node isn't empty - should not be [null]");
		}

		protected override ITWMessageBuilder CreateNewMessageBuilder()
		{
			return new N5203MessageBuilder();
		}

		protected override IN5203Declaration CreateDataSource()
		{
			return new Message();
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclarationIfNodeValueIsEmpty()
		{
			TestPopulateDeclarationWhetherNodeValueIsEmpty(true);
			TestPopulateDeclarationWhetherNodeValueIsEmpty(false);
		}

		[ExpectNoExceptions]
		void TestPopulateDeclarationWhetherNodeValueIsEmpty(bool nodeValueIsEmpty)
		{
			Message.NodeValueIsEmpty = nodeValueIsEmpty;
			var message = new Message();
			var declaration = new N5203MessageBuilder().PopulateDeclaration(message, MessageFunctionCode.Replace);
			var xml = XmlHelper.Serializer(typeof(Declaration), declaration);
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);

			var namespacePrefix = "a";
			var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"].Value);
			foreach (var nodeName in NodeNames)
			{
				if (nodeValueIsEmpty)
				{
					NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode(nodeName, nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)), "$The {nodeName} node is empty - should be [null]");
					NUnit.Framework.Assert.That(xmlDocument.SelectNodes("a:Declaration/a:GoodsShipment/a:Consignment/a:GoodsLocation/a:ID", nameSpace).Count, NUnit.Framework.Is.EqualTo(1), "Don't need to output the node Declaration.GoodsShipment.Consignment.GoodsLocation.ID when the second repeat is empty");
				}
				else
				{
					NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode(nodeName, nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "$The {nodeName} node is not empty - should not be [null]");
					NUnit.Framework.Assert.That(xmlDocument.SelectNodes("a:Declaration/a:GoodsShipment/a:Consignment/a:GoodsLocation/a:ID", nameSpace).Count, NUnit.Framework.Is.EqualTo(2), "Don't need to output the node Declaration.GoodsShipment.Consignment.GoodsLocation.ID when the second repeat is not empty");
				}
			}
		}

		IEnumerable<ZString> NodeNames
		{
			get
			{
				yield return "a:Declaration/a:Authentication";
				yield return "a:Declaration/a:tw_AssociatedGovernmentProcedureCode";
				yield return "a:Declaration/a:Agent/a:LPCOAuthorizedParty";
				yield return "a:Declaration/a:Agent/a:LPCOAuthorizedParty/a:ID";
				yield return "a:Declaration/a:DutyTaxFee";
				yield return "a:Declaration/a:DutyTaxFee/a:tw_DutyMethodCode";
				yield return "a:Declaration/a:GoodsShipment/a:Buyer/a:ID";
				yield return "a:Declaration/a:GoodsShipment/a:Buyer/a:tw_CustomsControlID";
				yield return "a:Declaration/a:GoodsShipment/a:Buyer/a:tw_TypeCode";
				yield return "a:Declaration/a:GoodsShipment/a:Buyer/a:Address/a:Line";
				yield return "a:Declaration/a:GoodsShipment/a:Buyer/a:LPCOAuthorizedParty";
				yield return "a:Declaration/a:GoodsShipment/a:Buyer/a:LPCOAuthorizedParty/a:ID";
				yield return "a:Declaration/a:GoodsShipment/a:Consignment/a:tw_ShippingOrderNumber";
				yield return "a:Declaration/a:GoodsShipment/a:Consignment/a:BorderTransportMeans/a:tw_CallSignID";
				yield return "a:Declaration/a:GoodsShipment/a:Consignment/a:BorderTransportMeans/a:tw_Registration";
				yield return "a:Declaration/a:GoodsShipment/a:Consignment/a:DepartureTransportMeans/a:ID";
				yield return "a:Declaration/a:GoodsShipment/a:Consignment/a:DepartureTransportMeans/a:Name";
				yield return "a:Declaration/a:GoodsShipment/a:Consignment/a:LoadingLocation";
				yield return "a:Declaration/a:GoodsShipment/a:Consignment/a:LoadingLocation/a:ID";
				yield return "a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_DocumentCode";
				yield return "a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport";
				yield return "a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_MonthNumeric";
				yield return "a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_TraderReferenceID";
				yield return "a:Declaration/a:GoodsShipment/a:CustomsValuation";
				yield return "a:Declaration/a:GoodsShipment/a:CustomsValuation/a:ExitToEntryChargeAmount";
				yield return "a:Declaration/a:GoodsShipment/a:CustomsValuation/a:FreightChargeAmount";
				yield return "a:Declaration/a:GoodsShipment/a:CustomsValuation/a:tw_OtherChargeAmount";
				yield return "a:Declaration/a:GoodsShipment/a:CustomsValuation/a:tw_OtherDeductionAmount";
				yield return "a:Declaration/a:GoodsShipment/a:Exporter/a:tw_CustomsControlID";
				yield return "a:Declaration/a:GoodsShipment/a:Exporter/a:tw_PaymentOnAccountBusinessID";
				yield return "a:Declaration/a:GoodsShipment/a:Exporter/a:Address";
				yield return "a:Declaration/a:GoodsShipment/a:Exporter/a:Address/a:Line";
				yield return "a:Declaration/a:GoodsShipment/a:Exporter/a:LPCOAuthorizedParty";
				yield return "a:Declaration/a:GoodsShipment/a:Exporter/a:LPCOAuthorizedParty/a:ID";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:CommercialCategorizationID";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_BondedNoteCode";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:Constituent";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:Constituent/a:ElementDescription";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_DutyOtherTaxFee";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_DutyOtherTaxFee/a:tw_TaxRateNumeric";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_DutyOtherTaxFee/a:tw_TypeCode";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Manufacturer";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Manufacturer/a:ID";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Manufacturer/a:Name";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Manufacturer/a:tw_TypeCode";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Origin";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Origin/a:CountryCode";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Origin/a:AdditionalDocument";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Origin/a:AdditionalDocument/a:ID";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Origin/a:AdditionalDocument/a:tw_SequenceNumeric";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:PreviousDocument";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:PreviousDocument/a:ID";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:PreviousDocument/a:LineNumeric";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:tw_GoodsStatisticalMeasure";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:tw_GoodsStatisticalMeasure/a:tw_StatisticalUnitCode";
				yield return "a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:tw_GoodsStatisticalMeasure/a:tw_TariffQuantity";
				yield return "a:Declaration/a:GoodsShipment/a:UCR";
				yield return "a:Declaration/a:GoodsShipment/a:UCR/a:ID";
				yield return "a:Declaration/a:GovernmentProcedure";
				yield return "a:Declaration/a:GovernmentProcedure/a:Description";
				yield return "a:Declaration/a:Packaging/a:PackagingMaterialDescription";
				yield return "a:Declaration/a:Packaging/a:tw_Combination";
				yield return "a:Declaration/a:GoodsShipment/a:Consignee/a:tw_TypeCode";
				yield return "a:Declaration/a:GoodsShipment/a:Consignor/a:tw_TypeCode";
			}
		}

		[ExpectNoExceptions]
		public void TestNodeForDeclaration_GoodsShipment_Consignment_tw_BondedGoods_tw_BondedGoodsMonthlyReport()
		{
			var message = new Message();
			var bondedGoodsMonthlyReport = message.GoodsShipment.Consignment.BondedGoods.BondedGoodsMonthlyReport;
			bondedGoodsMonthlyReport.MonthNumeric = 1;
			bondedGoodsMonthlyReport.TraderReferenceID = "TraderReferenceID";
			var xmlDocumentInformation = GetXmlDocumentInformation(message);
			var xmlDocument = xmlDocumentInformation.Item1;
			var nameSpace = xmlDocumentInformation.Item2;

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_MonthNumeric", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_TraderReferenceID", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_MonthNumeric", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("1"));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_TraderReferenceID", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("TraderReferenceID"));

			bondedGoodsMonthlyReport.MonthNumeric = 2;
			bondedGoodsMonthlyReport.TraderReferenceID = ZString.Empty;
			xmlDocumentInformation = GetXmlDocumentInformation(message);
			xmlDocument = xmlDocumentInformation.Item1;
			nameSpace = xmlDocumentInformation.Item2;

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_MonthNumeric", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_TraderReferenceID", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_MonthNumeric", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("2"));

			bondedGoodsMonthlyReport.MonthNumeric = 0;
			bondedGoodsMonthlyReport.TraderReferenceID = ZString.Empty;
			xmlDocumentInformation = GetXmlDocumentInformation(message);
			xmlDocument = xmlDocumentInformation.Item1;
			nameSpace = xmlDocumentInformation.Item2;

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_MonthNumeric", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:Consignment/a:tw_BondedGoods/a:tw_BondedGoodsMonthlyReport/a:tw_TraderReferenceID", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
		}

		[ExpectNoExceptions]
		public void TestNodeForDeclaration_GoodsShipment_CustomsValuation()
		{
			var message = new Message();
			var customsValuation = message.GoodsShipment.CustomsValuation;
			customsValuation.ExitToEntryChargeAmount = 1;
			customsValuation.FreightChargeAmount = 2;
			customsValuation.OtherChargeAmount = 3;
			customsValuation.OtherDeductionAmount = 4;
			var xmlDocumentInformation = GetXmlDocumentInformation(message);
			var xmlDocument = xmlDocumentInformation.Item1;
			var nameSpace = xmlDocumentInformation.Item2;

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:ExitToEntryChargeAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:FreightChargeAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:tw_OtherChargeAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:tw_OtherDeductionAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:ExitToEntryChargeAmount", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("1"));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:FreightChargeAmount", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("2"));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:tw_OtherChargeAmount", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("3"));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:tw_OtherDeductionAmount", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("4"));

			customsValuation.ExitToEntryChargeAmount = 0;
			customsValuation.FreightChargeAmount = 2;
			customsValuation.OtherChargeAmount = 3;
			customsValuation.OtherDeductionAmount = 4;
			xmlDocumentInformation = GetXmlDocumentInformation(message);
			xmlDocument = xmlDocumentInformation.Item1;
			nameSpace = xmlDocumentInformation.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:ExitToEntryChargeAmount", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:FreightChargeAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:tw_OtherChargeAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:tw_OtherDeductionAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));

			customsValuation.ExitToEntryChargeAmount = 1;
			customsValuation.FreightChargeAmount = 0;
			customsValuation.OtherChargeAmount = 3;
			customsValuation.OtherDeductionAmount = 4;
			xmlDocumentInformation = GetXmlDocumentInformation(message);
			xmlDocument = xmlDocumentInformation.Item1;
			nameSpace = xmlDocumentInformation.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:ExitToEntryChargeAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:FreightChargeAmount", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:tw_OtherChargeAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:tw_OtherDeductionAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));

			customsValuation.ExitToEntryChargeAmount = 1;
			customsValuation.FreightChargeAmount = 2;
			customsValuation.OtherChargeAmount = 0;
			customsValuation.OtherDeductionAmount = 4;
			xmlDocumentInformation = GetXmlDocumentInformation(message);
			xmlDocument = xmlDocumentInformation.Item1;
			nameSpace = xmlDocumentInformation.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:ExitToEntryChargeAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:FreightChargeAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:tw_OtherChargeAmount", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:tw_OtherDeductionAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));

			customsValuation.ExitToEntryChargeAmount = 1;
			customsValuation.FreightChargeAmount = 2;
			customsValuation.OtherChargeAmount = 3;
			customsValuation.OtherDeductionAmount = 0;
			xmlDocumentInformation = GetXmlDocumentInformation(message);
			xmlDocument = xmlDocumentInformation.Item1;
			nameSpace = xmlDocumentInformation.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:ExitToEntryChargeAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:FreightChargeAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:tw_OtherChargeAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:tw_OtherDeductionAmount", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));

			customsValuation.ExitToEntryChargeAmount = 0;
			customsValuation.FreightChargeAmount = 0;
			customsValuation.OtherChargeAmount = 0;
			customsValuation.OtherDeductionAmount = 0;
			xmlDocumentInformation = GetXmlDocumentInformation(message);
			xmlDocument = xmlDocumentInformation.Item1;
			nameSpace = xmlDocumentInformation.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:ExitToEntryChargeAmount", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:FreightChargeAmount", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:tw_OtherChargeAmount", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:CustomsValuation/a:tw_OtherDeductionAmount", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
		}

		[ExpectNoExceptions]
		public void TestNodeForDeclaration_GoodsShipment_GovernmentAgencyGoodsItem_Commodity_tw_DutyOtherTaxFee()
		{
			var message = new Message();
			var governmentAgencyGoodsItems = message.GoodsShipment.GovernmentAgencyGoodsItems.ToList();
			governmentAgencyGoodsItems.ForEach(x => { x.Commodity.DutyOtherTaxFees.ToList().ForEach(y => { y.TaxRateNumeric = 1.3333333; y.TypeCode = "TypeCode"; }); });
			var xmlDocumentInformation = GetXmlDocumentInformation(message);
			var xmlDocument = xmlDocumentInformation.Item1;
			var nameSpace = xmlDocumentInformation.Item2;

			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_DutyOtherTaxFee", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_DutyOtherTaxFee/a:tw_TaxRateNumeric", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_DutyOtherTaxFee/a:tw_TypeCode", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_DutyOtherTaxFee/a:tw_TaxRateNumeric", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("1.33333"));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_DutyOtherTaxFee/a:tw_TypeCode", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("TypeCode"));

			governmentAgencyGoodsItems.ForEach(x => { x.Commodity.DutyOtherTaxFees.ToList().ForEach(y => { y.TaxRateNumeric = 0; y.TypeCode = ZString.Empty; }); });
			xmlDocumentInformation = GetXmlDocumentInformation(message);
			xmlDocument = xmlDocumentInformation.Item1;
			nameSpace = xmlDocumentInformation.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_DutyOtherTaxFee", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_DutyOtherTaxFee/a:tw_TaxRateNumeric", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:tw_DutyOtherTaxFee/a:tw_TypeCode", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
		}

		[ExpectNoExceptions]
		public void TestNodeForDeclaration_GoodsShipment_GovernmentAgencyGoodsItem_PreviousDocument()
		{
			var message = new Message();
			var governmentAgencyGoodsItems = message.GoodsShipment.GovernmentAgencyGoodsItems.ToList();
			governmentAgencyGoodsItems.ForEach(x => { x.PreviousDocument.ID = "ID"; x.PreviousDocument.LineNumeric = 1; });
			var xmlDocumentInformation = GetXmlDocumentInformation(message);
			var xmlDocument = xmlDocumentInformation.Item1;
			var nameSpace = xmlDocumentInformation.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:PreviousDocument", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:PreviousDocument/a:ID", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:PreviousDocument/a:LineNumeric", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:PreviousDocument/a:ID", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("ID"));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:PreviousDocument/a:LineNumeric", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("1"));

			governmentAgencyGoodsItems.ForEach(x => { x.PreviousDocument.ID = ZString.Empty; x.PreviousDocument.LineNumeric = 0; });
			xmlDocumentInformation = GetXmlDocumentInformation(message);
			xmlDocument = xmlDocumentInformation.Item1;
			nameSpace = xmlDocumentInformation.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:PreviousDocument", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:PreviousDocument/a:ID", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:PreviousDocument/a:LineNumeric", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
		}

		[ExpectNoExceptions]
		public void TestNodeForDeclaration_GoodsShipment_GovernmentAgencyGoodsItem_tw_GoodsStatisticalMeasure()
		{
			var message = new Message();
			var governmentAgencyGoodsItems = message.GoodsShipment.GovernmentAgencyGoodsItems.ToList();
			governmentAgencyGoodsItems.ForEach(x => { x.GoodsStatisticalMeasure.StatisticalUnitCode = "Code"; x.GoodsStatisticalMeasure.TariffQuantity = 1; });
			var xmlDocumentInformation = GetXmlDocumentInformation(message);
			var xmlDocument = xmlDocumentInformation.Item1;
			var nameSpace = xmlDocumentInformation.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:tw_GoodsStatisticalMeasure", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:tw_GoodsStatisticalMeasure/a:tw_StatisticalUnitCode", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:tw_GoodsStatisticalMeasure/a:tw_TariffQuantity", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:tw_GoodsStatisticalMeasure/a:tw_StatisticalUnitCode", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("Code"));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:tw_GoodsStatisticalMeasure/a:tw_TariffQuantity", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("1"));

			governmentAgencyGoodsItems.ForEach(x => { x.GoodsStatisticalMeasure.StatisticalUnitCode = ""; x.GoodsStatisticalMeasure.TariffQuantity = 0; });
			xmlDocumentInformation = GetXmlDocumentInformation(message);
			xmlDocument = xmlDocumentInformation.Item1;
			nameSpace = xmlDocumentInformation.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:tw_GoodsStatisticalMeasure", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:tw_GoodsStatisticalMeasure/a:tw_StatisticalUnitCode", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:tw_GoodsStatisticalMeasure/a:tw_TariffQuantity", nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)));
		}

		Tuple<XmlDocument, XmlNamespaceManager> GetXmlDocumentInformation(Message message)
		{
			var messageString = new N5203MessageBuilder().SerializeToMessageString(message, MessageFunctionCode.Replace);
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(messageString);

			var namespacePrefix = "a";
			var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"].Value);
			return Tuple.Create(xmlDocument, nameSpace);
		}

		[ExpectNoExceptions]
		public void TestNodeForDeclaration_GoodsShipment_AdditionalDocument()
		{
			var message = new Message();
			var additionalDocuments = message.GoodsShipment.AdditionalDocuments.Cast<AdditionalDocument>().ToList();
			additionalDocuments.ForEach(x => { x.ID = "AdditionalDocumentID"; x.Content = "Content"; x.ImageFileFormat = "PDF"; x.ImageFileName = "FileName"; x.SizeMeasure = 1024; x.TypeCode = "TVC"; });
			var xmlDocumentInformation = GetXmlDocumentInformation(message);
			var xmlDocument = xmlDocumentInformation.Item1;
			var nameSpace = xmlDocumentInformation.Item2;

			CombineAssertions("Node of Declaration_GoodsShipment_AdditionalDocument values not null", () =>
			{
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:ID", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_Content", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_ImageFileFormat", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_ImageFileName", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_SizeMeasure", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:TypeCode", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));

				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:ID", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("AdditionalDocumentID"));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_Content", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("Content"));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_ImageFileFormat", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("PDF"));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_ImageFileName", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("FileName"));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:tw_SizeMeasure", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("1024"));
				NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:AdditionalDocument/a:TypeCode", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("TVC"));
			});

			additionalDocuments.ForEach(x => { x.ID = ZString.Empty; x.Content = ZString.Empty; x.ImageFileFormat = ZString.Empty; x.ImageFileName = ZString.Empty; x.SizeMeasure = 0; x.TypeCode = ZString.Empty; });
			xmlDocumentInformation = GetXmlDocumentInformation(message);
			xmlDocument = xmlDocumentInformation.Item1;
			nameSpace = xmlDocumentInformation.Item2;
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
		}

		[ExpectNoExceptions]
		public override void TestSerializeToMessageString()
		{
			var expectedString = TWMessageBuilder.SerializeToMessageString(Declaration, FunctionCode);
			if (!FunctionCode.IsEmpty)
			{
				AssertXMLContains(ZString.Format("<FunctionCode>{0}</FunctionCode>", FunctionCode), expectedString);
			}
			AsserContainsXmlValueByType(expectedString, Declaration, typeof(Message));
		}

		[ExpectNoExceptions]
		public void TestNodeForDeclaration_GoodsShipment_GovernmentAgencyGoodsItem_Commodity_InvoiceLine_ItemChargeAmount()
		{
			var message = new Message();
			var governmentAgencyGoodsItems = message.GoodsShipment.GovernmentAgencyGoodsItems.ToList();
			governmentAgencyGoodsItems.ForEach(x => x.Commodity.InvoiceLine.ItemChargeAmount = 1.45m);
			var xmlDocumentInformation = GetXmlDocumentInformation(message);
			var xmlDocument = xmlDocumentInformation.Item1;
			var nameSpace = xmlDocumentInformation.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:InvoiceLine", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:InvoiceLine/a:ItemChargeAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:InvoiceLine/a:ItemChargeAmount", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("1.45"));

			governmentAgencyGoodsItems.ForEach(x => x.Commodity.InvoiceLine.ItemChargeAmount = 1.000000000m);
			xmlDocumentInformation = GetXmlDocumentInformation(message);
			xmlDocument = xmlDocumentInformation.Item1;
			nameSpace = xmlDocumentInformation.Item2;
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:InvoiceLine", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:InvoiceLine/a:ItemChargeAmount", nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)));
			NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode("a:Declaration/a:GoodsShipment/a:GovernmentAgencyGoodsItem/a:Commodity/a:InvoiceLine/a:ItemChargeAmount", nameSpace).InnerText, NUnit.Framework.Is.EqualTo("1"));
		}

		#region MaxLength
		[ExpectNoExceptions]
		public void TestPopulateNotifyPartyMaxLength()
		{
			var obj = SetupPartyDetails();
			var messageBuiler = new N5203MessageBuilder();
			var goodsShipment = new DeclarationGoodsShipment();
			messageBuiler.PopulateNotifyParty(goodsShipment, obj);
			var notifyParty = goodsShipment.NotifyParty;
			NUnit.Framework.Assert.That(notifyParty.Name.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 80)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(notifyParty.TwChineseName.Value, NUnit.Framework.Is.EqualTo(new ZString('C', 70)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(notifyParty.Address.Line.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 120)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(notifyParty.Address.TwChineseLine.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 100)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateConsigneeMaxLength()
		{
			var obj = SetupPartyDetails();
			var messageBuiler = new N5203MessageBuilder();
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
			var messageBuiler = new N5203MessageBuilder();
			var goodsShipment = new DeclarationGoodsShipment();
			messageBuiler.PopulateConsignor(goodsShipment, obj);
			var consignor = goodsShipment.Consignor;

			NUnit.Framework.Assert.That(consignor.Name.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 80)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(consignor.TwChineseName.Value, NUnit.Framework.Is.EqualTo(new ZString('C', 70)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(consignor.Address.Line.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 120)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(consignor.Address.TwChineseLine.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 100)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateBuyerMaxLength()
		{
			var obj = SetupPartyDetails();
			var messageBuiler = new N5203MessageBuilder();
			var goodsShipment = new DeclarationGoodsShipment();
			messageBuiler.PopulateBuyer(goodsShipment, obj);
			var buyer = goodsShipment.Buyer;
			NUnit.Framework.Assert.That(buyer.Id.Value.Length, NUnit.Framework.Is.EqualTo(14));
			NUnit.Framework.Assert.That(buyer.Name.Value.Length, NUnit.Framework.Is.EqualTo(80));
			NUnit.Framework.Assert.That(buyer.TwCustomsControlId.Value.Length, NUnit.Framework.Is.EqualTo(8));
			NUnit.Framework.Assert.That(buyer.TwTypeCode.Value.Length, NUnit.Framework.Is.EqualTo(3));
			NUnit.Framework.Assert.That(buyer.Address.Line.Value.Length, NUnit.Framework.Is.EqualTo(120));
			NUnit.Framework.Assert.That(buyer.LpcoAuthorizedParty.Id.Value.Length, NUnit.Framework.Is.EqualTo(20));
		}

		[ExpectNoExceptions]
		public void TestPopulateExporterMaxLength()
		{
			var obj = SetupPartyDetails();
			var messageBuiler = new N5203MessageBuilder();
			var goodsShipment = new DeclarationGoodsShipment();
			messageBuiler.PopulateExporter(goodsShipment, obj);
			var exporter = goodsShipment.Exporter;
			NUnit.Framework.Assert.That(exporter.Id.Value.Length, NUnit.Framework.Is.EqualTo(14));
			NUnit.Framework.Assert.That(exporter.Name.Value.Length, NUnit.Framework.Is.EqualTo(80));
			NUnit.Framework.Assert.That(exporter.TwCustomsControlId.Value.Length, NUnit.Framework.Is.EqualTo(8));
			NUnit.Framework.Assert.That(exporter.TwPaymentOnAccountBusinessId.Value.Length, NUnit.Framework.Is.EqualTo(8));
			NUnit.Framework.Assert.That(exporter.TwTypeCode.Value.Length, NUnit.Framework.Is.EqualTo(3));
			NUnit.Framework.Assert.That(exporter.Address.Line.Value.Length, NUnit.Framework.Is.EqualTo(120));
			NUnit.Framework.Assert.That(exporter.LpcoAuthorizedParty.Id.Value.Length, NUnit.Framework.Is.EqualTo(20));
		}

		[ExpectNoExceptions]
		public void TestPopulateCommodityMaxLength()
		{
			var obj = SetupCommodityDetails();
			var messageBuiler = new N5203MessageBuilder();
			var governmentAgencyGoodsItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
			messageBuiler.PopulateCommodity(governmentAgencyGoodsItem, obj);
			var commodity = governmentAgencyGoodsItem.Commodity;

			NUnit.Framework.Assert.That(commodity.Name.Value, NUnit.Framework.Is.EqualTo(new ZString('A', 50)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodity.Description.Value, NUnit.Framework.Is.EqualTo(new ZString('B', 512)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodity.CommercialCategorizationId.Value, NUnit.Framework.Is.EqualTo(new ZString('C', 80)).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(commodity.Constituent.ElementDescription.Value, NUnit.Framework.Is.EqualTo(new ZString('D', 256)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateManufacturerMaxLength()
		{
			var obj = SetupPartyDetails();
			var messageBuiler = new N5203MessageBuilder();
			var governmentAgencyGoodsItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem();
			messageBuiler.PopulateManufacturer(governmentAgencyGoodsItem, obj);
			var manufacturer = governmentAgencyGoodsItem.Manufacturer;
			NUnit.Framework.Assert.That(manufacturer.Name.Value, NUnit.Framework.Is.EqualTo(new ZString('N', 70)).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclarationPackagingMaxLength()
		{
			var obj = new DeclarationPackaging();
			obj.MarksNumbers = new ZString('M', 1000);
			var messageBuiler = new N5203MessageBuilder();
			var declaration = new Declaration();
			messageBuiler.PopulateDeclarationPackaging(declaration, obj);
			var packaging = declaration.Packaging;
			NUnit.Framework.Assert.That(packaging.MarksNumbers.Value, NUnit.Framework.Is.EqualTo(new ZString('M', 512)).Using(CustomComparers.TypeComparison));
		}

		PartyDetails SetupPartyDetails()
		{
			var result = new PartyDetails();
			result.Name = new ZString('N', 200);
			result.ChineseName = new ZString('C', 200);
			result.ID = new ZString('Z', 20);
			result.CustomsControlID = new ZString('Z', 10);
			result.PaymentOnAccountBusinessID = new ZString('Z', 10);
			result.TypeCode = new ZString('Z', 10);
			var lPCOAuthorizedParty = new LPCOAuthorizedParty();
			lPCOAuthorizedParty.ID = new ZString('Z', 30);
			result.LPCOAuthorizedParty = lPCOAuthorizedParty;
			var address = new Address();
			address.ChineseLine = new ZString('N', 200);
			address.Line = new ZString('N', 200);
			result.Address = address;
			return result;
		}

		Commodity SetupCommodityDetails()
		{
			var result = new Commodity();
			result.Name = new ZString('A', 100);
			result.Description = new ZString('B', 1000);
			result.CommercialCategorizationID = new ZString('C', 100);

			var constituent = new Constituent();
			constituent.ElementDescription = new ZString('D', 1000);
			result.Constituent = constituent;
			return result;
		}
		#endregion
	}
}
