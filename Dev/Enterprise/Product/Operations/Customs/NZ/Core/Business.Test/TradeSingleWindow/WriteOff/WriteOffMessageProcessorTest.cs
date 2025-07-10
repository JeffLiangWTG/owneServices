using System;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Customs.NZ.Business.MessageProcessors;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ManifestingCusEntryHeader = Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.CusEntryHeader;
using TestManifestCreator = Enterprise.Customs.NZ.Business.Declaration.ECIWriteOff.Manifesting.Testing.TestManifestCreator;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	sealed class CREMessageProcessorMAWBTest : TestCaseWithFactory
	{
		public void TestAcknowledgementResponse()
		{
			var creMessage = Factory.New<TSWMessage>();
			creMessage.EM_MessageType = "RES";
			creMessage.EM_MessageText = AcknowledgementResponse;
			creMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(creMessage);
			AssertEquals(mawb, creMessage.EM_LinkedObject);
			AssertEquals("EM_MessageSubType - Acknowledgement message", TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement, creMessage.EM_MessageSubType);
			AssertEquals(AcknowledgementResponseFormatted, creMessage.EM_MessageInterpretation);
			AssertEquals("71533489", mawb.ECINumber);
			AssertEquals("ACK", mawb.CM_CustomsStatus);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteOffMessageResponse()
		{
			var creMessage = Factory.New<TSWMessage>();
			creMessage.EM_MessageType = "RES";
			creMessage.EM_MessageText = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\TradeSingleWindow\WriteOff\TestFiles\CREWriteOffResponse.txt"));
			creMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(creMessage);
			AssertEquals(WriteOffResponseFormatted, creMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, creMessage.EM_MessageSubType);
			AssertEquals("86747270", mawb.ECINumber);
			AssertEquals("CLR", mawb.CM_CustomsStatus);
			AssertEquals("WOF", mawb.ChildBills[0].CS_CustomsStatus);

			var messageWrittenOffEvent = mawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageAcceptedCode)).First();
			AssertNotNull("messageWrittenOffEvent", messageWrittenOffEvent);
			AssertEquals("messageWrittenOffEvent = MessageAccepted", "REG", messageWrittenOffEvent.SL_Reference);
		}

		public void TestDeliveryInstructionsIncludeMBMovementStatus()
		{
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "X00002082";

			mawb.CM_MessageReference = "X00002082";
			mawb.CM_MAWB = "08154214742";

			hawb1.CS_HAWB = "HBILL3";
			hawb1.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "HBULL4";
			hawb2.CS_ConsignmentNum = 2;
			hawb1.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var icrBioResponseMessage = Factory.New<TSWMessage>();
			icrBioResponseMessage.EM_MessageType = "TWR";
			icrBioResponseMessage.EM_MessageText = BioMovementMessageResponse;
			icrBioResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponseMessage);
			AssertEquals(BioMovementResponseFormatted, icrBioResponseMessage.EM_MessageInterpretation);
			AssertEquals("CM_CustomsDeliveryInstructions - includes MasterBill movement status", true, mawb.CM_CustomsDeliveryInstructions.Contains("MB 08154214742 - MOVEMENT HELD"));

			var icrNZCSResponseMessage = Factory.New<TSWMessage>();
			icrNZCSResponseMessage.EM_MessageType = "TWR";
			icrNZCSResponseMessage.EM_MessageText = NZCSMovementMessageResponse;
			icrNZCSResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			processor.ProcessMessage(icrNZCSResponseMessage);
			AssertEquals(NZCSMovementResponseFormatted, icrNZCSResponseMessage.EM_MessageInterpretation);
			AssertEquals("CM_CustomsDeliveryInstructions - still includes MasterBill movement status", true, mawb.CM_CustomsDeliveryInstructions.Contains("MB 08154214742 - MOVEMENT HELD"));

			var formattedDeliveryInstructions = @"MB 08154214742 - MOVEMENT HELD
X00002082-1:-<WOF>   X00002082-2:-<ITA>";
			AssertEquals("CM_CustomsDeliveryInstructions", formattedDeliveryInstructions, mawb.CM_CustomsDeliveryInstructions);
		}

		#region Movement Response Messages

		public const string BioMovementMessageResponse =
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
    <IssueDateTime formatCode=""204"">20210702160337</IssueDateTime>
    <FunctionalReferenceID>9447</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MB 08154214742 - MOVEMENT HELD</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>37189370</ID>
        <AcceptanceDateTime formatCode=""204"">20210702160337</AcceptanceDateTime>
        <FunctionalReferenceID>X00002082</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Friday, 2 July 2021 4:03:35 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08154214742</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBILL3</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ITA</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">ITA</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08154214742</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBULL4</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20210702160337</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <ReleaseDateTime formatCode=""204"">20210702160337</ReleaseDateTime>
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
</DocumentMetadata>
";

		public const string BioMovementResponseFormatted =
@"[Inspections/Audit Requirements] Response for AirCargo ICR/CRE: X00002082

Clearance / Acceptance Instructions
---------------------------------------------------------------------
AirCargo ICR/CRE : X00002082
Entry Number     : 37189370
Master Bill      : 081-54214742
Message No       : 9447

Message Status   : (B07) MPI Biosecurity Cargo Report Notification - consignment status as specified

Summary          : 0 out of 2 Jobs have been Written Off.

Customs Instructions
---------------------------------------------------------------------
To be held pending further instructions by MPI,issuedDate:Friday, 2 July 2021 4:03:35 PM
MB 08154214742 - MOVEMENT HELD

Job Responses
---------------------------------------------------------------------
Job Number: X00002082-1   House Bill: HBILL3
--- Clearance Status: HLD-Consignment Held ---

Job Number: X00002082-2   House Bill: HBULL4
--- Clearance Status: ITA-International Transhipment Approved ---
--- Movement Status: ITA-International Transhipment Approved ---
";

		public const string NZCSMovementMessageResponse =
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
    <IssueDateTime formatCode=""204"">20210702160341</IssueDateTime>
    <FunctionalReferenceID>9448</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MB 08154214742 - MOVEMENT HELD</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>37189370</ID>
        <AcceptanceDateTime formatCode=""204"">20210702160341</AcceptanceDateTime>
        <FunctionalReferenceID>X00002082</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08154214742</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBILL3</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ITA</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">ITA</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08154214742</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBULL4</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20210702160341</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20210702160341</ReleaseDateTime>
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
</DocumentMetadata>
";

		public const string NZCSMovementResponseFormatted =
@"[ICR/CRE Accepted, Check Consignments for Status] Response for AirCargo ICR/CRE: X00002082

Clearance / Acceptance Instructions
---------------------------------------------------------------------
AirCargo ICR/CRE : X00002082
Entry Number     : 37189370
Master Bill      : 081-54214742
Message No       : 9448

Message Status   : (C06) Customs Cargo Report Notification - consignment status as specified

Summary          : 1 out of 2 Jobs have been Written Off.

Customs Instructions
---------------------------------------------------------------------
MB 08154214742 - MOVEMENT HELD

Job Responses
---------------------------------------------------------------------
Job Number: X00002082-1   House Bill: HBILL3
--- Clearance Status: WOF-Consignment Written Off/Cleared ---

Job Number: X00002082-2   House Bill: HBULL4
--- Clearance Status: ITA-International Transhipment Approved ---
--- Movement Status: ITA-International Transhipment Approved ---
";

		#endregion

		public void TestWriteOffResponseForDuplicateHawbs()
		{
			mawb.CM_MessageReference = "X00001317";
			mawb.CM_MAWB = "08106080955";

			hawb1.CS_HAWB = "HBILL1";

			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "HBILL2";
			hawb2.CS_ConsignmentNum = 2;

			var hawb3 = mawb.ChildBills.AddNew();
			hawb3.CS_HAWB = "HBILL2";
			hawb3.CS_ConsignmentNum = 3;

			outgoingMessage.EM_ApplicationReference = "X00001317";

			var creMessage = Factory.New<TSWMessage>();
			creMessage.EM_MessageType = "RES";
			creMessage.EM_MessageText = WOFDuplicateHawbsMessage;
			creMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(creMessage);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, creMessage.EM_MessageSubType);
			AssertEquals("14754751", mawb.ECINumber);
			AssertEquals("CLR", mawb.CM_CustomsStatus);
			AssertEquals("WOF", mawb.ChildBills[0].CS_CustomsStatus);
			AssertEquals("EDR", mawb.ChildBills[1].CS_CustomsStatus);
			AssertEquals("EDR", mawb.ChildBills[2].CS_CustomsStatus);
			AssertEquals(WOFDupHawbResponseFormatted, creMessage.EM_MessageInterpretation);
		}

		public void TestResponseForDupHawbsChangedSequenceToOriginal()
		{
			mawb.CM_MessageReference = "X00001440";
			mawb.CM_MAWB = "08600239245";

			hawb1.CS_HAWB = "HB001";
			hawb1.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "HB942";
			hawb2.CS_ConsignmentNum = 2;
			hawb1.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb3 = mawb.ChildBills.AddNew();
			hawb3.CS_HAWB = "HB001";
			hawb3.CS_ConsignmentNum = 3;
			hawb3.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			outgoingMessage.EM_ApplicationReference = "X00001440";
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;

			var creMessage = Factory.New<TSWMessage>();
			creMessage.EM_MessageType = "RES";
			creMessage.EM_MessageText = X00001440OriginalResponse;
			creMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(creMessage);
			AssertEquals("EM_MessageSubType - original response message from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, creMessage.EM_MessageSubType);
			AssertEquals("7395968", mawb.ECINumber);
			AssertEquals("CLR", mawb.CM_CustomsStatus);
			AssertEquals("WOF", mawb.ChildBills[0].CS_CustomsStatus);
			AssertEquals("WOF", mawb.ChildBills[1].CS_CustomsStatus);
			AssertEquals("WOF", mawb.ChildBills[2].CS_CustomsStatus);
			AssertEquals(X00001440OriginalResponseFormatted, creMessage.EM_MessageInterpretation);

			var hawb4 = mawb.ChildBills.AddNew();
			hawb4.CS_HAWB = "HB001";
			hawb4.CS_ConsignmentNum = 4;
			hawb4.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			hawb1.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			hawb2.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			hawb3.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var outgoingReplacmentMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "X00001440";
			outgoingMessage.EM_LinkedObject = mawb;
			outgoingMessage.EM_MessageText = X00001440Replacement;

			creMessage = Factory.New<TSWMessage>();
			creMessage.EM_MessageType = "RES";
			creMessage.EM_MessageText = X00001440ReplacementResponse;
			creMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(creMessage);
			AssertEquals("EM_MessageSubType - replacement message response message from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, creMessage.EM_MessageSubType);
			AssertEquals("7395968", mawb.ECINumber);
			AssertEquals("CLR", mawb.CM_CustomsStatus);
			AssertEquals("WOF", mawb.ChildBills[0].CS_CustomsStatus);
			AssertEquals("WOF", mawb.ChildBills[1].CS_CustomsStatus);
			AssertEquals("WOF", mawb.ChildBills[2].CS_CustomsStatus);
			AssertEquals("WOF", mawb.ChildBills[3].CS_CustomsStatus);
			AssertEquals(X00001440ReplacementResponseFormatted, creMessage.EM_MessageInterpretation);
		}

		public void TestWriteOffResponseForOutOfSequenceLineNumbers()
		{
			mawb.CM_MessageReference = "X00001317";
			mawb.CM_MAWB = "08106080955";

			hawb1.CS_HAWB = "HBILL1";
			hawb1.CS_ConsignmentNum = 1;

			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "HBILL2";
			hawb2.CS_ConsignmentNum = 2;

			var hawb3 = mawb.ChildBills.AddNew();
			hawb3.CS_HAWB = "HBILL3";
			hawb3.CS_ConsignmentNum = 3;

			var hawb4 = mawb.ChildBills.AddNew();
			hawb4.CS_HAWB = "HBILL4";
			hawb4.CS_ConsignmentNum = 4;

			outgoingMessage.EM_ApplicationReference = "X00001317";

			var creMessage = Factory.New<TSWMessage>();
			creMessage.EM_MessageType = "RES";
			creMessage.EM_MessageText = WOFOutOfSequenceHawbsMessage;
			creMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(creMessage);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, creMessage.EM_MessageSubType);
			AssertEquals("14754751", mawb.ECINumber);
			AssertEquals("CLR", mawb.CM_CustomsStatus);
			AssertEquals("Formatted message should not show 'UNKNOWN CONSIGNMENT NUMBER' & should find all HouseBill numbers", WOFOutOfSequenceHawbsMessageFormatted, creMessage.EM_MessageInterpretation);

			AssertEquals("EDR", mawb.ChildBills[0].CS_CustomsStatus);
			AssertEquals("Export Declaration Required", mawb.ChildBills[0].CustomsStatusDescription);
			AssertEquals("EDR", mawb.ChildBills[1].CS_CustomsStatus);
			AssertEquals("Export Declaration Required", mawb.ChildBills[1].CustomsStatusDescription);
			AssertEquals("WOF", mawb.ChildBills[2].CS_CustomsStatus);
			AssertEquals("Consignment Written Off/Cleared", mawb.ChildBills[2].CustomsStatusDescription);
			AssertEquals("ITD", mawb.ChildBills[3].CS_CustomsStatus);
			AssertEquals("International Transhipment Declined", mawb.ChildBills[3].CustomsStatusDescription);
		}

		public void TestClearedMessageResponseLog()
		{
			var creMessage = Factory.New<TSWMessage>();
			creMessage.EM_MessageType = "RES";
			creMessage.EM_MessageText = ClearedResponse;
			creMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(creMessage);
			AssertEquals("28917338", mawb.ECINumber);
			AssertEquals("CLR", mawb.CM_CustomsStatus);

			var messageWrittenOffEvent = mawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageAcceptedCode)).First();
			AssertNotNull("messageWrittenOffEvent", messageWrittenOffEvent);
			AssertEquals("messageWrittenOffEvent = MessageAccepted", "REG", messageWrittenOffEvent.SL_Reference);
		}

		public void TestFormattedInterpretationIncludesConsignmentMovementStatus()
		{
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "X00002287";

			mawb.CM_MessageReference = "X00002287";
			mawb.CM_MAWB = "08166552253";

			hawb1.CS_HAWB = "HB1";
			hawb1.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "HB2";
			hawb2.CS_ConsignmentNum = 2;
			hawb1.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var icrBioResponseMessage = Factory.New<TSWMessage>();
			icrBioResponseMessage.EM_MessageType = "TWR";
			icrBioResponseMessage.EM_MessageText = X00002287BioMovementMessageResponse;
			icrBioResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponseMessage);
			AssertEquals(X00002287BioMovementResponseFormatted, icrBioResponseMessage.EM_MessageInterpretation);
			AssertEquals("CM_CustomsDeliveryInstructions - includes movement status", "MB 08166552253 - MOVEMENT APPROVED. All movements approved under a DTR for uncleared goods must be moved by way of an enclosed, secure, leak-proof conveyance, being either within an enclosed vehicle, or within an enclosed container.\r\nX00002287-1:-<HLD>   X00002287-2:-<IDR>\r\nseq:1,instructions:To be held pending further instructions by MPI,issuedDate:Wednesday, 16 August 2023 1:42:32 PM", mawb.CM_CustomsDeliveryInstructions);

			var icrNZCSResponseMessage = Factory.New<TSWMessage>();
			icrNZCSResponseMessage.EM_MessageType = "TWR";
			icrNZCSResponseMessage.EM_MessageText = X00002287NZCSMovementMessageResponse;
			icrNZCSResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			processor.ProcessMessage(icrNZCSResponseMessage);
			AssertEquals(X00002287NZCSMovementResponseFormatted, icrNZCSResponseMessage.EM_MessageInterpretation);

			var formattedDeliveryInstructions = @"MB 08166552253 - MOVEMENT APPROVED
X00002287-1:-<WOF>   X00002287-2:-<IDR>";
			AssertEquals("CM_CustomsDeliveryInstructions", formattedDeliveryInstructions, mawb.CM_CustomsDeliveryInstructions);
		}

		#region Movement Response Messages

		public const string X00002287BioMovementMessageResponse =
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
    <IssueDateTime formatCode=""204"">20230816134253</IssueDateTime>
    <FunctionalReferenceID>10998</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MB 08166552253 - MOVEMENT APPROVED. All movements approved under a DTR for uncleared goods must be moved by way of an enclosed, secure, leak-proof conveyance, being either within an enclosed vehicle, or within an enclosed container.</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>66918914</ID>
        <AcceptanceDateTime formatCode=""204"">20230816134253</AcceptanceDateTime>
        <FunctionalReferenceID>X00002287</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">DTA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Wednesday, 16 August 2023 1:42:32 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08166552253</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HB1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">IDR</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">DTA</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08166552253</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HB2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20230816134253</EffectiveDateTime>
      <NameCode>B07</NameCode>
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
</DocumentMetadata>
";

		public const string X00002287BioMovementResponseFormatted =
@"[Inspections/Audit Requirements] Response for AirCargo ICR/CRE: X00002287

Clearance / Acceptance Instructions
---------------------------------------------------------------------
AirCargo ICR/CRE : X00002287
Entry Number     : 66918914
Master Bill      : 081-66552253
Message No       : 10998

Message Status   : (B07) MPI Biosecurity Cargo Report Notification - consignment status as specified

Summary          : 0 out of 2 Jobs have been Written Off.

Customs Instructions
---------------------------------------------------------------------
To be held pending further instructions by MPI,issuedDate:Wednesday, 16 August 2023 1:42:32 PM
MB 08166552253 - MOVEMENT APPROVED. All movements approved under a DTR for uncleared goods must be moved by way of an enclosed, secure, leak-proof conveyance, being either within an enclosed vehicle, or within an enclosed container.

Job Responses
---------------------------------------------------------------------
Job Number: X00002287-1   House Bill: HB1
--- Clearance Status: HLD-Consignment Held ---
--- Movement Status: DTA-Domestic Transhipment Approved ---

Job Number: X00002287-2   House Bill: HB2
--- Clearance Status: IDR-Import Declaration Required ---
--- Movement Status: DTA-Domestic Transhipment Approved ---
";

		public const string X00002287NZCSMovementMessageResponse =
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
    <IssueDateTime formatCode=""204"">20230816134233</IssueDateTime>
    <FunctionalReferenceID>10997</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MB 08166552253 - MOVEMENT APPROVED</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>66918914</ID>
        <AcceptanceDateTime formatCode=""204"">20230816134233</AcceptanceDateTime>
        <FunctionalReferenceID>X00002287</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">DTA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08166552253</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HB1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">IDR</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">DTA</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08166552253</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HB2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20230816134233</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20230816134233</ReleaseDateTime>
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
</DocumentMetadata>
";

		public const string X00002287NZCSMovementResponseFormatted =
@"[ICR/CRE Accepted, Check Consignments for Status] Response for AirCargo ICR/CRE: X00002287

Clearance / Acceptance Instructions
---------------------------------------------------------------------
AirCargo ICR/CRE : X00002287
Entry Number     : 66918914
Master Bill      : 081-66552253
Message No       : 10997

Message Status   : (C06) Customs Cargo Report Notification - consignment status as specified

Summary          : 1 out of 2 Jobs have been Written Off.

Customs Instructions
---------------------------------------------------------------------
MB 08166552253 - MOVEMENT APPROVED

Job Responses
---------------------------------------------------------------------
Job Number: X00002287-1   House Bill: HB1
--- Clearance Status: WOF-Consignment Written Off/Cleared ---

Job Number: X00002287-2   House Bill: HB2
--- Clearance Status: IDR-Import Declaration Required ---
--- Movement Status: DTA-Domestic Transhipment Approved ---
";

		#endregion

		#region Implementation

		CusMAWB mawb;
		CusHAWB hawb1;
		TSWMessage outgoingMessage;

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_MessageReference = "X00001002";
			mawb.CM_MAWB = "08112345678";
			hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "869020238";
			hawb1.CS_ConsignmentNum = 1;
			outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.CRE;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "X00001002";
			outgoingMessage.EM_LinkedObject = mawb;
		}

		#region Messages

		#region Acknowledgement

		public const string AcknowledgementResponse =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns='urn:wco:datamodel:WCO:DM:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
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
    <IssueDateTime formatCode=""204"" >20130326170357</IssueDateTime>
    <FunctionalReferenceID>10</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>71533489</ID>
        <FunctionalReferenceID>X00001002</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"" >20130326170357</EffectiveDateTime>
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

		public const string AcknowledgementResponseFormatted = "[Acknowledgement] Response for AirCargo ICR/CRE: X00001002";

		#endregion

		#region WriteOff

		public const string WriteOffResponseFormatted =
@"[ICR/CRE Accepted, Check Consignments for Status] Response for AirCargo ICR/CRE: X00001002

Clearance / Acceptance Instructions
---------------------------------------------------------------------
AirCargo ICR/CRE : X00001002
Entry Number     : 86747270
Master Bill      : 081-12345678
Message No       : 4211

Message Status   : (C06) Customs Cargo Report Notification - consignment status as specified

Job Responses
---------------------------------------------------------------------
Job Number: X00001002-1   House Bill: 869020238
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";

		#endregion // WriteOff

		#region WOF Duplicate Hawbs

		public const string WOFDuplicateHawbsMessage =
@"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20181009134223</IssueDateTime>
    <FunctionalReferenceID>4182</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>14754751</ID>
        <AcceptanceDateTime formatCode=""204"">20181009134223</AcceptanceDateTime>
        <FunctionalReferenceID>X00001317</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode>WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>HBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode>EDR</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>HBILL2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode>EDR</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>HBILL2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20181009134223</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20181009134223</ReleaseDateTime>
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

		public const string WOFDupHawbResponseFormatted =
@"[ICR/CRE Accepted, Check Consignments for Status] Response for AirCargo ICR/CRE: X00001317

Clearance / Acceptance Instructions
---------------------------------------------------------------------
AirCargo ICR/CRE : X00001317
Entry Number     : 14754751
Master Bill      : 081-06080955
Message No       : 4182

Message Status   : (C06) Customs Cargo Report Notification - consignment status as specified

Summary          : 1 out of 3 Jobs have been Written Off.

Job Responses
---------------------------------------------------------------------
Job Number: X00001317-1   House Bill: HBILL1
--- Clearance Status: WOF-Consignment Written Off/Cleared ---

Job Number: X00001317-2   House Bill: HBILL2
--- Clearance Status: EDR-Export Declaration Required ---

Job Number: X00001317-3   House Bill: HBILL2
--- Clearance Status: EDR-Export Declaration Required ---
";

		#endregion // WOF Duplicate Hawbs

		public const string WOFOutOfSequenceHawbsMessage =
@"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20181009134223</IssueDateTime>
    <FunctionalReferenceID>4182</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>14754751</ID>
        <AcceptanceDateTime formatCode=""204"">20181009134223</AcceptanceDateTime>
        <FunctionalReferenceID>X00001317</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode>WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>HBILL3</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode>EDR</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>HBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode>ITD</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>HBILL4</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode>EDR</GoodsStatusCode>
          <SequenceNumeric>4</SequenceNumeric>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>HBILL2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20181009134223</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20181009134223</ReleaseDateTime>
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

		public const string WOFOutOfSequenceHawbsMessageFormatted =
@"[ICR/CRE Accepted, Check Consignments for Status] Response for AirCargo ICR/CRE: X00001317

Clearance / Acceptance Instructions
---------------------------------------------------------------------
AirCargo ICR/CRE : X00001317
Entry Number     : 14754751
Master Bill      : 081-06080955
Message No       : 4182

Message Status   : (C06) Customs Cargo Report Notification - consignment status as specified

Summary          : 1 out of 4 Jobs have been Written Off.

Job Responses
---------------------------------------------------------------------
Job Number: X00001317-3   House Bill: HBILL3
--- Clearance Status: WOF-Consignment Written Off/Cleared ---

Job Number: X00001317-1   House Bill: HBILL1
--- Clearance Status: EDR-Export Declaration Required ---

Job Number: X00001317-4   House Bill: HBILL4
--- Clearance Status: ITD-International Transhipment Declined ---

Job Number: X00001317-2   House Bill: HBILL2
--- Clearance Status: EDR-Export Declaration Required ---
";

		#region X00001440

		public const string X00001440OriginalResponse =
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
    <IssueDateTime formatCode=""204"">20190121182537</IssueDateTime>
    <FunctionalReferenceID>5074</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>7395968</ID>
        <AcceptanceDateTime formatCode=""204"">20190121182537</AcceptanceDateTime>
        <FunctionalReferenceID>X00001440</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08600239245</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HB001</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08600239245</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HB942</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08600239245</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HB001</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190121182537</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20190121182537</ReleaseDateTime>
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

		public const string X00001440OriginalResponseFormatted =
@"[ICR/CRE Accepted, Check Consignments for Status] Response for AirCargo ICR/CRE: X00001440

Clearance / Acceptance Instructions
---------------------------------------------------------------------
AirCargo ICR/CRE : X00001440
Entry Number     : 7395968
Master Bill      : 086-00239245
Message No       : 5074

Message Status   : (C06) Customs Cargo Report Notification - consignment status as specified

Summary          : 3 out of 3 Jobs have been Written Off.

Job Responses
---------------------------------------------------------------------
Job Number: X00001440-1   House Bill: HB001
--- Clearance Status: WOF-Consignment Written Off/Cleared ---

Job Number: X00001440-2   House Bill: HB942
--- Clearance Status: WOF-Consignment Written Off/Cleared ---

