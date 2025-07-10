using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(EDIMessage))]
	sealed class EDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<EDIMessage>();
			AssertEquals(EDIInterchange.ApplicationCodes.USCustomsDIS, message.EM_ApplicationCode);
		}

		public void TestMessageNumStrategy()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();

			var message2 = Factory.New<EDIMessage>();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();

			AssertEquals("EDIEDIDAT_1", message.EM_MessageNum);
			AssertEquals("EDIEDIDAT_2", message2.EM_MessageNum);
		}

		public void TestReplacePlaceHolderWithMessageAttachmentPK()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = MessageTypeList.Codes.Submission;
			message.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-16""?>
<DocumentSubmissionPackage xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <SubmittedToPortCode xmlns=""http://cbp.dhs.gov/DIS"">123456</SubmittedToPortCode>
  <ActionCode xmlns=""http://cbp.dhs.gov/DIS"">ADD</ActionCode>
  <DocumentObject>!dOcUmEnTiMaGePlAcEHoLdEr:{0}!</DocumentObject>
</DocumentSubmissionPackage>";

			var attachment = message.MessageAttachments.AddNew();

			Factory.Save();
			AssertContains(attachment.PK.ToString(), message.EM_MessageText);
		}

		public void TestReplacePlaceHolderWhenNoAttachment()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = MessageTypeList.Codes.Submission;
			message.EM_MessageText = @"<?xml version=""1.0"" encoding=""utf-16""?>
<DocumentSubmissionPackage xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <SubmittedToPortCode xmlns=""http://cbp.dhs.gov/DIS"">123456</SubmittedToPortCode>
  <ActionCode xmlns=""http://cbp.dhs.gov/DIS"">ADD</ActionCode>
  <DocumentObject>!dOcUmEnTiMaGePlAcEHoLdEr:{0}!</DocumentObject>
