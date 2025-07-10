using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.TradeSingleWindow.Testing
{
	public class OCRMessageProcessorTest : TestCaseWithFactory
	{
		public void TestErrorResponse()
		{
			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = "RES";
			ocrMessage.EM_MessageText = ErrorResponse;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ocrMessage);
			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			AssertEquals(ErrorResponseFormatted, ocrMessage.EM_MessageInterpretation);
		}

		public void TestAcceptanceResponse()
		{
			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = "RES";
			ocrMessage.EM_MessageText = AcceptanceResponse;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ocrMessage);
			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			AssertEquals(AcceptanceResponseFormatted, ocrMessage.EM_MessageInterpretation);
		}

		public void TestPortNotificationErrata()
		{
			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = "RES";
			ocrMessage.EM_MessageText = PortNotificationResponse;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ocrMessage);
			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			AssertEquals(PortNotificationResponseFormatted, ocrMessage.EM_MessageInterpretation);
		}

		public void TestThirdPartyClearanceResponse()
		{
			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			ocrMessage.EM_MessageText = ThirdPartyClearanceResponse;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ocrMessage);
			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			AssertEquals(ThirdPartyClearanceResponseFormatted, ocrMessage.EM_MessageInterpretation);
		}

		public void TestAcknowledgementResponse()
		{
			consol.JK_UniqueConsignRef = "C00001173";
			consol.JK_MasterBillNum = "08152834272";
			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-8""?>
<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRE</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>OCR</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>OCR</TypeCode>
  <FunctionalReferenceID>C00001173</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalInformation>
    <StatementCode>Y</StatementCode>
    <StatementTypeCode>CON</StatementTypeCode>
  </AdditionalInformation>
  <BorderTransportMeans>
    <Name>QF11</Name>
    <TypeCode>4</TypeCode>
    <DepartureDateTime formatCode=""102"">20140508</DepartureDateTime>
    <Itinerary>
      <SequenceNumeric>1</SequenceNumeric>
      <RoutingCountryCode>AU</RoutingCountryCode>
    </Itinerary>
  </BorderTransportMeans>
  <Carrier>
    <Name>QANTAS AIRWAYS LIMITED A REALLY GOOD AIRLINE FOR FREIGHT FORWARDING</Name>
  </Carrier>
  <Consignment>
    <SequenceNumeric>1</SequenceNumeric>
    <AdditionalDocument>
      <ID>UNKNOWN</ID>
      <TypeCode>EDO</TypeCode>
    </AdditionalDocument>
    <AssociatedTransportDocument>
      <ID>S00001233</ID>
      <TypeCode>HWB</TypeCode>
    </AssociatedTransportDocument>
    <TransportContractDocument>
      <ID>08152834272</ID>
      <TypeCode>MB</TypeCode>
      <Consolidator>
        <Name>EDI DEMONSTRATION SYSTEM NZ BASED IN MAKAU IN AUKLAND NEW ZEALAND</Name>
      </Consolidator>
    </TransportContractDocument>
  </Consignment>
  <ExitOffice>
    <ID>NZAKL</ID>
  </ExitOffice>
</Declaration>
</DocumentMetadata>";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "C00001173";
			outgoingMessage.EM_LinkedObject = consol;

			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = "RES";
			ocrMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse;
			ocrMessage.EM_MessageText =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20140508154803</IssueDateTime>
    <FunctionalReferenceID>1267</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>22428381</ID>
        <FunctionalReferenceID>C00001173</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20140508154803</EffectiveDateTime>
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
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ocrMessage);

			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			AssertEquals("Entry number from ACK message", "22428381", consol.CusEntryNums[0].CE_EntryNum);
			AssertEquals("response.EnterpriseStatus for Acknowledgement message", OutwardReportStatusList.Codes.Acknowledgement, consol.CusEntryNums[0].CE_EntryStatus);
		}

		public void TestErrorIDOverwritesACKID()
		{
			consol.JK_UniqueConsignRef = "C00001150";
			consol.JK_MasterBillNum = "08111111111";
			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_MessageText = OriginalOCR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "C00001150";
			outgoingMessage.EM_LinkedObject = consol;

			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = "RES";
			ocrMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement;
			ocrMessage.EM_MessageText = ACKResponse;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ocrMessage);
			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			AssertEquals("Entry number from ACK message is processed OK", consol.CusEntryNums[0].CE_EntryNum, "81039019");

			ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = "RES";
			ocrMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse;
			ocrMessage.EM_MessageText = NZCSErrorResponse;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(ocrMessage);
			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			AssertEquals("Error entry number from NZCS message should override ACK TSW Message Ref. & set entry number to spaces again", consol.CusEntryNums[0].CE_EntryNum, "");
		}

		public void TestACKIDDoesNotOverwriteErrorIDWhenProcessedOutOfOrder()
		{
			consol.JK_UniqueConsignRef = "C00001150";
			consol.JK_MasterBillNum = "08111111111";
			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_MessageText = OriginalOCR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "C00001150";
			outgoingMessage.EM_LinkedObject = consol;

			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = "RES";
			ocrMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse;
			ocrMessage.EM_MessageText = NZCSErrorResponse;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ocrMessage);
			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			AssertEquals("Error entry number from NZCS message when processed first", consol.CusEntryNums[0].CE_EntryNum, "");

			ocrMessage.EM_MessageType = "RES";
			ocrMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement;
			ocrMessage.EM_MessageText = ACKResponse;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(ocrMessage);
			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			AssertEquals("Entry number from ACK message when processed second should not override", consol.CusEntryNums[0].CE_EntryNum, "");
		}

		public void TestAddOCREvents_WhenAccepted()
		{
			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ocrMessage.EM_MessageText = AcceptanceResponse;
			Assert("Precondition: there is no ACC event.", !consol.Logs.HasLogWith(x => x.SL_SE_NKEvent == Events.MessageReceived.Code && x.SL_Reference == "ACC"));
			new MessageProcessorFactory(new LoggingInformation()).ProcessMessage(ocrMessage);
			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			Assert("It should create a new ACC event.", consol.Logs.HasLogWith(x => x.SL_SE_NKEvent == Events.MessageReceived.Code && x.SL_Reference == "ACC"));
		}

		public void TestAddOCREvents_WhenRejected()
		{
			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ocrMessage.EM_MessageText = ErrorResponse;
			Assert("Precondition: there is no FAL event.", !consol.Logs.HasLogWith(x => x.SL_SE_NKEvent == Events.MessageReceived.Code && x.SL_Reference == "FAL"));
			new MessageProcessorFactory(new LoggingInformation()).ProcessMessage(ocrMessage);
			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			Assert("It should create a new FAL event.", consol.Logs.HasLogWith(x => x.SL_SE_NKEvent == Events.MessageReceived.Code && x.SL_Reference == "FAL"));
		}

		public void TestAddOCREvents_WhenCancelled()
		{
			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ocrMessage.EM_MessageText = CancelResponse;
			Assert("Precondition: there is no CAN event.", !consol.Logs.HasLogWith(x => x.SL_SE_NKEvent == Events.MessageReceived.Code && x.SL_Reference == "CAN"));
			new MessageProcessorFactory(new LoggingInformation()).ProcessMessage(ocrMessage);
			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			Assert("It should create a new CAN event.", consol.Logs.HasLogWith(x => x.SL_SE_NKEvent == Events.MessageReceived.Code && x.SL_Reference == "CAN"));
		}

		public void TestNotifyPartyMessageGeneratesEmail()
		{
			// All matched messages that are not for the Submitter must generate the notification message email
			var matchedConsol = Factory.New<ForwardingConsol>();
			matchedConsol.JK_UniqueConsignRef = "C00001622";
			matchedConsol.JK_MasterBillNum = "BKG191009OBL1";

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "C00001622";
			outgoingMessage.EM_LinkedObject = matchedConsol;

			var ocrNotificationMessage = Factory.New<TSWMessage>();
			ocrNotificationMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			ocrNotificationMessage.EM_MessageText = NotificationResponse;
			ocrNotificationMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			var unsolicitedDOGroup = Factory.New<GlbGroup>();
			unsolicitedDOGroup.GG_Code = "USR";
			unsolicitedDOGroup.GG_Desc = "Unsolicited Delivery Order Responses";
			var staff = unsolicitedDOGroup.Staff.AddNew();
			staff.GS_Code = "TST";
			staff.GS_FullName = "John Tester";
			staff.GS_LoginName = "JT";
			staff.GS_EmailAddress = "JohnTester@TestingCompany.com";
			Factory.Save();

			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponsesSendToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unsolicitedDOGroup.PK.ToGuid());
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.EmailTo.NominatedGroup);

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ocrNotificationMessage);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull("Should have found the email generated from processing this Transhipment Destination Response message", email);
			AssertEquals(1, email.CCRecipients.Count);
			AssertEquals("JohnTester@TestingCompany.com", email.CCRecipients[0].Email);
		}

		public void TestACKStatusDoesNotOverwriteClearedStatusMessageTimeIsTheSame()
		{
			consol.JK_UniqueConsignRef = "C00001909";
			consol.JK_MasterBillNum = "08654675644";
			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_MessageText = OriginalOCR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "C00001909";
			outgoingMessage.EM_LinkedObject = consol;

			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse;
			ocrMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			ocrMessage.EM_MessageText = NZCSClearedResponse;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ocrMessage);
			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			AssertEquals("Entry number from NZCS Cleared message when processed first", consol.CusEntryNums[0].CE_EntryNum, "4901197");
			AssertEquals("Entry status", OutwardReportStatusList.Codes.Cleared, consol.CusEntryNums[0].CE_EntryStatus);

			ocrMessage.EM_MessageType = TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse;
			ocrMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement;
			ocrMessage.EM_MessageText = NZCSAcknowledgedResponse;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(ocrMessage);
			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			AssertEquals("Entry number should remain", consol.CusEntryNums[0].CE_EntryNum, "4901197");
			AssertEquals("Entry status from ACK message when processed second should not override CLR status", OutwardReportStatusList.Codes.Cleared, consol.CusEntryNums[0].CE_EntryStatus);
		}

		public void TestCLRStatusDoesOverwriteACKStatusIfMessageTimeIsTheSame()
		{
			consol.JK_UniqueConsignRef = "C00001909";
			consol.JK_MasterBillNum = "08654675644";
			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_MessageText = OriginalOCR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "C00001909";
			outgoingMessage.EM_LinkedObject = consol;

			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse;
			ocrMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement;
			ocrMessage.EM_MessageText = NZCSAcknowledgedResponse;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ocrMessage);
			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			AssertEquals("Entry number should be on ACK response", consol.CusEntryNums[0].CE_EntryNum, "4901197");
			AssertEquals("Entry status from ACK message", OutwardReportStatusList.Codes.Acknowledgement, consol.CusEntryNums[0].CE_EntryStatus);

			ocrMessage = Factory.New<TSWMessage>();
			ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse;
			ocrMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			ocrMessage.EM_MessageText = NZCSClearedResponse;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ocrMessage);
			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			AssertEquals("Entry number from NZCS Cleared message", consol.CusEntryNums[0].CE_EntryNum, "4901197");
			AssertEquals("Entry status from CLR message with same IssueDate should override ACK status to CLR status", OutwardReportStatusList.Codes.Cleared, consol.CusEntryNums[0].CE_EntryStatus);

			ocrMessage.EM_MessageType = TSWMessage.ResponseMessage.MessageSubTypes.TSWWCOResponse;
			ocrMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement;
			ocrMessage.EM_MessageText = NZCSAcknowledgedResponse;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(ocrMessage);
			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			AssertEquals("Entry number should remain", consol.CusEntryNums[0].CE_EntryNum, "4901197");
			AssertEquals("Entry status from ACK message when processed second should not override CLR status", OutwardReportStatusList.Codes.Cleared, consol.CusEntryNums[0].CE_EntryStatus);
		}

		#region Implementation

		ForwardingConsol consol;

		protected override void SetUp()
		{
			base.SetUp();

			consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C99999999";
			consol.JK_MasterBillNum = "08111111111";
			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "APPLICATION_REFERENCE";
			outgoingMessage.EM_LinkedObject = consol;
		}

		#endregion // Implementation

		#region Messages

		#region Error

		public const string ErrorResponse =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns='urn:wco:datamodel:WCO:ResponseModel:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
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
	<IssueDateTime formatCode=""204"">XXXXXXXXXX</IssueDateTime>
	<FunctionalReferenceID>XXXXXXXXXX</FunctionalReferenceID>
	<FunctionCode>48</FunctionCode>
	<AdditionalInformation>
		<StatementDescription>XXXXXXXXXX</StatementDescription>
		<StatementTypeCode>DIN</StatementTypeCode>
	</AdditionalInformation>
	<OverallDeclaration>
		<Declaration>
			<ID>XXXXXXXXXX</ID>
			<FunctionalReferenceID>APPLICATION_REFERENCE</FunctionalReferenceID>
			<VersionID>XXXXXXXXXX</VersionID>
			<RejectionDateTime formatCode=""204"">XXXXXXXXXX</RejectionDateTime>
			<Submitter>
				<Name>XXXXXXXXXX</Name>
				<ID>XXXXXXXXXX</ID>
			</Submitter>
			<ResponsibleGovernmentAgency>
				<ID>NZCS</ID>
			</ResponsibleGovernmentAgency>
		</Declaration>
	</OverallDeclaration>
	<Error>
		<ValidationCode>101</ValidationCode>
		<Pointer>
			<SequenceNumeric>1</SequenceNumeric>
			<DocumentSectionCode>XXXXXXXXXX</DocumentSectionCode>
			<TagID>XXXXXXXXXX</TagID>
		</Pointer>
	</Error>
	<Status>
		<EffectiveDateTime formatCode=""204"">XXXXXXXXXX</EffectiveDateTime>
		<NameCode>841</NameCode>
		<ReleaseDateTime formatCode=""204"">XXXXXXXXXX</ReleaseDateTime>
		<Pointer>
			<SequenceNumeric>1</SequenceNumeric>
			<DocumentSectionCode>XXXXXXXXXX</DocumentSectionCode>
			<TagID>XXXXXXXXXX</TagID>
		</Pointer>
	</Status>
