using System;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.MAFeBACCa;
using Enterprise.Customs.NZ.Business.MAFeBACCa.MessageBuilders;
using Enterprise.Customs.NZ.Business.MessageProcessors;
using Enterprise.Customs.NZ.Registry;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Moq;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	public class IPIMessageProcessorTest : TestCaseWithFactory
	{
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
			AssertEquals(consol, im1Message.EM_LinkedObject);
			AssertEquals("Message Type is changed to TWR when processed", MessageTypeList.Codes.TWR, im1Message.EM_MessageType);
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

		public void TestFullMsgResponseSuiteIsProcessed()
		{
			consol.JK_UniqueConsignRef = "C00001708";
			consol.JK_MasterBillNum = "GAZ05238299";
			outgoingMessage.EM_ApplicationReference = "C00001708";

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var nzcMessage = Factory.New<TSWMessage>();
			nzcMessage.EM_MessageType = "RES";
			nzcMessage.EM_MessageText = acknowledgeResponse;
			nzcMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcMessage);

			const string ackFormatted = @"[New Zealand Customs Service - Receipt acknowledgment] Response for Customs Import Declaration: C00001708";
			AssertEquals(ackFormatted, nzcMessage.EM_MessageInterpretation);

			nzcMessage = Factory.New<TSWMessage>();
			nzcMessage.EM_MessageType = "RES";
			nzcMessage.EM_MessageText = mpiFoodResponse;
			nzcMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcMessage);
			const string mpiFoodFormatted = @"[Ministry for Primary Industries (Food) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: C00001708

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : C00001708
Master Bill    : GAZ05238299
Entry Type     : TSW IPI ORN:
Entry Number   : 30766889
Message No     : 8626

Message Status : (F04) MPI Food - Cleared

Delivery Instructions
---------------------------------------------------------------------
Food risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.
";
			AssertEquals(mpiFoodFormatted, nzcMessage.EM_MessageInterpretation);

			nzcMessage = Factory.New<TSWMessage>();
			nzcMessage.EM_MessageType = "RES";
			nzcMessage.EM_MessageText = mpiBioResponse;
			nzcMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcMessage);
			const string mpiBioFormatted = @"[Ministry for Primary Industries (Biosecurity) - Inspection / Audit requirements] Response for Customs Import Declaration: C00001708

Inspection / Audit requirements
---------------------------------------------------------------------
Job Number     : C00001708
Master Bill    : GAZ05238299
Entry Type     : TSW IPI ORN:
Entry Number   : 30766889
Message No     : 8627

Message Status : (B05) MPI Biosecurity - Directions Given

