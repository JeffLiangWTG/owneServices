using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.NZ.Business.Declaration.FormalEntry.CusEntryHeader;

namespace Enterprise.Customs.NZ.Business.MessageBuilders.FormalEntry.Testing
{
	using Enterprise.Customs.Business.Testing;
	using Enterprise.Customs.NZ.Registry;
	using NUnit.Framework;
	using NZ.TradeSingleWindow;

	[TestedType(typeof(MessageManager))]
	public class MessageManagerTest : MessageBuilders.Testing.MessageManagerForDeclarationTest
	{
		public override void TestExecuteReplaceRejectedEntry()
		{
			SetDeclarationToRejected();
			Declaration.DeclarationNumber = "12345678";
			MessageManagerForDeclaration manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			AssertEquals("Manager.Execute()", true, manager.Execute());
			AssertEquals("Manager.LastHumanReadableStatus", "Replacement Entry Message " + MessageManagerForDeclaration.MessageReportingImmediateSend, manager.LastHumanReadableStatus);
		}

		public void TestEverythingGetsResetProperlyOnASaveFailureOnANewJob()
		{
			JobDeclaration declaration = Declaration;
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessageManager manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			AssertEquals("Precondition: Declaration.IsInDatabase", false, declaration.IsInDatabase);
			CusEntryLineFee spannerInTheWorks = Factory.New<CusEntryLineFee>(); // Unassociated CusEntryLineFee should blow the save at the lower level Factory / DB Level...
			spannerInTheWorks.CF_ChargeAmount = -1m; // ...but it has to have HasChanges set to even TRY and save.
			AssertEquals("manager.Execute()", false, manager.Execute());

			CusEntryHeader entryHeader = EntryHeader;
			AssertEquals("entryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, entryHeader.CH_EntryStatus);
			AssertEquals("declaration.JE_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("declaration.JE_EntrySubmittedDate", ZDateTime.Empty, declaration.JE_EntrySubmittedDate);
			AssertEquals("entryHeader.Messages.Count", 0, entryHeader.Messages.Count);
			AssertEquals("declaration.IsInDatabase", false, declaration.IsInDatabase);
		}

		public void TestEverythingGetsResetProperlyOnASaveFailureOnAPreviouslySentJob()
		{
			JobDeclaration declaration = Declaration;
			AssertEquals("Precondition: Declaration.IsInDatabase", false, declaration.IsInDatabase);
			declaration.JE_EDITransmitDate = declaration.CachedTodaysDate;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessageManager manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			AssertEquals("manager.Execute()", true, manager.Execute());
			AssertEquals("manager.LastHumanReadableStatus", "Original Entry Message " + MessageManager.MessageReportingImmediateSend, manager.LastHumanReadableStatus);

			CusEntryHeader entryHeader = EntryHeader;
			AssertEquals("entryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.SentToCustoms, entryHeader.CH_EntryStatus);
			AssertEquals("declaration.JE_EntryStatus", FormalEntryStatusList.Codes.SentToCustoms, declaration.JE_EntryStatus);
			AssertEquals("declaration.JE_EntrySubmittedDate", declaration.CachedTodaysDate, declaration.JE_EntrySubmittedDate);
			AssertEquals("entryHeader.Messages.Count", 1, entryHeader.Messages.Count);
			AssertEquals("declaration.IsInDatabase", true, declaration.IsInDatabase);

			entryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryRejected;
			declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryRejected;
			ZDateTime backAFewDaysAgo = declaration.CachedTodaysDate.AddDays(-2);
			declaration.JE_EntrySubmittedDate = backAFewDaysAgo;
			Factory.Save();

			CusEntryLineFee spannerInTheWorks = Factory.New<CusEntryLineFee>(); // Unassociated CusEntryLineFee should blow the save at the lower level Factory / DB Level...
			spannerInTheWorks.CF_ChargeAmount = -1m; // ...but it has to have HasChanges set to even TRY and save.
			AssertEquals("manager.Execute()", false, manager.Execute());

			AssertEquals("entryHeader.CH_EntryStatus", FormalEntryStatusList.Codes.EntryRejected, entryHeader.CH_EntryStatus);
			AssertEquals("declaration.JE_EntryStatus", FormalEntryStatusList.Codes.EntryRejected, declaration.JE_EntryStatus);
			AssertEquals("declaration.JE_EntrySubmittedDate", backAFewDaysAgo, declaration.JE_EntrySubmittedDate);
			AssertEquals("entryHeader.Messages.Count", 1, entryHeader.Messages.Count);
		}

		public void TestRunMergeIfTransmitDateWasEmpty()
		{
			var declaration = Declaration;
			declaration.JE_EDITransmitDate = ZDateTime.Empty;
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			declaration.Invoices.DeleteAll();//error condition for merge

			var manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			AssertEquals("manager.Execute()", false, manager.Execute());
			AssertEquals(ZDateTime.Today, declaration.JE_EDITransmitDate);

			var mergeResult = ((Customs.Business.SendsMessagesToCustomsShutterUpperer)declaration.MessageInitiator).InvalidOperationText;
			AssertEquals("You can't merge this entry because there are no invoice headers.", mergeResult);
		}

		public void TestErrorCantSendWithMoreLinesThanCustomsWillAccept()
		{
			string expectedErrorMessage = MessageManager.ErrorCantSendWithMoreLinesThanCustomsWillAccept + MessageManager.MaximumEntryLinesSentToCustomsInOneEntry.ToString() + ".";

			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			Declaration.DeclarationNumber = "";
			AssertEquals("Precondition: manager.IsOkToExecute", true, manager.IsOkToExecute);
			AssertEquals("Precondition: manager.LastHumanReadableStatus", MessageManager.MessageReadyToSendMessage, manager.LastHumanReadableStatus);

			for (int count = 0; count < MessageManager.MaximumEntryLinesSentToCustomsInOneEntry + 2; count++)
			{
				Declaration.CusEntryHeader.MergedLines.AddNew();
			}
			AssertEquals("manager.IsOkToExecute with " + EntryHeader.MergedLines.Count.ToString() + " lines.", false, manager.IsOkToExecute);
			AssertEquals("manager.LastHumanReadableStatus with " + EntryHeader.MergedLines.Count.ToString() + " lines.", expectedErrorMessage, manager.LastHumanReadableStatus);

			EntryHeader.MergedLines.RemoveAndDelete(EntryHeader.MergedLines[0]);
			AssertEquals("manager.IsOkToExecute with " + EntryHeader.MergedLines.Count.ToString() + " lines.", false, manager.IsOkToExecute);
			AssertEquals("manager.LastHumanReadableStatus with " + EntryHeader.MergedLines.Count.ToString() + " lines.", expectedErrorMessage, manager.LastHumanReadableStatus);

			EntryHeader.MergedLines.RemoveAndDelete(EntryHeader.MergedLines[0]);
			AssertEquals("manager.IsOkToExecute with " + EntryHeader.MergedLines.Count.ToString() + " lines.", true, manager.IsOkToExecute);
			AssertEquals("manager.LastHumanReadableStatus with " + EntryHeader.MergedLines.Count.ToString() + " lines.", MessageManager.MessageReadyToSendMessage, manager.LastHumanReadableStatus);

			EntryHeader.MergedLines.RemoveAndDelete(EntryHeader.MergedLines[0]);
			AssertEquals("manager.IsOkToExecute with " + EntryHeader.MergedLines.Count.ToString() + " lines.", true, manager.IsOkToExecute);
			AssertEquals("manager.LastHumanReadableStatus with " + EntryHeader.MergedLines.Count.ToString() + " lines.", MessageManager.MessageReadyToSendMessage, manager.LastHumanReadableStatus);
		}

		public void TestExposedFields()
		{
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			manager.EnteredOverrideFlag = false;
			manager.EnteredPDOFlag = false;
			manager.EnteredPinNumber = "1234";
			manager.EnteredRemarks = "Why Not";
			AssertEquals("EntryHeader.CH_OverrideIndicator", false, EntryHeader.CH_OverrideIndicator);
			AssertEquals("Manager.EnteredPDOFlag", false, manager.EnteredPDOFlag);
			AssertEquals("Manager.EnteredPinNumber", "1234", manager.EnteredPinNumber);
			AssertEquals("Manager.EnteredRemarks", "Why Not", manager.EnteredRemarks);
		}

		public void TestSetPDOFlagIfCurrentSendIsAmendment()
		{
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			AssertEquals("Manager.EnteredPDOFlag - is always true for TSW", true, manager.EnteredPDOFlag);

			manager.EnteredPDOFlag = false;
			manager.EnteredPinNumber = CurrentUsersPin.TestSystemPinCode;
			AssertEquals("Manager.EnteredPDOFlag", false, manager.EnteredPDOFlag);
			manager.EnteredPinNumber = "";

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.ResponseReceived;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.ResponseReceived;
			Declaration.DeclarationNumber = "12345678";

			manager.EnteredPinNumber = CurrentUsersPin.TestSystemPinCode;
			AssertEquals("Manager.EnteredPDOFlag", true, manager.EnteredPDOFlag);
			manager.EnteredPinNumber = "";
			manager.EnteredPDOFlag = false;

			manager.EnteredPinNumber = "WRONG";
			AssertEquals("Manager.EnteredPDOFlag", false, manager.EnteredPDOFlag);
		}

		public void TestCancellingACurrentlyQueuedMessage()
		{
			ZDateTime futureEDITransmitDate = Declaration.CachedTodaysDate.AddDays(2);
			Declaration.JE_EDITransmitDate = futureEDITransmitDate;
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessageManager manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			AssertEquals(false, manager.MessageIsCurrentlyQueued);
			AssertEquals("Manager.Execute()", true, manager.Execute());
			AssertEquals("Manager.LastHumanReadableStatus", "Original Entry Message " + MessageManager.MessageReportingQueuedSend + futureEDITransmitDate.ToShortDateString(), manager.LastHumanReadableStatus);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.QueuedForSending, Declaration.JE_EntryStatus);
			AssertEquals(1, Declaration.CusEntryHeader.Messages.Count);
			NZCMessage message = Declaration.CusEntryHeader.Messages[0];
			AssertEquals(NZCMessage.Status.Queued, message.EM_Status);

			Declaration.JE_EDITransmitDate = Declaration.CachedTodaysDate;
			AssertEquals(true, manager.MessageIsCurrentlyQueued);
			AssertEquals(false, manager.IsOkToExecute);
			AssertEquals("Cannot Send Message - Message is Already Queued to be sent on: " + futureEDITransmitDate.ToShortDateString(), manager.LastHumanReadableStatus);
			AssertEquals(manager.LastHumanReadableStatus + MessageManager.CurrentlyQueuedMessageSuffixToTurnLastHumanReadableStatusIntoAnOverrideQuestion, manager.MessageIsCurrentlyQueuedShouldWeCancelQuestion);
			manager.CancelCurrentlyQueuedMessage();

			AssertEquals(NZCMessage.Status.Cancelled, message.EM_Status);
			AssertEquals(false, manager.MessageIsCurrentlyQueued);
			AssertEquals(true, manager.IsOkToExecute);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.NotSentToCustoms, Declaration.JE_EntryStatus);

			AssertEquals("Manager.Execute()", true, manager.Execute());
			AssertEquals("Manager.LastHumanReadableStatus", "Original Entry Message " + MessageManagerForDeclaration.MessageReportingImmediateSend, manager.LastHumanReadableStatus);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.SentToCustoms, Declaration.JE_EntryStatus);
			AssertEquals(2, Declaration.CusEntryHeader.Messages.Count);
		}