</Response>
</DocumentMetadata>";

		const string ErrorResponseFormatted =
@"[Outward Report Rejected] Response for Consol: C99999999

Consol Number     : C99999999
Outward Report No : XXXXXXXXXX
Master Bill       : 08111111111

Rsp Message No    : XXXXXXXXXX
Message Type      : Error report
TSW Status Code   : 841 - OCR Rejected, error report herewith
Status            : Outward Report Rejected

Message Errors
---------------------------------------------------------------------
--Error Found     : Flight No. : Not specified
";

		#endregion // Error

		#region Acceptance

		public const string AcceptanceResponse =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns='urn:wco:datamodel:WCO:ResponseModel:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
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
  <IssueDateTime formatCode=""204"">XXXXXXXXXX</IssueDateTime>
  <FunctionalReferenceID>XXXXXXXXXX</FunctionalReferenceID>
  <FunctionCode>24</FunctionCode>
  <AdditionalDocument>
	<CategoryCode>CDO</CategoryCode>
	<ImageBinaryObject mimeCode=""application/pdf"" uri=""XXXXXXXXXX"" filename=""XXXXXXXXXX"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>
  <AdditionalInformation>
	<StatementDescription>XXXXXXXXXX</StatementDescription>
	<StatementTypeCode>DIN</StatementTypeCode>
  </AdditionalInformation>
  <OverallDeclaration>
	<Declaration>
	  <ID>XXXXXXXXXX</ID>
	  <AcceptanceDateTime formatCode=""204"">XXXXXXXXXX</AcceptanceDateTime>
	  <FunctionalReferenceID>APPLICATION_REFERENCE</FunctionalReferenceID>
	  <VersionID>XXXXXXXXXX</VersionID>
	  <Submitter>
		<Name>XXXXXXXXXX</Name>
		<ID>XXXXXXXXXX</ID>
	  </Submitter>
	  <ResponsibleGovernmentAgency>
		<ID>NZCS</ID>
	  </ResponsibleGovernmentAgency>
	</Declaration>
  </OverallDeclaration>
  <Status>
	<EffectiveDateTime formatCode=""204"">XXXXXXXXXX</EffectiveDateTime>
	<NameCode>847</NameCode>
	<ReleaseDateTime formatCode=""204"">XXXXXXXXXX</ReleaseDateTime>
	<Pointer>
	  <SequenceNumeric>1</SequenceNumeric>
	  <DocumentSectionCode>XXXXXXXXXX</DocumentSectionCode>
	  <TagID>XXXXXXXXXX</TagID>
	</Pointer>
  </Status>