Customs Instructions
---------------------------------------------------------------------
MPI HOLD. Please attach relevant docs and re-submit - Consignment is on hold until this information is submitted
";
			AssertEquals(mpiBioFormatted, nzcMessage.EM_MessageInterpretation);
		}

		public void TesteBACCaStatusIsUpdated()
		{
			consol.JK_UniqueConsignRef = "C00001708";
			consol.JK_MasterBillNum = "GAZ05238299";
			outgoingMessage.EM_ApplicationReference = "C00001708";

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var nzcMessage = Factory.New<TSWMessage>();
			nzcMessage.EM_MessageType = "RES";
			nzcMessage.EM_MessageText = acknowledgeResponse;
			nzcMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcMessage);
			AssertEquals("Combined Status - after ACK response", ConsolIPIStatusList.Codes.PP, consolMafMessaging.ZX_MessagingStatus);
			AssertEquals("Response Pending", consolMafMessaging.ZX_MessagingStatusDescription);

			nzcMessage = Factory.New<TSWMessage>();
			nzcMessage.EM_MessageType = "RES";
			nzcMessage.EM_MessageText = mpiBioResponse;
			nzcMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcMessage);
			AssertEquals("Combined Status - after MPI Biosecurity response", ConsolIPIStatusList.Codes.HP, consolMafMessaging.ZX_MessagingStatus);
			AssertEquals("MPI Biosecurity - Entry Held / MPI Food - Pending", consolMafMessaging.ZX_MessagingStatusDescription);

			nzcMessage = Factory.New<TSWMessage>();
			nzcMessage.EM_MessageType = "RES";
			nzcMessage.EM_MessageText = mpiFoodResponse;
			nzcMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcMessage);
			AssertEquals("Combined Status - after MPI Food response", ConsolIPIStatusList.Codes.HC, consolMafMessaging.ZX_MessagingStatus);
			AssertEquals("MPI Biosecurity - Entry Held / MPI Food - Cleared", consolMafMessaging.ZX_MessagingStatusDescription);
		}

		public void TestRejectionStatus()
		{
			consol.JK_UniqueConsignRef = "C00001717";
			consol.JK_MasterBillNum = "BKG200825OBL1";
			outgoingMessage.EM_ApplicationReference = "C00001717";

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var nzcMessage = Factory.New<TSWMessage>();
			nzcMessage.EM_MessageType = "RES";
			nzcMessage.EM_MessageText = tswResponse;
			nzcMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcMessage);
			AssertEquals("Combined Status - after TSW rejection response", ConsolIPIStatusList.Codes.EntryRejected, consolMafMessaging.ZX_MessagingStatus);
			AssertEquals("Entry Rejected", consolMafMessaging.ZX_MessagingStatusDescription);
		}

		public void TestAttachedDocumentIsProcessed_DocumentDataIsInDatabase()
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.DB))
			{
				var expectedDocData = "%PDF <BAC> %EOF\n";
				var docDataInDb = Encoding.ASCII.GetBytes(expectedDocData);

				AssertAttachedDocumentIsProcessed(docDataInDb, expectedDocData);
			}
		}

		public void TestAttachedDocumentIsProcessed_DocumentDataIsInS3Server()
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			{
				var expectedDocData = "%PDF <BAC> %EOF\n";
				var docDataInDb = Encoding.ASCII.GetBytes(expectedDocData);

				var persisterProviderMock = new Mock<IExternalPersisterProvider>();
				var persisterMock = new Mock<IExternalPersister>();

				persisterMock.Setup(x => x.RetrieveStream(It.IsAny<ZGuid>())).Returns((new System.IO.MemoryStream(docDataInDb), string.Empty));

				persisterProviderMock.Setup(p => p.GetExternalPersister(Core.Constants.EDocsStorageProviders.Code.S3)).Returns(persisterMock.Object);
				using (ObjectFactory.Substitute(persisterProviderMock.Object))
				{
					AssertAttachedDocumentIsProcessed(Array.Empty<byte>(), expectedDocData);
				}
			}
		}

		void AssertAttachedDocumentIsProcessed(byte[] docDataInDb, string expectedDocData)
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000033293";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = ClearWithBACC;
			var docManager = interchange.DocManagerInfo();
			var eDocs = (StorageDocsBase)docManager.AddFileOrDocument(new byte[] { 1, 2, 3 }, "BACC_B2020_71.pdf", "FCT");
			eDocs.SC_ImageData = docDataInDb;
			AssertEquals("Pre-condition: Test BACC document should be attached to the interchange", 1, ((IDocManagerSupport)interchange).DocManagerInfo.AllEDocs.Count);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001717";
			consol.JK_MasterBillNum = "BKG200825OBL1";
			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.IPI;
			outgoingMessage.EM_MessageText = OriginalIPI;
			outgoingMessage.EM_MessageNum = "1";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "C00001717";
			outgoingMessage.EM_LinkedObject = consol;

			var responseMessage = Factory.New<TSWMessage>();
			responseMessage.EM_EI = interchange.PK;
			responseMessage.EM_MessageType = "RES";
			responseMessage.EM_MessageText = ClearWithBACC;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_LinkedObject = consol;
			interchange.DocManagerInfo().MasterFactory.Save();
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(responseMessage);
			Factory.Save();

			AssertEquals(consol, responseMessage.EM_LinkedObject);

			var reLoadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
			AssertEquals("BACC document should have been attached to the Consol", 1, ((IDocManagerSupport)reLoadedConsol).DocManagerInfo.AllEDocs.Count);

			var consolDoc = ((IDocManagerSupport)reLoadedConsol).DocManagerInfo.AllEDocs[0];
			AssertEquals("BACC document file name", "BACC_B2020_71.pdf", consolDoc.FileName);
			AssertEquals("Doc Type should be ??", "MCD", consolDoc.DocType);
			AssertEquals("Doc data should be copied", expectedDocData, (consolDoc as StorageFile).SC_ImageDataFromDb.ToAscii());
		}

		[TestDate(2022, 08, 22, 10, 30, 00)]
		public void TestMessageWithEmptyDocumentIsReProcessed()
		{
			using (SystemDataRegistry.Instance.EDocsStorageProvider.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Core.Constants.EDocsStorageProviders.Code.S3))
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_UniqueConsignRef = "C00001717";
				consol.JK_MasterBillNum = "BKG200825OBL1";

				var interchange = Factory.New<NZCInterchange>();
				interchange.EI_From = "CUSSWT";
				interchange.EI_To = "00009908C";
				interchange.EI_ApplicationCode = "NZC";
				interchange.EI_InterchangeType = "NZC";
				interchange.EI_InterchangeNum = "00000000000000033293";
				interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
				interchange.EI_IsActive = true;
				interchange.EI_BodyText = ClearWithBACC;

				var outgoingMessage = Factory.New<TSWMessage>();
				outgoingMessage.EM_MessageType = MessageTypeList.Codes.IPI;
				outgoingMessage.EM_MessageText = OriginalIPI;
				outgoingMessage.EM_MessageNum = "M001";
				outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
				outgoingMessage.EM_ApplicationReference = "C00001717";
				outgoingMessage.EM_LinkedObject = consol;

				var responseMessage = Factory.New<TSWMessage>();
				responseMessage.EM_EI = interchange.PK;
				responseMessage.EM_MessageType = MessageTypeList.Codes.TWR;
				responseMessage.EM_MessageText = ClearWithBACC;
				responseMessage.EM_MessageNum = "M002";
				responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				responseMessage.EM_LinkedObject = consol;

				var docManager = interchange.DocManagerInfo();
				var emptyDocument = (StorageDocsBase)docManager.AddFileOrDocument(new byte[] { 1, 2, 3 }, "BACC_B2020_71.pdf", "FCT");
				emptyDocument.SC_ImageData = Array.Empty<byte>();

				AssertEquals("Pre-condition: Test BACC document should be attached to the interchange", 1, docManager.AllEDocs.Count);
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
					var responseMsg = factory2.Load<TSWMessage>(responseMessage.PK);

					var logger = new LoggingInformation();
					var processor = new IPIMessageProcessor(logger);

					var exception = AssertExceptionThrown<Exception>(() => processor.ProcessMessage(responseMsg, new IM1Response(new BaseTSWResponse(responseMsg))));
					AssertContains("System encountered an ExternalStorageException in message M002.  Document Name: BACC_B2020_71.pdf", exception.Message);
					responseMsg.Reload();
					AssertEquals(EDIMessage.Status.Queued, responseMsg.EM_Status);

					processor.ProcessMessage(responseMsg, new IM1Response(new BaseTSWResponse(responseMsg)));
					factory2.Save();
					responseMsg.Reload();
					AssertEquals(EDIMessage.Status.Received, responseMsg.EM_Status);

					var consolInNewFactory = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
					var consolDocManager = ((IDocManagerSupport)consolInNewFactory).DocManagerInfo;
					AssertEquals("BACC document should be attached to the Consol", 1, consolDocManager.AllEDocs.Count);
					AssertEquals("Retrieves updated data", "12345XYZ", consolDocManager.AllEDocs[0].ImageData.ToAscii());
				}
			}
		}

		public void TestLateBIOResponseEmailsCorrectUser()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponsesSendToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unsolicitedDOGroup.PK.ToGuid());
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.NominatedGroup);

			consol.JK_UniqueConsignRef = "C00001708";
			consol.JK_MasterBillNum = "GAZ05238299";
			outgoingMessage.EM_ApplicationReference = "C00001708";
			outgoingMessage.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + "/" + EDIMessage.SendersReferencePlaceHolder;

			var otherConsolMsg1 = Factory.New<TSWMessage>();
			otherConsolMsg1.EM_ApplicationCode = "AUC";
			otherConsolMsg1.EM_ApplicationReference = "C00001708";
			otherConsolMsg1.EM_ReceiveTransmit = "TRX";
			otherConsolMsg1.EM_SystemCreateUser = "JAK";
			otherConsolMsg1.EM_MessageText = EDIMessage.MessageNumberPlaceHolder + "/" + EDIMessage.SendersReferencePlaceHolder;
			otherConsolMsg1.EM_LinkedObject = consol;

			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var nzcMessage = Factory.New<TSWMessage>();
			nzcMessage.EM_MessageType = "RES";
			nzcMessage.EM_MessageText = mpiBioResponse;
			nzcMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcMessage);

			var email = Env.OutgoingMailManager.EmailsCreated.Find(emailToMatched => emailToMatched.Subject == "[Ministry for Primary Industries (Biosecurity) - Inspection / Audit requirements] Response for Customs Import Declaration: C00001708");
			AssertNotNull("An email should be generated from processing this message", email);
			AssertEquals(1, email.Recipients.Count);
			AssertEquals("The email generated from processing this response message should have gone to the submitter of the Consol customs messages", "BillBroker@testingCompany.com.nz", email.Recipients[0].Email);
		}

		#region Implementation

		ForwardingConsol consol;
		MAFMessagingBO consolMafMessaging;
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

			var broker = Factory.NewWithValidTestData<GlbStaff>();
			broker.GS_Code = "BB";
			broker.GS_FullName = "Bill Broker";
			broker.GS_LoginName = "BB";
			broker.GS_EmailAddress = "BillBroker@testingCompany.com.nz";

			var otherCountryStaffMember = Factory.NewWithValidTestData<GlbStaff>();
			otherCountryStaffMember.GS_Code = "JAK";
			otherCountryStaffMember.GS_FullName = "John King";
			otherCountryStaffMember.GS_LoginName = "JAK";
			otherCountryStaffMember.GS_EmailAddress = "JohnKing@testingCompany.com.au";
			Factory.Save();

			consol = Factory.New<ForwardingConsol>();
			outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.IPI;
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "APPLICATION_REFERENCE";
			outgoingMessage.EM_LinkedObject = consol;
			outgoingMessage.EM_SystemCreateUser = broker.GS_LoginName;

			consolMafMessaging = GetMAFMessaging(consol);
		}

		public MAFMessagingBO GetMAFMessaging(ForwardingConsol consol)
		{
			return new MAFMessagingBO(new MAFPlugInSupportConsolWrapper(consol));
		}

		#endregion // Implementation

		#region Messages

		#region Both Agencies and Acknowledgement

		public const string acknowledgeResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20200804181313</IssueDateTime>
    <FunctionalReferenceID>8625</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>30766889</ID>
        <FunctionalReferenceID>C00001708</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20200804181313</EffectiveDateTime>
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

		public const string mpiBioResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20200804181343</IssueDateTime>
    <FunctionalReferenceID>8627</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MPI HOLD. Please attach relevant docs and re-submit - Consignment is on hold until this information is submitted</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>30766889</ID>
        <AcceptanceDateTime formatCode=""204"">20200804181343</AcceptanceDateTime>
        <FunctionalReferenceID>C00001708</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20200804181343</EffectiveDateTime>
      <NameCode>B05</NameCode>
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

		public const string mpiFoodResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20200804181342</IssueDateTime>
    <FunctionalReferenceID>8626</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Food risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>30766889</ID>
        <AcceptanceDateTime formatCode=""204"">20200804181342</AcceptanceDateTime>
        <FunctionalReferenceID>C00001708</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20200804181342</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20200804181342</ReleaseDateTime>
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

		#region TSWResponse

		public const string tswResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20200825094323</IssueDateTime>
    <FunctionalReferenceID>8736</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>00000000</ID>
        <FunctionalReferenceID>C00001717</FunctionalReferenceID>
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
      <ValidationCode>4077</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>67A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>28A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>64A/L017</DocumentSectionCode>
        <TagID>L017</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>136</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>08A</DocumentSectionCode>
        <TagID>L058</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20200825094323</EffectiveDateTime>
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

		#endregion

		#region Error

		public const string ErrorResponse =
