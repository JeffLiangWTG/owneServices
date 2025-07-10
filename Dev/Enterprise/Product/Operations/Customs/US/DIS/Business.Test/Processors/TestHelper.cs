using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using USCustoms = Enterprise.Integration.Customs.US;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	public class TestHelper
	{
		public TestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}
		readonly BusinessObjectFactory factory;

		public BusinessObject GetJobDeclaration()
		{
			var importer = factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "IMPORTER";

			var result = (BusinessObject)factory.New<USCustoms.IJobDeclaration>();
			result[JobDeclarationSchema.JE_MessageType.Name] = "IMP";
			result[JobDeclarationSchema.JE_ApplicationCode.Name] = "ACE";
			result[JobDeclarationSchema.JE_DeclarationReference] = "B00001000";
			result[JobDeclarationSchema.JE_OH_Importer] = importer.PK;

			return result;
		}

		public const string DocumentSubmissionXml = @"<?xml version=""1.0"" encoding=""utf-16""?>
<DIS:MessageEnvelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:DIS=""http://cbp.dhs.gov/DIS"">
  <DIS:MessageHeader>
    <DIS:MessageID>HYEDUSCMT_124</DIS:MessageID>
    <DIS:MessageType>DocumentSubmission</DIS:MessageType>
    <DIS:SentDateTime>2016-02-04T14:42:47.92+11:00</DIS:SentDateTime>
    <DIS:TransmitterID />
    <DIS:TransmitterSiteCode />
    <DIS:PreparerID>SV9</DIS:PreparerID>
    <DIS:PreparerSiteCode>3901</DIS:PreparerSiteCode>
  </DIS:MessageHeader>
  <DIS:MessageBody>
    <DIS:DocumentSubmissionPackage>
      <DIS:PackageIdentifier>
        <DIS:ImporterOfRecordNbr>123-45-1456</DIS:ImporterOfRecordNbr>
      </DIS:PackageIdentifier>
      <DIS:ActionCode>REPLACE</DIS:ActionCode>
      <DIS:TradeTransaction>
        <DIS:TransactionCategory>SINGLE_TXN</DIS:TransactionCategory>
        <DIS:EntrySummary>
          <DIS:EntryNumber>70041213</DIS:EntryNumber>
          <DIS:Filer>SV9</DIS:Filer>
          <DIS:ReferenceNumber>B00001000</DIS:ReferenceNumber>
        </DIS:EntrySummary>
      </DIS:TradeTransaction>
      <DIS:CBPRequest>
        <DIS:CBPRequestID>123456789012</DIS:CBPRequestID>
        <DIS:CBPRequestType>ACEActionNumber</DIS:CBPRequestType>
      </DIS:CBPRequest>
      <DIS:DocumentData>
        <DIS:DocumentHeader>
          <DIS:DocumentID>B00001000_DIS1</DIS:DocumentID>
          <DIS:DocumentLabel>Generic Document</DIS:DocumentLabel>
          <DIS:CompleteFileName>B00001000_DIS1.pdf</DIS:CompleteFileName>
          <DIS:FileExtensionType>pdf</DIS:FileExtensionType>
          <DIS:DocumentDescription>Your United States Demo Company - PHL - Entry Summ</DIS:DocumentDescription>
          <DIS:DocPreviouslySubmitted>Y</DIS:DocPreviouslySubmitted>
        </DIS:DocumentHeader>
        <DIS:GovtAgencyList>
          <DIS:GovtAgency>CBP</DIS:GovtAgency>
        </DIS:GovtAgencyList>
        <DIS:DocumentObject>!dOcUmEnTiMaGePlAcEHoLdEr:9d0be2ca-4eab-49eb-a19b-bba232bf3f11!</DIS:DocumentObject>
      </DIS:DocumentData>
    </DIS:DocumentSubmissionPackage>
  </DIS:MessageBody>
</DIS:MessageEnvelope>";

		public const string BondSubmissionXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<!--Sample DIS XML file created by CBP TASPO - POCs: Shailesh Sardesai and Ed Jenkins (CUSTOMS & BORDER PROTECTION) -->