</Response>
</DocumentMetadata>";

		const string AcceptanceResponseFormatted =
@"[Outward Report Accepted] Response for Consol: C99999999

Consol Number     : C99999999
Outward Report No : XXXXXXXXXX
Master Bill       : 08111111111

Rsp Message No    : XXXXXXXXXX
Message Type      : Clearance / Acceptance Instructions
TSW Status Code   : 847 - OCR Received OK
Status            : Outward Report Accepted
";

		#endregion // Acceptance

		#region Notification Response

		public const string NotificationResponse =
@"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">    <WCODataModelVersion>3.2</WCODataModelVersion>    <WCODocumentName>RES</WCODocumentName>    <CountryCode>NZ</CountryCode>    <AgencyAssignedCustomizedDocumentName>RESOCR</AgencyAssignedCustomizedDocumentName>    <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>    <CommunicationMetaData>      <Recipient>        <ID>7179L</ID>        <RoleCode>N2</RoleCode>      </Recipient>    </CommunicationMetaData>    <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">      <IssueDateTime formatCode=""204"">20191009111147</IssueDateTime>      <FunctionalReferenceID>2728</FunctionalReferenceID>      <FunctionCode>24</FunctionCode>      <OverallDeclaration>        <Declaration>          <ID>66600386</ID>          <AcceptanceDateTime formatCode=""204"">20191009111148</AcceptanceDateTime>          <FunctionalReferenceID>C00001622</FunctionalReferenceID>          <VersionID>1</VersionID>          <Submitter>            <Name>CargoWise 2</Name>          </Submitter>          <BorderTransportMeans>            <Name>AAL FREMANTLE</Name>            <ID>4823981</ID>            <TypeCode>1</TypeCode>            <DepartureDateTime formatCode=""102"">20191010</DepartureDateTime>            <JourneyID>82476</JourneyID>          </BorderTransportMeans>          <Consignment>            <SequenceNumeric>1</SequenceNumeric>            <AdditionalDocument>              <ID>9014883</ID>              <TypeCode>EDO</TypeCode>            </AdditionalDocument>            <AssociatedTransportDocument>              <ID>BKG191009HBL1</ID>              <TypeCode>HWB</TypeCode>            </AssociatedTransportDocument>            <TransportContractDocument>              <ID>BKG191009OBL1</ID>              <TypeCode>MB</TypeCode>              <Consolidator>                <Name>EDI DEMONSTRATION SYSTEM NZ BASED IN MAKAU IN AUKL</Name>              </Consolidator>            </TransportContractDocument>            <TransportEquipment>              <SequenceNumeric>1</SequenceNumeric>              <FullnessCode>7</FullnessCode>              <ID>BKGU1111110</ID>            </TransportEquipment>          </Consignment>          <Consignment>            <SequenceNumeric>2</SequenceNumeric>            <AdditionalDocument>              <ID>25619707</ID>              <TypeCode>EDO</TypeCode>            </AdditionalDocument>            <AssociatedTransportDocument>              <ID>BKG19109HBL2</ID>              <TypeCode>HWB</TypeCode>            </AssociatedTransportDocument>          </Consignment>          <ExitOffice>            <ID>NZAKL</ID>          </ExitOffice>          <ResponsibleGovernmentAgency>            <ID>NZCS</ID>          </ResponsibleGovernmentAgency>        </Declaration>      </OverallDeclaration>      <Status>        <EffectiveDateTime formatCode=""204"">20191009111147</EffectiveDateTime>        <NameCode>C07</NameCode>        <ReleaseDateTime formatCode=""204"">20191009111147</ReleaseDateTime>        <Pointer>          <DocumentSectionCode>07B</DocumentSectionCode>        </Pointer>        <Pointer>          <DocumentSectionCode>42A</DocumentSectionCode>        </Pointer>        <Pointer>          <SequenceNumeric>1</SequenceNumeric>          <DocumentSectionCode>08B</DocumentSectionCode>          <TagID>G007</TagID>        </Pointer>      </Status>    </Response>  </DocumentMetadata>";

		#endregion

		#region Third Party Clearance

		public const string ThirdPartyClearanceResponse =
@"<DocumentMetadata xmlns='urn:wco:datamodel:WCO:ResponseModel:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
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
  <IssueDateTime formatCode=""204"">XXXXXXXXXX</IssueDateTime>
  <FunctionalReferenceID>XXXXXXXXXX</FunctionalReferenceID>
  <FunctionCode>24</FunctionCode>
  <AdditionalDocument>
	<CategoryCode>CDO</CategoryCode>
	<ImageBinaryObject mimeCode=""application/pdf"" uri=""XXXXXXXXXX"" filename=""XXXXXXXXXX"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>
  <AdditionalInformation>
	<StatementDescription>XXXXXXXXXX</StatementDescription>
	<StatementTypeCode>DIN</StatementTypeCode>
  </AdditionalInformation>
  <OverallDeclaration>
	<Declaration>
	  <ID>XXXXXXXXXX</ID>
	  <AcceptanceDateTime formatCode=""204"">XXXXXXXXXX</AcceptanceDateTime>
	  <FunctionalReferenceID>APPLICATION_REFERENCE</FunctionalReferenceID>
	  <VersionID>XXXXXXXXXX</VersionID>
	  <Submitter>
		<Name>XXXXXXXXXX</Name>
		<ID>XXXXXXXXXX</ID>
	  </Submitter>
	  <AdditionalInformation>
		<StatementDescription>XXXXXXXXXX</StatementDescription>
		<StatementTypeCode>DIN</StatementTypeCode>
	  </AdditionalInformation>
	  <BorderTransportMeans>
		<Name>XXXXXXXXXX</Name>
		<ID>XXXXXXXXXX</ID>
		<TypeCode>XXXXXXXXXX</TypeCode>
		<JourneyID>XXXXXXXXXX</JourneyID>
	  </BorderTransportMeans>
	  <Consignment>
		<SequenceNumeric>1</SequenceNumeric>
		<AdditionalDocument>
		  <ID>XXXXXXXXXX</ID>
		  <TypeCode>XXXXXXXXXX</TypeCode>
		</AdditionalDocument>
		<AdditionalInformation>
		  <StatementDescription>XXXXXXXXXX</StatementDescription>
		  <StatementTypeCode>ICN</StatementTypeCode>
		</AdditionalInformation>
		<AssociatedTransportDocument>
		  <ID>XXXXXXXXXX</ID>
		  <TypeCode>BM</TypeCode>
		</AssociatedTransportDocument>
		<TransportContractDocument>
		  <ID>XXXXXXXXXX</ID>
		  <TypeCode>BN</TypeCode>
		  <Consolidator>
			<Name>XXXXXXXXXX</Name>
		  </Consolidator>
		</TransportContractDocument>
		<TransportEquipment>
		  <SequenceNumeric>1</SequenceNumeric>
		  <FullnessCode>7</FullnessCode>
		  <ID>XXXXXXXXXX</ID>
		</TransportEquipment>
	  </Consignment>
	  <ExitOffice>
		<ID>XXXXXXXXXX</ID>
	  </ExitOffice>
	  <ResponsibleGovernmentAgency>
		<ID>NZCS</ID>
	  </ResponsibleGovernmentAgency>
	</Declaration>
  </OverallDeclaration>
  <Status>
	<EffectiveDateTime formatCode=""204"">XXXXXXXXXX</EffectiveDateTime>
	<NameCode>847</NameCode>
	<ReleaseDateTime formatCode=""204"">XXXXXXXXXX</ReleaseDateTime>
	<Pointer>
	  <SequenceNumeric>1</SequenceNumeric>
	  <DocumentSectionCode>XXXXXXXXXX</DocumentSectionCode>
	  <TagID>XXXXXXXXXX</TagID>
	</Pointer>
  </Status>
