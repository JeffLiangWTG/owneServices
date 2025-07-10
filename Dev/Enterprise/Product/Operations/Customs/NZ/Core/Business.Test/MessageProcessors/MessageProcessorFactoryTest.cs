using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using NZCMessage = Enterprise.Customs.NZ.Business.Declaration.NZCMessage;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.Testing
{
	using Enterprise.Core;
	using Enterprise.Customs.Business.Testing;
	using Enterprise.Customs.NZ.Business.Declaration.Testing;
	using Enterprise.Customs.NZ.Business.Express.Testing;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Environment;
	using NUnit.Framework;
	using Manifesting = Declaration.ECIWriteOff.Manifesting;

	public class MessageProcessorFactoryTest : DeclarationsAndShipmentsCreatedCancelledTestCase
	{
		public void TestUnsolicitedMessagesForDepot()
		{
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
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.NominatedGroup);

			// Linked Depot Message
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = "B00004099";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			declaration.DeclarationNumber = "48380311";

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_LinkUniqueID = entryHeader.PK;
			outgoingMessage.EM_ApplicationReference = "B00004099";
			outgoingMessage.EM_Status = "SNT";
			entryHeader.Messages.Add(outgoingMessage);

			var depotMessage = Factory.New<TSWMessage>();
			depotMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			depotMessage.EM_MessageText = LinkedDepotResponse;
			depotMessage.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			depotMessage.EM_ReceiveTransmit = NZCMessage.Direction.Receive;

			messageProcessorFactory.ProcessMessage(depotMessage);
			AssertEquals("Message.EM_Status - Linked Depot Message should be processed", EDIMessage.Status.Received, depotMessage.EM_Status);

			// Unsolicited depot message
			var unsolicitedDepotMessage = Factory.New<TSWMessage>();
			unsolicitedDepotMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			unsolicitedDepotMessage.EM_MessageText = UnsolicitedDepotResponse;
			unsolicitedDepotMessage.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			unsolicitedDepotMessage.EM_ReceiveTransmit = NZCMessage.Direction.Receive;

			messageProcessorFactory.ProcessMessage(unsolicitedDepotMessage);
			AssertEquals("Message.EM_Status - Depot Message should be treated (processed) as a Notification message", EDIMessage.Status.Received, unsolicitedDepotMessage.EM_Status);
			AssertEquals("Message.Notes", true, unsolicitedDepotMessage.Notes.HasNotes);
			var noteCreated = unsolicitedDepotMessage.Notes.FindByDescription("Message Interpretation");
			AssertNotNull(noteCreated);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull("Should have found the email generated from processing this Depot Response message", email);
			AssertEquals(1, email.CCRecipients.Count);
			AssertEquals("JohnTester@TestingCompany.com", email.CCRecipients[0].Email);
		}

		public void TestDepotMessageGeneratesEmail()
		{
			// All matched messages that are not for the Submitter must generate the unsolicted message email
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
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.NominatedGroup);

			// Linked Depot Message
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = "B00004099";
			declaration.JE_MasterBill = "08100394827";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			declaration.DeclarationNumber = "48380311";

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_LinkUniqueID = entryHeader.PK;
			outgoingMessage.EM_ApplicationReference = "B00004099";
			outgoingMessage.EM_Status = "SNT";
			entryHeader.Messages.Add(outgoingMessage);

			var depotMessage = Factory.New<TSWMessage>();
			depotMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			depotMessage.EM_MessageText = LinkedDepotResponse;
			depotMessage.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			depotMessage.EM_ReceiveTransmit = NZCMessage.Direction.Receive;

			messageProcessorFactory.ProcessMessage(depotMessage);
			AssertEquals("Message.EM_Status - Matched Depot Message should be treated (processed) as a notification message", EDIMessage.Status.Received, depotMessage.EM_Status);
			AssertEquals("Message.Notes", true, depotMessage.Notes.HasNotes);
			var noteCreated = depotMessage.Notes.FindByDescription("Message Interpretation");
			AssertNotNull(noteCreated);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull("Should have found the email generated from processing this Depot Response message", email);
			AssertEquals(1, email.CCRecipients.Count);
			AssertEquals("JohnTester@TestingCompany.com", email.CCRecipients[0].Email);
		}

		public void TestNotifyPartyMessageGeneratesEmail()
		{
			// All matched messages that are not for the Submitter must generate the notification message email
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = "B00004099";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			declaration.DeclarationNumber = "47296134";

			var entryHeader = declaration.CusEntryHeader; //set DeclarationNumber will add a new entry header

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_LinkUniqueID = entryHeader.PK;
			outgoingMessage.EM_ApplicationReference = "B00004099";
			outgoingMessage.EM_Status = "SNT";
			entryHeader.Messages.Add(outgoingMessage);

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
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.NominatedGroup);

			var unsolicitedDeliveryNotificationMessage = Factory.New<TSWMessage>();
			unsolicitedDeliveryNotificationMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			unsolicitedDeliveryNotificationMessage.EM_MessageText = MatchedDeliveryMessage;
			unsolicitedDeliveryNotificationMessage.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			unsolicitedDeliveryNotificationMessage.EM_ReceiveTransmit = NZCMessage.Direction.Receive;

			messageProcessorFactory.ProcessMessage(unsolicitedDeliveryNotificationMessage);
			AssertEquals("Message.EM_Status - A Deliviery Notification Message matched to a CW1 job should be treated (processed) as a Notification message", EDIMessage.Status.Received, unsolicitedDeliveryNotificationMessage.EM_Status);
			AssertEquals("Message.Notes", true, unsolicitedDeliveryNotificationMessage.Notes.HasNotes);
			var noteCreated = unsolicitedDeliveryNotificationMessage.Notes.FindByDescription("Message Interpretation");
			AssertNotNull(noteCreated);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull("Should have found the email generated from processing this Transhipment Destination Response message", email);
			AssertEquals(1, email.CCRecipients.Count);
			AssertEquals("JohnTester@TestingCompany.com", email.CCRecipients[0].Email);
		}

		public void TestNotificationMessagesForTranshipmentDest()
		{
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
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.NominatedGroup);

			// Notification Transhipment message
			var unsolicitedDestMessage = Factory.New<TSWMessage>();
			unsolicitedDestMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			unsolicitedDestMessage.EM_MessageText = UnsolicitedDestMessage;
			unsolicitedDestMessage.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			unsolicitedDestMessage.EM_ReceiveTransmit = NZCMessage.Direction.Receive;

			messageProcessorFactory.ProcessMessage(unsolicitedDestMessage);
			AssertEquals("Message.EM_Status - Unlinked Depot Message should be treated (processed) like an unsolicted message", EDIMessage.Status.Received, unsolicitedDestMessage.EM_Status);
			AssertEquals("Message.Notes", true, unsolicitedDestMessage.Notes.HasNotes);
			var noteCreated = unsolicitedDestMessage.Notes.FindByDescription("Message Interpretation");
			AssertNotNull(noteCreated);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull("Should have found the email generated from processing this Transhipment Destination Response message", email);
			AssertEquals(1, email.CCRecipients.Count);
			AssertEquals("JohnTester@TestingCompany.com", email.CCRecipients[0].Email);
		}

		public void TestNotificationMessagesForDeliveryNotificationParty()
		{
			/*
			 *	If we receive a TT or N2 role code message that is not linked to any CW1 job then we should send the Notification email
			 */
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
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.NominatedGroup);

			// Unsolicited Transhipment message
			var unsolicitedDeliveryNotificationMessage = Factory.New<TSWMessage>();
			unsolicitedDeliveryNotificationMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			unsolicitedDeliveryNotificationMessage.EM_MessageText = UnsolicitedDeliveryMessage;
			unsolicitedDeliveryNotificationMessage.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			unsolicitedDeliveryNotificationMessage.EM_ReceiveTransmit = NZCMessage.Direction.Receive;

			messageProcessorFactory.ProcessMessage(unsolicitedDeliveryNotificationMessage);
			AssertEquals("Message.EM_Status - A Deliviery Notification Message should be treated (processed) like an unsolicted message", EDIMessage.Status.Received, unsolicitedDeliveryNotificationMessage.EM_Status);
			AssertEquals("Message.Notes", true, unsolicitedDeliveryNotificationMessage.Notes.HasNotes);
			var noteCreated = unsolicitedDeliveryNotificationMessage.Notes.FindByDescription("Message Interpretation");
			AssertNotNull(noteCreated);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull("Should have found the email generated from processing this Delivery Notification message", email);
			AssertEquals(1, email.CCRecipients.Count);
			AssertEquals("JohnTester@TestingCompany.com", email.CCRecipients[0].Email);
		}

		public void TestCorrectNumberOfHeaders()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B01001001";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.Factory.Save();

			messageProcessorFactory.ProcessMessage(GetNZCMessage(EDIFACTMessageEntryAccepted));
			var originalHeader = declaration.CusEntryHeader;
			AssertEquals("Precondition: declaration.CustomsEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);
			AssertEquals("Precondition: originalHeader.EntryNumber", "77013452", originalHeader.EntryNumber);
			AssertEquals("originalHeader.CH_IsActive", true, originalHeader.CH_IsActive);

			messageProcessorFactory.ProcessMessage(GetNZCMessage(EDIFACTMessageEntryCancelled));
			AssertEquals("originalHeader.CH_EntryStatus", FormalEntryStatusList.Codes.EntryCancelled, originalHeader.CH_EntryStatus);
			AssertEquals("originalHeader.CH_IsActive", false, originalHeader.CH_IsActive);

			messageProcessorFactory.ProcessMessage(GetNZCMessage(EDIFACTMessageEntryRestored));
			var restoredHeader = declaration.CusEntryHeader;
			AssertEquals("declaration.CustomsEntryHeaders.Count", 2, declaration.CustomsEntryHeaders.Count);
			AssertNotEquals("restoredHeader", originalHeader, restoredHeader);
			AssertEquals("restoredHeader.EntryNumber", "77013452", restoredHeader.EntryNumber);
			AssertEquals("restoredHeader.CH_EntryStatus", FormalEntryStatusList.Codes.EntryRestored, restoredHeader.CH_EntryStatus);
			AssertEquals("restoredHeader.CH_IsActive", true, restoredHeader.CH_IsActive);

			messageProcessorFactory.ProcessMessage(GetNZCMessage(EDIFACTMessageDeliveryOrderReceived));
			AssertEquals("declaration.CustomsEntryHeaders.Count", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("declaration.CusEntryHeader", restoredHeader, declaration.CusEntryHeader);
			AssertEquals("restoredHeader.CH_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, restoredHeader.CH_EntryStatus);
			AssertEquals("restoredHeader.CH_IsActive", true, restoredHeader.CH_IsActive);
		}

		const string EDIFACTMessageEntryAccepted =
@"UNH+82995+CUSRES:D:96B:UN+B01001001'
BGM+962+77013452:02'
FTX+ICN+++ADJUSTMENT RECEIVED, CLEARANCE / APPROVAL BY CUSTOMS OFFICER REQUIRED.'
GIS+805:120:143'
UNT+5+82995'";

		const string EDIFACTMessageEntryCancelled =
@"UNH+83031+CUSRES:D:96B:UN+B01001001'
BGM+962+77013452:03'
GIS+814:120:143'
UNT+4+83031'";

		const string EDIFACTMessageEntryRestored =
@"UNH+83032+CUSRES:D:96B:UN+B01001001'
BGM+962+77013452:04'
GIS+815:120:143'
UNT+4+83032'";

		const string EDIFACTMessageDeliveryOrderReceived =
@"UNH+83033+CUSRES:D:96B:UN+B01001001'
BGM+932+77013452:04'
FTX+DIN+++1 LOOSE PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
TAX+4+TOT'
MOA+161:3924.25'
GIS+D:134:143'
UNT+8+83033'";

		public void TestUnrecognisedCommonAccessReference()
		{
			string unrecognisedEDIFACTResponse =
	@"UNH+3448+CUSRES:D:96B:UN+BOLLOCKS'
BGM+932+75044225'
GIS+842:120:143'
DOC+WOF:148:143+1::HOUSE BILL 1'
DOC+WOF:148:143+2::HOUSE BILL 2'
CNT+10:2'
UNT+7+3448'
";
			EDIMessage message = GetNZCMessage(unrecognisedEDIFACTResponse);
			messageProcessorFactory.ProcessMessage(message);
			AssertEquals(EDIMessage.Status.Discarded, message.EM_Status);
		}

		public void TestExpressTSWResponseProcessed()
		{
			var mawb = Factory.New<CusMAWB>();
			var manifestCreator = new TestCusMAWBCreator(mawb, "081-11111111", "QF117", "AUSYD", "NZAKL", new ZDateTime(2019, 10, 19), new ZDateTime(2019, 10, 19));
			var hawb1 = manifestCreator.AddHAWB(manifestCreator.Supplier1, manifestCreator.Importer1, "BILL1", "BALLS", 15.2m, 4, 45.54m);
			var hawb2 = manifestCreator.AddHAWB(manifestCreator.Supplier1, manifestCreator.Importer2, "BILL2", "BALLS", 14.3m, 3, 36.63m);
			mawb.CM_CustomsStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			hawb1.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.PC;
			hawb2.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.PC;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = mawb;
			outgoingMessage.EM_LinkUniqueID = mawb.PK;
			outgoingMessage.EM_ApplicationReference = "X01010101";
			outgoingMessage.EM_Status = "SNT";
			mawb.Messages.Add(outgoingMessage);

			var message = GetNZCustomsResponseMessage(ExpressTSWResponse);
			messageProcessorFactory.ProcessMessage(message);

			AssertEquals("mawb.CM_CustomsStatus", LowValueManifestStatusList.Codes.ManifestAccepted, mawb.CM_CustomsStatus);
			AssertEquals("hawb1.CS_CustomsStatus - Customs cleared", LowValueConsignmentStatusList.Codes.CC, hawb1.CS_CustomsStatus);
			AssertEquals("hawb2.CS_CustomsStatus - Customs cleared", LowValueConsignmentStatusList.Codes.CC, hawb2.CS_CustomsStatus);
			AssertEquals("message has been processed", EDIMessage.Status.Received, message.EM_Status);
		}

		#region ExpressTSWResponse
		const string ExpressTSWResponse =
@"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20191023195857</IssueDateTime>
    <FunctionalReferenceID>7345</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>73416366</ID>
        <AcceptanceDateTime formatCode=""204"">20191023195857</AcceptanceDateTime>
        <FunctionalReferenceID>X01010101</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08111111111</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>BILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08111111111</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>BILL2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20191023195857</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20191023195857</ReleaseDateTime>
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

		#region ExpressEDIFACTResponse
		const string ExpressEDIFACTResponse =
@"UNH+3448+CUSRES:D:96B:UN+X01010101'
BGM+932+75044225'
GIS+842:120:143'
DOC+WOF:148:143+1::HOUSE BILL 1'
DOC+WOF:148:143+2::HOUSE BILL 2'
CNT+10:2'
UNT+7+3448'
";
		#endregion

		public void TestExpressECIResponseProcessedWithBadExpressReference()
		{
			EDIMessage message = GetNZCMessage(ExpressEDIFACTResponse);
			messageProcessorFactory.ProcessMessage(message);
			AssertEquals("message.EM_Status", EDIMessage.Status.Discarded, message.EM_Status);
		}

		public void TestDuplicateEntryResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = "S02371881";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryCleared;
			declaration.DeclarationNumber = "48380311";

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_LinkUniqueID = entryHeader.PK;
			outgoingMessage.EM_ApplicationReference = "S02371881";
			outgoingMessage.EM_Status = "SNT";
			entryHeader.Messages.Add(outgoingMessage);

			var clearedMessage = Factory.New<TSWMessage>();
			clearedMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			clearedMessage.EM_MessageText = "Cleared Message Response";
			clearedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			clearedMessage.EM_ApplicationReference = "S02371881";
			clearedMessage.EM_Status = "RCV";
			entryHeader.Messages.Add(clearedMessage);

			var duplicateErrorMessage = Factory.New<TSWMessage>();
			duplicateErrorMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			duplicateErrorMessage.EM_MessageText = DuplicateEntryResponse;
			duplicateErrorMessage.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			duplicateErrorMessage.EM_ReceiveTransmit = NZCMessage.Direction.Receive;

			messageProcessorFactory.ProcessMessage(duplicateErrorMessage);
			AssertEquals("Message.EM_Status", EDIMessage.Status.Error, duplicateErrorMessage.EM_Status);
			AssertEquals("Message.Notes", true, duplicateErrorMessage.Notes.HasNotes);
			var noteCreated = duplicateErrorMessage.Notes.FindByDescription("Senders Ref Duplicate");
			AssertEquals("This unsolicited duplicate message error was detected and ignored due to the lack of any pending outgoing messages.", noteCreated[0].ST_NoteDataAsText);
		}

		public void TestEntryNoLongerExistsResponse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = "S00045822";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_LinkUniqueID = entryHeader.PK;
			outgoingMessage.EM_ApplicationReference = "S00045822";
			outgoingMessage.EM_Status = "SNT";
			entryHeader.Messages.Add(outgoingMessage);

			declaration.ResetToOriginal();
			AssertEquals("Pre-condition", 0, declaration.ActiveEntryHeaders.Count);

			entryHeader.Delete();
			var orphanedMessageResponse = Factory.New<TSWMessage>();
			orphanedMessageResponse.EM_MessageType = MessageTypeList.Codes.TWR;
			orphanedMessageResponse.EM_MessageText = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>00251596C</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20190722121205</IssueDateTime>
    <FunctionalReferenceID>82211</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>6026605</ID>
        <AcceptanceDateTime formatCode=""204"">20190722121205</AcceptanceDateTime>
        <FunctionalReferenceID>S00045822</FunctionalReferenceID>
        <VersionID>2</VersionID>
        <Submitter>
          <ID>00251596C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190722121205</EffectiveDateTime>
      <NameCode>804</NameCode>
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
			orphanedMessageResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			orphanedMessageResponse.EM_ApplicationReference = "S00045822";
			orphanedMessageResponse.EM_Status = "RCV";

			messageProcessorFactory.ProcessMessage(orphanedMessageResponse);
			AssertEquals("Message Status is Discarded", EDIMessage.Status.Discarded, orphanedMessageResponse.EM_Status);
			AssertEquals("Message.Notes", true, orphanedMessageResponse.Notes.HasNotes);
			var noteCreated = orphanedMessageResponse.Notes.FindByDescription("Senders Ref sending object");
			AssertEquals("This message was discarded due to the outgoing message business object missing.", noteCreated[0].ST_NoteDataAsText);
			noteCreated = orphanedMessageResponse.Notes.FindByDescription("Senders Ref missing linked object");
			AssertEquals("Response for S00045822 expecting business object for CusEntryHeader PK key '" + orphanedMessageResponse.EM_LinkUniqueID.ToString() + "' cannot be processed as the linked object cannot be found.", noteCreated[0].ST_NoteDataAsText);
		}

		[ExpectNoExceptions]
		public void TestMessageAlreadyInCollection()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_DeclarationReference = "B00071881";
			declaration.JE_HouseBill = "HB00329";
			declaration.JE_RL_NKPortOfLoading = "NZAKL";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			var entryHeader = declaration.CusEntryHeader;
			AssertEquals("Pre-condition: Declaration.CusEntryHeaders.Count", 1, declaration.CustomsEntryHeaders.Count);

			Manifesting.CusEntryHeader manifestEntryHeader = Factory.New<Manifesting.CusEntryHeader>();
			manifestEntryHeader.CH_JE = declaration.PK;
			manifestEntryHeader.CH_BGMReference = "B00071881";

			declaration.CustomsEntryHeaders.Add(manifestEntryHeader);
			declaration.JE_IsCancelled = false;
			Factory.Save();

			AssertEquals("Pre-condition: Declaration.CusEntryHeaders.Count - should now have write-off manifest header as well", 2, declaration.CustomsEntryHeaders.Count);
			AssertEquals("MessageSubType needs to be a write-off", JobMessageSubTypeList.Codes.WriteOff, declaration.JE_MessageSubType);
			AssertEquals("ManifestEntryHeader.Declarations.Count", 1, manifestEntryHeader.Declarations.Count);

			NZCMessage message = Factory.New<NZCMessage>();
			message.EM_MessageText = EDIFACTMessageECIManifestWrittenOffResponse.Replace("\r", "").Replace("\n", "");

			IProcessorDelegator[] delegators = new IProcessorDelegator[] { new DeclarationDelegator(), new ECIManifestDelegator() };
			foreach (IProcessorDelegator delegator in delegators)
			{
				AssertEquals("Pre-condition: Both delegators can process message", true, delegator.CanProcess(message));
			}

			messageProcessorFactory.ProcessMessage(message);
			AssertEquals("Manifest response should have only been attached to ManifestEntryHeader", 1, manifestEntryHeader.Messages.Count);
		}

		const string EDIFACTMessageECIManifestWrittenOffResponse =
