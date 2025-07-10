using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Testing
{
	[TestedType(typeof(NonPersistentStmALog))]
	sealed class NonPersistentStmALogTest : BaseStmALogTest
	{
		public void TestGetDummyLog()
		{
			var parent = Factory.NewWithValidTestData<CommonShipment>();
			Factory.Save();

			var queuedLog = new QueuedLogForTesting(parent, Events.SubscriptionRequestedCode, new ZDateTimeOffset(2021, 7, 28));

			var log = NonPersistentStmALog.GetDummyLog(Factory, parent, queuedLog, "test");

			AssertEquals(nameof(NonPersistentStmALog.SL_Table), JobShipmentSchema.Constants.TableName, log.SL_Table);
			AssertEquals(nameof(NonPersistentStmALog.SL_Parent), parent.PK, log.SL_Parent);
			AssertEquals(nameof(NonPersistentStmALog.SL_EventTime), new ZDateTime(2021, 7, 28), log.SL_EventTime);
			AssertEquals(nameof(NonPersistentStmALog.SL_SE_NKEvent), Events.SubscriptionRequestedCode, log.SL_SE_NKEvent);

			Factory.Save();

			Assert("NonPersistentStmALog wasn't saved", !log.IsInDatabase);
		}

		public void TestGetDummyLogWithParameters()
		{
			var parent = Factory.NewWithValidTestData<CommonShipment>();
			Factory.Save();

			var eventTime = new ZDateTime(2021, 7, 28);
			var queuedLog = new QueuedLogForTesting(parent, Events.SubscriptionRequestedCode);
			queuedLog.SJ_EventTime = eventTime;
			queuedLog.SJ_EventTimeUtc = eventTime;
			queuedLog.SJ_Reference = "|LOC=TestLOC|FAC=TestFAC|";

			var log = NonPersistentStmALog.GetDummyLog(Factory, parent, queuedLog);

			AssertEquals(nameof(NonPersistentStmALog.SL_Table), JobShipmentSchema.Constants.TableName, log.SL_Table);
			AssertEquals(nameof(NonPersistentStmALog.SL_Parent), parent.PK, log.SL_Parent);
			AssertEquals(nameof(NonPersistentStmALog.SL_EventTime), eventTime, log.SL_EventTime);
			AssertEquals(nameof(NonPersistentStmALog.SL_SE_NKEvent), Events.SubscriptionRequestedCode, log.SL_SE_NKEvent);
			AssertEquals("TestLOC", log.Parameters["LOC"]);
			AssertEquals("TestFAC", log.Parameters["FAC"]);

			Factory.Save();

			Assert("NonPersistentStmALog wasn't saved", !log.IsInDatabase);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<NonPersistentStmALog>();
		}

		#endregion
	}
}