Job Number: X00001440-3   House Bill: HB001
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";

		public const string X00001440Replacement = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRI</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>ICR</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <ID>7395968</ID>
  <TypeCode>ICR</TypeCode>
  <FunctionalReferenceID>X00001440</FunctionalReferenceID>
  <FunctionCode>5</FunctionCode>
  <Submitter>
    <ID>51358596K</ID>
  </Submitter>
  <AdditionalInformation>
    <StatementDescription>testing consignment sequence numbers</StatementDescription>
    <StatementTypeCode>AES</StatementTypeCode>
  </AdditionalInformation>
  <BorderTransportMeans>
    <Name>NZ104</Name>
    <TypeCode>4</TypeCode>
    <ArrivalDateTime formatCode=""102"">20190121</ArrivalDateTime>
    <FirstArrivalLocationID>NZAKL</FirstArrivalLocationID>
  </BorderTransportMeans>
  <Carrier>
    <Name>AIR NEW ZEALAND (NZ) LIMITED</Name>
  </Carrier>
  <Consignment>
    <SequenceNumeric>1</SequenceNumeric>
    <ValueAmount currencyID=""NZD"">63.85</ValueAmount>
    <AdditionalInformation>
      <StatementCode>Y</StatementCode>
      <StatementTypeCode>WOF</StatementTypeCode>
    </AdditionalInformation>
    <AssociatedTransportDocument>
      <ID>08600239245</ID>
      <TypeCode>MB</TypeCode>
    </AssociatedTransportDocument>
    <Consignee>
      <Name>AMANDA CHRISP</Name>
      <Address>
        <CityName>TE PUKE</CityName>
        <CountryCode>NZ</CountryCode>
        <CountrySubDivisionName>TE PUKE</CountrySubDivisionName>
        <Line>66 POKARE RD RD 6</Line>
        <PostcodeID>3186</PostcodeID>
      </Address>
    </Consignee>
    <ConsignmentItem>
      <SequenceNumeric>1</SequenceNumeric>
      <Commodity>
        <CargoDescription>BEDDING</CargoDescription>
        <ValueAmount currencyID=""NZD"">63.85</ValueAmount>
        <Temperature />
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">2.3</GrossMassMeasure>
      </GoodsMeasure>
      <Origin>
        <CountryCode>AU</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>1</QuantityQuantity>
        <TypeCode>CT</TypeCode>
      </Packaging>
    </ConsignmentItem>
    <Consignor>
      <Name>CANNINGVALE AUSTRALIA PTY LTD</Name>
      <Address>
        <CityName>DERRIMUT</CityName>
        <CountryCode>AU</CountryCode>
        <Line>AREA A - 61 AUSTRALIS DRIVE</Line>
        <PostcodeID>3030</PostcodeID>
      </Address>
    </Consignor>
    <Freight>
      <PaymentMethodCode />
    </Freight>
    <GoodsConsignedPlace>
      <ID />
    </GoodsConsignedPlace>
    <GoodsLocation>
      <ID>7179L</ID>
    </GoodsLocation>
    <LoadingLocation>
      <ID>AUSYD</ID>
    </LoadingLocation>
    <TransportContractDocument>
      <ID>HB001</ID>
      <TypeCode>HWB</TypeCode>
    </TransportContractDocument>
    <UnloadingLocation>
      <ID>NZAKL</ID>
      <ArrivalDateTime formatCode=""203"">201901210000</ArrivalDateTime>
    </UnloadingLocation>
  </Consignment>
  <Consignment>
    <SequenceNumeric>2</SequenceNumeric>
    <ValueAmount currencyID=""NZD"">85.00</ValueAmount>
    <AdditionalInformation>
      <StatementCode>Y</StatementCode>
      <StatementTypeCode>WOF</StatementTypeCode>
    </AdditionalInformation>
    <AssociatedTransportDocument>
      <ID>08600239245</ID>
      <TypeCode>MB</TypeCode>
    </AssociatedTransportDocument>
    <Consignee>
      <ID>51352368J</ID>
      <Address>
        <CityName>AUCKALND</CityName>
        <CountryCode>NZ</CountryCode>
        <CountrySubDivisionName>AUK</CountrySubDivisionName>
        <Line>2 TEST STREET</Line>
        <PostcodeID>54788</PostcodeID>
      </Address>
      <Communication>
        <ID>bill.smith@ab.com.nz</ID>
        <TypeID>EM</TypeID>
      </Communication>
      <Communication>
        <ID>64966667792</ID>
        <TypeID>TE</TypeID>
      </Communication>
    </Consignee>
    <ConsignmentItem>
      <SequenceNumeric>1</SequenceNumeric>
      <Commodity>
        <CargoDescription>CONSUMER ELECTRONICS</CargoDescription>
        <ValueAmount currencyID=""NZD"">85.00</ValueAmount>
        <Temperature />
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">4</GrossMassMeasure>
      </GoodsMeasure>
      <Origin>
        <CountryCode>AU</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>1</QuantityQuantity>
        <TypeCode>PK</TypeCode>
      </Packaging>
    </ConsignmentItem>
    <Consignor>
      <Name>TEST SUPPLIER</Name>
      <Address>
        <CityName>THE ROCKS</CityName>
        <CountryCode>AU</CountryCode>
        <Line>1B GEORGE STREET</Line>
        <PostcodeID>2000</PostcodeID>
      </Address>
      <Communication>
        <ID>brendon.paine@cargowise.com</ID>
        <TypeID>EM</TypeID>
      </Communication>
      <Communication>
        <ID>61288889999</ID>
        <TypeID>TE</TypeID>
      </Communication>
    </Consignor>
    <Freight>
      <PaymentMethodCode />
    </Freight>
    <GoodsConsignedPlace>
      <ID>AUSYD</ID>
    </GoodsConsignedPlace>
    <GoodsLocation>
      <ID>7179L</ID>
    </GoodsLocation>
    <LoadingLocation>
      <ID>AUSYD</ID>
    </LoadingLocation>
    <TransportContractDocument>
      <ID>HB942</ID>
      <TypeCode>HWB</TypeCode>
    </TransportContractDocument>
    <UnloadingLocation>
      <ID>NZAKL</ID>
      <ArrivalDateTime formatCode=""203"">201901210000</ArrivalDateTime>
    </UnloadingLocation>
  </Consignment>
  <Consignment>
    <SequenceNumeric>3</SequenceNumeric>
    <ValueAmount currencyID=""NZD"">78.90</ValueAmount>
    <AdditionalInformation>
      <StatementCode>Y</StatementCode>
      <StatementTypeCode>WOF</StatementTypeCode>
    </AdditionalInformation>
    <AssociatedTransportDocument>
      <ID>08600239245</ID>
      <TypeCode>MB</TypeCode>
    </AssociatedTransportDocument>
    <Consignee>
      <ID>51352368J</ID>
      <Address>
        <CityName>AUCKALND</CityName>
        <CountryCode>NZ</CountryCode>
        <CountrySubDivisionName>AUK</CountrySubDivisionName>
        <Line>2 TEST STREET</Line>
        <PostcodeID>54788</PostcodeID>
      </Address>
      <Communication>
        <ID>bill.smith@ab.com.nz</ID>
        <TypeID>EM</TypeID>
      </Communication>
      <Communication>
        <ID>64966667792</ID>
        <TypeID>TE</TypeID>
      </Communication>
    </Consignee>
    <ConsignmentItem>
      <SequenceNumeric>1</SequenceNumeric>
      <Commodity>
        <CargoDescription>CLOTHING</CargoDescription>
        <ValueAmount currencyID=""NZD"">78.90</ValueAmount>
        <Temperature />
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">3</GrossMassMeasure>
      </GoodsMeasure>
      <Origin>
        <CountryCode>AU</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>1</QuantityQuantity>
        <TypeCode>BX</TypeCode>
      </Packaging>
    </ConsignmentItem>
    <Consignor>
      <Name>TEST SUPPLIER</Name>
      <Address>
        <CityName>THE ROCKS</CityName>
        <CountryCode>AU</CountryCode>
        <Line>1B GEORGE STREET</Line>
        <PostcodeID>2000</PostcodeID>
      </Address>
      <Communication>
        <ID>brendon.paine@cargowise.com</ID>
        <TypeID>EM</TypeID>
      </Communication>
      <Communication>
        <ID>61288889999</ID>
        <TypeID>TE</TypeID>
      </Communication>
    </Consignor>
    <Freight>
      <PaymentMethodCode />
    </Freight>
    <GoodsConsignedPlace>
      <ID>AUSYD</ID>
    </GoodsConsignedPlace>
    <GoodsLocation>
      <ID>7179L</ID>
    </GoodsLocation>
    <LoadingLocation>
      <ID>AUSYD</ID>
    </LoadingLocation>
    <TransportContractDocument>
      <ID>HB001</ID>
      <TypeCode>HWB</TypeCode>
    </TransportContractDocument>
    <UnloadingLocation>
      <ID>NZAKL</ID>
      <ArrivalDateTime formatCode=""203"">201901210000</ArrivalDateTime>
    </UnloadingLocation>
  </Consignment>
  <Consignment>
    <SequenceNumeric>4</SequenceNumeric>
    <ValueAmount currencyID=""NZD"">45</ValueAmount>
    <AdditionalInformation>
      <StatementCode>Y</StatementCode>
      <StatementTypeCode>WOF</StatementTypeCode>
    </AdditionalInformation>
    <AssociatedTransportDocument>
      <ID>08600239245</ID>
      <TypeCode>MB</TypeCode>
    </AssociatedTransportDocument>
    <Consignee>
      <ID>51352368J</ID>
      <Address>
        <CityName>AUCKALND</CityName>
        <CountryCode>NZ</CountryCode>
        <CountrySubDivisionName>AUK</CountrySubDivisionName>
        <Line>2 TEST STREET</Line>
        <PostcodeID>54788</PostcodeID>
      </Address>
      <Communication>
        <ID>bill.smith@ab.com.nz</ID>
        <TypeID>EM</TypeID>
      </Communication>
      <Communication>
        <ID>64966667792</ID>
        <TypeID>TE</TypeID>
      </Communication>
    </Consignee>
    <ConsignmentItem>
      <SequenceNumeric>1</SequenceNumeric>
      <Commodity>
        <CargoDescription>DVDS</CargoDescription>
        <ValueAmount currencyID=""NZD"">45</ValueAmount>
        <Temperature />
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">4</GrossMassMeasure>
      </GoodsMeasure>
      <Origin>
        <CountryCode>AU</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>1</QuantityQuantity>
        <TypeCode>BX</TypeCode>
      </Packaging>
    </ConsignmentItem>
    <Consignor>
      <Name>TEST SUPPLIER</Name>
      <Address>
        <CityName>THE ROCKS</CityName>
        <CountryCode>AU</CountryCode>
        <Line>1B GEORGE STREET</Line>
        <PostcodeID>2000</PostcodeID>
      </Address>
      <Communication>
        <ID>brendon.paine@cargowise.com</ID>
        <TypeID>EM</TypeID>
      </Communication>
      <Communication>
        <ID>61288889999</ID>
        <TypeID>TE</TypeID>
      </Communication>
    </Consignor>
    <Freight>
      <PaymentMethodCode />
    </Freight>
    <GoodsConsignedPlace>
      <ID>AUSYD</ID>
    </GoodsConsignedPlace>
    <GoodsLocation>
      <ID>7179L</ID>
    </GoodsLocation>
    <LoadingLocation>
      <ID>AUSYD</ID>
    </LoadingLocation>
    <TransportContractDocument>
      <ID>HB001</ID>
      <TypeCode>HWB</TypeCode>
    </TransportContractDocument>
    <UnloadingLocation>
      <ID>NZAKL</ID>
      <ArrivalDateTime formatCode=""203"">201901210000</ArrivalDateTime>
    </UnloadingLocation>
  </Consignment>
  <Declarant>
    <ID>51361819A</ID>
    <Communication>
      <ID>gary.odea@wisetechglobal.com</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>61280012200</ID>
      <TypeID>TE</TypeID>
    </Communication>
  </Declarant>
</Declaration>
</DocumentMetadata>";

		public const string X00001440ReplacementResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20190121183427</IssueDateTime>
    <FunctionalReferenceID>5080</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>7395968</ID>
        <AcceptanceDateTime formatCode=""204"">20190121183427</AcceptanceDateTime>
        <FunctionalReferenceID>X00001440</FunctionalReferenceID>
        <VersionID>3</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08600239245</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HB001</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08600239245</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HB942</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08600239245</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HB001</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>4</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08600239245</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HB001</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190121183427</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20190121183427</ReleaseDateTime>
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

		public const string X00001440ReplacementResponseFormatted = @"[ICR/CRE Accepted, Check Consignments for Status] Response for AirCargo ICR/CRE: X00001440

Clearance / Acceptance Instructions
---------------------------------------------------------------------
AirCargo ICR/CRE : X00001440
Entry Number     : 7395968
Master Bill      : 086-00239245
Message No       : 5080

Message Status   : (C06) Customs Cargo Report Notification - consignment status as specified

Summary          : 4 out of 4 Jobs have been Written Off.

Job Responses
---------------------------------------------------------------------
Job Number: X00001440-1   House Bill: HB001
--- Clearance Status: WOF-Consignment Written Off/Cleared ---

Job Number: X00001440-2   House Bill: HB942
--- Clearance Status: WOF-Consignment Written Off/Cleared ---

Job Number: X00001440-3   House Bill: HB001
--- Clearance Status: WOF-Consignment Written Off/Cleared ---

