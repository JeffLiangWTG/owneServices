using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AutoSendEntrySummaryMessageProcessor))]
	sealed class AutoSendEntrySummaryMessageProcessorTest : USAutoSendCustomsMessageProcessorTest
	{
		protected override void PrepareDeclaration(BaseJobDeclaration declaration)
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");

			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "IAN";
			broker.GS_FullName = "Ian Test";
			broker.GS_WorkPhone = "12345678";

			var usDeclaration = (JobDeclaration)declaration;
			usDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			usDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			usDeclaration.US_EnableENS = true;
			usDeclaration.US_EntryType = EntryTypeList.Codes.TemporaryImportationBond;
			usDeclaration.JE_GS_NKCusAgent = "IAN";

			var branch = Factory.Load<GlbBranch>(Env.CurrentBranchPK);
			branch.GB_Phone = "+1 738 294 5000";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "amy xiang";
			staff.GS_WorkPhone = "+86 158 5050 3354";
			staff.GS_GB_HomeBranch = branch.PK;

			USCustomsDataRegistry.Instance.CargoReleaseFTZContact.SetValue(Guid.Empty, branch.PK.ToGuid(), Guid.Empty, staff.PK.ToGuid());
		}

		protected override Customs.Business.CusEntryHeader GetEntryHeader(BaseJobDeclaration declaration)
		{
			return ((JobDeclaration)declaration).ActiveEntryHeaders.EntrySummaryEntry;
		}

		protected override IProcessor CreateProcessor(BaseJobDeclaration declaration)
		{
			return new AutoSendEntrySummaryMessageProcessor((JobDeclaration)declaration);
		}

		protected override void AssertEntryAndMessageResultForEndToEndTest(Customs.Business.CusEntryHeader entry)
		{
			AssertEquals("Original ENS message should be generated", 1, entry.Messages.Count);
			AssertEquals(ImportMessageStatusList.Codes.AwaitingEntrySummaryOriginal, entry.CH_Status);
			var originalMessage = entry.Messages[0];
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.EntrySummary, originalMessage.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.EntrySummaryAdd, originalMessage.EM_MessageSubType);
			AssertEquals(MQEDIMessage.Status.Queued, originalMessage.EM_Status);
			AssertContains("SE13AMY XIANG                               15850503354", originalMessage.EM_MessageText);
			AssertContains("10AXJ5  00000014     B00001000   23  0XA                            Y           ", originalMessage.EM_MessageText);
		}

		protected override void SetEntryClearedStatus(Customs.Business.CusEntryHeader entry)
		{
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
		}

		protected override ZString ExpectedMessageDescription => CusEntryHeaderMessageTypeList.Descriptions.EntrySummary;

		public void TestSendReplacement()
		{
			var declaration = Factory.New<JobDeclaration>();
			PrepareDeclaration(declaration);
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = CusEntryHeaderMessageTypeList.Codes.EntrySummary;
			entry.CH_Status = ImportMessageStatusList.Codes.ClearEntrySummaryOriginal;
			Factory.Save();

			IProcessor processor = new AutoSendEntrySummaryMessageProcessor(declaration);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);
			AssertContains("message has been sent to customs for Job", notifications.AsString);

			AssertEquals("Replacement Entry Summary message should be generated", 1, entry.Messages.Count);
			var replaceMessage = entry.Messages[0];
			AssertEquals(ACEApplicationIdentifierCodeList.Codes.EntrySummary, replaceMessage.EM_MessageType);
			AssertEquals(EM_MessageSubTypeList.Codes.EntrySummaryReplace, replaceMessage.EM_MessageSubType);
			AssertEquals(MQEDIMessage.Status.Queued, replaceMessage.EM_Status);
		}

		public void TestUpdateWarehouseWithdrawalWhenSendMessage()
		{
			var whsDataHelper = new WhsDataTestHelper(Factory);
			var declaration = whsDataHelper.GetNewDeclaration(EntryTypeList.Codes.Warehouse, "B000000002", "XJ5", "ENT326", 50m);
			declaration.US_QtyInWHBeforeWithdrawal = 150m;
			declaration.US_QtyBeingWithdrawn = 0m;
			Factory.Save();

			IProcessor processor = new AutoSendEntrySummaryMessageProcessor(declaration);
			var notifications = new NotificationBuffer();
			processor.Process(notifications);
			AssertEquals(150m, declaration.US_QtyInWHBeforeWithdrawal);
			AssertEquals(50m, declaration.US_QtyBeingWithdrawn);
			AssertEquals(100m, declaration.US_QtyInWHAfterWithdrawal);
			AssertEquals(false, declaration.US_IsFinalWHS);
		}
	}
}