</Response>
</DocumentMetadata>";

		const string ThirdPartyClearanceResponseFormatted =
@"[Outward Report Accepted] Response for Consol: C99999999

Consol Number     : C99999999
Outward Report No : XXXXXXXXXX
Master Bill       : 08111111111

Rsp Message No    : XXXXXXXXXX
Message Type      : Clearance / Acceptance Instructions
TSW Status Code   : 847 - OCR Received OK
Status            : Outward Report Accepted

Customs Instructions
---------------------------------------------------------------------
XXXXXXXXXX
";
		#endregion // Third Party Clearance

		#region Acknowledgement TSW ID followed by Error ID

		public const string OriginalOCR = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRE</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>OCR</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>OCR</TypeCode>
  <FunctionalReferenceID>C00001150</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalInformation>
    <StatementCode>Y</StatementCode>
    <StatementTypeCode>CON</StatementTypeCode>
  </AdditionalInformation>
  <BorderTransportMeans>
    <Name>QF108</Name>
    <TypeCode>4</TypeCode>
    <DepartureDateTime formatCode=""102"">20140318</DepartureDateTime>
    <Itinerary>
      <SequenceNumeric>1</SequenceNumeric>
      <RoutingCountryCode>AU</RoutingCountryCode>
    </Itinerary>
  </BorderTransportMeans>
  <Carrier>
    <Name>QANTAS AIRWAYS LIMITED</Name>
  </Carrier>
  <Consignment>
    <SequenceNumeric>1</SequenceNumeric>
    <AdditionalDocument>
      <ID />
      <TypeCode>EDO</TypeCode>
    </AdditionalDocument>
    <AssociatedTransportDocument>
      <ID>S00001203</ID>
      <TypeCode>HWB</TypeCode>
    </AssociatedTransportDocument>
    <TransportContractDocument>
      <ID>08148234723</ID>
      <TypeCode>MB</TypeCode>
      <Consolidator>
        <Name>EDI Demonstration System NZ</Name>
      </Consolidator>
    </TransportContractDocument>
  </Consignment>
  <ExitOffice>
    <ID>NZAKL</ID>
  </ExitOffice>
</Declaration>
</DocumentMetadata>";

		public const string ACKResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20140318132136</IssueDateTime>
    <FunctionalReferenceID>1131</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>81039019</ID>
        <FunctionalReferenceID>C00001150</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20140318132136</EffectiveDateTime>
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

		public const string NZCSErrorResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20140318132146</IssueDateTime>
    <FunctionalReferenceID>1132</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>00000000</ID>
        <FunctionalReferenceID>C00001150</FunctionalReferenceID>
        <RejectionDateTime formatCode=""204"">20140318132146</RejectionDateTime>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode>ERR</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
        </Consignment>
        <ExitOffice>
          <ID>NZAKL</ID>
        </ExitOffice>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>678</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>02A</DocumentSectionCode>
        <TagID>D005</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20140318132146</EffectiveDateTime>
      <NameCode>841</NameCode>
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

		#region Cleared Same Response Time as Acknowledged Message

		public const string NZCSClearedResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20241001170409</IssueDateTime>
    <FunctionalReferenceID>666</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>4901197</ID>
        <AcceptanceDateTime formatCode=""204"">20241001170409</AcceptanceDateTime>
        <FunctionalReferenceID>C00001909</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20241001170429</EffectiveDateTime>
      <NameCode>847</NameCode>
      <ReleaseDateTime formatCode=""204"">20241001170429</ReleaseDateTime>
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

		public const string NZCSAcknowledgedResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20241001170409</IssueDateTime>
    <FunctionalReferenceID>665</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>4901197</ID>
        <FunctionalReferenceID>C00001909</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20241001170409</EffectiveDateTime>
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

		#endregion

		#region Port Notification

		public const string PortNotificationResponse =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns='urn:wco:datamodel:WCO:ResponseModel:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
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
  <IssueDateTime formatCode=""204"">XXXXXXXXXX</IssueDateTime>
  <FunctionalReferenceID>XXXXXXXXXX</FunctionalReferenceID>
  <FunctionCode>24</FunctionCode>
  <AdditionalDocument>
	<CategoryCode>CDO</CategoryCode>
	<ImageBinaryObject mimeCode=""application/pdf"" uri=""XXXXXXXXXX"" filename=""XXXXXXXXXX"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>
  <AdditionalInformation>
	<StatementDescription>XXXXXXXXXX</StatementDescription>
	<StatementTypeCode>DIN</StatementTypeCode>
  </AdditionalInformation>
  <OverallDeclaration>
	<Declaration>
	  <ID>XXXXXXXXXX</ID>
	  <AcceptanceDateTime formatCode=""204"">XXXXXXXXXX</AcceptanceDateTime>
	  <FunctionalReferenceID>APPLICATION_REFERENCE</FunctionalReferenceID>
	  <VersionID>XXXXXXXXXX</VersionID>
	  <Submitter>
		<Name>XXXXXXXXXX</Name>
		<ID>XXXXXXXXXX</ID>
	  </Submitter>
	  <ResponsibleGovernmentAgency>
		<ID>NZCS</ID>
	  </ResponsibleGovernmentAgency>
	</Declaration>
  </OverallDeclaration>
  <Status>
	<EffectiveDateTime formatCode=""204"">XXXXXXXXXX</EffectiveDateTime>
	<NameCode>846</NameCode>
	<ReleaseDateTime formatCode=""204"">XXXXXXXXXX</ReleaseDateTime>
	<Pointer>
	  <SequenceNumeric>1</SequenceNumeric>
	  <DocumentSectionCode>XXXXXXXXXX</DocumentSectionCode>
	  <TagID>XXXXXXXXXX</TagID>
	</Pointer>
  </Status>
</Response>
</DocumentMetadata>";

		const string PortNotificationResponseFormatted =
@"[Outward Report Accepted] Response for Consol: C99999999

Consol Number     : C99999999
Outward Report No : XXXXXXXXXX
Master Bill       : 08111111111

Rsp Message No    : XXXXXXXXXX
Message Type      : Clearance / Acceptance Instructions
TSW Status Code   : 846 - Export Clearance Port Notification **NOTE: users will need to continue providing manual delivery orders to Port Authorities till a fix is made in JBMS.
Status            : Outward Report Accepted
";

		#endregion // Acceptance

		#region Cancel

		public const string CancelResponse =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns='urn:wco:datamodel:WCO:ResponseModel:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
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
	<IssueDateTime formatCode=""204"">XXXXXXXXXX</IssueDateTime>
	<FunctionalReferenceID>XXXXXXXXXX</FunctionalReferenceID>
	<FunctionCode>48</FunctionCode>
	<AdditionalInformation>
		<StatementDescription>XXXXXXXXXX</StatementDescription>
		<StatementTypeCode>DIN</StatementTypeCode>
	</AdditionalInformation>
	<OverallDeclaration>
		<Declaration>
			<ID>XXXXXXXXXX</ID>
			<FunctionalReferenceID>APPLICATION_REFERENCE</FunctionalReferenceID>
			<VersionID>XXXXXXXXXX</VersionID>
			<RejectionDateTime formatCode=""204"">XXXXXXXXXX</RejectionDateTime>
			<Submitter>
				<Name>XXXXXXXXXX</Name>
				<ID>XXXXXXXXXX</ID>
			</Submitter>
			<ResponsibleGovernmentAgency>
				<ID>NZCS</ID>
			</ResponsibleGovernmentAgency>
		</Declaration>
	</OverallDeclaration>
	<Error>
		<ValidationCode>101</ValidationCode>
		<Pointer>
			<SequenceNumeric>1</SequenceNumeric>
			<DocumentSectionCode>XXXXXXXXXX</DocumentSectionCode>
			<TagID>XXXXXXXXXX</TagID>
		</Pointer>
	</Error>
	<Status>
		<EffectiveDateTime formatCode=""204"">XXXXXXXXXX</EffectiveDateTime>
		<NameCode>814</NameCode>
		<ReleaseDateTime formatCode=""204"">XXXXXXXXXX</ReleaseDateTime>
		<Pointer>
			<SequenceNumeric>1</SequenceNumeric>
			<DocumentSectionCode>XXXXXXXXXX</DocumentSectionCode>
			<TagID>XXXXXXXXXX</TagID>
		</Pointer>
	</Status>
