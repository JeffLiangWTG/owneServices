using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.TradeSingleWindow;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NZ.Business.TradeSingleWindow.Testing
{
	class DeclarationResponseForTest : DeclarationResponse
	{
		public DeclarationResponseForTest(BaseTSWResponse response)
			: base(response)
		{
		}

		protected override string GetJobName()
		{
			return "Declaration";
		}

		protected override bool GetIsImportEntry() => true;
	}

	public class DeclarationResponseTest : TestCaseWithFactory
	{
		public void TestEndToEnd()
		{
			var header = Factory.New<CusEntryHeader>();
			var line1 = header.MergedLines.AddNew();
			line1.CL_LineNumber = 1;
			var line2 = header.MergedLines.AddNew();
			line2.CL_LineNumber = 2;
			var line3 = header.MergedLines.AddNew();
			line3.CL_LineNumber = 3;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "SENDERS_REFERENCE";
			outgoingMessage.EM_LinkedObject = header;

			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText =
			#region Message Text
 @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns='urn:wco:datamodel:WCO:DM:1' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESI10</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response>
    <IssueDateTime formatCode=""204"" >20130403161149</IssueDateTime>
    <FunctionalReferenceID>17</FunctionalReferenceID> 
		<FunctionCode>24</FunctionCode>
		<AdditionalInformation>
			<StatementTypeCode>DIN</StatementTypeCode>
			<StatementDescription>Delivery instruction 1</StatementDescription>
		</AdditionalInformation>
		<AdditionalInformation>
			<StatementTypeCode>DIN</StatementTypeCode>
			<StatementDescription>Delivery instruction 2</StatementDescription>
		</AdditionalInformation>
		<OverallDeclaration>
			<Declaration>
				<FunctionalReferenceID>SENDERS_REFERENCE</FunctionalReferenceID>
				<DutyTaxFee>
					<Payment>
						<MethodCode>C</MethodCode>
                    </Payment>
                </DutyTaxFee>
               <DutyTaxFee>
                    <TypeCode>TOT</TypeCode>
                    <Payment>
						<TaxAssessedAmount>999.55</TaxAssessedAmount>
					</Payment>
				</DutyTaxFee>
			</Declaration>
		</OverallDeclaration>
		<Error>
			<ValidationCode>153</ValidationCode>
			<Pointer>
				<SequenceNumeric>1</SequenceNumeric>
				<DocumentSectionCode>42A</DocumentSectionCode>
			</Pointer>
			<Pointer>
				<DocumentSectionCode>67A</DocumentSectionCode>
			</Pointer>
			<Pointer>
				<SequenceNumeric>1</SequenceNumeric>
				<DocumentSectionCode>68A</DocumentSectionCode>
			</Pointer>
			<Pointer>
				<DocumentSectionCode>92A</DocumentSectionCode>
			</Pointer>
			<Pointer>
				<DocumentSectionCode>92A</DocumentSectionCode>
				<TagID>063</TagID>
			</Pointer>
		</Error>
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
			</Pointer>
			<Pointer>
				<DocumentSectionCode>92A</DocumentSectionCode>
				<TagID>063</TagID>
			</Pointer>
		</Error>
		<Status>
			<NameCode>830</NameCode>
		</Status>
	</Response>
</DocumentMetadata>";
			#endregion // Message Text

			BaseTSWResponse baseResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingMessage, out baseResponse));
			var response = new DeclarationResponseForTest(baseResponse);
			AssertEquals("Header", header, response.EntryHeader);
			AssertEquals("DeliveryInstructions.Count", 2, response.DeliveryInstructions.Count());
			AssertEquals("First instruction", "Delivery instruction 1", response.DeliveryInstructions.ElementAt(0));
			AssertEquals("Second instruction", "Delivery instruction 2", response.DeliveryInstructions.ElementAt(1));
			AssertEquals("response.EnterpriseStatusDescription", "New Zealand Customs Service - Clearance / Acceptance Instructions", response.EnterpriseStatusDescription);
			AssertEquals("response.PaymentMethod", "C", response.PaymentMethod);
			AssertEquals("response.PaymentMethodDescription", "Cash", response.PaymentMethodDescription);
			AssertEquals("response.HasTotalAmount", true, response.NullableTotalAmount.HasValue);
			AssertEquals("response.TotalAmount", 999.55M, response.TotalAmount);
			AssertEquals("EntryLinesWithErrors.Count", 2, response.EntryLinesWithErrors.Count());
			AssertEquals("First error line", line1, response.EntryLinesWithErrors.ElementAt(0));
			AssertEquals("Second error line", line3, response.EntryLinesWithErrors.ElementAt(1));
		}

		public void TestProcessAllLodgementResponses()
		{
			var header = Factory.New<CusEntryHeader>();
			var line1 = header.MergedLines.AddNew();
			line1.CL_LineNumber = 1;
			var line2 = header.MergedLines.AddNew();
			line2.CL_LineNumber = 2;
			var line3 = header.MergedLines.AddNew();
			line3.CL_LineNumber = 3;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "B00001252";
			outgoingMessage.EM_LinkedObject = header;

			var incomingMessageMPIFood = Factory.New<TSWMessage>();
			incomingMessageMPIFood.EM_MessageText = MPIFoodResponse;

			var incomingMessageMPIBIO = Factory.New<TSWMessage>();
			incomingMessageMPIBIO.EM_MessageText = MPIBIOResponse;

			var incomingMessageNZCS = Factory.New<TSWMessage>();
			incomingMessageNZCS.EM_MessageText = NZCSResponse;

			BaseTSWResponse foodResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingMessageMPIFood, out foodResponse));
			var mpiFood = new DeclarationResponseForTest(foodResponse);

			BaseTSWResponse bioResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingMessageMPIBIO, out bioResponse));
			var mpiBio = new DeclarationResponseForTest(bioResponse);

			BaseTSWResponse customsResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingMessageNZCS, out customsResponse));
			var nzcs = new DeclarationResponseForTest(customsResponse);

			AssertEquals("Header should be linked to all 3 message responses - MPI Food", header, mpiFood.EntryHeader);
			AssertEquals("Header should be linked to all 3 message responses - MPI Biosecurity", header, mpiBio.EntryHeader);
			AssertEquals("Header should be linked to all 3 message responses - NZCS", header, nzcs.EntryHeader);

			AssertEquals("MPIFood - EnterpriseStatus", "0", mpiFood.EnterpriseStatus);
			AssertEquals("MPIBIO - EnterpriseStatus", "0", mpiBio.EnterpriseStatus);
			AssertEquals("NZCS - EnterpriseStatus - Pending Payment", "6", nzcs.EnterpriseStatus);

			AssertEquals("DeliveryInstructions", "", header.CH_CustomsDeliveryInstructions);

			AssertEquals("nzcs response.PaymentMethod", "C", nzcs.PaymentMethod);
			AssertEquals("nzcs response.PaymentMethodDescription", "Cash", nzcs.PaymentMethodDescription);
			AssertEquals("mpiFood response.HasTotalAmount", false, mpiFood.NullableTotalAmount.HasValue);
			AssertEquals("mpiFood response.TotalAmount", 0M, mpiFood.TotalAmount);
			AssertEquals("mpiBio response.HasTotalAmount", true, mpiBio.NullableTotalAmount.HasValue);
			AssertEquals("mpiBio response.TotalAmount", 0M, mpiBio.TotalAmount);
			AssertEquals("nzcs response.HasTotalAmount", true, nzcs.NullableTotalAmount.HasValue);
			AssertEquals("nzcs response.TotalAmount", 441.57M, nzcs.TotalAmount);
		}

		public void TestIsTSWAcknowledgementResponse()
		{
			BaseTSWResponse baseResponse;
			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText = MPIFoodResponse;
			BaseTSWResponse.TryParse(incomingMessage, out baseResponse);
			var response = new DeclarationResponseForTest(baseResponse);
			AssertEquals("IsTSWAcknowledgementResponse", false, response.IsTSWAcknowledgementResponse);

			incomingMessage.EM_MessageText = AckResponse;
			BaseTSWResponse.TryParse(incomingMessage, out baseResponse);
			var responseACK = new DeclarationResponseForTest(baseResponse);
			AssertEquals("IsTSWResponse", true, responseACK.IsTSWAcknowledgementResponse);
		}

		public void TestIsTSWResponse()
		{
			BaseTSWResponse baseResponse;
			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText = MPIFoodResponse;
			BaseTSWResponse.TryParse(incomingMessage, out baseResponse);
			var responseMPI = new DeclarationResponseForTest(baseResponse);
			AssertEquals("IsTSWResponse", false, responseMPI.IsTSWResponse);

			incomingMessage.EM_MessageText = TSWMsgResponse;
			BaseTSWResponse.TryParse(incomingMessage, out baseResponse);
			var responseTSW = new DeclarationResponseForTest(baseResponse);
			AssertEquals("IsTSWResponse", true, responseTSW.IsTSWResponse);
		}

		public void TestIsClearedStatus()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_DeclarationReference = "B00001252";
			CusEntryHeader entryHeader = jobDeclaration.ActiveEntryHeaders.AddNew();

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_MessageText = "Outgoing Message";
			outgoingMessage.EM_MessageNum = "1";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "B00001252";
			outgoingMessage.EM_LinkedObject = entryHeader;

			BaseTSWResponse baseResponse;
			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText = MPIFoodResponse;
			incomingMessage.EM_LinkedObject = entryHeader;
			BaseTSWResponse.TryParse(incomingMessage, out baseResponse);
			var response = new DeclarationResponseForTest(baseResponse);

			Assert(!response.IsClearedStatus(FormalEntryStatusList.Codes.ResponseReceived));
			Assert(response.IsClearedStatus(FormalEntryStatusList.Codes.DeliveryOnPayment));
			Assert(response.IsClearedStatus(FormalEntryStatusList.Codes.DeliveryOrderReceived));
			Assert(response.IsClearedStatus(FormalEntryStatusList.Codes.EntryCleared));
			Assert(response.IsClearedStatus(FormalEntryStatusList.Codes.DeliveryOrderReceived));
		}

		public void TestProcessRestoredResponse()
		{
			// Test processing an unsolicited restoredEntry response message finds and links the relevant entry header
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_DeclarationReference = "B00001268";
			var entryHeader = jobDeclaration.ActiveEntryHeaders.AddNew();

			var line1 = entryHeader.MergedLines.AddNew();
			line1.CL_LineNumber = 1;
			var line2 = entryHeader.MergedLines.AddNew();
			line2.CL_LineNumber = 2;
			var line3 = entryHeader.MergedLines.AddNew();
			line3.CL_LineNumber = 3;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "B00001268";
			outgoingMessage.EM_LinkedObject = entryHeader;

			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText =
			#region Message Text
 @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20131115112604</IssueDateTime>
    <FunctionalReferenceID>540</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>79569757</ID>
        <AcceptanceDateTime formatCode=""204"">20131115112604</AcceptanceDateTime>
        <FunctionalReferenceID>B00001268</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>C</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount>441.57</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131115112604</EffectiveDateTime>
      <NameCode>822</NameCode>
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
			#endregion // Message Text

			BaseTSWResponse baseResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingMessage, out baseResponse));
			var response = new DeclarationResponseForTest(baseResponse);
			AssertEquals("EntryHeader is active", true, response.EntryHeader.IsActive);

			// cancel message
			#region CancelMessage Text
			string cancelMessage =
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
    <IssueDateTime formatCode=""204"">20131115112604</IssueDateTime>
    <FunctionalReferenceID>540</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>79569757</ID>
        <AcceptanceDateTime formatCode=""204"">20131115112604</AcceptanceDateTime>
        <FunctionalReferenceID>B00001268</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>C</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount>441.57</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131115112604</EffectiveDateTime>
      <NameCode>822</NameCode>
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
			var incomingCancelMessage = Factory.New<TSWMessage>();
			incomingCancelMessage.EM_MessageText = cancelMessage;
			BaseTSWResponse cancelResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingCancelMessage, out cancelResponse));
			var cancellationResponse = new DeclarationResponseForTest(cancelResponse);
			AssertEquals("EntryHeader is linked correctly", entryHeader.PK, cancellationResponse.EntryHeader.PK);

			jobDeclaration.ActiveEntryHeaders.AddNew();

			// restore message
			#region RestoreMessage Text
			string restoreMessage =
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
    <IssueDateTime formatCode=""204"">20131115125604</IssueDateTime>
    <FunctionalReferenceID>540</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>79569757</ID>
        <AcceptanceDateTime formatCode=""204"">20131115125604</AcceptanceDateTime>
        <FunctionalReferenceID>B00001268</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>C</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount>441.57</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131115112604</EffectiveDateTime>
      <NameCode>815</NameCode>
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
			var incomingRestoreMessage = Factory.New<TSWMessage>();
			incomingCancelMessage.EM_MessageText = restoreMessage;
			BaseTSWResponse restoreResponse;
			Assert("Can parse message", BaseTSWResponse.TryParse(incomingCancelMessage, out restoreResponse));
			var restorationResponse = new DeclarationResponseForTest(restoreResponse);
			AssertEquals("Restore messages finds the correct Header to link to", entryHeader.PK, restorationResponse.EntryHeader.PK);
		}

		public void TestProcessResponseForConsolidatedDeclaration()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
			consolidatedDeclaration.CRD_JobReferenceNumber = "CE00000052";
			var leadEntryHeader = ((JobDeclaration)consolidatedDeclaration.LeadDeclaration).CusEntryHeader;
			leadEntryHeader.CH_TotalPaid = 10.5m;
			leadEntryHeader.EntryFeeAmount = 15.5m;
			var entryHeader = consolidatedDeclaration.JobDeclarations.Cast<JobDeclaration>().Single(dec => !dec.IsLeadDeclarationOfConsolidatedDeclarations).CusEntryHeader;
			entryHeader.CH_TotalPaid = 20.5m;
			entryHeader.EntryFeeAmount = 25.5m;

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ApplicationCode = EDIInterchange.ApplicationCodes.NewZealandCustoms;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "CE00000052";
			outgoingMessage.EM_LinkedObject = consolidatedDeclaration;

			var incomingMessage = Factory.New<TSWMessage>();
			incomingMessage.EM_MessageText = NZCResponseForConsolidatedDeclaration;
			BaseTSWResponse.TryParse(incomingMessage, out var tswResponse);
			var declarationResponse = new DeclarationResponseForTest(tswResponse);
			AssertEquals("ExpectedTotalAmount", 46.5m, declarationResponse.ExpectedTotalAmount);
		}

		#region Response Messages

		const string MPIFoodResponse =
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
    <IssueDateTime formatCode=""204"">20131031130944</IssueDateTime>
    <FunctionalReferenceID>474</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>8200273</ID>
        <AcceptanceDateTime formatCode=""204"">20131031130944</AcceptanceDateTime>
        <FunctionalReferenceID>B00001252</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131031130944</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20131031130944</ReleaseDateTime>
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

		const string MPIBIOResponse =
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
    <IssueDateTime formatCode=""204"">20131031131008</IssueDateTime>
    <FunctionalReferenceID>476</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>8200273</ID>
        <AcceptanceDateTime formatCode=""204"">20131031131008</AcceptanceDateTime>
        <FunctionalReferenceID>B00001252</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>C</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount>0.0</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131031131008</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20131031131013</ReleaseDateTime>
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

		const string NZCSResponse =
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
    <IssueDateTime formatCode=""204"">20131031131008</IssueDateTime>
    <FunctionalReferenceID>475</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>8200273</ID>
        <AcceptanceDateTime formatCode=""204"">20131031131008</AcceptanceDateTime>
        <FunctionalReferenceID>B00001252</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>C</MethodCode>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount>441.57</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131031131008</EffectiveDateTime>
      <NameCode>822</NameCode>
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

		const string TSWMsgResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
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
    <IssueDateTime formatCode=""204"">20140821164119</IssueDateTime>
    <FunctionalReferenceID>1547</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>37470892</ID>
        <FunctionalReferenceID>B00001496</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20140821164119</EffectiveDateTime>
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

		const string AckResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
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
    <IssueDateTime formatCode=""204"">20140912130824</IssueDateTime>
    <FunctionalReferenceID>1612</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>19410937</ID>
        <FunctionalReferenceID>B00001506</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20140912130824</EffectiveDateTime>
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

		const string NZCResponseForConsolidatedDeclaration = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20231127184243</IssueDateTime>
    <FunctionalReferenceID>162</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>20 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>17998628</ID>
        <AcceptanceDateTime formatCode=""204"">20231127184243</AcceptanceDateTime>
        <FunctionalReferenceID>CE00000052</FunctionalReferenceID>
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
            <TaxAssessedAmount currencyID=""NZD"">46.50</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20231127184243</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20231127184243</ReleaseDateTime>
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
