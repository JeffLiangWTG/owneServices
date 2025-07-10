using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.InBond.Business.Testing
{
	sealed class StatusLogManagerTest : TestCaseWithFactory
	{
		public void TestAddALogIfNecessary()
		{
			StmALog deleteStatusEvent = moveHeader.Logs.AddNew(Events.MessageCancelHeader);
			AssertEquals("PreCondition: Not Cancelled", false, deleteStatusEvent.SL_IsCancelled);
			AssertNull(logManager.AddALogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Codes.AwaitingDepartureOriginal));
			StmALog log1 = logManager.AddALogIfNecessary(ImportMessageStatusList.Codes.NotSent, ImportMessageStatusList.Codes.AwaitingDepartureOriginal);
			AssertNull("Awaiting statuses should not be logged", log1);
			AssertEquals("Not Cancelled", false, deleteStatusEvent.SL_IsCancelled);
			AssertEquals(1, moveHeader.Logs.LogsNotInDB.Length);
			log1 = logManager.AddALogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			AssertNotNull("one log added", log1);
			AssertEquals(ImportMessageStatusList.Codes.ClearDepartureOriginal, log1.SL_Reference);
			AssertEquals(2, moveHeader.Logs.LogsNotInDB.Length);
			AssertEquals("Not Cancelled", true, deleteStatusEvent.SL_IsCancelled);
		}

		public void TestAddAClearLogIfNecessary()
		{
			AssertNull(logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Codes.ErrorDepartureOriginal));
			StmALog log1 = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Codes.ClearDeparturePartialOriginal);
			AssertNotNull("one log added", log1);
			AssertEquals(ImportMessageStatusList.Codes.ClearDeparturePartialOriginal, log1.SL_Reference);
			AssertEquals("HasAClearLog", true, logManager.HasAClearLog);
			using (log1.LockForUpdatingKeyFieldsForTesting())
			{
				log1.SL_Reference = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			}

			AssertEquals("HasAClearLog", true, logManager.HasAClearLog);
			StmALog log2 = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.ClearDepartureOriginal, ImportMessageStatusList.Codes.AwaitingDepartureAmendment);
			AssertNull("no change", log2);
			StmALog log3 = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureAmendment, ImportMessageStatusList.Codes.ClearDepartureAmendment);
			AssertNotNull("should have added clear replace", log3);
			StmALog log4 = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureAmendment, ImportMessageStatusList.Codes.AwaitingDepartureWithdraw);
			AssertNull(log4);
			StmALog log5 = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureWithdraw, ImportMessageStatusList.Codes.ClearDepartureWithdraw);
			AssertNotNull("should have added clear withdrawal", log5);
			AssertEquals("log1 should be cancelled", true, log1.SL_IsCancelled);
			AssertEquals("log2 should have been cancelled", true, log3.SL_IsCancelled);
			AssertEquals("HasAClearLog", false, logManager.HasAClearLog);
			StmALog log6 = logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			AssertNotNull("one log added", log6);
			AssertEquals("A withdrawal log should have been cancelled", true, log5.SL_IsCancelled);
		}

		public void TestAddingClearLogCancellsOthersIfNecessary()
		{
			StmALog log1 = moveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.AwaitingDepartureOriginal);
			AssertEquals(false, log1.SL_IsCancelled);
			StmALog log2 = moveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			AssertEquals("Original clear message - pre-condition", false, log2.SL_IsCancelled);
			StmALog log3 = moveHeader.Logs.AddNew(Events.MessageCancelHeader, ImportMessageStatusList.Codes.AwaitingDepartureWithdraw);
			StmALog log4 = moveHeader.Logs.AddNew(Events.MessageCancelHeader, ImportMessageStatusList.Codes.ClearDepartureWithdraw);
			logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureWithdraw, ImportMessageStatusList.Codes.ClearDepartureWithdraw);
			AssertEquals("Original departure clear should be cancelled by Delete clear message", true, log2.SL_IsCancelled);
			AssertEquals("Delete clear message - pre-condition", false, log4.SL_IsCancelled);
			StmALog log6 = moveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			logManager.AddAClearLogIfNecessary(ImportMessageStatusList.Codes.AwaitingDepartureOriginal, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			AssertEquals("Re-Send of Original departure & subsequent clearance should now cancel the Delete clear message", true, log4.SL_IsCancelled);
		}

		public void TestGetLatestLog()
		{
			moveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.AwaitingDepartureOriginal, new ZDateTimeOffset(2008, 12, 10));
			moveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.AwaitingDepartureOriginal, new ZDateTimeOffset(2008, 12, 11));
			moveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.ClearDepartureOriginal, new ZDateTimeOffset(2008, 12, 13));
			moveHeader.Logs.AddNew(Events.Manifested, new ZDateTimeOffset(2008, 12, 14));
			var latestLog = moveHeader.Logs.AddNew(Events.MessageCancelHeader, ImportMessageStatusList.Codes.ClearDeparturePartialWithdraw, new ZDateTimeOffset(2008, 12, 19));
			AssertEquals(latestLog, logManager.GetLatestLog());
		}

		public void TestCancelAll()
		{
			StmALog log1 = moveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.AwaitingDepartureOriginal);
			StmALog log2 = moveHeader.Logs.AddNew(Events.MessageCancelHeader, ImportMessageStatusList.Codes.ClearDepartureWithdraw);
			StmALog log3 = moveHeader.Logs.AddNew(Events.Manifested);
			StmALog log5 = moveHeader.Logs.AddNew(Events.MessageStatusChange, ImportMessageStatusList.Codes.ClearDepartureOriginal);
			logManager.CancelAll();
			AssertEquals(true, log1.SL_IsCancelled);
			AssertEquals(true, log2.SL_IsCancelled);
			AssertEquals(false, log3.SL_IsCancelled);
			AssertEquals(true, log5.SL_IsCancelled);
		}

		public void TestAwaitingStatusesAreNotLogged()
		{
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureOriginal;
			AssertEquals("Awaiting status should not be logged", 0, moveHeader.Logs.LogsNotInDB.Length);
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureOriginal;
			AssertEquals("Clear status should be logged", 1, moveHeader.Logs.LogsNotInDB.Length);
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.AwaitingDepartureWithdraw;
			AssertEquals("Awaiting status should not be logged", 1, moveHeader.Logs.LogsNotInDB.Length);
			moveHeader.BM_CustomsStatus = ImportMessageStatusList.Codes.ClearDepartureWithdraw;
			AssertEquals("Clear status should be logged", 2, moveHeader.Logs.LogsNotInDB.Length);
		}

		CusInBondHeader header;
		CusInBondMoveHeader moveHeader;
		StatusLogManager logManager;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusInBondHeader>();
			moveHeader = header.MovementHeaders.AddNew();
			logManager = new StatusLogManager(moveHeader.Logs);
		}
	}
}