</Response>
</DocumentMetadata>";

		#endregion

		#endregion // Messages
	}

	public class MessageAttachmentsTest : TestCaseWithFactory
	{
		public void TestAttachedDocumentIsProcessed()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000000031";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = ClearWithDeliveryOrder;
			interchange.DocManagerInfo().AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <DeliveryOrder> %EOF\n"), "OCR_Delivery_Order-31723722-2014-08-25-191822892.pdf", "FCT");
			AssertEquals("Pre-condition: Test Delivery Order should be attached to interchange", 1, ((IDocManagerSupport)interchange).DocManagerInfo.AllEDocs.Count);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001198";
			consol.JK_MasterBillNum = "08154234294";
			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_MessageText = OriginalOCR;
			outgoingMessage.EM_MessageNum = "1";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "C00001198";
			outgoingMessage.EM_LinkedObject = consol;

			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_EI = interchange.PK;
			ocrMessage.EM_MessageType = "RES";
			ocrMessage.EM_MessageText = ClearWithDeliveryOrder;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ocrMessage.EM_LinkedObject = consol;
			interchange.DocManagerInfo().MasterFactory.Save();
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ocrMessage);
			Factory.Save();

			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			var reLoadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			AssertEquals("Delivery Order should have been attached to Consol", 1, ((IDocManagerSupport)reLoadedConsol).DocManagerInfo.AllEDocs.Count);
			var consolDoc = ((IDocManagerSupport)reLoadedConsol).DocManagerInfo.AllEDocs[0];
			AssertEquals("OCR Deliver Order file name", "OCR_DeliveryOrder_C00001198.pdf", consolDoc.FileName);
			AssertEquals("Doc Type should be Delivery Order", Core.Constants.RefDocTypes.DeliveryOrder, consolDoc.DocType);
		}

		public void TestAttachedDocumentFromReplacement()
		{
			#region Original Dec Processing
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000004772";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = ClearWithDO;
			interchange.DocManagerInfo().AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <DeliveryOrder> %EOF\n"), "OCR_Delivery_Order-23887197-2014-10-10-135448874.pdf", "FCT");
			AssertEquals("Pre-condition: Test Delivery Order should be attached to interchange", 1, ((IDocManagerSupport)interchange).DocManagerInfo.AllEDocs.Count);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001239";
			consol.JK_MasterBillNum = "OB545424";
			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_MessageText = OriginalOCRDec;
			outgoingMessage.EM_MessageNum = "1";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "C00001239";
			outgoingMessage.EM_LinkedObject = consol;

			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_EI = interchange.PK;
			ocrMessage.EM_MessageType = "RES";
			ocrMessage.EM_MessageText = ClearWithDO;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ocrMessage.EM_LinkedObject = consol;
			interchange.DocManagerInfo().MasterFactory.Save();
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ocrMessage);
			Factory.Save();

			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			var reLoadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			AssertEquals("Delivery Order should have been attached to Consol", 1, ((IDocManagerSupport)reLoadedConsol).DocManagerInfo.AllEDocs.Count);
			var consolDoc = ((IDocManagerSupport)reLoadedConsol).DocManagerInfo.AllEDocs[0];
			AssertEquals("OCR Deliver Order file name", "OCR_DeliveryOrder_C00001239.pdf", consolDoc.FileName);
			AssertEquals("Doc Type should be Delivery Order", Core.Constants.RefDocTypes.DeliveryOrder, consolDoc.DocType);
			#endregion

			#region Replacement Dec Processing
			var interchangeReplaceResponse = Factory.New<NZCInterchange>();
			interchangeReplaceResponse.EI_From = "CUSSWT";
			interchangeReplaceResponse.EI_To = "00009908C";
			interchangeReplaceResponse.EI_ApplicationCode = "NZC";
			interchangeReplaceResponse.EI_InterchangeType = "NZC";
			interchangeReplaceResponse.EI_InterchangeNum = "00000000000000004774";
			interchangeReplaceResponse.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeReplaceResponse.EI_IsActive = true;
			interchangeReplaceResponse.EI_BodyText = ResponseClearWithDO;

			interchangeReplaceResponse.DocManagerInfo().AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <DeliveryOrder> %EOF\n"), "OCR_Delivery_Order-23887197-2014-10-10-154808297.pdf", "FCT");
			Factory.Save();
			AssertEquals("Pre-condition: Test Delivery Order should be attached to interchange", 1, ((IDocManagerSupport)interchangeReplaceResponse).DocManagerInfo.AllEDocs.Count);

			var outgoingReplacementMessage = Factory.New<TSWMessage>();
			outgoingReplacementMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingReplacementMessage.EM_MessageText = ReplacementOCRDec;
			outgoingReplacementMessage.EM_MessageNum = "1";
			outgoingReplacementMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingReplacementMessage.EM_ApplicationReference = "C00001239";
			outgoingReplacementMessage.EM_LinkedObject = consol;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			var ocrReplaceResponse = Factory.New<TSWMessage>();
			ocrReplaceResponse.EM_EI = interchangeReplaceResponse.PK;
			ocrReplaceResponse.EM_MessageType = "RES";
			ocrReplaceResponse.EM_MessageText = ResponseClearWithDO;
			ocrReplaceResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ocrReplaceResponse.EM_LinkedObject = consol;
			interchangeReplaceResponse.DocManagerInfo().MasterFactory.Save();
			Factory.Save();

			processor.ProcessMessage(ocrReplaceResponse);
			Factory.Save();

			AssertEquals(consol, ocrReplaceResponse.EM_LinkedObject);
			var reLoadedConsolAgain = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			AssertEquals("Replacement Delivery Order should have been attached to Consol - should now be 2 documents attached", 2, ((IDocManagerSupport)reLoadedConsolAgain).DocManagerInfo.AllEDocs.Count);

			var consolDoc2 = ((IDocManagerSupport)reLoadedConsolAgain).DocManagerInfo.AllEDocs[1];
			AssertEquals("Replacement OCR Deliver Order file name", "OCR_DeliveryOrder_C00001239.pdf", consolDoc2.FileName);
			AssertEquals("Replacement Doc Type should still be Delivery Order", Core.Constants.RefDocTypes.DeliveryOrder, consolDoc2.DocType);
			#endregion
		}

		public void TestAttachedDocumentsWhenConsolAlreadyHasDocuments()
		{
			#region Original Dec Processing
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001239";
			consol.JK_MasterBillNum = "OB545424";
			consol.DocManagerInfo().AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <ConsolInvoice> %EOF\n"), "ConsolInvoice.pdf", "INV");
			Factory.Save();

			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000004772";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = ClearWithDO;
			interchange.DocManagerInfo().AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <DeliveryOrder> %EOF\n"), "OCR_Delivery_Order-23887197-2014-10-10-135448874.pdf", "FCT");
			AssertEquals("Pre-condition: Test Delivery Order should be attached to interchange", 1, ((IDocManagerSupport)interchange).DocManagerInfo.AllEDocs.Count);

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_MessageText = OriginalOCRDec;
			outgoingMessage.EM_MessageNum = "1";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "C00001239";
			outgoingMessage.EM_LinkedObject = consol;

			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_EI = interchange.PK;
			ocrMessage.EM_MessageType = "RES";
			ocrMessage.EM_MessageText = ClearWithDO;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ocrMessage.EM_LinkedObject = consol;
			interchange.DocManagerInfo().MasterFactory.Save();
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ocrMessage);
			Factory.Save();

			AssertEquals(consol, ocrMessage.EM_LinkedObject);
			var reLoadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			AssertEquals("Delivery Order should have been attached to Consol", 1, ((IDocManagerSupport)reLoadedConsol).DocManagerInfo.AllEDocs.Count);
			var consolDoc = ((IDocManagerSupport)reLoadedConsol).DocManagerInfo.AllEDocs[0];
			AssertEquals("OCR Deliver Order file name", "OCR_DeliveryOrder_C00001239.pdf", consolDoc.FileName);
			AssertEquals("Doc Type should be Delivery Order", Core.Constants.RefDocTypes.DeliveryOrder, consolDoc.DocType);
			#endregion

			#region Replacement Dec Processing
			var interchangeReplaceResponse = Factory.New<NZCInterchange>();
			interchangeReplaceResponse.EI_From = "CUSSWT";
			interchangeReplaceResponse.EI_To = "00009908C";
			interchangeReplaceResponse.EI_ApplicationCode = "NZC";
			interchangeReplaceResponse.EI_InterchangeType = "NZC";
			interchangeReplaceResponse.EI_InterchangeNum = "00000000000000004774";
			interchangeReplaceResponse.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchangeReplaceResponse.EI_IsActive = true;
			interchangeReplaceResponse.EI_BodyText = ResponseClearWithDO;

			interchangeReplaceResponse.DocManagerInfo().AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <DeliveryOrder> %EOF\n"), "OCR_Delivery_Order-23887197-2014-10-10-154808297.pdf", "FCT");
			Factory.Save();
			AssertEquals("Pre-condition: Test Delivery Order should be attached to interchange", 1, ((IDocManagerSupport)interchangeReplaceResponse).DocManagerInfo.AllEDocs.Count);

			var outgoingReplacementMessage = Factory.New<TSWMessage>();
			outgoingReplacementMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingReplacementMessage.EM_MessageText = ReplacementOCRDec;
			outgoingReplacementMessage.EM_MessageNum = "1";
			outgoingReplacementMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingReplacementMessage.EM_ApplicationReference = "C00001239";
			outgoingReplacementMessage.EM_LinkedObject = consol;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			var ocrReplaceResponse = Factory.New<TSWMessage>();
			ocrReplaceResponse.EM_EI = interchangeReplaceResponse.PK;
			ocrReplaceResponse.EM_MessageType = "RES";
			ocrReplaceResponse.EM_MessageText = ResponseClearWithDO;
			ocrReplaceResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ocrReplaceResponse.EM_LinkedObject = consol;
			interchangeReplaceResponse.DocManagerInfo().MasterFactory.Save();
			Factory.Save();

			processor.ProcessMessage(ocrReplaceResponse);
			Factory.Save();

			AssertEquals(consol, ocrReplaceResponse.EM_LinkedObject);
			var reLoadedConsolAgain = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			AssertEquals("Replacement Delivery Order should have been attached to Consol - should now be 2 documents attached to Consol", 2, ((IDocManagerSupport)reLoadedConsolAgain).DocManagerInfo.AllEDocs.Count);
			var consolDoc2 = ((IDocManagerSupport)reLoadedConsolAgain).DocManagerInfo.AllEDocs[1];
			AssertEquals("Replacement OCR Deliver Order file name", "OCR_DeliveryOrder_C00001239.pdf", consolDoc2.FileName);
			AssertEquals("Replacement Doc Type should still be Delivery Order", Core.Constants.RefDocTypes.DeliveryOrder, consolDoc2.DocType);
			#endregion
		}

		#region Messages

		#region Original and DO Response

		public const string OriginalOCR = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRE</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>OCR</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>OCR</TypeCode>
  <FunctionalReferenceID>C00001150</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalInformation>
    <StatementCode>Y</StatementCode>
    <StatementTypeCode>CON</StatementTypeCode>
  </AdditionalInformation>
  <BorderTransportMeans>
    <Name>QF108</Name>
    <TypeCode>4</TypeCode>
    <DepartureDateTime formatCode=""102"">20140318</DepartureDateTime>
    <Itinerary>
      <SequenceNumeric>1</SequenceNumeric>
      <RoutingCountryCode>AU</RoutingCountryCode>
    </Itinerary>
  </BorderTransportMeans>
  <Carrier>
    <Name>QANTAS AIRWAYS LIMITED</Name>
  </Carrier>
  <Consignment>
    <SequenceNumeric>1</SequenceNumeric>
    <AdditionalDocument>
      <ID />
      <TypeCode>EDO</TypeCode>
    </AdditionalDocument>
    <AssociatedTransportDocument>
      <ID>S00001203</ID>
      <TypeCode>HWB</TypeCode>
    </AssociatedTransportDocument>
    <TransportContractDocument>
      <ID>08148234723</ID>
      <TypeCode>MB</TypeCode>
      <Consolidator>
        <Name>EDI Demonstration System NZ</Name>
      </Consolidator>
    </TransportContractDocument>
  </Consignment>
  <ExitOffice>
    <ID>NZAKL</ID>
  </ExitOffice>
