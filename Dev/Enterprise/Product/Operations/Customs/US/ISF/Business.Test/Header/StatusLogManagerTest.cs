using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	sealed class StatusLogManagerTest : TestCaseWithFactory
	{
		public void TestAddALogIfNecessary()
		{
			StmALog deleteStatusEvent = header.Logs.AddNew(Events.MessageCancelHeader);
			AssertEquals("PreCondition: Not Cancelled", false, deleteStatusEvent.SL_IsCancelled);
			AssertNull(logManager.AddALogIfNecessary(MessageStatusList.Codes.AwaitingISFAdd, MessageStatusList.Codes.AwaitingISFAdd));
			StmALog log1 = logManager.AddALogIfNecessary(MessageStatusList.Codes.NotSentISF, MessageStatusList.Codes.AwaitingISFAdd);
			AssertNotNull("one log added", log1);
			AssertEquals(MessageStatusList.Codes.AwaitingISFAdd, log1.SL_Reference);
			AssertEquals("Not Cancelled", false, deleteStatusEvent.SL_IsCancelled);
			AssertEquals(2, header.Logs.LogsNotInDB.Length);
			log1 = logManager.AddALogIfNecessary(MessageStatusList.Codes.AwaitingISFAdd, MessageStatusList.Codes.ClearISFAdd);
			AssertNotNull("one log added", log1);
			AssertEquals(MessageStatusList.Codes.ClearISFAdd, log1.SL_Reference);
			AssertEquals(3, header.Logs.LogsNotInDB.Length);
			AssertEquals("Not Cancelled", true, deleteStatusEvent.SL_IsCancelled);
		}

		public void TestAddAClearLogIfNecessary()
		{
			AssertNull(logManager.AddAClearLogIfNecessary(MessageStatusList.Codes.AwaitingISFAdd, MessageStatusList.Codes.ErrorISFAdd));
			StmALog log1 = logManager.AddAClearLogIfNecessary(MessageStatusList.Codes.AwaitingISFAdd, MessageStatusList.Codes.ClearISFAdd);
			AssertNotNull("one log added", log1);
			AssertEquals(MessageStatusList.Codes.ClearISFAdd, log1.SL_Reference);
			AssertEquals("HasAClearLog", true, logManager.HasAClearLog());
			StmALog log2 = logManager.AddAClearLogIfNecessary(MessageStatusList.Codes.ClearISFAdd, MessageStatusList.Codes.AwaitingISFReplace);
			AssertNull("no change", log2);
			StmALog log3 = logManager.AddAClearLogIfNecessary(MessageStatusList.Codes.AwaitingISFReplace, MessageStatusList.Codes.ClearISFReplace);
			AssertNotNull("should have added clear replace", log3);
			StmALog log4 = logManager.AddAClearLogIfNecessary(MessageStatusList.Codes.ClearISFReplace, MessageStatusList.Codes.AwaitingISFDelete);
			AssertNull(log4);
			StmALog log5 = logManager.AddAClearLogIfNecessary(MessageStatusList.Codes.AwaitingISFDelete, MessageStatusList.Codes.ClearISFDelete);
			AssertNotNull("should have added clear withdrawal", log5);
			AssertEquals("log1 should be cancelled", true, log1.SL_IsCancelled);
			AssertEquals("log2 should have been cancelled", true, log3.SL_IsCancelled);
			AssertEquals("HasAClearLog", false, logManager.HasAClearLog());
			StmALog log6 = logManager.AddAClearLogIfNecessary(MessageStatusList.Codes.AwaitingISFAdd, MessageStatusList.Codes.ClearISFAdd);
			AssertNotNull("one log added", log6);
			AssertEquals("A withdrawal log should have been cancelled", true, log5.SL_IsCancelled);
		}

		public void TestGetLatestLog()
		{
			header.Logs.AddNew(Events.MessageStatusChange, MessageStatusList.Codes.AwaitingISFAdd, new ZDateTimeOffset(2008, 12, 10));
			header.Logs.AddNew(Events.MessageStatusChange, MessageStatusList.Codes.AwaitingISFAdd, new ZDateTimeOffset(2008, 12, 11));
			var log3 = header.Logs.AddNew(Events.MessageCancelHeader, MessageStatusList.Codes.ClearWithWarningISFDelete, new ZDateTimeOffset(2008, 12, 19));
			header.Logs.AddNew(Events.MessageStatusChange, MessageStatusList.Codes.ClearISFAdd, new ZDateTimeOffset(2008, 12, 13));
			header.Logs.AddNew(Events.Manifested, new ZDateTimeOffset(2008, 12, 14));
			AssertEquals(log3, logManager.GetLatestLog());
		}

		public void TestCancelAll()
		{
			StmALog log1 = header.Logs.AddNew(Events.MessageStatusChange, MessageStatusList.Codes.AwaitingISFAdd);
			StmALog log2 = header.Logs.AddNew(Events.MessageCancelHeader, MessageStatusList.Codes.ClearISFDelete);
			StmALog log3 = header.Logs.AddNew(Events.Manifested);
			StmALog log4 = header.Logs.AddNew(Events.MessageCancelHeader, MessageStatusList.Codes.ClearWithWarningISFDelete);
			StmALog log5 = header.Logs.AddNew(Events.MessageStatusChange, MessageStatusList.Codes.ClearISFAdd);
			logManager.CancelAll();
			AssertEquals(true, log1.SL_IsCancelled);
			AssertEquals(true, log2.SL_IsCancelled);
			AssertEquals(false, log3.SL_IsCancelled);
			AssertEquals(true, log4.SL_IsCancelled);
			AssertEquals(true, log4.SL_IsCancelled);
		}

		CusISFHeader header;
		StatusLogManager logManager;
		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<CusISFHeader>();
			logManager = new StatusLogManager(header.Logs);
		}
	}
}
