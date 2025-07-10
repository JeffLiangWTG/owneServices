using System.Collections.Generic;
using CargoWise.Customs.TW.MessageDefinitions.NX603;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	[TestedType(typeof(NX603MessageBuilder))]
	sealed class NX603MessageBuilderTest : BaseTWMessageBuilderTest<INX603Declaration, Declaration>
	{
		[ExpectNoExceptions]
		public override void TestSerializeToMessageString()
		{
			var expectedString = TWMessageBuilder.SerializeToMessageString(Declaration, "9");
			AssertXMLContains("<FunctionCode>9</FunctionCode>", expectedString);
			AsserContainsXmlValueByType(expectedString, Declaration, typeof(INX603Declaration), IgnoreAssertPropertyNames);
		}

		IEnumerable<ZString> IgnoreAssertPropertyNames
		{
			get
			{
				yield return "Enterprise.Customs.TW.Messaging.IGoodsShipment.ExitDateTime";
				yield return "Enterprise.Customs.TW.Messaging.IAppointment.ExitDateTime";
				yield return "Enterprise.Customs.TW.Messaging.IAppointment.ReservationDate";
			}
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclarationWithOptionalNodes()
		{
			var message = GetMessage();
			var declaration = new NX603MessageBuilder().PopulateDeclaration(message);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("NX603.xml");

			CombineAssertions(() =>
			{
				AssertXMLContains(expected, actual);
				AsserContainsXmlByType(actual, declaration.GetType());
			});
		}

		public void TestPopulateDeclarationWithoutOptionalNodes()
		{
			var message = GetMessage(false);
			var declaration = new NX603MessageBuilder().PopulateDeclaration(message);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("NX603_WithoutOptionalNodes.xml");
			AssertXMLContains(expected, actual);
		}

		protected override INX603Declaration CreateDataSource() => GetMessage();

		protected override ITWMessageBuilder CreateNewMessageBuilder() => new NX603MessageBuilder();

		IAddress GetSellerAddress()
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.CountryCode).Returns("US");
			return mock.Object;
		}

		IPartyDetails GetSeller()
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ID).Returns("SID");
			mock.Setup(x => x.ChineseName).Returns("Seller. Chinese Name. Text");
			mock.Setup(x => x.TypeCode).Returns("STC");
			mock.Setup(x => x.Address).Returns(GetSellerAddress);
			return mock.Object;
		}

		IEnumerable<IShippingIdentification> GetShippingIdentifications()
		{
			var mock1 = new Mock<IShippingIdentification>();
			mock1.Setup(x => x.LotNumberID).Returns("Identification number of a production lot 1.");
			mock1.Setup(x => x.ProductBestBeforeDateTime).Returns(ZDateTime.BrettsBirthday.AddYears(1));
			mock1.Setup(x => x.ProductManufacturedDate).Returns(ZDateTime.BrettsBirthday);
			yield return mock1.Object;

			var mock2 = new Mock<IShippingIdentification>();
			mock2.Setup(x => x.LotNumberID).Returns("Identification number of a production lot 2.");
			mock1.Setup(x => x.ProductBestBeforeDateTime).Returns(ZDateTime.BrettsBirthday.AddYears(2));
			mock2.Setup(x => x.ProductManufacturedDate).Returns(ZDateTime.BrettsBirthday.AddDays(1));
			yield return mock2.Object;
		}

		INX603Declaration GetMessage(bool optionalNodesArePopulated = true)
		{
			var mock = new Mock<INX603Declaration>();
			mock.Setup(x => x.FunctionalReferenceID).Returns("12345678002111070001");
			mock.Setup(x => x.FunctionCode).Returns("9");
			mock.Setup(x => x.ID).Returns("A0123456789");
			mock.Setup(x => x.TypeCode).Returns("A");
			mock.Setup(x => x.AdditionalDocument).Returns(GetAdditionalDocument(optionalNodesArePopulated));
			mock.Setup(x => x.BorderTransportMeans).Returns(GetBorderTransportMeans());
			mock.Setup(x => x.CurrencyExchange).Returns(GetCurrencyExchange());
			mock.Setup(x => x.GoodsShipment).Returns(GetGoodsShipment(optionalNodesArePopulated));
			mock.Setup(x => x.Importer).Returns(GetImporter());
			mock.Setup(x => x.Application).Returns(GetApplication(optionalNodesArePopulated));
			return mock.Object;
		}

		IPayment GetApplicationPayment()
		{
			var mock = new Mock<IPayment>();
			mock.Setup(x => x.MethodCode).Returns("CA");
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

		IApplicationAdditionalInformation GetApplicationAdditionalInformation()
		{
			var mock = new Mock<IApplicationAdditionalInformation>();
			mock.Setup(x => x.StatementDescription).Returns("Description of an additional statement.");
			mock.Setup(x => x.ElectronicReceipt).Returns("A");
			mock.Setup(x => x.ProvedPaper).Returns("B");
			return mock.Object;
		}

		IPartyDetails GetApplicationAgent()
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ID).Returns("AG001");
			mock.Setup(x => x.TypeCode).Returns("58");
			return mock.Object;
		}

		IAppointment GetApplicationAppointment()
		{
			var mock = new Mock<IAppointment>();
			mock.Setup(x => x.ReservationDate).Returns(ZDateTime.BrettsBirthday.AddYears(3));
			mock.Setup(x => x.ReservationPeriodCode).Returns("C");
			return mock.Object;
		}

		IAddress GetDeclarerAddress()
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.ChineseLine).Returns("Address. Chinese Line. Text of Declarer");
			return mock.Object;
		}

		IEnumerable<ICommunication> GetDeclarerCommunications()
		{
			var mock1 = new Mock<ICommunication>();
			mock1.Setup(x => x.ID).Returns("0378451314");
			mock1.Setup(x => x.TypeID).Returns("TL");
			yield return mock1.Object;

			var mock2 = new Mock<ICommunication>();
			mock2.Setup(x => x.ID).Returns("xx2@hotmail.com");
			mock2.Setup(x => x.TypeID).Returns("EM");
			yield return mock2.Object;
		}

		IPartyDetails GetDeclarer()
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ID).Returns("TW088");
			mock.Setup(x => x.Name).Returns("Declarer. Name. Text");
			mock.Setup(x => x.ChineseName).Returns("報驗義務人中文名稱");
			mock.Setup(x => x.TypeCode).Returns("T04");
			mock.Setup(x => x.Address).Returns(GetDeclarerAddress);
			mock.Setup(x => x.Communications).Returns(GetDeclarerCommunications);
			return mock.Object;
		}

		IApplication GetApplication(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IApplication>();
			mock.Setup(x => x.TypeCode).Returns("2");
			mock.Setup(x => x.ContactOffice).Returns("FTTPE");
			mock.Setup(x => x.Payment).Returns(GetApplicationPayment());

			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.AdditionalDocuments).Returns(GetApplicationAdditionalDocuments);
				mock.Setup(x => x.AdditionalInformation).Returns(GetApplicationAdditionalInformation);
				mock.Setup(x => x.Agent).Returns(GetApplicationAgent);
				mock.Setup(x => x.Appointment).Returns(GetApplicationAppointment);
				mock.Setup(x => x.Declarer).Returns(GetDeclarer);
			}

			return mock.Object;
		}

		IAddress GetImporterAddress()
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.ChineseLine).Returns("進口人(納稅義務人)中文地址");
			return mock.Object;
		}

		IPartyDetails GetImporter()
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ID).Returns("1234643212");
			mock.Setup(x => x.Name).Returns("Importer Name");
			mock.Setup(x => x.ChineseName).Returns("進口人(納稅義務人)中文名稱");
			mock.Setup(x => x.TypeCode).Returns("X21");
			mock.Setup(x => x.Address).Returns(GetImporterAddress);
			return mock.Object;
		}

		ILPCOAuthorizedParty GetLPCOAuthorizedParty()
		{
			var mock = new Mock<ILPCOAuthorizedParty>();
			mock.Setup(x => x.ID).Returns("Authorized");
			mock.Setup(x => x.TypeCode).Returns("ABC");
			return mock.Object;
		}

		ILPCODetail GetMedicalInstrument()
		{
			var mock = new Mock<ILPCODetail>();
			mock.Setup(x => x.LPCOID).Returns("Identifier");
			mock.Setup(x => x.LPCOAuthorizedParty).Returns(GetLPCOAuthorizedParty);
			return mock.Object;
		}

		IGoodsStatisticalMeasure GetGoodsStatisticalMeasure()
		{
			var mock = new Mock<IGoodsStatisticalMeasure>();
			mock.Setup(x => x.StatisticalUnitCode).Returns("PKG");
			mock.Setup(x => x.TariffQuantity).Returns(12.5645m);
			return mock.Object;
		}

		ICommoditySpecification GetCommoditySpecification()
		{
			var mock = new Mock<ICommoditySpecification>();
			mock.Setup(x => x.CharacteristicQualifierCode).Returns("334");
			mock.Setup(x => x.ElementDescription).Returns("Element Description. Text");
			return mock.Object;
		}

		IPackaging GetPackaging()
		{
			var mock = new Mock<IPackaging>();
			mock.Setup(x => x.QuantityQuantity).Returns(8);
			mock.Setup(x => x.TypeCode).Returns("PKG");
			return mock.Object;
		}

		IOrigin GetOrigin()
		{
			var mock = new Mock<IOrigin>();
			mock.Setup(x => x.CountryCode).Returns("US");
			return mock.Object;
		}

		IPartyDetails GetManufacturer(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.Name).Returns("Manufacturer Name");
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ID).Returns("Manufacturer ID");
				var mockIAddress = new Mock<IAddress>();
				mockIAddress.Setup(x => x.Line).Returns("Manufacturer address line");
				mock.Setup(x => x.Address).Returns(mockIAddress.Object);
			}

			return mock.Object;
		}

		IGoodsMeasure GetGoodsMeasure()
		{
			var mock = new Mock<IGoodsMeasure>();
			mock.Setup(x => x.NetWeightMeasure).Returns(11.22m);
			mock.Setup(x => x.TariffQuantity).Returns(22m);
			mock.Setup(x => x.UnitCode).Returns("PKG");
			return mock.Object;
		}

		ICommodity GetCommodity(bool optionalNodesArePopulated)
		{
			var mock = new Mock<ICommodity>();
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.CommercialCategorizationID).Returns("Commodity CommercialCategorizationID");
				mock.Setup(x => x.Description).Returns("Commodity Description");
				mock.Setup(x => x.TariffCodeExtensionCode).Returns("E");
				var mockIConstituent = new Mock<IConstituent>();
				mockIConstituent.Setup(x => x.ElementDescription).Returns("Constituent ElementDescription");
				mockIConstituent.Setup(x => x.LevelID).Returns("Constituent LevelID");
				mockIConstituent.Setup(x => x.Thickness).Returns("Constituent Thickness");
				mock.Setup(x => x.Constituent).Returns(mockIConstituent.Object);
				var mockIPreviousDocument = new Mock<IPreviousDocument>();
				mockIPreviousDocument.Setup(x => x.ID).Returns("IPreviousDocumentID");
				mock.Setup(x => x.PreviousDocument).Returns(mockIPreviousDocument.Object);
			}

			mock.Setup(x => x.Name).Returns("Commodity Name");
			mock.Setup(x => x.ChineseDescription).Returns("Commodity 中文 ChineseDescription");
			mock.Setup(x => x.EnglishDescription).Returns("Commodity EN EnglishDescription");
			var mockIClassification = new Mock<IClassification>();
			mockIClassification.Setup(x => x.ID).Returns("ClassificationID");
			mock.Setup(x => x.Classification).Returns(mockIClassification.Object);
			var mockICommodityDutyTaxFee = new Mock<ICommodityDutyTaxFee>();
			mockICommodityDutyTaxFee.Setup(x => x.AdValoremTaxBaseAmount).Returns(1122m);
			mock.Setup(x => x.DutyTaxFee).Returns(mockICommodityDutyTaxFee.Object);
			return mock.Object;
		}

		IEnumerable<IAdditionalInformation> GetAdditionalInformations()
		{
			var mock = new Mock<IAdditionalInformation>();
			mock.Setup(x => x.StatementCode).Returns("1");
			mock.Setup(x => x.StatementDescription).Returns("Description of an additional statement.");
			var mock2 = new Mock<IAdditionalInformation>();
			mock2.Setup(x => x.StatementCode).Returns("2");
			mock2.Setup(x => x.StatementDescription).Returns("Description of an additional statement 2.");
			yield return mock.Object;
			yield return mock2.Object;
		}

		IEnumerable<IGovernmentAgencyGoodsItem> GetGovernmentAgencyGoodsItems(bool optionalNodesArePopulated)
		{
			var agencyGoodsItems = new Mock<IGovernmentAgencyGoodsItem>();
			agencyGoodsItems.Setup(x => x.SequenceNumeric).Returns(10);
			agencyGoodsItems.Setup(x => x.Commodity).Returns(GetCommodity(optionalNodesArePopulated));
			agencyGoodsItems.Setup(x => x.GoodsMeasure).Returns(GetGoodsMeasure());
			agencyGoodsItems.Setup(x => x.Manufacturer).Returns(GetManufacturer(optionalNodesArePopulated));
			agencyGoodsItems.Setup(x => x.Origin).Returns(GetOrigin());
			agencyGoodsItems.Setup(x => x.Packaging).Returns(GetPackaging());
			agencyGoodsItems.Setup(x => x.GoodsStatisticalMeasure).Returns(GetGoodsStatisticalMeasure());
			if (optionalNodesArePopulated)
			{
				agencyGoodsItems.Setup(x => x.AdditionalInformations).Returns(GetAdditionalInformations);
				agencyGoodsItems.Setup(x => x.CommoditySpecification).Returns(GetCommoditySpecification());
				agencyGoodsItems.Setup(x => x.MedicalInstrument).Returns(GetMedicalInstrument());
				agencyGoodsItems.Setup(x => x.ShippingIdentifications).Returns(GetShippingIdentifications());
			}

			yield return agencyGoodsItems.Object;
		}

		IEnumerable<ITransportEquipment> GetTransportEquipments()
		{
			var transportEquipment = new Mock<ITransportEquipment>();
			transportEquipment.Setup(x => x.ID).Returns("CID");
			var transportEquipment2 = new Mock<ITransportEquipment>();
			transportEquipment2.Setup(x => x.ID).Returns("DID");
			yield return transportEquipment.Object;
			yield return transportEquipment2.Object;
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

		ITransportMeans GetTransportMeans()
		{
			var transportMeans = new Mock<ITransportMeans>();
			transportMeans.Setup(x => x.ID).Returns("abcd1234");
			transportMeans.Setup(x => x.JourneyID).Returns("JourneyID123");
			return transportMeans.Object;
		}

		IEnumerable<IAdditionalInformation> GetAdditionalInformation()
		{
			var mock = new Mock<IAdditionalInformation>();
			mock.Setup(x => x.StatementCode).Returns("1");
			mock.Setup(x => x.StatementDescription).Returns("NIL");
			var mock2 = new Mock<IAdditionalInformation>();
			mock2.Setup(x => x.StatementCode).Returns("2");
			mock2.Setup(x => x.StatementDescription).Returns("XXX");
			yield return mock.Object;
			yield return mock2.Object;
		}

		IConsignment GetConsignment(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IConsignment>();
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ManifestSerialNumber).Returns("abcd");
				mock.Setup(x => x.AdditionalInformations).Returns(GetAdditionalInformation());
				mock.Setup(x => x.BorderTransportMeans).Returns(GetTransportMeans());
				mock.Setup(x => x.TransportContractDocuments).Returns(GetTransportContractDocuments());
				mock.Setup(x => x.TransportEquipments).Returns(GetTransportEquipments());
			}

			mock.Setup(x => x.GoodsLocation).Returns("USLAX");
			mock.Setup(x => x.LoadingLocation).Returns(GetLoadingLocation());
			return mock.Object;
		}

		IGoodsShipment GetGoodsShipment(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IGoodsShipment>();
			mock.Setup(x => x.ExitDateTime).Returns(ZDateTime.BrettsBirthday);
			mock.Setup(x => x.Consignment).Returns(GetConsignment(optionalNodesArePopulated));
			mock.Setup(x => x.GovernmentAgencyGoodsItems).Returns(GetGovernmentAgencyGoodsItems(optionalNodesArePopulated));
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.Seller).Returns(GetSeller());
			}

			return mock.Object;
		}

		ICurrencyExchange GetCurrencyExchange()
		{
			var mock = new Mock<ICurrencyExchange>();
			mock.Setup(x => x.CurrencyTypeCode).Returns("USD");
			mock.Setup(x => x.RateNumeric).Returns(1.12345m);
			return mock.Object;
		}

		ITransportMeans GetBorderTransportMeans()
		{
			var mock = new Mock<ITransportMeans>();
			mock.Setup(x => x.ArrivalDateTime).Returns(new ZDate(2023, 4, 24));
			mock.Setup(x => x.TypeCode).Returns("S");
			return mock.Object;
		}

		IDeclarationAdditionalDocument GetAdditionalDocument(bool optionalNodesArePopulated)
		{
			IDeclarationAdditionalDocument document = null;
			if (optionalNodesArePopulated)
			{
				var mock = new Mock<IDeclarationAdditionalDocument>();
				mock.Setup(x => x.ID).Returns("FTBX93E0001373");
				document = mock.Object;
			}
			return document;
		}
	}
}
