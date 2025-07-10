using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.US;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(ImportMessageSendingActionCollection))]
	sealed class ImportMessageSendingActionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ImportMessageSendingActionCollection>
	{
		public void TestSendeBondRequest()
		{
			var declaration = DeclarationTestHelper.GetDeclarationForeBond(Factory);
			var entryHeader = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("SQMZ");
			DeclarationTestHelper.SetBRecordOfficeCode("HZ");
			var wrapperPks = declaration.InBondRelatedRecords.Cast<MessageActionRelatedRecordWrapper>().Select(c => ((BusinessObject)c.relatedRecord).PK);
			AssertCollectionNotContains("Precondition", entryHeader.PK, wrapperPks);
			AssertEquals("Precondition", ZString.Empty, declaration.US_InsuranceDisposition);
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.EBondRequest, header => header.IsFormalEntry);
			actions.SelectAll();
			AssertEquals(actions.Count, 1);
			AssertEquals("Should create EBondMessageSendingAction for EBondRequest.", typeof(EBondMessageSendingAction), actions[0].GetType());
			var result = actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			AssertEquals("Message generated for eBond.", true, result);
			AssertEquals("US_InsuranceDisposition", InsuranceDispositionCodeList.Codes.SentToSurety, declaration.US_InsuranceDisposition);
			var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, entryHeader.PK);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.USeBond);
			var message = Factory.LoadTop1<Enterprise.Messaging.Business.EDIMessage>(query);
			AssertNotNull("Should create a message for eBond.", message);
			AssertEquals("Should create a message for eBond.", ApplicationCodeList.Codes.USeBond, message.EM_ApplicationCode);
			AssertEquals("Should create a message for eBond.", ApplicationCodeList.Codes.USeBond, message.Interchange.EI_ApplicationCode);
			wrapperPks = declaration.InBondRelatedRecords.Cast<MessageActionRelatedRecordWrapper>().Select(c => ((BusinessObject)c.relatedRecord).PK);
			AssertCollectionContains("Should rebuild the InBondRelatedRecords.", entryHeader.PK, wrapperPks);
		}

		public void TestLogCustomsCommencedEventForACECargoRelease()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = false;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			actions.SelectAll();
			AssertEquals(actions.Count, 1);
			Assert(actions[0].IsACECargoRelease);
			StmALog[] logs = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCommenced.Code));
			AssertEquals("Pre-assertion - customs not yet commenced", 0, logs.Length);
			bool messagesGenerated = actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Message generated for SE entry", true, messagesGenerated);
			logs = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCommenced.Code));
			var log = logs[0];
			AssertNotNull("declaration Log", log);
			AssertEquals("Most Recent Customs Commenced Log", log.SL_EventTime.Date, ZDateTime.Now.Date);
		}

		public void TestSEEntryWhen3461NotTickedDoesNotResultInMessageSendingAction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("PreCondition", 2, declaration.ActiveEntryHeaders.Count);
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("As users have not ticked 3461, users do not intend to send SE messages.", 1, actions.Count);
			Assert(actions[0].IsEntrySummary);
			var crlEntry = declaration.ActiveEntryHeaders.SimplifiedEntry;
			crlEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			declaration.ActiveEntryHeaders.EntrySummaryEntry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			AssertEquals("As SE has been added successfully, users should be able to send Update/Replace message", 2, actions.Count);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = true;
			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("PreCondition", 2, declaration.ActiveEntryHeaders.Count);
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("As users have ticked 3461, users do intend to send SE messages.", 2, actions.Count);
		}

		public void TestCargoReleaseActionWhenEntrySummaryIsOriginalSendable()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableCRL = true;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			CusEntryHeader crlEntry = declaration.CustomsEntryHeaders.AddNew();
			crlEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.CargoRelease;
			crlEntry.CH_Status = ImportMessageStatusList.Codes.ClearCargoReleaseOriginal;
			AssertEquals("HasBeenLodgedAtCustoms", true, crlEntry.HasBeenLodgedAtCustoms);
			ImportMessageSendingActionCollection originalActions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("there should be only one action", 1, originalActions.Count);
			AssertEquals("original action for ens entry", ensEntry, ((EntryHeaderMessageSendingAction)originalActions[0]).entry);
		}

		public void TestGenerateEntrySubmittedDate()
		{
			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XDG";
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration.US_EntryDate = new ZDateTime(2012, 1, 3);
			declaration.US_PaymentDueDate = ZDateTime.Empty;
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			ImportMessageSendingActionCollection actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			bool messagesGenerated = actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Messages generated for ens entry", true, messagesGenerated);
			AssertEquals(1, entry.Messages.Count);
			AssertEquals(ZDateTime.Now.Year, entry.CH_EntrySubmittedDate.Year);
			AssertEquals(ZDateTime.Now.Month, entry.CH_EntrySubmittedDate.Month);
			AssertEquals(ZDateTime.Now.Day, entry.CH_EntrySubmittedDate.Day);
			Assert(declaration.JE_EntrySubmittedDate.IsValid);
			Assert(declaration.US_PaymentDueDate.IsValid);
		}

		public void TestDoNotGenerateEntrySubmittedDateWhenExists()
		{
			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XDG";
			declaration.JE_EntrySubmittedDate = ZDateTime.BrettsBirthday;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_EntrySubmittedDate = ZDateTime.BrettsBirthday;
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			ImportMessageSendingActionCollection actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			bool messagesGenerated = actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Messages generated for ens entry", true, messagesGenerated);
			AssertEquals(1, entry.Messages.Count);
			AssertEquals(ZDateTime.BrettsBirthday, declaration.JE_EntrySubmittedDate);
			AssertEquals(ZDateTime.BrettsBirthday, entry.CH_EntrySubmittedDate);
		}

		public void TestDefaultUS_SendMessageIfThereIsOneElementPopulated()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			CusEntryHeader inbEntry = declaration.CustomsEntryHeaders.AddNew();
			inbEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = ensEntry.MergedLines.AddNew().PK;
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(inbEntry.MergedLines.AddNew());
			ImportMessageSendingActionCollection actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.ExtendTIB);
			AssertEquals("There should be only one element", 1, actions.Count);
			AssertEquals(ensEntry, ((EntryHeaderMessageSendingAction)actions[0]).entry);
			AssertEquals("US_SendMessage is defaulted to true", true, actions[0].US_SendMessage);
		}

		public void TestGenerateRequestToExtendTIB()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			CusEntryHeader inbEntry = declaration.CustomsEntryHeaders.AddNew();
			inbEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = ensEntry.MergedLines.AddNew().PK;
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(inbEntry.MergedLines.AddNew());
			ImportMessageSendingActionCollection actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.ExtendTIB);
			bool messagesGenerated = actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Messages generated", false, messagesGenerated);
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.ExtendTIB);
			messagesGenerated = actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Messages generated for ens entry", true, messagesGenerated);
			AssertEquals(1, ensEntry.Messages.Count);
			AssertEquals(0, inbEntry.Messages.Count);
			AssertEquals(EM_MessageSubTypeList.Codes.TemporaryImportationBondRequestToExtend, ensEntry.Messages[0].EM_MessageSubType);
		}

		public void TestHasAtLeastOneToSendMessageFor()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableINB = true;
			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			CusEntryHeader inbEntry = declaration.CustomsEntryHeaders.AddNew();
			inbEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			ImportMessageSendingActionCollection coll = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("HasAtLeastOneToSendMessageFor", false, coll.HasAtLeastOneToSendMessageFor);
			ImportMessageSendingAction inbAction = coll.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.InBondDeparture);
			AssertNotNull(inbAction);
			inbAction.US_SendMessage = true;
			AssertEquals("HasAtLeastOneToSendMessageFor", true, coll.HasAtLeastOneToSendMessageFor);
		}

		public void TestSendMessagesWithoutSavingSendsOnlyForEntriesThatAreSelected()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableINB = true;
			CusEntryHeader ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			CusEntryHeader inbEntry = declaration.CustomsEntryHeaders.AddNew();
			inbEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			ImportMessageSendingActionCollection coll = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			EntryHeaderMessageSendingAction inbAction = coll.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.InBondDeparture);
			AssertNotNull(inbAction);
			inbAction.US_SendMessage = false;
			EntryHeaderMessageSendingAction ensAction = coll.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			AssertNotNull(ensAction);
			ensAction.US_SendMessage = true;
			ensAction.US_CertifyCargoRelease = true;
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(declaration.MergeManager);
			coll.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer(false));
			Factory.Save();
			AssertEquals("Only ensEntry has a message generated", 0, inbEntry.Messages.Count);
			AssertEquals("Only ensEntry has a message generated", 1, ensEntry.Messages.Count);
			AssertEquals("CRL Certification status is updated", CargoReleaseCertificationStatusList.Codes.CertificationSentAckPending, ensEntry.US_CRLCertStatus);
		}

		public void TestPopulateElementsAndFind()
		{
			CusEntryHeader ensEntry = Declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			CusEntryHeader inbEntry = Declaration.CustomsEntryHeaders.AddNew();
			inbEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.InBond;
			ImportMessageSendingActionCollection collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("two elements", 2, collection.Count);
			EntryHeaderMessageSendingAction ensAction = collection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.EntrySummary);
			AssertNotNull(ensAction);
			AssertEquals("ensAction is wrapping ens entry", ensEntry, ensAction.entry);
			EntryHeaderMessageSendingAction inbAction = collection.FindFirstElement<EntryHeaderMessageSendingAction>(ImportMessageStatusList.MessageType.InBondDeparture);
			AssertNotNull(inbAction);
			AssertEquals("inbAction is wrapping inb entry", inbEntry, inbAction.entry);
		}

		public void TestAllowNew()
		{
			ImportMessageSendingActionCollection collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("Users do not add a new element to this collection in the grid", false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			ImportMessageSendingActionCollection collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original);
			AssertEquals("Users cannot remove an element from this collection in the grid", false, collection.AllowRemove);
		}

		public void TestCustomsCommencedLogIsAdded()
		{
			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			StmALog[] logs = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCommenced.Code));
			AssertEquals("Pre-assertion - customs not yet commenced", 0, logs.Length);
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			ImportMessageSendingActionCollection actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			bool messagesGenerated = actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Message generated for ENS entry", true, messagesGenerated);
			logs = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCommenced.Code));
			StmALog log = logs[0];
			AssertNotNull("declaration Log", log);
			AssertEquals("Most Recent Customs Commenced Log", log.SL_EventTime.Date, ZDateTime.Now.Date);
		}

		public void TestCustomsCommencedLogIsAdded_ForSendingMessageTypeReplacement()
		{
			DeclarationTestHelper.SetupCompanySpecificFormalEntryNumber("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			StmALog[] logs = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCommenced.Code));
			AssertEquals("Pre-assertion - customs not yet commenced", 0, logs.Length);
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			ImportMessageSendingActionCollection actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Replacement);
			bool messagesGenerated = actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Message generated for ENS entry", true, messagesGenerated);
			logs = declaration.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsCommenced.Code));
			StmALog log = logs[0];
			AssertNotNull("declaration Log", log);
			AssertEquals("Most Recent Customs Commenced Log", log.SL_EventTime.Date, ZDateTime.Now.Date);
		}

		public void TestGenerateStandAlonePriorNoticeMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			declaration.US_EnableSPN = true;
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			declaration.JE_MasterBill = "MB12345";
			declaration.JE_TransportMode = "SEA";
			declaration.MessageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "00000000";
			tariff.UE_DateFrom = ZDateTime.Today.AddYears(-1);
			tariff.UE_DateTo = ZDateTime.Today.AddMonths(1);
			tariff.UE_PGACodes = "FD3";
			tariff.UE_OGACodes = "FD3";
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			var fda = invoiceLine.ACE_FDALines.AddNew();
			invoiceLine.US_FDAIndicator = OGAIndicatorList.Codes.Declared;
			fda.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			fda.US_ProcessingCode = FDAProcessingCodeList.Codes.FOO_FEE;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			var messagesGenerated = actions.SendMessagesWithoutSaving(declaration.MessageInitiator);
			AssertEquals("Messages generated", true, messagesGenerated);
			AssertEquals(1, declaration.Messages.Count);
			AssertEquals(EM_MessageSubTypeList.Codes.FDAPriorNotice, declaration.Messages[0].EM_MessageSubType);
			AssertEquals(ZString.Empty, declaration.FDAMsgStatus);
			declaration.US_EnableENS = false;
			declaration.US_EnableCRL = false;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			messagesGenerated = actions.SendMessagesWithoutSaving(declaration.MessageInitiator);
			AssertEquals("Messages generated", true, messagesGenerated);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableSPN = true;
			declaration.FDAMsgStatus = ZString.Empty;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			messagesGenerated = actions.SendMessagesWithoutSaving(declaration.MessageInitiator);
			AssertEquals("Messages generated", true, messagesGenerated);
			AssertEquals(ZString.Empty, declaration.FDAMsgStatus);
		}

		public void TestCopyPSCReasonsAndExplanation()
		{
			var reasonQuery = new ZQuery();
			reasonQuery.AddToFilter(CusCodeDataSchema.CY_Type, CusCodeDataTypeList.Codes.PSCReasonCodes);
			int existingReasonCount = Factory.GetDatabaseCount(typeof(PSCReasonCusCodeData), reasonQuery);
			var explanationQuery = new ZQuery();
			explanationQuery.AddToFilter(CusAddInfoSchema.B7_Type, CusCodeDataTypeList.Codes.PSCReasonCodes);
			int existingExplanationCount = Factory.GetDatabaseCount(typeof(PSCExplanationCusAddInfo), explanationQuery);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_PSC = true;
			var actions = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original);
			var entry1 = Declaration.ActiveEntryHeaders.AddNew();
			var entry2 = Declaration.ActiveEntryHeaders.AddNew();
			entry1.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry2.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var entryLine = entry2.MergedLines.AddNew();
			var action1 = new EntryHeaderMessageSendingAction(entry1, ImportMessageStatusList.MessageType.EntrySummary, actions);
			var action2 = new EntryHeaderMessageSendingAction(entry2, ImportMessageStatusList.MessageType.EntrySummary, actions);
			action1.US_SendMessage = true;
			action2.US_SendMessage = true;
			action1.US_PSCExplanation = "Explanations 1";
			action2.US_PSCExplanation = "Explanations 2";
			var pscCode1 = action1.PSCReasonCodes.AddNew();
			pscCode1.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.Entry;
			pscCode1.Reason1 = PSCHeaderReasonList.Codes.H01;
			var pscCode2 = action2.PSCReasonCodes.AddNew();
			pscCode2.ParentTypeIndicator = PSCReasonCodeParentTypeList.Codes.EntryLine;
			pscCode2.Reason1 = PSCHeaderReasonList.Codes.H02;
			pscCode2.LineNumber = entryLine.CL_LineNumberFormatted;
			actions.Add(action1);
			actions.Add(action2);
			actions.CopyPSCReasonsAndExplanation();
			Factory.Save();
			int reasonCount = Factory.GetDatabaseCount(typeof(PSCReasonCusCodeData), reasonQuery);
			AssertEquals("There are 2 more reasons", existingReasonCount + 2, reasonCount);
			int explanationCount = Factory.GetDatabaseCount(typeof(PSCExplanationCusAddInfo), explanationQuery);
			AssertEquals("There are 2 more explanation", existingExplanationCount + 2, explanationCount);
			action2.PSCReasonCodes.RemoveAll();
			actions.CopyPSCReasonsAndExplanation();
			Factory.Save();
			reasonCount = Factory.GetDatabaseCount(typeof(PSCReasonCusCodeData), reasonQuery);
			AssertEquals("There is 1 more reasons", existingReasonCount + 1, reasonCount);
		}

		public void TestIssue0097360()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var header = declaration.ActiveEntryHeaders.AddNew();
			header.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.Export;
			ImportMessageSendingActionCollection actionCollection = null;
			AssertNoExceptionThrown(delegate
			{
				actionCollection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.Original);
			});
			AssertNotNull(actionCollection);
			AssertEquals(0, actionCollection.Count);
			AssertEquals("invalid entry.CH_MessageType ITN", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestGenerateRequestToClosureTIB()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var ensEntry = declaration.CustomsEntryHeaders.AddNew();
			ensEntry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = ensEntry.MergedLines.AddNew().PK;
			var actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.ClosureTIB);
			bool messagesGenerated = actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("No messages generated", false, messagesGenerated);
			declaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			actions = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.ClosureTIB);
			messagesGenerated = actions.SendMessagesWithoutSaving(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Messages generated for ens entry", true, messagesGenerated);
			AssertEquals(1, ensEntry.Messages.Count);
			AssertEquals(EM_MessageSubTypeList.Codes.TemporaryImportationBondRequestToExtend, ensEntry.Messages[0].EM_MessageSubType);
		}

		public void TestACEStandAlonePriorNoticeMessageSendingActions()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ImportEntryNumber = "23648975";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableSPN = true;
			declaration.JE_MasterBill = "MB11111111";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			invoiceLine.ACE_FDALines.AddNew();
			var actionCollection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			AssertEquals(1, actionCollection.Count);
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.ENT;
			actionCollection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			AssertEquals(1, actionCollection.Count);
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			var masterBill = declaration.Bills.CreatePrimaryBill("MB");
			masterBill.CU_BillNum = "MB222222";
			var invoice1 = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			invoiceLine1.ACE_FDALines.AddNew();
			invoice.JZ_CU_RelatedHouseBill = declaration.PrimaryMasterBill.PK;
			invoice1.JZ_CU_RelatedHouseBill = masterBill.PK;
			actionCollection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			AssertEquals(2, actionCollection.Count);
			invoice1.JZ_CU_RelatedHouseBill = ZGuid.Empty;
			actionCollection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			AssertEquals(2, actionCollection.Count);
		}

		public void TestFTZStandAlonePriorNoticeMessageSendingActions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = US.Business.JobMessageTypeList.Codes.FTZ;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			declaration.US_F_AdmissionType = FTZAdmissionTypeCodeList.Codes.RegularAdmission;
			declaration.US_EnableSPN = true;
			declaration.US_F_PNMode = PriorNoticeModeCodeList.Codes.P;
			declaration.FTZAdmissionNumber = "2140000|17|00000001";
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.FTZ;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = USCTariff.FDAPriorNoticeRequiredTariff;
			invoiceLine.ACE_FDALines.AddNew();
			var actionCollection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			AssertEquals(1, actionCollection.Count);
			declaration.US_SPNIDType = StandAlonePriorNoticeIDTypeList.Codes.BLN;
			var masterBill = declaration.Bills.CreatePrimaryBill("MB");
			masterBill.CU_BillNum = "MB111111";
			actionCollection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			AssertEquals(1, actionCollection.Count);
			var masterBill1 = declaration.Bills.CreatePrimaryBill("MB");
			masterBill1.CU_BillNum = "MB222222";
			actionCollection = new ImportMessageSendingActionCollection(declaration, ImportMessageSendingMessageType.StandAlonePriorNotice);
			AssertEquals(2, actionCollection.Count);
		}

		protected override ImportMessageSendingActionCollection GetCollectionToTest() => new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original);

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var collection = new ImportMessageSendingActionCollection(Declaration, ImportMessageSendingMessageType.Original);
			CusEntryHeader entry = Declaration.ActiveEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			return new EntryHeaderMessageSendingAction(entry, ImportMessageStatusList.MessageType.EntrySummary, collection);
		}

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
	}
}
