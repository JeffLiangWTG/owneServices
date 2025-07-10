using System.Collections.Generic;
using CargoWise.Customs.TW.MessageDefinitions.NX301_DN;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	sealed class NX301_DNMessageBuilderTest : BaseTWMessageBuilderTest<INX301_DNDeclaration, Declaration>
	{
		[ExpectNoExceptions]
		public override void TestSerializeToMessageString()
		{
			var expectedString = TWMessageBuilder.SerializeToMessageString(Declaration, "9");
			AssertXMLContains("<FunctionCode>9</FunctionCode>", expectedString);
			AsserContainsXmlValueByType(expectedString, Declaration, typeof(INX301_DNDeclaration), IgnoreAssertPropertyNames);
		}

		IEnumerable<ZString> IgnoreAssertPropertyNames
		{
			get
			{
				yield return "Enterprise.Customs.TW.Messaging.IAppointment.ReservationDate";
			}
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclarationWithOptionalNodes()
		{
			var message = GetMessage();
			var declaration = new NX301_DNMessageBuilder().PopulateDeclaration(message);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("NX301_DN.xml");

			CombineAssertions(() =>
			{
				AssertXMLContains(expected, actual);
				AsserContainsXmlByType(actual, declaration.GetType());
			});
		}

		public void TestPopulateDeclarationWithoutOptionalNodes()
		{
			var message = GetMessage(false);
			var declaration = new NX301_DNMessageBuilder().PopulateDeclaration(message);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("NX301_DN_WithoutOptionalNodes.xml");
			AssertXMLContains(expected, actual);
		}

		protected override INX301_DNDeclaration CreateDataSource()
		{
			return GetMessage();
		}

		protected override ITWMessageBuilder CreateNewMessageBuilder()
		{
			return new NX301_DNMessageBuilder();
		}

		INX301_DNDeclaration GetMessage(bool optionalNodesArePopulated = true)
		{
			var mock = new Mock<INX301_DNDeclaration>();
			mock.Setup(x => x.FunctionalReferenceID).Returns("12345678002111070001");
			mock.Setup(x => x.FunctionCode).Returns("9");
			mock.Setup(x => x.ID).Returns("A0123456789");
			mock.Setup(x => x.BorderTransportMeans).Returns(GetITransportMeans);
			mock.Setup(x => x.Consignment).Returns(GetConsignment);
			mock.Setup(x => x.CurrencyExchange).Returns(GetCurrencyExchange);
			mock.Setup(x => x.GoodsShipment).Returns(GetGoodsShipment(optionalNodesArePopulated));
			mock.Setup(x => x.Importer).Returns(GetImporter);
			mock.Setup(x => x.Application).Returns(GetApplication(optionalNodesArePopulated));

			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.TypeCode).Returns("BA");
				mock.Setup(x => x.AdditionalDocument).Returns(GetAdditionalDocument);
				mock.Setup(x => x.Agent).Returns(GetAgent);
			}

			return mock.Object;
		}

		IPreviousDocument GetApplicationWinePreviousDocument()
		{
			var mock = new Mock<IPreviousDocument>();
			mock.Setup(x => x.ID).Returns("ID0123456789");
			return mock.Object;
		}

		IEnumerable<IAdditionalDocument> GetApplicationWineAdditionalDocuments()
		{
			var mock1 = new Mock<IAdditionalDocument>();
			mock1.Setup(x => x.ID).Returns("Q");
			yield return mock1.Object;

			var mock2 = new Mock<IAdditionalDocument>();
			mock2.Setup(x => x.ID).Returns("R");
			yield return mock2.Object;
		}

		IApplicationWine GetApplicationWine(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IApplicationWine>();
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.AdditionalDocuments).Returns(GetApplicationWineAdditionalDocuments);
				mock.Setup(x => x.PreviousDocument).Returns(GetApplicationWinePreviousDocument);
			}
			mock.Setup(x => x.GovernmentProcedurePreviousCode).Returns("S");
			return mock.Object;
		}

		IAdditionalDocument GetApplicationAuthorizedInformationAdditionalDocument()
		{
			var mock = new Mock<IAdditionalDocument>();
			mock.Setup(x => x.ID).Returns("AAIAD");
			return mock.Object;
		}

		IAuthorizedInformation GetApplicationAuthorizedInformation()
		{
			var mock = new Mock<IAuthorizedInformation>();
			mock.Setup(x => x.AuthorizedTypeCode).Returns("K");
			mock.Setup(x => x.AdditionalDocument).Returns(GetApplicationAuthorizedInformationAdditionalDocument);
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

		IApplicationAdditionalInformation GetApplicationAdditionalInformation()
		{
			var mock = new Mock<IApplicationAdditionalInformation>();
			mock.Setup(x => x.StatementDescription).Returns("Description of an additional statement.");
			mock.Setup(x => x.AddressChineseLine).Returns("Address. Chinese Line. Text");
			return mock.Object;
		}

		IEnumerable<IAdditionalDocument> GetApplicationAdditionalDocuments()
		{
			var mock1 = new Mock<IAdditionalDocument>();
			mock1.Setup(x => x.ID).Returns("Additional Document. Identification. Identifier");
			mock1.Setup(x => x.Content).Returns("Additional Document. Content. Text");
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
			mock.Setup(x => x.PurposeCode).Returns("F3");
			mock.Setup(x => x.TypeCode).Returns("P12");
			mock.Setup(x => x.Payment).Returns(GetApplicationPayment);
			mock.Setup(x => x.Wine).Returns(GetApplicationWine(optionalNodesArePopulated));
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.AdditionalDocuments).Returns(GetApplicationAdditionalDocuments);
				mock.Setup(x => x.AdditionalInformation).Returns(GetApplicationAdditionalInformation);
				mock.Setup(x => x.Agent).Returns(GetApplicationAgent);
				mock.Setup(x => x.BankAccount).Returns("2152125160");
				mock.Setup(x => x.Appointment).Returns(GetApplicationAppointment);
				mock.Setup(x => x.AuthorizedInformation).Returns(GetApplicationAuthorizedInformation);
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
			mock.Setup(x => x.ChineseLine).Returns("Address. Chinese Line. Text");
			return mock.Object;
		}

		IPartyDetails GetImporter()
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ID).Returns("1234643212");
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
			return mock.Object;
		}

		IPartyDetails GetSeller()
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.Address).Returns(GetSellerAddress);
			return mock.Object;
		}

		IEnumerable<IShippingIdentification> GetShippingIdentifications()
		{
			var mock1 = new Mock<IShippingIdentification>();
			mock1.Setup(x => x.LotNumberID).Returns("Identification number of a production lot 1.");
			mock1.Setup(x => x.ProductLotNumberAmount).Returns(1.23342m);
			mock1.Setup(x => x.ProductManufacturedDate).Returns(ZDateTime.BrettsBirthday);
			yield return mock1.Object;

			var mock2 = new Mock<IShippingIdentification>();
			mock2.Setup(x => x.LotNumberID).Returns("Identification number of a production lot 2.");
			mock2.Setup(x => x.ProductLotNumberAmount).Returns(4.53337m);
			mock2.Setup(x => x.ProductManufacturedDate).Returns(ZDateTime.BrettsBirthday.AddDays(1));
			yield return mock2.Object;
		}

		IPreviousDocument GetPreBondedDocument()
		{
			var mock = new Mock<IPreviousDocument>();
			mock.Setup(x => x.ID).Returns("123456789");
			mock.Setup(x => x.LineNumeric).Returns(1234);
			return mock.Object;
		}

		IGoodsStatisticalMeasure GetGoodsStatisticalMeasure()
		{
			var mock = new Mock<IGoodsStatisticalMeasure>();
			mock.Setup(x => x.StatisticalUnitCode).Returns("PKG");
			mock.Setup(x => x.TariffQuantity).Returns(12.5645m);
			return mock.Object;
		}

		IPackaging GetPackaging()
		{
			var mock = new Mock<IPackaging>();
			mock.Setup(x => x.QuantityQuantity).Returns(1);
			mock.Setup(x => x.TypeCode).Returns("PLT");
			return mock.Object;
		}

		IAddress GetManufacturerAddress()
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.Line).Returns("Address. Line. Text");
			return mock.Object;
		}

		IPartyDetails GetManufacturer()
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.Name).Returns("Manufacturer. Name. Text");
			mock.Setup(x => x.Address).Returns(GetManufacturerAddress);
			return mock.Object;
		}

		IGoodsMeasure GetGoodsMeasure()
		{
			var mock = new Mock<IGoodsMeasure>();
			mock.Setup(x => x.TariffQuantity).Returns(12);
			mock.Setup(x => x.UnitCode).Returns("PCE");
			return mock.Object;
		}

		IWine GetWine(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IWine>();
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.AgeNumeric).Returns(10);
				mock.Setup(x => x.BottledDate).Returns(ZDateTime.BrettsBirthday);
				mock.Setup(x => x.CoverLotNumberAmount).Returns(15.4m);
				mock.Setup(x => x.GeographicRegion).Returns("Wine. Geographic Region. Text");
				mock.Setup(x => x.OriginalNonLotNumberAmount).Returns(14.3m);
				mock.Setup(x => x.ProductBestBeforeDateTime).Returns(ZDateTime.BrettsBirthday.AddDays(50));
				mock.Setup(x => x.ProductExpiryDateTime).Returns(ZDateTime.BrettsBirthday.AddDays(100));
				mock.Setup(x => x.RemoveLotNumberAmount).Returns(11.6m);
				mock.Setup(x => x.YearNumeric).Returns(30);
			}
			mock.Setup(x => x.AlcoholContentNumeric).Returns(12);
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
			mock.Setup(x => x.Specification).Returns("Commodity Related Packaging. Specification. Text");
			return mock.Object;
		}

		IClassification GetClassification()
		{
			var mock = new Mock<IClassification>();
			mock.Setup(x => x.ID).Returns("WCOID145");
			return mock.Object;
		}

		ICommodity GetGovernmentAgencyGoodsItemCommodity(bool optionalNodesArePopulated)
		{
			var mock = new Mock<ICommodity>();
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ChineseDescription).Returns("貨物中文名稱(品名)");
				mock.Setup(x => x.CommodityRelatedPackaging).Returns(GetGovernmentAgencyGoodsItemCommodityRelatedPackaging);
			}
			mock.Setup(x => x.EnglishDescription).Returns("Commodity. English Description. Text");
			mock.Setup(x => x.Classification).Returns(GetClassification);
			mock.Setup(x => x.DutyTaxFee).Returns(GetDutyTaxFee);
			mock.Setup(x => x.Wine).Returns(GetWine(optionalNodesArePopulated));
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
				mock1.Setup(x => x.PreBondedDocument).Returns(GetPreBondedDocument);
				mock1.Setup(x => x.ShippingIdentifications).Returns(GetShippingIdentifications);

				mock2.Setup(x => x.AdditionalInformations).Returns(GetGovernmentAgencyGoodsItemsAdditionalInformations);
				mock2.Setup(x => x.PreBondedDocument).Returns(GetPreBondedDocument);
				mock2.Setup(x => x.ShippingIdentifications).Returns(GetShippingIdentifications);
			}
			mock1.Setup(x => x.Commodity).Returns(GetGovernmentAgencyGoodsItemCommodity(optionalNodesArePopulated));
			mock1.Setup(x => x.GoodsMeasure).Returns(GetGoodsMeasure);
			mock1.Setup(x => x.Manufacturer).Returns(GetManufacturer);
			mock1.Setup(x => x.Packaging).Returns(GetPackaging);
			mock1.Setup(x => x.GoodsStatisticalMeasure).Returns(GetGoodsStatisticalMeasure);

			mock2.Setup(x => x.Commodity).Returns(GetGovernmentAgencyGoodsItemCommodity(optionalNodesArePopulated));
			mock2.Setup(x => x.GoodsMeasure).Returns(GetGoodsMeasure);
			mock2.Setup(x => x.Manufacturer).Returns(GetManufacturer);
			mock2.Setup(x => x.Packaging).Returns(GetPackaging);
			mock2.Setup(x => x.GoodsStatisticalMeasure).Returns(GetGoodsStatisticalMeasure);
			yield return mock1.Object;
			yield return mock2.Object;
		}

		IEnumerable<ITransportEquipment> GetTransportEquipments()
		{
			var mock1 = new Mock<ITransportEquipment>();
			mock1.Setup(x => x.ID).Returns("MSKU 907032-3");
			var mock2 = new Mock<ITransportEquipment>();
			mock2.Setup(x => x.ID).Returns("MSCU5285725");
			yield return mock1.Object;
			yield return mock2.Object;
		}

		ILocation GetLoadingLocation()
		{
			var mock = new Mock<ILocation>();
			mock.Setup(x => x.ID).Returns("TWKEL");
			return mock.Object;
		}

		IConsignment GetGoodsShipmentConsignment(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IConsignment>();
			mock.Setup(x => x.GoodsLocation).Returns("USLAX");
			mock.Setup(x => x.LoadingLocation).Returns(GetLoadingLocation);
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.TransportEquipments).Returns(GetTransportEquipments);
			}
			return mock.Object;
		}

		IGoodsShipment GetGoodsShipment(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IGoodsShipment>();
			mock.Setup(x => x.Consignment).Returns(GetGoodsShipmentConsignment(optionalNodesArePopulated));
			mock.Setup(x => x.GovernmentAgencyGoodsItems).Returns(GetGovernmentAgencyGoodsItems(optionalNodesArePopulated));
			mock.Setup(x => x.Seller).Returns(GetSeller);
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

		ICommodityRelatedPackaging GetCommodityRelatedPackaging()
		{
			var mock = new Mock<ICommodityRelatedPackaging>();
			mock.Setup(x => x.PackingMethodDescription).Returns("PLT");
			mock.Setup(x => x.MaterialCode).Returns("PMC");
			return mock.Object;
		}

		ICommodity GetCommodity()
		{
			var mock = new Mock<ICommodity>();
			mock.Setup(x => x.Description).Returns("Commodity. Description. Text");
			mock.Setup(x => x.GoodsGroupNameCode).Returns("ID332");
			mock.Setup(x => x.CommodityRelatedPackaging).Returns(GetCommodityRelatedPackaging);
			return mock.Object;
		}

		IConsignmentItem GetConsignmentItem()
		{
			var mock = new Mock<IConsignmentItem>();
			mock.Setup(x => x.Commodity).Returns(GetCommodity);
			mock.Setup(x => x.Origin).Returns(GetOrigin);
			return mock.Object;
		}

		IConsignment GetConsignment()
		{
			var mock = new Mock<IConsignment>();
			mock.Setup(x => x.ConsignmentItem).Returns(GetConsignmentItem);
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