Job Number: X00001440-4   House Bill: HB001
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";

		#endregion

		#region Cleared

		public const string ClearedResponse =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns='urn:wco:datamodel:WCO:DM:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
	<IssueDateTime formatCode=""204"">20150904190345</IssueDateTime>
    <FunctionalReferenceID>2962</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>28917338</ID>
        <AcceptanceDateTime formatCode=""204"">20150904190345</AcceptanceDateTime>
		<FunctionalReferenceID>X00001002</FunctionalReferenceID>
		<Submitter>
		  <ID>00009908C</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode>WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <TransportContractDocument>
            <ID>HHJ</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20150904190345</EffectiveDateTime>
	  <NameCode>842</NameCode>
	  <ReleaseDateTime formatCode=""204"">20150904190345</ReleaseDateTime>
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

		#endregion // Messages

		#endregion // Implementation
	}

	sealed class CREMessageProcessorDeclarationTest : TestCaseWithFactory
	{
		public void TestACKResponseEmailForWOFGoesToExportNotificationGroup()
		{
			var staffExportUser1 = Factory.NewWithValidTestData<GlbStaff>();
			staffExportUser1.GS_Code = "TST";
			staffExportUser1.GS_EmailAddress = "Tester@test.com.au";
			staffExportUser1.GS_LoginName = "tester";

			var staffMember2 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember2.GS_Code = "SAM";
			staffMember2.GS_EmailAddress = "Sam@test.com.au";
			staffMember2.GS_LoginName = "sam";

			var staffExportUser2 = Factory.NewWithValidTestData<GlbStaff>();
			staffExportUser2.GS_Code = "EX2";
			staffExportUser2.GS_EmailAddress = "ExportManager@test.com.au";
			staffExportUser2.GS_LoginName = "ex2";

			var importAckGroup = Factory.NewWithValidTestData<GlbGroup>();
			importAckGroup.Staff.Add(staffMember2);
			ZString importAckMode = Core.Constants.EmailTo.NominatedGroup;

			var exportAckGroup = Factory.NewWithValidTestData<GlbGroup>();
			exportAckGroup.Staff.Add(staffExportUser1);
			exportAckGroup.Staff.Add(staffMember2);
			exportAckGroup.Staff.Add(staffExportUser2);
			ZString exportAckMode = Core.Constants.EmailTo.NominatedGroup;

			NZCustomsDataRegistry.Instance.ImportEciSendAcknowledgementsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importAckGroup.PK.ToGuid());
			NZCustomsDataRegistry.Instance.ImportEciSendAcknowledgements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importAckMode);

			NZCustomsDataRegistry.Instance.ExportEciSendAcknowledgementsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportAckGroup.PK.ToGuid());
			NZCustomsDataRegistry.Instance.ExportEciSendAcknowledgements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportAckMode);

			var creMessage = Factory.New<TSWMessage>();
			creMessage.EM_MessageType = "RES";
			creMessage.EM_MessageText = WOFMessageResponse;
			creMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(creMessage);
			AssertEquals(WOFMessageResponseFormatted, creMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, creMessage.EM_MessageSubType);
			AssertEquals("CLR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("WOF", declaration.JE_EntryStatus);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(3, email.CCRecipients.Count);
			AssertEquals("Tester@test.com.au", email.CCRecipients[0].Email);
			AssertEquals("Sam@test.com.au", email.CCRecipients[1].Email);
			AssertEquals("ExportManager@test.com.au", email.CCRecipients[2].Email);

			var emailPrefix = @"[ICR/CRE Accepted, Check Consignments for Status] Response for ECI Write-Off: B00001162

";
			AssertEquals("Email details generated", WOFMessageResponseFormatted, emailPrefix + email.Body);
		}

		public void TestErrorResponseEmailGoesToExportNotificationGroup()
		{
			var staffExportUser1 = Factory.NewWithValidTestData<GlbStaff>();
			staffExportUser1.GS_Code = "BRK";
			staffExportUser1.GS_EmailAddress = "Broker@test.com.au";
			staffExportUser1.GS_LoginName = "broker";

			var staffMember2 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember2.GS_Code = "SAM";
			staffMember2.GS_EmailAddress = "Sam@test.com.au";
			staffMember2.GS_LoginName = "sam";

			var staffExportUser2 = Factory.NewWithValidTestData<GlbStaff>();
			staffExportUser2.GS_Code = "EX2";
			staffExportUser2.GS_EmailAddress = "ExportManager@test.com.au";
			staffExportUser2.GS_LoginName = "ex2";

			var importErrorGroup = Factory.NewWithValidTestData<GlbGroup>();
			importErrorGroup.Staff.Add(staffMember2);
			ZString importErrorMode = Core.Constants.EmailTo.NominatedGroup;

			var exportErrorGroup = Factory.NewWithValidTestData<GlbGroup>();
			exportErrorGroup.Staff.Add(staffExportUser1);
			exportErrorGroup.Staff.Add(staffMember2);
			exportErrorGroup.Staff.Add(staffExportUser2);
			ZString exportErrorMode = Core.Constants.EmailTo.NominatedGroup;

			NZCustomsDataRegistry.Instance.ImportEciSendErrorsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importErrorGroup.PK.ToGuid());
			NZCustomsDataRegistry.Instance.ImportEciSendErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importErrorMode);

			NZCustomsDataRegistry.Instance.ExportEciSendErrorsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportErrorGroup.PK.ToGuid());
			NZCustomsDataRegistry.Instance.ExportEciSendErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportErrorMode);

			var creMessage = Factory.New<TSWMessage>();
			creMessage.EM_MessageType = "RES";
			creMessage.EM_MessageText = ErrorMessageResponse;
			creMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(creMessage);
			AssertEquals(ErrorMessageResponseFormatted, creMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - error message comes from TSW", TSWMessage.ResponseMessage.MessageSubTypes.TradeSingleWindow, creMessage.EM_MessageSubType);
			AssertEquals("REJ", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("REJ", declaration.JE_EntryStatus);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(3, email.CCRecipients.Count);
			AssertEquals("Broker@test.com.au", email.CCRecipients[0].Email);
			AssertEquals("Sam@test.com.au", email.CCRecipients[1].Email);
			AssertEquals("ExportManager@test.com.au", email.CCRecipients[2].Email);
		}

		public void TestAcknowledgementResponse()
		{
			var creMessage = Factory.New<TSWMessage>();
			creMessage.EM_MessageType = "RES";
			creMessage.EM_MessageText = AcknowledgementResponse;
			creMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(creMessage);
			AssertEquals(declaration.CusEntryHeader, creMessage.EM_LinkedObject);
			AssertEquals("[Acknowledgement] Response for ECI Write-Off: B00001162", creMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - Acknowledgement message", TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement, creMessage.EM_MessageSubType);
			AssertEquals("71533489", declaration.CusEntryHeader.EntryNumber);
			AssertEquals("ACK", declaration.CusEntryHeader.CH_EntryStatus);
		}

		public void TestErrorMessageResponse()
		{
			var creMessage = Factory.New<TSWMessage>();
			creMessage.EM_MessageType = "RES";
			creMessage.EM_MessageText = ErrorMessageResponse;
			creMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(creMessage);
			AssertEquals(ErrorMessageResponseFormatted, creMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - error message comes from TSW", TSWMessage.ResponseMessage.MessageSubTypes.TradeSingleWindow, creMessage.EM_MessageSubType);
			AssertEquals("REJ", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("REJ", declaration.JE_EntryStatus);
		}

		public void TestWriteOffMessageResponse()
		{
			var creMessage = Factory.New<TSWMessage>();
			creMessage.EM_MessageType = "RES";
			creMessage.EM_MessageText = WOFMessageResponse;
			creMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(creMessage);
			AssertEquals(WOFMessageResponseFormatted, creMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, creMessage.EM_MessageSubType);
			AssertEquals("CLR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("WOF", declaration.JE_EntryStatus);
		}

		public void TestWriteOffMessageOfDirectMasterBill()
		{
			declaration.JE_DeclarationReference = "B00002104";
			declaration.JE_MasterBill = "08100234986";
			declaration.JE_HouseBill = "";
			var outgoingMessage = declaration.CusEntryHeader.Messages[0];
			outgoingMessage.EM_ApplicationReference = "B00002104";

			var creMessage = Factory.New<TSWMessage>();
			creMessage.EM_MessageType = "RES";
			creMessage.EM_MessageText = DirectMasterResponse;
			creMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(creMessage);
			AssertEquals(DirectMasterResponseFormatted, creMessage.EM_MessageInterpretation);
			AssertEquals("CLR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("WOF", declaration.JE_EntryStatus);
		}

		public void TestITRApprovedResponse()
		{
			declaration.JE_DeclarationReference = "B00003997";
			declaration.JE_MasterBill = "08600239234";
			declaration.JE_HouseBill = "G002398";
			var outgoingMessage = declaration.CusEntryHeader.Messages[0];
			outgoingMessage.EM_ApplicationReference = "B00003997";

			var itrACKMessage = Factory.New<TSWMessage>();
			itrACKMessage.EM_MessageType = "RES";
			itrACKMessage.EM_MessageText = itrACKResponse;
			itrACKMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(itrACKMessage);
			AssertEquals("CH_EntryStatus Code = ACK", StatusListBase.Codes.Acknowledgement, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("JE_EntryStatus Code = STC", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);

			var itrBIOMessage = Factory.New<TSWMessage>();
			itrBIOMessage.EM_MessageType = "RES";
			itrBIOMessage.EM_MessageText = itrBIOResponse;
			itrBIOMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(itrBIOMessage);
			AssertEquals(itrBioResponseFormatted, itrBIOMessage.EM_MessageInterpretation);
			AssertEquals("CLR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("JE_EntryStatus Code = ITA", LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus Code = ITA", LowValueConsignmentStatusList.Codes.InternationalTranshipmentApproved, declaration.JE_TSWCombinedStatus);
			AssertEquals("International Transhipment Approved", declaration.JE_TSWCombinedStatusDesc);

			var itrNCSMessage = Factory.New<TSWMessage>();
			itrNCSMessage.EM_MessageType = "RES";
			itrNCSMessage.EM_MessageText = itrNZCResponse;
			itrNCSMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(itrNCSMessage);
			AssertEquals(itrNZCResponseFormatted, itrNCSMessage.EM_MessageInterpretation);
			AssertEquals("CLR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("JE_EntryStatus Code = WOF", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus Code = 06", LowValueConsignmentStatusList.Codes.CI, declaration.JE_TSWCombinedStatus);
			AssertEquals("Consignment Written Off / International Transhipment Approved", declaration.JE_TSWCombinedStatusDesc);
		}

		public void TestITRApprovedResponseDifferentMsgReceiptOrder()
		{
			declaration.JE_DeclarationReference = "B00003997";
			declaration.JE_MasterBill = "08600239234";
			declaration.JE_HouseBill = "G002398";
			var outgoingMessage = declaration.CusEntryHeader.Messages[0];
			outgoingMessage.EM_ApplicationReference = "B00003997";

			var itrACKMessage = Factory.New<TSWMessage>();
			itrACKMessage.EM_MessageType = "RES";
			itrACKMessage.EM_MessageText = itrACKResponse;
			itrACKMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(itrACKMessage);
			AssertEquals("CH_EntryStatus Code = ACK", StatusListBase.Codes.Acknowledgement, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("JE_EntryStatus Code = STC", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);

			var itrNCSMessage = Factory.New<TSWMessage>();
			itrNCSMessage.EM_MessageType = "RES";
			itrNCSMessage.EM_MessageText = itrNZCResponse;
			itrNCSMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(itrNCSMessage);
			AssertEquals(itrNZCResponseFormatted, itrNCSMessage.EM_MessageInterpretation);
			AssertEquals("CLR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("JE_EntryStatus Code = WOF", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus Code = WOF", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration.JE_TSWCombinedStatus);
			AssertEquals("Consignment Written Off/Cleared", declaration.JE_TSWCombinedStatusDesc);

			var itrBIOMessage = Factory.New<TSWMessage>();
			itrBIOMessage.EM_MessageType = "RES";
			itrBIOMessage.EM_MessageText = itrBIOResponse;
			itrBIOMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(itrBIOMessage);
			AssertEquals(itrBioResponseFormatted, itrBIOMessage.EM_MessageInterpretation);
			AssertEquals("CLR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("JE_EntryStatus Code = WOF", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus Code = 06", LowValueConsignmentStatusList.Codes.CI, declaration.JE_TSWCombinedStatus);
			AssertEquals("Consignment Written Off / International Transhipment Approved", declaration.JE_TSWCombinedStatusDesc);
		}

		public void TestITRDeclinedResponse()
		{
			declaration.JE_DeclarationReference = "B00003997";
			declaration.JE_MasterBill = "08600239234";
			declaration.JE_HouseBill = "G002398";
			var outgoingMessage = declaration.CusEntryHeader.Messages[0];
			outgoingMessage.EM_ApplicationReference = "B00003997";

			var itrACKMessage = Factory.New<TSWMessage>();
			itrACKMessage.EM_MessageType = "RES";
			itrACKMessage.EM_MessageText = itrACKResponse;
			itrACKMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(itrACKMessage);
			AssertEquals("CH_EntryStatus Code = ACK", StatusListBase.Codes.Acknowledgement, declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("JE_EntryStatus Code = STC", LowValueConsignmentStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);

			var itrBIOMessage = Factory.New<TSWMessage>();
			itrBIOMessage.EM_MessageType = "RES";
			itrBIOMessage.EM_MessageText = itrDeclinedResponse;
			itrBIOMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(itrBIOMessage);
			AssertEquals(itrDeclinedResponseFormatted, itrBIOMessage.EM_MessageInterpretation);
			AssertEquals("IAR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("JE_EntryStatus Code = HLD", LowValueConsignmentStatusList.Codes.ConsignmentHeld, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus Code = ITD", LowValueConsignmentStatusList.Codes.InternationalTranshipmentDeclined, declaration.JE_TSWCombinedStatus);
			AssertEquals("International Transhipment Declined", declaration.JE_TSWCombinedStatusDesc);

			var itrNCSMessage = Factory.New<TSWMessage>();
			itrNCSMessage.EM_MessageType = "RES";
			itrNCSMessage.EM_MessageText = itrNZCResponse;
			itrNCSMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(itrNCSMessage);
			AssertEquals(itrNZCResponseFormatted, itrNCSMessage.EM_MessageInterpretation);
			AssertEquals("CLR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("JE_EntryStatus Code = HLD / even though Customs has written off consignment", LowValueConsignmentStatusList.Codes.ConsignmentHeld, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus Code = ITD", LowValueConsignmentStatusList.Codes.InternationalTranshipmentDeclined, declaration.JE_TSWCombinedStatus);
			AssertEquals("TSW combined status to remain showing ITR declined even though Customs has written off consignment", "International Transhipment Declined", declaration.JE_TSWCombinedStatusDesc);

			// Now process a subsequent ITR approved response:
			itrBIOMessage = Factory.New<TSWMessage>();
			itrBIOMessage.EM_MessageType = "RES";
			itrBIOMessage.EM_MessageText = itrBIOResponse;
			itrBIOMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(itrBIOMessage);
			AssertEquals(itrBioResponseFormatted, itrBIOMessage.EM_MessageInterpretation);
			AssertEquals("CLR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("JE_EntryStatus Code = WOF", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus Code = 06", LowValueConsignmentStatusList.Codes.CI, declaration.JE_TSWCombinedStatus);
			AssertEquals("Consignment Written Off / International Transhipment Approved", declaration.JE_TSWCombinedStatusDesc);
		}

		#region Implementation

		JobDeclaration declaration;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_DeclarationReference = "B00001162";
			declaration.JE_MasterBill = "08112345678";
			declaration.JE_HouseBill = "HOUSETEST123";
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.CRE;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "B00001162";
			outgoingMessage.EM_LinkedObject = declaration.CusEntryHeader;
		}

		#region Messages

		#region Acknowledgement

		public const string AcknowledgementResponse =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns='urn:wco:datamodel:WCO:DM:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
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
    <IssueDateTime formatCode=""204"" >20130326170357</IssueDateTime>
    <FunctionalReferenceID>10</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>71533489</ID>
        <FunctionalReferenceID>B00001162</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"" >20130326170357</EffectiveDateTime>
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

		#endregion

		#region Error Message

		public const string ErrorMessageResponse =
@"<DocumentMetadata xmlns='urn:wco:datamodel:WCO:DM:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
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
    <IssueDateTime formatCode=""204"" ></IssueDateTime>
    <FunctionalReferenceID/>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>00000000</ID>
        <FunctionalReferenceID>B00001162</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>460</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
        <TagID>D013</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"" ></EffectiveDateTime>
      <NameCode>801</NameCode>
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
</DocumentMetadata>
";
		public const string ErrorMessageResponseFormatted =
@"[ICR/CRE Rejected] Response for ECI Write-Off: B00001162

Error report
---------------------------------------------------------------------
ECI Write-Off  : B00001162
Entry Number   : 
Master Bill    : 081-12345678
Message No     : 

Message Status : (801) Lodgement rejected

Message Errors
---------------------------------------------------------------------
**Error** in {Declaration/TypeCode}:-
  Authentication Result : PIN Failure
";

		#endregion // CRE Error Message

		#region WOF Message

		public const string WOFMessageResponse =
@"<DocumentMetadata xmlns='urn:wco:datamodel:WCO:ResponseModel:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
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
    <IssueDateTime formatCode=""204"" >20130605120516</IssueDateTime>
    <FunctionalReferenceID>117</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>7205323</ID>
        <AcceptanceDateTime formatCode=""204"" >20130605120516</AcceptanceDateTime>
        <FunctionalReferenceID>B00001162</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode>WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <TransportContractDocument>
            <ID>HOUSETEST123</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"" >20130605120516</EffectiveDateTime>
      <NameCode>842</NameCode>
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
</DocumentMetadata>
";
		public const string WOFMessageResponseFormatted =
@"[ICR/CRE Accepted, Check Consignments for Status] Response for ECI Write-Off: B00001162

Clearance / Acceptance Instructions
---------------------------------------------------------------------
ECI Write-Off  : B00001162
Entry Number   : 7205323
Master Bill    : 081-12345678
Message No     : 117

Message Status : (842) ECI Received OK
               : consignments have been written off.

Job Responses
---------------------------------------------------------------------
Job Number: B00001162   House Bill: HOUSETEST123
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";

		#endregion // WOF Message

		#region Direct Master Message

		public const string DirectMasterResponse =
@"<DocumentMetadata xmlns='urn:wco:datamodel:WCO:ResponseModel:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20151102141631</IssueDateTime>
    <FunctionalReferenceID>3459</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>96133063</ID>
        <AcceptanceDateTime formatCode=""204"">20151102141631</AcceptanceDateTime>
        <FunctionalReferenceID>B00002104</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode>WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <TransportContractDocument>
            <ID>08100234986</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20151102141631</EffectiveDateTime>
      <NameCode>842</NameCode>
      <ReleaseDateTime formatCode=""204"">20151102141631</ReleaseDateTime>
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

		public const string DirectMasterResponseFormatted =
@"[ICR/CRE Accepted, Check Consignments for Status] Response for ECI Write-Off: B00002104

Clearance / Acceptance Instructions
---------------------------------------------------------------------
ECI Write-Off  : B00002104
Entry Number   : 96133063
Master Bill    : 081-00234986
Message No     : 3459

Message Status : (842) ECI Received OK
               : consignments have been written off.

Job Responses
---------------------------------------------------------------------
Job Number: B00002104   House Bill: 
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";

		#endregion // WOF Message

		#region CRE ITR Messages
		public const string itrACKResponse =
@"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20180919131702</IssueDateTime>
    <FunctionalReferenceID>4051</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>70339833</ID>
        <FunctionalReferenceID>B00003997</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20180919131702</EffectiveDateTime>
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

		public const string itrBIOResponse =
			@"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20180919131710</IssueDateTime>
    <FunctionalReferenceID>4052</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>70339833</ID>
        <AcceptanceDateTime formatCode=""204"">20180919131710</AcceptanceDateTime>
        <FunctionalReferenceID>B00003997</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode>ITA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>G002398</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180919131710</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <ReleaseDateTime formatCode=""204"">20180919131710</ReleaseDateTime>
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

		public const string itrBioResponseFormatted =
@"[ICR/CRE Accepted, Check Consignments for Status] Response for ECI Write-Off: B00003997

Clearance / Acceptance Instructions
---------------------------------------------------------------------
ECI Write-Off  : B00003997
Entry Number   : 70339833
Master Bill    : 086-00239234
Message No     : 4052

Message Status : (B07) MPI Biosecurity Cargo Report Notification - consignment status as specified

Job Responses
---------------------------------------------------------------------
Job Number: B00003997   House Bill: G002398
--- Clearance Status: ITA-International Transhipment Approved ---
";

		public const string itrNZCResponse =
			@"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20180919131717</IssueDateTime>
    <FunctionalReferenceID>4053</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>70339833</ID>
        <AcceptanceDateTime formatCode=""204"">20180919131717</AcceptanceDateTime>
        <FunctionalReferenceID>B00003997</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode>WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>G002398</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180919131717</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20180919131717</ReleaseDateTime>
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

		public const string itrNZCResponseFormatted =
@"[ICR/CRE Accepted, Check Consignments for Status] Response for ECI Write-Off: B00003997

Clearance / Acceptance Instructions
---------------------------------------------------------------------
ECI Write-Off  : B00003997
Entry Number   : 70339833
Master Bill    : 086-00239234
Message No     : 4053

Message Status : (C06) Customs Cargo Report Notification - consignment status as specified

Job Responses
---------------------------------------------------------------------
Job Number: B00003997   House Bill: G002398
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";

		public const string itrDeclinedResponse =
			@"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20180919131710</IssueDateTime>
    <FunctionalReferenceID>4052</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>70339833</ID>
        <AcceptanceDateTime formatCode=""204"">20180919131710</AcceptanceDateTime>
        <FunctionalReferenceID>B00003997</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode>ITD</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>G002398</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180919131710</EffectiveDateTime>
      <NameCode>B05</NameCode>
      <ReleaseDateTime formatCode=""204"">20180919131710</ReleaseDateTime>
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

		public const string itrDeclinedResponseFormatted =
@"[Inspections/Audit Requirements] Response for ECI Write-Off: B00003997

Inspection / Audit requirements
---------------------------------------------------------------------
ECI Write-Off  : B00003997
Entry Number   : 70339833
Master Bill    : 086-00239234
Message No     : 4052

Message Status : (B05) MPI Biosecurity - Directions Given

Job Responses
---------------------------------------------------------------------
Job Number: B00003997   House Bill: G002398
--- Clearance Status: ITD-International Transhipment Declined ---
";

		#endregion

		#endregion // Messages

		#endregion // Implementation
	}

	sealed class CREMessageProcessorManifestingTest : TestCaseWithFactory
	{
		public void TestAcknowledgementResponse()
		{
			var creMessage = Factory.New<TSWMessage>();
			creMessage.EM_MessageType = "RES";
			creMessage.EM_MessageText = AcknowledgementResponse;
			creMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(creMessage);
			AssertEquals(cusEntryHeader, creMessage.EM_LinkedObject);
			AssertEquals("EM_MessageSubType - Acknowledgement message", TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement, creMessage.EM_MessageSubType);
			AssertEquals(AcknowledgementResponseFormatted, creMessage.EM_MessageInterpretation);
			AssertEquals("71533489", cusEntryHeader.EntryNumber);
			AssertEquals("ACK", cusEntryHeader.CH_EntryStatus);
		}

		public void TestWriteOffMessageResponse()
		{
			var creMessage = Factory.New<TSWMessage>();
			creMessage.EM_MessageType = "RES";
			creMessage.EM_MessageText = WOFMessageResponse;
			creMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(creMessage);
			AssertEquals(WOFMessageResponseFormatted, creMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, creMessage.EM_MessageSubType);
			AssertEquals("7205323", cusEntryHeader.EntryNumber);
			AssertEquals("CLR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("WOF", cusEntryHeader.Declarations[0].JE_EntryStatus);
			AssertEquals("WOF", cusEntryHeader.Declarations[0].JE_ECI_LastResponseStatus);
			AssertEquals("WOF", cusEntryHeader.Declarations[1].JE_EntryStatus);
			AssertEquals("WOF", cusEntryHeader.Declarations[1].JE_ECI_LastResponseStatus);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			cusEntryHeader = Factory.New<ManifestingCusEntryHeader>();
			var manifestCreator = new TestManifestCreator(cusEntryHeader);
			var declaration1 = manifestCreator.AddDeclaration();
			declaration1.JE_MasterBill = "08112345678";
			declaration1.JE_HouseBill = "HB001";
			var declaration2 = manifestCreator.AddDeclaration();
			declaration2.JE_MasterBill = "08112345678";
			declaration2.JE_HouseBill = "HB002";

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.CRE;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "M01010101";
			outgoingMessage.EM_LinkedObject = cusEntryHeader;
		}

		ManifestingCusEntryHeader cusEntryHeader;

		#region Messages

		#region Acknowledgement

		public const string AcknowledgementResponse =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns='urn:wco:datamodel:WCO:DM:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
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
    <IssueDateTime formatCode=""204"" >20130326170357</IssueDateTime>
    <FunctionalReferenceID>10</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>71533489</ID>
        <FunctionalReferenceID>M01010101</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"" >20130326170357</EffectiveDateTime>
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

		public const string AcknowledgementResponseFormatted = "[Acknowledgement] Response for ECI Manifest: M01010101";

		#endregion // Acknowledgement

		#region WOF Message

		public const string WOFMessageResponse =
@"<DocumentMetadata xmlns='urn:wco:datamodel:WCO:ResponseModel:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
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
    <IssueDateTime formatCode=""204"" >20130605120516</IssueDateTime>
    <FunctionalReferenceID>117</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>7205323</ID>
        <AcceptanceDateTime formatCode=""204"" >20130605120516</AcceptanceDateTime>
        <FunctionalReferenceID>M01010101</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode>WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>HB001</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode>WOF</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <GoodsLocation>
            <Name>ECT - CCA Wgtn</Name>
          </GoodsLocation>
          <TransportContractDocument>
            <ID>HB002</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20130605120516</EffectiveDateTime>
      <NameCode>842</NameCode>
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
</DocumentMetadata>
";
		public const string WOFMessageResponseFormatted =
@"[ICR/CRE Accepted, Check Consignments for Status] Response for ECI Manifest: M01010101

Clearance / Acceptance Instructions
---------------------------------------------------------------------
ECI Manifest   : M01010101
Entry Number   : 7205323
Master Bill    : 081-12345678
Message No     : 117

Message Status : (842) ECI Received OK
               : consignments have been written off.

Summary        : 2 out of 2 Jobs have been Written Off.

Job Responses
---------------------------------------------------------------------
Job Number: M01010101-1   House Bill: HB001
--- Clearance Status: WOF-Consignment Written Off/Cleared ---

Job Number: M01010101-2   House Bill: HB002
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";

		#endregion // WOF Message

		#endregion // Messages

		#endregion // Implementation
	}

	sealed class ICRMessageProcessorConsolTest : TestCaseWithFactory
	{
		public void TestErrorMessageResponse()
		{
			var icrMessage = Factory.New<TSWMessage>();
			icrMessage.EM_MessageType = "RES";
			icrMessage.EM_MessageText = ErrorMessageResponse;
			icrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrMessage);

			AssertEquals(ErrorMessageResponseFormatted, icrMessage.EM_MessageInterpretation);
			AssertEquals("ICR/CRE Rejected", new ICRManifestStatus(consol).E2_MessageStatus);
			AssertEquals("EM_MessageSubType - error message comes from TSW", TSWMessage.ResponseMessage.MessageSubTypes.TradeSingleWindow, icrMessage.EM_MessageSubType);
		}

		public void TestWriteOffMessageResponse()
		{
			var icrResponseMessage = Factory.New<TSWMessage>();
			icrResponseMessage.EM_MessageType = "RES";
			icrResponseMessage.EM_MessageText = WOFMessageResponse;
			icrResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrResponseMessage);
			AssertEquals(WOFMessageResponseFormatted, icrResponseMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, icrResponseMessage.EM_MessageSubType);

			var manifestStatus = new ICRManifestStatus(consol);
			AssertEquals("1234567", manifestStatus.E2_CustomsEntryNumber);
			AssertEquals("Inspections/Audit Requirements", manifestStatus.E2_MessageStatus);
			var statusProvider1 = new ForwardingShipmentCustomsStatusProvider(consol.Shipments[0]);
			AssertEquals("Consignment Written Off/Cleared", statusProvider1.CustomsCargoStatus());
			var statusProvider2 = new ForwardingShipmentCustomsStatusProvider(consol.Shipments[1]);
			AssertEquals("Consignment Held", statusProvider2.CustomsCargoStatus());
		}

		public void TestWriteOffBIOMessageResponse()
		{
			var icrBIOResponse = Factory.New<TSWMessage>();
			icrBIOResponse.EM_MessageType = "RES";
			icrBIOResponse.EM_MessageText = BIOMessageResponse;
			icrBIOResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBIOResponse);
			AssertEquals("EM_MessageSubType - response message comes from MPI BIO", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBIOResponse.EM_MessageSubType);
		}

		#region Implementation

		ForwardingConsol consol;

		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001000";
			consol.JK_MasterBillNum = "0811111111";

			var entryNumber = Factory.New<CusEntryNumber>();
			entryNumber.CE_ParentID = consol.PK;
			entryNumber.CE_EntryType = Enterprise.Customs.Common.NZ.CusEntryNumberTypeList.Codes.ICRNumber;

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_HouseBill = "H1";
			shipment1.JS_UniqueConsignRef = "S00001001";
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_JS = shipment1.PK;
			declaration1.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_HouseBill = "H2";
			shipment2.JS_UniqueConsignRef = "S00001002";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_JS = shipment2.PK;
			declaration2.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "C00001000";
			outgoingMessage.EM_LinkedObject = consol;
		}

		#region Messages

		#region Error Message

		public const string ErrorMessageResponse =
@"<DocumentMetadata xmlns='urn:wco:datamodel:WCO:DM:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response>
    <IssueDateTime formatCode=""204"" >20130617160523</IssueDateTime>
    <FunctionalReferenceID>137</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>00000000</ID>
        <FunctionalReferenceID>C00001000</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>1020</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>03A</DocumentSectionCode>
        <TagID>369</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"" >20130617160523</EffectiveDateTime>
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
</DocumentMetadata>
";
		public const string ErrorMessageResponseFormatted =
@"[ICR/CRE Rejected] Response for ICR Consol: C00001000

Error report
---------------------------------------------------------------------
ICR Consol     : C00001000
Entry Number   : 
Master Bill    : 0811111111
Message No     : 137

Message Status : (858) Customs Processing Error

Summary        : 0 out of 2 Jobs have been Written Off.

Message Errors
---------------------------------------------------------------------
**Error** in {Declaration/AdditionalInformation[1]/StatementTypeCode}:-
  Additional Statement Type: Not specified or invalid
";

		#endregion // Error Message

		#region WOF Message

		public const string WOFMessageResponse =
@"<DocumentMetadata xmlns='urn:wco:datamodel:WCO:ResponseModel:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
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
    <IssueDateTime formatCode=""204"" >20130605120516</IssueDateTime>
    <FunctionalReferenceID>117</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>1234567</ID>
        <AcceptanceDateTime formatCode=""204"" >20130605120516</AcceptanceDateTime>
        <FunctionalReferenceID>C00001000</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode>WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <TransportContractDocument>
            <ID>H1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode>HLD</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <TransportContractDocument>
            <ID>H2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"" >20130605120516</EffectiveDateTime>
      <NameCode>842</NameCode>
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
</DocumentMetadata>
";

		public const string WOFMessageResponseFormatted =
@"[Inspections/Audit Requirements] Response for ICR Consol: C00001000

Clearance / Acceptance Instructions
---------------------------------------------------------------------
ICR Consol     : C00001000
Entry Number   : 1234567
Master Bill    : 0811111111
Message No     : 117

Message Status : (842) ECI Received OK
               : consignments have been written off.

Summary        : 1 out of 2 Jobs have been Written Off.

Job Responses
---------------------------------------------------------------------
Job Number: S00001001   House Bill: H1
--- Clearance Status: WOF-Consignment Written Off/Cleared ---

Job Number: S00001002   House Bill: H2
--- Clearance Status: HLD-Consignment Held ---
";

		public const string BIOMessageResponse =
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
    <IssueDateTime formatCode=""204"">20171220133511</IssueDateTime>
    <FunctionalReferenceID>2003</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>43435384</ID>
        <AcceptanceDateTime formatCode=""204"">20171220133511</AcceptanceDateTime>
        <FunctionalReferenceID>B00003412</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription><![CDATA[seq:1,instructions:'No Movement Requested.,Held pending assessment by MPI']]></StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08111223343</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>BKGTESTHB1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20171220133511</EffectiveDateTime>
      <NameCode>B07</NameCode>
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

		#endregion // Messages

		#endregion // Implementation

		#endregion
	}

	sealed class ICRMessageProcessorDeclarationTest : TestCaseWithFactory
	{
		public void TestACKResponseEmailForWOFGoesToImportNotificationGroup()
		{
			var staffMember1 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember1.GS_Code = "TST";
			staffMember1.GS_EmailAddress = "Tester@test.com.au";
			staffMember1.GS_LoginName = "tester";

			var staffMember2 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember2.GS_Code = "SAM";
			staffMember2.GS_EmailAddress = "Sam@test.com.au";
			staffMember2.GS_LoginName = "sam";

			var importAckGroup = Factory.NewWithValidTestData<GlbGroup>();
			importAckGroup.Staff.Add(staffMember1);
			importAckGroup.Staff.Add(staffMember2);
			ZString importAckMode = Core.Constants.EmailTo.NominatedGroup;

			var exportAckGroup = Factory.NewWithValidTestData<GlbGroup>();
			exportAckGroup.Staff.Add(staffMember2);
			ZString exportAckMode = Core.Constants.EmailTo.NominatedGroup;

			NZCustomsDataRegistry.Instance.ImportEciSendAcknowledgementsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importAckGroup.PK.ToGuid());
			NZCustomsDataRegistry.Instance.ImportEciSendAcknowledgements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importAckMode);

			NZCustomsDataRegistry.Instance.ExportEciSendAcknowledgementsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportAckGroup.PK.ToGuid());
			NZCustomsDataRegistry.Instance.ExportEciSendAcknowledgements.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportAckMode);

			// MPI Biosecurity response
			var icrBioMessage = Factory.New<TSWMessage>();
			icrBioMessage.EM_MessageType = "RES";
			icrBioMessage.EM_MessageText = MPIBIOResponse;
			icrBioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioMessage);
			AssertEquals(MPIBIOResponseFormatted, icrBioMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - response message comes from MPI Biosecurity", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBioMessage.EM_MessageSubType);
			AssertEquals("No customs status reported - just a BIO response", "ARP", declaration.JE_EntryStatus);
			AssertEquals("Bio Status", "MDR", declaration.CusEntryHeader.CH_MPIBioStatus);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(2, email.CCRecipients.Count);
			AssertEquals("Tester@test.com.au", email.CCRecipients[0].Email);
			AssertEquals("Sam@test.com.au", email.CCRecipients[1].Email);
			var emailPrefix = @"[Inspections/Audit Requirements] Response for ECI Write-Off: B00003483

";
			AssertEquals("Email details generated", MPIBIOResponseFormatted, emailPrefix + email.Body);

			// NZCS response
			var icrCustomsMessage = Factory.New<TSWMessage>();
			icrCustomsMessage.EM_MessageType = "RES";
			icrCustomsMessage.EM_MessageText = WOFMessageResponse;
			icrCustomsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrCustomsMessage);
			AssertEquals(WOFMessageResponseFormatted, icrCustomsMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, icrCustomsMessage.EM_MessageSubType);
			AssertEquals("CLR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("BIO & Customs responses received in this test", "MDR", declaration.JE_EntryStatus);

			AssertEquals(2, Env.OutgoingMailManager.EmailsCreated.Count);
			email = Env.OutgoingMailManager.EmailsCreated[1];
			AssertEquals(2, email.CCRecipients.Count);
			AssertEquals("Tester@test.com.au", email.CCRecipients[0].Email);
			AssertEquals("Sam@test.com.au", email.CCRecipients[1].Email);

			emailPrefix = @"[ICR/CRE Accepted, Check Consignments for Status] Response for ECI Write-Off: B00003483

";
			AssertEquals("Email details generated", WOFMessageResponseFormatted, emailPrefix + email.Body);
		}

		public void TestErrorResponseEmailGoesToImportNotificationGroup()
		{
			var staffImportUser1 = Factory.NewWithValidTestData<GlbStaff>();
			staffImportUser1.GS_Code = "BRK";
			staffImportUser1.GS_EmailAddress = "ImportBroker@test.com.au";
			staffImportUser1.GS_LoginName = "broker";

			var staffMember2 = Factory.NewWithValidTestData<GlbStaff>();
			staffMember2.GS_Code = "SAM";
			staffMember2.GS_EmailAddress = "Sam@test.com.au";
			staffMember2.GS_LoginName = "sam";

			var staffImportManager = Factory.NewWithValidTestData<GlbStaff>();
			staffImportManager.GS_Code = "MGR";
			staffImportManager.GS_EmailAddress = "ImportManager@test.com.au";
			staffImportManager.GS_LoginName = "manager";

			var importErrorGroup = Factory.NewWithValidTestData<GlbGroup>();
			importErrorGroup.Staff.Add(staffImportUser1);
			importErrorGroup.Staff.Add(staffMember2);
			importErrorGroup.Staff.Add(staffImportManager);
			ZString importErrorMode = Core.Constants.EmailTo.NominatedGroup;

			var exportErrorGroup = Factory.NewWithValidTestData<GlbGroup>();
			exportErrorGroup.Staff.Add(staffMember2);
			ZString exportErrorMode = Core.Constants.EmailTo.NominatedGroup;

			NZCustomsDataRegistry.Instance.ImportEciSendErrorsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importErrorGroup.PK.ToGuid());
			NZCustomsDataRegistry.Instance.ImportEciSendErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, importErrorMode);

			NZCustomsDataRegistry.Instance.ExportEciSendErrorsToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportErrorGroup.PK.ToGuid());
			NZCustomsDataRegistry.Instance.ExportEciSendErrors.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, exportErrorMode);

			var icrMessage = Factory.New<TSWMessage>();
			icrMessage.EM_MessageType = "RES";
			icrMessage.EM_MessageText = ErrorMessageResponse;
			icrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrMessage);
			AssertEquals(declaration.CusEntryHeader, icrMessage.EM_LinkedObject);
			AssertEquals(ErrorMessageResponseFormatted, icrMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - error message comes from TSW", TSWMessage.ResponseMessage.MessageSubTypes.TradeSingleWindow, icrMessage.EM_MessageSubType);
			AssertEquals("REJ", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("REJ", declaration.JE_EntryStatus);

			AssertEquals(1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(3, email.CCRecipients.Count);
			AssertEquals("ImportBroker@test.com.au", email.CCRecipients[0].Email);
			AssertEquals("Sam@test.com.au", email.CCRecipients[1].Email);
			AssertEquals("ImportManager@test.com.au", email.CCRecipients[2].Email);
		}

		public void TestAcknowledgementResponse()
		{
			var icrMessage = Factory.New<TSWMessage>();
			icrMessage.EM_MessageType = "RES";
			icrMessage.EM_MessageText = AcknowledgementResponse;
			icrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrMessage);
			AssertEquals(declaration.CusEntryHeader, icrMessage.EM_LinkedObject);
			AssertEquals("[Acknowledgement] Response for ECI Write-Off: B00003483", icrMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - Acknowledgement message", TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement, icrMessage.EM_MessageSubType);
			AssertEquals("49972962", declaration.CusEntryHeader.EntryNumber);
			AssertEquals("ACK", declaration.CusEntryHeader.CH_EntryStatus);
		}

		public void TestProcessWriteOffResponseAfterChangeToIPI()
		{
			/*
			 *	create original write-off ICR entry.
			 */
			var shippingLine = Factory.NewWithValidTestData<OrgHeader>();
			shippingLine.OH_FullName = "QANTAS AIRFREIGHT";
			declaration.JE_TransportMode = "AIR";
			declaration.JE_TotalWeight = 150m;
			declaration.JE_TotalWeightUnit = "KG";
			declaration.JE_VoyageFlightNo = "QF108";
			declaration.JE_DateOfArrival = new ZDateTime(2019, 8, 19);
			declaration.JE_ExportDate = new ZDateTime(2019, 8, 19);
			declaration.JE_GoodsDescription = "NEWS PAPER";
			declaration.JE_GS_NKCusAgent = "JKS";
			declaration.JE_MasterBill = "OB020389";
			declaration.JE_HouseBill = "WI00190886";
			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			declaration.JE_OH_ShippingLine = shippingLine.PK;
			declaration.JE_RL_NKFinalDestination = "NZAKL";
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKPortOfArrival = "NZAKL";
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			declaration.JE_TotalNoOfPacks = 15;
			declaration.JE_TotalNoOfPacksPackType = "PCS";

			var icrEntryHeader = declaration.CusEntryHeader;
			icrEntryHeader.CH_Status = "STC";
			icrEntryHeader.EntryNumber = "";

			outgoingMessage.EM_MessageText = "Outgoing ICR Message";
			AssertEquals("Entry status", "STC", declaration.JE_EntryStatus);

			// now change the current declaration to an IPI declaration
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.CusEntryHeader.CH_LastEntryStyle = JobMessageSubTypeList.Codes.WriteOff;

			var ipiHeader = declaration.CusEntryHeader; // IPI entry is now active entry.
			ipiHeader.CH_EDITransmitDate = new ZDateTime(2019, 8, 19);
			ipiHeader.CH_Status = "CLR";
			ipiHeader.EntryNumber = "75328491";
			AssertEquals("IPI entry header status", "CLR", declaration.CusEntryHeader.CH_Status);
			AssertEquals("IPI Entry number", "75328491", declaration.CusEntryHeader.EntryNumber);
			Assert(icrEntryHeader.PK != ipiHeader.PK);

			// now process response messages to the original write-off message
			var icrBioResponse = Factory.New<TSWMessage>();
			icrBioResponse.EM_MessageType = "RES";
			icrBioResponse.EM_MessageText = MPIBIOResponse;
			icrBioResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponse);
			AssertEquals("EM_MessageSubType - response message comes from MPIBIO", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBioResponse.EM_MessageSubType);
			AssertEquals("IAR", icrEntryHeader.CH_EntryStatus);
			AssertEquals("MPI Import Dec Required", "MDR", icrEntryHeader.CH_MPIBioStatus);

			var icrCustomsResponse = Factory.New<TSWMessage>();
			icrCustomsResponse.EM_MessageType = "RES";
			icrCustomsResponse.EM_MessageText = WOFMessageResponse;
			icrCustomsResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrCustomsResponse);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, icrCustomsResponse.EM_MessageSubType);
			AssertEquals("CLR", icrEntryHeader.CH_EntryStatus);
			AssertEquals("WOF", icrEntryHeader.CH_NZCSStatus);

			AssertEquals("The correct Write-off entry header status has been updated", "CLR", icrEntryHeader.CH_EntryStatus);
			AssertEquals("The correct Write-off entry header, Entry number has been updated", "49972962", icrEntryHeader.EntryNumber);
			Assert(icrEntryHeader.PK != ipiHeader.PK);

			// change the current declaration back to the Write-Off entry (ICR declaration) - statuses should switch to IPI entry details
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.WriteOff;
			AssertEquals("Statuses from IPI entry should be shown on Declaration now", "NZCS - Consignment Written Off / BIO - MPI Import Dec Required", declaration.JE_TSWCombinedStatusDesc);
			AssertEquals(icrEntryHeader, declaration.EntryHeaderForWriteOffEntryNumber);
			AssertEquals(ipiHeader, declaration.EntryHeaderForIPIEntryNumber);
		}

		public void TestErrorMessageResponse()
		{
			var icrMessage = Factory.New<TSWMessage>();
			icrMessage.EM_MessageType = "RES";
			icrMessage.EM_MessageText = ErrorMessageResponse;
			icrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrMessage);
			AssertEquals(ErrorMessageResponseFormatted, icrMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - error message comes from TSW", TSWMessage.ResponseMessage.MessageSubTypes.TradeSingleWindow, icrMessage.EM_MessageSubType);
			AssertEquals("REJ", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("REJ", declaration.JE_EntryStatus);
		}

		public void TestWriteOffBIOResponse()
		{
			var icrMessage = Factory.New<TSWMessage>();
			icrMessage.EM_MessageType = "RES";
			icrMessage.EM_MessageText = MPIBIOResponse;
			icrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrMessage);
			AssertEquals(MPIBIOResponseFormatted, icrMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - response message comes from MPI Biosecurity", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrMessage.EM_MessageSubType);
			AssertEquals("No customs status reported - just a BIO response", "ARP", declaration.JE_EntryStatus);
			AssertEquals("Bio Status", "MDR", declaration.CusEntryHeader.CH_MPIBioStatus);
		}

		public void TestWriteOffMessageResponse()
		{
			var icrMessage = Factory.New<TSWMessage>();
			icrMessage.EM_MessageType = "RES";
			icrMessage.EM_MessageText = WOFMessageResponse;
			icrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrMessage);
			AssertEquals(WOFMessageResponseFormatted, icrMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, icrMessage.EM_MessageSubType);
			AssertEquals("CLR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("No BIO response in this test", "ARP", declaration.JE_EntryStatus);
		}

		public void TestICRStatusDetermined()
		{
			var icrBioResponse = Factory.New<TSWMessage>();
			icrBioResponse.EM_MessageType = "RES";
			icrBioResponse.EM_MessageText = MPIBIOResponse;
			icrBioResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponse);
			AssertEquals(MPIBIOResponseFormatted, icrBioResponse.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - response message comes from MPIBIO", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBioResponse.EM_MessageSubType);
			AssertEquals("IAR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("MPI Import Dec Required", "MDR", declaration.CusEntryHeader.CH_MPIBioStatus);
			AssertEquals("No customs status reported yet - just the BIO response", "ARP", declaration.JE_EntryStatus);
			AssertEquals("NZCS - Pending / BIO - MPI Import Dec Required", declaration.JE_TSWCombinedStatusDesc);

			var icrCustomsResponse = Factory.New<TSWMessage>();
			icrCustomsResponse.EM_MessageType = "RES";
			icrCustomsResponse.EM_MessageText = WOFMessageResponse;
			icrCustomsResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrCustomsResponse);
			AssertEquals(WOFMessageResponseFormatted, icrCustomsResponse.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, icrCustomsResponse.EM_MessageSubType);
			AssertEquals("CLR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("WOF", declaration.CusEntryHeader.CH_NZCSStatus);
			AssertEquals("BIO response currently trumps WOF status from Customs for overall status", "MDR", declaration.JE_EntryStatus);
			AssertEquals("NZCS - Consignment Written Off / BIO - MPI Import Dec Required", declaration.JE_TSWCombinedStatusDesc);
		}

		public void TestITRCombinedStatusDetermined()
		{
			outgoingMessage.EM_ApplicationReference = "B00004206";
			declaration.JE_DeclarationReference = "B00004206";
			declaration.JE_MasterBill = "08100234920";
			declaration.JE_HouseBill = "G45023";
			TranshipmentRequest.Create(declaration);
			declaration.TranshipmentRequest.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			var icrBioResponse = Factory.New<TSWMessage>();
			icrBioResponse.EM_MessageType = "RES";
			icrBioResponse.EM_MessageText = MPIBIO_ITRResponse;
			icrBioResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var expectedDate = new ZDateTime(2019, 02, 26, 17, 23, 13);
			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponse);
			AssertEquals("EM_MessageSubType - response message comes from MPIBIO", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBioResponse.EM_MessageSubType);
			AssertEquals("CH_MPIBioMovementStatus", "ITA", declaration.CusEntryHeader.CH_MPIBioMovementStatus);
			AssertEquals("CH_MPIBioMovementStatusTime", expectedDate, declaration.CusEntryHeader.CH_MPIBioMovementStatusTime);

			AssertEquals("ITR Combined Status - NZCS Request Pending / MPI Bio ITR Approved", "92", declaration.TranshipmentRequest.C4_Status);

			var icrCustomsResponse = Factory.New<TSWMessage>();
			icrCustomsResponse.EM_MessageType = "RES";
			icrCustomsResponse.EM_MessageText = NZCS_ITRResponse;
			icrCustomsResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			expectedDate = new ZDateTime(2019, 02, 26, 17, 23, 11);

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrCustomsResponse);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, icrCustomsResponse.EM_MessageSubType);
			AssertEquals("CH_NZCSMovementStatus", "ITA", declaration.CusEntryHeader.CH_NZCSMovementStatus);
			AssertEquals("CH_NZCSMovementStatusTime", expectedDate, declaration.CusEntryHeader.CH_NZCSMovementStatusTime);

			AssertEquals("ITR Combined Status - ITR Approved", "22", declaration.TranshipmentRequest.C4_Status);
		}

		public void TestMixedITRCombinedStatusDetermined()
		{
			outgoingMessage.EM_ApplicationReference = "B00004206";
			declaration.JE_DeclarationReference = "B00004206";
			declaration.JE_MasterBill = "08100234920";
			declaration.JE_HouseBill = "G45023";
			TranshipmentRequest.Create(declaration);
			declaration.TranshipmentRequest.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			var icrBioResponse = Factory.New<TSWMessage>();
			icrBioResponse.EM_MessageType = "RES";
			icrBioResponse.EM_MessageText = MPIBIO_ITRHeldResponse;
			icrBioResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var expectedDate = new ZDateTime(2019, 02, 26, 17, 23, 13);
			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponse);
			AssertEquals("EM_MessageSubType - response message comes from MPIBIO", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBioResponse.EM_MessageSubType);
			AssertEquals("CH_MPIBioMovementStatus", "HLD", declaration.CusEntryHeader.CH_MPIBioMovementStatus);
			AssertEquals("CH_MPIBioMovementStatusTime", expectedDate, declaration.CusEntryHeader.CH_MPIBioMovementStatusTime);

			AssertEquals("ITR Combined Status - NZCS Request Pending / MPI Bio ITR Held", "91", declaration.TranshipmentRequest.C4_Status);

			var icrCustomsResponse = Factory.New<TSWMessage>();
			icrCustomsResponse.EM_MessageType = "RES";
			icrCustomsResponse.EM_MessageText = NZCS_ITRResponse;
			icrCustomsResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			expectedDate = new ZDateTime(2019, 02, 26, 17, 23, 11);

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrCustomsResponse);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, icrCustomsResponse.EM_MessageSubType);
			AssertEquals("CH_NZCSMovementStatus", "ITA", declaration.CusEntryHeader.CH_NZCSMovementStatus);
			AssertEquals("CH_NZCSMovementStatusTime", expectedDate, declaration.CusEntryHeader.CH_NZCSMovementStatusTime);

			AssertEquals("ITR Combined Status - NZCS ITR Approved / MPI Bio ITR Held", "21", declaration.TranshipmentRequest.C4_Status);
		}

		public void TestReplacementMessageTypeCanBeDeterminedOnRejection()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "S02866347";
			declaration.JE_MasterBill = "08619635755";
			declaration.JE_HouseBill = "BNAKL2866347";
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "S02866347";
			outgoingMessage.EM_LinkedObject = declaration.CusEntryHeader;

			var tswACKResponse = Factory.New<TSWMessage>();
			tswACKResponse.EM_MessageType = "RES";
			tswACKResponse.EM_MessageText = S02866347ACKResponse;
			tswACKResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(tswACKResponse);
			AssertEquals("EM_MessageSubType - response message comes from TSW", TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement, tswACKResponse.EM_MessageSubType);
			AssertEquals("ACK", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("5118958", declaration.CusEntryHeader.EntryNumber);
			AssertEquals("No customs status reported", "STC", declaration.JE_EntryStatus);

			var icrBioResponse1 = Factory.New<TSWMessage>();
			icrBioResponse1.EM_MessageType = "RES";
			icrBioResponse1.EM_MessageText = S02866347BIOResponse1;
			icrBioResponse1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponse1);
			AssertEquals("EM_MessageSubType - response message comes from MPIBIO", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBioResponse1.EM_MessageSubType);
			AssertEquals("IAR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("5118958", declaration.CusEntryHeader.EntryNumber);
			AssertEquals("No customs status reported yet - just the BIO response", "ARP", declaration.JE_EntryStatus);
			AssertEquals("Bio Status", "HLD", declaration.CusEntryHeader.CH_MPIBioStatus);

			var icrBioResponse2 = Factory.New<TSWMessage>();
			icrBioResponse2.EM_MessageType = "RES";
			icrBioResponse2.EM_MessageText = S02866347BIOResponse2;
			icrBioResponse2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponse2);
			AssertEquals("EM_MessageSubType - response message comes from MPIBIO", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBioResponse2.EM_MessageSubType);
			AssertEquals("CLR", declaration.CusEntryHeader.CH_EntryStatus);
			AssertEquals("5118958", declaration.CusEntryHeader.EntryNumber);
			AssertEquals("No customs status reported yet - just the BIO responses", LowValueConsignmentStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("No customs status reported yet - just the BIO responses", LowValueConsignmentStatusList.Descriptions.AgencyResponsePending, declaration.JE_EntryStatusDescription);
			AssertEquals("Bio Status", "MDR", declaration.CusEntryHeader.CH_MPIBioStatus);

			var icrCustomsResponse = Factory.New<TSWMessage>();
			icrCustomsResponse.EM_MessageType = "RES";
			icrCustomsResponse.EM_MessageText = S02866347NZCSResponse;
			icrCustomsResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrCustomsResponse);
			AssertEquals("5118958", declaration.CusEntryHeader.EntryNumber);

			declaration.CusEntryHeader.CH_EntryStatus = "";
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.NoStatusReported;
			AssertEquals("Pre-condition to set up client issue scenario (entry number returned but no status...)", LowValueConsignmentStatusList.Descriptions.NoStatusReported, declaration.JE_EntryStatusDescription);

			declaration.CusEntryHeader.Messages.RemoveAndDeleteAll();
			AssertEquals("Clear out any existing messages", 0, declaration.CusEntryHeader.Messages.Count);

			var manager = new MessageBuilders.ECIWriteOff.MessageManager(declaration, MessageBuilders.ECIWriteOff.MessageManager.OperationType.SubmitMessage);
			AssertEquals("Manager.Execute()", true, manager.Execute());
			var messageGenerated = declaration.CusEntryHeader.Messages[0];
			AssertNotNull(messageGenerated);
			AssertEquals("Sending message type should be able to be determined", MessageSubTypeList.Codes.Replacement, messageGenerated.EM_MessageSubType);
		}

		#region  S02866347

		public const string S02866347ACKResponse =
