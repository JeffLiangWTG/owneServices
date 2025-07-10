using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.IO;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	public class ICRMessageBuilderTest : TSWMessageBuilderTest
	{
		public void TestDeclarantPinRequired()
		{
			SetUpMocks();
			Assert(icrBuilder.DeclarantPinRequired);
			consignment1.Reset();
			consignment1.Setup(m => m.WriteOffRequest).Returns(false);
			Assert(icrBuilder.DeclarantPinRequired);
			consignment2.Reset();
			consignment2.Setup(m => m.WriteOffRequest).Returns(false);
			Assert(!icrBuilder.DeclarantPinRequired);
		}

		public void TestICRMessageOriginal()
		{
			SetUpMocks();
			AssertMultilineASCIIEquals(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestICRMessageOriginal.txt")), icrBuilder.GetXMLMessage());
			iInwardCargoReportMock.Reset();
			var carrierXML = @"<Carrier>
    <ID>51358595A</ID>
  </Carrier>";
			AssertNotContains(carrierXML, icrBuilder.GetXMLMessage());
		}

		public void TestICRMessageReplace()
		{
			SetUpMocks();
			icrBuilder = new ICRMessageBuilder(iInwardCargoReportMock.Object, TSWTransactionTypes.Replace, "00009908C");
			AssertMultilineASCIIEquals(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestICRMessageReplace.txt")), icrBuilder.GetXMLMessage());
			iInwardCargoReportMock.Reset();
			var carrierXML = @"<Carrier>
    <ID>51358595A</ID>
  </Carrier>";
			AssertNotContains(carrierXML, icrBuilder.GetXMLMessage());
		}

		public void TestICRMessageCancel()
		{
			SetUpMocks();
			icrBuilder = new ICRMessageBuilder(iInwardCargoReportMock.Object, TSWTransactionTypes.Cancel, "00009908C");
			AssertMultilineASCIIEquals(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestICRMessageCancel.txt")), icrBuilder.GetXMLMessage());
		}

		public void TestPopulateStuffingEstablishments()
		{
			SetUpMocks();
			var expectedXML = @"<StuffingEstablishment>
      <Name>IORGANISATION1_NAME</Name>
      <Address>
        <CityName>IORGANISATION1_CITY</CityName>
        <CountryCode>IORGANISATION1_COUNTRYCODE</CountryCode>
        <Line>IORGANISATION1_ADDRESS</Line>
        <PostcodeID>IORGANISATION1_POSTCODE</PostcodeID>
      </Address>
    </StuffingEstablishment>
    <StuffingEstablishment>
      <Name>IORGANISATION2_NAME</Name>
      <Address>
        <CityName>IORGANISATION2_CITY</CityName>
        <CountryCode>IORGANISATION2_COUNTRYCODE</CountryCode>
        <Line>IORGANISATION2_ADDRESS</Line>
        <PostcodeID>IORGANISATION2_POSTCODE</PostcodeID>
      </Address>
    </StuffingEstablishment>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateReferenceNo()
		{
			SetUpMocks();
			icrBuilder = new ICRMessageBuilder(iInwardCargoReportMock.Object, TSWTransactionTypes.Replace, "00009908C");
			AssertContains("<ID>ENTRY12345</ID>", icrBuilder.GetXMLMessage());
		}

		public void TestPopulateMessageType()
		{
			SetUpMocks();
			AssertContains("<TypeCode>ICR</TypeCode>", icrBuilder.GetXMLMessage());
		}

		public void TestPopulateSendersRef()
		{
			SetUpMocks();
			AssertContains("<FunctionalReferenceID>C00001165</FunctionalReferenceID>", icrBuilder.GetXMLMessage());
		}

		public void TestPopulateTransType()
		{
			SetUpMocks();
			icrBuilder = new ICRMessageBuilder(iInwardCargoReportMock.Object, TSWTransactionTypes.Cancel, "00009908C");
			AssertContains("<FunctionCode>1</FunctionCode>", icrBuilder.GetXMLMessage());
			icrBuilder = new ICRMessageBuilder(iInwardCargoReportMock.Object, TSWTransactionTypes.Change, "00009908C");
			AssertContains("<FunctionCode>4</FunctionCode>", icrBuilder.GetXMLMessage());
			icrBuilder = new ICRMessageBuilder(iInwardCargoReportMock.Object, TSWTransactionTypes.Completion, "00009908C");
			AssertContains("<FunctionCode>22</FunctionCode>", icrBuilder.GetXMLMessage());
			icrBuilder = new ICRMessageBuilder(iInwardCargoReportMock.Object, TSWTransactionTypes.None, "00009908C");
			AssertContains("<FunctionCode />", icrBuilder.GetXMLMessage());
			icrBuilder = new ICRMessageBuilder(iInwardCargoReportMock.Object, TSWTransactionTypes.Original, "00009908C");
			AssertContains("<FunctionCode>9</FunctionCode>", icrBuilder.GetXMLMessage());
			icrBuilder = new ICRMessageBuilder(iInwardCargoReportMock.Object, TSWTransactionTypes.Replace, "00009908C");
			AssertContains("<FunctionCode>5</FunctionCode>", icrBuilder.GetXMLMessage());
		}

		public void TestPopulateSubmitter()
		{
			SetUpMocks();
			var expetedSubmitterXML = @"<Submitter>
    <ID>00009908C</ID>
  </Submitter>";
			AssertContains(expetedSubmitterXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalDocs()
		{
			SetUpMocks();
			var expectedAdditionalDocsXML = @"<AdditionalDocument>
    <CategoryCode>CDO</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""TEST1.PDF"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>
  <AdditionalDocument>
    <CategoryCode>INV</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""TEST2.PDF"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>";
			AssertContains(expectedAdditionalDocsXML, icrBuilder.GetXMLMessage());

			var tswAttachment1 = new Mock<ITSWAttachment>();
			var tswAttachment2 = new Mock<ITSWAttachment>();
			tswAttachment1.Setup(m => m.DocType).Returns("AAA");
			tswAttachment1.Setup(m => m.FileName).Returns("TEST11.PDF");
			tswAttachment2.Setup(m => m.DocType).Returns("BBB");
			tswAttachment2.Setup(m => m.FileName).Returns("TEST22.PDF");
			additionalInformationMock.Setup(m => m.SupportingDocuments).Returns(Array.Empty<ITSWAttachment>());
			iInwardCargoReportMock.Setup(m => m.SupportingDocuments).Returns(new ITSWAttachment[] { tswAttachment1.Object, tswAttachment2.Object });
			expectedAdditionalDocsXML = @"<AdditionalDocument>
    <CategoryCode>AAA</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""TEST11.PDF"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>
  <AdditionalDocument>
    <CategoryCode>BBB</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""TEST22.PDF"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>";
			AssertContains(expectedAdditionalDocsXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalMPIAccountDetails()
		{
			SetUpMocks();
			var expectedAdditionalMPIAccountDetailsXML = @"<AdditionalInformation>
    <StatementDescription>IINWARDCARGOREPORTMOCK_MPIACCOUNT</StatementDescription>
    <StatementTypeCode>MAC</StatementTypeCode>
  </AdditionalInformation>";
			AssertContains(expectedAdditionalMPIAccountDetailsXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalDocsWithNullAdditionalInformation()
		{
			SetUpMocks();
			var expectedAdditionalDocsXML = @"<AdditionalDocument>
    <CategoryCode>CDO</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""TEST1.PDF"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>
  <AdditionalDocument>
    <CategoryCode>INV</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""TEST2.PDF"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>";
			iInwardCargoReportMock.Reset();
			AssertNotContains(expectedAdditionalDocsXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalInfoOriginal()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
    <StatementCode>Y</StatementCode>
    <StatementTypeCode>CCR</StatementTypeCode>
  </AdditionalInformation>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
			additionalInformationMock.Reset();
			additionalInformationMock.Setup(m => m.FreeText).Returns(ZString.Empty);
			AssertNotContains("<Content>ADDITIONALINFORMATIONMOCK_FREETEXT</Content>", icrBuilder.GetXMLMessage());
			additionalInformationMock.Reset();
			additionalInformationMock.Setup(m => m.ManualOverrideText).Returns(ZString.Empty);
			expectedXML = @"<AdditionalInformation>
    <RequestOverrideCode>Y</RequestOverrideCode>
    <StatementDescription>ADDITIONALINFORMATIONMOCK_MANUALOVERRIDETEXT</StatementDescription>
    <StatementTypeCode>ALP</StatementTypeCode>
  </AdditionalInformation>";
			AssertNotContains(expectedXML, icrBuilder.GetXMLMessage());
			iInwardCargoReportMock.Reset();
			iInwardCargoReportMock.Setup(m => m.IsCarrierCargoReport).Returns(false);
			expectedXML = @"<AdditionalInformation>
    <StatementCode>Y</StatementCode>
    <StatementTypeCode>CCR</StatementTypeCode>
  </AdditionalInformation>";
			AssertNotContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalInfoReplace()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
    <Content>ADDITIONALINFORMATIONMOCK_FREETEXT</Content>
    <StatementDescription>ADDITIONALINFORMATIONMOCK_ADDITIONALSTATEMENTTEXT</StatementDescription>
    <StatementTypeCode>AES</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <RequestOverrideCode>Y</RequestOverrideCode>
    <StatementDescription>ADDITIONALINFORMATIONMOCK_MANUALOVERRIDETEXT</StatementDescription>
    <StatementTypeCode>ALP</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>Y</StatementCode>
    <StatementTypeCode>CCR</StatementTypeCode>
  </AdditionalInformation>";
			icrBuilder = new ICRMessageBuilder(iInwardCargoReportMock.Object, TSWTransactionTypes.Replace, "00009908C");
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
			additionalInformationMock.Reset();
			additionalInformationMock.Setup(m => m.FreeText).Returns(ZString.Empty);
			AssertNotContains("<Content>ADDITIONALINFORMATIONMOCK_FREETEXT</Content>", icrBuilder.GetXMLMessage());
			additionalInformationMock.Reset();
			additionalInformationMock.Setup(m => m.ManualOverrideText).Returns(ZString.Empty);
			expectedXML = @"<AdditionalInformation>
    <RequestOverrideCode>Y</RequestOverrideCode>
    <StatementDescription>ADDITIONALINFORMATIONMOCK_MANUALOVERRIDETEXT</StatementDescription>
    <StatementTypeCode>ALP</StatementTypeCode>
  </AdditionalInformation>";
			AssertNotContains(expectedXML, icrBuilder.GetXMLMessage());
			iInwardCargoReportMock.Reset();
			iInwardCargoReportMock.Setup(m => m.IsCarrierCargoReport).Returns(false);
			expectedXML = @"<AdditionalInformation>
    <StatementCode>Y</StatementCode>
    <StatementTypeCode>CCR</StatementTypeCode>
  </AdditionalInformation>";
			AssertNotContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalInfoCancel()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
    <StatementDescription>ADDITIONALINFORMATIONMOCK_ADDITIONALSTATEMENTTEXT</StatementDescription>
    <StatementTypeCode>AES</StatementTypeCode>
  </AdditionalInformation>";
			icrBuilder = new ICRMessageBuilder(iInwardCargoReportMock.Object, TSWTransactionTypes.Cancel, "00009908C");
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
			additionalInformationMock.Reset();
			additionalInformationMock.Setup(m => m.FreeText).Returns(ZString.Empty);
			AssertNotContains("<Content>ADDITIONALINFORMATIONMOCK_FREETEXT</Content>", icrBuilder.GetXMLMessage());
			additionalInformationMock.Reset();
			additionalInformationMock.Setup(m => m.ManualOverrideText).Returns(ZString.Empty);
			expectedXML = @"<AdditionalInformation>
    <RequestOverrideCode>Y</RequestOverrideCode>
    <StatementDescription>ADDITIONALINFORMATIONMOCK_MANUALOVERRIDETEXT</StatementDescription>
    <StatementTypeCode>ALP</StatementTypeCode>
  </AdditionalInformation>";
			AssertNotContains(expectedXML, icrBuilder.GetXMLMessage());
			expectedXML = @"<AdditionalInformation>
    <StatementCode>Y</StatementCode>
    <StatementTypeCode>CCR</StatementTypeCode>
  </AdditionalInformation>";
			iInwardCargoReportMock.Reset();
			iInwardCargoReportMock.Setup(m => m.IsCarrierCargoReport).Returns(false);
			AssertNotContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateBorderTMSea()
		{
			SetUpMocks();
			var expectedBorderTransportMeansXML = @"<BorderTransportMeans>
    <Name>CRAFTNAME</Name>
    <ID>LLOYDSNO</ID>
    <TypeCode>1</TypeCode>
    <ArrivalDateTime formatCode=""102"">20181022</ArrivalDateTime>
    <FirstArrivalLocationID>PORTOFARRIVAL</FirstArrivalLocationID>
    <JourneyID>VOYAGENO</JourneyID>
  </BorderTransportMeans>";
			AssertContains(expectedBorderTransportMeansXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateBorderTMAir()
		{
			SetUpMocks();
			var expectedBorderTransportMeansXML = @"<BorderTransportMeans>
    <Name>FLIGHTNO</Name>
    <TypeCode>4</TypeCode>
    <ArrivalDateTime formatCode=""102"">20181022</ArrivalDateTime>
    <FirstArrivalLocationID>PORTOFARRIVAL</FirstArrivalLocationID>
  </BorderTransportMeans>";
			iInwardCargoReportMock.Setup(m => m.IsSea).Returns(false);
			AssertContains(expectedBorderTransportMeansXML, icrBuilder.GetXMLMessage());
		}

		public void TestFlightNoIsCapitalized()
		{
			SetUpMocks();
			iInwardCargoReportMock.Setup(m => m.IsSea).Returns(false);
			iInwardCargoReportMock.Setup(m => m.FlightNo).Returns("nz119");
			iInwardCargoReportMock.Setup(m => m.ArrivalDate).Returns(new ZDateTime(2025, 05, 06));
			var expectedBorderTransportMeansXML = @"<BorderTransportMeans>
    <Name>NZ119</Name>
    <TypeCode>4</TypeCode>
    <ArrivalDateTime formatCode=""102"">20250506</ArrivalDateTime>
    <FirstArrivalLocationID>PORTOFARRIVAL</FirstArrivalLocationID>
  </BorderTransportMeans>";
			AssertContains("Flight Number should be in Uppercase", expectedBorderTransportMeansXML, icrBuilder.GetXMLMessage());
		}

		public void TestCraftAndVoyageNumberIsCapitalized()
		{
			SetUpMocks();
			iInwardCargoReportMock.Setup(m => m.CraftName).Returns("Long March");
			iInwardCargoReportMock.Setup(m => m.LloydsNo).Returns("5892347");
			iInwardCargoReportMock.Setup(m => m.VoyageNo).Returns("152e");
			iInwardCargoReportMock.Setup(m => m.ArrivalDate).Returns(new ZDateTime(2025, 05, 06));
			iInwardCargoReportMock.Setup(m => m.PortOfArrival).Returns("NZAKL");
			var expectedBorderTransportMeansXML = @"<BorderTransportMeans>
    <Name>LONG MARCH</Name>
    <ID>5892347</ID>
    <TypeCode>1</TypeCode>
    <ArrivalDateTime formatCode=""102"">20250506</ArrivalDateTime>
    <FirstArrivalLocationID>NZAKL</FirstArrivalLocationID>
    <JourneyID>152E</JourneyID>
  </BorderTransportMeans>";
			AssertContains(expectedBorderTransportMeansXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateCarrier()
		{
			SetUpMocks();
			var expectedXML = @"<Carrier>
    <ID>51358595A</ID>
  </Carrier>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
			iOrganisationSimpleMock.Setup(m => m.CustomsClientCode).Returns(ZString.Empty);
			expectedXML = @"<Carrier>
    <Name>IORGANISATIONSIMPLEMOCK_NAME</Name>
  </Carrier>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentsSea()
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
			additionalInformationMock.Setup(m => m.AdditionalStatementText).Returns("ADDITIONALINFORMATIONMOCK_ADDITIONALSTATEMENTTEXT");

			iOrganisationSimpleMock = new Mock<IOrganisationSimple>();
			iOrganisationSimpleMock.Setup(m => m.Name).Returns("IORGANISATIONSIMPLEMOCK_NAME");
			iOrganisationSimpleMock.Setup(m => m.CustomsClientCode).Returns("51358595A");

			var consignment1ITRDetails = new Mock<ITranshipmentDetails>();
			consignment1ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment1ITRDetails.Setup(m => m.DomesticTranshipmentRequest).Returns(false);
			consignment1ITRDetails.Setup(m => m.ITRImportMode).Returns("4");
			consignment1ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("NZ18");
			consignment1ITRDetails.Setup(m => m.ITRDepartureDate).Returns(new ZDateTime(2019, 02, 18));
			consignment1ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("4");
			consignment1ITRDetails.Setup(m => m.ITRImportCraft).Returns("");
			consignment1ITRDetails.Setup(m => m.PremiseCode).Returns("CONSIGNMENT1_PREMISECODE");

			consignment2ITRDetails = new Mock<ITranshipmentDetails>();
			consignment2ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment2ITRDetails.Setup(m => m.DomesticTranshipmentRequest).Returns(false);
			consignment2ITRDetails.Setup(m => m.ITRImportMode).Returns("1");
			consignment2ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("55W");
			consignment2ITRDetails.Setup(m => m.ITRDepartureDate).Returns(new ZDateTime(2019, 02, 22));
			consignment2ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("1C");
			consignment2ITRDetails.Setup(m => m.ITRImportCraft).Returns("AAL FREMANTLE");
			consignment2ITRDetails.Setup(m => m.PremiseCode).Returns("CONSIGNMENT2_PREMISECODE");

			var iCommunication1 = new Mock<ICommunication>();
			iCommunication1.Setup(m => m.ContactDetail).Returns("COMMUNICATION1_CONTACTDETAIL");
			iCommunication1.Setup(m => m.ContactType).Returns("COMMUNICATION1_CONTACTTYPE");

			var iCommunication2 = new Mock<ICommunication>();
			iCommunication2.Setup(m => m.ContactDetail).Returns("COMMUNICATION2_CONTACTDETAIL");
			iCommunication2.Setup(m => m.ContactType).Returns("COMMUNICATION2_CONTACTTYPE");

			var consignee1 = new Mock<IPartyInformation>();
			consignee1.Setup(m => m.CustomsClientCode).Returns("CONSIGNEE_CUSTOMSCLIENTCODE");
			consignee1.Setup(m => m.Name).Returns("CONSIGNEE_NAME");
			consignee1.Setup(m => m.City).Returns("CONSIGNEE_CITY");
			consignee1.Setup(m => m.CountryCode).Returns("CONSIGNEE_COUNTRYCODE");
			consignee1.Setup(m => m.CountryRegion).Returns("CONSIGNEE_COUNTRYREGION");
			consignee1.Setup(m => m.Address).Returns("CONSIGNEE_ADDRESS");
			consignee1.Setup(m => m.PostCode).Returns("CONSIGNEE_POSTCODE");
			consignee1.Setup(m => m.Communications).Returns(new List<ICommunication>()
			{ iCommunication1.Object, iCommunication2.Object });

			var consignee2 = new Mock<IPartyInformation>();
			consignee2.Setup(m => m.CustomsClientCode).Returns("");
			consignee2.Setup(m => m.Name).Returns("CONSIGNEE2_NAME");
			consignee2.Setup(m => m.City).Returns("CONSIGNEE2_CITY");
			consignee2.Setup(m => m.CountryCode).Returns("CONSIGNEE2_COUNTRYCODE");
			consignee2.Setup(m => m.CountryRegion).Returns("CONSIGNEE2_COUNTRYREGION");
			consignee2.Setup(m => m.Address).Returns("CONSIGNEE2_ADDRESS");
			consignee2.Setup(m => m.PostCode).Returns("CONSIGNEE2_POSTCODE");
			consignee2.Setup(m => m.Communications).Returns(new List<ICommunication>()
			{ iCommunication1.Object, iCommunication2.Object });

			var iDeclarant = new Mock<IDeclarant>();
			iDeclarant.Setup(m => m.DeclarantID).Returns("DECLARANTID");
			iDeclarant.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var iContact1 = new Mock<IContact>();
			iContact1.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var iContact2 = new Mock<IContact>();
			iContact2.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			iOrganisation1 = new Mock<IOrganisation>();
			iOrganisation1.Setup(m => m.Name).Returns("IORGANISATION1_NAME");
			iOrganisation1.Setup(m => m.CustomsClientCode).Returns("IORGANISATION1_CUSTOMSCLIENTCODE");
			iOrganisation1.Setup(m => m.City).Returns("IORGANISATION1_CITY");
			iOrganisation1.Setup(m => m.CountryCode).Returns("IORGANISATION1_COUNTRYCODE");
			iOrganisation1.Setup(m => m.CountryRegion).Returns("IORGANISATION1_COUNTRYREGION");
			iOrganisation1.Setup(m => m.Address).Returns("IORGANISATION1_ADDRESS");
			iOrganisation1.Setup(m => m.PostCode).Returns("IORGANISATION1_POSTCODE");
			iOrganisation1.Setup(m => m.Communications).Returns(new List<ICommunication>()
			{ iCommunication1.Object, iCommunication2.Object });
			iOrganisation1.Setup(m => m.Contacts).Returns(new List<IContact>()
			{ iContact1.Object, iContact2.Object });

			var iOrganisation2 = new Mock<IOrganisation>();
			iOrganisation2.Setup(m => m.Name).Returns("IORGANISATION2_NAME");
			iOrganisation2.Setup(m => m.CustomsClientCode).Returns("IORGANISATION2_CUSTOMSCLIENTCODE");
			iOrganisation2.Setup(m => m.City).Returns("IORGANISATION2_CITY");
			iOrganisation2.Setup(m => m.CountryCode).Returns("IORGANISATION2_COUNTRYCODE");
			iOrganisation2.Setup(m => m.Address).Returns("IORGANISATION2_ADDRESS");
			iOrganisation2.Setup(m => m.PostCode).Returns("IORGANISATION2_POSTCODE");
			iOrganisation2.Setup(m => m.Communications).Returns(new List<ICommunication>()
			{ iCommunication1.Object, iCommunication2.Object });
			iOrganisation2.Setup(m => m.Contacts).Returns(new List<IContact>()
			{ iContact1.Object, iContact2.Object });

			var notify = new Mock<IPartyInformation>();
			notify.Setup(m => m.CustomsClientCode).Returns("NOTIFY_CUSTOMSCLIENTCODE");
			notify.Setup(m => m.Name).Returns("NOTIFY_NAME");
			notify.Setup(m => m.City).Returns("NOTIFY_CITY");
			notify.Setup(m => m.CountryCode).Returns("NOTIFY_COUNTRYCODE");
			notify.Setup(m => m.CountryRegion).Returns("NOTIFY_COUNTRYREGION");
			notify.Setup(m => m.Address).Returns("NOTIFY_ADDRESS");
			notify.Setup(m => m.PostCode).Returns("NOTIFY_POSTCODE");
			notify.Setup(m => m.Communications).Returns(new List<ICommunication>()
			{ iCommunication1.Object, iCommunication2.Object });

			var consignor = new Mock<IPartyInformation>();
			consignor.Setup(m => m.CustomsClientCode).Returns("CONSIGNOR_CUSTOMSCLIENTCODE");
			consignor.Setup(m => m.Name).Returns("CONSIGNOR_NAME");
			consignor.Setup(m => m.City).Returns("CONSIGNOR_CITY");
			consignor.Setup(m => m.CountryCode).Returns("CONSIGNOR_COUNTRYCODE");
			consignor.Setup(m => m.CountryRegion).Returns("CONSIGNOR_COUNTRYREGION");
			consignor.Setup(m => m.Address).Returns("CONSIGNOR_ADDRESS");
			consignor.Setup(m => m.PostCode).Returns("CONSIGNOR_POSTCODE");
			consignor.Setup(m => m.Communications).Returns(new List<ICommunication>()
			{ iCommunication1.Object, iCommunication2.Object });

			var iTemperatureRequirements = new Mock<ITemperatureRequirements>();
			iTemperatureRequirements.Setup(m => m.StorageTemp).Returns(1m);
			iTemperatureRequirements.Setup(m => m.MinStorageTemp).Returns(2m);
			iTemperatureRequirements.Setup(m => m.MaxStorageTemp).Returns(3m);

			var classifications1 = new Mock<IClassification>();
			classifications1.Setup(m => m.Classification).Returns("CLASSIFICATIONS1_CLASSIFICATION");
			classifications1.Setup(m => m.ClassificationTypeCode).Returns("SSO");

			var classifications2 = new Mock<IClassification>();
			classifications2.Setup(m => m.Classification).Returns("CLASSIFICATIONS2_CLASSIFICATION");
			classifications2.Setup(m => m.ClassificationTypeCode).Returns("SSI");

			var iTransportEquipment1 = new Mock<ITransportEquipment>();
			iTransportEquipment1.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSIZE");
			iTransportEquipment1.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT1_CONTAINERNUMBER");
			iTransportEquipment1.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSTATUS");
			iTransportEquipment1.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT1_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment1.Setup(m => m.StowPosition).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSTOWPOSITION");
			iTransportEquipment1.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "ITRANSPORTEQUIPMENT1_SEALNUMBER1", "ITRANSPORTEQUIPMENT1_SEALNUMBER2" });
			iTransportEquipment1.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			iTransportEquipment1.Setup(m => m.StuffingLocation).Returns(ZGuid.NewZGuid());
			iTransportEquipment1.Setup(m => m.MessageSequence).Returns(1);

			var iTransportEquipment2 = new Mock<ITransportEquipment>();
			iTransportEquipment2.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT2_CONTAINERSIZE");
			iTransportEquipment2.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT2_CONTAINERNUMBER");
			iTransportEquipment2.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT2_CONTAINERSTATUS");
			iTransportEquipment2.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT2_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment2.Setup(m => m.StowPosition).Returns("ITRANSPORTEQUIPMENT2_CONTINERSTOWPOSITION");
			iTransportEquipment2.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "ITRANSPORTEQUIPMENT2_SEALNUMBER1", "ITRANSPORTEQUIPMENT2_SEALNUMBER2" });
			iTransportEquipment2.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			iTransportEquipment2.Setup(m => m.StuffingLocation).Returns(ZGuid.NewZGuid());
			iTransportEquipment2.Setup(m => m.MessageSequence).Returns(2);

			var consignmentItem1 = new Mock<IICRConsignmentItem>();
			consignmentItem1.Setup(m => m.SendFlashpointTemp).Returns(true);
			consignmentItem1.Setup(m => m.FlashpointTempInCelsius).Returns(1m);
			consignmentItem1.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM1_12345ABCD");
			consignmentItem1.Setup(m => m.Value).Returns(2m);
			consignmentItem1.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.Australia);
			consignmentItem1.Setup(m => m.IdentityNumber).Returns("CONSIGNMENTITEM1_IDENTITYNUMBER");
			consignmentItem1.Setup(m => m.IdentityType).Returns("CONSIGNMENTITEM1_IDENTITYTYPE");
			consignmentItem1.Setup(m => m.GrossWeightInKg).Returns(3m);
			consignmentItem1.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM1_GOODSORIGINCOUNTRY");
			consignmentItem1.Setup(m => m.PackageQty).Returns(2);
			consignmentItem1.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM1_PACKAGETYPE");
			consignmentItem1.Setup(m => m.ContainerNumber).Returns("CONSIGNMENTITEM1_CONTAINERNUMBER");
			consignmentItem1.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem1.Setup(m => m.SequenceNumber).Returns((ZShort)1);

			var consignmentItem2 = new Mock<IICRConsignmentItemWithExtraInfos>();
			consignmentItem2.Setup(m => m.SendFlashpointTemp).Returns(true);
			consignmentItem2.Setup(m => m.FlashpointTempInCelsius).Returns(4m);
			consignmentItem2.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM2_12345ABCD");
			consignmentItem2.Setup(m => m.Value).Returns(5m);
			consignmentItem2.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.UnitedStates);
			consignmentItem2.Setup(m => m.IdentityNumber).Returns("CONSIGNMENTITEM2_IDENTITYNUMBER");
			consignmentItem2.Setup(m => m.IdentityType).Returns("CONSIGNMENTITEM2_IDENTITYTYPE");
			consignmentItem2.Setup(m => m.GrossWeightInKg).Returns(6m);
			consignmentItem2.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM2_GOODSORIGINCOUNTRY");
			consignmentItem2.Setup(m => m.PackageQty).Returns(2);
			consignmentItem2.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM2_PACKAGETYPE");
			consignmentItem2.Setup(m => m.ContainerNumber).Returns("CONSIGNMENTITEM2_CONTAINERNUMBER");
			consignmentItem2.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem2.Setup(m => m.SequenceNumber).Returns((ZShort)2);

			consignmentItem1.Setup(m => m.Classifications).Returns(new List<IClassification>()
			{ classifications1.Object, classifications2.Object });
			consignmentItem2.Setup(m => m.Classifications).Returns(new List<IClassification>()
			{ classifications1.Object, classifications2.Object });
			consignmentItem1.Setup(m => m.Temperatures).Returns(iTemperatureRequirements.Object);
			consignmentItem2.Setup(m => m.Temperatures).Returns(iTemperatureRequirements.Object);

			var consignment1 = new Mock<IICRConsignment>();
			consignment1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignment1.Setup(m => m.ConsignmentValueInNZD).Returns(5.123);
			consignment1.Setup(m => m.Permits).Returns(new List<ZString>() { "CONSIGNMENT1_PERMITS1", "CONSIGNMENT1_PERMITS2", ZString.Empty });
			consignment1.Setup(m => m.WriteOffRequest).Returns(true);
			consignment1.Setup(m => m.TranshipmentDetails).Returns(consignment1ITRDetails.Object);
			consignment1.Setup(m => m.IsConsolidation).Returns(true);
			consignment1.Setup(m => m.MAFContainerDeclaration).Returns(true);
			consignment1.Setup(m => m.MAFContainerStatements).Returns(new List<ZString>() { "CONSIGNMENT1_MAFCONTAINERSTATEMENT1", "CONSIGNMENT1_MAFCONTAINERSTATEMENT2" });
			consignment1.Setup(m => m.MPIApprovedSystemNumbers).Returns(new List<ZString>() { "CONSIGNMENT1_MPIApprovedSystemNumber1", "CONSIGNMENT1_MPIApprovedSystemNumber2" });
			consignment1.Setup(m => m.MPIAccountDetails).Returns("CONSIGNMENT1_MPIACCOUNT");
			consignment1.Setup(m => m.HandlingInformation).Returns("CONSIGNMENT1_HandlingInfo");
			consignment1.Setup(m => m.MasterBill).Returns("CONSIGNMENT1_MASTERBILL");
			consignment1.Setup(m => m.PortOfOrigin).Returns("CONSIGNMENT1_PORTOFORIGIN");
			consignment1.Setup(m => m.GoodsLocation).Returns("CONSIGNMENT1_GOODSLOCATION");
			consignment1.Setup(m => m.PortOfLoading).Returns("CONSIGNMENT1_PORTOFLOADING");
			consignment1.Setup(m => m.BillNumber).Returns("CONSIGNMENT1_BILLNUMBER");
			consignment1.Setup(m => m.BillType).Returns("CONSIGNMENT1_BILLTYPE");
			consignment1.Setup(m => m.PortOfDischarge).Returns("CONSIGNMENT1_PORTOFDISCHARGE");
			consignment1.Setup(m => m.FreightPaymentMethod).Returns("CONSIGNMENT1_FREIGHTPAYMENTMETHOD");
			consignment1.Setup(m => m.Deconsolidator).Returns(iOrganisationSimpleMock.Object);
			consignment1.Setup(m => m.ContainerPackingLocations).Returns(new List<IOrganisation>() { iOrganisation1.Object, iOrganisation2.Object });
			consignment1.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment1.Setup(m => m.TranshipmentPorts).Returns(new List<ZString>() { "CONSIGNMENT1_TRANSHIPMENTPORTS1", "CONSIGNMENT1_TRANSHIPMENTPORTS2", ZString.Empty });
			consignment1.Setup(m => m.IsGSTPrePaid).Returns("Y");
			consignment1.Setup(m => m.VendorIdentifier).Returns("1234567");
			consignment1.Setup(m => m.IsLinkEmptyContainer).Returns(false);
			consignment1.Setup(m => m.ApprovedTransitionalFacilityCode).Returns("090500");
			consignment1.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.False);
			consignment1.Setup(m => m.DeliveryNotifyParties).Returns(new List<IOrganisationSimple> { iOrganisation1.Object, iOrganisation2.Object });
			consignment1.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "11111A", "PYKE" });
			consignment1.Setup(m => m.Consignee).Returns(consignee1.Object);
			consignment1.Setup(m => m.Consignor).Returns(consignor.Object);
			consignment1.Setup(m => m.NotifyParty).Returns(notify.Object);
			consignment1.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { iTransportEquipment1.Object, iTransportEquipment2.Object });
			consignment1.Setup(m => m.HasContainers).Returns(true);
			consignment1.Setup(m => m.ConsignmentItems).Returns(new List<IICRConsignmentItem>() { consignmentItem1.Object, consignmentItem2.Object });

			var consignment2 = new Mock<IICRConsignmentWithExtraInfos>();
			consignment2.Setup(m => m.SequenceNumber).Returns((ZShort)2);
			consignment2.Setup(m => m.ConsignmentValueInNZD).Returns(6.2);
			consignment2.Setup(m => m.Permits).Returns(new List<ZString>() { "CONSIGNMENT2_PERMITS1", "CONSIGNMENT2_PERMITS2" });
			consignment2.Setup(m => m.WriteOffRequest).Returns(true);
			consignment2.Setup(m => m.TranshipmentDetails).Returns(consignment2ITRDetails.Object);
			consignment2.Setup(m => m.IsConsolidation).Returns(true);
			consignment2.Setup(m => m.MAFContainerDeclaration).Returns(true);
			consignment2.Setup(m => m.MAFContainerStatements).Returns(new List<ZString>() { "CONSIGNMENT2_MAFCONTAINERSTATEMENT1", "CONSIGNMENT2_MAFCONTAINERSTATEMENT2" });
			consignment2.Setup(m => m.MPIApprovedSystemNumbers).Returns(new List<ZString>() { "CONSIGNMENT2_MPIApprovedSystemNumber1", "CONSIGNMENT2_MPIApprovedSystemNumber2" });
			consignment2.Setup(m => m.MPIAccountDetails).Returns("");
			consignment2.Setup(m => m.HandlingInformation).Returns("");
			consignment2.Setup(m => m.MasterBill).Returns("CONSIGNMENT2_MASTERBILL");
			consignment2.Setup(m => m.PortOfOrigin).Returns("CONSIGNMENT2_PORTOFORIGIN");
			consignment2.Setup(m => m.GoodsLocation).Returns("CONSIGNMENT2_GOODSLOCATION");
			consignment2.Setup(m => m.PortOfLoading).Returns("CONSIGNMENT2_PORTOFLOADING");
			consignment2.Setup(m => m.BillNumber).Returns("CONSIGNMENT2_BILLNUMBER");
			consignment2.Setup(m => m.BillType).Returns("CONSIGNMENT2_BILLTYPE");
			consignment2.Setup(m => m.PortOfDischarge).Returns("CONSIGNMENT2_PORTOFDISCHARGE");
			consignment2.Setup(m => m.FreightPaymentMethod).Returns("CONSIGNMENT2_FREIGHTPAYMENTMETHOD");
			consignment2.Setup(m => m.Deconsolidator).Returns(iOrganisationSimpleMock.Object);
			consignment2.Setup(m => m.ContainerPackingLocations).Returns(new List<IOrganisation>() { iOrganisation1.Object, iOrganisation2.Object });
			consignment2.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment2.Setup(m => m.TranshipmentPorts).Returns(new List<ZString>() { "CONSIGNMENT2_TRANSHIPMENTPORTS1", "CONSIGNMENT2_TRANSHIPMENTPORTS2" });
			consignment2.Setup(m => m.IsGSTPrePaid).Returns("N");
			consignment2.Setup(m => m.VendorIdentifier).Returns("2345678");
			consignment2.Setup(m => m.IsLinkEmptyContainer).Returns(false);
			consignment2.Setup(m => m.ApprovedTransitionalFacilityCode).Returns("090501");
			consignment2.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.False);
			consignment2.Setup(m => m.DeliveryNotifyParties).Returns(new List<IOrganisationSimple> { iOrganisation1.Object, iOrganisation2.Object });
			consignment2.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "22222B", "TARTH" });
			consignment2.Setup(m => m.Consignee).Returns(consignee2.Object);
			consignment2.Setup(m => m.Consignor).Returns(consignor.Object);
			consignment2.Setup(m => m.NotifyParty).Returns(notify.Object);
			consignment2.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { iTransportEquipment1.Object, iTransportEquipment2.Object });
			consignment2.Setup(m => m.HasContainers).Returns(true);
			consignment2.Setup(m => m.ConsignmentItems).Returns(new List<IICRConsignmentItem>() { consignmentItem1.Object, consignmentItem2.Object });

			var iInwardCargoReportMock = new Mock<IInwardCargoReport>();
			iInwardCargoReportMock.Setup(m => m.TSWReferenceNumber).Returns("ENTRY12345");
			iInwardCargoReportMock.Setup(m => m.SenderReferenceNumber).Returns("C00001165");
			iInwardCargoReportMock.Setup(m => m.IsCarrierCargoReport).Returns(true);
			iInwardCargoReportMock.Setup(m => m.IsSea).Returns(true);
			iInwardCargoReportMock.Setup(m => m.CraftName).Returns("CRAFTNAME");
			iInwardCargoReportMock.Setup(m => m.FlightNo).Returns("FLIGHTNO");
			iInwardCargoReportMock.Setup(m => m.ArrivalDate).Returns(new ZDateTime(2018, 10, 22));
			iInwardCargoReportMock.Setup(m => m.PortOfArrival).Returns("PORTOFARRIVAL");
			iInwardCargoReportMock.Setup(m => m.LloydsNo).Returns("LLOYDSNO");
			iInwardCargoReportMock.Setup(m => m.VoyageNo).Returns("VOYAGENO");
			iInwardCargoReportMock.Setup(m => m.MPIAccountDetails).Returns("IINWARDCARGOREPORTMOCK_MPIACCOUNT");
			iInwardCargoReportMock.Setup(m => m.AdditionalInformation).Returns(additionalInformationMock.Object);
			iInwardCargoReportMock.Setup(m => m.Carrier).Returns(iOrganisationSimpleMock.Object);
			iInwardCargoReportMock.Setup(m => m.UseInterfaceSequenceNumber).Returns(true);
			iInwardCargoReportMock.Setup(m => m.Declarant).Returns(iDeclarant.Object);
			iInwardCargoReportMock.Setup(m => m.Consignments).Returns(new List<IICRConsignment>() { consignment1.Object, consignment2.Object });

			icrBuilder = new ICRMessageBuilder(iInwardCargoReportMock.Object, TSWTransactionTypes.Original, "00009908C");
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestICRPopulateConsignmentsSea.txt")), icrBuilder.GetXMLMessage());

			consignment1.Setup(m => m.MasterBill).Returns(ZString.Empty);
			consignment2.Setup(m => m.MasterBill).Returns(ZString.Empty);
			var unexpectedXML = @"<AssociatedTransportDocument>
      <ID>CONSIGNMENT1_MASTERBILL</ID>
      <TypeCode>MB</TypeCode>
    </AssociatedTransportDocument>";
			AssertNotContains(unexpectedXML, icrBuilder.GetXMLMessage());
			consignment1.Setup(m => m.DeliverToParty);
			consignment2.Setup(m => m.DeliverToParty);
			unexpectedXML = @"<DeliveryDestination>
      <Name>IORGANISATION1_NAME</Name>
      <Address>
        <CityName>IORGANISATION1_CITY</CityName>
        <CountryCode>IORGANISATION1_COUNTRYCODE</CountryCode>
        <Line>IORGANISATION1_ADDRESS</Line>
        <PostcodeID>IORGANISATION1_POSTCODE</PostcodeID>
      </Address>
    </DeliveryDestination>";
			AssertNotContains(unexpectedXML, icrBuilder.GetXMLMessage());
			iOrganisation1.Setup(m => m.Name).Returns(consignee1.Object.Name);
			consignment1.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment2.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			AssertNotContains(unexpectedXML, icrBuilder.GetXMLMessage());
			consignment1.Setup(m => m.GoodsLocation).Returns(ZString.Empty);
			consignment2.Setup(m => m.GoodsLocation);
			consignment2.Setup(m => m.GoodsLocation).Returns(ZString.Empty);
			unexpectedXML = @"<GoodsLocation>
      <ID>CONSIGNMENT1_GOODSLOCATION</ID>
    </GoodsLocation>";
			AssertNotContains(unexpectedXML, icrBuilder.GetXMLMessage());
			consignment1ITRDetails = new Mock<ITranshipmentDetails>();
			consignment1ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment1ITRDetails.Setup(m => m.DomesticTranshipmentRequest).Returns(false);
			consignment1ITRDetails.Setup(m => m.ITRImportMode).Returns("4");
			consignment1ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("NZ18");
			consignment1ITRDetails.Setup(m => m.ITRDepartureDate).Returns(new ZDateTime(2019, 02, 18));
			consignment1ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("4");
			consignment1ITRDetails.Setup(m => m.ITRImportCraft).Returns("");
			consignment1ITRDetails.Setup(m => m.PremiseCode).Returns("");
			consignment1.Setup(m => m.TranshipmentDetails).Returns(consignment1ITRDetails.Object);
			unexpectedXML = @"<ID>CONSIGNMENT1_PREMISECODE</ID>";
			AssertNotContains(unexpectedXML, icrBuilder.GetXMLMessage());
			consignment1.Setup(m => m.Containers).Returns(Enumerable.Empty<ITransportEquipment>());
			consignment2.Setup(m => m.Containers).Returns(Enumerable.Empty<ITransportEquipment>());
			icrBuilder = new ICRMessageBuilder(iInwardCargoReportMock.Object, TSWTransactionTypes.Original, "00009908C"); //TSWMessage Builder Caches the Container Details
			AssertNotContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestICRPopulateTransportEquipment.txt")), icrBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentsAir()
		{
			var tswAttachment1 = new Mock<ITSWAttachment>();
			tswAttachment1.Setup(m => m.DocType).Returns("CDO");
			tswAttachment1.Setup(m => m.FileName).Returns("TEST1.PDF");

			var tswAttachment2 = new Mock<ITSWAttachment>();
			tswAttachment2.Setup(m => m.DocType).Returns("INV");
			tswAttachment2.Setup(m => m.FileName).Returns("TEST2.PDF");

			var additionalInformationMock = new Mock<IAdditionalInformation>();
			additionalInformationMock.Setup(m => m.SupportingDocuments).Returns(new List<ITSWAttachment>() { tswAttachment1.Object, tswAttachment2.Object });
			additionalInformationMock.Setup(m => m.FreeText).Returns("ADDITIONALINFORMATIONMOCK_FREETEXT");
			additionalInformationMock.Setup(m => m.ManualOverrideText).Returns("ADDITIONALINFORMATIONMOCK_MANUALOVERRIDETEXT");
			additionalInformationMock.Setup(m => m.AdditionalStatementText).Returns("ADDITIONALINFORMATIONMOCK_ADDITIONALSTATEMENTTEXT");

			var iOrganisationSimpleMock = new Mock<IOrganisationSimple>();
			iOrganisationSimpleMock.Setup(m => m.Name).Returns("IORGANISATIONSIMPLEMOCK_NAME");
			iOrganisationSimpleMock.Setup(m => m.CustomsClientCode).Returns("51358595A");

			var iCommunication1 = new Mock<ICommunication>();
			iCommunication1.Setup(m => m.ContactDetail).Returns("COMMUNICATION1_CONTACTDETAIL");
			iCommunication1.Setup(m => m.ContactType).Returns("COMMUNICATION1_CONTACTTYPE");

			var iCommunication2 = new Mock<ICommunication>();
			iCommunication2.Setup(m => m.ContactDetail).Returns("COMMUNICATION2_CONTACTDETAIL");
			iCommunication2.Setup(m => m.ContactType).Returns("COMMUNICATION2_CONTACTTYPE");

			var iContact1 = new Mock<IContact>();
			iContact1.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var iContact2 = new Mock<IContact>();
			iContact2.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var iOrganisation1 = new Mock<IOrganisation>();
			iOrganisation1.Setup(m => m.Name).Returns("IORGANISATION1_NAME");
			iOrganisation1.Setup(m => m.City).Returns("IORGANISATION1_CITY");
			iOrganisation1.Setup(m => m.CountryCode).Returns("IORGANISATION1_COUNTRYCODE");
			iOrganisation1.Setup(m => m.Address).Returns("IORGANISATION1_ADDRESS");
			iOrganisation1.Setup(m => m.PostCode).Returns("IORGANISATION1_POSTCODE");
			iOrganisation1.Setup(m => m.CustomsClientCode).Returns("IORGANISATION1_CUSTOMSCLIENTCODE");
			iOrganisation1.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });
			iOrganisation1.Setup(m => m.Contacts).Returns(new List<IContact>() { iContact1.Object, iContact2.Object });

			var iOrganisation2 = new Mock<IOrganisation>();
			iOrganisation2.Setup(m => m.Name).Returns("IORGANISATION2_NAME");
			iOrganisation2.Setup(m => m.City).Returns("IORGANISATION2_CITY");
			iOrganisation2.Setup(m => m.CountryCode).Returns("IORGANISATION2_COUNTRYCODE");
			iOrganisation2.Setup(m => m.Address).Returns("IORGANISATION2_ADDRESS");
			iOrganisation2.Setup(m => m.PostCode).Returns("IORGANISATION2_POSTCODE");
			iOrganisation2.Setup(m => m.CustomsClientCode).Returns("IORGANISATION2_CUSTOMSCLIENTCODE");
			iOrganisation2.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });
			iOrganisation2.Setup(m => m.Contacts).Returns(new List<IContact>() { iContact1.Object, iContact2.Object });

			var consignment1ITRDetails = new Mock<ITranshipmentDetails>();
			consignment1ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment1ITRDetails.Setup(m => m.DomesticTranshipmentRequest).Returns(false);
			consignment1ITRDetails.Setup(m => m.ITRImportMode).Returns("4");
			consignment1ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("NZ18");
			consignment1ITRDetails.Setup(m => m.ITRDepartureDate).Returns(new ZDateTime(2019, 02, 18));
			consignment1ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("4");
			consignment1ITRDetails.Setup(m => m.ITRImportCraft).Returns("");
			consignment1ITRDetails.Setup(m => m.PremiseCode).Returns("CONSIGNMENT1_PREMISECODE");

			var consignment2ITRDetails = new Mock<ITranshipmentDetails>();
			consignment2ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment2ITRDetails.Setup(m => m.DomesticTranshipmentRequest).Returns(false);
			consignment2ITRDetails.Setup(m => m.ITRImportMode).Returns("1");
			consignment2ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("55W");
			consignment2ITRDetails.Setup(m => m.ITRDepartureDate).Returns(new ZDateTime(2019, 02, 22));
			consignment2ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("1C");
			consignment2ITRDetails.Setup(m => m.ITRImportCraft).Returns("AAL FREMANTLE");
			consignment2ITRDetails.Setup(m => m.PremiseCode).Returns("CONSIGNMENT2_PREMISECODE");

			var iTransportEquipment1 = new Mock<ITransportEquipment>();
			iTransportEquipment1.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSIZE");
			iTransportEquipment1.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT1_CONTAINERNUMBER");
			iTransportEquipment1.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSTATUS");
			iTransportEquipment1.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT1_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment1.Setup(m => m.StowPosition).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSTOWPOSITION");
			iTransportEquipment1.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "ITRANSPORTEQUIPMENT1_SEALNUMBER1", "ITRANSPORTEQUIPMENT1_SEALNUMBER2" });
			iTransportEquipment1.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			iTransportEquipment1.Setup(m => m.StuffingLocation).Returns(ZGuid.NewZGuid());
			iTransportEquipment1.Setup(m => m.MessageSequence).Returns(1);

			var iTransportEquipment2 = new Mock<ITransportEquipment>();
			iTransportEquipment2.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT2_CONTAINERSIZE");
			iTransportEquipment2.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT2_CONTAINERNUMBER");
			iTransportEquipment2.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT2_CONTAINERSTATUS");
			iTransportEquipment2.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT2_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment2.Setup(m => m.StowPosition).Returns("ITRANSPORTEQUIPMENT2_CONTINERSTOWPOSITION");
			iTransportEquipment2.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "ITRANSPORTEQUIPMENT2_SEALNUMBER1", "ITRANSPORTEQUIPMENT2_SEALNUMBER2" });
			iTransportEquipment2.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			iTransportEquipment2.Setup(m => m.StuffingLocation).Returns(ZGuid.NewZGuid());
			iTransportEquipment2.Setup(m => m.MessageSequence).Returns(2);

			var consignee1 = new Mock<IPartyInformation>();
			consignee1.Setup(m => m.CustomsClientCode).Returns("CONSIGNEE_CUSTOMSCLIENTCODE");
			consignee1.Setup(m => m.Name).Returns("CONSIGNEE_NAME");
			consignee1.Setup(m => m.City).Returns("CONSIGNEE_CITY");
			consignee1.Setup(m => m.CountryCode).Returns("CONSIGNEE_COUNTRYCODE");
			consignee1.Setup(m => m.CountryRegion).Returns("CONSIGNEE_COUNTRYREGION");
			consignee1.Setup(m => m.Address).Returns("CONSIGNEE_ADDRESS");
			consignee1.Setup(m => m.PostCode).Returns("CONSIGNEE_POSTCODE");
			consignee1.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var consignee2 = new Mock<IPartyInformation>();
			consignee2.Setup(m => m.CustomsClientCode).Returns("");
			consignee2.Setup(m => m.Name).Returns("CONSIGNEE2_NAME");
			consignee2.Setup(m => m.City).Returns("CONSIGNEE2_CITY");
			consignee2.Setup(m => m.CountryCode).Returns("CONSIGNEE2_COUNTRYCODE");
			consignee2.Setup(m => m.CountryRegion).Returns("CONSIGNEE2_COUNTRYREGION");
			consignee2.Setup(m => m.Address).Returns("CONSIGNEE2_ADDRESS");
			consignee2.Setup(m => m.PostCode).Returns("CONSIGNEE2_POSTCODE");
			consignee2.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var consignor = new Mock<IPartyInformation>();
			consignor.Setup(m => m.CustomsClientCode).Returns("CONSIGNOR_CUSTOMSCLIENTCODE");
			consignor.Setup(m => m.Name).Returns("CONSIGNOR_NAME");
			consignor.Setup(m => m.City).Returns("CONSIGNOR_CITY");
			consignor.Setup(m => m.CountryCode).Returns("CONSIGNOR_COUNTRYCODE");
			consignor.Setup(m => m.CountryRegion).Returns("CONSIGNOR_COUNTRYREGION");
			consignor.Setup(m => m.Address).Returns("CONSIGNOR_ADDRESS");
			consignor.Setup(m => m.PostCode).Returns("CONSIGNOR_POSTCODE");
			consignor.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var notify = new Mock<IPartyInformation>();
			notify.Setup(m => m.CustomsClientCode).Returns("NOTIFY_CUSTOMSCLIENTCODE");
			notify.Setup(m => m.Name).Returns("NOTIFY_NAME");
			notify.Setup(m => m.City).Returns("NOTIFY_CITY");
			notify.Setup(m => m.CountryCode).Returns("NOTIFY_COUNTRYCODE");
			notify.Setup(m => m.CountryRegion).Returns("NOTIFY_COUNTRYREGION");
			notify.Setup(m => m.Address).Returns("NOTIFY_ADDRESS");
			notify.Setup(m => m.PostCode).Returns("NOTIFY_POSTCODE");
			notify.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var classifications1 = new Mock<IClassification>();
			classifications1.Setup(m => m.Classification).Returns("CLASSIFICATIONS1_CLASSIFICATION");
			classifications1.Setup(m => m.ClassificationTypeCode).Returns("SSO");

			var classifications2 = new Mock<IClassification>();
			classifications2.Setup(m => m.Classification).Returns("CLASSIFICATIONS2_CLASSIFICATION");
			classifications2.Setup(m => m.ClassificationTypeCode).Returns("SSI");

			var iTemperatureRequirements = new Mock<ITemperatureRequirements>();
			iTemperatureRequirements.Setup(m => m.StorageTemp).Returns(1m);
			iTemperatureRequirements.Setup(m => m.MinStorageTemp).Returns(2m);
			iTemperatureRequirements.Setup(m => m.MaxStorageTemp).Returns(3m);

			var consignmentItem1 = new Mock<IICRConsignmentItem>();
			consignmentItem1.Setup(m => m.SendFlashpointTemp).Returns(true);
			consignmentItem1.Setup(m => m.FlashpointTempInCelsius).Returns(1m);
			consignmentItem1.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM1_12345ABCD");
			consignmentItem1.Setup(m => m.Value).Returns(2m);
			consignmentItem1.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.Australia);
			consignmentItem1.Setup(m => m.IdentityNumber).Returns("CONSIGNMENTITEM1_IDENTITYNUMBER");
			consignmentItem1.Setup(m => m.IdentityType).Returns("CONSIGNMENTITEM1_IDENTITYTYPE");
			consignmentItem1.Setup(m => m.GrossWeightInKg).Returns(3m);
			consignmentItem1.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM1_GOODSORIGINCOUNTRY");
			consignmentItem1.Setup(m => m.PackageQty).Returns(2);
			consignmentItem1.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM1_PACKAGETYPE");
			consignmentItem1.Setup(m => m.ContainerNumber).Returns("CONSIGNMENTITEM1_CONTAINERNUMBER");
			consignmentItem1.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignmentItem1.Setup(m => m.Classifications).Returns(new List<IClassification>() { classifications1.Object, classifications2.Object });
			consignmentItem1.Setup(m => m.Temperatures).Returns(iTemperatureRequirements.Object);

			var consignmentItem2 = new Mock<IICRConsignmentItemWithExtraInfos>();
			consignmentItem2.Setup(m => m.SendFlashpointTemp).Returns(true);
			consignmentItem2.Setup(m => m.FlashpointTempInCelsius).Returns(4m);
			consignmentItem2.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM2_12345ABCD");
			consignmentItem2.Setup(m => m.Value).Returns(5m);
			consignmentItem2.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.UnitedStates);
			consignmentItem2.Setup(m => m.IdentityNumber).Returns("CONSIGNMENTITEM2_IDENTITYNUMBER");
			consignmentItem2.Setup(m => m.IdentityType).Returns("CONSIGNMENTITEM2_IDENTITYTYPE");
			consignmentItem2.Setup(m => m.GrossWeightInKg).Returns(6m);
			consignmentItem2.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM2_GOODSORIGINCOUNTRY");
			consignmentItem2.Setup(m => m.PackageQty).Returns(2);
			consignmentItem2.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM2_PACKAGETYPE");
			consignmentItem2.Setup(m => m.ContainerNumber).Returns("CONSIGNMENTITEM2_CONTAINERNUMBER");
			consignmentItem2.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem2.Setup(m => m.SequenceNumber).Returns((ZShort)2);
			consignmentItem2.Setup(m => m.Classifications).Returns(new List<IClassification>() { classifications1.Object, classifications2.Object });
			consignmentItem2.Setup(m => m.Temperatures).Returns(iTemperatureRequirements.Object);

			var consignment1 = new Mock<IICRConsignment>();
			consignment1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignment1.Setup(m => m.ConsignmentValueInNZD).Returns(5.123);
			consignment1.Setup(m => m.Permits).Returns(new List<ZString>() { "CONSIGNMENT1_PERMITS1", "CONSIGNMENT1_PERMITS2", ZString.Empty });
			consignment1.Setup(m => m.WriteOffRequest).Returns(true);
			consignment1.Setup(m => m.TranshipmentDetails).Returns(consignment1ITRDetails.Object);
			consignment1.Setup(m => m.IsConsolidation).Returns(true);
			consignment1.Setup(m => m.MAFContainerDeclaration).Returns(true);
			consignment1.Setup(m => m.MAFContainerStatements).Returns(new List<ZString>() { "CONSIGNMENT1_MAFCONTAINERSTATEMENT1", "CONSIGNMENT1_MAFCONTAINERSTATEMENT2" });
			consignment1.Setup(m => m.MPIApprovedSystemNumbers).Returns(new List<ZString>() { "CONSIGNMENT1_MPIApprovedSystemNumber1", "CONSIGNMENT1_MPIApprovedSystemNumber2" });
			consignment1.Setup(m => m.MPIAccountDetails).Returns("CONSIGNMENT1_MPIACCOUNT");
			consignment1.Setup(m => m.HandlingInformation).Returns("CONSIGNMENT1_HandlingInfo");
			consignment1.Setup(m => m.MasterBill).Returns("CONSIGNMENT1_MASTERBILL");
			consignment1.Setup(m => m.PortOfOrigin).Returns("CONSIGNMENT1_PORTOFORIGIN");
			consignment1.Setup(m => m.GoodsLocation).Returns("CONSIGNMENT1_GOODSLOCATION");
			consignment1.Setup(m => m.PortOfLoading).Returns("CONSIGNMENT1_PORTOFLOADING");
			consignment1.Setup(m => m.BillNumber).Returns("CONSIGNMENT1_BILLNUMBER");
			consignment1.Setup(m => m.BillType).Returns("CONSIGNMENT1_BILLTYPE");
			consignment1.Setup(m => m.PortOfDischarge).Returns("CONSIGNMENT1_PORTOFDISCHARGE");
			consignment1.Setup(m => m.FreightPaymentMethod).Returns("CONSIGNMENT1_FREIGHTPAYMENTMETHOD");
			consignment1.Setup(m => m.Deconsolidator).Returns(iOrganisationSimpleMock.Object);
			consignment1.Setup(m => m.ContainerPackingLocations).Returns(new List<IOrganisation>() { iOrganisation1.Object, iOrganisation2.Object });
			consignment1.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment1.Setup(m => m.TranshipmentPorts).Returns(new List<ZString>() { "CONSIGNMENT1_TRANSHIPMENTPORTS1", "CONSIGNMENT1_TRANSHIPMENTPORTS2", ZString.Empty });
			consignment1.Setup(m => m.IsGSTPrePaid).Returns("Y");
			consignment1.Setup(m => m.VendorIdentifier).Returns("1234567");
			consignment1.Setup(m => m.IsLinkEmptyContainer).Returns(false);
			consignment1.Setup(m => m.ApprovedTransitionalFacilityCode).Returns("090500");
			consignment1.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.False);
			consignment1.Setup(m => m.DeliveryNotifyParties).Returns(new List<IOrganisationSimple> { iOrganisation1.Object, iOrganisation2.Object });
			consignment1.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "11111A", "PYKE" });
			consignment1.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { iTransportEquipment1.Object, iTransportEquipment2.Object });
			consignment1.Setup(m => m.HasContainers).Returns(true);
			consignment1.Setup(m => m.Consignee).Returns(consignee1.Object);
			consignment1.Setup(m => m.Consignor).Returns(consignor.Object);
			consignment1.Setup(m => m.NotifyParty).Returns(notify.Object);
			consignment1.Setup(m => m.ConsignmentItems).Returns(new List<IICRConsignmentItem>() { consignmentItem1.Object, consignmentItem2.Object });

			var consignment2 = new Mock<IICRConsignmentWithExtraInfos>();
			consignment2.Setup(m => m.SequenceNumber).Returns((ZShort)2);
			consignment2.Setup(m => m.ConsignmentValueInNZD).Returns(6.2);
			consignment2.Setup(m => m.Permits).Returns(new List<ZString>() { "CONSIGNMENT2_PERMITS1", "CONSIGNMENT2_PERMITS2" });
			consignment2.Setup(m => m.WriteOffRequest).Returns(true);
			consignment2.Setup(m => m.TranshipmentDetails).Returns(consignment2ITRDetails.Object);
			consignment2.Setup(m => m.IsConsolidation).Returns(true);
			consignment2.Setup(m => m.MAFContainerDeclaration).Returns(true);
			consignment2.Setup(m => m.MAFContainerStatements).Returns(new List<ZString>() { "CONSIGNMENT2_MAFCONTAINERSTATEMENT1", "CONSIGNMENT2_MAFCONTAINERSTATEMENT2" });
			consignment2.Setup(m => m.MPIApprovedSystemNumbers).Returns(new List<ZString>() { "CONSIGNMENT2_MPIApprovedSystemNumber1", "CONSIGNMENT2_MPIApprovedSystemNumber2" });
			consignment2.Setup(m => m.MPIAccountDetails).Returns("");
			consignment2.Setup(m => m.HandlingInformation).Returns("");
			consignment2.Setup(m => m.MasterBill).Returns("CONSIGNMENT2_MASTERBILL");
			consignment2.Setup(m => m.PortOfOrigin).Returns("CONSIGNMENT2_PORTOFORIGIN");
			consignment2.Setup(m => m.GoodsLocation).Returns("CONSIGNMENT2_GOODSLOCATION");
			consignment2.Setup(m => m.PortOfLoading).Returns("CONSIGNMENT2_PORTOFLOADING");
			consignment2.Setup(m => m.BillNumber).Returns("CONSIGNMENT2_BILLNUMBER");
			consignment2.Setup(m => m.BillType).Returns("CONSIGNMENT2_BILLTYPE");
			consignment2.Setup(m => m.PortOfDischarge).Returns("CONSIGNMENT2_PORTOFDISCHARGE");
			consignment2.Setup(m => m.FreightPaymentMethod).Returns("CONSIGNMENT2_FREIGHTPAYMENTMETHOD");
			consignment2.Setup(m => m.Deconsolidator).Returns(iOrganisationSimpleMock.Object);
			consignment2.Setup(m => m.ContainerPackingLocations).Returns(new List<IOrganisation>() { iOrganisation1.Object, iOrganisation2.Object });
			consignment2.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment2.Setup(m => m.TranshipmentPorts).Returns(new List<ZString>() { "CONSIGNMENT2_TRANSHIPMENTPORTS1", "CONSIGNMENT2_TRANSHIPMENTPORTS2" });
			consignment2.Setup(m => m.IsGSTPrePaid).Returns("N");
			consignment2.Setup(m => m.VendorIdentifier).Returns("2345678");
			consignment2.Setup(m => m.IsLinkEmptyContainer).Returns(false);
			consignment2.Setup(m => m.ApprovedTransitionalFacilityCode).Returns("090501");
			consignment2.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.False);
			consignment2.Setup(m => m.DeliveryNotifyParties).Returns(new List<IOrganisationSimple> { iOrganisation1.Object, iOrganisation2.Object });
			consignment2.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "22222B", "TARTH" });
			consignment2.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { iTransportEquipment1.Object, iTransportEquipment2.Object });
			consignment2.Setup(m => m.HasContainers).Returns(true);
			consignment2.Setup(m => m.Consignee).Returns(consignee2.Object);
			consignment2.Setup(m => m.Consignor).Returns(consignor.Object);
			consignment2.Setup(m => m.NotifyParty).Returns(notify.Object);
			consignment2.Setup(m => m.ConsignmentItems).Returns(new List<IICRConsignmentItem>() { consignmentItem1.Object, consignmentItem2.Object });

			var iDeclarant = new Mock<IDeclarant>();
			iDeclarant.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });
			iDeclarant.Setup(m => m.DeclarantID).Returns("DECLARANTID");

			var iInwardCargoReportMock = new Mock<IInwardCargoReport>();
			iInwardCargoReportMock.Setup(m => m.TSWReferenceNumber).Returns("ENTRY12345");
			iInwardCargoReportMock.Setup(m => m.SenderReferenceNumber).Returns("C00001165");
			iInwardCargoReportMock.Setup(m => m.IsCarrierCargoReport).Returns(true);
			iInwardCargoReportMock.Setup(m => m.IsSea).Returns(true);
			iInwardCargoReportMock.Setup(m => m.CraftName).Returns("CRAFTNAME");
			iInwardCargoReportMock.Setup(m => m.FlightNo).Returns("FLIGHTNO");
			iInwardCargoReportMock.Setup(m => m.ArrivalDate).Returns(new ZDateTime(2018, 10, 22));
			iInwardCargoReportMock.Setup(m => m.PortOfArrival).Returns("PORTOFARRIVAL");
			iInwardCargoReportMock.Setup(m => m.LloydsNo).Returns("LLOYDSNO");
			iInwardCargoReportMock.Setup(m => m.VoyageNo).Returns("VOYAGENO");
			iInwardCargoReportMock.Setup(m => m.MPIAccountDetails).Returns("IINWARDCARGOREPORTMOCK_MPIACCOUNT");
			iInwardCargoReportMock.Setup(m => m.AdditionalInformation).Returns(additionalInformationMock.Object);
			iInwardCargoReportMock.Setup(m => m.Carrier).Returns(iOrganisationSimpleMock.Object);
			iInwardCargoReportMock.Setup(m => m.UseInterfaceSequenceNumber).Returns(true);
			iInwardCargoReportMock.Setup(m => m.Consignments).Returns(new List<IICRConsignment>() { consignment1.Object, consignment2.Object });
			iInwardCargoReportMock.Setup(m => m.Declarant).Returns(iDeclarant.Object);

			icrBuilder = new ICRMessageBuilder(iInwardCargoReportMock.Object, TSWTransactionTypes.Original, "00009908C");
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			iInwardCargoReportMock.Setup(m => m.IsSea).Returns(false);
			iInwardCargoReportMock.Setup(m => m.UseInterfaceSequenceNumber).Returns(false);
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestICRPopulateConsignmentsAir.txt")), icrBuilder.GetXMLMessage());

			consignment1.Setup(m => m.MasterBill).Returns(ZString.Empty);
			consignment2.Setup(m => m.MasterBill).Returns(ZString.Empty);
			var unexpectedXML = @"<AssociatedTransportDocument>
      <ID>CONSIGNMENT1_MASTERBILL</ID>
      <TypeCode>MB</TypeCode>
    </AssociatedTransportDocument>";
			AssertNotContains(unexpectedXML, icrBuilder.GetXMLMessage());
			consignment1.Setup(m => m.DeliverToParty);
			consignment2.Setup(m => m.DeliverToParty);
			unexpectedXML = @"<DeliveryDestination>
      <Name>IORGANISATION1_NAME</Name>
      <Address>
        <CityName>IORGANISATION1_CITY</CityName>
        <CountryCode>IORGANISATION1_COUNTRYCODE</CountryCode>
        <Line>IORGANISATION1_ADDRESS</Line>
        <PostcodeID>IORGANISATION1_POSTCODE</PostcodeID>
      </Address>
    </DeliveryDestination>";
			AssertNotContains(unexpectedXML, icrBuilder.GetXMLMessage());
			iOrganisation1.Setup(m => m.Name).Returns(consignee1.Object.Name);
			consignment1.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment2.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			AssertNotContains(unexpectedXML, icrBuilder.GetXMLMessage());
			consignment1.Setup(m => m.GoodsLocation).Returns(ZString.Empty);
			consignment2.Setup(m => m.GoodsLocation).Returns(ZString.Empty);
			unexpectedXML = @"<GoodsLocation>
      <ID>CONSIGNMENT1_GOODSLOCATION</ID>
    </GoodsLocation>";
			AssertNotContains(unexpectedXML, icrBuilder.GetXMLMessage());
			unexpectedXML = @"<StuffingEstablishment>
      <Name>IORGANISATION1_NAME</Name>
      <Address>
        <CityName>IORGANISATION1_CITY</CityName>
        <CountryCode>IORGANISATION1_COUNTRYCODE</CountryCode>
        <Line>IORGANISATION1_ADDRESS</Line>
        <PostcodeID>IORGANISATION1_POSTCODE</PostcodeID>
      </Address>
    </StuffingEstablishment>
    <StuffingEstablishment>
      <Name>IORGANISATION2_NAME</Name>
      <Address>
        <CityName>IORGANISATION2_CITY</CityName>
        <CountryCode>IORGANISATION2_COUNTRYCODE</CountryCode>
        <Line>IORGANISATION2_ADDRESS</Line>
        <PostcodeID>IORGANISATION2_POSTCODE</PostcodeID>
      </Address>
    </StuffingEstablishment>";
			AssertNotContains(unexpectedXML, icrBuilder.GetXMLMessage());
			consignment1ITRDetails = new Mock<ITranshipmentDetails>();
			consignment1ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment1ITRDetails.Setup(m => m.DomesticTranshipmentRequest).Returns(false);
			consignment1ITRDetails.Setup(m => m.ITRImportMode).Returns("4");
			consignment1ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("NZ18");
			consignment1ITRDetails.Setup(m => m.ITRDepartureDate).Returns(new ZDateTime(2019, 02, 18));
			consignment1ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("4");
			consignment1ITRDetails.Setup(m => m.ITRImportCraft).Returns("");
			consignment1ITRDetails.Setup(m => m.PremiseCode).Returns("");
			consignment1.Setup(m => m.TranshipmentDetails).Returns(consignment1ITRDetails.Object);
			unexpectedXML = @"<ID>CONSIGNMENT1_PREMISECODE</ID>";
			AssertNotContains(unexpectedXML, icrBuilder.GetXMLMessage());
			consignment1.Setup(m => m.Containers);
			unexpectedXML = @"<TransportEquipment>
        <ID>CONSIGNMENTITEM1_CONTAINERNUMBER</ID>
      </TransportEquipment>";
			AssertNotContains(unexpectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateDeclarant()
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
			additionalInformationMock.Setup(m => m.AdditionalStatementText).Returns("ADDITIONALINFORMATIONMOCK_ADDITIONALSTATEMENTTEXT");

			iOrganisationSimpleMock = new Mock<IOrganisationSimple>();
			iOrganisationSimpleMock.Setup(m => m.Name).Returns("IORGANISATIONSIMPLEMOCK_NAME");
			iOrganisationSimpleMock.Setup(m => m.CustomsClientCode).Returns("51358595A");

			consignment1ITRDetails = new Mock<ITranshipmentDetails>();
			consignment1ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment1ITRDetails.Setup(m => m.DomesticTranshipmentRequest).Returns(false);
			consignment1ITRDetails.Setup(m => m.ITRImportMode).Returns("4");
			consignment1ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("NZ18");
			consignment1ITRDetails.Setup(m => m.ITRArrivalDate).Returns(new ZDateTime(2019, 02, 18));
			consignment1ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("4");
			consignment1ITRDetails.Setup(m => m.ITRImportCraft).Returns("");
			consignment1ITRDetails.Setup(m => m.PremiseCode).Returns("CONSIGNMENT1_PREMISECODE");

			consignment2ITRDetails = new Mock<ITranshipmentDetails>();
			consignment2ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment2ITRDetails.Setup(m => m.DomesticTranshipmentRequest).Returns(false);
			consignment2ITRDetails.Setup(m => m.ITRImportMode).Returns("1");
			consignment2ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("55W");
			consignment2ITRDetails.Setup(m => m.ITRDepartureDate).Returns(new ZDateTime(2019, 02, 22));
			consignment2ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("1C");
			consignment2ITRDetails.Setup(m => m.ITRImportCraft).Returns("AAL FREMANTLE");
			consignment2ITRDetails.Setup(m => m.PremiseCode).Returns("CONSIGNMENT2_PREMISECODE");

			var iCommunication1 = new Mock<ICommunication>();
			iCommunication1.Setup(m => m.ContactDetail).Returns("COMMUNICATION1_CONTACTDETAIL");
			iCommunication1.Setup(m => m.ContactType).Returns("COMMUNICATION1_CONTACTTYPE");

			var iCommunication2 = new Mock<ICommunication>();
			iCommunication2.Setup(m => m.ContactDetail).Returns("COMMUNICATION2_CONTACTDETAIL");
			iCommunication2.Setup(m => m.ContactType).Returns("COMMUNICATION2_CONTACTTYPE");

			var iContact1 = new Mock<IContact>();
			iContact1.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var iContact2 = new Mock<IContact>();
			iContact2.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			iOrganisation1 = new Mock<IOrganisation>();
			iOrganisation1.Setup(m => m.Name).Returns("IORGANISATION1_NAME");
			iOrganisation1.Setup(m => m.CustomsClientCode).Returns("IORGANISATION1_CUSTOMSCLIENTCODE");
			iOrganisation1.Setup(m => m.City).Returns("IORGANISATION1_CITY");
			iOrganisation1.Setup(m => m.CountryCode).Returns("IORGANISATION1_COUNTRYCODE");
			iOrganisation1.Setup(m => m.CountryRegion).Returns("IORGANISATION1_COUNTRYREGION");
			iOrganisation1.Setup(m => m.Address).Returns("IORGANISATION1_ADDRESS");
			iOrganisation1.Setup(m => m.PostCode).Returns("IORGANISATION1_POSTCODE");
			iOrganisation1.Setup(m => m.Communications).Returns(new List<ICommunication>()
			{ iCommunication1.Object, iCommunication2.Object });
			iOrganisation1.Setup(m => m.Contacts).Returns(new List<IContact>()
			{ iContact1.Object, iContact2.Object });

			var iOrganisation2 = new Mock<IOrganisation>();
			iOrganisation2.Setup(m => m.Name).Returns("IORGANISATION2_NAME");
			iOrganisation2.Setup(m => m.CustomsClientCode).Returns("IORGANISATION2_CUSTOMSCLIENTCODE");
			iOrganisation2.Setup(m => m.City).Returns("IORGANISATION2_CITY");
			iOrganisation2.Setup(m => m.CountryCode).Returns("IORGANISATION2_COUNTRYCODE");
			iOrganisation2.Setup(m => m.CountryRegion).Returns("IORGANISATION2_COUNTRYREGION");
			iOrganisation2.Setup(m => m.Address).Returns("IORGANISATION2_ADDRESS");
			iOrganisation2.Setup(m => m.PostCode).Returns("IORGANISATION2_POSTCODE");
			iOrganisation2.Setup(m => m.Communications).Returns(new List<ICommunication>()
			{ iCommunication1.Object, iCommunication2.Object });
			iOrganisation2.Setup(m => m.Contacts).Returns(new List<IContact>()
			{ iContact1.Object, iContact2.Object });

			var iTransportEquipment1 = new Mock<ITransportEquipment>();
			iTransportEquipment1.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSIZE");
			iTransportEquipment1.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT1_CONTAINERNUMBER");
			iTransportEquipment1.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSTATUS");
			iTransportEquipment1.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT1_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment1.Setup(m => m.StowPosition).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSTOWPOSITION");
			iTransportEquipment1.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "ITRANSPORTEQUIPMENT1_SEALNUMBER1", "ITRANSPORTEQUIPMENT1_SEALNUMBER2" });
			iTransportEquipment1.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			iTransportEquipment1.Setup(m => m.StuffingLocation).Returns(ZGuid.NewZGuid());
			iTransportEquipment1.Setup(m => m.MessageSequence).Returns(1);

			var iTransportEquipment2 = new Mock<ITransportEquipment>();
			iTransportEquipment2.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT2_CONTAINERSIZE");
			iTransportEquipment2.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT2_CONTAINERNUMBER");
			iTransportEquipment2.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT2_CONTAINERSTATUS");
			iTransportEquipment2.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT2_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment2.Setup(m => m.StowPosition).Returns("ITRANSPORTEQUIPMENT2_CONTINERSTOWPOSITION");
			iTransportEquipment2.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "ITRANSPORTEQUIPMENT2_SEALNUMBER1", "ITRANSPORTEQUIPMENT2_SEALNUMBER2" });
			iTransportEquipment2.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			iTransportEquipment2.Setup(m => m.StuffingLocation).Returns(ZGuid.NewZGuid());
			iTransportEquipment2.Setup(m => m.MessageSequence).Returns(2);

			var consignee2 = new Mock<IPartyInformation>();
			consignee2.Setup(m => m.CustomsClientCode).Returns("");
			consignee2.Setup(m => m.Name).Returns("CONSIGNEE2_NAME");
			consignee2.Setup(m => m.City).Returns("CONSIGNEE2_CITY");
			consignee2.Setup(m => m.CountryCode).Returns("CONSIGNEE2_COUNTRYCODE");
			consignee2.Setup(m => m.CountryRegion).Returns("CONSIGNEE2_COUNTRYREGION");
			consignee2.Setup(m => m.Address).Returns("CONSIGNEE2_ADDRESS");
			consignee2.Setup(m => m.PostCode).Returns("CONSIGNEE2_POSTCODE");
			consignee2.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var consignor = new Mock<IPartyInformation>();
			consignor.Setup(m => m.CustomsClientCode).Returns("CONSIGNOR_CUSTOMSCLIENTCODE");
			consignor.Setup(m => m.Name).Returns("CONSIGNOR_NAME");
			consignor.Setup(m => m.City).Returns("CONSIGNOR_CITY");
			consignor.Setup(m => m.CountryCode).Returns("CONSIGNOR_COUNTRYCODE");
			consignor.Setup(m => m.CountryRegion).Returns("CONSIGNOR_COUNTRYREGION");
			consignor.Setup(m => m.Address).Returns("CONSIGNOR_ADDRESS");
			consignor.Setup(m => m.PostCode).Returns("CONSIGNOR_POSTCODE");
			consignor.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var notify = new Mock<IPartyInformation>();
			notify.Setup(m => m.Name).Returns("NOTIFY_CUSTOMSCLIENTCODE");
			notify.Setup(m => m.Name).Returns("NOTIFY_NAME");
			notify.Setup(m => m.City).Returns("NOTIFY_CITY");
			notify.Setup(m => m.CountryCode).Returns("NOTIFY_COUNTRYCODE");
			notify.Setup(m => m.CountryRegion).Returns("NOTIFY_COUNTRYREGION");
			notify.Setup(m => m.Address).Returns("NOTIFY_ADDRESS");
			notify.Setup(m => m.PostCode).Returns("NOTIFY_POSTCODE");
			notify.Setup(m => m.Communications).Returns(new List<ICommunication>()
			{ iCommunication1.Object, iCommunication2.Object });

			var classifications1 = new Mock<IClassification>();
			classifications1.Setup(m => m.Classification).Returns("CLASSIFICATIONS1_CLASSIFICATION");
			classifications1.Setup(m => m.ClassificationTypeCode).Returns("SSO");

			var classifications2 = new Mock<IClassification>();
			classifications2.Setup(m => m.Classification).Returns("CLASSIFICATIONS2_CLASSIFICATION");
			classifications2.Setup(m => m.ClassificationTypeCode).Returns("SSI");

			var iTemperatureRequirements = new Mock<ITemperatureRequirements>();
			iTemperatureRequirements.Setup(m => m.StorageTemp).Returns(1m);
			iTemperatureRequirements.Setup(m => m.MinStorageTemp).Returns(2m);
			iTemperatureRequirements.Setup(m => m.MaxStorageTemp).Returns(3m);

			var consignmentItem1 = new Mock<IICRConsignmentItem>();
			consignmentItem1.Setup(m => m.SendFlashpointTemp).Returns(true);
			consignmentItem1.Setup(m => m.FlashpointTempInCelsius).Returns(1m);
			consignmentItem1.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM1_12345ABCD");
			consignmentItem1.Setup(m => m.Value).Returns(2m);
			consignmentItem1.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.Australia);
			consignmentItem1.Setup(m => m.IdentityNumber).Returns("CONSIGNMENTITEM1_IDENTITYNUMBER");
			consignmentItem1.Setup(m => m.IdentityType).Returns("CONSIGNMENTITEM1_IDENTITYTYPE");
			consignmentItem1.Setup(m => m.GrossWeightInKg).Returns(3m);
			consignmentItem1.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM1_GOODSORIGINCOUNTRY");
			consignmentItem1.Setup(m => m.PackageQty).Returns(2);
			consignmentItem1.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM1_PACKAGETYPE");
			consignmentItem1.Setup(m => m.ContainerNumber).Returns("CONSIGNMENTITEM1_CONTAINERNUMBER");
			consignmentItem1.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignmentItem1.Setup(m => m.Classifications).Returns(new List<IClassification>() { classifications1.Object, classifications2.Object });
			consignmentItem1.Setup(m => m.Temperatures).Returns(iTemperatureRequirements.Object);

			var consignmentItem2 = new Mock<IICRConsignmentItemWithExtraInfos>();
			consignmentItem2.Setup(m => m.SendFlashpointTemp).Returns(true);
			consignmentItem2.Setup(m => m.FlashpointTempInCelsius).Returns(4m);
			consignmentItem2.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM2_12345ABCD");
			consignmentItem2.Setup(m => m.Value).Returns(5m);
			consignmentItem2.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.UnitedStates);
			consignmentItem2.Setup(m => m.IdentityNumber).Returns("CONSIGNMENTITEM2_IDENTITYNUMBER");
			consignmentItem2.Setup(m => m.IdentityType).Returns("CONSIGNMENTITEM2_IDENTITYTYPE");
			consignmentItem2.Setup(m => m.GrossWeightInKg).Returns(6m);
			consignmentItem2.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM2_GOODSORIGINCOUNTRY");
			consignmentItem2.Setup(m => m.PackageQty).Returns(2);
			consignmentItem2.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM2_PACKAGETYPE");
			consignmentItem2.Setup(m => m.ContainerNumber).Returns("CONSIGNMENTITEM2_CONTAINERNUMBER");
			consignmentItem2.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem2.Setup(m => m.SequenceNumber).Returns((ZShort)2);
			consignmentItem2.Setup(m => m.Classifications).Returns(new List<IClassification>() { classifications1.Object, classifications2.Object });
			consignmentItem2.Setup(m => m.Temperatures).Returns(iTemperatureRequirements.Object);

			var consignee1 = new Mock<IPartyInformation>();
			consignee1.Setup(m => m.CustomsClientCode).Returns("CONSIGNEE_CUSTOMSCLIENTCODE");
			consignee1.Setup(m => m.Name).Returns("CONSIGNEE_NAME");
			consignee1.Setup(m => m.City).Returns("CONSIGNEE_CITY");
			consignee1.Setup(m => m.CountryCode).Returns("CONSIGNEE_COUNTRYCODE");
			consignee1.Setup(m => m.CountryRegion).Returns("CONSIGNEE_COUNTRYREGION");
			consignee1.Setup(m => m.Address).Returns("CONSIGNEE_ADDRESS");
			consignee1.Setup(m => m.PostCode).Returns("CONSIGNEE_POSTCODE");
			consignee1.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });

			var consignment1 = new Mock<IICRConsignment>();
			consignment1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignment1.Setup(m => m.ConsignmentValueInNZD).Returns(5.123);
			consignment1.Setup(m => m.Permits).Returns(new List<ZString>() { "CONSIGNMENT1_PERMITS1", "CONSIGNMENT1_PERMITS2", ZString.Empty });
			consignment1.Setup(m => m.WriteOffRequest).Returns(true);
			consignment1.Setup(m => m.TranshipmentDetails).Returns(consignment1ITRDetails.Object);
			consignment1.Setup(m => m.IsConsolidation).Returns(true);
			consignment1.Setup(m => m.MAFContainerDeclaration).Returns(true);
			consignment1.Setup(m => m.MAFContainerStatements).Returns(new List<ZString>() { "CONSIGNMENT1_MAFCONTAINERSTATEMENT1", "CONSIGNMENT1_MAFCONTAINERSTATEMENT2" });
			consignment1.Setup(m => m.MPIApprovedSystemNumbers).Returns(new List<ZString>() { "CONSIGNMENT1_MPIApprovedSystemNumber1", "CONSIGNMENT1_MPIApprovedSystemNumber2" });
			consignment1.Setup(m => m.MPIAccountDetails).Returns("CONSIGNMENT1_MPIACCOUNT");
			consignment1.Setup(m => m.HandlingInformation).Returns("CONSIGNMENT1_HandlingInfo");
			consignment1.Setup(m => m.MasterBill).Returns("CONSIGNMENT1_MASTERBILL");
			consignment1.Setup(m => m.PortOfOrigin).Returns("CONSIGNMENT1_PORTOFORIGIN");
			consignment1.Setup(m => m.GoodsLocation).Returns("CONSIGNMENT1_GOODSLOCATION");
			consignment1.Setup(m => m.PortOfLoading).Returns("CONSIGNMENT1_PORTOFLOADING");
			consignment1.Setup(m => m.BillNumber).Returns("CONSIGNMENT1_BILLNUMBER");
			consignment1.Setup(m => m.BillType).Returns("CONSIGNMENT1_BILLTYPE");
			consignment1.Setup(m => m.PortOfDischarge).Returns("CONSIGNMENT1_PORTOFDISCHARGE");
			consignment1.Setup(m => m.FreightPaymentMethod).Returns("CONSIGNMENT1_FREIGHTPAYMENTMETHOD");
			consignment1.Setup(m => m.Deconsolidator).Returns(iOrganisationSimpleMock.Object);
			consignment1.Setup(m => m.ContainerPackingLocations).Returns(new List<IOrganisation>() { iOrganisation1.Object, iOrganisation2.Object });
			consignment1.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment1.Setup(m => m.TranshipmentPorts).Returns(new List<ZString>() { "CONSIGNMENT1_TRANSHIPMENTPORTS1", "CONSIGNMENT1_TRANSHIPMENTPORTS2", ZString.Empty });
			consignment1.Setup(m => m.IsGSTPrePaid).Returns("Y");
			consignment1.Setup(m => m.VendorIdentifier).Returns("1234567");
			consignment1.Setup(m => m.IsLinkEmptyContainer).Returns(false);
			consignment1.Setup(m => m.ApprovedTransitionalFacilityCode).Returns("090500");
			consignment1.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.False);
			consignment1.Setup(m => m.DeliveryNotifyParties).Returns(new List<IOrganisationSimple> { iOrganisation1.Object, iOrganisation2.Object });
			consignment1.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "11111A", "PYKE" });
			consignment1.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { iTransportEquipment1.Object, iTransportEquipment2.Object });
			consignment1.Setup(m => m.HasContainers).Returns(true);
			consignment1.Setup(m => m.Consignee).Returns(consignee1.Object);
			consignment1.Setup(m => m.Consignor).Returns(consignor.Object);
			consignment1.Setup(m => m.NotifyParty).Returns(notify.Object);

			var consignment2 = new Mock<IICRConsignmentWithExtraInfos>();
			consignment2.Setup(m => m.SequenceNumber).Returns((ZShort)2);
			consignment2.Setup(m => m.ConsignmentValueInNZD).Returns(6.2);
			consignment2.Setup(m => m.Permits).Returns(new List<ZString>() { "CONSIGNMENT2_PERMITS1", "CONSIGNMENT2_PERMITS2" });
			consignment2.Setup(m => m.WriteOffRequest).Returns(true);
			consignment2.Setup(m => m.TranshipmentDetails).Returns(consignment2ITRDetails.Object);
			consignment2.Setup(m => m.IsConsolidation).Returns(true);
			consignment2.Setup(m => m.MAFContainerDeclaration).Returns(true);
			consignment2.Setup(m => m.MAFContainerStatements).Returns(new List<ZString>() { "CONSIGNMENT2_MAFCONTAINERSTATEMENT1", "CONSIGNMENT2_MAFCONTAINERSTATEMENT2" });
			consignment2.Setup(m => m.MPIApprovedSystemNumbers).Returns(new List<ZString>() { "CONSIGNMENT2_MPIApprovedSystemNumber1", "CONSIGNMENT2_MPIApprovedSystemNumber2" });
			consignment2.Setup(m => m.MPIAccountDetails).Returns("");
			consignment2.Setup(m => m.HandlingInformation).Returns("");
			consignment2.Setup(m => m.MasterBill).Returns("CONSIGNMENT2_MASTERBILL");
			consignment2.Setup(m => m.PortOfOrigin).Returns("CONSIGNMENT2_PORTOFORIGIN");
			consignment2.Setup(m => m.GoodsLocation).Returns("CONSIGNMENT2_GOODSLOCATION");
			consignment2.Setup(m => m.PortOfLoading).Returns("CONSIGNMENT2_PORTOFLOADING");
			consignment2.Setup(m => m.BillNumber).Returns("CONSIGNMENT2_BILLNUMBER");
			consignment2.Setup(m => m.BillType).Returns("CONSIGNMENT2_BILLTYPE");
			consignment2.Setup(m => m.PortOfDischarge).Returns("CONSIGNMENT2_PORTOFDISCHARGE");
			consignment2.Setup(m => m.FreightPaymentMethod).Returns("CONSIGNMENT2_FREIGHTPAYMENTMETHOD");
			consignment2.Setup(m => m.Deconsolidator).Returns(iOrganisationSimpleMock.Object);
			consignment2.Setup(m => m.ContainerPackingLocations).Returns(new List<IOrganisation>() { iOrganisation1.Object, iOrganisation2.Object });
			consignment2.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment2.Setup(m => m.TranshipmentPorts).Returns(new List<ZString>() { "CONSIGNMENT2_TRANSHIPMENTPORTS1", "CONSIGNMENT2_TRANSHIPMENTPORTS2" });
			consignment2.Setup(m => m.IsGSTPrePaid).Returns("N");
			consignment2.Setup(m => m.VendorIdentifier).Returns("2345678");
			consignment2.Setup(m => m.IsLinkEmptyContainer).Returns(false);
			consignment2.Setup(m => m.ApprovedTransitionalFacilityCode).Returns("090501");
			consignment2.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.False);
			consignment2.Setup(m => m.DeliveryNotifyParties).Returns(new List<IOrganisationSimple> { iOrganisation1.Object, iOrganisation2.Object });
			consignment2.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "22222B", "TARTH" });
			consignment2.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { iTransportEquipment1.Object, iTransportEquipment2.Object });
			consignment2.Setup(m => m.HasContainers).Returns(true);
			consignment2.Setup(m => m.Consignee).Returns(consignee2.Object);
			consignment2.Setup(m => m.Consignor).Returns(consignor.Object);
			consignment2.Setup(m => m.NotifyParty).Returns(notify.Object);

			var iDeclarant = new Mock<IDeclarant>();
			iDeclarant.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });
			iDeclarant.Setup(m => m.DeclarantID).Returns("DECLARANTID");

			consignment1.Setup(m => m.ConsignmentItems).Returns(new List<IICRConsignmentItem>()
			{ consignmentItem1.Object, consignmentItem2.Object });
			consignment2.Setup(m => m.ConsignmentItems).Returns(new List<IICRConsignmentItem>()
			{ consignmentItem1.Object, consignmentItem2.Object });

			var iInwardCargoReportMock = new Mock<IInwardCargoReport>();
			iInwardCargoReportMock.Setup(m => m.TSWReferenceNumber).Returns("ENTRY12345");
			iInwardCargoReportMock.Setup(m => m.SenderReferenceNumber).Returns("C00001165");
			iInwardCargoReportMock.Setup(m => m.IsCarrierCargoReport).Returns(true);
			iInwardCargoReportMock.Setup(m => m.IsSea).Returns(true);
			iInwardCargoReportMock.Setup(m => m.CraftName).Returns("CRAFTNAME");
			iInwardCargoReportMock.Setup(m => m.FlightNo).Returns("FLIGHTNO");
			iInwardCargoReportMock.Setup(m => m.ArrivalDate).Returns(new ZDateTime(2018, 10, 22));
			iInwardCargoReportMock.Setup(m => m.PortOfArrival).Returns("PORTOFARRIVAL");
			iInwardCargoReportMock.Setup(m => m.LloydsNo).Returns("LLOYDSNO");
			iInwardCargoReportMock.Setup(m => m.VoyageNo).Returns("VOYAGENO");
			iInwardCargoReportMock.Setup(m => m.MPIAccountDetails).Returns("IINWARDCARGOREPORTMOCK_MPIACCOUNT");
			iInwardCargoReportMock.Setup(m => m.AdditionalInformation).Returns(additionalInformationMock.Object);
			iInwardCargoReportMock.Setup(m => m.Carrier).Returns(iOrganisationSimpleMock.Object);
			iInwardCargoReportMock.Setup(m => m.UseInterfaceSequenceNumber).Returns(true);
			iInwardCargoReportMock.Setup(m => m.Consignments).Returns(new List<IICRConsignment>() { consignment1.Object, consignment2.Object });
			iInwardCargoReportMock.Setup(m => m.Declarant).Returns(iDeclarant.Object);

			icrBuilder = new ICRMessageBuilder(iInwardCargoReportMock.Object, TSWTransactionTypes.Original, "00009908C");
			embeddedResourceRetriever = new EmbeddedResourceRetriever();

			var expectedXML = @"<Declarant>
    <ID>DECLARANTID</ID>
    <Communication>
      <ID>COMMUNICATION1_CONTACTDETAIL</ID>
      <TypeID>COMMUNICATION1_CONTACTTYPE</TypeID>
    </Communication>
    <Communication>
      <ID>COMMUNICATION2_CONTACTDETAIL</ID>
      <TypeID>COMMUNICATION2_CONTACTTYPE</TypeID>
    </Communication>
  </Declarant>";
			consignment1.Setup(m => m.WriteOffRequest).Returns(false);
			consignment2.Setup(m => m.WriteOffRequest).Returns(false);
			AssertNotContains(expectedXML, icrBuilder.GetXMLMessage());
			consignment1.Setup(m => m.WriteOffRequest).Returns(true);
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
			iInwardCargoReportMock.Setup(m => m.Declarant);
			AssertNotContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentValueAmount()
		{
			SetUpMocks();
			AssertConsignmentValue(false);
			AssertConsignmentValue(true);
		}

		public void TestPopulateAdditionalDocument()
		{
			SetUpMocks();
			var tswAttachment3 = new Mock<ITSWAttachment>();
			var tswAttachment4 = new Mock<ITSWAttachment>();
			tswAttachment3.Setup(m => m.DocType).Returns("OTH");
			tswAttachment3.Setup(m => m.FileName).Returns("TEST3.PDF");
			tswAttachment4.Setup(m => m.DocType).Returns("TST");
			tswAttachment4.Setup(m => m.FileName).Returns("TEST4.PDF");
			consignment1.Setup(m => m.SupportingDocuments).Returns(new List<ITSWAttachment>() { tswAttachment3.Object, tswAttachment4.Object });

			var expectedXML = @"<Consignment>
    <SequenceNumeric>1</SequenceNumeric>
    <ValueAmount currencyID=""NZD"">5.12</ValueAmount>
    <AdditionalDocument>
      <CategoryCode>OTH</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" filename=""TEST3.PDF"">ATTACHED</ImageBinaryObject>
    </AdditionalDocument>
    <AdditionalDocument>
      <CategoryCode>TST</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" filename=""TEST4.PDF"">ATTACHED</ImageBinaryObject>
    </AdditionalDocument>";

			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalInformation_IsSAC()
		{
			SetUpMocks();
			consignment1ITRDetails = new Mock<ITranshipmentDetails>();
			consignment1ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(false);
			consignment1ITRDetails.Setup(m => m.DomesticTranshipmentRequest).Returns(false);
			consignment1ITRDetails.Setup(m => m.ITRImportMode).Returns("");
			consignment1ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("");
			consignment1ITRDetails.Setup(m => m.ITRDepartureDate).Returns(ZDateTime.Empty);
			consignment1ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("");
			consignment1ITRDetails.Setup(m => m.ITRImportCraft).Returns("");
			consignment1.Setup(m => m.TranshipmentDetails).Returns(consignment1ITRDetails.Object);
			var expectedXML = @"</AdditionalDocument>
    <AdditionalInformation>
      <StatementCode>Y</StatementCode>
      <StatementTypeCode>WOF</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementDescription>CONSIGNMENT1_MAFCONTAINERSTATEMENT1</StatementDescription>
      <StatementTypeCode>MCD</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementDescription>CONSIGNMENT1_MAFCONTAINERSTATEMENT2</StatementDescription>
      <StatementTypeCode>MCD</StatementTypeCode>
    </AdditionalInformation>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalInformation_NotSAC()
		{
			SetUpMocks();
			var expectedXML = @"</AdditionalDocument>
    <AdditionalInformation>
      <StatementDescription>CONSIGNMENT1_MAFCONTAINERSTATEMENT1</StatementDescription>
      <StatementTypeCode>MCD</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementDescription>CONSIGNMENT1_MAFCONTAINERSTATEMENT2</StatementDescription>
      <StatementTypeCode>MCD</StatementTypeCode>
    </AdditionalInformation>";
			consignment1.Setup(m => m.WriteOffRequest).Returns(false);
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateITRElements()
		{
			SetUpMocks();
			var expectedITRAdditionalInfoElementsConsignment1 = @"<AdditionalInformation>
      <StatementDescription>Y</StatementDescription>
      <StatementTypeCode>ITR</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementCode>4</StatementCode>
      <StatementTypeCode>MTT</StatementTypeCode>
    </AdditionalInformation>";
			var expectedPremiseIDConsign1 = @"<TransitDestination>
      <ID>CONSIGNMENT1_PREMISECODE</ID>
    </TransitDestination>";
			var expectedITRAdditionalInfoElementsConsignment2 = @"<AdditionalInformation>
      <StatementDescription>AAL FREMANTLE,1,20190222,55W</StatementDescription>
      <StatementTypeCode>ITR</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementCode>1C</StatementCode>
      <StatementTypeCode>MTT</StatementTypeCode>
    </AdditionalInformation>";
			var expectedPremiseIDConsign2 = @"<TransitDestination>
      <ID>CONSIGNMENT2_PREMISECODE</ID>
    </TransitDestination>";
			var messageText = icrBuilder.GetXMLMessage();
			AssertContains(expectedITRAdditionalInfoElementsConsignment1, messageText);
			AssertContains(expectedPremiseIDConsign1, messageText);
			AssertContains(expectedITRAdditionalInfoElementsConsignment2, messageText);
			AssertContains(expectedPremiseIDConsign2, messageText);
		}

		public void TestPopulateDRTElements()
		{
			SetUpMocks();
			var expectedDTRAdditionalInfoElementsConsignment1 = @"<AdditionalInformation>
      <StatementCode>Y</StatementCode>
      <StatementTypeCode>DTR</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementCode>1O</StatementCode>
      <StatementTypeCode>MTT</StatementTypeCode>
    </AdditionalInformation>";
			var expectedPremiseIDConsign1 = @"<TransitDestination>
      <ID>7139P</ID>
    </TransitDestination>";
			// Ensure appropriate DTR details are included in the message generated
			consignment1DTRDetails = new Mock<ITranshipmentDetails>();
			consignment1DTRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(false);
			consignment1DTRDetails.Setup(m => m.DomesticTranshipmentRequest).Returns(true);
			consignment1DTRDetails.Setup(m => m.ITRImportMode).Returns("4");
			consignment1DTRDetails.Setup(m => m.ITRVoyageFlight).Returns("NZ18");
			consignment1DTRDetails.Setup(m => m.ITRDepartureDate).Returns(new ZDateTime(2020, 01, 08));
			consignment1DTRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("1O");
			consignment1DTRDetails.Setup(m => m.ITRImportCraft).Returns("");
			consignment1DTRDetails.Setup(m => m.PremiseCode).Returns("7139P");
			consignment1.Setup(m => m.TranshipmentDetails).Returns(consignment1DTRDetails.Object);
			var messageText = icrBuilder.GetXMLMessage();
			AssertContains(expectedDTRAdditionalInfoElementsConsignment1, messageText);
			AssertContains(expectedPremiseIDConsign1, messageText);
		}

		public void TestPopulateAdditionalInformation_Consignment()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
      <StatementCode>Y</StatementCode>
      <StatementTypeCode>CON</StatementTypeCode>
    </AdditionalInformation>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
			consignment1.Setup(m => m.IsConsolidation).Returns(false);
			consignment2.Setup(m => m.IsConsolidation).Returns(false);
			AssertNotContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateAssociatedTransportDocument()
		{
			SetUpMocks();
			var expectedXML = @"<AssociatedTransportDocument>
      <ID>CONSIGNMENT1_MASTERBILL</ID>
      <TypeCode>MB</TypeCode>
    </AssociatedTransportDocument>";
			consignment1.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.True);
			AssertNotContains(expectedXML, icrBuilder.GetXMLMessage());
			consignment1.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.False);
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignee()
		{
			SetUpMocks();
			var expectedXML = @"<Consignee>
      <ID>CONSIGNEE_CUSTOMSCLIENTCODE</ID>
      <Address>
        <CityName>CONSIGNEE_CITY</CityName>
        <CountryCode>CONSIGNEE_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>CONSIGNEE_COUNTRYREGION</CountrySubDivisionName>
        <Line>CONSIGNEE_ADDRESS</Line>
        <PostcodeID>CONSIGNEE_POSTCODE</PostcodeID>
      </Address>
      <Communication>
        <ID>COMMUNICATION1_CONTACTDETAIL</ID>
        <TypeID>COMMUNICATION1_CONTACTTYPE</TypeID>
      </Communication>
      <Communication>
        <ID>COMMUNICATION2_CONTACTDETAIL</ID>
        <TypeID>COMMUNICATION2_CONTACTTYPE</TypeID>
      </Communication>
    </Consignee>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
			consignee1.Setup(m => m.CustomsClientCode).Returns(ZString.Empty);
			expectedXML = @"<Consignee>
      <Name>CONSIGNEE_NAME</Name>
      <Address>
        <CityName>CONSIGNEE_CITY</CityName>
        <CountryCode>CONSIGNEE_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>CONSIGNEE_COUNTRYREGION</CountrySubDivisionName>
        <Line>CONSIGNEE_ADDRESS</Line>
        <PostcodeID>CONSIGNEE_POSTCODE</PostcodeID>
      </Address>
      <Communication>
        <ID>COMMUNICATION1_CONTACTDETAIL</ID>
        <TypeID>COMMUNICATION1_CONTACTTYPE</TypeID>
      </Communication>
      <Communication>
        <ID>COMMUNICATION2_CONTACTDETAIL</ID>
        <TypeID>COMMUNICATION2_CONTACTTYPE</TypeID>
      </Communication>
    </Consignee>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
			consignee1.Setup(m => m.Name).Returns("CONSIGNEE_NAME".PadLeft(71, '-'));
			consignee1.Setup(m => m.City).Returns("CONSIGNEE_CITY".PadLeft(36, '-'));
			expectedXML = @"<Consignee>
      <Name>---------------------------------------------------------CONSIGNEE_NAM</Name>
      <Address>
        <CityName>----------------------CONSIGNEE_CIT</CityName>
        <CountryCode>CONSIGNEE_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>CONSIGNEE_COUNTRYREGION</CountrySubDivisionName>
        <Line>CONSIGNEE_ADDRESS</Line>
        <PostcodeID>CONSIGNEE_POSTCODE</PostcodeID>
      </Address>
      <Communication>
        <ID>COMMUNICATION1_CONTACTDETAIL</ID>
        <TypeID>COMMUNICATION1_CONTACTTYPE</TypeID>
      </Communication>
      <Communication>
        <ID>COMMUNICATION2_CONTACTDETAIL</ID>
        <TypeID>COMMUNICATION2_CONTACTTYPE</TypeID>
      </Communication>
    </Consignee>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateConsigneeWithNoComms()
		{
			SetUpMocks();
			var iContact1 = new Mock<IContact>();
			var iContact2 = new Mock<IContact>();
			var iCommunication1 = new Mock<ICommunication>();
			iCommunication1.Setup(m => m.ContactDetail).Returns("");
			iCommunication1.Setup(m => m.ContactType).Returns("TE");
			var iCommunication2 = new Mock<ICommunication>();
			iCommunication2.Setup(m => m.ContactDetail).Returns("");
			iCommunication2.Setup(m => m.ContactType).Returns("FX");
			iContact1.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });
			iContact2.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });
			iOrganisation1.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object });
			iOrganisation1.Setup(m => m.Contacts).Returns(new List<IContact>() { iContact1.Object });
			consignee1.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });
			var expectedXML = @"<Consignee>
      <ID>CONSIGNEE_CUSTOMSCLIENTCODE</ID>
      <Address>
        <CityName>CONSIGNEE_CITY</CityName>
        <CountryCode>CONSIGNEE_COUNTRYCODE</CountryCode>
        <CountrySubDivisionName>CONSIGNEE_COUNTRYREGION</CountrySubDivisionName>
        <Line>CONSIGNEE_ADDRESS</Line>
        <PostcodeID>CONSIGNEE_POSTCODE</PostcodeID>
      </Address>
    </Consignee>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignor()
		{
			SetUpMocks();
			var expectedXML = @"<Consignor>
      <Name>CONSIGNOR_NAME</Name>
      <Address>
        <CityName>CONSIGNOR_CITY</CityName>
        <CountryCode>CONSIGNOR_COUNTRYCODE</CountryCode>
        <Line>CONSIGNOR_ADDRESS</Line>
        <PostcodeID>CONSIGNOR_POSTCODE</PostcodeID>
      </Address>
      <Communication>
        <ID>COMMUNICATION1_CONTACTDETAIL</ID>
        <TypeID>COMMUNICATION1_CONTACTTYPE</TypeID>
      </Communication>
      <Communication>
        <ID>COMMUNICATION2_CONTACTDETAIL</ID>
        <TypeID>COMMUNICATION2_CONTACTTYPE</TypeID>
      </Communication>
    </Consignor>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
			consignor.Setup(m => m.Name).Returns("CONSIGNOR_NAME".PadLeft(71, '-'));
			consignor.Setup(m => m.City).Returns("CONSIGNOR_CITY".PadLeft(36, '-'));
			expectedXML = @"<Consignor>
      <Name>---------------------------------------------------------CONSIGNOR_NAM</Name>
      <Address>
        <CityName>----------------------CONSIGNOR_CIT</CityName>
        <CountryCode>CONSIGNOR_COUNTRYCODE</CountryCode>
        <Line>CONSIGNOR_ADDRESS</Line>
        <PostcodeID>CONSIGNOR_POSTCODE</PostcodeID>
      </Address>
      <Communication>
        <ID>COMMUNICATION1_CONTACTDETAIL</ID>
        <TypeID>COMMUNICATION1_CONTACTTYPE</TypeID>
      </Communication>
      <Communication>
        <ID>COMMUNICATION2_CONTACTDETAIL</ID>
        <TypeID>COMMUNICATION2_CONTACTTYPE</TypeID>
      </Communication>
    </Consignor>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateDeliveryDestination()
		{
			SetUpMocks();
			var expectedXML = @"<DeliveryDestination>
      <ID>090500</ID>
    </DeliveryDestination>";
			consignment1.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.True);
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
			expectedXML = @"<DeliveryDestination>
      <Name>IORGANISATION1_NAME</Name>
      <Address>
        <CityName>IORGANISATION1_CITY</CityName>
        <CountryCode>IORGANISATION1_COUNTRYCODE</CountryCode>
        <Line>IORGANISATION1_ADDRESS</Line>
        <PostcodeID>IORGANISATION1_POSTCODE</PostcodeID>
      </Address>
    </DeliveryDestination>";
			consignment1.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.False);
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateFreight()
		{
			SetUpMocks();
			var expectedXML = @"<Freight>
      <PaymentMethodCode>CONSIGNMENT1_FREIGHTPAYMENTMETHOD</PaymentMethodCode>
    </Freight>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateGoodsConsignedPlace()
		{
			SetUpMocks();
			var expectedXML = @"<GoodsConsignedPlace>
      <ID>CONSIGNMENT1_PORTOFORIGIN</ID>
    </GoodsConsignedPlace>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateGoodsLocation()
		{
			SetUpMocks();
			var expectedXML = @"<GoodsLocation>
      <ID>CONSIGNMENT1_GOODSLOCATION</ID>
    </GoodsLocation>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateLoadingLocation()
		{
			SetUpMocks();
			var expectedXML = @"<LoadingLocation>
      <ID>CONSIGNMENT1_PORTOFLOADING</ID>
    </LoadingLocation>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateNotifyParty()
		{
			SetUpMocks();
			var expectedXML = @"    <NotifyParty>
      <Name>NOTIFY_NAME</Name>
      <RoleCode>NI</RoleCode>
      <Address>
        <CityName>NOTIFY_CITY</CityName>
        <CountryCode>NOTIFY_COUNTRYCODE</CountryCode>
        <Line>NOTIFY_ADDRESS</Line>
        <PostcodeID>NOTIFY_POSTCODE</PostcodeID>
      </Address>
    </NotifyParty>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestNotifyPartyDoesNotPopulateWhenEmpty()
		{
			SetUpMocks();
			var notify = new Mock<IPartyInformation>();
			notify.Setup(m => m.CustomsClientCode).Returns("");
			notify.Setup(m => m.Name).Returns("");
			notify.Setup(m => m.City).Returns("");
			notify.Setup(m => m.CountryCode).Returns("");
			notify.Setup(m => m.CountryRegion).Returns("");
			notify.Setup(m => m.Address).Returns("");
			notify.Setup(m => m.PostCode).Returns("");
			consignment1.Setup(m => m.NotifyParty).Returns(notify.Object);
			consignment2.Setup(m => m.NotifyParty).Returns(notify.Object);
			consignment1.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "", "", });
			consignment1.Setup(m => m.DeliveryNotifyParties).Returns(Enumerable.Empty<IOrganisationSimple>());
			consignment2.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "", "", });
			consignment2.Setup(m => m.DeliveryNotifyParties).Returns(Enumerable.Empty<IOrganisationSimple>());
			var communication1 = new Mock<ICommunication>();
			communication1.Setup(m => m.ContactType).Returns("");
			communication1.Setup(m => m.ContactDetail).Returns("");
			var contact1 = new Mock<IContact>();
			contact1.Setup(m => m.ContactName).Returns("");
			contact1.Setup(m => m.Communications).Returns(new List<ICommunication> { communication1.Object });
			var deliveryNotify1 = new Mock<IOrganisationSimple>();
			deliveryNotify1.Setup(m => m.Name).Returns("");
			deliveryNotify1.Setup(m => m.CustomsClientCode).Returns("");
			deliveryNotify1.Setup(m => m.CustomsSupplierCode).Returns("");
			deliveryNotify1.Setup(m => m.Contacts).Returns(new List<IContact> { contact1.Object });
			consignment1.Setup(m => m.DeliveryNotifyParties).Returns(new List<IOrganisationSimple> { deliveryNotify1.Object });
			var notifyPartyElement = @"<NotifyParty>
      <Name />
      <ID />
      <RoleCode>NI</RoleCode>
      <Address>
        <CityName />
        <CountryCode />
        <Line />
        <PostcodeID />
      </Address>
    </NotifyParty>";
			var icrMessageGenerated = icrBuilder.GetXMLMessage();
			AssertNotContains("Notify Party element should not be included in the message when empty", notifyPartyElement, icrMessageGenerated);
			var notifyPartyElementHeader = @"<NotifyParty>";
			AssertNotContains("Notify Party element should not be included in the message when empty", notifyPartyElementHeader, icrMessageGenerated);
		}

		public void TestNotifyPartyPopulatesWithIDCode()
		{
			SetUpMocks();
			var notify = new Mock<IPartyInformation>();
			notify.Setup(m => m.CustomsClientCode).Returns("TSWCodeIssued");
			notify.Setup(m => m.Name).Returns("WINTERFELL");
			notify.Setup(m => m.City).Returns("");
			notify.Setup(m => m.CountryCode).Returns("");
			notify.Setup(m => m.CountryRegion).Returns("");
			notify.Setup(m => m.Address).Returns("");
			notify.Setup(m => m.PostCode).Returns("");
			consignment1.Setup(m => m.NotifyParty).Returns(notify.Object);
			consignment1.Setup(m => m.NotifyPartyCodes).Returns(Enumerable.Empty<ZString>());
			consignment2.Setup(m => m.NotifyPartyCodes).Returns(Enumerable.Empty<ZString>());
			var notifyPartyElement = @"<NotifyParty>
      <Name>WINTERFELL</Name>
      <RoleCode>NI</RoleCode>
      <Address>
        <CityName />
        <CountryCode />
        <Line />
        <PostcodeID />
      </Address>
    </NotifyParty>";
			AssertContains("Notify Party element does not need address if using the TSW issued code only", notifyPartyElement, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateTranshipmentPorts()
		{
			SetUpMocks();
			var expectedXML = @"<TranshipmentLocation>
      <ID>CONSIGNMENT1_TRANSHIPMENTPORTS1</ID>
    </TranshipmentLocation>
    <TranshipmentLocation>
      <ID>CONSIGNMENT1_TRANSHIPMENTPORTS2</ID>
    </TranshipmentLocation>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
			expectedXML = @"<TranshipmentLocation>
      <ID></ID>
    </TranshipmentLocation>";
			AssertNotContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateTransportContractDocument()
		{
			SetUpMocks();
			var expectedXML = @"<TransportContractDocument>
      <ID>CONSIGNMENT1_BILLNUMBER</ID>
      <TypeCode>CONSIGNMENT1_BILLTYPE</TypeCode>
      <Deconsolidator>
        <ID>51358595A</ID>
      </Deconsolidator>
    </TransportContractDocument>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateTransportEquipment()
		{
			SetUpMocks();
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestICRPopulateTransportEquipment.txt")), icrBuilder.GetXMLMessage());
		}

		public void TestPopulateRORTransportEquipment()
		{
			SetUpMocks();
			var expectedXML = @"<TransportEquipment>
      <SequenceNumeric>3</SequenceNumeric>
      <CharacteristicCode>ITRANSPORTEQUIPMENT3_CONTAINERSIZE</CharacteristicCode>
      <FullnessCode>ITRANSPORTEQUIPMENT3_CONTAINERSTATUS</FullnessCode>
      <AttachedCode>ITRANSPORTEQUIPMENT3_CONTAINERATTACHEDEQUIPMENTCODE</AttachedCode>
      <ID>ITRANSPORTEQUIPMENT3_CONTAINERNUMBER</ID>
    </TransportEquipment>";
			AssertNotContains(expectedXML, icrBuilder.GetXMLMessage());
			iTransportEquipment3.Setup(m => m.ContainerMode).Returns(ContainerModeList.Codes.FCL);
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateUnloadingLocation()
		{
			SetUpMocks();
			var expectedXML = @"<UnloadingLocation>
      <ID>CONSIGNMENT1_PORTOFDISCHARGE</ID>
      <ArrivalDateTime formatCode=""203"">201810220000</ArrivalDateTime>
    </UnloadingLocation>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentItemsSea()
		{
			SetUpMocks();
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestICRPopulateConsignmentItemsSea.txt")), icrBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentItemsAir()
		{
			SetUpMocks();
			iInwardCargoReportMock.Setup(m => m.IsSea).Returns(false);
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestICRPopulateConsignmentItemsAir.txt")), icrBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentItemsSequenceForDecWithMultipleInvoices()
		{
			SetUpMocks();
			var expectedConsignmentItem1 = @"<ConsignmentItem>
      <SequenceNumeric>1</SequenceNumeric>";
			var expectedConsignmentItem2 = @"<ConsignmentItem>
      <SequenceNumeric>2</SequenceNumeric>";
			var expectedConsignmentItem3 = @"<ConsignmentItem>
      <SequenceNumeric>3</SequenceNumeric>";
			iInwardCargoReportMock.Setup(m => m.Consignments).Returns(new List<IICRConsignment>() { consignment1.Object });
			Mock<IICRConsignmentItem> consignmentItem3;
			consignmentItem1 = new Mock<IICRConsignmentItem>();
			consignmentItem2 = new Mock<IICRConsignmentItemWithExtraInfos>();
			consignmentItem3 = new Mock<IICRConsignmentItem>();
			consignment1.Setup(m => m.ConsignmentItems).Returns(new List<IICRConsignmentItem>() { consignmentItem1.Object, consignmentItem2.Object, consignmentItem3.Object });
			consignmentItem1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignmentItem1.Setup(m => m.Value).Returns(150.00m);
			consignmentItem1.Setup(m => m.Currency).Returns("NZD");
			consignmentItem1.Setup(m => m.IdentityNumber).Returns("");
			consignmentItem1.Setup(m => m.SendFlashpointTemp).Returns(true);
			consignmentItem1.Setup(m => m.FlashpointTempInCelsius).Returns(-15m);
			consignmentItem1.Setup(m => m.GoodsDescription).Returns("ARTICLES OF PLASTICS OF OTHER MATERIALS ETC");
			consignmentItem1.Setup(m => m.GrossWeightInKg).Returns(3m);
			consignmentItem1.Setup(m => m.GoodsOriginCountry).Returns("AU");
			consignmentItem1.Setup(m => m.PackageQty).Returns(2);
			consignmentItem1.Setup(m => m.PackageType).Returns("BOX");
			consignmentItem1.Setup(m => m.ContainerNumber).Returns("MSKU0049285");
			consignmentItem1.Setup(m => m.IsEmptyContainer).Returns(false);
			var tariffClassification = new Mock<IClassification>();
			tariffClassification.Setup(m => m.Classification).Returns("3926906979F");
			tariffClassification.Setup(m => m.ClassificationTypeCode).Returns("HS");
			var dgClassification = new Mock<IClassification>();
			dgClassification.Setup(m => m.Classification).Returns("3255");
			dgClassification.Setup(m => m.ClassificationTypeCode).Returns("SSO");
			consignmentItem1.Setup(m => m.Classifications).Returns(new List<IClassification>() { tariffClassification.Object, dgClassification.Object });
			consignmentItem2.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignmentItem2.Setup(m => m.Value).Returns(150.00m);
			consignmentItem2.Setup(m => m.Currency).Returns("NZD");
			consignmentItem2.Setup(m => m.IdentityNumber).Returns("");
			consignmentItem2.Setup(m => m.SendFlashpointTemp).Returns(true);
			consignmentItem2.Setup(m => m.FlashpointTempInCelsius).Returns(-15m);
			consignmentItem2.Setup(m => m.GoodsDescription).Returns("ARTICLES OF PLASTICS OF OTHER MATERIALS ETC");
			consignmentItem2.Setup(m => m.GrossWeightInKg).Returns(3m);
			consignmentItem2.Setup(m => m.GoodsOriginCountry).Returns("AU");
			consignmentItem2.Setup(m => m.PackageQty).Returns(2);
			consignmentItem2.Setup(m => m.PackageType).Returns("BOX");
			consignmentItem2.Setup(m => m.ContainerNumber).Returns("MSKU0049285");
			consignmentItem2.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem2.Setup(m => m.Classifications).Returns(new List<IClassification>() { tariffClassification.Object, dgClassification.Object });
			consignmentItem3.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignmentItem3.Setup(m => m.Value).Returns(150.00m);
			consignmentItem3.Setup(m => m.Currency).Returns("NZD");
			consignmentItem3.Setup(m => m.IdentityNumber).Returns("");
			consignmentItem3.Setup(m => m.SendFlashpointTemp).Returns(true);
			consignmentItem3.Setup(m => m.FlashpointTempInCelsius).Returns(-15m);
			consignmentItem3.Setup(m => m.GoodsDescription).Returns("ARTICLES OF PLASTICS OF OTHER MATERIALS ETC");
			consignmentItem3.Setup(m => m.GrossWeightInKg).Returns(3m);
			consignmentItem3.Setup(m => m.GoodsOriginCountry).Returns("AU");
			consignmentItem3.Setup(m => m.PackageQty).Returns(2);
			consignmentItem3.Setup(m => m.PackageType).Returns("BOX");
			consignmentItem3.Setup(m => m.ContainerNumber).Returns("MSKU0049285");
			consignmentItem3.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem3.Setup(m => m.Classifications).Returns(new List<IClassification>() { tariffClassification.Object, dgClassification.Object });
			consignment1.Setup(m => m.ConsignmentItems).Returns(new List<IICRConsignmentItem>() { consignmentItem1.Object, consignmentItem2.Object, consignmentItem3.Object });
			var im1Message = icrBuilder.GetXMLMessage();
			AssertContains(expectedConsignmentItem1, im1Message);
			AssertContains("ConsignmentItem sequence number must increment", expectedConsignmentItem2, im1Message);
			AssertContains("ConsignmentItem sequence number must increment", expectedConsignmentItem3, im1Message);
		}

		public void TestPopulateCommodity()
		{
			SetUpMocks();
			var expectedXML = @"<Commodity>
        <CargoDescription>CONSIGNMENTITEM112345ABCD</CargoDescription>
        <CommercialCategorizationID>CONSIGNMENTITEM1_IDENTITYNUMBER</CommercialCategorizationID>
        <ValueAmount currencyID=""AUD"">2</ValueAmount>
        <IdentityQualifierCode>CONSIGNMENTITEM1_IDENTITYTYPE</IdentityQualifierCode>
        <Classification>
          <ID>CLASSIFICATIONS1_CLASSIFICATION</ID>
          <IdentificationTypeCode>SSO</IdentificationTypeCode>
        </Classification>
        <Classification>
          <ID>CLASSIFICATIONS2_CLASSIFICATION</ID>
          <IdentificationTypeCode>SSI</IdentificationTypeCode>
        </Classification>
        <Temperature>
          <FlashpointMeasure unitCode=""CEL"">1</FlashpointMeasure>
          <StorageRequirementMeasure unitCode=""CEL"">1</StorageRequirementMeasure>
          <MinimumStorageRequirementMeasure unitCode=""CEL"">2</MinimumStorageRequirementMeasure>
          <MaximumStorageRequirementMeasure unitCode=""CEL"">3</MaximumStorageRequirementMeasure>
        </Temperature>
      </Commodity>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateClassifications()
		{
			SetUpMocks();
			var expectedXML = @"<Classification>
          <ID>CLASSIFICATIONS1_CLASSIFICATION</ID>
          <IdentificationTypeCode>SSO</IdentificationTypeCode>
        </Classification>
        <Classification>
          <ID>CLASSIFICATIONS2_CLASSIFICATION</ID>
          <IdentificationTypeCode>SSI</IdentificationTypeCode>
        </Classification>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestClassificationAndTemperatureElements()
		{
			SetUpMocks();
			consignmentItem1 = new Mock<IICRConsignmentItem>();
			consignmentItem1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignmentItem1.Setup(m => m.Value).Returns(150.00m);
			consignmentItem1.Setup(m => m.Currency).Returns("NZD");
			consignmentItem1.Setup(m => m.IdentityNumber).Returns("");
			consignmentItem1.Setup(m => m.SendFlashpointTemp).Returns(true);
			consignmentItem1.Setup(m => m.FlashpointTempInCelsius).Returns(-15m);
			consignmentItem1.Setup(m => m.GoodsDescription).Returns("ARTICLES OF PLASTICS OF OTHER MATERIALS ETC");
			consignmentItem1.Setup(m => m.GrossWeightInKg).Returns(3m);
			consignmentItem1.Setup(m => m.GoodsOriginCountry).Returns("AU");
			consignmentItem1.Setup(m => m.PackageQty).Returns(2);
			consignmentItem1.Setup(m => m.PackageType).Returns("BOX");
			consignmentItem1.Setup(m => m.ContainerNumber).Returns("MSKU0049285");
			consignmentItem1.Setup(m => m.IsEmptyContainer).Returns(false);
			consignment1.Setup(m => m.ConsignmentItems).Returns(new List<IICRConsignmentItem>() { consignmentItem1.Object });
			var tariffClassification = new Mock<IClassification>();
			tariffClassification.Setup(m => m.Classification).Returns("3926906979F");
			tariffClassification.Setup(m => m.ClassificationTypeCode).Returns("HS");
			var dgClassification = new Mock<IClassification>();
			dgClassification.Setup(m => m.Classification).Returns("3255");
			dgClassification.Setup(m => m.ClassificationTypeCode).Returns("SSO");
			consignmentItem1.Setup(m => m.Classifications).Returns(new List<IClassification>() { tariffClassification.Object, dgClassification.Object });
			var iTemperatureRequirements = new Mock<ITemperatureRequirements>();
			iTemperatureRequirements.Setup(m => m.StorageTemp).Returns(-25m);
			iTemperatureRequirements.Setup(m => m.MinStorageTemp).Returns(-45m);
			iTemperatureRequirements.Setup(m => m.MaxStorageTemp).Returns(-20m);
			consignmentItem1.Setup(m => m.Temperatures).Returns(iTemperatureRequirements.Object);
			var classAndTempElementsConsignment1 = @"<ConsignmentItem>
      <SequenceNumeric>1</SequenceNumeric>
      <Commodity>
        <CargoDescription>ARTICLES OF PLASTICS OF OTHER MATERIALS ETC</CargoDescription>
        <ValueAmount currencyID=""NZD"">150.00</ValueAmount>
        <Classification>
          <ID>3926906979F</ID>
          <IdentificationTypeCode>HS</IdentificationTypeCode>
        </Classification>
        <Classification>
          <ID>3255</ID>
          <IdentificationTypeCode>SSO</IdentificationTypeCode>
        </Classification>
        <Temperature>
          <FlashpointMeasure unitCode=""CEL"">-15</FlashpointMeasure>
          <StorageRequirementMeasure unitCode=""CEL"">-25</StorageRequirementMeasure>
          <MinimumStorageRequirementMeasure unitCode=""CEL"">-45</MinimumStorageRequirementMeasure>
          <MaximumStorageRequirementMeasure unitCode=""CEL"">-20</MaximumStorageRequirementMeasure>
        </Temperature>
      </Commodity>";
			AssertContains(classAndTempElementsConsignment1, icrBuilder.GetXMLMessage());
		}

		public void TestFlashpointTempElementNotSentConditionally()
		{
			SetUpMocks();
			consignmentItem1 = new Mock<IICRConsignmentItem>();
			consignmentItem1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignmentItem1.Setup(m => m.Value).Returns(150.00m);
			consignmentItem1.Setup(m => m.Currency).Returns("NZD");
			consignmentItem1.Setup(m => m.IdentityNumber).Returns("");
			consignmentItem1.Setup(m => m.SendFlashpointTemp).Returns(false);
			consignmentItem1.Setup(m => m.FlashpointTempInCelsius).Returns(0m);
			consignmentItem1.Setup(m => m.GoodsDescription).Returns("ARTICLES OF PLASTICS OF OTHER MATERIALS ETC");
			consignmentItem1.Setup(m => m.GrossWeightInKg).Returns(3m);
			consignmentItem1.Setup(m => m.GoodsOriginCountry).Returns("AU");
			consignmentItem1.Setup(m => m.PackageQty).Returns(2);
			consignmentItem1.Setup(m => m.PackageType).Returns("BOX");
			consignmentItem1.Setup(m => m.ContainerNumber).Returns("MSKU0049285");
			consignmentItem1.Setup(m => m.IsEmptyContainer).Returns(false);
			consignment1.Setup(m => m.ConsignmentItems).Returns(new List<IICRConsignmentItem>() { consignmentItem1.Object });
			var tariffClassification = new Mock<IClassification>();
			tariffClassification.Setup(m => m.Classification).Returns("3926906979F");
			tariffClassification.Setup(m => m.ClassificationTypeCode).Returns("HS");
			var dgClassification = new Mock<IClassification>();
			dgClassification.Setup(m => m.Classification).Returns("3255");
			dgClassification.Setup(m => m.ClassificationTypeCode).Returns("SSO");
			consignmentItem1.Setup(m => m.Classifications).Returns(new List<IClassification>() { tariffClassification.Object, dgClassification.Object });
			var classElementsConsignment1 = @"<ConsignmentItem>
      <SequenceNumeric>1</SequenceNumeric>
      <Commodity>
        <CargoDescription>ARTICLES OF PLASTICS OF OTHER MATERIALS ETC</CargoDescription>
        <ValueAmount currencyID=""NZD"">150.00</ValueAmount>
        <Classification>
          <ID>3926906979F</ID>
          <IdentificationTypeCode>HS</IdentificationTypeCode>
        </Classification>
        <Classification>
          <ID>3255</ID>
          <IdentificationTypeCode>SSO</IdentificationTypeCode>
        </Classification>
      </Commodity>";
			AssertContains("Temperature elements should not be included in this scenario", classElementsConsignment1, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateItemCommodityTemperature()
		{
			SetUpMocks();
			var expectedXML = @"<Commodity>
        <CargoDescription>CONSIGNMENTITEM112345ABCD</CargoDescription>
        <CommercialCategorizationID>CONSIGNMENTITEM1_IDENTITYNUMBER</CommercialCategorizationID>
        <ValueAmount currencyID=""AUD"">2</ValueAmount>
        <IdentityQualifierCode>CONSIGNMENTITEM1_IDENTITYTYPE</IdentityQualifierCode>
        <Classification>
          <ID>CLASSIFICATIONS1_CLASSIFICATION</ID>
          <IdentificationTypeCode>SSO</IdentificationTypeCode>
        </Classification>
        <Classification>
          <ID>CLASSIFICATIONS2_CLASSIFICATION</ID>
          <IdentificationTypeCode>SSI</IdentificationTypeCode>
        </Classification>
        <Temperature>
          <FlashpointMeasure unitCode=""CEL"">1</FlashpointMeasure>
          <StorageRequirementMeasure unitCode=""CEL"">1</StorageRequirementMeasure>
          <MinimumStorageRequirementMeasure unitCode=""CEL"">2</MinimumStorageRequirementMeasure>
          <MaximumStorageRequirementMeasure unitCode=""CEL"">3</MaximumStorageRequirementMeasure>
        </Temperature>
      </Commodity>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateGoodsMeasure()
		{
			SetUpMocks();
			var expectedXML = @"<GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">3</GrossMassMeasure>
      </GoodsMeasure>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateItemOrigin()
		{
			SetUpMocks();
			var expectedXML = @"<Origin>
        <CountryCode>CONSIGNMENTITEM1_GOODSORIGINCOUNTRY</CountryCode>
      </Origin>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateItemOriginWithEmptyCountry()
		{
			SetUpMocks();
			consignmentItem1.Setup(m => m.GoodsOriginCountry).Returns(ZString.Empty);
			consignmentItem2.Setup(m => m.GoodsOriginCountry).Returns(ZString.Empty);
			var expectedXML = @"<Origin>
        <CountryCode />
      </Origin>";
			AssertNotContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateItemPackaging()
		{
			SetUpMocks();
			var expectedXML = @"<Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>2</QuantityQuantity>
        <TypeCode>CONSIGNMENTITEM1_PACKAGETYPE</TypeCode>
      </Packaging>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateItemTransportEquipment()
		{
			SetUpMocks();
			var expectedXML = @"<TransportEquipment>
        <ID>ITRANSPORTEQUIPMENT1_CONTAINERNUMBER</ID>
      </TransportEquipment>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateRORItemTransportEquipment()
		{
			SetUpMocks();
			var expectedXML = @"<TransportEquipment>
        <ID>ITRANSPORTEQUIPMENT3_CONTAINERNUMBER</ID>
      </TransportEquipment>";
			AssertNotContains(expectedXML, icrBuilder.GetXMLMessage());
			iTransportEquipment3.Setup(m => m.ContainerMode).Returns(ContainerModeList.Codes.FCL);
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateStufffingLocation()
		{
			SetUpMocks();
			var expectedStuffingElements = @"<StuffingEstablishment>
      <Name>IORGANISATION1_NAME</Name>
      <Address>
        <CityName>IORGANISATION1_CITY</CityName>
        <CountryCode>IORGANISATION1_COUNTRYCODE</CountryCode>
        <Line>IORGANISATION1_ADDRESS</Line>
        <PostcodeID>IORGANISATION1_POSTCODE</PostcodeID>
      </Address>
    </StuffingEstablishment>
    <StuffingEstablishment>
      <Name>IORGANISATION2_NAME</Name>
      <Address>
        <CityName>IORGANISATION2_CITY</CityName>
        <CountryCode>IORGANISATION2_COUNTRYCODE</CountryCode>
        <Line>IORGANISATION2_ADDRESS</Line>
        <PostcodeID>IORGANISATION2_POSTCODE</PostcodeID>
      </Address>
    </StuffingEstablishment>";
			AssertContains(expectedStuffingElements, icrBuilder.GetXMLMessage());
			var expectedTransportElements = @"<TransportEquipment>
      <SequenceNumeric>1</SequenceNumeric>
      <CharacteristicCode>ITRANSPORTEQUIPMENT1_CONTAINERSIZE</CharacteristicCode>
      <FullnessCode>ITRANSPORTEQUIPMENT1_CONTAINERSTATUS</FullnessCode>
      <AttachedCode>ITRANSPORTEQUIPMENT1_CONTAINERATTACHEDEQUIPMENTCODE</AttachedCode>
      <ID>ITRANSPORTEQUIPMENT1_CONTAINERNUMBER</ID>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>16B</DocumentSectionCode>
      </Pointer>
      <StowPosition>
        <ID>ITRANSPORTEQUIPMENT1_CONTAINERSTOWPOSITION</ID>
      </StowPosition>
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
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>2</SequenceNumeric>
        <DocumentSectionCode>16B</DocumentSectionCode>
      </Pointer>
      <StowPosition>
        <ID>ITRANSPORTEQUIPMENT2_CONTINERSTOWPOSITION</ID>
      </StowPosition>
      <Seal>
        <SequenceNumeric>1</SequenceNumeric>
        <ID>ITRANSPORTEQUIPMENT2_SEALNUMBER1</ID>
      </Seal>
      <Seal>
        <SequenceNumeric>2</SequenceNumeric>
        <ID>ITRANSPORTEQUIPMENT2_SEALNUMBER2</ID>
      </Seal>
    </TransportEquipment>";
			AssertContains(expectedTransportElements, icrBuilder.GetXMLMessage());
		}

		public void TestTransportEquipmentOnMultipleConsignments()
		{
			SetUpMocks();
			var container1 = new Mock<ITransportEquipment>();
			var container2 = new Mock<ITransportEquipment>();
			var container3 = new Mock<ITransportEquipment>();
			container1.Setup(m => m.Size).Returns("23");
			container1.Setup(m => m.ContainerNumber).Returns("AALU9384019");
			container1.Setup(m => m.Status).Returns("5");
			container1.Setup(m => m.AttachedEquipmentCode).Returns("");
			container1.Setup(m => m.StowPosition).Returns("");
			container1.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "123" });
			container1.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			container1.Setup(m => m.StuffingLocation).Returns(ZGuid.Empty);
			container1.Setup(m => m.MessageSequence).Returns(1);
			container2.Setup(m => m.Size).Returns("21");
			container2.Setup(m => m.ContainerNumber).Returns("AALU3049121");
			container2.Setup(m => m.Status).Returns("5");
			container2.Setup(m => m.AttachedEquipmentCode).Returns("");
			container2.Setup(m => m.StowPosition).Returns("");
			container2.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "456" });
			container2.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			container2.Setup(m => m.StuffingLocation).Returns(ZGuid.Empty);
			container2.Setup(m => m.MessageSequence).Returns(1);
			container3.Setup(m => m.Size).Returns("40");
			container3.Setup(m => m.ContainerNumber).Returns("AALU0394016");
			container3.Setup(m => m.Status).Returns("5");
			container3.Setup(m => m.AttachedEquipmentCode).Returns("");
			container3.Setup(m => m.StowPosition).Returns("");
			container3.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "789" });
			container3.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			container3.Setup(m => m.StuffingLocation).Returns(ZGuid.Empty);
			container3.Setup(m => m.MessageSequence).Returns(2);
			consignment1.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { container1.Object });
			consignment2.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { container2.Object, container3.Object });
			consignment1.Setup(m => m.HasContainers).Returns(true);
			consignment2.Setup(m => m.HasContainers).Returns(true);
			consignment1.Setup(m => m.VendorIdentifier).Returns("TEST 123. - 011");
			consignment1.Setup(m => m.IsGSTPrePaid).Returns("Y");
			consignment1.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.False);
			consignment2.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.False);
			var icrMessageGenerated = icrBuilder.GetXMLMessage();
			var expectedTransportEquipmentElementsConsignment1 = @"    </TransportContractDocument>
    <TransportEquipment>
      <SequenceNumeric>1</SequenceNumeric>
      <CharacteristicCode>23</CharacteristicCode>
      <FullnessCode>5</FullnessCode>
      <ID>AALU9384019</ID>
      <Seal>
        <SequenceNumeric>1</SequenceNumeric>
        <ID>123</ID>
      </Seal>
    </TransportEquipment>
    <UnloadingLocation>";
			AssertContains("Consignment 1 must only have container1 generated", expectedTransportEquipmentElementsConsignment1, icrMessageGenerated);
			var expectedTransportEquipmentElementsConsignment2 = @"    </TransportContractDocument>
    <TransportEquipment>
      <SequenceNumeric>1</SequenceNumeric>
      <CharacteristicCode>21</CharacteristicCode>
      <FullnessCode>5</FullnessCode>
      <ID>AALU3049121</ID>
      <Seal>
        <SequenceNumeric>1</SequenceNumeric>
        <ID>456</ID>
      </Seal>
    </TransportEquipment>
    <TransportEquipment>
      <SequenceNumeric>2</SequenceNumeric>
      <CharacteristicCode>40</CharacteristicCode>
      <FullnessCode>5</FullnessCode>
      <ID>AALU0394016</ID>
      <Seal>
        <SequenceNumeric>1</SequenceNumeric>
        <ID>789</ID>
      </Seal>
    </TransportEquipment>
    <UnloadingLocation>";
			AssertContains("Consignment 2 must only have container2 & container3 generated with sequence 1 & 2 (for this consignment)", expectedTransportEquipmentElementsConsignment2, icrMessageGenerated);
		}

		public void TestMPIApprovedSystemNumbers()
		{
			SetUpMocks();
			var expectedMPINumbersConsignment1 = @"<AdditionalInformation>
      <StatementDescription>CONSIGNMENT1_MPIApprovedSystemNumber1</StatementDescription>
      <StatementTypeCode>MAS</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementDescription>CONSIGNMENT1_MPIApprovedSystemNumber2</StatementDescription>
      <StatementTypeCode>MAS</StatementTypeCode>
    </AdditionalInformation>";
			AssertContains(expectedMPINumbersConsignment1, icrBuilder.GetXMLMessage());
			var expectedMPINumbersConsignment2 = @"<AdditionalInformation>
      <StatementDescription>CONSIGNMENT2_MPIApprovedSystemNumber1</StatementDescription>
      <StatementTypeCode>MAS</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementDescription>CONSIGNMENT2_MPIApprovedSystemNumber2</StatementDescription>
      <StatementTypeCode>MAS</StatementTypeCode>
    </AdditionalInformation>";
			AssertContains(expectedMPINumbersConsignment2, icrBuilder.GetXMLMessage());
		}

		public void TestMPIAccountDetails()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
      <StatementDescription>CONSIGNMENT1_MPIACCOUNT</StatementDescription>
      <StatementTypeCode>MAC</StatementTypeCode>
    </AdditionalInformation>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateContainerConsignmentLevelMASIfEntered()
		{
			// Registry setting is not required as value will only be populated to interface if registry setting is on
			SetUpMocks();
			var expectedXMLConsignment1 = @"<AdditionalInformation>
      <StatementDescription>CONSIGNMENT1_MPIApprovedSystemNumber1</StatementDescription>
      <StatementTypeCode>MAS</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementDescription>CONSIGNMENT1_MPIApprovedSystemNumber2</StatementDescription>
      <StatementTypeCode>MAS</StatementTypeCode>
    </AdditionalInformation>
";
			AssertContains(expectedXMLConsignment1, icrBuilder.GetXMLMessage());

			var expectedXMLConsignment2 = @"<AdditionalInformation>
      <StatementDescription>CONSIGNMENT2_MPIApprovedSystemNumber1</StatementDescription>
      <StatementTypeCode>MAS</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementDescription>CONSIGNMENT2_MPIApprovedSystemNumber2</StatementDescription>
      <StatementTypeCode>MAS</StatementTypeCode>
    </AdditionalInformation>";
			AssertContains(expectedXMLConsignment2, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentItemPackingLevelMASIfEntered()
		{
			// Registry setting is not required as the MAS value will only be populated to interface if registry setting is on
			SetUpMocks();
			var expectedXMLSnippetItem1 = @"<ConsignmentItem>
      <SequenceNumeric>1</SequenceNumeric>
      <AdditionalInformation>
        <StatementCode>MAS592872</StatementCode>
        <StatementTypeCode>MAS</StatementTypeCode>
      </AdditionalInformation>
      <Commodity>
        <CargoDescription>CONSIGNMENTITEM112345ABCD</CargoDescription>";
			AssertContains("MPI Approved System Number at consignment item level is popultate to message", expectedXMLSnippetItem1, icrBuilder.GetXMLMessage());

			var expectedXMLSnippetItem2 = @"<ConsignmentItem>
      <SequenceNumeric>2</SequenceNumeric>
      <Commodity>
        <CargoDescription>CONSIGNMENTITEM212345ABCD</CargoDescription>";
			AssertContains("MAS element is not built for item 2 as it is empty", expectedXMLSnippetItem2, icrBuilder.GetXMLMessage());
		}

		public void TestHandlingInformation()
		{
			SetUpMocks();
			var expectedXML = @"<AdditionalInformation>
      <StatementDescription>CONSIGNMENT1_HandlingInfo</StatementDescription>
      <StatementTypeCode>HAN</StatementTypeCode>
    </AdditionalInformation>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPaymentTerms()
		{
			SetUpMocks();
			consignment1.Setup(m => m.FreightPaymentMethod).Returns(FreightPaymentMethodList.Codes.CC);
			var expectedXML = @"<Freight>
      <PaymentMethodCode>CC</PaymentMethodCode>
    </Freight>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
		}

		public void TestPopulateCommodityValueAmount()
		{
			SetUpMocks();
			iInwardCargoReportMock.Setup(m => m.IsCarrierCargoReport).Returns(false);
			consignmentItem1.Setup(m => m.Value).Returns(0.00);
			consignmentItem1.Setup(m => m.Currency).Returns(ZString.Empty);
			AssertNotContains("Value can now be zero & if so, it should not generate the commodity value element", @"<ValueAmount currencyID="""">0.00</ValueAmount>", icrBuilder.GetXMLMessage());
			AssertNotContains(@">0.00</ValueAmount>", icrBuilder.GetXMLMessage());
			consignmentItem1.Setup(m => m.Value).Returns(16.9);
			consignmentItem1.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.Australia);
			AssertContains(@"<ValueAmount currencyID=""AUD"">16.9</ValueAmount>", icrBuilder.GetXMLMessage());
			consignmentItem1.Setup(m => m.Currency).Returns(ZString.Empty);
			AssertNotContains(@"<ValueAmount currencyID=""AUD"">16.9</ValueAmount>", icrBuilder.GetXMLMessage());
			consignmentItem1.Setup(m => m.Value).Returns(ZDecimal.Zero);
			AssertNotContains(@"<ValueAmount currencyID=""AUD"">0</ValueAmount>", icrBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentsInLineNumberOrder()
		{
			SetUpMocks();
			iInwardCargoReportMock.Setup(m => m.UseInterfaceSequenceNumber).Returns(true);
			consignment2.Setup(m => m.SequenceNumber).Returns((ZShort)2);
			consignment1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			iInwardCargoReportMock.Setup(m => m.Consignments).Returns(new List<IICRConsignment>() { consignment2.Object, consignment1.Object });
			consignmentItem2.Setup(m => m.SequenceNumber).Returns((ZShort)2);
			consignmentItem1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignment1.Setup(m => m.ConsignmentItems).Returns(new List<IICRConsignmentItem>() { consignmentItem2.Object, consignmentItem1.Object });
			consignment2.Setup(m => m.ConsignmentItems).Returns(new List<IICRConsignmentItem>() { consignmentItem2.Object, consignmentItem1.Object });
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestPopulateConsignmentsInLineNumberOrder.txt")), icrBuilder.GetXMLMessage());
		}

		public void TestPopulateExtraAdditionalInformations()
		{
			SetUpMocks();
			var extraInfosOnConsignment = new[] { ((ZString)"001", (ZString)"TEST DESCRIPTION 001", (ZString)"TEST TYPE CODE 001") };
			consignment2.Setup(m => m.ExtraAdditionalInformations).Returns(extraInfosOnConsignment);
			var expectedXML = @"<AdditionalInformation>
      <StatementCode>001</StatementCode>
      <StatementDescription>TEST DESCRIPTION 001</StatementDescription>
      <StatementTypeCode>TEST TYPE CODE 001</StatementTypeCode>
    </AdditionalInformation>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
			var extraInfosOnConsignmentItem = new[] { ((ZString)"002", (ZString)"TEST DESCRIPTION 002", (ZString)"TEST TYPE CODE 002") };
			consignmentItem2.Setup(m => m.ExtraAdditionalInformations).Returns(extraInfosOnConsignmentItem);
			expectedXML = @"<AdditionalInformation>
        <StatementCode>002</StatementCode>
        <StatementDescription>TEST DESCRIPTION 002</StatementDescription>
        <StatementTypeCode>TEST TYPE CODE 002</StatementTypeCode>
      </AdditionalInformation>";
			AssertContains(expectedXML, icrBuilder.GetXMLMessage());
			consignment2.Setup(m => m.ExtraAdditionalInformations).Returns((IEnumerable<(ZString, ZString, ZString)>)null);
			consignmentItem2.Setup(m => m.ExtraAdditionalInformations).Returns((IEnumerable<(ZString, ZString, ZString)>)null);
		}

		public void TestATFSendsForEmptyContainersWriteOff()
		{
			SetUpMocks();
			consignment1.Setup(m => m.ApprovedTransitionalFacilityCode).Returns("090500");
			consignment1.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.True);
			consignment2.Setup(m => m.ApprovedTransitionalFacilityCode).Returns("27505");
			consignment2.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.True);
			var expectedConsignment1XML = @"<DeliveryDestination>
      <ID>090500</ID>
    </DeliveryDestination>";
			var expectedConsignment2XML = @"<DeliveryDestination>
      <ID>27505</ID>
    </DeliveryDestination>";
			AssertContains(expectedConsignment1XML, icrBuilder.GetXMLMessage());
			AssertContains(expectedConsignment2XML, icrBuilder.GetXMLMessage());
		}

		void SetUpMocks()
		{
			iInwardCargoReportMock = new Mock<IInwardCargoReport>();
			iInwardCargoReportMock.Setup(m => m.TSWReferenceNumber).Returns("ENTRY12345");
			iInwardCargoReportMock.Setup(m => m.SenderReferenceNumber).Returns("C00001165");
			iInwardCargoReportMock.Setup(m => m.IsCarrierCargoReport).Returns(true);
			iInwardCargoReportMock.Setup(m => m.IsSea).Returns(true);
			iInwardCargoReportMock.Setup(m => m.CraftName).Returns("CRAFTNAME");
			iInwardCargoReportMock.Setup(m => m.FlightNo).Returns("FLIGHTNO");
			iInwardCargoReportMock.Setup(m => m.ArrivalDate).Returns(new ZDateTime(2018, 10, 22));
			iInwardCargoReportMock.Setup(m => m.PortOfArrival).Returns("PORTOFARRIVAL");
			iInwardCargoReportMock.Setup(m => m.LloydsNo).Returns("LLOYDSNO");
			iInwardCargoReportMock.Setup(m => m.VoyageNo).Returns("VOYAGENO");
			iInwardCargoReportMock.Setup(m => m.MPIAccountDetails).Returns("IINWARDCARGOREPORTMOCK_MPIACCOUNT");
			additionalInformationMock = new Mock<IAdditionalInformation>();
			var tswAttachment1 = new Mock<ITSWAttachment>();
			var tswAttachment2 = new Mock<ITSWAttachment>();
			tswAttachment1.Setup(m => m.DocType).Returns("CDO");
			tswAttachment1.Setup(m => m.FileName).Returns("TEST1.PDF");
			tswAttachment2.Setup(m => m.DocType).Returns("INV");
			tswAttachment2.Setup(m => m.FileName).Returns("TEST2.PDF");
			additionalInformationMock.Setup(m => m.SupportingDocuments).Returns(new List<ITSWAttachment>() { tswAttachment1.Object, tswAttachment2.Object });
			additionalInformationMock.Setup(m => m.FreeText).Returns("ADDITIONALINFORMATIONMOCK_FREETEXT");
			additionalInformationMock.Setup(m => m.ManualOverrideText).Returns("ADDITIONALINFORMATIONMOCK_MANUALOVERRIDETEXT");
			additionalInformationMock.Setup(m => m.AdditionalStatementText).Returns("ADDITIONALINFORMATIONMOCK_ADDITIONALSTATEMENTTEXT");
			iInwardCargoReportMock.Setup(m => m.AdditionalInformation).Returns(additionalInformationMock.Object);
			iOrganisationSimpleMock = new Mock<IOrganisationSimple>();
			iOrganisationSimpleMock.Setup(m => m.Name).Returns("IORGANISATIONSIMPLEMOCK_NAME");
			iOrganisationSimpleMock.Setup(m => m.CustomsClientCode).Returns("51358595A");
			var iCommunication1 = new Mock<ICommunication>();
			iCommunication1.Setup(m => m.ContactDetail).Returns("COMMUNICATION1_CONTACTDETAIL");
			iCommunication1.Setup(m => m.ContactType).Returns("COMMUNICATION1_CONTACTTYPE");
			var iCommunication2 = new Mock<ICommunication>();
			iCommunication2.Setup(m => m.ContactDetail).Returns("COMMUNICATION2_CONTACTDETAIL");
			iCommunication2.Setup(m => m.ContactType).Returns("COMMUNICATION2_CONTACTTYPE");
			var iContact1 = new Mock<IContact>();
			var iContact2 = new Mock<IContact>();
			iContact1.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });
			iContact2.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });
			iOrganisation1 = new Mock<IOrganisation>();
			iOrganisation1.Setup(m => m.Name).Returns("IORGANISATION1_NAME");
			iOrganisation1.Setup(m => m.City).Returns("IORGANISATION1_CITY");
			iOrganisation1.Setup(m => m.CountryCode).Returns("IORGANISATION1_COUNTRYCODE");
			iOrganisation1.Setup(m => m.Address).Returns("IORGANISATION1_ADDRESS");
			iOrganisation1.Setup(m => m.PostCode).Returns("IORGANISATION1_POSTCODE");
			iOrganisation1.Setup(m => m.CustomsClientCode).Returns("IORGANISATION1_CUSTOMSCLIENTCODE");
			iOrganisation1.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });
			iOrganisation1.Setup(m => m.Contacts).Returns(new List<IContact>() { iContact1.Object, iContact2.Object });
			var iOrganisation2 = new Mock<IOrganisation>();
			iOrganisation2.Setup(m => m.Name).Returns("IORGANISATION2_NAME");
			iOrganisation2.Setup(m => m.City).Returns("IORGANISATION2_CITY");
			iOrganisation2.Setup(m => m.CountryCode).Returns("IORGANISATION2_COUNTRYCODE");
			iOrganisation2.Setup(m => m.Address).Returns("IORGANISATION2_ADDRESS");
			iOrganisation2.Setup(m => m.PostCode).Returns("IORGANISATION2_POSTCODE");
			iOrganisation2.Setup(m => m.CustomsClientCode).Returns("IORGANISATION2_CUSTOMSCLIENTCODE");
			iOrganisation2.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });
			iOrganisation2.Setup(m => m.Contacts).Returns(new List<IContact>() { iContact1.Object, iContact2.Object });
			iInwardCargoReportMock.Setup(m => m.Carrier).Returns(iOrganisationSimpleMock.Object);
			iInwardCargoReportMock.Setup(m => m.UseInterfaceSequenceNumber).Returns(true);
			consignment1ITRDetails = new Mock<ITranshipmentDetails>();
			consignment1ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment1ITRDetails.Setup(m => m.DomesticTranshipmentRequest).Returns(false);
			consignment1ITRDetails.Setup(m => m.ITRImportMode).Returns("4");
			consignment1ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("NZ18");
			consignment1ITRDetails.Setup(m => m.ITRDepartureDate).Returns(new ZDateTime(2019, 02, 18));
			consignment1ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("4");
			consignment1ITRDetails.Setup(m => m.ITRImportCraft).Returns("");
			consignment1ITRDetails.Setup(m => m.PremiseCode).Returns("CONSIGNMENT1_PREMISECODE");
			consignment2ITRDetails = new Mock<ITranshipmentDetails>();
			consignment2ITRDetails.Setup(m => m.InternationalTranshipmentRequest).Returns(true);
			consignment2ITRDetails.Setup(m => m.DomesticTranshipmentRequest).Returns(false);
			consignment2ITRDetails.Setup(m => m.ITRImportMode).Returns("1");
			consignment2ITRDetails.Setup(m => m.ITRVoyageFlight).Returns("55W");
			consignment2ITRDetails.Setup(m => m.ITRDepartureDate).Returns(new ZDateTime(2019, 02, 22));
			consignment2ITRDetails.Setup(m => m.ModeOfTransportForTransfer).Returns("1C");
			consignment2ITRDetails.Setup(m => m.ITRImportCraft).Returns("AAL FREMANTLE");
			consignment2ITRDetails.Setup(m => m.PremiseCode).Returns("CONSIGNMENT2_PREMISECODE");
			consignment1 = new Mock<IICRConsignment>();
			consignment2 = new Mock<IICRConsignmentWithExtraInfos>();
			consignment1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignment1.Setup(m => m.ConsignmentValueInNZD).Returns(5.123);
			consignment1.Setup(m => m.Permits).Returns(new List<ZString>() { "CONSIGNMENT1_PERMITS1", "CONSIGNMENT1_PERMITS2", ZString.Empty });
			consignment1.Setup(m => m.WriteOffRequest).Returns(true);
			consignment1.Setup(m => m.TranshipmentDetails).Returns(consignment1ITRDetails.Object);
			consignment1.Setup(m => m.IsConsolidation).Returns(true);
			consignment1.Setup(m => m.MAFContainerDeclaration).Returns(true);
			consignment1.Setup(m => m.MAFContainerStatements).Returns(new List<ZString>() { "CONSIGNMENT1_MAFCONTAINERSTATEMENT1", "CONSIGNMENT1_MAFCONTAINERSTATEMENT2" });
			consignment1.Setup(m => m.MPIApprovedSystemNumbers).Returns(new List<ZString>() { "CONSIGNMENT1_MPIApprovedSystemNumber1", "CONSIGNMENT1_MPIApprovedSystemNumber2" });
			consignment1.Setup(m => m.MPIAccountDetails).Returns("CONSIGNMENT1_MPIACCOUNT");
			consignment1.Setup(m => m.HandlingInformation).Returns("CONSIGNMENT1_HandlingInfo");
			consignment1.Setup(m => m.MasterBill).Returns("CONSIGNMENT1_MASTERBILL");
			consignment1.Setup(m => m.PortOfOrigin).Returns("CONSIGNMENT1_PORTOFORIGIN");
			consignment1.Setup(m => m.GoodsLocation).Returns("CONSIGNMENT1_GOODSLOCATION");
			consignment1.Setup(m => m.PortOfLoading).Returns("CONSIGNMENT1_PORTOFLOADING");
			consignment1.Setup(m => m.BillNumber).Returns("CONSIGNMENT1_BILLNUMBER");
			consignment1.Setup(m => m.BillType).Returns("CONSIGNMENT1_BILLTYPE");
			consignment1.Setup(m => m.PortOfDischarge).Returns("CONSIGNMENT1_PORTOFDISCHARGE");
			consignment1.Setup(m => m.FreightPaymentMethod).Returns("CONSIGNMENT1_FREIGHTPAYMENTMETHOD");
			consignment1.Setup(m => m.Deconsolidator).Returns(iOrganisationSimpleMock.Object);
			consignment1.Setup(m => m.ContainerPackingLocations).Returns(new List<IOrganisation>() { iOrganisation1.Object, iOrganisation2.Object });
			consignment1.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment1.Setup(m => m.TranshipmentPorts).Returns(new List<ZString>() { "CONSIGNMENT1_TRANSHIPMENTPORTS1", "CONSIGNMENT1_TRANSHIPMENTPORTS2", ZString.Empty });
			consignment1.Setup(m => m.IsGSTPrePaid).Returns("Y");
			consignment1.Setup(m => m.VendorIdentifier).Returns("1234567");
			consignment1.Setup(m => m.IsLinkEmptyContainer).Returns(false);
			consignment1.Setup(m => m.ApprovedTransitionalFacilityCode).Returns("090500");
			consignment1.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.False);
			consignment1.Setup(m => m.DeliveryNotifyParties).Returns(new List<IOrganisationSimple> { iOrganisation1.Object, iOrganisation2.Object });
			consignment1.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "11111A", "PYKE" });
			consignment2.Setup(m => m.SequenceNumber).Returns((ZShort)2);
			consignment2.Setup(m => m.ConsignmentValueInNZD).Returns(6.2);
			consignment2.Setup(m => m.Permits).Returns(new List<ZString>() { "CONSIGNMENT2_PERMITS1", "CONSIGNMENT2_PERMITS2" });
			consignment2.Setup(m => m.WriteOffRequest).Returns(true);
			consignment2.Setup(m => m.TranshipmentDetails).Returns(consignment2ITRDetails.Object);
			consignment2.Setup(m => m.IsConsolidation).Returns(true);
			consignment2.Setup(m => m.MAFContainerDeclaration).Returns(true);
			consignment2.Setup(m => m.MAFContainerStatements).Returns(new List<ZString>() { "CONSIGNMENT2_MAFCONTAINERSTATEMENT1", "CONSIGNMENT2_MAFCONTAINERSTATEMENT2" });
			consignment2.Setup(m => m.MPIApprovedSystemNumbers).Returns(new List<ZString>() { "CONSIGNMENT2_MPIApprovedSystemNumber1", "CONSIGNMENT2_MPIApprovedSystemNumber2" });
			consignment2.Setup(m => m.MPIAccountDetails).Returns("");
			consignment2.Setup(m => m.HandlingInformation).Returns("");
			consignment2.Setup(m => m.MasterBill).Returns("CONSIGNMENT2_MASTERBILL");
			consignment2.Setup(m => m.PortOfOrigin).Returns("CONSIGNMENT2_PORTOFORIGIN");
			consignment2.Setup(m => m.GoodsLocation).Returns("CONSIGNMENT2_GOODSLOCATION");
			consignment2.Setup(m => m.PortOfLoading).Returns("CONSIGNMENT2_PORTOFLOADING");
			consignment2.Setup(m => m.BillNumber).Returns("CONSIGNMENT2_BILLNUMBER");
			consignment2.Setup(m => m.BillType).Returns("CONSIGNMENT2_BILLTYPE");
			consignment2.Setup(m => m.PortOfDischarge).Returns("CONSIGNMENT2_PORTOFDISCHARGE");
			consignment2.Setup(m => m.FreightPaymentMethod).Returns("CONSIGNMENT2_FREIGHTPAYMENTMETHOD");
			consignment2.Setup(m => m.Deconsolidator).Returns(iOrganisationSimpleMock.Object);
			consignment2.Setup(m => m.ContainerPackingLocations).Returns(new List<IOrganisation>() { iOrganisation1.Object, iOrganisation2.Object });
			consignment2.Setup(m => m.DeliverToParty).Returns(iOrganisation1.Object);
			consignment2.Setup(m => m.TranshipmentPorts).Returns(new List<ZString>() { "CONSIGNMENT2_TRANSHIPMENTPORTS1", "CONSIGNMENT2_TRANSHIPMENTPORTS2" });
			consignment2.Setup(m => m.IsGSTPrePaid).Returns("N");
			consignment2.Setup(m => m.VendorIdentifier).Returns("2345678");
			consignment2.Setup(m => m.IsLinkEmptyContainer).Returns(false);
			consignment2.Setup(m => m.ApprovedTransitionalFacilityCode).Returns("090501");
			consignment2.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.False);
			consignment2.Setup(m => m.DeliveryNotifyParties).Returns(new List<IOrganisationSimple> { iOrganisation1.Object, iOrganisation2.Object });
			consignment2.Setup(m => m.NotifyPartyCodes).Returns(new ZString[] { "22222B", "TARTH" });
			var iTransportEquipment1 = new Mock<ITransportEquipment>();
			var iTransportEquipment2 = new Mock<ITransportEquipment>();
			iTransportEquipment3 = new Mock<ITransportEquipment>();
			iTransportEquipment1.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSIZE");
			iTransportEquipment1.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT1_CONTAINERNUMBER");
			iTransportEquipment1.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSTATUS");
			iTransportEquipment1.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT1_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment1.Setup(m => m.StowPosition).Returns("ITRANSPORTEQUIPMENT1_CONTAINERSTOWPOSITION");
			iTransportEquipment1.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "ITRANSPORTEQUIPMENT1_SEALNUMBER1", "ITRANSPORTEQUIPMENT1_SEALNUMBER2" });
			iTransportEquipment1.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			iTransportEquipment1.Setup(m => m.StuffingLocation).Returns(ZGuid.NewZGuid());
			iTransportEquipment1.Setup(m => m.MessageSequence).Returns(1);
			iTransportEquipment2.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT2_CONTAINERSIZE");
			iTransportEquipment2.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT2_CONTAINERNUMBER");
			iTransportEquipment2.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT2_CONTAINERSTATUS");
			iTransportEquipment2.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT2_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment2.Setup(m => m.StowPosition).Returns("ITRANSPORTEQUIPMENT2_CONTINERSTOWPOSITION");
			iTransportEquipment2.Setup(m => m.SealNumbers).Returns(new List<ZString>() { "ITRANSPORTEQUIPMENT2_SEALNUMBER1", "ITRANSPORTEQUIPMENT2_SEALNUMBER2" });
			iTransportEquipment2.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			iTransportEquipment2.Setup(m => m.StuffingLocation).Returns(ZGuid.NewZGuid());
			iTransportEquipment2.Setup(m => m.MessageSequence).Returns(2);
			iTransportEquipment3.Setup(m => m.Size).Returns("ITRANSPORTEQUIPMENT3_CONTAINERSIZE");
			iTransportEquipment3.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT3_CONTAINERNUMBER");
			iTransportEquipment3.Setup(m => m.Status).Returns("ITRANSPORTEQUIPMENT3_CONTAINERSTATUS");
			iTransportEquipment3.Setup(m => m.AttachedEquipmentCode).Returns("ITRANSPORTEQUIPMENT3_CONTAINERATTACHEDEQUIPMENTCODE");
			iTransportEquipment3.Setup(m => m.PK).Returns(ZGuid.NewZGuid());
			iTransportEquipment3.Setup(m => m.MessageSequence).Returns(3);
			iTransportEquipment3.Setup(m => m.ContainerMode).Returns(ContainerModeList.Codes.ROR);
			consignment1.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { iTransportEquipment1.Object, iTransportEquipment2.Object, iTransportEquipment3.Object });
			consignment2.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { iTransportEquipment1.Object, iTransportEquipment2.Object, iTransportEquipment3.Object });
			consignment1.Setup(m => m.HasContainers).Returns(true);
			consignment2.Setup(m => m.HasContainers).Returns(true);
			iInwardCargoReportMock.Setup(m => m.Consignments).Returns(new List<IICRConsignment>() { consignment1.Object, consignment2.Object });
			consignee1 = new Mock<IPartyInformation>();
			consignee1.Setup(m => m.CustomsClientCode).Returns("CONSIGNEE_CUSTOMSCLIENTCODE");
			consignee1.Setup(m => m.Name).Returns("CONSIGNEE_NAME");
			consignee1.Setup(m => m.City).Returns("CONSIGNEE_CITY");
			consignee1.Setup(m => m.CountryCode).Returns("CONSIGNEE_COUNTRYCODE");
			consignee1.Setup(m => m.CountryRegion).Returns("CONSIGNEE_COUNTRYREGION");
			consignee1.Setup(m => m.Address).Returns("CONSIGNEE_ADDRESS");
			consignee1.Setup(m => m.PostCode).Returns("CONSIGNEE_POSTCODE");
			consignee2 = new Mock<IPartyInformation>();
			consignee2.Setup(m => m.CustomsClientCode).Returns("");
			consignee2.Setup(m => m.Name).Returns("CONSIGNEE2_NAME");
			consignee2.Setup(m => m.City).Returns("CONSIGNEE2_CITY");
			consignee2.Setup(m => m.CountryCode).Returns("CONSIGNEE2_COUNTRYCODE");
			consignee2.Setup(m => m.CountryRegion).Returns("CONSIGNEE2_COUNTRYREGION");
			consignee2.Setup(m => m.Address).Returns("CONSIGNEE2_ADDRESS");
			consignee2.Setup(m => m.PostCode).Returns("CONSIGNEE2_POSTCODE");
			consignor = new Mock<IPartyInformation>();
			consignor.Setup(m => m.CustomsClientCode).Returns("CONSIGNOR_CUSTOMSCLIENTCODE");
			consignor.Setup(m => m.Name).Returns("CONSIGNOR_NAME");
			consignor.Setup(m => m.City).Returns("CONSIGNOR_CITY");
			consignor.Setup(m => m.CountryCode).Returns("CONSIGNOR_COUNTRYCODE");
			consignor.Setup(m => m.CountryRegion).Returns("CONSIGNOR_COUNTRYREGION");
			consignor.Setup(m => m.Address).Returns("CONSIGNOR_ADDRESS");
			consignor.Setup(m => m.PostCode).Returns("CONSIGNOR_POSTCODE");
			var notify = new Mock<IPartyInformation>();
			notify.Setup(m => m.CustomsClientCode).Returns("NOTIFY_CUSTOMSCLIENTCODE");
			notify.Setup(m => m.Name).Returns("NOTIFY_NAME");
			notify.Setup(m => m.City).Returns("NOTIFY_CITY");
			notify.Setup(m => m.CountryCode).Returns("NOTIFY_COUNTRYCODE");
			notify.Setup(m => m.CountryRegion).Returns("NOTIFY_COUNTRYREGION");
			notify.Setup(m => m.Address).Returns("NOTIFY_ADDRESS");
			notify.Setup(m => m.PostCode).Returns("NOTIFY_POSTCODE");
			consignee1.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });
			consignee2.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });
			consignor.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });
			notify.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });
			consignment1.Setup(m => m.Consignee).Returns(consignee1.Object);
			consignment1.Setup(m => m.Consignor).Returns(consignor.Object);
			consignment1.Setup(m => m.NotifyParty).Returns(notify.Object);
			consignment2.Setup(m => m.Consignee).Returns(consignee2.Object);
			consignment2.Setup(m => m.Consignor).Returns(consignor.Object);
			consignment2.Setup(m => m.NotifyParty).Returns(notify.Object);
			var iDeclarant = new Mock<IDeclarant>();
			iDeclarant.Setup(m => m.Communications).Returns(new List<ICommunication>() { iCommunication1.Object, iCommunication2.Object });
			iDeclarant.Setup(m => m.DeclarantID).Returns("DECLARANTID");
			iInwardCargoReportMock.Setup(m => m.Declarant).Returns(iDeclarant.Object);
			consignmentItem1 = new Mock<IICRConsignmentItem>();
			consignmentItem2 = new Mock<IICRConsignmentItemWithExtraInfos>();
			var consignmentItem3 = new Mock<IICRConsignmentItem>();
			consignment1.Setup(m => m.ConsignmentItems).Returns(new List<IICRConsignmentItem>() { consignmentItem1.Object, consignmentItem2.Object, consignmentItem3.Object });
			consignment2.Setup(m => m.ConsignmentItems).Returns(new List<IICRConsignmentItem>() { consignmentItem1.Object, consignmentItem2.Object, consignmentItem3.Object });
			consignmentItem1.Setup(m => m.SendFlashpointTemp).Returns(true);
			consignmentItem1.Setup(m => m.FlashpointTempInCelsius).Returns(1m);
			consignmentItem1.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM1_12345ABCD");
			consignmentItem1.Setup(m => m.Value).Returns(2m);
			consignmentItem1.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.Australia);
			consignmentItem1.Setup(m => m.IdentityNumber).Returns("CONSIGNMENTITEM1_IDENTITYNUMBER");
			consignmentItem1.Setup(m => m.IdentityType).Returns("CONSIGNMENTITEM1_IDENTITYTYPE");
			consignmentItem1.Setup(m => m.GrossWeightInKg).Returns(3m);
			consignmentItem1.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM1_GOODSORIGINCOUNTRY");
			consignmentItem1.Setup(m => m.PackageQty).Returns(2);
			consignmentItem1.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM1_PACKAGETYPE");
			consignmentItem1.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT1_CONTAINERNUMBER");
			consignmentItem1.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem1.Setup(m => m.MPIApprovedSystemNumber).Returns("MAS592872");
			consignmentItem1.Setup(m => m.SequenceNumber).Returns((ZShort)1);
			consignmentItem2.Setup(m => m.SendFlashpointTemp).Returns(true);
			consignmentItem2.Setup(m => m.FlashpointTempInCelsius).Returns(4m);
			consignmentItem2.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM2_12345ABCD");
			consignmentItem2.Setup(m => m.Value).Returns(5m);
			consignmentItem2.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.UnitedStates);
			consignmentItem2.Setup(m => m.IdentityNumber).Returns("CONSIGNMENTITEM2_IDENTITYNUMBER");
			consignmentItem2.Setup(m => m.IdentityType).Returns("CONSIGNMENTITEM2_IDENTITYTYPE");
			consignmentItem2.Setup(m => m.GrossWeightInKg).Returns(6m);
			consignmentItem2.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM2_GOODSORIGINCOUNTRY");
			consignmentItem2.Setup(m => m.PackageQty).Returns(2);
			consignmentItem2.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM2_PACKAGETYPE");
			consignmentItem2.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT2_CONTAINERNUMBER");
			consignmentItem2.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem2.Setup(m => m.SequenceNumber).Returns((ZShort)2);
			consignmentItem3.Setup(m => m.FlashpointTempInCelsius).Returns(4m);
			consignmentItem3.Setup(m => m.GoodsDescription).Returns("CONSIGNMENTITEM3_12345ABCD");
			consignmentItem3.Setup(m => m.Value).Returns(5m);
			consignmentItem3.Setup(m => m.Currency).Returns(Core.Constants.CurrencyCodes.UnitedStates);
			consignmentItem3.Setup(m => m.IdentityNumber).Returns("CONSIGNMENTITEM3_IDENTITYNUMBER");
			consignmentItem3.Setup(m => m.IdentityType).Returns("CONSIGNMENTITEM3_IEDNTITYTYPE");
			consignmentItem3.Setup(m => m.GrossWeightInKg).Returns(6m);
			consignmentItem3.Setup(m => m.GoodsOriginCountry).Returns("CONSIGNMENTITEM3_GOODSORIGINCOUNTRY");
			consignmentItem3.Setup(m => m.PackageQty).Returns(2);
			consignmentItem3.Setup(m => m.PackageType).Returns("CONSIGNMENTITEM2_PACKAGETYPE");
			consignmentItem3.Setup(m => m.ContainerNumber).Returns("ITRANSPORTEQUIPMENT3_CONTAINERNUMBER");
			consignmentItem3.Setup(m => m.IsEmptyContainer).Returns(false);
			consignmentItem3.Setup(m => m.SequenceNumber).Returns((ZShort)3);
			consignmentItem3.Setup(m => m.SendFlashpointTemp).Returns(true);
			var classifications1 = new Mock<IClassification>();
			var classifications2 = new Mock<IClassification>();
			classifications1.Setup(m => m.Classification).Returns("CLASSIFICATIONS1_CLASSIFICATION");
			classifications1.Setup(m => m.ClassificationTypeCode).Returns("SSO");
			classifications2.Setup(m => m.Classification).Returns("CLASSIFICATIONS2_CLASSIFICATION");
			classifications2.Setup(m => m.ClassificationTypeCode).Returns("SSI");
			consignmentItem1.Setup(m => m.Classifications).Returns(new List<IClassification>() { classifications1.Object, classifications2.Object });
			consignmentItem2.Setup(m => m.Classifications).Returns(new List<IClassification>() { classifications1.Object, classifications2.Object });
			consignmentItem3.Setup(m => m.Classifications).Returns(new List<IClassification>() { classifications1.Object, classifications2.Object });
			var iTemperatureRequirements = new Mock<ITemperatureRequirements>();
			iTemperatureRequirements.Setup(m => m.StorageTemp).Returns(1m);
			iTemperatureRequirements.Setup(m => m.MinStorageTemp).Returns(2m);
			iTemperatureRequirements.Setup(m => m.MaxStorageTemp).Returns(3m);
			consignmentItem1.Setup(m => m.Temperatures).Returns(iTemperatureRequirements.Object);
			consignmentItem2.Setup(m => m.Temperatures).Returns(iTemperatureRequirements.Object);
			consignmentItem3.Setup(m => m.Temperatures).Returns(iTemperatureRequirements.Object);
			icrBuilder = new ICRMessageBuilder(iInwardCargoReportMock.Object, TSWTransactionTypes.Original, "00009908C");
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		Mock<IInwardCargoReport> iInwardCargoReportMock;
		ICRMessageBuilder icrBuilder;
		Mock<IAdditionalInformation> additionalInformationMock;
		Mock<IOrganisationSimple> iOrganisationSimpleMock;
		Mock<IICRConsignment> consignment1;
		Mock<IICRConsignmentWithExtraInfos> consignment2;
		Mock<ITransportEquipment> iTransportEquipment3;
		Mock<IPartyInformation> consignee1;
		Mock<IPartyInformation> consignee2;
		Mock<IOrganisation> iOrganisation1;
		Mock<IICRConsignmentItem> consignmentItem1;
		Mock<IICRConsignmentItemWithExtraInfos> consignmentItem2;
		Mock<IPartyInformation> consignor;
		Mock<ITranshipmentDetails> consignment1ITRDetails;
		Mock<ITranshipmentDetails> consignment2ITRDetails;
		Mock<ITranshipmentDetails> consignment1DTRDetails;
		EmbeddedResourceRetriever embeddedResourceRetriever;

		public interface IICRConsignmentWithExtraInfos : IICRConsignment, IExtraAdditionalInformationParent
		{
		}

		public interface IICRConsignmentItemWithExtraInfos : IICRConsignmentItem, IExtraAdditionalInformationParent
		{
		}

		void AssertConsignmentValue(bool isCarrierCargoReport)
		{
			iInwardCargoReportMock.Setup(m => m.IsCarrierCargoReport).Returns(isCarrierCargoReport);
			consignment1.Setup(m => m.ConsignmentValueInNZD).Returns(0.00);
			AssertNotContains("Value can now be zero & if so, it should not generate the commodity value element", @"<ValueAmount currencyID=""NZD"">0.00</ValueAmount>", icrBuilder.GetXMLMessage());
			AssertNotContains(@">0.00</ValueAmount>", icrBuilder.GetXMLMessage());
			consignment1.Setup(m => m.ConsignmentValueInNZD).Returns(34.12);
			AssertContains(@"<ValueAmount currencyID=""NZD"">34.12</ValueAmount>", icrBuilder.GetXMLMessage());
			consignment1.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.True);
			AssertNotContains(@"<ValueAmount currencyID=""NZD"">34.12</ValueAmount>", icrBuilder.GetXMLMessage());
			consignment1.Setup(m => m.IsLinkEmptyContainer).Returns(ZBool.False);
			consignment1.Setup(m => m.ConsignmentValueInNZD).Returns(0.00);
			AssertNotContains("Zero value is not required to be sent whether for carrier reporting or otherwise", @"<ValueAmount currencyID=""NZD"">0</ValueAmount>", icrBuilder.GetXMLMessage());
		}
	}
}
