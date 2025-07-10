using System.Collections.Generic;
using CargoWise.Customs.TW.MessageDefinitions.NX5901;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	sealed class NX5901MessageBuilderTest : BaseTWMessageBuilderTest<INX5901Declaration, Declaration>
	{
		class Message : INX5901Declaration
		{
			public ZString FunctionalReferenceID => "FunctionalReferenceID";

			public ZString ID => "ID";

			public ZString TypeCode => "TypeCode";

			public IAdditionalDocument AdditionalDocument => new AdditionalDocument();

			public ZString ContactOffice => "ContactOffice";

			public IGoodsShipment GoodsShipment => new GoodsShipment();

			public IGovernmentProcedure GovernmentProcedure => new GovernmentProcedure();

			public IPreviousDocument PreviousDocument => new PreviousDocument();

			public ZString ResponsibleGovernmentAgency => "ResponsibleGovernmentAgency";
		}

		class AdditionalDocument : IAdditionalDocument
		{
			public ZString ID => "ID";

			public ZString Content => null;

			public ZString ImageFileFormat => null;

			public ZString ImageFileName => null;

			public ZInt SequenceNumeric => 0;

			public ZLong SizeMeasure => 0;

			public ZString TypeCode => null;

			public ZString ResponsibleGovernmentAgency => null;

			ZDate IAdditionalDocument.SlaughterDateTime => ZDate.Empty;
		}

		class GoodsShipment : IGoodsShipment
		{
			public IEnumerable<IAdditionalDocument> AdditionalDocuments => new List<IAdditionalDocument>() { new AdditionalDocument(), new AdditionalDocument() };

			public IEnumerable<IGovernmentAgencyGoodsItem> GovernmentAgencyGoodsItems => new List<IGovernmentAgencyGoodsItem>() { new GovernmentAgencyGoodsItem(), new GovernmentAgencyGoodsItem() };

			public ZDateTime ExitDateTime => ZDateTime.Empty;

			public ZDecimal ItemChargeAmount => -1;

			public ZDecimal TotalCIFAmount => -1;

			public IPartyDetails Consignee => null;

			public IConsignment Consignment => null;

			public IPartyDetails Consignor => null;

			public ICustomsValuation CustomsValuation => null;

			public ZString DeliveryDestinationName => null;

			public IEnumerable<IGoodsShipmentDutyTaxFee> DutyTaxFees => null;

			public IPartyDetails NotifyParty => null;

			public IPartyDetails Seller => null;

			public ZString TradeTermsConditionCode => null;

			public ZString UCR => null;

			public IPartyDetails Buyer => null;

			public IPartyDetails Exporter => null;

			public IEnumerable<IGoodsMeasure> GoodsMeasures => null;

			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;

			public IEnumerable<IAdditionalDeclaration> AdditionalDeclarations => null;

			IPartyDetails IGoodsShipment.Consignor => null;
		}

		class GovernmentAgencyGoodsItem : IGovernmentAgencyGoodsItem
		{
			public ZInt SequenceNumeric => 3;

			public ICommodity Commodity => new Commodity();

			public IEnumerable<IAdditionalDocument> AdditionalDocuments => new List<IAdditionalDocument> { new AdditionalDocument() };

			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;

			public IGoodsMeasure GoodsMeasure => null;

			public IPartyDetails Manufacturer => null;

			public IOrigin Origin => null;

			public IPackaging Packaging => null;

			public IPreviousDocument PreviousDocument => null;

			public ILPCODetail ApprovalDocument => null;

			public ICommoditySpecification CommoditySpecification => null;

			public IGoodsLicensingStatisticalMeasure GoodsLicensingStatisticalMeasure => null;

			public IGoodsStatisticalMeasure GoodsStatisticalMeasure => null;

			public ILPCODetail MedicalInstrument => null;

			public IPreviousDocument PreBondedDocument => null;

			public IEnumerable<IShippingIdentification> ShippingIdentifications => null;

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

		class Commodity : ICommodity
		{
			public IEnumerable<IAdditionalDocument> AdditionalDocuments => new List<IAdditionalDocument>() { new AdditionalDocument(), new AdditionalDocument() };

			public ZString CommercialCategorizationID => null;

			public ZString Description => null;

			public ZString GoodsGroupNameCode => null;

			public ZString Name => null;

			public ZString BarCode => null;

			public ZString ChineseDescription => null;

			public ZString EnglishDescription => null;

			public ZString CITESImportPermitID => null;

			public ZString FTATariffCode => null;

			public ZString SHTCImportPermitID => null;

			public ZString TariffCodeExtensionCode => null;

			public IEnumerable<IClassification> Classifications => null;

			public ICommodityRelatedPackaging CommodityRelatedPackaging => null;

			public IConstituent Constituent => null;

			public ICommodityDutyTaxFee DutyTaxFee => null;

			public IGovernmentProcedure GovernmentProcedure => null;

			public IEnumerable<ZString> HandlingInstructionsCodes => null;

			public IInvoiceLine InvoiceLine => null;

			public IPreviousDocument PreviousDocument => null;

			public IEnumerable<ICommodityNumber> CommodityNumbers => null;

			public IEnumerable<IDutyOtherTaxFee> DutyOtherTaxFees => null;

			public IDutyTaxFeeAmount DutyTaxFeeAmount => null;

			public IDutyTaxFeeQuantity DutyTaxFeeQuantity => null;

			public IFood Food => null;

			public IQuarantine Quarantine => null;

			public IVehicle Vehicle => null;

			public IWine Wine => null;

			public ZString CargoDescription => null;

			public ZString BondedNoteCode => null;

			public IEnumerable<ZString> VehicleIDs => null;

			public IInvoice Invoice => null;

			public ZString PrintingTariffCode => null;

			IClassification ICommodity.Classification => null;
		}

		class GovernmentProcedure : IGovernmentProcedure
		{
			public ZString TransportTypeCode => "TransportTypeCode";

			public ZString CurrentCode => null;

			public ZString Description => null;
		}

		class PreviousDocument : IPreviousDocument
		{
			public ZString FunctionalReferenceID => "FunctionalReferenceID";

			public ZString ID => null;

			public ZInt LineNumeric => 0;
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclaration()
		{
			var message = new Message();
			var declaration = new NX5901MessageBuilder().PopulateDeclaration(message);

			var xml = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			AssertXMLContains(GetExpectedMessageXML("NX5901.xml"), xml);
			CombineAssertions(() =>
			{
				AsserContainsXmlValueByType(xml, message, typeof(Message));
				AsserContainsXmlByType(xml, declaration.GetType());
			});
		}

		protected override ITWMessageBuilder CreateNewMessageBuilder()
		{
			return new NX5901MessageBuilder();
		}

		protected override INX5901Declaration CreateDataSource()
		{
			return new Message();
		}

		protected override ZString FunctionCode => ZString.Empty;
	}
}