</Declaration>
</DocumentMetadata>";

		public const string ClearWithDeliveryOrder =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20140825191813</IssueDateTime>
    <FunctionalReferenceID>1556</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>CDO</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_3D904F43-C050-416B-B007-FC9C25B0430F"" filename=""OCR_Delivery_Order-31723722-2014-08-25-191822892.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <OverallDeclaration>
      <Declaration>
        <ID>31723722</ID>
        <AcceptanceDateTime formatCode=""204"">20140825191813</AcceptanceDateTime>
        <FunctionalReferenceID>C00001198</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ExitOffice>
          <ID>NZAKL</ID>
        </ExitOffice>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20140825191813</EffectiveDateTime>
      <NameCode>847</NameCode>
      <ReleaseDateTime formatCode=""204"">20140825191813</ReleaseDateTime>
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

		#region Replacement DO After Original DO

		public const string OriginalOCRDec = @"<?xml version=""1.0"" encoding=""utf-8""?>
<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRE</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>OCR</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>OCR</TypeCode>
  <FunctionalReferenceID>C00001239</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalInformation>
    <StatementCode>Y</StatementCode>
    <StatementTypeCode>CON</StatementTypeCode>
  </AdditionalInformation>
  <BorderTransportMeans>
    <Name>CAI YUN HE</Name>
    <ID>9228758</ID>
    <TypeCode>1</TypeCode>
    <DepartureDateTime formatCode=""102"">20141010</DepartureDateTime>
    <JourneyID>57W</JourneyID>
    <Itinerary>
      <SequenceNumeric>1</SequenceNumeric>
      <RoutingCountryCode>AU</RoutingCountryCode>
    </Itinerary>
  </BorderTransportMeans>
  <Carrier>
    <Name>A.A.L. SHIPPING AGENCIES P/L</Name>
  </Carrier>
  <Consignment>
    <SequenceNumeric>1</SequenceNumeric>
    <AdditionalDocument>
      <ID>42345528</ID>
      <TypeCode>EDO</TypeCode>
    </AdditionalDocument>
    <AssociatedTransportDocument>
      <ID>S00001325</ID>
      <TypeCode>HWB</TypeCode>
    </AssociatedTransportDocument>
    <TransportContractDocument>
      <ID>OB545424</ID>
      <TypeCode>MB</TypeCode>
      <Consolidator>
        <Name>EDI DEMONSTRATION SYSTEM NZ BASED IN MAKAU IN AUKL</Name>
      </Consolidator>
    </TransportContractDocument>
  </Consignment>
  <Consignment>
    <SequenceNumeric>2</SequenceNumeric>
    <AdditionalDocument>
      <ID>42569860</ID>
      <TypeCode>EDO</TypeCode>
    </AdditionalDocument>
    <AssociatedTransportDocument>
      <ID>S00001326</ID>
      <TypeCode>HWB</TypeCode>
    </AssociatedTransportDocument>
  </Consignment>
  <Consignment>
    <SequenceNumeric>3</SequenceNumeric>
    <AdditionalDocument>
      <ID>69827441</ID>
      <TypeCode>EDO</TypeCode>
    </AdditionalDocument>
    <AssociatedTransportDocument>
      <ID>S00001327</ID>
      <TypeCode>HWB</TypeCode>
    </AssociatedTransportDocument>
  </Consignment>
  <ExitOffice>
    <ID>NZAKL</ID>
  </ExitOffice>
