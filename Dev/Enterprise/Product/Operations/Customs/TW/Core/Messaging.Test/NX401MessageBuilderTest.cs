using System.Collections.Generic;
using CargoWise.Customs.TW.MessageDefinitions.NX401;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	[TestedType(typeof(NX401MessageBuilder))]
	sealed class NX401MessageBuilderTest : BaseTWMessageBuilderTest<INX401Declaration, Declaration>
	{
		[ExpectNoExceptions]
		public override void TestSerializeToMessageString()
		{
			var expectedString = TWMessageBuilder.SerializeToMessageString(Declaration, "9");
			CombineAssertions(() =>
			{
				AssertXMLContains("<FunctionCode>9</FunctionCode>", expectedString);
				AsserContainsXmlValueByType(expectedString, Declaration, typeof(INX401Declaration), IgnoreAssertPropertyNames);
			});
		}

		IEnumerable<ZString> IgnoreAssertPropertyNames
		{
			get
			{
				yield return "Enterprise.Customs.TW.Messaging.IGoodsShipment.ExitDateTime";
				yield return "Enterprise.Customs.TW.Messaging.IAppointment.ReservationDate";
			}
		}

		[ExpectNoExceptions]
		public void TestPopulateDeclarationWithOptionalNodes()
		{
			var message = GetMessage();
			var declaration = new NX401MessageBuilder().PopulateDeclaration(message);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("NX401.xml");

			CombineAssertions(() =>
			{
				AssertXMLContains(expected, actual);
				AsserContainsXmlByType(actual, declaration.GetType());
			});
		}

		public void TestPopulateDeclarationWithoutOptionalNodes()
		{
			var message = GetMessage(false);
			var declaration = new NX401MessageBuilder().PopulateDeclaration(message);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("NX401_WithoutOptionalNodes.xml");
			AssertXMLContains(expected, actual);
		}

		protected override INX401Declaration CreateDataSource()
		{
			return GetMessage();
		}

		protected override ITWMessageBuilder CreateNewMessageBuilder()
		{
			return new NX401MessageBuilder();
		}

		INX401Declaration GetMessage(bool optionalNodesArePopulated = true)
		{
			var mock = new Mock<INX401Declaration>();
			mock.Setup(x => x.FunctionalReferenceID).Returns("12345678002111070001");
			mock.Setup(x => x.FunctionCode).Returns("9");
			mock.Setup(x => x.ID).Returns("A0123456789");
			mock.Setup(x => x.Application).Returns(GetApplication(optionalNodesArePopulated));

			if (optionalNodesArePopulated)
			{
				mock.Setup(x => x.AcceptanceDateTime).Returns(new ZDate(2023, 3, 16));
				mock.Setup(x => x.TypeCode).Returns("A");
				mock.Setup(x => x.AdditionalDocument).Returns(GetAdditionalDocument);
				mock.Setup(x => x.AdditionalInformation).Returns(GetAdditionalInformation);
				mock.Setup(x => x.Agent).Returns(GetAgent);
				mock.Setup(x => x.BorderTransportMeans).Returns(GetITransportMeans(optionalNodesArePopulated));
				mock.Setup(x => x.CurrencyExchange).Returns(GetCurrencyExchange(optionalNodesArePopulated));
				mock.Setup(x => x.GoodsShipment).Returns(GetGoodsShipment(optionalNodesArePopulated));
				mock.Setup(x => x.Importer).Returns(GetImporter(optionalNodesArePopulated));
				mock.Setup(x => x.Packaging).Returns(GetPackaging);
			}
			return mock.Object;
		}

		IApplication GetApplication(bool optionalNodesArePopulated)
		{
			var application = new Mock<IApplication>();
			application.Setup(x => x.TypeCode).Returns("AP1");
			application.Setup(x => x.ContactOffice).Returns("AA");
			application.Setup(x => x.Payment).Returns(GetApplicationPayment);
			if (optionalNodesArePopulated)
			{
				application.Setup(x => x.AdditionalDocuments).Returns(GetApplicationAdditionalDocuments);
				application.Setup(x => x.AdditionalInformation).Returns(GetApplicationAdditionalInformation);
				application.Setup(x => x.Agent).Returns(GetApplicationAgent(optionalNodesArePopulated));
				application.Setup(x => x.Appointment).Returns(GetApplicationAppointment);
				application.Setup(x => x.AuthorizedInformation).Returns(GetAuthorizedInformation);
			}
			return application.Object;
		}

		IAuthorizedInformation GetAuthorizedInformation()
		{
			var authorizedInformation = new Mock<IAuthorizedInformation>();
			authorizedInformation.Setup(x => x.AdditionalDocument).Returns(GetAuthorizedInformationAdditionalDocument);
			return authorizedInformation.Object;
		}

		IAdditionalDocument GetAuthorizedInformationAdditionalDocument()
		{
			var document = new Mock<IAdditionalDocument>();
			document.Setup(x => x.ID).Returns("WCOIDD005");
			return document.Object;
		}

		IAppointment GetApplicationAppointment()
		{
			var appointment = new Mock<IAppointment>();
			appointment.Setup(x => x.ReservationDate).Returns(new ZDateTime(2023, 3, 20, 16, 3, 43));
			appointment.Setup(x => x.ReservationPeriodCode).Returns("P");
			return appointment.Object;
		}

		IPayment GetApplicationPayment()
		{
			var payment = new Mock<IPayment>();
			payment.Setup(x => x.MethodCode).Returns("CC");
			return payment.Object;
		}

		IPartyDetails GetApplicationAgent(bool optionalNodesArePopulated)
		{
			var importer = new Mock<IPartyDetails>();
			importer.Setup(x => x.ID).Returns("AgentID");
			importer.Setup(x => x.Name).Returns("Agent name");
			importer.Setup(x => x.TypeCode).Returns("A98");
			importer.Setup(x => x.Address).Returns(GetAgentAddress(optionalNodesArePopulated));
			if (optionalNodesArePopulated)
			{
				importer.Setup(x => x.Communications).Returns(GetAgentCommunications);
			}

			return importer.Object;
		}

		IEnumerable<ICommunication> GetAgentCommunications()
		{
			var communications = new List<ICommunication>();
			var communication1 = new Mock<ICommunication>();
			communication1.Setup(x => x.ID).Returns("xx4@gmail.com");
			communication1.Setup(x => x.TypeID).Returns("EM");

			var communication2 = new Mock<ICommunication>();
			communication2.Setup(x => x.ID).Returns("02 99999999");
			communication2.Setup(x => x.TypeID).Returns("TL");

			communications.Add(communication1.Object);
			communications.Add(communication2.Object);
			return communications;
		}

		IAddress GetAgentAddress(bool optionalNodesArePopulate)
		{
			var address = new Mock<IAddress>();
			if (optionalNodesArePopulate)
			{
				address.Setup(x => x.ChineseLine).Returns("申辦代理人中文地址");
			}
			return address.Object;
		}

		IApplicationAdditionalInformation GetApplicationAdditionalInformation()
		{
			var applicationAdditionalInformation = new Mock<IApplicationAdditionalInformation>();
			applicationAdditionalInformation.Setup(x => x.StatementDescription).Returns("Additional Information. Statement Description. Text");
			applicationAdditionalInformation.Setup(x => x.ProvedPaper).Returns("P");
			return applicationAdditionalInformation.Object;
		}

		IEnumerable<IAdditionalDocument> GetApplicationAdditionalDocuments()
		{
			var documents = new List<IAdditionalDocument>();
			var document1 = new Mock<IAdditionalDocument>();
			document1.Setup(x => x.ID).Returns("ID01");
			document1.Setup(x => x.Content).Returns("Additional Document. Content. Text1");
			document1.Setup(x => x.ImageFileFormat).Returns("PDF");
			document1.Setup(x => x.ImageFileName).Returns("PDF1");
			document1.Setup(x => x.TypeCode).Returns("T1");
			document1.Setup(x => x.ResponsibleGovernmentAgency).Returns("R1");
			documents.Add(document1.Object);

			var document2 = new Mock<IAdditionalDocument>();
			document2.Setup(x => x.ID).Returns("ID02");
			document2.Setup(x => x.Content).Returns("Additional Document. Content. Text2");
			document2.Setup(x => x.ImageFileFormat).Returns("PDF");
			document2.Setup(x => x.ImageFileName).Returns("PDF2");
			document2.Setup(x => x.TypeCode).Returns("T2");
			document2.Setup(x => x.ResponsibleGovernmentAgency).Returns("R2");
			documents.Add(document2.Object);
			return documents;
		}

		IPackaging GetPackaging()
		{
			var packing = new Mock<IPackaging>();
			packing.Setup(x => x.MarksNumbers).Returns("Packing Marks Numbers");
			return packing.Object;
		}

		IPartyDetails GetImporter(bool optionalNodesArePopulated)
		{
			var importer = new Mock<IPartyDetails>();
			importer.Setup(x => x.ID).Returns("ImporterID");
			importer.Setup(x => x.Name).Returns("importer name");
			importer.Setup(x => x.ChineseName).Returns("進口人名稱。");
			importer.Setup(x => x.TypeCode).Returns("I84");
			importer.Setup(x => x.Address).Returns(GetImporterAddress(optionalNodesArePopulated));
			if (optionalNodesArePopulated)
			{
				importer.Setup(x => x.Communications).Returns(GetImporterCommunications);
			}

			return importer.Object;
		}

		IEnumerable<ICommunication> GetImporterCommunications()
		{
			var communications = new List<ICommunication>();
			var communication1 = new Mock<ICommunication>();
			communication1.Setup(x => x.ID).Returns("xx1@gmail.com");
			communication1.Setup(x => x.TypeID).Returns("EM");

			var communication2 = new Mock<ICommunication>();
			communication2.Setup(x => x.ID).Returns("02 88888888");
			communication2.Setup(x => x.TypeID).Returns("TL");

			communications.Add(communication1.Object);
			communications.Add(communication2.Object);
			return communications;
		}

		IAddress GetImporterAddress(bool optionalNodesArePopulate)
		{
			var address = new Mock<IAddress>();
			address.Setup(x => x.Line).Returns("Importer Address. Line. Text");
			if (optionalNodesArePopulate)
			{
				address.Setup(x => x.ChineseLine).Returns("進口人中文地址");
			}
			return address.Object;
		}

		IGoodsShipment GetGoodsShipment(bool optionalNodesArePopulated)
		{
			var goodsShipment = new Mock<IGoodsShipment>();
			if (optionalNodesArePopulated)
			{
				goodsShipment.Setup(x => x.ExitDateTime).Returns(new ZDateTime(2023, 3, 15, 0, 0, 0));
				goodsShipment.Setup(x => x.ItemChargeAmount).Returns(1001.21m);
				goodsShipment.Setup(x => x.Consignee).Returns(GetConsignee(optionalNodesArePopulated));
				goodsShipment.Setup(x => x.Consignment).Returns(GetConsignment(optionalNodesArePopulated));
				goodsShipment.Setup(x => x.Exporter).Returns(GetExporter(optionalNodesArePopulated));
				goodsShipment.Setup(x => x.Seller).Returns(GetSeller(optionalNodesArePopulated));
			}
			goodsShipment.Setup(x => x.GovernmentAgencyGoodsItems).Returns(new[] { GetGovernmentAgencyGoodsItem(optionalNodesArePopulated), GetGovernmentAgencyGoodsItem(!optionalNodesArePopulated) });
			return goodsShipment.Object;
		}

		IPartyDetails GetSeller(bool optionalNodesArePopulated)
		{
			var seller = new Mock<IPartyDetails>();
			seller.Setup(x => x.Name).Returns("seller name");
			seller.Setup(x => x.Address).Returns(GetSellerAddress(optionalNodesArePopulated));
			if (optionalNodesArePopulated)
			{
				seller.Setup(x => x.ChineseName).Returns("賣方名稱。");
			}

			return seller.Object;
		}

		IAddress GetSellerAddress(bool optionalNodesArePopulate)
		{
			var address = new Mock<IAddress>();
			address.Setup(x => x.Line).Returns("Seller Address. Line. Text");
			if (optionalNodesArePopulate)
			{
				address.Setup(x => x.CountryCode).Returns("CN");
				address.Setup(x => x.ChineseLine).Returns("賣方中文地址");
			}
			return address.Object;
		}

		IGovernmentAgencyGoodsItem GetGovernmentAgencyGoodsItem(bool optionalNodesArePopulated)
		{
			var governmentAgencyGoodsItem = new Mock<IGovernmentAgencyGoodsItem>();
			governmentAgencyGoodsItem.Setup(x => x.SequenceNumeric).Returns(1);
			governmentAgencyGoodsItem.Setup(x => x.AdditionalDocuments).Returns(new[] { GetGovernmentAgencyGoodsItemAdditionalDocument() });
			if (optionalNodesArePopulated)
			{
				governmentAgencyGoodsItem.Setup(x => x.AdditionalInformations).Returns(new[] { GetGovernmentAgencyGoodsItemAdditionalInformation(), GetGovernmentAgencyGoodsItemAdditionalInformation() });
				governmentAgencyGoodsItem.Setup(x => x.Origin).Returns(GetGovernmentAgencyGoodsItemOrigin);
				governmentAgencyGoodsItem.Setup(x => x.GoodsStatisticalMeasure).Returns(GetGovernmentAgencyGoodsItemGoodsStatisticalMeasure);
			}
			governmentAgencyGoodsItem.Setup(x => x.Commodity).Returns(GetCommodity(optionalNodesArePopulated));
			governmentAgencyGoodsItem.Setup(x => x.GoodsMeasure).Returns(GetGovernmentAgencyGoodsItemGoodsMeasure);
			governmentAgencyGoodsItem.Setup(x => x.Packaging).Returns(GetGovernmentAgencyGoodsItemPackaging);
			return governmentAgencyGoodsItem.Object;
		}

		IGoodsStatisticalMeasure GetGovernmentAgencyGoodsItemGoodsStatisticalMeasure()
		{
			var goodsStatisticalMeasure = new Mock<IGoodsStatisticalMeasure>();
			goodsStatisticalMeasure.Setup(x => x.StatisticalUnitCode).Returns("U");
			goodsStatisticalMeasure.Setup(x => x.TariffQuantity).Returns(12m);
			return goodsStatisticalMeasure.Object;
		}

		IPackaging GetGovernmentAgencyGoodsItemPackaging()
		{
			var packing = new Mock<IPackaging>();
			packing.Setup(x => x.QuantityQuantity).Returns(1);
			packing.Setup(x => x.TypeCode).Returns("G");
			return packing.Object;
		}

		IOrigin GetGovernmentAgencyGoodsItemOrigin()
		{
			var origin = new Mock<IOrigin>();
			origin.Setup(x => x.CountryCode).Returns("FR");
			return origin.Object;
		}

		IGoodsMeasure GetGovernmentAgencyGoodsItemGoodsMeasure()
		{
			var goodsMeasure = new Mock<IGoodsMeasure>();
			goodsMeasure.Setup(x => x.NetWeightMeasure).Returns(2m);
			goodsMeasure.Setup(x => x.TariffQuantity).Returns(4m);
			goodsMeasure.Setup(x => x.UnitCode).Returns("PCE");
			return goodsMeasure.Object;
		}

		ICommodity GetCommodity(bool optionalNodesArePopulated)
		{
			var commodity = new Mock<ICommodity>();
			commodity.Setup(x => x.Description).Returns("com desc");
			commodity.Setup(x => x.Classification).Returns(GetClassification);
			if (optionalNodesArePopulated)
			{
				commodity.Setup(x => x.ChineseDescription).Returns("中文描述");
				commodity.Setup(x => x.Quarantine).Returns(GetQuarantine);
			}
			commodity.Setup(x => x.InvoiceLine).Returns(GetInvoiceLine(optionalNodesArePopulated));
			commodity.Setup(x => x.DutyTaxFee).Returns(GetDutyTaxFee(optionalNodesArePopulated));
			commodity.Setup(x => x.GovernmentProcedure).Returns(GetGovernmentProcedure(optionalNodesArePopulated));

			return commodity.Object;
		}

		IQuarantine GetQuarantine()
		{
			var quarantine = new Mock<IQuarantine>();
			quarantine.Setup(x => x.ObjectFeature).Returns("ObjectFeature");
			quarantine.Setup(x => x.Treatment).Returns("Treatment");
			quarantine.Setup(x => x.AdditionalDocument).Returns(new[] { GetQuarantineAdditionalDocument() });
			quarantine.Setup(x => x.AdditionalInformation).Returns(new[] { GetQuarantineAdditionalInformation() });
			quarantine.Setup(x => x.Packing).Returns(new[] { GetQuarantinePacking() });
			quarantine.Setup(x => x.Animal).Returns(GetQuarantineAnimal);
			return quarantine.Object;
		}

		IAnimal GetQuarantineAnimal()
		{
			var animal = new Mock<IAnimal>();
			animal.Setup(x => x.AgeMonthNumeric).Returns(10);
			animal.Setup(x => x.AgeYearNumeric).Returns(105);
			animal.Setup(x => x.FemaleQuantity).Returns(3004);
			animal.Setup(x => x.MaleQuantity).Returns(2005);
			animal.Setup(x => x.MicrochipID).Returns("ABC1234567890");
			animal.Setup(x => x.Vaccination).Returns("cov19 20230103");
			return animal.Object;
		}

		IPackaging GetQuarantinePacking()
		{
			var packing = new Mock<IPackaging>();
			packing.Setup(x => x.PackingDateTime).Returns(new ZDate(2023, 3, 20));
			return packing.Object;
		}

		IAdditionalInformation GetQuarantineAdditionalInformation()
		{
			var additionalInformation = new Mock<IAdditionalInformation>();
			additionalInformation.Setup(x => x.PackingHouse).Returns("house");
			return additionalInformation.Object;
		}

		IAdditionalDocument GetQuarantineAdditionalDocument()
		{
			var additionalDocument = new Mock<IAdditionalDocument>();
			additionalDocument.Setup(x => x.SlaughterDateTime).Returns(new ZDate(2023, 4, 20));
			return additionalDocument.Object;
		}

		IInvoiceLine GetInvoiceLine(bool optionalNodesArePopulated)
		{
			var invoiceLine = new Mock<IInvoiceLine>();
			if (optionalNodesArePopulated)
			{
				invoiceLine.Setup(x => x.ItemChargeAmount).Returns(100.03);
			}
			else
			{
				invoiceLine.Setup(x => x.ItemChargeAmount).Returns(0m);
			}
			return invoiceLine.Object;
		}

		IGovernmentProcedure GetGovernmentProcedure(bool optionalNodesArePopulated)
		{
			var governmentProcedure = new Mock<IGovernmentProcedure>();
			governmentProcedure.Setup(x => x.CurrentCode).Returns(optionalNodesArePopulated ? "CC" : ZString.Empty);
			return governmentProcedure.Object;
		}

		ICommodityDutyTaxFee GetDutyTaxFee(bool optionalNodesArePopulated)
		{
			var commodityDutyTaxFee = new Mock<ICommodityDutyTaxFee>();
			commodityDutyTaxFee.Setup(x => x.AdValoremTaxBaseAmount).Returns(optionalNodesArePopulated ? 201m : 0m);
			return commodityDutyTaxFee.Object;
		}

		IClassification GetClassification()
		{
			var classification = new Mock<IClassification>();
			classification.Setup(x => x.ID).Returns("CFid");
			return classification.Object;
		}

		IAdditionalInformation GetGovernmentAgencyGoodsItemAdditionalInformation()
		{
			var additionalInformation = new Mock<IAdditionalInformation>();
			additionalInformation.Setup(x => x.StatementCode).Returns("S");
			additionalInformation.Setup(x => x.StatementDescription).Returns("SDESC");
			return additionalInformation.Object;
		}

		IAdditionalDocument GetGovernmentAgencyGoodsItemAdditionalDocument()
		{
			var governmentAgencyGoodsItemAdditionalDocument = new Mock<IAdditionalDocument>();
			governmentAgencyGoodsItemAdditionalDocument.Setup(x => x.SequenceNumeric).Returns(1);
			return governmentAgencyGoodsItemAdditionalDocument.Object;
		}

		IAddress GetExporterAddress(bool optionalNodesArePopulate)
		{
			var address = new Mock<IAddress>();
			address.Setup(x => x.Line).Returns("Exporter Address. Line. Text");
			if (optionalNodesArePopulate)
			{
				address.Setup(x => x.ChineseLine).Returns("出口人中文地址");
			}
			return address.Object;
		}

		IPartyDetails GetExporter(bool optionalNodesArePopulated)
		{
			var exporter = new Mock<IPartyDetails>();
			exporter.Setup(x => x.ID).Returns("exporter ID");
			exporter.Setup(x => x.Name).Returns("exporter name");
			exporter.Setup(x => x.TypeCode).Returns("E01");
			exporter.Setup(x => x.Address).Returns(GetExporterAddress(optionalNodesArePopulated));
			if (optionalNodesArePopulated)
			{
				exporter.Setup(x => x.ChineseName).Returns("出口人名稱。");
			}

			return exporter.Object;
		}

		IConsignment GetConsignment(bool optionalNodesArePopulated)
		{
			var consignment = new Mock<IConsignment>();
			if (optionalNodesArePopulated)
			{
				consignment.Setup(x => x.ArrivalTransportMeansTypeCode).Returns("SEA");
				consignment.Setup(x => x.BorderTransportMeans).Returns(GetTransportMeans(optionalNodesArePopulated));
				consignment.Setup(x => x.DepartureTransportMeans).Returns(GetDepartureTransportMeans(optionalNodesArePopulated));
				consignment.Setup(x => x.GoodsLocation).Returns("GL");
				consignment.Setup(x => x.LoadingLocation).Returns(GetLoadingLocation);
				consignment.Setup(x => x.TransportContractDocuments).Returns(GetTransportContractDocuments(optionalNodesArePopulated));
				consignment.Setup(x => x.TransportEquipments).Returns(GetTransportEquipments(optionalNodesArePopulated));
				consignment.Setup(x => x.UnloadingLocation).Returns(GetUnloadingLocation);
			}
			return consignment.Object;
		}

		ILocation GetUnloadingLocation()
		{
			var loadingLocation = new Mock<ILocation>();
			loadingLocation.Setup(x => x.ID).Returns("USLAX");
			return loadingLocation.Object;
		}

		IEnumerable<ITransportEquipment> GetTransportEquipments(bool optionalNodesArePopulated)
		{
			var transportEquipments = new List<ITransportEquipment>();
			if (optionalNodesArePopulated)
			{
				var transportEquipment = new Mock<ITransportEquipment>();
				transportEquipment.Setup(x => x.CharacteristicCode).Returns("C1");
				transportEquipment.Setup(x => x.ID).Returns("CID");
				transportEquipment.Setup(x => x.UsedCapacityCode).Returns("4");
				transportEquipment.Setup(x => x.Seals).Returns(new ZString[] { "B", "C", "F" });
			}
			return transportEquipments;
		}

		IEnumerable<ITransportContractDocument> GetTransportContractDocuments(bool optionalNodesArePopulated)
		{
			var transportContractDocuments = new List<ITransportContractDocument>();
			if (optionalNodesArePopulated)
			{
				var transportContractDocument = new Mock<ITransportContractDocument>();
				transportContractDocument.Setup(x => x.ID).Returns("TCD1");
				transportContractDocument.Setup(x => x.TypeCode).Returns("TCDTC1");
				transportContractDocuments.Add(transportContractDocument.Object);

				transportContractDocument = new Mock<ITransportContractDocument>();
				transportContractDocument.Setup(x => x.ID).Returns("TCD2");
				transportContractDocument.Setup(x => x.TypeCode).Returns("TCDTC2");
				transportContractDocuments.Add(transportContractDocument.Object);
			}

			return transportContractDocuments;
		}

		ILocation GetLoadingLocation()
		{
			var loadingLocation = new Mock<ILocation>();
			loadingLocation.Setup(x => x.ID).Returns("LL");
			return loadingLocation.Object;
		}

		ITransportMeans GetDepartureTransportMeans(bool optionalNodesArePopulated)
		{
			var departureTransportMeans = new Mock<ITransportMeans>();
			if (optionalNodesArePopulated)
			{
				departureTransportMeans.Setup(x => x.ID).Returns("X3");
				departureTransportMeans.Setup(x => x.TypeCode).Returns("XT");
			}
			return departureTransportMeans.Object;
		}

		ITransportMeans GetTransportMeans(bool optionalNodesArePopulated)
		{
			var transportMeans = new Mock<ITransportMeans>();
			if (optionalNodesArePopulated)
			{
				transportMeans.Setup(x => x.ID).Returns("X1");
				transportMeans.Setup(x => x.JourneyID).Returns("X2");
			}
			return transportMeans.Object;
		}

		IPartyDetails GetConsignee(bool optionalNodesArePopulate)
		{
			var consignee = new Mock<IPartyDetails>();
			consignee.Setup(x => x.Name).Returns("Consignee. Name. Text");
			consignee.Setup(x => x.Address).Returns(GetConsigneeAddress(optionalNodesArePopulate));
			if (optionalNodesArePopulate)
			{
				consignee.Setup(x => x.ID).Returns("Consignee01");
				consignee.Setup(x => x.ChineseName).Returns("收貨人中文名稱");
			}
			return consignee.Object;
		}

		IAddress GetConsigneeAddress(bool optionalNodesArePopulate)
		{
			var address = new Mock<IAddress>();
			address.Setup(x => x.Line).Returns("Consignee Address. Line. Text");
			if (optionalNodesArePopulate)
			{
				address.Setup(x => x.ChineseLine).Returns("收貨人中文地址");
			}
			return address.Object;
		}

		ICurrencyExchange GetCurrencyExchange(bool optionalNodesArePopulate)
		{
			var currencyExchange = new Mock<ICurrencyExchange>();
			currencyExchange.Setup(x => x.CurrencyTypeCode).Returns("USD");
			if (optionalNodesArePopulate)
			{
				currencyExchange.Setup(x => x.RateNumeric).Returns(7.66858);
			}
			return currencyExchange.Object;
		}

		ITransportMeans GetITransportMeans(bool optionalNodesArePopulated)
		{
			var transportMeans = new Mock<ITransportMeans>();
			transportMeans.Setup(x => x.TypeCode).Returns("4");
			if (optionalNodesArePopulated)
			{
				transportMeans.Setup(x => x.ArrivalDateTime).Returns(new ZDate(2023, 3, 17));
				transportMeans.Setup(x => x.ItineraryRoutingCountryCodes).Returns(new ZString[] { "ZA", "US" });
			}
			return transportMeans.Object;
		}

		IDeclarationAgent GetAgent()
		{
			var declarationAgent = new Mock<IDeclarationAgent>();
			declarationAgent.Setup(x => x.ID).Returns("05A");
			declarationAgent.Setup(x => x.RoleCode).Returns("AR");
			declarationAgent.Setup(x => x.SubBoxID).Returns("C");
			return declarationAgent.Object;
		}

		IDeclarationAdditionalInformation GetAdditionalInformation()
		{
			var declarationAdditionalInformation = new Mock<IDeclarationAdditionalInformation>();
			declarationAdditionalInformation.Setup(x => x.StatementDescription).Returns("Description of an additional statement.");
			return declarationAdditionalInformation.Object;
		}

		IDeclarationAdditionalDocument GetAdditionalDocument()
		{
			var declarationAdditionalDocument = new Mock<IDeclarationAdditionalDocument>();
			declarationAdditionalDocument.Setup(x => x.ID).Returns("WCOIDD005");
			return declarationAdditionalDocument.Object;
		}
	}
}