		public void TestBrokersDeclaration()
		{
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			AssertEquals("I " + GlbStaff.CurrentUser.GS_FullName + " (65432198B) HEREBY DECLARE THAT THE PARTICULARS CONTAINED IN THIS ELECTRONIC ENTRY "
				+ "MESSAGE ARE TRUE AND CORRECT AND ARE IN ACCORDANCE WITH THE CUSTOMS AND EXCISE ACT 1996. "
				+ "VALIDATED BY THE ENDORSEMENT OF MY ELECTRONIC SIGNATURE NUMBER.", manager.BrokersDeclaration);
		}

		public void TestValidateUnbalancedApportionment()
		{
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
			invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100m, "NZD");
			invoice.JobComInvoiceLines.AddNew();
			string message;

			AssertEquals("PreCondition:Apportionment is not balanced", false, Declaration.Invoices.AreChargesBalancedForInvoices(out message));
			AssertEquals(false, manager.IsOkToExecute);
			ZString message1 = manager.LastHumanReadableStatus;
			AssertEquals(true, message1.Contains("Current apportionment is not balanced"));
		}

		public void TestQueuedExecuteOriginalEntry()
		{
			ZDateTime eDITransmitDate = Declaration.CachedTodaysDate.AddDays(1);
			Declaration.JE_EDITransmitDate = eDITransmitDate;
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			JobComInvoiceLine invoiceLine = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_HadErrorInLastResponse = true;
			MessageManager manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			AssertEquals("Precondition: Declaration.IsInDatabase", false, Declaration.IsInDatabase);
			Assert(manager.Execute());
			AssertEquals("Manager.LastHumanReadableStatus", "Original Entry Message " + MessageManager.MessageReportingQueuedSend + eDITransmitDate.ToShortDateString(), manager.LastHumanReadableStatus);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.QueuedForSending, Declaration.JE_EntryStatus);
			AssertEquals("invoiceLine.JI_HadErrorInLastResponse", false, invoiceLine.JI_HadErrorInLastResponse);
			AssertEquals("Declaration.IsInDatabase", true, Declaration.IsInDatabase);
		}

		public void TestQueuedExecuteOriginalEntrySendsTSWMessage()
		{
			var eDITransmitDate = Declaration.CachedTodaysDate.AddDays(1);
			Declaration.JE_EDITransmitDate = eDITransmitDate;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_PaymentMethod = "CPB";
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var invoiceLine = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_HadErrorInLastResponse = true;
			var manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			AssertEquals("Precondition: Declaration.IsInDatabase", false, Declaration.IsInDatabase);
			Assert(manager.Execute());
			AssertEquals("Manager.LastHumanReadableStatus", "Original Entry Message " + MessageManager.MessageReportingQueuedSend + eDITransmitDate.ToShortDateString(), manager.LastHumanReadableStatus);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.QueuedForSending, Declaration.JE_EntryStatus);
			AssertEquals("invoiceLine.JI_HadErrorInLastResponse", false, invoiceLine.JI_HadErrorInLastResponse);
			AssertEquals("Declaration.IsInDatabase", true, Declaration.IsInDatabase);
			var entryHeader = Declaration.CusEntryHeader;
			var messageCreated = entryHeader.Messages[0];
			ZString expectedMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<DocumentMetadata xmlns=\"urn:wco:datamodel:WCO:DM:1\">\n<WCODataModelVersion>3.2</WCODataModelVersion>\n<WCODocumentName>EX</WCODocumentName>\n<CountryCode>NZ</CountryCode>\n<AgencyAssignedCustomizedDocumentName>EX1</AgencyAssignedCustomizedDocumentName>\n<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>\n<Declaration xmlns=\"urn:wco:datamodel:WCO:DeclarationModel:1\">\r\n  <TypeCode>E40</TypeCode>\r\n  <FunctionalReferenceID>B00001000</FunctionalReferenceID>\r\n  <FunctionCode>9</FunctionCode>\r\n  <TotalGrossMassMeasure unitCode=\"KGM\">0</TotalGrossMassMeasure>\r\n  <Submitter>\r\n    <ID>00009917B</ID>\r\n  </Submitter>\r\n  <Agent>\r\n    <ID>00009917B</ID>\r\n    <RoleCode>CB</RoleCode>\r\n  </Agent>\r\n  <BorderTransportMeans>\r\n    <TypeCode />\r\n  </BorderTransportMeans>\r\n  <Declarant>\r\n    <ID />\r\n  </Declarant>\r\n  <Exporter>\r\n    <ID />\r\n  </Exporter>\r\n  <GoodsShipment>\r\n    <ExitDateTime formatCode=\"102\" />\r\n    <TransactionNatureCode>10</TransactionNatureCode>\r\n    <Consignment>\r\n      <LoadingLocation>\r\n        <ID />\r\n      </LoadingLocation>\r\n      <UnloadingLocation>\r\n        <ID />\r\n      </UnloadingLocation>\r\n    </Consignment>\r\n    <Importer />\r\n  </GoodsShipment>\r\n</Declaration>\n</DocumentMetadata>";
			AssertEquals("When TSW is active & is a TSW Dec, message generated/sent should be xml message to TSW system", expectedMessage, messageCreated.EM_MessageText);
		}

		public void TestQueuedExecuteReplaceRejectedEntry()
		{
			ZDateTime eDITransmitDate = Declaration.CachedTodaysDate.AddDays(1);
			Declaration.JE_EDITransmitDate = eDITransmitDate;
			SetDeclarationToRejected();
			Declaration.DeclarationNumber = "12345678";
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			Assert(manager.Execute());
			AssertEquals("Manager.LastHumanReadableStatus", "Replacement Entry Message " + MessageManager.MessageReportingQueuedSend + eDITransmitDate.ToShortDateString(), manager.LastHumanReadableStatus);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.QueuedForSending, Declaration.JE_EntryStatus);
		}

		public void TestQueuedExecuteReplaceHeaderAndLines()
		{
			ZDateTime eDITransmitDate = Declaration.CachedTodaysDate.AddDays(1);
			Declaration.JE_EDITransmitDate = eDITransmitDate;
			SetDeclarationToClearanceOK();
			Declaration.DeclarationNumber = "12345678";
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			Assert(manager.Execute());
			AssertEquals("Manager.LastHumanReadableStatus", "Replacement Entry Message " + MessageManager.MessageReportingQueuedSend + eDITransmitDate.ToShortDateString(), manager.LastHumanReadableStatus);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.QueuedForSending, Declaration.JE_EntryStatus);
		}

		public void TestQueuedExecuteCancelEntry()
		{
			ZDateTime eDITransmitDate = Declaration.CachedTodaysDate.AddDays(1);
			Declaration.JE_EDITransmitDate = eDITransmitDate;
			SetDeclarationToClearanceOK();
			Declaration.DeclarationNumber = "12345678";
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessageManager manager = GetNewManager(MessageManager.OperationType.CancelMessage);
			Assert(manager.Execute());
			AssertEquals("Manager.LastHumanReadableStatus", "Cancel Entry Message " + MessageManager.MessageReportingQueuedSend + eDITransmitDate.ToShortDateString(), manager.LastHumanReadableStatus);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.QueuedForSending, Declaration.JE_EntryStatus);
		}

		public void TestMessageTypeToBeSentDOToRHL()
		{
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			Declaration.DeclarationNumber = "12345678";
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			AssertEquals(MessageManager.MessageType.Replacement, manager.MessageTypeToBeSent);
		}

		public void TestMessageStatusResponsePending()
		{
			var manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.AgencyResponsePending;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.AgencyResponsePending;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.DeclarationNumber = "";
			AssertEquals(false, manager.IsOkToExecute);
			string expectedError = @"Cannot Send Message - Job is not in a state that is valid for sending:
This entry is currently waiting for a response from the TSW/Customs system.";
			AssertEquals("Manager.LastHumanReadableStatus", expectedError, manager.LastHumanReadableStatus);
		}

		public void TestCreditAdviceStatusAllowsReplacmentMessageBeSent()
		{
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			Declaration.DeclarationNumber = "12345678";
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.CreditAdvice;
			Declaration.CusEntryHeader.CH_EntryStatus = StatusList.Codes.RefundApprovedAmountAsSpecified;
			AssertEquals("Job with 'Credit Advice' status should be able to send a replacement message", MessageManager.MessageType.Replacement, manager.MessageTypeToBeSent);
		}

		public void TestMessageTypeToBeSentCompletionEntryFirstSend()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			Declaration.JE_OriginalEntryNumber = "01020304";
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();

			MessageManager manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			AssertEquals("Manager.Execute()", true, manager.Execute());
			AssertEquals("Manager.LastHumanReadableStatus", "'Original Completion Entry' Message " + MessageManagerForDeclaration.MessageReportingImmediateSend, manager.LastHumanReadableStatus);

			var message = Declaration.CusEntryHeader.Messages[0];
			AssertEquals(MsgTransportList.Codes.TSW, message.MsgTransMode);
		}

		public void TestMessageTypeToBeSentCompletionEntryReplacement()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Sight;
			Declaration.DeclarationNumber = "12345678";
			SetDeclarationToClearanceOK();

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			MessageManager manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			AssertEquals("Manager.Execute()", true, manager.Execute());
			AssertEquals("Manager.LastHumanReadableStatus", "'Original Completion Entry' Message " + MessageManagerForDeclaration.MessageReportingImmediateSend, manager.LastHumanReadableStatus);

			var message = Declaration.CusEntryHeader.Messages[0];
			AssertEquals(MsgTransportList.Codes.TSW, message.MsgTransMode);
		}

		[TestDate(2014, 03, 25)]
		public void TestExportCompletionEntry()
		{
			var eDITransmitDate = Declaration.CachedTodaysDate.AddDays(1);
			Declaration.JE_EDITransmitDate = eDITransmitDate;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Sight;
			Declaration.JE_PaymentMethod = "CPB";
			Declaration.DeclarationNumber = "12345678";
			SetDeclarationToClearanceOK();

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var invoiceLine = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_HadErrorInLastResponse = true;
			var packaging1 = Declaration.InvoiceLines[0].ItemPackages.AddNew();
			packaging1.NZ_NumberOfPackages = 21;
			packaging1.NZ_PackageUQ = "PK";
			packaging1.NZ_PackageVolume = 2m;
			packaging1.NZ_ShippingMarks = "DJC";
			var packaging2 = Declaration.InvoiceLines[1].ItemPackages.AddNew();
			packaging2.NZ_NumberOfPackages = 12;
			packaging2.NZ_PackageUQ = "BX";
			packaging2.NZ_PackageVolume = 1m;
			packaging2.NZ_ShippingMarks = Core.Constants.ContainerMarking.NoMarks;

			var manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			AssertEquals("Precondition: Declaration.IsInDatabase", false, Declaration.IsInDatabase);
			Assert(manager.Execute());
			AssertEquals("Manager.LastHumanReadableStatus", "'Original Completion Entry' Message Generated and Ready to be sent by Service Tasks.", manager.LastHumanReadableStatus);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.SentToCustoms, Declaration.JE_EntryStatus);
			AssertEquals("invoiceLine.JI_HadErrorInLastResponse", false, invoiceLine.JI_HadErrorInLastResponse);
			AssertEquals("Declaration.IsInDatabase", true, Declaration.IsInDatabase);
			var entryHeader = Declaration.CusEntryHeader;
			var messageCreated = entryHeader.Messages[0];
			ZString expectedMessage = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<DocumentMetadata xmlns=\"urn:wco:datamodel:WCO:DM:1\">\n<WCODataModelVersion>3.2</WCODataModelVersion>\n<WCODocumentName>EX</WCODocumentName>\n<CountryCode>NZ</CountryCode>\n<AgencyAssignedCustomizedDocumentName>EX1</AgencyAssignedCustomizedDocumentName>\n<AgencyAssignedCustomizedDocumentVersion>V1.1</AgencyAssignedCustomizedDocumentVersion>\n<Declaration xmlns=\"urn:wco:datamodel:WCO:DeclarationModel:1\">\r\n  <TypeCode>E40</TypeCode>\r\n  <FunctionalReferenceID>B00001000C</FunctionalReferenceID>\r\n  <FunctionCode>22</FunctionCode>\r\n  <TotalGrossMassMeasure unitCode=\"KGM\">0</TotalGrossMassMeasure>\r\n  <Submitter>\r\n    <ID>00009917B</ID>\r\n  </Submitter>\r\n  <Agent>\r\n    <ID>00009917B</ID>\r\n    <RoleCode>CB</RoleCode>\r\n  </Agent>\r\n  <BorderTransportMeans>\r\n    <TypeCode />\r\n  </BorderTransportMeans>\r\n  <CurrencyExchange>\r\n    <RateNumeric>1</RateNumeric>\r\n    <CurrencyTypeCode>NZD</CurrencyTypeCode>\r\n  </CurrencyExchange>\r\n  <Declarant>\r\n    <ID />\r\n  </Declarant>\r\n  <Exporter>\r\n    <ID />\r\n  </Exporter>\r\n  <GoodsShipment>\r\n    <ExitDateTime formatCode=\"102\" />\r\n    <TransactionNatureCode>10</TransactionNatureCode>\r\n    <Consignment>\r\n      <LoadingLocation>\r\n        <ID />\r\n      </LoadingLocation>\r\n      <UnloadingLocation>\r\n        <ID />\r\n      </UnloadingLocation>\r\n    </Consignment>\r\n    <GovernmentAgencyGoodsItem>\r\n      <SequenceNumeric>1</SequenceNumeric>\r\n      <Commodity>\r\n        <Description />\r\n        <ValueAmount currencyID=\"NZD\">0</ValueAmount>\r\n      </Commodity>\r\n      <GoodsMeasure>\r\n        <GrossMassMeasure unitCode=\"KGM\">0</GrossMassMeasure>\r\n        <NetNetWeightMeasure unitCode=\"KGM\">0</NetNetWeightMeasure>\r\n      </GoodsMeasure>\r\n      <Origin>\r\n        <CountryCode />\r\n      </Origin>\r\n      <Packaging>\r\n        <SequenceNumeric>1</SequenceNumeric>\r\n        <MarksNumbersID>DJC</MarksNumbersID>\r\n        <QuantityQuantity>21</QuantityQuantity>\r\n        <TypeCode>PK</TypeCode>\r\n        <VolumeMeasure unitCode=\"MTQ\">2</VolumeMeasure>\r\n      </Packaging>\r\n      <Packaging>\r\n        <SequenceNumeric>2</SequenceNumeric>\r\n        <MarksNumbersID>N/M</MarksNumbersID>\r\n        <QuantityQuantity>12</QuantityQuantity>\r\n        <TypeCode>BX</TypeCode>\r\n        <VolumeMeasure unitCode=\"MTQ\">1</VolumeMeasure>\r\n      </Packaging>\r\n    </GovernmentAgencyGoodsItem>\r\n    <Importer />\r\n    <Invoice>\r\n      <IssueDateTime formatCode=\"102\">20140325</IssueDateTime>\r\n      <ID />\r\n      <SequenceNumeric>1</SequenceNumeric>\r\n    </Invoice>\r\n    <Invoice>\r\n      <IssueDateTime formatCode=\"102\">20140325</IssueDateTime>\r\n      <ID />\r\n      <SequenceNumeric>2</SequenceNumeric>\r\n    </Invoice>\r\n  </GoodsShipment>\r\n  <PreviousDocument>\r\n    <ID>12345678</ID>\r\n    <TypeCode>I51</TypeCode>\r\n  </PreviousDocument>\r\n</Declaration>\n</DocumentMetadata>";
			AssertEquals("TSW completion message expected", expectedMessage, messageCreated.EM_MessageText);
		}

		public void TestMessageTypeToBeSentManualEntryToNone()
		{
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			Declaration.DeclarationNumber = "";
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.ManualEntryCannotBeSentToCustoms;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.ManualEntryCannotBeSentToCustoms;
			AssertEquals(MessageManager.MessageType.None, manager.MessageTypeToBeSent);
		}

		public void TestMessageTypeToBeSentQueuedToNone()
		{
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			Declaration.DeclarationNumber = "";
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.QueuedForSending;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.QueuedForSending;
			AssertEquals(MessageManager.MessageType.None, manager.MessageTypeToBeSent);
		}

		public void TestIsOkToExecuteCannotSendManual()
		{
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.ManualEntryCannotBeSentToCustoms;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.ManualEntryCannotBeSentToCustoms;
			Declaration.DeclarationNumber = "";
			AssertEquals(false, manager.IsOkToExecute);
			AssertEquals("Cannot Send Message - Job is a 'Manual Entry'", manager.LastHumanReadableStatus);
		}

		public void TestIsOkToExecuteCannotSendQueued()
		{
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.QueuedForSending;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.QueuedForSending;
			Declaration.DeclarationNumber = "";
			AssertEquals(false, manager.IsOkToExecute);
			AssertEquals("Cannot Send Message - Message is Already Queued to be sent on: " + Declaration.CachedTodaysDate.ToShortDateString(), manager.LastHumanReadableStatus);
		}

		public void TestGetErrorsForEnteredValues()
		{
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.ResponseReceived;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.ResponseReceived;
			Declaration.DeclarationNumber = "12345678";
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);

			manager.EnteredPinNumber = CurrentUsersPin.TestSystemPinCode;
			manager.EnteredOverrideFlag = true;
			manager.EnteredRemarks = "You Gotta Laugh";
			ZString result = manager.GetErrorsForEnteredValues();
			AssertEquals("Result should be empty", "", result);
			AssertEquals("Override should be set", true, EntryHeader.CH_OverrideIndicator);
		}

		public void TestGetErrorsForEnteredValuesMessageDoesNotContainPIN()
		{
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.ResponseReceived;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.ResponseReceived;
			Declaration.DeclarationNumber = "12345678";
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);

			manager.EnteredPinNumber = "";
			manager.EnteredOverrideFlag = true;
			manager.EnteredRemarks = "You Gotta Laugh";
			ZString result = manager.GetErrorsForEnteredValues();
			Assert("Message should not contain PIN from Master File.", !result.Contains(CurrentUsersPin.TestSystemPinCode));
		}

		[TestDate(2005, 12, 5)]
		public void TestGetErrorTextForEDITransmitDateCannotBeInThePast()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			EntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			EntryHeader.CH_EDITransmitDate = new ZDateTime(2005, 12, 4);
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);

			AssertEquals("Manager.IsOkToExecute", false, manager.IsOkToExecute);
			AssertEquals(MessageManager.ErrorEDITransmitDateCannotBeInThePast, manager.LastHumanReadableStatus);

			EntryHeader.CH_EDITransmitDate = new ZDateTime(2005, 12, 5);

			AssertEquals("Manager.IsOkToExecute", true, manager.IsOkToExecute);
			AssertEquals(MessageManager.MessageReadyToSendMessage, manager.LastHumanReadableStatus);

			EntryHeader.CH_EDITransmitDate = new ZDateTime(2005, 12, 6);

			AssertEquals("Manager.IsOkToExecute", true, manager.IsOkToExecute);
			AssertEquals(MessageManager.MessageReadyToSendMessage, manager.LastHumanReadableStatus);

			EntryHeader.CH_EDITransmitDate = new ZDateTime(2005, 12, 4);
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			EntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			EntryHeader.EntryNumber = "64321879";

			AssertEquals("Manager.IsOkToExecute", true, manager.IsOkToExecute);
			AssertEquals(MessageManager.MessageReadyToSendMessage, manager.LastHumanReadableStatus);

			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			EntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			EntryHeader.EntryNumber = "";

			AssertEquals("Manager.IsOkToExecute", false, manager.IsOkToExecute);
			AssertEquals(MessageManager.ErrorEDITransmitDateCannotBeInThePast, manager.LastHumanReadableStatus);
		}

		public void TestEDITransmitDateInThePastExportJobs()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			EntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			EntryHeader.CH_EDITransmitDate = ZDateTime.Today.AddDays(-10);
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);

			AssertEquals("Manager.IsOkToExecute", false, manager.IsOkToExecute);
			AssertEquals(MessageManager.ExportEDITransmitDateInThePast, manager.LastHumanReadableStatus);

			EntryHeader.CH_EDITransmitDate = ZDateTime.Today;
			AssertEquals("Manager.IsOkToExecute", true, manager.IsOkToExecute);
			AssertEquals(MessageManager.MessageReadyToSendMessage, manager.LastHumanReadableStatus);
		}

		#region BrokerIDAndPIN

		public void TestEmptyBrokerID()
		{
			using (NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				CurrentUserWrapper.NZBPassword.GP_UserID = "";
				new CurrentUsersPin(Factory, false).DecryptedPinCode = "123";

				ConfigureDeclarationForTSWImport();
				AssertBrokerIdAndPIN("EmptyBrokerID ", MessageSubTypeCombinedList.Codes.Normal, okToExecute: false, MessageManager.ErrorMustEnterBrokerIDAndPINAgainstStaffRecord);
				AssertBrokerIdAndPIN("EmptyBrokerID ", MessageSubTypeCombinedList.Codes.IPI, okToExecute: true, MessageManager.MessageReadyToSendMessage);
			}
		}

		public void TestEmptyBrokerPIN()
		{
			using (NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				CurrentUserWrapper.NZBPassword.GP_UserID = "ABC";
				new CurrentUsersPin(Factory, false).DecryptedPinCode = "";

				ConfigureDeclarationForTSWImport();
				AssertBrokerIdAndPIN("EmptyBrokerPIN ", MessageSubTypeCombinedList.Codes.Normal, okToExecute: false, MessageManager.ErrorMustEnterBrokerIDAndPINAgainstStaffRecord);
				AssertBrokerIdAndPIN("EmptyBrokerPIN ", MessageSubTypeCombinedList.Codes.IPI, okToExecute: true, MessageManager.MessageReadyToSendMessage);
			}
		}

		public void TestBrokerIDAndPIN()
		{
			using (NZCustomsDataRegistry.Instance.ImportDeclarationsTestMode.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				CurrentUserWrapper.NZBPassword.GP_UserID = "ABC";
				new CurrentUsersPin(Factory, false).DecryptedPinCode = "123";

				ConfigureDeclarationForTSWImport();
				AssertBrokerIdAndPIN("BrokerIDAndPIN ", MessageSubTypeCombinedList.Codes.Normal, okToExecute: true, MessageManager.MessageReadyToSendMessage);
				AssertBrokerIdAndPIN("BrokerIDAndPIN ", MessageSubTypeCombinedList.Codes.IPI, okToExecute: true, MessageManager.MessageReadyToSendMessage);
			}
		}

		void ConfigureDeclarationForTSWImport()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
		}

		void AssertBrokerIdAndPIN(ZString testName, ZString messageSubType, bool okToExecute, ZString expectedStatus)
		{
			Declaration.JE_MessageSubType = messageSubType;
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			AssertEquals(testName + messageSubType + ": Manager.IsOkToExecute", okToExecute, manager.IsOkToExecute);
			AssertEquals(testName + messageSubType, expectedStatus, manager.LastHumanReadableStatus);
		}

		#endregion

		public void TestQueuedExecuteOriginalTSWEntry()
		{
			var eDITransmitDate = Declaration.CachedTodaysDate.AddDays(1);
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_EDITransmitDate = eDITransmitDate;
			Declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			JobComInvoiceLine invoiceLine = Declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_HadErrorInLastResponse = true;
			MessageManager manager = GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
			AssertEquals("Precondition: Declaration.IsInDatabase", false, Declaration.IsInDatabase);
			Assert(manager.Execute());
			AssertEquals("Manager.LastHumanReadableStatus", "Original Entry Message " + MessageManager.MessageReportingQueuedSend + eDITransmitDate.ToShortDateString(), manager.LastHumanReadableStatus);
			AssertEquals("Declaration.JE_EntryStatus", FormalEntryStatusList.Codes.QueuedForSending, Declaration.JE_EntryStatus);
			AssertEquals("invoiceLine.JI_HadErrorInLastResponse", false, invoiceLine.JI_HadErrorInLastResponse);
			AssertEquals("Declaration.IsInDatabase", true, Declaration.IsInDatabase);
			AssertEquals("Entry Message should have been created", 1, Declaration.CustomsEntryHeaders[0].Messages.Count);
			AssertEquals("FOR TSW Messages going via eHub, Status should still be set to Queued", "QUE", Declaration.CustomsEntryHeaders[0].Messages[0].EM_Status);
		}

		public void TestEntryHeaderForCompletion()
		{
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			MessageManager manager = GetNewManager(MessageManager.OperationType.SubmitMessage);
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.NotSentToCustoms;
			Declaration.DeclarationNumber = "";
			CusEntryHeader entryHeader = manager.EntryHeader;
			AssertEquals("entryHeader should return Completion Entry Header", Enterprise.Customs.Business.CusEntryHeader.EntryHeaderTypes.NZ.FormalEntry, entryHeader.CH_MessageType);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			entryHeader = manager.EntryHeader;
			AssertEquals("entryHeader should return Completion Entry Header", Enterprise.Customs.Business.CusEntryHeader.EntryHeaderTypes.NZ.Completion, entryHeader.CH_MessageType);
		}

		public void TestIsOKToSendForConsolidatedDeclaration()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 2);
			Factory.Save();
			var messageInitiator = new Customs.Business.SendsMessagesToCustomsReturningResultsAsProperties(false);
			consolidatedDeclaration.LeadDeclaration.MessageInitiator = messageInitiator;
			consolidatedDeclaration.LeadDeclaration.JE_DeclarationReference = "Original Declaration";
			MessageManagerForDeclaration manager = new MessageManager(consolidatedDeclaration.BuildAggregateJobDeclaration() as JobDeclaration, MessageBuilders.MessageManager.OperationType.SubmitMessage);
			CombineAssertions(() =>
			{
				AssertEquals("Should be errors", false, manager.IsOKToSendWithMessagingErrors());
				AssertContains("Errors should contain original declaration notifications", "Original Declaration", messageInitiator.MergeResult);
			});
		}

		#region Implementation
		protected override string ExpectedAlphabeticallyOrderedMessageErrors
		{
			get
			{
				return @"Message Error - JE_ExportDate: You have not entered a Date of Export.
Message Error - JE_OH_Supplier: You have not entered a Supplier.
Message Error - JE_RL_NKFinalDestination: You have not entered a Port Of Final Destination.
Message Error - JE_RL_NKOrigin: Must be transmitted to state the Country/Region of export of the shipment.
Message Error - JE_RL_NKPortOfArrival: You have not entered a Port Of Arrival.
Message Error - JE_RL_NKPortOfLoading: You have not entered a Port Of Loading.
Message Error - JE_SoldOrConsigned: You must enter the Terms of Sale for Export Customs Entries.
Message Error - JE_TotalWeight: Weight should be greater than zero.
Message Error - JE_TransportMode: You have not entered a Mode of Transportation.";
			}
		}

		protected override string ExpectedAlphabeticallyOrderedMessageErrorsForTSW
		{
			get
			{
				return @"Message Error - JE_ExportDate: You have not entered a Date of Export.
Message Error - JE_OH_Supplier: You have not entered a Supplier.
Message Error - JE_RL_NKFinalDestination: You have not entered a Port Of Final Destination.
Message Error - JE_RL_NKOrigin: Must be transmitted to state the Country/Region of export of the shipment.
Message Error - JE_RL_NKPortOfArrival: You have not entered a Port Of Arrival.
Message Error - JE_RL_NKPortOfLoading: You have not entered a Port Of Loading.
Message Error - JE_TotalWeight: Weight should be greater than zero.
Message Error - JE_TransportMode: You have not entered a Mode of Transportation.";
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewManager(MessageManager.OperationType.SubmitMessage);
		}

		protected override MessageBuilders.MessageManager GetNewMessageManager(MessageBuilders.MessageManager.OperationType operationType)
		{
			return new MessageManager(Declaration, operationType);
		}

		MessageManager GetNewManager(MessageBuilders.MessageManager.OperationType operationType)
		{
			return (MessageManager)GetNewMessageManager(operationType);
		}

		protected CusEntryHeader EntryHeader
		{
			get
			{
				if (fEntryHeader == null)
				{
					fEntryHeader = (CusEntryHeader)Declaration.CusEntryHeader;
				}
				return fEntryHeader;
			}
		}
		CusEntryHeader fEntryHeader;

		protected override JobDeclaration GetNewJobDeclaration()
		{
			JobDeclaration declaration = Enterprise.Customs.NZ.Business.Declaration.JobDeclaration.New(Factory);
			declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			return declaration;
		}

		protected GlbStaff CurrentUser
		{
			get
			{
				if (fCurrentUser == null)
				{
					fCurrentUser = Factory.Load<GlbStaff>(Env.CurrentUser.PK);
				}
				return fCurrentUser;
			}
		}
		GlbStaff fCurrentUser;

		protected Enterprise.MasterFiles.Integration.Customs.NZ.INZGlbStaffWrapper CurrentUserWrapper => currentUserWrapper ?? (currentUserWrapper = CurrentUser.GetNZWrapper());
		Enterprise.MasterFiles.Integration.Customs.NZ.INZGlbStaffWrapper currentUserWrapper;

		protected override void SetDeclarationToSentToCustoms()
		{
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.SentToCustoms;
		}

		protected override void SetDeclarationToClearanceOK()
		{
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
		}

		protected override void SetDeclarationToRejected()
		{
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryRejected;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryRejected;
		}

		protected override void SetDeclarationToInspectionsAuditRequirements()
		{
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.InspectionsAuditRequirements;
		}

		protected override void SetDeclarationToEntryInError()
		{
			Declaration.JE_EntryStatus = FormalEntryStatusList.Codes.EntryInError;
			Declaration.CusEntryHeader.CH_EntryStatus = FormalEntryStatusList.Codes.EntryInError;
		}

		protected override MessageManagerForClearance GetManagerWithValidParent()
		{
			new CurrentUsersPin(Factory, false).DecryptedPinCode = "123";
			return GetNewManager(MessageManagerForDeclaration.OperationType.SubmitMessage);
		}

		protected override MessageBuilders.MessageManager.MessageType MessageTypeForCompleteReplacement
		{
			get { return MessageManager.MessageType.Replacement; }
		}

		#endregion
	}
}