</DocumentSubmissionPackage>";

			AssertNoExceptionThrown(delegate
			{
				Factory.Save();
			});
		}

		public void TestAccessingEM_LinkedObject()
		{
			var declaration = new TestHelper(Factory).GetJobDeclaration();
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(declaration, "TST");
			var eDocs = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);

			var requiredDoc = ((IDocsAndCartageParent)declaration).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var requiredDocAddInfo = requiredDoc.AddInfos.AddNew();
			requiredDocAddInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;

			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = MessageTypeList.Codes.Submission;
			message.EM_LinkedObject = requiredDocAddInfo;

			var attachment = message.MessageAttachments.AddNew();
			attachment.EG_StorageDocsGuid = eDocs.UniqueKey;

			Factory.Save();
			docManagerSupport.DocManagerInfo.MasterFactory.Save();

			var factory2 = new BusinessObjectFactory();
			var messageLoaded = factory2.Load<EDIMessage>(message.PK);
			AssertEquals("AfterNewObjectIsLinked should be called from the getter of EM_LinkedObject", requiredDocAddInfo.PK, messageLoaded.EM_LinkedObject.PK);
			AssertEquals(eDocs.UniqueKey, messageLoaded.MessageAttachments[0].GetAttachment().UniqueKey);
		}

		public void TestAccessingEM_LinkedObject_CusISFHeader()
		{
			//var declaration = new TestHelper(Factory).GetJobDeclaration();
			var cusISFHeader = (BusinessObject)Factory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			var docManagerSupport = (IDocManagerSupport)cusISFHeader;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(cusISFHeader, "TST");
			var eDocs = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);

			var requiredDoc = ((IDISHost)cusISFHeader).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var requiredDocAddInfo = requiredDoc.AddInfos.AddNew();
			requiredDocAddInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;

			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = MessageTypeList.Codes.Submission;
			message.EM_LinkedObject = requiredDocAddInfo;

			var attachment = message.MessageAttachments.AddNew();
			attachment.EG_StorageDocsGuid = eDocs.UniqueKey;

			Factory.Save();
			docManagerSupport.DocManagerInfo.MasterFactory.Save();

			var factory2 = new BusinessObjectFactory();
			var messageLoaded = factory2.Load<EDIMessage>(message.PK);
			AssertEquals("AfterNewObjectIsLinked should be called from the getter of EM_LinkedObject", requiredDocAddInfo.PK, messageLoaded.EM_LinkedObject.PK);
			AssertEquals("LinkedObject should be CusISFHeader.", ObjectFactory.GetType<Integration.Customs.US.ISF.ICusISFHeader>(), ((JobRequiredDocumentAddInfo)messageLoaded.EM_LinkedObject).RequiredDocument.ParentType);
		}

		public void TestMessageContent()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageTypeList.Codes.DocumentValidationResponse;
			message.EM_MessageText = @"<DIS:MessageEnvelope xmlns=""http://cbp.dhs.gov/DIS"" xmlns:DIS=""http://cbp.dhs.gov/DIS"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><DIS:MessageHeader><DIS:MessageID>176353348</DIS:MessageID><DIS:MessageType>DocumentValidationResponse</DIS:MessageType><DIS:SentDateTime>2014-04-17T00:00:02.747-04:00</DIS:SentDateTime><DIS:TransmitterID /><DIS:TransmitterSiteCode /><DIS:PreparerID>XJ5</DIS:PreparerID><DIS:PreparerSiteCode>3902</DIS:PreparerSiteCode>
      </DIS:MessageHeader><DIS:MessageBody><DIS:MessageValidationResponse>
         <DIS:MessageLevelResult>
            <DIS:ProcessedMessageHeader xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
    <DIS:MessageID>HYEDUSDAT_1</DIS:MessageID>
    <DIS:MessageType>DocumentSubmission</DIS:MessageType>
    <DIS:SentDateTime>2014-04-15T19:58:12.863+10:00</DIS:SentDateTime>
    <DIS:TransmitterID>SV9</DIS:TransmitterID>
    <DIS:TransmitterSiteCode>3910</DIS:TransmitterSiteCode>
    <DIS:PreparerID>XJ5</DIS:PreparerID>
    <DIS:PreparerSiteCode>3902</DIS:PreparerSiteCode>
  </DIS:ProcessedMessageHeader>
            <DIS:MessageProcessingResult>
               <DIS:ProcessingEvent>INITIAL_VALIDATION</DIS:ProcessingEvent>
               <DIS:ProcessingStatus>FAILED</DIS:ProcessingStatus>
               <DIS:ProcessingLogText>One or more of following fields are invalid: TransmitterID, TransmitterSiteCode, PreparerID, and PreparerSiteCode</DIS:ProcessingLogText>
            </DIS:MessageProcessingResult>
         </DIS:MessageLevelResult>
         <DIS:DocumentLevelResult>
            <DIS:ProcessedDocumentHeader xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
          <DIS:DocumentID>BCHI40007235_DIS1</DIS:DocumentID>
          <DIS:DocumentLabel>Certificate Of Origin</DIS:DocumentLabel>
          <DIS:CompleteFileName>certificate of originBCHI40007235_DIS1.pdf</DIS:CompleteFileName>
          <DIS:FileExtensionType>pdf</DIS:FileExtensionType>
          <DIS:DocumentDescription>CERTIFICATE OF ORIGIN</DIS:DocumentDescription>
          <DIS:DocPreviouslySubmitted>N</DIS:DocPreviouslySubmitted>
        </DIS:ProcessedDocumentHeader>
            <DIS:DocumentProcessingResult>
               <DIS:ProcessingEvent>INITIAL_VALIDATION</DIS:ProcessingEvent>
               <DIS:ProcessingStatus>PASSED</DIS:ProcessingStatus>
               <DIS:ProcessingLogText>No errors found for document</DIS:ProcessingLogText>
            </DIS:DocumentProcessingResult>
         </DIS:DocumentLevelResult>
      </DIS:MessageValidationResponse></DIS:MessageBody>
   </DIS:MessageEnvelope>";

			AssertNoExceptionThrown(delegate
			{ var accessed = message.MessageContent; });

			var messageID = message.MessageContent.Descendants().FirstOrDefault(z => z.Matches("DocumentID"));
			AssertNotNull(messageID);
			AssertEquals("BCHI40007235_DIS1", messageID.Value);
		}

		public void TestRelatedMessage()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = MessageTypeList.Codes.Submission;
			message.EM_MessageNum = "1";
			AssertNull(message.RelatedMessage);

			var message2 = Factory.New<EDIMessage>();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_MessageType = MessageTypeList.Codes.Submission;
			message2.EM_MessageNum = "1";

			AssertEquals(message, message2.RelatedMessage);
			AssertEquals(message2, message.RelatedMessage);
		}

		public void TestResetToQueue()
		{
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = MessageTypeList.Codes.DocumentValidationResponse;
			message.EM_MessageNum = "1";
			message.EM_Status = EDIMessage.Status.Received;

			message.ResetToQueuedStatus();
			AssertEquals(MessageTypeList.Codes.DocumentValidationResponse, message.EM_MessageType);
			AssertEquals("1", message.EM_MessageNum);
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
		}

		public void TestGetAttachment()
		{
			var declaration = new TestHelper(Factory).GetJobDeclaration();
			var importer = Factory.Load<OrgHeader>((ZGuid)declaration[JobDeclarationSchema.JE_OH_Importer]);
			var docManagerSupport = (IDocManagerSupport)importer;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(importer, "TST");
			var eDocs = storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "ABC.pdf", "ABC", false);

			AssertCollectionContains("Precondition: declaration.EDocs should contains the importer's eDocs", eDocs, ((IDISHost)declaration).EDocs);

			var requiredDoc = ((IDocsAndCartageParent)declaration).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var requiredDocAddInfo = requiredDoc.AddInfos.AddNew();
			requiredDocAddInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;

			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = MessageTypeList.Codes.Submission;
			message.EM_LinkedObject = requiredDocAddInfo;

			var messageAttachment = message.MessageAttachments.AddNew();
			messageAttachment.EG_StorageDocsGuid = eDocs.UniqueKey;

			Factory.Save();
			docManagerSupport.DocManagerInfo.MasterFactory.Save();

			var factory2 = new BusinessObjectFactory();
			var messageLoaded = factory2.Load<EDIMessage>(message.PK);
			AssertEquals("AfterNewObjectIsLinked should be called from the getter of EM_LinkedObject", requiredDocAddInfo.PK, messageLoaded.EM_LinkedObject.PK);
			var attachment = messageLoaded.MessageAttachments[0].GetAttachment();
			AssertNotNull("attachment should not be null", attachment);
			AssertEquals("get the correct eDocs", eDocs.UniqueKey, attachment.UniqueKey);
		}

		public void TestDocumentReviewProperties()
		{
			var ediMessage = Factory.New<EDIMessage>();
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_SystemCreateTimeUtc = ZDateTime.Today;
			ediMessage.EM_MessageText = TestHelper.DocumentReviewRejectedResponseXml;
			ediMessage.EM_MessageType = MessageTypeList.Codes.DocumentReviewResponse;
			AssertEquals("INCOMPLETE_DOCUMENT_SET", ediMessage.DocReviewRejectReason);
			AssertEquals("BTA anticipated arrival information is missing", ediMessage.DocReviewComment);
		}

		public void TestEM_MessageTextAndEM_MessageTextDetail()
		{
			var ediMessage = Factory.New<EDIMessage>();
			ediMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			ediMessage.EM_SystemCreateTimeUtc = ZDateTime.Today;
			ediMessage.EM_MessageText = TestHelper.DocumentSubmissionXml;

			var expectedMessage = "<DIS:ImporterOfRecordNbr>123-45-1456</DIS:ImporterOfRecordNbr>";
			AssertContains(expectedMessage, ediMessage.EM_MessageText);
			AssertContains(expectedMessage, ediMessage.EM_MessageTextDetail);

			expectedMessage = "<DIS:ImporterOfRecordNbr>***-**-****</DIS:ImporterOfRecordNbr>";
			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertContains(expectedMessage, ediMessage.EM_MessageText);
			AssertContains(expectedMessage, ediMessage.EM_MessageTextDetail);

			expectedMessage = "<DIS:ImporterOfRecordNbr>123456-6789</DIS:ImporterOfRecordNbr>";
			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = true;
			ediMessage.EM_MessageText = Regex.Replace(TestHelper.DocumentSubmissionXml, @">[0-9]{3}-[0-9]{2}-[0-9]{4}<\/", ">123456-6789</");
			AssertContains(expectedMessage, ediMessage.EM_MessageText);
			AssertContains(expectedMessage, ediMessage.EM_MessageTextDetail);

			Env.Security.OrgDetailsViewPersonalInformation.IsAllowed = false;
			AssertContains(expectedMessage, ediMessage.EM_MessageText);
			AssertContains(expectedMessage, ediMessage.EM_MessageTextDetail);
		}
	}
}