@" <?xml version=""1.0"" encoding=""UTF-8""?>
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

		#region BACC Document with message

		public const string OriginalIPI = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>IM</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>IPI</TypeCode>
  <FunctionalReferenceID>C00001717</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <TotalGrossMassMeasure unitCode=""KGM"">3</TotalGrossMassMeasure>
  <JurisdictionDateTime formatCode=""102"">20200826</JurisdictionDateTime>
  <Submitter>
    <ID>51358596K</ID>
  </Submitter>
  <AdditionalDocument>
    <CategoryCode>INV</CategoryCode>
    <ImageBinaryObject mimeCode=""application/pdf"" filename=""CIV-dummy.pdf"">ATTACHED</ImageBinaryObject>
  </AdditionalDocument>
  <AdditionalInformation>
    <StatementDescription>tes999,EDI TEST BRANCH NZAKL</StatementDescription>
    <StatementTypeCode>MAC</StatementTypeCode>
  </AdditionalInformation>
  <Agent>
    <ID>51358596K</ID>
    <RoleCode>CB</RoleCode>
  </Agent>
  <ApprovedEstablishmentPlace>
    <ID>25004</ID>
  </ApprovedEstablishmentPlace>
  <BorderTransportMeans>
    <Name>AAL FREMANTLE</Name>
    <ID>4823981</ID>
    <TypeCode>1</TypeCode>
    <JourneyID>9743</JourneyID>
  </BorderTransportMeans>
  <Carrier>
    <Name>A.A.L. SHIPPING AGENCIES P/L</Name>
  </Carrier>
  <GoodsShipment>
    <ExportationCountryCode>AU</ExportationCountryCode>
    <TransactionNatureCode>90</TransactionNatureCode>
    <Consignment>
      <AdditionalInformation>
        <StatementDescription>BKGU1111110</StatementDescription>
        <StatementTypeCode>MCD</StatementTypeCode>
      </AdditionalInformation>
      <GoodsLocation>
        <ID>7179L</ID>
      </GoodsLocation>
      <LoadingLocation>
        <ID>AUSYD</ID>
      </LoadingLocation>
      <TransportContractDocument>
        <ID>BKG200825OBL1</ID>
        <TypeCode>MB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>3</SequenceNumeric>
          <DocumentSectionCode>30B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HBILL1</ID>
        <TypeCode>BM</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>HBILL2</ID>
        <TypeCode>BM</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>28A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>31B</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <TransportEquipment>
        <SequenceNumeric>1</SequenceNumeric>
        <CharacteristicCode>23</CharacteristicCode>
        <FullnessCode>7</FullnessCode>
        <ID>BKGU1111110</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Seal>
          <SequenceNumeric>1</SequenceNumeric>
          <ID>123</ID>
        </Seal>
      </TransportEquipment>
      <TransportEquipment>
        <SequenceNumeric>2</SequenceNumeric>
        <CharacteristicCode>23</CharacteristicCode>
        <FullnessCode>7</FullnessCode>
        <ID>BKGU1111110</ID>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>2</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
        <Seal>
          <SequenceNumeric>1</SequenceNumeric>
          <ID>123</ID>
        </Seal>
      </TransportEquipment>
      <UnloadingLocation>
        <ID>NZAKL</ID>
      </UnloadingLocation>
    </Consignment>
    <GovernmentAgencyGoodsItem>
      <SequenceNumeric>1</SequenceNumeric>
      <AdditionalInformation>
        <StatementCode>135</StatementCode>
        <StatementTypeCode>REL</StatementTypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <DocumentSectionCode>67A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>18B</DocumentSectionCode>
        </Pointer>
      </AdditionalInformation>
      <Commodity>
        <Description>FAK container</Description>
        <Classification>
          <ID>8609000916L</ID>
          <IdentificationTypeCode>HS</IdentificationTypeCode>
        </Classification>
        <Source>
          <CountryCode>AU</CountryCode>
        </Source>
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">3</GrossMassMeasure>
        <NetNetWeightMeasure unitCode=""KGM"">3</NetNetWeightMeasure>
      </GoodsMeasure>
      <Origin>
        <CountryCode>AU</CountryCode>
      </Origin>
    </GovernmentAgencyGoodsItem>
    <Invoice>
      <ID>INV1</ID>
      <ConditionCode>FOB</ConditionCode>
      <SequenceNumeric>1</SequenceNumeric>
    </Invoice>
    <Supplier>
      <Name>YOUR AUSTRALIAN COMPANY</Name>
      <Address>
        <CityName>SURRY HILLS</CityName>
        <CountryCode>AU</CountryCode>
        <CountrySubDivisionName>NSW</CountrySubDivisionName>
        <Line>5010 ALEXANDRIA LANE</Line>
        <PostcodeID>2010</PostcodeID>
      </Address>
    </Supplier>
  </GoodsShipment>
  <Importer>
    <ID>51352368J</ID>
    <Contact>
      <Name>Gary O'Dea</Name>
      <Communication>
        <ID>gary@younewakl.com.au</ID>
        <TypeID>EM</TypeID>
      </Communication>
    </Contact>
  </Importer>
  <Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>1</QuantityQuantity>
    <TypeCode>PX</TypeCode>
  </Packaging>
  <Packaging>
    <SequenceNumeric>2</SequenceNumeric>
    <QuantityQuantity>2</QuantityQuantity>
    <TypeCode>PX</TypeCode>
  </Packaging>
</Declaration>
</DocumentMetadata>";

		public const string ClearWithBACC =
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
    <IssueDateTime formatCode=""204"">20200825113228</IssueDateTime>
    <FunctionalReferenceID>8747</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>BAC</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""8e1edde3-b83f-4b29-8b16-1fab87958107-lodgement"" filename=""BACC_B2020_71.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <AdditionalDocument>
      <CategoryCode>BAC</CategoryCode>
      <ImageBinaryObject mimeCode=""text/plain"" uri=""2bd5705e-eec1-4eaf-88d5-04ebf820172f-lodgement"" filename=""BACC_B2020_71_xml.txt"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <AdditionalInformation>
      <StatementDescription>Please refer to attached BACC for Directions</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>60567817</ID>
        <AcceptanceDateTime formatCode=""204"">20200825113228</AcceptanceDateTime>
        <FunctionalReferenceID>C00001717</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20200825113228</EffectiveDateTime>
      <NameCode>B05</NameCode>
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
	}
}
