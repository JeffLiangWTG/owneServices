using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.N5167;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	sealed class N5167MessageBuilderTest : BaseTWMessageBuilderTest<IN5167Declaration, Declaration>
	{
		class Message : IN5167Declaration
		{
			public Message()
			{
				Agent = new PartyDetails();
				Consignment = new Consignment();
				GoodsShipment = new GoodsShipment();
				Importer = new PartyDetails();
			}

			public ZString DeclarationOfficeID => "DeclarationOfficeID";

			public ZString FunctionalReferenceID => "FunctionalReferenceID";

			IPartyDetails IN5167Declaration.Agent => Agent;

			IConsignment IN5167Declaration.Consignment => Consignment;

			IGoodsShipment IN5167Declaration.GoodsShipment => GoodsShipment;

			IPartyDetails IN5167Declaration.Importer => Importer;

			internal readonly IPartyDetails Agent;

			internal readonly IConsignment Consignment;

			internal readonly IGoodsShipment GoodsShipment;

			internal readonly IPartyDetails Importer;
		}

		class PartyDetails : IPartyDetails
		{
			public ZString ID => "PartyDetailID";

			public ZString RoleCode => "RoleCode";

			public ZString SubBoxID => "SubBoxID";

			public ZString Name => null;

			public ZString ChineseName => null;

			public ZString TypeCode => null;

			internal ZString CustomsControlID = "CustomsControlID";

			public ZString PaymentOnAccountBusinessID => null;

			public IAddress Address => null;

			public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

			public IEnumerable<ICommunication> Communications => null;

			public ZString ContactName => null;

			ZString IPartyDetails.CustomsControlID => CustomsControlID;

			public ZString MainManufacturer => null;

			public ZString UndertakeCode => null;

			public ZString OwnerName => null;

			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;
		}

		class Consignment : IConsignment
		{
			public ZString ManifestSerialNumber => null;

			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;

			public ZString ArrivalTransportMeansTypeCode => null;

			public ITransportMeans BorderTransportMeans => null;

			public IPartyDetails Carrier => null;

			public IConsignmentItem ConsignmentItem => null;

			public ZString GoodsLocation => null;

			public ILocation LoadingLocation => null;

			public IEnumerable<ITransportContractDocument> TransportContractDocuments => new List<ITransportContractDocument> { new TransportContractDocument() };

			public IEnumerable<ITransportEquipment> TransportEquipments => null;

			public IBondedGoods BondedGoods => null;

			public ZString ShippingOrderNumber => null;

			public ITransportMeans DepartureTransportMeans => null;

			public ZString TransitTransportMeansTypeCode => null;

			public IEnumerable<ZString> GoodsLocations => null;

			public ILocation UnloadingLocation => null;

			public IGovernmentAgencyGoodsItem GovernmentAgencyGoodsItem => null;

			public ILocation TranshipmentLocation => null;

			public ILocation TransitDeparture => null;
		}

		class TransportContractDocument : ITransportContractDocument
		{
			public IPartyDetails Deconsolidator => new PartyDetails();

			public ZString ID => null;

			public ZString TypeCode => null;
		}

		class GoodsShipment : IGoodsShipment
		{
			public GoodsShipment()
			{
				Consignment = new Consignment();
				GovernmentAgencyGoodsItems = new List<IGovernmentAgencyGoodsItem>() { new GovernmentAgencyGoodsItem(), new GovernmentAgencyGoodsItem() };
			}

			internal readonly IConsignment Consignment;

			internal readonly IEnumerable<IGovernmentAgencyGoodsItem> GovernmentAgencyGoodsItems;

			public IEnumerable<IAdditionalDocument> AdditionalDocuments => null;

			public ZDateTime ExitDateTime => ZDateTime.Empty;

			public ZDecimal ItemChargeAmount => -1;

			public ZDecimal TotalCIFAmount => -1;

			public IPartyDetails Consignee => null;

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

			IEnumerable<IGovernmentAgencyGoodsItem> IGoodsShipment.GovernmentAgencyGoodsItems => GovernmentAgencyGoodsItems;

			IConsignment IGoodsShipment.Consignment => Consignment;

			public IEnumerable<IGoodsMeasure> GoodsMeasures => null;

			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;

			public IEnumerable<IAdditionalDeclaration> AdditionalDeclarations => null;
		}

		class GovernmentAgencyGoodsItem : IGovernmentAgencyGoodsItem
		{
			internal ZDateTime ControlInspectionStartDateTime = new ZDateTime(2011, 05, 23, 9, 30, 1);

			internal ZString ExaminationPlace = "ExaminationPlace";

			public IEnumerable<ITransportEquipment> TransportEquipments => new List<ITransportEquipment>() { new TransportEquipment(), new TransportEquipment() };

			public IAdditionalDeclaration AdditionalDeclaration => new AdditionalDeclaration();

			public ZInt SequenceNumeric => 0;

			public IAdditionalDocument AdditionalDocument => null;

			public ICommodity Commodity => null;

			public IEnumerable<IAdditionalDocument> AdditionalDocuments => null;

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

			public ZString CriteriaCode => null;

			public ZString PreferentialCriteria => null;

			public ZString ProducerCode => null;

			public ZString OtherCriteria => null;

			ZDateTime IGovernmentAgencyGoodsItem.ControlInspectionStartDateTime => ControlInspectionStartDateTime;

			ZString IGovernmentAgencyGoodsItem.ExaminationPlace => ExaminationPlace;
		}

		class TransportEquipment : ITransportEquipment
		{
			public ZString CharacteristicCode => "CharacteristicCode";

			public ZString ID => "TransportEquipmentID";

			public ZString UsedCapacityCode => "UsedCapacityCode";

			public IEnumerable<ZString> Seals => new List<ZString>() { "SealID", "SealID" };
		}

		class AdditionalDeclaration : IAdditionalDeclaration
		{
			public ZString ID => "AdditionalDeclarationID";

			public ZString TypeCode => "TypeCode";

			public ZDecimal SequenceNumeric => ZDecimal.Zero;
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclaration()
		{
			var message = new Message();
			var declaration = new N5167MessageBuilder().PopulateDeclaration(message);
			NUnit.Framework.Assert.That(declaration.FunctionCode.Value, NUnit.Framework.Is.EqualTo("9"), "FunctionCode is 9");

			var xml = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			AssertXMLContains(GetExpectedMessageXML("N5167.xml"), xml);
			AssertXMLContains("<FunctionCode>9</FunctionCode>", xml);

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
			var xml = new N5167MessageBuilder().PopulateXml(message);
			AssertXMLContains(GetExpectedMessageXML("N5167.xml"), xml);
			AssertXMLContains("<FunctionCode>9</FunctionCode>", xml);
			AsserContainsXmlValueByType(xml, message, typeof(Message));
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclarationIfNodeValueIsEmpty()
		{
			var message = new Message();
			((PartyDetails)message.Importer).CustomsControlID = ZString.Empty;
			message.GoodsShipment.GovernmentAgencyGoodsItems.Cast<GovernmentAgencyGoodsItem>().ToList().ForEach(x => { x.ControlInspectionStartDateTime = ZDateTime.Empty; x.ExaminationPlace = ZString.Empty; });
			var xml = new N5167MessageBuilder().PopulateXml(message);
			AssertXMLContains(GetExpectedMessageXML("N5167_ValueEmpty.xml"), xml);
			AssertXMLContains("<FunctionCode>9</FunctionCode>", xml);
			AsserContainsXmlValueByType(xml, message, typeof(Message));
		}

		protected override ITWMessageBuilder CreateNewMessageBuilder()
		{
			return new N5167MessageBuilder();
		}

		protected override IN5167Declaration CreateDataSource()
		{
			return new Message();
		}
	}
}
