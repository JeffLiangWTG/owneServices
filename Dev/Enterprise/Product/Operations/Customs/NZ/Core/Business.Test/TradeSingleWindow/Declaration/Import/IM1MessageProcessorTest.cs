using System;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.MessageProcessors;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	public class IM1MessageProcessorTest : TestCaseWithFactory
	{
		public void TestErrorResponse()
		{
			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var im1Message = Factory.New<TSWMessage>();
			im1Message.EM_MessageType = "RES";
			im1Message.EM_MessageText = ErrorResponse;
			im1Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(im1Message);
			AssertEquals(header, im1Message.EM_LinkedObject);
			AssertEquals(ErrorResponseFormatted, im1Message.EM_MessageInterpretation);
		}

		public void TestCancelResponse()
		{
			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var im1Message = Factory.New<TSWMessage>();
			im1Message.EM_MessageType = "RES";
			im1Message.EM_MessageText = CancelResponse;
			im1Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(im1Message);
			AssertEquals(header, im1Message.EM_LinkedObject);
			AssertEquals(CancelResponseFormatted, im1Message.EM_MessageInterpretation);
		}

		public void TestEntryIsNotCancelledJustOnAnyCancelSubmissionResponse()
		{
			outgoingMessage.EM_ApplicationReference = "B05002717";
			header.CH_Status = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			header.Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			header.Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCI;
			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var im1Message1 = Factory.New<TSWMessage>();
			im1Message1.EM_MessageType = "RES";
			im1Message1.EM_MessageText = InvalidCancelResponse1;
			im1Message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(im1Message1);
			AssertEquals(header, im1Message1.EM_LinkedObject);
			AssertEquals(FormalEntryStatusList.Codes.InspectionsAuditRequirements, header.Declaration.JE_EntryStatus);

			var im1Message2 = Factory.New<TSWMessage>();
			im1Message2.EM_MessageType = "RES";
			im1Message2.EM_MessageText = InvalidCancelResponse2;
			im1Message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(im1Message2);
			AssertEquals(header, im1Message2.EM_LinkedObject);

			var im1Message3 = Factory.New<TSWMessage>();
			im1Message3.EM_MessageType = "RES";
			im1Message3.EM_MessageText = InvalidCancelResponse3;
			im1Message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(im1Message3);
			AssertEquals(header, im1Message3.EM_LinkedObject);
			AssertEquals(FormalEntryStatusList.Codes.DeliveryOrderReceived, header.Declaration.JE_EntryStatus);
		}

		public void TestProcessAcceptedResponseWhenMessageTypeEmpty()
		{
			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var im1Message = Factory.New<TSWMessage>();
			im1Message.EM_MessageType = "";
			im1Message.EM_MessageSubType = MessageTypeList.Codes.TWR;
			im1Message.EM_MessageText = AcceptedResponse;
			im1Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			im1Message.EM_Status = EDIMessage.Status.Queued;
			processor.ProcessMessage(im1Message);
			AssertEquals(header, im1Message.EM_LinkedObject);
			AssertEquals("Message Type is changed to TWR when processed", MessageTypeList.Codes.TWR, im1Message.EM_MessageType);
		}

		public void TestProcessAttachedBACCDocument_Unicode()
		{
			AssertProcessAttachedBACCDocument(Encoding.Unicode, valid: true);
		}

		public void TestProcessAttachedBACCDocument_ASCII()
		{
			AssertProcessAttachedBACCDocument(Encoding.ASCII, valid: true);
		}

		public void TestProcessAttachedBACCDocument_WrongEncoding()
		{
			AssertProcessAttachedBACCDocument(Encoding.BigEndianUnicode, valid: false);
		}

		public void AssertProcessAttachedBACCDocument(Encoding encoding, bool valid)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.DocManagerInfo.AddFileOrDocument(encoding.GetBytes(BACC_xml), "BACC_B2019_147_xml", "TXT");
			header.CH_Status = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			header.Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			header.Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCI;
			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var im1Message1 = Factory.New<TSWMessage>();
			im1Message1.EM_EI = interchange.PK;
			im1Message1.EM_MessageType = "RES";
			im1Message1.EM_MessageText = BACCMessage;
			im1Message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertEquals("Precondition: Before processing, the consignment number is empty.", ZString.Empty, header.Declaration.JE_MAF_ConsignmentNumber);
			processor.ProcessMessage(im1Message1);
			if (valid)
			{
				AssertEquals("After processing, the consignment number is populated.", "C2019/171", header.Declaration.JE_MAF_ConsignmentNumber);
			}
			else
			{
				AssertEquals("After processing, the consignment number is NOT populated.", ZString.Empty, header.Declaration.JE_MAF_ConsignmentNumber);

				string expectedLogMessage = @"Failed to read ConsignmentNumber from the BACC XML file 'BACC_B2019_147_xml' with encoding UTF16 => Data at the root level is invalid. Line 1, position 1.  UTF8 => Root element is missing.
File Contents: ADwAPwB4AG0AbAAgAHYAZQByAHMAaQBvAG4APQAiADEALgAwACIAIABlAG4AYwBvAGQAaQBuAGcAPQAiAFUAVABGAC0AMQA2AEwARQAiACAAcwB0AGEAbgBkAGEAbABvAG4AZQA9ACIAbgBvACIAPwA+AA0ACgA8AHQAYQBiAGwAZQA";

				AssertContains(expectedLogMessage, logger.Logs.First().Message);
			}
		}

		public void TestProcessIncompleteDocument()
		{
			var incompleteDocument = BACC_xml.Substring(0, 1000);
			var interchange = Factory.New<EDIInterchange>();
			interchange.DocManagerInfo.AddFileOrDocument(Encoding.Unicode.GetBytes(incompleteDocument), "BACC_B2019_147_xml", "TXT");
			header.CH_Status = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			header.Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			header.Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCI;
			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var im1Message1 = Factory.New<TSWMessage>();
			im1Message1.EM_EI = interchange.PK;
			im1Message1.EM_MessageType = "RES";
			im1Message1.EM_MessageText = BACCMessage;
			im1Message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertEquals("Precondition: Before processing, the consignment number is empty.", ZString.Empty, header.Declaration.JE_MAF_ConsignmentNumber);
			processor.ProcessMessage(im1Message1);

			AssertEquals("After processing, the consignment number is NOT populated.", ZString.Empty, header.Declaration.JE_MAF_ConsignmentNumber);

			string expectedLogMessage = @"Failed to read ConsignmentNumber from the BACC XML file 'BACC_B2019_147_xml' with encoding UTF16 => Unexpected end of file while parsing Name has occurred. Line 30, position 8.  UTF8 => Name cannot begin with the '.' character, hexadecimal value 0x00. Line 1, position 2.
File Contents: PAA/AHgAbQBsACAAdgBlAHIAcwBpAG8AbgA9ACIAMQAuADAAIgAgAGUAbgBjAG8AZABpAG4AZwA9ACIAVQBUAEYALQAxADYATABFACIAIABzAHQAYQBuAGQAYQBsAG8AbgBlAD0AIgBuAG8AIgA";

			AssertContains(expectedLogMessage, logger.Logs.First().Message);
		}

		public void TestProcessIncompleteDocumentThatHasTheConsignmentNumber()
		{
			var incompleteDocument = BACC_xml.Substring(0, 8000);
			var interchange = Factory.New<EDIInterchange>();
			interchange.DocManagerInfo.AddFileOrDocument(Encoding.Unicode.GetBytes(incompleteDocument), "BACC_B2019_147_xml", "TXT");
			header.CH_Status = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			header.Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			header.Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCI;
			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var im1Message1 = Factory.New<TSWMessage>();
			im1Message1.EM_EI = interchange.PK;
			im1Message1.EM_MessageType = "RES";
			im1Message1.EM_MessageText = BACCMessage;
			im1Message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			AssertEquals("Precondition: Before processing, the consignment number is empty.", ZString.Empty, header.Declaration.JE_MAF_ConsignmentNumber);
			processor.ProcessMessage(im1Message1);

			AssertEquals("After processing, the consignment number is populated.", "C2019/171", header.Declaration.JE_MAF_ConsignmentNumber);

			AssertEquals(0, logger.Logs.Count());
		}

		public void TestDocumentRetrievalFailure()
		{
			const string retrievalFailedMessage = @"Error 500: TSW_ObjectStoreService: Reached the limit 3 for gateway timeout retries:";

			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				header.CH_Status = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
				header.Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
				header.Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCI;

				var interchange = Factory.New<EDIInterchange>();
				interchange.DocManagerInfo.AddFileOrDocument(Encoding.ASCII.GetBytes(retrievalFailedMessage), "BACC_B2019_147_xml", "TXT");
				interchange.EI_From = "ZNC";
				interchange.EI_To = "CW1";
				interchange.EI_SessionGUID = interchange.PK;
				interchange.EI_ReceiveTransmit = "RCV";
				var im1Message = Factory.New<TSWMessage>();
				im1Message.EM_EI = interchange.PK;
				im1Message.EM_MessageType = "RES";
				im1Message.EM_MessageText = BACCMessage;
				im1Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

				var logger = new LoggingInformation();
				var processor = new MessageProcessorFactory(logger);
				processor.ProcessMessage(im1Message);

				AssertEquals("consignment number is empty.", ZString.Empty, header.Declaration.JE_MAF_ConsignmentNumber);
				AssertEquals("processing was successful", header, im1Message.EM_LinkedObject);
				AssertEquals("processing was successful", EDIMessage.Status.Received, im1Message.EM_Status);

				string expectedLogMessage = @"Failed to read ConsignmentNumber from the BACC XML file 'BACC_B2019_147_xml' with encoding UTF16 => Data at the root level is invalid. Line 1, position 1.  UTF8 => Data at the root level is invalid. Line 1, position 1.
File Contents: Error 500: TSW_ObjectStoreService: Reached the limit 3 for gateway timeout retries:";

				AssertContains(expectedLogMessage, logger.Logs.First().Message);
			}
		}

		[TestDate(2022, 08, 22, 10, 30, 00)]
		public void TestMessageWithEmptyDocumentIsReProcessed()
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				header.CH_Status = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
				header.Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
				header.Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCI;

				var interchange = Factory.New<EDIInterchange>();
				interchange.EI_From = "ZNC";
				interchange.EI_To = "CW1";
				interchange.EI_SessionGUID = interchange.PK;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;

				var im1Message = Factory.New<TSWMessage>();
				im1Message.EM_EI = interchange.PK;
				im1Message.EM_MessageNum = "M001";
				im1Message.EM_MessageType = MessageTypeList.Codes.TWR;
				im1Message.EM_MessageText = BACCMessage.Replace("APPLICATION_REFERENCE", header.Declaration.JE_DeclarationReference);
				im1Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				im1Message.EM_LinkedObject = header;

				var docManager = interchange.DocManagerInfo();
				var emptyDocument = (StorageDocsBase)docManager.AddFileOrDocument(new byte[] { 1, 2, 3 }, "BACC_B2019_44.pdf", "FCT");
				emptyDocument.SC_ImageData = Array.Empty<byte>();
				docManager.MasterFactory.Save();
				Factory.Save();

				var persisterMock = new Mock<IExternalPersister>();
				persisterMock.SetupSequence(x => x.RetrieveStream(emptyDocument.PK))
					.Throws(new ExternalStorageException("TEST Download Exception", "S3", new Exception()))
					.Returns((new System.IO.MemoryStream(Encoding.ASCII.GetBytes("12345XYZ")), string.Empty)); // returns a valid doc on the second access
				var persisterProviderMock = new Mock<IExternalPersisterProvider>();
				persisterProviderMock.Setup(p => p.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3)).Returns(persisterMock.Object);

				using (ObjectFactory.Substitute(persisterProviderMock.Object))
				{
					var factory2 = new BusinessObjectFactory();
					var responseMsg = factory2.Load<TSWMessage>(im1Message.PK);

					var logger = new LoggingInformation();
					var processor = new IM1MessageProcessor(logger);

					var exception = AssertExceptionThrown<Exception>(() => processor.ProcessMessage(responseMsg, new IM1Response(new BaseTSWResponse(responseMsg))));
					AssertContains("System encountered an ExternalStorageException in message M001.  Document Name: BACC_B2019_44.pdf", exception.Message);
					responseMsg.Reload();
					AssertEquals(EDIMessage.Status.Queued, responseMsg.EM_Status);

					processor.ProcessMessage(responseMsg, new IM1Response(new BaseTSWResponse(responseMsg)));
					factory2.Save();
					responseMsg.Reload();
					AssertEquals(EDIMessage.Status.Received, responseMsg.EM_Status);

					var declarationInNewFactory = new BusinessObjectFactory().Load<JobDeclaration>(header.Declaration.PK);
					var declarationDocManager = ((IDocManagerSupport)declarationInNewFactory).DocManagerInfo;
					AssertEquals("BACC document should be attached to the Declaration", 1, declarationDocManager.AllEDocs.Count);
					AssertEquals("Retrieves updated data", "12345XYZ", declarationDocManager.AllEDocs[0].ImageData.ToAscii());
				}
			}
		}

		[TestDate(2022, 08, 22, 10, 30, 00)]
		public void TestMessageWithEmptyDocumentIsReProcessedTwice()
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				header.CH_Status = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
				header.Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
				header.Declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCI;

				var interchange = Factory.New<EDIInterchange>();
				interchange.EI_From = "ZNC";
				interchange.EI_To = "CW1";
				interchange.EI_SessionGUID = interchange.PK;
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;

				var im1Message = Factory.New<TSWMessage>();
				im1Message.EM_EI = interchange.PK;
				im1Message.EM_MessageNum = "M001";
				im1Message.EM_MessageType = MessageTypeList.Codes.TWR;
				im1Message.EM_MessageText = BACCMessage.Replace("APPLICATION_REFERENCE", header.Declaration.JE_DeclarationReference);
				im1Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				im1Message.EM_LinkedObject = header;

				var docManager = interchange.DocManagerInfo();
				var emptyDocument = (StorageDocsBase)docManager.AddFileOrDocument(new byte[] { 1, 2, 3 }, "BACC_B2019_44.pdf", "FCT");
				emptyDocument.SC_ImageData = Array.Empty<byte>();
				docManager.MasterFactory.Save();
				Factory.Save();

				var persisterMock = new Mock<IExternalPersister>();
				persisterMock.Setup(x => x.RetrieveStream(emptyDocument.PK))
					.Throws(new ExternalStorageException("TEST Download Exception", "S3", new Exception()));

				var persisterProviderMock = new Mock<IExternalPersisterProvider>();
				persisterProviderMock.Setup(p => p.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3)).Returns(persisterMock.Object);

				using (ObjectFactory.Substitute(persisterProviderMock.Object))
				{
					AssertEquals("No Failures recorded yet", 0, im1Message.DataImportLogNoteCount);

					var processor = new BatchProcessor.Processor(new LoggingInformation());
					processor.ExecuteBatch();
					var responseMsg = new BusinessObjectFactory().Load<TSWMessage>(im1Message.PK);
					AssertEquals("Remains queued", EDIMessage.Status.Queued, responseMsg.EM_Status);
					AssertEquals("Failure Count increased", 1, responseMsg.DataImportLogNoteCount);

					processor.ExecuteBatch();
					responseMsg = new BusinessObjectFactory().Load<TSWMessage>(im1Message.PK);
					AssertEquals("Remains queued", EDIMessage.Status.Queued, responseMsg.EM_Status);
					AssertEquals("Failure Count increased", 2, responseMsg.DataImportLogNoteCount);

					processor.ExecuteBatch();
					responseMsg = new BusinessObjectFactory().Load<TSWMessage>(im1Message.PK);
					AssertEquals("Is Processed", EDIMessage.Status.Received, responseMsg.EM_Status);
					AssertEquals("Failure Count unchanged", 2, responseMsg.DataImportLogNoteCount);

					var declarationInNewFactory = new BusinessObjectFactory().Load<JobDeclaration>(header.Declaration.PK);
					var declarationDocManager = ((IDocManagerSupport)declarationInNewFactory).DocManagerInfo;
					AssertEquals("BACC document was not retrieved", 0, declarationDocManager.AllEDocs.Count);
				}
			}
		}

		public void TestUnsolicitedResponse()
		{
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponsesSendToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unsolicitedDOGroup.PK.ToGuid());
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.NominatedGroup);

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var nzcMessage = Factory.New<TSWMessage>();
			nzcMessage.EM_MessageType = "RES";
			nzcMessage.EM_MessageText = UnsolicitedDeliveryOrderResponse;
			nzcMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcMessage);

			ZString responseOutputExpected = @"[New Zealand Customs Service - Clearance / Acceptance Instructions] Response for TSW Unsolicited Delivery Order: 