<DIS:MessageEnvelope xsi:schemaLocation=""http://cbp.dhs.gov/DIS ../MessageEnvelope.xsd"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:DIS=""http://cbp.dhs.gov/DIS"" xmlns=""http://cbp.dhs.gov/DIS""><DIS:MessageHeader><DIS:MessageID>200</DIS:MessageID><DIS:MessageType>DocumentSubmission</DIS:MessageType><DIS:SentDateTime>2010-07-28T09:30:47-04:00</DIS:SentDateTime><DIS:TransmitterID>ABC</DIS:TransmitterID><DIS:TransmitterSiteCode>AVLToCBP01</DIS:TransmitterSiteCode><DIS:PreparerID>ABC</DIS:PreparerID><DIS:PreparerSiteCode>AVLToCBP01</DIS:PreparerSiteCode></DIS:MessageHeader><DIS:MessageBody><DIS:DocumentSubmissionPackage><DIS:SubmittedToPortCode>AV-1240</DIS:SubmittedToPortCode><DIS:ActionCode>ADD</DIS:ActionCode><DIS:TradeTransaction><DIS:TransactionCategory>SINGLE_TXN</DIS:TransactionCategory><DIS:Entry><DIS:EntryNumber>444782120</DIS:EntryNumber><DIS:Filer>562</DIS:Filer></DIS:Entry></DIS:TradeTransaction><DIS:CBPRequest><DIS:CBPRequestID>221230001</DIS:CBPRequestID><DIS:CBPRequestType>ACEActionNumber</DIS:CBPRequestType><DIS:CBPRequestDate>2010-07-22T09:30:47-04:00</DIS:CBPRequestDate></DIS:CBPRequest><DIS:DocumentData><DIS:DocumentHeader><DIS:DocumentID>AV-1241</DIS:DocumentID><DIS:DocumentLabel>EPA TSCA Certification</DIS:DocumentLabel><DIS:CompleteFileName>444782120_TSCA.pdf</DIS:CompleteFileName><DIS:FileExtensionType>pdf</DIS:FileExtensionType><DIS:DocumentDescription>TSCA Certification</DIS:DocumentDescription></DIS:DocumentHeader><DIS:GovtAgencyList><DIS:GovtAgency>CBP</DIS:GovtAgency><DIS:GovtAgency>FDA</DIS:GovtAgency></DIS:GovtAgencyList><DIS:Comment>Bond covers entire amount of transaction. Please contact John Smith at 703-287-2298 for further clarification if needed</DIS:Comment><DIS:OptionalData><DIS:ToxicSubstancesData><DIS:CASNbr>8191277761</DIS:CASNbr><DIS:EPARegistrationNbr>CS-81779181</DIS:EPARegistrationNbr><DIS:EPAProducerEstNbr>SST-182821</DIS:EPAProducerEstNbr></DIS:ToxicSubstancesData><DIS:CommodityList><DIS:CommodityData><DIS:HTSNumber>6680</DIS:HTSNumber><DIS:CommodityDescription>Motor Parts</DIS:CommodityDescription><DIS:CountryOfOrigin>PK</DIS:CountryOfOrigin><DIS:PortOfLading>53551</DIS:PortOfLading><DIS:PortOfEntry>1401</DIS:PortOfEntry><DIS:TradeParties><DIS:TradeParty><DIS:TradePartyID>8817882002</DIS:TradePartyID><DIS:TradePartyType>MANUFACTURER</DIS:TradePartyType></DIS:TradeParty></DIS:TradeParties></DIS:CommodityData></DIS:CommodityList></DIS:OptionalData><DIS:DocumentObject> /9j/4AAQSkZJRgABAQEAeAB4AAD/2wBDAAgGBgcGBQgHBwcJCQgKDBQNDAsLDBkSEw8UHRofHh0aHBwgJC4nICIsIxwcKDcpLDAxNDQ0Hyc5PTgyPC4zNDL/2wBDAQkJCQwLDBgNDRgyIRwhMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjIyMjL/wAARCAMFA6QDASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmqKjpKWmp6ipqrKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZJhtVQrx4LE/ux1yCegfTrx1uT5BBk1WG5UFlz5a+Xk9e21uDtYAyNkYOQOeBgAuaf4Ht7HTE0+TWNUu7aK0+xW6zPEvkREBSF8uNckgAbmyR2Iyc2G8F6Kmn6rYWVqun2mp2htZ4bJEiTBDKXChcB8ORnnoM9Ku3rC91W305W+WLbdXAH90H92p9MuCfpGR3rBivfCkvjgJa3enJrCzPHKUuAbqZwjZjKg7tijJweAVGB3oA2dR0GW81I39rrWo6dK0KwuLVYGDqpYjPmxPyC7dMVsjp61xvhu98KXXiSdtDu9ONy8cnmJa3AeWf5l3SSgEng4ALc/MelX3msX07Vdc1WYJp0sRjVi5QLbJnnIP8RLNk </DIS:DocumentObject></DIS:DocumentData></DIS:DocumentSubmissionPackage></DIS:MessageBody></DIS:MessageEnvelope>";

		public const string DocumentValidationResponseFailedXml = @"<DIS:MessageEnvelope xmlns=""http://cbp.dhs.gov/DIS"" xmlns:DIS=""http://cbp.dhs.gov/DIS"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><DIS:MessageHeader><DIS:MessageID>292393908</DIS:MessageID><DIS:MessageType>DocumentValidationResponse</DIS:MessageType><DIS:SentDateTime>2014-05-27T22:14:48.231-04:00</DIS:SentDateTime><DIS:TransmitterID /><DIS:TransmitterSiteCode /><DIS:PreparerID>SV9</DIS:PreparerID><DIS:PreparerSiteCode>3910</DIS:PreparerSiteCode>
      </DIS:MessageHeader><DIS:MessageBody><DIS:MessageValidationResponse>
         <DIS:MessageLevelResult>
            <DIS:ProcessedMessageHeader xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
    <DIS:MessageID>HYEDUSDAT_13</DIS:MessageID>
    <DIS:MessageType>DocumentSubmission</DIS:MessageType>
    <DIS:SentDateTime>2014-05-27T17:22:35.757+10:00</DIS:SentDateTime>
    <DIS:TransmitterID>SV9</DIS:TransmitterID>
    <DIS:TransmitterSiteCode>3910</DIS:TransmitterSiteCode>
    <DIS:PreparerID>SV9</DIS:PreparerID>
    <DIS:PreparerSiteCode>3910</DIS:PreparerSiteCode>
  </DIS:ProcessedMessageHeader>
            <DIS:MessageProcessingResult>
               <DIS:ProcessingEvent>INITIAL_VALIDATION</DIS:ProcessingEvent>
               <DIS:ProcessingStatus>FAILED</DIS:ProcessingStatus>
               <DIS:ProcessingLogText>Message processing failed while validating DocumentData</DIS:ProcessingLogText>
            </DIS:MessageProcessingResult>
         </DIS:MessageLevelResult>
      </DIS:MessageValidationResponse></DIS:MessageBody>
   </DIS:MessageEnvelope>";

		public const string DocumentValidationPassedResponseXml = @"<DIS:MessageEnvelope xmlns=""http://cbp.dhs.gov/DIS"" xmlns:DIS=""http://cbp.dhs.gov/DIS"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <DIS:MessageHeader>
    <DIS:MessageID>320487132</DIS:MessageID>
    <DIS:MessageType>DocumentValidationResponse</DIS:MessageType>
    <DIS:SentDateTime>2014-06-06T01:54:08.640-04:00</DIS:SentDateTime>
    <DIS:TransmitterID />
    <DIS:TransmitterSiteCode />
    <DIS:PreparerID>SV9</DIS:PreparerID>
    <DIS:PreparerSiteCode>3910</DIS:PreparerSiteCode>
  </DIS:MessageHeader>
  <DIS:MessageBody>
    <DIS:MessageValidationResponse>
      <DIS:MessageLevelResult>
        <DIS:ProcessedMessageHeader xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
          <DIS:MessageID>HYEDUSDAT_25</DIS:MessageID>
          <DIS:MessageType>DocumentSubmission</DIS:MessageType>
          <DIS:SentDateTime>2014-06-06T00:51:37.447+10:00</DIS:SentDateTime>
          <DIS:TransmitterID>SV9</DIS:TransmitterID>
          <DIS:TransmitterSiteCode>3910</DIS:TransmitterSiteCode>
          <DIS:PreparerID>SV9</DIS:PreparerID>
          <DIS:PreparerSiteCode>3910</DIS:PreparerSiteCode>
        </DIS:ProcessedMessageHeader>
        <DIS:MessageProcessingResult>
          <DIS:ProcessingEvent>INITIAL_VALIDATION</DIS:ProcessingEvent>
          <DIS:ProcessingStatus>PASSED</DIS:ProcessingStatus>
          <DIS:ProcessingLogText>Submitted message received successfully in DIS. Ready for Review</DIS:ProcessingLogText>
        </DIS:MessageProcessingResult>
      </DIS:MessageLevelResult>
      <DIS:DocumentLevelResult>
        <DIS:ProcessedDocumentHeader xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
          <DIS:DocumentID>B00001000_DIS1</DIS:DocumentID>
          <DIS:DocumentLabel>Generic Document</DIS:DocumentLabel>
          <DIS:CompleteFileName>B00001000_DIS1.pdf</DIS:CompleteFileName>
          <DIS:FileExtensionType>pdf</DIS:FileExtensionType>
          <DIS:DocumentDescription>bill of lading</DIS:DocumentDescription>
          <DIS:DocPreviouslySubmitted>N</DIS:DocPreviouslySubmitted>
        </DIS:ProcessedDocumentHeader>
        <DIS:DocumentProcessingResult>
          <DIS:ProcessingEvent>INITIAL_VALIDATION</DIS:ProcessingEvent>
          <DIS:ProcessingStatus>PASSED</DIS:ProcessingStatus>
          <DIS:ProcessingLogText>No errors found for document</DIS:ProcessingLogText>
        </DIS:DocumentProcessingResult>
      </DIS:DocumentLevelResult>
    </DIS:MessageValidationResponse>
  </DIS:MessageBody>
