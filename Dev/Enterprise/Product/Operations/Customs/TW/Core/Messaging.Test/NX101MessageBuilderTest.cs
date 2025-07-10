using System.Collections.Generic;
using CargoWise.Customs.TW.MessageDefinitions.NX101;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging.MessageBuilders;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Messaging.Testing
{
	[TestedType(typeof(NX101MessageBuilder))]
	sealed class NX101MessageBuilderTest : BaseTWMessageBuilderTest<INX101, Declaration>
	{
		[ExpectNoExceptions]
		public void TestPopulateDeclarationWithOptionalNodes()
		{
			var message = GetMessage();
			var declaration = new NX101MessageBuilder().PopulateDeclaration(message, MessageFunctionCode.Add);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("NX101.xml");

			CombineAssertions(() =>
			{
				AssertXMLContains(expected, actual);
				AsserContainsXmlByType(actual, declaration.GetType());
			});
		}

		public void TestPopulateDeclarationWithoutOptionalNodes()
		{
			var message = GetMessage(false);
			var declaration = new NX101MessageBuilder().PopulateDeclaration(message, MessageFunctionCode.Add);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("NX101_WithoutOptionalNodes.xml");
			AssertXMLContains(expected, actual);
		}

		public void TestDonotPopulateNodesWhenNoValue()
		{
			var message = GetMessage(true, true);
			var declaration = new NX101MessageBuilder().PopulateDeclaration(message, MessageFunctionCode.Add);

			var actual = XmlHelper.Serializer(typeof(Declaration), declaration, true);
			var expected = GetExpectedMessageXML("NX101_NotPopulateWhenNoValue.xml");
			AssertXMLContains(expected, actual);
		}

		protected override INX101 CreateDataSource()
		{
			return GetMessage();
		}

		protected override ITWMessageBuilder CreateNewMessageBuilder()
		{
			return new NX101MessageBuilder();
		}

		ICommunication GetCommunication(ZString id, ZString typeId)
		{
			var mock = new Mock<ICommunication>();
			mock.Setup(x => x.ID).Returns(id);
			mock.Setup(x => x.TypeID).Returns(typeId);
			return mock.Object;
		}

		IAddress GetAddress(ZString line, ZString chineseLine)
		{
			var mock = new Mock<IAddress>();
			mock.Setup(x => x.Line).Returns(line);
			mock.Setup(x => x.ChineseLine).Returns(chineseLine);
			return mock.Object;
		}

		IOrigin GetOrigin(ZString countryCode)
		{
			var mock = new Mock<IOrigin>();
			mock.Setup(x => x.CountryCode).Returns(countryCode);
			return mock.Object;
		}

		IPreviousDocument GetPreviousDocument(bool testNotPopulateWhenNoValue)
		{
			var mock = new Mock<IPreviousDocument>();
			mock.Setup(x => x.ID).Returns(testNotPopulateWhenNoValue ? ZString.Empty : new ZString("123456789_123456789_123456789_12345"));
			return mock.Object;
		}

		ILocation GetLocation(ZString id, ZString name, ZDate loadingDateTime, ZString estimatedLoadingCode)
		{
			var mock = new Mock<ILocation>();
			mock.Setup(x => x.ID).Returns(id);
			mock.Setup(x => x.Name).Returns(name);
			mock.Setup(x => x.LoadingDateTime).Returns(loadingDateTime);
			mock.Setup(x => x.EstimatedLoadingCode).Returns(estimatedLoadingCode);
			return mock.Object;
		}

		IAdditionalDocument GetAdditionalDocument()
		{
			var mock = new Mock<IAdditionalDocument>();
			mock.Setup(x => x.ID).Returns("123456789_");
			return mock.Object;
		}

		ITransportMeans GetBorderTransportMeans()
		{
			var mock = new Mock<ITransportMeans>();
			mock.Setup(x => x.Name).Returns("ABC");
			return mock.Object;
		}

		ITransportEquipment GetTransportEquipment()
		{
			var mock = new Mock<ITransportEquipment>();
			mock.Setup(x => x.ID).Returns("123456789_");
			return mock.Object;
		}

		IGoodsMeasure GetGoodsMeasure(ZDecimal tariffQuantity, ZString unitCode)
		{
			var mock = new Mock<IGoodsMeasure>();
			mock.Setup(x => x.TariffQuantity).Returns(tariffQuantity);
			mock.Setup(x => x.UnitCode).Returns(unitCode);
			return mock.Object;
		}

		IGoodsMeasure GetDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure(ZDecimal tariffQuantity, ZString unitCode, ZString customUnitCode)
		{
			var mock = new Mock<IGoodsMeasure>();
			mock.Setup(x => x.TariffQuantity).Returns(tariffQuantity);
			mock.Setup(x => x.UnitCode).Returns(unitCode);
			mock.Setup(x => x.CustomUnitCode).Returns(customUnitCode);
			return mock.Object;
		}

		IAdditionalInformation GetAdditionalInformation()
		{
			var mock = new Mock<IAdditionalInformation>();
			mock.Setup(x => x.ApprovalID).Returns("123456789_");
			return mock.Object;
		}

		IAdditionalDeclaration GetAdditionalDeclaration()
		{
			var mock = new Mock<IAdditionalDeclaration>();
			mock.Setup(x => x.ID).Returns("12345679_");
			mock.Setup(x => x.SequenceNumeric).Returns(1);
			return mock.Object;
		}

		IPackaging GetPackaging(bool testNotPopulateWhenNoValue = false)
		{
			var mock = new Mock<IPackaging>();
			mock.Setup(x => x.MarksNumbers).Returns(testNotPopulateWhenNoValue ? ZString.Empty : new ZString("100 PCE"));
			return mock.Object;
		}

		ICommodity GetDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity(bool optionalNodesArePopulated, bool testNotPopulateWhenNoValue)
		{
			var mockClassification = new Mock<IClassification>();
			mockClassification.Setup(x => x.ID).Returns("98123456781");
			mockClassification.Setup(x => x.IdentificationTypeCode).Returns("HS");

			var mockDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity = new Mock<ICommodity>();
			mockDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity.Setup(x => x.Description).Returns(new ZString('A', 513));
			mockDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity.Setup(x => x.Classifications).Returns(new[] { mockClassification.Object });
			mockDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity.Setup(x => x.PrintingTariffCode).Returns("01");
			if (optionalNodesArePopulated)
			{
				var mockInvoiceLine = new Mock<IInvoiceLine>();
				mockInvoiceLine.Setup(x => x.CurrencyTypeCode).Returns("TWD");
				mockInvoiceLine.Setup(x => x.ItemChargeAmount).Returns(123);

				var mockInvoice = new Mock<IInvoice>();
				mockInvoice.Setup(x => x.ID).Returns("123456");
				mockInvoice.Setup(x => x.IssueDateTime).Returns(new ZDate(2022, 02, 02));

				var mockConstituent = new Mock<IConstituent>();
				var mockCommodityRelatedPackaging = new Mock<ICommodityRelatedPackaging>();

				if (testNotPopulateWhenNoValue)
				{
					mockConstituent.Setup(x => x.ElementDescription).Returns(ZString.Empty);
					mockCommodityRelatedPackaging.Setup(x => x.Specification).Returns(ZString.Empty);
				}
				else
				{
					mockConstituent.Setup(x => x.ElementDescription).Returns("123");
					mockCommodityRelatedPackaging.Setup(x => x.Specification).Returns("SPE");
				}

				mockDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity.Setup(x => x.Constituent).Returns(mockConstituent.Object);
				mockDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity.Setup(x => x.CommodityRelatedPackaging).Returns(mockCommodityRelatedPackaging.Object);
				mockDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity.Setup(x => x.InvoiceLine).Returns(mockInvoiceLine.Object);
				mockDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity.Setup(x => x.Invoice).Returns(mockInvoice.Object);
			}
			return mockDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity.Object;
		}

		IGovernmentAgencyGoodsItem GetDeclarationGoodsShipmentGovernmentAgencyGoodsItem(bool optionalNodesArePopulated, bool testNotPopulateWhenNoValue)
		{
			var mockDeclarationGoodsShipmentGovernmentAgencyGoodsItem = new Mock<IGovernmentAgencyGoodsItem>();
			mockDeclarationGoodsShipmentGovernmentAgencyGoodsItem.Setup(x => x.SequenceNumeric).Returns(1);
			mockDeclarationGoodsShipmentGovernmentAgencyGoodsItem.Setup(x => x.Commodity).Returns(GetDeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity(optionalNodesArePopulated, testNotPopulateWhenNoValue));
			mockDeclarationGoodsShipmentGovernmentAgencyGoodsItem.Setup(x => x.GoodsMeasure).Returns(GetDeclarationGoodsShipmentGovernmentAgencyGoodsItemGoodsMeasure(123, "PCE", "CUC"));
			if (optionalNodesArePopulated)
			{
				var mockManufacturer = new Mock<IPartyDetails>();
				mockManufacturer.Setup(x => x.ID).Returns("55558585");

				mockDeclarationGoodsShipmentGovernmentAgencyGoodsItem.Setup(x => x.Manufacturer).Returns(mockManufacturer.Object);
				mockDeclarationGoodsShipmentGovernmentAgencyGoodsItem.Setup(x => x.AdditionalDeclaration).Returns(GetAdditionalDeclaration());
				mockDeclarationGoodsShipmentGovernmentAgencyGoodsItem.Setup(x => x.Packaging).Returns(GetPackaging(testNotPopulateWhenNoValue));
			}
			return mockDeclarationGoodsShipmentGovernmentAgencyGoodsItem.Object;
		}

		IPartyDetails GetExporter(bool optionalNodesArePopulated, bool testNotPopulateWhenNoValue)
		{
			var mockExporter = new Mock<IPartyDetails>();
			if (optionalNodesArePopulated)
			{
				mockExporter.Setup(x => x.ID).Returns("5289317");
				mockExporter.Setup(x => x.Name).Returns("Exporter Chinese Name");
				mockExporter.Setup(x => x.ChineseName).Returns("出口人中文名稱");
				mockExporter.Setup(x => x.TypeCode).Returns("58");
				mockExporter.Setup(x => x.Address).Returns(GetAddress(testNotPopulateWhenNoValue ? "" : "Exporter English Address", testNotPopulateWhenNoValue ? "" : "出口人英文名稱"));
			}
			return mockExporter.Object;
		}

		IEnumerable<IAdditionalInformation> GetGoodsShipmentConsignmentAdditionalInformations()
		{
			var mock = new Mock<IAdditionalInformation>();
			mock.Setup(x => x.StatementDescription).Returns("Statement description");
			yield return mock.Object;
		}

		IConsignment GetDeclarationGoodsShipmentConsignment(bool optionalNodesArePopulated)
		{
			var mockDeclarationGoodsShipmentConsignment = new Mock<IConsignment>();
			mockDeclarationGoodsShipmentConsignment.Setup(x => x.BorderTransportMeans).Returns(GetBorderTransportMeans());
			mockDeclarationGoodsShipmentConsignment.Setup(x => x.UnloadingLocation).Returns(GetLocation("TWTPE", null, ZDate.Empty, null));
			mockDeclarationGoodsShipmentConsignment.Setup(x => x.AdditionalInformations).Returns(GetGoodsShipmentConsignmentAdditionalInformations);

			if (optionalNodesArePopulated)
			{
				mockDeclarationGoodsShipmentConsignment.Setup(x => x.LoadingLocation).Returns(GetLocation("TWTPE", "Z99Port", new ZDate(2022, 02, 02), "Y"));
				mockDeclarationGoodsShipmentConsignment.Setup(x => x.TransportEquipments).Returns(new[] { GetTransportEquipment(), GetTransportEquipment() });
				mockDeclarationGoodsShipmentConsignment.Setup(x => x.DepartureTransportMeans).Returns(GetBorderTransportMeans());
			}
			return mockDeclarationGoodsShipmentConsignment.Object;
		}

		IGoodsShipment GetGoodsShipment(bool optionalNodesArePopulated, bool testNotPopulateWhenNoValue)
		{
			var mockGoodsShipment = new Mock<IGoodsShipment>();
			mockGoodsShipment.Setup(x => x.GovernmentAgencyGoodsItems).Returns(new[] { GetDeclarationGoodsShipmentGovernmentAgencyGoodsItem(optionalNodesArePopulated, testNotPopulateWhenNoValue) });

			if (optionalNodesArePopulated)
			{
				mockGoodsShipment.Setup(x => x.AdditionalDocuments).Returns(new[] { GetAdditionalDocument(), GetAdditionalDocument() });
				mockGoodsShipment.Setup(x => x.Consignment).Returns(GetDeclarationGoodsShipmentConsignment(optionalNodesArePopulated));
				mockGoodsShipment.Setup(x => x.Exporter).Returns(GetExporter(optionalNodesArePopulated, testNotPopulateWhenNoValue));
				mockGoodsShipment.Setup(x => x.GoodsMeasures).Returns(new[] { GetGoodsMeasure(123, "PCE"), GetGoodsMeasure(456, "BOX") });
				mockGoodsShipment.Setup(x => x.AdditionalInformations).Returns(new[] { GetAdditionalInformation(), GetAdditionalInformation() });
				mockGoodsShipment.Setup(x => x.AdditionalDeclarations).Returns(new[] { GetAdditionalDeclaration(), GetAdditionalDeclaration() });
			}
			return mockGoodsShipment.Object;
		}

		INX101Application GetApplication(bool optionalNodesArePopulated)
		{
			var mockApplication = new Mock<INX101Application>();
			mockApplication.Setup(x => x.AdhocCode).Returns("Y");
			mockApplication.Setup(x => x.GoodsReleaseCode).Returns("01");
			mockApplication.Setup(x => x.OriginalCopyQuantity).Returns(3);
			mockApplication.Setup(x => x.PrintingCode).Returns("01");
			mockApplication.Setup(x => x.TypeCode).Returns("15");
			mockApplication.Setup(x => x.ContactOffice).Returns("CA");
			mockApplication.Setup(x => x.DescriptionTooLong).Returns("Y");

			if (optionalNodesArePopulated)
			{
				var mockAgent = new Mock<IPartyDetails>();
				mockAgent.Setup(x => x.ID).Returns("45614567");
				mockAgent.Setup(x => x.Name).Returns("報驗申辦代理人中文名稱");
				mockAgent.Setup(x => x.TypeCode).Returns("58");
				mockAgent.Setup(x => x.Address).Returns(GetAddress(null, "報驗申辦代理人中文地址"));
				mockAgent.Setup(x => x.Communications).Returns(new[] { GetCommunication("02-4125-9856", "TA") });

				var mockApplicant = new Mock<IPartyDetails>();
				mockApplicant.Setup(x => x.ID).Returns("12349999");
				mockApplicant.Setup(x => x.ChineseName).Returns("申請人中文名稱");
				mockApplicant.Setup(x => x.TypeCode).Returns("58");
				mockApplicant.Setup(x => x.Address).Returns(GetAddress(null, "申請人中文地址"));
				mockApplicant.Setup(x => x.Communications).Returns(new[] { GetCommunication("02-4125-9856", "TA") });
				mockApplicant.Setup(x => x.UndertakeCode).Returns("01");

				mockApplication.Setup(x => x.AdhocProcessNumber).Returns("ABC123");
				mockApplication.Setup(x => x.CopyQuantity).Returns(new ZInt(1));
				mockApplication.Setup(x => x.ECFAPrintingDescription).Returns("01");
				mockApplication.Setup(x => x.EUSteelDeclarationCode).Returns("01");
				mockApplication.Setup(x => x.EUSteelPhaseCode).Returns("01");
				mockApplication.Setup(x => x.FishingBoatName).Returns("ABC");
				mockApplication.Setup(x => x.FishingCONoExport).Returns("12345");
				mockApplication.Setup(x => x.GoodsReleaseReasonCode).Returns("01");
				mockApplication.Setup(x => x.ManufacturerPrintingCode).Returns("01");
				mockApplication.Setup(x => x.Observations).Returns("Observations");
				mockApplication.Setup(x => x.PreviousCORenderCode).Returns("Y");
				mockApplication.Setup(x => x.TriangularTradeCode).Returns("Y");
				mockApplication.Setup(x => x.Agent).Returns(mockAgent.Object);
				mockApplication.Setup(x => x.Applicant).Returns(mockApplicant.Object);
			}
			return mockApplication.Object;
		}

		IPartyDetails GetManufacturer(bool optionalNodesArePopulated, bool testNotPopulateWhenNoValue)
		{
			var mockManufacturer = new Mock<IPartyDetails>();
			mockManufacturer.Setup(x => x.TypeCode).Returns("58");
			mockManufacturer.Setup(x => x.ID).Returns("52889317");
			mockManufacturer.Setup(x => x.MainManufacturer).Returns("Y");
			if (optionalNodesArePopulated)
			{
				mockManufacturer.Setup(x => x.Name).Returns("Manufacturer English Name");
				mockManufacturer.Setup(x => x.ChineseName).Returns("製造廠商中文名稱");
				mockManufacturer.Setup(x => x.Address).Returns(GetAddress(testNotPopulateWhenNoValue ? "" : "Manufacturer English Address", testNotPopulateWhenNoValue ? "" : "製造廠商中文地址"));
				mockManufacturer.Setup(x => x.Communications).Returns(new[] { GetCommunication("example@gamil.com", "MA"), GetCommunication("02-9999-9999", "TE"), GetCommunication("02-8888-8888", "FX") });
			}
			return mockManufacturer.Object;
		}

		INX101GovernmentAgencyGoodsItem GetDeclarationConsignmentGovernmentAgencyGoodsItem(bool optionalNodesArePopulated, bool testNotPopulateWhenNoValue)
		{
			var mockINX101GovernmentAgencyGoodsItem = new Mock<INX101GovernmentAgencyGoodsItem>();
			mockINX101GovernmentAgencyGoodsItem.Setup(x => x.Manufacturers).Returns(new[] { GetManufacturer(optionalNodesArePopulated, testNotPopulateWhenNoValue), GetManufacturer(optionalNodesArePopulated, testNotPopulateWhenNoValue) });
			mockINX101GovernmentAgencyGoodsItem.Setup(x => x.Origins).Returns(new[] { GetOrigin("TW"), GetOrigin("AU") });
			mockINX101GovernmentAgencyGoodsItem.Setup(x => x.PreviousDocuments).Returns(new[] { GetPreviousDocument(testNotPopulateWhenNoValue), GetPreviousDocument(testNotPopulateWhenNoValue) });
			return mockINX101GovernmentAgencyGoodsItem.Object;
		}

		INX101Consignment GetDeclarationConsignment(bool optionalNodesArePopulated, bool testNotPopulateWhenNoValue)
		{
			var mockINX101Consignment = new Mock<INX101Consignment>();
			if (optionalNodesArePopulated)
			{
				mockINX101Consignment.Setup(x => x.AdditionalDocument).Returns(new[] { GetAdditionalDocument(), GetAdditionalDocument() });
				mockINX101Consignment.Setup(x => x.GovernmentAgencyGoodsItem).Returns(GetDeclarationConsignmentGovernmentAgencyGoodsItem(optionalNodesArePopulated, testNotPopulateWhenNoValue));
				mockINX101Consignment.Setup(x => x.UnloadingLocation).Returns(GetLocation("TWTPE", "Name", new ZDate(2022, 02, 02), null));
			}
			return mockINX101Consignment.Object;
		}

		INX101 GetMessage(bool optionalNodesArePopulated = true, bool testNotPopulateWhenNoValue = false)
		{
			var mock = new Mock<INX101>();
			mock.Setup(x => x.FunctionalReferenceID).Returns("12345678002111070001");
			mock.Setup(x => x.FunctionCode).Returns("9");
			mock.Setup(x => x.GoodsShipment).Returns(GetGoodsShipment(optionalNodesArePopulated, testNotPopulateWhenNoValue));
			mock.Setup(x => x.Application).Returns(GetApplication(optionalNodesArePopulated));

			if (optionalNodesArePopulated)
			{
				var mockImporter = new Mock<IPartyDetails>();
				mockImporter.Setup(x => x.ID).Returns("55558888");
				mockImporter.Setup(x => x.Name).Returns("Importer English Name");
				mockImporter.Setup(x => x.ChineseName).Returns("進口人中文名稱");
				mockImporter.Setup(x => x.Address).Returns(GetAddress(testNotPopulateWhenNoValue ? "" : "Importer English Address", testNotPopulateWhenNoValue ? "" : "進口人中文地址"));
				mockImporter.Setup(x => x.Communications).Returns(new[] { GetCommunication("importer@gmail.com", "MA"), GetCommunication("02-6666-7777", "TE"), GetCommunication("02-9999-0000", "FX") });
				mockImporter.Setup(x => x.UndertakeCode).Returns("01");

				var mockGovernmentProcedure = new Mock<IGovernmentProcedure>();
				mockGovernmentProcedure.Setup(x => x.Description).Returns("其他申報事項");

				mock.Setup(x => x.Consignment).Returns(GetDeclarationConsignment(optionalNodesArePopulated, testNotPopulateWhenNoValue));
				mock.Setup(x => x.GovernmentProcedure).Returns(new[] { mockGovernmentProcedure.Object });
				mock.Setup(x => x.Packaging).Returns(GetPackaging(testNotPopulateWhenNoValue));
				mock.Setup(x => x.PreviousDocument).Returns(GetPreviousDocument(testNotPopulateWhenNoValue));
				mock.Setup(x => x.COImporter).Returns(mockImporter.Object);
			}
			return mock.Object;
		}
	}
}