@"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>00326958C</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20180514083432</IssueDateTime>
    <FunctionalReferenceID>274206</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>5118958</ID>
        <FunctionalReferenceID>S02866347</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20180514083432</EffectiveDateTime>
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

		public const string S02866347BIOResponse1 =
@"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>00326958C</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20180514083443</IssueDateTime>
    <FunctionalReferenceID>274207</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>5118958</ID>
        <AcceptanceDateTime formatCode=""204"">20180514083443</AcceptanceDateTime>
        <FunctionalReferenceID>S02866347</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00326958C</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription><![CDATA[seq:1,instructions:'Held',issuedDate:'Monday, 14 May 2018 8:34:40 a.m.']]></StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08619635755</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>BNAKL2866347</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180514083443</EffectiveDateTime>
      <NameCode>B07</NameCode>
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

		public const string S02866347BIOResponse2 =
@"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>00326958C</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20180514092908</IssueDateTime>
    <FunctionalReferenceID>274287</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>5118958</ID>
        <AcceptanceDateTime formatCode=""204"">20180514092908</AcceptanceDateTime>
        <FunctionalReferenceID>S02866347</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00326958C</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">MDR</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08619635755</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>BNAKL2866347</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180514092908</EffectiveDateTime>
      <NameCode>B07</NameCode>
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

		public const string S02866347NZCSResponse =
@"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>00326958C</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20180514083451</IssueDateTime>
    <FunctionalReferenceID>274208</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>5118958</ID>
        <AcceptanceDateTime formatCode=""204"">20180514083451</AcceptanceDateTime>
        <FunctionalReferenceID>S02866347</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00326958C</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ERR</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08619635755</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>BNAKL2866347</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>632</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>27A</DocumentSectionCode>
        <TagID>246</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180514083451</EffectiveDateTime>
      <NameCode>C06</NameCode>
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

		#region Implementation

		JobDeclaration declaration;
		TSWMessage outgoingMessage;

		protected override void SetUp()
		{
			base.SetUp();
			TestHelper.SetupMessagingEnvironment();
			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "B00003483";
			declaration.JE_MasterBill = "OB020389";
			declaration.JE_HouseBill = "WI00190886";
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration.CusEntryHeader.CH_EntryStatus = LowValueManifestStatusList.Codes.SentToCustoms;

			outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "B00003483";
			outgoingMessage.EM_LinkedObject = declaration.CusEntryHeader;
		}

		#region Messages

		#region Acknowledgement

		public const string AcknowledgementResponse =
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
    <IssueDateTime formatCode=""204"">20180228152401</IssueDateTime>
    <FunctionalReferenceID>2213</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>49972962</ID>
        <FunctionalReferenceID>B00003483</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20180228152401</EffectiveDateTime>
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

		#region Error Message

		public const string ErrorMessageResponse =
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
    <IssueDateTime formatCode=""204"">20170905153252</IssueDateTime>
    <FunctionalReferenceID>1275</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>00000000</ID>
        <FunctionalReferenceID>B00003483</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>189</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>29A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>93A</DocumentSectionCode>
        <TagID>141</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>4077</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>64A</DocumentSectionCode>
        <TagID>L017</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>1025</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>30A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>04A</DocumentSectionCode>
        <TagID>242</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>522</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>38B/L013</DocumentSectionCode>
        <TagID>173</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>552</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>02A</DocumentSectionCode>
        <TagID>D005</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>549</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>02A</DocumentSectionCode>
        <TagID>D005</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20170905153252</EffectiveDateTime>
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
		public const string ErrorMessageResponseFormatted =
@"[ICR/CRE Rejected] Response for ECI Write-Off: B00003483

Error report
---------------------------------------------------------------------
ECI Write-Off  : B00003483
Entry Number   : 
Master Bill    : OB0-20389
Message No     : 1275

Message Status : (841) ICR / CRE / OCR / ANA / AND rejected
               : error report attached.

Message Errors
---------------------------------------------------------------------
**Error** in {Declaration/Consignment[1]/ConsignmentItem[1]/Packaging[1]/TypeCode}:-
  Type of Package : Not specified or invalid
**Error** in {Declaration/Consignment[1]/GoodsLocation/ID}:-
  Location of Goods Code : Not specified or invalid
**Error** in {Declaration/Consignment[1]/Consignor/Address/CountryCode}:-
  Country/Region: Not specified or invalid
**Error** in {Declaration/Consignment[1]/Unknown/Unknown}:-
  Date of Arrival / Departure : Not specified or invalid
**Error** in {Declaration/Consignment[1]/AdditionalDocument[1]/ID}:-
  Permit Authority Number : Not specified
**Error** in {Declaration/Consignment[1]/AdditionalDocument[1]/ID}:-
  Permit Authority Code : Not specified
";

		#endregion

		#region WOF Message

		public const string WOFMessageResponse =
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
    <IssueDateTime formatCode=""204"">20180228152417</IssueDateTime>
    <FunctionalReferenceID>2220</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>49972962</ID>
        <AcceptanceDateTime formatCode=""204"">20180228152417</AcceptanceDateTime>
        <FunctionalReferenceID>B00003483</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>OB020389</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>WI00190886</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180228152417</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20180228152417</ReleaseDateTime>
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
		public const string WOFMessageResponseFormatted =
@"[ICR/CRE Accepted, Check Consignments for Status] Response for ECI Write-Off: B00003483

Clearance / Acceptance Instructions
---------------------------------------------------------------------
ECI Write-Off  : B00003483
Entry Number   : 49972962
Master Bill    : OB0-20389
Message No     : 2220

Message Status : (C06) Customs Cargo Report Notification - consignment status as specified

Job Responses
---------------------------------------------------------------------
Job Number: B00003483   House Bill: WI00190886
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";

		#endregion

		#region MPIBIO Message

		public const string MPIBIOResponse =
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
    <IssueDateTime formatCode=""204"">20180228152410</IssueDateTime>
    <FunctionalReferenceID>2218</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>49972962</ID>
        <AcceptanceDateTime formatCode=""204"">20180228152410</AcceptanceDateTime>
        <FunctionalReferenceID>B00003483</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">MDR</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>OB020389</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>WI00190886</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180228152410</EffectiveDateTime>
      <NameCode>B07</NameCode>
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

		public const string MPIBIOResponseFormatted =
@"[Inspections/Audit Requirements] Response for ECI Write-Off: B00003483

Inspection / Audit requirements
---------------------------------------------------------------------
ECI Write-Off  : B00003483
Entry Number   : 49972962
Master Bill    : OB0-20389
Message No     : 2218

Message Status : (B07) MPI Biosecurity Cargo Report Notification - consignment status as specified

