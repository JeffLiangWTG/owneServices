using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	sealed class InboundInterchangeProcessorTest : TestCaseWithFactory
	{
		public void TestProcessInterchange()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_BodyText = TestHelper.DocumentValidationResponseFailedXml;
			interchange.EI_From = "USC";
			interchange.EI_To = "ABC";
			Factory.Save();
			var logger = new LoggingInformation();
			new InboundInterchangeProcessor(logger).ExecuteBatch();
			interchange.Reload();
			AssertEquals(EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals(MessageTypeList.Codes.DocumentValidationResponse, interchange.EI_InterchangeType);
			AssertEquals(1, interchange.ContainedMessages.Count);
			var message = interchange.ContainedMessages[0];
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(MessageTypeList.Codes.DocumentValidationResponse, message.EM_MessageType);
			AssertEquals("HYEDUSDAT_13", message.EM_MessageNum);
		}

		public void TestMessageIDOfOutgoingMessageIsSetProperly()
		{
			var declaration = new TestHelper(Factory).GetJobDeclaration();
			var requiredDoc = ((IDocsAndCartageParent)declaration).RequiredDocumentsProvider.RequiredDocuments.AddNew();
			var requiredDocAddInfo = requiredDoc.AddInfos.AddNew();
			requiredDocAddInfo.EX_ApplicationCode = Core.Constants.Customs.DocumentImageSystemIDs.US_DIS;
			var message = Factory.New<EDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = MessageTypeList.Codes.Submission;
			message.EM_LinkedObject = requiredDocAddInfo;
			Factory.Save();
			message.EM_MessageNum = "HYEDUSDAT_1";
			Factory.Save();
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_BodyText = @"<DIS:MessageEnvelope xmlns=""http://cbp.dhs.gov/DIS"" xmlns:DIS=""http://cbp.dhs.gov/DIS"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
<DIS:MessageHeader>
	<DIS:MessageID>176353348</DIS:MessageID>
	<DIS:MessageType>DocumentValidationResponse</DIS:MessageType>
	<DIS:SentDateTime>2014-04-17T00:00:02.747-04:00</DIS:SentDateTime>
	<DIS:TransmitterID />
	<DIS:TransmitterSiteCode />
	<DIS:PreparerID>XJ5</DIS:PreparerID>
	<DIS:PreparerSiteCode>3902</DIS:PreparerSiteCode>
</DIS:MessageHeader>
<DIS:MessageBody>
	<DIS:MessageValidationResponse>
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
			interchange.EI_From = "USC";
			interchange.EI_To = "ABC";
			Factory.Save();
			var logger = new LoggingInformation();
			new InboundInterchangeProcessor(logger).ExecuteBatch();
			AssertEquals(1, interchange.ContainedMessages.Count);
			var responseMesage = (EDIMessage)interchange.ContainedMessages[0];
			AssertNotNull(responseMesage.RelatedMessage);
		}

		public void TestProcessInterchangeForDocumentReviewResponse()
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_BodyText = TestHelper.DocumentReviewRejectedResponseXml;
			interchange.EI_From = "USC";
			interchange.EI_To = "ABC";
			Factory.Save();
			var logger = new LoggingInformation();
			new InboundInterchangeProcessor(logger).ExecuteBatch();
			interchange.Reload();
			AssertEquals(EDIInterchange.Status.Received, interchange.EI_Status);
			AssertEquals(MessageTypeList.Codes.DocumentReviewResponse, interchange.EI_InterchangeType);
			AssertEquals(1, interchange.ContainedMessages.Count);
			var message = interchange.ContainedMessages[0];
			AssertEquals(EDIMessage.Status.Queued, message.EM_Status);
			AssertEquals(MessageTypeList.Codes.DocumentReviewResponse, message.EM_MessageType);
		}
	}
}