@"UNH+3143+CUSRES:D:96B:UN+B00071881'
BGM+932+10926889'
GIS+842:120:143'
DOC+WOF:148:143+1::PINGPONG'
DOC+WOF:148:143+2::RUBBER'
DOC+WOF:148:143+3::TENNIS'
CNT+10:3'
UNT+8+3143'";

		public void TestEntryHeaderSwitchingForDeclarations()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = "B01001001";
			declaration.DontReAssignReferenceNoForUnitTest = true;
			CusEntryHeader rightEntryHeader = declaration.CusEntryHeader;
			rightEntryHeader.EntryNumber = "47975057";
			rightEntryHeader.CH_IsActive = false;
			CusEntryHeader wrongEntryHeader = declaration.CusEntryHeader;
			wrongEntryHeader.EntryNumber = "12344321";
			AssertNotEquals("2 entry headers got from CusEntryNubmer should be different as the first one was set inactive", rightEntryHeader, wrongEntryHeader);

			// Using EDIMessage Type to test Override of ProcessMessage()
			EDIMessage message = GetNZCMessage(FormalEntry.Testing.MessageProcessorTest.EDIFACTMessageDeliveryOrder);
			messageProcessorFactory.ProcessMessage(message);

			AssertEquals("declaration.JE_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("rightEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, rightEntryHeader.CH_EntryStatus);
			AssertEquals("rightEntryHeader.Messages.Count", 1, rightEntryHeader.Messages.Count);
			AssertEquals("rightEntryHeader.Messages[0]", message, rightEntryHeader.Messages[0]);
			AssertEquals("wrongEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, wrongEntryHeader.CH_EntryStatus);
			AssertEquals("wrongEntryHeader.Messages.Count", 0, wrongEntryHeader.Messages.Count);

			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			rightEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			rightEntryHeader.CH_IsActive = true;
			wrongEntryHeader.CH_IsActive = false;
			message = GetNZCMessage(FormalEntry.Testing.MessageProcessorTest.EDIFACTMessageDeliveryOrder);
			messageProcessorFactory.ProcessMessage(message);

			AssertEquals("declaration.JE_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
			AssertEquals("rightEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, rightEntryHeader.CH_EntryStatus);
			AssertEquals("rightEntryHeader.Messages.Count", 2, rightEntryHeader.Messages.Count);
			AssertEquals("rightEntryHeader.Messages[1]", message, rightEntryHeader.Messages[1]);
			AssertEquals("wrongEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, wrongEntryHeader.CH_EntryStatus);
			AssertEquals("wrongEntryHeader.Messages.Count", 0, wrongEntryHeader.Messages.Count);
		}

		public void TestECIWriteoffMessageWithInvalidJobNumberShouldBeIgnored()
		{
			SetupJobDeclarationImportAirB01001001();
			declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.NotSentToCustoms;
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.NotSentToCustoms;

			NZCMessage message = GetNZCMessage(EDIFACTMessageECIWithInvalidJobNumber);
			messageProcessorFactory.ProcessMessage(message);

			AssertEquals("Declaration.JE_EntryStatus", LowValueConsignmentStatusList.Codes.NotSentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.NotSentToCustoms, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader.Messages.Count", 0, declaration.CusEntryHeader.Messages.Count);
		}

		public void TestFormalEntryResponseOnJobThatWasPartOfAnECIManifestWithOldReference()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = NumberFountains.OldECIManifestReferencePrefix + "01001001-2";
			declaration.DontReAssignReferenceNoForUnitTest = true;
			declaration.Factory.Save();

			NZCMessage message = GetNZCMessage(EDIFACTMessageDeliveryOrderOnDeclarationWithEWMReference);
			messageProcessorFactory.ProcessMessage(message);

			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader.Messages.Count", 1, declaration.CusEntryHeader.Messages.Count);
			AssertEquals("Declaration.CusEntryHeader.Messages[0]", message, declaration.CusEntryHeader.Messages[0]);
		}

		public void TestFormalEntryResponseOnJobThatWasPartOfAnECIManifestWithNewReference()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = NumberFountains.ECIManifestReferencePrefix + "01001001-2";
			declaration.DontReAssignReferenceNoForUnitTest = true;
			declaration.Factory.Save();

			NZCMessage message = GetNZCMessage(EDIFACTMessageDeliveryOrderOnDeclarationWithMReference);
			messageProcessorFactory.ProcessMessage(message);

			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader.Messages.Count", 1, declaration.CusEntryHeader.Messages.Count);
			AssertEquals("Declaration.CusEntryHeader.Messages[0]", message, declaration.CusEntryHeader.Messages[0]);
		}

		public void TestECIWriteoffResponseProcessedProperly()
		{
			SetupJobDeclarationImportAirB01001001();
			declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			NZCMessage message = GetNZCMessage(ECIWriteOff.Testing.MessageProcessorTest.EDIFACTMessageSingleWriteOff);
			messageProcessorFactory.ProcessMessage(message);

			AssertEquals("Declaration.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration.JE_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.ManifestAccepted, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader.Messages.Count", 1, declaration.CusEntryHeader.Messages.Count);
			AssertEquals("Declaration.CusEntryHeader.Messages[0]", message, declaration.CusEntryHeader.Messages[0]);
		}

		public void TestProcessingCorruptedMessageReturnsFalse()
		{
			SetupJobDeclarationImportAirB01001001();
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;

			NZCMessage message = GetNZCMessage("I'VE BEEN CORRUPTED.");
			messageProcessorFactory.ProcessMessage(message);

			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.SentToCustoms, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader.Messages.Count", 0, declaration.CusEntryHeader.Messages.Count);
		}

		public void TestProcessingMessageWithInvalidDeclarationReferenceFails()
		{
			SetupJobDeclarationImportAirB01001001();
			declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration.JE_DeclarationReference = "lkjsdfl";

			NZCMessage message = GetNZCMessage(EDIFACTMessageJobNotFound);
			messageProcessorFactory.ProcessMessage(message);

			AssertEquals("Declaration.JE_EntryStatus", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.SentToCustoms, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("Declaration.CusEntryHeader.Messages.Count", 0, declaration.CusEntryHeader.Messages.Count);
		}

		public void TestFormalEntryDeliveryOrderResponseProcessedOK()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_DeclarationReference = "B01001001";
			declaration.DontReAssignReferenceNoForUnitTest = true;

			NZCMessage message = GetNZCMessage(FormalEntry.Testing.MessageProcessorTest.EDIFACTMessageDeliveryOrder);
			messageProcessorFactory.ProcessMessage(message);
			AssertEquals("RCV", message.EM_Status);

			AssertEquals("declaration.JE_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
			AssertEquals("declaration.CusEntryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("declaration.CusEntryHeader.Messages.Count", 1, declaration.CusEntryHeader.Messages.Count);
			AssertEquals("declaration.CusEntryHeader.Messages[0]", message, declaration.CusEntryHeader.Messages[0]);
		}

		public void TestDirectForOutwardReportResponse()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00654307";
			consol.JK_MasterBillNum = "08112345678";

			NZCMessage message = GetNZCMessage(EDIFACTMessageAdjustmentResponse);
			messageProcessorFactory.ProcessMessage(message);
			AssertEquals("RCV", message.EM_Status);

			AssertEquals("consol.Messages.Count", 1, consol.Messages.Count);
			AssertEquals("consol.Messages[0]", message, consol.Messages[0]);
		}

		public void TestDirectForOutwardReportResponseWhenConsolIsNotThere()
		{
			NZCMessage message = GetNZCMessage(EDIFACTMessageAdjustmentResponse);
			messageProcessorFactory.ProcessMessage(message);
			AssertEquals("message.EM_Status", EDIMessage.Status.Discarded, message.EM_Status);
		}

		public void TestDoubledUpDeclarationsOnTheShipmentAreCateredFor()
		{
			ZString commonDeclarationReference = "B01001001";

			JobDeclaration declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration1.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			declaration1.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			declaration1.JE_DeclarationReference = commonDeclarationReference;
			declaration1.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration1.DontReAssignReferenceNoForUnitTest = true;

			GlbCompany otherCompany = Factory.New<GlbCompany>();
			otherCompany.FillWithValidTestData();
			GlbBranch branchInOtherCompany = otherCompany.Branches.AddNew();
			branchInOtherCompany.FillWithValidTestData();
			branchInOtherCompany.GB_RL_NKHomePort = "XXYYY";

			JobDeclaration declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration2.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			declaration2.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			declaration2.JE_DeclarationReference = commonDeclarationReference;
			declaration2.JE_GB = branchInOtherCompany.PK;
			declaration2.DontReAssignReferenceNoForUnitTest = true;

			Factory.Save();

			NZCMessage message = GetNZCMessage(FormalEntry.Testing.MessageProcessorTest.EDIFACTMessageDeliveryOrder);

			AssertEquals(commonDeclarationReference, declaration1.JE_DeclarationReference);
			AssertEquals(GlbBranch.CurrentBranch.PK, declaration1.JE_GB);

			AssertEquals(commonDeclarationReference, declaration2.JE_DeclarationReference);
			AssertEquals(branchInOtherCompany.PK, declaration2.JE_GB);

			AssertEquals("Should not have a Delivery Order status.", FormalEntryStatusList.Codes.SentToCustoms, declaration1.JE_EntryStatus);
			AssertEquals("Should not have a Delivery Order status.", FormalEntryStatusList.Codes.SentToCustoms, declaration1.CusEntryHeader.CH_EntryStatus);
			messageProcessorFactory.ProcessMessage(message);
			AssertEquals("RCV", message.EM_Status);

			AssertEquals("Should now have a Delivery Order status.", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration1.JE_EntryStatus);
			AssertEquals("Should now have a Delivery Order status.", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration1.CusEntryHeader.CH_EntryStatus);
		}

		public void TestUnsolicitedEdifactMessage()
		{
			NZCMessage message = Factory.New<NZCMessage>();
			message.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			message.EM_MessageText = @"UNH+553211+CUSRES:D:96B:UN+22326971872015'BGM+932+61352032:01'FTX+DIN+++39 LOOSE PACKAGE(S) OR ITEM(S)'TDT+20++4+++++:::NZ99'LOC+9+NZAKL'GIS+819:120:143'NAD+AL+40342956C:ZZZ:143+FONTERRA LIMITED'NAD+CB+40342956C:ZZZ:143+FONTERRA LIMITED'DOC+964+1'PAC+39++CT'RFF+HWB:08651411091'UNT+12+553211'";
			LoggingInformation logger = new LoggingInformation();
			MessageProcessorFactory messageProcessorFactory = new MessageProcessorFactory(logger);
			messageProcessorFactory.ProcessMessage(message);
			AssertEquals("message.EM_Status should indicated the unsolicited Delivery Order has been processed.", EDIMessage.Status.Received, message.EM_Status);

			var expectedOutputEmail = @"Unsolicited Delivery Order

An unsolicited Delivery Order message has been received from New Zealand Customs.
Below are the details contained in the message:

Delivery Order
----------------------------------------------------------------------
Client Reference Number: 22326971872015
Entry Number   : 61352032
Message No     : 553211

Message Status : (819) Delivery Order Herewith.
                 Method of Payment as Specified.
Transport Carriage : NZ99
Port of Loading : NZAKL

Client         : FONTERRA LIMITED
Broker         : FONTERRA LIMITED
Bill Reference : 08651411091

Delivery Instructions
----------------------------------------------------------------------
39 LOOSE PACKAGE(S) OR ITEM(S)
";
			AssertEquals("Output generated from unsolicited Delivery Order", expectedOutputEmail, message.EM_MessageInterpretation);
		}

		#region MessageDeliveryOrder
		const string EDIFACTMessageDeliveryOrderOnDeclarationWithEWMReference = @"UNH+293602+CUSRES:D:96B:UN+EWM01001001-2'
BGM+932+47975057:01'
FTX+DIN+++2 FCL(S) SAID TO CONTAIN 258 PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
TAX+4+TOT'
MOA+161:11319.5'
GIS+D:134:143'
UNT+8+293602'
";
		const string EDIFACTMessageDeliveryOrderOnDeclarationWithMReference = @"UNH+293602+CUSRES:D:96B:UN+M01001001-2'
BGM+932+47975057:01'
FTX+DIN+++2 FCL(S) SAID TO CONTAIN 258 PACKAGE(S) OR ITEM(S)'
GIS+819:120:143'
TAX+4+TOT'
MOA+161:11319.5'
GIS+D:134:143'
UNT+8+293602'
";
		#endregion
		#region MessageJobNotFound
		const string EDIFACTMessageJobNotFound =
@"UNH+2449+CUSRES:D:98A:UN+B1001001X'
BGM+932+98279024'
GIS+842:120:143'
DOC+WOF:148:143+1::HOUSETEST123'
CNT+10:1'
UNT+6+2449'
";
		#endregion
		#region MessageAdjustmentResponse
		const string EDIFACTMessageAdjustmentResponse =
@"UNH+TRN888007+CUSRES:D:03A:UN+C00654307'
BGM+965+45678901'
GEI+6+830:120:143'
UNT+4+TRN888007'
";
		#endregion
		#region MessageECIWithInvalidJobNumber
		public const string EDIFACTMessageECIWithInvalidJobNumber =
@"UNH+2449+CUSRES:D:98A:UN+X01001001'
BGM+932+98279024'
GIS+842:120:143'
DOC+WOF:148:143+1::HOUSETEST123'
CNT+10:1'
UNT+6+2449'
";
		#endregion

		#region Implementation
		protected JobDeclaration declaration;
		protected MessageProcessorFactory messageProcessorFactory;
		protected LoggingInformation logger;

		protected override void SetUp()
		{
			base.SetUp();
			logger = new LoggingInformation();
			messageProcessorFactory = new MessageProcessorFactory(logger);
		}

		protected void SetupJobDeclarationImportAirB01001001()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			TestECIWriteOffCreator decCreator = new TestECIWriteOffCreator(declaration);
			decCreator.SetupTestForECIWriteoffWithConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupTestCommercialInvoiceHeaderFOB100NZD();
			decCreator.SetEDITransmitDateToToday();
			decCreator.SetUniqueishJobNumber();
			decCreator.SetImportDateToToday();
			declaration.JE_DeclarationReference = "B01001001";
			declaration.DontReAssignReferenceNoForUnitTest = true;
		}

		protected NZCMessage GetNZCMessage(ZString messageText)
		{
			NZCMessage message = Factory.New<NZCMessage>();
			message.EM_MessageText = messageText.Replace("\r", "").Replace("\n", "");
			message.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			return message;
		}

		protected TSWMessage GetNZCustomsResponseMessage(ZString messageText)
		{
			var message = Factory.New<TSWMessage>();
			message.EM_MessageText = messageText;
			message.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			message.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			return message;
		}

		const string DuplicateEntryResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
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
    <IssueDateTime formatCode=""204"">20141121102930</IssueDateTime>
    <FunctionalReferenceID>1865</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>00000000</ID>
        <FunctionalReferenceID>S02371881</FunctionalReferenceID>
        <RejectionDateTime formatCode=""204"">20141121102930</RejectionDateTime>
        <Submitter>
          <ID>00326958C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>4027</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>57B</DocumentSectionCode>
        <TagID>R123</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>487</ValidationCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
        <TagID>D026</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20141121102930</EffectiveDateTime>
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

		const string LinkedDepotResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>7179L</ID>
      <RoleCode>GC</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20181115163852</IssueDateTime>
    <FunctionalReferenceID>1798</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>47296134</ID>
        <AcceptanceDateTime formatCode=""204"">20181115163852</AcceptanceDateTime>
        <FunctionalReferenceID>B00004099</FunctionalReferenceID>
        <TotalGrossMassMeasure unitCode=""KGM"">100</TotalGrossMassMeasure>
        <VersionID>1</VersionID>
        <JurisdictionDateTime formatCode=""102"">20181116</JurisdictionDateTime>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <Agent>
          <Name>CargoWise 2</Name>
        </Agent>
        <BorderTransportMeans>
          <Name>QF108</Name>
          <TypeCode>4</TypeCode>
        </BorderTransportMeans>
        <Carrier>
          <Name>AIR NEW ZEALAND (NZ) LIMITED</Name>
        </Carrier>
        <GoodsShipment>
          <Consignment>
            <GoodsLocation>
              <Name>ECT - CCA Wgtn</Name>
            </GoodsLocation>
            <TransportContractDocument>
              <ID>2314687</ID>
              <TypeCode>HWB</TypeCode>
              <Pointer>
                <DocumentSectionCode>42A</DocumentSectionCode>
              </Pointer>
              <Pointer>
                <SequenceNumeric>1</SequenceNumeric>
                <DocumentSectionCode>93A</DocumentSectionCode>
              </Pointer>
            </TransportContractDocument>
          </Consignment>
        </GoodsShipment>
        <Importer>
          <Name>Importer for ECT</Name>
        </Importer>
        <Packaging>
          <SequenceNumeric>1</SequenceNumeric>
          <QuantityQuantity>10</QuantityQuantity>
          <TypeCode>PK</TypeCode>
        </Packaging>
        <UnloadingLocation>
          <ID>NZAKL</ID>
        </UnloadingLocation>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20181115163852</EffectiveDateTime>
      <NameCode>822</NameCode>
      <ReleaseDateTime formatCode=""204"">20181115163852</ReleaseDateTime>
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

		const string UnsolicitedDepotResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>7179L</ID>
      <RoleCode>GC</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20181115163852</IssueDateTime>
    <FunctionalReferenceID>1798</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>47296134</ID>
        <AcceptanceDateTime formatCode=""204"">20181115163852</AcceptanceDateTime>
        <FunctionalReferenceID>I02299482Y</FunctionalReferenceID>
        <TotalGrossMassMeasure unitCode=""KGM"">100</TotalGrossMassMeasure>
        <VersionID>1</VersionID>
        <JurisdictionDateTime formatCode=""102"">20181116</JurisdictionDateTime>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <Agent>
          <Name>CargoWise 2</Name>
        </Agent>
        <BorderTransportMeans>
          <Name>QF108</Name>
          <TypeCode>4</TypeCode>
        </BorderTransportMeans>
        <Carrier>
          <Name>AIR NEW ZEALAND (NZ) LIMITED</Name>
        </Carrier>
        <GoodsShipment>
          <Consignment>
            <GoodsLocation>
              <Name>ECT - CCA Wgtn</Name>
            </GoodsLocation>
            <TransportContractDocument>
              <ID>2314687</ID>
              <TypeCode>HWB</TypeCode>
              <Pointer>
                <DocumentSectionCode>42A</DocumentSectionCode>
              </Pointer>
              <Pointer>
                <SequenceNumeric>1</SequenceNumeric>
                <DocumentSectionCode>93A</DocumentSectionCode>
              </Pointer>
            </TransportContractDocument>
          </Consignment>
        </GoodsShipment>
        <Importer>
          <Name>Importer for ECT</Name>
        </Importer>
        <Packaging>
          <SequenceNumeric>1</SequenceNumeric>
          <QuantityQuantity>10</QuantityQuantity>
          <TypeCode>PK</TypeCode>
        </Packaging>
        <UnloadingLocation>
          <ID>NZAKL</ID>
        </UnloadingLocation>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20181115163852</EffectiveDateTime>
      <NameCode>822</NameCode>
      <ReleaseDateTime formatCode=""204"">20181115163852</ReleaseDateTime>
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

		const string UnsolicitedDestMessage = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>7179L</ID>
      <RoleCode>TT</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20181115163852</IssueDateTime>
    <FunctionalReferenceID>1798</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>47296134</ID>
        <AcceptanceDateTime formatCode=""204"">20181115163852</AcceptanceDateTime>
        <FunctionalReferenceID>I02299482Y</FunctionalReferenceID>
        <TotalGrossMassMeasure unitCode=""KGM"">100</TotalGrossMassMeasure>
        <VersionID>1</VersionID>
        <JurisdictionDateTime formatCode=""102"">20181116</JurisdictionDateTime>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <Agent>
          <Name>CargoWise 2</Name>
        </Agent>
        <BorderTransportMeans>
          <Name>QF108</Name>
          <TypeCode>4</TypeCode>
        </BorderTransportMeans>
        <Carrier>
          <Name>AIR NEW ZEALAND (NZ) LIMITED</Name>
        </Carrier>
        <GoodsShipment>
          <Consignment>
            <GoodsLocation>
              <Name>ECT - CCA Wgtn</Name>
            </GoodsLocation>
            <TransportContractDocument>
              <ID>2314687</ID>
              <TypeCode>HWB</TypeCode>
              <Pointer>
                <DocumentSectionCode>42A</DocumentSectionCode>
              </Pointer>
              <Pointer>
                <SequenceNumeric>1</SequenceNumeric>
                <DocumentSectionCode>93A</DocumentSectionCode>
              </Pointer>
            </TransportContractDocument>
          </Consignment>
        </GoodsShipment>
        <Importer>
          <Name>Importer for ECT</Name>
        </Importer>
        <Packaging>
          <SequenceNumeric>1</SequenceNumeric>
          <QuantityQuantity>10</QuantityQuantity>
          <TypeCode>PK</TypeCode>
        </Packaging>
        <UnloadingLocation>
          <ID>NZAKL</ID>
        </UnloadingLocation>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20181115163852</EffectiveDateTime>
      <NameCode>822</NameCode>
      <ReleaseDateTime formatCode=""204"">20181115163852</ReleaseDateTime>
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

		const string UnsolicitedDeliveryMessage = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>7179L</ID>
      <RoleCode>N2</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20181115163852</IssueDateTime>
    <FunctionalReferenceID>1798</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>47296134</ID>
        <AcceptanceDateTime formatCode=""204"">20181115163852</AcceptanceDateTime>
        <FunctionalReferenceID>I02299482Y</FunctionalReferenceID>
        <TotalGrossMassMeasure unitCode=""KGM"">100</TotalGrossMassMeasure>
        <VersionID>1</VersionID>
        <JurisdictionDateTime formatCode=""102"">20181116</JurisdictionDateTime>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <Agent>
          <Name>CargoWise 2</Name>
        </Agent>
        <BorderTransportMeans>
          <Name>QF108</Name>
          <TypeCode>4</TypeCode>
        </BorderTransportMeans>
        <Carrier>
          <Name>AIR NEW ZEALAND (NZ) LIMITED</Name>
        </Carrier>
        <GoodsShipment>
          <Consignment>
            <GoodsLocation>
              <Name>ECT - CCA Wgtn</Name>
            </GoodsLocation>
            <TransportContractDocument>
              <ID>2314687</ID>
              <TypeCode>HWB</TypeCode>
              <Pointer>
                <DocumentSectionCode>42A</DocumentSectionCode>
              </Pointer>
              <Pointer>
                <SequenceNumeric>1</SequenceNumeric>
                <DocumentSectionCode>93A</DocumentSectionCode>
              </Pointer>
            </TransportContractDocument>
          </Consignment>
        </GoodsShipment>
        <Importer>
          <Name>Importer for ECT</Name>
        </Importer>
        <Packaging>
          <SequenceNumeric>1</SequenceNumeric>
          <QuantityQuantity>10</QuantityQuantity>
          <TypeCode>PK</TypeCode>
        </Packaging>
        <UnloadingLocation>
          <ID>NZAKL</ID>
        </UnloadingLocation>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20181115163852</EffectiveDateTime>
      <NameCode>822</NameCode>
      <ReleaseDateTime formatCode=""204"">20181115163852</ReleaseDateTime>
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

		const string MatchedDeliveryMessage = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>7179L</ID>
      <RoleCode>N2</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20181115163852</IssueDateTime>
    <FunctionalReferenceID>1798</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>47296134</ID>
        <AcceptanceDateTime formatCode=""204"">20181115163852</AcceptanceDateTime>
        <FunctionalReferenceID>B00004099</FunctionalReferenceID>
        <TotalGrossMassMeasure unitCode=""KGM"">100</TotalGrossMassMeasure>
        <VersionID>1</VersionID>
        <JurisdictionDateTime formatCode=""102"">20181116</JurisdictionDateTime>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <Agent>
          <Name>CargoWise 2</Name>
        </Agent>
        <BorderTransportMeans>
          <Name>QF108</Name>
          <TypeCode>4</TypeCode>
        </BorderTransportMeans>
        <Carrier>
          <Name>AIR NEW ZEALAND (NZ) LIMITED</Name>
        </Carrier>
        <GoodsShipment>
          <Consignment>
            <GoodsLocation>
              <Name>ECT - CCA Wgtn</Name>
            </GoodsLocation>
            <TransportContractDocument>
              <ID>2314687</ID>
              <TypeCode>HWB</TypeCode>
              <Pointer>
                <DocumentSectionCode>42A</DocumentSectionCode>
              </Pointer>
              <Pointer>
                <SequenceNumeric>1</SequenceNumeric>
                <DocumentSectionCode>93A</DocumentSectionCode>
              </Pointer>
            </TransportContractDocument>
          </Consignment>
        </GoodsShipment>
        <Importer>
          <Name>Importer for ECT</Name>
        </Importer>
        <Packaging>
          <SequenceNumeric>1</SequenceNumeric>
          <QuantityQuantity>10</QuantityQuantity>
          <TypeCode>PK</TypeCode>
        </Packaging>
        <UnloadingLocation>
          <ID>NZAKL</ID>
        </UnloadingLocation>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20181115163852</EffectiveDateTime>
      <NameCode>822</NameCode>
      <ReleaseDateTime formatCode=""204"">20181115163852</ReleaseDateTime>
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
	}
}

