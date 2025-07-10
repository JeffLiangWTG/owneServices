using System.Collections.Generic;
using CargoWise.Customs.TW.MessageDefinitions.NX601;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	[TestedType(typeof(NX601MessageBuilder))]
	sealed class NX601MessageBuilderTest : BaseTWMessageBuilderTest<INX601Declaration, Declaration>
	{
		[ExpectNoExceptions]
		public override void TestSerializeToMessageString()
		{
			var expectedString = TWMessageBuilder.SerializeToMessageString(Declaration, "9");
			AssertXMLContains("<FunctionCode>9</FunctionCode>", expectedString);
			AsserContainsXmlValueByType(expectedString, Declaration, typeof(INX601Declaration), IgnoreAssertPropertyNames);
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
			var declaration = new NX601MessageBuilder().PopulateDeclaration(message);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("NX601.xml");

			CombineAssertions(() =>
			{
				AssertXMLContains(expected, actual);
				AsserContainsXmlByType(actual, declaration.GetType());
			});
		}

		public void TestPopulateDeclarationWithoutOptionalNodes()
		{
			var message = GetMessage(false);
			var declaration = new NX601MessageBuilder().PopulateDeclaration(message);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("NX601_WithoutOptionalNodes.xml");
			AssertXMLContains(expected, actual);
		}

		protected override INX601Declaration CreateDataSource()
		{
			return GetMessage();
		}

		protected override ITWMessageBuilder CreateNewMessageBuilder()
		{
			return new NX601MessageBuilder();
		}

		INX601Declaration GetMessage(bool optionalNodesArePopulated = true)
		{
			var mock = new Mock<INX601Declaration>();
			mock.Setup(x => x.FunctionalReferenceID).Returns("12345678002111070001");
			mock.Setup(x => x.FunctionCode).Returns("9");
			mock.Setup(x => x.ID).Returns("A0123456789");
			mock.Setup(x => x.TypeCode).Returns("G1");
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

		IAppointment GetApplicationAppointment()
		{
			var mock = new Mock<IAppointment>();
			mock.Setup(x => x.ReservationDate).Returns(ZDateTime.BrettsBirthday);
			mock.Setup(x => x.ReservationPeriodCode).Returns("C");
			return mock.Object;
		}

		IPayment GetApplicationPayment()
		{
			var mock = new Mock<IPayment>();
			mock.Setup(x => x.MethodCode).Returns("CA");
			return mock.Object;
		}

		IAddress GetApplicationAgentAddress()
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.ChineseLine).Returns("報驗/申辦代理人中文地址");
			return mock.Object;
		}

		IEnumerable<ICommunication> GetApplicationAgentCommunications()
		{
			var mock1 = new Mock<ICommunication>();
			mock1.Setup(x => x.ID).Returns("0288888445");
			mock1.Setup(x => x.TypeID).Returns("TL");

			var mock2 = new Mock<ICommunication>();
			mock2.Setup(x => x.ID).Returns("abc**@gmail.com");
			mock2.Setup(x => x.TypeID).Returns("EM");

			yield return mock1.Object;
			yield return mock2.Object;
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

		IApplicationAdditionalInformation GetApplicationAdditionalInformation()
		{
			var mock = new Mock<IApplicationAdditionalInformation>();
			mock.Setup(x => x.StatementDescription).Returns("Description of an additional statement.");
			mock.Setup(x => x.DeductionSample).Returns("Reason to reduce sampling.");
			mock.Setup(x => x.ProvedPaper).Returns("F");
			mock.Setup(x => x.ReturnSample).Returns("T");
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

		IAddress GetApplicationLocalManufacturerAddress()
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.ChineseLine).Returns("國內負責廠商中文地址");
			return mock.Object;
		}

		IEnumerable<ICommunication> GetApplicationLocalManufacturerCommunications()
		{
			var mock1 = new Mock<ICommunication>();
			mock1.Setup(x => x.ID).Returns("0266666666");
			mock1.Setup(x => x.TypeID).Returns("TL");

			var mock2 = new Mock<ICommunication>();
			mock2.Setup(x => x.ID).Returns("f2**@gmail.com");
			mock2.Setup(x => x.TypeID).Returns("EM");

			yield return mock1.Object;
			yield return mock2.Object;
		}

		IPartyDetails GetApplicationLocalManufacturer()
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ChineseName).Returns("Local Manufacturer. Chinese Name. Text");
			mock.Setup(x => x.Address).Returns(GetApplicationLocalManufacturerAddress);
			mock.Setup(x => x.Communications).Returns(GetApplicationLocalManufacturerCommunications);
			return mock.Object;
		}

		IApplication GetApplication(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IApplication>();
			mock.Setup(x => x.ContactOffice).Returns("WCOIDG002");
			mock.Setup(x => x.Payment).Returns(GetApplicationPayment);
			mock.Setup(x => x.LocalManufacturer).Returns(GetApplicationLocalManufacturer);
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.AdditionalDocuments).Returns(GetApplicationAdditionalDocuments);
				mock.Setup(x => x.AdditionalInformation).Returns(GetApplicationAdditionalInformation());
				mock.Setup(x => x.Agent).Returns(GetApplicationAgent);
				mock.Setup(x => x.Appointment).Returns(GetApplicationAppointment);
			}
			return mock.Object;
		}

		IAddress GetImporterAddress()
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.Line).Returns("Address Line. Text");
			mock.Setup(x => x.ChineseLine).Returns("Address. Chinese Line. Text");
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

		IPartyDetails GetImporter()
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ID).Returns("1234643212");
			mock.Setup(x => x.Name).Returns("Importer Name");
			mock.Setup(x => x.ChineseName).Returns("Name of the importer ( duty payer) in Chinese.");
			mock.Setup(x => x.TypeCode).Returns("X21");
			mock.Setup(x => x.Address).Returns(GetImporterAddress);
			mock.Setup(x => x.Communications).Returns(GetImporterCommunications);
			return mock.Object;
		}

		IAddress GetSellerAddress(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.CountryCode).Returns("USA");
			mock.Setup(x => x.Line).Returns("Seller Address. Line. Text");
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ChineseLine).Returns("Seller Chinese. Line. Text");
			}
			return mock.Object;
		}

		IEnumerable<ICommunication> GetSellerCommunications()
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

		IPartyDetails GetSeller(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IPartyDetails>();
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ID).Returns("SID");
				mock.Setup(x => x.ChineseName).Returns("Seller. Chinese Name. Text");
				mock.Setup(x => x.TypeCode).Returns("STC");
				mock.Setup(x => x.Communications).Returns(GetSellerCommunications);
				mock.Setup(x => x.ContactName).Returns("Seller Contact. Name. Text");
			}
			mock.Setup(x => x.Name).Returns("Seller. Name");
			mock.Setup(x => x.Address).Returns(GetSellerAddress(optionalNodesArePopulated));
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
			mock2.Setup(x => x.ProductBestBeforeDateTime).Returns(ZDateTime.BrettsBirthday.AddDays(11));
			mock2.Setup(x => x.ProductManufacturedDate).Returns(ZDateTime.BrettsBirthday.AddDays(1));
			yield return mock2.Object;
		}

		IGoodsMeasure GetGoodsMeasure()
		{
			var mock = new Mock<IGoodsMeasure>();
			mock.Setup(x => x.NetWeightMeasure).Returns(1.23m);
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

		ICommodityRelatedPackaging GetGovernmentAgencyGoodsItemCommodityRelatedPackaging(bool optionalNodesArePopulated)
		{
			var mock = new Mock<ICommodityRelatedPackaging>();
			mock.Setup(x => x.PackingMethodDescription).Returns("ABC");
			mock.Setup(x => x.MaterialCode).Returns("PCE");
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.Specification).Returns("TW110");
			}
			return mock.Object;
		}

		IClassification GetClassification()
		{
			var mock = new Mock<IClassification>();
			mock.Setup(x => x.ID).Returns("WCOID145");
			return mock.Object;
		}

		IEnumerable<IFoodConstituent> GetFoodConstituents(bool optionalNodesArePopulated)
		{
			var mock1 = new Mock<IFoodConstituent>();
			var mock2 = new Mock<IFoodConstituent>();
			mock1.Setup(x => x.ElementName).Returns("WCOID348");
			mock2.Setup(x => x.ElementName).Returns("WCOID358");
			if (optionalNodesArePopulated)
			{
				mock1.Setup(x => x.ElementPercentNumeric).Returns(2.4m);
				mock2.Setup(x => x.ElementPercentNumeric).Returns(5.2m);
			}
			yield return mock1.Object;
			yield return mock2.Object;
		}

		IFood GetFood(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IFood>();
			mock.Setup(x => x.PHValueNumeric).Returns(4.3m);
			mock.Setup(x => x.SterilizationValueNumeric).Returns(9.6m);
			mock.Setup(x => x.Constituents).Returns(GetFoodConstituents(optionalNodesArePopulated));
			return mock.Object;
		}

		ICommodity GetGovernmentAgencyGoodsItemCommodity(bool optionalNodesArePopulated)
		{
			var mock = new Mock<ICommodity>();
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.CommercialCategorizationID).Returns("Commercial Categorization");
				mock.Setup(x => x.Description).Returns("Commodity. Description. Text");
				mock.Setup(x => x.BarCode).Returns("Barcode");
				mock.Setup(x => x.TariffCodeExtensionCode).Returns("F");
				mock.Setup(x => x.DutyTaxFee).Returns(GetDutyTaxFee);
				mock.Setup(x => x.Food).Returns(GetFood(optionalNodesArePopulated));
			}
			mock.Setup(x => x.GoodsGroupNameCode).Returns("ID332");
			mock.Setup(x => x.ChineseDescription).Returns("貨物中文名稱(品名)");
			mock.Setup(x => x.EnglishDescription).Returns("English Description");
			mock.Setup(x => x.Classification).Returns(GetClassification);
			mock.Setup(x => x.CommodityRelatedPackaging).Returns(GetGovernmentAgencyGoodsItemCommodityRelatedPackaging(optionalNodesArePopulated));
			mock.Setup(x => x.Constituent).Returns(GetGovernmentAgencyGoodsItemCommodityConstituent);
			mock.Setup(x => x.HandlingInstructionsCodes).Returns(new ZString[] { "A", "B", "C" });
			return mock.Object;
		}

		IConstituent GetGovernmentAgencyGoodsItemCommodityConstituent()
		{
			var mock = new Mock<IConstituent>();
			mock.Setup(x => x.ElementDescription).Returns("Element Description");
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
				mock1.Setup(x => x.ShippingIdentifications).Returns(GetShippingIdentifications);
				mock2.Setup(x => x.AdditionalInformations).Returns(GetGovernmentAgencyGoodsItemsAdditionalInformations);
				mock2.Setup(x => x.ShippingIdentifications).Returns(GetShippingIdentifications);
			}
			mock1.Setup(x => x.Commodity).Returns(GetGovernmentAgencyGoodsItemCommodity(optionalNodesArePopulated));
			mock1.Setup(x => x.GoodsMeasure).Returns(GetGoodsMeasure);
			mock2.Setup(x => x.Commodity).Returns(GetGovernmentAgencyGoodsItemCommodity(optionalNodesArePopulated));
			mock2.Setup(x => x.GoodsMeasure).Returns(GetGoodsMeasure);
			yield return mock1.Object;
			yield return mock2.Object;
		}

		IEnumerable<ITransportContractDocument> GetTransportContractDocuments()
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

		ILocation GetLoadingLocation()
		{
			var mock = new Mock<ILocation>();
			mock.Setup(x => x.ID).Returns("TWKEL");
			return mock.Object;
		}

		ITransportMeans GetGoodsShipmentConsignmentBorderTransportMeans()
		{
			var transportMeans = new Mock<ITransportMeans>();
			transportMeans.Setup(x => x.ID).Returns("WCOIDT006");
			transportMeans.Setup(x => x.JourneyID).Returns("Journey");
			return transportMeans.Object;
		}

		IEnumerable<ITransportEquipment> GetTransportEquipments()
		{
			var mock1 = new Mock<ITransportEquipment>();
			mock1.Setup(x => x.ID).Returns("MSCU5285728");
			var mock2 = new Mock<ITransportEquipment>();
			mock2.Setup(x => x.ID).Returns("MSCU5285725");
			yield return mock1.Object;
			yield return mock2.Object;
		}

		IConsignment GetGoodsShipmentConsignment(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IConsignment>();
			mock.Setup(x => x.GoodsLocation).Returns("USLAX");
			mock.Setup(x => x.LoadingLocation).Returns(GetLoadingLocation);
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ManifestSerialNumber).Returns("T153");
				mock.Setup(x => x.BorderTransportMeans).Returns(GetGoodsShipmentConsignmentBorderTransportMeans);
				mock.Setup(x => x.TransportContractDocuments).Returns(GetTransportContractDocuments);
				mock.Setup(x => x.TransportEquipments).Returns(GetTransportEquipments);
			}
			return mock.Object;
		}

		IGoodsShipment GetGoodsShipment(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IGoodsShipment>();
			mock.Setup(x => x.ExitDateTime).Returns(ZDateTime.BrettsBirthday);
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

		ITransportMeans GetITransportMeans()
		{
			var transportMeans = new Mock<ITransportMeans>();
			transportMeans.Setup(x => x.ArrivalDateTime).Returns(new ZDate(2023, 4, 25));
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