An unsolicited Delivery Order message has been received from New Zealand Customs.
The message is for the: Submitter.
Below are the details contained in the message:

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Entry Number    : 73218058
Message No      : 3158

Message Status  : (819) Delivery Order Herewith
                : method of Payment as specified.

Amount Returned : $502.99
Terms           : Client Deferred

Delivery Instructions
---------------------------------------------------------------------
1 LOOSE PACKAGE(S) OR ITEM(S)
";
			AssertEquals(responseOutputExpected, nzcMessage.EM_MessageInterpretation);

			var email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "[New Zealand Customs Service - Clearance / Acceptance Instructions] Response for TSW Unsolicited Delivery Order: ");
			AssertNotNull("Should have found the email generated from processing this message", email);
			AssertEquals(1, email.CCRecipients.Count);
			AssertEquals("JohnTester@testingCompany.com", email.CCRecipients[0].Email);
		}

		#region Implementation

		CusEntryHeader header;
		TSWMessage outgoingMessage;
		GlbGroup unsolicitedDOGroup;

		protected override void SetUp()
		{
			base.SetUp();

			unsolicitedDOGroup = Factory.New<GlbGroup>();
			unsolicitedDOGroup.GG_Code = "USR";
			unsolicitedDOGroup.GG_Desc = "Unsolicited Delivery Order Responses";
			var staff = unsolicitedDOGroup.Staff.AddNew();
			staff.GS_Code = "TST";
			staff.GS_FullName = "John Tester";
			staff.GS_LoginName = "JT";
			staff.GS_EmailAddress = "JohnTester@testingCompany.com";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
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
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "APPLICATION_REFERENCE";
			outgoingMessage.EM_LinkedObject = header;
		}

		#endregion // Implementation

		#region Messages

		#region Error

		public const string ErrorResponse =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns='urn:wco:datamodel:WCO:DM:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
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
  <Response xmlns='urn:wco:datamodel:WCO:ResponseModel:1'>
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
@"[New Zealand Customs Service - Error report] Response for Customs Import Declaration: B99999999

