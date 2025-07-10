using System.Collections.Generic;
using CargoWise.Customs.TW.MessageDefinitions.N5135;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	sealed class N5135MessageBuilderTest : BaseTWMessageBuilderTest<IN5135Declaration, Declaration>
	{
		[ExpectNoExceptions]
		public void TestPopulateDeclarationWithOptionalNodes()
		{
			var message = GetMessage();
			var declaration = new N5135MessageBuilder().PopulateDeclaration(message, MessageFunctionCode.Add);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("N5135.xml");

			CombineAssertions(() =>
			{
				AssertXMLContains(expected, actual);
				AsserContainsXmlByType(actual, declaration.GetType());
			});
		}

		public void TestPopulateDeclarationWithoutOptionalNodes()
		{
			var message = GetMessage(false);
			var declaration = new N5135MessageBuilder().PopulateDeclaration(message, MessageFunctionCode.Add);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("N5135_WithoutOptionalNodes.xml");
			AssertXMLContains(expected, actual);
		}

		public void TestPopulateXml()
		{
			var message = GetMessage();
			var xml = new N5135MessageBuilder().PopulateXml(message, MessageFunctionCode.Add);
			AssertXMLContains("<FunctionCode>9</FunctionCode>", xml);
			AssertXMLContains(GetExpectedMessageXML("N5135.xml"), xml);
		}

		protected override IN5135Declaration CreateDataSource()
		{
			return GetMessage();
		}

		protected override ITWMessageBuilder CreateNewMessageBuilder()
		{
			return new N5135MessageBuilder();
		}

		IN5135Declaration GetMessage(bool optionalNodesArePopulated = true)
		{
			var mock = new Mock<IN5135Declaration>();
			mock.Setup(x => x.AcceptanceDateTime).Returns(new ZDate(2023, 11, 07));
			mock.Setup(x => x.FunctionCode).Returns("9");
			mock.Setup(x => x.ID).Returns("123456789");
			mock.Setup(x => x.TypeCode).Returns("2");
			mock.Setup(x => x.Agent).Returns(GetAgent);
			mock.Setup(x => x.BorderTransportMeans).Returns(GetITransportMeans);
			mock.Setup(x => x.DutyTaxFee).Returns(GetDutyTaxFee);
			mock.Setup(x => x.Importer).Returns(GetImporter(optionalNodesArePopulated));
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

		IAddress GetConsigneeAddress(bool optionalNodesArePopulate)
		{
			var address = new Mock<IAddress>();
			if (optionalNodesArePopulate)
			{
				address.Setup(x => x.Line).Returns("Consignee Address. Line. Text");
				address.Setup(x => x.ChineseLine).Returns("收貨人中文地址");
			}
			return address.Object;
		}

		IPartyDetails GetConsignee(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IPartyDetails>();
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ID).Returns("c id");
				mock.Setup(x => x.Name).Returns("c name");
				mock.Setup(x => x.ChineseName).Returns("cn name");
				mock.Setup(x => x.TypeCode).Returns("tc");
			}
			mock.Setup(x => x.Address).Returns(GetConsigneeAddress(optionalNodesArePopulated));
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

		ILocation GetGoodsShipmentConsignmentLoadingLocation()
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

		IBCDConsignment GetGoodsShipmentConsignment(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IBCDConsignment>();
			mock.Setup(x => x.TotalPackageQuantity).Returns(123);
			mock.Setup(x => x.InvoiceAmount).Returns(778);
			mock.Setup(x => x.LoadingLocation).Returns(GetGoodsShipmentConsignmentLoadingLocation);
			mock.Setup(x => x.Packaging).Returns(GetGoodsShipmentConsignmentPackaging);
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ConsignmentItem).Returns(GetGoodsShipmentConsignmentItem);
				mock.Setup(x => x.GovernmentProcedures).Returns(GetGoodsShipmentGovernmentProcedures);
				mock.Setup(x => x.TransportContractDocumentId).Returns("tcid1");
			}
			return mock.Object;
		}

		ICurrencyExchange GetGoodsShipmentCurrencyExchange()
		{
			var mock = new Mock<ICurrencyExchange>();
			mock.Setup(x => x.CurrencyTypeCode).Returns("USD");
			return mock.Object;
		}

		ICustomsValuation GetGoodsShipmentCustomsValuation(bool optionalNodesArePopulated)
		{
			var mock = new Mock<ICustomsValuation>();
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ExitToEntryChargeAmount).Returns(111.999999m);
				mock.Setup(x => x.FreightChargeAmount).Returns(222.999999m);
				mock.Setup(x => x.OtherChargeDeductionAmount).Returns(333.999999m);
				mock.Setup(x => x.OtherChargeAmount).Returns(444.999999m);
				mock.Setup(x => x.OtherDeductionAmount).Returns(555.999999m);
				mock.Setup(x => x.TotalDutyTaxFeeAmount).Returns(666.999999m);
			}
			return mock.Object;
		}

		IEnumerable<IGoodsShipmentDutyTaxFee> GetGoodsShipmentDutyTaxFee()
		{
			var mock = new Mock<IGoodsShipmentDutyTaxFee>();
			mock.Setup(x => x.TypeCode).Returns("dtt1");
			mock.Setup(x => x.AdValoremTaxBaseAmount).Returns(888);
			yield return mock.Object;

			mock = new Mock<IGoodsShipmentDutyTaxFee>();
			mock.Setup(x => x.TypeCode).Returns("dtt2");
			mock.Setup(x => x.AdValoremTaxBaseAmount).Returns(777);
			yield return mock.Object;
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

		ICommodityDutyTaxFee GetCommodityDutyTaxFee()
		{
			var mock = new Mock<ICommodityDutyTaxFee>();
			mock.Setup(x => x.AdValoremTaxBaseAmount).Returns(666);
			mock.Setup(x => x.SpecificTaxBaseQuantity).Returns(667);
			return mock.Object;
		}

		IGovernmentProcedure IGovernmentProcedure()
		{
			var mock = new Mock<IGovernmentProcedure>();
			mock.Setup(x => x.CurrentCode).Returns("cc");
			return mock.Object;
		}

		IDutyOtherTaxFee GetCommodityDutyOtherTaxFee(bool optionalNodesArePopulated, string typeCode)
		{
			var mock = new Mock<IDutyOtherTaxFee>();
			mock.Setup(x => x.TypeCode).Returns(typeCode);
			mock.Setup(x => x.TaxRateNumeric).Returns(111.999999m);
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.PercentageNumeric).Returns(222.999999m);
			}
			return mock.Object;
		}

		IDutyTaxFeeAmount GetCommodityDutyTaxFeeAmount(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IDutyTaxFeeAmount>();
			mock.Setup(x => x.TaxRateNumeric).Returns(231.999999m);
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.PercentageNumeric).Returns(211.999999m);
			}
			return mock.Object;
		}

		IDutyTaxFeeQuantity GetCommodityDutyTaxFeeQuantity(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IDutyTaxFeeQuantity>();
			mock.Setup(x => x.DutyUnitCode).Returns("duc");
			mock.Setup(x => x.TaxRateNumeric).Returns(888.1111111m);
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.PercentageNumeric).Returns(511.999999m);
			}
			return mock.Object;
		}

		ICommodity GetCommodity(bool optionalNodesArePopulated)
		{
			var mock = new Mock<ICommodity>();
			mock.Setup(x => x.Description).Returns("commodity desc");
			mock.Setup(x => x.GovernmentProcedure).Returns(IGovernmentProcedure);
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.CommercialCategorizationID).Returns("ccId");
				mock.Setup(x => x.Name).Returns("commodity name");
				mock.Setup(x => x.Classification).Returns(GetClassification);
				mock.Setup(x => x.Constituent).Returns(GetConstituent);
				mock.Setup(x => x.DutyTaxFee).Returns(GetCommodityDutyTaxFee);
				mock.Setup(x => x.DutyOtherTaxFees).Returns(new[] { GetCommodityDutyOtherTaxFee(optionalNodesArePopulated, "ty1"), GetCommodityDutyOtherTaxFee(false, "ty2") });
				mock.Setup(x => x.DutyTaxFeeAmount).Returns(GetCommodityDutyTaxFeeAmount(optionalNodesArePopulated));
				mock.Setup(x => x.DutyTaxFeeQuantity).Returns(GetCommodityDutyTaxFeeQuantity(optionalNodesArePopulated));
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

		IOrigin GetOrigin()
		{
			var mock = new Mock<IOrigin>();
			mock.Setup(x => x.CountryCode).Returns("TW");
			return Mock.Of<IOrigin>();
		}

		IGovernmentAgencyGoodsItem GetGovernmentAgencyGoodsItem(bool optionalNodesArePopulated, string sequenceNumeric)
		{
			var mock = new Mock<IGovernmentAgencyGoodsItem>();
			mock.Setup(x => x.SequenceNumeric).Returns(1);
			mock.Setup(x => x.Commodity).Returns(GetCommodity(optionalNodesArePopulated));
			mock.Setup(x => x.GoodsMeasure).Returns(GetGoodsMeasure);
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.Origin).Returns(GetOrigin);
			}
			return mock.Object;
		}

		IPartyDetails GetSupplier(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.Name).Returns("supplier name");
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ChineseName).Returns("supplier cn name");
			}
			return mock.Object;
		}

		IN5135GoodsShipment GetGoodsShipments(bool optionalNodesArePopulated, ZInt sequenceNumeric)
		{
			var mock = new Mock<IN5135GoodsShipment>();
			mock.Setup(x => x.SequenceNumeric).Returns(sequenceNumeric);
			mock.Setup(x => x.InvoiceAmount).Returns(999);
			mock.Setup(x => x.TotalGrossMassMeasure).Returns(234);
			mock.Setup(x => x.Consignment).Returns(GetGoodsShipmentConsignment(optionalNodesArePopulated));
			mock.Setup(x => x.CurrencyExchange).Returns(GetGoodsShipmentCurrencyExchange);
			mock.Setup(x => x.GovernmentAgencyGoodsItems).Returns(new[] { GetGovernmentAgencyGoodsItem(optionalNodesArePopulated, "1"), GetGovernmentAgencyGoodsItem(false, "2") });
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.TaxFeeDeclared).Returns("t1");
				mock.Setup(x => x.AdditionalDocuments).Returns(new[] { GetAdditionalDocuments(optionalNodesArePopulated, "1"), GetAdditionalDocuments(false, "2") });
				mock.Setup(x => x.Consignee).Returns(GetConsignee(optionalNodesArePopulated));
				mock.Setup(x => x.CustomsValuation).Returns(GetGoodsShipmentCustomsValuation(optionalNodesArePopulated));
				mock.Setup(x => x.DutyTaxFees).Returns(GetGoodsShipmentDutyTaxFee);
				mock.Setup(x => x.Supplier).Returns(GetSupplier(optionalNodesArePopulated));
				mock.Setup(x => x.TradeTermsConditionCode).Returns("FOB");
			}
			return mock.Object;
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

		IPartyDetails GetImporter(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ID).Returns("1234643212");
			mock.Setup(x => x.Name).Returns("Importer name");
			mock.Setup(x => x.TypeCode).Returns("X21");
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ChineseName).Returns("Name of the importer ( duty payer) in Chinese.");
			}
			mock.Setup(x => x.Address).Returns(GetImporterAddress(optionalNodesArePopulated));
			return mock.Object;
		}

		IAddress GetImporterAddress(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.Line).Returns("Address. Line. Text");
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ChineseLine).Returns("Address. Chinese Line. Text");
			}
			return mock.Object;
		}

		IDutyTaxFee GetDutyTaxFee()
		{
			var mock = new Mock<IDutyTaxFee>();
			mock.Setup(x => x.DutyMethodCode).Returns("C");
			mock.Setup(x => x.PaymentObligationGuaranteeReferenceID).Returns("X1");
			return mock.Object;
		}

		ITransportMeans GetITransportMeans()
		{
			var transportMeans = new Mock<ITransportMeans>();
			transportMeans.Setup(x => x.ArrivalDateTime).Returns(new ZDate(2023, 11, 08));
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

		ITransportMeans GetTransportMeans(bool optionalNodesArePopulated)
		{
			var transportMeans = new Mock<ITransportMeans>();
			if (optionalNodesArePopulated)
			{
				transportMeans.Setup(x => x.ID).Returns("X1");
				transportMeans.Setup(x => x.Registration).Returns("X3");
			}
			transportMeans.Setup(x => x.JourneyID).Returns("X2");
			return transportMeans.Object;
		}

		IBCDConsignment GetConsignment(bool optionalNodesArePopulated)
		{
			var consignment = new Mock<IBCDConsignment>();
			if (optionalNodesArePopulated)
			{
				consignment.Setup(x => x.BoardedQuantity).Returns(999);
				consignment.Setup(x => x.ManifestSerialNumber).Returns("123456789");
				consignment.Setup(x => x.AssociatedTransportDocumentId).Returns("66668888");
				consignment.Setup(x => x.TransportContractDocumentId).Returns("TCD1");
			}
			consignment.Setup(x => x.BorderTransportMeans).Returns(GetTransportMeans(optionalNodesArePopulated));
			consignment.Setup(x => x.GoodsLocation).Returns("GL");
			return consignment.Object;
		}
	}
}
