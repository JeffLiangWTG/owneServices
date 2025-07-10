using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FormalENSValidatorTest : TestCaseWithFactory
	{
		public void TestGetEntrySummaryMessageErrors()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var ensEntry = Factory.NewMoq<CusEntryHeader>();
			ensEntry.Object.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.Object.CH_JE = declaration.PK;
			ensEntry.Object.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			declaration.CustomsEntryHeaders.Add(ensEntry.Object);
			invoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(ensEntry.Object.MergedLines.AddNew());
			declaration.AllocateEntryNumber("TEST");
			var statementHeader = Factory.New<CusStatementHeader>();
			var statementLine = statementHeader.StatementLines.AddNew();
			statementLine.B3_Status = StatementLineStatusList.Codes.Active;
			statementLine.B3_EntryFilerCode = "XJ5";
			statementLine.B3_EntryNum = "TEST";
			Factory.Save();
			var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
			var text = FormalENSValidator.GetEntrySummaryMessageErrors(entry, ImportMessageSendingMessageType.Original);
			var messageErrorText = ValidationConstants.EntrySummary.AlreadyOnStatement("entry summary");
			AssertEquals(messageErrorText, text[0]);
			declaration.US_PSC = true;
			declaration.US_EntryFilerCode = "XXX";
			ensEntry.Setup(m => m.HasBeenLodgedAtCustoms).Returns(false);
			text = FormalENSValidator.GetEntrySummaryMessageErrors(entry, ImportMessageSendingMessageType.Original);
			AssertEquals(ValidationConstants.EntrySummary.PSCFilingOfEntriesFiledByOtherBroker, text[0]);
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			text = FormalENSValidator.GetEntrySummaryMessageErrors(entry, ImportMessageSendingMessageType.Original, true);
			AssertEquals(ValidationConstants.PSC.NoReasonCodeEntered, text[0]);
			entry.CH_Status = ImportMessageStatusList.Codes.EntrySummaryCanceled;
			text = FormalENSValidator.GetEntrySummaryMessageErrors(entry, ImportMessageSendingMessageType.Original);
			AssertEquals(ValidationConstants.EntrySummary.HasBeenCancelled, text[0]);
		}

		public void TestCheckSendRLFEntrySummarySecurity()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_SchDEntry = "0901";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var ensEntry = Factory.NewMoq<CusEntryHeader>();
			ensEntry.Object.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			ensEntry.Object.CH_JE = declaration.PK;
			declaration.CustomsEntryHeaders.Add(ensEntry.Object);
			string messageSendingType = "Entry Summary";
			var text = FormalENSValidator.CheckSendRLFSecurity(declaration.ActiveEntryHeaders.EntrySummaryEntry, declaration.RegistryCompanyPK, messageSendingType);
			AssertEquals(ZString.Empty, text);
			var registryItemsCollection = new BranchDistrictPortCollection(new ZArchitecture.Environment.FallbackLevel(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty), Factory);
			var item1 = registryItemsCollection.AddNew();
			item1.PortCode = "39";
			item1.BranchPK = GlbBranch.CurrentBranch.PK;
			USCustomsDataRegistry.Instance.BranchDistrictPortRelationship.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, registryItemsCollection);
			GlbStaff.CurrentUser.GS_GB_HomeBranch = ZGuid.Empty;
			string branchIsNotConfiguredUnableToSendMsg = string.Format(ValidationConstants.Declaration.BranchIsNotConfiguredUnableToSendMsg, messageSendingType);
			string notRLFJob = string.Format(ValidationConstants.Declaration.NotRLFJob, messageSendingType);
			string rLFJob = string.Format(ValidationConstants.Declaration.RLFJob, messageSendingType);
			text = FormalENSValidator.CheckSendRLFSecurity(declaration.ActiveEntryHeaders.EntrySummaryEntry, declaration.RegistryCompanyPK, messageSendingType);
			AssertEquals("CWSupport should be able to send RLF messages without Home Branch", ZString.Empty, text);
			GlbStaff.CurrentUser.GS_LoginName = "NotSupport";
			text = FormalENSValidator.CheckSendRLFSecurity(declaration.ActiveEntryHeaders.EntrySummaryEntry, declaration.RegistryCompanyPK, messageSendingType);
			AssertEquals(branchIsNotConfiguredUnableToSendMsg, text);
			GlbStaff.CurrentUser.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			text = FormalENSValidator.CheckSendRLFSecurity(declaration.ActiveEntryHeaders.EntrySummaryEntry, declaration.RegistryCompanyPK, messageSendingType);
			AssertEquals(notRLFJob, text);
			declaration.US_SchDEntry = "3902";
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			text = FormalENSValidator.CheckSendRLFSecurity(declaration.ActiveEntryHeaders.EntrySummaryEntry, declaration.RegistryCompanyPK, messageSendingType);
			AssertEquals(rLFJob, text);
			declaration.US_SchDEntry = "3903";
			AssertEquals("no longer RLF: precondition", ZString.Empty, declaration.US_EntryMode);
			text = FormalENSValidator.CheckSendRLFSecurity(declaration.ActiveEntryHeaders.EntrySummaryEntry, declaration.RegistryCompanyPK, messageSendingType);
			AssertEquals(ZString.Empty, text);
		}

		public void TestCheckSendRLFCargoReleaseSecurity()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_SchDEntry = "0901";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var ensEntry = Factory.NewMoq<CusEntryHeader>();
			ensEntry.Object.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.ACECargoRelease;
			ensEntry.Object.CH_JE = declaration.PK;
			declaration.CustomsEntryHeaders.Add(ensEntry.Object);
			string messageSendingType = "Cargo Release";
			var text = FormalENSValidator.CheckSendRLFSecurity(declaration.ActiveEntryHeaders.SimplifiedEntry, declaration.RegistryCompanyPK, messageSendingType);
			AssertEquals(ZString.Empty, text);
			var registryItemsCollection = new BranchDistrictPortCollection(new ZArchitecture.Environment.FallbackLevel(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty), Factory);
			var item1 = registryItemsCollection.AddNew();
			item1.PortCode = "39";
			item1.BranchPK = GlbBranch.CurrentBranch.PK;
			USCustomsDataRegistry.Instance.BranchDistrictPortRelationship.SetValue(declaration.RegistryCompanyPK, Guid.Empty, Guid.Empty, registryItemsCollection);
			GlbStaff.CurrentUser.GS_GB_HomeBranch = ZGuid.Empty;
			string branchIsNotConfiguredUnableToSendMsg = string.Format(ValidationConstants.Declaration.BranchIsNotConfiguredUnableToSendMsg, messageSendingType);
			string notRLFJob = string.Format(ValidationConstants.Declaration.NotRLFJob, messageSendingType);
			string rLFJob = string.Format(ValidationConstants.Declaration.RLFJob, messageSendingType);
			text = FormalENSValidator.CheckSendRLFSecurity(declaration.ActiveEntryHeaders.SimplifiedEntry, declaration.RegistryCompanyPK, messageSendingType);
			AssertEquals("CWSupport should be able to send RLF messages without Home Branch", ZString.Empty, text);
			GlbStaff.CurrentUser.GS_LoginName = "NotSupport";
			text = FormalENSValidator.CheckSendRLFSecurity(declaration.ActiveEntryHeaders.SimplifiedEntry, declaration.RegistryCompanyPK, messageSendingType);
			AssertEquals(branchIsNotConfiguredUnableToSendMsg, text);
			GlbStaff.CurrentUser.GS_GB_HomeBranch = GlbBranch.CurrentBranch.PK;
			text = FormalENSValidator.CheckSendRLFSecurity(declaration.ActiveEntryHeaders.SimplifiedEntry, declaration.RegistryCompanyPK, messageSendingType);
			AssertEquals(notRLFJob, text);
			declaration.US_SchDEntry = "3902";
			declaration.US_EntryMode = EntryModeList.Codes.RLF;
			text = FormalENSValidator.CheckSendRLFSecurity(declaration.ActiveEntryHeaders.SimplifiedEntry, declaration.RegistryCompanyPK, messageSendingType);
			AssertEquals(rLFJob, text);
			declaration.US_SchDEntry = "3903";
			AssertEquals("no longer RLF: precondition", ZString.Empty, declaration.US_EntryMode);
			text = FormalENSValidator.CheckSendRLFSecurity(declaration.ActiveEntryHeaders.SimplifiedEntry, declaration.RegistryCompanyPK, messageSendingType);
			AssertEquals(ZString.Empty, text);
		}
	}
}