Job Responses
---------------------------------------------------------------------
Job Number: B00003483   House Bill: WI00190886
--- Clearance Status: MDR-MPI Import Dec Required ---
";

		#endregion

		#region ITRResponses

		#region MPIBIO ITR Message

		public const string MPIBIO_ITRResponse =
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
    <IssueDateTime formatCode=""204"">20190226172313</IssueDateTime>
    <FunctionalReferenceID>5283</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MB 08100234920 - MOVEMENT APPROVED</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>14168768</ID>
        <AcceptanceDateTime formatCode=""204"">20190226172313</AcceptanceDateTime>
        <FunctionalReferenceID>B00004206</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ITA</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">ITA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08100234920</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>G45023</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190226172313</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <ReleaseDateTime formatCode=""204"">20190226172313</ReleaseDateTime>
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

		#region NZCS ITR Message

		public const string NZCS_ITRResponse =
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
    <IssueDateTime formatCode=""204"">20190226172311</IssueDateTime>
    <FunctionalReferenceID>5282</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MB 08100234920 - MOVEMENT HELD</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>14168768</ID>
        <AcceptanceDateTime formatCode=""204"">20190226172311</AcceptanceDateTime>
        <FunctionalReferenceID>B00004206</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ITA</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">ITA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08100234920</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>G45023</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190226172311</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20190226172311</ReleaseDateTime>
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

		#region MPIBIO ITR Message

		public const string MPIBIO_ITRHeldResponse =
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
    <IssueDateTime formatCode=""204"">20190226172313</IssueDateTime>
    <FunctionalReferenceID>5283</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MB 08100234920 - MOVEMENT HELD</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>14168768</ID>
        <AcceptanceDateTime formatCode=""204"">20190226172313</AcceptanceDateTime>
        <FunctionalReferenceID>B00004206</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ITA</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">HLD</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08100234920</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>G45023</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190226172313</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <ReleaseDateTime formatCode=""204"">20190226172313</ReleaseDateTime>
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

		#endregion

		#endregion
	}

	sealed class ICRMessageProcessorMAWBTest : TestCaseWithFactory
	{
		public void TestAcknowledgementResponse()
		{
			var icrMessage = Factory.New<TSWMessage>();
			icrMessage.EM_MessageType = "RES";
			icrMessage.EM_MessageText = AcknowledgementResponse;
			icrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrMessage);
			AssertEquals(mawb, icrMessage.EM_LinkedObject);
			AssertEquals("EM_MessageSubType - Acknowledgement message", TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement, icrMessage.EM_MessageSubType);
			AssertEquals(AcknowledgementResponseFormatted, icrMessage.EM_MessageInterpretation);
			AssertEquals("7435668", mawb.ECINumber);
			AssertEquals("ACK", mawb.CM_CustomsStatus);
			AssertEquals("Acknowledgement", mawb.CustomsStatusDescription);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBIOResponse()
		{
			var icrBioMessage = Factory.New<TSWMessage>();
			icrBioMessage.EM_MessageType = "RES";
			icrBioMessage.EM_MessageText = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\TradeSingleWindow\WriteOff\TestFiles\ICRBioHoldResponse.txt"));
			icrBioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioMessage);
			AssertEquals(mawb, icrBioMessage.EM_LinkedObject);
			AssertEquals("EM_MessageSubType - MPI BIO message", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBioMessage.EM_MessageSubType);
			AssertEquals("7435668", mawb.ECINumber);
			AssertEquals("IAR", mawb.CM_CustomsStatus);
			AssertEquals("Inspections/Audit Requirements", mawb.CustomsStatusDescription);

			AssertEquals("Consignment Status - Customs Pending / BIO Held", LowValueConsignmentStatusList.Codes.PH, hawb.CS_CustomsStatus);
			AssertEquals("Consignment Status", "NZCS - Pending / BIO - Consignment Held", hawb.CustomsStatusDescription);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteOffMessageResponse()
		{
			hawb.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.PC;
			var icrMessage = Factory.New<TSWMessage>();
			icrMessage.EM_MessageType = "RES";
			icrMessage.EM_MessageText = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\TradeSingleWindow\WriteOff\TestFiles\ICRAirCargoWriteOffResponse.txt"));
			icrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			var logEntries = hawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrMessage);
			AssertEquals(WriteOffResponseFormatted, icrMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, icrMessage.EM_MessageSubType);
			AssertEquals("7435668", mawb.ECINumber);
			AssertEquals("CLR", mawb.CM_CustomsStatus);
			AssertEquals("Consignment Status, NZCS WOF", LowValueConsignmentStatusList.Codes.CC, hawb.CS_CustomsStatus);
			AssertEquals("Consignment Status", "Consignment Written Off/Cleared", hawb.CustomsStatusDescription);

			var messageWrittenOffEvent = mawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageAcceptedCode)).First();
			AssertNotNull("messageWrittenOffEvent", messageWrittenOffEvent);
			AssertEquals("messageWrittenOffEvent = MessageAccepted", "REG", messageWrittenOffEvent.SL_Reference);
			logEntries = hawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry event has been created for this cleared write-off entry", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
		}

		public void TestCombinedStatusForClearedFullSuiteResponse()
		{
			mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_MessageReference = "X00001209";
			mawb.CM_MAWB = "08112121211";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			mawb.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;

			hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "TSETBILL1";
			hawb.CS_ConsignmentNum = 1;
			hawb.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "X00001209";
			outgoingMessage.EM_LinkedObject = mawb;

			var logEntries = hawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			#region Acknowledgement Response

			var icrACKMessage = Factory.New<TSWMessage>();
			icrACKMessage.EM_MessageType = "RES";
			icrACKMessage.EM_MessageText = ACKResponse;
			icrACKMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrACKMessage);
			AssertEquals(mawb, icrACKMessage.EM_LinkedObject);
			AssertEquals("EM_MessageSubType - Acknowledgement message", TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement, icrACKMessage.EM_MessageSubType);
			AssertEquals("56624546", mawb.ECINumber);
			AssertEquals("ACK", mawb.CM_CustomsStatus);
			AssertEquals("Acknowledgement", mawb.CustomsStatusDescription);

			AssertEquals("Consignment Status is not affected by ACK message", "STC", hawb.CS_CustomsStatus);

			#endregion

			#region Initial BIO Held Response

			var icrBioMessage = Factory.New<TSWMessage>();
			icrBioMessage.EM_MessageType = "RES";
			icrBioMessage.EM_MessageText = BIOHeldResponse;
			icrBioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioMessage);
			AssertEquals(mawb, icrBioMessage.EM_LinkedObject);
			AssertEquals("EM_MessageSubType - MPI BIO message", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBioMessage.EM_MessageSubType);
			AssertEquals("56624546", mawb.ECINumber);
			AssertEquals("IAR", mawb.CM_CustomsStatus);
			AssertEquals("Inspections/Audit Requirements", mawb.CustomsStatusDescription);

			AssertEquals("Consignment Status - Customs Pending / BIO Held", LowValueConsignmentStatusList.Codes.PH, hawb.CS_CustomsStatus);
			AssertEquals("Consignment Status", "NZCS - Pending / BIO - Consignment Held", hawb.CustomsStatusDescription);
			logEntries = hawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry for MPI Bio has been created for this held write-off", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - BIO")));

			#endregion

			#region Customs Write-off Response

			var icrMessage = Factory.New<TSWMessage>();
			icrMessage.EM_MessageType = "RES";
			icrMessage.EM_MessageText = CustomsWOFResponse;
			icrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrMessage);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, icrMessage.EM_MessageSubType);
			AssertEquals("56624546", mawb.ECINumber);
			AssertEquals("CLR", mawb.CM_CustomsStatus);
			AssertEquals("Consignment Status, NZCS WOF - Bio Held", LowValueConsignmentStatusList.Codes.CH, hawb.CS_CustomsStatus);
			AssertEquals("Consignment Status - Customs WOF", "NZCS - Consignment Written Off / BIO - Consignment Held", hawb.CustomsStatusDescription);

			var messageAcceptedEvent = mawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageAcceptedCode)).First();
			AssertNotNull("messageAcceptedEvent - MessageAccepted", messageAcceptedEvent);

			#endregion

			#region Subsequent BIO Write-off Response

			var bioClearMessage = Factory.New<TSWMessage>();
			bioClearMessage.EM_MessageType = "RES";
			bioClearMessage.EM_MessageText = BIOClearedResponse;
			bioClearMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(bioClearMessage);
			AssertEquals("56624546", mawb.ECINumber);
			AssertEquals("CLR", mawb.CM_CustomsStatus);

			AssertEquals("Consignment Status, NZCS WOF - Bio now WOF as well", LowValueConsignmentStatusList.Codes.CC, hawb.CS_CustomsStatus);
			AssertEquals("Consignment Status - written off", "Consignment Written Off/Cleared", hawb.CustomsStatusDescription);

			messageAcceptedEvent = mawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageAcceptedCode)).First();
			AssertNotNull("messageAcceptedEvent", messageAcceptedEvent);
			AssertEquals("messageAcceptedEvent - MessageAccepted", "REG", messageAcceptedEvent.SL_Reference);

			logEntries = hawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry has been updated for hawb", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - BIO")));

			logEntries = hawb.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry event has been created for the cleared customs response", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));

			#endregion
		}

		public void TestICRImportDeclarationRequiredStatus()
		{
			mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_MessageReference = "X00001250";
			mawb.CM_MAWB = "08111223343";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			mawb.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;

			hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "HBILL1";
			hawb.CS_ConsignmentNum = 1;
			hawb.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "HBILL2";
			hawb2.CS_ConsignmentNum = 2;
			hawb2.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb3 = mawb.ChildBills.AddNew();
			hawb3.CS_HAWB = "HBILL3";
			hawb3.CS_ConsignmentNum = 3;
			hawb3.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "X00001250";
			outgoingMessage.EM_LinkedObject = mawb;

			#region Customs Write-off Response

			var icrMessage = Factory.New<TSWMessage>();
			icrMessage.EM_MessageType = "RES";
			icrMessage.EM_MessageText = X00001250Response;
			icrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrMessage);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, icrMessage.EM_MessageSubType);
			AssertEquals("94209793", mawb.ECINumber);
			AssertEquals("CLR", mawb.CM_CustomsStatus);
			AssertEquals("Consignment Status, NZCS IDR (BIO Pending)", LowValueConsignmentStatusList.Codes.DP, hawb.CS_CustomsStatus);
			AssertEquals("Consignment Status - Customs IDR", "NZCS - Import Declaration Required / BIO - Pending", hawb.CustomsStatusDescription);
			AssertEquals("Consignment Status, NZCS IDR (BIO Pending)", LowValueConsignmentStatusList.Codes.DP, hawb2.CS_CustomsStatus);
			AssertEquals("Consignment Status - Customs IDR", "NZCS - Import Declaration Required / BIO - Pending", hawb2.CustomsStatusDescription);
			AssertEquals("Consignment Status, NZCS IDR (BIO Pending)", LowValueConsignmentStatusList.Codes.DP, hawb3.CS_CustomsStatus);
			AssertEquals("Consignment Status - Customs IDR", "NZCS - Import Declaration Required / BIO - Pending", hawb3.CustomsStatusDescription);

			#endregion
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestBIOSecondResponseWithoutAllConsignments()
		{
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_MessageReference = "X00016201";
			mawb.CM_MAWB = "08654569874";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			mawb.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;

			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "92094210311901276383";
			hawb1.CS_ConsignmentNum = 1;
			hawb1.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "92094210305202467571";
			hawb2.CS_ConsignmentNum = 2;
			hawb2.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb3 = mawb.ChildBills.AddNew();
			hawb3.CS_HAWB = "92094210305202467663";
			hawb3.CS_ConsignmentNum = 3;
			hawb3.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb4 = mawb.ChildBills.AddNew();
			hawb4.CS_HAWB = "UCI003243435";
			hawb4.CS_ConsignmentNum = 4;
			hawb4.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb5 = mawb.ChildBills.AddNew();
			hawb5.CS_HAWB = "UCI003242322";
			hawb5.CS_ConsignmentNum = 5;
			hawb5.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb6 = mawb.ChildBills.AddNew();
			hawb6.CS_HAWB = "92094210311901279089";
			hawb6.CS_ConsignmentNum = 6;
			hawb6.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb7 = mawb.ChildBills.AddNew();
			hawb7.CS_HAWB = "92094210311901278418";
			hawb7.CS_ConsignmentNum = 7;
			hawb7.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb8 = mawb.ChildBills.AddNew();
			hawb8.CS_HAWB = "92094210305202466802";
			hawb8.CS_ConsignmentNum = 8;
			hawb8.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb9 = mawb.ChildBills.AddNew();
			hawb9.CS_HAWB = "92094210305203260225";
			hawb9.CS_ConsignmentNum = 9;
			hawb9.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb10 = mawb.ChildBills.AddNew();
			hawb10.CS_HAWB = "92094210305203260225";
			hawb10.CS_ConsignmentNum = 10;
			hawb10.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb11 = mawb.ChildBills.AddNew();
			hawb11.CS_HAWB = "92094210311901278265";
			hawb11.CS_ConsignmentNum = 11;
			hawb11.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb12 = mawb.ChildBills.AddNew();
			hawb12.CS_HAWB = "92094210311901278265";
			hawb12.CS_ConsignmentNum = 12;
			hawb12.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "X00016201";
			outgoingMessage.EM_LinkedObject = mawb;

			var icrBioResponse1 = Factory.New<TSWMessage>();
			icrBioResponse1.EM_MessageType = "RES";
			icrBioResponse1.EM_MessageText = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\TradeSingleWindow\WriteOff\TestFiles\ICRBioResponse1.txt"));
			icrBioResponse1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponse1);
			AssertEquals(mawb, icrBioResponse1.EM_LinkedObject);
			AssertEquals("EM_MessageSubType - MPI BIO message", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBioResponse1.EM_MessageSubType);
			AssertEquals("22925526", mawb.ECINumber);
			AssertEquals("IAR", mawb.CM_CustomsStatus);
			AssertEquals("Inspections/Audit Requirements", mawb.CustomsStatusDescription);

			AssertEquals("Consignment1 Status - Customs Pending / BIO Held", LowValueConsignmentStatusList.Codes.PH, hawb1.CS_CustomsStatus);
			AssertEquals("Consignment1 Status", "NZCS - Pending / BIO - Consignment Held", hawb1.CustomsStatusDescription);

			AssertEquals("Consignment2 Status - Customs Pending / BIO Held", LowValueConsignmentStatusList.Codes.PH, hawb2.CS_CustomsStatus);
			AssertEquals("Consignment2 Status", "NZCS - Pending / BIO - Consignment Held", hawb2.CustomsStatusDescription);

			AssertEquals("Consignment3 Status - Customs Pending / BIO Held", LowValueConsignmentStatusList.Codes.PH, hawb3.CS_CustomsStatus);
			AssertEquals("Consignment3 Status", "NZCS - Pending / BIO - Consignment Held", hawb3.CustomsStatusDescription);

			AssertEquals("Consignment4 Status - Customs Pending / BIO Held", LowValueConsignmentStatusList.Codes.PD, hawb4.CS_CustomsStatus);
			AssertEquals("Consignment4 Status", "NZCS - Pending / BIO - Import Declaration Required", hawb4.CustomsStatusDescription);

			AssertEquals("Consignment5 Status - Customs Pending / BIO Held", LowValueConsignmentStatusList.Codes.PD, hawb5.CS_CustomsStatus);
			AssertEquals("Consignment5 Status", "NZCS - Pending / BIO - Import Declaration Required", hawb5.CustomsStatusDescription);

			AssertEquals("Consignment6 Status - Customs Pending / BIO Held", LowValueConsignmentStatusList.Codes.PH, hawb6.CS_CustomsStatus);
			AssertEquals("Consignment6 Status", "NZCS - Pending / BIO - Consignment Held", hawb6.CustomsStatusDescription);

			AssertEquals("Consignment7 Status - Customs Pending / BIO Held", LowValueConsignmentStatusList.Codes.PH, hawb7.CS_CustomsStatus);
			AssertEquals("Consignment7 Status", "NZCS - Pending / BIO - Consignment Held", hawb7.CustomsStatusDescription);

			AssertEquals("Consignment8 Status - Customs Pending / BIO Held", LowValueConsignmentStatusList.Codes.PH, hawb8.CS_CustomsStatus);
			AssertEquals("Consignment8 Status", "NZCS - Pending / BIO - Consignment Held", hawb8.CustomsStatusDescription);

			AssertEquals("Consignment9 Status - Customs Pending / BIO Held", LowValueConsignmentStatusList.Codes.PH, hawb9.CS_CustomsStatus);
			AssertEquals("Consignment9 Status", "NZCS - Pending / BIO - Consignment Held", hawb9.CustomsStatusDescription);

			AssertEquals("Consignment10 Status - Customs Pending / BIO Held", LowValueConsignmentStatusList.Codes.PH, hawb10.CS_CustomsStatus);
			AssertEquals("Consignment10 Status", "NZCS - Pending / BIO - Consignment Held", hawb10.CustomsStatusDescription);

			AssertEquals("Consignment11 Status - Customs Pending / BIO Held", LowValueConsignmentStatusList.Codes.PH, hawb11.CS_CustomsStatus);
			AssertEquals("Consignment11 Status", "NZCS - Pending / BIO - Consignment Held", hawb11.CustomsStatusDescription);

			AssertEquals("Consignment12 Status - Customs Pending / BIO Held", LowValueConsignmentStatusList.Codes.PH, hawb12.CS_CustomsStatus);
			AssertEquals("Consignment12 Status", "NZCS - Pending / BIO - Consignment Held", hawb12.CustomsStatusDescription);

			// second Bio response does not contain statuses for all consignments - consignments without statuses should not get their existing status overridden.
			var icrBioResponse2 = Factory.New<TSWMessage>();
			icrBioResponse2.EM_MessageType = "RES";
			icrBioResponse2.EM_MessageText = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\TradeSingleWindow\WriteOff\TestFiles\ICRBioResponse2.txt"));
			icrBioResponse2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponse2);
			AssertEquals(mawb, icrBioResponse2.EM_LinkedObject);
			AssertEquals("ICRBioResponse2 - EM_MessageSubType - MPI BIO message", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBioResponse2.EM_MessageSubType);
			AssertEquals("22925526", mawb.ECINumber);
			AssertEquals("CLR", mawb.CM_CustomsStatus);
			AssertEquals("ICR/CRE Accepted, Check Consignments for Status", mawb.CustomsStatusDescription);

			AssertEquals("Consignment1 Status - Customs Pending / BIO WOF", LowValueConsignmentStatusList.Codes.PC, hawb1.CS_CustomsStatus);
			AssertEquals("Consignment1 Status", "NZCS - Pending / BIO - Consignment Written Off/Cleared", hawb1.CustomsStatusDescription);

			AssertEquals("Consignment2 Status - Customs Pending / BIO WOF", LowValueConsignmentStatusList.Codes.PC, hawb2.CS_CustomsStatus);
			AssertEquals("Consignment2 Status", "NZCS - Pending / BIO - Consignment Written Off/Cleared", hawb2.CustomsStatusDescription);

			AssertEquals("Consignment3 Status - Customs Pending / BIO WOF", LowValueConsignmentStatusList.Codes.PC, hawb3.CS_CustomsStatus);
			AssertEquals("Consignment3 Status", "NZCS - Pending / BIO - Consignment Written Off/Cleared", hawb3.CustomsStatusDescription);

			AssertEquals("Consignment6 Status - Customs Pending / BIO WOF", LowValueConsignmentStatusList.Codes.PM, hawb6.CS_CustomsStatus);
			AssertEquals("Consignment6 Status", "NZCS - Pending / BIO - MPI Import Dec Required", hawb6.CustomsStatusDescription);

			AssertEquals("Consignment7 Status - Customs Pending / BIO WOF", LowValueConsignmentStatusList.Codes.PC, hawb7.CS_CustomsStatus);
			AssertEquals("Consignment7 Status", "NZCS - Pending / BIO - Consignment Written Off/Cleared", hawb7.CustomsStatusDescription);

			AssertEquals("Consignment8 Status - Customs Pending / BIO WOF", LowValueConsignmentStatusList.Codes.PC, hawb8.CS_CustomsStatus);
			AssertEquals("Consignment8 Status", "NZCS - Pending / BIO - Consignment Written Off/Cleared", hawb8.CustomsStatusDescription);

			AssertEquals("Consignment9 Status - Customs Pending / BIO WOF", LowValueConsignmentStatusList.Codes.PC, hawb9.CS_CustomsStatus);
			AssertEquals("Consignment9 Status", "NZCS - Pending / BIO - Consignment Written Off/Cleared", hawb9.CustomsStatusDescription);

			AssertEquals("Consignment10 Status - Customs Pending / BIO WOF", LowValueConsignmentStatusList.Codes.PC, hawb10.CS_CustomsStatus);
			AssertEquals("Consignment10 Status", "NZCS - Pending / BIO - Consignment Written Off/Cleared", hawb10.CustomsStatusDescription);

			AssertEquals("Consignment11 Status - Customs Pending / BIO WOF", LowValueConsignmentStatusList.Codes.PC, hawb11.CS_CustomsStatus);
			AssertEquals("Consignment11 Status", "NZCS - Pending / BIO - Consignment Written Off/Cleared", hawb11.CustomsStatusDescription);

			AssertEquals("Consignment12 Status - Customs Pending / BIO WOF", LowValueConsignmentStatusList.Codes.PC, hawb12.CS_CustomsStatus);
			AssertEquals("Consignment12 Status", "NZCS - Pending / BIO - Consignment Written Off/Cleared", hawb12.CustomsStatusDescription);

			// Consignments not updated by this message should not have their previous status overridden
			AssertEquals("Consignment4 Status should not have been overridden - Customs Pending / BIO Held", LowValueConsignmentStatusList.Codes.PD, hawb4.CS_CustomsStatus);
			AssertEquals("Consignment4 Status", "NZCS - Pending / BIO - Import Declaration Required", hawb4.CustomsStatusDescription);

			AssertEquals("Consignment5 Status should not have been overridden - Customs Pending / BIO Held", LowValueConsignmentStatusList.Codes.PD, hawb5.CS_CustomsStatus);
			AssertEquals("Consignment5 Status", "NZCS - Pending / BIO - Import Declaration Required", hawb5.CustomsStatusDescription);
		}

		public void TestMovementStatusResponsesForMAWB()
		{
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_MessageReference = "X00001580";
			mawb.CM_MAWB = "08112221112";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			mawb.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;

			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "HBILL1";
			hawb1.CS_ConsignmentNum = 1;
			var hawb1ITR = TranshipmentRequest.Create(hawb1);
			hawb1ITR.C4_ModeOfMovement = "4";
			hawb1.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var hawb2 = mawb.ChildBills.AddNew();
			hawb2.CS_HAWB = "HBILL2";
			hawb2.CS_ConsignmentNum = 2;
			var hawb2ITR = TranshipmentRequest.Create(hawb2);
			hawb2ITR.C4_ModeOfMovement = "1";
			hawb2.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "X00001580";
			outgoingMessage.EM_LinkedObject = mawb;

			#region Bio Response

			var icrBioResponse = Factory.New<TSWMessage>();
			icrBioResponse.EM_MessageType = "RES";
			icrBioResponse.EM_MessageText = X00001580BioResponse;
			icrBioResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponse);
			AssertEquals(mawb, icrBioResponse.EM_LinkedObject);
			AssertEquals("EM_MessageSubType - MPI BIO message", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBioResponse.EM_MessageSubType);
			AssertEquals("24181441", mawb.ECINumber);
			AssertEquals("CLR", mawb.CM_CustomsStatus);
			AssertEquals("ICR/CRE Accepted, Check Consignments for Status", mawb.CustomsStatusDescription);

			AssertEquals("Consignment1 Status - Customs Pending / BIO ITR Approved", LowValueConsignmentStatusList.Codes.PA, hawb1.CS_CustomsStatus);
			AssertEquals("Consignment1 Status", "NZCS - Pending / BIO - ITR Approved", hawb1.CustomsStatusDescription);
			AssertEquals("Consignment1 Bio Movement Status", "ITA", hawb1.CS_BioMovementStatus);
			AssertEquals("TranshipmentRequest.C4_Status", CombinedMovementStatus.Codes.PI, hawb1.TranshipmentRequest.C4_Status);
			AssertEquals("TranshipmentRequest MovementStatusDesc", "NZCS Transhipment Request Pending / MPI Biosecurity ITR Approved", hawb1.TranshipmentRequest.MovementStatusDesc);

			AssertEquals("Consignment2 Status - Customs Pending / BIO ITR Declined", LowValueConsignmentStatusList.Codes.PT, hawb2.CS_CustomsStatus);
			AssertEquals("Consignment2 Status", "NZCS - Pending / BIO - Transhipment Declined", hawb2.CustomsStatusDescription);
			AssertEquals("CS_BioMovementStatus", "ITD", hawb2.CS_BioMovementStatus);
			AssertEquals("TranshipmentRequest.C4_Status", CombinedMovementStatus.Codes.PX, hawb2.TranshipmentRequest.C4_Status);
			AssertEquals("TranshipmentRequest MovementStatusDesc", "NZCS Transhipment Request Pending / MPI Biosecurity ITR Declined", hawb2.TranshipmentRequest.MovementStatusDesc);

			#endregion

			#region Customs Response

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = "RES";
			nzcsMessage.EM_MessageText = X00001580NZCSResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, nzcsMessage.EM_MessageSubType);
			AssertEquals("24181441", mawb.ECINumber);
			AssertEquals("CLR", mawb.CM_CustomsStatus);
			AssertEquals("Consignment Status, ITA Approved", LowValueConsignmentStatusList.Codes.AA, hawb1.CS_CustomsStatus);
			AssertEquals("Consignment Status - ITA", "International Transhipment Approved", hawb1.CustomsStatusDescription);
			AssertEquals("Consignment1 Bio Movement Status", "ITA", hawb1.CS_CustomsMovementStatus);
			AssertEquals("TranshipmentRequest.C4_Status", CombinedMovementStatus.Codes.II, hawb1.TranshipmentRequest.C4_Status);
			AssertEquals("TranshipmentRequest MovementStatusDesc", "ITR Approved", hawb1.TranshipmentRequest.MovementStatusDesc);

			AssertEquals("Consignment2 Status - ITR declined", LowValueConsignmentStatusList.Codes.TT, hawb2.CS_CustomsStatus);
			AssertEquals("Consignment2 Status", "Transhipment Declined", hawb2.CustomsStatusDescription);
			AssertEquals("Consignment1 Bio Movement Status", "ITD", hawb2.CS_CustomsMovementStatus);
			AssertEquals("TranshipmentRequest.C4_Status", CombinedMovementStatus.Codes.XX, hawb2.TranshipmentRequest.C4_Status);
			AssertEquals("TranshipmentRequest MovementStatusDesc", "ITR Declined", hawb2.TranshipmentRequest.MovementStatusDesc);

			#endregion
		}

		public void TestStatusResponseForDTR()
		{
			var mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_MessageReference = "X00001816";
			mawb.CM_MAWB = "08644568764";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			mawb.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;

			var hawb1 = mawb.ChildBills.AddNew();
			hawb1.CS_HAWB = "SYD1145";
			hawb1.CS_ConsignmentNum = 1;
			var hawb1DTR = TranshipmentRequest.Create(hawb1);
			hawb1DTR.C4_MovementReason = MovementReason.Codes.DomesticTranshipmentRequest;
			hawb1DTR.C4_ModeOfMovement = "3";
			hawb1.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "X00001816";
			outgoingMessage.EM_LinkedObject = mawb;

			#region Bio Response

			var icrBioResponse = Factory.New<TSWMessage>();
			icrBioResponse.EM_MessageType = "RES";
			icrBioResponse.EM_MessageText = X00001816BioResponse;
			icrBioResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponse);
			AssertEquals(mawb, icrBioResponse.EM_LinkedObject);
			AssertEquals("EM_MessageSubType - MPI BIO message", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBioResponse.EM_MessageSubType);
			AssertEquals("72285586", mawb.ECINumber);
			AssertEquals("IAR", mawb.CM_CustomsStatus);
			AssertEquals("Inspections/Audit Requirements", mawb.CustomsStatusDescription);

			AssertEquals("Consignment1 Status - Customs Pending / BIO - Consignment Held", LowValueConsignmentStatusList.Codes.PH, hawb1.CS_CustomsStatus);
			AssertEquals("Consignment1 Status", "NZCS - Pending / BIO - Consignment Held", hawb1.CustomsStatusDescription);
			AssertEquals("Consignment1 Bio Movement Status", "", hawb1.CS_BioMovementStatus);
			AssertEquals("TranshipmentRequest.C4_Status", "", hawb1.TranshipmentRequest.C4_Status);
			AssertEquals("TranshipmentRequest MovementStatusDesc", "", hawb1.TranshipmentRequest.MovementStatusDesc);

			#endregion

			#region Customs Response

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = "RES";
			nzcsMessage.EM_MessageText = X00001816NZCSResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, nzcsMessage.EM_MessageSubType);
			AssertEquals("72285586", mawb.ECINumber);
			AssertEquals("CLR", mawb.CM_CustomsStatus);
			AssertEquals("Consignment Status, Held", LowValueConsignmentStatusList.Codes.CH, hawb1.CS_CustomsStatus);
			AssertEquals("Consignment Status - ITA", "NZCS - Consignment Written Off / BIO - Consignment Held", hawb1.CustomsStatusDescription);
			AssertEquals("Consignment1 Bio Movement Status", "", hawb1.CS_CustomsMovementStatus);
			AssertEquals("TranshipmentRequest.C4_Status", "", hawb1.TranshipmentRequest.C4_Status);
			AssertEquals("TranshipmentRequest MovementStatusDesc", "", hawb1.TranshipmentRequest.MovementStatusDesc);

			#endregion
		}

		public void TestICRCancellationResponseForAirCargo()
		{
			mawb.Logs.RemoveAndDeleteAll();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00009090";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

			mawb.CM_MessageReference = "X00002314";
			hawb.CS_JS = shipment.PK;
			hawb.CS_IsHVLV = true;
			hawb.CS_RL_NKDestination = "NZAKL";
			hawb.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.AA;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "X00002314";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageSubType = NZ.TradeSingleWindow.MessageSubTypeList.Codes.Cancellation;
			outgoingMessage.EM_LinkedObject = mawb;

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = "RES";
			nzcsMessage.EM_MessageText = airCargoCancellationMessage;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			Factory.Save();

			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, nzcsMessage.EM_MessageSubType);
			AssertEquals("90284777", mawb.ECINumber);
			AssertEquals("CAN", mawb.CM_CustomsStatus);
			AssertEquals("Consignment Status", LowValueConsignmentStatusList.Codes.ConsignmentCancelled, hawb.CS_CustomsStatus);
			AssertEquals("Consignment Status Description", "Consignment Cancelled", hawb.CustomsStatusDescription);

			var query = new ZQuery(StmALogSchema.SL_Parent, hawb.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			var cESEventLogs = Factory.Load<StmALog>(query);
			AssertEquals("One CES log entry", 1, cESEventLogs.Length);
			AssertEquals("SER included in Free text", "|LOC=NZAKL|SER=PCS|TYP=HLD", cESEventLogs[0].SL_Reference);
		}

		#region Implementation

		CusMAWB mawb;
		CusHAWB hawb;

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			mawb = Factory.NewWithValidTestData<CusMAWB>();
			mawb.CM_MessageReference = "X00001215";
			mawb.CM_MAWB = "08100428245";
			mawb.CM_RL_NKLoadPort = "AUSYD";
			mawb.CM_RL_NKDischargePort = "NZAKL";
			mawb.CM_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;

			hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "Y202498";
			hawb.CS_ConsignmentNum = 1;
			hawb.CS_CustomsStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "X00001215";
			outgoingMessage.EM_LinkedObject = mawb;
		}

		#region Messages

		#region X00001215

		#region Acknowledgement

		public const string AcknowledgementResponse =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20180328125652</IssueDateTime>
    <FunctionalReferenceID>2523</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>7435668</ID>
        <FunctionalReferenceID>X00001215</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20180328125652</EffectiveDateTime>
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

		public const string AcknowledgementResponseFormatted = "[Acknowledgement] Response for AirCargo ICR/CRE: X00001215";

		#endregion

		#region MPI BIO Hold

		public const string BIOResponseFormatted = @"[Inspections/Audit Requirements] Response for AirCargo ECI/ICR: X00001215

Clearance / Acceptance Instructions
---------------------------------------------------------------------
AirCargo ECI/ICR : X00001215
ECI Number       : 7435668
Master Bill      : 081-00428245
Message No       : 2524

Message Status   : (B07) MPI Biosecurity Cargo Report Notification - consignment status as specified

Customs Instructions
---------------------------------------------------------------------
seq:1,instructions:'
To be held pending further instructions by MPI'
,issuedDate:'
Wednesday, 28 March 2018 12:56:58 p.m.'


Job Responses
---------------------------------------------------------------------
Job Number: X00001215-1   House Bill: Y202498
--- Clearance Status: HLD-Consignment Held ---";

		#endregion

		#region WriteOff

		public const string WriteOffResponseFormatted =
@"[ICR/CRE Accepted, Check Consignments for Status] Response for AirCargo ICR/CRE: X00001215

Clearance / Acceptance Instructions
---------------------------------------------------------------------
AirCargo ICR/CRE : X00001215
Entry Number     : 7435668
Master Bill      : 081-00428245
Message No       : 2525

Message Status   : (C06) Customs Cargo Report Notification - consignment status as specified

Customs Instructions
---------------------------------------------------------------------
StatementDescriptionTest
MB 08601123426 - MOVEMENT HELD

Job Responses
---------------------------------------------------------------------
Job Number: X00001215-1   House Bill: Y202498
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";

		#endregion

		#endregion

		#region X00001209 Cleared

		public const string ACKResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20180305103129</IssueDateTime>
    <FunctionalReferenceID>2259</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>56624546</ID>
        <FunctionalReferenceID>X00001209</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20180305103129</EffectiveDateTime>
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

		public const string BIOHeldResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20180305103135</IssueDateTime>
    <FunctionalReferenceID>2260</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>56624546</ID>
        <AcceptanceDateTime formatCode=""204"">20180305103135</AcceptanceDateTime>
        <FunctionalReferenceID>X00001209</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08112121211</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>TSETBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180305103135</EffectiveDateTime>
      <NameCode>B07</NameCode>
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

		public const string CustomsWOFResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20180305103142</IssueDateTime>
    <FunctionalReferenceID>2261</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>56624546</ID>
        <AcceptanceDateTime formatCode=""204"">20180305103142</AcceptanceDateTime>
        <FunctionalReferenceID>X00001209</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08112121211</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>TSETBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180305103142</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20180305103142</ReleaseDateTime>
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

		public const string BIOClearedResponse =
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
    <IssueDateTime formatCode=""204"">20180305151425</IssueDateTime>
    <FunctionalReferenceID>2266</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>56624546</ID>
        <AcceptanceDateTime formatCode=""204"">20180305151425</AcceptanceDateTime>
        <FunctionalReferenceID>X00001209</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08112121211</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>TSETBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180305151425</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <ReleaseDateTime formatCode=""204"">20180305151425</ReleaseDateTime>
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

		public const string ClearedResponseFormatted =
@"[Inspections/Audit Requirements] Response for AirCargo ECI/ICR: X00001209

Inspection / Audit requirements
---------------------------------------------------------------------
AirCargo ECI/ICR : X00001209
ECI Number       : 56624546
Master Bill      : 081-12121211
Message No       : 2266

Message Status   : (B07) MPI Biosecurity Cargo Report Notification - consignment status as specified

