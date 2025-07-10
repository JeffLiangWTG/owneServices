using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.BatchProcessor.Testing
{
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Customs.Business.Testing;
	using Enterprise.Customs.NZ.Business.MAFeBACCa.Testing;
	using Enterprise.Integration;

	class InboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestLegacyCUSRESProcessing()
		{
			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000031";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_HeaderText = @"UNA:+.? 'UNB+UNOA:2+EXT.TSW.GOVT.NZ:ZZZ+00009908C:ZZZ+130226:1106+586'";
			interchange.EI_BodyText = @"UNH+1+CUSRES:D:96B:UN+B00001125'BGM+932+66674026:03'FTX+DIN+++100 LOOSE PACKAGE(S) OR ITEM(S)'GIS+819:120:143'TAX+4+TOT'MOA+161:584.57'GIS+D:134:143'UNT+8+1'";
			interchange.EI_FooterText = @"UNZ+1+586'";
			Factory.Save();

			var log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", "NZC", createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", "XXX", createdMessage.EM_MessageSubType);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", interchange.EI_BodyText, createdMessage.EM_MessageText);
			AssertEquals("EM_ApplicationReference", "000001", createdMessage.EM_ApplicationReference);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
		}

		public void TestGenerateTSWMessageFromOCRInterchangeResponse()
		{
			var xmlText = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESOCR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response>
    <IssueDateTime formatCode=""204"">20130326165111</IssueDateTime>
    <FunctionalReferenceID>7</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>85979296</ID>
        <FunctionalReferenceID>C00001033</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20130326165111</EffectiveDateTime>
      <NameCode>ACK</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";
			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000037";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = xmlText;
			Factory.Save();

			var log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", "OCR", createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse, createdMessage.EM_MessageSubType);
			AssertEquals("EM_MessageNum", "7", createdMessage.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", xmlText, createdMessage.EM_MessageText);
			AssertEquals("EM_ApplicationReference", "C00001033", createdMessage.EM_ApplicationReference);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
		}

		public void TestGenerateTSWMessageFromCREInterchangeResponse()
		{
			var xmlText = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESCRE</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response>
    <IssueDateTime formatCode=""204"">20130328175823</IssueDateTime>
    <FunctionalReferenceID>14</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>83563537</ID>
        <FunctionalReferenceID>X00001004</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20130328175823</EffectiveDateTime>
      <NameCode>ACK</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";
			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000044";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = xmlText;
			Factory.Save();

			var log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", "CRE", createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse, createdMessage.EM_MessageSubType);
			AssertEquals("EM_MessageNum", "14", createdMessage.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", xmlText, createdMessage.EM_MessageText);
			AssertEquals("EM_ApplicationReference", "X00001004", createdMessage.EM_ApplicationReference);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
		}

		public void TestGenerateTSWMessageFromUNKInterchangeResponse()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_DeclarationReference = "B00001175";

			var xmlText = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESUNK</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response>
    <IssueDateTime formatCode=""204"">20130524142112</IssueDateTime>
    <FunctionalReferenceID>88</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>00000000</ID>
        <FunctionalReferenceID>B00001175</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>4096</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>67A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>68A</DocumentSectionCode>
        <TagID>23A</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20130524142112</EffectiveDateTime>
      <NameCode>858</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";
			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000152";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = xmlText;
			Factory.Save();

			var log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", "CRE", createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse, createdMessage.EM_MessageSubType);
			AssertEquals("EM_MessageNum", "88", createdMessage.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", xmlText, createdMessage.EM_MessageText);
			AssertEquals("EM_ApplicationReference", "B00001175", createdMessage.EM_ApplicationReference);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
		}

		public void TestGenerateTSWMessageFromIM1InterchangeResponse()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = "B00001138";

			var xmlText = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response>
    <IssueDateTime formatCode=""204"">20130403161205</IssueDateTime>
    <FunctionalReferenceID>18</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>92667646</ID>
        <FunctionalReferenceID>B00001138</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20130403161205</EffectiveDateTime>
      <NameCode>802</NameCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";
			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000049";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = xmlText;
			Factory.Save();

			var log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType - message still needs to use TSW code so eHub can recognise this as a TSW message", MessageTypeList.Codes.I10, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse, createdMessage.EM_MessageSubType);
			AssertEquals("EM_MessageNum", "18", createdMessage.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", xmlText, createdMessage.EM_MessageText);
			AssertEquals("EM_ApplicationReference", "B00001138", createdMessage.EM_ApplicationReference);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
		}

		public void TestGenerateTSWMessageFromEX1InterchangeResponse()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = "B00001145";

			var xmlText = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESEX1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response>
    <IssueDateTime formatCode=""204"">20130409161056</IssueDateTime>
    <FunctionalReferenceID>22</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>67415226</ID>
        <FunctionalReferenceID>B00001145</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <RejectionDateTime formatCode=""204"">20130409161056</RejectionDateTime>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>140</ValidationCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>57A</DocumentSectionCode>
        <TagID>R032</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>614</ValidationCode>
      <Pointer>
        <DocumentSectionCode />
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>166</ValidationCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>67A</DocumentSectionCode>
        <TagID>062</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20130409161056</EffectiveDateTime>
      <NameCode>801</NameCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";
			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000049";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = xmlText;
			Factory.Save();

			var log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType", "E40", createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse, createdMessage.EM_MessageSubType);
			AssertEquals("EM_MessageNum", "22", createdMessage.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", xmlText, createdMessage.EM_MessageText);
			AssertEquals("EM_ApplicationReference", "B00001145", createdMessage.EM_ApplicationReference);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
		}

		public void TestImportDecAllAgencyResponses()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = "B00001252";

			var xmlText = AcknowledgementResponse;
			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000052";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = xmlText;
			Factory.Save();

			var log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType - message still needs to use TSW code so eHub can recognise this as a TSW message", MessageTypeList.Codes.I10, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageNum", "473", createdMessage.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", xmlText, createdMessage.EM_MessageText);
			AssertEquals("EM_ApplicationReference", "B00001252", createdMessage.EM_ApplicationReference);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);

			xmlText = MPIFoodResponse;
			interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000055";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = xmlText;
			Factory.Save();

			log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			interchange.ContainedMessages.Load();
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType - message still needs to use TSW code so eHub can recognise this as a TSW message", MessageTypeList.Codes.I10, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageNum", "474", createdMessage.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", xmlText, createdMessage.EM_MessageText);
			AssertEquals("EM_ApplicationReference", "B00001252", createdMessage.EM_ApplicationReference);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);

			xmlText = MPIBIOResponse;
			interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000057";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = xmlText;
			Factory.Save();

			log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType - message still needs to use TSW code so eHub can recognise this as a TSW message", MessageTypeList.Codes.I10, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageNum", "476", createdMessage.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", xmlText, createdMessage.EM_MessageText);
			AssertEquals("EM_ApplicationReference", "B00001252", createdMessage.EM_ApplicationReference);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);

			xmlText = NZCSResponse;
			interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000056";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = xmlText;
			Factory.Save();

			log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType - message still needs to use TSW code so eHub can recognise this as a TSW message", MessageTypeList.Codes.I10, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageNum", "475", createdMessage.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", xmlText, createdMessage.EM_MessageText);
			AssertEquals("EM_ApplicationReference", "B00001252", createdMessage.EM_ApplicationReference);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
		}

		public void TestGenerateTSWMessagesFromIM1CompletionMessageResponses()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			declaration.JE_DeclarationReference = "B00001326";
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "1";

			#region Acknowledgement
			var ackInterchange = Factory.New<NZCInterchange>();
			ackInterchange.EI_From = "CUSSWT";
			ackInterchange.EI_To = "00009908C";
			ackInterchange.EI_ApplicationCode = "NZC";
			ackInterchange.EI_InterchangeType = "NZC";
			ackInterchange.EI_InterchangeNum = "00000000000000001712";
			ackInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			ackInterchange.EI_Status = EDIInterchange.Status.Queued;
			ackInterchange.EI_IsActive = true;
			ackInterchange.EI_BodyText = CompletionAckResponse;
			Factory.Save();

			var log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			ackInterchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, ackInterchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, ackInterchange.ContainedMessages.Count);

			var createdMessage = ackInterchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", ackInterchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType - message still needs to use TSW code so eHub can recognise this as a TSW message", MessageTypeList.Codes.I10, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse, createdMessage.EM_MessageSubType);
			AssertEquals("EM_MessageNum", "762", createdMessage.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", CompletionAckResponse, createdMessage.EM_MessageText);
			AssertEquals("EM_ApplicationReference", "1", createdMessage.EM_ApplicationReference);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
			#endregion

			#region MPI Food
			var mpiInterchange = Factory.New<NZCInterchange>();
			mpiInterchange.EI_From = "CUSSWT";
			mpiInterchange.EI_To = "00009908C";
			mpiInterchange.EI_ApplicationCode = "NZC";
			mpiInterchange.EI_InterchangeType = "NZC";
			mpiInterchange.EI_InterchangeNum = "00000000000000001713";
			mpiInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			mpiInterchange.EI_Status = EDIInterchange.Status.Queued;
			mpiInterchange.EI_IsActive = true;
			mpiInterchange.EI_BodyText = CompletionFoodResponse;
			Factory.Save();

			log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			mpiInterchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, mpiInterchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, mpiInterchange.ContainedMessages.Count);

			createdMessage = mpiInterchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", mpiInterchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType - message still needs to use TSW code so eHub can recognise this as a TSW message", MessageTypeList.Codes.I10, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse, createdMessage.EM_MessageSubType);
			AssertEquals("EM_MessageNum", "763", createdMessage.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", CompletionFoodResponse, createdMessage.EM_MessageText);
			AssertEquals("EM_ApplicationReference", "1", createdMessage.EM_ApplicationReference);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
			#endregion

			#region NZCS
			var nzcInterchange = Factory.New<NZCInterchange>();
			nzcInterchange.EI_From = "CUSSWT";
			nzcInterchange.EI_To = "00009908C";
			nzcInterchange.EI_ApplicationCode = "NZC";
			nzcInterchange.EI_InterchangeType = "NZC";
			nzcInterchange.EI_InterchangeNum = "00000000000000001714";
			nzcInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			nzcInterchange.EI_Status = EDIInterchange.Status.Queued;
			nzcInterchange.EI_IsActive = true;
			nzcInterchange.EI_BodyText = CompletionNZCSResponse;
			Factory.Save();

			log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			nzcInterchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, nzcInterchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, nzcInterchange.ContainedMessages.Count);

			createdMessage = nzcInterchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", nzcInterchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType - message still needs to use TSW code so eHub can recognise this as a TSW message", MessageTypeList.Codes.I10, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse, createdMessage.EM_MessageSubType);
			AssertEquals("EM_MessageNum", "764", createdMessage.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", CompletionNZCSResponse, createdMessage.EM_MessageText);
			AssertEquals("EM_ApplicationReference", "1", createdMessage.EM_ApplicationReference);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
			#endregion

			#region MPI Biosecurity
			var bioInterchange = Factory.New<NZCInterchange>();
			bioInterchange.EI_From = "CUSSWT";
			bioInterchange.EI_To = "00009908C";
			bioInterchange.EI_ApplicationCode = "NZC";
			bioInterchange.EI_InterchangeType = "NZC";
			bioInterchange.EI_InterchangeNum = "00000000000000001715";
			bioInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			bioInterchange.EI_Status = EDIInterchange.Status.Queued;
			bioInterchange.EI_IsActive = true;
			bioInterchange.EI_BodyText = CompletionBioResponse;
			Factory.Save();

			log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			bioInterchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, bioInterchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, bioInterchange.ContainedMessages.Count);

			createdMessage = bioInterchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", bioInterchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType - message still needs to use TSW code so eHub can recognise this as a TSW message", MessageTypeList.Codes.I10, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse, createdMessage.EM_MessageSubType);
			AssertEquals("EM_MessageNum", "766", createdMessage.EM_MessageNum);
			AssertEquals("EM_IsTestMessage", true, createdMessage.EM_IsTestMessage);
			AssertEquals("EM_MessageText", CompletionBioResponse, createdMessage.EM_MessageText);
			AssertEquals("EM_ApplicationReference", "1", createdMessage.EM_ApplicationReference);
			AssertEquals("EM_ReceiveTransmit", "RCV", createdMessage.EM_ReceiveTransmit);
			AssertEquals("EM_Status", "QUE", createdMessage.EM_Status);
			AssertEquals("EM_LinkTable - this is set by message processor", "", createdMessage.EM_LinkTable);
			AssertEquals("EM_LinkUniqueID - this is set by message processor", ZGuid.Empty, createdMessage.EM_LinkUniqueID);
			#endregion
		}

		public void TestGetMessageTypeFromIM1InterchangeResponse()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = "S02082461";

			var xmlText = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20151123112323</IssueDateTime>
    <FunctionalReferenceID>2067</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>44885032</ID>
        <FunctionalReferenceID>S02082461</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <RejectionDateTime formatCode=""204"">20151123112323</RejectionDateTime>
        <Submitter>
          <ID>00326958C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>699</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>67A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>68A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>08A</DocumentSectionCode>
        <TagID>L058</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20151123112323</EffectiveDateTime>
      <NameCode>801</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";
			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000049";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = xmlText;
			Factory.Save();

			var log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType - message still needs to use TSW code so eHub can recognise this as a TSW message", MessageTypeList.Codes.I10, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse, createdMessage.EM_MessageSubType);
		}

		public void TestGetMessageTypeFromInterchangeResponse()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = "S02082461";

			var xmlText = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20151123112213</IssueDateTime>
    <FunctionalReferenceID>2056</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>44885032</ID>
        <FunctionalReferenceID>S02082461</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00326958C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20151123112213</EffectiveDateTime>
      <NameCode>ACK</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000049";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = xmlText;
			Factory.Save();

			var log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType - message still needs to use TSW code so eHub can recognise this as a TSW message", MessageTypeList.Codes.I10, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse, createdMessage.EM_MessageSubType);
		}

		public void TestCorrectEntryHeaderChosenForResponse()
		{
			var usDec = Factory.NewWithValidTestData<Enterprise.Customs.Business.BaseJobDeclaration>();
			usDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			usDec.JE_MessageSubType = "";
			usDec.JE_ApplicationCode = "";
			usDec.JE_DeclarationReference = "S02082460";
			usDec.JE_ContainerMode = "NCT";

			var usEntryHeader = usDec.CustomsEntryHeaders.AddNew();
			usEntryHeader.CH_MessageType = "ITN";
			usEntryHeader.CH_Status = "OSC";
			usEntryHeader.CH_EntryStatus = "OSC";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = "S02082461";

			var xmlText = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20151123112213</IssueDateTime>
    <FunctionalReferenceID>2056</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>44885032</ID>
        <FunctionalReferenceID>S02082461</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00326958C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20151123112213</EffectiveDateTime>
      <NameCode>ACK</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000049";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = xmlText;
			Factory.Save();

			var log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType - message still needs to use TSW code so eHub can recognise this as a TSW message", MessageTypeList.Codes.I10, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse, createdMessage.EM_MessageSubType);
		}

		public void TestGetImportMessageTypeFromIM1InterchangeResponse()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = "S00085548";

			var xmlText = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>  <WCODocumentName>RES</WCODocumentName>  <CountryCode>NZ</CountryCode>  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>  <Response xmlns=""urn: wco: datamodel: WCO: ResponseModel: 1"">    <IssueDateTime formatCode=""204"">20171017145521</IssueDateTime>    <FunctionalReferenceID>6837</FunctionalReferenceID>    <FunctionCode>24</FunctionCode>    <AdditionalInformation>      <StatementDescription>8 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>      <StatementTypeCode>DIN</StatementTypeCode>    </AdditionalInformation>    <OverallDeclaration>      <Declaration>        <ID>46496719</ID>        <AcceptanceDateTime formatCode=""204"">20171017145521</AcceptanceDateTime>        <FunctionalReferenceID>S00085548</FunctionalReferenceID>        <VersionID>1</VersionID>        <Submitter>          <ID>00303940E</ID>        </Submitter>        <DutyTaxFee>          <Payment>            <MethodCode>B</MethodCode>          </Payment>        </DutyTaxFee>        <DutyTaxFee>          <TypeCode>TOT</TypeCode>          <Payment>            <TaxAssessedAmount currencyID=""NZD"">144.93</TaxAssessedAmount>          </Payment>        </DutyTaxFee>        <ResponsibleGovernmentAgency>          <ID>NZCS</ID>        </ResponsibleGovernmentAgency>      </Declaration>    </OverallDeclaration>    <Status>      <EffectiveDateTime formatCode=""204"">20171017145521</EffectiveDateTime>      <NameCode>819</NameCode>      <ReleaseDateTime formatCode=""204"">20171017145521</ReleaseDateTime>      <Pointer>        <DocumentSectionCode>07B</DocumentSectionCode>      </Pointer>      <Pointer>        <DocumentSectionCode>42A</DocumentSectionCode>      </Pointer>      <Pointer>        <SequenceNumeric>1</SequenceNumeric>        <DocumentSectionCode>08B</DocumentSectionCode>        <TagID>G007</TagID>      </Pointer>    </Status>  </Response></DocumentMetadata>";

			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000073";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = xmlText;
			Factory.Save();

			var log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType - message still needs to use TSW import type code so the system can recognise this as a TSW message", MessageTypeList.Codes.I10, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse, createdMessage.EM_MessageSubType);
		}

		public void TestGetImportMessageTypeFromIM1InterchangeResponse_ConsolidatedDeclaration()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			var declaration = consolidatedDeclaration.LeadDeclaration;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			var xmlText = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>  <WCODocumentName>RES</WCODocumentName>  <CountryCode>NZ</CountryCode>  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>  <Response xmlns=""urn: wco: datamodel: WCO: ResponseModel: 1"">    <IssueDateTime formatCode=""204"">20171017145521</IssueDateTime>    <FunctionalReferenceID>6837</FunctionalReferenceID>    <FunctionCode>24</FunctionCode>    <AdditionalInformation>      <StatementDescription>8 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>      <StatementTypeCode>DIN</StatementTypeCode>    </AdditionalInformation>    <OverallDeclaration>      <Declaration>        <ID>46496719</ID>        <AcceptanceDateTime formatCode=""204"">20171017145521</AcceptanceDateTime>        <FunctionalReferenceID>S00085548</FunctionalReferenceID>        <VersionID>1</VersionID>        <Submitter>          <ID>00303940E</ID>        </Submitter>        <DutyTaxFee>          <Payment>            <MethodCode>B</MethodCode>          </Payment>        </DutyTaxFee>        <DutyTaxFee>          <TypeCode>TOT</TypeCode>          <Payment>            <TaxAssessedAmount currencyID=""NZD"">144.93</TaxAssessedAmount>          </Payment>        </DutyTaxFee>        <ResponsibleGovernmentAgency>          <ID>NZCS</ID>        </ResponsibleGovernmentAgency>      </Declaration>    </OverallDeclaration>    <Status>      <EffectiveDateTime formatCode=""204"">20171017145521</EffectiveDateTime>      <NameCode>819</NameCode>      <ReleaseDateTime formatCode=""204"">20171017145521</ReleaseDateTime>      <Pointer>        <DocumentSectionCode>07B</DocumentSectionCode>      </Pointer>      <Pointer>        <DocumentSectionCode>42A</DocumentSectionCode>      </Pointer>      <Pointer>        <SequenceNumeric>1</SequenceNumeric>        <DocumentSectionCode>08B</DocumentSectionCode>        <TagID>G007</TagID>      </Pointer>    </Status>  </Response></DocumentMetadata>";

			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000073";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = xmlText;
			Factory.Save();
			consolidatedDeclaration.CRD_JobReferenceNumber = "S00085548";
			Factory.Save();

			var log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType - message still needs to use TSW import type code so the system can recognise this as a TSW message", MessageTypeList.Codes.I10, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse, createdMessage.EM_MessageSubType);
		}

		public void TestGetMsgTypeFromIM1ResponseUsesDefaultIfNoMatch()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = ""; // Force declaration to not be found when searching for match so default value is returned for message type

			var xmlText = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>  <WCODocumentName>RES</WCODocumentName>  <CountryCode>NZ</CountryCode>  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>  <Response xmlns=""urn: wco: datamodel: WCO: ResponseModel: 1"">    <IssueDateTime formatCode=""204"">20171017145521</IssueDateTime>    <FunctionalReferenceID>6837</FunctionalReferenceID>    <FunctionCode>24</FunctionCode>    <AdditionalInformation>      <StatementDescription>8 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>      <StatementTypeCode>DIN</StatementTypeCode>    </AdditionalInformation>    <OverallDeclaration>      <Declaration>        <ID>46496719</ID>        <AcceptanceDateTime formatCode=""204"">20171017145521</AcceptanceDateTime>        <FunctionalReferenceID>S00085548</FunctionalReferenceID>        <VersionID>1</VersionID>        <Submitter>          <ID>00303940E</ID>        </Submitter>        <DutyTaxFee>          <Payment>            <MethodCode>B</MethodCode>          </Payment>        </DutyTaxFee>        <DutyTaxFee>          <TypeCode>TOT</TypeCode>          <Payment>            <TaxAssessedAmount currencyID=""NZD"">144.93</TaxAssessedAmount>          </Payment>        </DutyTaxFee>        <ResponsibleGovernmentAgency>          <ID>NZCS</ID>        </ResponsibleGovernmentAgency>      </Declaration>    </OverallDeclaration>    <Status>      <EffectiveDateTime formatCode=""204"">20171017145521</EffectiveDateTime>      <NameCode>819</NameCode>      <ReleaseDateTime formatCode=""204"">20171017145521</ReleaseDateTime>      <Pointer>        <DocumentSectionCode>07B</DocumentSectionCode>      </Pointer>      <Pointer>        <DocumentSectionCode>42A</DocumentSectionCode>      </Pointer>      <Pointer>        <SequenceNumeric>1</SequenceNumeric>        <DocumentSectionCode>08B</DocumentSectionCode>        <TagID>G007</TagID>      </Pointer>    </Status>  </Response></DocumentMetadata>";

			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000073";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = xmlText;
			Factory.Save();

			var log = GetNewLogger();
			new InboundInterchangeProcessor(log).ExecuteBatch();
			interchange.Reload();
			AssertEquals("Interchange status should be set to Received", EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals("Should have been 1 message extracted from interchange", 1, interchange.ContainedMessages.Count);

			var createdMessage = interchange.ContainedMessages[0];
			AssertEquals("message linked to interchange", interchange.PK, createdMessage.EM_EI);
			AssertEquals("EM_ApplicationCode", "NZC", createdMessage.EM_ApplicationCode);
			AssertEquals("EM_MessageType - message needs to use TSW default type if it cannot find a declaration match, so the system can recognise this as a TSW xml message", MessageTypeList.Codes.TWR, createdMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse, createdMessage.EM_MessageSubType);
		}

		#region Implementation

		protected LoggingInformation GetNewLogger()
		{
			var result = new LoggingInformation();
			result.OnLogInfoAdded += new LoggingInformation.LogInfoAdded(Logger_OnLogInfoAdded);
			return result;
		}

		void Logger_OnLogInfoAdded(string log, LogType logType)
		{
			var logger = new LoggerForTesting();
			logger.Log(logType, log.Trim());
		}

		#region Response Messages

		const string AcknowledgementResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20131031130950</IssueDateTime>
    <FunctionalReferenceID>473</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>8200273</ID>
        <FunctionalReferenceID>B00001252</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131031130950</EffectiveDateTime>
      <NameCode>ACK</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string MPIFoodResponse =
 @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20131031130944</IssueDateTime>
    <FunctionalReferenceID>474</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>8200273</ID>
        <AcceptanceDateTime formatCode=""204"">20131031130944</AcceptanceDateTime>
        <FunctionalReferenceID>B00001252</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131031130944</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20131031130944</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string MPIBIOResponse =
 @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20131031131008</IssueDateTime>
    <FunctionalReferenceID>476</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>8200273</ID>
        <AcceptanceDateTime formatCode=""204"">20131031131008</AcceptanceDateTime>
        <FunctionalReferenceID>B00001252</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>C</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount>441.57</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131031131008</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20131031131013</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string NZCSResponse =
 @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20131031131008</IssueDateTime>
    <FunctionalReferenceID>475</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>8200273</ID>
        <AcceptanceDateTime formatCode=""204"">20131031131008</AcceptanceDateTime>
        <FunctionalReferenceID>B00001252</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>C</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount>441.57</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131031131008</EffectiveDateTime>
      <NameCode>822</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		#endregion

		#region Completion Response Messages

		const string CompletionAckResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20140203134338</IssueDateTime>
    <FunctionalReferenceID>762</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>62663761</ID>
        <FunctionalReferenceID>1</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20140203134338</EffectiveDateTime>
      <NameCode>ACK</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string CompletionFoodResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20140203134330</IssueDateTime>
    <FunctionalReferenceID>763</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>62663761</ID>
        <AcceptanceDateTime formatCode=""204"">20140203134330</AcceptanceDateTime>
        <FunctionalReferenceID>1</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20140203134330</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20140203134330</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string CompletionNZCSResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20140203134349</IssueDateTime>
    <FunctionalReferenceID>764</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>62663761</ID>
        <AcceptanceDateTime formatCode=""204"">20140203134349</AcceptanceDateTime>
        <FunctionalReferenceID>1</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>C</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount>593.39</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20140203134349</EffectiveDateTime>
      <NameCode>822</NameCode>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		const string CompletionBioResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20140203134349</IssueDateTime>
    <FunctionalReferenceID>766</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>62663761</ID>
        <AcceptanceDateTime formatCode=""204"">20140203134349</AcceptanceDateTime>
        <FunctionalReferenceID>1</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>C</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount>593.39</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20140203134349</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20140203134356</ReleaseDateTime>
      <Pointer>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08B</DocumentSectionCode>
        <TagID>G007</TagID>
      </Pointer>
    </Status>
  </Response>
</DocumentMetadata>";

		#endregion

		#endregion
	}
}
