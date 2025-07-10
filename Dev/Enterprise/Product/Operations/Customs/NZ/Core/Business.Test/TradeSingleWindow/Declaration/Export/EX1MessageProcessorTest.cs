using System.Linq;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	using System;
	using CargoWise.Common;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.Business.MessageProcessors;
	using Enterprise.Customs.NZ.TradeSingleWindow;
	using Enterprise.Messaging.Business;

	public class EX1MessageProcessorTest : TestCaseWithFactory
	{
		public void TestErrorResponse()
		{
			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var ex1Message = Factory.New<TSWMessage>();
			ex1Message.EM_MessageType = "RES";
			ex1Message.EM_MessageText = ErrorResponse;
			ex1Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(ex1Message);
			AssertEquals(header, ex1Message.EM_LinkedObject);
			AssertEquals(ErrorResponseFormatted, ex1Message.EM_MessageInterpretation);
		}

		public void TestCancelResponse()
		{
			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var ex1Message = Factory.New<TSWMessage>();
			ex1Message.EM_MessageType = MessageTypeList.Codes.TWR;
			ex1Message.EM_MessageText = CancelResponse;
			ex1Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(ex1Message);
			AssertEquals(header, ex1Message.EM_LinkedObject);
			AssertEquals(CancelResponseFormatted, ex1Message.EM_MessageInterpretation);
		}

		public void TestCancelResponseAfterInspectionSetsStatus()
		{
			declaration.JE_DeclarationReference = "B00001544";
			declaration.JE_MasterBill = "08600239481";
			declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.IAR;
			outgoingMessage.EM_ApplicationReference = "B00001544";

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var ex1Message = Factory.New<TSWMessage>();
			ex1Message.EM_MessageType = MessageTypeList.Codes.TWR;
			ex1Message.EM_MessageText = ResEx1;
			ex1Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(ex1Message);
			AssertEquals(header, ex1Message.EM_LinkedObject);
			AssertEquals("Entry header cancelled - no longer active", false, header.IsActive);
			AssertEquals("Entry header cancelled - CH_EntryStatus", FormalEntryStatusList.Codes.EntryCancelled, header.CH_EntryStatus);
			AssertEquals("NZCS cancelled - CH_NZCSStatus", StatusList.Codes.EntryCancelled, header.CH_NZCSStatus);
			AssertEquals("JE_TSWCombinedStatus", TSWStatus.StatusCodes.Cancelled, declaration.JE_TSWCombinedStatus);
			AssertEquals("JE_EntryStatus", FormalEntryStatusList.Codes.EntryCancelled, declaration.JE_EntryStatus);
		}

		public void TestFailuresAreReported()
		{
			var ex1Message = Factory.New<TSWMessage>();
			ex1Message.EM_MessageType = "RES";
			ex1Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ex1Message.EM_MessageText = ErrorResponse;
			ex1Message.EM_MessageNum = "M0123";

			var logger = new LoggingInformation();
			var response = new BaseTSWResponse(ex1Message);
			var processor = new EX1MessageProcessorForTest(logger);
			processor.ProcessMessage(ex1Message, new EX1Response(response));

			AssertEquals(EDIMessage.Status.Failed, ex1Message.EM_Status);
			var log = logger.Logs.First(l => l.Message.StartsWith("Error happened while processing "));
			AssertEquals("Error happened while processing Export Declaration TSW WCO Response: Simulated Failure. Please contact support to help resolve this problem.", log.Message);

			AssertEquals("Error Processing Response for Customs Export Declaration: B99999999  Message: M0123", ErrorReporter.LastMessageReported);
			AssertContains("Stack contains failing method", "ProcessCore()", ErrorReporter.LastExceptionReported.StackTrace);
			ErrorReporter.Clear();
		}

		#region Implementation

		class EX1MessageProcessorForTest : EX1MessageProcessor
		{
			public EX1MessageProcessorForTest(LoggingInformation logger) : base(logger)
			{
			}

			protected override void ProcessCore() => throw new ApplicationException("Simulated Failure.");
		}

		JobDeclaration declaration;
		CusEntryHeader header;
		TSWMessage outgoingMessage;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B99999999";
			declaration.JE_MasterBill = "08111111111";
			header = declaration.CusEntryHeader;
			var line1 = header.MergedLines.AddNew();
			line1.CL_LineNumber = 1;
			var line2 = header.MergedLines.AddNew();
			line2.CL_LineNumber = 2;
			var line3 = header.MergedLines.AddNew();
			line3.CL_LineNumber = 3;
			outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.E40;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "APPLICATION_REFERENCE";
			outgoingMessage.EM_LinkedObject = header;
		}

		#endregion

		#region Messages

		#region Error

		public const string ErrorResponse =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" >
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESE40</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response>
	<IssueDateTime formatCode=""204"">XXXXXXXXXX</IssueDateTime>
	<FunctionalReferenceID>2400</FunctionalReferenceID>
	<FunctionCode>48</FunctionCode>	
	<AdditionalInformation>
		<StatementDescription>XXXXXXXXXX</StatementDescription>
		<StatementTypeCode>XXXXXXXXXX</StatementTypeCode>
	</AdditionalInformation>
	<OverallDeclaration>
		<Declaration>
			<ID>04481317</ID>
			<FunctionalReferenceID>APPLICATION_REFERENCE</FunctionalReferenceID>
			<VersionID>XXXXXXXXXX</VersionID>
			<RejectionDateTime formatCode=""204"">XXXXXXXXXX</RejectionDateTime>
			<Submitter>
				<Name>XXXXXXXXXX</Name>
				<ID>XXXXXXXXXX</ID>
			</Submitter>			
			<ResponsibleGovernmentAgency>
				<ID>XXXXXXXXXX</ID>
			</ResponsibleGovernmentAgency>
		</Declaration>
	</OverallDeclaration>
	<Error>
		<ValidationCode>156</ValidationCode>
		<Pointer>
			<SequenceNumeric>1</SequenceNumeric>
			<DocumentSectionCode>42A</DocumentSectionCode>
		</Pointer>
		<Pointer>
			<DocumentSectionCode>67A</DocumentSectionCode>
		</Pointer>
		<Pointer>
			<SequenceNumeric>3</SequenceNumeric>
			<DocumentSectionCode>68A</DocumentSectionCode>
		</Pointer>
		<Pointer>
			<DocumentSectionCode>92A</DocumentSectionCode>
			<TagID>063</TagID>
		</Pointer>
	</Error>
	<Status>
		<EffectiveDateTime formatCode=""204"">XXXXXXXXXX</EffectiveDateTime>
		<NameCode>801</NameCode>
		<Pointer>
			<SequenceNumeric>1</SequenceNumeric>
			<DocumentSectionCode>XXXXXXXXXX</DocumentSectionCode>
			<TagID>XXXXXXXXXX</TagID>
		</Pointer>
	</Status>