</DIS:MessageEnvelope>";

		public const string DocumentReviewRejectedResponseXml = @"<MessageEnvelope xmlns=""http://cbp.dhs.gov/DIS""
    xmlns:DIS=""http://cbp.dhs.gov/DIS"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <DIS:MessageHeader>
        <DIS:MessageID>24385168</DIS:MessageID>
        <DIS:MessageType>DocumentSubmission</DIS:MessageType>
        <DIS:SentDateTime>2016-06-23T11:30:26</DIS:SentDateTime>
        <DIS:TransmitterID>SV9</DIS:TransmitterID>
        <DIS:TransmitterSiteCode>3910</DIS:TransmitterSiteCode>
        <DIS:PreparerID>SV9</DIS:PreparerID>
        <DIS:PreparerSiteCode>1101</DIS:PreparerSiteCode>
    </DIS:MessageHeader>
    <DIS:MessageBody>
        <DIS:DocumentReviewResponse>
            <DIS:DocumentHeader>
                <DIS:DocumentID>BCH140007522_DIS1</DIS:DocumentID>
                <DIS:DocumentLabel>COMMERCIAL_INVOICE</DIS:DocumentLabel>
                <DIS:CompleteFileName>BCH140007522_DIS1-ModifyABIMessage.pdf</DIS:CompleteFileName>
                <DIS:FileExtensionType>pdf</DIS:FileExtensionType>
                <DIS:DocumentDescription></DIS:DocumentDescription>
            </DIS:DocumentHeader>
            <DIS:SubmittedToPortCode>1101</DIS:SubmittedToPortCode>
            <DIS:TradeTransaction>
                <DIS:TransactionCategory>SINGLE_TXN</DIS:TransactionCategory>
                <DIS:Entry>
                    <DIS:EntryNumber>71021438</DIS:EntryNumber>
                    <DIS:Filer>SV9</DIS:Filer>
                </DIS:Entry>
            </DIS:TradeTransaction>
            <DIS:DocumentReviewResult>
                <DIS:ProcessingEvent>REVIEW</DIS:ProcessingEvent>
                <DIS:DocumentReviewStatus>rejected</DIS:DocumentReviewStatus>
                <DIS:DocumentReviewComment>BTA anticipated arrival information is missing</DIS:DocumentReviewComment>
                <DIS:DocumentRejectReason>INCOMPLETE_DOCUMENT_SET</DIS:DocumentRejectReason>
            </DIS:DocumentReviewResult>
        </DIS:DocumentReviewResponse>
    </DIS:MessageBody>
