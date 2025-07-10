using System.Collections.Generic;
using CargoWise.Customs.TW.MessageDefinitions.N5205;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	sealed class N5205MessageBuilderTest : BaseTWMessageBuilderTest<IN5205Declaration, Declaration>
	{
		[ExpectNoExceptions]
		public void TestPopulateDeclarationWithOptionalNodes()
		{
			var message = GetMessage();
			var declaration = new N5205MessageBuilder().PopulateDeclaration(message, MessageFunctionCode.Add);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("N5205.xml");

			CombineAssertions(() =>
			{
				AssertXMLContains(expected, actual);
				AsserContainsXmlByType(actual, declaration.GetType());
			});
		}

		public void TestPopulateDeclarationWithoutOptionalNodes()
		{
			var message = GetMessage(false);
			var declaration = new N5205MessageBuilder().PopulateDeclaration(message, MessageFunctionCode.Add);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("N5205_WithoutOptionalNodes.xml");
			AssertXMLContains(expected, actual);
		}

		public void TestPopulateXml()
		{
			var message = GetMessage();
			var xml = new N5205MessageBuilder().PopulateXml(message, MessageFunctionCode.Add);
			AssertXMLContains("<FunctionCode>9</FunctionCode>", xml);
			AssertXMLContains(GetExpectedMessageXML("N5205.xml"), xml);
		}

		protected override IN5205Declaration CreateDataSource()
		{
			return GetMessage();
		}

		protected override ITWMessageBuilder CreateNewMessageBuilder()
		{
			return new N5205MessageBuilder();
		}

		IN5205Declaration GetMessage(bool optionalNodesArePopulated = true, bool testNotPopulateWhenNoValue = false)
		{
			var mock = new Mock<IN5205Declaration>();
			mock.Setup(x => x.AcceptanceDateTime).Returns(new ZDate(2023, 11, 07));
			mock.Setup(x => x.FunctionCode).Returns("9");
			mock.Setup(x => x.ID).Returns("123456789");
			mock.Setup(x => x.TypeCode).Returns("2");
			mock.Setup(x => x.Agent).Returns(GetAgent);
			mock.Setup(x => x.BorderTransportMeans).Returns(GetITransportMeans);
			mock.Setup(x => x.Exporter).Returns(GetExporter(optionalNodesArePopulated));
			mock.Setup(x => x.RepresentativePersonName).Returns("Representative Person Name");
			mock.Setup(x => x.ExpressCarrier).Returns(GetExpressCarrier(optionalNodesArePopulated));
			mock.Setup(x => x.OnBoardCourier).Returns(GetOnBoardCourier(optionalNodesArePopulated));
			mock.Setup(x => x.Consignment).Returns(GetConsignment(optionalNodesArePopulated));
			mock.Setup(x => x.GoodsShipments).Returns(new[] { GetGoodsShipments(optionalNodesArePopulated, 1), GetGoodsShipments(false, 2) });

			return mock.Object;
		}

		IAdditionalDocument GetAdditionalDocuments(bool optionalNodesArePopulated, string id)
		{
			var mock = new Mock<IAdditionalDocument>();
			mock.Setup(x => x.ImageFileFormat).Returns("jpg");
			mock.Setup(x => x.SizeMeasure).Returns(256);
			mock.Setup(x => x.TypeCode).Returns("c1");
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ID).Returns(id);
				mock.Setup(x => x.Content).Returns("content test");
				mock.Setup(x => x.ImageFileName).Returns("name");
				mock.Setup(x => x.ResponsibleGovernmentAgency).Returns("Responsible Government Agency");
			}
			return mock.Object;
		}

		IAddress GetBuyerAddress(bool optionalNodesArePopulate)
		{
			var address = new Mock<IAddress>();
			address.Setup(x => x.CountryCode).Returns("US");
			if (optionalNodesArePopulate)
			{
				address.Setup(x => x.Line).Returns("Buyer Address. Line. Text");
			}
			return address.Object;
		}

		IPartyDetails GetBuyer(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.Name).Returns("b name");
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ID).Returns("b id");
				mock.Setup(x => x.TypeCode).Returns("tc");
			}
			mock.Setup(x => x.Address).Returns(GetBuyerAddress(optionalNodesArePopulated));
			return mock.Object;
		}

		IConsignmentItem GetGoodsShipmentConsignmentItem()
		{
			var mock = new Mock<IConsignmentItem>();
			mock.Setup(x => x.AssociatedGovernmentProcedureCode).Returns("Associated Government Procedure Code");
			return mock.Object;
		}

		IEnumerable<IGovernmentProcedure> GetGoodsShipmentGovernmentProcedures()
		{
			var mock = new Mock<IGovernmentProcedure>();
			mock.Setup(x => x.Description).Returns("gp desc1");
			yield return mock.Object;

			mock = new Mock<IGovernmentProcedure>();
			mock.Setup(x => x.Description).Returns("gp desc2");
			yield return mock.Object;
		}

		ILocation GetGoodsShipmentConsignmentUnloadingLocation()
		{
			var mock = new Mock<ILocation>();
			mock.Setup(x => x.ID).Returns("lcid");
			return mock.Object;
		}

		IPackaging GetGoodsShipmentConsignmentPackaging()
		{
			var mock = new Mock<IPackaging>();
			mock.Setup(x => x.TypeCode).Returns("tc1");
			return mock.Object;
		}

		IEnumerable<ITransportContractDocument> GetGoodsShipmentConsignmentPackagingTransportContractDocument()
		{
			var mock = new Mock<ITransportContractDocument>();
			mock.Setup(x => x.ID).Returns("tcid1");
			yield return mock.Object;

			mock = new Mock<ITransportContractDocument>();
			mock.Setup(x => x.ID).Returns("tcid2");
			yield return mock.Object;
		}

		IBCDConsignment GetGoodsShipmentConsignment(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IBCDConsignment>();
			mock.Setup(x => x.TotalPackageQuantity).Returns(123);
			mock.Setup(x => x.Packaging).Returns(GetGoodsShipmentConsignmentPackaging);
			mock.Setup(x => x.UnloadingLocation).Returns(GetGoodsShipmentConsignmentUnloadingLocation);
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ConsignmentItem).Returns(GetGoodsShipmentConsignmentItem);
				mock.Setup(x => x.GovernmentProcedures).Returns(GetGoodsShipmentGovernmentProcedures);
				mock.Setup(x => x.TransportContractDocuments).Returns(GetGoodsShipmentConsignmentPackagingTransportContractDocument);
			}
			return mock.Object;
		}

		IAddress GetGoodsShipmentsExporterAddress()
		{
			var address = new Mock<IAddress>();
			address.Setup(x => x.Line).Returns("Exporter Address. Line. Text");
			address.Setup(x => x.ChineseLine).Returns("出口商中文地址");
			return address.Object;
		}

		IPartyDetails GetGoodsShipmentsExporter(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ID).Returns("exporter id");
			mock.Setup(x => x.TypeCode).Returns("58");
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.Name).Returns("exporter name");
				mock.Setup(x => x.ChineseName).Returns("exporter cn name");
				mock.Setup(x => x.Address).Returns(GetGoodsShipmentsExporterAddress);
			}
			return mock.Object;
		}

		IGovernmentProcedure IGovernmentProcedure()
		{
			var mock = new Mock<IGovernmentProcedure>();
			mock.Setup(x => x.CurrentCode).Returns("cc");
			return mock.Object;
		}

		IClassification GetClassification()
		{
			var mock = new Mock<IClassification>();
			mock.Setup(x => x.ID).Returns("Classification ID");
			return mock.Object;
		}

		IConstituent GetConstituent()
		{
			var mock = new Mock<IConstituent>();
			mock.Setup(x => x.ElementDescription).Returns("Element Description");
			return mock.Object;
		}

		IInvoiceLine GetCommodityInvoiceLine()
		{
			var mock = new Mock<IInvoiceLine>();
			mock.Setup(x => x.ItemChargeAmount).Returns(999.99);
			return mock.Object;
		}

		ICommodityNumber GetCommodityNumber()
		{
			var mock = new Mock<ICommodityNumber>();
			mock.Setup(x => x.ID).Returns("cnID");
			mock.Setup(x => x.IdentifierTypeCode).Returns("I91");
			return mock.Object;
		}

		ICommodity GetCommodity(bool optionalNodesArePopulated)
		{
			var mock = new Mock<ICommodity>();
			mock.Setup(x => x.Description).Returns("commodity desc");
			mock.Setup(x => x.Name).Returns("commodity name");
			mock.Setup(x => x.InvoiceLine).Returns(GetCommodityInvoiceLine);
			mock.Setup(x => x.GovernmentProcedure).Returns(IGovernmentProcedure);
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.CommercialCategorizationID).Returns("ccId");
				mock.Setup(x => x.Classification).Returns(GetClassification);
				mock.Setup(x => x.Constituent).Returns(GetConstituent);
				mock.Setup(x => x.CommodityNumbers).Returns(new[] { GetCommodityNumber(), GetCommodityNumber() });
			}
			return mock.Object;
		}

		IGoodsMeasure GetGoodsMeasure()
		{
			var mock = new Mock<IGoodsMeasure>();
			mock.Setup(x => x.NetWeightMeasure).Returns(45.333333m);
			mock.Setup(x => x.TariffQuantity).Returns(44.133333m);
			mock.Setup(x => x.UnitCode).Returns("PLE");
			return mock.Object;
		}

		IGovernmentProcedure GetGovernmentAgencyGoodsItemGovernmentProcedures()
		{
			var mock = new Mock<IGovernmentProcedure>();
			mock.Setup(x => x.CurrentCode).Returns("c1");
			return mock.Object;
		}

		IOrigin GetOrigin()
		{
			var mock = new Mock<IOrigin>();
			mock.Setup(x => x.CountryCode).Returns("TW");
			return Mock.Of<IOrigin>();
		}

		IPreviousDocument GetGovernmentAgencyGoodsItemPreBondedDocument()
		{
			var mock = new Mock<IPreviousDocument>();
			mock.Setup(x => x.ID).Returns("pid");
			mock.Setup(x => x.LineNumeric).Returns(6);
			return mock.Object;
		}

		IGovernmentAgencyGoodsItem GetGovernmentAgencyGoodsItem(bool optionalNodesArePopulated, string sequenceNumeric)
		{
			var mock = new Mock<IGovernmentAgencyGoodsItem>();
			mock.Setup(x => x.SequenceNumeric).Returns(1);
			mock.Setup(x => x.Commodity).Returns(GetCommodity(optionalNodesArePopulated));
			mock.Setup(x => x.GoodsMeasure).Returns(GetGoodsMeasure);
			mock.Setup(x => x.GovernmentProcedure).Returns(GetGovernmentAgencyGoodsItemGovernmentProcedures);
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.Origin).Returns(GetOrigin);
				mock.Setup(x => x.PreBondedDocument).Returns(GetGovernmentAgencyGoodsItemPreBondedDocument);
			}
			return mock.Object;
		}

		IBCDGoodsShipment GetGoodsShipments(bool optionalNodesArePopulated, ZInt sequenceNumeric)
		{
			var mock = new Mock<IBCDGoodsShipment>();
			mock.Setup(x => x.SequenceNumeric).Returns(sequenceNumeric);
			mock.Setup(x => x.ItemChargeAmount).Returns(999);
			mock.Setup(x => x.TotalGrossMassMeasure).Returns(234);
			mock.Setup(x => x.Consignment).Returns(GetGoodsShipmentConsignment(optionalNodesArePopulated));
			mock.Setup(x => x.Exporter).Returns(GetGoodsShipmentsExporter(optionalNodesArePopulated));
			mock.Setup(x => x.GovernmentAgencyGoodsItems).Returns(new[] { GetGovernmentAgencyGoodsItem(optionalNodesArePopulated, "1"), GetGovernmentAgencyGoodsItem(false, "2") });
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.AdditionalDocuments).Returns(new[] { GetAdditionalDocuments(optionalNodesArePopulated, "1"), GetAdditionalDocuments(false, "2") });
				mock.Setup(x => x.Buyer).Returns(GetBuyer(optionalNodesArePopulated));
				mock.Setup(x => x.DeliveryDestinationName).Returns("Delivery Destination Name");
			}
			return mock.Object;
		}

		ITransportMeans GetTransportMeans()
		{
			var mock = new Mock<ITransportMeans>();
			mock.Setup(x => x.JourneyID).Returns("X2");
			mock.Setup(x => x.CallSignID).Returns("N978CP");
			mock.Setup(x => x.Registration).Returns("REG1");
			return mock.Object;
		}

		ITransportMeans GetDepartureTransportMeans()
		{
			var mock = new Mock<ITransportMeans>();
			mock.Setup(x => x.ID).Returns("X1");
			mock.Setup(x => x.Registration).Returns("X3");
			return mock.Object;
		}

		IBCDConsignment GetConsignment(bool optionalNodesArePopulated)
		{
			var consignment = new Mock<IBCDConsignment>();
			if (optionalNodesArePopulated)
			{
				consignment.Setup(x => x.BoardedQuantity).Returns(999);
				consignment.Setup(x => x.ShippingOrderNumber).Returns("123456789");
				consignment.Setup(x => x.AssociatedTransportDocumentId).Returns("66668888");
				consignment.Setup(x => x.DepartureTransportMeans).Returns(GetDepartureTransportMeans);
			}
			consignment.Setup(x => x.TransportContractDocumentId).Returns("TTT111");
			consignment.Setup(x => x.BorderTransportMeans).Returns(GetTransportMeans);
			consignment.Setup(x => x.GoodsLocation).Returns("GL");
			return consignment.Object;
		}

		IPartyDetails GetOnBoardCourier(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ID).Returns("8666666");
			mock.Setup(x => x.Name).Returns("OnBoardCourier name");
			mock.Setup(x => x.TypeCode).Returns("X29");
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ChineseName).Returns("Name of the OnBoard Courier in Chinese.");
			}
			return mock.Object;
		}

		IPartyDetails GetExpressCarrier(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ID).Returns("987654");
			mock.Setup(x => x.Name).Returns("ExpressCarrier name");
			mock.Setup(x => x.TypeCode).Returns("X24");
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ChineseName).Returns("Name of the express carrier in Chinese.");
			}
			return mock.Object;
		}

		IPartyDetails GetExporter(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ID).Returns("1234643212");
			mock.Setup(x => x.Name).Returns("exporter name");
			mock.Setup(x => x.TypeCode).Returns("X21");
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ChineseName).Returns("Name of the exporter ( duty payer) in Chinese.");
				mock.Setup(x => x.CustomsControlID).Returns("Customs Control ID");
			}
			mock.Setup(x => x.Address).Returns(GetExporterAddress(optionalNodesArePopulated));
			return mock.Object;
		}

		IAddress GetExporterAddress(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.Line).Returns("Address. Line. Text");
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ChineseLine).Returns("Address. Chinese Line. Text");
			}
			return mock.Object;
		}

		ITransportMeans GetITransportMeans()
		{
			var transportMeans = new Mock<ITransportMeans>();
			transportMeans.Setup(x => x.TypeCode).Returns("Z");
			return transportMeans.Object;
		}

		IPartyDetails GetAgent()
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ID).Returns("01A");
			mock.Setup(x => x.RoleCode).Returns("AD");
			mock.Setup(x => x.SubBoxID).Returns("F");
			return mock.Object;
		}
	}
}