Job Responses
---------------------------------------------------------------------
Job Number: X00001209-1   House Bill: TSETBILL1
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";

		#endregion

		#region X00001250

		public const string X00001250Response = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20180627150943</IssueDateTime>
    <FunctionalReferenceID>3194</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>94209793</ID>
        <AcceptanceDateTime formatCode=""204"">20180627150943</AcceptanceDateTime>
        <FunctionalReferenceID>X00001250</FunctionalReferenceID>
        <VersionID>3</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">IDR</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08111223343</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">IDR</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08111223343</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBILL2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">IDR</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08111223343</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBILL3</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180627150943</EffectiveDateTime>
      <NameCode>C06</NameCode>
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

		#region X00001580

		public const string X00001580BioResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20190726113418</IssueDateTime>
    <FunctionalReferenceID>6417</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MB 08112221112 - MOVEMENT HELD</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>24181441</ID>
        <AcceptanceDateTime formatCode=""204"">20190726113418</AcceptanceDateTime>
        <FunctionalReferenceID>X00001580</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ITA</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">ITA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08112221112</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ITD</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">ITD</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08112221112</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBILL2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190726113418</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <ReleaseDateTime formatCode=""204"">20190726113418</ReleaseDateTime>
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

		public const string X00001580NZCSResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20190726113432</IssueDateTime>
    <FunctionalReferenceID>6418</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MB 08112221112 - MOVEMENT APPROVED</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>24181441</ID>
        <AcceptanceDateTime formatCode=""204"">20190726113432</AcceptanceDateTime>
        <FunctionalReferenceID>X00001580</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ITA</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">ITA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08112221112</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ITD</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">ITD</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08112221112</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBILL2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190726113432</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20190726113432</ReleaseDateTime>
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

		#region X00001816

		public const string X00001816BioResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20200108124552</IssueDateTime>
    <FunctionalReferenceID>7919</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MB 08644568764 - MOVEMENT HELD</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>72285586</ID>
        <AcceptanceDateTime formatCode=""204"">20200108124552</AcceptanceDateTime>
        <FunctionalReferenceID>X00001816</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Wednesday, 8 January 2020 12:45:48 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08644568764</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>SYD1145</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20200108124552</EffectiveDateTime>
      <NameCode>B07</NameCode>
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

		public const string X00001816NZCSResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20200108124555</IssueDateTime>
    <FunctionalReferenceID>7920</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MB 08644568764 - MOVEMENT HELD</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>72285586</ID>
        <AcceptanceDateTime formatCode=""204"">20200108124555</AcceptanceDateTime>
        <FunctionalReferenceID>X00001816</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08644568764</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>SYD1145</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20200108124555</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20200108124555</ReleaseDateTime>
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

		#region X00002314 Cancel
		public const string airCargoCancellationMessage =
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
	<IssueDateTime formatCode=""204"">20231027112353</IssueDateTime>
    <FunctionalReferenceID>117</FunctionalReferenceID>
    <FunctionCode>34</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>90284777</ID>
        <FunctionalReferenceID>X00002314</FunctionalReferenceID>
        <VersionID>2</VersionID>
        <CancellationDateTime formatCode=""204"">20231027112353</CancellationDateTime>
		<Submitter>
		  <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20231027112353</EffectiveDateTime>
	  <NameCode>814</NameCode >
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

		#endregion
	}

	sealed class ICRMessageProcessorManifestingTest : TestCaseWithFactory
	{
		public void TestAcknowledgementResponse()
		{
			var icrMessage = Factory.New<TSWMessage>();
			icrMessage.EM_MessageType = "RES";
			icrMessage.EM_MessageText = AcknowledgementResponse;
			icrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrMessage);
			AssertEquals(cusEntryHeader, icrMessage.EM_LinkedObject);
			AssertEquals("EM_MessageSubType - Acknowledgement message", TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement, icrMessage.EM_MessageSubType);
			AssertEquals(AcknowledgementResponseFormatted, icrMessage.EM_MessageInterpretation);
			AssertEquals("55165469", cusEntryHeader.EntryNumber);
			AssertEquals("ACK", cusEntryHeader.CH_EntryStatus);
		}

		public void TestMPIBIOMessageResponse()
		{
			var icrMessage = Factory.New<TSWMessage>();
			icrMessage.EM_MessageType = "RES";
			icrMessage.EM_MessageText = BIOMessageResponse;
			icrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrMessage);
			AssertEquals(BIOMessageResponseFormatted, icrMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - response message from MPI Biosecurity", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrMessage.EM_MessageSubType);
			AssertEquals("55165469", cusEntryHeader.EntryNumber);
			AssertEquals("IAR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("ARP", cusEntryHeader.Declarations[0].JE_EntryStatus);
			AssertEquals("HLD", cusEntryHeader.Declarations[0].JE_ECI_LastResponseStatus);
			AssertEquals("91", cusEntryHeader.Declarations[0].JE_TSWCombinedStatus);
			AssertEquals("ARP", cusEntryHeader.Declarations[1].JE_EntryStatus);
			AssertEquals("HLD", cusEntryHeader.Declarations[1].JE_ECI_LastResponseStatus);
			AssertEquals("91", cusEntryHeader.Declarations[1].JE_TSWCombinedStatus);
		}

		public void TestWriteOffMessageResponse()
		{
			var icrMessage = Factory.New<TSWMessage>();
			icrMessage.EM_MessageType = "RES";
			icrMessage.EM_MessageText = WOFMessageResponse;
			icrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrMessage);
			AssertEquals(WOFMessageResponseFormatted, icrMessage.EM_MessageInterpretation);
			AssertEquals("EM_MessageSubType - response message from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, icrMessage.EM_MessageSubType);
			AssertEquals("55165469", cusEntryHeader.EntryNumber);
			AssertEquals("CLR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("ARP", cusEntryHeader.Declarations[0].JE_EntryStatus);
			AssertEquals("WOF", cusEntryHeader.Declarations[0].JE_ECI_LastResponseStatus);
			AssertEquals("09", cusEntryHeader.Declarations[0].JE_TSWCombinedStatus);
			AssertEquals("ARP", cusEntryHeader.Declarations[1].JE_EntryStatus);
			AssertEquals("WOF", cusEntryHeader.Declarations[1].JE_ECI_LastResponseStatus);
			AssertEquals("09", cusEntryHeader.Declarations[1].JE_TSWCombinedStatus);
		}

		public void TestCustomsAndBIOResponses()
		{
			// MPI BIO Response
			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = "RES";
			bioMessage.EM_MessageText = BIOMessageResponse;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(bioMessage);
			AssertEquals("EM_MessageSubType - response message from MPI Biosecurity", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, bioMessage.EM_MessageSubType);
			AssertEquals("55165469", cusEntryHeader.EntryNumber);
			AssertEquals("IAR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("ARP", cusEntryHeader.Declarations[0].JE_EntryStatus);
			AssertEquals("91", cusEntryHeader.Declarations[0].JE_TSWCombinedStatus);
			AssertEquals("ARP", cusEntryHeader.Declarations[1].JE_EntryStatus);
			AssertEquals("91", cusEntryHeader.Declarations[1].JE_TSWCombinedStatus);

			// NZCS Response
			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = "RES";
			nzcsMessage.EM_MessageText = WOFMessageResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			AssertEquals("EM_MessageSubType - response message from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, nzcsMessage.EM_MessageSubType);
			AssertEquals("55165469", cusEntryHeader.EntryNumber);
			AssertEquals("CLR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("Hold status on MPI Bio response trumps Clear/WOF status from Customs", "HLD", cusEntryHeader.Declarations[0].JE_EntryStatus);
			AssertEquals("NZCS - Written Off / BIO - Consignment Held", "01", cusEntryHeader.Declarations[0].JE_TSWCombinedStatus);
			AssertEquals("Hold status on MPI Bio response trumps Clear/WOF status from Customs", "HLD", cusEntryHeader.Declarations[1].JE_EntryStatus);
			AssertEquals("NZCS - Written Off / BIO - Consignment Held", "01", cusEntryHeader.Declarations[1].JE_TSWCombinedStatus);
		}

		public void TestBioSecondResponseUpdatesIndividualConsigmentsWithinManifest()
		{
			outgoingMessage.EM_ApplicationReference = "M00001136";
			cusEntryHeader.CH_BGMReference = "M00001136";

			cusEntryHeader.Declarations[0].JE_DeclarationReference = "M00001136-1";
			cusEntryHeader.Declarations[0].JE_MasterBill = "08187215295";
			cusEntryHeader.Declarations[0].JE_HouseBill = "190611HBILL1";

			cusEntryHeader.Declarations[1].JE_DeclarationReference = "M00001136-2";
			cusEntryHeader.Declarations[1].JE_MasterBill = "08187215295";
			cusEntryHeader.Declarations[1].JE_HouseBill = "190611HBILL2";

			var manifestCreator = new TestManifestCreator(cusEntryHeader);
			var declaration3 = manifestCreator.AddDeclaration();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration3.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration3.JE_DeclarationReference = "M00001136-3";
			declaration3.JE_MasterBill = "08187215295";
			declaration3.JE_HouseBill = "190611HBILL3";
			declaration3.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration3.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.PP;

			var declaration4 = manifestCreator.AddDeclaration();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration4.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration4.JE_DeclarationReference = "M00001136-4";
			declaration4.JE_MasterBill = "08187215295";
			declaration4.JE_HouseBill = "190611HBILL4";
			declaration4.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration4.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.PP;

			var declaration5 = manifestCreator.AddDeclaration();
			declaration5.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration5.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration5.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration5.JE_DeclarationReference = "M00001136-5";
			declaration5.JE_MasterBill = "08187215295";
			declaration5.JE_HouseBill = "190611HBILL5";
			declaration5.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration5.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.PP;

			var declaration6 = manifestCreator.AddDeclaration();
			declaration6.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration6.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration6.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration6.JE_DeclarationReference = "M00001136-6";
			declaration6.JE_MasterBill = "08187215295";
			declaration6.JE_HouseBill = "190611HBILL6";
			declaration6.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration6.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.PP;

			var icrMessage = Factory.New<TSWMessage>();
			icrMessage.EM_MessageType = "RES";
			icrMessage.EM_MessageText = M00001136Acknowledgement;
			icrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrMessage);
			AssertEquals(cusEntryHeader, icrMessage.EM_LinkedObject);
			AssertEquals("EM_MessageSubType - Acknowledgement message", TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement, icrMessage.EM_MessageSubType);
			AssertEquals("96827342", cusEntryHeader.EntryNumber);
			AssertEquals("ACK", cusEntryHeader.CH_EntryStatus);

			//First Biosecurity response
			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = "RES";
			bioMessage.EM_MessageText = BIOFirstResponse;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(bioMessage);
			AssertEquals("EM_MessageSubType - response message from MPI Biosecurity", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, bioMessage.EM_MessageSubType);
			AssertEquals("96827342", cusEntryHeader.EntryNumber);
			AssertEquals("IAR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("ARP", cusEntryHeader.Declarations[0].JE_EntryStatus);
			AssertEquals("91", cusEntryHeader.Declarations[0].JE_TSWCombinedStatus);
			AssertEquals("ARP", cusEntryHeader.Declarations[1].JE_EntryStatus);
			AssertEquals("91", cusEntryHeader.Declarations[1].JE_TSWCombinedStatus);
			AssertEquals("ARP", declaration3.JE_EntryStatus);
			AssertEquals("91", declaration3.JE_TSWCombinedStatus);
			AssertEquals("ARP", declaration4.JE_EntryStatus);
			AssertEquals("91", declaration4.JE_TSWCombinedStatus);
			AssertEquals("ARP", declaration5.JE_EntryStatus);
			AssertEquals("91", declaration5.JE_TSWCombinedStatus);
			AssertEquals("ARP", declaration6.JE_EntryStatus);
			AssertEquals("91", declaration6.JE_TSWCombinedStatus);

			// NZCS Response
			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = "RES";
			nzcsMessage.EM_MessageText = NZCustomsResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			AssertEquals("EM_MessageSubType - response message from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, nzcsMessage.EM_MessageSubType);
			AssertEquals("96827342", cusEntryHeader.EntryNumber);
			AssertEquals("CLR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("Hold status on MPI Bio response trumps Clear/WOF status from Customs", "HLD", cusEntryHeader.Declarations[0].JE_EntryStatus);
			AssertEquals("NZCS - Written Off / BIO - Consignment Held", "01", cusEntryHeader.Declarations[0].JE_TSWCombinedStatus);
			AssertEquals("Hold status on MPI Bio response trumps Clear/WOF status from Customs", "HLD", cusEntryHeader.Declarations[1].JE_EntryStatus);
			AssertEquals("NZCS - Written Off / BIO - Consignment Held", "01", cusEntryHeader.Declarations[1].JE_TSWCombinedStatus);
			AssertEquals("HLD", declaration3.JE_EntryStatus);
			AssertEquals("01", declaration3.JE_TSWCombinedStatus);
			AssertEquals("HLD", declaration4.JE_EntryStatus);
			AssertEquals("01", declaration4.JE_TSWCombinedStatus);
			AssertEquals("HLD", declaration5.JE_EntryStatus);
			AssertEquals("01", declaration5.JE_TSWCombinedStatus);
			AssertEquals("HLD", declaration6.JE_EntryStatus);
			AssertEquals("01", declaration6.JE_TSWCombinedStatus);

			//Second Biosecurity response
			bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = "RES";
			bioMessage.EM_MessageText = BIOSecondResponse;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(bioMessage);
			AssertEquals("EM_MessageSubType - second response message from MPI Biosecurity", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, bioMessage.EM_MessageSubType);
			AssertEquals("96827342", cusEntryHeader.EntryNumber);
			AssertEquals("IAR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("WOF", cusEntryHeader.Declarations[0].JE_EntryStatus);
			AssertEquals("00", cusEntryHeader.Declarations[0].JE_TSWCombinedStatus);
			AssertEquals("WOF", cusEntryHeader.Declarations[1].JE_EntryStatus);
			AssertEquals("00", cusEntryHeader.Declarations[1].JE_TSWCombinedStatus);
			AssertEquals("HLD", declaration3.JE_EntryStatus);
			AssertEquals("01", declaration3.JE_TSWCombinedStatus);
			AssertEquals("HLD", declaration4.JE_EntryStatus);
			AssertEquals("01", declaration4.JE_TSWCombinedStatus);
			AssertEquals("MDR", declaration5.JE_EntryStatus);
			AssertEquals("03", declaration5.JE_TSWCombinedStatus);
			AssertEquals("MDR", declaration6.JE_EntryStatus);
			AssertEquals("03", declaration6.JE_TSWCombinedStatus);
		}

		public void TestITRStatusForConsigmentsWithinManifest()
		{
			var transitDestOrg = Factory.New<OrgHeader>();
			transitDestOrg.OH_Code = "AIRNZ";

			var transitDestOrgAddr = transitDestOrg.Addresses.AddNew();
			transitDestOrgAddr.OA_Address1 = "1001 Airport Drive";
			transitDestOrgAddr.OA_Code = "ANZTransit";

			outgoingMessage.EM_ApplicationReference = "M00001141";
			cusEntryHeader.CH_BGMReference = "M00001141";

			var declaration1 = cusEntryHeader.Declarations[0];
			declaration1.JE_DeclarationReference = "M00001141-1";
			declaration1.JE_MasterBill = "081M00001141";
			declaration1.JE_HouseBill = "BKG190627HBL1";
			TranshipmentRequest.Create(declaration1);
			declaration1.TranshipmentRequest.C4_ModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Road;
			declaration1.TranshipmentRequest.C4_TranshipModeOfMovement = TranshipmentRequestModeOfMovement.Codes.Air;
			declaration1.TranshipmentRequest.C4_OA_DestinationAddress = transitDestOrgAddr.PK;

			var declaration2 = cusEntryHeader.Declarations[1];
			declaration2.JE_DeclarationReference = "M00001141-2";
			declaration2.JE_MasterBill = "081M00001141";
			declaration2.JE_HouseBill = "BKG190628HBL1";

			var icrMessage = Factory.New<TSWMessage>();
			icrMessage.EM_MessageType = "RES";
			icrMessage.EM_MessageText = M00001141Acknowledgement;
			icrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrMessage);
			AssertEquals(cusEntryHeader, icrMessage.EM_LinkedObject);
			AssertEquals("EM_MessageSubType - Acknowledgement message", TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement, icrMessage.EM_MessageSubType);
			AssertEquals("38445867", cusEntryHeader.EntryNumber);
			AssertEquals("ACK", cusEntryHeader.CH_EntryStatus);

			// NZCS Response
			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = "RES";
			nzcsMessage.EM_MessageText = M00001141NZCResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			AssertEquals("EM_MessageSubType - response message from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, nzcsMessage.EM_MessageSubType);
			AssertEquals("38445867", cusEntryHeader.EntryNumber);
			AssertEquals("CLR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("Pending status on MPI Bio response trumps ITA status from Customs at this time", "ARP", declaration1.JE_EntryStatus);
			AssertEquals("NZCS - ITA / BIO - Consignment Pending", "A9", declaration1.JE_TSWCombinedStatus);
			AssertEquals("Pending status on MPI Bio response trumps Clear/WOF status from Customs", "ARP", declaration2.JE_EntryStatus);
			AssertEquals("NZCS - Written Off / BIO - Consignment Pending", "09", declaration2.JE_TSWCombinedStatus);

			// Biosecurity response
			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = "RES";
			bioMessage.EM_MessageText = M00001141BIOResponse;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(bioMessage);
			AssertEquals("EM_MessageSubType - response message from MPI Biosecurity", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, bioMessage.EM_MessageSubType);
			AssertEquals("38445867", cusEntryHeader.EntryNumber);
			AssertEquals("IAR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("ITA", declaration1.JE_EntryStatus);
			AssertEquals("AA", declaration1.JE_TSWCombinedStatus);
			AssertEquals("HLD", declaration2.JE_EntryStatus);
			AssertEquals("01", declaration2.JE_TSWCombinedStatus);
		}

		public void TestManifestEntryBioHeldThenCleared()
		{
			var entryHeader = Factory.New<ManifestingCusEntryHeader>();
			var manifestCreator = new TestManifestCreator(entryHeader);
			var jobDeclaration = manifestCreator.AddDeclaration();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			jobDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			jobDeclaration.JE_MasterBill = "176";
			jobDeclaration.JE_HouseBill = "N18T1Z7DJ89";
			jobDeclaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			jobDeclaration.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.PP;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "M00003840";
			outgoingMessage.EM_LinkedObject = entryHeader;
			entryHeader.CH_BGMReference = "M00003840";

			var icrCustomsMessage = Factory.New<TSWMessage>();
			icrCustomsMessage.EM_MessageType = "RES";
			icrCustomsMessage.EM_MessageText = CustomsWOF;
			icrCustomsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrCustomsMessage);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, icrCustomsMessage.EM_MessageSubType);
			AssertEquals("Customs status reported - no BIO response yet", "ARP", jobDeclaration.JE_EntryStatus);
			AssertEquals("No customs status reported - just a BIO response", "Agency Response Pending", jobDeclaration.JE_EntryStatusDescription);

			var icrBioHeldMessage = Factory.New<TSWMessage>();
			icrBioHeldMessage.EM_MessageType = "RES";
			icrBioHeldMessage.EM_MessageText = BioHeld;
			icrBioHeldMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioHeldMessage);
			AssertEquals("EM_MessageSubType - response message comes from MPI Bio", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBioHeldMessage.EM_MessageSubType);
			AssertEquals("No customs status reported - just a BIO response", "HLD", jobDeclaration.JE_EntryStatus);
			AssertEquals("Entry Status held", "Consignment Held", jobDeclaration.JE_EntryStatusDescription);

			var icrBioWOFMessage = Factory.New<TSWMessage>();
			icrBioWOFMessage.EM_MessageType = "RES";
			icrBioWOFMessage.EM_MessageText = BioWOF;
			icrBioWOFMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioWOFMessage);
			AssertEquals("EM_MessageSubType - response message comes from MPI Bio", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBioWOFMessage.EM_MessageSubType);
			AssertEquals("No customs status reported - just a BIO response", "WOF", jobDeclaration.JE_EntryStatus);
			AssertEquals("Entry Status WOF", "Consignment Written Off/Cleared", jobDeclaration.JE_EntryStatusDescription);
		}

		public void TestTNTManifestStatusIssue()
		{
			outgoingMessage.EM_ApplicationReference = "M00119782";
			cusEntryHeader.CH_BGMReference = "M00119782";

			cusEntryHeader.Declarations[0].JE_DeclarationReference = "M00119782-1";
			cusEntryHeader.Declarations[0].JE_MasterBill = "02317786226";
			cusEntryHeader.Declarations[0].JE_HouseBill = "145684815";

			cusEntryHeader.Declarations[1].JE_DeclarationReference = "M00119782-2";
			cusEntryHeader.Declarations[1].JE_MasterBill = "02317786226";
			cusEntryHeader.Declarations[1].JE_HouseBill = "151202747";

			var manifestCreator = new TestManifestCreator(cusEntryHeader);
			var declaration3 = manifestCreator.AddDeclaration();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration3.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration3.JE_DeclarationReference = "M00119782-3";
			declaration3.JE_MasterBill = "02317786226";
			declaration3.JE_HouseBill = "152017579";
			declaration3.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration3.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.PP;

			var declaration4 = manifestCreator.AddDeclaration();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration4.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration4.JE_DeclarationReference = "M00119782-4";
			declaration4.JE_MasterBill = "02317786226";
			declaration4.JE_HouseBill = "152020785";
			declaration4.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration4.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.PP;

			var declaration5 = manifestCreator.AddDeclaration();
			declaration5.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration5.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration5.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration5.JE_DeclarationReference = "M00119782-5";
			declaration5.JE_MasterBill = "02317786226";
			declaration5.JE_HouseBill = "152025116";
			declaration5.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration5.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.PP;

			var declaration6 = manifestCreator.AddDeclaration();
			declaration6.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration6.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration6.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration6.JE_DeclarationReference = "M00119782-6";
			declaration6.JE_MasterBill = "02317786226";
			declaration6.JE_HouseBill = "152026712";
			declaration6.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration6.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.PP;

			//First Biosecurity response
			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = "RES";
			bioMessage.EM_MessageText = M00119782BIOFirstResponse;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(bioMessage);
			AssertEquals("EM_MessageSubType - response message from MPI Biosecurity", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, bioMessage.EM_MessageSubType);
			AssertEquals("89474101", cusEntryHeader.EntryNumber);
			AssertEquals("IAR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("ARP", cusEntryHeader.Declarations[0].JE_EntryStatus);
			AssertEquals("91", cusEntryHeader.Declarations[0].JE_TSWCombinedStatus);
			AssertEquals("ARP", cusEntryHeader.Declarations[1].JE_EntryStatus);
			AssertEquals("91", cusEntryHeader.Declarations[1].JE_TSWCombinedStatus);
			AssertEquals("ARP", declaration3.JE_EntryStatus);
			AssertEquals("91", declaration3.JE_TSWCombinedStatus);
			AssertEquals("ARP", declaration4.JE_EntryStatus);
			AssertEquals("91", declaration4.JE_TSWCombinedStatus);
			AssertEquals("ARP", declaration5.JE_EntryStatus);
			AssertEquals("91", declaration5.JE_TSWCombinedStatus);
			AssertEquals("ARP", declaration6.JE_EntryStatus);
			AssertEquals("91", declaration6.JE_TSWCombinedStatus);

			// NZCS Response
			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = "RES";
			nzcsMessage.EM_MessageText = M00119782NZCustomsResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			AssertEquals("EM_MessageSubType - response message from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, nzcsMessage.EM_MessageSubType);
			AssertEquals("89474101", cusEntryHeader.EntryNumber);
			AssertEquals("ERR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("Hold status on MPI Bio response trumps Clear/WOF status from Customs", "HLD", cusEntryHeader.Declarations[0].JE_EntryStatus);
			AssertEquals("NZCS - Written Off / BIO - Consignment Held", "01", cusEntryHeader.Declarations[0].JE_TSWCombinedStatus);

			AssertEquals("Hold status on MPI Bio response trumps Clear/WOF status from Customs", "HLD", cusEntryHeader.Declarations[1].JE_EntryStatus);
			AssertEquals("NZCS - Written Off / BIO - Consignment Held", "01", cusEntryHeader.Declarations[1].JE_TSWCombinedStatus);

			AssertEquals("HLD", declaration3.JE_ManifestBioStatus);
			AssertEquals("ERR", declaration3.JE_ManifestNZCSStatus);
			AssertEquals("ERR", declaration3.JE_EntryStatus);
			AssertEquals("71", declaration3.JE_TSWCombinedStatus);

			AssertEquals("HLD", declaration4.JE_ManifestBioStatus);
			AssertEquals("WOF", declaration4.JE_ManifestNZCSStatus);
			AssertEquals("HLD", declaration4.JE_EntryStatus);
			AssertEquals("01", declaration4.JE_TSWCombinedStatus);

			AssertEquals("HLD", declaration5.JE_ManifestBioStatus);
			AssertEquals("ERR", declaration5.JE_ManifestNZCSStatus);
			AssertEquals("ERR", declaration5.JE_EntryStatus);
			AssertEquals("71", declaration5.JE_TSWCombinedStatus);

			AssertEquals("HLD", declaration6.JE_ManifestBioStatus);
			AssertEquals("WOF", declaration6.JE_ManifestNZCSStatus);
			AssertEquals("HLD", declaration6.JE_EntryStatus);
			AssertEquals("01", declaration6.JE_TSWCombinedStatus);

			//Second Biosecurity response
			bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = "RES";
			bioMessage.EM_MessageText = M00119782BIOSecondResponse;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(bioMessage);
			AssertEquals("EM_MessageSubType - second response message from MPI Biosecurity", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, bioMessage.EM_MessageSubType);
			AssertEquals("89474101", cusEntryHeader.EntryNumber);
			AssertEquals("IAR", cusEntryHeader.CH_EntryStatus);

			var declaration1 = cusEntryHeader.Declarations[0];
			AssertEquals("WOF", declaration1.JE_ManifestBioStatus);
			AssertEquals("WOF", declaration1.JE_ManifestNZCSStatus);
			AssertEquals("WOF", declaration1.JE_EntryStatus);
			AssertEquals("00", declaration1.JE_TSWCombinedStatus);

			var declaration2 = cusEntryHeader.Declarations[1];
			AssertEquals("WOF", declaration2.JE_ManifestBioStatus);
			AssertEquals("WOF", declaration2.JE_ManifestNZCSStatus);
			AssertEquals("WOF", declaration2.JE_EntryStatus);
			AssertEquals("00", declaration2.JE_TSWCombinedStatus);

			AssertEquals("WOF", declaration3.JE_ManifestBioStatus);
			AssertEquals("ERR", declaration3.JE_ManifestNZCSStatus);
			AssertEquals("ERR - this declaration still has the NZCS error", "ERR", declaration3.JE_EntryStatus);
			AssertEquals("70", declaration3.JE_TSWCombinedStatus);

			AssertEquals("HLD", declaration4.JE_ManifestBioStatus);
			AssertEquals("WOF", declaration4.JE_ManifestNZCSStatus);
			AssertEquals("HLD - this declaration still has Bio Hold status", "HLD", declaration4.JE_EntryStatus);
			AssertEquals("01", declaration4.JE_TSWCombinedStatus);

			AssertEquals("MDR", declaration5.JE_ManifestBioStatus);
			AssertEquals("ERR", declaration5.JE_ManifestNZCSStatus);
			AssertEquals("ERR - this declaration still has the NZCS error as highest order of status", "ERR", declaration5.JE_EntryStatus);
			AssertEquals("73", declaration5.JE_TSWCombinedStatus);

			AssertEquals("MDR", declaration6.JE_ManifestBioStatus);
			AssertEquals("WOF", declaration6.JE_ManifestNZCSStatus);
			AssertEquals("MDR", declaration6.JE_EntryStatus);
			AssertEquals("03", declaration6.JE_TSWCombinedStatus);
		}

		public void TestManifestChangedToFormal()
		{
			outgoingMessage.EM_ApplicationReference = "M00121638";
			outgoingMessage.EM_MessageText = M00121638Original;
			cusEntryHeader.CH_BGMReference = "M00121638";

			var declaration1 = cusEntryHeader.Declarations[0];
			declaration1.JE_DeclarationReference = "M00121638-1";
			declaration1.JE_MasterBill = "61877950670";
			declaration1.JE_HouseBill = "117895725";

			var declaration2 = cusEntryHeader.Declarations[1];
			declaration2.JE_DeclarationReference = "M00121638-2";
			declaration2.JE_MasterBill = "61877950670";
			declaration2.JE_HouseBill = "121713251";

			var manifestCreator = new TestManifestCreator(cusEntryHeader);
			var declaration3 = manifestCreator.AddDeclaration();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration3.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration3.JE_DeclarationReference = "M00121638-3";
			declaration3.JE_MasterBill = "61877950670";
			declaration3.JE_HouseBill = "121713574";
			declaration3.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration3.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.PP;

			//First Biosecurity response
			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = "RES";
			bioMessage.EM_MessageText = M00121638BIOFirstResponse;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(bioMessage);
			AssertEquals("EM_MessageSubType - response message from MPI Biosecurity", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, bioMessage.EM_MessageSubType);
			AssertEquals("81539585", cusEntryHeader.EntryNumber);
			AssertEquals("IAR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("ARP", declaration1.JE_EntryStatus);
			AssertEquals("91", declaration1.JE_TSWCombinedStatus);
			AssertEquals("Bio Status", "HLD", declaration1.CusEntryHeader.CH_MPIBioStatus);
			AssertEquals("ARP", declaration2.JE_EntryStatus);
			AssertEquals("91", declaration2.JE_TSWCombinedStatus);
			AssertEquals("Bio Status", "HLD", declaration2.CusEntryHeader.CH_MPIBioStatus);
			AssertEquals("ARP", declaration3.JE_EntryStatus);
			AssertEquals("91", declaration3.JE_TSWCombinedStatus);
			AssertEquals("Bio Status", "HLD", declaration3.CusEntryHeader.CH_MPIBioStatus);

			var impedimentLogDec2 = declaration2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceivedCode)).First();
			AssertNotNull("impedimentLog Event", impedimentLogDec2);
			AssertEquals("impedimentLog ref", "HLD - BIO", impedimentLogDec2.SL_Reference);

			var impedimentLogDec3 = declaration3.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceivedCode)).First();
			AssertNotNull("impedimentLog Event", impedimentLogDec3);
			AssertEquals("impedimentLog ref", "HLD - BIO", impedimentLogDec3.SL_Reference);

			// NZCS Response
			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = "RES";
			nzcsMessage.EM_MessageText = M00121638NZCustomsResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			AssertEquals("EM_MessageSubType - response message from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, nzcsMessage.EM_MessageSubType);
			AssertEquals("81539585", cusEntryHeader.EntryNumber);
			AssertEquals("ERR", cusEntryHeader.CH_EntryStatus);
			AssertEquals("Hold status on MPI Bio response trumps Clear/WOF status from Customs", "HLD", declaration1.JE_EntryStatus);
			AssertEquals("NZCS - Written Off / BIO - Consignment Held", "01", declaration1.JE_TSWCombinedStatus);

			AssertEquals("HLD", declaration2.JE_ManifestBioStatus);
			AssertEquals("ERR", declaration2.JE_ManifestNZCSStatus);
			AssertEquals("Error status on Customs response trumps HLD status from MPI BIO", "ERR", declaration2.JE_EntryStatus);
			AssertEquals("NZCS - Error / BIO - Consignment Held", "71", declaration2.JE_TSWCombinedStatus);

			AssertEquals("HLD", declaration3.JE_ManifestBioStatus);
			AssertEquals("ERR", declaration3.JE_ManifestNZCSStatus);
			AssertEquals("ERR", declaration3.JE_EntryStatus);
			AssertEquals("71", declaration3.JE_TSWCombinedStatus);

			//Second Biosecurity response
			bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = "RES";
			bioMessage.EM_MessageText = M00121638BIOSecondResponse;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(bioMessage);
			AssertEquals("EM_MessageSubType - second response message from MPI Biosecurity", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, bioMessage.EM_MessageSubType);
			AssertEquals("81539585", cusEntryHeader.EntryNumber);

			//declaration1 = cusEntryHeader.Declarations[0];
			AssertEquals("WOF", declaration1.JE_ManifestBioStatus);
			AssertEquals("WOF", declaration1.JE_ManifestNZCSStatus);
			AssertEquals("WOF", declaration1.JE_EntryStatus);
			AssertEquals("00", declaration1.JE_TSWCombinedStatus);
			AssertEquals("Bio Status", "WOF", declaration1.CusEntryHeader.CH_MPIBioStatus);

			//declaration2 = cusEntryHeader.Declarations[1];
			AssertEquals("WOF", declaration2.JE_ManifestBioStatus);
			AssertEquals("ERR", declaration2.JE_ManifestNZCSStatus);
			AssertEquals("ERR", declaration2.JE_EntryStatus);
			AssertEquals("70", declaration2.JE_TSWCombinedStatus);
			AssertEquals("Bio Status", "WOF", declaration2.CusEntryHeader.CH_MPIBioStatus);

			AssertEquals("WOF", declaration3.JE_ManifestBioStatus);
			AssertEquals("ERR", declaration3.JE_ManifestNZCSStatus);
			AssertEquals("ERR - this declaration still has the NZCS error", "ERR", declaration3.JE_EntryStatus);
			AssertEquals("70", declaration3.JE_TSWCombinedStatus);
			AssertEquals("Bio Status", "WOF", declaration3.CusEntryHeader.CH_MPIBioStatus);

			var customsCompletedLog = declaration1.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsClearedCode)).First();
			AssertNotNull("customsCompletedLog cleared Event", customsCompletedLog);
			AssertEquals("customsCompletedLog ref", "NZ Import", customsCompletedLog.SL_Reference);

			StmALog[] impedimentLogsDec2 = declaration2.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceivedCode));
			var dec2ImpedimentLog1 = impedimentLogsDec2[0];
			var dec2ImpedimentLog2 = impedimentLogsDec2[1];
			AssertNotNull("impedimentLog1", dec2ImpedimentLog1);
			AssertNotNull("impedimentLog2", dec2ImpedimentLog2);
			AssertEquals("impedimentLog ref", "HLD - BIO", dec2ImpedimentLog1.SL_Reference);
			AssertEquals("impedimentLog ref", "CLR - BIO", dec2ImpedimentLog2.SL_Reference);

			StmALog[] impedimentLogsDec3 = declaration3.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceivedCode));
			var dec3ImpedimentLog1 = impedimentLogsDec3[0];
			var dec3ImpedimentLog2 = impedimentLogsDec3[1];
			AssertNotNull("impedimentLog1", dec3ImpedimentLog1);
			AssertNotNull("impedimentLog2", dec3ImpedimentLog2);
			AssertEquals("impedimentLog ref", "HLD - BIO", dec3ImpedimentLog1.SL_Reference);
			AssertEquals("impedimentLog ref", "CLR - BIO", dec3ImpedimentLog2.SL_Reference);

			// Second NZCS Response
			// Users have changed declarations that were in error to Formal entries now:
			declaration2.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;

			nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = "RES";
			nzcsMessage.EM_MessageText = M00121638NZCustomsSecondResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			AssertEquals("EM_MessageSubType - response message from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, nzcsMessage.EM_MessageSubType);
			AssertEquals("81539585", cusEntryHeader.EntryNumber);
			AssertEquals("ERR", cusEntryHeader.CH_EntryStatus);

			AssertEquals("WOF", declaration1.JE_ManifestBioStatus);
			AssertEquals("WOF", declaration1.JE_ManifestNZCSStatus);
			AssertEquals("WOF", declaration1.JE_EntryStatus);
			AssertEquals("00", declaration1.JE_TSWCombinedStatus);

			// Statuses for declarations that are no longer part of the manifest should not be updated by the manifest response, but should have been reset when changing type
			// declaration2 has been changed to a formal entry, processing of manifest response should therefore not change the current declaration statuses.
			AssertEquals("", declaration2.JE_ManifestBioStatus);
			AssertEquals("", declaration2.JE_ManifestNZCSStatus);
			AssertEquals("NSC", declaration2.JE_EntryStatus);
			AssertEquals("", declaration2.JE_TSWCombinedStatus);

			AssertEquals("WOF", declaration3.JE_ManifestBioStatus);
			AssertEquals("ERR", declaration3.JE_ManifestNZCSStatus);
			AssertEquals("ERR", declaration3.JE_EntryStatus);
			AssertEquals("70", declaration3.JE_TSWCombinedStatus);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			cusEntryHeader = Factory.New<ManifestingCusEntryHeader>();
			var manifestCreator = new TestManifestCreator(cusEntryHeader);
			var declaration1 = manifestCreator.AddDeclaration();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration1.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration1.JE_MasterBill = "08100239245";
			declaration1.JE_HouseBill = "Y203448";
			declaration1.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration1.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.PP;

			var declaration2 = manifestCreator.AddDeclaration();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration2.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration2.JE_MasterBill = "08100239245";
			declaration2.JE_HouseBill = "U203498";
			declaration2.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;
			declaration2.JE_TSWCombinedStatus = LowValueConsignmentStatusList.Codes.PP;

			outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "M00001058";
			outgoingMessage.EM_LinkedObject = cusEntryHeader;
			cusEntryHeader.CH_BGMReference = "M00001058";
		}

		ManifestingCusEntryHeader cusEntryHeader;
		TSWMessage outgoingMessage;

		#region Messages

		#region Acknowledgement

		public const string AcknowledgementResponse =
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
    <IssueDateTime formatCode=""204"">20180404192344</IssueDateTime>
    <FunctionalReferenceID>2550</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>55165469</ID>
        <FunctionalReferenceID>M00001058</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20180404192344</EffectiveDateTime>
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

		public const string AcknowledgementResponseFormatted = "[Acknowledgement] Response for ECI Manifest: M00001058";

		#endregion // Acknowledgement

		#region BIO Message

		public const string BIOMessageResponse =
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
    <IssueDateTime formatCode=""204"">20180404192353</IssueDateTime>
    <FunctionalReferenceID>2551</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>55165469</ID>
        <AcceptanceDateTime formatCode=""204"">20180404192353</AcceptanceDateTime>
        <FunctionalReferenceID>M00001058</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription><![CDATA[seq:1,instructions:'To be held pending further instructions by MPI',issuedDate:'Wednesday, 4 April 2018 7:23:47 p.m.']]></StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08100239245</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>Y203448</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription><![CDATA[seq:1,instructions:'To be held pending further instructions by MPI',issuedDate:'Wednesday, 4 April 2018 7:23:47 p.m.']]></StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08100239245</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>U203498</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180404192353</EffectiveDateTime>
      <NameCode>B07</NameCode>
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

		public const string BIOMessageResponseFormatted =
@"[Inspections/Audit Requirements] Response for ECI Manifest: M00001058

Clearance / Acceptance Instructions
---------------------------------------------------------------------
ECI Manifest   : M00001058
Entry Number   : 55165469
Master Bill    : 081-00239245
Message No     : 2551

Message Status : (B07) MPI Biosecurity Cargo Report Notification - consignment status as specified

Summary        : 0 out of 2 Jobs have been Written Off.

Customs Instructions
---------------------------------------------------------------------
'To be held pending further instructions by MPI',issuedDate:'Wednesday, 4 April 2018 7:23:47 p.m.'
'To be held pending further instructions by MPI',issuedDate:'Wednesday, 4 April 2018 7:23:47 p.m.'

Job Responses
---------------------------------------------------------------------
Job Number: M01010101-1   House Bill: Y203448
--- Clearance Status: HLD-Consignment Held ---

Job Number: M01010101-2   House Bill: U203498
--- Clearance Status: HLD-Consignment Held ---
";

		#endregion // BIO Message

		#region WOF Message

		public const string WOFMessageResponse =
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
    <IssueDateTime formatCode=""204"">20180404192402</IssueDateTime>
    <FunctionalReferenceID>2552</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>55165469</ID>
        <AcceptanceDateTime formatCode=""204"">20180404192402</AcceptanceDateTime>
        <FunctionalReferenceID>M00001058</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08100239245</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>Y203448</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08100239245</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>U203498</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180404192402</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20180404192402</ReleaseDateTime>
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

		public const string WOFMessageResponseFormatted =
@"[ICR/CRE Accepted, Check Consignments for Status] Response for ECI Manifest: M00001058

Clearance / Acceptance Instructions
---------------------------------------------------------------------
ECI Manifest   : M00001058
Entry Number   : 55165469
Master Bill    : 081-00239245
Message No     : 2552

Message Status : (C06) Customs Cargo Report Notification - consignment status as specified

Summary        : 2 out of 2 Jobs have been Written Off.

Job Responses
---------------------------------------------------------------------
Job Number: M01010101-1   House Bill: Y203448
--- Clearance Status: WOF-Consignment Written Off/Cleared ---

Job Number: M01010101-2   House Bill: U203498
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";

		#endregion // WOF Message

		#region M00001136 Test Messages

		#region Acknowledgement

		public const string M00001136Acknowledgement =
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
    <IssueDateTime formatCode=""204"">20190611150047</IssueDateTime>
    <FunctionalReferenceID>5937</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>96827342</ID>
        <FunctionalReferenceID>M00001136</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20190611150047</EffectiveDateTime>
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

		#region BIO Messages

		public const string BIOFirstResponse =
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
    <IssueDateTime formatCode=""204"">20190611150058</IssueDateTime>
    <FunctionalReferenceID>5938</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>96827342</ID>
        <AcceptanceDateTime formatCode=""204"">20190611150058</AcceptanceDateTime>
        <FunctionalReferenceID>M00001136</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 11 June 2019 3:00:55 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 11 June 2019 3:00:55 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 11 June 2019 3:00:55 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL3</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>4</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 11 June 2019 3:00:55 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL4</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>5</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 11 June 2019 3:00:55 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL5</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>6</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 11 June 2019 3:00:55 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL6</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190611150058</EffectiveDateTime>
      <NameCode>B07</NameCode>
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

		public const string BIOSecondResponse =
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
    <IssueDateTime formatCode=""204"">20190611164152</IssueDateTime>
    <FunctionalReferenceID>5940</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>96827342</ID>
        <AcceptanceDateTime formatCode=""204"">20190611164152</AcceptanceDateTime>
        <FunctionalReferenceID>M00001136</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 11 June 2019 4:41:49 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL3</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>4</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 11 June 2019 4:41:49 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL4</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">MDR</GoodsStatusCode>
          <SequenceNumeric>5</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL5</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">MDR</GoodsStatusCode>
          <SequenceNumeric>6</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL6</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190611164152</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <ReleaseDateTime formatCode=""204"">20190611164152</ReleaseDateTime>
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

		#region NZCs Message

		public const string NZCustomsResponse =
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
    <IssueDateTime formatCode=""204"">20190611150105</IssueDateTime>
    <FunctionalReferenceID>5939</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>96827342</ID>
        <AcceptanceDateTime formatCode=""204"">20190611150105</AcceptanceDateTime>
        <FunctionalReferenceID>M00001136</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL3</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>4</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL4</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>5</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL5</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>6</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08187215295</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>190611HBILL6</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190611150105</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20190611150105</ReleaseDateTime>
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

		#endregion // WOF Message

		#endregion

		#region M00001141 Test Messages

		#region Acknowledgement

		public const string M00001141Acknowledgement =
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
    <IssueDateTime formatCode=""204"">20190628123308</IssueDateTime>
    <FunctionalReferenceID>6100</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>38445867</ID>
        <FunctionalReferenceID>M00001141</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20190628123308</EffectiveDateTime>
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

		#region BIO Message

		public const string M00001141BIOResponse =
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
    <IssueDateTime formatCode=""204"">20190628123325</IssueDateTime>
    <FunctionalReferenceID>6102</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MB 08152528582 - MOVEMENT HELD</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>38445867</ID>
        <AcceptanceDateTime formatCode=""204"">20190628123325</AcceptanceDateTime>
        <FunctionalReferenceID>M00001141</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ITA</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">ITA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08152528582</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>BKG190627HBL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Friday, 28 June 2019 12:33:21 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08152528582</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>BKG190628HBL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190628123325</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <ReleaseDateTime formatCode=""204"">20190628123325</ReleaseDateTime>
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

		#region NZCs Message

		public const string M00001141NZCResponse =
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
    <IssueDateTime formatCode=""204"">20190628123324</IssueDateTime>
    <FunctionalReferenceID>6101</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MB 08152528582 - MOVEMENT HELD</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>38445867</ID>
        <AcceptanceDateTime formatCode=""204"">20190628123324</AcceptanceDateTime>
        <FunctionalReferenceID>M00001141</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ITA</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">ITA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08152528582</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>BKG190627HBL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08152528582</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>BKG190628HBL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190628123324</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20190628123324</ReleaseDateTime>
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

		#endregion // WOF Message

		#endregion

		#region Bio Held Then Cleared

		const string CustomsWOF = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>40336042C</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20190809102010</IssueDateTime>
    <FunctionalReferenceID>456907</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>11593985</ID>
        <AcceptanceDateTime formatCode=""204"">20190809102010</AcceptanceDateTime>
        <FunctionalReferenceID>M00003840</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>40336042C</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>176</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>N18T1Z7DJ89</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190809102010</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20190809102010</ReleaseDateTime>
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

		const string BioHeld = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>40336042C</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20190809102013</IssueDateTime>
    <FunctionalReferenceID>456908</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>11593985</ID>
        <AcceptanceDateTime formatCode=""204"">20190809102013</AcceptanceDateTime>
        <FunctionalReferenceID>M00003840</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>40336042C</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:Held,issuedDate:Friday, 9 August 2019 10:20:10 AM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>176</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>N18T1Z7DJ89</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190809102013</EffectiveDateTime>
      <NameCode>B07</NameCode>
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

		const string BioWOF = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>40336042C</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20190809120408</IssueDateTime>
    <FunctionalReferenceID>457227</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>11593985</ID>
        <AcceptanceDateTime formatCode=""204"">20190809120408</AcceptanceDateTime>
        <FunctionalReferenceID>M00003840</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>40336042C</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>176</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>N18T1Z7DJ89</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190809120408</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <ReleaseDateTime formatCode=""204"">20190809120408</ReleaseDateTime>
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

		#region M00119782 Test Messages

		#region BIO Messages

		public const string M00119782BIOFirstResponse =
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
    <IssueDateTime formatCode=""204"">20200222173611</IssueDateTime>
    <FunctionalReferenceID>5938</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>89474101</ID>
        <AcceptanceDateTime formatCode=""204"">20200222173611</AcceptanceDateTime>
        <FunctionalReferenceID>M00119782</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 11 June 2019 3:00:55 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>145684815</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 11 June 2019 3:00:55 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>151202747</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 11 June 2019 3:00:55 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>152017579</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>4</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 11 June 2019 3:00:55 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>152020785</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>5</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 11 June 2019 3:00:55 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>152025116</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>6</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 11 June 2019 3:00:55 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>152026712</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20200222173611</EffectiveDateTime>
      <NameCode>B07</NameCode>
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

		public const string M00119782BIOSecondResponse =
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
    <IssueDateTime formatCode=""204"">20200222173611</IssueDateTime>
    <FunctionalReferenceID>5940</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>89474101</ID>
        <AcceptanceDateTime formatCode=""204"">20200222173611</AcceptanceDateTime>
        <FunctionalReferenceID>M00119782</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>145684815</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>151202747</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>152017579</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>4</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:To be held pending further instructions by MPI,issuedDate:Tuesday, 11 June 2019 4:41:49 PM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>152020785</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">MDR</GoodsStatusCode>
          <SequenceNumeric>5</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>152025116</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">MDR</GoodsStatusCode>
          <SequenceNumeric>6</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>152026712</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20200222173611</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <ReleaseDateTime formatCode=""204"">20200222173611</ReleaseDateTime>
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

		#region NZCs Message

		public const string M00119782NZCustomsResponse =
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
    <IssueDateTime formatCode=""204"">20200222173611</IssueDateTime>
    <FunctionalReferenceID>5939</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>89474101</ID>
        <AcceptanceDateTime formatCode=""204"">20200222173611</AcceptanceDateTime>
        <FunctionalReferenceID>M00119782</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>145684815</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>151202747</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ERR</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>152017579</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>4</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>152020785</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ERR</GoodsStatusCode>
          <SequenceNumeric>5</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>152025116</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>6</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>02317786226</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>152026712</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20200222173611</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20200222173611</ReleaseDateTime>
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

		#region M00121638 Event Issue Test Messages

		public const string M00121638Original = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRI</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>ICR</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>ICR</TypeCode>
  <FunctionalReferenceID>M00121638</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <Submitter>
    <ID>40085811L</ID>
  </Submitter>
  <BorderTransportMeans>
    <Name>SQ7294</Name>
    <TypeCode>4</TypeCode>
    <ArrivalDateTime formatCode=""102"">20200628</ArrivalDateTime>
    <FirstArrivalLocationID>NZAKL</FirstArrivalLocationID>
  </BorderTransportMeans>
  <Carrier>
    <Name>SINGAPORE AIRLINES</Name>
  </Carrier>
  <Consignment>
    <SequenceNumeric>1</SequenceNumeric>
    <ValueAmount currencyID=""NZD"">368.13</ValueAmount>
    <AdditionalInformation>
      <StatementCode>Y</StatementCode>
      <StatementTypeCode>WOF</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementDescription>2421400,FEDEX EXPRESS NEW ZEALAND</StatementDescription>
      <StatementTypeCode>MAC</StatementTypeCode>
    </AdditionalInformation>
    <AssociatedTransportDocument>
      <ID>61877950670</ID>
      <TypeCode>MB</TypeCode>
    </AssociatedTransportDocument>
    <Consignee>
      <Name>FARMER JON S ORGANICS</Name>
      <Address>
        <CityName>TE AROHA</CityName>
        <CountryCode>NZ</CountryCode>
        <CountrySubDivisionName>WAIKATO REGION</CountrySubDivisionName>
        <Line>20 POOLES ROAD</Line>
        <PostcodeID>3320</PostcodeID>
      </Address>
      <Communication>
        <ID>64276006568</ID>
        <TypeID>TE</TypeID>
      </Communication>
    </Consignee>
    <ConsignmentItem>
      <SequenceNumeric>1</SequenceNumeric>
      <Commodity>
        <CargoDescription>CHAINPOTS</CargoDescription>
        <ValueAmount currencyID=""AUD"">335.00</ValueAmount>
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">6.94</GrossMassMeasure>
      </GoodsMeasure>
      <Origin>
        <CountryCode>AU</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>1</QuantityQuantity>
        <TypeCode>PK</TypeCode>
      </Packaging>
    </ConsignmentItem>
    <Consignor>
      <Name>ACTIVE VISTA</Name>
      <Address>
        <CityName>LONGLEY</CityName>
        <CountryCode>AU</CountryCode>
        <Line>1690 HUON ROAD</Line>
        <PostcodeID>7150</PostcodeID>
      </Address>
      <Communication>
        <ID>6142799586</ID>
        <TypeID>TE</TypeID>
      </Communication>
    </Consignor>
    <Freight>
      <PaymentMethodCode />
    </Freight>
    <GoodsConsignedPlace>
      <ID>AUHBA</ID>
    </GoodsConsignedPlace>
    <GoodsLocation>
      <ID>13967L</ID>
    </GoodsLocation>
    <LoadingLocation>
      <ID>AUMEL</ID>
    </LoadingLocation>
    <TransportContractDocument>
      <ID>117895725</ID>
      <TypeCode>HWB</TypeCode>
    </TransportContractDocument>
    <UnloadingLocation>
      <ID>NZAKL</ID>
      <ArrivalDateTime formatCode=""203"">202006280410</ArrivalDateTime>
    </UnloadingLocation>
  </Consignment>
  <Consignment>
    <SequenceNumeric>2</SequenceNumeric>
    <ValueAmount currencyID=""NZD"">406.58</ValueAmount>
    <AdditionalInformation>
      <StatementCode>Y</StatementCode>
      <StatementTypeCode>WOF</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementDescription>2421400,FEDEX EXPRESS NEW ZEALAND</StatementDescription>
      <StatementTypeCode>MAC</StatementTypeCode>
    </AdditionalInformation>
    <AssociatedTransportDocument>
      <ID>61877950670</ID>
      <TypeCode>MB</TypeCode>
    </AssociatedTransportDocument>
    <Consignee>
      <Name>RESIDENCE OF ROBYN AND KERRY BARTOSH</Name>
      <Address>
        <CityName>HASTINGS</CityName>
        <CountryCode>NZ</CountryCode>
        <CountrySubDivisionName>HKB</CountrySubDivisionName>
        <Line>7 PALMER PLACE PARKVALE</Line>
        <PostcodeID>4122</PostcodeID>
      </Address>
      <Communication>
        <ID>61412059919</ID>
        <TypeID>TE</TypeID>
      </Communication>
    </Consignee>
    <ConsignmentItem>
      <SequenceNumeric>1</SequenceNumeric>
      <Commodity>
        <CargoDescription>SHEET SETS HAIR CUTTER TIRE INFLA</CargoDescription>
        <ValueAmount currencyID=""AUD"">369.99</ValueAmount>
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">75</GrossMassMeasure>
      </GoodsMeasure>
      <Origin>
        <CountryCode>AU</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>3</QuantityQuantity>
        <TypeCode>PK</TypeCode>
      </Packaging>
    </ConsignmentItem>
    <Consignor>
      <Name>KERRY BARTOSH C/- TNT HALLAM</Name>
      <Address>
        <CityName>HALLAM</CityName>
        <CountryCode>AU</CountryCode>
        <Line>34-38 WEDGEWOOD ROAD</Line>
        <PostcodeID>3803</PostcodeID>
      </Address>
      <Communication>
        <ID>61412059919</ID>
        <TypeID>TE</TypeID>
      </Communication>
    </Consignor>
    <Freight>
      <PaymentMethodCode />
    </Freight>
    <GoodsConsignedPlace>
      <ID>AUHLM</ID>
    </GoodsConsignedPlace>
    <GoodsLocation>
      <ID>13967L</ID>
    </GoodsLocation>
    <LoadingLocation>
      <ID>AUMEL</ID>
    </LoadingLocation>
    <TransportContractDocument>
      <ID>121713251</ID>
      <TypeCode>HWB</TypeCode>
    </TransportContractDocument>
    <UnloadingLocation>
      <ID>NZAKL</ID>
      <ArrivalDateTime formatCode=""203"">202006280410</ArrivalDateTime>
    </UnloadingLocation>
  </Consignment>
  <Consignment>
    <SequenceNumeric>3</SequenceNumeric>
    <ValueAmount currencyID=""NZD"">549.45</ValueAmount>
    <AdditionalInformation>
      <StatementCode>Y</StatementCode>
      <StatementTypeCode>WOF</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementDescription>2421400,FEDEX EXPRESS NEW ZEALAND</StatementDescription>
      <StatementTypeCode>MAC</StatementTypeCode>
    </AdditionalInformation>
    <AssociatedTransportDocument>
      <ID>61877950670</ID>
      <TypeCode>MB</TypeCode>
    </AssociatedTransportDocument>
    <Consignee>
      <Name>FLASHBAY NZ LTD</Name>
      <Address>
        <CityName>AUCKLAND</CityName>
        <CountryCode>NZ</CountryCode>
        <CountrySubDivisionName>AUK</CountrySubDivisionName>
        <Line>FLASHBAY NZ LTD   *** USE ""FLASNZAKL"" **** CLIENT CODE   40329262B</Line>
        <PostcodeID>0600</PostcodeID>
      </Address>
      <Communication>
        <ID>1800621978</ID>
        <TypeID>TE</TypeID>
      </Communication>
    </Consignee>
    <ConsignmentItem>
      <SequenceNumeric>1</SequenceNumeric>
      <Commodity>
        <CargoDescription>BOOLSTERS AND FACE PADS</CargoDescription>
        <ValueAmount currencyID=""AUD"">500.00</ValueAmount>
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">5</GrossMassMeasure>
      </GoodsMeasure>
      <Origin>
        <CountryCode>AU</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>1</QuantityQuantity>
        <TypeCode>PK</TypeCode>
      </Packaging>
    </ConsignmentItem>
    <Consignor>
      <Name>ATHLEGEN</Name>
      <Address>
        <CityName>ALFREDTON</CityName>
        <CountryCode>AU</CountryCode>
        <Line>8 PRODUCTION DRIVE</Line>
        <PostcodeID>3350</PostcodeID>
      </Address>
      <Communication>
        <ID>0353376977</ID>
        <TypeID>TE</TypeID>
      </Communication>
    </Consignor>
    <Freight>
      <PaymentMethodCode />
    </Freight>
    <GoodsConsignedPlace>
      <ID>AUMEL</ID>
    </GoodsConsignedPlace>
    <GoodsLocation>
      <ID>13967L</ID>
    </GoodsLocation>
    <LoadingLocation>
      <ID>AUMEL</ID>
    </LoadingLocation>
    <TransportContractDocument>
      <ID>121713574</ID>
      <TypeCode>HWB</TypeCode>
    </TransportContractDocument>
    <UnloadingLocation>
      <ID>NZAKL</ID>
      <ArrivalDateTime formatCode=""203"">202006280410</ArrivalDateTime>
    </UnloadingLocation>
  </Consignment>
  <Declarant>
    <ID>40525155J</ID>
    <Communication>
      <ID>harpreet.aulakh@tnt.co.nz</ID>
      <TypeID>EM</TypeID>
    </Communication>
  </Declarant>
</Declaration>
</DocumentMetadata>";

		#region BIO Messages

		public const string M00121638BIOFirstResponse =
 @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>40085811L</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20200627100134</IssueDateTime>
    <FunctionalReferenceID>815471</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>81539585</ID>
        <AcceptanceDateTime formatCode=""204"">20200627100134</AcceptanceDateTime>
        <FunctionalReferenceID>M00121638</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>40085811L</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:Held,issuedDate:Saturday, 27 June 2020 10:01:31 AM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>61877950670</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>117895725</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:Held,issuedDate:Saturday, 27 June 2020 10:01:31 AM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>61877950670</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>121713251</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription>seq:1,instructions:Held,issuedDate:Saturday, 27 June 2020 10:01:31 AM</StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>61877950670</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>121713574</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20200627100134</EffectiveDateTime>
      <NameCode>B07</NameCode>
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

		public const string M00121638BIOSecondResponse =
@"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>40085811L</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20200627100228</IssueDateTime>
    <FunctionalReferenceID>815475</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>81539585</ID>
        <AcceptanceDateTime formatCode=""204"">20200627100228</AcceptanceDateTime>
        <FunctionalReferenceID>M00121638</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>40085811L</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>61877950670</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>117895725</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>61877950670</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>121713251</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>61877950670</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>121713574</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20200627100228</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <ReleaseDateTime formatCode=""204"">20200627100228</ReleaseDateTime>
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

		#region NZCs Message

		public const string M00121638NZCustomsResponse =
@"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>40085811L</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20200627100152</IssueDateTime>
    <FunctionalReferenceID>815472</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>81539585</ID>
        <AcceptanceDateTime formatCode=""204"">20200627100152</AcceptanceDateTime>
        <FunctionalReferenceID>M00121638</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>40085811L</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>61877950670</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>117895725</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ERR</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>61877950670</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>121713251</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ERR</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>61877950670</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>121713574</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>199</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>4</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>29A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>21A</DocumentSectionCode>
        <TagID>145</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>199</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>3</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>29A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>21A</DocumentSectionCode>
        <TagID>145</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>199</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>41</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>29A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>21A</DocumentSectionCode>
        <TagID>145</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>199</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>36</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>29A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>21A</DocumentSectionCode>
        <TagID>145</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>199</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>12</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>29A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>21A</DocumentSectionCode>
        <TagID>145</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>199</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>30</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>29A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>21A</DocumentSectionCode>
        <TagID>145</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>199</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>25</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>29A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>21A</DocumentSectionCode>
        <TagID>145</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>199</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>23</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>29A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>21A</DocumentSectionCode>
        <TagID>145</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>199</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>27</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>29A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>21A</DocumentSectionCode>
        <TagID>145</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>199</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>28</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>29A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>21A</DocumentSectionCode>
        <TagID>145</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>199</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>29</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>29A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>21A</DocumentSectionCode>
        <TagID>145</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>199</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>18</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>29A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>21A</DocumentSectionCode>
        <TagID>145</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>199</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>2</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>29A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>21A</DocumentSectionCode>
        <TagID>145</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>199</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>13</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>29A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>21A</DocumentSectionCode>
        <TagID>145</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>199</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>26</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>29A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>21A</DocumentSectionCode>
        <TagID>145</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>199</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>34</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>29A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>21A</DocumentSectionCode>
        <TagID>145</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20200627100152</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20200627100152</ReleaseDateTime>
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

		public const string M00121638NZCustomsSecondResponse =
@"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>40085811L</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20200629175150</IssueDateTime>
    <FunctionalReferenceID>818740</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>81539585</ID>
        <AcceptanceDateTime formatCode=""204"">20200629175150</AcceptanceDateTime>
        <FunctionalReferenceID>M00121638</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>40085811L</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>61877950670</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>117895725</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ERR</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>61877950670</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>121713251</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ERR</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>61877950670</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>121713574</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20200629175150</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20200629175150</ReleaseDateTime>
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

		#endregion // Messages

		#endregion // Implementation
	}

	sealed class ICRMessageProcessorCusSCAOceanBillTest : TestCaseWithFactory
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestWriteOffMessageResponse()
		{
			oceanBill.CB_CustomsStatus = LowValueManifestStatusList.Codes.SentToCustoms;
			houseBill.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.PC;
			var icrMessage = Factory.New<TSWMessage>();
			icrMessage.EM_MessageType = "RES";
			icrMessage.EM_MessageText = File.ReadAllText(Path.Combine(BaseSourcePath, @"Enterprise\Product\Operations\Customs\NZ\Core\Business.Test\TradeSingleWindow\WriteOff\TestFiles\ICRSeaCargoWriteOffResponse.txt"));
			icrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			var logEntries = houseBill.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrMessage);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, icrMessage.EM_MessageSubType);
			AssertEquals("7435668", oceanBill.EntryNumber);
			AssertEquals("CLR", oceanBill.CB_CustomsStatus);
			AssertEquals("Consignment Status, NZCS WOF", LowValueConsignmentStatusList.Codes.CC, houseBill.CA_ShipmentStatus);
			AssertEquals("Consignment Status", "Consignment Written Off/Cleared", houseBill.ShipmentStatusDescription);
			Assert("Consignment Status, IsWrittenOff", houseBill.IsWrittenOff);

			var messageWrittenOffEvent = oceanBill.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageAcceptedCode)).First();
			AssertNotNull("messageWrittenOffEvent", messageWrittenOffEvent);
			AssertEquals("messageWrittenOffEvent = MessageAccepted", "REG", messageWrittenOffEvent.SL_Reference);
			logEntries = houseBill.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry event has been created for this cleared write-off entry", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
		}

		public void TestCombinedStatusForClearedFullSuiteResponse()
		{
			oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill.CB_MessageReference = "X00001209";
			oceanBill.CB_OceanBill = "08112121211";
			oceanBill.CB_RL_NKPortOfLoading = "AUSYD";
			oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
			oceanBill.CB_ApplicationCode = JobApplicationCodeList.Codes.TSW;

			houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_HouseBill = "TSETBILL1";
			houseBill.CA_ConsignmentNum = 1;
			houseBill.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "X00001209";
			outgoingMessage.EM_LinkedObject = oceanBill;

			var logEntries = houseBill.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Log Entry created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code));

			#region Acknowledgement Response

			var icrACKMessage = Factory.New<TSWMessage>();
			icrACKMessage.EM_MessageType = "RES";
			icrACKMessage.EM_MessageText = ACKResponse;
			icrACKMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrACKMessage);
			AssertEquals(oceanBill, icrACKMessage.EM_LinkedObject);
			AssertEquals("EM_MessageSubType - Acknowledgement message", TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement, icrACKMessage.EM_MessageSubType);
			AssertEquals("56624546", oceanBill.EntryNumber);
			AssertEquals("ACK", oceanBill.CB_CustomsStatus);
			AssertEquals("Acknowledgement", oceanBill.CustomsStatusDescription);

			AssertEquals("Consignment Status is not affected by ACK message", "STC", houseBill.CA_ShipmentStatus);

			#endregion

			#region Initial BIO Held Response

			var icrBioMessage = Factory.New<TSWMessage>();
			icrBioMessage.EM_MessageType = "RES";
			icrBioMessage.EM_MessageText = BIOHeldResponse;
			icrBioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioMessage);
			AssertEquals(oceanBill, icrBioMessage.EM_LinkedObject);
			AssertEquals("EM_MessageSubType - MPI BIO message", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBioMessage.EM_MessageSubType);
			AssertEquals("56624546", oceanBill.EntryNumber);
			AssertEquals("IAR", oceanBill.CB_CustomsStatus);
			AssertEquals("Inspections/Audit Requirements", oceanBill.CustomsStatusDescription);

			AssertEquals("Consignment Status - Customs Pending / BIO Held", LowValueConsignmentStatusList.Codes.PH, houseBill.CA_ShipmentStatus);
			AssertEquals("Consignment Status", "NZCS - Pending / BIO - Consignment Held", houseBill.ShipmentStatusDescription);
			logEntries = houseBill.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Impediment Log Entry for MPI Bio has been created for this held write-off", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("HLD - BIO")));

			#endregion

			#region Customs Write-off Response

			var icrMessage = Factory.New<TSWMessage>();
			icrMessage.EM_MessageType = "RES";
			icrMessage.EM_MessageText = CustomsWOFResponse;
			icrMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrMessage);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, icrMessage.EM_MessageSubType);
			AssertEquals("56624546", oceanBill.EntryNumber);
			AssertEquals("CLR", oceanBill.CB_CustomsStatus);
			AssertEquals("Consignment Status, NZCS WOF - Bio Held", LowValueConsignmentStatusList.Codes.CH, houseBill.CA_ShipmentStatus);
			AssertEquals("Consignment Status - Customs WOF", "NZCS - Consignment Written Off / BIO - Consignment Held", houseBill.ShipmentStatusDescription);

			var messageAcceptedEvent = oceanBill.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageAcceptedCode));
			AssertNotNull("messageAcceptedEvent - MessageAccepted", messageAcceptedEvent);

			#endregion

			#region Subsequent BIO Write-off Response

			var bioClearMessage = Factory.New<TSWMessage>();
			bioClearMessage.EM_MessageType = "RES";
			bioClearMessage.EM_MessageText = BIOClearedResponse;
			bioClearMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(bioClearMessage);
			AssertEquals("56624546", oceanBill.EntryNumber);
			AssertEquals("CLR", oceanBill.CB_CustomsStatus);

			AssertEquals("Consignment Status, NZCS WOF - Bio now WOF as well", LowValueConsignmentStatusList.Codes.CC, houseBill.CA_ShipmentStatus);
			AssertEquals("Consignment Status - written off", "Consignment Written Off/Cleared", houseBill.ShipmentStatusDescription);

			messageAcceptedEvent = oceanBill.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageAcceptedCode));
			AssertNotNull("messageAcceptedEvent", messageAcceptedEvent);
			//AssertEquals("messageAcceptedEvent - MessageAccepted", "REG", messageAcceptedEvent.SL_Reference);

			logEntries = houseBill.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsImpedimentReceived.Code));
			AssertEquals("Cleared Impediment Log Entry has been updated for houseBill", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsImpedimentReceived.Code && le.referenceFreeText.Value.Contains("CLR - BIO")));

			logEntries = houseBill.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry event has been created for the cleared customs response", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));

			#endregion
		}

		public void TestMovementStatusResponsesForOceanBill()
		{
			var oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill.CB_MessageReference = "X00001580";
			oceanBill.CB_OceanBill = "08112221112";
			oceanBill.CB_RL_NKPortOfLoading = "AUSYD";
			oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
			oceanBill.CB_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;

			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_HouseBill = "HBILL1";
			houseBill.CA_ConsignmentNum = 1;
			var houseBill1ITR = TranshipmentRequest.Create(houseBill);
			houseBill1ITR.C4_ModeOfMovement = "4";
			houseBill.CA_MessageStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var houseBill2 = oceanBill.HouseBills.AddNew();
			houseBill2.CA_HouseBill = "HBILL2";
			houseBill2.CA_ConsignmentNum = 2;
			var houseBill2ITR = TranshipmentRequest.Create(houseBill2);
			houseBill2ITR.C4_ModeOfMovement = "1";
			houseBill2.CA_MessageStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "X00001580";
			outgoingMessage.EM_LinkedObject = oceanBill;

			#region Bio Response

			var icrBioResponse = Factory.New<TSWMessage>();
			icrBioResponse.EM_MessageType = "RES";
			icrBioResponse.EM_MessageText = X00001580BioResponse;
			icrBioResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(icrBioResponse);
			AssertEquals(oceanBill, icrBioResponse.EM_LinkedObject);
			AssertEquals("EM_MessageSubType - MPI BIO message", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, icrBioResponse.EM_MessageSubType);
			AssertEquals("24181441", oceanBill.EntryNumber);
			AssertEquals("CLR", oceanBill.CB_CustomsStatus);
			AssertEquals("ICR/CRE Accepted, Check Consignments for Status", oceanBill.CustomsStatusDescription);

			AssertEquals("Consignment1 Status - Customs Pending / BIO ITR Approved", LowValueConsignmentStatusList.Codes.PA, houseBill.CA_ShipmentStatus);
			AssertEquals("Consignment1 Status", "NZCS - Pending / BIO - ITR Approved", houseBill.ShipmentStatusDescription);
			AssertEquals("Consignment1 Bio Movement Status", "ITA", houseBill.CA_BioMovementStatus);
			AssertEquals("TranshipmentRequest.C4_Status", CombinedMovementStatus.Codes.PI, houseBill.TranshipmentRequest.C4_Status);
			AssertEquals("TranshipmentRequest MovementStatusDesc", "NZCS Transhipment Request Pending / MPI Biosecurity ITR Approved", houseBill.TranshipmentRequest.MovementStatusDesc);

			AssertEquals("Consignment2 Status - Customs Pending / BIO ITR Declined", LowValueConsignmentStatusList.Codes.PT, houseBill2.CA_ShipmentStatus);
			AssertEquals("Consignment2 Status", "NZCS - Pending / BIO - Transhipment Declined", houseBill2.ShipmentStatusDescription);
			AssertEquals("CS_BioMovementStatus", "ITD", houseBill2.CA_BioMovementStatus);
			AssertEquals("TranshipmentRequest.C4_Status", CombinedMovementStatus.Codes.PX, houseBill2.TranshipmentRequest.C4_Status);
			AssertEquals("TranshipmentRequest MovementStatusDesc", "NZCS Transhipment Request Pending / MPI Biosecurity ITR Declined", houseBill2.TranshipmentRequest.MovementStatusDesc);

			#endregion

			#region Customs Response

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = "RES";
			nzcsMessage.EM_MessageText = X00001580NZCSResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, nzcsMessage.EM_MessageSubType);
			AssertEquals("24181441", oceanBill.EntryNumber);
			AssertEquals("CLR", oceanBill.CB_CustomsStatus);
			AssertEquals("Consignment Status, ITA Approved", LowValueConsignmentStatusList.Codes.AA, houseBill.CA_ShipmentStatus);
			AssertEquals("Consignment Status - ITA", "International Transhipment Approved", houseBill.ShipmentStatusDescription);
			AssertEquals("Consignment1 Bio Movement Status", "ITA", houseBill.CA_CustomsMovementStatus);
			AssertEquals("TranshipmentRequest.C4_Status", CombinedMovementStatus.Codes.II, houseBill.TranshipmentRequest.C4_Status);
			AssertEquals("TranshipmentRequest MovementStatusDesc", "ITR Approved", houseBill.TranshipmentRequest.MovementStatusDesc);

			AssertEquals("Consignment2 Status - ITR declined", LowValueConsignmentStatusList.Codes.TT, houseBill2.CA_ShipmentStatus);
			AssertEquals("Consignment2 Status", "Transhipment Declined", houseBill2.ShipmentStatusDescription);
			AssertEquals("Consignment1 Bio Movement Status", "ITD", houseBill2.CA_CustomsMovementStatus);
			AssertEquals("TranshipmentRequest.C4_Status", CombinedMovementStatus.Codes.XX, houseBill2.TranshipmentRequest.C4_Status);
			AssertEquals("TranshipmentRequest MovementStatusDesc", "ITR Declined", houseBill2.TranshipmentRequest.MovementStatusDesc);

			AssertEquals(X00001580NZCSMovementResponseFormatted, nzcsMessage.EM_MessageInterpretation);

			#endregion
		}

		public void TestICRCancellationResponseForSeaCargo()
		{
			oceanBill.Logs.RemoveAndDeleteAll();
			TestCaseHelper.ClearTable(EDIMessageSchema.Constants.TableName);
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00009090";
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

			oceanBill.CB_MessageReference = "X00002315";
			houseBill.CA_JS = shipment.PK;
			houseBill.CA_IsHVLV = true;
			houseBill.CA_RL_NK_PortOfDestination = "NZAKL";
			houseBill.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.AA;
			Factory.Save();

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "X00002315";
			outgoingMessage.EM_Status = EDIMessage.Status.Sent;
			outgoingMessage.EM_MessageSubType = NZ.TradeSingleWindow.MessageSubTypeList.Codes.Cancellation;
			outgoingMessage.EM_LinkedObject = oceanBill;

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = "RES";
			nzcsMessage.EM_MessageText = seaCargoCancellationMessage;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			Factory.Save();

			AssertEquals("EM_MessageSubType - response message comes from NZ Customs", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, nzcsMessage.EM_MessageSubType);
			AssertEquals("94011135", oceanBill.EntryNumber);
			AssertEquals("CAN", oceanBill.CB_CustomsStatus);
			AssertEquals("Consignment Status", LowValueConsignmentStatusList.Codes.ConsignmentCancelled, houseBill.CA_ShipmentStatus);
			AssertEquals("Consignment Status Description", "Consignment Cancelled", houseBill.ShipmentStatusDescription);

			var query = new ZQuery(StmALogSchema.SL_Parent, houseBill.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code);
			var cESEventLogs = Factory.Load<StmALog>(query);
			AssertEquals("One CES log entry", 1, cESEventLogs.Length);
			AssertEquals("SER included in Free text", "|LOC=NZAKL|SER=PCS|TYP=HLD", cESEventLogs[0].SL_Reference);
		}

		public const string X00001580NZCSMovementResponseFormatted =