</MessageEnvelope>";

		public const string DocumentReviewAcceptedResponseXml = @"<MessageEnvelope xmlns=""http://cbp.dhs.gov/DIS""
    xmlns:DIS=""http://cbp.dhs.gov/DIS"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <DIS:MessageHeader>
        <DIS:MessageID>24385168</DIS:MessageID>
        <DIS:MessageType>DocumentSubmission</DIS:MessageType>
        <DIS:SentDateTime>2016-06-23T11:30:26</DIS:SentDateTime>
        <DIS:TransmitterID>SV9</DIS:TransmitterID>
        <DIS:TransmitterSiteCode>3910</DIS:TransmitterSiteCode>
        <DIS:PreparerID>SV9</DIS:PreparerID>
        <DIS:PreparerSiteCode>1101</DIS:PreparerSiteCode>
    </DIS:MessageHeader>
    <DIS:MessageBody>
        <DIS:DocumentReviewResponse>
            <DIS:DocumentHeader>
                <DIS:DocumentID>BCH140007522_DIS1</DIS:DocumentID>
                <DIS:DocumentLabel>COMMERCIAL_INVOICE</DIS:DocumentLabel>
                <DIS:CompleteFileName>BCH140007522_DIS1-ModifyABIMessage.pdf</DIS:CompleteFileName>
                <DIS:FileExtensionType>pdf</DIS:FileExtensionType>
                <DIS:DocumentDescription></DIS:DocumentDescription>
            </DIS:DocumentHeader>
            <DIS:SubmittedToPortCode>1101</DIS:SubmittedToPortCode>
            <DIS:TradeTransaction>
                <DIS:TransactionCategory>SINGLE_TXN</DIS:TransactionCategory>
                <DIS:Entry>
                    <DIS:EntryNumber>71021438</DIS:EntryNumber>
                    <DIS:Filer>SV9</DIS:Filer>
                </DIS:Entry>
            </DIS:TradeTransaction>
            <DIS:DocumentReviewResult>
                <DIS:ProcessingEvent>REVIEW</DIS:ProcessingEvent>
                <DIS:DocumentReviewStatus>ACCEPTED</DIS:DocumentReviewStatus>
            </DIS:DocumentReviewResult>
        </DIS:DocumentReviewResponse>
    </DIS:MessageBody>