</Declaration>
</DocumentMetadata>";

		public const string ClearWithDO =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20141010135440</IssueDateTime>
    <FunctionalReferenceID>1729</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>CDO</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_667F208A-14E3-430A-9A94-2AD6A0CB5969"" filename=""OCR_Delivery_Order-23887197-2014-10-10-135448874.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <OverallDeclaration>
      <Declaration>
        <ID>23887197</ID>
        <AcceptanceDateTime formatCode=""204"">20141010135440</AcceptanceDateTime>
        <FunctionalReferenceID>C00001239</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ExitOffice>
          <ID>NZAKL</ID>
        </ExitOffice>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20141010135440</EffectiveDateTime>
      <NameCode>847</NameCode>
      <ReleaseDateTime formatCode=""204"">20141010135440</ReleaseDateTime>
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

		public const string ReplacementOCRDec = @"<?xml version=""1.0"" encoding=""utf-8""?>
<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRE</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>OCR</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <ID>23887197</ID>
  <TypeCode>OCR</TypeCode>
  <FunctionalReferenceID>C00001239</FunctionalReferenceID>
  <FunctionCode>5</FunctionCode>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalInformation>
    <StatementCode>Y</StatementCode>
    <StatementTypeCode>CON</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementDescription>testing</StatementDescription>
    <StatementTypeCode>AES</StatementTypeCode>
  </AdditionalInformation>
  <BorderTransportMeans>
    <Name>CAI YUN HE</Name>
    <ID>9228758</ID>
    <TypeCode>1</TypeCode>
    <DepartureDateTime formatCode=""102"">20141010</DepartureDateTime>
    <JourneyID>57W</JourneyID>
    <Itinerary>
      <SequenceNumeric>1</SequenceNumeric>
      <RoutingCountryCode>AU</RoutingCountryCode>
    </Itinerary>
  </BorderTransportMeans>
  <Carrier>
    <Name>A.A.L. SHIPPING AGENCIES P/L</Name>
  </Carrier>
  <Consignment>
    <SequenceNumeric>1</SequenceNumeric>
    <AdditionalDocument>
      <ID>69827441</ID>
      <TypeCode>EDO</TypeCode>
    </AdditionalDocument>
    <AssociatedTransportDocument>
      <ID>S00001327</ID>
      <TypeCode>HWB</TypeCode>
    </AssociatedTransportDocument>
    <TransportContractDocument>
      <ID>OB545424</ID>
      <TypeCode>MB</TypeCode>
      <Consolidator>
        <Name>EDI DEMONSTRATION SYSTEM NZ BASED IN MAKAU IN AUKL</Name>
      </Consolidator>
    </TransportContractDocument>
  </Consignment>
  <Consignment>
    <SequenceNumeric>2</SequenceNumeric>
    <AdditionalDocument>
      <ID>42345528</ID>
      <TypeCode>EDO</TypeCode>
    </AdditionalDocument>
    <AssociatedTransportDocument>
      <ID>S00001325</ID>
      <TypeCode>HWB</TypeCode>
    </AssociatedTransportDocument>
  </Consignment>
  <Consignment>
    <SequenceNumeric>3</SequenceNumeric>
    <AdditionalDocument>
      <ID>42569860</ID>
      <TypeCode>EDO</TypeCode>
    </AdditionalDocument>
    <AssociatedTransportDocument>
      <ID>S00001326</ID>
      <TypeCode>HWB</TypeCode>
    </AssociatedTransportDocument>
  </Consignment>
  <ExitOffice>
    <ID>NZAKL</ID>
  </ExitOffice>
