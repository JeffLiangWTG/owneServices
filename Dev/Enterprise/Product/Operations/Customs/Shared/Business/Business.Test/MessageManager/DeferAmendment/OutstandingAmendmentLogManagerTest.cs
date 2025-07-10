using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business.Testing
{
	sealed class OutstandingAmendmentLogManagerTest : TestCaseWithFactory
	{
		public void TestLogsForNominatedEvent()
		{
			AssertEquals("Nominated Event", Events.DeclarationAmendmentQueued, testManager.AllAmendmentLogs.NominatedEvent);
			AssertEquals("Nominated Event", Events.DeclarationAmendedPermitApproved, testManager.SavedWithoutEntryChangesLogs.NominatedEvent);
		}

		public void TestHasOutstandingAmendments()
		{
			AssertEquals("Has no outstanding amendments yet", false, testManager.HasOutstandingAmendments);

			StmALog newLog = dummySupporter.Logs.AddNew(Events.DeclarationAmendmentQueued, "Test");
			AssertEquals("HasOutstanding Amendment", true, testManager.HasOutstandingAmendments);

			newLog.Cancel();
			AssertEquals("Has no outstanding amendments", 0, testManager.AllAmendmentLogs.Count);
			AssertEquals("Has no outstanding amendments", false, testManager.HasOutstandingAmendments);
		}

		public void TestHasOutstandingManualAmendments()
		{
			AssertEquals("Has no outstanding manual amendments yet", false, testManager.HasOutstandingManualAmendments);
			StmALog newLog = dummySupporter.Logs.AddNew(Events.ManualMatchDone, "Reply to CI Amendment Received");
			AssertEquals("HasOutstanding Manual Amendment", true, testManager.HasOutstandingManualAmendments);
		}

		public void TestHasOutstandingFailedAmendments()
		{
			Assert("Has no outstanding failed amendments yet", !testManager.HasOutstandingFailedAmendments);
			testManager.AddRejectedAmendmentLog();
			Assert("HasOutstanding Manual Amendment", testManager.HasOutstandingFailedAmendments);
		}

		public void TestHasConsolidatedEntryChanges()
		{
			AssertEquals("Has no Consolidated Entry Changes yet", false, testManager.HasConsolidatedEntryChanges);
			StmALog newLog = dummySupporter.Logs.AddNew(Events.ConsolidatedEntryChanged);
			AssertEquals("Has Consolidated Entry Changes", true, testManager.HasConsolidatedEntryChanges);
			newLog.Cancel();
			AssertEquals("Has no Consolidated Entry Changes", false, testManager.HasConsolidatedEntryChanges);
		}

		public void TestCancelAllOutstandingAmendments()
		{
			StmALog dapLog1 = dummySupporter.Logs.AddNew(Events.DeclarationAmendmentQueued, "Test DAP");
			AssertEquals("PreCondition:NewLog is not cancelled", false, dapLog1.SL_IsCancelled);

			StmALog dapLog2 = dummySupporter.Logs.AddNew(Events.DeclarationAmendmentQueued, "Test DAP (2)");
			AssertEquals("PreCondition:NewLog is not cancelled", false, dapLog2.SL_IsCancelled);

			StmALog cecLog = dummySupporter.Logs.AddNew(Events.ConsolidatedEntryChanged, "Test CEC");
			AssertEquals("PreCondition:NewLog is not cancelled", false, cecLog.SL_IsCancelled);

			testManager.CancelAllOutstandingAmendments();
			AssertEquals("dapLog1 is cancelled", true, dapLog1.SL_IsCancelled);
			AssertEquals("dapLog2 is cancelled", true, dapLog2.SL_IsCancelled);
			AssertEquals("cecLog is cancelled", true, cecLog.SL_IsCancelled);
			AssertEquals("AllAmendmentLogs does not have any item", 0, testManager.AllAmendmentLogs.Count);
		}

		public void TestAddANewOutstandingAmendmentLog()
		{
			StmALog result = testManager.AddANewOutstandingAmendmentLog("Test");
			AssertEquals("Event type", Events.DeclarationAmendmentQueued.Code, result.SL_SE_NKEvent);
			AssertEquals("Reference is set", "Test", result.SL_Reference);
		}

		public void TestAllOutstandingAmendmentDetailsIncludingEventTimes()
		{
			var log1 = testManager.AddANewOutstandingAmendmentLog("AAAAAAAAAA", new ZDateTime(2000, 1, 1, 1, 1, 1));

			var log2 = testManager.AddANewOutstandingAmendmentLog("BBBBBBBBBB", new ZDateTime(2000, 2, 1, 1, 1, 2));
			log2.Cancel();

			var log3 = testManager.AddANewOutstandingAmendmentLog("CCCCCCCCCC", new ZDateTime(2000, 3, 1, 1, 1, 3));

			ZString result = testManager.AllOutstandingAmendmentDetailsIncludingEventTimes;
			AssertEquals("Result should have Log1 details", true, result.Contains(log1.SL_EventTime.ToString()));
			AssertEquals("Result should have Log1 details", true, result.Contains("AAAAAAAAAA"));

			AssertEquals("Result should not have Log2 details", false, result.Contains(log2.SL_EventTime.ToString()));
			AssertEquals("Result should not have Log2 details", false, result.Contains("BBBBBBBBBB"));

			AssertEquals("Result should have Log3 details", true, result.Contains(log1.SL_EventTime.ToString()));
			AssertEquals("Result should have Log3 details", true, result.Contains("CCCCCCCCCC"));

			result = testManager.AllOustandingAmendmentReferences;
			AssertEquals("Result should have Log1 details", true, result.Contains("AAAAAAAAAA"));
			AssertEquals("Result should not have Log2 details", false, result.Contains("BBBBBBBBBB"));
			AssertEquals("Result should have Log3 details", true, result.Contains("CCCCCCCCCC"));
		}

		BaseJobDeclaration dummySupporter;
		OutstandingAmendmentLogManager testManager;
		protected override void SetUp()
		{
			base.SetUp();
			dummySupporter = Factory.New<BaseJobDeclaration>();
			testManager = new OutstandingAmendmentLogManager(dummySupporter);
		}
	}
}