</MessageEnvelope>";

		public const string DocumentReviewUnderReviewResponseXml = @"<MessageEnvelope xmlns=""http://cbp.dhs.gov/DIS""
    xmlns:DIS=""http://cbp.dhs.gov/DIS"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <DIS:MessageHeader>
        <DIS:MessageID>24385168</DIS:MessageID>
        <DIS:MessageType>DocumentSubmission</DIS:MessageType>
        <DIS:SentDateTime>2016-06-23T11:30:26</DIS:SentDateTime>
        <DIS:TransmitterID>SV9</DIS:TransmitterID>
        <DIS:TransmitterSiteCode>3910</DIS:TransmitterSiteCode>
        <DIS:PreparerID>SV9</DIS:PreparerID>
        <DIS:PreparerSiteCode>1101</DIS:PreparerSiteCode>
    </DIS:MessageHeader>
    <DIS:MessageBody>
        <DIS:DocumentReviewResponse>
            <DIS:DocumentHeader>
                <DIS:DocumentID>BCH140007522_DIS1</DIS:DocumentID>
                <DIS:DocumentLabel>COMMERCIAL_INVOICE</DIS:DocumentLabel>
                <DIS:CompleteFileName>BCH140007522_DIS1-ModifyABIMessage.pdf</DIS:CompleteFileName>
                <DIS:FileExtensionType>pdf</DIS:FileExtensionType>
                <DIS:DocumentDescription></DIS:DocumentDescription>
            </DIS:DocumentHeader>
            <DIS:SubmittedToPortCode>1101</DIS:SubmittedToPortCode>
            <DIS:TradeTransaction>
                <DIS:TransactionCategory>SINGLE_TXN</DIS:TransactionCategory>
                <DIS:Entry>
                    <DIS:EntryNumber>71021438</DIS:EntryNumber>
                    <DIS:Filer>SV9</DIS:Filer>
                </DIS:Entry>
            </DIS:TradeTransaction>
            <DIS:DocumentReviewResult>
                <DIS:ProcessingEvent>REVIEW</DIS:ProcessingEvent>
                <DIS:DocumentReviewStatus>UNDER_REVIEW</DIS:DocumentReviewStatus>
            </DIS:DocumentReviewResult>
        </DIS:DocumentReviewResponse>
    </DIS:MessageBody>