</Declaration>
</DocumentMetadata>";

		public const string ResponseClearWithDO =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20141010154801</IssueDateTime>
    <FunctionalReferenceID>1731</FunctionalReferenceID>
    <FunctionCode>34</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>CDO</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_D4A554BA-0AD9-4F44-8D32-D664DB32239F"" filename=""OCR_Delivery_Order-23887197-2014-10-10-154808297.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <OverallDeclaration>
      <Declaration>
        <ID>23887197</ID>
        <AcceptanceDateTime formatCode=""204"">20141010154801</AcceptanceDateTime>
        <FunctionalReferenceID>C00001239</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ExitOffice>
          <ID>NZAKL</ID>
        </ExitOffice>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20141010154801</EffectiveDateTime>
      <NameCode>830</NameCode>
      <ReleaseDateTime formatCode=""204"">20141010154803</ReleaseDateTime>
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

	public class SendOCRFromManifestResponsesAreProcessed : TestCaseWithFactory
	{
		public void TestProcessResponseToManifest_WhenAccepted()
		{
			manifest.AMA_JobReference = "MAN0000025";
			manifest.AMA_MessageStatus = Common.NZ.OutwardReportStatusList.Codes.AwaitingResponse;
			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "MAN0000025";
			outgoingMessage.EM_LinkTable = "AsycudaManifestHeader";
			outgoingMessage.EM_LinkUniqueID = manifest.PK;

			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ocrMessage.EM_MessageText = ManifestAcceptanceResponse;

			new MessageProcessorFactory(new LoggingInformation()).ProcessMessage(ocrMessage);
			AssertEquals(manifest, ocrMessage.EM_LinkedObject);
			AssertEquals("Manifest status has been updated", Common.NZ.OutwardReportStatusList.Codes.Acknowledgement, manifest.RegistrationStatus);
			AssertEquals("Message status has been updated", NZMessageStatusList.Codes.Acknowledged, manifest.AMA_MessageStatus);
			AssertEquals("Manifest entry number", "64872109", manifest.RegistrationNumber);
		}

		public void TestProcessRejectedResponseToManifest()
		{
			manifest.AMA_JobReference = "MAN0000025";
			manifest.AMA_MessageStatus = Common.NZ.OutwardReportStatusList.Codes.AwaitingResponse;
			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "MAN0000025";
			outgoingMessage.EM_LinkTable = "AsycudaManifestHeader";
			outgoingMessage.EM_LinkUniqueID = manifest.PK;

			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ocrMessage.EM_MessageText = ManifestErrorResponse;

			new MessageProcessorFactory(new LoggingInformation()).ProcessMessage(ocrMessage);
			AssertEquals(manifest, ocrMessage.EM_LinkedObject);
			AssertEquals("Manifest status has been updated", Common.NZ.OutwardReportStatusList.Codes.Rejected, manifest.RegistrationStatus);
			AssertEquals("Message status has been updated", NZMessageStatusList.Codes.Accepted, manifest.AMA_MessageStatus);
		}

		public void TestProcessCancelledResponse()
		{
			manifest.MasterBill.ABL_BillNumber = "086-00023192";
			manifest.AMA_JobReference = "MAN0000039";
			manifest.AMA_MessageStatus = Common.NZ.OutwardReportStatusList.Codes.AwaitingResponse;
			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_MessageSubType = NZ.TradeSingleWindow.MessageSubTypeList.Codes.Cancellation;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "MAN0000039";
			outgoingMessage.EM_LinkTable = "AsycudaManifestHeader";
			outgoingMessage.EM_LinkUniqueID = manifest.PK;

			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ocrMessage.EM_MessageText = ManifestCanceledResponse;

			new MessageProcessorFactory(new LoggingInformation()).ProcessMessage(ocrMessage);
			AssertEquals(manifest, ocrMessage.EM_LinkedObject);
			AssertEquals("Manifest status has been updated", Common.NZ.OutwardReportStatusList.Codes.Cancelled, manifest.RegistrationStatus);
			AssertEquals("Message status has been updated", NZMessageStatusList.Codes.Accepted, manifest.AMA_MessageStatus);
			AssertEquals("Manifest entry/registration number from clearance is retained", "8147448", manifest.RegistrationNumber);
			AssertEquals("Formatted Response Expected.", FormattedCancelResponseExpected, ocrMessage.EM_MessageInterpretation);
		}

		public void TestClearedResponseIsProcessedAndLinkedToManifest()
		{
			manifest.AMA_JobReference = "MAN0000026";
			manifest.AMA_MessageStatus = Common.NZ.OutwardReportStatusList.Codes.AwaitingResponse;
			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "MAN0000026";
			outgoingMessage.EM_LinkTable = "AsycudaManifestHeader";
			outgoingMessage.EM_LinkUniqueID = manifest.PK;

			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ocrMessage.EM_MessageText = ManifestClearedResponse;

			new MessageProcessorFactory(new LoggingInformation()).ProcessMessage(ocrMessage);
			AssertEquals(manifest, ocrMessage.EM_LinkedObject);
			AssertEquals("Manifest status has been updated", Common.NZ.OutwardReportStatusList.Codes.Cleared, manifest.RegistrationStatus);
			AssertEquals("Message status has been updated", NZMessageStatusList.Codes.Accepted, manifest.AMA_MessageStatus);
			AssertEquals("Manifest registration date has been updated", new ZDateTime(2022, 02, 16, 11, 56, 22), manifest.RegistrationDate);
			AssertEquals("Manifest entry/registration number has been updated", "32741954", manifest.RegistrationNumber);
		}

		public void TestProcesseOCRResponseMessage()
		{
			manifest.MasterBill.ABL_BillNumber = "BILL123456";
			manifest.AMA_JobReference = "MAN2003289";
			manifest.AMA_MasterBill = manifest.AMA_JobReference;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "MAN2003289";
			outgoingMessage.EM_LinkTable = "AsycudaManifestHeader";
			outgoingMessage.EM_LinkUniqueID = manifest.PK;

			var ocrMessage = Factory.New<TSWMessage>();
			ocrMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			ocrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ocrMessage.EM_MessageText = AcceptanceResponse;

			new MessageProcessorFactory(new LoggingInformation()).ProcessMessage(ocrMessage);
			AssertEquals("ManifestHeader should have been linked to the OCR Message.", manifest, ocrMessage.EM_LinkedObject);
			AssertContains("Message should have the correct discription for the ManifestHeader.", "[Outward Report Accepted] Response for Global Manifest: MAN2003289", ocrMessage.EM_MessageInterpretation);
			AssertEquals("Formatted Response Expected.", FormattedResponseExpected, ocrMessage.EM_MessageInterpretation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			manifest = Factory.New<Integration.Customs.ASYCUDA.IAsycudaManifestHeader>();
		}
		Integration.Customs.ASYCUDA.IAsycudaManifestHeader manifest;

		#region Manifest Response Messages

		#region Acceptance

		public const string ManifestAcceptanceResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
	<IssueDateTime formatCode=""204"">20220214191018</IssueDateTime>
    <FunctionalReferenceID>9694</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>64872109</ID>
        <FunctionalReferenceID>MAN0000025</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20220214191018</EffectiveDateTime>
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

		const string AcceptanceResponse =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns='urn:wco:datamodel:WCO:ResponseModel:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
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
  <IssueDateTime formatCode=""204"">XXXXXXXXXX</IssueDateTime>
  <FunctionalReferenceID>XXXXXXXXXX</FunctionalReferenceID>
  <FunctionCode>24</FunctionCode>
  <AdditionalDocument>
	<CategoryCode>CDO</CategoryCode>
	<ImageBinaryObject mimeCode=""application/pdf"" uri=""XXXXXXXXXX"" filename=""XXXXXXXXXX"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>
  <AdditionalInformation>
	<StatementDescription>XXXXXXXXXX</StatementDescription>
	<StatementTypeCode>DIN</StatementTypeCode>
  </AdditionalInformation>
  <OverallDeclaration>
	<Declaration>
	  <ID>XXXXXXXXXX</ID>
	  <AcceptanceDateTime formatCode=""204"">XXXXXXXXXX</AcceptanceDateTime>
	  <FunctionalReferenceID>MAN2003289</FunctionalReferenceID>
	  <VersionID>XXXXXXXXXX</VersionID>
	  <Submitter>
		<Name>XXXXXXXXXX</Name>
		<ID>XXXXXXXXXX</ID>
	  </Submitter>
	  <ResponsibleGovernmentAgency>
		<ID>NZCS</ID>
	  </ResponsibleGovernmentAgency>
	</Declaration>
  </OverallDeclaration>
  <Status>
	<EffectiveDateTime formatCode=""204"">XXXXXXXXXX</EffectiveDateTime>
	<NameCode>847</NameCode>
	<ReleaseDateTime formatCode=""204"">XXXXXXXXXX</ReleaseDateTime>
	<Pointer>
	  <SequenceNumeric>1</SequenceNumeric>
	  <DocumentSectionCode>XXXXXXXXXX</DocumentSectionCode>
	  <TagID>XXXXXXXXXX</TagID>
	</Pointer>
  </Status>
</Response>
</DocumentMetadata>";

		const string FormattedResponseExpected = @"[Outward Report Accepted] Response for Global Manifest: MAN2003289

Manifest Number   : MAN2003289
Outward Report No : XXXXXXXXXX
Master Bill       : MAN2003289

Rsp Message No    : XXXXXXXXXX
Message Type      : Clearance / Acceptance Instructions
TSW Status Code   : 847 - OCR Received OK
Status            : Outward Report Accepted
";

		#endregion

		#region Cancellation

		public const string ManifestCanceledResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20220404141110</IssueDateTime>
    <FunctionalReferenceID>9739</FunctionalReferenceID>
    <FunctionCode>34</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>8147448</ID>
        <FunctionalReferenceID>MAN0000039</FunctionalReferenceID>
        <VersionID>2</VersionID>
        <CancellationDateTime formatCode=""204"">20220404141110</CancellationDateTime>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20220404141110</EffectiveDateTime>
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

		const string FormattedCancelResponseExpected = @"[Outward Report Cancelled] Response for Global Manifest: MAN0000039

Manifest Number   : MAN0000039
Outward Report No : 8147448
Master Bill       : 086-00023192

Rsp Message No    : 9739
Message Type      : Confirmation of Transaction 
TSW Status Code   : 814
Status            : Outward Report Cancelled
";

		#endregion

		#region Error

		public const string ManifestErrorResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
	<IssueDateTime formatCode=""204"">20220214191059</IssueDateTime>
    <FunctionalReferenceID>9695</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>64872109</ID>
        <FunctionalReferenceID>MAN0000025</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <RejectionDateTime formatCode=""204"">20220214191044</RejectionDateTime>
		<Submitter>
		  <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>678</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>02A</DocumentSectionCode>
        <TagID>D005</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20220214191059</EffectiveDateTime>
	  <NameCode>841</NameCode>
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

		public const string ManifestClearedResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20220216115622</IssueDateTime>
    <FunctionalReferenceID>9699</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>32741954</ID>
        <AcceptanceDateTime formatCode=""204"">20220216115622</AcceptanceDateTime>
        <FunctionalReferenceID>MAN0000026</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20220216115622</EffectiveDateTime>
      <NameCode>847</NameCode>
      <ReleaseDateTime formatCode=""204"">20220216115622</ReleaseDateTime>
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
