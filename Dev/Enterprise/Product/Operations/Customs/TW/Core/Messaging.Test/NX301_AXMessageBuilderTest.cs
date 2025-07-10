using System.Collections.Generic;
using CargoWise.Customs.TW.MessageDefinitions.NX301_AX;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	sealed class NX301_AXMessageBuilderTest : BaseTWMessageBuilderTest<INX301_AXDeclaration, Declaration>
	{
		[ExpectNoExceptions]
		public override void TestSerializeToMessageString()
		{
			var expectedString = TWMessageBuilder.SerializeToMessageString(Declaration, "9");
			AssertXMLContains("<FunctionCode>9</FunctionCode>", expectedString);
			AsserContainsXmlValueByType(expectedString, Declaration, typeof(INX301_AXDeclaration), IgnoreAssertPropertyNames);
		}

		IEnumerable<ZString> IgnoreAssertPropertyNames
		{
			get
			{
				yield return "Enterprise.Customs.TW.Messaging.IAppointment.ReservationDate";
				yield return "Enterprise.Customs.TW.Messaging.IGoodsShipment.ExitDateTime";
			}
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclarationWithOptionalNodes()
		{
			var message = GetMessage();
			var declaration = new NX301_AXMessageBuilder().PopulateDeclaration(message);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("NX301_AX.xml");

			CombineAssertions(() =>
			{
				AssertXMLContains(expected, actual);
				AsserContainsXmlByType(actual, declaration.GetType());
			});
		}

		public void TestPopulateDeclarationWithoutOptionalNodes()
		{
			var message = GetMessage(false);
			var declaration = new NX301_AXMessageBuilder().PopulateDeclaration(message);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("NX301_AX_WithoutOptionalNodes.xml");
			AssertXMLContains(expected, actual);
		}

		protected override INX301_AXDeclaration CreateDataSource()
		{
			return GetMessage();
		}

		protected override ITWMessageBuilder CreateNewMessageBuilder()
		{
			return new NX301_AXMessageBuilder();
		}

		INX301_AXDeclaration GetMessage(bool optionalNodesArePopulated = true)
		{
			var mock = new Mock<INX301_AXDeclaration>();
			mock.Setup(x => x.FunctionalReferenceID).Returns("12345678002111070001");
			mock.Setup(x => x.FunctionCode).Returns("9");
			mock.Setup(x => x.ID).Returns("A0123456789");
			mock.Setup(x => x.BorderTransportMeans).Returns(GetITransportMeans);
			mock.Setup(x => x.Consignment).Returns(GetConsignment(optionalNodesArePopulated));
			mock.Setup(x => x.CurrencyExchange).Returns(GetCurrencyExchange);
			mock.Setup(x => x.GoodsShipment).Returns(GetGoodsShipment(optionalNodesArePopulated));
			mock.Setup(x => x.Importer).Returns(GetImporter);
			mock.Setup(x => x.Application).Returns(GetApplication(optionalNodesArePopulated));

			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.AdditionalDocument).Returns(GetAdditionalDocument);
				mock.Setup(x => x.Agent).Returns(GetAgent);
			}

			return mock.Object;
		}

		IAppointment GetApplicationAppointment()
		{
			var mock = new Mock<IAppointment>();
			mock.Setup(x => x.ReservationDate).Returns(ZDateTime.BrettsBirthday);
			mock.Setup(x => x.ReservationPeriodCode).Returns("C");
			return mock.Object;
		}

		IPayment GetApplicationPayment(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IPayment>();
			mock.Setup(x => x.MethodCode).Returns("CA");
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ReferenceID).Returns("WCOID014");
			}
			return mock.Object;
		}

		IEnumerable<ICommunication> GetApplicationAgentCommunications()
		{
			var mock1 = new Mock<ICommunication>();
			mock1.Setup(x => x.ID).Returns("0378711314");
			mock1.Setup(x => x.TypeID).Returns("TL");
			yield return mock1.Object;

			var mock2 = new Mock<ICommunication>();
			mock2.Setup(x => x.ID).Returns("xxx@hotmail.com");
			mock2.Setup(x => x.TypeID).Returns("EM");
			yield return mock2.Object;
		}

		IAddress GetApplicationAgentAddress()
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.ChineseLine).Returns("Address. Chinese Line. Text");
			return mock.Object;
		}

		IPartyDetails GetApplicationAgent()
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ID).Returns("AG001");
			mock.Setup(x => x.Name).Returns("Agent. Name. Text");
			mock.Setup(x => x.TypeCode).Returns("58");
			mock.Setup(x => x.Address).Returns(GetApplicationAgentAddress);
			mock.Setup(x => x.Communications).Returns(GetApplicationAgentCommunications);
			return mock.Object;
		}

		IApplicationAdditionalInformation GetApplicationAdditionalInformation(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IApplicationAdditionalInformation>();
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.StatementDescription).Returns("Description of an additional statement.");
				mock.Setup(x => x.BulkApplicationID).Returns("TW587");
				mock.Setup(x => x.ProvedPaper).Returns("P");
			}
			mock.Setup(x => x.ElectronicReceipt).Returns("E");
			return mock.Object;
		}

		IEnumerable<IAdditionalDocument> GetApplicationAdditionalDocuments()
		{
			var mock1 = new Mock<IAdditionalDocument>();
			mock1.Setup(x => x.ID).Returns("Additional Document. Identification. Identifier");
			mock1.Setup(x => x.ImageFileFormat).Returns("PDF");
			mock1.Setup(x => x.ImageFileName).Returns("Name of Attached Image File.");
			mock1.Setup(x => x.TypeCode).Returns("T5");
			mock1.Setup(x => x.ResponsibleGovernmentAgency).Returns("M3");

			var mock2 = new Mock<IAdditionalDocument>();
			mock2.Setup(x => x.TypeCode).Returns("T7");

			yield return mock1.Object;
			yield return mock2.Object;
		}

		IApplication GetApplication(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IApplication>();
			mock.Setup(x => x.AdditionalInformation).Returns(GetApplicationAdditionalInformation(optionalNodesArePopulated));
			mock.Setup(x => x.ContactOffice).Returns("A1");
			mock.Setup(x => x.Payment).Returns(GetApplicationPayment(optionalNodesArePopulated));
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.AdditionalDocuments).Returns(GetApplicationAdditionalDocuments);
				mock.Setup(x => x.Agent).Returns(GetApplicationAgent);
				mock.Setup(x => x.Appointment).Returns(GetApplicationAppointment);
			}

			return mock.Object;
		}

		IEnumerable<ICommunication> GetImporterCommunications()
		{
			var mock1 = new Mock<ICommunication>();
			mock1.Setup(x => x.ID).Returns("0288888888");
			mock1.Setup(x => x.TypeID).Returns("TL");

			var mock2 = new Mock<ICommunication>();
			mock2.Setup(x => x.ID).Returns("***@gmail.com");
			mock2.Setup(x => x.TypeID).Returns("EM");

			yield return mock1.Object;
			yield return mock2.Object;
		}

		IAddress GetImporterAddress()
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.Line).Returns("Address. Line. Text");
			mock.Setup(x => x.ChineseLine).Returns("進口人(納稅義務人)中文地址");
			return mock.Object;
		}

		IPartyDetails GetImporter()
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ID).Returns("1234643212");
			mock.Setup(x => x.Name).Returns("Importer name");
			mock.Setup(x => x.ChineseName).Returns("Name of the importer ( duty payer) in Chinese.");
			mock.Setup(x => x.TypeCode).Returns("X21");
			mock.Setup(x => x.Address).Returns(GetImporterAddress);
			mock.Setup(x => x.Communications).Returns(GetImporterCommunications);
			return mock.Object;
		}

		IAddress GetSellerAddress()
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.CountryCode).Returns("USA");
			mock.Setup(x => x.Line).Returns("Seller Address. Line. Text");
			mock.Setup(x => x.ChineseLine).Returns("出口人(或賣方)中文地址");
			return mock.Object;
		}

		IPartyDetails GetSeller(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IPartyDetails>();
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ID).Returns("Seller id");
				mock.Setup(x => x.ChineseName).Returns("Seller Chinese Name");
				mock.Setup(x => x.TypeCode).Returns("58");
			}
			mock.Setup(x => x.Name).Returns("Seller name");
			mock.Setup(x => x.Address).Returns(GetSellerAddress);
			return mock.Object;
		}

		IEnumerable<IShippingIdentification> GetShippingIdentifications()
		{
			var mock1 = new Mock<IShippingIdentification>();
			mock1.Setup(x => x.LotNumberID).Returns("Identification number of a production lot 1.");
			mock1.Setup(x => x.ProductBestBeforeDateTime).Returns(ZDateTime.BrettsBirthday.AddDays(10));
			mock1.Setup(x => x.ProductManufacturedDate).Returns(ZDateTime.BrettsBirthday);
			yield return mock1.Object;

			var mock2 = new Mock<IShippingIdentification>();
			mock2.Setup(x => x.LotNumberID).Returns("Identification number of a production lot 2.");
			mock2.Setup(x => x.ProductBestBeforeDateTime).Returns(ZDateTime.BrettsBirthday.AddDays(12));
			mock2.Setup(x => x.ProductManufacturedDate).Returns(ZDateTime.BrettsBirthday.AddDays(1));
			yield return mock2.Object;
		}

		IGoodsStatisticalMeasure GetGoodsStatisticalMeasure()
		{
			var mock = new Mock<IGoodsStatisticalMeasure>();
			mock.Setup(x => x.StatisticalUnitCode).Returns("PKG");
			mock.Setup(x => x.TariffQuantity).Returns(12.5645m);
			return mock.Object;
		}

		IGoodsMeasure GetGoodsMeasure()
		{
			var mock = new Mock<IGoodsMeasure>();
			mock.Setup(x => x.NetWeightMeasure).Returns(11.34m);
			mock.Setup(x => x.TariffQuantity).Returns(12);
			mock.Setup(x => x.UnitCode).Returns("PCE");
			return mock.Object;
		}

		ICommodityDutyTaxFee GetDutyTaxFee()
		{
			var mock = new Mock<ICommodityDutyTaxFee>();
			mock.Setup(x => x.AdValoremTaxBaseAmount).Returns(1125m);
			return mock.Object;
		}

		ICommodityRelatedPackaging GetGovernmentAgencyGoodsItemCommodityRelatedPackaging()
		{
			var mock = new Mock<ICommodityRelatedPackaging>();
			mock.Setup(x => x.PackingMethodDescription).Returns("C12");
			return mock.Object;
		}

		IClassification GetClassification()
		{
			var mock = new Mock<IClassification>();
			mock.Setup(x => x.ID).Returns("WCOID145");
			return mock.Object;
		}

		IConstituent GetGovernmentAgencyGoodsItemCommodityConstituent()
		{
			var mock = new Mock<IConstituent>();
			mock.Setup(x => x.ElementDescription).Returns("Element Description");
			return mock.Object;
		}

		ICommodity GetGovernmentAgencyGoodsItemCommodity(bool optionalNodesArePopulated)
		{
			var mock = new Mock<ICommodity>();
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.CommercialCategorizationID).Returns("Commodity. Commercial Categorization. Identifier");
				mock.Setup(x => x.TariffCodeExtensionCode).Returns("T");
			}
			mock.Setup(x => x.Description).Returns("Commodity. Description. Text");
			mock.Setup(x => x.ChineseDescription).Returns("貨物中文名稱(品名)");
			mock.Setup(x => x.Classification).Returns(GetClassification);
			mock.Setup(x => x.CommodityRelatedPackaging).Returns(GetGovernmentAgencyGoodsItemCommodityRelatedPackaging);
			mock.Setup(x => x.Constituent).Returns(GetGovernmentAgencyGoodsItemCommodityConstituent);
			mock.Setup(x => x.DutyTaxFee).Returns(GetDutyTaxFee);
			return mock.Object;
		}

		IEnumerable<IAdditionalInformation> GetGovernmentAgencyGoodsItemsAdditionalInformations()
		{
			var mock1 = new Mock<IAdditionalInformation>();
			mock1.Setup(x => x.StatementCode).Returns("A");
			mock1.Setup(x => x.StatementDescription).Returns("Statement Description. Text 1");
			yield return mock1.Object;

			var mock2 = new Mock<IAdditionalInformation>();
			mock2.Setup(x => x.StatementCode).Returns("B");
			mock2.Setup(x => x.StatementDescription).Returns("Statement Description. Text 2");
			yield return mock2.Object;
		}

		IEnumerable<IGovernmentAgencyGoodsItem> GetGovernmentAgencyGoodsItems(bool optionalNodesArePopulated)
		{
			var mock1 = new Mock<IGovernmentAgencyGoodsItem>();
			mock1.Setup(x => x.SequenceNumeric).Returns(1);
			var mock2 = new Mock<IGovernmentAgencyGoodsItem>();
			mock2.Setup(x => x.SequenceNumeric).Returns(2);
			if (optionalNodesArePopulated)
			{
				mock1.Setup(x => x.AdditionalInformations).Returns(GetGovernmentAgencyGoodsItemsAdditionalInformations);
				mock1.Setup(x => x.GoodsStatisticalMeasure).Returns(GetGoodsStatisticalMeasure);
				mock1.Setup(x => x.ShippingIdentifications).Returns(GetShippingIdentifications);

				mock2.Setup(x => x.AdditionalInformations).Returns(GetGovernmentAgencyGoodsItemsAdditionalInformations);
				mock2.Setup(x => x.GoodsStatisticalMeasure).Returns(GetGoodsStatisticalMeasure);
				mock2.Setup(x => x.ShippingIdentifications).Returns(GetShippingIdentifications);
			}
			mock1.Setup(x => x.Commodity).Returns(GetGovernmentAgencyGoodsItemCommodity(optionalNodesArePopulated));
			mock1.Setup(x => x.GoodsMeasure).Returns(GetGoodsMeasure);

			mock2.Setup(x => x.Commodity).Returns(GetGovernmentAgencyGoodsItemCommodity(optionalNodesArePopulated));
			mock2.Setup(x => x.GoodsMeasure).Returns(GetGoodsMeasure);
			yield return mock1.Object;
			yield return mock2.Object;
		}

		IEnumerable<ITransportEquipment> GetTransportEquipments()
		{
			var mock1 = new Mock<ITransportEquipment>();
			mock1.Setup(x => x.CharacteristicCode).Returns("MAB3");
			mock1.Setup(x => x.ID).Returns("MSCU5285728");
			yield return mock1.Object;

			var mock2 = new Mock<ITransportEquipment>();
			mock2.Setup(x => x.CharacteristicCode).Returns("MAB5");
			mock2.Setup(x => x.ID).Returns("MSCU5285725");
			yield return mock2.Object;
		}

		ILocation GetLoadingLocation()
		{
			var mock = new Mock<ILocation>();
			mock.Setup(x => x.ID).Returns("TWKEL");
			return mock.Object;
		}

		IEnumerable<IAdditionalInformation> GetGoodsShipmentConsignmentAdditionalInformations()
		{
			var mock1 = new Mock<IAdditionalInformation>();
			mock1.Setup(x => x.StatementCode).Returns("A");
			mock1.Setup(x => x.StatementDescription).Returns("Statement Description. Text 1");
			yield return mock1.Object;

			var mock2 = new Mock<IAdditionalInformation>();
			mock2.Setup(x => x.StatementCode).Returns("B");
			mock2.Setup(x => x.StatementDescription).Returns("Statement Description. Text 2");
			yield return mock2.Object;
		}

		ITransportMeans GetGoodsShipmentConsignmentBorderTransportMeans()
		{
			var transportMeans = new Mock<ITransportMeans>();
			transportMeans.Setup(x => x.ID).Returns("WCOIDT006");
			transportMeans.Setup(x => x.JourneyID).Returns("Journey");
			return transportMeans.Object;
		}

		IEnumerable<ITransportContractDocument> GetGoodsShipmentConsignmentTransportContractDocuments()
		{
			var mock1 = new Mock<ITransportContractDocument>();
			mock1.Setup(x => x.ID).Returns("MSKU 907032-3");
			mock1.Setup(x => x.TypeCode).Returns("M1");
			var mock2 = new Mock<ITransportContractDocument>();
			mock2.Setup(x => x.ID).Returns("MSCU5285725");
			mock1.Setup(x => x.TypeCode).Returns("M2");
			yield return mock1.Object;
			yield return mock2.Object;
		}

		IConsignment GetGoodsShipmentConsignment(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IConsignment>();
			mock.Setup(x => x.BorderTransportMeans).Returns(GetGoodsShipmentConsignmentBorderTransportMeans);
			mock.Setup(x => x.GoodsLocation).Returns("USLAX");
			mock.Setup(x => x.LoadingLocation).Returns(GetLoadingLocation);
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.AdditionalInformations).Returns(GetGoodsShipmentConsignmentAdditionalInformations);
				mock.Setup(x => x.TransportContractDocuments).Returns(GetGoodsShipmentConsignmentTransportContractDocuments);
				mock.Setup(x => x.TransportEquipments).Returns(GetTransportEquipments);
			}
			return mock.Object;
		}

		IGoodsShipment GetGoodsShipment(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IGoodsShipment>();
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ExitDateTime).Returns(ZDateTime.BrettsBirthday);
			}
			mock.Setup(x => x.Consignment).Returns(GetGoodsShipmentConsignment(optionalNodesArePopulated));
			mock.Setup(x => x.GovernmentAgencyGoodsItems).Returns(GetGovernmentAgencyGoodsItems(optionalNodesArePopulated));
			mock.Setup(x => x.Seller).Returns(GetSeller(optionalNodesArePopulated));
			return mock.Object;
		}

		ICurrencyExchange GetCurrencyExchange()
		{
			var mock = new Mock<ICurrencyExchange>();
			mock.Setup(x => x.CurrencyTypeCode).Returns("USD");
			return mock.Object;
		}

		IOrigin GetOrigin()
		{
			var mock = new Mock<IOrigin>();
			mock.Setup(x => x.CountryCode).Returns("FR");
			return mock.Object;
		}

		ICommodity GetCommodity()
		{
			var mock = new Mock<ICommodity>();
			mock.Setup(x => x.Name).Returns("Commodity. Name. Text");
			return mock.Object;
		}

		IConsignmentItem GetConsignmentItem()
		{
			var mock = new Mock<IConsignmentItem>();
			mock.Setup(x => x.Commodity).Returns(GetCommodity);
			mock.Setup(x => x.Origin).Returns(GetOrigin);
			return mock.Object;
		}

		IAddress GetGovernmentAgencyGoodsItemManufacturerAddress()
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.Line).Returns("Address. Line. Text");
			mock.Setup(x => x.CountrySubDivisionID).Returns("Sub");
			mock.Setup(x => x.CountrySubDivisionName).Returns("Sub name");
			return mock.Object;
		}

		IEnumerable<ICommunication> GetGovernmentAgencyGoodsItemManufacturerCommunications()
		{
			var mock1 = new Mock<ICommunication>();
			mock1.Setup(x => x.ID).Returns("0378451314");
			mock1.Setup(x => x.TypeID).Returns("TL");
			yield return mock1.Object;

			var mock2 = new Mock<ICommunication>();
			mock2.Setup(x => x.ID).Returns("x22@hotmail.com");
			mock2.Setup(x => x.TypeID).Returns("EM");
			yield return mock2.Object;
		}

		IPartyDetails GetGovernmentAgencyGoodsItemManufacturer(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IPartyDetails>();
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ID).Returns("Manuf ID");
				mock.Setup(x => x.Address).Returns(GetGovernmentAgencyGoodsItemManufacturerAddress);
				mock.Setup(x => x.Communications).Returns(GetGovernmentAgencyGoodsItemManufacturerCommunications);
				mock.Setup(x => x.ContactName).Returns("Contact. Name. Text");
			}
			mock.Setup(x => x.Name).Returns("Manufacture Name");

			return mock.Object;
		}

		IGovernmentAgencyGoodsItem GetGovernmentAgencyGoodsItem(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IGovernmentAgencyGoodsItem>();
			mock.Setup(x => x.Manufacturer).Returns(GetGovernmentAgencyGoodsItemManufacturer(optionalNodesArePopulated));
			return mock.Object;
		}

		IConsignment GetConsignment(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IConsignment>();
			mock.Setup(x => x.ConsignmentItem).Returns(GetConsignmentItem);
			mock.Setup(x => x.GovernmentAgencyGoodsItem).Returns(GetGovernmentAgencyGoodsItem(optionalNodesArePopulated));
			return mock.Object;
		}

		ITransportMeans GetITransportMeans()
		{
			var transportMeans = new Mock<ITransportMeans>();
			transportMeans.Setup(x => x.ArrivalDateTime).Returns(new ZDate(2023, 4, 11));
			transportMeans.Setup(x => x.TypeCode).Returns("Z");
			return transportMeans.Object;
		}

		IDeclarationAgent GetAgent()
		{
			var declarationAgent = new Mock<IDeclarationAgent>();
			declarationAgent.Setup(x => x.ID).Returns("01A");
			declarationAgent.Setup(x => x.RoleCode).Returns("AD");
			declarationAgent.Setup(x => x.SubBoxID).Returns("F");
			return declarationAgent.Object;
		}

		IDeclarationAdditionalDocument GetAdditionalDocument()
		{
			var declarationAdditionalDocument = new Mock<IDeclarationAdditionalDocument>();
			declarationAdditionalDocument.Setup(x => x.ID).Returns("WCOIDD001");
			return declarationAdditionalDocument.Object;
		}
	}
}
