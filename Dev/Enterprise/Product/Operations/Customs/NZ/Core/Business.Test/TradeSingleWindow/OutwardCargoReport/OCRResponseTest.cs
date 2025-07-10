using System.Linq;
using Enterprise.Customs.Common.NZ;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Messaging.Business;

	public class OCRResponseTest : TestCaseWithFactory
	{
		public void TestEndToEnd()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.PopulateJK_UniqueConsignRefIfNeeded();

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "SENDERS_REFERENCE";
			outgoingMessage.EM_LinkedObject = consol;

			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText =
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
    <IssueDateTime formatCode=""204"" >20130328173928</IssueDateTime>
    <FunctionalReferenceID>MESSAGE_NUMBER</FunctionalReferenceID>
		<FunctionCode>24</FunctionCode>
		<OverallDeclaration>
			<Declaration>
				<FunctionalReferenceID>SENDERS_REFERENCE</FunctionalReferenceID>
				<Consignment>
					<AdditionalInformation>
						<StatementDescription>Customs instruction 1</StatementDescription>
						<StatementTypeCode>ICN</StatementTypeCode>
					</AdditionalInformation>
				</Consignment>
				<Consignment>
					<AdditionalInformation>
						<StatementDescription>Customs instruction 2</StatementDescription>
						<StatementTypeCode>ICN</StatementTypeCode>
					</AdditionalInformation>
				</Consignment>
			</Declaration>
		</OverallDeclaration>
		<Status>
			<NameCode>830</NameCode>
		</Status>
	</Response>
</DocumentMetadata>";

			BaseTSWResponse baseResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingMessage, out baseResponse));
			var response = new OCRResponse(baseResponse);
			AssertEquals("Processing object", "Consol", response.JobName);
			AssertEquals("response.EnterpriseStatus", OutwardReportStatusList.Codes.Cleared, response.EnterpriseStatus);
			AssertEquals("response.EnterpriseStatusDescription", "Outward Report Accepted", response.EnterpriseStatusDescription);
			AssertEquals("CustomsInstructions.Count", 2, response.CustomsInstructions.Count());
			AssertEquals("First instruction", "Customs instruction 1", response.CustomsInstructions.ElementAt(0));
			AssertEquals("Second instruction", "Customs instruction 2", response.CustomsInstructions.ElementAt(1));
		}

		public void TestAcknowledgementResponse()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00001173";

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "C00001173";
			outgoingMessage.EM_LinkedObject = consol;

			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText =
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

			BaseTSWResponse baseResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingMessage, out baseResponse));
			var response = new OCRResponse(baseResponse);
			AssertEquals("response.EnterpriseStatus for Acknowledgement message", OutwardReportStatusList.Codes.Acknowledgement, response.EnterpriseStatus);
			AssertEquals("response.EnterpriseStatusDescription", "OCR Acknowledged", response.EnterpriseStatusDescription);
		}

		public void TestGetEventReference()
		{
			AssertOCRResponseEventReference("815", "ACC");
			AssertOCRResponseEventReference("830", "ACC");
			AssertOCRResponseEventReference("847", "ACC");
			AssertOCRResponseEventReference("841", "FAL");
			AssertOCRResponseEventReference("858", "FAL");
			AssertOCRResponseEventReference("814", "CAN");
			AssertOCRResponseEventReference("801", "");
		}

		public void AssertOCRResponseEventReference(string status, string reference)
		{
			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText =
$@"<?xml version=""1.0"" encoding=""UTF-8""?>
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
      <NameCode>{status}</NameCode>
    </Status>
  </Response>
</DocumentMetadata>";
			BaseTSWResponse.TryParse(incomingMessage, out var baseTswResponse);
			var ocrResponse = new OCRResponse(baseTswResponse);
			AssertEquals(status, ocrResponse.Status);
			AssertEquals(reference, ocrResponse.GetEventReference());
		}
	}
}
