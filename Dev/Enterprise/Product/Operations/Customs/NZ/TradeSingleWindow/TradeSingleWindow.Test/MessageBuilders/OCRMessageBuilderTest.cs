using System.Collections.Generic;
using CargoWise.IO;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class OCRMessageBuilderTest : TSWMessageBuilderTest
	{
		public void TestOCRMessageOriginal()
		{
			SetUpMocks();
			AssertMultilineASCIIEquals(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestOCRMessageOriginal.txt")), ocrBuilder.GetXMLMessage());
		}

		public void TestOCRMessageReplace()
		{
			SetUpMocks();
			ocrBuilder = new OCRMessageBuilder(outwardCargoReportHeaderMock.Object, TSWTransactionTypes.Replace, "00009908C");
			AssertMultilineASCIIEquals(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestOCRMessageReplace.txt")), ocrBuilder.GetXMLMessage());
		}

		public void TestOCRMessageCancel()
		{
			SetUpMocks();
			ocrBuilder = new OCRMessageBuilder(outwardCargoReportHeaderMock.Object, TSWTransactionTypes.Cancel, "00009908C");
			AssertMultilineASCIIEquals(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestOCRMessageCancel.txt")), ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateReferenceNo()
		{
			SetUpMocks();
			ocrBuilder = new OCRMessageBuilder(outwardCargoReportHeaderMock.Object, TSWTransactionTypes.Replace, "00009908C");
			AssertContains("<ID>ENTRY12345</ID>", ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateMessageType()
		{
			SetUpMocks();
			AssertContains("<TypeCode>OCR</TypeCode>", ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateTransType()
		{
			SetUpMocks();
			ocrBuilder = new OCRMessageBuilder(outwardCargoReportHeaderMock.Object, TSWTransactionTypes.Cancel, "00009908C");
			AssertContains("<FunctionCode>1</FunctionCode>", ocrBuilder.GetXMLMessage());
			ocrBuilder = new OCRMessageBuilder(outwardCargoReportHeaderMock.Object, TSWTransactionTypes.Change, "00009908C");
			AssertContains("<FunctionCode>4</FunctionCode>", ocrBuilder.GetXMLMessage());
			ocrBuilder = new OCRMessageBuilder(outwardCargoReportHeaderMock.Object, TSWTransactionTypes.Completion, "00009908C");
			AssertContains("<FunctionCode>22</FunctionCode>", ocrBuilder.GetXMLMessage());
			ocrBuilder = new OCRMessageBuilder(outwardCargoReportHeaderMock.Object, TSWTransactionTypes.None, "00009908C");
			AssertContains("<FunctionCode />", ocrBuilder.GetXMLMessage());
			ocrBuilder = new OCRMessageBuilder(outwardCargoReportHeaderMock.Object, TSWTransactionTypes.Original, "00009908C");
			AssertContains("<FunctionCode>9</FunctionCode>", ocrBuilder.GetXMLMessage());
			ocrBuilder = new OCRMessageBuilder(outwardCargoReportHeaderMock.Object, TSWTransactionTypes.Replace, "00009908C");
			AssertContains("<FunctionCode>5</FunctionCode>", ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateSubmitter()
		{
			SetUpMocks();
			var expetedSubmitterXML = @"<Submitter>
    <ID>00009908C</ID>
  </Submitter>";
			AssertContains(expetedSubmitterXML, ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalDocs()
		{
			SetUpMocks();
			var expectedAdditionalDocsXML = @"  <AdditionalDocument>
    <CategoryCode>CDO</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""TEST1.PDF"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>
  <AdditionalDocument>
    <CategoryCode>INV</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""TEST2.PDF"">ATTACHED</ImageBinaryObject>";
			AssertContains(expectedAdditionalDocsXML, ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalInfoOriginal()
		{
			SetUpMocks();
			var expectedAdditionalInfoXML = @"<AdditionalInformation>
    <Content>FREETEXTTEST</Content>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>Y</StatementCode>
    <StatementTypeCode>CON</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <RequestOverrideCode>Y</RequestOverrideCode>
    <StatementDescription>MANUALOVERRIDETEXTTEST</StatementDescription>
    <StatementTypeCode>ALP</StatementTypeCode>
  </AdditionalInformation>";
			AssertContains(expectedAdditionalInfoXML, ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalInfoCancel()
		{
			SetUpMocks();
			var expectedAdditionalInfoXML = @"<AdditionalInformation>
    <StatementDescription>ADDITIONALSTATEMENTTEXTTEST</StatementDescription>
    <StatementTypeCode>AES</StatementTypeCode>
  </AdditionalInformation>";
			ocrBuilder = new OCRMessageBuilder(outwardCargoReportHeaderMock.Object, TSWTransactionTypes.Cancel, "00009908C");
			AssertContains(expectedAdditionalInfoXML, ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateAdditionalInfoReplace()
		{
			SetUpMocks();
			var expectedAdditionalInfoXML = @"<AdditionalInformation>
    <Content>FREETEXTTEST</Content>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>Y</StatementCode>
    <StatementTypeCode>CON</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <RequestOverrideCode>Y</RequestOverrideCode>
    <StatementDescription>MANUALOVERRIDETEXTTEST</StatementDescription>
    <StatementTypeCode>ALP</StatementTypeCode>
  </AdditionalInformation>";
			ocrBuilder = new OCRMessageBuilder(outwardCargoReportHeaderMock.Object, TSWTransactionTypes.Replace, "00009908C");
			AssertContains(expectedAdditionalInfoXML, ocrBuilder.GetXMLMessage());
		}

		public void TestFreeTextInfo()
		{
			SetUpMocks();
			var expectedFreeTextInfoXML = @"<AdditionalInformation>
    <Content>FREETEXTTEST</Content>
  </AdditionalInformation>";
			AssertContains(expectedFreeTextInfoXML, ocrBuilder.GetXMLMessage());
		}

		public void TestConsolidationInfo()
		{
			SetUpMocks();
			var consolidationInfo = @"<AdditionalInformation>
    <StatementCode>Y</StatementCode>
    <StatementTypeCode>CON</StatementTypeCode>
  </AdditionalInformation>";
			AssertContains(consolidationInfo, ocrBuilder.GetXMLMessage());
		}

		public void TestManualOverrideInfo()
		{
			SetUpMocks();
			var manualOverrideInfoXML = @"<AdditionalInformation>
    <RequestOverrideCode>Y</RequestOverrideCode>
    <StatementDescription>MANUALOVERRIDETEXTTEST</StatementDescription>
    <StatementTypeCode>ALP</StatementTypeCode>
  </AdditionalInformation>";
			AssertContains(manualOverrideInfoXML, ocrBuilder.GetXMLMessage());
		}

		public void TestChangeCancelReason()
		{
			SetUpMocks();
			var changeCancelReasonXML = @"<AdditionalInformation>
    <StatementDescription>ADDITIONALSTATEMENTTEXTTEST</StatementDescription>
    <StatementTypeCode>AES</StatementTypeCode>
  </AdditionalInformation>";
			ocrBuilder = new OCRMessageBuilder(outwardCargoReportHeaderMock.Object, TSWTransactionTypes.Cancel, "00009908C");
			AssertContains(changeCancelReasonXML, ocrBuilder.GetXMLMessage());
			ocrBuilder = new OCRMessageBuilder(outwardCargoReportHeaderMock.Object, TSWTransactionTypes.Replace, "00009908C");
			AssertContains(changeCancelReasonXML, ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateBorderTMSea()
		{
			SetUpMocks();
			var expectedBorderTransportMeansXML = @"<BorderTransportMeans>
    <Name>CRAFTNAMETEST</Name>
    <ID>LLOYDSNOTEST</ID>
    <TypeCode>1</TypeCode>
    <DepartureDateTime formatCode=""102"" />
    <JourneyID>VOYAGENOTEST</JourneyID>
    <Itinerary>
      <SequenceNumeric>1</SequenceNumeric>
      <RoutingCountryCode>AU</RoutingCountryCode>
    </Itinerary>
    <Itinerary>
      <SequenceNumeric>2</SequenceNumeric>
      <RoutingCountryCode>NZ</RoutingCountryCode>
    </Itinerary>
  </BorderTransportMeans>";
			AssertContains(expectedBorderTransportMeansXML, ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateBorderTMAir()
		{
			SetUpMocks();
			var expectedBorderTransportMeansXML = @"<BorderTransportMeans>
    <Name>FLIGHTNOTEST</Name>
    <TypeCode>4</TypeCode>
    <DepartureDateTime formatCode=""102"" />
    <Itinerary>
      <SequenceNumeric>1</SequenceNumeric>
      <RoutingCountryCode>AU</RoutingCountryCode>
    </Itinerary>
    <Itinerary>
      <SequenceNumeric>2</SequenceNumeric>
      <RoutingCountryCode>NZ</RoutingCountryCode>
    </Itinerary>
  </BorderTransportMeans>";
			outwardCargoReportHeaderMock.Setup(m => m.IsSea).Returns(false);
			AssertContains(expectedBorderTransportMeansXML, ocrBuilder.GetXMLMessage());
		}

		public void TestFlightNoIsCapitalized()
		{
			SetUpMocks();
			outwardCargoReportHeaderMock.Setup(m => m.IsSea).Returns(false);
			outwardCargoReportHeaderMock.Setup(m => m.FlightNo).Returns("nz210");
			outwardCargoReportHeaderMock.Setup(m => m.DepartureDate).Returns(new ZDateTime(2025, 05, 06));
			var expectedBorderTransportMeansXML = @"<BorderTransportMeans>
    <Name>NZ210</Name>
    <TypeCode>4</TypeCode>
    <DepartureDateTime formatCode=""102"">20250506</DepartureDateTime>";
			AssertContains("Flight Number should be in Uppercase", expectedBorderTransportMeansXML, ocrBuilder.GetXMLMessage());
		}

		public void TestCraftAndVoyageNumberIsCapitalized()
		{
			SetUpMocks();
			outwardCargoReportHeaderMock.Setup(m => m.CraftName).Returns("Hyogo Maru");
			outwardCargoReportHeaderMock.Setup(m => m.DepartureDate).Returns(new ZDateTime(2025, 05, 18));
			outwardCargoReportHeaderMock.Setup(m => m.LloydsNo).Returns("5892347");
			outwardCargoReportHeaderMock.Setup(m => m.VoyageNo).Returns("128n");
			var expectedBorderTransportMeansXML = @"<BorderTransportMeans>
    <Name>HYOGO MARU</Name>
    <ID>5892347</ID>
    <TypeCode>1</TypeCode>
    <DepartureDateTime formatCode=""102"">20250518</DepartureDateTime>
    <JourneyID>128N</JourneyID>";
			AssertContains(expectedBorderTransportMeansXML, ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateRoutingCountries()
		{
			SetUpMocks();
			var expectedRoutingCountriesXML = @"<Itinerary>
      <SequenceNumeric>1</SequenceNumeric>
      <RoutingCountryCode>AU</RoutingCountryCode>
    </Itinerary>
    <Itinerary>
      <SequenceNumeric>2</SequenceNumeric>
      <RoutingCountryCode>NZ</RoutingCountryCode>
    </Itinerary>";
			AssertContains(expectedRoutingCountriesXML, ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateCarrier()
		{
			SetUpMocks();
			var expectedCarrierXML = @"<Carrier>
    <Name>CONSOLIDATORNAME</Name>
    <ID>51358595A</ID>
  </Carrier>";
			AssertContains(expectedCarrierXML, ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentsSea()
		{
			SetUpMocks();
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestOCRPopulateConsignmentsSea.txt")), ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateConsignmentsAir()
		{
			SetUpMocks();
			outwardCargoReportHeaderMock.Setup(m => m.IsSea).Returns(false);
			AssertContains(embeddedResourceRetriever.GetString(GetEmbeddedResourcePath("TestOCRPopulateConsignmentsAir.txt")), ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateClearanceNumber()
		{
			SetUpMocks();
			var expectedClearance1 = @"<AdditionalDocument>
      <ID>12345</ID>
      <TypeCode>EDO</TypeCode>
    </AdditionalDocument>";
			var expectedClearance2 = @"<AdditionalDocument>
      <ID>67890</ID>
      <TypeCode>EDO</TypeCode>
    </AdditionalDocument>";
			var expectedClearance3 = @"<AdditionalDocument>
      <ID></ID>
      <TypeCode>EDO</TypeCode>
    </AdditionalDocument>";
			var xmlMessage = ocrBuilder.GetXMLMessage();
			AssertContains(expectedClearance1, xmlMessage);
			AssertContains(expectedClearance2, xmlMessage);
			AssertNotContains(expectedClearance3, xmlMessage);
		}

		public void TestPopulateBillNumber()
		{
			SetUpMocks();
			var expectedBillNumberXML = @"<AssociatedTransportDocument>
      <ID>BILLNUMBERTEST</ID>
      <TypeCode>BILLTYPETEST</TypeCode>
    </AssociatedTransportDocument>";
			AssertContains(expectedBillNumberXML, ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateMultipleNotifyParties()
		{
			SetUpMocks();
			var expectedNotifyParties = @"    <NotifyParty>
      <ID>CCP Number</ID>
      <RoleCode>N2</RoleCode>
    </NotifyParty>
    <NotifyParty>
      <ID>PORT</ID>
      <RoleCode>N2</RoleCode>
    </NotifyParty>
    <NotifyParty>
      <Name>NOTIFYPARTYNAMETEST</Name>
      <RoleCode>N2</RoleCode>
      <Communication>
        <ID>NOTIFYPARTYEMAILTEST</ID>
        <TypeID>EM</TypeID>
      </Communication>
    </NotifyParty>
";
			AssertContains(expectedNotifyParties, ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateSendersRef()
		{
			SetUpMocks();
			AssertContains("<FunctionalReferenceID>C00001165</FunctionalReferenceID>", ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateTransportContractID()
		{
			SetUpMocks();
			var expectedTransportContractDocumentXML = @"<TransportContractDocument>
      <ID>MASTERBILLNUMBERTEST</ID>
      <TypeCode>MB</TypeCode>
      <Consolidator>
        <ID>51358595A</ID>
      </Consolidator>
    </TransportContractDocument>";
			AssertContains(expectedTransportContractDocumentXML, ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateTransportContractName()
		{
			SetUpMocks();
			var expectedTransportContractDocumentXML = @"<TransportContractDocument>
      <ID>MASTERBILLNUMBERTEST</ID>
      <TypeCode>MB</TypeCode>
      <Consolidator>
        <Name>CONSOLIDATORNAME</Name>
      </Consolidator>
    </TransportContractDocument>";
			iOrganisationSimpleMock.Setup(m => m.CustomsClientCode).Returns(ZString.Empty);
			AssertContains(expectedTransportContractDocumentXML, ocrBuilder.GetXMLMessage());
		}

		public void TestPopulateContainers()
		{
			SetUpMocks();
			var expectedContainersXML = @"<TransportEquipment>
      <SequenceNumeric>1</SequenceNumeric>
      <FullnessCode>STATUSTEST</FullnessCode>
      <ID>CONTAINERNUMBERTEST</ID>
    </TransportEquipment>
    <TransportEquipment>
      <SequenceNumeric>2</SequenceNumeric>
      <FullnessCode>STATUSTEST2</FullnessCode>
      <ID>CONTAINERNUMBERTEST2</ID>
    </TransportEquipment>";
			AssertContains(expectedContainersXML, ocrBuilder.GetXMLMessage());
		}

		public void TestPopulatePortOfDeparture()
		{
			SetUpMocks();
			var expectedPortOfDepartureXML = @"<ExitOffice>
    <ID>PORTOFDEPARTURE</ID>
  </ExitOffice>";
			AssertContains(expectedPortOfDepartureXML, ocrBuilder.GetXMLMessage());
		}

		void SetUpMocks()
		{
			outwardCargoReportHeaderMock = new Mock<IOutwardCargoReportHeader>();
			outwardCargoReportHeaderMock.Setup(m => m.TSWReferenceNumber).Returns("ENTRY12345");
			outwardCargoReportHeaderMock.Setup(m => m.SenderReferenceNumber).Returns("C00001165");
			outwardCargoReportHeaderMock.Setup(m => m.IsSea).Returns(true);
			outwardCargoReportHeaderMock.Setup(m => m.CraftName).Returns("CRAFTNAMETEST");
			outwardCargoReportHeaderMock.Setup(m => m.FlightNo).Returns("FLIGHTNOTEST");
			outwardCargoReportHeaderMock.Setup(m => m.LloydsNo).Returns("LLOYDSNOTEST");
			outwardCargoReportHeaderMock.Setup(m => m.VoyageNo).Returns("VOYAGENOTEST");
			outwardCargoReportHeaderMock.Setup(m => m.DepartureDate).Returns(new ZDateTime());
			outwardCargoReportHeaderMock.Setup(m => m.RoutingCountryCodes).Returns(new List<ZString>() { "AU", "NZ" });
			outwardCargoReportHeaderMock.Setup(m => m.PortOfDeparture).Returns("PORTOFDEPARTURE");
			outwardCargoReportHeaderMock.Setup(m => m.IsConsolidation).Returns(true);
			var additionalInformationMock = new Mock<IAdditionalInformation>();
			var tswAttachment1 = new Mock<ITSWAttachment>();
			var tswAttachment2 = new Mock<ITSWAttachment>();
			tswAttachment1.Setup(m => m.DocType).Returns("CDO");
			tswAttachment1.Setup(m => m.FileName).Returns("TEST1.PDF");
			tswAttachment2.Setup(m => m.DocType).Returns("INV");
			tswAttachment2.Setup(m => m.FileName).Returns("TEST2.PDF");
			additionalInformationMock.Setup(m => m.SupportingDocuments).Returns(new List<ITSWAttachment>() { tswAttachment1.Object, tswAttachment2.Object });
			additionalInformationMock.Setup(m => m.FreeText).Returns("FREETEXTTEST");
			additionalInformationMock.Setup(m => m.ManualOverrideText).Returns("MANUALOVERRIDETEXTTEST");
			additionalInformationMock.Setup(m => m.AdditionalStatementText).Returns("ADDITIONALSTATEMENTTEXTTEST");
			outwardCargoReportHeaderMock.Setup(m => m.AdditionalInformation).Returns(additionalInformationMock.Object);
			iOrganisationSimpleMock = new Mock<IOrganisationSimple>();
			iOrganisationSimpleMock.Setup(m => m.Name).Returns("CONSOLIDATORNAME");
			iOrganisationSimpleMock.Setup(m => m.CustomsClientCode).Returns("51358595A");
			outwardCargoReportHeaderMock.Setup(m => m.Consolidator).Returns(iOrganisationSimpleMock.Object);
			outwardCargoReportHeaderMock.Setup(m => m.Carrier).Returns(iOrganisationSimpleMock.Object);
			var iOCRConsignment1 = new Mock<IOCRConsignment>();
			var iOCRConsignment2 = new Mock<IOCRConsignment>();
			var iOCRConsignment3 = new Mock<IOCRConsignment>();
			iOCRConsignment1.Setup(m => m.CustomsClearanceNo).Returns("A12345");
			iOCRConsignment2.Setup(m => m.CustomsClearanceNo).Returns("B67890");
			iOCRConsignment3.Setup(m => m.CustomsClearanceNo).Returns(ZString.Empty);
			var iAssociatedTransportDocument = new Mock<IAssociatedTransportDocument>();
			iAssociatedTransportDocument.Setup(m => m.BillNumber).Returns("BILLNUMBERTEST");
			iAssociatedTransportDocument.Setup(m => m.BillType).Returns("BILLTYPETEST");
			iOCRConsignment1.Setup(m => m.BillNumber).Returns(iAssociatedTransportDocument.Object);
			iOCRConsignment2.Setup(m => m.BillNumber).Returns(iAssociatedTransportDocument.Object);
			iOCRConsignment3.Setup(m => m.BillNumber).Returns(iAssociatedTransportDocument.Object);
			outwardCargoReportHeaderMock.Setup(m => m.OCRLines).Returns(new List<IOCRConsignment>() { iOCRConsignment1.Object, iOCRConsignment2.Object, iOCRConsignment3.Object });
			outwardCargoReportHeaderMock.Setup(m => m.NotifyPartyCodes).Returns(new List<ZString>() { "CCP Number", "PORT" });
			outwardCargoReportHeaderMock.Setup(m => m.NotifyPartyName).Returns("NOTIFYPARTYNAMETEST");
			outwardCargoReportHeaderMock.Setup(m => m.NotifyPartyEmail).Returns("NOTIFYPARTYEMAILTEST");
			outwardCargoReportHeaderMock.Setup(m => m.IsConsolidation).Returns(true);
			outwardCargoReportHeaderMock.Setup(m => m.MasterBillNumber).Returns("MASTERBILLNUMBERTEST");
			var iTransportEquipment1 = new Mock<ITransportEquipment>();
			var iTransportEquipment2 = new Mock<ITransportEquipment>();
			iTransportEquipment1.Setup(m => m.ContainerNumber).Returns("CONTAINERNUMBERTEST");
			iTransportEquipment1.Setup(m => m.Status).Returns("STATUSTEST");
			iTransportEquipment2.Setup(m => m.ContainerNumber).Returns("CONTAINERNUMBERTEST2");
			iTransportEquipment2.Setup(m => m.Status).Returns("STATUSTEST2");
			outwardCargoReportHeaderMock.Setup(m => m.Containers).Returns(new List<ITransportEquipment>() { iTransportEquipment1.Object, iTransportEquipment2.Object });
			ocrBuilder = new OCRMessageBuilder(outwardCargoReportHeaderMock.Object, TSWTransactionTypes.Original, "00009908C");
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
		}

		Mock<IOutwardCargoReportHeader> outwardCargoReportHeaderMock;
		Mock<IOrganisationSimple> iOrganisationSimpleMock;
		OCRMessageBuilder ocrBuilder;
		EmbeddedResourceRetriever embeddedResourceRetriever;
	}
}