</MessageEnvelope>";

		public const string DocumentReviewEmptyStatusResponseXml = @"<MessageEnvelope xmlns=""http://cbp.dhs.gov/DIS""
    xmlns:DIS=""http://cbp.dhs.gov/DIS"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <DIS:MessageHeader>
        <DIS:MessageID>24385168</DIS:MessageID>
        <DIS:MessageType>DocumentSubmission</DIS:MessageType>
        <DIS:SentDateTime>2016-06-23T11:30:26</DIS:SentDateTime>
        <DIS:TransmitterID>SV9</DIS:TransmitterID>
        <DIS:TransmitterSiteCode>3910</DIS:TransmitterSiteCode>
        <DIS:PreparerID>SV9</DIS:PreparerID>
        <DIS:PreparerSiteCode>1101</DIS:PreparerSiteCode>
    </DIS:MessageHeader>
    <DIS:MessageBody>
        <DIS:DocumentReviewResponse>
            <DIS:DocumentHeader>
                <DIS:DocumentID>BCH140007522_DIS1</DIS:DocumentID>
                <DIS:DocumentLabel>COMMERCIAL_INVOICE</DIS:DocumentLabel>
                <DIS:CompleteFileName>BCH140007522_DIS1-ModifyABIMessage.pdf</DIS:CompleteFileName>
                <DIS:FileExtensionType>pdf</DIS:FileExtensionType>
                <DIS:DocumentDescription></DIS:DocumentDescription>
            </DIS:DocumentHeader>
            <DIS:SubmittedToPortCode>1101</DIS:SubmittedToPortCode>
            <DIS:TradeTransaction>
                <DIS:TransactionCategory>SINGLE_TXN</DIS:TransactionCategory>
                <DIS:Entry>
                    <DIS:EntryNumber>71021438</DIS:EntryNumber>
                    <DIS:Filer>SV9</DIS:Filer>
                </DIS:Entry>
            </DIS:TradeTransaction>
            <DIS:DocumentReviewResult>
                <DIS:ProcessingEvent>REVIEW</DIS:ProcessingEvent>
            </DIS:DocumentReviewResult>
        </DIS:DocumentReviewResponse>
    </DIS:MessageBody>
</MessageEnvelope>";

		public const string ISFDocumentReviewResponseXml = @"<MessageEnvelope xmlns=""http://cbp.dhs.gov/DIS""
    xmlns:DIS=""http://cbp.dhs.gov/DIS"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
    <DIS:MessageHeader>
        <DIS:MessageID>24385168</DIS:MessageID>
        <DIS:MessageType>DocumentSubmission</DIS:MessageType>
        <DIS:SentDateTime>2016-06-23T11:30:26</DIS:SentDateTime>
        <DIS:TransmitterID>SV9</DIS:TransmitterID>
        <DIS:TransmitterSiteCode>3910</DIS:TransmitterSiteCode>
        <DIS:PreparerID>SV9</DIS:PreparerID>
        <DIS:PreparerSiteCode>1101</DIS:PreparerSiteCode>
    </DIS:MessageHeader>
    <DIS:MessageBody>
        <DIS:DocumentReviewResponse>
            <DIS:DocumentHeader>
                <DIS:DocumentID>ISF140007522_DIS1</DIS:DocumentID>
                <DIS:DocumentLabel>COMMERCIAL_INVOICE</DIS:DocumentLabel>
                <DIS:CompleteFileName>BCH140007522_DIS1-ModifyABIMessage.pdf</DIS:CompleteFileName>
                <DIS:FileExtensionType>pdf</DIS:FileExtensionType>
                <DIS:DocumentDescription></DIS:DocumentDescription>
            </DIS:DocumentHeader>
            <DIS:SubmittedToPortCode>1101</DIS:SubmittedToPortCode>
            <DIS:TradeTransaction>
                <DIS:TransactionCategory>SINGLE_TXN</DIS:TransactionCategory>
                <DIS:ISFNumber>
                    <DIS:ISFNumber>XJ5-92496379926</DIS:ISFNumber>
                </DIS:ISFNumber>
            </DIS:TradeTransaction>
            <DIS:DocumentReviewResult>
                <DIS:ProcessingEvent>REVIEW</DIS:ProcessingEvent>
                <DIS:DocumentReviewStatus>REJECTED</DIS:DocumentReviewStatus>
                <DIS:DocumentReviewComment>BTA anticipated arrival information is missing</DIS:DocumentReviewComment>
                <DIS:DocumentRejectReason>INCOMPLETE_DOCUMENT_SET</DIS:DocumentRejectReason>
            </DIS:DocumentReviewResult>
        </DIS:DocumentReviewResponse>
    </DIS:MessageBody>
</MessageEnvelope>";
	}
}
