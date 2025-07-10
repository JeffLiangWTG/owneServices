using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NZ.Business.BatchProcessor.Testing
{
	using System.Threading;
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	public class NZCOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		[TestDate(2020, 02, 18, 10, 00, 0)]
		public void TestUpdateSubmitDateAfterProcessing()
		{
			var factory = new BusinessObjectFactory();
			var declaration = factory.New<JobDeclaration>();
			var entryheader = declaration.CustomsEntryHeaders.AddNew();

			var message = new NZDiagnosticConsolForTesting(factory).CreateTestMessage_Exposed();
			message.EM_MessageType = MessageTypeList.Codes.CRE;
			message.EM_LinkTable = CusEntryHeader.Schema.TableName;
			message.EM_LinkUniqueID = entryheader.PK;
			AssertEquals("Initial EDI Message Status", EDIMessage.Status.Queued, message.EM_Status);
			factory.Save();
			message.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRE</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>CRE</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1""><TypeCode>CRE</TypeCode><FunctionalReferenceID /><FunctionCode>9</FunctionCode><Submitter><ID>00009908C</ID></Submitter><AdditionalInformation><Content>TEST CRE Message</Content></AdditionalInformation><AdditionalInformation><RequestOverrideCode>Y</RequestOverrideCode><StatementDescription>Include manual processing request</StatementDescription><StatementTypeCode>ALP</StatementTypeCode></AdditionalInformation><BorderTransportMeans><Name>QF108</Name><TypeCode>4</TypeCode><DepartureDateTime formatCode=""102"">20130301</DepartureDateTime></BorderTransportMeans><Carrier><Name>QANTAS AIRWAYS LIMITED</Name></Carrier><Consignment><SequenceNumeric>1</SequenceNumeric><AdditionalInformation><StatementCode>Y</StatementCode><StatementTypeCode>WOF</StatementTypeCode></AdditionalInformation><Consignee><Name>B &amp; A INTERNATIONAL FASHION</Name><Address><CityName>WEST PENNANT HILLS NSW</CityName><CountryCode>AU</CountryCode><CountrySubDivisionName>NSW</CountrySubDivisionName><Line>14 LYNTON GREEN</Line><PostcodeID>2152</PostcodeID></Address></Consignee><ConsignmentItem><SequenceNumeric>1</SequenceNumeric><Commodity><CargoDescription>MENS GARMENTS</CargoDescription><CommercialCategorizationID /><ValueAmount currencyID=""NZD"">320.0000</ValueAmount><IdentityQualifierCode /></Commodity><GoodsMeasure><GrossMassMeasure unitCode=""KGM"">2</GrossMassMeasure></GoodsMeasure><Origin><CountryCode>NZAKL</CountryCode></Origin><Packaging><SequenceNumeric>1</SequenceNumeric><QuantityQuantity>1</QuantityQuantity><TypeCode>BG</TypeCode></Packaging></ConsignmentItem><Consignor><Name>BRACKS APPAREL (NZ) PTY LTD</Name><Address><CityName>AUCKLAND</CityName><CountryCode>NZ</CountryCode><CountrySubDivisionName>AUK</CountrySubDivisionName><Line>9 DOUGLAS ALEXANDER PDE</Line><PostcodeID /></Address></Consignor><Freight><PaymentMethodCode /></Freight><GoodsLocation><ID /></GoodsLocation><LoadingLocation><ID>NZAKL</ID></LoadingLocation><TransportContractDocument><ID>92840289</ID><TypeCode>HWB</TypeCode><Consolidator><Name>EDI Demonstration System NZ</Name><ID /></Consolidator></TransportContractDocument><UnloadingLocation><ID>AUSYD</ID></UnloadingLocation></Consignment><Declarant><ID>65432198B</ID><Communication><ID>gary.odea@cargowise.com</ID><TypeID>EM</TypeID></Communication><Communication><ID>61280012206</ID><TypeID>TE</TypeID></Communication></Declarant><ExitOffice><ID>NZAKL</ID></ExitOffice></Declaration>
</DocumentMetadata>";
			factory.Save();
			AssertEquals(ZDateTime.Empty, declaration.JE_EntrySubmittedDate);

			var nzcOutgoingMessageProcessor = new NZCOutgoingMessageProcessor(new LoggingInformation());
			nzcOutgoingMessageProcessor.ProcessMessage(CancellationToken.None);
			factory.Save();
			var dec = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals(ZDateTime.Today, dec.JE_EntrySubmittedDate);
		}

		public void TestTSWInterchange()
		{
			var message = new NZDiagnosticConsolForTesting(Factory).CreateTestMessage_Exposed();
			AssertEquals("Initial EDI Message Status", EDIMessage.Status.Queued, message.EM_Status);
			Factory.Save();

			message.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRE</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>CRE</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1""><TypeCode>CRE</TypeCode><FunctionalReferenceID /><FunctionCode>9</FunctionCode><Submitter><ID>00009908C</ID></Submitter><AdditionalInformation><Content>TEST CRE Message</Content></AdditionalInformation><AdditionalInformation><RequestOverrideCode>Y</RequestOverrideCode><StatementDescription>Include manual processing request</StatementDescription><StatementTypeCode>ALP</StatementTypeCode></AdditionalInformation><BorderTransportMeans><Name>QF108</Name><TypeCode>4</TypeCode><DepartureDateTime formatCode=""102"">20130301</DepartureDateTime></BorderTransportMeans><Carrier><Name>QANTAS AIRWAYS LIMITED</Name></Carrier><Consignment><SequenceNumeric>1</SequenceNumeric><AdditionalInformation><StatementCode>Y</StatementCode><StatementTypeCode>WOF</StatementTypeCode></AdditionalInformation><Consignee><Name>B &amp; A INTERNATIONAL FASHION</Name><Address><CityName>WEST PENNANT HILLS NSW</CityName><CountryCode>AU</CountryCode><CountrySubDivisionName>NSW</CountrySubDivisionName><Line>14 LYNTON GREEN</Line><PostcodeID>2152</PostcodeID></Address></Consignee><ConsignmentItem><SequenceNumeric>1</SequenceNumeric><Commodity><CargoDescription>MENS GARMENTS</CargoDescription><CommercialCategorizationID /><ValueAmount currencyID=""NZD"">320.0000</ValueAmount><IdentityQualifierCode /></Commodity><GoodsMeasure><GrossMassMeasure unitCode=""KGM"">2</GrossMassMeasure></GoodsMeasure><Origin><CountryCode>NZAKL</CountryCode></Origin><Packaging><SequenceNumeric>1</SequenceNumeric><QuantityQuantity>1</QuantityQuantity><TypeCode>BG</TypeCode></Packaging></ConsignmentItem><Consignor><Name>BRACKS APPAREL (NZ) PTY LTD</Name><Address><CityName>AUCKLAND</CityName><CountryCode>NZ</CountryCode><CountrySubDivisionName>AUK</CountrySubDivisionName><Line>9 DOUGLAS ALEXANDER PDE</Line><PostcodeID /></Address></Consignor><Freight><PaymentMethodCode /></Freight><GoodsLocation><ID /></GoodsLocation><LoadingLocation><ID>NZAKL</ID></LoadingLocation><TransportContractDocument><ID>92840289</ID><TypeCode>HWB</TypeCode><Consolidator><Name>EDI Demonstration System NZ</Name><ID /></Consolidator></TransportContractDocument><UnloadingLocation><ID>AUSYD</ID></UnloadingLocation></Consignment><Declarant><ID>65432198B</ID><Communication><ID>gary.odea@cargowise.com</ID><TypeID>EM</TypeID></Communication><Communication><ID>61280012206</ID><TypeID>TE</TypeID></Communication></Declarant><ExitOffice><ID>NZAKL</ID></ExitOffice></Declaration>
</DocumentMetadata>";
			message.EM_MessageType = MessageTypeList.Codes.CRE;
			Factory.Save();

			var nzcOutgoingMessageProcessor = new NZCOutgoingMessageProcessor(new LoggingInformation());
			nzcOutgoingMessageProcessor.ProcessMessage(CancellationToken.None);

			var interchangesCreated = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("NumberOfInterchanges", 1, interchangesCreated.Length);

			var interchange = interchangesCreated[0];
			AssertEquals("Header for xml interchange should be empty", ZString.Empty, interchange.EI_HeaderText);
			AssertEquals("Footer for xml interchange should be empty", ZString.Empty, interchange.EI_FooterText);
			AssertEquals("Body should contain the message xml message text", message.EM_MessageText, interchange.EI_BodyText);
		}

		[TestDate(2013, 03, 05, 18, 36, 0)]
		public void TestLegacyAndTSWMessageProcessedAtSameTime()
		{
			var legacyMessage = new NZDiagnosticConsolForTesting(Factory).CreateTestMessage_Exposed();
			legacyMessage.EM_MessageType = NZCMessage.MessageTypes.FormalEntry.MessageType;
			legacyMessage.EM_MessageText = @"UNH+177+CUSCAR:D:03A:UN'BGM+833:::DEPART+C00025635+9'NAD+CS+00009908C:ZZZ:143'NAD+CH++QANTAS AIRWAYS LIMITED'TDT+20++4+++++:::QF47'LOC+5+NZAKL'LOC+8+AU'DTM+136:20120828:102'CNT+2:1'CNI+1+24204920'RFF+HWB:U0239892340'UNT+12+177'";
			AssertEquals("Initial EDI Message Status", EDIMessage.Status.Queued, legacyMessage.EM_Status);
			Factory.Save();

			var nzcOutgoingMessageProcessor = new NZCOutgoingMessageProcessor(new LoggingInformation());
			nzcOutgoingMessageProcessor.ProcessMessage(CancellationToken.None);
			var interchangesCreated = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("NumberOfInterchanges - legacy messages are no longer generated", 0, interchangesCreated.Length);

			var tswMessage = new NZDiagnosticConsolForTesting(Factory).CreateTestMessage_Exposed();
			tswMessage.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRE</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>CRE</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1""><TypeCode>CRE</TypeCode><FunctionalReferenceID /><FunctionCode>9</FunctionCode><Submitter><ID>00009908C</ID></Submitter><AdditionalInformation><Content>TEST CRE Message</Content></AdditionalInformation><AdditionalInformation><RequestOverrideCode>Y</RequestOverrideCode><StatementDescription>Include manual processing request</StatementDescription><StatementTypeCode>ALP</StatementTypeCode></AdditionalInformation><BorderTransportMeans><Name>QF108</Name><TypeCode>4</TypeCode><DepartureDateTime formatCode=""102"">20130301</DepartureDateTime></BorderTransportMeans><Carrier><Name>QANTAS AIRWAYS LIMITED</Name></Carrier><Consignment><SequenceNumeric>1</SequenceNumeric><AdditionalInformation><StatementCode>Y</StatementCode><StatementTypeCode>WOF</StatementTypeCode></AdditionalInformation><Consignee><Name>B &amp; A INTERNATIONAL FASHION</Name><Address><CityName>WEST PENNANT HILLS NSW</CityName><CountryCode>AU</CountryCode><CountrySubDivisionName>NSW</CountrySubDivisionName><Line>14 LYNTON GREEN</Line><PostcodeID>2152</PostcodeID></Address></Consignee><ConsignmentItem><SequenceNumeric>1</SequenceNumeric><Commodity><CargoDescription>MENS GARMENTS</CargoDescription><CommercialCategorizationID /><ValueAmount currencyID=""NZD"">320.0000</ValueAmount><IdentityQualifierCode /></Commodity><GoodsMeasure><GrossMassMeasure unitCode=""KGM"">2</GrossMassMeasure></GoodsMeasure><Origin><CountryCode>NZAKL</CountryCode></Origin><Packaging><SequenceNumeric>1</SequenceNumeric><QuantityQuantity>1</QuantityQuantity><TypeCode>BG</TypeCode></Packaging></ConsignmentItem><Consignor><Name>BRACKS APPAREL (NZ) PTY LTD</Name><Address><CityName>AUCKLAND</CityName><CountryCode>NZ</CountryCode><CountrySubDivisionName>AUK</CountrySubDivisionName><Line>9 DOUGLAS ALEXANDER PDE</Line><PostcodeID /></Address></Consignor><Freight><PaymentMethodCode /></Freight><GoodsLocation><ID /></GoodsLocation><LoadingLocation><ID>NZAKL</ID></LoadingLocation><TransportContractDocument><ID>92840289</ID><TypeCode>HWB</TypeCode><Consolidator><Name>EDI Demonstration System NZ</Name><ID /></Consolidator></TransportContractDocument><UnloadingLocation><ID>AUSYD</ID></UnloadingLocation></Consignment><Declarant><ID>65432198B</ID><Communication><ID>gary.odea@cargowise.com</ID><TypeID>EM</TypeID></Communication><Communication><ID>61280012206</ID><TypeID>TE</TypeID></Communication></Declarant><ExitOffice><ID>NZAKL</ID></ExitOffice></Declaration>
</DocumentMetadata>";
			tswMessage.EM_MessageType = MessageTypeList.Codes.CRE;
			AssertEquals("Initial EDI Message Status", EDIMessage.Status.Queued, tswMessage.EM_Status);
			Factory.Save();

			nzcOutgoingMessageProcessor = new NZCOutgoingMessageProcessor(new LoggingInformation());
			nzcOutgoingMessageProcessor.ProcessMessage(CancellationToken.None);
			var interchangeQuery = new ZQuery();
			interchangeQuery.OrderBy = EDIInterchangeSchema.EI_InterchangeNum.Name + OrderByClause.Descending;
			interchangesCreated = Factory.Load<EDIInterchange>(interchangeQuery);
			AssertEquals("NumberOfInterchanges - should pick up TSW message for processing - should now be an interchange created", 1, interchangesCreated.Length);
			var interchange = interchangesCreated[0];
			AssertEquals("Header for xml interchange should be empty", ZString.Empty, interchange.EI_HeaderText);
			AssertEquals("Footer for xml interchange should be empty", ZString.Empty, interchange.EI_FooterText);
			AssertEquals("Body should contain the message xml message text", tswMessage.EM_MessageText, interchange.EI_BodyText);
		}

		public void TestOCRMessageIsDeterminedAsTSWInterchange()
		{
			var message = new NZDiagnosticConsolForTesting(Factory).CreateTestMessage_Exposed();
			AssertEquals("Initial EDI Message Status", EDIMessage.Status.Queued, message.EM_Status);
			Factory.Save();

			message.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRE</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>OCR</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1""><TypeCode>OCR</TypeCode><FunctionalReferenceID>C00025937</FunctionalReferenceID><FunctionCode>9</FunctionCode><Submitter><ID>00009908C</ID></Submitter><AdditionalInformation><Content>TEST OCR message is a TSW message</Content><StatementCode>Y</StatementCode><StatementTypeCode>CON</StatementTypeCode></AdditionalInformation><BorderTransportMeans><Name>AALSMEERGRACHT</Name><ID>9044748</ID><TypeCode>1</TypeCode><DepartureDateTime formatCode=""102"">20130201</DepartureDateTime><JourneyID>350</JourneyID><Itinerary><SequenceNumeric>1</SequenceNumeric><RoutingCountryCode>AU</RoutingCountryCode></Itinerary></BorderTransportMeans><Carrier><Name>ASIAWORLD SHIPPING SERVICES</Name></Carrier><Consignment><SequenceNumeric>1</SequenceNumeric><AdditionalDocument><ID /><TypeCode>EDO</TypeCode></AdditionalDocument><AssociatedTransportDocument><ID>MATTESTHBL009</ID><TypeCode>HWB</TypeCode></AssociatedTransportDocument><TransportContractDocument><ID>MATTESTOBL25</ID><TypeCode>MB</TypeCode><Consolidator><Name>EDI Demonstration System NZ</Name></Consolidator></TransportContractDocument><TransportEquipment><SequenceNumeric>1</SequenceNumeric><FullnessCode>7</FullnessCode><ID /></TransportEquipment></Consignment><ExitOffice><ID>NZAKL</ID></ExitOffice></Declaration>
</DocumentMetadata>";
			message.EM_MessageType = MessageTypeList.Codes.OCR;
			Factory.Save();

			var nzcOutgoingMessageProcessor = new NZCOutgoingMessageProcessor(new LoggingInformation());
			nzcOutgoingMessageProcessor.ProcessMessage(CancellationToken.None);

			var interchangesCreated = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("NumberOfInterchanges", 1, interchangesCreated.Length);

			var interchange = interchangesCreated[0];
			AssertEquals("Header for xml interchange should be empty", ZString.Empty, interchange.EI_HeaderText);
			AssertEquals("Footer for xml interchange should be empty", ZString.Empty, interchange.EI_FooterText);
			AssertEquals("Body should contain the message xml message text", message.EM_MessageText, interchange.EI_BodyText);
		}

		public void TestEX1DrawbackMessageIsDeterminedAsTSWInterchange()
		{
			var message = new NZDiagnosticConsolForTesting(Factory).CreateTestMessage_Exposed();
			AssertEquals("Initial EDI Message Status", EDIMessage.Status.Queued, message.EM_Status);
			Factory.Save();

			message.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>EX</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>EX1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">  <TypeCode>E41</TypeCode>  <FunctionalReferenceID>B00001383</FunctionalReferenceID>  <FunctionCode>9</FunctionCode>  <TotalGrossMassMeasure unitCode=""KGM"">15000</TotalGrossMassMeasure>  <Submitter>    <ID>00009908C</ID>  </Submitter>  <AdditionalInformation>    <Content>EX1 - drawback testing</Content>  </AdditionalInformation>  <AdditionalInformation>    <StatementCode>N</StatementCode>    <StatementTypeCode>ERI</StatementTypeCode>    <Pointer>      <SequenceNumeric>1</SequenceNumeric>      <DocumentSectionCode>40A</DocumentSectionCode>    </Pointer>  </AdditionalInformation>  <Agent>    <ID>00009908C</ID>    <RoleCode>CB</RoleCode>  </Agent>  <BorderTransportMeans>    <Name>NZ1</Name>    <TypeCode>4</TypeCode>  </BorderTransportMeans>  <Carrier>    <Name>AIR NEW ZEALAND (NZ) LIMITED</Name>  </Carrier>  <CurrencyExchange>    <RateNumeric>1</RateNumeric>    <CurrencyTypeCode>NZD</CurrencyTypeCode>  </CurrencyExchange>  <Declarant>    <ID>40006206E</ID>    <Communication>      <ID>gary.odea@cargowise.com</ID>      <TypeID>EM</TypeID>    </Communication>    <Communication>      <ID>61280012200</ID>      <TypeID>TE</TypeID>    </Communication>  </Declarant>  <DutyTaxFee>    <Payment>      <MethodCode>C</MethodCode>    </Payment>  </DutyTaxFee>  <Exporter>    <ID>51352368J</ID>  </Exporter>  <GoodsShipment>    <ExitDateTime formatCode=""102"">20140303</ExitDateTime>    <TransactionNatureCode>12</TransactionNatureCode>    <Consignment>      <GoodsLocation>        <ID>7175H</ID>      </GoodsLocation>      <LoadingLocation>        <ID>NZAKL</ID>      </LoadingLocation>      <TransportContractDocument>        <ID>08600293823</ID>        <TypeCode>MB</TypeCode>        <Pointer>          <DocumentSectionCode>42A</DocumentSectionCode>        </Pointer>        <Pointer>          <DocumentSectionCode>67A</DocumentSectionCode>        </Pointer>        <Pointer>          <DocumentSectionCode>28A</DocumentSectionCode>        </Pointer>        <Pointer>          <SequenceNumeric>2</SequenceNumeric>          <DocumentSectionCode>30B</DocumentSectionCode>        </Pointer>      </TransportContractDocument>      <TransportContractDocument>        <ID>F42948</ID>        <TypeCode>HWB</TypeCode>        <Pointer>          <DocumentSectionCode>42A</DocumentSectionCode>        </Pointer>        <Pointer>          <SequenceNumeric>1</SequenceNumeric>          <DocumentSectionCode>93A</DocumentSectionCode>        </Pointer>      </TransportContractDocument>      <UnloadingLocation>        <ID>AUSYD</ID>      </UnloadingLocation>    </Consignment>    <DeliveryDestination>      <Name>TEST SUPPLIER</Name>      <Address>        <CityName>SYDNEY</CityName>        <CountryCode>AU</CountryCode>        <CountrySubDivisionName>NSW</CountrySubDivisionName>        <Line>1 GEORGE ST</Line>        <PostcodeID>2000</PostcodeID>      </Address>    </DeliveryDestination>    <GovernmentAgencyGoodsItem>      <SequenceNumeric>1</SequenceNumeric>      <Commodity>        <Description>TYPESCRIPTS IN SINGLE SHEETS ETC</Description>        <ValueAmount currencyID=""NZD"">3600.00</ValueAmount>        <Classification>          <ID>4901100001L</ID>          <IdentificationTypeCode>HS</IdentificationTypeCode>        </Classification>      </Commodity>      <GoodsMeasure>        <GrossMassMeasure unitCode=""KGM"">15000</GrossMassMeasure>        <NetNetWeightMeasure unitCode=""KGM"">0</NetNetWeightMeasure>      </GoodsMeasure>      <Origin>        <CountryCode>NZ</CountryCode>      </Origin>      <Packaging>        <SequenceNumeric>1</SequenceNumeric>        <MarksNumbersID>N/M</MarksNumbersID>        <QuantityQuantity>300</QuantityQuantity>        <TypeCode>PK</TypeCode>        <VolumeMeasure>1.3</VolumeMeasure>      </Packaging>    </GovernmentAgencyGoodsItem>    <Importer>      <Name>TEST SUPPLIER</Name>      <Address>        <CityName>SYDNEY</CityName>        <CountryCode>AU</CountryCode>        <CountrySubDivisionName>NSW</CountrySubDivisionName>        <Line>1 GEORGE ST</Line>        <PostcodeID>2000</PostcodeID>      </Address>    </Importer>    <Invoice>      <IssueDateTime formatCode=""102"">20130904</IssueDateTime>      <ID>562-DWB-2014</ID>      <SequenceNumeric>1</SequenceNumeric>    </Invoice>  </GoodsShipment>  <Packaging>    <SequenceNumeric>1</SequenceNumeric>    <QuantityQuantity>22</QuantityQuantity>    <TypeCode>07</TypeCode>  </Packaging>
</Declaration></DocumentMetadata>";
			message.EM_MessageType = MessageTypeList.Codes.E41;
			Factory.Save();

			var nzcOutgoingMessageProcessor = new NZCOutgoingMessageProcessor(new LoggingInformation());
			nzcOutgoingMessageProcessor.ProcessMessage(CancellationToken.None);

			var interchangesCreated = Factory.Load<EDIInterchange>(new ZQuery());
			AssertEquals("NumberOfInterchanges", 1, interchangesCreated.Length);

			var interchange = interchangesCreated[0];
			AssertEquals("EI_InterchangeType should reflect message type", "E41", interchange.EI_InterchangeType);
			AssertEquals("Header for xml interchange should be empty", ZString.Empty, interchange.EI_HeaderText);
			AssertEquals("Footer for xml interchange should be empty", ZString.Empty, interchange.EI_FooterText);
			AssertEquals("Body should contain the message xml message text", message.EM_MessageText, interchange.EI_BodyText);
		}

		public void TestAddEventsForOCRMessages()
		{
			var factory = new BusinessObjectFactory();
			var consol = factory.New<ForwardingConsol>();
			var message = new NZDiagnosticConsolForTesting(factory).CreateTestMessage_Exposed();
			message.EM_MessageType = MessageTypeList.Codes.OCR;
			message.EM_LinkTable = AutoJobConsol.Schema.TableName;
			message.EM_LinkUniqueID = consol.PK;
			message.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>EX</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>EX1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">  <TypeCode>E41</TypeCode>  <FunctionalReferenceID>B00001383</FunctionalReferenceID>  <FunctionCode>9</FunctionCode>  <TotalGrossMassMeasure unitCode=""KGM"">15000</TotalGrossMassMeasure>  <Submitter>    <ID>00009908C</ID>  </Submitter>  <AdditionalInformation>    <Content>EX1 - drawback testing</Content>  </AdditionalInformation>  <AdditionalInformation>    <StatementCode>N</StatementCode>    <StatementTypeCode>ERI</StatementTypeCode>    <Pointer>      <SequenceNumeric>1</SequenceNumeric>      <DocumentSectionCode>40A</DocumentSectionCode>    </Pointer>  </AdditionalInformation>  <Agent>    <ID>00009908C</ID>    <RoleCode>CB</RoleCode>  </Agent>  <BorderTransportMeans>    <Name>NZ1</Name>    <TypeCode>4</TypeCode>  </BorderTransportMeans>  <Carrier>    <Name>AIR NEW ZEALAND (NZ) LIMITED</Name>  </Carrier>  <CurrencyExchange>    <RateNumeric>1</RateNumeric>    <CurrencyTypeCode>NZD</CurrencyTypeCode>  </CurrencyExchange>  <Declarant>    <ID>40006206E</ID>    <Communication>      <ID>gary.odea@cargowise.com</ID>      <TypeID>EM</TypeID>    </Communication>    <Communication>      <ID>61280012200</ID>      <TypeID>TE</TypeID>    </Communication>  </Declarant>  <DutyTaxFee>    <Payment>      <MethodCode>C</MethodCode>    </Payment>  </DutyTaxFee>  <Exporter>    <ID>51352368J</ID>  </Exporter>  <GoodsShipment>    <ExitDateTime formatCode=""102"">20140303</ExitDateTime>    <TransactionNatureCode>12</TransactionNatureCode>    <Consignment>      <GoodsLocation>        <ID>7175H</ID>      </GoodsLocation>      <LoadingLocation>        <ID>NZAKL</ID>      </LoadingLocation>      <TransportContractDocument>        <ID>08600293823</ID>        <TypeCode>MB</TypeCode>        <Pointer>          <DocumentSectionCode>42A</DocumentSectionCode>        </Pointer>        <Pointer>          <DocumentSectionCode>67A</DocumentSectionCode>        </Pointer>        <Pointer>          <DocumentSectionCode>28A</DocumentSectionCode>        </Pointer>        <Pointer>          <SequenceNumeric>2</SequenceNumeric>          <DocumentSectionCode>30B</DocumentSectionCode>        </Pointer>      </TransportContractDocument>      <TransportContractDocument>        <ID>F42948</ID>        <TypeCode>HWB</TypeCode>        <Pointer>          <DocumentSectionCode>42A</DocumentSectionCode>        </Pointer>        <Pointer>          <SequenceNumeric>1</SequenceNumeric>          <DocumentSectionCode>93A</DocumentSectionCode>        </Pointer>      </TransportContractDocument>      <UnloadingLocation>        <ID>AUSYD</ID>      </UnloadingLocation>    </Consignment>    <DeliveryDestination>      <Name>TEST SUPPLIER</Name>      <Address>        <CityName>SYDNEY</CityName>        <CountryCode>AU</CountryCode>        <CountrySubDivisionName>NSW</CountrySubDivisionName>        <Line>1 GEORGE ST</Line>        <PostcodeID>2000</PostcodeID>      </Address>    </DeliveryDestination>    <GovernmentAgencyGoodsItem>      <SequenceNumeric>1</SequenceNumeric>      <Commodity>        <Description>TYPESCRIPTS IN SINGLE SHEETS ETC</Description>        <ValueAmount currencyID=""NZD"">3600.00</ValueAmount>        <Classification>          <ID>4901100001L</ID>          <IdentificationTypeCode>HS</IdentificationTypeCode>        </Classification>      </Commodity>      <GoodsMeasure>        <GrossMassMeasure unitCode=""KGM"">15000</GrossMassMeasure>        <NetNetWeightMeasure unitCode=""KGM"">0</NetNetWeightMeasure>      </GoodsMeasure>      <Origin>        <CountryCode>NZ</CountryCode>      </Origin>      <Packaging>        <SequenceNumeric>1</SequenceNumeric>        <MarksNumbersID>N/M</MarksNumbersID>        <QuantityQuantity>300</QuantityQuantity>        <TypeCode>PK</TypeCode>        <VolumeMeasure>1.3</VolumeMeasure>      </Packaging>    </GovernmentAgencyGoodsItem>    <Importer>      <Name>TEST SUPPLIER</Name>      <Address>        <CityName>SYDNEY</CityName>        <CountryCode>AU</CountryCode>        <CountrySubDivisionName>NSW</CountrySubDivisionName>        <Line>1 GEORGE ST</Line>        <PostcodeID>2000</PostcodeID>      </Address>    </Importer>    <Invoice>      <IssueDateTime formatCode=""102"">20130904</IssueDateTime>      <ID>562-DWB-2014</ID>      <SequenceNumeric>1</SequenceNumeric>    </Invoice>  </GoodsShipment>  <Packaging>    <SequenceNumeric>1</SequenceNumeric>    <QuantityQuantity>22</QuantityQuantity>    <TypeCode>07</TypeCode>  </Packaging>
</Declaration></DocumentMetadata>";
			factory.Save();
			Assert("Precondition: There is no MSN event with reference STC.", !consol.Logs.HasLogWith(x => x.SL_SE_NKEvent == Events.MessageSent.Code && x.SL_Reference == "STC"));
			new NZCOutgoingMessageProcessor(new LoggingInformation()).ProcessMessage(CancellationToken.None);
			factory.Save();

			consol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			Assert("We have a MSN event with reference STC.", consol.Logs.HasLogWith(x => x.SL_SE_NKEvent == Events.MessageSent.Code && x.SL_Reference == "STC"));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
		}

		class NZDiagnosticConsolForTesting : NZDiagnosticConsol
		{
			public NZDiagnosticConsolForTesting(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public NZCMessage CreateTestMessage_Exposed()
			{
				ZGuid msgPk = CreateTestMessage(Factory, "NZDiagnosticConsolForTestingKey");
				return Factory.Load<NZCMessage>(msgPk);
			}
		}

		#endregion
	}
}
