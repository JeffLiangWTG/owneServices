using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration.FormalEntry;
using Enterprise.Environment;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.TradeSingleWindow.Testing
{
	using System;
	using System.Linq;
	using CargoWise.BrandManager;
	using CargoWise.EntityFramework.Testing;
	using Common;
	using DocumentEngine.Scheduler.Business;
	using Enterprise.Core;
	using Enterprise.Customs.Business.Testing;
	using Enterprise.Customs.DataRegistry.Business;
	using Enterprise.Customs.NZ.Business.Declaration;
	using Enterprise.Customs.NZ.Business.Declaration.Testing;
	using Enterprise.Customs.NZ.Registry;
	using Enterprise.Customs.NZ.TradeSingleWindow;
	using Enterprise.Customs.Universal.Testing;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Messaging.Business;
	using Enterprise.ZArchitecture.Environment;
	using ZArchitecture.Business;
	using ZArchitecture.Schema;

	public class DeclarationMessageProcessorTest : TestCaseWithFactory
	{
		public void TestNonSubmitterResponsesAreProcessed()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponsesSendToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unsolicitedDOGroup.PK.ToGuid());
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Enterprise.Core.Constants.EmailTo.NominatedGroup);

			declaration.JE_DeclarationReference = "B00004036";
			outgoingMessage.EM_ApplicationReference = "B00004036";

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var nzcsSubmitterMessage = Factory.New<TSWMessage>();
			nzcsSubmitterMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsSubmitterMessage.EM_MessageText = CustomsSubmitterResponse;
			nzcsSubmitterMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcsSubmitterMessage);
			AssertEquals(entryHeader, nzcsSubmitterMessage.EM_LinkedObject);
			AssertEquals("Entry Number should be obtained from this response message for this declaration", "43350351", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "819", entryHeader.CH_NZCSStatus);
			AssertEquals("Delivery/Customs instructions:", "50 LOOSE PACKAGE(S) OR ITEM(S)", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("nzcsSubmitterMessage should have been succesfully processed", EDIMessage.Status.Received, nzcsSubmitterMessage.EM_Status);

			var nzcsDepotMessage = Factory.New<TSWMessage>();
			nzcsDepotMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsDepotMessage.EM_MessageText = CustomsDepotResponse;
			nzcsDepotMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcsDepotMessage);
			AssertEquals(null, nzcsDepotMessage.EM_LinkedObject);
			AssertEquals("nzcsDepotMessage should have been processed as a notification message", EDIMessage.Status.Received, nzcsDepotMessage.EM_Status);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull("Should have generated a notification email when processing this Depot Response message", email);
		}

		public void TestUnsolicitedResponse()
		{
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponsesSendToGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, unsolicitedDOGroup.PK.ToGuid());
			NZCustomsDataRegistry.Instance.UnsolicitedDeliveryOrderResponses.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.EmailTo.NominatedGroup);

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var nzcMessage = Factory.New<TSWMessage>();
			nzcMessage.EM_MessageType = "RES";
			nzcMessage.EM_MessageText = CustomsDepotResponse;
			nzcMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcMessage);

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertNotNull("Should have found the email generated from processing this Depot Response message", email);
			AssertEquals(1, email.CCRecipients.Count);
			AssertEquals("JohnTester@testingCompany.com", email.CCRecipients[0].Email);
			AssertEquals("Unsolicited DepotMessage should have been processed", EDIMessage.Status.Received, nzcMessage.EM_Status);
		}

		public void TestClearInvoiceErrorFlagWhenProcessCorrectResponseSecondTime()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			declaration.JE_DeclarationReference = "B00001249";
			JobComInvoiceHeader invoice1 = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			JobComInvoiceLine invoiceLine2 = invoice1.JobComInvoiceLines.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.Duty, 100m);
			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.GST, 101m);
			entryLine.CL_LineNumber = 1;
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			entryHeader.Factory.Save();

			outgoingMessage.EM_ApplicationReference = "B00001249";

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			var nzcsMessageWithError = Factory.New<TSWMessage>();
			nzcsMessageWithError.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsMessageWithError.EM_MessageText = NZCSRejectionWithError;
			nzcsMessageWithError.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcsMessageWithError);
			AssertEquals(entryHeader, nzcsMessageWithError.EM_LinkedObject);
			Assert("HasError flag set from NZCS message", invoiceLine1.JI_HadErrorInLastResponse);
			Assert("HasError flag set from NZCS message", invoiceLine2.JI_HadErrorInLastResponse);

			AssertProcessMessageResult(ResponsibleGovernmentAgencyList.Codes.MPIBIO, declaration, cleared: true);
			AssertProcessMessageResult(ResponsibleGovernmentAgencyList.Codes.MPIFOOD, declaration, cleared: true);
			AssertProcessMessageResult(ResponsibleGovernmentAgencyList.Codes.NZCS, declaration, cleared: false);
		}

		void AssertProcessMessageResult(string agencyCode, JobDeclaration declaration, bool cleared)
		{
			var logger = new LoggingInformation();
			var message = Factory.New<TSWMessage>();
			message.EM_MessageType = MessageTypeList.Codes.TWR;
			message.EM_MessageText = string.Format(ResponsePlaceHolderWithNoError, agencyCode);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(message);
			foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
			{
				AssertEquals("HasError flag after process", cleared, invoiceLine.JI_HadErrorInLastResponse);
			}
		}

		public void TestMPIFoodClearCustomsRejectionResponse()
		{
			declaration.JE_DeclarationReference = "B00001249";
			outgoingMessage.EM_ApplicationReference = "B00001249";

			ZString mpiFoodResponseExpected = @"[Ministry for Primary Industries (Food) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001249

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001249
Master Bill    : 081-003498238
Entry Type     : Import (Normal)
Entry Number   : 9535406
Message No     : 471

Message Status : (F04) MPI Food - Cleared
";

			var mpifoodMessage = Factory.New<TSWMessage>();
			mpifoodMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpifoodMessage.EM_MessageText = MPIFoodResOK;
			mpifoodMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpifoodMessage);
			AssertEquals(entryHeader, mpifoodMessage.EM_LinkedObject);
			AssertEquals(mpiFoodResponseExpected, mpifoodMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIFood, entryStatus component representing MPIFood should be 0 for cleared.", "990", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "9535406", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "F04", entryHeader.CH_MPIFoodStatus);
			AssertEquals("JE_EntryStatus should reflect pending agency responses", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus", TSWEntryStatusList.Codes.PPC, declaration.JE_TSWCombinedStatus);

			ZString errorResponseExpected = @"[New Zealand Customs Service - Error report] Response for Customs Import Declaration: B00001249

Error report
---------------------------------------------------------------------
Job Number     : B00001249
Master Bill    : 081-003498238
Entry Type     : Import (Normal)
Entry Number   : 9535406
Message No     : 472

Message Status : (801) Lodgement rejected

Message Errors
---------------------------------------------------------------------
**Error** in {Declaration[1]/CurrencyExchange[1]/CurrencyTypeCode}:-
  Currency Code : Not specified or invalid
**Error** in {Declaration[1]/CurrencyExchange[1]/RateNumeric}:-
  Exchange Rate : Not specified or invalid
";

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsMessage.EM_MessageText = NZCSRejection;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcsMessage);
			AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
			AssertEquals(errorResponseExpected, nzcsMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should now also reflect the rejection response from Customs.", "790", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "9535406", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "801", entryHeader.CH_NZCSStatus);
			AssertEquals("JE_EntryStatus should reflect the Customs error as priority.", FormalEntryStatusList.Codes.EntryInError, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect Food Clear, Bio pending, Customs error", TSWEntryStatusList.Codes.EPC, declaration.JE_TSWCombinedStatus);
		}

		public void TestCompletionErrorResponseIsProcessed()
		{
			declaration.JE_DeclarationReference = "B00001319";
			declaration.JE_MasterBill = "08624289893";
			declaration.DeclarationNumber = "50267345";
			declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;

			var sightEntry = declaration.CustomsEntryHeaders[0];
			var completionEntry = declaration.CustomsEntryHeaders[1];
			completionEntry.CH_IsActive = true;
			completionEntry.CH_BGMReference = "1001";

			TSWMessage outgoingCompletionMessage = Factory.New<TSWMessage>();
			outgoingCompletionMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingCompletionMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingCompletionMessage.EM_LinkedObject = completionEntry;
			outgoingCompletionMessage.EM_LinkUniqueID = completionEntry.PK;
			outgoingCompletionMessage.EM_ApplicationReference = "1001";
			outgoingCompletionMessage.EM_Status = "SNT";

			completionEntry.Messages.Add(outgoingCompletionMessage);

			var rejectionMessage = Factory.New<TSWMessage>();
			rejectionMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			rejectionMessage.EM_MessageSubType = "TSW";
			rejectionMessage.EM_MessageText = CompletionRejection;
			rejectionMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			declaration.JE_EntryStatus = TSWEntryStatusList.Codes.STC;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(rejectionMessage);
			AssertEquals(completionEntry.PK, rejectionMessage.EM_LinkedObject.PK);
			AssertEquals("JE_TSWCombinedStatus should reflect the Completion request has been rejected.", TSWEntryStatusList.Codes.EPP, declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "", declaration.DeclarationNumber);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, rejectionMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.TradeSingleWindow, rejectionMessage.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect rejection response", FormalEntryStatusList.Codes.EntryInError, declaration.JE_EntryStatus);
		}

		public void TestClearanceResponse()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			declaration.JE_DeclarationReference = "B00001252";
			outgoingMessage.EM_ApplicationReference = "B00001252";

			ZString mpiFoodResponseExpected = @"[Ministry for Primary Industries (Food) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001252

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001252
Master Bill    : 081-003498238
Entry Type     : Import (Normal)
Entry Number   : 8200273
Message No     : 474

Message Status : (F04) MPI Food - Cleared
";

			var mpifoodMessage = Factory.New<TSWMessage>();
			mpifoodMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpifoodMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
			mpifoodMessage.EM_MessageText = MPIFoodResponse;
			mpifoodMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpifoodMessage);
			AssertEquals(entryHeader, mpifoodMessage.EM_LinkedObject);
			AssertEquals(mpiFoodResponseExpected, mpifoodMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIFood, entryStatus component representing MPIFood should be 0 for cleared.", "990", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "8200273", declaration.DeclarationNumber);
			AssertEquals("CH_MPIFoodStatus", "F04", entryHeader.CH_MPIFoodStatus);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpifoodMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, mpifoodMessage.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect pending agency responses", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should show MPI Food has cleared", TSWEntryStatusList.Codes.PPC, declaration.JE_TSWCombinedStatus);

			ZString mpiBioResponseExpected = @"[Ministry for Primary Industries (Biosecurity) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001252

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001252
Master Bill    : 081-003498238
Entry Type     : Import (Normal)
Entry Number   : 8200273
Message No     : 476

Message Status : (B04) MPI Biosecurity - Cleared

Delivery Instructions
---------------------------------------------------------------------
Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.
";

			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			bioMessage.EM_MessageText = MPIBIOResponse;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(bioMessage);
			AssertEquals(entryHeader, bioMessage.EM_LinkedObject);
			AssertEquals(mpiBioResponseExpected, bioMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIBio, entryStatus components representing MPIFood & MPIBio should now both be 0 for cleared.", "900", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "8200273", declaration.DeclarationNumber);
			AssertEquals("CH_MPIBioStatus", "B04", entryHeader.CH_MPIBioStatus);
			AssertEquals("JE_EntryStatus should still reflect pending agency response - Customs message response still to process", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect both Food & Bio have cleared", TSWEntryStatusList.Codes.PCC, declaration.JE_TSWCombinedStatus);

			ZString nzcsResponseExpected = @"[New Zealand Customs Service - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001252

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number      : B00001252
Master Bill     : 081-003498238
Entry Type      : Import (Normal)
Entry Number    : 8200273
Message No      : 475

Message Status  : (822) Entry Cleared
                : cash to pay prior to delivery.

Amount Returned : $441.57
Terms           : Cash
";

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;

			nzcsMessage.EM_MessageText = NZCSResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcsMessage);
			AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
			AssertEquals(nzcsResponseExpected, nzcsMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from Customs, entryStatus components representing MPIFood & MPIBio should now be 0 for cleared.", "600", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "8200273", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "822", entryHeader.CH_NZCSStatus);
			AssertEquals("Delivery/Customs instructions:", "Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("JE_EntryStatus should now reflect TSW cleared", FormalEntryStatusList.Codes.DeliveryOnPayment, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect all agency responses have cleared but Customs is pending payment", TSWEntryStatusList.Codes.YCC, declaration.JE_TSWCombinedStatus);

			declaration.SaveHandlingSaveExceptions();
			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("CLR Log Entry event should not have been created - payment is pending", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
		}

		public void TestEntryHeaderStatusAndMessageTimeUpdated()
		{
			declaration.JE_DeclarationReference = "B00001249";
			outgoingMessage.EM_ApplicationReference = "B00001249";
			AssertEquals("Pre-condition:", "STC", declaration.JE_EntryStatus);

			ZDateTime expectedMPIFoodResponseDate = new ZDateTime(2013, 10, 29, 19, 18, 16);
			var mpifoodMessage = Factory.New<TSWMessage>();
			mpifoodMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpifoodMessage.EM_MessageText = MPIFoodResOK;
			mpifoodMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpifoodMessage);
			AssertEquals("JE_TSWCombinedStatus should reflect MPI Food Response has been cleared", "990", declaration.JE_TSWCombinedStatus);
			AssertEquals("CH_MPIFoodStatus should have the actual MPI Food Response message status", "F04", entryHeader.CH_MPIFoodStatus);
			AssertEquals("CH_MPIFoodResponseTime should have the actual MPI Food Response message acceptance date", expectedMPIFoodResponseDate, entryHeader.CH_MPIFoodResponseTime);

			ZDateTime expectedNZCSResponseDate = new ZDateTime(2013, 10, 29, 20, 06, 07);
			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsMessage.EM_MessageText = NZCSRejection;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcsMessage);
			AssertEquals("JE_TSWCombinedStatus should now reflect MPI Food Response cleared and NZCS response rejected", "790", declaration.JE_TSWCombinedStatus);
			AssertEquals("CH_NZCSStatus should have the actual Customs Response message status", "801", entryHeader.CH_NZCSStatus);
			AssertEquals("CH_NZCSResponseTime should have the actual Customs Response message rejection date", expectedNZCSResponseDate, entryHeader.CH_NZCSResponseTime);
		}

		public void TestTotalAmountReturned()
		{
			string message =
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
    <FunctionalReferenceID>541</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MPI approval to Move from Wharf to ATF ECT TF Only, 23b Sunny Grove, South-West, Tauranga</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
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
{0}
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131115112604</EffectiveDateTime>
      <NameCode>B06</NameCode>
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

			outgoingMessage.EM_ApplicationReference = "B00001268";

			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();
			declaration.CusEntryHeader.CH_TotalAmountReturned = 99.9M;

			var message1 = ZString.Format(message, @"
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount>441.57</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>");

			ProcessMessage(message1);
			AssertEquals("Original value replaced with TaxAssessedAmount", 441.57M, declaration.CusEntryHeader.CH_TotalAmountReturned);

			var message2 = ZString.Format(message, "");

			ProcessMessage(message2);
			AssertEquals("Retains Original value when no TaxAssessedAmount present", 441.57M, declaration.CusEntryHeader.CH_TotalAmountReturned);

			var message3 = ZString.Format(message, @"
        <DutyTaxFee>
          <TypeCode>TOT</TypeCode>
          <Payment>
            <TaxAssessedAmount>0.0</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>");

			ProcessMessage(message3);
			AssertEquals("Original value replaced with TaxAssessedAmount even when zero", 0.0M, declaration.CusEntryHeader.CH_TotalAmountReturned);
		}

		public void TestCH_IsRestored()
		{
			outgoingMessage.EM_ApplicationReference = "B00001268";
			string message1 =
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
    <FunctionalReferenceID>541</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MPI approval to Move from Wharf to ATF ECT TF Only, 23b Sunny Grove, South-West, Tauranga</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
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
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131115112604</EffectiveDateTime>
      <NameCode>B06</NameCode>
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

			string message2 =
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
    <IssueDateTime formatCode=""204"">20131115112547</IssueDateTime>
    <FunctionalReferenceID>539</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>79569757</ID>
        <AcceptanceDateTime formatCode=""204"">20131115112547</AcceptanceDateTime>
        <FunctionalReferenceID>B00001268</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131115112547</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20131115112547</ReleaseDateTime>
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

			string message3 =
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

			string message4 =
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

			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();

			ProcessMessage(message1);
			var originalEntry = declaration.CusEntryHeader;
			ProcessMessage(message2);
			ProcessMessage(message3);
			AssertEquals(false, declaration.CusEntryHeader.CH_IsRestored);

			ProcessMessage(message4);
			AssertEquals(true, declaration.CusEntryHeader.CH_IsRestored);
			AssertEquals("Original entry should be the current EntryHeader", originalEntry.PK, declaration.CusEntryHeader.PK);
		}

		public void TestCH_IsRestored_ConsolidatedDeclaration()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			using (Env.SetTemporaryUserContext(new UserContext(User.ServiceUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				outgoingMessage.EM_ApplicationReference = "B00001268";
				var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
				consolidatedDeclaration.CRD_JobReferenceNumber = "B00001268";
				outgoingMessage.EM_LinkedObject = consolidatedDeclaration;
				string message1 =
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
    <FunctionalReferenceID>541</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MPI approval to Move from Wharf to ATF ECT TF Only, 23b Sunny Grove, South-West, Tauranga</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
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
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131115112604</EffectiveDateTime>
      <NameCode>B06</NameCode>
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

				string message2 =
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
    <IssueDateTime formatCode=""204"">20131115112547</IssueDateTime>
    <FunctionalReferenceID>539</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>79569757</ID>
        <AcceptanceDateTime formatCode=""204"">20131115112547</AcceptanceDateTime>
        <FunctionalReferenceID>B00001268</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131115112547</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20131115112547</ReleaseDateTime>
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

				string message3 =
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

				string message4 =
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

				var jobDeclarations = consolidatedDeclaration.JobDeclarations.Cast<JobDeclaration>().ToArray();
				var decCreator = new TestFormalEntryCreator(jobDeclarations[0]);
				decCreator.SetupTestConsignmentDetails();
				decCreator.SetupTestForAir();
				decCreator.SetupTestForImportFromAU();
				decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
				decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
				decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
				decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
				decCreator.MergeDeclaration();

				decCreator = new TestFormalEntryCreator(jobDeclarations[1]);
				decCreator.SetupTestConsignmentDetails();
				decCreator.SetupTestForAir();
				decCreator.SetupTestForImportFromAU();
				decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
				decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
				decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
				decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
				decCreator.MergeDeclaration();

				ProcessMessage(message1);
				Factory.Save();
				var originalEntries = jobDeclarations.Select(dec => dec.CusEntryHeader);
				ProcessMessage(message2);
				Factory.Save();
				ProcessMessage(message3);
				Factory.Save();
				AssertEquals(false, jobDeclarations[0].CusEntryHeader.CH_IsRestored || jobDeclarations[1].CusEntryHeader.CH_IsRestored);

				ProcessMessage(message4);
				Factory.Save();
				AssertEquals(true, jobDeclarations[0].CusEntryHeader.CH_IsRestored && jobDeclarations[1].CusEntryHeader.CH_IsRestored);
				AssertContainsExactElementsInExactOrder("Original entry should be the current EntryHeader", originalEntries, jobDeclarations.Select(dec => dec.CusEntryHeader));
			}
		}

		public void TestEntryIsCancelledThenRestored()
		{
			outgoingMessage.EM_ApplicationReference = "B00001268";
			string message1 =
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
    <FunctionalReferenceID>541</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>MPI approval to Move from Wharf to ATF ECT TF Only, 23b Sunny Grove, South-West, Tauranga</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
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
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131115112604</EffectiveDateTime>
      <NameCode>B06</NameCode>
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

			string message2 =
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
    <IssueDateTime formatCode=""204"">20131115112547</IssueDateTime>
    <FunctionalReferenceID>539</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>79569757</ID>
        <AcceptanceDateTime formatCode=""204"">20131115112547</AcceptanceDateTime>
        <FunctionalReferenceID>B00001268</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131115112547</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20131115112547</ReleaseDateTime>
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

			string cancellationMessage =
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
    <FunctionalReferenceID>581</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>8915911</ID>
        <AcceptanceDateTime formatCode=""204"">20131115112604</AcceptanceDateTime>
        <FunctionalReferenceID>B00001268</FunctionalReferenceID>
        <VersionID>3</VersionID>
        <CancellationDateTime formatCode=""204"">20131115112604</CancellationDateTime>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131115112604</EffectiveDateTime>
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

			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();

			ProcessMessage(message1);
			var originalEntry = declaration.CusEntryHeader;
			ProcessMessage(message2);
			ProcessMessage(cancellationMessage);
			AssertEquals(false, declaration.CusEntryHeader.CH_IsRestored);
			AssertEquals("Pre-condition: Entry has been canceled by Customs", true, declaration.CusEntryHeader.CH_IsEntryCancelled);

			ProcessMessage(restoreMessage);
			AssertEquals(true, declaration.CusEntryHeader.CH_IsRestored);
			AssertEquals("Original entry should be the current EntryHeader", originalEntry.PK, declaration.CusEntryHeader.PK);
			AssertEquals("Entry has been restored by Customs - cancelled flag should be reset", false, declaration.CusEntryHeader.CH_IsEntryCancelled);
			AssertEquals("TotalAmountReturned", 441.57M, declaration.CusEntryHeader.CH_TotalAmountReturned);
		}

		public void TestCancellationResponse()
		{
			declaration.JE_DeclarationReference = "B00001280";
			declaration.JE_MasterBill = "08134248325";
			declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.DCP;
			outgoingMessage.EM_ApplicationReference = "B00001280";
			outgoingMessage.EM_MessageSubType = NZ.TradeSingleWindow.MessageSubTypeList.Codes.Cancellation;

			var acknowledgementMessage = Factory.New<TSWMessage>();
			acknowledgementMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			acknowledgementMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.TradeSingleWindow;
			acknowledgementMessage.EM_MessageText = CancelAcknowledgeResponse;
			acknowledgementMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(acknowledgementMessage);
			AssertEquals(entryHeader, acknowledgementMessage.EM_LinkedObject);
			AssertEquals("JE_MessageStatus should reflect the Acknowledgement has been received.", StatusList.Codes.Acknowledgement, declaration.JE_MessageStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect the Cancellation request is pending.", TSWEntryStatusList.Codes.DCP, declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "8915911", declaration.DeclarationNumber);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, acknowledgementMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement, acknowledgementMessage.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect pending agency responses", FormalEntryStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);

			ZString inspectionResponseExpected = @"[New Zealand Customs Service - Inspection / Audit requirements] Response for Customs Import Declaration: B00001280

Inspection / Audit requirements
---------------------------------------------------------------------
Job Number     : B00001280
Master Bill    : 081-34248325
Entry Type     : Import (Normal)
Entry Number   : 8915911
Message No     : 580

Message Status : (804) Entry routed to Document Verification - Documents required as specified
";

			var inspectionMessage = Factory.New<TSWMessage>();
			inspectionMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			inspectionMessage.EM_MessageText = CancelInspectionRequiredResponse;
			inspectionMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(inspectionMessage);
			AssertEquals(entryHeader, inspectionMessage.EM_LinkedObject);
			AssertEquals(inspectionResponseExpected, inspectionMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the cancellation response has inspection requirements.", TSWEntryStatusList.Codes.DCI, declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "8915911", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "804", entryHeader.CH_NZCSStatus);
			AssertEquals("JE_EntryStatus should still reflect the inspection/audit response", FormalEntryStatusList.Codes.InspectionsAuditRequirements, declaration.JE_EntryStatus);

			ZString cancelledResponseExpected = @"[New Zealand Customs Service - Inspection / Audit requirements] Response for Customs Import Declaration: B00001280

Inspection / Audit requirements
---------------------------------------------------------------------
Job Number     : B00001280
Master Bill    : 081-34248325
Entry Type     : Import (Normal)
Entry Number   : 8915911
Message No     : 581

Message Status : (814) Lodgement cancelled
";

			var cancelAcceptedMessage = Factory.New<TSWMessage>();
			cancelAcceptedMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			cancelAcceptedMessage.EM_MessageText = CancelAcceptedResponse;
			cancelAcceptedMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(cancelAcceptedMessage);
			AssertEquals(entryHeader, cancelAcceptedMessage.EM_LinkedObject);
			AssertEquals(cancelledResponseExpected, cancelAcceptedMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the declaration has been cancelled.", TSWEntryStatusList.Codes.DCC, declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should be cleared out as declaration has been cancelled & CusEntryHeader de-activated with new active CusEntryHeader created", "8915911", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "814", entryHeader.CH_NZCSStatus);
			AssertEquals("JE_EntryStatus should also reflect TSW declaration has been cancelled", FormalEntryStatusList.Codes.EntryCancelled, declaration.JE_EntryStatus);
		}

		public void TestCustomsAcknowledgementReceivedAfterMPIClearResponse()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			declaration.JE_DeclarationReference = "B00001296";
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			declaration.JE_MasterBill = "08133459823";
			outgoingMessage.EM_ApplicationReference = "B00001296";
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I11;

			ZString mpiFoodResponseExpected = @"[Ministry for Primary Industries (Food) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001296

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001296
Master Bill    : 081-33459823
Entry Type     : Import (Simplified)
Entry Number   : 47248628
Message No     : 629

Message Status : (F04) MPI Food - Cleared
";

			var mpifoodMessage = Factory.New<TSWMessage>();
			mpifoodMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpifoodMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
			mpifoodMessage.EM_MessageText = MPIFR;
			mpifoodMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpifoodMessage);
			AssertEquals(entryHeader, mpifoodMessage.EM_LinkedObject);
			AssertEquals(mpiFoodResponseExpected, mpifoodMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIFood, entryStatus component representing MPIFood should be 0 for cleared.", "990", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "47248628", declaration.DeclarationNumber);
			AssertEquals("CH_MPIFoodStatus", "F04", entryHeader.CH_MPIFoodStatus);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpifoodMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, mpifoodMessage.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect pending agency responses", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should show MPI Food has cleared", TSWEntryStatusList.Codes.PPC, declaration.JE_TSWCombinedStatus);

			var ackMessage = Factory.New<TSWMessage>();
			ackMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			ackMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement;
			ackMessage.EM_MessageText = NZCSAcknowledgment;
			ackMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(ackMessage);
			AssertEquals(entryHeader, ackMessage.EM_LinkedObject);
			AssertEquals("JE_MessageStatus should reflect the Acknowledgement has been received.", StatusList.Codes.Acknowledgement, declaration.JE_MessageStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from NZCS is still pending and entryStatus components representing MPIFood pending & MPIBio cleared.", TSWEntryStatusList.Codes.PPC, declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "47248628", declaration.DeclarationNumber);
			AssertEquals("JE_EntryStatus should still reflect pending agency response - Customs message response still to process", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);

			ZString nzcsResponseExpected = @"[New Zealand Customs Service - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001296

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number      : B00001296
Master Bill     : 081-33459823
Entry Type      : Import (Simplified)
Entry Number    : 47248628
Message No      : 636

Message Status  : (822) Entry Cleared
                : cash to pay prior to delivery.

Amount Returned : $450.39
Terms           : Cash
";

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			nzcsMessage.EM_MessageText = NZCSR;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcsMessage);
			AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
			AssertEquals(nzcsResponseExpected, nzcsMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the payment required response from Customs, entryStatus components representing MPIFood Clear & MPIBio Pending.", TSWEntryStatusList.Codes.YPC, declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "47248628", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "822", entryHeader.CH_NZCSStatus);
			AssertEquals("JE_EntryStatus should still reflect pending agency response - MPI Biosecurity message response still to process", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);

			ZString mpiBioResponseExpected = @"[Ministry for Primary Industries (Biosecurity) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001296

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001296
Master Bill    : 081-33459823
Entry Type     : Import (Simplified)
Entry Number   : 47248628
Message No     : 638

Message Status : (B04) MPI Biosecurity - Cleared

Delivery Instructions
---------------------------------------------------------------------
Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.
";

			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			bioMessage.EM_MessageSubType = "BIO";
			bioMessage.EM_MessageText = MPIBR;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(bioMessage);
			AssertEquals(entryHeader, bioMessage.EM_LinkedObject);
			AssertEquals(mpiBioResponseExpected, bioMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIBio, entryStatus components representing MPIFood & MPIBio should now both be 0 for cleared.", "600", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "47248628", declaration.DeclarationNumber);
			AssertEquals("CH_MPIBioStatus", "B04", entryHeader.CH_MPIBioStatus);
			AssertEquals("Delivery/Customs instructions:", "Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("JE_EntryStatus should now reflect TSW cleared but pending payment", FormalEntryStatusList.Codes.DeliveryOnPayment, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect both Food & Bio have cleared and payment is pending", TSWEntryStatusList.Codes.YCC, declaration.JE_TSWCombinedStatus);

			declaration.SaveHandlingSaveExceptions();
			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry event has been not been created - payment is required", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
			logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("CLR Log Entry event should not have been created - CLR will only be created when SCM is created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
		}

		public void TestCompletionClearanceResponseProcessing()
		{
			#region Sight entry
			declaration.JE_DeclarationReference = "B00001324";
			declaration.JE_MasterBill = "08600392346";
			declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Sight;
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I52;
			outgoingMessage.EM_ApplicationReference = "B00001324";

			ZString mpiFoodResponseExpected = @"[Ministry for Primary Industries (Food) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001324

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001324
Master Bill    : 086-00392346
Entry Type     : Import (Sight)
Entry Number   : 89411896
Message No     : 737

Message Status : (F04) MPI Food - Cleared
";

			var mpifoodMessage = Factory.New<TSWMessage>();
			mpifoodMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpifoodMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
			mpifoodMessage.EM_MessageText = originalSightMPIResponse;
			mpifoodMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpifoodMessage);
			AssertEquals(entryHeader, mpifoodMessage.EM_LinkedObject);
			AssertEquals(mpiFoodResponseExpected, mpifoodMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIFood, entryStatus component representing MPIFood should be 0 for cleared.", "990", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "89411896", declaration.DeclarationNumber);
			AssertEquals("CH_MPIFoodStatus", "F04", entryHeader.CH_MPIFoodStatus);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpifoodMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, mpifoodMessage.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect pending agency responses", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should show MPI Food has cleared", TSWEntryStatusList.Codes.PPC, declaration.JE_TSWCombinedStatus);

			ZString nzcsResponseExpected = @"[New Zealand Customs Service - Inspection / Audit requirements] Response for Customs Import Declaration: B00001324

Inspection / Audit requirements
---------------------------------------------------------------------
Job Number     : B00001324
Master Bill    : 086-00392346
Entry Type     : Import (Sight)
Entry Number   : 89411896
Message No     : 738

Message Status : (806) Entry routed to Service Delivery - Documents required as specified
";

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;

			nzcsMessage.EM_MessageText = originalSightNZCResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcsMessage);
			AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
			AssertEquals(nzcsResponseExpected, nzcsMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from Customs, requiring inspection for the sight entry.", "190", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "89411896", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "806", entryHeader.CH_NZCSStatus);
			AssertEquals("JE_EntryStatus should also reflect inspection required", FormalEntryStatusList.Codes.InspectionsAuditRequirements, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect customs inspection requirement", TSWEntryStatusList.Codes.IPC, declaration.JE_TSWCombinedStatus);

			ZString mpiBioResponseExpected = @"[Ministry for Primary Industries (Biosecurity) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001324

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001324
Master Bill    : 086-00392346
Entry Type     : Import (Sight)
Entry Number   : 89411896
Message No     : 739

Message Status : (B04) MPI Biosecurity - Cleared

Delivery Instructions
---------------------------------------------------------------------
Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.
";

			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			bioMessage.EM_MessageText = originalSightBioResponse;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(bioMessage);
			AssertEquals(entryHeader, bioMessage.EM_LinkedObject);
			AssertEquals(mpiBioResponseExpected, bioMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect all responses to the sight entry, from MPIFood & MPIBio cleared & Customs inspection required.", "100", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "89411896", declaration.DeclarationNumber);
			AssertEquals("CH_MPIBioStatus", "B04", entryHeader.CH_MPIBioStatus);
			AssertEquals("JE_EntryStatus should still reflect the inspection requirement", FormalEntryStatusList.Codes.InspectionsAuditRequirements, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect both Food & Bio have cleared & Customs Inspection is required", TSWEntryStatusList.Codes.ICC, declaration.JE_TSWCombinedStatus);
			#endregion

			#region Completion entry
			declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;

			var sightEntry = declaration.ActiveEntryHeaders[0];
			var completionEntry = declaration.CustomsEntryHeaders[1];
			completionEntry.CH_IsActive = true;
			completionEntry.CH_BGMReference = "51938";

			TSWMessage outgoingCompletionMessage = Factory.New<TSWMessage>();
			outgoingCompletionMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingCompletionMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingCompletionMessage.EM_LinkedObject = completionEntry;
			outgoingCompletionMessage.EM_LinkUniqueID = completionEntry.PK;
			outgoingCompletionMessage.EM_ApplicationReference = "51938";

			var acknowlegementMessage = Factory.New<TSWMessage>();
			acknowlegementMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			acknowlegementMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.TradeSingleWindow;
			acknowlegementMessage.EM_MessageText = completionAcknowledgement;
			acknowlegementMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			declaration.JE_EntryStatus = TSWEntryStatusList.Codes.STC;
			processor.ProcessMessage(acknowlegementMessage);
			AssertEquals("Acknowledgement response should find completion entry to link to", completionEntry.PK, acknowlegementMessage.EM_LinkedObject.PK);

			var mpiResponse = Factory.New<TSWMessage>();
			mpiResponse.EM_MessageType = MessageTypeList.Codes.TWR;
			mpiResponse.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.TradeSingleWindow;
			mpiResponse.EM_MessageText = completionMPIResponse;
			mpiResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			declaration.JE_EntryStatus = TSWEntryStatusList.Codes.STC;
			processor.ProcessMessage(mpiResponse);
			AssertEquals("Message should be linked to completion entry", completionEntry.PK, mpiResponse.EM_LinkedObject.PK);
			AssertEquals("JE_TSWCombinedStatus should reflect the Completion request has cleared MPI & pending Bio & Customs.", TSWEntryStatusList.Codes.PPC, declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should reflect the NEW Completion entry number", "95929208", declaration.DeclarationNumber);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpiResponse.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, mpiResponse.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect further responses are still pending", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);

			var nzcsResponse = Factory.New<TSWMessage>();
			nzcsResponse.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsResponse.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.TradeSingleWindow;
			nzcsResponse.EM_MessageText = completionNZCSResponse;
			nzcsResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcsResponse);
			AssertEquals("Message should be linked to completion entry", completionEntry.PK, nzcsResponse.EM_LinkedObject.PK);
			AssertEquals("JE_TSWCombinedStatus should reflect the Completion request has now cleared Customs as well.", TSWEntryStatusList.Codes.YPC, declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should reflect the Completion entry number", "95929208", declaration.DeclarationNumber);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, nzcsResponse.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms, nzcsResponse.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect further responses (Bio) are still pending", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);

			var bioResponse = Factory.New<TSWMessage>();
			bioResponse.EM_MessageType = MessageTypeList.Codes.TWR;
			bioResponse.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.TradeSingleWindow;
			bioResponse.EM_MessageText = completionBioResponse;
			bioResponse.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(bioResponse);
			AssertEquals("Message should be linked to completion entry", completionEntry.PK, bioResponse.EM_LinkedObject.PK);
			AssertEquals("JE_TSWCombinedStatus should reflect the Completion request has been cleared by all Agencies.", TSWEntryStatusList.Codes.YCC, declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should reflect the Completion entry number", "95929208", declaration.DeclarationNumber);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, bioResponse.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIBIO, bioResponse.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect the completion message has now been cleared but payment is pending", FormalEntryStatusList.Codes.DeliveryOnPayment, declaration.JE_EntryStatus);
			#endregion
		}

		public void TestExportClearanceResponse()
		{
			declaration.JE_DeclarationReference = "B00001338";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			outgoingMessage.EM_ApplicationReference = "B00001338";
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.E40;

			ZString nzcsResponseExpected = @"[New Zealand Customs Service - Clearance / Acceptance Instructions] Response for Customs Export Declaration: B00001338

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number      : B00001338
Master Bill     : 081-003498238
Entry Type      : Export (Normal)
Entry Number    : 83093658
Message No      : 821

Message Status  : (809) Delivery Order sent to recipient
                : method of payment as specified.

Amount Returned : Amount Not Included in Response.
Terms           : Cash

Delivery Instructions
---------------------------------------------------------------------
22 LOOSE PACKAGE(S) OR ITEM(S)
";

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsMessage.EM_MessageText = ExportClearedResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
			AssertEquals(nzcsResponseExpected, nzcsMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from Customs, entryStatus components representing MPIFood & MPIBio & NZCS should now all be 0 for cleared.", "0", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "83093658", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "809", entryHeader.CH_NZCSStatus);
			AssertEquals("Delivery/Customs instructions:", "22 LOOSE PACKAGE(S) OR ITEM(S)", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("JE_EntryStatus should now reflect TSW Delivery order has been sent to the recipient", FormalEntryStatusList.Codes.DOSentToRecipient, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect Customs response has cleared", "0", declaration.JE_TSWCombinedStatus);
		}

		public void TestExportClearanceDeliveryOrderResponse()
		{
			declaration.JE_DeclarationReference = "B00001854";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			outgoingMessage.EM_ApplicationReference = "B00001854";
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.E40;

			ZString nzcsResponseExpected = @"[New Zealand Customs Service - Clearance / Acceptance Instructions] Response for Customs Export Declaration: B00001854

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number      : B00001854
Master Bill     : 081-003498238
Entry Type      : Export (Normal)
Entry Number    : 8410144
Message No      : 2875

Message Status  : (819) Delivery Order Herewith
                : method of Payment as specified.

Amount Returned : Amount Not Included in Response.
Terms           : Client Deferred

Delivery Instructions
---------------------------------------------------------------------
1 LOOSE PACKAGE(S) OR ITEM(S)
";

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsMessage.EM_MessageText = ExportDeliveryOrderResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
			AssertEquals(nzcsResponseExpected, nzcsMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from Customs, entryStatus components representing MPIFood & MPIBio & NZCS should now all be 0 for cleared.", "0", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "8410144", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "819", entryHeader.CH_NZCSStatus);
			AssertEquals("Delivery/Customs instructions:", "1 LOOSE PACKAGE(S) OR ITEM(S)", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("JE_EntryStatus should now reflect TSW Delivery Order Received", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
			AssertEquals("JE_EntryStatusDescription - TSW procesing", FormalEntryStatusList.Descriptions.DeliveryOrderReceived, declaration.JE_EntryStatusDescription);
			AssertEquals("JE_TSWCombinedStatus should reflect Customs response has cleared", "0", declaration.JE_TSWCombinedStatus);
		}

		public void TestZeroTOTResponse()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "NZBrokerage"))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var testTaxOrFee_IET = helper.CreateTaxOrFee("IET", 20m, "NZ", new ZDateTime(2001, 1, 1), new ZDateTime(2079, 6, 6), "Import Entry Transaction Fee (IETF)");
				var testTaxOrFee_GST = helper.CreateTaxOrFee("GST", 2m, "NZ", new ZDateTime(2001, 1, 1), new ZDateTime(2079, 6, 6), "Goods and Services Tax");
				Factory.Save();

				declaration.JE_DeclarationReference = "B00165917";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				outgoingMessage.EM_ApplicationReference = "B00165917";
				outgoingMessage.EM_MessageType = MessageTypeList.Codes.I11;

				declaration.Invoices.AddNew().InvoiceLines.AddNew();
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				Assert("Precondition: The entry fee is not waived.", !declaration.CusEntryHeader.CH_EntryChargeWaived);
				AssertEquals("Precondition: we have two charges with charge type ENF and EFG.", 2, declaration.CusEntryHeader.Charges.Cast<CusEntryHeaderCharge>().Count(charge => charge.IsWaivable));

				ZString nzcsResponseExpected = @"[New Zealand Customs Service - Inspection / Audit requirements] Response for Customs Import Declaration: B00165917

Inspection / Audit requirements
---------------------------------------------------------------------
Job Number      : B00165917
Master Bill     : 081-003498238
Entry Type      : Import (Normal)
Entry Number    : 34589090
Message No      : 5728

Message Status  : (802) Lodgement routed to Inspections Evaluation - Please await requirements

Amount Returned : $0.00
Terms           : Broker Deferred

Declaration Status Updated
---------------------------------------------------------------------
From STC to IAR.

";

				var nzcsMessage = Factory.New<TSWMessage>();
				nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;
				nzcsMessage.EM_MessageText = ZeroTOTResponse;
				nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

				var logger = new LoggingInformation();
				var processor = new MessageProcessorFactory(logger);
				processor.ProcessMessage(nzcsMessage);
				AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
				AssertEquals(nzcsResponseExpected, nzcsMessage.EM_MessageInterpretation);
				Assert("The entry fee is waived.", declaration.CusEntryHeader.CH_EntryChargeWaived);
				AssertEquals("The charges with charge type ENF and EFG are removed.", 0, declaration.CusEntryHeader.Charges.Cast<CusEntryHeaderCharge>().Count(charge => charge.IsWaivable));

				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
				AssertEquals("Once the entry fee is waived, the charges won't be created even if we do merge again.", 0, declaration.CusEntryHeader.Charges.Cast<CusEntryHeaderCharge>().Count(charge => charge.IsWaivable));
			}
		}

		public void TestNonZeroTOTResponseShouldNotSetAmountToZero()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, "NZBrokerage"))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var testTaxOrFee_IET = helper.CreateTaxOrFee("IET", 20m, "NZ", new ZDateTime(2001, 1, 1), new ZDateTime(2079, 6, 6), "Import Entry Transaction Fee (IETF)");
				var testTaxOrFee_GST = helper.CreateTaxOrFee("GST", 2m, "NZ", new ZDateTime(2001, 1, 1), new ZDateTime(2079, 6, 6), "Goods and Services Tax");
				Factory.Save();

				declaration.JE_DeclarationReference = "B00165917";
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Temporary;
				outgoingMessage.EM_ApplicationReference = "B00165917";
				outgoingMessage.EM_MessageType = MessageTypeList.Codes.I11;

				declaration.Invoices.AddNew().InvoiceLines.AddNew();
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				Assert("Precondition: The entry fee is not waived.", !declaration.CusEntryHeader.CH_EntryChargeWaived);
				AssertEquals("Precondition: we have two charges with charge type ENF and EFG.", 2, declaration.CusEntryHeader.Charges.Cast<CusEntryHeaderCharge>().Count(charge => charge.IsWaivable));

				ZString nzcsResponseExpected = @$"[New Zealand Customs Service - Inspection / Audit requirements] Response for Customs Import Declaration: B00165917

Inspection / Audit requirements
---------------------------------------------------------------------
Job Number      : B00165917
Master Bill     : 081-003498238
Entry Type      : Import (Temporary)
Entry Number    : 34589090
Message No      : 5728

Message Status  : (802) Lodgement routed to Inspections Evaluation - Please await requirements

Amount Returned : Amount Not Included in Response.
** WARNING - Total Amount Payable returned by Customs does not match the amount calculated by {BrandingFactory.Instance.ProductName}. (60.00) **
Terms           : Broker Deferred

Declaration Status Updated
---------------------------------------------------------------------
From STC to IAR.

";

				var nzcsMessage = Factory.New<TSWMessage>();
				nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;
				nzcsMessage.EM_MessageText = NonZeroTOTResponse;
				nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

				var logger = new LoggingInformation();
				var processor = new MessageProcessorFactory(logger);
				processor.ProcessMessage(nzcsMessage);
				AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
				AssertEquals(nzcsResponseExpected, nzcsMessage.EM_MessageInterpretation);
				Assert("The entry fee is not waived.", !declaration.CusEntryHeader.CH_EntryChargeWaived);
				AssertEquals("The charges with charge type ENF and EFG are not removed.", 2, declaration.CusEntryHeader.Charges.Cast<CusEntryHeaderCharge>().Count(charge => charge.IsWaivable));
			}
		}

		public void TestExportResponseUpdatesMSGAndEntryStatus()
		{
			declaration.JE_DeclarationReference = "B00001601";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			outgoingMessage.EM_ApplicationReference = "B00001601";
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.E40;

			var ackMessage = Factory.New<TSWMessage>();
			ackMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			ackMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement;
			ackMessage.EM_MessageText = EX1ACK;
			ackMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ackMessage);
			AssertEquals(entryHeader, ackMessage.EM_LinkedObject);
			AssertEquals("JE_MessageStatus should reflect the Acknowledgement has been received.", StatusList.Codes.Acknowledgement, declaration.JE_MessageStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "64528325", declaration.DeclarationNumber);
			AssertEquals("JE_EntryStatus should reflect a response is still pending from Customs", FormalEntryStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should not be updated by ACK message", ZString.Empty, declaration.JE_TSWCombinedStatus);

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsMessage.EM_MessageText = EX1CLR;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from Customs", "0", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "64528325", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "809", entryHeader.CH_NZCSStatus);
			AssertEquals("Delivery/Customs instructions:", "20 LOOSE PACKAGE(S) OR ITEM(S)", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("JE_EntryStatus should now reflect TSW D/O has been sent to the recipient", FormalEntryStatusList.Codes.DOSentToRecipient, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect Customs response has cleared", "0", declaration.JE_TSWCombinedStatus);
			AssertEquals("JE_MessageStatus should remain as Acknowledgement.", StatusList.Codes.Acknowledgement, declaration.JE_MessageStatus);
		}

		public void Test816StatusIsIgnored()
		{
			declaration.JE_DeclarationReference = "B00004056";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			outgoingMessage.EM_ApplicationReference = "B00004056C";
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.E40;

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsMessage.EM_MessageText = EX1CompletionCleared;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsMessage);
			AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
			AssertEquals("JE_TSWCombinedStatus should reflect the cleared response from Customs", TSWEntryStatusList.Codes.CLR, declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "84994890", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "819", entryHeader.CH_NZCSStatus);
			AssertEquals("Delivery/Customs instructions:", "20 LOOSE PACKAGE(S) OR ITEM(S)", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("JE_EntryStatus should now reflect TSW D/O has been sent", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);

			var nzcsInfoMessage = Factory.New<TSWMessage>();
			nzcsInfoMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsInfoMessage.EM_MessageText = EX1CompletionInformation;
			nzcsInfoMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsInfoMessage);
			AssertEquals(entryHeader, nzcsInfoMessage.EM_LinkedObject);
			AssertEquals("JE_TSWCombinedStatus should not be updated to Inspection / Audit Requirements (TSWEntryStatusList.Codes.IAR) by an 816 status message from Customs", TSWEntryStatusList.Codes.CLR, declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "84994890", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus should not change", "819", entryHeader.CH_NZCSStatus);
			AssertEquals("JE_EntryStatus should still reflect TSW D/O has been sent to the recipient", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
		}

		public void Test816StatusIsNotIgnoredWhenDecStatusIsSTC()
		{
			declaration.JE_DeclarationReference = "B00004056";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			outgoingMessage.EM_ApplicationReference = "B00004056C";
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.E40;

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			var nzcsInfoMessage = Factory.New<TSWMessage>();
			nzcsInfoMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsInfoMessage.EM_MessageText = EX1CompletionInformation;
			nzcsInfoMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsInfoMessage);
			AssertEquals(entryHeader, nzcsInfoMessage.EM_LinkedObject);
			AssertEquals("JE_TSWCombinedStatus should be updated to Inspection / Audit Requirements (TSWEntryStatusList.Codes.IAR) by this 816 status message from Customs as declaration is in STC state", TSWEntryStatusList.Codes.IAR, declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should be updated", "84994890", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus should be set", "816", entryHeader.CH_NZCSStatus);
			AssertEquals("JE_EntryStatus should reflect the 816 status", FormalEntryStatusList.Codes.InspectionsAuditRequirements, declaration.JE_EntryStatus);
		}

		public void TestTSWGatewayRejectionOfIPI()
		{
			declaration.JE_DeclarationReference = "B00003778";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			outgoingMessage.EM_ApplicationReference = "B00003778";
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.IPI;
			outgoingMessage.EM_LinkedObject = declaration.CusEntryHeader;
			outgoingMessage.EM_LinkUniqueID = declaration.CusEntryHeader.PK;

			var tswGatewayMessage = Factory.New<TSWMessage>();
			tswGatewayMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			tswGatewayMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.TradeSingleWindow;
			tswGatewayMessage.EM_MessageText = IPIB00003778TSWMsg;
			tswGatewayMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(tswGatewayMessage);
			AssertEquals(declaration.CusEntryHeader, tswGatewayMessage.EM_LinkedObject);
			AssertEquals("JE_MessageStatus should reflect the entry has been rejected.", StatusList.Codes.EntryRejected, declaration.JE_MessageStatus);
			AssertEquals("Entry Number should be blank, rejected by gateway", "", declaration.DeclarationNumber);
			AssertEquals("JE_EntryStatus should show declaration is rejected", FormalEntryStatusList.Codes.EntryRejected, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus will not have been updated by gateway rejection message", ZString.Empty, declaration.JE_TSWCombinedStatus);
		}

		public void TestPrimaryIndustryDecClearanceResponseStatus()
		{
			declaration.JE_DeclarationReference = "B00001338";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			outgoingMessage.EM_ApplicationReference = "B00001482";
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.IPI;
			outgoingMessage.EM_LinkedObject = declaration.CusEntryHeader;
			outgoingMessage.EM_LinkUniqueID = declaration.CusEntryHeader.PK;

			var ackMessage = Factory.New<TSWMessage>();
			ackMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			ackMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement;
			ackMessage.EM_MessageText = IPIAcknowledgement;
			ackMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ackMessage);
			AssertEquals(declaration.CusEntryHeader, ackMessage.EM_LinkedObject);
			AssertEquals("JE_MessageStatus should reflect the Acknowledgement has been received.", StatusList.Codes.Acknowledgement, declaration.JE_MessageStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "99809018", declaration.DeclarationNumber);
			AssertEquals("JE_EntryStatus should reflect a response is still pending", FormalEntryStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should not be updated by ACK message", ZString.Empty, declaration.JE_TSWCombinedStatus);

			var mpiMessage = Factory.New<TSWMessage>();
			mpiMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpiMessage.EM_MessageText = IPIClearance;
			mpiMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			processor.ProcessMessage(mpiMessage);
			AssertEquals(declaration.CusEntryHeader, mpiMessage.EM_LinkedObject);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from TSW, entryStatus components representing MPIBIO should be 9 for pending & MPIFood should be 0 for cleared.", "N90", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "99809018", declaration.DeclarationNumber);
			AssertEquals("JE_EntryStatus should now reflect TSW cleared", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect MPI response has cleared but MPI Biosecurity is still pending", TSWEntryStatusList.Codes.NPC, declaration.JE_TSWCombinedStatus);
		}

		public void TestIPIResponseCombinedStatus()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "B00003768";
			declaration.JE_MasterBill = "08600249281";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.IPI;
			outgoingMessage.EM_ApplicationReference = "B00003768";
			outgoingMessage.EM_LinkedObject = declaration.CusEntryHeader;   // this is now the new IPI entry header
			outgoingMessage.EM_LinkUniqueID = declaration.CusEntryHeader.PK;

			ZString mpiFoodResponseExpected = @"[Ministry for Primary Industries (Food) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00003768

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00003768
Master Bill    : 086-00249281
Entry Type     : Import (Primary Industries Import)
Entry Number   : 79323392
Message No     : 3268

Message Status : (F04) MPI Food - Cleared
";

			var mpifoodMessage = Factory.New<TSWMessage>();
			mpifoodMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpifoodMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
			mpifoodMessage.EM_MessageText = B00003768MPIFoodResponse;
			mpifoodMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpifoodMessage);
			AssertEquals("Response message should be linked to the IPI entry header", declaration.CusEntryHeader, mpifoodMessage.EM_LinkedObject);
			AssertEquals(mpiFoodResponseExpected, mpifoodMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIFood, entryStatus component representing MPIFood should be 0 for cleared.", "N90", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "79323392", declaration.DeclarationNumber);
			AssertEquals("CH_MPIFoodStatus", "F04", declaration.CusEntryHeader.CH_MPIFoodStatus);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpifoodMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, mpifoodMessage.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect pending Biosecurity agency response", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should show MPI Food has cleared but MPI Biosecurity is pending", TSWEntryStatusList.Codes.NPC, declaration.JE_TSWCombinedStatus);
		}

		public void TestIPIClearanceDoesNotGeneratesCLREvent()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "B00001252";
			declaration.JE_MasterBill = "081003498238";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.IPI;
			outgoingMessage.EM_ApplicationReference = "B00001252I";
			outgoingMessage.EM_LinkedObject = declaration.CusEntryHeader;   // this is the IPI entry header
			outgoingMessage.EM_LinkUniqueID = declaration.CusEntryHeader.PK;

			ZString mpiFoodResponseExpected = @"[Ministry for Primary Industries (Food) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001252

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001252
Master Bill    : 081-003498238
Entry Type     : Import (Primary Industries Import)
Entry Number   : 8200273
Message No     : 474

Message Status : (F04) MPI Food - Cleared
";

			var mpifoodMessage = Factory.New<TSWMessage>();
			mpifoodMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpifoodMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
			mpifoodMessage.EM_MessageText = MPIFoodResponseForIPI;
			mpifoodMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpifoodMessage);
			AssertEquals(declaration.CusEntryHeader, mpifoodMessage.EM_LinkedObject);
			AssertEquals(mpiFoodResponseExpected, mpifoodMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIFood, entryStatus component representing MPIFood should be 0 for cleared.", "N90", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "8200273", declaration.DeclarationNumber);
			AssertEquals("CH_MPIFoodStatus", "F04", declaration.CusEntryHeader.CH_MPIFoodStatus);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpifoodMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, mpifoodMessage.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect pending agency response (Biosecurity)", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should show MPI Food has cleared but Bio is pending", TSWEntryStatusList.Codes.NPC, declaration.JE_TSWCombinedStatus);

			declaration.SaveHandlingSaveExceptions();
			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("CLR Log Entry event should not be created yet as only one of MPIFood & MPIBio have cleared for this entry", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));

			ZString mpiBioResponseExpected = @"[Ministry for Primary Industries (Biosecurity) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001252

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001252
Master Bill    : 081-003498238
Entry Type     : Import (Primary Industries Import)
Entry Number   : 8200273
Message No     : 476

Message Status : (B04) MPI Biosecurity - Cleared

Delivery Instructions
---------------------------------------------------------------------
Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.

Declaration Status Updated
---------------------------------------------------------------------
From ARP to CLR.

";

			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			bioMessage.EM_MessageText = MPIBIOResponseForIPI;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(bioMessage);
			AssertEquals(declaration.CusEntryHeader, bioMessage.EM_LinkedObject);
			AssertEquals(mpiBioResponseExpected, bioMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIBio, entryStatus components representing MPIFood & MPIBio should now both be 0 for cleared.", "N00", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "8200273", declaration.DeclarationNumber);
			AssertEquals("CH_MPIBioStatus", "B04", declaration.CusEntryHeader.CH_MPIBioStatus);
			AssertEquals("JE_EntryStatus should reflect entry now cleared", FormalEntryStatusList.Codes.EntryCleared, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect both Food & Bio have cleared", TSWEntryStatusList.Codes.NCC, declaration.JE_TSWCombinedStatus);

			declaration.SaveHandlingSaveExceptions();
			logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("CLR Log Entry event should never be created for any IPI entry", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
		}

		public void TestIPIFoodResponseDoesNotGenerateCLREvent()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "B00001252";
			declaration.JE_MasterBill = "081003498238";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.IPI;
			outgoingMessage.EM_ApplicationReference = "B00001252";
			outgoingMessage.EM_LinkedObject = declaration.CusEntryHeader;
			outgoingMessage.EM_LinkUniqueID = declaration.CusEntryHeader.PK;

			ZString mpiFoodResponseExpected = @"[Ministry for Primary Industries (Food) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001252

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001252
Master Bill    : 081-003498238
Entry Type     : Import (Primary Industries Import)
Entry Number   : 8200273
Message No     : 474

Message Status : (F04) MPI Food - Cleared
";

			var mpifoodMessage = Factory.New<TSWMessage>();
			mpifoodMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpifoodMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
			mpifoodMessage.EM_MessageText = MPIFoodResponse;
			mpifoodMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpifoodMessage);
			AssertEquals(declaration.CusEntryHeader, mpifoodMessage.EM_LinkedObject);
			AssertEquals(mpiFoodResponseExpected, mpifoodMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIFood, entryStatus component representing MPIFood should be 0 for cleared.", "N90", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "8200273", declaration.DeclarationNumber);
			AssertEquals("CH_MPIFoodStatus", "F04", declaration.CusEntryHeader.CH_MPIFoodStatus);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpifoodMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, mpifoodMessage.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect pending agency response (Biosecurity)", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should show MPI Food has cleared but Bio is pending", TSWEntryStatusList.Codes.NPC, declaration.JE_TSWCombinedStatus);

			declaration.SaveHandlingSaveExceptions();
			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("CLR Log Entry event should not be created for cleared MPI Food response only", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
		}

		public void TestIPIBioResponseOnlyDoesNotGenerateCLREvent()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "B00001252";
			declaration.JE_MasterBill = "081003498238";
			declaration.JE_EntryStatus = "STC";
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.IPI;
			outgoingMessage.EM_ApplicationReference = "B00001252";
			outgoingMessage.EM_LinkedObject = declaration.CusEntryHeader;
			outgoingMessage.EM_LinkUniqueID = declaration.CusEntryHeader.PK;

			ZString mpiBioResponseExpected = @"[Ministry for Primary Industries (Biosecurity) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001252

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001252
Master Bill    : 081-003498238
Entry Type     : Import (Primary Industries Import)
Entry Number   : 8200273
Message No     : 476

Message Status : (B04) MPI Biosecurity - Cleared

Delivery Instructions
---------------------------------------------------------------------
Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.
";

			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			bioMessage.EM_MessageText = MPIBIOResponse;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(bioMessage);
			AssertEquals(declaration.CusEntryHeader, bioMessage.EM_LinkedObject);
			AssertEquals(mpiBioResponseExpected, bioMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIBio, entryStatus component representing MPIFood is pending.", "N09", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "8200273", declaration.DeclarationNumber);
			AssertEquals("CH_MPIBioStatus", "B04", declaration.CusEntryHeader.CH_MPIBioStatus);
			AssertEquals("JE_EntryStatus should reflect a response is still pending", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect Food response is pending & Bio has been cleared", TSWEntryStatusList.Codes.NCP, declaration.JE_TSWCombinedStatus);

			declaration.SaveHandlingSaveExceptions();
			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("CLR Log Entry event should not be created for cleared MPI Bio response only", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
		}

		public void TestIPIClearanceDoesNotGenerateCLREvent()
		{
			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Pre-conditon - should be no CLR log entries for this declaration", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));

			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "B00001252";
			declaration.JE_MasterBill = "081003498238";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.IPI;
			outgoingMessage.EM_ApplicationReference = "B00001252I";
			outgoingMessage.EM_LinkedObject = declaration.CusEntryHeader;
			outgoingMessage.EM_LinkUniqueID = declaration.CusEntryHeader.PK;

			ZString mpiFoodResponseExpected = @"[Ministry for Primary Industries (Food) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001252

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001252
Master Bill    : 081-003498238
Entry Type     : Import (Primary Industries Import)
Entry Number   : 8200273
Message No     : 474

Message Status : (F04) MPI Food - Cleared
";

			var mpifoodMessage = Factory.New<TSWMessage>();
			mpifoodMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpifoodMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
			mpifoodMessage.EM_MessageText = MPIFoodResponseForIPI;
			mpifoodMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpifoodMessage);
			AssertEquals(declaration.CusEntryHeader, mpifoodMessage.EM_LinkedObject);
			AssertEquals(mpiFoodResponseExpected, mpifoodMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIFood, entryStatus component representing MPIFood should be 0 for cleared.", "N90", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "8200273", declaration.DeclarationNumber);
			AssertEquals("CH_MPIFoodStatus", "F04", declaration.CusEntryHeader.CH_MPIFoodStatus);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpifoodMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, mpifoodMessage.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect pending agency response (Biosecurity)", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should show MPI Food has cleared but Bio is pending", TSWEntryStatusList.Codes.NPC, declaration.JE_TSWCombinedStatus);

			declaration.SaveHandlingSaveExceptions();
			logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("CLR Log Entry event should not be created for any cleared agency component of an IPI entry", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));

			ZString mpiBioResponseExpected = @"[Ministry for Primary Industries (Biosecurity) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001252

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001252
Master Bill    : 081-003498238
Entry Type     : Import (Primary Industries Import)
Entry Number   : 8200273
Message No     : 476

Message Status : (B04) MPI Biosecurity - Cleared

Delivery Instructions
---------------------------------------------------------------------
Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.

Declaration Status Updated
---------------------------------------------------------------------
From ARP to CLR.

";

			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			bioMessage.EM_MessageText = MPIBIOResponseForIPI;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(bioMessage);
			AssertEquals(declaration.CusEntryHeader, bioMessage.EM_LinkedObject);
			AssertEquals(mpiBioResponseExpected, bioMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIBio, entryStatus components representing MPIFood & MPIBio should now both be 0 for cleared.", "N00", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "8200273", declaration.DeclarationNumber);
			AssertEquals("CH_MPIBioStatus", "B04", declaration.CusEntryHeader.CH_MPIBioStatus);
			AssertEquals("JE_EntryStatus should reflect entry now cleared", FormalEntryStatusList.Codes.EntryCleared, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect both Food & Bio have cleared", TSWEntryStatusList.Codes.NCC, declaration.JE_TSWCombinedStatus);

			declaration.SaveHandlingSaveExceptions();
			logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("CLR Log Entry event should not be created for a cleared IPI entry", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
		}

		public void TestACKReceivedAfterMPIClearResponseIPIDec()
		{
			declaration.JE_DeclarationReference = "B00001496";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MasterBill = "08600239282";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			outgoingMessage.EM_ApplicationReference = "B00001496";
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.IPI;
			outgoingMessage.EM_LinkedObject = declaration.CusEntryHeader;
			outgoingMessage.EM_LinkUniqueID = declaration.CusEntryHeader.PK;

			ZString mpiFoodResponseExpected = @"[Ministry for Primary Industries (Food) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001496

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001496
Master Bill    : 086-00239282
Entry Type     : Import (Primary Industries Import)
Entry Number   : 37470892
Message No     : 1548

Message Status : (F04) MPI Food - Cleared
";

			var mpiMessage = Factory.New<TSWMessage>();
			mpiMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpiMessage.EM_MessageText = IPIB00001496Clr;
			mpiMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpiMessage);
			AssertEquals(declaration.CusEntryHeader, mpiMessage.EM_LinkedObject);
			AssertEquals(mpiFoodResponseExpected, mpiMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from TSW, entryStatus components representing MPIFood should be 0 for cleared.", "N90", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "37470892", declaration.DeclarationNumber);
			AssertEquals("JE_EntryStatus should now reflect TSW agency response is still pending", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect MPI Food response has cleared, Bio pending", TSWEntryStatusList.Codes.NPC, declaration.JE_TSWCombinedStatus);

			var ackMessage = Factory.New<TSWMessage>();
			ackMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			ackMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement;
			ackMessage.EM_MessageText = IPIB00001496Ack;
			ackMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ackMessage);
			AssertEquals(declaration.CusEntryHeader, ackMessage.EM_LinkedObject);
			AssertEquals("Status should not be overriden by ACK message received second - JE_TSWCombinedStatus should reflect the response from TSW, entryStatus components representing MPIFood should be 0 for cleared.", "N90", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "37470892", declaration.DeclarationNumber);
			AssertEquals("Status should not be overriden by ACK message received second - JE_EntryStatus should reflect MPI Biosecurity response still pending", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("Status should not be overriden by ACK message received second - JE_TSWCombinedStatus should reflect MPI response has cleared", TSWEntryStatusList.Codes.NPC, declaration.JE_TSWCombinedStatus);
		}

		public void TestBIOClearanceResponseReleasesDeliveryOrderStatus()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			declaration.JE_DeclarationReference = "B00001980";
			declaration.JE_MasterBill = "09811111111";
			outgoingMessage.EM_ApplicationReference = "B00001980";

			ZString mpiFoodResponseExpected = @"[Ministry for Primary Industries (Food) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001980

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001980
Master Bill    : 098-11111111
Entry Type     : Import (Normal)
Entry Number   : 73218058
Message No     : 3157

Message Status : (F04) MPI Food - Cleared
";

			var mpifoodMessage = Factory.New<TSWMessage>();
			mpifoodMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpifoodMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
			mpifoodMessage.EM_MessageText = MPIFoodResponseB00001980;
			mpifoodMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpifoodMessage);
			AssertEquals(entryHeader, mpifoodMessage.EM_LinkedObject);
			AssertEquals(mpiFoodResponseExpected, mpifoodMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIFood, entryStatus component representing MPIFood should be 0 for cleared.", "990", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "73218058", declaration.DeclarationNumber);
			AssertEquals("CH_MPIFoodStatus", "F04", entryHeader.CH_MPIFoodStatus);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpifoodMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, mpifoodMessage.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect pending agency responses", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should show MPI Food has cleared", TSWEntryStatusList.Codes.PPC, declaration.JE_TSWCombinedStatus);

			ZString nzcsResponseExpected = @"[New Zealand Customs Service - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001980

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number      : B00001980
Master Bill     : 098-11111111
Entry Type      : Import (Normal)
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

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			nzcsMessage.EM_MessageText = NZCSResponseB00001980;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcsMessage);
			AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
			AssertEquals(nzcsResponseExpected, nzcsMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from Customs, entryStatus components representing MPIFood & NZCS should now be updated.", "090", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "73218058", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "819", entryHeader.CH_NZCSStatus);
			AssertEquals("Delivery/Customs instructions:", "1 LOOSE PACKAGE(S) OR ITEM(S)", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("JE_EntryStatus should show a Delivery Order has been received from Customs even though Agency Responses are still pending.", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect agency responses and payment is pending", TSWEntryStatusList.Codes.CPC, declaration.JE_TSWCombinedStatus);

			ZString mpiBioResponseExpected = @"[Ministry for Primary Industries (Biosecurity) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001980

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001980
Master Bill    : 098-11111111
Entry Type     : Import (Normal)
Entry Number   : 73218058
Message No     : 3159

Message Status : (B04) MPI Biosecurity - Cleared

Delivery Instructions
---------------------------------------------------------------------
Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.
";

			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			bioMessage.EM_MessageSubType = "BIO";
			bioMessage.EM_MessageText = MPIBIOResponseB00001980;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(bioMessage);
			AssertEquals(entryHeader, bioMessage.EM_LinkedObject);
			AssertEquals(mpiBioResponseExpected, bioMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIBio, entryStatus components representing MPIFood, NZCS & MPIBio should now all be 0 for cleared.", "000", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "73218058", declaration.DeclarationNumber);
			AssertEquals("CH_MPIBioStatus", "B04", entryHeader.CH_MPIBioStatus);
			AssertEquals("JE_EntryStatus should now reflect that a Delivery Order has already been received & can be used", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect all agencies have cleared", TSWEntryStatusList.Codes.CCC, declaration.JE_TSWCombinedStatus);

			declaration.SaveHandlingSaveExceptions();

			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("CLR Log Entry event should have now been created - CLR will be created when SCM is created", true, logEntries.Any());
			AssertEquals("Only 1 CLR event should be created", 1, logEntries.Length);
			AssertEquals("NZ Import", logEntries[0].SL_Reference);
		}

		public void TestBIOClearanceResponseReleasesDeliveryOrderStatus_OnShipment()
		{
			// repeat of above, only with dec attached to shipment
			// Verify log entry is on shipment.
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C"))
			{
				var shipment = Factory.New<ForwardingShipment>();
				declaration.JE_JS = shipment.PK;

				declaration.JE_DeclarationReference = "B00001980";
				declaration.JE_MasterBill = "09811111111";
				outgoingMessage.EM_ApplicationReference = "B00001980";

				ZString mpiFoodResponseExpected = @"[Ministry for Primary Industries (Food) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001980

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001980
Master Bill    : 098-11111111
Entry Type     : Import (Normal)
Entry Number   : 73218058
Message No     : 3157

Message Status : (F04) MPI Food - Cleared
";

				var mpifoodMessage = Factory.New<TSWMessage>();
				mpifoodMessage.EM_MessageType = MessageTypeList.Codes.TWR;
				mpifoodMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
				mpifoodMessage.EM_MessageText = MPIFoodResponseB00001980;
				mpifoodMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

				var logger = new LoggingInformation();
				var processor = new MessageProcessorFactory(logger);
				processor.ProcessMessage(mpifoodMessage);
				AssertEquals(entryHeader, mpifoodMessage.EM_LinkedObject);
				AssertEquals(mpiFoodResponseExpected, mpifoodMessage.EM_MessageInterpretation);
				AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIFood, entryStatus component representing MPIFood should be 0 for cleared.", "990", declaration.JE_TSWCombinedStatus);
				AssertEquals("Entry Number", "73218058", declaration.DeclarationNumber);
				AssertEquals("CH_MPIFoodStatus", "F04", entryHeader.CH_MPIFoodStatus);
				AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpifoodMessage.EM_MessageType);
				AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, mpifoodMessage.EM_MessageSubType);
				AssertEquals("JE_EntryStatus should reflect pending agency responses", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
				AssertEquals("JE_TSWCombinedStatus should show MPI Food has cleared", TSWEntryStatusList.Codes.PPC, declaration.JE_TSWCombinedStatus);

				ZString nzcsResponseExpected = @"[New Zealand Customs Service - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001980

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number      : B00001980
Master Bill     : 098-11111111
Entry Type      : Import (Normal)
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

				var nzcsMessage = Factory.New<TSWMessage>();
				nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;
				nzcsMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
				nzcsMessage.EM_MessageText = NZCSResponseB00001980;
				nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				processor.ProcessMessage(nzcsMessage);
				AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
				AssertEquals(nzcsResponseExpected, nzcsMessage.EM_MessageInterpretation);
				AssertEquals("JE_TSWCombinedStatus should reflect the response from Customs, entryStatus components representing MPIFood & NZCS should now be updated.", "090", declaration.JE_TSWCombinedStatus);
				AssertEquals("Entry Number should remain the same for all responses on the same declaration", "73218058", declaration.DeclarationNumber);
				AssertEquals("CH_NZCSStatus", "819", entryHeader.CH_NZCSStatus);
				AssertEquals("Delivery/Customs instructions:", "1 LOOSE PACKAGE(S) OR ITEM(S)", entryHeader.CH_CustomsDeliveryInstructions);
				AssertEquals("JE_EntryStatus should show a Delivery Order has been received from Customs even though Agency Responses are still pending.", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
				AssertEquals("JE_TSWCombinedStatus should reflect agency responses and payment is pending", TSWEntryStatusList.Codes.CPC, declaration.JE_TSWCombinedStatus);

				ZString mpiBioResponseExpected = @"[Ministry for Primary Industries (Biosecurity) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00001980

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00001980
Master Bill    : 098-11111111
Entry Type     : Import (Normal)
Entry Number   : 73218058
Message No     : 3159

Message Status : (B04) MPI Biosecurity - Cleared

Delivery Instructions
---------------------------------------------------------------------
Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.
";

				var bioMessage = Factory.New<TSWMessage>();
				bioMessage.EM_MessageType = MessageTypeList.Codes.TWR;
				bioMessage.EM_MessageSubType = "BIO";
				bioMessage.EM_MessageText = MPIBIOResponseB00001980;
				bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				processor.ProcessMessage(bioMessage);
				AssertEquals(entryHeader, bioMessage.EM_LinkedObject);
				AssertEquals(mpiBioResponseExpected, bioMessage.EM_MessageInterpretation);
				AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIBio, entryStatus components representing MPIFood, NZCS & MPIBio should now all be 0 for cleared.", "000", declaration.JE_TSWCombinedStatus);
				AssertEquals("Entry Number should remain the same for all responses on the same declaration", "73218058", declaration.DeclarationNumber);
				AssertEquals("CH_MPIBioStatus", "B04", entryHeader.CH_MPIBioStatus);
				AssertEquals("JE_EntryStatus should now reflect that a Delivery Order has already been received & can be used", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
				AssertEquals("JE_TSWCombinedStatus should reflect all agencies have cleared", TSWEntryStatusList.Codes.CCC, declaration.JE_TSWCombinedStatus);

				declaration.SaveHandlingSaveExceptions();
				var logEntries = shipment.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
				AssertEquals("Only 1 CLR event should be created", 1, logEntries.Length);
				AssertEquals("NZ Import", logEntries[0].SL_Reference);
				var clrEventTime = logEntries[0].SL_EventTime;
			}
		}

		public void TestACKWithTimeStampAfterCLR()
		{
			declaration.JE_DeclarationReference = "B00001575";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MasterBill = "08600393282";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			outgoingMessage.EM_ApplicationReference = "B00001575";
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.IPI;
			outgoingMessage.EM_LinkedObject = declaration.CusEntryHeader;
			outgoingMessage.EM_LinkUniqueID = declaration.CusEntryHeader.PK;

			var mpiMessage = Factory.New<TSWMessage>();
			mpiMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpiMessage.EM_MessageText = B00001575Clr;
			mpiMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpiMessage);
			AssertEquals(declaration.CusEntryHeader, mpiMessage.EM_LinkedObject);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from TSW, entryStatus components representing MPIFood should be 0 for cleared.", "N90", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "48380311", declaration.DeclarationNumber);
			AssertEquals("JE_EntryStatus should reflect at least 1 TSW response is still pending", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect MPI Food response has cleared", TSWEntryStatusList.Codes.NPC, declaration.JE_TSWCombinedStatus);
			AssertEquals("Message status should be empty as acknowledgment message not yet processed", ZString.Empty, declaration.JE_MessageStatus);

			var ackMessage = Factory.New<TSWMessage>();
			ackMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			ackMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement;
			ackMessage.EM_MessageText = B00001575Ack;
			ackMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ackMessage);
			AssertEquals(declaration.CusEntryHeader, ackMessage.EM_LinkedObject);
			AssertEquals("Combined Entry Status should not be overriden by ACK message received second, even though the time stamp is slightly after the Clearance - JE_TSWCombinedStatus should reflect the response from TSW, entryStatus components representing MPIFood should be 0 for cleared.", "N90", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "48380311", declaration.DeclarationNumber);
			AssertEquals("Entry Status should not be overriden by ACK message received second by a few seconds - JE_EntryStatus should reflect TSW Food cleared, BIO pending", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("Status should not be overriden by ACK message received second by a few seconds - JE_TSWCombinedStatus should reflect MPI response has cleared", TSWEntryStatusList.Codes.NPC, declaration.JE_TSWCombinedStatus);
			AssertEquals("Processing of Acknowledgement message should have updated message status", StatusListBase.Codes.Acknowledgement, declaration.JE_MessageStatus);
		}

		public void TestDeliveryInstructionsIsNotDuplicated()
		{
			AssertEquals("Pre-condition: CH_CustomsDeliveryInstructions", ZString.Empty, declaration.CusEntryHeader.CH_CustomsDeliveryInstructions);
			outgoingMessage.EM_ApplicationReference = "B00002120";

			#region Original Responses

			string originalAck =
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
    <IssueDateTime formatCode=""204"">20151104134415</IssueDateTime>
    <FunctionalReferenceID>3481</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>72195033</ID>
        <FunctionalReferenceID>B00002120</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20151104134415</EffectiveDateTime>
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

			string originalFood =
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
    <IssueDateTime formatCode=""204"">20151104134414</IssueDateTime>
    <FunctionalReferenceID>3484</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>72195033</ID>
        <AcceptanceDateTime formatCode=""204"">20151104134414</AcceptanceDateTime>
        <FunctionalReferenceID>B00002120</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20151104134414</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20151104134414</ReleaseDateTime>
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

			string originalBio =
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
    <IssueDateTime formatCode=""204"">20151104134431</IssueDateTime>
    <FunctionalReferenceID>3490</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>72195033</ID>
        <AcceptanceDateTime formatCode=""204"">20151104134431</AcceptanceDateTime>
        <FunctionalReferenceID>B00002120</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20151104134431</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20151104134435</ReleaseDateTime>
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

			string originalNZC =
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
    <IssueDateTime formatCode=""204"">20151104134431</IssueDateTime>
    <FunctionalReferenceID>3488</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>200 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>72195033</ID>
        <AcceptanceDateTime formatCode=""204"">20151104134431</AcceptanceDateTime>
        <FunctionalReferenceID>B00002120</FunctionalReferenceID>
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
            <TaxAssessedAmount>7697.44</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20151104134431</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20151104134431</ReleaseDateTime>
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

			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();

			ProcessMessage(originalAck);
			ProcessMessage(originalFood);
			AssertEquals("Acknowledgement & MPI Food responses do not have any delivery instructions", ZString.Empty, declaration.CusEntryHeader.CH_CustomsDeliveryInstructions);
			ProcessMessage(originalNZC);
			AssertEquals("CH_CustomsDeliveryInstructions", "200 LOOSE PACKAGE(S) OR ITEM(S)", declaration.CusEntryHeader.CH_CustomsDeliveryInstructions);
			ProcessMessage(originalBio);
			AssertEquals("CH_CustomsDeliveryInstructions should now combine MPI & NZC instructions", "200 LOOSE PACKAGE(S) OR ITEM(S)\r\nBiosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.", declaration.CusEntryHeader.CH_CustomsDeliveryInstructions);

			#region Replacement Responses

			string replacementAck =
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
    <IssueDateTime formatCode=""204"">20151104140417</IssueDateTime>
    <FunctionalReferenceID>3497</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>72195033</ID>
        <FunctionalReferenceID>B00002120</FunctionalReferenceID>
        <VersionID>3</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20151104140417</EffectiveDateTime>
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

			string replacementFood =
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
    <IssueDateTime formatCode=""204"">20151104140416</IssueDateTime>
    <FunctionalReferenceID>3498</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>72195033</ID>
        <AcceptanceDateTime formatCode=""204"">20151104140416</AcceptanceDateTime>
        <FunctionalReferenceID>B00002120</FunctionalReferenceID>
        <VersionID>3</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20151104140416</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20151104140416</ReleaseDateTime>
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

			string replacementBio =
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
    <IssueDateTime formatCode=""204"">20151104140433</IssueDateTime>
    <FunctionalReferenceID>3500</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>72195033</ID>
        <AcceptanceDateTime formatCode=""204"">20151104140433</AcceptanceDateTime>
        <FunctionalReferenceID>B00002120</FunctionalReferenceID>
        <VersionID>3</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20151104140433</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20151104140436</ReleaseDateTime>
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

			string replacementNZC =
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
    <IssueDateTime formatCode=""204"">20151104140433</IssueDateTime>
    <FunctionalReferenceID>3499</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>200 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>72195033</ID>
        <AcceptanceDateTime formatCode=""204"">20151104140433</AcceptanceDateTime>
        <FunctionalReferenceID>B00002120</FunctionalReferenceID>
        <VersionID>3</VersionID>
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
            <TaxAssessedAmount>7697.44</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20151104140433</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20151104140433</ReleaseDateTime>
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

			ProcessMessage(replacementAck);
			ProcessMessage(replacementFood);
			AssertEquals("Acknowledgement & MPI Food responses do not have any delivery instructions", ZString.Empty, declaration.CusEntryHeader.CH_CustomsDeliveryInstructions);
			ProcessMessage(replacementNZC);
			AssertEquals("CH_CustomsDeliveryInstructions", "200 LOOSE PACKAGE(S) OR ITEM(S)", declaration.CusEntryHeader.CH_CustomsDeliveryInstructions);
			ProcessMessage(replacementBio);
			AssertEquals("CH_CustomsDeliveryInstructions should now combine MPI & NZC instructions", "200 LOOSE PACKAGE(S) OR ITEM(S)\r\nBiosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.", declaration.CusEntryHeader.CH_CustomsDeliveryInstructions);
		}

		public void TestDeliveryInstructionsOnlyIncludeLatestOneForEachSubType()
		{
			AssertEquals("Pre-condition: CH_CustomsDeliveryInstructions", ZString.Empty, declaration.CusEntryHeader.CH_CustomsDeliveryInstructions);
			outgoingMessage.EM_ApplicationReference = "B00002120";

			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			decCreator.MergeDeclaration();

			#region Original Responses

			string originalACK =
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
    <IssueDateTime formatCode=""204"">20160106093515</IssueDateTime>
    <FunctionalReferenceID>3481</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>72195033</ID>
        <FunctionalReferenceID>B00002120</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20160106093515</EffectiveDateTime>
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

			string originalFOOD =
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
    <IssueDateTime formatCode=""204"">20160106093615</IssueDateTime>
    <FunctionalReferenceID>3484</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>72195033</ID>
        <AcceptanceDateTime formatCode=""204"">20160106093615</AcceptanceDateTime>
        <FunctionalReferenceID>B00002120</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20160106093615</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20160106093615</ReleaseDateTime>
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

			string originalBIO1 =
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
    <IssueDateTime formatCode=""204"">20160106093615</IssueDateTime>
    <FunctionalReferenceID>3490</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand1.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>72195033</ID>
        <AcceptanceDateTime formatCode=""204"">20160106093615</AcceptanceDateTime>
        <FunctionalReferenceID>B00002120</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20160106093615</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20160106093615</ReleaseDateTime>
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

			string originalNZC1 =
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
    <IssueDateTime formatCode=""204"">20160106093615</IssueDateTime>
    <FunctionalReferenceID>3488</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>100 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>72195033</ID>
        <AcceptanceDateTime formatCode=""204"">20160106093615</AcceptanceDateTime>
        <FunctionalReferenceID>B00002120</FunctionalReferenceID>
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
            <TaxAssessedAmount>7697.44</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20160106093615</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20160106093615</ReleaseDateTime>
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

			ProcessMessage(originalACK, new ZDateTime(2016, 01, 06, 09, 35, 15));
			ProcessMessage(originalFOOD, new ZDateTime(2016, 01, 06, 09, 36, 15));
			ProcessMessage(originalNZC1, new ZDateTime(2016, 01, 06, 09, 35, 15));
			ProcessMessage(originalBIO1, new ZDateTime(2016, 01, 06, 09, 35, 15));
			AssertEquals("CH_CustomsDeliveryInstructions should now combine MPI & NZC instructions", "100 LOOSE PACKAGE(S) OR ITEM(S)\r\nBiosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand1.", declaration.CusEntryHeader.CH_CustomsDeliveryInstructions);

			#region Additional Original Responses

			string originalBIO2 =
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
    <IssueDateTime formatCode=""204"">20160106094015</IssueDateTime>
    <FunctionalReferenceID>3490</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand2.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>72195033</ID>
        <AcceptanceDateTime formatCode=""204"">20160106094015</AcceptanceDateTime>
        <FunctionalReferenceID>B00002120</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20160106094015</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20160106094015</ReleaseDateTime>
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

			string originalNZC2 =
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
    <IssueDateTime formatCode=""204"">20160106093915</IssueDateTime>
    <FunctionalReferenceID>3488</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>200 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>72195033</ID>
        <AcceptanceDateTime formatCode=""204"">20160106093915</AcceptanceDateTime>
        <FunctionalReferenceID>B00002120</FunctionalReferenceID>
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
            <TaxAssessedAmount>7697.44</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20160106093915</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20160106093915</ReleaseDateTime>
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

			ProcessMessage(originalNZC2, new ZDateTime(2016, 01, 06, 09, 39, 15));
			ProcessMessage(originalBIO2, new ZDateTime(2016, 01, 06, 09, 40, 15));
			AssertEquals("CH_CustomsDeliveryInstructions should now combine MPI & NZC instructions", "200 LOOSE PACKAGE(S) OR ITEM(S)\r\nBiosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand2.", declaration.CusEntryHeader.CH_CustomsDeliveryInstructions);

			#region Additional Original Responses

			string originalBIO3 =
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
    <IssueDateTime formatCode=""204"">20160108104015</IssueDateTime>
    <FunctionalReferenceID>3490</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand3.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>72195033</ID>
        <AcceptanceDateTime formatCode=""204"">20160108104015</AcceptanceDateTime>
        <FunctionalReferenceID>B00002120</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20160108104015</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20160108104015</ReleaseDateTime>
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

			string originalNZC3 =
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
    <IssueDateTime formatCode=""204"">20160108104015</IssueDateTime>
    <FunctionalReferenceID>3488</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>300 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>72195033</ID>
        <AcceptanceDateTime formatCode=""204"">20160108104015</AcceptanceDateTime>
        <FunctionalReferenceID>B00002120</FunctionalReferenceID>
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
            <TaxAssessedAmount>7697.44</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20160108104015</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20160108104015</ReleaseDateTime>
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

			ProcessMessage(originalBIO3, new ZDateTime(2016, 01, 08, 10, 40, 15));
			ProcessMessage(originalNZC3, new ZDateTime(2016, 01, 08, 10, 40, 15));
			AssertEquals("CH_CustomsDeliveryInstructions should now combine MPI & NZC instructions", "Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand3.\r\n300 LOOSE PACKAGE(S) OR ITEM(S)", declaration.CusEntryHeader.CH_CustomsDeliveryInstructions);
		}

		public void TestInvoicePostedAndLoggedAfterClearResponse()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");

			var testHelper = new InvoicingTestHelper(Factory);
			testHelper.SetUpDisbursementCreditorAndChargeCode();

			var group = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			group.Staff[0].GS_EmailAddress = "test@cargowise.com";
			var groupNotification = new AutoBillingGroupNotification();
			groupNotification.SendGroupPK = group.PK;
			CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);

			var option = new AccountingIntegrationOptions();
			option.EnableAccountingIntegration = true;
			option.APPostDSB = true;
			option.ARPostDSB = true;
			CustomsDataRegistry.Instance.EnableAccountingIntegration.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, option);

			declaration.JE_PaymentMethod = Enterprise.MasterFiles.Business.PaymentMethodList.Codes.CashPaidByBroker;
			declaration.JE_DeclarationReference = "B00001296";
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Simplified;
			declaration.JE_MasterBill = "08133459823";
			outgoingMessage.EM_ApplicationReference = "B00001296";
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I11;

			var mpifoodMessage = Factory.New<TSWMessage>();
			mpifoodMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpifoodMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
			mpifoodMessage.EM_MessageText = MPIFR;
			mpifoodMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpifoodMessage);
			AssertEquals(entryHeader, mpifoodMessage.EM_LinkedObject);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpifoodMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, mpifoodMessage.EM_MessageSubType);

			var ackMessage = Factory.New<TSWMessage>();
			ackMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			ackMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.Acknowledgement;
			ackMessage.EM_MessageText = NZCSAcknowledgment;
			ackMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(ackMessage);
			AssertEquals(entryHeader, ackMessage.EM_LinkedObject);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "47248628", declaration.DeclarationNumber);

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			nzcsMessage.EM_MessageText = NZCSR;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcsMessage);
			AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "47248628", declaration.DeclarationNumber);

			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			bioMessage.EM_MessageSubType = "BIO";
			bioMessage.EM_MessageText = MPIBR;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(bioMessage);
			AssertEquals(entryHeader, bioMessage.EM_LinkedObject);

			var orgBoth = Factory.NewWithValidTestData<OrgHeader>();
			orgBoth.OH_IsConsignee = true;
			orgBoth.OH_IsConsignor = true;
			orgBoth.OH_IsDebtor = true;
			declaration.JE_OH_Importer = orgBoth.PK;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.Duty, 100m);
			entryLine.Fees.AddOrUpdate(EntryChargeTypeList.Codes.GST, 101m);

			entryHeader.Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var query = new ZQuery(JobHeaderSchema.JH_ParentID, declaration.PK);
			query.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
			var job = newFactory.LoadTop1<JobHeader>(query);
			AssertNotNull("Auto rated", job);

			query = new ZQuery(StmALogSchema.SL_SE_NKEvent, ZArchitecture.Business.Events.ServiceInvoicePosted.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, "FIN INV B00001296");
			query.AddToFilter(StmALogSchema.SL_IsCancelled, false);
			AssertNotNull(job.Logs.Find(query).FirstOrDefault());
		}

		public void TestFormalDecChangedToIPIClearanceResponse()
		{
			#region Formal Declaration processing

			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			declaration.JE_DeclarationReference = "B00002656";
			declaration.JE_MasterBill = "08100239234";
			outgoingMessage.EM_ApplicationReference = "B00002656";

			ZString mpiFoodResponseExpected = @"[Ministry for Primary Industries (Food) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00002656

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00002656
Master Bill    : 081-00239234
Entry Type     : Import (Normal)
Entry Number   : 56734084
Message No     : 5375

Message Status : (F04) MPI Food - Cleared
";

			var mpifoodMessage = Factory.New<TSWMessage>();
			mpifoodMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpifoodMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
			mpifoodMessage.EM_MessageText = MPIFoodResponseB00002656;
			mpifoodMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpifoodMessage);
			AssertEquals(entryHeader, mpifoodMessage.EM_LinkedObject);
			AssertEquals(mpiFoodResponseExpected, mpifoodMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIFood, entryStatus component representing MPIFood should be 0 for cleared.", "990", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "56734084", declaration.DeclarationNumber);
			AssertEquals("CH_MPIFoodStatus", "F04", entryHeader.CH_MPIFoodStatus);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpifoodMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, mpifoodMessage.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect pending agency responses", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should show MPI Food has cleared", TSWEntryStatusList.Codes.PPC, declaration.JE_TSWCombinedStatus);
			AssertEquals("CH_EntryStatus should reflect the Formal (NOR) declaration entry status", declaration.JE_EntryStatus, entryHeader.CH_EntryStatus);

			ZString mpiBioResponseExpected = @"[Ministry for Primary Industries (Biosecurity) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00002656

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00002656
Master Bill    : 081-00239234
Entry Type     : Import (Normal)
Entry Number   : 56734084
Message No     : 5377

Message Status : (B04) MPI Biosecurity - Cleared

Delivery Instructions
---------------------------------------------------------------------
Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.
";

			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			bioMessage.EM_MessageText = MPIBIOResponseB00002656;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(bioMessage);
			AssertEquals(entryHeader, bioMessage.EM_LinkedObject);
			AssertEquals(mpiBioResponseExpected, bioMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIBio, entryStatus components representing MPIFood & MPIBio should now both be 0 for cleared.", "900", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "56734084", declaration.DeclarationNumber);
			AssertEquals("CH_MPIBioStatus", "B04", entryHeader.CH_MPIBioStatus);
			AssertEquals("JE_EntryStatus should still reflect pending agency response - Customs message response still to process", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("CH_EntryStatus should reflect the Formal (NOR) declaration entry status", declaration.JE_EntryStatus, entryHeader.CH_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect both Food & Bio have cleared", TSWEntryStatusList.Codes.PCC, declaration.JE_TSWCombinedStatus);

			ZString nzcsResponseExpected = @"[New Zealand Customs Service - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00002656

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number      : B00002656
Master Bill     : 081-00239234
Entry Type      : Import (Normal)
Entry Number    : 56734084
Message No      : 5376

Message Status  : (822) Entry Cleared
                : cash to pay prior to delivery.

Amount Returned : $1,425.12
Terms           : Cash
";

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsMessage.EM_MessageText = NZCSResponseB00002656;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcsMessage);
			AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
			AssertEquals(nzcsResponseExpected, nzcsMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from Customs, entryStatus components representing MPIFood & MPIBio & NZCS should now be 0 for cleared.", "600", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "56734084", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "822", entryHeader.CH_NZCSStatus);
			AssertEquals("Delivery/Customs instructions:", "Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("JE_EntryStatus should now reflect TSW cleared", FormalEntryStatusList.Codes.DeliveryOnPayment, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect all agency responses have cleared, but payment is pending", TSWEntryStatusList.Codes.YCC, declaration.JE_TSWCombinedStatus);
			AssertEquals("CH_EntryStatus should reflect the Formal (NOR) declaration entry status", declaration.JE_EntryStatus, entryHeader.CH_EntryStatus);

			declaration.SaveHandlingSaveExceptions();
			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Log Entry event has not been created as payment is pending", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
			logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("CLR Log Entry event should not have been created - CLR will only be created when SCM is created", false, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));

			#endregion

			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			PrimaryIndustriesCusEntryHeader iPIHeader = (PrimaryIndustriesCusEntryHeader)declaration.GetAppropriateCusEntryHeaderIfExists();
			AssertNotNull("IPIHeader should have been created when approved formal declaration was changed to add an IPI entry", iPIHeader);
			iPIHeader.CH_BGMReference = "B00002656I";

			outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.IPI;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = iPIHeader;
			outgoingMessage.EM_LinkUniqueID = iPIHeader.PK;
			outgoingMessage.EM_ApplicationReference = "B00002656I";

			ZString mpiIPIResponseExpected = @"[Ministry for Primary Industries (Food) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00002656

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00002656
Master Bill    : 081-00239234
Entry Type     : Import (Primary Industries Import)
Entry Number   : 80843452
Message No     : 5378

Message Status : (F04) MPI Food - Cleared

Declaration Status Updated
---------------------------------------------------------------------
From DOP to ARP.

";

			var mpiIPIMessage = Factory.New<TSWMessage>();
			mpiIPIMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpiIPIMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
			mpiIPIMessage.EM_ApplicationReference = "B00002656I";
			mpiIPIMessage.EM_MessageText = MPIIPIResponseB00002656;
			mpiIPIMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpiIPIMessage);
			AssertEquals(iPIHeader, mpiIPIMessage.EM_LinkedObject);
			AssertEquals(mpiIPIResponseExpected, mpiIPIMessage.EM_MessageInterpretation);
			AssertEquals("Entry Number should be a new number for the IPI declaration", "80843452", declaration.DeclarationNumber);
			var entryNum = Factory.LoadTop1<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_EntryNum, "80843452"));
			AssertEquals("Entry Number type should be IPI for Primary Industries", "IPI", entryNum.CE_EntryType);
			AssertEquals("CH_MPIFoodStatus", "F04", entryHeader.CH_MPIFoodStatus);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpiIPIMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, mpiIPIMessage.EM_MessageSubType);
			AssertEquals("JE_EntryStatus", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should show MPI Food has cleared", TSWEntryStatusList.Codes.NPC, declaration.JE_TSWCombinedStatus);
			AssertEquals("IPI header CH_EntryStatus should reflect the Primary Industries (IPI) declaration entry status", declaration.JE_EntryStatus, iPIHeader.CH_EntryStatus);

			ZString mpiIPIBioResponseExpected = @"[Ministry for Primary Industries (Biosecurity) - Inspection / Audit requirements] Response for Customs Import Declaration: B00002656

Inspection / Audit requirements
---------------------------------------------------------------------
Job Number     : B00002656
Master Bill    : 081-00239234
Entry Type     : Import (Primary Industries Import)
Entry Number   : 80843452
Message No     : 5380

Message Status : (B05) MPI Biosecurity - Directions Given

Customs Instructions
---------------------------------------------------------------------
Lodgement has been received by MPI, please await further direction

Declaration Status Updated
---------------------------------------------------------------------
From DOP to IAR.

";

			var mpiIPIBIOMessage = Factory.New<TSWMessage>();
			mpiIPIBIOMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpiIPIBIOMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
			mpiIPIBIOMessage.EM_ApplicationReference = "B00002656I";
			mpiIPIBIOMessage.EM_MessageText = MPIIPIBioResponseB00002656;
			mpiIPIBIOMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpiIPIBIOMessage);
			AssertEquals(iPIHeader, mpiIPIBIOMessage.EM_LinkedObject);
			AssertEquals(mpiIPIBioResponseExpected, mpiIPIBIOMessage.EM_MessageInterpretation);
			AssertEquals("Entry Number should be the same new entry number previously provided for the IPI declaration", "80843452", declaration.DeclarationNumber);
			AssertEquals("CH_MPIBioStatus", "B05", iPIHeader.CH_MPIBioStatus);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpiIPIBIOMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType", "BIO", mpiIPIBIOMessage.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect inspection response", FormalEntryStatusList.Codes.InspectionsAuditRequirements, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should show MPI Food has cleared - MPI Biosecurity impediment", TSWEntryStatusList.Codes.NIC, declaration.JE_TSWCombinedStatus);
			AssertEquals("IPI header CH_EntryStatus should reflect the Primary Industries (IPI) declaration entry status", declaration.JE_EntryStatus, iPIHeader.CH_EntryStatus);

			//Changing between entry headers should display the relevant status for the current entry showing on the declaration.
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.Normal;
			AssertEquals("EntryStatusDescription", FormalEntryStatusList.Descriptions.DeliveryOnPayment, declaration.JE_EntryStatusDescription);

			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			AssertEquals("EntryStatusDescription", FormalEntryStatusList.Descriptions.InspectionsAuditRequirements, declaration.JE_EntryStatusDescription);

			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.Normal;
			AssertEquals("EntryStatusDescription", FormalEntryStatusList.Descriptions.DeliveryOnPayment, declaration.JE_EntryStatusDescription);
		}

		public void TestTransmitDateIsResetOnEntryRejection()
		{
			declaration.JE_DeclarationReference = "B00001825";
			declaration.JE_EDITransmitDate = ZDateTime.Today;
			var originalEntryHeader = declaration.CusEntryHeader;
			originalEntryHeader.CH_EDITransmitDate = ZDateTime.Today;
			var outgoingMessage = originalEntryHeader.Messages.AddNew();
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_MessageText = OutgoingRejection;
			outgoingMessage.EM_MessageNum = "1";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "B00001825";
			outgoingMessage.EM_LinkedObject = originalEntryHeader;

			var nzcsResponseMessage = Factory.New<TSWMessage>();
			nzcsResponseMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsResponseMessage.EM_MessageText = NZCS00000000Rejection;
			nzcsResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsResponseMessage);
			AssertEquals(entryHeader, nzcsResponseMessage.EM_LinkedObject);
			AssertEquals("CH_EDITransmitDate should have been reset on original entry fail", ZDateTime.Empty, entryHeader.CH_EDITransmitDate);
			AssertEquals("Entry Number should be empty", "", declaration.DeclarationNumber);
			AssertEquals("JE_EDITransmitDate should have been reset on original entry fail", ZDateTime.Empty, declaration.JE_EDITransmitDate);

			AssertEquals("CH_NZCSStatus", "801", entryHeader.CH_NZCSStatus);
			AssertEquals("JE_EntryStatus should reflect the Customs error as priority.", FormalEntryStatusList.Codes.EntryInError, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect Food Clear, Bio pending, Customs error", TSWEntryStatusList.Codes.EPP, declaration.JE_TSWCombinedStatus);
		}

		public void TestNullRefFromRejectedStandAloneDecWriteoff()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "B00151034";
			declaration.JE_EDITransmitDate = ZDateTime.Today;
			var originalEntryHeader = declaration.CusEntryHeader;
			var outgoingMessage = originalEntryHeader.Messages.AddNew();
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_MessageText = B151034Msg;
			outgoingMessage.EM_MessageNum = "1";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "B00151034";
			outgoingMessage.EM_LinkedObject = originalEntryHeader;

			var nzcsResponseMessage = Factory.New<TSWMessage>();
			nzcsResponseMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsResponseMessage.EM_MessageText = MessageError;
			nzcsResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsResponseMessage);
			AssertEquals(originalEntryHeader, nzcsResponseMessage.EM_LinkedObject);
			AssertEquals("Response message Status", EDIMessage.Status.Received, nzcsResponseMessage.EM_Status);
			AssertEquals("Entry Number should be empty", "", declaration.DeclarationNumber);
			AssertEquals("JE_EDITransmitDate should have been reset on original entry fail", ZDateTime.Empty, declaration.JE_EDITransmitDate);

			AssertEquals("JE_MessageStatus", "841", declaration.JE_MessageStatus);
			AssertEquals("JE_EntryStatus should reflect the Customs error", LowValueConsignmentStatusList.Codes.ConsignmentInError, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect the Customs entry error", LowValueConsignmentStatusList.Codes.ConsignmentInError, declaration.JE_TSWCombinedStatus);
		}

		public void TestTransmitDateIsResetOnEntryFail()
		{
			declaration.JE_DeclarationReference = "B00002644";
			declaration.JE_EDITransmitDate = ZDateTime.Today;
			var originalEntryHeader = declaration.CusEntryHeader;
			var outgoingMessage = originalEntryHeader.Messages.AddNew();
			outgoingMessage.EM_MessageSubType = "ORG";
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_MessageText = OutgoingRejection;
			outgoingMessage.EM_MessageNum = "1";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "B00002644";
			outgoingMessage.EM_LinkedObject = originalEntryHeader;

			var nzcsResponseMessage = Factory.New<TSWMessage>();
			nzcsResponseMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsResponseMessage.EM_MessageText = NZCS00000000Error;
			nzcsResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(nzcsResponseMessage);
			AssertEquals(entryHeader, nzcsResponseMessage.EM_LinkedObject);
			AssertEquals("Entry Number should be empty", "", declaration.DeclarationNumber);
			AssertEquals("JE_EDITransmitDate should have been reset on original entry fail", ZDateTime.Empty, declaration.JE_EDITransmitDate);

			AssertEquals("CH_NZCSStatus", "858", entryHeader.CH_NZCSStatus);
			AssertEquals("JE_EntryStatus should reflect the Customs error as priority.", FormalEntryStatusList.Codes.EntryInError, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect Food Clear, Bio pending, Customs error", TSWEntryStatusList.Codes.EPP, declaration.JE_TSWCombinedStatus);
		}

		public void TestDuplicateMessageErrorResponseIsNotProcessedToJob()
		{
			declaration.JE_DeclarationReference = "B00001575";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MasterBill = "08600393282";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			outgoingMessage.EM_ApplicationReference = "B00001575";
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.IPI;
			outgoingMessage.EM_Status = "SNT";
			outgoingMessage.EM_LinkedObject = declaration.CusEntryHeader;
			outgoingMessage.EM_LinkUniqueID = declaration.CusEntryHeader.PK;
			declaration.CusEntryHeader.Messages.Add(outgoingMessage);

			var mpiMessage = Factory.New<TSWMessage>();
			mpiMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpiMessage.EM_MessageText = B00001575Clr;
			mpiMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpiMessage);
			AssertEquals(declaration.CusEntryHeader, mpiMessage.EM_LinkedObject);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from TSW, entryStatus components representing MPIFood should be 0 for cleared.", "N90", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "48380311", declaration.DeclarationNumber);
			AssertEquals("JE_EntryStatus should now reflect TSW cleared", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect MPI response has cleared", TSWEntryStatusList.Codes.NPC, declaration.JE_TSWCombinedStatus);
			AssertEquals("Message status should be empty as acknowledgment message not yet processed", ZString.Empty, declaration.JE_MessageStatus);
			declaration.CusEntryHeader.Messages.Add(mpiMessage);

			var dupMessage = Factory.New<TSWMessage>();
			dupMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			dupMessage.EM_MessageText = B00001575Dup;
			dupMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(dupMessage);
			AssertNotEquals("Duplicate error Message should not be linked to entry", declaration.CusEntryHeader, dupMessage.EM_LinkedObject);
			AssertEquals("Combined Entry Status should not be overriden by error message received.", "N90", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "48380311", declaration.DeclarationNumber);
			AssertEquals("Entry Status should not be overriden by error message received - JE_EntryStatus should remain as TSW cleared", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("Status should not be overriden by the error message received - JE_TSWCombinedStatus should still reflect MPI response has cleared", TSWEntryStatusList.Codes.NPC, declaration.JE_TSWCombinedStatus);
			AssertEquals("EM_Status", "ERR", dupMessage.EM_Status);
			bool messageHasIgnoredNote = false;
			StmNote[] msgNotes = dupMessage.Notes.FindByDescription("Senders Ref Duplicate");
			foreach (StmNote note in msgNotes)
			{
				if (note.ST_NoteDataAsText.ToString() == "This unsolicited duplicate message error was detected and ignored due to the lack of any pending outgoing messages.")
				{
					messageHasIgnoredNote = true;
					break;
				}
			}
			AssertEquals("Message has ignored note", true, messageHasIgnoredNote);

			var dupMessage2 = Factory.New<TSWMessage>();
			dupMessage2.EM_MessageType = MessageTypeList.Codes.TWR;
			dupMessage2.EM_MessageText = B00001575Dup;
			dupMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			logger = new LoggingInformation();
			processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(dupMessage2);
			AssertNotEquals("Duplicate error Messages should not be linked to entry", declaration.CusEntryHeader, dupMessage2.EM_LinkedObject);
			AssertEquals("Combined Entry Status should not be overriden by the duplicate error message received.", "N90", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain", "48380311", declaration.DeclarationNumber);
			AssertEquals("Entry Status should not be overriden by error message received - JE_EntryStatus should remain as TSW agency response pending", FormalEntryStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("Status should not be overriden by the error message received - JE_TSWCombinedStatus should still reflect MPI response has cleared", TSWEntryStatusList.Codes.NPC, declaration.JE_TSWCombinedStatus);
			AssertEquals("EM_Status", "ERR", dupMessage2.EM_Status);
		}

		public void TestDuplicateMessageErrorResponseIsProcessedWhenValidResponseToJob()
		{
			declaration.JE_DeclarationReference = "B00001575";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = MessageSubTypeCombinedList.Codes.IPI;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MasterBill = "08600393282";

			outgoingMessage.EM_ApplicationReference = "B00001575";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.IPI;
			outgoingMessage.EM_Status = "SNT";
			outgoingMessage.EM_LinkedObject = declaration.CusEntryHeader;
			outgoingMessage.EM_LinkUniqueID = declaration.CusEntryHeader.PK;
			declaration.CusEntryHeader.Messages.Add(outgoingMessage);

			var errorMessage = Factory.New<TSWMessage>();
			errorMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			errorMessage.EM_MessageText = B00001575Dup;
			errorMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(errorMessage);
			AssertEquals("Message with Duplicate error Message should be linked to entry as entry was waiting for response", declaration.CusEntryHeader, errorMessage.EM_LinkedObject);
			AssertEquals("Entry Number should have been overwrittern by error message value", "", declaration.DeclarationNumber);
			AssertEquals("EM_Status - Message processed", "RCV", errorMessage.EM_Status);
		}

		public void TestWriteOffEntryStatusIsConsistentWithMultipleAgencyResponses()
		{
			/*
			 *	Import ICR write off messages:
			 *	JE_EntryStatus is sometimes showing an ECI Consignment as Written Off/Cleared, when the TSW combined status reflects 1 agency may in fact have a hold on the consignment
			 *	e.g. NZCS - Written Off / BIO - Consignment Held	- see B00003591 in NZ UAT
			 *	this is the crux of this test case:
			 *		AssertEquals("JE_EntryStatus should reflect entry is still HELD", LowValueConsignmentStatusList.Codes.ConsignmentHeld, declaration.JE_EntryStatus);
			*/
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "B00003591";
			declaration.JE_MasterBill = "08100023494";
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			Declaration.ECIWriteOff.CusEntryHeader entryHeader = (Declaration.ECIWriteOff.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew(typeof(Declaration.ECIWriteOff.CusEntryHeader));

			outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_LinkUniqueID = entryHeader.PK;
			outgoingMessage.EM_ApplicationReference = "B00003591";

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);

			ZString mpiBioResponseExpected = @"[Inspections/Audit Requirements] Response for ECI Write-Off: B00003591

Clearance / Acceptance Instructions
---------------------------------------------------------------------
ECI Write-Off  : B00003591
Entry Number   : 86015949
Master Bill    : 081-00023494
Message No     : 2708

Message Status : (B07) MPI Biosecurity Cargo Report Notification - consignment status as specified

Customs Instructions
---------------------------------------------------------------------
'To be held pending further instructions by MPI',issuedDate:'Thursday, 3 May 2018 4:12:29 p.m.'

Job Responses
---------------------------------------------------------------------
Job Number: B00003591   House Bill: 
--- Clearance Status: HLD-Consignment Held ---
";

			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			bioMessage.EM_MessageText = B00003591_BIOResponse;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(bioMessage);
			AssertEquals(entryHeader, bioMessage.EM_LinkedObject);
			AssertEquals(mpiBioResponseExpected, bioMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIBio", "91", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "86015949", declaration.DeclarationNumber);
			AssertEquals("CH_MPIBioStatus", "HLD", entryHeader.CH_MPIBioStatus);
			AssertEquals("JE_EntryStatus should still reflect pending agency response - Customs message response still to process", LowValueConsignmentStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect both Bio and Customs status", LowValueConsignmentStatusList.Codes.PH, declaration.JE_TSWCombinedStatus);

			ZString nzcsResponseExpected = @"[ICR/CRE Accepted, Check Consignments for Status] Response for ECI Write-Off: B00003591

Clearance / Acceptance Instructions
---------------------------------------------------------------------
ECI Write-Off  : B00003591
Entry Number   : 86015949
Master Bill    : 081-00023494
Message No     : 2709

Message Status : (C06) Customs Cargo Report Notification - consignment status as specified

Job Responses
---------------------------------------------------------------------
Job Number: B00003591   House Bill: 
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;

			nzcsMessage.EM_MessageText = B00003591_NZCSResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcsMessage);
			AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
			AssertEquals(nzcsResponseExpected, nzcsMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from Customs, entryStatus components representing MPIBio & NZCS should now show customs cleared / BIO held.", "01", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "86015949", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "WOF", entryHeader.CH_NZCSStatus);
			AssertEquals("Delivery/Customs instructions:", "Response Status: WOF-Consignment Written Off/Cleared", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("JE_EntryStatus should reflect entry overall is still HELD", LowValueConsignmentStatusList.Codes.ConsignmentHeld, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect both agencies current responses", LowValueConsignmentStatusList.Codes.CH, declaration.JE_TSWCombinedStatus);
		}

		public void TestImportEntryStatusIsConsistentWithMultipleAgencyResponses()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			declaration.JE_DeclarationReference = "B00003787";
			declaration.JE_MasterBill = "08130492884";
			outgoingMessage.EM_ApplicationReference = "B00003787";

			ZString mpiFoodResponseExpected = @"[Ministry for Primary Industries (Food) - Inspection / Audit requirements] Response for Customs Import Declaration: B00003787

Inspection / Audit requirements
---------------------------------------------------------------------
Job Number     : B00003787
Master Bill    : 081-30492884
Entry Type     : Import (Normal)
Entry Number   : 44061970
Message No     : 3344

Message Status : (F05) MPI Food - Directions Given

Customs Instructions
---------------------------------------------------------------------
Lodgement has been received by MPI, please await further direction
";

			var mpifoodMessage = Factory.New<TSWMessage>();
			mpifoodMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			mpifoodMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
			mpifoodMessage.EM_MessageText = MPIFoodResponseB00003787;
			mpifoodMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(mpifoodMessage);
			AssertEquals(entryHeader, mpifoodMessage.EM_LinkedObject);
			AssertEquals(mpiFoodResponseExpected, mpifoodMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIFood, entryStatus component representing MPIFood should be 0 for cleared.", "991", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "44061970", declaration.DeclarationNumber);
			AssertEquals("CH_MPIFoodStatus", "F05", entryHeader.CH_MPIFoodStatus);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpifoodMessage.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, mpifoodMessage.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should reflect pending agency responses", FormalEntryStatusList.Codes.InspectionsAuditRequirements, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should show MPI Food has cleared", TSWEntryStatusList.Codes.PPI, declaration.JE_TSWCombinedStatus);

			ZString nzcsResponseExpected = @"[New Zealand Customs Service - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00003787

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number      : B00003787
Master Bill     : 081-30492884
Entry Type      : Import (Normal)
Entry Number    : 44061970
Message No      : 3345

Message Status  : (819) Delivery Order Herewith
                : method of Payment as specified.

Amount Returned : $264.62
Terms           : Client Deferred

Delivery Instructions
---------------------------------------------------------------------
10 LOOSE PACKAGE(S) OR ITEM(S)
";

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			nzcsMessage.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.NZCustoms;
			nzcsMessage.EM_MessageText = NZCSResponseB00003787;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcsMessage);
			AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
			AssertEquals(nzcsResponseExpected, nzcsMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from Customs, entryStatus components representing MPIFood & NZCS should now be updated.", "091", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "44061970", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "819", entryHeader.CH_NZCSStatus);
			AssertEquals("Delivery/Customs instructions:", "Lodgement has been received by MPI, please await further direction\r\n10 LOOSE PACKAGE(S) OR ITEM(S)", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("JE_EntryStatus should show a Delivery Order has been received from Customs, even though there is an Agency inspection requirement", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect agency responses and payment is pending", TSWEntryStatusList.Codes.CPI, declaration.JE_TSWCombinedStatus);

			ZString mpiBioResponseExpected = @"[Ministry for Primary Industries (Biosecurity) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00003787

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00003787
Master Bill    : 081-30492884
Entry Type     : Import (Normal)
Entry Number   : 44061970
Message No     : 3346

Message Status : (B04) MPI Biosecurity - Cleared

Delivery Instructions
---------------------------------------------------------------------
Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.
";

			var bioMessage = Factory.New<TSWMessage>();
			bioMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			bioMessage.EM_MessageSubType = "BIO";
			bioMessage.EM_MessageText = MPIBIOResponseB00003787;
			bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(bioMessage);
			AssertEquals(entryHeader, bioMessage.EM_LinkedObject);
			AssertEquals(mpiBioResponseExpected, bioMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIBio, entryStatus components representing MPIFood, NZCS & MPIBio should now all be 0 for cleared.", "001", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number should remain the same for all responses on the same declaration", "44061970", declaration.DeclarationNumber);
			AssertEquals("CH_MPIBioStatus", "B04", entryHeader.CH_MPIBioStatus);
			AssertEquals("JE_EntryStatus should show the Delivery Order having already been received from Customs", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect all agencies have cleared", TSWEntryStatusList.Codes.CCI, declaration.JE_TSWCombinedStatus);

			// now dummy up the subsequent cleared response from MPI Food
			ZString mpiFoodClearedExpected = @"[Ministry for Primary Industries (Food) - Clearance / Acceptance Instructions] Response for Customs Import Declaration: B00003787

Clearance / Acceptance Instructions
---------------------------------------------------------------------
Job Number     : B00003787
Master Bill    : 081-30492884
Entry Type     : Import (Normal)
Entry Number   : 44061970
Message No     : 3577

Message Status : (F04) MPI Food - Cleared
";

			var mpifoodCleared = Factory.New<TSWMessage>();
			mpifoodCleared.EM_MessageType = MessageTypeList.Codes.TWR;
			mpifoodCleared.EM_MessageSubType = TSWMessage.ResponseMessage.MessageSubTypes.MPIFood;
			mpifoodCleared.EM_MessageText = MPIFoodClearedB00003787;
			mpifoodCleared.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			processor.ProcessMessage(mpifoodCleared);
			AssertEquals(entryHeader, mpifoodCleared.EM_LinkedObject);
			AssertEquals(mpiFoodClearedExpected, mpifoodCleared.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from MPIFood, entryStatus component representing MPIFood should be 0 for cleared.", "000", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "44061970", declaration.DeclarationNumber);
			AssertEquals("CH_MPIFoodStatus", "F04", entryHeader.CH_MPIFoodStatus);
			AssertEquals("EM_MessageType should be set to TWR", MessageTypeList.Codes.TWR, mpifoodCleared.EM_MessageType);
			AssertEquals("EM_MessageSubType should not be set to same code as EM_MessageType", TSWMessage.ResponseMessage.MessageSubTypes.MPIFood, mpifoodCleared.EM_MessageSubType);
			AssertEquals("JE_EntryStatus should now reflect all agencies are cleared & Customs had sent DO", FormalEntryStatusList.Codes.DeliveryOrderReceived, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should show MPI Food has cleared", TSWEntryStatusList.Codes.CCC, declaration.JE_TSWCombinedStatus);

			declaration.SaveHandlingSaveExceptions();
			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCleared.Code));
			AssertEquals("Only 1 SCM event should be created", 1, logEntries.Length);
			AssertEquals("CLR Log Entry event has been created", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.CustomsCleared.Code));
		}

		public void TestWriteOffEntryStatusForExportCRE()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "B00003594";
			declaration.JE_MasterBill = "08100324925";
			declaration.JE_HouseBill = "G02388";
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			Declaration.ECIWriteOff.CusEntryHeader entryHeader = (Declaration.ECIWriteOff.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew(typeof(Declaration.ECIWriteOff.CusEntryHeader));

			outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.CRE;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_LinkUniqueID = entryHeader.PK;
			outgoingMessage.EM_ApplicationReference = "B00003594";

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);

			ZString nzcsResponseExpected = @"[ICR/CRE Accepted, Check Consignments for Status] Response for ECI Write-Off: B00003594

Clearance / Acceptance Instructions
---------------------------------------------------------------------
ECI Write-Off  : B00003594
Entry Number   : 88257194
Master Bill    : 081-00324925
Message No     : 2728

Message Status : (C06) Customs Cargo Report Notification - consignment status as specified

Job Responses
---------------------------------------------------------------------
Job Number: B00003594   House Bill: G02388
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";

			nzcsMessage.EM_MessageText = B00003594_NZCSResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcsMessage);
			AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
			AssertEquals(nzcsResponseExpected, nzcsMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from Customs only", "WOF", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "88257194", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "WOF", entryHeader.CH_NZCSStatus);
			AssertEquals("Delivery/Customs instructions:", "Response Status: WOF-Consignment Written Off/Cleared", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("JE_EntryStatus should reflect message status for Export CRE", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should show single agency response for Export CRE", LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff, declaration.JE_TSWCombinedStatus);
		}

		public void TestWriteOffStatusWhen1AgencyCleared1AgencyPending()
		{
			/*
			 *	Customs response Cleared, BIO response pending, entry status should NOT be WOF....
			 */
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_TransportMode = JobTransportModeList.Codes.Sea;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "B00003610";
			declaration.JE_MasterBill = "OB992308";
			declaration.JE_HouseBill = "L02398";
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			Declaration.ECIWriteOff.CusEntryHeader entryHeader = (Declaration.ECIWriteOff.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew(typeof(Declaration.ECIWriteOff.CusEntryHeader));

			outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.ICR;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_LinkUniqueID = entryHeader.PK;
			outgoingMessage.EM_ApplicationReference = "B00003610";

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);

			ZString nzcsResponseExpected = @"[ICR/CRE Accepted, Check Consignments for Status] Response for ECI Write-Off: B00003610

Clearance / Acceptance Instructions
---------------------------------------------------------------------
ECI Write-Off  : B00003610
Entry Number   : 91414377
Master Bill    : OB992308
Message No     : 2764

Message Status : (C06) Customs Cargo Report Notification - consignment status as specified

Job Responses
---------------------------------------------------------------------
Job Number: B00003610   House Bill: L02398
--- Clearance Status: WOF-Consignment Written Off/Cleared ---
";

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;

			nzcsMessage.EM_MessageText = B00003610_NZCSResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcsMessage);
			AssertEquals(entryHeader, nzcsMessage.EM_LinkedObject);
			AssertEquals(nzcsResponseExpected, nzcsMessage.EM_MessageInterpretation);
			AssertEquals("JE_TSWCombinedStatus should reflect the response from Customs, entryStatus components representing MPIBio & NZCS should now show customs cleared / BIO Pending.", "09", declaration.JE_TSWCombinedStatus);
			AssertEquals("Entry Number", "91414377", declaration.DeclarationNumber);
			AssertEquals("CH_NZCSStatus", "WOF", entryHeader.CH_NZCSStatus);
			AssertEquals("Delivery/Customs instructions:", "Response Status: WOF-Consignment Written Off/Cleared", entryHeader.CH_CustomsDeliveryInstructions);
			AssertEquals("JE_EntryStatus should reflect entry overall is still PENDING", LowValueConsignmentStatusList.Codes.AgencyResponsePending, declaration.JE_EntryStatus);
			AssertEquals("JE_TSWCombinedStatus should reflect both agencies current responses", LowValueConsignmentStatusList.Codes.CP, declaration.JE_TSWCombinedStatus);
		}

		public void TestExportWriteOffClearance()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "009908C");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.WriteOff;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_DeclarationReference = "B00003998";
			declaration.JE_MasterBill = "08600239282";
			declaration.JE_HouseBill = "G923878";
			declaration.JE_EntryStatus = LowValueConsignmentStatusList.Codes.SentToCustoms;

			Declaration.ECIWriteOff.CusEntryHeader entryHeader = (Declaration.ECIWriteOff.CusEntryHeader)declaration.ActiveEntryHeaders.AddNew(typeof(Declaration.ECIWriteOff.CusEntryHeader));

			outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.CRE;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_LinkUniqueID = entryHeader.PK;
			outgoingMessage.EM_ApplicationReference = "B00003998";

			var nzcsMessage = Factory.New<TSWMessage>();
			nzcsMessage.EM_MessageType = MessageTypeList.Codes.TWR;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			nzcsMessage.EM_MessageText = B00003998_NZCSResponse;
			nzcsMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			processor.ProcessMessage(nzcsMessage);

			declaration.SaveHandlingSaveExceptions();
			var logEntries = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.ExportCustomsCleared.Code));
			AssertEquals("Only 1 ECC event should be created", 1, logEntries.Length);
			AssertEquals("ECC (Export Clearance) Log Entry event should now have been created", true, logEntries.Any(le => le.SL_SE_NKEvent == Events.ExportCustomsCleared.Code));
		}

		public void TestCombinedStatusOnReplacementResponse()
		{
			AssertEquals("Pre-condition: JE_TSWCombinedStatus", ZString.Empty, declaration.JE_TSWCombinedStatus);
			outgoingMessage.EM_ApplicationReference = "B00003981";

			#region Original Responses

			string originalAck =
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
    <IssueDateTime formatCode=""204"">20180906122012</IssueDateTime>
    <FunctionalReferenceID>3927</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>37151566</ID>
        <FunctionalReferenceID>B00003981</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20180906122012</EffectiveDateTime>
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

			string originalFood =
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
    <IssueDateTime formatCode=""204"">20180906122017</IssueDateTime>
    <FunctionalReferenceID>3928</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Lodgement has been received by MPI, please await further direction</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>37151566</ID>
        <AcceptanceDateTime formatCode=""204"">20180906122017</AcceptanceDateTime>
        <FunctionalReferenceID>B00003981</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20180906122017</EffectiveDateTime>
      <NameCode>F05</NameCode>
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

			string originalBio =
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
    <IssueDateTime formatCode=""204"">20180906122025</IssueDateTime>
    <FunctionalReferenceID>3930</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>37151566</ID>
        <AcceptanceDateTime formatCode=""204"">20180906122025</AcceptanceDateTime>
        <FunctionalReferenceID>B00003981</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20180906122025</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20180906122025</ReleaseDateTime>
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

			string originalNZC =
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
    <IssueDateTime formatCode=""204"">20180906122023</IssueDateTime>
    <FunctionalReferenceID>3929</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>10 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>37151566</ID>
        <AcceptanceDateTime formatCode=""204"">20180906122023</AcceptanceDateTime>
        <FunctionalReferenceID>B00003981</FunctionalReferenceID>
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
            <TaxAssessedAmount currencyID=""NZD"">264.62</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180906122023</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20180906122023</ReleaseDateTime>
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

			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("2845", "FOB", "NZD", 1250m);
			decCreator.SetupImportInvoiceLine("0504000051L", "BEEF AND VEAL TRIPE", "AU", "AU", "N", 1250m);
			decCreator.AddHouseBillWithPackingDetails("G52882", 100, "PK");
			decCreator.MergeDeclaration();
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.STC;

			ProcessMessage(originalAck);
			ProcessMessage(originalFood);
			AssertEquals("JE_TSWCombinedStatus - MPI Food original response processed", TSWEntryStatusList.Codes.PPI, declaration.JE_TSWCombinedStatus);
			ProcessMessage(originalNZC);
			AssertEquals("JE_TSWCombinedStatus - NZCS original response processed", TSWEntryStatusList.Codes.CPI, declaration.JE_TSWCombinedStatus);
			ProcessMessage(originalBio);
			AssertEquals("JE_TSWCombinedStatus - MPI Biosecurity original response processed", TSWEntryStatusList.Codes.CCI, declaration.JE_TSWCombinedStatus);

			#region Replacement Responses

			string replacementAck =
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
    <IssueDateTime formatCode=""204"">20181016131247</IssueDateTime>
    <FunctionalReferenceID>4256</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>37151566</ID>
        <FunctionalReferenceID>B00003981</FunctionalReferenceID>
        <VersionID>2</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20181016131247</EffectiveDateTime>
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

			string replacementNZC =
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
    <IssueDateTime formatCode=""204"">20181016131258</IssueDateTime>
    <FunctionalReferenceID>4257</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>10 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>37151566</ID>
        <AcceptanceDateTime formatCode=""204"">20181016131258</AcceptanceDateTime>
        <FunctionalReferenceID>B00003981</FunctionalReferenceID>
        <VersionID>2</VersionID>
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
            <TaxAssessedAmount currencyID=""NZD"">264.62</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20181016131258</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20181016131258</ReleaseDateTime>
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

			declaration.JE_TSWCombinedStatus = TSWEntryStatusList.Codes.PCI;
			ProcessMessage(replacementAck);
			ProcessMessage(replacementNZC);
			AssertEquals("JE_TSWCombinedStatus - NZCS replacement msg response processed - other agencies retain current status, are not showing 'response pending'", TSWEntryStatusList.Codes.CCI, declaration.JE_TSWCombinedStatus);
		}

		public void TestConsolidatedDeclaration()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			using (Env.SetTemporaryUserContext(new UserContext(User.ServiceUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				CombineAssertions(() =>
				{
					var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
					outgoingMessage.EM_LinkedObject = consolidatedDeclaration;
					outgoingMessage.EM_ApplicationReference = "APPLICATION_REFERENCE";
					consolidatedDeclaration.JobDeclarations[0].JE_MasterBill = "123";
					consolidatedDeclaration.JobDeclarations[1].JE_MasterBill = "XYZ";
					consolidatedDeclaration.CRD_CustomsStatus = consolidatedDeclaration.JobDeclarations[0].JE_EntryStatus = consolidatedDeclaration.JobDeclarations[1].JE_EntryStatus = "";
					Factory.Save();
					var logger = new LoggingInformation();
					var processor = new MessageProcessorFactory(logger);
					var im1Message = Factory.New<TSWMessage>();
					im1Message.EM_MessageType = MessageTypeList.Codes.I10;
					im1Message.EM_MessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?>
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
					im1Message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					processor.ProcessMessage(im1Message);
					AssertEquals("First message linked", consolidatedDeclaration, im1Message.EM_LinkedObject);
					AssertContainsExactElementsInAnyOrder("Entry Number", new[] { "44885032", "44885032", "44885032" }, new[] { consolidatedDeclaration.LeadDeclaration.DeclarationNumber, consolidatedDeclaration.JobDeclarations[0].DeclarationNumber, consolidatedDeclaration.JobDeclarations[1].DeclarationNumber });
					AssertContainsExactElementsInAnyOrder("Message Status", new int[3].Select(_ => StatusListBase.Codes.Acknowledgement), new[] { consolidatedDeclaration.CRD_MessageStatus, consolidatedDeclaration.JobDeclarations[0].JE_MessageStatus, consolidatedDeclaration.JobDeclarations[1].JE_MessageStatus });
					var im1Message2 = Factory.New<TSWMessage>();
					im1Message2.EM_MessageType = MessageTypeList.Codes.I10;
					im1Message2.EM_MessageText = @"<?xml version=""1.0"" encoding=""UTF-8""?>
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
				<ID>TSW</ID>
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
					im1Message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					processor.ProcessMessage(im1Message2);
					AssertEquals("Second message linked", consolidatedDeclaration, im1Message.EM_LinkedObject);
					AssertContainsExactElementsInAnyOrder("JE_TSWCombinedStatus", new int[2].Select(_ => TSWEntryStatusList.Codes.CAN), new[] { (consolidatedDeclaration.JobDeclarations[0] as JobDeclaration).JE_TSWCombinedStatus, (consolidatedDeclaration.JobDeclarations[1] as JobDeclaration).JE_TSWCombinedStatus });
					AssertContainsExactElementsInAnyOrder("Entry Status", new int[3].Select(_ => FormalEntryStatusList.Codes.EntryCancelled), new[] { consolidatedDeclaration.CRD_CustomsStatus, consolidatedDeclaration.JobDeclarations[0].JE_EntryStatus, consolidatedDeclaration.JobDeclarations[1].JE_EntryStatus });
					AssertContains("Consolidated Job Number", consolidatedDeclaration.CRD_JobReferenceNumber, im1Message2.EM_MessageInterpretation);
					AssertContains("Master Bill 1", "123", im1Message2.EM_MessageInterpretation);
					AssertContains("Master Bill 1", "XYZ", im1Message2.EM_MessageInterpretation);
				});
			}
		}

		#region Implementation

		void ProcessMessage(string messageText)
		{
			var tswMessage = Factory.New<TSWMessage>();
			tswMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			tswMessage.EM_MessageText = messageText;
			tswMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(tswMessage);
		}

		void ProcessMessage(string messageText, ZDateTime messageCreateTime)
		{
			var tswMessage = Factory.New<TSWMessage>();
			tswMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			tswMessage.EM_MessageText = messageText;
			tswMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			tswMessage.EM_SystemCreateTimeUtc = messageCreateTime;
			entryHeader.Messages.Add(tswMessage);

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(tswMessage);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
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

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MasterBill = "081003498238";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;

			entryHeader = declaration.ActiveEntryHeaders.AddNew();

			outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_LinkUniqueID = entryHeader.PK;
		}

		#endregion

		#region Response Messages

		#region Rejection Response

		const string MPIFoodResOK =
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
    <IssueDateTime formatCode=""204"">20131029191816</IssueDateTime>
    <FunctionalReferenceID>471</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>9535406</ID>
        <AcceptanceDateTime formatCode=""204"">20131029191816</AcceptanceDateTime>
        <FunctionalReferenceID>B00001249</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131029191816</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20131029191816</ReleaseDateTime>
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

		const string NZCSRejection =
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
    <IssueDateTime formatCode=""204"">20131029200607</IssueDateTime>
    <FunctionalReferenceID>472</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>9535406</ID>
        <FunctionalReferenceID>B00001249</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <RejectionDateTime formatCode=""204"">20131029200608</RejectionDateTime>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>458</ValidationCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>40A</DocumentSectionCode>
        <TagID>135</TagID>
      </Pointer>
    </Error>
    <Error>
      <ValidationCode>245</ValidationCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>40A</DocumentSectionCode>
        <TagID>118</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131029200608</EffectiveDateTime>
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
</DocumentMetadata>
";

		const string OutgoingRejection =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns = ""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>IM</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns = ""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>I10</TypeCode>
  <FunctionalReferenceID>B00001825</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <TotalGrossMassMeasure unitCode=""KGM"">1</TotalGrossMassMeasure>
  <JurisdictionDateTime formatCode = ""102"">20150814</JurisdictionDateTime>
  <Submitter>
	<ID>00009908C</ID>
 </Submitter>
  <AdditionalInformation>
    <StatementCode>PDO</StatementCode>
    <StatementTypeCode>OIN</StatementTypeCode>
 </AdditionalInformation>
  <Agent>
    <ID>00009908C</ID>
    <RoleCode>CB</RoleCode>
 </Agent>
  <BorderTransportMeans>
    <Name>NZ56</Name>
    <TypeCode>4</TypeCode>
 </BorderTransportMeans>
  <Carrier>
    <Name>AIR NEW ZEALAND(NZ) LIMITED</Name>
 </Carrier>
  <CurrencyExchange>
    <RateNumeric>1.00</RateNumeric>
    <CurrencyTypeCode>NZD</CurrencyTypeCode>
 </CurrencyExchange>
  <Declarant>
    <ID>40006206E</ID>
    <Communication>
      <ID>Ian.Chen @wisetechglobal.com</ID>
      <TypeID>EM</TypeID>
   </Communication>
 </Declarant>
  <DutyTaxFee>
    <Payment>
      <MethodCode>B</MethodCode>
   </Payment>
 </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID = ""NZD"">454.65</TaxAssessedAmount>
	</Payment>
 </DutyTaxFee>
  <DutyTaxFee>
	<TypeCode>CUD</TypeCode>
	<Payment>
	  <TaxAssessedAmount currencyID= ""NZD"">0</TaxAssessedAmount>
	</Payment>
 </DutyTaxFee>
  <DutyTaxFee>
	<TypeCode>TOT</TypeCode>
	<Payment>
	  <TaxAssessedAmount currencyID= ""NZD"">454.65</TaxAssessedAmount>
	</Payment>
 </DutyTaxFee>
  <GoodsShipment>
	<ExportationCountryCode>AU</ExportationCountryCode>
	<TransactionNatureCode>10</TransactionNatureCode>
	<Consignment>
	  <GoodsLocation>
		<ID>7175H</ID>
     </GoodsLocation>
      <LoadingLocation>
        <ID>AUSYD</ID>
     </LoadingLocation>
      <TransportContractDocument>
        <ID>08135214034</ID>
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
     </TransportContractDocument>
      <TransportContractDocument>
        <ID>IAN123456</ID>
        <TypeCode>HWB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
       </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
       </Pointer>
     </TransportContractDocument>
      <UnloadingLocation>
        <ID>NZAKL</ID>
     </UnloadingLocation>
   </Consignment>
    <CustomsValuation>
      <FreightChargeAmount currencyID = ""NZD"">30</FreightChargeAmount>
	  <FreightChargeApportionmentCode>161</FreightChargeApportionmentCode>
	</CustomsValuation>
	<DeliveryDestination>
	  <Name>ADULATION BOOKS LTD (NZ CUSTOMS)</Name>
      <Address>
        <CityName>AUCKLAND</CityName>
        <CountryCode>NZ</CountryCode>
        <CountrySubDivisionName>AUK</CountrySubDivisionName>
        <Line>1 MAIN PLACE</Line>
        <PostcodeID>2000</PostcodeID>
     </Address>
   </DeliveryDestination>
    <GovernmentAgencyGoodsItem>
      <SequenceNumeric>1</SequenceNumeric>
      <CustomsValueAmount currencyID = ""NZD"">3000</CustomsValueAmount>
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
      <ApprovedEstablishmentPlace>
        <ID>25001</ID>
     </ApprovedEstablishmentPlace>
      <Commodity>
        <Description>ARTICLES OF PLASTICS &amp; OF OTHER MATERIALS OF 3901 TO 3914 N.E.C.IN CHPT 39</Description>
        <ValueAmount currencyID = ""NZD"">3000</ValueAmount>
		<Classification>
		  <ID>3926906969J</ID>
          <IdentificationTypeCode>HS</IdentificationTypeCode>
       </Classification>
        <DutyTaxFee>
          <DutyRegimeCode>NML</DutyRegimeCode>
       </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>GST</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID = ""NZD"">454.65</TaxAssessedAmount>
		 </Payment>
		</DutyTaxFee>
		<DutyTaxFee>
		  <TypeCode>CUD</TypeCode>
		  <Payment>
			<TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
         </Payment>
       </DutyTaxFee>
        <Source>
          <CountryCode>AU</CountryCode>
       </Source>
     </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode = ""KGM"">1</GrossMassMeasure>
		<NetNetWeightMeasure unitCode=""KGM"">1</NetNetWeightMeasure>
     </GoodsMeasure>
      <Origin>
        <CountryCode>AU</CountryCode>
     </Origin>
      <ValuationAdjustment>
        <AdditionCode>151</AdditionCode>
        <AmountAmount currencyID = ""NZD"">30</AmountAmount>
	 </ValuationAdjustment>
	  <ValuationAdjustment>
		<AdditionCode>150</AdditionCode>
		<AmountAmount currencyID=""NZD"">1</AmountAmount>
     </ValuationAdjustment>
   </GovernmentAgencyGoodsItem>
    <Invoice>
      <ID>12</ID>
      <ConditionCode>FOB</ConditionCode>
      <SequenceNumeric>1</SequenceNumeric>
   </Invoice>
    <Supplier>
      <ID>00710841Y</ID>
      <Contact>
        <Name>Unknown</Name>
        <Communication>
          <ID>brendon.paine @cargowise.com</ID>
          <TypeID>EM</TypeID>
       </Communication>
        <Communication>
          <ID>61288889999</ID>
          <TypeID>TE</TypeID>
       </Communication>
        <Communication>
          <ID>61266665557</ID>
          <TypeID>FX</TypeID>
       </Communication>
     </Contact>
   </Supplier>
 </GoodsShipment>
  <Importer>
    <ID>51352368J</ID>
    <Contact>
      <Name>Bill Smith</Name>
      <Communication>
        <ID>brendon.paine @cargowise.com</ID>
        <TypeID>EM</TypeID>
     </Communication>
      <Communication>
        <ID>64966667777</ID>
        <TypeID>TE</TypeID>
     </Communication>
      <Communication>
        <ID>64966667777</ID>
        <TypeID>FX</TypeID>
     </Communication>
   </Contact>
 </Importer>
  <Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>1</QuantityQuantity>
    <TypeCode>PK</TypeCode>
 </Packaging>
</Declaration>
</DocumentMetadata>";

		const string NZCS00000000Rejection =
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
  <Response>
    <IssueDateTime formatCode = ""204""></IssueDateTime>
	<FunctionalReferenceID/>
	<FunctionCode>48</FunctionCode>
	<OverallDeclaration>
	  <Declaration>
		<ID>00000000</ID>
		<FunctionalReferenceID>B00001825</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode = ""204"">
	  </EffectiveDateTime>
	  <NameCode>801</NameCode>
	  <Pointer >
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

		const string NZCS00000000Error =
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
  <Response>
	<IssueDateTime formatCode=""204"">20160811111957</IssueDateTime>
    <FunctionalReferenceID>5333</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>00000000</ID>
        <FunctionalReferenceID>B00002644</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>121</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>67A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>18B</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <DocumentSectionCode>R053</DocumentSectionCode>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20160811111957</EffectiveDateTime>
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
</DocumentMetadata>";

		const string CompletionRejection =
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
			<IssueDateTime formatCode=""204"">20140128181315</IssueDateTime>    
		<FunctionalReferenceID>723</FunctionalReferenceID>    
		<FunctionCode>48</FunctionCode>    
		<OverallDeclaration>      
		<Declaration>        
		<ID>00000000</ID>        
		<FunctionalReferenceID>1001</FunctionalReferenceID>        
		<RejectionDateTime formatCode=""204"">20140128181315</RejectionDateTime>        
		<Submitter>          
		<ID>00009908C</ID>        
		</Submitter>        
		<ResponsibleGovernmentAgency>          
		<ID>TSW</ID>        
		</ResponsibleGovernmentAgency>      
		</Declaration>    
		</OverallDeclaration>    
		<Error>      
		<ValidationCode>1075</ValidationCode>
		<Pointer>
		<SequenceNumeric>1</SequenceNumeric>
		<DocumentSectionCode>42A</DocumentSectionCode>
		</Pointer>
		<Pointer>
		<SequenceNumeric>1</SequenceNumeric>
		<DocumentSectionCode>40A</DocumentSectionCode>
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
		<Error> 
		<ValidationCode>1092</ValidationCode>
		<Pointer>
		<SequenceNumeric>1</SequenceNumeric> 
		<DocumentSectionCode>42A</DocumentSectionCode>
		</Pointer>
		<Pointer>
		<SequenceNumeric>1</SequenceNumeric>
		<DocumentSectionCode>67A</DocumentSectionCode>
		</Pointer>
		<Pointer>
		<SequenceNumeric>1</SequenceNumeric>
		<DocumentSectionCode>68A</DocumentSectionCode>
		</Pointer>
		</Error>
		<Error>  
		<ValidationCode>679</ValidationCode> 
		<Pointer>  
		<SequenceNumeric>1</SequenceNumeric>
		<DocumentSectionCode>42A</DocumentSectionCode>
		</Pointer>
		<Pointer>
		<SequenceNumeric>1</SequenceNumeric>
		<DocumentSectionCode>67A</DocumentSectionCode>
		</Pointer> 
		<Pointer> 
		<SequenceNumeric>1</SequenceNumeric>
		<DocumentSectionCode>68A</DocumentSectionCode> 
		</Pointer> 
		</Error>  
		<Error>     
		<ValidationCode>519</ValidationCode> 
		<Pointer>  
		<SequenceNumeric>1</SequenceNumeric> 
		<DocumentSectionCode>42A</DocumentSectionCode> 
		</Pointer>
		<Pointer>
		<SequenceNumeric>1</SequenceNumeric> 
		<DocumentSectionCode>67A</DocumentSectionCode> 
		</Pointer> 
		<Pointer>  
		<SequenceNumeric>1</SequenceNumeric>
		<DocumentSectionCode>78A</DocumentSectionCode>
		<TagID>D016</TagID>
		</Pointer>
		</Error> 
		<Error>  
		<ValidationCode>120</ValidationCode>
		<Pointer>
		<SequenceNumeric>1</SequenceNumeric>
		<DocumentSectionCode>42A</DocumentSectionCode>
		</Pointer>
		<Pointer>
		<SequenceNumeric>1</SequenceNumeric> 
		<DocumentSectionCode>67A</DocumentSectionCode>
		</Pointer>
		<Pointer>
		<SequenceNumeric>1</SequenceNumeric>
		<DocumentSectionCode>18B</DocumentSectionCode>
		</Pointer>
		</Error>
		<Error> 
		<ValidationCode>4027</ValidationCode>
		<Pointer>
		<SequenceNumeric>1</SequenceNumeric> 
		<DocumentSectionCode>42A</DocumentSectionCode>
		</Pointer>
		<Pointer>
		<SequenceNumeric>1</SequenceNumeric>
		<DocumentSectionCode>57B</DocumentSectionCode>
		<TagID>R123</TagID>
		</Pointer>
		</Error>
		<Status> 
		<EffectiveDateTime formatCode=""204"">20140128181315</EffectiveDateTime>
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
		</Response></DocumentMetadata>
";

		const string NZCSRejectionWithError =
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
    <IssueDateTime formatCode=""204"">20131029200607</IssueDateTime>
    <FunctionalReferenceID>472</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>9535406</ID>
        <FunctionalReferenceID>B00001249</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <RejectionDateTime formatCode=""204"">20131029200608</RejectionDateTime>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>458</ValidationCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>68A</DocumentSectionCode>
        <TagID>135</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131029200608</EffectiveDateTime>
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
</DocumentMetadata>
";
		#endregion

		#region IPIB00003768

		const string B00003768MPIFoodResponse =
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
    <IssueDateTime formatCode=""204"">20180705100158</IssueDateTime>
    <FunctionalReferenceID>3268</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>79323392</ID>
        <AcceptanceDateTime formatCode=""204"">20180705100158</AcceptanceDateTime>
        <FunctionalReferenceID>B00003768</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20180705100158</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20180705100158</ReleaseDateTime>
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

		#region Cleared Response

		const string ResponsePlaceHolderWithNoError =
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
        <FunctionalReferenceID>B00001249</FunctionalReferenceID>
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
          <ID>{0}</ID>
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
            <TaxAssessedAmount>441.57</TaxAssessedAmount>
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

		#endregion

		#region Cancellation Responses

		const string CancelAcknowledgeResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
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
    <IssueDateTime formatCode=""204"">20131126191640</IssueDateTime>
    <FunctionalReferenceID>579</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>8915911</ID>
        <FunctionalReferenceID>B00001280</FunctionalReferenceID>
        <VersionID>2</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131126191640</EffectiveDateTime>
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

		const string CancelInspectionRequiredResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
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
    <IssueDateTime formatCode=""204"">20131126191652</IssueDateTime>
    <FunctionalReferenceID>580</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>8915911</ID>
        <AcceptanceDateTime formatCode=""204"">20131126191652</AcceptanceDateTime>
        <FunctionalReferenceID>B00001280</FunctionalReferenceID>
        <VersionID>2</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131126191652</EffectiveDateTime>
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

		const string CancelAcceptedResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
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
    <IssueDateTime formatCode=""204"">20131128125315</IssueDateTime>
    <FunctionalReferenceID>581</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>8915911</ID>
        <AcceptanceDateTime formatCode=""204"">20131128125315</AcceptanceDateTime>
        <FunctionalReferenceID>B00001280</FunctionalReferenceID>
        <VersionID>3</VersionID>
        <CancellationDateTime formatCode=""204"">20131128125315</CancellationDateTime>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131128125315</EffectiveDateTime>
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

		#endregion

		#region Cleared Response Out of Sequence

		const string MPIFR =
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
    <IssueDateTime formatCode=""204"">20131217155316</IssueDateTime>
    <FunctionalReferenceID>629</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>47248628</ID>
        <AcceptanceDateTime formatCode=""204"">20131217155316</AcceptanceDateTime>
        <FunctionalReferenceID>B00001296</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131217155316</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20131217155316</ReleaseDateTime>
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

		const string NZCSAcknowledgment =
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
    <IssueDateTime formatCode=""204"">20131217155337</IssueDateTime>
    <FunctionalReferenceID>630</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>47248628</ID>
        <FunctionalReferenceID>B00001296</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20131217155337</EffectiveDateTime>
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

		const string MPIBR =
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
    <IssueDateTime formatCode=""204"">20131217155357</IssueDateTime>
    <FunctionalReferenceID>638</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>47248628</ID>
        <AcceptanceDateTime formatCode=""204"">20131217155357</AcceptanceDateTime>
        <FunctionalReferenceID>B00001296</FunctionalReferenceID>
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
            <TaxAssessedAmount>450.39</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131217155357</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20131217155406</ReleaseDateTime>
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

		const string NZCSR =
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
    <IssueDateTime formatCode=""204"">20131217155357</IssueDateTime>
    <FunctionalReferenceID>636</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>47248628</ID>
        <AcceptanceDateTime formatCode=""204"">20131217155357</AcceptanceDateTime>
        <FunctionalReferenceID>B00001296</FunctionalReferenceID>
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
            <TaxAssessedAmount>450.39</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20131217155357</EffectiveDateTime>
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
		#region Completion Messages

		const string originalSightMPIResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20140130182202</IssueDateTime>
    <FunctionalReferenceID>737</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>89411896</ID>
        <AcceptanceDateTime formatCode=""204"">20140130182202</AcceptanceDateTime>
        <FunctionalReferenceID>B00001324</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20140130182202</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20140130182202</ReleaseDateTime>
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

		const string originalSightNZCResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20140130182227</IssueDateTime>
    <FunctionalReferenceID>738</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>89411896</ID>
        <AcceptanceDateTime formatCode=""204"">20140130182227</AcceptanceDateTime>
        <FunctionalReferenceID>B00001324</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20140130182227</EffectiveDateTime>
      <NameCode>806</NameCode>
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

		const string originalSightBioResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20140130182227</IssueDateTime>
    <FunctionalReferenceID>739</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>89411896</ID>
        <AcceptanceDateTime formatCode=""204"">20140130182227</AcceptanceDateTime>
        <FunctionalReferenceID>B00001324</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20140130182227</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20140130182231</ReleaseDateTime>
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
		const string completionAcknowledgement = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  <WCODataModelVersion>3.2</WCODataModelVersion>  <WCODocumentName>RES</WCODocumentName>  <CountryCode>NZ</CountryCode>  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>  <CommunicationMetaData>    <Recipient>      <ID>51358596K</ID>      <RoleCode>TB</RoleCode>    </Recipient>  </CommunicationMetaData>  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">    <IssueDateTime formatCode=""204"">20140130184959</IssueDateTime>    <FunctionalReferenceID>745</FunctionalReferenceID>    <FunctionCode>12</FunctionCode>    <OverallDeclaration>      <Declaration>        <ID>95929208</ID>        <FunctionalReferenceID>51938</FunctionalReferenceID>        <VersionID>1</VersionID>        <Submitter>          <ID>00009908C</ID>        </Submitter>        <ResponsibleGovernmentAgency>          <ID>TSW</ID>        </ResponsibleGovernmentAgency>      </Declaration>    </OverallDeclaration>    <Status>      <EffectiveDateTime formatCode=""204"">20140130184959</EffectiveDateTime>      <NameCode>ACK</NameCode>      <Pointer>        <DocumentSectionCode>07B</DocumentSectionCode>      </Pointer>      <Pointer>        <DocumentSectionCode>42A</DocumentSectionCode>      </Pointer>      <Pointer>        <SequenceNumeric>1</SequenceNumeric>        <DocumentSectionCode>08B</DocumentSectionCode>        <TagID>G007</TagID>      </Pointer>    </Status>  </Response></DocumentMetadata>";
		const string completionMPIResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  <WCODataModelVersion>3.2</WCODataModelVersion>  <WCODocumentName>RES</WCODocumentName>  <CountryCode>NZ</CountryCode>  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>  <CommunicationMetaData>    <Recipient>      <ID>51358596K</ID>      <RoleCode>TB</RoleCode>    </Recipient>  </CommunicationMetaData>  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">    <IssueDateTime formatCode=""204"">20140130184953</IssueDateTime>    <FunctionalReferenceID>746</FunctionalReferenceID>    <FunctionCode>24</FunctionCode>    <OverallDeclaration>      <Declaration>        <ID>95929208</ID>        <AcceptanceDateTime formatCode=""204"">20140130184953</AcceptanceDateTime>        <FunctionalReferenceID>51938</FunctionalReferenceID>        <Submitter>          <ID>00009908C</ID>        </Submitter>        <ResponsibleGovernmentAgency>          <ID>MPIFOOD</ID>        </ResponsibleGovernmentAgency>      </Declaration>    </OverallDeclaration>    <Status>      <EffectiveDateTime formatCode=""204"">20140130184953</EffectiveDateTime>      <NameCode>F04</NameCode>      <ReleaseDateTime formatCode=""204"">20140130184953</ReleaseDateTime>      <Pointer>        <DocumentSectionCode>07B</DocumentSectionCode>      </Pointer>      <Pointer>        <DocumentSectionCode>42A</DocumentSectionCode>      </Pointer>      <Pointer>        <SequenceNumeric>1</SequenceNumeric>        <DocumentSectionCode>08B</DocumentSectionCode>        <TagID>G007</TagID>      </Pointer>    </Status>  </Response></DocumentMetadata>";
		const string completionNZCSResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  <WCODataModelVersion>3.2</WCODataModelVersion>  <WCODocumentName>RES</WCODocumentName>  <CountryCode>NZ</CountryCode>  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>  <CommunicationMetaData>    <Recipient>      <ID>51358596K</ID>      <RoleCode>TB</RoleCode>    </Recipient>  </CommunicationMetaData>  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">    <IssueDateTime formatCode=""204"">20140130185020</IssueDateTime>    <FunctionalReferenceID>747</FunctionalReferenceID>    <FunctionCode>24</FunctionCode>    <OverallDeclaration>      <Declaration>        <ID>95929208</ID>        <AcceptanceDateTime formatCode=""204"">20140130185020</AcceptanceDateTime>        <FunctionalReferenceID>51938</FunctionalReferenceID>        <VersionID>1</VersionID>        <Submitter>          <ID>00009908C</ID>        </Submitter>        <DutyTaxFee>          <Payment>            <MethodCode>C</MethodCode>          </Payment>        </DutyTaxFee>        <DutyTaxFee>          <TypeCode>TOT</TypeCode>          <Payment>            <TaxAssessedAmount>593.39</TaxAssessedAmount>          </Payment>        </DutyTaxFee>        <ResponsibleGovernmentAgency>          <ID>NZCS</ID>        </ResponsibleGovernmentAgency>      </Declaration>    </OverallDeclaration>    <Status>      <EffectiveDateTime formatCode=""204"">20140130185020</EffectiveDateTime>      <NameCode>822</NameCode>      <Pointer>        <DocumentSectionCode>07B</DocumentSectionCode>      </Pointer>      <Pointer>        <DocumentSectionCode>42A</DocumentSectionCode>      </Pointer>      <Pointer>        <SequenceNumeric>1</SequenceNumeric>        <DocumentSectionCode>08B</DocumentSectionCode>        <TagID>G007</TagID>      </Pointer>    </Status>  </Response></DocumentMetadata>";
		const string completionBioResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  <WCODataModelVersion>3.2</WCODataModelVersion>  <WCODocumentName>RES</WCODocumentName>  <CountryCode>NZ</CountryCode>  <AgencyAssignedCustomizedDocumentName>RESIM1</AgencyAssignedCustomizedDocumentName>  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>  <CommunicationMetaData>    <Recipient>      <ID>51358596K</ID>      <RoleCode>TB</RoleCode>    </Recipient>  </CommunicationMetaData>  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">    <IssueDateTime formatCode=""204"">20140130185020</IssueDateTime>    <FunctionalReferenceID>749</FunctionalReferenceID>    <FunctionCode>24</FunctionCode>    <AdditionalInformation>      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>      <StatementTypeCode>DIN</StatementTypeCode>    </AdditionalInformation>    <OverallDeclaration>      <Declaration>        <ID>95929208</ID>        <AcceptanceDateTime formatCode=""204"">20140130185020</AcceptanceDateTime>        <FunctionalReferenceID>51938</FunctionalReferenceID>        <VersionID>1</VersionID>        <Submitter>          <ID>00009908C</ID>        </Submitter>        <DutyTaxFee>          <Payment>            <MethodCode>C</MethodCode>          </Payment>        </DutyTaxFee>        <DutyTaxFee>          <TypeCode>TOT</TypeCode>          <Payment>            <TaxAssessedAmount>593.39</TaxAssessedAmount>          </Payment>        </DutyTaxFee>        <ResponsibleGovernmentAgency>          <ID>MPIBIO</ID>        </ResponsibleGovernmentAgency>      </Declaration>    </OverallDeclaration>    <Status>      <EffectiveDateTime formatCode=""204"">20140130185020</EffectiveDateTime>      <NameCode>B04</NameCode>      <ReleaseDateTime formatCode=""204"">20140130185024</ReleaseDateTime>      <Pointer>        <DocumentSectionCode>07B</DocumentSectionCode>      </Pointer>      <Pointer>        <DocumentSectionCode>42A</DocumentSectionCode>      </Pointer>      <Pointer>        <SequenceNumeric>1</SequenceNumeric>        <DocumentSectionCode>08B</DocumentSectionCode>        <TagID>G007</TagID>      </Pointer>    </Status>  </Response></DocumentMetadata>";

		#endregion

		#region Export Messages

		const string ExportClearedResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESEX1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20140214160646</IssueDateTime>
    <FunctionalReferenceID>821</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>CDO</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_A3610ED2-C676-45D5-BD2E-892AFE400C4F"" filename=""EX1_Delivery_Order-83093658-2014-02-14-160656248.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <AdditionalInformation>
      <StatementDescription>22 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>83093658</ID>
        <AcceptanceDateTime formatCode=""204"">20140214160646</AcceptanceDateTime>
        <FunctionalReferenceID>B00001338</FunctionalReferenceID>
        <VersionID>5</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>C</MethodCode>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20140214160646</EffectiveDateTime>
      <NameCode>809</NameCode>
      <ReleaseDateTime formatCode=""204"">20140214160646</ReleaseDateTime>
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

		const string EX1ACK = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESEX1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20141217134448</IssueDateTime>
    <FunctionalReferenceID>1986</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>64528325</ID>
        <FunctionalReferenceID>B00001601</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20141217134448</EffectiveDateTime>
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

		const string EX1CLR = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESEX1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20141217134459</IssueDateTime>
    <FunctionalReferenceID>1987</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>CDO</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_A04E4278-375C-4C78-8DF1-BA072E655EB4"" filename=""EX1_Delivery_Order-64528325-2014-12-17-134508740.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <AdditionalInformation>
      <StatementDescription>20 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>64528325</ID>
        <AcceptanceDateTime formatCode=""204"">20141217134459</AcceptanceDateTime>
        <FunctionalReferenceID>B00001601</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>C</MethodCode>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20141217134459</EffectiveDateTime>
      <NameCode>809</NameCode>
      <ReleaseDateTime formatCode=""204"">20141217134459</ReleaseDateTime>
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

		const string ExportDeliveryOrderResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESEX1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20150828161959</IssueDateTime>
    <FunctionalReferenceID>2875</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>CDO</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_D3C5DF6C-00B9-4D06-8B38-5F7129B6294A"" filename=""EX1_Delivery_Order-8410144-2015-08-28-162005969.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <AdditionalInformation>
      <StatementDescription>1 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>8410144</ID>
        <AcceptanceDateTime formatCode=""204"">20150828161959</AcceptanceDateTime>
        <FunctionalReferenceID>B00001854</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>D</MethodCode>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20150828161959</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20150828161959</ReleaseDateTime>
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

		const string ZeroTOTResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" 
    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
        <IssueDateTime formatCode=""204"">20190423193227</IssueDateTime>
        <FunctionalReferenceID>5728</FunctionalReferenceID>
        <FunctionCode>23</FunctionCode>
        <OverallDeclaration>
            <Declaration>
                <ID>34589090</ID>
                <AcceptanceDateTime formatCode=""204"">20190423193227</AcceptanceDateTime>
                <FunctionalReferenceID>B00165917</FunctionalReferenceID>
                <VersionID>2</VersionID>
                <Submitter>
                    <ID>51358596K</ID>
                </Submitter>
                <DutyTaxFee>
                    <Payment>
                        <MethodCode>B</MethodCode>
                    </Payment>
                </DutyTaxFee>
                <DutyTaxFee>
                    <TypeCode>TOT</TypeCode>
                    <Payment>
                        <TaxAssessedAmount currencyID=""NZD"">0</TaxAssessedAmount>
                    </Payment>
                </DutyTaxFee>
                <ResponsibleGovernmentAgency>
                    <ID>NZCS</ID>
                </ResponsibleGovernmentAgency>
            </Declaration>
        </OverallDeclaration>
        <Status>
            <EffectiveDateTime formatCode=""204"">20190423193227</EffectiveDateTime>
            <NameCode>802</NameCode>
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

		const string NonZeroTOTResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" 
    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
        <IssueDateTime formatCode=""204"">20190423193227</IssueDateTime>
        <FunctionalReferenceID>5728</FunctionalReferenceID>
        <FunctionCode>23</FunctionCode>
        <OverallDeclaration>
            <Declaration>
                <ID>34589090</ID>
                <AcceptanceDateTime formatCode=""204"">20190423193227</AcceptanceDateTime>
                <FunctionalReferenceID>B00165917</FunctionalReferenceID>
                <VersionID>2</VersionID>
                <Submitter>
                    <ID>51358596K</ID>
                </Submitter>
                <DutyTaxFee>
		          <Payment>
		            <MethodCode>B</MethodCode>
		          </Payment>
		        </DutyTaxFee>
                <ResponsibleGovernmentAgency>
                    <ID>NZCS</ID>
                </ResponsibleGovernmentAgency>
            </Declaration>
        </OverallDeclaration>
        <Status>
            <EffectiveDateTime formatCode=""204"">20190423193227</EffectiveDateTime>
            <NameCode>802</NameCode>
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

		const string EX1CompletionCleared = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESEX1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20181031162303</IssueDateTime>
    <FunctionalReferenceID>1987</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>20 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>84994890</ID>
        <AcceptanceDateTime formatCode=""204"">20181031162303</AcceptanceDateTime>
        <FunctionalReferenceID>B00004056C</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>C</MethodCode>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20181031162303</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20181031162303</ReleaseDateTime>
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

		const string EX1CompletionInformation = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESEX1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20181031162304</IssueDateTime>
    <FunctionalReferenceID>1732</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>84994890</ID>
        <AcceptanceDateTime formatCode=""204"">20181031162304</AcceptanceDateTime>
        <FunctionalReferenceID>B00004056C</FunctionalReferenceID>
        <TotalGrossMassMeasure unitCode=""KGM"">1</TotalGrossMassMeasure>
        <VersionID>1</VersionID>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <Agent>
          <Name>CargoWise 2</Name>
        </Agent>
        <BorderTransportMeans>
          <Name>QF11</Name>
          <TypeCode>4</TypeCode>
        </BorderTransportMeans>
        <Carrier>
          <Name>AIR NEW ZEALAND (NZ) LIMITED</Name>
        </Carrier>
        <Exporter>
          <Name>Importer for ECT</Name>
        </Exporter>
        <GoodsShipment>
          <ExitDateTime formatCode=""102"">20181031</ExitDateTime>
          <Consignment>
            <GoodsLocation>
              <Name>ECT - CCA Wgtn</Name>
            </GoodsLocation>
            <TransportContractDocument>
              <ID>08100234920</ID>
              <TypeCode>MB</TypeCode>
            </TransportContractDocument>
            <TransportContractDocument>
              <ID>G8239</ID>
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
        <LoadingLocation>
          <ID>NZAKL</ID>
        </LoadingLocation>
        <Packaging>
          <SequenceNumeric>1</SequenceNumeric>
          <QuantityQuantity>1</QuantityQuantity>
          <TypeCode>PK</TypeCode>
        </Packaging>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20181031162304</EffectiveDateTime>
      <NameCode>816</NameCode>
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

		#region IPI Message Response

		const string MPIFoodResponseForIPI =
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
        <FunctionalReferenceID>B00001252I</FunctionalReferenceID>
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

		const string MPIBIOResponseForIPI =
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
        <FunctionalReferenceID>B00001252I</FunctionalReferenceID>
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

		const string IPIB00003778TSWMsg =
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
    <IssueDateTime formatCode=""204"">20180706184219</IssueDateTime>
    <FunctionalReferenceID>3301</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>00000000</ID>
        <FunctionalReferenceID>B00003778</FunctionalReferenceID>
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
    <Status>
      <EffectiveDateTime formatCode=""204"">20180706184219</EffectiveDateTime>
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

		const string IPIAcknowledgement =
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
    <IssueDateTime formatCode=""204"">20140812140154</IssueDateTime>
    <FunctionalReferenceID>1508</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>99809018</ID>
        <FunctionalReferenceID>B00001482</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20140812140154</EffectiveDateTime>
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

		const string IPIClearance =
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
    <IssueDateTime formatCode=""204"">20140812140144</IssueDateTime>
    <FunctionalReferenceID>1509</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>99809018</ID>
        <AcceptanceDateTime formatCode=""204"">20140812140144</AcceptanceDateTime>
        <FunctionalReferenceID>B00001482</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20140812140144</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20140812140144</ReleaseDateTime>
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

		const string IPIB00001496Clr =
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
    <IssueDateTime formatCode=""204"">20140821164122</IssueDateTime>
    <FunctionalReferenceID>1548</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>37470892</ID>
        <AcceptanceDateTime formatCode=""204"">20140821164122</AcceptanceDateTime>
        <FunctionalReferenceID>B00001496</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20140821164122</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20140821164122</ReleaseDateTime>
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

		const string IPIB00001496Ack =
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

		const string B00001575Clr =
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
    <IssueDateTime formatCode=""204"">20141120152941</IssueDateTime>
    <FunctionalReferenceID>1865</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>48380311</ID>
        <AcceptanceDateTime formatCode=""204"">20141120152941</AcceptanceDateTime>
        <FunctionalReferenceID>B00001575</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20141120152941</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20141120152941</ReleaseDateTime>
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

		const string B00001575Dup =
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
    <IssueDateTime formatCode=""204"">20141121102930</IssueDateTime>
    <FunctionalReferenceID>1865</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>00000000</ID>
        <FunctionalReferenceID>B00001575</FunctionalReferenceID>
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

		const string B00001575Ack =
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
    <IssueDateTime formatCode=""204"">20141120152950</IssueDateTime>
    <FunctionalReferenceID>1866</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>48380311</ID>
        <FunctionalReferenceID>B00001575</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20141120152950</EffectiveDateTime>
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

		#region B00001980 Response

		const string MPIFoodResponseB00001980 =
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
    <IssueDateTime formatCode=""204"">20150925161022</IssueDateTime>
    <FunctionalReferenceID>3157</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>73218058</ID>
        <AcceptanceDateTime formatCode=""204"">20150925161022</AcceptanceDateTime>
        <FunctionalReferenceID>B00001980</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20150925161022</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20150925161022</ReleaseDateTime>
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

		const string MPIBIOResponseB00001980 =
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
    <FunctionalReferenceID>3159</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>73218058</ID>
        <AcceptanceDateTime formatCode=""204"">20150925161034</AcceptanceDateTime>
        <FunctionalReferenceID>B00001980</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20150925161034</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20150925161037</ReleaseDateTime>
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

		const string NZCSResponseB00001980 =
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
        <FunctionalReferenceID>B00001980</FunctionalReferenceID>
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

		#endregion

		#region Cleared IPI Response

		const string MPIFoodResponseB00002656 =
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"" >
	<IssueDateTime formatCode=""204"">20160817142131</IssueDateTime>
    <FunctionalReferenceID>5375</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>56734084</ID>
        <AcceptanceDateTime formatCode=""204"">20160817142131</AcceptanceDateTime>
		<FunctionalReferenceID>B00002656</FunctionalReferenceID>
		<VersionID>1</VersionID>
		<Submitter>
		  <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20160817142131</EffectiveDateTime>
	  <NameCode>F04</NameCode>
	  <ReleaseDateTime formatCode=""204"">20160817142131</ReleaseDateTime>
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

		const string MPIBIOResponseB00002656 =
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
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"" >
	<IssueDateTime formatCode=""204"">20160817142143</IssueDateTime>
    <FunctionalReferenceID>5377</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>56734084</ID>
        <AcceptanceDateTime formatCode=""204"">20160817142143</AcceptanceDateTime>
		<FunctionalReferenceID>B00002656</FunctionalReferenceID>
		<VersionID>1</VersionID>
		<Submitter>
		  <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20160817142143</EffectiveDateTime>
	  <NameCode>B04</NameCode>
	  <ReleaseDateTime formatCode=""204"">20160817142146</ReleaseDateTime>
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

		const string NZCSResponseB00002656 =
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
	<IssueDateTime formatCode=""204"">20160817142143</IssueDateTime>
    <FunctionalReferenceID>5376</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>56734084</ID>
        <AcceptanceDateTime formatCode=""204"">20160817142143</AcceptanceDateTime>
		<FunctionalReferenceID>B00002656</FunctionalReferenceID>
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
            <TaxAssessedAmount>1425.12</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20160817142143</EffectiveDateTime >
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

		const string MPIIPIResponseB00002656 =
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
	<IssueDateTime formatCode=""204"">20160817160033</IssueDateTime>
    <FunctionalReferenceID>5378</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>80843452</ID>
        <AcceptanceDateTime formatCode=""204"">20160817160033</AcceptanceDateTime>
		<FunctionalReferenceID>B00002656I</FunctionalReferenceID>
		<VersionID>1</VersionID>
		<Submitter>
		  <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIFOOD</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20160817160033</EffectiveDateTime>
	  <NameCode>F04</NameCode>
	  <ReleaseDateTime formatCode=""204"">20160817160033</ReleaseDateTime>
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

		const string MPIIPIBioResponseB00002656 =
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
	<IssueDateTime formatCode=""204"">20160817160246</IssueDateTime>
    <FunctionalReferenceID>5380</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Lodgement has been received by MPI, please await further direction</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>80843452</ID>
        <FunctionalReferenceID>B00002656I</FunctionalReferenceID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20160817160246</EffectiveDateTime>
	  <NameCode>B05</NameCode>
	</Status>
  </Response>
</DocumentMetadata>";

		#endregion

		#region B00003591
		const string B00003591_BIOResponse =
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
    <IssueDateTime formatCode=""204"">20180503161232</IssueDateTime>
    <FunctionalReferenceID>2708</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>86015949</ID>
        <AcceptanceDateTime formatCode=""204"">20180503161232</AcceptanceDateTime>
        <FunctionalReferenceID>B00003591</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">HLD</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AdditionalInformation>
            <StatementDescription><![CDATA[seq:1,instructions:'To be held pending further instructions by MPI',issuedDate:'Thursday, 3 May 2018 4:12:29 p.m.']]></StatementDescription>
            <StatementTypeCode>ICN</StatementTypeCode>
          </AdditionalInformation>
          <AssociatedTransportDocument>
            <ID>08100023494</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>U20492438</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>MPIBIO</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180503161232</EffectiveDateTime>
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

		const string B00003591_NZCSResponse =
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
    <IssueDateTime formatCode=""204"">20180503161236</IssueDateTime>
    <FunctionalReferenceID>2709</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>86015949</ID>
        <AcceptanceDateTime formatCode=""204"">20180503161236</AcceptanceDateTime>
        <FunctionalReferenceID>B00003591</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>08100023494</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>U20492438</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180503161236</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20180503161236</ReleaseDateTime>
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

		#region B00003594

		const string B00003594_NZCSResponse =
 @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20180503180742</IssueDateTime>
    <FunctionalReferenceID>2728</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>88257194</ID>
        <AcceptanceDateTime formatCode=""204"">20180503180743</AcceptanceDateTime>
        <FunctionalReferenceID>B00003594</FunctionalReferenceID>
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
            <ID>G02388</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180503180742</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20180503180742</ReleaseDateTime>
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

		#region B00003610

		const string B00003610_NZCSResponse =
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
    <IssueDateTime formatCode=""204"">20180508175150</IssueDateTime>
    <FunctionalReferenceID>2764</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>91414377</ID>
        <AcceptanceDateTime formatCode=""204"">20180508175150</AcceptanceDateTime>
        <FunctionalReferenceID>B00003610</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>51358596K</ID>
        </Submitter>
        <Consignment>
          <GoodsStatusCode StatusType=""CLEARANCE"">WOF</GoodsStatusCode>
          <SequenceNumeric>1</SequenceNumeric>
          <AssociatedTransportDocument>
            <ID>OB992308</ID>
            <TypeCode>MB</TypeCode>
          </AssociatedTransportDocument>
          <TransportContractDocument>
            <ID>L02398</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180508175150</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20180508175150</ReleaseDateTime>
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

		#region B00003787 Response

		const string MPIFoodResponseB00003787 =
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
    <IssueDateTime formatCode=""204"">20180710163634</IssueDateTime>
    <FunctionalReferenceID>3344</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Lodgement has been received by MPI, please await further direction</StatementDescription>
      <StatementTypeCode>ICN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>44061970</ID>
        <AcceptanceDateTime formatCode=""204"">20180710163634</AcceptanceDateTime>
        <FunctionalReferenceID>B00003787</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20180710163634</EffectiveDateTime>
      <NameCode>F05</NameCode>
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

		const string MPIBIOResponseB00003787 =
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
    <IssueDateTime formatCode=""204"">20180710163646</IssueDateTime>
    <FunctionalReferenceID>3346</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>44061970</ID>
        <AcceptanceDateTime formatCode=""204"">20180710163646</AcceptanceDateTime>
        <FunctionalReferenceID>B00003787</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20180710163646</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20180710163646</ReleaseDateTime>
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

		const string NZCSResponseB00003787 =
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
    <IssueDateTime formatCode=""204"">20180710163644</IssueDateTime>
    <FunctionalReferenceID>3345</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>10 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>44061970</ID>
        <AcceptanceDateTime formatCode=""204"">20180710163644</AcceptanceDateTime>
        <FunctionalReferenceID>B00003787</FunctionalReferenceID>
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
            <TaxAssessedAmount currencyID=""NZD"">264.62</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180710163644</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20180710163644</ReleaseDateTime>
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

		const string MPIFoodClearedB00003787 =
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
    <IssueDateTime formatCode=""204"">20180810173937</IssueDateTime>
    <FunctionalReferenceID>3577</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>44061970</ID>
        <AcceptanceDateTime formatCode=""204"">20180810173937</AcceptanceDateTime>
        <FunctionalReferenceID>B00003787</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20180810173937</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20180810173937</ReleaseDateTime>
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

		#region B00003998

		const string B00003998_NZCSResponse =
 @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20180919144210</IssueDateTime>
    <FunctionalReferenceID>4055</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>11436474</ID>
        <AcceptanceDateTime formatCode=""204"">20180919144210</AcceptanceDateTime>
        <FunctionalReferenceID>B00003998</FunctionalReferenceID>
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
            <ID>G923878</ID>
            <TypeCode>HWB</TypeCode>
          </TransportContractDocument>
        </Consignment>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20180919144210</EffectiveDateTime>
      <NameCode>C06</NameCode>
      <ReleaseDateTime formatCode=""204"">20180919144210</ReleaseDateTime>
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

		#region TestMessages

		const string CustomsSubmitterResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
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

		const string CustomsDepotResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20181017161431</IssueDateTime>
    <FunctionalReferenceID>1701</FunctionalReferenceID>
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
        <TotalGrossMassMeasure unitCode=""KGM"">500</TotalGrossMassMeasure>
        <VersionID>1</VersionID>
        <JurisdictionDateTime formatCode=""102"">20181017</JurisdictionDateTime>
        <Submitter>
          <Name>CargoWise 2</Name>
        </Submitter>
        <Agent>
          <Name>CargoWise 2</Name>
        </Agent>
        <BorderTransportMeans>
          <Name>NZ1</Name>
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
              <ID>08600234920</ID>
              <TypeCode>MB</TypeCode>
            </TransportContractDocument>
            <TransportContractDocument>
              <ID>G02294</ID>
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
          <QuantityQuantity>50</QuantityQuantity>
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

		#endregion

		#region B00151034 Response

		const string B151034Msg = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>CRI</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>ICR</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>ICR</TypeCode>
  <FunctionalReferenceID>B00151034</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <Submitter>
    <ID>40445226G </ID>
  </Submitter>
  <BorderTransportMeans>
    <Name>CX113</Name>
    <TypeCode>4</TypeCode>
    <ArrivalDateTime formatCode=""102"">20190709</ArrivalDateTime>
    <FirstArrivalLocationID>NZAKL</FirstArrivalLocationID>
  </BorderTransportMeans>
  <Carrier>
    <Name>CATHAY PACIFIC</Name>
  </Carrier>
  <Consignment>
    <SequenceNumeric>1</SequenceNumeric>
    <ValueAmount currencyID=""NZD"">80.00</ValueAmount>
    <AdditionalInformation>
      <StatementCode>Y</StatementCode>
      <StatementTypeCode>WOF</StatementTypeCode>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementDescription>1780800,Kuehne &amp; Nagel Ltd</StatementDescription>
      <StatementTypeCode>MAC</StatementTypeCode>
    </AdditionalInformation>
    <AssociatedTransportDocument>
      <ID>16009163405</ID>
      <TypeCode>MB</TypeCode>
    </AssociatedTransportDocument>
    <Consignee>
      <ID>40402664L</ID>
      <Address>
        <CityName>AUCKLAND</CityName>
        <CountryCode>NZ</CountryCode>
        <CountrySubDivisionName>AUK</CountrySubDivisionName>
        <Line>33 GALWAY STREET AUCKLAND CENTRAL</Line>
        <PostcodeID>1010</PostcodeID>
      </Address>
      <Communication>
        <ID>salog-archive@Kuehne-Nagel.com</ID>
        <TypeID>EM</TypeID>
      </Communication>
    </Consignee>
    <ConsignmentItem>
      <SequenceNumeric>1</SequenceNumeric>
      <Commodity>
        <CargoDescription>RING RETURNED AFTER REPAIR</CargoDescription>
        <ValueAmount currencyID=""NZD"">80.00</ValueAmount>
        <Temperature />
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">0.4</GrossMassMeasure>
      </GoodsMeasure>
      <Origin>
        <CountryCode>HK</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <QuantityQuantity>1</QuantityQuantity>
        <TypeCode>PK</TypeCode>
      </Packaging>
    </ConsignmentItem>
    <Consignor>
      <Name>TIFFANY &amp; CO ASIA PACIFIC LTD</Name>
      <Address>
        <CityName>KOWLOON</CityName>
        <CountryCode>HK</CountryCode>
        <Line>6/F, YUE HWA INTERNATIONAL BUILDING, YAU TSIM MONG 1 KOWLOON PARK DRIV</Line>
        <PostcodeID />
      </Address>
    </Consignor>
    <Freight>
      <PaymentMethodCode />
    </Freight>
    <GoodsConsignedPlace>
      <ID>HKHKG</ID>
    </GoodsConsignedPlace>
    <GoodsLocation>
      <ID>8718B</ID>
    </GoodsLocation>
    <LoadingLocation>
      <ID>HKHKG</ID>
    </LoadingLocation>
    <TransportContractDocument>
      <ID />
      <TypeCode>HWB</TypeCode>
    </TransportContractDocument>
    <UnloadingLocation>
      <ID>NZAKL</ID>
      <ArrivalDateTime formatCode=""203"">201907090000</ArrivalDateTime>
    </UnloadingLocation>
  </Consignment>
  <Declarant>
    <ID>40171170J</ID>
    <Communication>
      <ID>Sophie.Loader@kuehne-nagel.com</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>6492574924</ID>
      <TypeID>TE</TypeID>
    </Communication>
  </Declarant>
</Declaration>
</DocumentMetadata>";

		const string MessageError = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESICR</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.0</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>40445226G</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20190711103843</IssueDateTime>
    <FunctionalReferenceID>377728</FunctionalReferenceID>
    <FunctionCode>48</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>00000000</ID>
        <FunctionalReferenceID>B00151034</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>40445226G</ID>
        </Submitter>
        <ResponsibleGovernmentAgency>
          <ID>TSW</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Error>
      <ValidationCode>187</ValidationCode>
      <Pointer>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>28A</DocumentSectionCode>
        <TagID>D023</TagID>
      </Pointer>
    </Error>
    <Status>
      <EffectiveDateTime formatCode=""204"">20190711103843</EffectiveDateTime>
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
		#endregion

		#endregion
	}

	public class DeclarationResponseAttachmentsTest : TestCaseWithFactory
	{
		public void TestAttachedDocumentIsProcessed()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000004642";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = ClearWithDeliveryOrder;
			interchange.DocManagerInfo().AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <DeliveryOrder> %EOF\n"), "IM1_Delivery_Order-20790638-2014-09-12-132701430.pdf", "FCT");
			AssertEquals("Pre-condition: Test Delivery Order should be attached to interchange", 1, ((IDocManagerSupport)interchange).DocManagerInfo.AllEDocs.Count);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001506";
			CusEntryHeader entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_MessageText = OriginalDeclaration;
			outgoingMessage.EM_MessageNum = "1";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "B00001506";
			outgoingMessage.EM_LinkedObject = entryHeader;

			var im1ResponseMessage = Factory.New<TSWMessage>();
			im1ResponseMessage.EM_EI = interchange.PK;
			im1ResponseMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			im1ResponseMessage.EM_MessageText = ClearWithDeliveryOrder;
			im1ResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			im1ResponseMessage.EM_LinkedObject = entryHeader;
			interchange.DocManagerInfo().MasterFactory.Save();
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(im1ResponseMessage);
			Factory.Save();

			AssertEquals(entryHeader, im1ResponseMessage.EM_LinkedObject);
			var reLoadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("Delivery Order should have been attached to the JobDeclaration", 1, ((IDocManagerSupport)reLoadedDeclaration).DocManagerInfo.AllEDocs.Count);
			var declarationDoc = ((IDocManagerSupport)reLoadedDeclaration).DocManagerInfo.AllEDocs[0];
			AssertEquals("IM1 Deliver Order file name", "IM1_DeliveryOrder_B00001506.pdf", declarationDoc.FileName);
			AssertEquals("Doc Type should be Delivery Order", Core.Constants.RefDocTypes.DeliveryOrder, declarationDoc.DocType);
		}

		public void TestAttachedDocumentIsProcessedFromShipmentResponse()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000004642";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = ClearWithDeliveryOrder;
			interchange.DocManagerInfo().AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <DeliveryOrder> %EOF\n"), "IM1_Delivery_Order-20790638-2014-09-12-132701430.pdf", "FCT");
			AssertEquals("Pre-condition: Test Delivery Order should be attached to interchange", 1, ((IDocManagerSupport)interchange).DocManagerInfo.AllEDocs.Count);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBill = "BOL01010101";
			shipment.JS_OuterPacks = 12;
			shipment.JS_F3_NKPackType = "BX";
			shipment.JS_RL_NKDestination = "NZNPE";
			shipment.JS_ActualWeight = 10;
			shipment.JS_UnitOfWeight = Constants.Weight.Kilograms;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001506";
			declaration.JE_JS = shipment.PK;
			CusEntryHeader entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_MessageText = OriginalDeclaration;
			outgoingMessage.EM_MessageNum = "1";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "B00001506";
			outgoingMessage.EM_LinkedObject = entryHeader;

			var im1ResponseMessage = Factory.New<TSWMessage>();
			im1ResponseMessage.EM_EI = interchange.PK;
			im1ResponseMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			im1ResponseMessage.EM_MessageText = ClearWithDeliveryOrder;
			im1ResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			im1ResponseMessage.EM_LinkedObject = entryHeader;
			interchange.DocManagerInfo().MasterFactory.Save();
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(im1ResponseMessage);
			Factory.Save();

			AssertEquals(entryHeader, im1ResponseMessage.EM_LinkedObject);
			var reLoadedShipment = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);
			AssertEquals("Delivery Order should not have been attached to the Shipment job", 0, ((IDocManagerSupport)reLoadedShipment).DocManagerInfo.AllEDocs.Count);
			var reLoadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("Delivery Order should have been attached to the Declaration job", 1, ((IDocManagerSupport)reLoadedDeclaration).DocManagerInfo.AllEDocs.Count);

			var shipmentBasedDeclarationResponseDocument = ((IDocManagerSupport)reLoadedDeclaration).DocManagerInfo.AllEDocs[0];
			AssertEquals("IM1 Deliver Order file name", "IM1_DeliveryOrder_B00001506.pdf", shipmentBasedDeclarationResponseDocument.FileName);
			AssertEquals("Doc Type should be Delivery Order", Core.Constants.RefDocTypes.DeliveryOrder, shipmentBasedDeclarationResponseDocument.DocType);
		}

		public void TestExportDecDOIsProcessed()
		{
			NZCustomsDataRegistry.Instance.NZBrokerageID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B");
			var interchange = Factory.New<NZCInterchange>();
			interchange.EI_From = "CUSSWT";
			interchange.EI_To = "00009908C";
			interchange.EI_ApplicationCode = "NZC";
			interchange.EI_InterchangeType = "NZC";
			interchange.EI_InterchangeNum = "00000000000000004670";
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_IsActive = true;
			interchange.EI_BodyText = EX1ClearWithDeliveryOrder;

			interchange.DocManagerInfo().AddFileOrDocument(Encoding.ASCII.GetBytes("%PDF <DeliveryOrder> %EOF\n"), "EX1_Delivery_Order-70733877-2014-09-23-194821877.pdf", "FCT");
			interchange.DocManagerInfo().MasterFactory.Save();

			Factory.Save();
			AssertEquals("Pre-condition: Test Delivery Order should be attached to interchange", 1, ((IDocManagerSupport)interchange).DocManagerInfo.AllEDocs.Count);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarationReference = "B00001515";
			CusEntryHeader entryHeader = declaration.ActiveEntryHeaders.AddNew();

			var outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.E40;
			outgoingMessage.EM_MessageText = EX1OriginalDeclaration;
			outgoingMessage.EM_MessageNum = "1";
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationReference = "B00001515";
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_LinkUniqueID = entryHeader.PK;

			var ex1ResponseMessage = Factory.New<TSWMessage>();
			ex1ResponseMessage.EM_EI = interchange.PK;
			ex1ResponseMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			ex1ResponseMessage.EM_MessageText = EX1ClearWithDeliveryOrder;
			ex1ResponseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;

			interchange.DocManagerInfo().MasterFactory.Save();
			Factory.Save();

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(ex1ResponseMessage);
			Factory.Save();

			AssertEquals(entryHeader, ex1ResponseMessage.EM_LinkedObject);
			var reLoadedDeclaration = new BusinessObjectFactory().Load<JobDeclaration>(declaration.PK);
			AssertEquals("Delivery Order should have been attached to the JobDeclaration", 1, ((IDocManagerSupport)reLoadedDeclaration).DocManagerInfo.AllEDocs.Count);
			var declarationDoc = ((IDocManagerSupport)reLoadedDeclaration).DocManagerInfo.AllEDocs[0];
			AssertEquals("EX1 Deliver Order file name", "EX1_DeliveryOrder_B00001515.pdf", declarationDoc.FileName);
			AssertEquals("Doc Type should be Delivery Order", Core.Constants.RefDocTypes.DeliveryOrder, declarationDoc.DocType);
		}

		#region Messages

		public const string OriginalDeclaration = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>IM</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>IM1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>I10</TypeCode>
  <FunctionalReferenceID>B00001506C</FunctionalReferenceID>
  <FunctionCode>22</FunctionCode>
  <TotalGrossMassMeasure unitCode=""KGM"">28000</TotalGrossMassMeasure>
  <JurisdictionDateTime formatCode=""102"">20140918</JurisdictionDateTime>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalInformation>
    <Content>Completion entry - testing response attached docs</Content>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>PDO</StatementCode>
    <StatementTypeCode>OIN</StatementTypeCode>
  </AdditionalInformation>
  <Agent>
    <ID>00009908C</ID>
    <RoleCode>CB</RoleCode>
  </Agent>
  <BorderTransportMeans>
    <Name>CAI YUN HE</Name>
    <ID>9228758</ID>
    <TypeCode>1</TypeCode>
    <JourneyID>243S</JourneyID>
  </BorderTransportMeans>
  <CurrencyExchange>
    <RateNumeric>1.00</RateNumeric>
    <CurrencyTypeCode>NZD</CurrencyTypeCode>
  </CurrencyExchange>
  <Declarant>
    <ID>40006206E</ID>
    <Communication>
      <ID>gary.odea@wisetechglobal.com</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>61280012200</ID>
      <TypeID>TE</TypeID>
    </Communication>
  </Declarant>
  <DutyTaxFee>
    <Payment>
      <MethodCode>C</MethodCode>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>SL</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">280.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>CUD</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">4750.00</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>GST</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">15645.75</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <DutyTaxFee>
    <TypeCode>TOT</TypeCode>
    <Payment>
      <TaxAssessedAmount currencyID=""NZD"">20675.75</TaxAssessedAmount>
    </Payment>
  </DutyTaxFee>
  <GoodsShipment>
    <ExportationCountryCode>JP</ExportationCountryCode>
    <TransactionNatureCode>40</TransactionNatureCode>
    <Consignment>
      <GoodsLocation>
        <ID>25001</ID>
      </GoodsLocation>
      <LoadingLocation>
        <ID>JPTYO</ID>
      </LoadingLocation>
      <TransportContractDocument>
        <ID>OB845349</ID>
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
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>S94248</ID>
        <TypeCode>BM</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <UnloadingLocation>
        <ID>NZWLG</ID>
      </UnloadingLocation>
    </Consignment>
    <CustomsValuation>
      <FreightChargeAmount currencyID=""NZD"">3780</FreightChargeAmount>
      <FreightChargeApportionmentCode>161</FreightChargeApportionmentCode>
    </CustomsValuation>
    <DeliveryDestination>
      <Name>ADULATION BOOKS LTD (NZ CUSTOMS)</Name>
      <Address>
        <CityName>AUCKLAND</CityName>
        <CountryCode>NZ</CountryCode>
        <CountrySubDivisionName>AUK</CountrySubDivisionName>
        <Line>1 MAIN PLACE</Line>
        <PostcodeID>2000</PostcodeID>
      </Address>
    </DeliveryDestination>
    <GovernmentAgencyGoodsItem>
      <SequenceNumeric>1</SequenceNumeric>
      <CustomsValueAmount currencyID=""NZD"">95000</CustomsValueAmount>
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
        <Description>FLAT ROLLED PROD ETC NOT COILED ETC EX 10MM THICK RLLD ETC WDTH NE 1250MM ET</Description>
        <ValueAmount currencyID=""NZD"">95000</ValueAmount>
        <Classification>
          <ID>7208511000F</ID>
          <IdentificationTypeCode>HS</IdentificationTypeCode>
        </Classification>
        <DutyTaxFee>
          <DutyRegimeCode>NML</DutyRegimeCode>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>SL</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">280.00</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>CUD</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">4750.00</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <DutyTaxFee>
          <TypeCode>GST</TypeCode>
          <Payment>
            <TaxAssessedAmount currencyID=""NZD"">15645.75</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <Source>
          <CountryCode>JP</CountryCode>
        </Source>
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">150</GrossMassMeasure>
        <NetNetWeightMeasure unitCode=""KGM"">0</NetNetWeightMeasure>
        <TariffQuantity unitCode=""KGM"">28000.000</TariffQuantity>
      </GoodsMeasure>
      <Origin>
        <CountryCode>JP</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <MarksNumbersID>NM</MarksNumbersID>
        <QuantityQuantity>80</QuantityQuantity>
        <TypeCode>OK</TypeCode>
        <VolumeMeasure unitCode=""MTQ"">150</VolumeMeasure>
      </Packaging>
      <ValuationAdjustment>
        <AdditionCode>151</AdditionCode>
        <AmountAmount currencyID=""NZD"">3780</AmountAmount>
      </ValuationAdjustment>
      <ValuationAdjustment>
        <AdditionCode>150</AdditionCode>
        <AmountAmount currencyID=""NZD"">495</AmountAmount>
      </ValuationAdjustment>
    </GovernmentAgencyGoodsItem>
    <Invoice>
      <ID>H429675Q</ID>
      <ConditionCode>FOB</ConditionCode>
      <SequenceNumeric>1</SequenceNumeric>
    </Invoice>
    <Supplier>
      <ID>00710841Y</ID>
      <Contact>
        <Name>Unknown</Name>
        <Communication>
          <ID>brendon.paine@cargowise.com</ID>
          <TypeID>EM</TypeID>
        </Communication>
        <Communication>
          <ID>61288889999</ID>
          <TypeID>TE</TypeID>
        </Communication>
        <Communication>
          <ID>61266665557</ID>
          <TypeID>FX</TypeID>
        </Communication>
      </Contact>
    </Supplier>
  </GoodsShipment>
  <Importer>
    <ID>51352368J</ID>
    <Contact>
      <Name>Bill Smith</Name>
      <Communication>
        <ID>brendon.paine@cargowise.com</ID>
        <TypeID>EM</TypeID>
      </Communication>
      <Communication>
        <ID>64966667777</ID>
        <TypeID>TE</TypeID>
      </Communication>
      <Communication>
        <ID>64966667777</ID>
        <TypeID>FX</TypeID>
      </Communication>
    </Contact>
  </Importer>
  <Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>80</QuantityQuantity>
    <TypeCode>OK</TypeCode>
  </Packaging>
  <PreviousDocument>
    <ID>19410937</ID>
    <TypeCode>I52</TypeCode>
  </PreviousDocument>
</Declaration>
</DocumentMetadata>";

		public const string ClearWithDeliveryOrder = @"<?xml version=""1.0"" encoding=""UTF-8""?>
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
    <IssueDateTime formatCode=""204"">20140912132642</IssueDateTime>
    <FunctionalReferenceID>1619</FunctionalReferenceID>
    <FunctionCode>23</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>CDO</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_E285778C-6894-4868-829B-0919652E3327"" filename=""IM1_Delivery_Order-20790638-2014-09-12-132701430.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <OverallDeclaration>
      <Declaration>
        <ID>20790638</ID>
        <AcceptanceDateTime formatCode=""204"">20140912132642</AcceptanceDateTime>
        <FunctionalReferenceID>B00001506</FunctionalReferenceID>
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
            <TaxAssessedAmount>20722.64</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20140912132642</EffectiveDateTime>
      <NameCode>816</NameCode>
      <ReleaseDateTime formatCode=""204"">20140912132656</ReleaseDateTime>
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

		public const string EX1OriginalDeclaration = @"<?xml version=""1.0"" encoding=""utf-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"">
<WCODataModelVersion>3.2</WCODataModelVersion>
<WCODocumentName>EX</WCODocumentName>
<CountryCode>NZ</CountryCode>
<AgencyAssignedCustomizedDocumentName>EX1</AgencyAssignedCustomizedDocumentName>
<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
<Declaration xmlns=""urn:wco:datamodel:WCO:DeclarationModel:1"">
  <TypeCode>E40</TypeCode>
  <FunctionalReferenceID>B00001515</FunctionalReferenceID>
  <FunctionCode>9</FunctionCode>
  <TotalGrossMassMeasure unitCode=""KGM"">100</TotalGrossMassMeasure>
  <Submitter>
    <ID>00009908C</ID>
  </Submitter>
  <AdditionalInformation>
    <StatementCode>N</StatementCode>
    <StatementTypeCode>ERI</StatementTypeCode>
    <Pointer>
      <SequenceNumeric>1</SequenceNumeric>
      <DocumentSectionCode>40A</DocumentSectionCode>
    </Pointer>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>PDO</StatementCode>
    <StatementTypeCode>OIN</StatementTypeCode>
  </AdditionalInformation>
  <Agent>
    <ID>00009908C</ID>
    <RoleCode>CB</RoleCode>
  </Agent>
  <BorderTransportMeans>
    <Name>QF108</Name>
    <TypeCode>4</TypeCode>
  </BorderTransportMeans>
  <CurrencyExchange>
    <RateNumeric>1</RateNumeric>
    <CurrencyTypeCode>NZD</CurrencyTypeCode>
  </CurrencyExchange>
  <Declarant>
    <ID>40006206E</ID>
    <Communication>
      <ID>gary.odea@wisetechglobal.com</ID>
      <TypeID>EM</TypeID>
    </Communication>
    <Communication>
      <ID>61280012200</ID>
      <TypeID>TE</TypeID>
    </Communication>
  </Declarant>
  <Exporter>
    <ID>51352368J</ID>
  </Exporter>
  <GoodsShipment>
    <ExitDateTime formatCode=""102"">20140923</ExitDateTime>
    <TransactionNatureCode>10</TransactionNatureCode>
    <Consignment>
      <GoodsLocation>
        <ID>7175H</ID>
      </GoodsLocation>
      <LoadingLocation>
        <ID>NZAKL</ID>
      </LoadingLocation>
      <TransportContractDocument>
        <ID>08104309233</ID>
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
      </TransportContractDocument>
      <TransportContractDocument>
        <ID>58237472</ID>
        <TypeCode>HWB</TypeCode>
        <Pointer>
          <DocumentSectionCode>42A</DocumentSectionCode>
        </Pointer>
        <Pointer>
          <SequenceNumeric>1</SequenceNumeric>
          <DocumentSectionCode>93A</DocumentSectionCode>
        </Pointer>
      </TransportContractDocument>
      <UnloadingLocation>
        <ID>AUSYD</ID>
      </UnloadingLocation>
    </Consignment>
    <DeliveryDestination>
      <Name>TEST SUPPLIER</Name>
      <Address>
        <CityName>SYDNEY</CityName>
        <CountryCode>AU</CountryCode>
        <CountrySubDivisionName>NSW</CountrySubDivisionName>
        <Line>1 GEORGE ST</Line>
        <PostcodeID>2000</PostcodeID>
      </Address>
    </DeliveryDestination>
    <GovernmentAgencyGoodsItem>
      <SequenceNumeric>1</SequenceNumeric>
      <Commodity>
        <Description>OTHER ARTICLES OF VULCANISED RUBBER</Description>
        <ValueAmount currencyID=""NZD"">5000.00</ValueAmount>
        <Classification>
          <ID>4016999929H</ID>
          <IdentificationTypeCode>HS</IdentificationTypeCode>
        </Classification>
      </Commodity>
      <GoodsMeasure>
        <GrossMassMeasure unitCode=""KGM"">100</GrossMassMeasure>
        <NetNetWeightMeasure unitCode=""KGM"">0</NetNetWeightMeasure>
      </GoodsMeasure>
      <Origin>
        <CountryCode>CN</CountryCode>
      </Origin>
      <Packaging>
        <SequenceNumeric>1</SequenceNumeric>
        <MarksNumbersID>N/M</MarksNumbersID>
        <QuantityQuantity>200</QuantityQuantity>
        <TypeCode>CT</TypeCode>
        <VolumeMeasure unitCode=""MTQ"">2</VolumeMeasure>
      </Packaging>
    </GovernmentAgencyGoodsItem>
    <Importer>
      <Name>TEST SUPPLIER</Name>
      <Address>
        <CityName>SYDNEY</CityName>
        <CountryCode>AU</CountryCode>
        <CountrySubDivisionName>NSW</CountrySubDivisionName>
        <Line>1 GEORGE ST</Line>
        <PostcodeID>2000</PostcodeID>
      </Address>
    </Importer>
    <Invoice>
      <IssueDateTime formatCode=""102"">20140328</IssueDateTime>
      <ID>616514214</ID>
      <SequenceNumeric>1</SequenceNumeric>
    </Invoice>
  </GoodsShipment>
  <Packaging>
    <SequenceNumeric>1</SequenceNumeric>
    <QuantityQuantity>20</QuantityQuantity>
    <TypeCode>CT</TypeCode>
  </Packaging>
</Declaration>
</DocumentMetadata>";

		public const string EX1ClearWithDeliveryOrder = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <WCODataModelVersion>3.2</WCODataModelVersion>
  <WCODocumentName>RES</WCODocumentName>
  <CountryCode>NZ</CountryCode>
  <AgencyAssignedCustomizedDocumentName>RESEX1</AgencyAssignedCustomizedDocumentName>
  <AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>
  <CommunicationMetaData>
    <Recipient>
      <ID>51358596K</ID>
      <RoleCode>TB</RoleCode>
    </Recipient>
  </CommunicationMetaData>
  <Response xmlns=""urn:wco:datamodel:WCO:ResponseModel:1"">
    <IssueDateTime formatCode=""204"">20140923194815</IssueDateTime>
    <FunctionalReferenceID>1646</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>CDO</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_6FD29B01-69CF-40B5-84EF-98A9FDB64EB9"" filename=""EX1_Delivery_Order-70733877-2014-09-23-194821877.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <AdditionalInformation>
      <StatementDescription>20 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>70733877</ID>
        <AcceptanceDateTime formatCode=""204"">20140923194815</AcceptanceDateTime>
        <FunctionalReferenceID>B00001515</FunctionalReferenceID>
        <VersionID>1</VersionID>
        <Submitter>
          <ID>00009908C</ID>
        </Submitter>
        <DutyTaxFee>
          <Payment>
            <MethodCode>C</MethodCode>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20140923194815</EffectiveDateTime>
      <NameCode>809</NameCode>
      <ReleaseDateTime formatCode=""204"">20140923194815</ReleaseDateTime>
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

	public class DeclarationResponseAutoPrintingTest : TestCaseWithFactory
	{
		public void TestDeliveryOrderResponseTriggersAutoPrinting()
		{
			outgoingMessage.EM_ApplicationReference = "B00001252";

			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			declaration.JE_DeclarationReference = "B00001252";
			declaration.JE_GB = otherBranch.PK;
			decCreator.MergeDeclaration();

			declaration.CusEntryHeader.MergedLines.AddNew().DutyAmount = 11319.5m;

			ProcessMessage(DOResponse, new ZDateTime(2016, 03, 15, 14, 35, 15));

			var cusCertJobs = GetPrintJobs(cusCertQueue);
			AssertEquals("CusCertJobs.Count", 1, cusCertJobs.Count);
			AssertEquals("CusCertJobs[0].SP_Copies", 1, cusCertJobs[0].SP_Copies.ToZInt());
			var emailSubject = GlbCompany.CurrentCompany.GC_Name.ToUpper() + " - DUMMY Branch 2 - {0}";
			AssertEquals("Auto Print EmailSubjectLine", string.Format(emailSubject, "Customs Certificate for " + declaration.JE_DeclarationReference), cusCertJobs[0].SP_EmailSubjectLine);
			AssertEquals("Auto Print should use Declaration's branch", otherBranch.PK, cusCertJobs[0].SP_GB);

			var entryJobs = GetPrintJobs(entryQueue);
			AssertEquals("EntryJobs.Count", 1, entryJobs.Count);
			AssertEquals("EntryJobs[0].SP_Copies", 2, entryJobs[0].SP_Copies.ToZInt());
			AssertEquals("Auto Print EmailSubjectLine", string.Format(emailSubject, "Customs Entry for " + declaration.JE_DeclarationReference), entryJobs[0].SP_EmailSubjectLine);
			AssertEquals("Auto Print should use Declaration's branch", otherBranch.PK, entryJobs[0].SP_GB);

			var dOrderJobs = GetPrintJobs(dOrderQueue);
			AssertEquals("DOrderJobs.Count", 1, dOrderJobs.Count);
			AssertEquals("DOrderJobs[0].SP_Copies", 4, dOrderJobs[0].SP_Copies.ToZInt());
			AssertEquals("Auto Print EmailSubjectLine", string.Format(emailSubject, "Delivery Order - 48151967"), dOrderJobs[0].SP_EmailSubjectLine);
			AssertEquals("Auto Print should use Declaration's branch", otherBranch.PK, dOrderJobs[0].SP_GB);
		}

		public void TestNonDOClearanceDoesNotPrintDOWhenAutoPrinting()
		{
			outgoingMessage.EM_ApplicationReference = "B00002306";

			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForExportToAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			declaration.JE_DeclarationReference = "B00002306";
			declaration.JE_GB = otherBranch.PK;
			decCreator.MergeDeclaration();

			declaration.CusEntryHeader.MergedLines.AddNew().DutyAmount = 11319.5m;
			ProcessMessage(ClearedResponse, new ZDateTime(2016, 03, 15, 14, 35, 15));

			var cusCertJobs = GetPrintJobs(cusCertQueue);
			AssertEquals("CusCertJobs.Count", 1, cusCertJobs.Count);
			AssertEquals("CusCertJobs[0].SP_Copies", 1, cusCertJobs[0].SP_Copies.ToZInt());
			var emailSubject = GlbCompany.CurrentCompany.GC_Name.ToUpper() + " - DUMMY Branch 2 - {0}";
			AssertEquals("Auto Print EmailSubjectLine", string.Format(emailSubject, "Customs Certificate for " + declaration.JE_DeclarationReference), cusCertJobs[0].SP_EmailSubjectLine);
			AssertEquals("Auto Print should use Declaration's branch", otherBranch.PK, cusCertJobs[0].SP_GB);

			var entryJobs = GetPrintJobs(entryQueue);
			AssertEquals("EntryJobs.Count", 1, entryJobs.Count);
			AssertEquals("EntryJobs[0].SP_Copies", 2, entryJobs[0].SP_Copies.ToZInt());
			AssertEquals("Auto Print EmailSubjectLine", string.Format(emailSubject, "Customs Entry for " + declaration.JE_DeclarationReference), entryJobs[0].SP_EmailSubjectLine);
			AssertEquals("Auto Print should use Declaration's branch", otherBranch.PK, entryJobs[0].SP_GB);

			var dOrderJobs = GetPrintJobs(dOrderQueue);
			AssertEquals("DOrderJobs.Count - D/O should not be printed for this response", 0, dOrderJobs.Count);
		}

		public void TestAutoPrintingOnlyTriggeredOneTimeForCustomsCertificateAndEntryPrint()
		{
			outgoingMessage.EM_ApplicationReference = "B00001252";

			var decCreator = new TestFormalEntryCreator(declaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			declaration.JE_DeclarationReference = "B00001252";
			declaration.JE_GB = otherBranch.PK;
			decCreator.MergeDeclaration();

			declaration.CusEntryHeader.MergedLines.AddNew().DutyAmount = 11319.5m;
			ProcessMessage(DOResponse, new ZDateTime(2016, 03, 15, 14, 35, 15));

			var cusCertJobs = GetPrintJobs(cusCertQueue);
			AssertEquals("CusCertJobs.Count", 1, cusCertJobs.Count);
			AssertEquals("CusCertJobs[0].SP_Copies", 1, cusCertJobs[0].SP_Copies.ToZInt());
			var emailSubject = GlbCompany.CurrentCompany.GC_Name.ToUpper() + " - DUMMY Branch 2 - {0}";
			AssertEquals("Auto Print EmailSubjectLine", string.Format(emailSubject, "Customs Certificate for " + declaration.JE_DeclarationReference), cusCertJobs[0].SP_EmailSubjectLine);
			AssertEquals("Auto Print should use Declaration's branch", otherBranch.PK, cusCertJobs[0].SP_GB);

			var entryJobs = GetPrintJobs(entryQueue);
			AssertEquals("EntryJobs.Count", 1, entryJobs.Count);
			AssertEquals("EntryJobs[0].SP_Copies", 2, entryJobs[0].SP_Copies.ToZInt());
			AssertEquals("Auto Print EmailSubjectLine", string.Format(emailSubject, "Customs Entry for " + declaration.JE_DeclarationReference), entryJobs[0].SP_EmailSubjectLine);
			AssertEquals("Auto Print should use Declaration's branch", otherBranch.PK, entryJobs[0].SP_GB);

			ProcessMessage(MPIBIOResponse, new ZDateTime(2016, 03, 15, 14, 35, 15));

			cusCertJobs = GetPrintJobs(cusCertQueue);
			AssertEquals("CusCertJobs.Count", 1, cusCertJobs.Count);
			AssertEquals("CusCertJobs[0].SP_Copies", 1, cusCertJobs[0].SP_Copies.ToZInt());
			emailSubject = GlbCompany.CurrentCompany.GC_Name.ToUpper() + " - DUMMY Branch 2 - {0}";
			AssertEquals("Auto Print EmailSubjectLine", string.Format(emailSubject, "Customs Certificate for " + declaration.JE_DeclarationReference), cusCertJobs[0].SP_EmailSubjectLine);
			AssertEquals("Auto Print should use Declaration's branch", otherBranch.PK, cusCertJobs[0].SP_GB);

			entryJobs = GetPrintJobs(entryQueue);
			AssertEquals("EntryJobs.Count", 1, entryJobs.Count);
			AssertEquals("EntryJobs[0].SP_Copies", 2, entryJobs[0].SP_Copies.ToZInt());
			AssertEquals("Auto Print EmailSubjectLine", string.Format(emailSubject, "Customs Entry for " + declaration.JE_DeclarationReference), entryJobs[0].SP_EmailSubjectLine);
			AssertEquals("Auto Print should use Declaration's branch", otherBranch.PK, entryJobs[0].SP_GB);
		}

		public void TestAutoPrintingOnlyTriggeredOneTimeForCustomsCertificateAndEntryPrint_ConsolidatedDeclaration()
		{
			outgoingMessage.EM_ApplicationReference = "B00001252";
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
			consolidatedDeclaration.CRD_JobReferenceNumber = "B00001252";
			consolidatedDeclaration.CRD_GB_Branch = otherBranch.PK;
			outgoingMessage.EM_LinkedObject = consolidatedDeclaration;

			var decCreator = new TestFormalEntryCreator(consolidatedDeclaration.JobDeclarations[0] as JobDeclaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			consolidatedDeclaration.JobDeclarations[0].JE_GB = otherBranch.PK;
			consolidatedDeclaration.JobDeclarations[0].JE_DeclarationReference = "123";
			decCreator.MergeDeclaration();

			decCreator = new TestFormalEntryCreator(consolidatedDeclaration.JobDeclarations[1] as JobDeclaration);
			decCreator.SetupTestConsignmentDetails();
			decCreator.SetupTestForAir();
			decCreator.SetupTestForImportFromAU();
			decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
			decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
			decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
			decCreator.AddHouseBillWithPackingDetails("HOUSEBILL1", 100, "PK");
			consolidatedDeclaration.JobDeclarations[1].JE_GB = otherBranch.PK;
			consolidatedDeclaration.JobDeclarations[1].JE_DeclarationReference = "456";
			decCreator.MergeDeclaration();

			declaration.CusEntryHeader.MergedLines.AddNew().DutyAmount = 11319.5m;
			using (Env.SetTemporaryUserContext(new UserContext(User.ServiceUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
			{
				var tswMessage = Factory.New<TSWMessage>();
				tswMessage.EM_MessageType = MessageTypeList.Codes.TWR;
				tswMessage.EM_MessageText = DOResponse;
				tswMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				tswMessage.EM_SystemCreateTimeUtc = new ZDateTime(2016, 03, 15, 14, 35, 15);
				consolidatedDeclaration.Messages.Add(tswMessage);

				var logger = new LoggingInformation();
				var processor = new MessageProcessorFactory(logger);
				processor.ProcessMessage(tswMessage);
			}

			var cusCertJobs = GetPrintJobs(cusCertQueue);
			AssertEquals("CusCertJobs.Count", 2, cusCertJobs.Count);
			AssertEquals("CusCertJobs[0].SP_Copies", 1, cusCertJobs[0].SP_Copies.ToZInt());
			var emailSubject = GlbCompany.CurrentCompany.GC_Name.ToUpper() + " - DUMMY Branch 2 - {0}";
			AssertEquals("Auto Print EmailSubjectLine", string.Format(emailSubject, "Customs Certificate for " + consolidatedDeclaration.JobDeclarations[0].JE_DeclarationReference), cusCertJobs[0].SP_EmailSubjectLine);
			AssertEquals("Auto Print should use Declaration's branch", otherBranch.PK, cusCertJobs[0].SP_GB);

			var entryJobs = GetPrintJobs(entryQueue);
			AssertEquals("EntryJobs.Count", 2, entryJobs.Count);
			AssertEquals("EntryJobs[0].SP_Copies", 2, entryJobs[0].SP_Copies.ToZInt());
			AssertEquals("Auto Print EmailSubjectLine", string.Format(emailSubject, "Customs Entry for " + consolidatedDeclaration.JobDeclarations[0].JE_DeclarationReference), entryJobs[0].SP_EmailSubjectLine);
			AssertEquals("Auto Print should use Declaration's branch", otherBranch.PK, entryJobs[0].SP_GB);
		}

		public void TestAutoPrintingForDeliveryOrder()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				NZCustomsDataRegistry.Instance.DeliveryOrderCopies.SetValue(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty, 1);
				Factory.Save();

				outgoingMessage.EM_ApplicationReference = "B00005395";

				var decCreator = new TestFormalEntryCreator(declaration);
				decCreator.SetupTestConsignmentDetails();
				decCreator.SetupTestForAir();
				decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
				decCreator.SetupImportInvoiceHeader("H059287", "FOB", "NZD", 2500m);
				decCreator.SetupImportInvoiceLine("4014900100B", "HOT-WATER BOTTLES", "AU", "AU", "N", 2500m);
				decCreator.AddHouseBillWithPackingDetails("GAZ95827", 10, "CT");
				declaration.JE_DeclarationReference = "B00005395";
				declaration.JE_GB = otherBranch.PK;
				decCreator.MergeDeclaration();

				declaration.CusEntryHeader.MergedLines.AddNew().GSTAmount = 403.5m;

				// Processing Customs response should cause the Delivery Order Printing
				ProcessMessage(B00005395CustomsResponse, new ZDateTime(2023, 02, 21, 15, 46, 45));

				var emailSubject = GlbCompany.CurrentCompany.GC_Name.ToUpper() + " - DUMMY Branch 2 - {0}";
				var dOrderJobs = GetPrintJobs(dOrderQueue);
				AssertEquals("DOrderJobs.Count", 1, dOrderJobs.Count);

				var doJob = dOrderJobs[0];
				AssertEquals("DOrderJobs[0].SP_Copies", 1, doJob.SP_Copies.ToZInt());
				AssertEquals("Auto Print email subject line", string.Format(emailSubject, "Delivery Order - 7263911"), doJob.SP_EmailSubjectLine);
				AssertEquals("Auto Print should use Declaration's branch", otherBranch.PK, doJob.SP_GB);

				// Processing MPIFood should not cause another Delivery Order Print
				ProcessMessage(B00005395FoodResponse, new ZDateTime(2023, 02, 21, 15, 47, 16));

				// Processing MPIBio response should also not cause another DO to be printed.
				ProcessMessage(B00005395BIOResponse, new ZDateTime(2023, 02, 21, 15, 47, 36));

				dOrderJobs = GetPrintJobs(dOrderQueue);
				AssertEquals("DOrderJobs.Count - system should still have only produced 1 DO", 1, dOrderJobs.Count);

				doJob = dOrderJobs[0];
				AssertEquals("DOrderJobs[0].SP_Copies", 1, doJob.SP_Copies.ToZInt());
				AssertEquals("Auto Print email subject line", string.Format(emailSubject, "Delivery Order - 7263911"), doJob.SP_EmailSubjectLine);
			}
		}

		public void TestAutoPrintConsolidationDOPrintsFullDeliveryInstructions()
		{
			using (NZCustomsDataRegistry.Instance.NZBrokerageID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "00009917B"))
			{
				NZCustomsDataRegistry.Instance.DeliveryOrderCopies.SetValue(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty, 1);

				outgoingMessage.EM_ApplicationReference = "CE00000007";
				var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
				consolidatedDeclaration.CRD_JobReferenceNumber = "CE00000007";
				consolidatedDeclaration.CRD_GB_Branch = otherBranch.PK;
				outgoingMessage.EM_LinkedObject = consolidatedDeclaration;

				var declaration1 = consolidatedDeclaration.JobDeclarations[0] as JobDeclaration;
				var decCreator = new TestFormalEntryCreator(declaration1);
				decCreator.SetupTestConsignmentDetails();
				decCreator.SetupTestForAir();
				decCreator.SetupTestForImportFromAU();
				decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
				decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
				decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
				decCreator.AddHouseBillWithPackingDetails("G68277", 10, "BX");
				consolidatedDeclaration.JobDeclarations[0].JE_GB = otherBranch.PK;
				consolidatedDeclaration.JobDeclarations[0].JE_DeclarationReference = "B00005393";
				decCreator.MergeDeclaration();

				var declaration2 = consolidatedDeclaration.JobDeclarations[1] as JobDeclaration;
				decCreator = new TestFormalEntryCreator(declaration2);
				decCreator.SetupTestConsignmentDetails();
				decCreator.SetupTestForAir();
				decCreator.SetupTestForImportFromAU();
				decCreator.SetupInvoiceGroup(1000m, "NZD", 10m, "NZD");
				decCreator.SetupImportInvoiceHeader("1001001", "FOB", "NZD", 10000m);
				decCreator.SetupImportInvoiceLine("2301100000F", "PROTONS", "AU", "AU", "N", 5000m);
				decCreator.AddHouseBillWithPackingDetails("G54528", 10, "BX");
				consolidatedDeclaration.JobDeclarations[1].JE_GB = otherBranch.PK;
				consolidatedDeclaration.JobDeclarations[1].JE_DeclarationReference = "B00005392";
				decCreator.MergeDeclaration();

				Factory.Save();

				declaration.CusEntryHeader.MergedLines.AddNew().DutyAmount = 11319.5m;

				using (Env.SetTemporaryUserContext(new UserContext(User.ServiceUserName, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
				{
					var acknowledgementMessage = Factory.New<TSWMessage>();
					acknowledgementMessage.EM_MessageType = MessageTypeList.Codes.TWR;
					acknowledgementMessage.EM_MessageText = ConsolidationACKResponse;
					acknowledgementMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					acknowledgementMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 02, 17, 15, 36, 13);
					consolidatedDeclaration.Messages.Add(acknowledgementMessage);

					var logger = new LoggingInformation();
					var processor = new MessageProcessorFactory(logger);
					processor.ProcessMessage(acknowledgementMessage);

					var tswMessage = Factory.New<TSWMessage>();
					tswMessage.EM_MessageType = MessageTypeList.Codes.TWR;
					tswMessage.EM_MessageText = ConsolidationNZCSResponse;
					tswMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					tswMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 02, 17, 15, 36, 53);
					consolidatedDeclaration.Messages.Add(tswMessage);

					logger = new LoggingInformation();
					processor = new MessageProcessorFactory(logger);
					processor.ProcessMessage(tswMessage);

					var bioMessage = Factory.New<TSWMessage>();
					bioMessage.EM_MessageType = MessageTypeList.Codes.TWR;
					bioMessage.EM_MessageText = ConsolidationBIOResponse;
					bioMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					bioMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 02, 17, 15, 37, 23);
					consolidatedDeclaration.Messages.Add(bioMessage);

					logger = new LoggingInformation();
					processor = new MessageProcessorFactory(logger);
					processor.ProcessMessage(bioMessage);

					var mpiMessage = Factory.New<TSWMessage>();
					mpiMessage.EM_MessageType = MessageTypeList.Codes.TWR;
					mpiMessage.EM_MessageText = ConsolidationMPIResponse;
					mpiMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
					mpiMessage.EM_SystemCreateTimeUtc = new ZDateTime(2023, 02, 17, 15, 37, 43);
					consolidatedDeclaration.Messages.Add(mpiMessage);

					logger = new LoggingInformation();
					processor = new MessageProcessorFactory(logger);
					processor.ProcessMessage(mpiMessage);
				}

				var expectedDeliveryInstructions = @"20 LOOSE PACKAGE(S) OR ITEM(S)
Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.
Food risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.
Delivery Order Herewith, method of Payment as specified";
				AssertEquals("CustomsDeliveryInstructions - declaration 1", expectedDeliveryInstructions, declaration1.CustomsDeliveryInstructions);
				AssertEquals("CustomsDeliveryInstructions - subsequent declarations in the consolidation have also been updated with the delivery instructions - declaration 2", expectedDeliveryInstructions, declaration2.CustomsDeliveryInstructions);

				var emailSubject = GlbCompany.CurrentCompany.GC_Name.ToUpper() + " - DUMMY Branch 2 - {0}";
				var dOrderJobs = GetPrintJobs(dOrderQueue);
				AssertEquals("DOrderJobs.Count", 2, dOrderJobs.Count);

				var deliveryOrderPrint1 = dOrderJobs[0];
				AssertEquals("DOrderJobs[0].SP_Copies", 1, deliveryOrderPrint1.SP_Copies.ToZInt());
				AssertEquals("Auto Print EmailSubjectLine", string.Format(emailSubject, "Delivery Order - 27456780"), deliveryOrderPrint1.SP_EmailSubjectLine);
				AssertEquals("Auto Print should use Declaration's branch", otherBranch.PK, deliveryOrderPrint1.SP_GB);

				var deliveryOrderPrint2 = dOrderJobs[1];
				AssertEquals("DOrderJobs[0].SP_Copies", 1, deliveryOrderPrint2.SP_Copies.ToZInt());
				AssertEquals("Auto Print EmailSubjectLine", string.Format(emailSubject, "Delivery Order - 27456780"), deliveryOrderPrint2.SP_EmailSubjectLine);
				AssertEquals("Auto Print should use Declaration's branch", otherBranch.PK, deliveryOrderPrint2.SP_GB);
			}
		}

		#region Implementation

		void ProcessMessage(string messageText, ZDateTime messageCreateTime)
		{
			var tswMessage = Factory.New<TSWMessage>();
			tswMessage.EM_MessageType = MessageTypeList.Codes.TWR;
			tswMessage.EM_MessageText = messageText;
			tswMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			tswMessage.EM_SystemCreateTimeUtc = messageCreateTime;
			entryHeader.Messages.Add(tswMessage);

			var logger = new LoggingInformation();
			var processor = new MessageProcessorFactory(logger);
			processor.ProcessMessage(tswMessage);
		}

		JobDeclaration declaration;
		CusEntryHeader entryHeader;
		TSWMessage outgoingMessage;

		StmPrintQueue entryQueue;
		StmPrintQueue cusCertQueue;
		StmPrintQueue dOrderQueue;

		GlbBranch otherBranch;

		protected override void SetUp()
		{
			SetUpPrinterQueues();
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			declaration.JE_MasterBill = "081003498238";
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;

			entryHeader = declaration.ActiveEntryHeaders.AddNew();

			outgoingMessage = Factory.New<TSWMessage>();
			outgoingMessage.EM_MessageType = MessageTypeList.Codes.I10;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_LinkedObject = entryHeader;
			outgoingMessage.EM_LinkUniqueID = entryHeader.PK;
		}

		void SetUpPrinterQueues()
		{
			entryQueue = GetNewPrintQueue("Entry Printer");
			cusCertQueue = GetNewPrintQueue("CusCert Printer");
			dOrderQueue = GetNewPrintQueue("DOrder Printer");

			otherBranch = Factory.New<GlbBranch>();
			otherBranch.FillWithValidTestData();
			otherBranch.GB_Code = "D!2";
			otherBranch.GB_RL_NKHomePort = "NZAKL";
			otherBranch.GB_BranchName = "DUMMY Branch 2";
			otherBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			otherBranch.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;

			NZCustomsDataRegistry.Instance.CustomsCertificatePrinter.SetValue(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty, cusCertQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.CustomsCertificateCopies.SetValue(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty, 1);

			NZCustomsDataRegistry.Instance.EntryPrintPrinter.SetValue(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty, entryQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.EntryPrintCopies.SetValue(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty, 2);

			NZCustomsDataRegistry.Instance.DeliveryOrderPrinter.SetValue(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty, dOrderQueue.PK.ToGuid());
			NZCustomsDataRegistry.Instance.DeliveryOrderCopies.SetValue(Guid.Empty, otherBranch.PK.ToGuid(), Guid.Empty, 4);
			Factory.Save();
		}

		StmPrintQueue GetNewPrintQueue(ZString displayName)
		{
			var printQueue = Factory.New<StmPrintQueue>();
			printQueue.SQ_AllowPrinting = true;
			printQueue.SQ_DisplayName = displayName;
			printQueue.SQ_QueueName = @"\\PrintServer\" + displayName;
			printQueue.SQ_PrintLanguage = "ESP";
			printQueue.SQ_Scale = 100m;
			printQueue.SQ_RowScale = 100m;
			printQueue.SQ_ColumnScale = 100m;
			printQueue.SQ_ServerName = "PRINTSERVER";
			return printQueue;
		}

		StmPrintJobCollection GetPrintJobs(StmPrintQueue printQueue)
		{
			var filter = new ZQuery(StmPrintJobSchema.SP_SQ, printQueue.PK);
			var entryPrintJobs = new StmPrintJobCollection(Factory, filter);
			entryPrintJobs.Load();
			return entryPrintJobs;
		}

		#region Delivery Order Cleared Response

		const string DOResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
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
    <IssueDateTime formatCode=""204"">20160121194817</IssueDateTime>
    <FunctionalReferenceID>4524</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalDocument>
      <CategoryCode>CDO</CategoryCode>
      <ImageBinaryObject mimeCode=""application/pdf"" uri=""idd_AA3F58D4-9555-4A51-BFB8-3E466D9E4558"" filename=""IM1_Delivery_Order-48151967-2016-01-21-194832582.pdf"">Attached</ImageBinaryObject>
    </AdditionalDocument>
    <AdditionalInformation>
      <StatementDescription>1 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>48151967</ID>
        <AcceptanceDateTime formatCode=""204"">20160121194817</AcceptanceDateTime>
        <FunctionalReferenceID>B00001252</FunctionalReferenceID>
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
            <TaxAssessedAmount>283.52</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20160121194817</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20160121194817</ReleaseDateTime>
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

		#region Cleared Response

		const string ClearedResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
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
    <IssueDateTime formatCode=""204"">20160107224205</IssueDateTime>
    <FunctionalReferenceID>4453</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>56762193</ID>
        <AcceptanceDateTime formatCode=""204"">20160107224205</AcceptanceDateTime>
        <FunctionalReferenceID>B00002306</FunctionalReferenceID>
        <VersionID>2</VersionID>
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
            <TaxAssessedAmount>675.19</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20160107224205</EffectiveDateTime>
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

		#region BIO Response

		const string MPIBIOResponse = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
            <TaxAssessedAmount>441.57</TaxAssessedAmount>
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

		#endregion

		#region Consolidation End to End full message set

		const string ConsolidationACKResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20230217153613</IssueDateTime>
    <FunctionalReferenceID>10268</FunctionalReferenceID>
    <FunctionCode>12</FunctionCode>
    <OverallDeclaration>
      <Declaration>
        <ID>27456780</ID>
        <FunctionalReferenceID>CE00000007</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20230217153613</EffectiveDateTime>
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

		const string ConsolidationNZCSResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20230217153653</IssueDateTime>
    <FunctionalReferenceID>10269</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>20 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>27456780</ID>
        <AcceptanceDateTime formatCode=""204"">20230217153653</AcceptanceDateTime>
        <FunctionalReferenceID>CE00000007</FunctionalReferenceID>
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
            <TaxAssessedAmount currencyID=""NZD"">1006.28</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20230217153653</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20230217153653</ReleaseDateTime>
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

		const string ConsolidationBIOResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20230217153723</IssueDateTime>
    <FunctionalReferenceID>10270</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>27456780</ID>
        <AcceptanceDateTime formatCode=""204"">20230217153723</AcceptanceDateTime>
        <FunctionalReferenceID>CE00000007</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20230217153723</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20230217153723</ReleaseDateTime>
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

		const string ConsolidationMPIResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20230217153743</IssueDateTime>
    <FunctionalReferenceID>10271</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Food risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>27456780</ID>
        <AcceptanceDateTime formatCode=""204"">20230217153743</AcceptanceDateTime>
        <FunctionalReferenceID>CE00000007</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20230217153743</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20230217153743</ReleaseDateTime>
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

		#region B00005395

		#region NZCS Response

		const string B00005395CustomsResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20230221154645</IssueDateTime>
    <FunctionalReferenceID>10278</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>10 LOOSE PACKAGE(S) OR ITEM(S)</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>7263911</ID>
        <AcceptanceDateTime formatCode=""204"">20230221154645</AcceptanceDateTime>
        <FunctionalReferenceID>B00005395</FunctionalReferenceID>
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
            <TaxAssessedAmount currencyID=""NZD"">462.98</TaxAssessedAmount>
          </Payment>
        </DutyTaxFee>
        <ResponsibleGovernmentAgency>
          <ID>NZCS</ID>
        </ResponsibleGovernmentAgency>
      </Declaration>
    </OverallDeclaration>
    <Status>
      <EffectiveDateTime formatCode=""204"">20230221154645</EffectiveDateTime>
      <NameCode>819</NameCode>
      <ReleaseDateTime formatCode=""204"">20230221154645</ReleaseDateTime>
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

		#region Food Response

		const string B00005395FoodResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20230221154716</IssueDateTime>
    <FunctionalReferenceID>10279</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Food risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>7263911</ID>
        <AcceptanceDateTime formatCode=""204"">20230221154716</AcceptanceDateTime>
        <FunctionalReferenceID>B00005395</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20230221154716</EffectiveDateTime>
      <NameCode>F04</NameCode>
      <ReleaseDateTime formatCode=""204"">20230221154716</ReleaseDateTime>
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

		#region BIO Response

		const string B00005395BIOResponse = @"<DocumentMetadata xmlns=""urn:wco:datamodel:WCO:DM:1"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
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
    <IssueDateTime formatCode=""204"">20230221154736</IssueDateTime>
    <FunctionalReferenceID>10280</FunctionalReferenceID>
    <FunctionCode>24</FunctionCode>
    <AdditionalInformation>
      <StatementDescription>Biosecurity risk assessed by the Ministry for Primary Industries and goods cleared for entry into New Zealand.</StatementDescription>
      <StatementTypeCode>DIN</StatementTypeCode>
    </AdditionalInformation>
    <OverallDeclaration>
      <Declaration>
        <ID>7263911</ID>
        <AcceptanceDateTime formatCode=""204"">20230221154736</AcceptanceDateTime>
        <FunctionalReferenceID>B00005395</FunctionalReferenceID>
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
      <EffectiveDateTime formatCode=""204"">20230221154736</EffectiveDateTime>
      <NameCode>B04</NameCode>
      <ReleaseDateTime formatCode=""204"">20230221154736</ReleaseDateTime>
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
