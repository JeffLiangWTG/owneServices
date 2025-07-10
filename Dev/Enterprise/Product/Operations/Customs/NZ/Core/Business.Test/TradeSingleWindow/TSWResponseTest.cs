using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	using CargoWise.EntityFramework.Testing;

	public class TSWResponseTest : TestCaseWithFactory
	{
		class DummyTSWResponse : TSWResponse
		{
			public DummyTSWResponse(BaseTSWResponse response)
				: base(response)
			{
			}

			protected override string GetJobID()
			{
				throw new NotImplementedException();
			}

			protected override string GetJobName()
			{
				throw new NotImplementedException();
			}

			protected override bool GetIsImportEntry()
			{
				throw new NotImplementedException();
			}

			protected override string GetEnterpriseStatus()
			{
				switch (MessageType)
				{
					case TransactionTypeList.Codes.ClearanceInstructions:
						return FormalEntryStatusList.Codes.DeliveryOrderReceived;
					case TransactionTypeList.Codes.Inspection:
						return FormalEntryStatusList.Codes.InspectionsAuditRequirements;
					case TransactionTypeList.Codes.Confirmation:
						return FormalEntryStatusList.Codes.ResponseReceived;
					case TransactionTypeList.Codes.CreditAdvice:
						return FormalEntryStatusList.Codes.CreditAdvice;
					case TransactionTypeList.Codes.Receipt:
						return FormalEntryStatusList.Codes.ResponseReceived;
					case TransactionTypeList.Codes.Error:
						return FormalEntryStatusList.Codes.EntryInError;
					default:
						return FormalEntryStatusList.Codes.ResponseReceived;
				}
			}

			protected override string GetEnterpriseStatusDescription()
			{
				switch (MessageType)
				{
					case TransactionTypeList.Codes.ClearanceInstructions:
						return FormalEntryStatusList.Descriptions.DeliveryOrderReceived;
					case TransactionTypeList.Codes.Inspection:
						return FormalEntryStatusList.Descriptions.InspectionsAuditRequirements;
					case TransactionTypeList.Codes.Confirmation:
						return FormalEntryStatusList.Descriptions.ResponseReceived;
					case TransactionTypeList.Codes.CreditAdvice:
						return FormalEntryStatusList.Descriptions.CreditAdvice;
					case TransactionTypeList.Codes.Receipt:
						return FormalEntryStatusList.Descriptions.ResponseReceived;
					case TransactionTypeList.Codes.Error:
						return FormalEntryStatusList.Descriptions.EntryInError;
					default:
						return FormalEntryStatusList.Descriptions.ResponseReceived;
				}
			}
		}

		public void TestRoleCode()
		{
			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
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
    <IssueDateTime formatCode=""204"">20181017161431</IssueDateTime>
    <FunctionalReferenceID>4285</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>50 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>43350351</ID>
        <AcceptanceDateTime formatCode=""204"">20181017161431</AcceptanceDateTime>
        <FunctionalReferenceID>B00004036</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>D</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">2043.67</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20181017161431</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20181017161431</ReleaseDateTime>
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
			var response = new DummyTSWResponse(baseResponse);
			AssertEquals("MessageNumber", "4285", response.MessageNumber);
			AssertEquals("RoleCode", RoleCodeList.Codes.TB, response.RoleCode);
			Assert("IsForSubmitter", response.IsForSubmitter);
		}

		public void TestEndToEnd()
		{
			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "SENDERS_REFERENCE";

			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns='urn:wco:datamodel:WCO:DM:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
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
        <ID>DECLARATION_ID</ID>
        <FunctionalReferenceID>SENDERS_REFERENCE</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <NameCode>ACK</NameCode>
    </Status>
    <Error>
      <ValidationCode>101</ValidationCode>
    </Error>
    <Error>
      <ValidationCode>102</ValidationCode>
    </Error>
    <Error>
      <ValidationCode>5023</ValidationCode>
    </Error>
    <Error>
      <ValidationCode>5030</ValidationCode>
    </Error>
  </Response>
</DocumentMetadata>";

			BaseTSWResponse baseResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingMessage, out baseResponse));
			var response = new DummyTSWResponse(baseResponse);
			AssertEquals("MessageNumber", "MESSAGE_NUMBER", response.MessageNumber);
			AssertEquals("MessageType", "Clearance / Acceptance Instructions", response.MessageTypeDescription);
			AssertEquals("DeclarationID", "DECLARATION_ID", response.DeclarationID);
			AssertEquals("SendersReference", "SENDERS_REFERENCE", response.SendersReference);
			AssertEquals("OutgoingMessage", outgoingMessage, response.OutgoingMessage);
			AssertEquals("response.Status", "ACK", response.Status);
			AssertEquals("response.StatusDescription", "Receipt Acknowledged", response.StatusDescription);
			AssertEquals("response.EnterpriseStatus", "DOR", response.EnterpriseStatus);
			AssertEquals("response.EnterpriseStatusDescription", "Delivery Order Received", response.EnterpriseStatusDescription);
			AssertEquals("ErrorsWithPointers.Count", 4, response.ErrorsWithPointers.Count());
			AssertEquals("First error", "Flight No. : Not specified", response.ErrorsWithPointers.ElementAt(0).Value);
			AssertEquals("Second error", "Craft Name : Not specified", response.ErrorsWithPointers.ElementAt(1).Value);
			AssertEquals("Third error", "MPI Account Number or MPI Account Name : Not specified or invalid.", response.ErrorsWithPointers.ElementAt(2).Value);
			AssertEquals("Fourth error", "Cancel request is pending - please await requirements", response.ErrorsWithPointers.ElementAt(3).Value);
			AssertEquals("ResponsibleGovernmentAgency", ResponsibleGovernmentAgencyList.Codes.TSW, response.ResponsibleGovernmentAgency);
		}

		public void TestEndToEnd2()
		{
			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.OCR;
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "C00001044";

			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText =
@"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"" >20130513171235</IssueDateTime>
    <FunctionalReferenceID>81</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>00000000</ID>
        <FunctionalReferenceID>C00001044</FunctionalReferenceID>
        <RejectionDateTime formatCode=""204"" >20130513171235</RejectionDateTime>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>1002</ValidationCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>17B</DocumentSectionCode>
        <TagID>R059</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"" >20130513171235</EffectiveDateTime>
      <NameCode>841</NameCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>07B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
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
			BaseTSWResponse baseResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingMessage, out baseResponse));
			var response = new DummyTSWResponse(baseResponse);
			AssertEquals("MessageNumber", "81", response.MessageNumber);
			AssertEquals("MessageType", "Error report", response.MessageTypeDescription);
			AssertEquals("DeclarationID - Invalid number responses should not populate", ZString.Empty, response.DeclarationID);
			AssertEquals("SendersReference", "C00001044", response.SendersReference);
			AssertEquals("OutgoingMessage", outgoingMessage, response.OutgoingMessage);
			AssertEquals("response.Status", "841", response.Status);
			AssertEquals("ErrorsWithPointers.Count", 1, response.ErrorsWithPointers.Count());
			AssertEquals("ResponsibleGovernmentAgency", ResponsibleGovernmentAgencyList.Codes.TSW, response.ResponsibleGovernmentAgency);
		}

		public void TestResponseTime()
		{
			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
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
    <IssueDateTime formatCode=""204"">20131107173534</IssueDateTime>
    <FunctionalReferenceID>499</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>68276411</ID>
        <FunctionalReferenceID>B00001258</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <RejectionDateTime formatCode=""204"">20131107173534</RejectionDateTime>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>699</ValidationCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>03A</DocumentSectionCode>
        <TagID>226</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131107173534</EffectiveDateTime>
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

			Assert("Can parse message", BaseTSWResponse.TryParse(incomingMessage, out var baseResponse));

			var response = new DummyTSWResponse(baseResponse);
			AssertEquals("MessageNumber", "499", response.MessageNumber);

			var expectedResponseTime = new ZDateTime(2013, 11, 07, 17, 35, 34);
			AssertEquals("ResponseTime", expectedResponseTime, response.ResponseTime);

			AssertEquals("AcceptanceTime - message in error, no acceptance time.", ZDateTime.Empty, response.AcceptanceTime);
			AssertEquals("ResponsibleGovernmentAgency", ResponsibleGovernmentAgencyList.Codes.NZCS, response.ResponsibleGovernmentAgency);

			var createTimeUtc = ZDateTime.UtcNow.AddDays(-1);

			incomingMessage.EM_SystemCreateTimeUtc = createTimeUtc;
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText.Replace(@"<IssueDateTime formatCode=""204"">20131107173534</IssueDateTime>", @"<IssueDateTime formatCode=""204""></IssueDateTime>");

			Assert("Can parse message", BaseTSWResponse.TryParse(incomingMessage, out baseResponse));

			response = new DummyTSWResponse(baseResponse);

			expectedResponseTime = EnvProxy.Instance.Time.GetLocalTimeFromUtc(createTimeUtc.ToDateTime());
			AssertEquals("Should return the message created time when the issue date time is empty.", expectedResponseTime, response.ResponseTime);
		}

		public void TestAcceptanceTime()
		{
			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
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
    <IssueDateTime formatCode=""204"">20131107173520</IssueDateTime>
    <FunctionalReferenceID>498</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>68276411</ID>
        <AcceptanceDateTime formatCode=""204"">20131107173520</AcceptanceDateTime>
        <FunctionalReferenceID>B00001258</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131107173520</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20131107173520</ReleaseDateTime>
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

			ZDateTime expectedTime = new ZDateTime(2013, 11, 07, 17, 35, 20);
			BaseTSWResponse baseResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingMessage, out baseResponse));
			var response = new DummyTSWResponse(baseResponse);
			AssertEquals("MessageNumber", "498", response.MessageNumber);
			AssertEquals("ResponseTime", expectedTime, response.ResponseTime);
			AssertEquals("AcceptanceTime", expectedTime, response.AcceptanceTime);
			AssertEquals("ResponsibleGovernmentAgency", ResponsibleGovernmentAgencyList.Codes.MPIFOOD, response.ResponsibleGovernmentAgency);
		}

		public void TestConsignmentStatus()
		{
			var incomingBioMessage = Factory.New<TSWMessage>();
			incomingBioMessage.EM_MessageText = MPIBIOStatusResponse;
			BaseTSWResponse baseResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingBioMessage, out baseResponse));
			var response = new DummyTSWResponse(baseResponse);
			AssertEquals("GoodsStatus - HLD", "HLD", response.GoodsStatus);
			AssertEquals("Clearance Status - HLD", "HLD", response.GoodsClearanceStatus);
			AssertEquals("Movement Status - DTA", "DTA", response.GoodsMovementStatus);
		}

		public void TestICRResponse()
		{
			var incomingBioMessage = Factory.New<TSWMessage>();
			incomingBioMessage.EM_MessageText = MPIBIOResponse;

			ZDateTime expectedTime = new ZDateTime(2018, 02, 28, 15, 24, 10);
			BaseTSWResponse baseResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingBioMessage, out baseResponse));
			var response = new DummyTSWResponse(baseResponse);
			AssertEquals("MessageNumber", "2218", response.MessageNumber);
			AssertEquals("ResponseTime", expectedTime, response.ResponseTime);
			AssertEquals("AcceptanceTime", expectedTime, response.AcceptanceTime);
			AssertEquals("ResponsibleGovernmentAgency", ResponsibleGovernmentAgencyList.Codes.MPIBIO, response.ResponsibleGovernmentAgency);
			AssertEquals("Message Status - B07", StatusList.Codes.MPIBiosecurityCargoReportNotification, response.Status);
			AssertEquals("Goods Status - MDR", "MDR", response.GoodsStatus);

			var incomingCustomsMessage = Factory.New<TSWMessage>();
			incomingCustomsMessage.EM_MessageText = NZCSResponse;

			expectedTime = new ZDateTime(2018, 02, 28, 15, 24, 17);
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingCustomsMessage, out baseResponse));
			response = new DummyTSWResponse(baseResponse);
			AssertEquals("MessageNumber", "2220", response.MessageNumber);
			AssertEquals("ResponseTime", expectedTime, response.ResponseTime);
			AssertEquals("AcceptanceTime", expectedTime, response.AcceptanceTime);
			AssertEquals("ResponsibleGovernmentAgency", ResponsibleGovernmentAgencyList.Codes.NZCS, response.ResponsibleGovernmentAgency);
			AssertEquals("Message Status - C06", StatusList.Codes.CustomsCargoReportNotification, response.Status);
			AssertEquals("Goods Status - WOF", "WOF", response.GoodsStatus);
		}

		public void TestIsForSubmitter()
		{
			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText = MPIBIOStatusResponse;
			BaseTSWResponse baseResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingMessage, out baseResponse));
			var response = new DummyTSWResponse(baseResponse);
			Assert("IsForSubmitter", response.IsForSubmitter);
			AssertEquals("Recipient", "Submitter", response.Recipient);
		}

		public void TestIsForDepot()
		{
			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText = DepotMessage;
			BaseTSWResponse baseResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingMessage, out baseResponse));
			var response = new DummyTSWResponse(baseResponse);
			Assert("IsForDepot", response.IsForDepot);
			AssertEquals("Recipient", "Depot (Location of Goods)", response.Recipient);
		}

		public void TestIsForTranshipmentDest()
		{
			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText = TranshipmentMessage;
			BaseTSWResponse baseResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingMessage, out baseResponse));
			var response = new DummyTSWResponse(baseResponse);
			Assert("IsForTranshipmentDest", response.IsForTranshipmentDest);
			AssertEquals("Recipient", "Transhipment Destination", response.Recipient);
		}

		public void TestIsForNotificationParty()
		{
			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText = DeliveryMessage;
			BaseTSWResponse baseResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingMessage, out baseResponse));
			var response = new DummyTSWResponse(baseResponse);
			Assert("IsForNotificationParty", response.IsForNotificationParty);
			AssertEquals("Recipient", "Delivery Notification Party", response.Recipient);
		}

		public void TestResponseType()
		{
			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
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
    <IssueDateTime formatCode=""204"">20181017161431</IssueDateTime>
    <FunctionalReferenceID>4285</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>50 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>43350351</ID>
        <AcceptanceDateTime formatCode=""204"">20181017161431</AcceptanceDateTime>
        <FunctionalReferenceID>B00004036</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>D</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">2043.67</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20181017161431</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20181017161431</ReleaseDateTime>
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
			var response = new DummyTSWResponse(baseResponse);
			AssertEquals("MessageNumber", "4285", response.MessageNumber);
			AssertEquals("RoleCode", RoleCodeList.Codes.TB, response.RoleCode);
			AssertEquals("ResponseType", "RESIM1", response.ResponseType);
			Assert("IsIM1 response message", response.IsIM1);
		}

		public void TestBillNumbers()
		{
			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
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
    <IssueDateTime formatCode=""204"">20181017161431</IssueDateTime>
    <FunctionalReferenceID>4285</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>50 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>33861696</ID>
        <AcceptanceDateTime formatCode=""204"">20180124192734</AcceptanceDateTime>
        <FunctionalReferenceID>M00001053</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">DTA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08100023925</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>J302399</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <GoodsShipment>
          <Consignment>
            <GoodsLocation>
              <Name>ECT - CCA Wgtn</Name>
            </GoodsLocation>
            <TransportContractDocument>
              <ID>H49024J</ID>
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
            </TransportContractDocument>
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
      </Declaration>
    </OverallDeclaration>
  </Response>
</DocumentMetadata>";

			Assert("Can parse message", BaseTSWResponse.TryParse(incomingMessage, out var baseResponse));
			var response = new DummyTSWResponse(baseResponse);
			CombineAssertions(() =>
			{
				AssertEquals("BillNumber", "08100023925", response.BillNumber);
				AssertEquals("HouseBill", "2314687", response.HouseBill);
				AssertEquals("MasterBillNumber", "H49024J", response.MasterBillNumber);
			});
		}

		#region TestMessages

		public const string MPIBIOStatusResponse =
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
    <IssueDateTime formatCode=""204"">20180124192734</IssueDateTime>
    <FunctionalReferenceID>2053</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>33861696</ID>
        <AcceptanceDateTime formatCode=""204"">20180124192734</AcceptanceDateTime>
        <FunctionalReferenceID>M00001053</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">DTA</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription><![CDATA[seq:1,instructions:'Held pending assessment by MPI']]></StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08100023925</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>J302399</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">DTA</GoodsStatusCode>
          <SequenceNumeric>2</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription><![CDATA[seq:1,instructions:'Held pending assessment by MPI']]></StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08100023925</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>F023982</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <GoodsStatusCode StatusType=""MOVEMENT"">DTA</GoodsStatusCode>
          <SequenceNumeric>3</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription><![CDATA[seq:1,instructions:'Held pending assessment by MP']]></StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08100023925</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>G342039</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180124192734</EffectiveDateTime>
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

		public const string NZCSResponse =
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

		const string DepotMessage = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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

		const string TranshipmentMessage = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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

		const string DeliveryMessage = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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

		#endregion
	}
}
