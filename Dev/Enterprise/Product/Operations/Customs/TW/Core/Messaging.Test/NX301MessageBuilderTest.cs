using System.Collections.Generic;
using CargoWise.Customs.TW.MessageDefinitions.NX301;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	[TestedType(typeof(NX301MessageBuilder))]
	sealed class NX301MessageBuilderTest : BaseTWMessageBuilderTest<INX301Declaration, Declaration>
	{
		[ExpectNoExceptions]
		public override void TestSerializeToMessageString()
		{
			var expectedString = TWMessageBuilder.SerializeToMessageString(Declaration, "9");
			AssertXMLContains("<FunctionCode>9</FunctionCode>", expectedString);
			AsserContainsXmlValueByType(expectedString, Declaration, typeof(INX301Declaration), IgnoreAssertPropertyNames);
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
			var declaration = new NX301MessageBuilder().PopulateDeclaration(message);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("NX301.xml");

			CombineAssertions(() =>
			{
				AssertXMLContains(expected, actual);
				AsserContainsXmlByType(actual, declaration.GetType());
			});
		}

		public void TestPopulateDeclarationWithoutOptionalNodes()
		{
			var message = GetMessage(false);
			var declaration = new NX301MessageBuilder().PopulateDeclaration(message);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("NX301_WithoutOptionalNodes.xml");
			AssertXMLContains(expected, actual);
		}

		protected override INX301Declaration CreateDataSource()
		{
			return GetMessage();
		}

		protected override ITWMessageBuilder CreateNewMessageBuilder()
		{
			return new NX301MessageBuilder();
		}

		INX301Declaration GetMessage(bool optionalNodesArePopulated = true)
		{
			var mock = new Mock<INX301Declaration>();
			mock.Setup(x => x.FunctionalReferenceID).Returns("12345678002111070001");
			mock.Setup(x => x.FunctionCode).Returns("9");
			mock.Setup(x => x.ID).Returns("A0123456789");
			mock.Setup(x => x.BorderTransportMeans).Returns(GetITransportMeans);
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

		IPayment GetApplicationPayment()
		{
			var mock = new Mock<IPayment>();
			mock.Setup(x => x.MethodCode).Returns("CA");
			return mock.Object;
		}

		IPartyDetails GetApplicationAgent()
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ID).Returns("AG001");
			mock.Setup(x => x.TypeCode).Returns("58");
			return mock.Object;
		}

		IApplicationAdditionalInformation GetApplicationAdditionalInformation(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IApplicationAdditionalInformation>();
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.StatementDescription).Returns("Description of an additional statement.");
			}
			mock.Setup(x => x.ElectronicReceipt).Returns("Description of an additional statement.");
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
			mock.Setup(x => x.ContactOffice).Returns("WCOIDG002");
			mock.Setup(x => x.Payment).Returns(GetApplicationPayment);
			mock.Setup(x => x.Declarer).Returns(GetDeclarer);
			mock.Setup(x => x.Labels).Returns(GetApplicationLabels(optionalNodesArePopulated));
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.AdditionalDocuments).Returns(GetApplicationAdditionalDocuments);
				mock.Setup(x => x.Agent).Returns(GetApplicationAgent);
				mock.Setup(x => x.Appointment).Returns(GetApplicationAppointment);
				mock.Setup(x => x.ApprovalAuthenticationInformation).Returns("D0005");
			}

			return mock.Object;
		}

		IEnumerable<ILabelDetail> GetApplicationLabelDetails()
		{
			var mock1 = new Mock<ILabelDetail>();
			mock1.Setup(x => x.EndNumber).Returns("TW087");
			mock1.Setup(x => x.StartNumber).Returns("TW089");
			mock1.Setup(x => x.Track).Returns("TW094");
			mock1.Setup(x => x.Year).Returns("579");
			yield return mock1.Object;

			var mock2 = new Mock<ILabelDetail>();
			mock2.Setup(x => x.EndNumber).Returns("TT087");
			mock2.Setup(x => x.StartNumber).Returns("TT089");
			mock2.Setup(x => x.Track).Returns("TT094");
			mock2.Setup(x => x.Year).Returns("578");
			yield return mock2.Object;
		}

		IEnumerable<ILabel> GetApplicationLabels(bool optionalNodesArePopulated)
		{
			var mock1 = new Mock<ILabel>();
			var mock2 = new Mock<ILabel>();
			mock1.Setup(x => x.StatusNameCode).Returns("name1");
			mock2.Setup(x => x.StatusNameCode).Returns("name2");
			if (optionalNodesArePopulated)
			{
				mock1.Setup(x => x.LabelDetails).Returns(GetApplicationLabelDetails);
				mock2.Setup(x => x.LabelDetails).Returns(GetApplicationLabelDetails);
			}
			yield return mock1.Object;
			yield return mock2.Object;
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

		IAddress GetDeclarerAddress()
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.ChineseLine).Returns("Address. Chinese Line. Text of Declarer");
			return mock.Object;
		}

		IPartyDetails GetDeclarer()
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ID).Returns("TW088");
			mock.Setup(x => x.Name).Returns("Declarer. Name. Text");
			mock.Setup(x => x.ChineseName).Returns("Declarer. Chinese Name. Text");
			mock.Setup(x => x.TypeCode).Returns("T04");
			mock.Setup(x => x.Address).Returns(GetDeclarerAddress);
			mock.Setup(x => x.Communications).Returns(GetDeclarerCommunications);
			return mock.Object;
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
			mock.Setup(x => x.Name).Returns("Importer Name");
			mock.Setup(x => x.ChineseName).Returns("Name of the importer ( duty payer) in Chinese.");
			mock.Setup(x => x.TypeCode).Returns("X21");
			mock.Setup(x => x.Address).Returns(GetImporterAddress);
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
			mock1.Setup(x => x.ProductManufacturedDate).Returns(ZDateTime.BrettsBirthday);
			yield return mock1.Object;

			var mock2 = new Mock<IShippingIdentification>();
			mock2.Setup(x => x.LotNumberID).Returns("Identification number of a production lot 2.");
			mock2.Setup(x => x.ProductManufacturedDate).Returns(ZDateTime.BrettsBirthday.AddDays(1));
			yield return mock2.Object;
		}

		IGoodsLicensingStatisticalMeasure GetGoodsLicensingStatisticalMeasure()
		{
			var mock = new Mock<IGoodsLicensingStatisticalMeasure>();
			mock.Setup(x => x.LicensingQuantity).Returns(123.44);
			mock.Setup(x => x.StatisticalUnitCode).Returns("PKG");
			mock.Setup(x => x.ResponsibleGovernmentAgency).Returns("AK");
			return mock.Object;
		}

		ICommoditySpecification GetGovernmentAgencyGoodsItemCommoditySpecification()
		{
			var mock = new Mock<ICommoditySpecification>();
			mock.Setup(x => x.CharacteristicQualifierCode).Returns("334");
			mock.Setup(x => x.ElementDescription).Returns("Commodity Specification. Element Description. Text");
			return mock.Object;
		}

		ILPCOAuthorizedParty GetLPCOAuthorizedParty()
		{
			var mock = new Mock<ILPCOAuthorizedParty>();
			mock.Setup(x => x.ID).Returns("Authorized");
			return mock.Object;
		}

		ILPCODetail GetGovernmentAgencyGoodsItemApprovalDocument()
		{
			var mock = new Mock<ILPCODetail>();
			mock.Setup(x => x.LPCOExemptionCode).Returns("P");
			mock.Setup(x => x.LPCOID).Returns("Identifier");
			mock.Setup(x => x.LPCOAuthorizedParty).Returns(GetLPCOAuthorizedParty);
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

		IPartyDetails GetManufacturer(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.Name).Returns("Manufacturer. Name. Text");
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.Address).Returns(GetManufacturerAddress);
			}
			return mock.Object;
		}

		IGoodsMeasure GetGoodsMeasure()
		{
			var mock = new Mock<IGoodsMeasure>();
			mock.Setup(x => x.NetWeightMeasure).Returns(1.23m);
			mock.Setup(x => x.TariffQuantity).Returns(12);
			mock.Setup(x => x.UnitCode).Returns("PCE");
			return mock.Object;
		}

		IPreviousDocument GetGovernmentAgencyGoodsItemCommodityPreviousDocument()
		{
			var mock = new Mock<IPreviousDocument>();
			mock.Setup(x => x.ID).Returns("WCOIDD018");
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
			mock.Setup(x => x.PackingMethodDescription).Returns("ABC");
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
				mock.Setup(x => x.CommercialCategorizationID).Returns("Commercial Categorization");
				mock.Setup(x => x.Name).Returns("Commodity. Name. Text");
				mock.Setup(x => x.TariffCodeExtensionCode).Returns("F");
				mock.Setup(x => x.CommodityRelatedPackaging).Returns(GetGovernmentAgencyGoodsItemCommodityRelatedPackaging);
				mock.Setup(x => x.Constituent).Returns(GetGovernmentAgencyGoodsItemCommodityConstituent);
			}
			mock.Setup(x => x.Description).Returns("Commodity. Description. Text");
			mock.Setup(x => x.ChineseDescription).Returns("貨物中文名稱(品名)");
			mock.Setup(x => x.Classification).Returns(GetClassification);
			mock.Setup(x => x.DutyTaxFee).Returns(GetDutyTaxFee);
			mock.Setup(x => x.PreviousDocument).Returns(GetGovernmentAgencyGoodsItemCommodityPreviousDocument);
			return mock.Object;
		}

		IConstituent GetGovernmentAgencyGoodsItemCommodityConstituent()
		{
			var mock = new Mock<IConstituent>();
			mock.Setup(x => x.ElementDescription).Returns("Element Description");
			mock.Setup(x => x.LevelID).Returns("Level of commodity.");
			mock.Setup(x => x.Thickness).Returns("Thickness");
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
				mock1.Setup(x => x.Packaging).Returns(GetPackaging);
				mock1.Setup(x => x.ApprovalDocument).Returns(GetGovernmentAgencyGoodsItemApprovalDocument);
				mock1.Setup(x => x.CommoditySpecification).Returns(GetGovernmentAgencyGoodsItemCommoditySpecification);

				mock2.Setup(x => x.AdditionalInformations).Returns(GetGovernmentAgencyGoodsItemsAdditionalInformations);
				mock2.Setup(x => x.ShippingIdentifications).Returns(GetShippingIdentifications);
				mock2.Setup(x => x.Packaging).Returns(GetPackaging);
				mock1.Setup(x => x.ApprovalDocument).Returns(GetGovernmentAgencyGoodsItemApprovalDocument);
				mock1.Setup(x => x.CommoditySpecification).Returns(GetGovernmentAgencyGoodsItemCommoditySpecification);
			}
			mock1.Setup(x => x.Commodity).Returns(GetGovernmentAgencyGoodsItemCommodity(optionalNodesArePopulated));
			mock1.Setup(x => x.GoodsMeasure).Returns(GetGoodsMeasure);
			mock1.Setup(x => x.Manufacturer).Returns(GetManufacturer(optionalNodesArePopulated));
			mock1.Setup(x => x.Origin).Returns(GetOrigin);
			mock1.Setup(x => x.GoodsLicensingStatisticalMeasure).Returns(GetGoodsLicensingStatisticalMeasure);

			mock2.Setup(x => x.Commodity).Returns(GetGovernmentAgencyGoodsItemCommodity(optionalNodesArePopulated));
			mock2.Setup(x => x.GoodsMeasure).Returns(GetGoodsMeasure);
			mock2.Setup(x => x.Manufacturer).Returns(GetManufacturer(optionalNodesArePopulated));
			mock1.Setup(x => x.Origin).Returns(GetOrigin);
			mock2.Setup(x => x.GoodsLicensingStatisticalMeasure).Returns(GetGoodsLicensingStatisticalMeasure);
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

		IConsignment GetGoodsShipmentConsignment(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IConsignment>();
			mock.Setup(x => x.GoodsLocation).Returns("USLAX");
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ManifestSerialNumber).Returns("M01");
				mock.Setup(x => x.LoadingLocation).Returns(GetLoadingLocation);
				mock.Setup(x => x.TransportContractDocuments).Returns(GetTransportContractDocuments);
				mock.Setup(x => x.BorderTransportMeans).Returns(GetTransportMeans);
				mock.Setup(x => x.TransportEquipments).Returns(GetTransportEquipments);
			}
			return mock.Object;
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

		ITransportMeans GetTransportMeans()
		{
			var transportMeans = new Mock<ITransportMeans>();
			transportMeans.Setup(x => x.ID).Returns("X1");
			transportMeans.Setup(x => x.JourneyID).Returns("X2");
			transportMeans.Setup(x => x.Registration).Returns("RE1");
			return transportMeans.Object;
		}

		IGoodsShipment GetGoodsShipment(bool optionalNodesArePopulated)
		{
			var mock = new Mock<IGoodsShipment>();
			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.ExitDateTime).Returns(ZDateTime.BrettsBirthday);
				mock.Setup(x => x.Seller).Returns(GetSeller);
			}
			mock.Setup(x => x.Consignment).Returns(GetGoodsShipmentConsignment(optionalNodesArePopulated));
			mock.Setup(x => x.GovernmentAgencyGoodsItems).Returns(GetGovernmentAgencyGoodsItems(optionalNodesArePopulated));
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
			transportMeans.Setup(x => x.ArrivalDateTime).Returns(new ZDate(2023, 4, 20));
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
