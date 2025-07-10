using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class CREMessageBuilderTest : TSWMessageBuilderTest
	{
		public void TestDeclarantPinRequired()
		{
			SetUpMocks();

			Assert(creBuilder.DeclarantPinRequired);
			consignment1.Reset();
			consignment1.Setup(m => m.WriteOffRequest).Returns(false);
			consignment2.Reset();
			consignment2.Setup(m => m.WriteOffRequest).Returns(false);
			Assert(!creBuilder.DeclarantPinRequired);
			iCargoReportExportMock.Reset();
			iCargoReportExportMock.Setup(m => m.HasEmptyContainersOnly).Returns(true);
			Assert(!creBuilder.DeclarantPinRequired);
			consignment1.Reset();
			consignment1.Setup(m => m.WriteOffRequest).Returns(true);
			Assert(!creBuilder.DeclarantPinRequired);
		}

		public void TestCREMessageOriginal()
		{
			SetUpMocks();

			AssertMultilineASCIIEquals(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestCREMessageOriginal.txt")), creBuilder.GetXMLMessage());
			iCargoReportExportMock.Reset();
			iCargoReportExportMock.Reset();
			iCargoReportExportMock.Setup(m => m.UseInterfaceSequenceNumber).Returns(false);
			var unexpectedXML = @"<Carrier>
    <Name>IORGANISATIONSIMPLEMOCK_NAME</Name>
    <ID>51358595A</ID>
  </Carrier>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
			iCargoReportExportMock.Reset();
			iCargoReportExportMock.Setup(m => m.HasEmptyContainersOnly).Returns(true);
			unexpectedXML = @"<Declarant>
    <ID>DeclarantID</ID>
    <Communication>
      <ID>ICOMMUNICATION1_CONTACTDETAIL</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>ICOMMUNICATION2_CONTACTDETAIL</ID>
      <TypeID>EM</TypeID>
    </Communication>
  </Declarant>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
		}

		public void TestCREMessageReplace()
		{
			SetUpMocks();

			creBuilder = new CREMessageBuilder(iCargoReportExportMock.Object, TSWTransactionTypes.Replace, "00009908C");
			AssertMultilineASCIIEquals(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestCREMessageReplace.txt")), creBuilder.GetXMLMessage());
			iCargoReportExportMock.Reset();
			var unexpectedXML = @"<Carrier>
    <Name>IORGANISATIONSIMPLEMOCK_NAME</Name>
    <ID>51358595A</ID>
  </Carrier>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
			iCargoReportExportMock.Reset();
			iCargoReportExportMock.Setup(m => m.HasEmptyContainersOnly).Returns(true);
			unexpectedXML = @"<Declarant>
    <ID>DeclarantID</ID>
    <Communication>
      <ID>ICOMMUNICATION1_CONTACTDETAIL</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>ICOMMUNICATION2_CONTACTDETAIL</ID>
      <TypeID>EM</TypeID>
    </Communication>
  </Declarant>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
		}

		public void TestCREMessageCancel()
		{
			SetUpMocks();

			creBuilder = new CREMessageBuilder(iCargoReportExportMock.Object, TSWTransactionTypes.Cancel, "00009908C");
			AssertMultilineASCIIEquals(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestCREMessageCancel.txt")), creBuilder.GetXMLMessage());
			iCargoReportExportMock.Reset();
			iCargoReportExportMock.Setup(m => m.HasEmptyContainersOnly).Returns(true);
			var unexpectedXML = @"<Declarant>
    <ID>DeclarantID</ID>
    <Communication>
      <ID>ICOMMUNICATION1_CONTACTDETAIL</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>ICOMMUNICATION2_CONTACTDETAIL</ID>
      <TypeID>EM</TypeID>
    </Communication>
  </Declarant>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
		}

		public void TestGrossWeightIsRoundedTo3DecimalPlaces()
		{
			SetUpMocks();
			AssertContains("<GrossMassMeasure unitCode=\"KGM\">1.361</GrossMassMeasure>", creBuilder.GetXMLMessage());
		}

		public void TestPopulateReferenceNo()
		{
			SetUpMocks();
			creBuilder = new CREMessageBuilder(iCargoReportExportMock.Object, TSWTransactionTypes.Replace, "00009908C");
			AssertContains("<ID>ENTRY12345</ID>", creBuilder.GetXMLMessage());
		}

		public void TestPopulateMessageType()
		{
			SetUpMocks();
			AssertContains("<TypeCode>CRE</TypeCode>", creBuilder.GetXMLMessage());
		}

		public void TestPopulateSendersRef()
		{
			SetUpMocks();
			AssertContains("<FunctionalReferenceID>C00001165</FunctionalReferenceID>", creBuilder.GetXMLMessage());
		}

		public void TestPopulateTransType()
		{
			SetUpMocks();
			creBuilder = new CREMessageBuilder(iCargoReportExportMock.Object, TSWTransactionTypes.Cancel, "00009908C");
			AssertContains("<FunctionCode>1</FunctionCode>", creBuilder.GetXMLMessage());
			creBuilder = new CREMessageBuilder(iCargoReportExportMock.Object, TSWTransactionTypes.Change, "00009908C");
			AssertContains("<FunctionCode>4</FunctionCode>", creBuilder.GetXMLMessage());
			creBuilder = new CREMessageBuilder(iCargoReportExportMock.Object, TSWTransactionTypes.Completion, "00009908C");
			AssertContains("<FunctionCode>22</FunctionCode>", creBuilder.GetXMLMessage());
			creBuilder = new CREMessageBuilder(iCargoReportExportMock.Object, TSWTransactionTypes.None, "00009908C");
			AssertContains("<FunctionCode />", creBuilder.GetXMLMessage());
			creBuilder = new CREMessageBuilder(iCargoReportExportMock.Object, TSWTransactionTypes.Original, "00009908C");
			AssertContains("<FunctionCode>9</FunctionCode>", creBuilder.GetXMLMessage());
			creBuilder = new CREMessageBuilder(iCargoReportExportMock.Object, TSWTransactionTypes.Replace, "00009908C");
			AssertContains("<FunctionCode>5</FunctionCode>", creBuilder.GetXMLMessage());
		}

		public void TestPopulateSubmitter()
		{
			SetUpMocks();
			var expectedXML = @"<Submitter>
    <ID>00009908C</ID>
  </Submitter>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalDocs()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalDocument>
    <CategoryCode>CDO</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""TEST1.PDF"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>
  <AdditionalDocument>
    <CategoryCode>INV</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""TEST2.PDF"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());

			var tswAttachment1 = new Mock<ITSWAttachment>();
			var tswAttachment2 = new Mock<ITSWAttachment>();
			tswAttachment1.Setup(m => m.DocType).Returns("AAA");
			tswAttachment1.Setup(m => m.FileName).Returns("TEST11.PDF");
			tswAttachment2.Setup(m => m.DocType).Returns("BBB");
			tswAttachment2.Setup(m => m.FileName).Returns("TEST22.PDF");
			additionalInformationMock.Setup(m => m.SupportingDocuments).Returns(Array.Empty<ITSWAttachment>());
			iCargoReportExportMock.Setup(m => m.SupportingDocuments).Returns(new ITSWAttachment[] { tswAttachment1.Object, tswAttachment2.Object });
			expectedXML = @"<AdditionalDocument>
    <CategoryCode>AAA</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""TEST11.PDF"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>
  <AdditionalDocument>
    <CategoryCode>BBB</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""TEST22.PDF"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalDocsWithNullAdditionalInformation()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalDocument>
    <CategoryCode>CDO</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""TEST1.PDF"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>
  <AdditionalDocument>
    <CategoryCode>INV</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""TEST2.PDF"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>";
			iCargoReportExportMock.Reset();
			AssertNotContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalInfoOriginal()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
    <Content>ADDITIONALINFORMATIONMOCK_FREETEXT</Content>
  </AdditionalInformation>
  <AdditionalInformation>
    <RequestOverrideCode>Y</RequestOverrideCode>
    <StatementDescription>ADDITIONALINFORMATIONMOCK_MANUALOVERRIDETEXT</StatementDescription>
    <StatementTypeCode>ALP</StatementTypeCode>
  </AdditionalInformation>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
			additionalInformationMock.Reset();
			additionalInformationMock.Setup(m => m.FreeText).Returns(ZString.Empty);
			expectedXML = @"<AdditionalInformation>
    <Content>ADDITIONALINFORMATIONMOCK_FREETEXT</Content>
  </AdditionalInformation>";
			AssertNotContains(expectedXML, creBuilder.GetXMLMessage());
			additionalInformationMock.Reset();
			additionalInformationMock.Setup(m => m.ManualOverrideText).Returns(ZString.Empty);
			AssertNotContains("<StatementDescription>ADDITIONALINFORMATIONMOCK_MANUALOVERRIDETEXT</StatementDescription>", creBuilder.GetXMLMessage());
			var changeCancelReasonXML = @"<AdditionalInformation>
    <StatementDescription>ADDITIONALSTATEMENTTEXT</StatementDescription>
    <StatementTypeCode>AES</StatementTypeCode>
  </AdditionalInformation>";
			AssertNotContains(changeCancelReasonXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalInfoReplace()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
    <Content>ADDITIONALINFORMATIONMOCK_FREETEXT</Content>
  </AdditionalInformation>
  <AdditionalInformation>
    <RequestOverrideCode>Y</RequestOverrideCode>
    <StatementDescription>ADDITIONALINFORMATIONMOCK_MANUALOVERRIDETEXT</StatementDescription>
    <StatementTypeCode>ALP</StatementTypeCode>
  </AdditionalInformation>";
			creBuilder = new CREMessageBuilder(iCargoReportExportMock.Object, TSWTransactionTypes.Replace, "00009908C");
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
			additionalInformationMock.Reset();
			additionalInformationMock.Setup(m => m.FreeText).Returns(ZString.Empty);
			expectedXML = @"<AdditionalInformation>
    <Content>ADDITIONALINFORMATIONMOCK_FREETEXT</Content>
  </AdditionalInformation>";
			AssertNotContains(expectedXML, creBuilder.GetXMLMessage());
			additionalInformationMock.Reset();
			additionalInformationMock.Setup(m => m.ManualOverrideText).Returns(ZString.Empty);
			AssertNotContains("<StatementDescription>ADDITIONALINFORMATIONMOCK_MANUALOVERRIDETEXT</StatementDescription>", creBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalInfoCancel()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
    <StatementDescription>ADDITIONALSTATEMENTTEXT</StatementDescription>
    <StatementTypeCode>AES</StatementTypeCode>
  </AdditionalInformation>";
			creBuilder = new CREMessageBuilder(iCargoReportExportMock.Object, TSWTransactionTypes.Cancel, "00009908C");
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalInfoWithNullAdditionalInformation()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
    <Content>ADDITIONALINFORMATIONMOCK_FREETEXT</Content>
  </AdditionalInformation>
  <AdditionalInformation>
    <RequestOverrideCode>Y</RequestOverrideCode>
    <StatementDescription>ADDITIONALINFORMATIONMOCK_MANUALOVERRIDETEXT</StatementDescription>
    <StatementTypeCode>ALP</StatementTypeCode>
  </AdditionalInformation>";
			iCargoReportExportMock.Reset();
			AssertNotContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestManualOverrideInfo()
		{
			SetUpMocks();
			AssertContains("<StatementDescription>ADDITIONALINFORMATIONMOCK_MANUALOVERRIDETEXT</StatementDescription>", creBuilder.GetXMLMessage());
		}

		public void TestChangeCancelReason()
		{
			SetUpMocks();
			var changeCancelReasonXML = @"<AdditionalInformation>
    <StatementDescription>ADDITIONALSTATEMENTTEXT</StatementDescription>
    <StatementTypeCode>AES</StatementTypeCode>
  </AdditionalInformation>";
			creBuilder = new CREMessageBuilder(iCargoReportExportMock.Object, TSWTransactionTypes.Cancel, "00009908C");
			AssertContains(changeCancelReasonXML, creBuilder.GetXMLMessage());
			creBuilder = new CREMessageBuilder(iCargoReportExportMock.Object, TSWTransactionTypes.Replace, "00009908C");
			AssertContains(changeCancelReasonXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateBorderTMSea()
		{
			SetUpMocks();
			var expectedBorderTransportMeansXML = @"<BorderTransportMeans>
    <Name>CRAFTNAME</Name>
    <ID>LLOYDSNO</ID>
    <TypeCode>1</TypeCode>
    <DepartureDateTime formatCode=""102"">20181022</DepartureDateTime>
    <JourneyID>VOYAGENO</JourneyID>
  </BorderTransportMeans>";
			AssertContains(expectedBorderTransportMeansXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateBorderTMAir()
		{
			SetUpMocks();
			var expectedBorderTransportMeansXML = @"<BorderTransportMeans>
    <Name>FLIGHTNO</Name>
    <TypeCode>4</TypeCode>
    <DepartureDateTime formatCode=""102"">20181022</DepartureDateTime>
  </BorderTransportMeans>";
			iCargoReportExportMock.Setup(m => m.IsSea).Returns(false);
			iCargoReportExportMock.Setup(m => m.IsAir).Returns(true);
			AssertContains(expectedBorderTransportMeansXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateBorderTMMail()
		{
			SetUpMocks();
			var expectedBorderTransportMeansXML = @"<BorderTransportMeans>
    <Name>FLIGHTNO</Name>
    <TypeCode>5</TypeCode>
    <DepartureDateTime formatCode=""102"">20181022</DepartureDateTime>
  </BorderTransportMeans>";
			iCargoReportExportMock.Setup(m => m.IsSea).Returns(false);
			iCargoReportExportMock.Setup(m => m.IsMail).Returns(true);
			AssertContains(expectedBorderTransportMeansXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateCarrier()
		{
			SetUpMocks();
			var expectedXML = @"<Carrier>
    <Name>IORGANISATIONSIMPLEMOCK_NAME</Name>
    <ID>51358595A</ID>
  </Carrier>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
			iOrganisationSimpleMock.Setup(m => m.CustomsClientCode).Returns(ZString.Empty);
			expectedXML = @"<Carrier>
    <Name>IORGANISATIONSIMPLEMOCK_NAME</Name>
  </Carrier>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestFlightNoIsCapitalized()
		{
			SetUpMocks();
			iCargoReportExportMock.Setup(m => m.IsSea).Returns(false);
			iCargoReportExportMock.Setup(m => m.IsAir).Returns(true);
			iCargoReportExportMock.Setup(m => m.FlightNo).Returns("nz210");
			iCargoReportExportMock.Setup(m => m.DepartureDate).Returns(new ZDateTime(2025, 05, 06));
			var expectedBorderTransportMeansXML = @"<BorderTransportMeans>
    <Name>NZ210</Name>
    <TypeCode>4</TypeCode>
    <DepartureDateTime formatCode=""102"">20250506</DepartureDateTime>
  </BorderTransportMeans>";
			AssertContains("Flight Number should be in Uppercase", expectedBorderTransportMeansXML, creBuilder.GetXMLMessage());
		}

		public void TestCraftAndVoyageNumberIsCapitalized()
		{
			SetUpMocks();
			iCargoReportExportMock.Setup(m => m.CraftName).Returns("Hyogo Maru");
			iCargoReportExportMock.Setup(m => m.DepartureDate).Returns(new ZDateTime(2025, 04, 29));
			iCargoReportExportMock.Setup(m => m.LloydsNo).Returns("5892347");
			iCargoReportExportMock.Setup(m => m.VoyageNo).Returns("85e");
			var expectedBorderTransportMeansXML = @"<BorderTransportMeans>
    <Name>HYOGO MARU</Name>
    <ID>5892347</ID>
    <TypeCode>1</TypeCode>
    <DepartureDateTime formatCode=""102"">20250429</DepartureDateTime>
    <JourneyID>85E</JourneyID>
  </BorderTransportMeans>";
			AssertContains(expectedBorderTransportMeansXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateDeclarant()
		{
			SetUpMocks();

			var expectedXML = @"<Declarant>
    <ID>DeclarantID</ID>
    <Communication>
      <ID>ICOMMUNICATION1_CONTACTDETAIL</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>ICOMMUNICATION2_CONTACTDETAIL</ID>
      <TypeID>EM</TypeID>
    </Communication>
  </Declarant>";
			iCargoReportExportMock.Setup(m => m.HasEmptyContainersOnly).Returns(true);
			AssertNotContains(expectedXML, creBuilder.GetXMLMessage());
			iCargoReportExportMock.Setup(m => m.HasEmptyContainersOnly).Returns(false);
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
			iCargoReportExportMock.Reset();
			AssertNotContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulatePortOfDeparture()
		{
			SetUpMocks();
			var expectedXML = @"<ExitOffice>
    <ID>PortOfDeparture</ID>
  </ExitOffice>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentsSea()
		{
			SetUpMocks();

			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestCREPopulateConsignmentsSea.txt")), creBuilder.GetXMLMessage());
			consignment1.Setup(m => m.DeliverToParty);
			consignment2.Setup(m => m.DeliverToParty);
			var unexpectedXML = @"<DeliveryDestination>
      <Name>IORGANISATION1_NAME</Name>
      <Address>
        <CityName>IORGANISATION1_CITY</CityName>
        <CountryCode>IORGANISATION1_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>IORGANISATION1_COUNTRYREGION</CountrySubDivisionName>
        <Line>IORGANISATION1_ADDRESS</Line>
        <PostcodeID>IORGANISATION1_POSTCODE</PostcodeID>
      </Address>
    </DeliveryDestination>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
			iOrganisation1.Setup(m => m.Name).Returns(consignee.Object.Name);
			consignment1.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment2.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
			consignment2.Setup(m => m.FreightPaymentMethod);
			consignment2.Setup(m => m.FreightPaymentMethod).Returns(ZString.Empty);
			unexpectedXML = @"<Freight>
      <PaymentMethodCode>CONSIGNMENT2_FREIGHTPAYMENTMETHOD</PaymentMethodCode>
    </Freight>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
			consignment1.Setup(m => m.GoodsLocation);
			consignment1.Setup(m => m.GoodsLocation).Returns(ZString.Empty);
			unexpectedXML = @"<GoodsLocation>
      <ID>CONSIGNMENT1_GOODSLOCATION</ID>
    </GoodsLocation>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentsAir()
		{
			var tswAttachment1 = new Mock<ITSWAttachment>();
			tswAttachment1.Setup(m => m.DocType).Returns("CDO");
			tswAttachment1.Setup(m => m.FileName).Returns("TEST1.PDF");

			var tswAttachment2 = new Mock<ITSWAttachment>();
			tswAttachment2.Setup(m => m.DocType).Returns("INV");
			tswAttachment2.Setup(m => m.FileName).Returns("TEST2.PDF");

			additionalInformationMock = new Mock<IAdditionalInformation>();
			additionalInformationMock.Setup(m => m.SupportingDocuments).Returns(new List<ITSWAttachment>() { tswAttachment1.Object, tswAttachment2.Object });
			additionalInformationMock.Setup(m => m.FreeText).Returns("ADDITIONALINFORMATIONMOCK_FREETEXT");
			additionalInformationMock.Setup(m => m.ManualOverrideText).Returns("ADDITIONALINFORMATIONMOCK_MANUALOVERRIDETEXT");
			additionalInformationMock.Setup(m => m.AdditionalStatementText).Returns("ADDITIONALSTATEMENTTEXT");

			iOrganisationSimpleMock = new Mock<IOrganisationSimple>();
			iOrganisationSimpleMock.Setup(m => m.Name).Returns("IORGANISATIONSIMPLEMOCK_NAME");
			iOrganisationSimpleMock.Setup(m => m.CustomsClientCode).Returns("51358595A");

			consignment1ITRDetails = new Mock<ITranshipmentDetails>();
			consignment1ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment1ITRDetails.Setup(m => m.ITRImportMode).Returns("4");
			consignment1ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("CONSIGNMENT1_ITRVOYAGEFLIGHT");
			consignment1ITRDetails.Setup(m => m.ITRArrivalDate).Returns(new ZDateTime(2018, 10, 22));
			consignment1ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("CONSIGNMENT1_MODEOFTRANSPORTFORTRANSFER");
			consignment1ITRDetails.Setup(m => m.ITRImportCraft).Returns("CONSIGNMENT1_ITRIMPORTCRAFT");

			consignment2ITRDetails = new Mock<ITranshipmentDetails>();
			consignment2ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment2ITRDetails.Setup(m => m.ITRImportMode).Returns("1");
			consignment2ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("CONSIGNMENT2_ITRVOYAGEFLIGHT");
			consignment2ITRDetails.Setup(m => m.ITRArrivalDate).Returns(new ZDateTime(2018, 10, 21));
			consignment2ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("CONSIGNMENT2_MODEOFTRANSPORTFORTRANSFER");
			consignment2ITRDetails.Setup(m => m.ITRImportCraft).Returns("CONSIGNMENT2_ITRIMPORTCRAFT");

			consignee = new Mock<IPartyInformation>();
			consignee.Setup(m => m.Name).Returns("CONSIGNEE_NAME");
			consignee.Setup(m => m.City).Returns("CONSIGNEE_CITY");
			consignee.Setup(m => m.CountryCode).Returns("CONSIGNEE_COUNTRYCODE");
			consignee.Setup(m => m.CountryRegion).Returns("CONSIGNEE_COUNTRYREGION");
			consignee.Setup(m => m.Address).Returns("CONSIGNEE_ADDRESS");
			consignee.Setup(m => m.PostCode).Returns("CONSIGNEE_POSTCODE");

			var classifications1 = new Mock<IClassification>();
			classifications1.Setup(m => m.Classification).Returns("CLASSIFICATIONS1_CLASSIFICATION");
			classifications1.Setup(m => m.ClassificationTypeCode).Returns("SSO");

			var classifications2 = new Mock<IClassification>();
			classifications2.Setup(m => m.Classification).Returns("CLASSIFICATIONS1_CLASSIFICATION");
			classifications2.Setup(m => m.ClassificationTypeCode).Returns("SSO");

			var identifier1 = new Mock<ICommodity>();
			identifier1.Setup(m => m.CommodityNumber).Returns("IDENTIFIER1_COMMODITYNUMBER");
			identifier1.Setup(m => m.CommodityType).Returns("IDENTIFIER1_COMMODITYTYPE");

			var identifier2 = new Mock<ICommodity>();
			identifier2.Setup(m => m.CommodityNumber).Returns("IDENTIFIER2_COMMODITYNUMBER");
			identifier2.Setup(m => m.CommodityType).Returns("IDENTIFIER2_COMMODITYTYPE");

			consignmentItem1 = new Mock<ICREConsignmentItem>();
			consignmentItem1.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM1_GOODSDESCRIPTION");
			consignmentItem1.Setup(m => m.UNDGHazardousGoodsCode).Returns("CONSIGNMENTITEM1_UNDGHAZARDOUSGOODSCODE");
			consignmentItem1.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem1.Setup(m => m.Value).Returns(9.673);
			consignmentItem1.Setup(m => m.GrossWeightInKg).Returns(1.360777m);
			consignmentItem1.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM1_GOODSORIGINCOUNTRY");
			consignmentItem1.Setup(m => m.PackageQty).Returns(2);
			consignmentItem1.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM1_PACKAGETYPE");
			consignmentItem1.Setup(m => m.ContainerNumber).Returns("CONSIGNMENTITEM1_CONTAINERNUMBER");
			consignmentItem1.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.Australia);
			consignmentItem1.Setup(m => m.Classifications).Returns(new List<IClassification>() { classifications1.Object, classifications2.Object });
			consignmentItem1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignmentItem1.Setup(m => m.Identifiers).Returns(new List<ICommodity>() { identifier1.Object, identifier2.Object });

			var consignmentItem2 = new Mock<ICREConsignmentItem>();
			consignmentItem2.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM2_GOODSDESCRIPTION");
			consignmentItem2.Setup(m => m.UNDGHazardousGoodsCode).Returns("CONSIGNMENTITEM2_UNDGHAZARDOUSGOODSCODE");
			consignmentItem2.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem2.Setup(m => m.Value).Returns(8.519);
			consignmentItem2.Setup(m => m.GrossWeightInKg).Returns(1.360777m);
			consignmentItem2.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM2_GOODSORIGINCOUNTRY");
			consignmentItem2.Setup(m => m.PackageQty).Returns(2);
			consignmentItem2.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM2_PACKAGETYPE");
			consignmentItem2.Setup(m => m.ContainerNumber).Returns("CONSIGNMENTITEM2_CONTAINERNUMBER");
			consignmentItem2.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.UnitedStates);
			consignmentItem2.Setup(m => m.Classifications).Returns(new List<IClassification>() { classifications1.Object, classifications2.Object });
			consignmentItem2.Setup(m => m.SequenceNumber).Returns((ZShort)2);
			consignmentItem2.Setup(m => m.Identifiers).Returns(new List<ICommodity>() { identifier1.Object, identifier2.Object });

			iOrganisation1 = new Mock<IOrganisation>();
			iOrganisation1.Setup(m => m.Name).Returns("IORGANISATION1_NAME");
			iOrganisation1.Setup(m => m.CustomsClientCode).Returns("IORGANISATION1_CUSTOMSCLIENTCODE");
			iOrganisation1.Setup(m => m.City).Returns("IORGANISATION1_CITY");
			iOrganisation1.Setup(m => m.CountryCode).Returns("IORGANISATION1_COUNTRYCODE");
			iOrganisation1.Setup(m => m.CountryRegion).Returns("IORGANISATION1_COUNTRYREGION");
			iOrganisation1.Setup(m => m.Address).Returns("IORGANISATION1_ADDRESS");
			iOrganisation1.Setup(m => m.PostCode).Returns("IORGANISATION1_POSTCODE");

			var iOrganisation2 = new Mock<IOrganisation>();
			iOrganisation2.Setup(m => m.Name).Returns("IORGANISATION2_NAME");
			iOrganisation2.Setup(m => m.CustomsClientCode).Returns("IORGANISATION2_CUSTOMSCLIENTCODE");
			iOrganisation2.Setup(m => m.City).Returns("IORGANISATION2_CITY");
			iOrganisation2.Setup(m => m.CountryCode).Returns("IORGANISATION2_COUNTRYCODE");
			iOrganisation2.Setup(m => m.CountryRegion).Returns("IORGANISATION2_COUNTRYREGION");
			iOrganisation2.Setup(m => m.Address).Returns("IORGANISATION2_ADDRESS");
			iOrganisation2.Setup(m => m.PostCode).Returns("IORGANISATION2_POSTCODE");

			iCommunication1 = new Mock<ICommunication>();
			iCommunication1.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION1_CONTACTDETAIL");
			iCommunication1.Setup(m => m.ContactType).Returns("EM");

			var iCommunication2 = new Mock<ICommunication>();
			iCommunication2.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION2_CONTACTDETAIL");
			iCommunication2.Setup(m => m.ContactType).Returns("EM");

			var iDeclarant = new Mock<IDeclarant>();
			iDeclarant.Setup(m => m.DeclarantID).Returns("DeclarantID");
			iDeclarant.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var iContact1 = new Mock<IContact>();
			iContact1.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var iContact2 = new Mock<IContact>();
			iContact2.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			iOrganisationSimple1 = new Mock<IOrganisationSimple>();
			iOrganisationSimple1.Setup(m => m.Name).Returns("IORGANISATIONSIMPLE1_NAME");
			iOrganisationSimple1.Setup(m => m.CustomsClientCode).Returns("IORGANISATIONSIMPLE1_CUSTOMSCLIENTCODE");
			iOrganisationSimple1.Setup(m => m.Contacts).Returns(new List<IContact>() { iContact1.Object, iContact2.Object });

			var iOrganisationSimple2 = new Mock<IOrganisationSimple>();
			iOrganisationSimple2.Setup(m => m.Name).Returns("IORGANISATIONSIMPLE2_NAME");
			iOrganisationSimple2.Setup(m => m.CustomsClientCode).Returns("IORGANISATIONSIMPLE2_CUSTOMSCLIENTCODE");
			iOrganisationSimple2.Setup(m => m.Contacts).Returns(new List<IContact>() { iContact1.Object, iContact2.Object });

			consignor = new Mock<IPartyInformation>();
			consignor.Setup(m => m.Name).Returns("CONSIGNOR_NAME");
			consignor.Setup(m => m.City).Returns("CONSIGNOR_CITY");
			consignor.Setup(m => m.CountryCode).Returns("CONSIGNOR_COUNTRYCODE");
			consignor.Setup(m => m.CountryRegion).Returns("CONSIGNOR_COUNTRYREGION");
			consignor.Setup(m => m.Address).Returns("CONSIGNOR_ADDRESS");
			consignor.Setup(m => m.PostCode).Returns("CONSIGNOR_POSTCODE");

			var notify1 = new Mock<IPartyInformation>();
			notify1.Setup(m => m.Name).Returns("NOTIFY1_NAME");
			notify1.Setup(m => m.City).Returns("NOTIFY1_CITY");
			notify1.Setup(m => m.CountryCode).Returns("NOTIFY1_COUNTRYCODE");
			notify1.Setup(m => m.CountryRegion).Returns("NOTIFY1_COUNTRYREGION");
			notify1.Setup(m => m.Address).Returns("NOTIFY1_ADDRESS");
			notify1.Setup(m => m.PostCode).Returns("NOTIFY1_POSTCODE");

			var notify2 = new Mock<IPartyInformation>();
			notify2.Setup(m => m.Name).Returns("NOTIFY2_NAME");
			notify2.Setup(m => m.City).Returns("NOTIFY2_CITY");
			notify2.Setup(m => m.CountryCode).Returns("NOTIFY2_COUNTRYCODE");
			notify2.Setup(m => m.CountryRegion).Returns("NOTIFY2_COUNTRYREGION");
			notify2.Setup(m => m.Address).Returns("NOTIFY2_ADDRESS");
			notify2.Setup(m => m.PostCode).Returns("NOTIFY2_POSTCODE");

			var iAssociatedTransportDocument = new Mock<IAssociatedTransportDocument>();
			iAssociatedTransportDocument.Setup(m => m.BillNumber).Returns("BillNumber");
			iAssociatedTransportDocument.Setup(m => m.BillType).Returns("BillType");

			iTransportEquipment1 = new Mock<ITransportEquipment>();
			iTransportEquipment1.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSIZE");
			iTransportEquipment1.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSTATUS");
			iTransportEquipment1.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT1_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment1.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT1_CONTAINERNUMBER");
			iTransportEquipment1.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "ITRANSPORTEQUIPMENT1_SEALNUMBER1", "ITRANSPORTEQUIPMENT1_SEALNUMBER2" });

			var iTransportEquipment2 = new Mock<ITransportEquipment>();
			iTransportEquipment2.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT2_CONTAINERSIZE");
			iTransportEquipment2.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT2_CONTAINERSTATUS");
			iTransportEquipment2.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT2_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment2.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT2_CONTAINERNUMBER");
			iTransportEquipment2.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "ITRANSPORTEQUIPMENT2_SEALNUMBER1", "ITRANSPORTEQUIPMENT2_SEALNUMBER2" });

			var consignment1 = new Mock<ICREConsignment>();
			consignment1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignment1.Setup(m => m.ConsignmentValueInNZD).Returns(5.123);
			consignment1.Setup(m => m.WriteOffRequest).Returns(true);
			consignment1.Setup(m => m.TranshipmentDetails).Returns(consignment1ITRDetails.Object);
			consignment1.Setup(m => m.HandlingInfo).Returns("CONSIGNMENT1_HANDLINGINFO");
			consignment1.Setup(m => m.PortOfDischarge).Returns("CONSIGNMENT1_PORTOFDISCHARGE");
			consignment1.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "11111A", "PYKE" });
			consignment1.Setup(m => m.Consignee).Returns(consignee.Object);
			consignment1.Setup(m => m.ConsignmentItems).Returns(new List<ICREConsignmentItem>() { consignmentItem1.Object, consignmentItem2.Object });
			consignment1.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment1.Setup(m => m.FreightPaymentMethod).Returns("CONSIGNMENT1_FREIGHTPAYMENTMETHOD");
			consignment1.Setup(m => m.GoodsLocation).Returns("CONSIGNMENT1_GOODSLOCATION");
			consignment1.Setup(m => m.PortOfLoading).Returns("CONSIGNMENT1_PORTOFLOADING");
			consignment1.Setup(m => m.DeliveryNotifyParties).Returns(new List<IOrganisationSimple>() { iOrganisationSimple1.Object, iOrganisationSimple2.Object });
			consignment1.Setup(m => m.Consignor).Returns(consignor.Object);
			consignment1.Setup(m => m.NotifyParties).Returns(new List<IPartyInformation> { notify1.Object, notify2.Object });
			consignment1.Setup(m => m.BillNumber).Returns(iAssociatedTransportDocument.Object);
			consignment1.Setup(m => m.Consolidator).Returns(iOrganisationSimple1.Object);
			consignment1.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { iTransportEquipment1.Object, iTransportEquipment2.Object });
			consignment1.Setup(m => m.HasContainers).Returns(true);

			var consignment2 = new Mock<ICREConsignment>();
			consignment2.Setup(m => m.SequenceNumber).Returns((ZShort)2);
			consignment2.Setup(m => m.ConsignmentValueInNZD).Returns(9.876);
			consignment2.Setup(m => m.WriteOffRequest).Returns(true);
			consignment2.Setup(m => m.TranshipmentDetails).Returns(consignment2ITRDetails.Object);
			consignment2.Setup(m => m.HandlingInfo).Returns("CONSIGNMENT2_HANDLINGINFO");
			consignment2.Setup(m => m.PortOfDischarge).Returns("CONSIGNMENT2_PORTOFDISCHARGE");
			consignment2.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "22222B", "TARTH" });
			consignment2.Setup(m => m.Consignee).Returns(consignee.Object);
			consignment2.Setup(m => m.ConsignmentItems).Returns(new List<ICREConsignmentItem>() { consignmentItem1.Object, consignmentItem2.Object });
			consignment2.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment2.Setup(m => m.FreightPaymentMethod).Returns("CONSIGNMENT2_FREIGHTPAYMENTMETHOD");
			consignment2.Setup(m => m.GoodsLocation).Returns("CONSIGNMENT2_GOODSLOCATION");
			consignment2.Setup(m => m.PortOfLoading).Returns("CONSIGNMENT2_PORTOFLOADING");
			consignment2.Setup(m => m.DeliveryNotifyParties).Returns(new List<IOrganisationSimple>() { iOrganisationSimple1.Object, iOrganisationSimple2.Object });
			consignment2.Setup(m => m.Consignor).Returns(consignor.Object);
			consignment2.Setup(m => m.NotifyParties).Returns(new List<IPartyInformation> { notify1.Object, notify2.Object });
			consignment2.Setup(m => m.BillNumber).Returns(iAssociatedTransportDocument.Object);
			consignment2.Setup(m => m.Consolidator).Returns(iOrganisationSimple1.Object);
			consignment2.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { iTransportEquipment1.Object, iTransportEquipment2.Object });
			consignment2.Setup(m => m.HasContainers).Returns(true);

			iCargoReportExportMock = new Mock<ICargoReportExport>();
			iCargoReportExportMock.Setup(m => m.TSWReferenceNumber).Returns("ENTRY12345");
			iCargoReportExportMock.Setup(m => m.SenderReferenceNumber).Returns("C00001165");
			iCargoReportExportMock.Setup(m => m.IsSea).Returns(true);
			iCargoReportExportMock.Setup(m => m.IsAir).Returns(false);
			iCargoReportExportMock.Setup(m => m.IsMail).Returns(false);
			iCargoReportExportMock.Setup(m => m.CraftName).Returns("CRAFTNAME");
			iCargoReportExportMock.Setup(m => m.FlightNo).Returns("FLIGHTNO");
			iCargoReportExportMock.Setup(m => m.DepartureDate).Returns(new ZDateTime(2018, 10, 22));
			iCargoReportExportMock.Setup(m => m.LloydsNo).Returns("LLOYDSNO");
			iCargoReportExportMock.Setup(m => m.VoyageNo).Returns("VOYAGENO");
			iCargoReportExportMock.Setup(m => m.HasEmptyContainersOnly).Returns(false);
			iCargoReportExportMock.Setup(m => m.PortOfDeparture).Returns("PortOfDeparture");
			iCargoReportExportMock.Setup(m => m.AdditionalInformation).Returns(additionalInformationMock.Object);
			iCargoReportExportMock.Setup(m => m.Carrier).Returns(iOrganisationSimpleMock.Object);
			iCargoReportExportMock.Setup(m => m.UseInterfaceSequenceNumber).Returns(true);

			creBuilder = new CREMessageBuilder(iCargoReportExportMock.Object, TSWTransactionTypes.Original, "00009908C");
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			iCargoReportExportMock.Setup(m => m.IsSea).Returns(false);
			iCargoReportExportMock.Setup(m => m.IsAir).Returns(true);
			iCargoReportExportMock.Setup(m => m.Declarant).Returns(iDeclarant.Object);
			iCargoReportExportMock.Setup(m => m.Consignments).Returns(new List<ICREConsignment>()
			{ consignment1.Object, consignment2.Object });
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestCREPopulateConsignmentsAir.txt")), creBuilder.GetXMLMessage());
			consignment1.Setup(m => m.DeliverToParty);
			consignment2.Setup(m => m.DeliverToParty);
			var unexpectedXML = @"<DeliveryDestination>
      <Name>IORGANISATION1_NAME</Name>
      <Address>
        <CityName>IORGANISATION1_CITY</CityName>
        <CountryCode>IORGANISATION1_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>IORGANISATION1_COUNTRYREGION</CountrySubDivisionName>
        <Line>IORGANISATION1_ADDRESS</Line>
        <PostcodeID>IORGANISATION1_POSTCODE</PostcodeID>
      </Address>
    </DeliveryDestination>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
			iOrganisation1.Setup(m => m.Name).Returns(consignee.Object.Name);
			consignment1.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment2.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
			consignment1.Setup(m => m.FreightPaymentMethod);
			consignment2.Setup(m => m.FreightPaymentMethod);
			consignment1.Setup(m => m.FreightPaymentMethod).Returns(ZString.Empty);
			consignment2.Setup(m => m.FreightPaymentMethod).Returns(ZString.Empty);
			unexpectedXML = @"<Freight>
      <PaymentMethodCode>CONSIGNMENT2_FREIGHTPAYMENTMETHOD</PaymentMethodCode>
    </Freight>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
			consignment1.Setup(m => m.GoodsLocation);
			consignment1.Setup(m => m.GoodsLocation).Returns(ZString.Empty);
			consignment2.Setup(m => m.GoodsLocation);
			consignment2.Setup(m => m.GoodsLocation).Returns(ZString.Empty);
			unexpectedXML = @"<GoodsLocation>
      <ID>CONSIGNMENT1_GOODSLOCATION</ID>
    </GoodsLocation>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentsMail()
		{
			var tswAttachment1 = new Mock<ITSWAttachment>();
			tswAttachment1.Setup(m => m.DocType).Returns("CDO");
			tswAttachment1.Setup(m => m.FileName).Returns("TEST1.PDF");

			var tswAttachment2 = new Mock<ITSWAttachment>();
			tswAttachment2.Setup(m => m.DocType).Returns("INV");
			tswAttachment2.Setup(m => m.FileName).Returns("TEST2.PDF");

			additionalInformationMock = new Mock<IAdditionalInformation>();
			additionalInformationMock.Setup(m => m.SupportingDocuments).Returns(new List<ITSWAttachment>() { tswAttachment1.Object, tswAttachment2.Object });
			additionalInformationMock.Setup(m => m.FreeText).Returns("ADDITIONALINFORMATIONMOCK_FREETEXT");
			additionalInformationMock.Setup(m => m.ManualOverrideText).Returns("ADDITIONALINFORMATIONMOCK_MANUALOVERRIDETEXT");
			additionalInformationMock.Setup(m => m.AdditionalStatementText).Returns("ADDITIONALSTATEMENTTEXT");

			iOrganisationSimpleMock = new Mock<IOrganisationSimple>();
			iOrganisationSimpleMock.Setup(m => m.Name).Returns("IORGANISATIONSIMPLEMOCK_NAME");
			iOrganisationSimpleMock.Setup(m => m.CustomsClientCode).Returns("51358595A");

			consignment1ITRDetails = new Mock<ITranshipmentDetails>();
			consignment1ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment1ITRDetails.Setup(m => m.ITRImportMode).Returns("4");
			consignment1ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("CONSIGNMENT1_ITRVOYAGEFLIGHT");
			consignment1ITRDetails.Setup(m => m.ITRArrivalDate).Returns(new ZDateTime(2018, 10, 22));
			consignment1ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("CONSIGNMENT1_MODEOFTRANSPORTFORTRANSFER");
			consignment1ITRDetails.Setup(m => m.ITRImportCraft).Returns("CONSIGNMENT1_ITRIMPORTCRAFT");

			consignment2ITRDetails = new Mock<ITranshipmentDetails>();
			consignment2ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment2ITRDetails.Setup(m => m.ITRImportMode).Returns("1");
			consignment2ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("CONSIGNMENT2_ITRVOYAGEFLIGHT");
			consignment2ITRDetails.Setup(m => m.ITRArrivalDate).Returns(new ZDateTime(2018, 10, 21));
			consignment2ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("CONSIGNMENT2_MODEOFTRANSPORTFORTRANSFER");
			consignment2ITRDetails.Setup(m => m.ITRImportCraft).Returns("CONSIGNMENT2_ITRIMPORTCRAFT");

			consignee = new Mock<IPartyInformation>();
			consignee.Setup(m => m.Name).Returns("CONSIGNEE_NAME");
			consignee.Setup(m => m.City).Returns("CONSIGNEE_CITY");
			consignee.Setup(m => m.CountryCode).Returns("CONSIGNEE_COUNTRYCODE");
			consignee.Setup(m => m.CountryRegion).Returns("CONSIGNEE_COUNTRYREGION");
			consignee.Setup(m => m.Address).Returns("CONSIGNEE_ADDRESS");
			consignee.Setup(m => m.PostCode).Returns("CONSIGNEE_POSTCODE");

			var classifications1 = new Mock<IClassification>();
			classifications1.Setup(m => m.Classification).Returns("CLASSIFICATIONS1_CLASSIFICATION");
			classifications1.Setup(m => m.ClassificationTypeCode).Returns("SSO");

			var classifications2 = new Mock<IClassification>();
			classifications2.Setup(m => m.Classification).Returns("CLASSIFICATIONS1_CLASSIFICATION");
			classifications2.Setup(m => m.ClassificationTypeCode).Returns("SSO");

			var identifier1 = new Mock<ICommodity>();
			identifier1.Setup(m => m.CommodityNumber).Returns("IDENTIFIER1_COMMODITYNUMBER");
			identifier1.Setup(m => m.CommodityType).Returns("IDENTIFIER1_COMMODITYTYPE");

			var identifier2 = new Mock<ICommodity>();
			identifier2.Setup(m => m.CommodityNumber).Returns("IDENTIFIER2_COMMODITYNUMBER");
			identifier2.Setup(m => m.CommodityType).Returns("IDENTIFIER2_COMMODITYTYPE");

			consignmentItem1 = new Mock<ICREConsignmentItem>();
			consignmentItem1.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM1_GOODSDESCRIPTION");
			consignmentItem1.Setup(m => m.UNDGHazardousGoodsCode).Returns("CONSIGNMENTITEM1_UNDGHAZARDOUSGOODSCODE");
			consignmentItem1.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem1.Setup(m => m.Value).Returns(9.673);
			consignmentItem1.Setup(m => m.GrossWeightInKg).Returns(1.360777m);
			consignmentItem1.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM1_GOODSORIGINCOUNTRY");
			consignmentItem1.Setup(m => m.PackageQty).Returns(2);
			consignmentItem1.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM1_PACKAGETYPE");
			consignmentItem1.Setup(m => m.ContainerNumber).Returns("CONSIGNMENTITEM1_CONTAINERNUMBER");
			consignmentItem1.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.Australia);
			consignmentItem1.Setup(m => m.Classifications).Returns(new List<IClassification>() { classifications1.Object, classifications2.Object });
			consignmentItem1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignmentItem1.Setup(m => m.Identifiers).Returns(new List<ICommodity>() { identifier1.Object, identifier2.Object });

			var consignmentItem2 = new Mock<ICREConsignmentItem>();
			consignmentItem2.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM2_GOODSDESCRIPTION");
			consignmentItem2.Setup(m => m.UNDGHazardousGoodsCode).Returns("CONSIGNMENTITEM2_UNDGHAZARDOUSGOODSCODE");
			consignmentItem2.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem2.Setup(m => m.Value).Returns(8.519);
			consignmentItem2.Setup(m => m.GrossWeightInKg).Returns(1.360777m);
			consignmentItem2.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM2_GOODSORIGINCOUNTRY");
			consignmentItem2.Setup(m => m.PackageQty).Returns(2);
			consignmentItem2.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM2_PACKAGETYPE");
			consignmentItem2.Setup(m => m.ContainerNumber).Returns("CONSIGNMENTITEM2_CONTAINERNUMBER");
			consignmentItem2.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.UnitedStates);
			consignmentItem2.Setup(m => m.Classifications).Returns(new List<IClassification>() { classifications1.Object, classifications2.Object });
			consignmentItem2.Setup(m => m.SequenceNumber).Returns((ZShort)2);
			consignmentItem2.Setup(m => m.Identifiers).Returns(new List<ICommodity>() { identifier1.Object, identifier2.Object });

			iOrganisation1 = new Mock<IOrganisation>();
			iOrganisation1.Setup(m => m.Name).Returns("IORGANISATION1_NAME");
			iOrganisation1.Setup(m => m.CustomsClientCode).Returns("IORGANISATION1_CUSTOMSCLIENTCODE");
			iOrganisation1.Setup(m => m.City).Returns("IORGANISATION1_CITY");
			iOrganisation1.Setup(m => m.CountryCode).Returns("IORGANISATION1_COUNTRYCODE");
			iOrganisation1.Setup(m => m.CountryRegion).Returns("IORGANISATION1_COUNTRYREGION");
			iOrganisation1.Setup(m => m.Address).Returns("IORGANISATION1_ADDRESS");
			iOrganisation1.Setup(m => m.PostCode).Returns("IORGANISATION1_POSTCODE");

			var iOrganisation2 = new Mock<IOrganisation>();
			iOrganisation2.Setup(m => m.Name).Returns("IORGANISATION2_NAME");
			iOrganisation2.Setup(m => m.CustomsClientCode).Returns("IORGANISATION2_CUSTOMSCLIENTCODE");
			iOrganisation2.Setup(m => m.City).Returns("IORGANISATION2_CITY");
			iOrganisation2.Setup(m => m.CountryCode).Returns("IORGANISATION2_COUNTRYCODE");
			iOrganisation2.Setup(m => m.CountryRegion).Returns("IORGANISATION2_COUNTRYREGION");
			iOrganisation2.Setup(m => m.Address).Returns("IORGANISATION2_ADDRESS");
			iOrganisation2.Setup(m => m.PostCode).Returns("IORGANISATION2_POSTCODE");

			iCommunication1 = new Mock<ICommunication>();
			iCommunication1.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION1_CONTACTDETAIL");
			iCommunication1.Setup(m => m.ContactType).Returns("EM");

			var iCommunication2 = new Mock<ICommunication>();
			iCommunication2.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION2_CONTACTDETAIL");
			iCommunication2.Setup(m => m.ContactType).Returns("EM");

			var iDeclarant = new Mock<IDeclarant>();
			iDeclarant.Setup(m => m.DeclarantID).Returns("DeclarantID");
			iDeclarant.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var iContact1 = new Mock<IContact>();
			iContact1.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var iContact2 = new Mock<IContact>();
			iContact2.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			iOrganisationSimple1 = new Mock<IOrganisationSimple>();
			iOrganisationSimple1.Setup(m => m.Name).Returns("IORGANISATIONSIMPLE1_NAME");
			iOrganisationSimple1.Setup(m => m.CustomsClientCode).Returns("IORGANISATIONSIMPLE1_CUSTOMSCLIENTCODE");
			iOrganisationSimple1.Setup(m => m.Contacts).Returns(new List<IContact>() { iContact1.Object, iContact2.Object });

			var iOrganisationSimple2 = new Mock<IOrganisationSimple>();
			iOrganisationSimple2.Setup(m => m.Name).Returns("IORGANISATIONSIMPLE2_NAME");
			iOrganisationSimple2.Setup(m => m.CustomsClientCode).Returns("IORGANISATIONSIMPLE2_CUSTOMSCLIENTCODE");
			iOrganisationSimple2.Setup(m => m.Contacts).Returns(new List<IContact>() { iContact1.Object, iContact2.Object });

			consignor = new Mock<IPartyInformation>();
			consignor.Setup(m => m.Name).Returns("CONSIGNOR_NAME");
			consignor.Setup(m => m.City).Returns("CONSIGNOR_CITY");
			consignor.Setup(m => m.CountryCode).Returns("CONSIGNOR_COUNTRYCODE");
			consignor.Setup(m => m.CountryRegion).Returns("CONSIGNOR_COUNTRYREGION");
			consignor.Setup(m => m.Address).Returns("CONSIGNOR_ADDRESS");
			consignor.Setup(m => m.PostCode).Returns("CONSIGNOR_POSTCODE");

			var notify1 = new Mock<IPartyInformation>();
			notify1.Setup(m => m.Name).Returns("NOTIFY1_NAME");
			notify1.Setup(m => m.City).Returns("NOTIFY1_CITY");
			notify1.Setup(m => m.CountryCode).Returns("NOTIFY1_COUNTRYCODE");
			notify1.Setup(m => m.CountryRegion).Returns("NOTIFY1_COUNTRYREGION");
			notify1.Setup(m => m.Address).Returns("NOTIFY1_ADDRESS");
			notify1.Setup(m => m.PostCode).Returns("NOTIFY1_POSTCODE");

			var notify2 = new Mock<IPartyInformation>();
			notify2.Setup(m => m.Name).Returns("NOTIFY2_NAME");
			notify2.Setup(m => m.City).Returns("NOTIFY2_CITY");
			notify2.Setup(m => m.CountryCode).Returns("NOTIFY2_COUNTRYCODE");
			notify2.Setup(m => m.CountryRegion).Returns("NOTIFY2_COUNTRYREGION");
			notify2.Setup(m => m.Address).Returns("NOTIFY2_ADDRESS");
			notify2.Setup(m => m.PostCode).Returns("NOTIFY2_POSTCODE");

			var iAssociatedTransportDocument = new Mock<IAssociatedTransportDocument>();
			iAssociatedTransportDocument.Setup(m => m.BillNumber).Returns("BillNumber");
			iAssociatedTransportDocument.Setup(m => m.BillType).Returns("BillType");

			iTransportEquipment1 = new Mock<ITransportEquipment>();
			iTransportEquipment1.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSIZE");
			iTransportEquipment1.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSTATUS");
			iTransportEquipment1.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT1_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment1.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT1_CONTAINERNUMBER");
			iTransportEquipment1.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "ITRANSPORTEQUIPMENT1_SEALNUMBER1", "ITRANSPORTEQUIPMENT1_SEALNUMBER2" });

			var iTransportEquipment2 = new Mock<ITransportEquipment>();
			iTransportEquipment2.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT2_CONTAINERSIZE");
			iTransportEquipment2.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT2_CONTAINERSTATUS");
			iTransportEquipment2.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT2_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment2.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT2_CONTAINERNUMBER");
			iTransportEquipment2.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "ITRANSPORTEQUIPMENT2_SEALNUMBER1", "ITRANSPORTEQUIPMENT2_SEALNUMBER2" });

			var consignment1 = new Mock<ICREConsignment>();
			consignment1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignment1.Setup(m => m.ConsignmentValueInNZD).Returns(5.123);
			consignment1.Setup(m => m.WriteOffRequest).Returns(true);
			consignment1.Setup(m => m.TranshipmentDetails).Returns(consignment1ITRDetails.Object);
			consignment1.Setup(m => m.HandlingInfo).Returns("CONSIGNMENT1_HANDLINGINFO");
			consignment1.Setup(m => m.PortOfDischarge).Returns("CONSIGNMENT1_PORTOFDISCHARGE");
			consignment1.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "11111A", "PYKE" });
			consignment1.Setup(m => m.ConsignmentItems).Returns(new List<ICREConsignmentItem>() { consignmentItem1.Object, consignmentItem2.Object });
			consignment1.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment1.Setup(m => m.FreightPaymentMethod).Returns("CONSIGNMENT1_FREIGHTPAYMENTMETHOD");
			consignment1.Setup(m => m.GoodsLocation).Returns("CONSIGNMENT1_GOODSLOCATION");
			consignment1.Setup(m => m.PortOfLoading).Returns("CONSIGNMENT1_PORTOFLOADING");
			consignment1.Setup(m => m.DeliveryNotifyParties).Returns(new List<IOrganisationSimple>() { iOrganisationSimple1.Object, iOrganisationSimple2.Object });
			consignment1.Setup(m => m.Consignor).Returns(consignor.Object);
			consignment1.Setup(m => m.NotifyParties).Returns(new List<IPartyInformation> { notify1.Object, notify2.Object });
			consignment1.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { iTransportEquipment1.Object, iTransportEquipment2.Object });
			consignment1.Setup(m => m.HasContainers).Returns(true);
			consignment1.Setup(m => m.BillNumber).Returns(iAssociatedTransportDocument.Object);
			consignment1.Setup(m => m.Consolidator).Returns(iOrganisationSimple1.Object);
			consignment1.Setup(m => m.Consignee).Returns(consignee.Object);

			var consignment2 = new Mock<ICREConsignment>();
			consignment2.Setup(m => m.SequenceNumber).Returns((ZShort)2);
			consignment2.Setup(m => m.ConsignmentValueInNZD).Returns(9.876);
			consignment2.Setup(m => m.WriteOffRequest).Returns(true);
			consignment2.Setup(m => m.TranshipmentDetails).Returns(consignment2ITRDetails.Object);
			consignment2.Setup(m => m.HandlingInfo).Returns("CONSIGNMENT2_HANDLINGINFO");
			consignment2.Setup(m => m.PortOfDischarge).Returns("CONSIGNMENT2_PORTOFDISCHARGE");
			consignment2.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "22222B", "TARTH" });
			consignment2.Setup(m => m.Consignee).Returns(consignee.Object);
			consignment2.Setup(m => m.ConsignmentItems).Returns(new List<ICREConsignmentItem>() { consignmentItem1.Object, consignmentItem2.Object });
			consignment2.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment2.Setup(m => m.FreightPaymentMethod).Returns("CONSIGNMENT2_FREIGHTPAYMENTMETHOD");
			consignment2.Setup(m => m.GoodsLocation).Returns("CONSIGNMENT2_GOODSLOCATION");
			consignment2.Setup(m => m.PortOfLoading).Returns("CONSIGNMENT2_PORTOFLOADING");
			consignment2.Setup(m => m.DeliveryNotifyParties).Returns(new List<IOrganisationSimple>() { iOrganisationSimple1.Object, iOrganisationSimple2.Object });
			consignment2.Setup(m => m.Consignor).Returns(consignor.Object);
			consignment2.Setup(m => m.NotifyParties).Returns(new List<IPartyInformation> { notify1.Object, notify2.Object });
			consignment2.Setup(m => m.BillNumber).Returns(iAssociatedTransportDocument.Object);
			consignment2.Setup(m => m.Consolidator).Returns(iOrganisationSimple1.Object);
			consignment2.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { iTransportEquipment1.Object, iTransportEquipment2.Object });
			consignment2.Setup(m => m.HasContainers).Returns(true);

			iCargoReportExportMock = new Mock<ICargoReportExport>();
			iCargoReportExportMock.Setup(m => m.TSWReferenceNumber).Returns("ENTRY12345");
			iCargoReportExportMock.Setup(m => m.SenderReferenceNumber).Returns("C00001165");
			iCargoReportExportMock.Setup(m => m.IsSea).Returns(true);
			iCargoReportExportMock.Setup(m => m.IsAir).Returns(false);
			iCargoReportExportMock.Setup(m => m.IsMail).Returns(false);
			iCargoReportExportMock.Setup(m => m.CraftName).Returns("CRAFTNAME");
			iCargoReportExportMock.Setup(m => m.FlightNo).Returns("FLIGHTNO");
			iCargoReportExportMock.Setup(m => m.DepartureDate).Returns(new ZDateTime(2018, 10, 22));
			iCargoReportExportMock.Setup(m => m.LloydsNo).Returns("LLOYDSNO");
			iCargoReportExportMock.Setup(m => m.VoyageNo).Returns("VOYAGENO");
			iCargoReportExportMock.Setup(m => m.HasEmptyContainersOnly).Returns(false);
			iCargoReportExportMock.Setup(m => m.PortOfDeparture).Returns("PortOfDeparture");
			iCargoReportExportMock.Setup(m => m.AdditionalInformation).Returns(additionalInformationMock.Object);
			iCargoReportExportMock.Setup(m => m.Carrier).Returns(iOrganisationSimpleMock.Object);
			iCargoReportExportMock.Setup(m => m.UseInterfaceSequenceNumber).Returns(true);
			iCargoReportExportMock.Setup(m => m.Consignments).Returns(new List<ICREConsignment>() { consignment1.Object, consignment2.Object });

			creBuilder = new CREMessageBuilder(iCargoReportExportMock.Object, TSWTransactionTypes.Original, "00009908C");
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			iCargoReportExportMock.Setup(m => m.IsSea).Returns(false);
			iCargoReportExportMock.Setup(m => m.IsMail).Returns(true);
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestCREPopulateConsignmentsMail.txt")), creBuilder.GetXMLMessage());
			consignment1.Setup(m => m.DeliverToParty);
			consignment2.Setup(m => m.DeliverToParty);
			var unexpectedXML = @"<DeliveryDestination>
      <Name>IORGANISATION1_NAME</Name>
      <Address>
        <CityName>IORGANISATION1_CITY</CityName>
        <CountryCode>IORGANISATION1_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>IORGANISATION1_COUNTRYREGION</CountrySubDivisionName>
        <Line>IORGANISATION1_ADDRESS</Line>
        <PostcodeID>IORGANISATION1_POSTCODE</PostcodeID>
      </Address>
    </DeliveryDestination>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
			iOrganisation1.Setup(m => m.Name).Returns(consignee.Object.Name);
			consignment1.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment2.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
			consignment1.Setup(m => m.FreightPaymentMethod);
			consignment2.Setup(m => m.FreightPaymentMethod);
			consignment1.Setup(m => m.FreightPaymentMethod).Returns(ZString.Empty);
			consignment2.Setup(m => m.FreightPaymentMethod).Returns(ZString.Empty);
			unexpectedXML = @"<Freight>
      <PaymentMethodCode>CONSIGNMENT2_FREIGHTPAYMENTMETHOD</PaymentMethodCode>
    </Freight>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
			consignment1.Setup(m => m.GoodsLocation);
			consignment1.Setup(m => m.GoodsLocation).Returns(ZString.Empty);
			consignment2.Setup(m => m.GoodsLocation);
			consignment2.Setup(m => m.GoodsLocation).Returns(ZString.Empty);
			unexpectedXML = @"<GoodsLocation>
      <ID>CONSIGNMENT1_GOODSLOCATION</ID>
    </GoodsLocation>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalInformation()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
      <StatementDescription>CONSIGNMENT1_ITRVOYAGEFLIGHT,4,20181022,</StatementDescription>
      <StatementTypeCode>ITR</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementCode>CONSIGNMENT1_MODEOFTRANSPORTFORTRANSFER</StatementCode>
      <StatementTypeCode>MTT</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementDescription>CONSIGNMENT1_HANDLINGINFO</StatementDescription>
      <StatementTypeCode>HAN</StatementTypeCode>
    </AdditionalInformation>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
			consignment1.Setup(m => m.WriteOffRequest).Returns(false);
			consignment2.Setup(m => m.WriteOffRequest).Returns(false);
			expectedXML = @"<AdditionalInformation>
      <StatementCode>Y</StatementCode>
      <StatementTypeCode>WOF</StatementTypeCode>
    </AdditionalInformation>";
			AssertNotContains(expectedXML, creBuilder.GetXMLMessage());
			consignment1.Setup(m => m.WriteOffRequest).Returns(true);
			consignment2.Setup(m => m.WriteOffRequest).Returns(true);
			iCargoReportExportMock.Setup(m => m.HasEmptyContainersOnly).Returns(true);
			AssertNotContains(expectedXML, creBuilder.GetXMLMessage());
			consignment1ITRDetails = new Mock<ITranshipmentDetails>();
			consignment1ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment1ITRDetails.Setup(m => m.ITRImportMode).Returns("1");
			consignment1ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("CONSIGNMENT1_ITRVOYAGEFLIGHT");
			consignment1ITRDetails.Setup(m => m.ITRArrivalDate).Returns(new ZDateTime(2018, 10, 22));
			consignment1ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("CONSIGNMENT1_MODEOFTRANSPORTFORTRANSFER");
			consignment1ITRDetails.Setup(m => m.ITRImportCraft).Returns("CONSIGNMENT1_ITRIMPORTCRAFT");
			consignment1.Setup(m => m.TranshipmentDetails).Returns(consignment1ITRDetails.Object);
			AssertContains("<StatementDescription>CONSIGNMENT1_ITRIMPORTCRAFT,1,20181022,CONSIGNMENT1_ITRVOYAGEFLIGHT</StatementDescription>", creBuilder.GetXMLMessage());
			expectedXML = @"<AdditionalInformation>
      <StatementDescription>CONSIGNMENT1_ITRVOYAGEFLIGHT,4,20181022</StatementDescription>
      <StatementTypeCode>ITR</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementCode>CONSIGNMENT1_MODEOFTRANSPORTFORTRANSFER</StatementCode>
      <StatementTypeCode>MTT</StatementTypeCode>
    </AdditionalInformation>";
			consignment1ITRDetails = new Mock<ITranshipmentDetails>();
			consignment1ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(false);
			consignment1ITRDetails.Setup(m => m.ITRImportMode).Returns("");
			consignment1ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("");
			consignment1ITRDetails.Setup(m => m.ITRArrivalDate).Returns(ZDateTime.Empty);
			consignment1ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("");
			consignment1ITRDetails.Setup(m => m.ITRImportCraft).Returns("");
			consignment1.Setup(m => m.TranshipmentDetails).Returns(consignment1ITRDetails.Object);
			AssertNotContains(expectedXML, creBuilder.GetXMLMessage());
			expectedXML = @"<AdditionalInformation>
      <StatementDescription>CONSIGNMENT1_HANDLINGINFO</StatementDescription>
      <StatementTypeCode>HAN</StatementTypeCode>
    </AdditionalInformation>";
			consignment1.Setup(m => m.HandlingInfo).Returns(ZString.Empty);
			AssertNotContains(expectedXML, creBuilder.GetXMLMessage());
			iCargoReportExportMock.Setup(m => m.HasEmptyContainersOnly).Returns(false);
			consignment1.Setup(m => m.WriteOffRequest).Returns(true);
			expectedXML = @"<AdditionalInformation>
      <StatementCode>Y</StatementCode>
      <StatementTypeCode>WOF</StatementTypeCode>
    </AdditionalInformation>";
			AssertContains("WOF element should be generated when no ITR details exist and not an empty container declaration", expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalInformation_NotSAC()
		{
			SetUpMocks();
			var expectedXML = @"<Consignment>
    <SequenceNumeric>1</SequenceNumeric>
    <ValueAmount currencyID=""NZD"">5.12</ValueAmount>
    <AdditionalInformation>
      <StatementDescription>CONSIGNMENT1_HANDLINGINFO</StatementDescription>
      <StatementTypeCode>HAN</StatementTypeCode>
    </AdditionalInformation>
    <Consignee>";
			consignment1ITRDetails = new Mock<ITranshipmentDetails>();
			consignment1ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(false);
			consignment1ITRDetails.Setup(m => m.ITRImportMode).Returns("");
			consignment1ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("");
			consignment1ITRDetails.Setup(m => m.ITRArrivalDate).Returns(ZDateTime.Empty);
			consignment1ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("");
			consignment1.Setup(m => m.TranshipmentDetails).Returns(consignment1ITRDetails.Object);
			consignment1.Setup(m => m.WriteOffRequest).Returns(false);
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignee()
		{
			SetUpMocks();
			var expectedXML = @"<Consignee>
      <Name>CONSIGNEE_NAME</Name>
      <Address>
        <CityName>CONSIGNEE_CITY</CityName>
        <CountryCode>CONSIGNEE_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>CONSIGNEE_COUNTRYREGION</CountrySubDivisionName>
        <Line>CONSIGNEE_ADDRESS</Line>
        <PostcodeID>CONSIGNEE_POSTCODE</PostcodeID>
      </Address>
    </Consignee>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
			consignee.Setup(m => m.Name).Returns("CONSIGNEE_NAME".PadLeft(71, '-'));
			consignee.Setup(m => m.City).Returns("CONSIGNEE_CITY".PadLeft(36, '-'));
			expectedXML = @"<Consignee>
      <Name>---------------------------------------------------------CONSIGNEE_NAM</Name>
      <Address>
        <CityName>----------------------CONSIGNEE_CIT</CityName>
        <CountryCode>CONSIGNEE_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>CONSIGNEE_COUNTRYREGION</CountrySubDivisionName>
        <Line>CONSIGNEE_ADDRESS</Line>
        <PostcodeID>CONSIGNEE_POSTCODE</PostcodeID>
      </Address>
    </Consignee>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignor()
		{
			SetUpMocks();
			var expectedXML = @"<Consignor>
      <Name>CONSIGNOR_NAME</Name>
      <Address>
        <CityName>CONSIGNOR_CITY</CityName>
        <CountryCode>CONSIGNOR_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>CONSIGNOR_COUNTRYREGION</CountrySubDivisionName>
        <Line>CONSIGNOR_ADDRESS</Line>
        <PostcodeID>CONSIGNOR_POSTCODE</PostcodeID>
      </Address>
    </Consignor>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
			consignor.Setup(m => m.Name).Returns("CONSIGNOR_NAME".PadLeft(71, '-'));
			consignor.Setup(m => m.City).Returns("CONSIGNOR_CITY".PadLeft(36, '-'));
			expectedXML = @"<Consignor>
      <Name>---------------------------------------------------------CONSIGNOR_NAM</Name>
      <Address>
        <CityName>----------------------CONSIGNOR_CIT</CityName>
        <CountryCode>CONSIGNOR_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>CONSIGNOR_COUNTRYREGION</CountrySubDivisionName>
        <Line>CONSIGNOR_ADDRESS</Line>
        <PostcodeID>CONSIGNOR_POSTCODE</PostcodeID>
      </Address>
    </Consignor>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateDeliveryDestination()
		{
			SetUpMocks();
			var expectedXML = @"<DeliveryDestination>
      <Name>IORGANISATION1_NAME</Name>
      <Address>
        <CityName>IORGANISATION1_CITY</CityName>
        <CountryCode>IORGANISATION1_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>IORGANISATION1_COUNTRYREGION</CountrySubDivisionName>
        <Line>IORGANISATION1_ADDRESS</Line>
        <PostcodeID>IORGANISATION1_POSTCODE</PostcodeID>
      </Address>
    </DeliveryDestination>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateFreightPayment()
		{
			SetUpMocks();
			var expectedXML = @"<Freight>
      <PaymentMethodCode>CONSIGNMENT2_FREIGHTPAYMENTMETHOD</PaymentMethodCode>
    </Freight>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateGoodsLocation()
		{
			SetUpMocks();
			var expectedXML = @"<GoodsLocation>
      <ID>CONSIGNMENT1_GOODSLOCATION</ID>
    </GoodsLocation>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateLoadingLocation()
		{
			SetUpMocks();
			var expectedXML = @"<LoadingLocation>
      <ID>CONSIGNMENT2_PORTOFLOADING</ID>
    </LoadingLocation>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateNotifyParties()
		{
			SetUpMocks();
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestCREPopulateNotifyParties.txt")), creBuilder.GetXMLMessage());

			var unexpectedXML = @"<NotifyParty>
      <Name>IORGANISATION1_NAME</Name>
      <RoleCode>NI</RoleCode>
      <Address>
        <CityName>IORGANISATION1_CITY</CityName>
        <CountryCode>IORGANISATION1_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>IORGANISATION1_COUNTRYREGION</CountrySubDivisionName>
        <Line>IORGANISATION1_ADDRESS</Line>
        <PostcodeID>IORGANISATION1_POSTCODE</PostcodeID>
      </Address>
    </NotifyParty>";
			consignment1.Setup(m => m.NotifyParties).Returns(Enumerable.Empty<IPartyInformation>());
			consignment2.Setup(m => m.NotifyParties).Returns(Enumerable.Empty<IPartyInformation>());
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
			iOrganisationSimple1.Setup(m => m.CustomsClientCode).Returns(ZString.Empty);
			AssertNotContains("<ID>IORGANISATIONSIMPLE1_CUSTOMSCLIENTCODE</ID>", creBuilder.GetXMLMessage());
			iCommunication1.Setup(m => m.ContactType).Returns("AA");
			unexpectedXML = @"<Communication>
        <ID>ICOMMUNICATION1_CONTACTDETAIL</ID>
        <TypeID>EM</TypeID>
      </Communication>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
			unexpectedXML = @"<NotifyParty>
      <Name>IORGANISATIONSIMPLE1_NAME</Name>
      <ID>IORGANISATIONSIMPLE1_CUSTOMSCLIENTCODE</ID>
      <RoleCode>N2</RoleCode>
      <Communication>
        <ID>ICOMMUNICATION1_CONTACTDETAIL</ID>
        <TypeID>EM</TypeID>
      </Communication>
      <Communication>
        <ID>ICOMMUNICATION1_CONTACTDETAIL</ID>
        <TypeID>EM</TypeID>
      </Communication>
    </NotifyParty>
    <NotifyParty>
      <Name>IORGANISATIONSIMPLE2_NAME</Name>
      <ID>IORGANISATIONSIMPLE2_CUSTOMSCLIENTCODE</ID>
      <RoleCode>N2</RoleCode>
      <Communication>
        <ID>ICOMMUNICATION1_CONTACTDETAIL</ID>
        <TypeID>EM</TypeID>
      </Communication>
      <Communication>
        <ID>ICOMMUNICATION1_CONTACTDETAIL</ID>
        <TypeID>EM</TypeID>
      </Communication>
    </NotifyParty>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateTransportContractDocument()
		{
			var tswAttachment1 = new Mock<ITSWAttachment>();
			tswAttachment1.Setup(m => m.DocType).Returns("CDO");
			tswAttachment1.Setup(m => m.FileName).Returns("TEST1.PDF");

			var tswAttachment2 = new Mock<ITSWAttachment>();
			tswAttachment2.Setup(m => m.DocType).Returns("INV");
			tswAttachment2.Setup(m => m.FileName).Returns("TEST2.PDF");

			additionalInformationMock = new Mock<IAdditionalInformation>();
			additionalInformationMock.Setup(m => m.SupportingDocuments).Returns(new List<ITSWAttachment>() { tswAttachment1.Object, tswAttachment2.Object });
			additionalInformationMock.Setup(m => m.FreeText).Returns("ADDITIONALINFORMATIONMOCK_FREETEXT");
			additionalInformationMock.Setup(m => m.ManualOverrideText).Returns("ADDITIONALINFORMATIONMOCK_MANUALOVERRIDETEXT");
			additionalInformationMock.Setup(m => m.AdditionalStatementText).Returns("ADDITIONALSTATEMENTTEXT");

			iOrganisationSimpleMock = new Mock<IOrganisationSimple>();
			iOrganisationSimpleMock.Setup(m => m.Name).Returns("IORGANISATIONSIMPLEMOCK_NAME");
			iOrganisationSimpleMock.Setup(m => m.CustomsClientCode).Returns("51358595A");

			consignment1ITRDetails = new Mock<ITranshipmentDetails>();
			consignment1ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment1ITRDetails.Setup(m => m.ITRImportMode).Returns("4");
			consignment1ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("CONSIGNMENT1_ITRVOYAGEFLIGHT");
			consignment1ITRDetails.Setup(m => m.ITRArrivalDate).Returns(new ZDateTime(2018, 10, 22));
			consignment1ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("CONSIGNMENT1_MODEOFTRANSPORTFORTRANSFER");
			consignment1ITRDetails.Setup(m => m.ITRImportCraft).Returns("CONSIGNMENT1_ITRIMPORTCRAFT");

			consignment2ITRDetails = new Mock<ITranshipmentDetails>();
			consignment2ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment2ITRDetails.Setup(m => m.ITRImportMode).Returns("1");
			consignment2ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("CONSIGNMENT2_ITRVOYAGEFLIGHT");
			consignment2ITRDetails.Setup(m => m.ITRArrivalDate).Returns(new ZDateTime(2018, 10, 21));
			consignment2ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("CONSIGNMENT2_MODEOFTRANSPORTFORTRANSFER");
			consignment2ITRDetails.Setup(m => m.ITRImportCraft).Returns("CONSIGNMENT2_ITRIMPORTCRAFT");

			consignee = new Mock<IPartyInformation>();
			consignee.Setup(m => m.Name).Returns("CONSIGNEE_NAME");
			consignee.Setup(m => m.City).Returns("CONSIGNEE_CITY");
			consignee.Setup(m => m.CountryCode).Returns("CONSIGNEE_COUNTRYCODE");
			consignee.Setup(m => m.CountryRegion).Returns("CONSIGNEE_COUNTRYREGION");
			consignee.Setup(m => m.Address).Returns("CONSIGNEE_ADDRESS");
			consignee.Setup(m => m.PostCode).Returns("CONSIGNEE_POSTCODE");

			var classifications1 = new Mock<IClassification>();
			classifications1.Setup(m => m.Classification).Returns("CLASSIFICATIONS1_CLASSIFICATION");
			classifications1.Setup(m => m.ClassificationTypeCode).Returns("SSO");

			var classifications2 = new Mock<IClassification>();
			classifications2.Setup(m => m.Classification).Returns("CLASSIFICATIONS1_CLASSIFICATION");
			classifications2.Setup(m => m.ClassificationTypeCode).Returns("SSO");

			var identifier1 = new Mock<ICommodity>();
			identifier1.Setup(m => m.CommodityNumber).Returns("IDENTIFIER1_COMMODITYNUMBER");
			identifier1.Setup(m => m.CommodityType).Returns("IDENTIFIER1_COMMODITYTYPE");

			var identifier2 = new Mock<ICommodity>();
			identifier2.Setup(m => m.CommodityNumber).Returns("IDENTIFIER2_COMMODITYNUMBER");
			identifier2.Setup(m => m.CommodityType).Returns("IDENTIFIER2_COMMODITYTYPE");

			consignmentItem1 = new Mock<ICREConsignmentItem>();
			consignmentItem1.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM1_GOODSDESCRIPTION");
			consignmentItem1.Setup(m => m.UNDGHazardousGoodsCode).Returns("CONSIGNMENTITEM1_UNDGHAZARDOUSGOODSCODE");
			consignmentItem1.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem1.Setup(m => m.Value).Returns(9.673);
			consignmentItem1.Setup(m => m.GrossWeightInKg).Returns(1.360777m);
			consignmentItem1.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM1_GOODSORIGINCOUNTRY");
			consignmentItem1.Setup(m => m.PackageQty).Returns(2);
			consignmentItem1.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM1_PACKAGETYPE");
			consignmentItem1.Setup(m => m.ContainerNumber).Returns("CONSIGNMENTITEM1_CONTAINERNUMBER");
			consignmentItem1.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.Australia);
			consignmentItem1.Setup(m => m.Classifications).Returns(new List<IClassification>() { classifications1.Object, classifications2.Object });
			consignmentItem1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignmentItem1.Setup(m => m.Identifiers).Returns(new List<ICommodity>() { identifier1.Object, identifier2.Object });

			var consignmentItem2 = new Mock<ICREConsignmentItem>();
			consignmentItem2.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM2_GOODSDESCRIPTION");
			consignmentItem2.Setup(m => m.UNDGHazardousGoodsCode).Returns("CONSIGNMENTITEM2_UNDGHAZARDOUSGOODSCODE");
			consignmentItem2.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem2.Setup(m => m.Value).Returns(8.519);
			consignmentItem2.Setup(m => m.GrossWeightInKg).Returns(1.360777m);
			consignmentItem2.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM2_GOODSORIGINCOUNTRY");
			consignmentItem2.Setup(m => m.PackageQty).Returns(2);
			consignmentItem2.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM2_PACKAGETYPE");
			consignmentItem2.Setup(m => m.ContainerNumber).Returns("CONSIGNMENTITEM2_CONTAINERNUMBER");
			consignmentItem2.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.UnitedStates);
			consignmentItem2.Setup(m => m.Classifications).Returns(new List<IClassification>() { classifications1.Object, classifications2.Object });
			consignmentItem2.Setup(m => m.SequenceNumber).Returns((ZShort)2);
			consignmentItem2.Setup(m => m.Identifiers).Returns(new List<ICommodity>() { identifier1.Object, identifier2.Object });

			iOrganisation1 = new Mock<IOrganisation>();
			iOrganisation1.Setup(m => m.Name).Returns("IORGANISATION1_NAME");
			iOrganisation1.Setup(m => m.CustomsClientCode).Returns("IORGANISATION1_CUSTOMSCLIENTCODE");
			iOrganisation1.Setup(m => m.City).Returns("IORGANISATION1_CITY");
			iOrganisation1.Setup(m => m.CountryCode).Returns("IORGANISATION1_COUNTRYCODE");
			iOrganisation1.Setup(m => m.CountryRegion).Returns("IORGANISATION1_COUNTRYREGION");
			iOrganisation1.Setup(m => m.Address).Returns("IORGANISATION1_ADDRESS");
			iOrganisation1.Setup(m => m.PostCode).Returns("IORGANISATION1_POSTCODE");

			var iOrganisation2 = new Mock<IOrganisation>();
			iOrganisation2.Setup(m => m.Name).Returns("IORGANISATION2_NAME");
			iOrganisation2.Setup(m => m.CustomsClientCode).Returns("IORGANISATION2_CUSTOMSCLIENTCODE");
			iOrganisation2.Setup(m => m.City).Returns("IORGANISATION2_CITY");
			iOrganisation2.Setup(m => m.CountryCode).Returns("IORGANISATION2_COUNTRYCODE");
			iOrganisation2.Setup(m => m.CountryRegion).Returns("IORGANISATION2_COUNTRYREGION");
			iOrganisation2.Setup(m => m.Address).Returns("IORGANISATION2_ADDRESS");
			iOrganisation2.Setup(m => m.PostCode).Returns("IORGANISATION2_POSTCODE");

			iCommunication1 = new Mock<ICommunication>();
			iCommunication1.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION1_CONTACTDETAIL");
			iCommunication1.Setup(m => m.ContactType).Returns("EM");

			var iCommunication2 = new Mock<ICommunication>();
			iCommunication2.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION2_CONTACTDETAIL");
			iCommunication2.Setup(m => m.ContactType).Returns("EM");

			var iDeclarant = new Mock<IDeclarant>();
			iDeclarant.Setup(m => m.DeclarantID).Returns("DeclarantID");
			iDeclarant.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var iContact1 = new Mock<IContact>();
			iContact1.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var iContact2 = new Mock<IContact>();
			iContact2.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			iOrganisationSimple1 = new Mock<IOrganisationSimple>();
			iOrganisationSimple1.Setup(m => m.Name).Returns("IORGANISATIONSIMPLE1_NAME");
			iOrganisationSimple1.Setup(m => m.CustomsClientCode).Returns("IORGANISATIONSIMPLE1_CUSTOMSCLIENTCODE");
			iOrganisationSimple1.Setup(m => m.Contacts).Returns(new List<IContact>() { iContact1.Object, iContact2.Object });

			var iOrganisationSimple2 = new Mock<IOrganisationSimple>();
			iOrganisationSimple2.Setup(m => m.Name).Returns("IORGANISATIONSIMPLE2_NAME");
			iOrganisationSimple2.Setup(m => m.CustomsClientCode).Returns("IORGANISATIONSIMPLE2_CUSTOMSCLIENTCODE");
			iOrganisationSimple2.Setup(m => m.Contacts).Returns(new List<IContact>() { iContact1.Object, iContact2.Object });

			consignor = new Mock<IPartyInformation>();
			consignor.Setup(m => m.Name).Returns("CONSIGNOR_NAME");
			consignor.Setup(m => m.City).Returns("CONSIGNOR_CITY");
			consignor.Setup(m => m.CountryCode).Returns("CONSIGNOR_COUNTRYCODE");
			consignor.Setup(m => m.CountryRegion).Returns("CONSIGNOR_COUNTRYREGION");
			consignor.Setup(m => m.Address).Returns("CONSIGNOR_ADDRESS");
			consignor.Setup(m => m.PostCode).Returns("CONSIGNOR_POSTCODE");

			var notify1 = new Mock<IPartyInformation>();
			notify1.Setup(m => m.Name).Returns("NOTIFY1_NAME");
			notify1.Setup(m => m.City).Returns("NOTIFY1_CITY");
			notify1.Setup(m => m.CountryCode).Returns("NOTIFY1_COUNTRYCODE");
			notify1.Setup(m => m.CountryRegion).Returns("NOTIFY1_COUNTRYREGION");
			notify1.Setup(m => m.Address).Returns("NOTIFY1_ADDRESS");
			notify1.Setup(m => m.PostCode).Returns("NOTIFY1_POSTCODE");

			var notify2 = new Mock<IPartyInformation>();
			notify2.Setup(m => m.Name).Returns("NOTIFY2_NAME");
			notify2.Setup(m => m.City).Returns("NOTIFY2_CITY");
			notify2.Setup(m => m.CountryCode).Returns("NOTIFY2_COUNTRYCODE");
			notify2.Setup(m => m.CountryRegion).Returns("NOTIFY2_COUNTRYREGION");
			notify2.Setup(m => m.Address).Returns("NOTIFY2_ADDRESS");
			notify2.Setup(m => m.PostCode).Returns("NOTIFY2_POSTCODE");

			var iAssociatedTransportDocument = new Mock<IAssociatedTransportDocument>();
			iAssociatedTransportDocument.Setup(m => m.BillNumber).Returns("BillNumber");
			iAssociatedTransportDocument.Setup(m => m.BillType).Returns("BillType");

			iTransportEquipment1 = new Mock<ITransportEquipment>();
			iTransportEquipment1.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSIZE");
			iTransportEquipment1.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSTATUS");
			iTransportEquipment1.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT1_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment1.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT1_CONTAINERNUMBER");
			iTransportEquipment1.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "ITRANSPORTEQUIPMENT1_SEALNUMBER1", "ITRANSPORTEQUIPMENT1_SEALNUMBER2" });

			var iTransportEquipment2 = new Mock<ITransportEquipment>();
			iTransportEquipment2.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT2_CONTAINERSIZE");
			iTransportEquipment2.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT2_CONTAINERSTATUS");
			iTransportEquipment2.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT2_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment2.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT2_CONTAINERNUMBER");
			iTransportEquipment2.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "ITRANSPORTEQUIPMENT2_SEALNUMBER1", "ITRANSPORTEQUIPMENT2_SEALNUMBER2" });

			var consignment1 = new Mock<ICREConsignment>();
			consignment1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignment1.Setup(m => m.ConsignmentValueInNZD).Returns(5.123);
			consignment1.Setup(m => m.WriteOffRequest).Returns(true);
			consignment1.Setup(m => m.TranshipmentDetails).Returns(consignment1ITRDetails.Object);
			consignment1.Setup(m => m.HandlingInfo).Returns("CONSIGNMENT1_HANDLINGINFO");
			consignment1.Setup(m => m.PortOfDischarge).Returns("CONSIGNMENT1_PORTOFDISCHARGE");
			consignment1.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "11111A", "PYKE" });
			consignment1.Setup(m => m.Consignee).Returns(consignee.Object);
			consignment1.Setup(m => m.ConsignmentItems).Returns(new List<ICREConsignmentItem>() { consignmentItem1.Object, consignmentItem2.Object });
			consignment1.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment1.Setup(m => m.FreightPaymentMethod).Returns("CONSIGNMENT1_FREIGHTPAYMENTMETHOD");
			consignment1.Setup(m => m.GoodsLocation).Returns("CONSIGNMENT1_GOODSLOCATION");
			consignment1.Setup(m => m.PortOfLoading).Returns("CONSIGNMENT1_PORTOFLOADING");
			consignment1.Setup(m => m.DeliveryNotifyParties).Returns(new List<IOrganisationSimple>() { iOrganisationSimple1.Object, iOrganisationSimple2.Object });
			consignment1.Setup(m => m.Consignor).Returns(consignor.Object);
			consignment1.Setup(m => m.NotifyParties).Returns(new List<IPartyInformation> { notify1.Object, notify2.Object });
			consignment1.Setup(m => m.BillNumber).Returns(iAssociatedTransportDocument.Object);
			consignment1.Setup(m => m.Consolidator).Returns(iOrganisationSimple1.Object);
			consignment1.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { iTransportEquipment1.Object, iTransportEquipment2.Object });
			consignment1.Setup(m => m.HasContainers).Returns(true);

			var consignment2 = new Mock<ICREConsignment>();
			consignment2.Setup(m => m.SequenceNumber).Returns((ZShort)2);
			consignment2.Setup(m => m.ConsignmentValueInNZD).Returns(9.876);
			consignment2.Setup(m => m.WriteOffRequest).Returns(true);
			consignment2.Setup(m => m.TranshipmentDetails).Returns(consignment2ITRDetails.Object);
			consignment2.Setup(m => m.HandlingInfo).Returns("CONSIGNMENT2_HANDLINGINFO");
			consignment2.Setup(m => m.PortOfDischarge).Returns("CONSIGNMENT2_PORTOFDISCHARGE");
			consignment2.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "22222B", "TARTH" });
			consignment2.Setup(m => m.Consignee).Returns(consignee.Object);
			consignment2.Setup(m => m.ConsignmentItems).Returns(new List<ICREConsignmentItem>() { consignmentItem1.Object, consignmentItem2.Object });
			consignment2.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment2.Setup(m => m.FreightPaymentMethod).Returns("CONSIGNMENT2_FREIGHTPAYMENTMETHOD");
			consignment2.Setup(m => m.GoodsLocation).Returns("CONSIGNMENT2_GOODSLOCATION");
			consignment2.Setup(m => m.PortOfLoading).Returns("CONSIGNMENT2_PORTOFLOADING");
			consignment2.Setup(m => m.DeliveryNotifyParties).Returns(new List<IOrganisationSimple>() { iOrganisationSimple1.Object, iOrganisationSimple2.Object });
			consignment2.Setup(m => m.Consignor).Returns(consignor.Object);
			consignment2.Setup(m => m.NotifyParties).Returns(new List<IPartyInformation> { notify1.Object, notify2.Object });
			consignment2.Setup(m => m.BillNumber).Returns(iAssociatedTransportDocument.Object);
			consignment2.Setup(m => m.Consolidator).Returns(iOrganisationSimple1.Object);
			consignment2.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { iTransportEquipment1.Object, iTransportEquipment2.Object });
			consignment2.Setup(m => m.HasContainers).Returns(true);

			iCargoReportExportMock = new Mock<ICargoReportExport>();
			iCargoReportExportMock.Setup(m => m.TSWReferenceNumber).Returns("ENTRY12345");
			iCargoReportExportMock.Setup(m => m.SenderReferenceNumber).Returns("C00001165");
			iCargoReportExportMock.Setup(m => m.IsSea).Returns(true);
			iCargoReportExportMock.Setup(m => m.IsAir).Returns(false);
			iCargoReportExportMock.Setup(m => m.IsMail).Returns(false);
			iCargoReportExportMock.Setup(m => m.CraftName).Returns("CRAFTNAME");
			iCargoReportExportMock.Setup(m => m.FlightNo).Returns("FLIGHTNO");
			iCargoReportExportMock.Setup(m => m.DepartureDate).Returns(new ZDateTime(2018, 10, 22));
			iCargoReportExportMock.Setup(m => m.LloydsNo).Returns("LLOYDSNO");
			iCargoReportExportMock.Setup(m => m.VoyageNo).Returns("VOYAGENO");
			iCargoReportExportMock.Setup(m => m.HasEmptyContainersOnly).Returns(false);
			iCargoReportExportMock.Setup(m => m.PortOfDeparture).Returns("PortOfDeparture");
			iCargoReportExportMock.Setup(m => m.AdditionalInformation).Returns(additionalInformationMock.Object);
			iCargoReportExportMock.Setup(m => m.Carrier).Returns(iOrganisationSimpleMock.Object);
			iCargoReportExportMock.Setup(m => m.UseInterfaceSequenceNumber).Returns(true);
			iCargoReportExportMock.Setup(m => m.Consignments).Returns(new List<ICREConsignment>() { consignment1.Object, consignment2.Object });
			iCargoReportExportMock.Setup(m => m.Declarant).Returns(iDeclarant.Object);

			creBuilder = new CREMessageBuilder(iCargoReportExportMock.Object, TSWTransactionTypes.Original, "00009908C");
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			var expectedXML = @"<TransportContractDocument>
      <ID>BillNumber</ID>
      <TypeCode>BillType</TypeCode>
      <Consolidator>
        <ID>IORGANISATIONSIMPLE1_CUSTOMSCLIENTCODE</ID>
      </Consolidator>
    </TransportContractDocument>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
			consignment1.Setup(m => m.BillNumber);
			consignment2.Setup(m => m.BillNumber);
			expectedXML = @"<ID>BillNumber</ID>
      <TypeCode>BillType</TypeCode>";
			AssertNotContains(expectedXML, creBuilder.GetXMLMessage());
			iOrganisationSimple1.Setup(m => m.CustomsClientCode).Returns(ZString.Empty);
			expectedXML = @"<Consolidator>
        <Name>IORGANISATIONSIMPLE1_NAME</Name>
      </Consolidator>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
			consignment1.Setup(m => m.Consolidator);
			expectedXML = @"<Consolidator>
        <ID>IORGANISATIONSIMPLE1_CUSTOMSCLIENTCODE</ID>
      </Consolidator>";
			AssertNotContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentTransportEquipment()
		{
			SetUpMocks();
			var expectedXML = @"<TransportEquipment>
      <SequenceNumeric>1</SequenceNumeric>
      <CharacteristicCode>ITRANSPORTEQUIPMENT1_CONTAINERSIZE</CharacteristicCode>
      <FullnessCode>ITRANSPORTEQUIPMENT1_CONTAINERSTATUS</FullnessCode>
      <AttachedCode>ITRANSPORTEQUIPMENT1_CONTAINERATTACHEDEQUIPMENTCODE</AttachedCode>
      <ID>ITRANSPORTEQUIPMENT1_CONTAINERNUMBER</ID>
      <Seal>
        <SequenceNumeric>1</SequenceNumeric>
        <ID>ITRANSPORTEQUIPMENT1_SEALNUMBER1</ID>
      </Seal>
      <Seal>
        <SequenceNumeric>2</SequenceNumeric>
        <ID>ITRANSPORTEQUIPMENT1_SEALNUMBER2</ID>
      </Seal>
    </TransportEquipment>
    <TransportEquipment>
      <SequenceNumeric>2</SequenceNumeric>
      <CharacteristicCode>ITRANSPORTEQUIPMENT2_CONTAINERSIZE</CharacteristicCode>
      <FullnessCode>ITRANSPORTEQUIPMENT2_CONTAINERSTATUS</FullnessCode>
      <AttachedCode>ITRANSPORTEQUIPMENT2_CONTAINERATTACHEDEQUIPMENTCODE</AttachedCode>
      <ID>ITRANSPORTEQUIPMENT2_CONTAINERNUMBER</ID>
      <Seal>
        <SequenceNumeric>1</SequenceNumeric>
        <ID>ITRANSPORTEQUIPMENT2_SEALNUMBER1</ID>
      </Seal>
      <Seal>
        <SequenceNumeric>2</SequenceNumeric>
        <ID>ITRANSPORTEQUIPMENT2_SEALNUMBER2</ID>
      </Seal>
    </TransportEquipment>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
			iTransportEquipment1.Setup(m => m.AttachedEquipmentCode).Returns(ZString.Empty);
			AssertNotContains("<AttachedCode>ITRANSPORTEQUIPMENT1_CONTAINERATTACHEDEQUIPMENTCODE</AttachedCode>", creBuilder.GetXMLMessage());
			consignment1.Setup(m => m.Containers).Returns(Enumerable.Empty<ITransportEquipment>());
			consignment2.Setup(m => m.Containers).Returns(Enumerable.Empty<ITransportEquipment>());
			expectedXML = @"<TransportEquipment>
      <SequenceNumeric>1</SequenceNumeric>
      <CharacteristicCode>ITRANSPORTEQUIPMENT1_CONTAINERSIZE</CharacteristicCode>
      <FullnessCode>ITRANSPORTEQUIPMENT1_CONTAINERSTATUS</FullnessCode>
      <ID>ITRANSPORTEQUIPMENT1_CONTAINERNUMBER</ID>
      <Seal>
        <SequenceNumeric>1</SequenceNumeric>
        <ID>ITRANSPORTEQUIPMENT1_SEALNUMBER1</ID>
      </Seal>
      <Seal>
        <SequenceNumeric>2</SequenceNumeric>
        <ID>ITRANSPORTEQUIPMENT1_SEALNUMBER2</ID>
      </Seal>
    </TransportEquipment>";
			AssertNotContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateRORConsignmentTransportEquipment()
		{
			SetUpMocks();
			var result = creBuilder.GetXMLMessage();
			var expectedXML = @"<TransportEquipment>
      <SequenceNumeric>3</SequenceNumeric>
      <CharacteristicCode>ITRANSPORTEQUIPMENT3_CONTAINERSIZE</CharacteristicCode>
      <FullnessCode>ITRANSPORTEQUIPMENT3_CONTAINERSTATUS</FullnessCode>
      <AttachedCode>ITRANSPORTEQUIPMENT3_CONTAINERATTACHEDEQUIPMENTCODE</AttachedCode>
      <ID>ITRANSPORTEQUIPMENT3_CONTAINERNUMBER</ID>
    </TransportEquipment>";
			AssertNotContains(expectedXML, result);
			iTransportEquipment3.Setup(m => m.ContainerMode).Returns(ContainerModeList.Codes.FCL);
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentItemTransportEquipment()
		{
			SetUpMocks();
			var expectedXML = @"<TransportEquipment>
        <ID>ITRANSPORTEQUIPMENT1_CONTAINERNUMBER</ID>
      </TransportEquipment>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateRORConsignmentItemTransportEquipment()
		{
			SetUpMocks();
			var result = creBuilder.GetXMLMessage();
			var expectedXML = @"<TransportEquipment>
        <ID>ITRANSPORTEQUIPMENT3_CONTAINERNUMBER</ID>
      </TransportEquipment>";
			AssertNotContains(expectedXML, result);
			iTransportEquipment3.Setup(m => m.ContainerMode).Returns(ContainerModeList.Codes.FCL);
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateUnloadingLocation()
		{
			SetUpMocks();
			var expectedXML = @"<UnloadingLocation>
      <ID>CONSIGNMENT1_PORTOFDISCHARGE</ID>
    </UnloadingLocation>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentItemsSea()
		{
			SetUpMocks();
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestCREPopulateConsignmentItemsSea.txt")), creBuilder.GetXMLMessage());

			consignmentItem1.Reset();
			consignmentItem1.Setup(m => m.IsEmptyContainer).Returns(true);
			var unexpectedXML = @"<GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">1.361</GrossMassMeasure>
      </GoodsMeasure>
      <Origin>
        <CountryCode>CONSIGNMENTITEM1_GOODSORIGINCOUNTRY</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>2</QuantityQuantity>
        <TypeCode>CONSIGNMENTITEM1_PACKAGETYPE</TypeCode>
      </Packaging>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
			consignmentItem1.Reset();
			consignmentItem1.Setup(m => m.ContainerNumber).Returns(ZString.Empty);
			unexpectedXML = @"<TransportEquipment>
        <ID>ITRANSPORTEQUIPMENT1_CONTAINERNUMBER</ID>
      </TransportEquipment>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentItemsAir()
		{
			SetUpMocks();
			iCargoReportExportMock.Setup(m => m.IsSea).Returns(false);
			iCargoReportExportMock.Setup(m => m.IsAir).Returns(true);
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestCREPopulateConsignmentItemsAir.txt")), creBuilder.GetXMLMessage());

			consignmentItem1.Setup(m => m.IsEmptyContainer).Returns(true);
			var unexpectedXML = @"<GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">1.361</GrossMassMeasure>
      </GoodsMeasure>
      <Origin>
        <CountryCode>CONSIGNMENTITEM1_GOODSORIGINCOUNTRY</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>2</QuantityQuantity>
        <TypeCode>CONSIGNMENTITEM1_PACKAGETYPE</TypeCode>
      </Packaging>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentItemsMail()
		{
			SetUpMocks();
			iCargoReportExportMock.Setup(m => m.IsSea).Returns(false);
			iCargoReportExportMock.Setup(m => m.IsMail).Returns(true);
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestCREPopulateConsignmentItemsMail.txt")), creBuilder.GetXMLMessage());

			consignmentItem1.Setup(m => m.IsEmptyContainer).Returns(true);
			var unexpectedXML = @"<GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">1.361</GrossMassMeasure>
      </GoodsMeasure>
      <Origin>
        <CountryCode>CONSIGNMENTITEM1_GOODSORIGINCOUNTRY</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>2</QuantityQuantity>
        <TypeCode>CONSIGNMENTITEM1_PACKAGETYPE</TypeCode>
      </Packaging>";
			AssertNotContains(unexpectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateCommodity()
		{
			SetUpMocks();
			var expectedXML = @"<Commodity>
        <CargoDescription>CONSIGNMENTITEM1GOODSDESCRIPTION</CargoDescription>
        <CommercialCategorizationID>IDENTIFIER2_COMMODITYNUMBER</CommercialCategorizationID>
        <ValueAmount currencyID=""AUD"">9.67</ValueAmount>
        <IdentityQualifierCode>IDENTIFIER2_COMMODITYTYPE</IdentityQualifierCode>
        <Classification>
          <ID>CONSIGNMENTITEM1_UNDGHAZARDOUSGOODSCODE</ID>
          <IdentificationTypeCode>SSO</IdentificationTypeCode>
        </Classification>
        <Classification>
          <ID>CLASSIFICATIONS1_CLASSIFICATION</ID>
          <IdentificationTypeCode>SSO</IdentificationTypeCode>
        </Classification>
        <Classification>
          <ID>CLASSIFICATIONS1_CLASSIFICATION</ID>
          <IdentificationTypeCode>SSO</IdentificationTypeCode>
        </Classification>
      </Commodity>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateItemClassifications()
		{
			SetUpMocks();
			var expectedXML = @"<Classification>
          <ID>CONSIGNMENTITEM1_UNDGHAZARDOUSGOODSCODE</ID>
          <IdentificationTypeCode>SSO</IdentificationTypeCode>
        </Classification>
        <Classification>
          <ID>CLASSIFICATIONS1_CLASSIFICATION</ID>
          <IdentificationTypeCode>SSO</IdentificationTypeCode>
        </Classification>
        <Classification>
          <ID>CLASSIFICATIONS1_CLASSIFICATION</ID>
          <IdentificationTypeCode>SSO</IdentificationTypeCode>
        </Classification>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPaymentTerms()
		{
			SetUpMocks();
			consignment1.Setup(m => m.FreightPaymentMethod).Returns(FreightPaymentMethodList.Codes.FO);
			var expectedXML = @"<Freight>
      <PaymentMethodCode>FO</PaymentMethodCode>
    </Freight>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateGoodsMeasure()
		{
			SetUpMocks();
			var expectedXML = @"<GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">1.361</GrossMassMeasure>
      </GoodsMeasure>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateOrigin()
		{
			SetUpMocks();
			var expectedXML = @"<Origin>
        <CountryCode>CONSIGNMENTITEM1_GOODSORIGINCOUNTRY</CountryCode>
      </Origin>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulatePackaging()
		{
			SetUpMocks();
			var expectedXML = @"<Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>2</QuantityQuantity>
        <TypeCode>CONSIGNMENTITEM1_PACKAGETYPE</TypeCode>
      </Packaging>";
			AssertContains(expectedXML, creBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentValueAmount()
		{
			SetUpMocks();
			consignment1.Setup(m => m.ConsignmentValueInNZD).Returns(-6.65);
			AssertContains(@"<ValueAmount currencyID=""NZD"">-6.65</ValueAmount>", creBuilder.GetXMLMessage());
			consignment1.Setup(m => m.ConsignmentValueInNZD).Returns(6.65);
			AssertContains(@"<ValueAmount currencyID=""NZD"">6.65</ValueAmount>", creBuilder.GetXMLMessage());
			consignment1.Setup(m => m.ConsignmentValueInNZD).Returns(0.00);
			AssertNotContains(@"<ValueAmount currencyID=""NZD"">0</ValueAmount>", creBuilder.GetXMLMessage());
			consignment1.Setup(m => m.ConsignmentValueInNZD).Returns(6.65);
			iCargoReportExportMock.Setup(m => m.HasEmptyContainersOnly).Returns(true);
			AssertNotContains(@"<ValueAmount currencyID=""NZD"">6.65</ValueAmount>", creBuilder.GetXMLMessage());
		}

		public void TestPopulateCommodityValueAmount()
		{
			SetUpMocks();
			consignmentItem1.Reset();
			consignmentItem1.Reset();
			consignmentItem1.Setup(m => m.Value).Returns(-13.54);
			consignmentItem1.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.Australia);
			AssertContains(@"<ValueAmount currencyID=""AUD"">-13.54</ValueAmount>", creBuilder.GetXMLMessage());
			consignmentItem1.Reset();
			consignmentItem1.Reset();
			consignmentItem1.Setup(m => m.Value).Returns(13.54);
			consignmentItem1.Setup(m => m.Currency).Returns(ZString.Empty);
			AssertContains(@"<ValueAmount currencyID=""AUD"">13.54</ValueAmount>", creBuilder.GetXMLMessage());
			consignmentItem1.Reset();
			consignmentItem1.Setup(m => m.Value).Returns(ZDecimal.Zero);
			AssertContains("commodity value is required even if zero - NZD will default if zero & no currency has been entered", @"<ValueAmount currencyID=""NZD"">0</ValueAmount>", creBuilder.GetXMLMessage());
			consignmentItem1.Reset();
			consignmentItem1.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.Australia);
			AssertContains("commodity value is required even if zero", @"<ValueAmount currencyID=""AUD"">0</ValueAmount>", creBuilder.GetXMLMessage());
			consignmentItem1.Reset();
			consignmentItem1.Setup(m => m.Value).Returns(13.54);
			consignmentItem1.Reset();
			consignmentItem1.Setup(m => m.IsEmptyContainer).Returns(true);
			AssertNotContains("Value is not required for empty containers", @"<ValueAmount currencyID=""AUD"">13.54</ValueAmount>", creBuilder.GetXMLMessage());
		}

		public void TestCREWithITRDetailsDoesNotSendWOFInfo()
		{
			SetUpMocks();
			consignment1ITRDetails = new Mock<ITranshipmentDetails>();
			consignment1ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment1ITRDetails.Setup(m => m.ITRImportMode).Returns("4");
			consignment1ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("QF107");
			consignment1ITRDetails.Setup(m => m.ITRArrivalDate).Returns(new ZDateTime(2018, 11, 27));
			consignment1ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("1");
			consignment1.Setup(m => m.TranshipmentDetails).Returns(consignment1ITRDetails.Object);
			var writeOffElementToBeExcluded = @"<AdditionalInformation>
      <StatementCode>Y</StatementCode>
      <StatementTypeCode>WOF</StatementTypeCode>
    </AdditionalInformation>";
			var itrElementExpected = @"AdditionalInformation>
      <StatementDescription>QF107,4,20181127,</StatementDescription>
      <StatementTypeCode>ITR</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementCode>1</StatementCode>
      <StatementTypeCode>MTT</StatementTypeCode>
    </AdditionalInformation>";
			var creMessage = creBuilder.GetXMLMessage();
			AssertNotContains("Where a consignment has ITR details, the WOF statement element should NOT be generated", writeOffElementToBeExcluded, creMessage);
			AssertContains("ITR elements have been included for this consignment", itrElementExpected, creMessage);
		}

		void SetUpMocks()
		{
			var tswAttachment1 = new Mock<ITSWAttachment>();
			tswAttachment1.Setup(m => m.DocType).Returns("CDO");
			tswAttachment1.Setup(m => m.FileName).Returns("TEST1.PDF");

			var tswAttachment2 = new Mock<ITSWAttachment>();
			tswAttachment2.Setup(m => m.DocType).Returns("INV");
			tswAttachment2.Setup(m => m.FileName).Returns("TEST2.PDF");

			additionalInformationMock = new Mock<IAdditionalInformation>();
			additionalInformationMock.Setup(m => m.SupportingDocuments).Returns(new List<ITSWAttachment>() { tswAttachment1.Object, tswAttachment2.Object });
			additionalInformationMock.Setup(m => m.FreeText).Returns("ADDITIONALINFORMATIONMOCK_FREETEXT");
			additionalInformationMock.Setup(m => m.ManualOverrideText).Returns("ADDITIONALINFORMATIONMOCK_MANUALOVERRIDETEXT");
			additionalInformationMock.Setup(m => m.AdditionalStatementText).Returns("ADDITIONALSTATEMENTTEXT");

			iOrganisationSimpleMock = new Mock<IOrganisationSimple>();
			iOrganisationSimpleMock.Setup(m => m.Name).Returns("IORGANISATIONSIMPLEMOCK_NAME");
			iOrganisationSimpleMock.Setup(m => m.CustomsClientCode).Returns("51358595A");

			consignment1ITRDetails = new Mock<ITranshipmentDetails>();
			consignment1ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment1ITRDetails.Setup(m => m.ITRImportMode).Returns("4");
			consignment1ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("CONSIGNMENT1_ITRVOYAGEFLIGHT");
			consignment1ITRDetails.Setup(m => m.ITRArrivalDate).Returns(new ZDateTime(2018, 10, 22));
			consignment1ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("CONSIGNMENT1_MODEOFTRANSPORTFORTRANSFER");
			consignment1ITRDetails.Setup(m => m.ITRImportCraft).Returns("CONSIGNMENT1_ITRIMPORTCRAFT");

			consignment2ITRDetails = new Mock<ITranshipmentDetails>();
			consignment2ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment2ITRDetails.Setup(m => m.ITRImportMode).Returns("1");
			consignment2ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("CONSIGNMENT2_ITRVOYAGEFLIGHT");
			consignment2ITRDetails.Setup(m => m.ITRArrivalDate).Returns(new ZDateTime(2018, 10, 21));
			consignment2ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("CONSIGNMENT2_MODEOFTRANSPORTFORTRANSFER");
			consignment2ITRDetails.Setup(m => m.ITRImportCraft).Returns("CONSIGNMENT2_ITRIMPORTCRAFT");

			consignee = new Mock<IPartyInformation>();
			consignee.Setup(m => m.Name).Returns("CONSIGNEE_NAME");
			consignee.Setup(m => m.City).Returns("CONSIGNEE_CITY");
			consignee.Setup(m => m.CountryCode).Returns("CONSIGNEE_COUNTRYCODE");
			consignee.Setup(m => m.CountryRegion).Returns("CONSIGNEE_COUNTRYREGION");
			consignee.Setup(m => m.Address).Returns("CONSIGNEE_ADDRESS");
			consignee.Setup(m => m.PostCode).Returns("CONSIGNEE_POSTCODE");

			var classifications1 = new Mock<IClassification>();
			classifications1.Setup(m => m.Classification).Returns("CLASSIFICATIONS1_CLASSIFICATION");
			classifications1.Setup(m => m.ClassificationTypeCode).Returns("SSO");

			var classifications2 = new Mock<IClassification>();
			classifications2.Setup(m => m.Classification).Returns("CLASSIFICATIONS1_CLASSIFICATION");
			classifications2.Setup(m => m.ClassificationTypeCode).Returns("SSO");

			var identifier1 = new Mock<ICommodity>();
			identifier1.Setup(m => m.CommodityNumber).Returns("IDENTIFIER1_COMMODITYNUMBER");
			identifier1.Setup(m => m.CommodityType).Returns("IDENTIFIER1_COMMODITYTYPE");

			var identifier2 = new Mock<ICommodity>();
			identifier2.Setup(m => m.CommodityNumber).Returns("IDENTIFIER2_COMMODITYNUMBER");
			identifier2.Setup(m => m.CommodityType).Returns("IDENTIFIER2_COMMODITYTYPE");

			consignmentItem1 = new Mock<ICREConsignmentItem>();
			consignmentItem1.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM1_GOODSDESCRIPTION");
			consignmentItem1.Setup(m => m.UNDGHazardousGoodsCode).Returns("CONSIGNMENTITEM1_UNDGHAZARDOUSGOODSCODE");
			consignmentItem1.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem1.Setup(m => m.Value).Returns(9.673);
			consignmentItem1.Setup(m => m.GrossWeightInKg).Returns(1.360777m);
			consignmentItem1.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM1_GOODSORIGINCOUNTRY");
			consignmentItem1.Setup(m => m.PackageQty).Returns(2);
			consignmentItem1.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM1_PACKAGETYPE");
			consignmentItem1.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT1_CONTAINERNUMBER");
			consignmentItem1.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.Australia);
			consignmentItem1.Setup(m => m.Classifications).Returns(new List<IClassification>() { classifications1.Object, classifications2.Object });
			consignmentItem1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignmentItem1.Setup(m => m.Identifiers).Returns(new List<ICommodity>() { identifier1.Object, identifier2.Object });

			var consignmentItem2 = new Mock<ICREConsignmentItem>();
			consignmentItem2.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM2_GOODSDESCRIPTION");
			consignmentItem2.Setup(m => m.UNDGHazardousGoodsCode).Returns("CONSIGNMENTITEM2_UNDGHAZARDOUSGOODSCODE");
			consignmentItem2.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem2.Setup(m => m.Value).Returns(8.519);
			consignmentItem2.Setup(m => m.GrossWeightInKg).Returns(1.360777m);
			consignmentItem2.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM2_GOODSORIGINCOUNTRY");
			consignmentItem2.Setup(m => m.PackageQty).Returns(2);
			consignmentItem2.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM2_PACKAGETYPE");
			consignmentItem2.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT2_CONTAINERNUMBER");
			consignmentItem2.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.UnitedStates);
			consignmentItem2.Setup(m => m.Classifications).Returns(new List<IClassification>() { classifications1.Object, classifications2.Object });
			consignmentItem2.Setup(m => m.SequenceNumber).Returns((ZShort)2);
			consignmentItem2.Setup(m => m.Identifiers).Returns(new List<ICommodity>() { identifier1.Object, identifier2.Object });

			var consignmentItem3 = new Mock<ICREConsignmentItem>();
			consignmentItem3.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM3_GOODSDESCRIPTION");
			consignmentItem3.Setup(m => m.UNDGHazardousGoodsCode).Returns("CONSIGNMENTITEM3_UNDGHAZARDOUSGOODSCODE");
			consignmentItem3.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem3.Setup(m => m.Value).Returns(8.519);
			consignmentItem3.Setup(m => m.GrossWeightInKg).Returns(1.360777m);
			consignmentItem3.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM3_GOODSORIGINCOUNTRY");
			consignmentItem3.Setup(m => m.PackageQty).Returns(2);
			consignmentItem3.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM3_PACKAGETYPE");
			consignmentItem3.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT3_CONTAINERNUMBER");
			consignmentItem3.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.UnitedStates);
			consignmentItem3.Setup(m => m.Classifications).Returns(new List<IClassification>() { classifications1.Object, classifications2.Object });
			consignmentItem3.Setup(m => m.SequenceNumber).Returns((ZShort)3);
			consignmentItem3.Setup(m => m.Identifiers).Returns(new List<ICommodity>() { identifier1.Object, identifier2.Object });

			iOrganisation1 = new Mock<IOrganisation>();
			iOrganisation1.Setup(m => m.Name).Returns("IORGANISATION1_NAME");
			iOrganisation1.Setup(m => m.CustomsClientCode).Returns("IORGANISATION1_CUSTOMSCLIENTCODE");
			iOrganisation1.Setup(m => m.City).Returns("IORGANISATION1_CITY");
			iOrganisation1.Setup(m => m.CountryCode).Returns("IORGANISATION1_COUNTRYCODE");
			iOrganisation1.Setup(m => m.CountryRegion).Returns("IORGANISATION1_COUNTRYREGION");
			iOrganisation1.Setup(m => m.Address).Returns("IORGANISATION1_ADDRESS");
			iOrganisation1.Setup(m => m.PostCode).Returns("IORGANISATION1_POSTCODE");

			var iOrganisation2 = new Mock<IOrganisation>();
			iOrganisation2.Setup(m => m.Name).Returns("IORGANISATION2_NAME");
			iOrganisation2.Setup(m => m.CustomsClientCode).Returns("IORGANISATION2_CUSTOMSCLIENTCODE");
			iOrganisation2.Setup(m => m.City).Returns("IORGANISATION2_CITY");
			iOrganisation2.Setup(m => m.CountryCode).Returns("IORGANISATION2_COUNTRYCODE");
			iOrganisation2.Setup(m => m.CountryRegion).Returns("IORGANISATION2_COUNTRYREGION");
			iOrganisation2.Setup(m => m.Address).Returns("IORGANISATION2_ADDRESS");
			iOrganisation2.Setup(m => m.PostCode).Returns("IORGANISATION2_POSTCODE");

			iCommunication1 = new Mock<ICommunication>();
			iCommunication1.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION1_CONTACTDETAIL");
			iCommunication1.Setup(m => m.ContactType).Returns("EM");

			var iCommunication2 = new Mock<ICommunication>();
			iCommunication2.Setup(m => m.ContactDetail).Returns("ICOMMUNICATION2_CONTACTDETAIL");
			iCommunication2.Setup(m => m.ContactType).Returns("EM");

			var iDeclarant = new Mock<IDeclarant>();
			iDeclarant.Setup(m => m.DeclarantID).Returns("DeclarantID");
			iDeclarant.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var iContact1 = new Mock<IContact>();
			iContact1.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var iContact2 = new Mock<IContact>();
			iContact2.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			iOrganisationSimple1 = new Mock<IOrganisationSimple>();
			iOrganisationSimple1.Setup(m => m.Name).Returns("IORGANISATIONSIMPLE1_NAME");
			iOrganisationSimple1.Setup(m => m.CustomsClientCode).Returns("IORGANISATIONSIMPLE1_CUSTOMSCLIENTCODE");
			iOrganisationSimple1.Setup(m => m.Contacts).Returns(new List<IContact>() { iContact1.Object, iContact2.Object });

			var iOrganisationSimple2 = new Mock<IOrganisationSimple>();
			iOrganisationSimple2.Setup(m => m.Name).Returns("IORGANISATIONSIMPLE2_NAME");
			iOrganisationSimple2.Setup(m => m.CustomsClientCode).Returns("IORGANISATIONSIMPLE2_CUSTOMSCLIENTCODE");
			iOrganisationSimple2.Setup(m => m.Contacts).Returns(new List<IContact>() { iContact1.Object, iContact2.Object });

			consignor = new Mock<IPartyInformation>();
			consignor.Setup(m => m.Name).Returns("CONSIGNOR_NAME");
			consignor.Setup(m => m.City).Returns("CONSIGNOR_CITY");
			consignor.Setup(m => m.CountryCode).Returns("CONSIGNOR_COUNTRYCODE");
			consignor.Setup(m => m.CountryRegion).Returns("CONSIGNOR_COUNTRYREGION");
			consignor.Setup(m => m.Address).Returns("CONSIGNOR_ADDRESS");
			consignor.Setup(m => m.PostCode).Returns("CONSIGNOR_POSTCODE");

			var notify1 = new Mock<IPartyInformation>();
			notify1.Setup(m => m.Name).Returns("NOTIFY1_NAME");
			notify1.Setup(m => m.City).Returns("NOTIFY1_CITY");
			notify1.Setup(m => m.CountryCode).Returns("NOTIFY1_COUNTRYCODE");
			notify1.Setup(m => m.CountryRegion).Returns("NOTIFY1_COUNTRYREGION");
			notify1.Setup(m => m.Address).Returns("NOTIFY1_ADDRESS");
			notify1.Setup(m => m.PostCode).Returns("NOTIFY1_POSTCODE");

			var notify2 = new Mock<IPartyInformation>();
			notify2.Setup(m => m.Name).Returns("NOTIFY2_NAME");
			notify2.Setup(m => m.City).Returns("NOTIFY2_CITY");
			notify2.Setup(m => m.CountryCode).Returns("NOTIFY2_COUNTRYCODE");
			notify2.Setup(m => m.CountryRegion).Returns("NOTIFY2_COUNTRYREGION");
			notify2.Setup(m => m.Address).Returns("NOTIFY2_ADDRESS");
			notify2.Setup(m => m.PostCode).Returns("NOTIFY2_POSTCODE");

			var iAssociatedTransportDocument = new Mock<IAssociatedTransportDocument>();
			iAssociatedTransportDocument.Setup(m => m.BillNumber).Returns("BillNumber");
			iAssociatedTransportDocument.Setup(m => m.BillType).Returns("BillType");

			iTransportEquipment1 = new Mock<ITransportEquipment>();
			iTransportEquipment1.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSIZE");
			iTransportEquipment1.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSTATUS");
			iTransportEquipment1.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT1_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment1.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT1_CONTAINERNUMBER");
			iTransportEquipment1.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "ITRANSPORTEQUIPMENT1_SEALNUMBER1", "ITRANSPORTEQUIPMENT1_SEALNUMBER2" });

			var iTransportEquipment2 = new Mock<ITransportEquipment>();
			iTransportEquipment2.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT2_CONTAINERSIZE");
			iTransportEquipment2.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT2_CONTAINERSTATUS");
			iTransportEquipment2.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT2_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment2.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT2_CONTAINERNUMBER");
			iTransportEquipment2.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "ITRANSPORTEQUIPMENT2_SEALNUMBER1", "ITRANSPORTEQUIPMENT2_SEALNUMBER2" });

			iTransportEquipment3 = new Mock<ITransportEquipment>();
			iTransportEquipment3.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT3_CONTAINERSIZE");
			iTransportEquipment3.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT3_CONTAINERSTATUS");
			iTransportEquipment3.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT3_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment3.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT3_CONTAINERNUMBER");
			iTransportEquipment3.Setup(m => m.ContainerMode).Returns(ContainerModeList.Codes.ROR);

			consignment1 = new Mock<ICREConsignment>();
			consignment1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignment1.Setup(m => m.ConsignmentValueInNZD).Returns(5.123);
			consignment1.Setup(m => m.WriteOffRequest).Returns(true);
			consignment1.Setup(m => m.TranshipmentDetails).Returns(consignment1ITRDetails.Object);
			consignment1.Setup(m => m.HandlingInfo).Returns("CONSIGNMENT1_HANDLINGINFO");
			consignment1.Setup(m => m.PortOfDischarge).Returns("CONSIGNMENT1_PORTOFDISCHARGE");
			consignment1.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "11111A", "PYKE" });
			consignment1.Setup(m => m.Consignee).Returns(consignee.Object);
			consignment1.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment1.Setup(m => m.FreightPaymentMethod).Returns("CONSIGNMENT1_FREIGHTPAYMENTMETHOD");
			consignment1.Setup(m => m.GoodsLocation).Returns("CONSIGNMENT1_GOODSLOCATION");
			consignment1.Setup(m => m.PortOfLoading).Returns("CONSIGNMENT1_PORTOFLOADING");
			consignment1.Setup(m => m.DeliveryNotifyParties).Returns(new List<IOrganisationSimple>() { iOrganisationSimple1.Object, iOrganisationSimple2.Object });
			consignment1.Setup(m => m.Consignor).Returns(consignor.Object);
			consignment1.Setup(m => m.ConsignmentItems).Returns(new List<ICREConsignmentItem>() { consignmentItem1.Object, consignmentItem2.Object, consignmentItem3.Object });
			consignment1.Setup(m => m.NotifyParties).Returns(new List<IPartyInformation> { notify1.Object, notify2.Object });
			consignment1.Setup(m => m.BillNumber).Returns(iAssociatedTransportDocument.Object);
			consignment1.Setup(m => m.Consolidator).Returns(iOrganisationSimple1.Object);
			consignment1.Setup(m => m.HasContainers).Returns(true);
			consignment1.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { iTransportEquipment1.Object, iTransportEquipment2.Object, iTransportEquipment3.Object });

			consignment2 = new Mock<ICREConsignment>();
			consignment2.Setup(m => m.SequenceNumber).Returns((ZShort)2);
			consignment2.Setup(m => m.ConsignmentValueInNZD).Returns(9.876);
			consignment2.Setup(m => m.WriteOffRequest).Returns(true);
			consignment2.Setup(m => m.TranshipmentDetails).Returns(consignment2ITRDetails.Object);
			consignment2.Setup(m => m.HandlingInfo).Returns("CONSIGNMENT2_HANDLINGINFO");
			consignment2.Setup(m => m.PortOfDischarge).Returns("CONSIGNMENT2_PORTOFDISCHARGE");
			consignment2.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "22222B", "TARTH" });
			consignment2.Setup(m => m.Consignee).Returns(consignee.Object);
			consignment2.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment2.Setup(m => m.FreightPaymentMethod).Returns("CONSIGNMENT2_FREIGHTPAYMENTMETHOD");
			consignment2.Setup(m => m.GoodsLocation).Returns("CONSIGNMENT2_GOODSLOCATION");
			consignment2.Setup(m => m.PortOfLoading).Returns("CONSIGNMENT2_PORTOFLOADING");
			consignment2.Setup(m => m.DeliveryNotifyParties).Returns(new List<IOrganisationSimple>() { iOrganisationSimple1.Object, iOrganisationSimple2.Object });
			consignment2.Setup(m => m.Consignor).Returns(consignor.Object);
			consignment2.Setup(m => m.ConsignmentItems).Returns(new List<ICREConsignmentItem>() { consignmentItem1.Object, consignmentItem2.Object, consignmentItem3.Object });
			consignment2.Setup(m => m.NotifyParties).Returns(new List<IPartyInformation> { notify1.Object, notify2.Object });
			consignment2.Setup(m => m.BillNumber).Returns(iAssociatedTransportDocument.Object);
			consignment2.Setup(m => m.Consolidator).Returns(iOrganisationSimple1.Object);
			consignment2.Setup(m => m.HasContainers).Returns(true);
			consignment2.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { iTransportEquipment1.Object, iTransportEquipment2.Object, iTransportEquipment3.Object });

			iCargoReportExportMock = new Mock<ICargoReportExport>();
			iCargoReportExportMock.Setup(m => m.TSWReferenceNumber).Returns("ENTRY12345");
			iCargoReportExportMock.Setup(m => m.SenderReferenceNumber).Returns("C00001165");
			iCargoReportExportMock.Setup(m => m.IsSea).Returns(true);
			iCargoReportExportMock.Setup(m => m.IsAir).Returns(false);
			iCargoReportExportMock.Setup(m => m.IsMail).Returns(false);
			iCargoReportExportMock.Setup(m => m.CraftName).Returns("CRAFTNAME");
			iCargoReportExportMock.Setup(m => m.FlightNo).Returns("FLIGHTNO");
			iCargoReportExportMock.Setup(m => m.DepartureDate).Returns(new ZDateTime(2018, 10, 22));
			iCargoReportExportMock.Setup(m => m.LloydsNo).Returns("LLOYDSNO");
			iCargoReportExportMock.Setup(m => m.VoyageNo).Returns("VOYAGENO");
			iCargoReportExportMock.Setup(m => m.HasEmptyContainersOnly).Returns(false);
			iCargoReportExportMock.Setup(m => m.PortOfDeparture).Returns("PortOfDeparture");
			iCargoReportExportMock.Setup(m => m.AdditionalInformation).Returns(additionalInformationMock.Object);
			iCargoReportExportMock.Setup(m => m.Carrier).Returns(iOrganisationSimpleMock.Object);
			iCargoReportExportMock.Setup(m => m.UseInterfaceSequenceNumber).Returns(true);
			iCargoReportExportMock.Setup(m => m.Consignments).Returns(new List<ICREConsignment>() { consignment1.Object, consignment2.Object });
			iCargoReportExportMock.Setup(m => m.Declarant).Returns(iDeclarant.Object);

			creBuilder = new CREMessageBuilder(iCargoReportExportMock.Object, TSWTransactionTypes.Original, "00009908C");
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		Mock<ICargoReportExport> iCargoReportExportMock;
		Mock<IAdditionalInformation> additionalInformationMock;
		Mock<IOrganisationSimple> iOrganisationSimpleMock;
		Mock<ICREConsignment> consignment1;
		Mock<ICREConsignment> consignment2;
		Mock<IPartyInformation> consignee;
		Mock<IOrganisation> iOrganisation1;
		Mock<ICREConsignmentItem> consignmentItem1;
		Mock<IOrganisationSimple> iOrganisationSimple1;
		Mock<ICommunication> iCommunication1;
		Mock<ITransportEquipment> iTransportEquipment1;
		Mock<ITransportEquipment> iTransportEquipment3;
		CREMessageBuilder creBuilder;
		Mock<IPartyInformation> consignor;
		Mock<ITranshipmentDetails> consignment1ITRDetails;
		Mock<ITranshipmentDetails> consignment2ITRDetails;
		EmbeddedResourceRetriever embeddedResourceRetriever;
	}
}
