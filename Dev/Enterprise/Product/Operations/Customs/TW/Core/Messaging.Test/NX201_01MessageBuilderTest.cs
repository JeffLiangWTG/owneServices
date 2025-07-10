using System;
using CargoWise.Customs.TW.MessageDefinitions.NX201_01;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	sealed class NX201_01MessageBuilderTest : BaseTWMessageBuilderTest<INX201_01Declaration, Declaration>
	{
		public void TestPopulateDeclarationIncludingConditionalFields()
		{
			AssertPopulateDeclaration(populateConditionalFields: true, "NX201_1.xml");
		}

		public void TestPopulateDeclarationIncludingConditionalFields_Action52()
		{
			AssertPopulateDeclaration(populateConditionalFields: true, "NX201_1_Action52.xml", "52");
		}

		public void TestPopulateDeclarationExcludingConditionalFields()
		{
			AssertPopulateDeclaration(populateConditionalFields: false, "NX201_1_WithoutOptionalNodes.xml");
		}

		[ExpectNoExceptions]
		void AssertPopulateDeclaration(bool populateConditionalFields, string testFileName, string functionCode = "9")
		{
			var data = GetDeclaration(populateConditionalFields, functionCode);
			var declaration = new NX201_01MessageBuilder().PopulateDeclaration(data);
			var declarationType = DeclarationType;
			var actual = XmlHelper.Serializer(declarationType, declaration, removeIndentAndLineBreak: true);
			var expected = GetExpectedMessageXML(testFileName);
			CombineAssertions(() =>
			{
				AssertXMLContains(expected, actual);
				AsserContainsXmlByType(actual, declarationType);
			});
		}

		protected override INX201_01Declaration CreateDataSource() => GetDeclaration(true);

		INX201_01Declaration GetDeclaration(bool populateConditionalFields, string functionCode = "9")
		{
			var mock = new Mock<INX201_01Declaration>();
			mock.Setup(x => x.FunctionalReferenceID).Returns("23322708001106180001");
			mock.Setup(x => x.FunctionCode).Returns(functionCode);
			mock.Setup(x => x.IssueDateTime).Returns(new DateTime(2011, 06, 18, 10, 30, 00));
			mock.Setup(x => x.GoodsShipment).Returns(GetGoodsShipment(populateConditionalFields));
			mock.Setup(x => x.Application).Returns(GetApplication(populateConditionalFields));

			if (populateConditionalFields)
			{
				mock.Setup(x => x.ID).Returns("AA 0010800135");
				mock.Setup(x => x.AdditionalDocument).Returns(GetDeclarationAdditionalDocument);
				mock.Setup(x => x.AdditionalInformation).Returns(GetDeclarationAdditionalInformation(populateConditionalFields));
				mock.Setup(x => x.AdditionalDeclarationID).Returns("FT20110310123456789");
			}

			return mock.Object;
		}

		IApplication GetApplication(bool populateConditionalFields)
		{
			var mock = new Mock<IApplication>();
			mock.Setup(x => x.TypeCode).Returns("2");
			mock.Setup(x => x.ContactOffice).Returns("FTTPE");
			mock.Setup(x => x.Applicant).Returns(GetApplicant(populateConditionalFields));

			if (populateConditionalFields)
			{
				mock.Setup(x => x.AdditionalDocuments).Returns(new[] { GetApplicationAdditionalDocument(populateConditionalFields) });
				mock.Setup(x => x.Agent).Returns(GetAgent);
			}

			return mock.Object;
		}

		IPartyDetails GetApplicant(bool populateConditionalFields)
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ChineseName).Returns("複合資訊股份有限公司");
			mock.Setup(x => x.ID).Returns("23322708");
			mock.Setup(x => x.TypeCode).Returns("58");
			mock.Setup(x => x.Address).Returns(GetApplicantAddress(populateConditionalFields));
			mock.Setup(x => x.Communications).Returns(new[] { GetCommunication("03-333-6666", "TE"), GetCommunication("mary@yahoo.com", "MA") });
			mock.Setup(x => x.ContactName).Returns("王大明");

			if (populateConditionalFields)
			{
				mock.Setup(x => x.Name).Returns("COMPOSE INFORMATION CO., LTD.");
				mock.Setup(x => x.OwnerName).Returns("張三");
			}

			return mock.Object;
		}

		IAddress GetApplicantAddress(bool populateConditionalFields)
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.ChineseLine).Returns("桃園縣中壢市遠東路88號");

			if (populateConditionalFields)
			{
				mock.Setup(x => x.Line).Returns("No. 88, Fareast Rd., Chungli City, Taoyuan, Taiwan, ROC");
			}

			return mock.Object;
		}

		IPartyDetails GetAgent()
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.ID).Returns("23322709");
			mock.Setup(x => x.Name).Returns("皇冠企業股份有限公司");
			mock.Setup(x => x.TypeCode).Returns("58");
			mock.Setup(x => x.Address).Returns(GetAgentAddress);
			mock.Setup(x => x.Communications).Returns(new[] { GetCommunication("02-2287-1489", "TE") });
			mock.Setup(x => x.ContactName).Returns("王小明");
			return mock.Object;
		}

		ICommunication GetCommunication(ZString id, ZString typeID)
		{
			var mock = new Mock<ICommunication>();
			mock.Setup(x => x.ID).Returns(id);
			mock.Setup(x => x.TypeID).Returns(typeID);
			return mock.Object;
		}

		IAddress GetAgentAddress()
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.ChineseLine).Returns("台北市忠孝東路四段341號3樓");
			return mock.Object;
		}

		IAdditionalDocument GetApplicationAdditionalDocument(bool populateConditionalFields)
		{
			var mock = new Mock<IAdditionalDocument>();
			mock.Setup(x => x.SequenceNumeric).Returns(1);
			mock.Setup(x => x.TypeCode).Returns("7");

			if (populateConditionalFields)
			{
				mock.Setup(x => x.ID).Returns("12678976");
				mock.Setup(x => x.ImageFileFormat).Returns("PDF");
				mock.Setup(x => x.ImageFileName).Returns("ABC");
			}

			return mock.Object;
		}

		IGoodsShipment GetGoodsShipment(bool populateConditionalFields)
		{
			var mock = new Mock<IGoodsShipment>();
			mock.Setup(x => x.GovernmentAgencyGoodsItems).Returns(new[] { GetGovernmentAgencyGoodsItem(populateConditionalFields) });

			if (populateConditionalFields)
			{
				mock.Setup(x => x.Buyer).Returns(GetBuyer(populateConditionalFields));
				mock.Setup(x => x.Consignee).Returns(GetConsignee(populateConditionalFields));
				mock.Setup(x => x.Consignment).Returns(GetConsignment(populateConditionalFields));
				mock.Setup(x => x.Seller).Returns(GetSeller(populateConditionalFields));
			}

			return mock.Object;
		}

		IPartyDetails GetSeller(bool populateConditionalFields)
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.Name).Returns("Nishimoto Trading Co., Ltd.");
			mock.Setup(x => x.Address).Returns(GetAddress(populateConditionalFields, "US", "Unit 8, Holes Bay Park, Sterte Avenue, West Poole CA., USA"));
			return mock.Object;
		}

		IGovernmentAgencyGoodsItem GetGovernmentAgencyGoodsItem(bool populateConditionalFields)
		{
			var mock = new Mock<IGovernmentAgencyGoodsItem>();
			mock.Setup(x => x.AdditionalDocuments).Returns(new[] { GetAdditionalDocument() });
			mock.Setup(x => x.Commodity).Returns(GetCommodity(populateConditionalFields));
			mock.Setup(x => x.GoodsMeasure).Returns(GetGoodsMeasure);

			if (populateConditionalFields)
			{
				mock.Setup(x => x.SequenceNumeric).Returns(1);
				mock.Setup(x => x.AdditionalInformations).Returns(new[] { GetAdditionalInformation() });
				mock.Setup(x => x.Origin).Returns(GetOrigin);
			}

			return mock.Object;
		}

		IOrigin GetOrigin()
		{
			var mock = new Mock<IOrigin>();
			mock.Setup(x => x.CountryCode).Returns("US");
			return mock.Object;
		}

		IGoodsMeasure GetGoodsMeasure()
		{
			var mock = new Mock<IGoodsMeasure>();
			mock.Setup(x => x.TariffQuantity).Returns(100);
			mock.Setup(x => x.UnitCode).Returns("PKG");
			return mock.Object;
		}

		ICommodity GetCommodity(bool populateConditionalFields)
		{
			var mock = new Mock<ICommodity>();
			mock.Setup(x => x.Description).Returns("Hot Rolled Wire Rod");
			mock.Setup(x => x.Classifications).Returns(new[] { GetClassification() });
			mock.Setup(x => x.InvoiceLine).Returns(GetInvoiceLine(populateConditionalFields));

			if (populateConditionalFields)
			{
				mock.Setup(x => x.CommercialCategorizationID).Returns("G3507-AISI1008");
				mock.Setup(x => x.Name).Returns("Tyco");
				mock.Setup(x => x.AdditionalDocuments).Returns(new[] { GetAdditionalDocument() });
				mock.Setup(x => x.Constituent).Returns(GetConstituent);
			}

			return mock.Object;
		}

		IInvoiceLine GetInvoiceLine(bool populateConditionalFields)
		{
			var mock = new Mock<IInvoiceLine>();
			mock.Setup(x => x.ChargesTypeCode).Returns("FOB");
			mock.Setup(x => x.CurrencyTypeCode).Returns("USD");
			mock.Setup(x => x.SubTotalAmount).Returns(40000);

			if (populateConditionalFields)
			{
				mock.Setup(x => x.UnitPriceAmount).Returns(40);
			}

			return mock.Object;
		}

		IConstituent GetConstituent()
		{
			var mock = new Mock<IConstituent>();
			mock.Setup(x => x.ElementDescription).Returns("dia. 38mm");
			return mock.Object;
		}

		IClassification GetClassification()
		{
			var mock = new Mock<IClassification>();
			mock.Setup(x => x.ID).Returns("72139110007");
			return mock.Object;
		}

		IAdditionalDocument GetAdditionalDocument()
		{
			var mock = new Mock<IAdditionalDocument>();
			mock.Setup(x => x.SequenceNumeric).Returns(1);
			return mock.Object;
		}

		IConsignment GetConsignment(bool populateConditionalFields)
		{
			var mock = new Mock<IConsignment>();

			if (populateConditionalFields)
			{
				mock.Setup(x => x.AdditionalInformations).Returns(new[] { GetAdditionalInformation() });
				mock.Setup(x => x.LoadingLocation).Returns(GetLocation("NZNPE"));
				mock.Setup(x => x.TranshipmentLocation).Returns(GetLocation("NZNPE"));
				mock.Setup(x => x.TransitDeparture).Returns(GetLocation("NZNPE"));
				mock.Setup(x => x.UnloadingLocation).Returns(GetLocation("HKHKG"));
			}

			return mock.Object;
		}

		ILocation GetLocation(ZString id)
		{
			var mock = new Mock<ILocation>();
			mock.Setup(x => x.ID).Returns(id);
			return mock.Object;
		}

		IAdditionalInformation GetAdditionalInformation()
		{
			var mock = new Mock<IAdditionalInformation>();
			mock.Setup(x => x.StatementCode).Returns("1");
			mock.Setup(x => x.StatementDescription).Returns("NIL");
			return mock.Object;
		}

		IPartyDetails GetConsignee(bool populateConditionalFields)
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.Name).Returns("Nishimoto Trading. Co., Ltd.");
			mock.Setup(x => x.Address).Returns(GetAddress(populateConditionalFields, "US", "Unit 8, Holes Bay Park, Sterte Avenue, West Poole CA., USA"));
			return mock.Object;
		}

		IPartyDetails GetBuyer(bool populateConditionalFields)
		{
			var mock = new Mock<IPartyDetails>();
			mock.Setup(x => x.Name).Returns("IND Technology Co.");
			mock.Setup(x => x.Address).Returns(GetAddress(populateConditionalFields, "US", "1290 DeAnza Blvd., Fremont, CA., USA"));
			return mock.Object;
		}

		IAddress GetAddress(bool populateConditionalFields, ZString countryCode, ZString line)
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.CountryCode).Returns(countryCode);

			if (populateConditionalFields)
			{
				mock.Setup(x => x.Line).Returns(line);
			}

			return mock.Object;
		}

		IAdditionalInformation GetDeclarationAdditionalInformation(bool populateConditionalFields)
		{
			var mock = new Mock<IAdditionalInformation>();

			if (populateConditionalFields)
			{
				mock.Setup(x => x.Content).Returns("Order cancelled");
				mock.Setup(x => x.DelProcessNumber).Returns("FTBX93E0001366");
				mock.Setup(x => x.ProcessNumber).Returns("FTBX93E0001369");
			}

			return mock.Object;
		}

		IDeclarationAdditionalDocument GetDeclarationAdditionalDocument()
		{
			var mock = new Mock<IDeclarationAdditionalDocument>();
			mock.Setup(x => x.ID).Returns("FTBX93E0001373");
			return mock.Object;
		}

		protected override ITWMessageBuilder CreateNewMessageBuilder() => new NX201_01MessageBuilder();
	}
}