</Response>
</DocumentMetadata>";

		const string ErrorResponseFormatted =
@"[New Zealand Customs Service - Error report] Response for Customs Export Declaration: B99999999

Error report
---------------------------------------------------------------------
Job Number     : B99999999
Master Bill    : 081-11111111
Entry Type     : Export (Normal)
Entry Number   : 04481317
Message No     : 2400

Message Status : (801) Lodgement rejected

Message Errors
---------------------------------------------------------------------
**Error** in {Declaration[1]/GoodsShipment/GovernmentAgencyGoodsItem[3]/Origin/CountryCode}:-
  Country/Region of Origin : Not current
";

		#endregion

		#region Cancel

		public const string CancelResponse =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" >
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESE40</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response>
	<IssueDateTime formatCode=""204"">XXXXXXXXXX</IssueDateTime>
	<FunctionalReferenceID>2400</FunctionalReferenceID>
	<FunctionCode>34</FunctionCode>
	<OverallDeclaration>
		<Declaration>
			<ID>04481317</ID>
			<FunctionalReferenceID>APPLICATION_REFERENCE</FunctionalReferenceID>
			<VersionID>XXXXXXXXXX</VersionID>
			<CancellationDateTime formatCode=""204"">XXXXXXXXXX</CancellationDateTime>
			<Submitter>
				<Name>XXXXXXXXXX</Name>
				<ID>XXXXXXXXXX</ID>
			</Submitter>
			<ResponsibleGovernmentAgency>
				<ID>XXXXXXXXXX</ID>
			</ResponsibleGovernmentAgency>
		</Declaration>
	</OverallDeclaration>
	<Status>
		<EffectiveDateTime formatCode=""204"">XXXXXXXXXX</EffectiveDateTime>
		<NameCode>814</NameCode>
		<Pointer>
			<SequenceNumeric>1</SequenceNumeric>
			<DocumentSectionCode>XXXXXXXXXX</DocumentSectionCode>
			<TagID>XXXXXXXXXX</TagID>
		</Pointer>
	</Status>
</Response>
</DocumentMetadata>";

		const string CancelResponseFormatted =
@"[New Zealand Customs Service - Confirmation of Transaction ] Response for Customs Export Declaration: B99999999

Confirmation of Transaction 
---------------------------------------------------------------------
Job Number     : B99999999
Master Bill    : 081-11111111
Entry Type     : Export (Normal)
Entry Number   : 04481317
Message No     : 2400

Message Status : (814) Lodgement cancelled
";

		public const string ResEx1 = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20141120110746</IssueDateTime>
    <FunctionalReferenceID>1864</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>41799507</ID>
        <AcceptanceDateTime formatCode=""204"">20141120110746</AcceptanceDateTime>
        <FunctionalReferenceID>B00001544</FunctionalReferenceID>
        <VersionID>3</VersionID>
        <CancellationDateTime formatCode=""204"">20141120110746</CancellationDateTime>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20141120110746</EffectiveDateTime>
      <NameCode>814</NameCode>
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