Error report
---------------------------------------------------------------------
Job Number     : B99999999
Master Bill    : 081-11111111
Entry Type     : Import (Normal)
Entry Number   : 04481317
Message No     : 2400

Message Status : (801) Lodgement rejected

Message Errors
---------------------------------------------------------------------
**Error** in {Declaration[1]/GoodsShipment/GovernmentAgencyGoodsItem[3]/Origin/CountryCode}:-
  Country/Region of Origin : Not current
";

		#endregion // Error

		#region Cancel

		public const string CancelResponse =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" >
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
	<IssueDateTime formatCode=""204"">20201123112213</IssueDateTime>
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
		<EffectiveDateTime formatCode=""204"">20201123112213</EffectiveDateTime>
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
@"[New Zealand Customs Service - Confirmation of Transaction ] Response for Customs Import Declaration: B99999999

Confirmation of Transaction 
---------------------------------------------------------------------
Job Number     : B99999999
Master Bill    : 081-11111111
Entry Type     : Import (Normal)
Entry Number   : 04481317
Message No     : 2400

Message Status : (814) Lodgement cancelled
";

		#endregion

		#region Invalid Cancel responses

		public const string InvalidCancelResponse1 =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>40271996G</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20190311154719</IssueDateTime>
    <FunctionalReferenceID>1250180</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>21803621</ID>
        <AcceptanceDateTime formatCode=""204"">20190311154719</AcceptanceDateTime>
        <FunctionalReferenceID>B05002717</FunctionalReferenceID>
        <VersionID>2</VersionID>
        <Submitter>
          <ID>40271996G</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190311154719</EffectiveDateTime>
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

		public const string InvalidCancelResponse2 =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>40271996G</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20190311155223</IssueDateTime>
    <FunctionalReferenceID>1250204</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>ADJUSTMENT RECEIVED, CLEARANCE / APPROVAL BY CUSTOMS OFFICER REQUIRED.</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>21803621</ID>
        <AcceptanceDateTime formatCode=""204"">20190311155223</AcceptanceDateTime>
        <FunctionalReferenceID>B05002717</FunctionalReferenceID>
        <VersionID>3</VersionID>
        <Submitter>
          <ID>40271996G</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190311155223</EffectiveDateTime>
      <NameCode>805</NameCode>
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

		public const string InvalidCancelResponse3 =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>40271996G</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20190314132453</IssueDateTime>
    <FunctionalReferenceID>1267686</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>1 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>21803621</ID>
        <AcceptanceDateTime formatCode=""204"">20190314132453</AcceptanceDateTime>
        <FunctionalReferenceID>B05002717</FunctionalReferenceID>
        <VersionID>3</VersionID>
        <Submitter>
          <ID>40271996G</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>D</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">4952.27</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190314132453</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20190314132453</ReleaseDateTime>
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

		#region Accepted

		public const string AcceptedResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
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
    <IssueDateTime formatCode=""204"">20151123112213</IssueDateTime>
    <FunctionalReferenceID>2056</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>44885032</ID>
        <FunctionalReferenceID>APPLICATION_REFERENCE</FunctionalReferenceID>
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

		public const string BACCMessage = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20190314184443</IssueDateTime>
    <FunctionalReferenceID>5421</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>BAC</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_49F32779-0C9C-44BF-8CA0-DE90CB1FF050"" filename=""BACC_B2019_44.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <AdditionalDocument>
      <CategoryCode>BAC</CategoryCode>
      <ImageBinaryObject mimeCode=""text/plain"" uri=""idd_190FF9DB-967C-480A-B607-42491257D397"" filename=""BACC_B2019_44_xml.txt"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <AdditionalInformation>
      <StatementDescription>Please refer to attached BACC for Directions</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>76341422</ID>
        <AcceptanceDateTime formatCode=""204"">20190314184443</AcceptanceDateTime>
        <FunctionalReferenceID>APPLICATION_REFERENCE</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190314184443</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20190314184443</ReleaseDateTime>
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

		public const string BACC_xml = @"<?xml version=""1.0"" encoding=""UTF-16LE"" standalone=""no""?>
