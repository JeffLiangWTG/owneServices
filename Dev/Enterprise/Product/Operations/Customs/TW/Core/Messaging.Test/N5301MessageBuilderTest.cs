using System.Collections.Generic;
using System.Xml;
using CargoWise.Customs.TW.MessageDefinitions.N5301;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	[TestedType(typeof(N5301MessageBuilder))]
	sealed class N5301MessageBuilderTest : MessageBuilderTest
	{
		public class Message : IN5301Declaration
		{
			public static bool NodeValueIsEmpty;
			public ZString ID => "ID";

			public ZDecimal TotalGrossMassMeasure => NodeValueIsEmpty ? 0 : 1;

			public ZInt TotalPackageQuantity => 1;

			public ZString TypeCode => "TypeCode";

			public IEnumerable<IAdditionalInformation> AdditionalInformations => new List<IAdditionalInformation>() { new AdditionalInformation(), new AdditionalInformation() };

			public IPartyDetails Agent => new PartyDetails();

			public ITransportMeans BorderTransportMeans => new BorderTransportMeans();

			public IPartyDetails Carrier => new PartyDetails();

			public IConsignment Consignment => new Consignment();

			public IPartyDetails Deconsolidator => new PartyDetails();

			public ZString LoadingLocation => "LoadingLocation";

			public ZString RepresentativePersonName => "RepresentativePerson";

			public IPartyDetails Applicant => new Applicant();

			public ZString UnloadingLocation => "UnloadingLocation";
		}

		class AdditionalInformation : IAdditionalInformation
		{
			public ZString StatementCode => "StatementCode";

			public ZString StatementDescription => "StatementDescription";

			public ZInt CopyQuantity => 0;

			public ZString ProcessNumber => null;

			public ZString Content => null;

			public ZString ApprovalID => null;

			public ZString DelProcessNumber => null;

			ZString IAdditionalInformation.PackingHouse => null;
		}

		class PartyDetails : IPartyDetails
		{
			public ZString ID => Message.NodeValueIsEmpty ? "" : "ID";

			public ZString RoleCode => "RoleCode";

			public ZString SubBoxID => "SubBoxID";

			public ZString Name => null;

			public ZString ChineseName => null;

			public ZString TypeCode => null;

			public ZString CustomsControlID => null;

			public ZString PaymentOnAccountBusinessID => null;

			public IAddress Address => null;

			public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

			public IEnumerable<ICommunication> Communications => null;

			public ZString ContactName => null;

			public ZString MainManufacturer => null;

			public ZString UndertakeCode => null;

			public ZString OwnerName => null;

			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;
		}

		class BorderTransportMeans : ITransportMeans
		{
			public ZDate ArrivalDateTime => Message.NodeValueIsEmpty ? ZDate.Empty : new ZDate(2011, 05, 23);

			public ZString ID => Message.NodeValueIsEmpty ? "" : "ID";

			public ZString JourneyID => Message.NodeValueIsEmpty ? "" : "JourneyID";

			public ZString Name => Message.NodeValueIsEmpty ? "" : "Name";

			public ZString Registration => Message.NodeValueIsEmpty ? "" : "Registration";

			public ZString TypeCode => "TypeCode";

			public IEnumerable<ZString> ItineraryRoutingCountryCodes => null;

			public ZString CallSignID => null;
		}

		class Consignment : IConsignment
		{
			public ZString ManifestSerialNumber => Message.NodeValueIsEmpty ? "" : "ManifestSerialNumber";

			public ZString ShippingOrderNumber => Message.NodeValueIsEmpty ? "" : "ShippingOrderNumber";

			public IConsignmentItem ConsignmentItem => new ConsignmentItem();

			public ITransportMeans DepartureTransportMeans => new DepartureTransportMeans();

			public ILocation LoadingLocation => new Location("LoadingLocation");

			public ZString TransitTransportMeansTypeCode => "TransitTransportMeans";

			public IEnumerable<ITransportContractDocument> TransportContractDocuments => new List<ITransportContractDocument>() { new TransportContractDocument(), new TransportContractDocument() };

			public IEnumerable<ITransportEquipment> TransportEquipments => new List<ITransportEquipment>() { new TransportEquipment(), new TransportEquipment() };

			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;

			public ZString ArrivalTransportMeansTypeCode => null;

			public ITransportMeans BorderTransportMeans => null;

			public IPartyDetails Carrier => null;

			public ZString GoodsLocation => null;

			public IBondedGoods BondedGoods => null;

			public IEnumerable<ZString> GoodsLocations => null;

			public ILocation UnloadingLocation => null;

			public ITransportContractDocument TransportContractDocument => null;

			public IGovernmentAgencyGoodsItem GovernmentAgencyGoodsItem => null;

			public ILocation TranshipmentLocation => null;

			public ILocation TransitDeparture => null;
		}

		class ConsignmentItem : IConsignmentItem
		{
			public ICommodity Commodity => new Commodity();

			public IGoodsMeasure GoodsMeasure => new GoodsMeasure();

			public IPackaging Packaging => new Packaging();

			public IEnumerable<ITransportContractDocument> TransportContractDocuments => new List<ITransportContractDocument>() { new TransportContractDocument(), new TransportContractDocument() };

			public ZString Split => null;

			public IOrigin Origin => null;

			public ZString AssociatedGovernmentProcedureCode => null;
		}

		class Commodity : ICommodity
		{
			public ZString CargoDescription => Message.NodeValueIsEmpty ? "" : "CargoDescription";

			public IEnumerable<IAdditionalDocument> AdditionalDocuments => null;

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

			public ZString BondedNoteCode => null;

			public IEnumerable<ZString> VehicleIDs => null;

			public IInvoice Invoice => null;

			public ZString PrintingTariffCode => null;

			IClassification ICommodity.Classification => null;
		}

		class GoodsMeasure : IGoodsMeasure
		{
			public ZDecimal TariffQuantity => Message.NodeValueIsEmpty ? 0 : 1;

			public ZString UnitCode => Message.NodeValueIsEmpty ? "" : "UnitCode";

			public ZDecimal NetWeightMeasure => -1;

			ZString IGoodsMeasure.CustomUnitCode => ZString.Empty;
		}

		class Packaging : IPackaging
		{
			public ZString MarksNumbers => Message.NodeValueIsEmpty ? "" : "MarksNumbers";

			public ZString PackagingMaterialDescription => Message.NodeValueIsEmpty ? "" : "PackagingMaterialDescription";

			public ZString Combination => Message.NodeValueIsEmpty ? "" : "Combination";

			public ZString TypeCode => "TypeCode";

			public ZDecimal QuantityQuantity => -99m;

			ZDate IPackaging.PackingDateTime => ZDate.Empty;
		}

		class TransportContractDocument : ITransportContractDocument
		{
			public ZString ID => "ID";

			public ZString TypeCode => "TypeCode";

			public IPartyDetails Deconsolidator => null;
		}

		class DepartureTransportMeans : ITransportMeans
		{
			public ZString ID => Message.NodeValueIsEmpty ? "" : "ID";

			public ZString Name => Message.NodeValueIsEmpty ? "" : "Name";

			public ZString JourneyID => Message.NodeValueIsEmpty ? "" : "JourneyID";

			public ZString Registration => Message.NodeValueIsEmpty ? "" : "Registration";

			public ZString TypeCode => null;

			public ZDate ArrivalDateTime => ZDate.Empty;

			public IEnumerable<ZString> ItineraryRoutingCountryCodes => null;

			public ZString CallSignID => null;
		}

		class TransportEquipment : ITransportEquipment
		{
			public ZString CharacteristicCode => "CharacteristicCode";

			public ZString ID => "ID";

			public ZString UsedCapacityCode => "UsedCapacityCode";

			public IEnumerable<ZString> Seals => null;
		}

		class Applicant : IPartyDetails
		{
			public Applicant()
			{
				ChineseName = "ChineseName";
				Name = "Name";
			}

			public ZString ChineseName { get; set; }

			public ZString CustomsControlID => "CustomsControlID";

			public ZString ID => "ID";

			public ZString Name { get; set; }

			public ZString TypeCode => "TypeCode";

			public ZString PaymentOnAccountBusinessID => null;

			public ZString RoleCode => null;

			public ZString SubBoxID => null;

			public IAddress Address => null;

			public ILPCOAuthorizedParty LPCOAuthorizedParty => null;

			public IEnumerable<ICommunication> Communications => null;

			public ZString ContactName => null;

			public ZString MainManufacturer => null;

			public ZString UndertakeCode => null;

			public ZString OwnerName => null;

			public IEnumerable<IAdditionalInformation> AdditionalInformations => null;
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
			var declaration = new N5301MessageBuilder().PopulateDeclaration(message);

			NUnit.Framework.Assert.That(declaration.FunctionCode.Value, NUnit.Framework.Is.EqualTo("9"), "FunctionCode is 9");

			var xml = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			AssertXMLContains("<FunctionCode>9</FunctionCode>", xml);
			AssertXMLContains(GetExpectedMessageXML("N5301.xml"), xml);
			CombineAssertions(() =>
			{
				AsserContainsXmlValueByType(xml, message, typeof(Message));
				AsserContainsXmlByType(xml, declaration.GetType());
			});
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
			var declaration = new N5301MessageBuilder().PopulateDeclaration(message);
			var xml = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);

			var namespacePrefix = "a";
			var nameSpace = new XmlNamespaceManager(xmlDocument.NameTable);
			nameSpace.AddNamespace(namespacePrefix, xmlDocument.DocumentElement.Attributes["xmlns"].Value);
			CombineAssertions(() =>
			{
				var utMessage = nodeValueIsEmpty ? " node Value is empty" : "node Value is not empty";
				NUnit.Framework.Assert.That(message.TotalGrossMassMeasure, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? 0m : 1m).Using(CustomComparers.TypeComparison), $"The Declaration.TotalGrossMassMeasure{utMessage}");
				NUnit.Framework.Assert.That(message.BorderTransportMeans.ArrivalDateTime, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? ZDate.Empty : new ZDate(2011, 05, 23)), $"The Declaration.BorderTransportMeans.ArrivalDateTime{utMessage}");
				NUnit.Framework.Assert.That(message.BorderTransportMeans.ID, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? "" : "ID").Using(CustomComparers.TypeComparison), $"The Declaration.BorderTransportMeans.ID{utMessage}");
				NUnit.Framework.Assert.That(message.BorderTransportMeans.JourneyID, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? "" : "JourneyID").Using(CustomComparers.TypeComparison), $"The Declaration.BorderTransportMeans.JourneyID{utMessage}");
				NUnit.Framework.Assert.That(message.BorderTransportMeans.Name, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? "" : "Name").Using(CustomComparers.TypeComparison), $"The Declaration.BorderTransportMeans.Name{utMessage}");
				NUnit.Framework.Assert.That(message.BorderTransportMeans.Registration, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? "" : "Registration").Using(CustomComparers.TypeComparison), $"The Declaration.BorderTransportMeans.tw_Registration{utMessage}");
				NUnit.Framework.Assert.That(message.Carrier.ID, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? "" : "ID").Using(CustomComparers.TypeComparison), $"The Declaration.Carrier.ID{utMessage}");
				NUnit.Framework.Assert.That(message.Consignment.ManifestSerialNumber, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? "" : "ManifestSerialNumber").Using(CustomComparers.TypeComparison), $"The Declaration.Consignment.tw_ManifestSerialNumber{utMessage}");
				NUnit.Framework.Assert.That(message.Consignment.ShippingOrderNumber, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? "" : "ShippingOrderNumber").Using(CustomComparers.TypeComparison), $"The Declaration.Consignment.tw_ShippingOrderNumber{utMessage}");
				NUnit.Framework.Assert.That(message.Consignment.ConsignmentItem.Commodity.CargoDescription, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? "" : "CargoDescription").Using(CustomComparers.TypeComparison), $"The Declaration.Consignment.ConsignmentItem.Commodity.CargoDescription{utMessage}");
				NUnit.Framework.Assert.That(message.Consignment.ConsignmentItem.GoodsMeasure.TariffQuantity, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? 0m : 1m).Using(CustomComparers.TypeComparison), $"The Declaration.Consignment.ConsignmentItem.GoodsMeasure{utMessage} if TariffQuantity and UnitCode are empty");
				NUnit.Framework.Assert.That(message.Consignment.ConsignmentItem.GoodsMeasure.UnitCode, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? "" : "UnitCode").Using(CustomComparers.TypeComparison), $"The Declaration.Consignment.ConsignmentItem.GoodsMeasure{utMessage} if TariffQuantity and UnitCode are empty");
				NUnit.Framework.Assert.That(message.Consignment.ConsignmentItem.Packaging.MarksNumbers, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? "" : "MarksNumbers").Using(CustomComparers.TypeComparison), $"The Declaration.Consignment.ConsignmentItem.Packaging.MarksNumbers{utMessage}");
				NUnit.Framework.Assert.That(message.Consignment.ConsignmentItem.Packaging.PackagingMaterialDescription, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? "" : "PackagingMaterialDescription").Using(CustomComparers.TypeComparison), $"The Declaration.Consignment.ConsignmentItem.Packaging.PackagingMaterialDescription{utMessage}");
				NUnit.Framework.Assert.That(message.Consignment.ConsignmentItem.Packaging.Combination, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? "" : "Combination").Using(CustomComparers.TypeComparison), $"The Declaration.Consignment.ConsignmentItem.Packaging.tw_Combination{utMessage}");
				NUnit.Framework.Assert.That(message.Consignment.DepartureTransportMeans.ID, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? "" : "ID").Using(CustomComparers.TypeComparison), $"The Declaration.Consignment.DepartureTransportMeans.ID{utMessage}");
				NUnit.Framework.Assert.That(message.Consignment.DepartureTransportMeans.Name, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? "" : "Name").Using(CustomComparers.TypeComparison), $"The Declaration.Consignment.DepartureTransportMeans.Name{utMessage}");
				NUnit.Framework.Assert.That(message.Consignment.DepartureTransportMeans.JourneyID, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? "" : "JourneyID").Using(CustomComparers.TypeComparison), $"The Declaration.Consignment.DepartureTransportMeans.tw_JourneyID{utMessage}");
				NUnit.Framework.Assert.That(message.Consignment.DepartureTransportMeans.Registration, NUnit.Framework.Is.EqualTo(nodeValueIsEmpty ? "" : "Registration").Using(CustomComparers.TypeComparison), $"The Declaration.Consignment.DepartureTransportMeans.tw_Registration{utMessage}");
			});

			foreach (var nodeName in NodeNames)
			{
				if (nodeValueIsEmpty)
				{
					NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode(nodeName, nameSpace), NUnit.Framework.Is.EqualTo(default(System.Xml.XmlNode)), "$The {nodeName} node is empty - should be [null]");
				}
				else
				{
					NUnit.Framework.Assert.That(xmlDocument.SelectSingleNode(nodeName, nameSpace), NUnit.Framework.Is.Not.EqualTo(default(System.Xml.XmlNode)), "$The {nodeName} node is not empty - should not be [null]");
				}
			}
		}

		IEnumerable<ZString> NodeNames
		{
			get
			{
				yield return "a:Declaration/a:TotalGrossMassMeasure";
				yield return "a:Declaration/a:BorderTransportMeans/a:ArrivalDateTime";
				yield return "a:Declaration/a:BorderTransportMeans/a:ID";
				yield return "a:Declaration/a:BorderTransportMeans/a:JourneyID";
				yield return "a:Declaration/a:BorderTransportMeans/a:Name";
				yield return "a:Declaration/a:BorderTransportMeans/a:tw_Registration";
				yield return "a:Declaration/a:Carrier";
				yield return "a:Declaration/a:Carrier/a:ID";
				yield return "a:Declaration/a:Consignment/a:tw_ManifestSerialNumber";
				yield return "a:Declaration/a:Consignment/a:tw_ShippingOrderNumber";
				yield return "a:Declaration/a:Consignment/a:ConsignmentItem/a:Commodity";
				yield return "a:Declaration/a:Consignment/a:ConsignmentItem/a:Commodity/a:CargoDescription";
				yield return "a:Declaration/a:Consignment/a:ConsignmentItem/a:GoodsMeasure";
				yield return "a:Declaration/a:Consignment/a:ConsignmentItem/a:Packaging/a:MarksNumbers";
				yield return "a:Declaration/a:Consignment/a:ConsignmentItem/a:Packaging/a:PackagingMaterialDescription";
				yield return "a:Declaration/a:Consignment/a:ConsignmentItem/a:Packaging/a:tw_Combination";
				yield return "a:Declaration/a:Consignment/a:DepartureTransportMeans";
				yield return "a:Declaration/a:Consignment/a:DepartureTransportMeans/a:ID";
				yield return "a:Declaration/a:Consignment/a:DepartureTransportMeans/a:Name";
				yield return "a:Declaration/a:Consignment/a:DepartureTransportMeans/a:tw_JourneyID";
				yield return "a:Declaration/a:Consignment/a:DepartureTransportMeans/a:tw_Registration";
			}
		}

		[ExpectNoExceptions]
		public void TestPopulateApplicantMaxLength()
		{
			var obj = new Applicant();
			obj.ChineseName = new ZString('C', 71);
			obj.Name = new ZString('E', 81);
			var messageBuilder = new N5301MessageBuilder();
			var declaration = new Declaration();
			messageBuilder.PopulateApplicant(declaration, obj);
			var applicant = declaration.TwApplicant;
			NUnit.Framework.Assert.That(applicant.TwChineseName.Value.Length, NUnit.Framework.Is.EqualTo(70));
			NUnit.Framework.Assert.That(applicant.TwName.Value.Length, NUnit.Framework.Is.EqualTo(80));
		}
	}
}