@"[ICR/CRE Accepted, Check Consignments for Status] Response for SeaCargo ICR/CRE: X00001580

Clearance / Acceptance Instructions
---------------------------------------------------------------------
SeaCargo ICR/CRE : X00001580
Entry Number     : 24181441
Master Bill      : 08112221112
Message No       : 6418

Message Status   : (C06) Customs Cargo Report Notification - consignment status as specified

Summary          : 0 out of 2 Jobs have been Written Off.

Customs Instructions
---------------------------------------------------------------------
MB 08112221112 - MOVEMENT APPROVED

Job Responses
---------------------------------------------------------------------
Job Number: X00001580-1   House Bill: HBILL1
--- Clearance Status: ITA-International Transhipment Approved ---
--- Movement Status: ITA-International Transhipment Approved ---

Job Number: X00001580-2   House Bill: HBILL2
--- Clearance Status: ITD-International Transhipment Declined ---
--- Movement Status: ITD-International Transhipment Declined ---
";

		#region Implementation

		CusSCAOceanBill oceanBill;
		CusSCAHouse houseBill;

		protected override void SetUp()
		{
			base.SetUp();
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			oceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
			oceanBill.CB_MessageReference = "X00001215";
			oceanBill.CB_OceanBill = "MOBL123456";
			oceanBill.CB_RL_NKPortOfLoading = "AUSYD";
			oceanBill.CB_RL_NKPortOfDischarge = "NZAKL";
			oceanBill.CB_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;

			houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_HouseBill = "Y202498";
			houseBill.CA_ConsignmentNum = 1;
			houseBill.CA_ShipmentStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "X00001215";
			outgoingMessage.EM_LinkedObject = oceanBill;
		}

		#region Messages

		#region X00001215

		#region Acknowledgement

		public const string AcknowledgementResponse =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20180328125652</IssueDateTime>
    <FunctionalReferenceID>2523</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>7435668</ID>
        <FunctionalReferenceID>X00001215</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20180328125652</EffectiveDateTime>
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

		public const string AcknowledgementResponseFormatted = "[Acknowledgement] Response for AirCargo ECI/ICR: X00001215";

		#endregion

		#region MPI BIO Hold

		public const string BIOResponseFormatted = @"[Inspections/Audit Requirements] Response for AirCargo ECI/ICR: X00001215