<table>
	<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
		<xs:element name=""d_bacc_header_rept"">
			<xs:complexType>
				<xs:sequence>
					<xs:element ref=""d_bacc_header_rept_row""/>
				</xs:sequence>
			</xs:complexType>
		</xs:element>
		<xs:element name=""d_bacc_header_rept_row"">
			<xs:complexType>
				<xs:sequence>
					<xs:element ref=""bacc_ref_num""/>
					<xs:element ref=""cusmod_release_num""/>
					<xs:element ref=""consign_num""/>
					<xs:element ref=""consign_cleared""/>
					<xs:element ref=""date_arriv""/>
					<xs:element ref=""bacc_name""/>
					<xs:element ref=""bacc_add1""/>
					<xs:element ref=""bacc_add2""/>
					<xs:element ref=""bacc_city""/>
					<xs:element ref=""imp_name""/>
					<xs:element ref=""imp_cont""/>
					<xs:element ref=""imp_add1""/>
					<xs:element ref=""imp_add2""/>
					<xs:element ref=""imp_city""/>
					<xs:element ref=""issued_by""/>
					<xs:element ref=""location""/>
					<xs:element ref=""signing_date""/>
					<xs:element ref=""consign_comments""/>
					<xs:element ref=""agent_name""/>
					<xs:element ref=""agent_cont""/>
					<xs:element ref=""agent_add1""/>
					<xs:element ref=""agent_add2""/>
					<xs:element ref=""agent_city""/>
					<xs:element ref=""ship_name""/>
					<xs:element ref=""fl_number""/>
					<xs:element ref=""voyage""/>
					<xs:element ref=""rejected_date""/>
					<xs:element ref=""ba_om_code""/>
					<xs:element ref=""ch_entry_no""/>
					<xs:element ref=""cash_collected""/>
					<xs:sequence maxOccurs=""unbounded"" minOccurs=""0"">
						<xs:element ref=""d_bacc_auth_rept""/>
						<xs:element ref=""d_bacc_header_identifiers""/>
						<xs:element ref=""d_bacc_header_identifiers_5col""/>
					</xs:sequence>
				</xs:sequence>
			</xs:complexType>
		</xs:element>
		<xs:element name=""d_bacc_auth_rept"">
			<xs:complexType>
				<xs:sequence>
					<xs:element ref=""d_bacc_auth_rept_row"" maxOccurs=""unbounded"" minOccurs=""0""/>
				</xs:sequence>
			</xs:complexType>
		</xs:element>
		<xs:element name=""d_bacc_auth_rept_row"">
			<xs:complexType>
				<xs:sequence>
					<xs:element ref=""authority_aom_code""/>
					<xs:element ref=""authority_comments""/>
					<xs:element ref=""authority_om_code""/>
					<xs:element ref=""authority_mo_code""/>
					<xs:element ref=""authority_date""/>
					<xs:element ref=""approved_operator_company""/>
					<xs:element ref=""city_region_city""/>
					<xs:element ref=""authority_ayd_code""/>
					<xs:element ref=""facility""/>
					<xs:element ref=""transitional_facility_crm_code""/>
					<xs:element ref=""for_mt_wording""/>
					<xs:element ref=""maf_office_office_name""/>
					<xs:element ref=""cofficer_fullname""/>
					<xs:element ref=""std_ref_name""/>
					<xs:element ref=""std_ref_date""/>
					<xs:element ref=""transitional_facility_city""/>
				</xs:sequence>
			</xs:complexType>
		</xs:element>
		<xs:element name=""authority_aom_code"" type=""xs:int"" nillable=""true""/>
		<xs:element name=""authority_comments"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""authority_om_code"" type=""xs:int"" nillable=""true""/>
		<xs:element name=""authority_mo_code"" type=""xs:int"" nillable=""true""/>
		<xs:element name=""authority_date"" type=""xs:dateTime"" nillable=""true""/>
		<xs:element name=""approved_operator_company"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""city_region_city"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""authority_ayd_code"" type=""xs:int"" nillable=""true""/>
		<xs:element name=""facility"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""transitional_facility_crm_code"" type=""xs:int"" nillable=""true""/>
		<xs:element name=""for_mt_wording"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""maf_office_office_name"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""cofficer_fullname"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""std_ref_name"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""std_ref_date"" type=""xs:dateTime"" nillable=""true""/>
		<xs:element name=""transitional_facility_city"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""d_bacc_header_identifiers"">
			<xs:complexType>
				<xs:sequence>
					<xs:element ref=""d_bacc_header_identifiers_row"" maxOccurs=""unbounded"" minOccurs=""0""/>
				</xs:sequence>
			</xs:complexType>
		</xs:element>
		<xs:element name=""d_bacc_header_identifiers_row"">
			<xs:complexType>
				<xs:sequence>
					<xs:element ref=""iden_desc""/>
				</xs:sequence>
			</xs:complexType>
		</xs:element>
		<xs:element name=""iden_desc"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""d_bacc_header_identifiers_5col"">
			<xs:complexType>
				<xs:sequence>
					<xs:element ref=""d_bacc_header_identifiers_5col_row"" maxOccurs=""unbounded"" minOccurs=""0""/>
				</xs:sequence>
			</xs:complexType>
		</xs:element>
		<xs:element name=""d_bacc_header_identifiers_5col_row"">
			<xs:complexType>
				<xs:sequence>
					<xs:element ref=""compute_0001""/>
				</xs:sequence>
			</xs:complexType>
		</xs:element>
		<xs:element name=""compute_0001"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""bacc_ref_num"" type=""xs:string""/>
		<xs:element name=""cusmod_release_num"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""consign_num"" type=""xs:string""/>
		<xs:element name=""consign_cleared"" type=""xs:string""/>
		<xs:element name=""date_arriv"" type=""xs:dateTime"" nillable=""true""/>
		<xs:element name=""bacc_name"" type=""xs:string""/>
		<xs:element name=""bacc_add1"" type=""xs:string""/>
		<xs:element name=""bacc_add2"" type=""xs:string""/>
		<xs:element name=""bacc_city"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""imp_name"" type=""xs:string""/>
		<xs:element name=""imp_cont"" type=""xs:string""/>
		<xs:element name=""imp_add1"" type=""xs:string""/>
		<xs:element name=""imp_add2"" type=""xs:string""/>
		<xs:element name=""imp_city"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""issued_by"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""location"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""signing_date"" type=""xs:dateTime""/>
		<xs:element name=""consign_comments"" type=""xs:string""/>
		<xs:element name=""agent_name"" type=""xs:string""/>
		<xs:element name=""agent_cont"" type=""xs:string""/>
		<xs:element name=""agent_add1"" type=""xs:string""/>
		<xs:element name=""agent_add2"" type=""xs:string""/>
		<xs:element name=""agent_city"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""ship_name"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""fl_number"" type=""xs:string"" nillable=""true""/>
		<xs:element name=""voyage"" type=""xs:string""/>
		<xs:element name=""rejected_date"" type=""xs:dateTime"" nillable=""true""/>
		<xs:element name=""ba_om_code"" type=""xs:double"" nillable=""true""/>
		<xs:element name=""ch_entry_no"" type=""xs:decimal"" nillable=""true""/>
		<xs:element name=""cash_collected"" type=""xs:double"" nillable=""true""/>
	</xs:schema>
	<d_bacc_header_rept xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
		<d_bacc_header_rept_row>
			<bacc_ref_num>B2019/147</bacc_ref_num>
			<cusmod_release_num>AF100100835422</cusmod_release_num>
			<consign_num>C2019/171</consign_num>
			<consign_cleared>Y</consign_cleared>
			<date_arriv>2019-10-12T00:00:00</date_arriv>
			<bacc_name>Importer for ECT</bacc_name>
			<bacc_add1>12 Nowhere Street Incoming</bacc_add1>
			<bacc_add2/>
			<bacc_city>Wellington</bacc_city>
			<imp_name>Importer for ECT</imp_name>
			<imp_cont>brett testing</imp_cont>
			<imp_add1>12 Nowhere Street Incoming</imp_add1>
			<imp_add2/>
			<imp_city>Wellington</imp_city>
			<issued_by>Dimitrios Papahadjis</issued_by>
			<location>zzzAdministration Team</location>
			<signing_date>2019-10-15T08:22:46</signing_date>
			<consign_comments/>
			<agent_name>CargoWise 2</agent_name>
			<agent_cont>Chris Mendoza</agent_cont>
			<agent_add1>75 Sunny Street</agent_add1>
			<agent_add2/>
			<agent_city>Auckland</agent_city>
			<ship_name>AAL FREMANTLE</ship_name>
			<fl_number xsi:nil=""true""/>
			<voyage>82354</voyage>
			<rejected_date xsi:nil=""true""/>
			<ba_om_code>1391</ba_om_code>
			<ch_entry_no>49546756</ch_entry_no>
			<cash_collected>0</cash_collected>
			<d_bacc_auth_rept>
				<d_bacc_auth_rept_row>
					<authority_aom_code>305</authority_aom_code>
					<authority_comments/>
					<authority_om_code>1391</authority_om_code>
					<authority_mo_code>121</authority_mo_code>
					<authority_date>2019-10-15 08:22:46</authority_date>
					<approved_operator_company>MAF Biosecurity New Zealand</approved_operator_company>
					<city_region_city>-</city_region_city>
					<authority_ayd_code>6809926</authority_ayd_code>
					<facility/>
					<transitional_facility_crm_code>0</transitional_facility_crm_code>
					<for_mt_wording>Cargo/Goods/Contents Released</for_mt_wording>
					<maf_office_office_name>zzzAdministration Team</maf_office_office_name>
					<cofficer_fullname>Papahadjis, Dimitrios</cofficer_fullname>
					<std_ref_name>152.01.01s</std_ref_name>
					<std_ref_date/>
					<transitional_facility_city/>
				</d_bacc_auth_rept_row>
			</d_bacc_auth_rept>
			<d_bacc_header_identifiers>
				<d_bacc_header_identifiers_row>
					<iden_desc/>
				</d_bacc_header_identifiers_row>
				<d_bacc_header_identifiers_row>
					<iden_desc>B/L:BKG191011HBL1;</iden_desc>
				</d_bacc_header_identifiers_row>
				<d_bacc_header_identifiers_row>
					<iden_desc>C/N:BKGU1111110;B/L:BKG191011OBL1;</iden_desc>
				</d_bacc_header_identifiers_row>
			</d_bacc_header_identifiers>
			<d_bacc_header_identifiers_5col/>
		</d_bacc_header_rept_row>
	</d_bacc_header_rept>
</table>
";

		#endregion

		const string UnsolicitedDeliveryOrderResponse =
 @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20150925161034</IssueDateTime>
    <FunctionalReferenceID>3158</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>CDO</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_CA7C4566-8C86-4E79-B77D-7874B1B34BC3"" filename=""IM1_Delivery_Order-73218058-2015-09-25-161045713.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <AdditionalInformation>
      <StatementDescription>1 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>73218058</ID>
        <AcceptanceDateTime formatCode=""204"">20150925161034</AcceptanceDateTime>
        <FunctionalReferenceID>73884726</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>D</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount>502.99</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20150925161034</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20150925161034</ReleaseDateTime>
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
	}
}