namespace Enterprise.Customs.NZ.Business.MessageProcessors.Testing
{
	using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting;
	using Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.Testing;
	using JobDeclaration = JobDeclaration;

	public class ECIManifestingMessageProcessorFactoryTest : TestCaseWithFactory
	{
		public void TestECIManifestResponseGetsProcessed()
		{
			CusEntryHeader entryHeader = Factory.New<CusEntryHeader>();
			TestManifestCreator manifestCreator = new TestManifestCreator(entryHeader, "081-11111111", "QF254", "USDNQ", "NZCHC", new ZDateTime(2005, 11, 27), new ZDateTime(2005, 12, 4));
			JobDeclaration declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "PINGPONG", "BALLS", 15.2m, 4, 45.54m);
			JobDeclaration declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer2, "RUBBER", "BALLS", 14.3m, 3, 36.63m);
			JobDeclaration declaration3 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer1, "TENNIS", "BALLS", 13.4m, 2, 27.72m);
			JobDeclaration declaration4 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer2, "BEACH", "BALLS", 12.5m, 2, 18.81m);
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			declaration1.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration3.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration4.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			Factory.Save();

			NZCMessage message = Factory.New<NZCMessage>();
			message.EM_MessageText = EDIFACTMessageECIManifest3ConsignmentsWrittenOffResponse.Replace("\r", "").Replace("\n", "");
			message.IsTransmitMessage = false;

			LoggingInformation logger = new LoggingInformation();
			MessageProcessorFactory messageProcessorFactory = new MessageProcessorFactory(logger);

			messageProcessorFactory.ProcessMessage(message);
			AssertEquals("RCV", message.EM_Status);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.ManifestAccepted, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration1.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration2.JE_EntryStatus);
			AssertEquals("Declaration3.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration3.JE_EntryStatus);
			AssertEquals("Declaration4.JE_EntryStatus", LowValueConsignmentStatusList.Codes.FormalDeclarationRequired, declaration4.JE_EntryStatus);
			AssertEquals("EntryHeader.Messages.Count", 1, entryHeader.Messages.Count);
		}

		public void TestECIManifestNotInSystemDiscardsMessage()
		{
			NZCMessage message = Factory.New<NZCMessage>();
			message.EM_MessageText = EDIFACTMessageECIManifest3ConsignmentsWrittenOffResponse.Replace("\r", "").Replace("\n", "");
			message.IsTransmitMessage = false;

			LoggingInformation logger = new LoggingInformation();
			MessageProcessorFactory messageProcessorFactory = new MessageProcessorFactory(logger);
			messageProcessorFactory.ProcessMessage(message);
			ZString firstLog = logger.DebugLogStrings[0];
			AssertEquals("message.EM_Status", EDIMessage.Status.Discarded, message.EM_Status);
		}

		#region TestConsignmentsWrittenOffAfterBeingHeld
		public void TestHeldManifestConsignmentNowWrittenOff()
		{
			// Process Manifest first
			var entryHeader = Factory.New<CusEntryHeader>();
			var manifestCreator = new TestManifestCreator(entryHeader, "081-11111111", "QF254", "USDNQ", "NZCHC", new ZDateTime(2005, 11, 27), new ZDateTime(2005, 12, 4));
			var declaration0 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "254763017", "BALLS", 15.2m, 4, 2000m);
			var declaration1 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "254763132", "BALLS", 15.2m, 4, 45.54m);
			var declaration2 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer2, "255125147", "BALLS", 14.3m, 3, 36.63m);
			var declaration3 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier2, manifestCreator.Importer1, "263923820", "BALLS", 13.4m, 2, 27.72m);
			var declaration4 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "254763137", "BALLS", 15.2m, 4, 1500m);
			var declaration5 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "254763139", "BALLS", 15.2m, 4, 2000m);
			var declaration6 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "254763145", "BALLS", 15.2m, 4, 2000m);
			var declaration7 = manifestCreator.AddDeclaration(JobMessageTypeList.Codes.Export, manifestCreator.Supplier1, manifestCreator.Importer1, "254763163", "BALLS", 15.2m, 4, 2000m);
			entryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			declaration0.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration1.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration3.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration4.JE_EntryStatus = LowValueConsignmentStatusList.Codes.FormalDeclarationRequired;
			declaration5.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			declaration6.JE_EntryStatus = LowValueConsignmentStatusList.Codes.FormalDeclarationRequired;
			declaration7.JE_EntryStatus = LowValueConsignmentStatusList.Codes.FormalDeclarationRequired;
			Factory.Save();

			var message = entryHeader.Messages.AddNew();
			message.EM_MessageText = Message4Consignments2WrittenOff2Held.Replace("\r", "").Replace("\n", "");
			message.IsTransmitMessage = false;

			var logger = new LoggingInformation();
			var messageProcessorFactory = new MessageProcessorFactory(logger);

			messageProcessorFactory.ProcessMessage(message);
			AssertEquals("EntryHeader.CH_EntryStatus", LowValueManifestStatusList.Codes.InspectionsAuditRequirements, entryHeader.CH_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentHeld, declaration0.JE_EntryStatus);
			AssertEquals("Declaration1.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration1.JE_EntryStatus);
			AssertEquals("Declaration2.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentHeld, declaration2.JE_EntryStatus);
			AssertEquals("Declaration3.JE_EntryStatus", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration3.JE_EntryStatus);
			AssertEquals("EntryHeader.Messages.Count", 1, entryHeader.Messages.Count);

			declaration0.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration4.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration5.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration6.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration7.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Factory.Save();

			// Now process the single write-off response
			var singleEntryHeader = declaration2.CusEntryHeader;
			var wofMessage = singleEntryHeader.Messages.AddNew();
			wofMessage.IsTransmitMessage = false;
			wofMessage.EM_MessageText = HeldConsignmentNowWrittenOff.Replace("\r", "").Replace("\n", "");
			wofMessage.EM_ApplicationCode = NZCMessage.ApplicationCodes.NewZealandCustoms;
			wofMessage.EM_ReceiveTransmit = NZCMessage.Direction.Receive;
			Factory.Save();

			messageProcessorFactory = new MessageProcessorFactory(logger);
			messageProcessorFactory.ProcessMessage(wofMessage);
			AssertEquals("Declaration2.JE_EntryStatus - this consignment status should now show it has been written off", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration2.JE_EntryStatus);
		}
		#endregion
		#region Message3Consignments2WrittenOff1Held
		const string Message4Consignments2WrittenOff2Held = @"
UNH+3143+CUSRES:D:96B:UN+M01010101'
BGM+932+10926889'
GIS+843:120:143'
DOC+HLD:148:143+1::254763017'
DOC+WOF:148:143+2::254763132'
DOC+HLD:148:143+3::255125147'
ERP+1::430'
ERC+153::143'
DOC+WOF:148:143+4::263923820'
CNT+10:4'
UNT+11+3143'
";
		const string HeldConsignmentNowWrittenOff = @"
UNH+1+CUSRES:D:98A:UN+M01010101'
BGM+932+10926889'
GIS+842:120:143'
DOC+WOF:148:143+3::255125147'
CNT+10:1'
UNT+6+1'
";
		#endregion

		#region MessageECIManifest3ConsignmentsWrittenOffResponse
		const string EDIFACTMessageECIManifest3ConsignmentsWrittenOffResponse =
@"UNH+3143+CUSRES:D:96B:UN+M01010101'
BGM+932+10926889'
GIS+842:120:143'
DOC+WOF:148:143+1::PINGPONG'
DOC+WOF:148:143+2::RUBBER'
DOC+WOF:148:143+3::TENNIS'
CNT+10:3'
UNT+8+3143'
";
		#endregion
	}
}