Clearance / Acceptance Instructions
---------------------------------------------------------------------
AirCargo ECI/ICR : X00001215
ECI Number       : 7435668
Master Bill      : 081-00428245
Message No       : 2524

Message Status   : (B07) MPI Biosecurity Cargo Report Notification - consignment status as specified

Customs Instructions
---------------------------------------------------------------------
To be held pending further instructions by MPI'
,issuedDate:'
Wednesday, 28 March 2018 12:56:58 p.m.'


Job Responses
---------------------------------------------------------------------
Job Number: X00001215-1   House Bill: Y202498
--- Clearance Status: HLD-Consignment Held ---";

		#endregion

		#region WriteOff

		public const string WriteOffResponseFormatted =
@"[ECI Accepted, Check Consignments for Status] Response for AirCargo ECI/ICR: X00001215

Clearance / Acceptance Instructions
---------------------------------------------------------------------
AirCargo ECI/ICR : X00001215
ECI Number       : 7435668
Master Bill      : 081-00428245
Message No       : 2525

Message Status   : (C06) Customs Cargo Report Notification - consignment status as specified

Job Responses
---------------------------------------------------------------------
Job Number: X00001215-1   House Bill: Y202498
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";

		#endregion

		#endregion

		#region X00001209 Cleared

		public const string ACKResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20180305103129</IssueDateTime>
    <FunctionalReferenceID>2259</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>56624546</ID>
        <FunctionalReferenceID>X00001209</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20180305103129</EffectiveDateTime>
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

		public const string BIOHeldResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20180305103135</IssueDateTime>
    <FunctionalReferenceID>2260</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>56624546</ID>
        <AcceptanceDateTime formatCode=""204"">20180305103135</AcceptanceDateTime>
        <FunctionalReferenceID>X00001209</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08112121211</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>TSETBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180305103135</EffectiveDateTime>
      <NameCode>B07</NameCode>
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

		public const string CustomsWOFResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20180305103142</IssueDateTime>
    <FunctionalReferenceID>2261</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>56624546</ID>
        <AcceptanceDateTime formatCode=""204"">20180305103142</AcceptanceDateTime>
        <FunctionalReferenceID>X00001209</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08112121211</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>TSETBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180305103142</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20180305103142</ReleaseDateTime>
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

		public const string BIOClearedResponse =
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
    <IssueDateTime formatCode=""204"">20180305151425</IssueDateTime>
    <FunctionalReferenceID>2266</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>56624546</ID>
        <AcceptanceDateTime formatCode=""204"">20180305151425</AcceptanceDateTime>
        <FunctionalReferenceID>X00001209</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08112121211</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>TSETBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180305151425</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <ReleaseDateTime formatCode=""204"">20180305151425</ReleaseDateTime>
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

		public const string seaCargoCancellationMessage =
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
    <IssueDateTime formatCode=""204"">20231109014835</IssueDateTime>
    <FunctionalReferenceID>126</FunctionalReferenceID>
    <FunctionCode>34</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>94011135</ID>
        <FunctionalReferenceID>X00002315</FunctionalReferenceID>
        <VersionID>2</VersionID>
        <CancellationDateTime formatCode=""204"">20231109014835</CancellationDateTime>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20231109014835</EffectiveDateTime>
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

		public const string ClearedResponseFormatted =
@"[Inspections/Audit Requirements] Response for AirCargo ECI/ICR: X00001209

Inspection / Audit requirements
---------------------------------------------------------------------
AirCargo ECI/ICR : X00001209
ECI Number       : 56624546
Master Bill      : 081-12121211
Message No       : 2266

Message Status   : (B07) MPI Biosecurity Cargo Report Notification - consignment status as specified

Job Responses
---------------------------------------------------------------------
Job Number: X00001209-1   House Bill: TSETBILL1
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";

		#endregion

		#region X00001250

		public const string X00001250Response = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20180627150943</IssueDateTime>
    <FunctionalReferenceID>3194</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>94209793</ID>
        <AcceptanceDateTime formatCode=""204"">20180627150943</AcceptanceDateTime>
        <FunctionalReferenceID>X00001250</FunctionalReferenceID>
        <VersionID>3</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">IDR</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08111223343</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">IDR</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08111223343</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBILL2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">IDR</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08111223343</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBILL3</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180627150943</EffectiveDateTime>
      <NameCode>C06</NameCode>
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

		#region X00001580

		public const string X00001580BioResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20190726113418</IssueDateTime>
    <FunctionalReferenceID>6417</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MB 08112221112 - MOVEMENT HELD</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>24181441</ID>
        <AcceptanceDateTime formatCode=""204"">20190726113418</AcceptanceDateTime>
        <FunctionalReferenceID>X00001580</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ITA</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">ITA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08112221112</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ITD</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">ITD</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08112221112</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBILL2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190726113418</EffectiveDateTime>
      <NameCode>B07</NameCode>
      <ReleaseDateTime formatCode=""204"">20190726113418</ReleaseDateTime>
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

		public const string X00001580NZCSResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20190726113432</IssueDateTime>
    <FunctionalReferenceID>6418</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MB 08112221112 - MOVEMENT APPROVED</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>24181441</ID>
        <AcceptanceDateTime formatCode=""204"">20190726113432</AcceptanceDateTime>
        <FunctionalReferenceID>X00001580</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ITA</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">ITA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08112221112</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBILL1</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">ITD</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">ITD</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08112221112</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>HBILL2</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190726113432</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20190726113432</ReleaseDateTime>
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

		#endregion
	}
}
