using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Moq;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class TriggerEventLogTest : TestCaseWithFactory
	{
		public void TestEventDataModelWithIStmChangeLog()
		{
			var changeLog = new Mock<IStmChangeLog>();
			var triggerEventLog = new TriggerEventLog(changeLog.Object);
			AssertNotNull(triggerEventLog.EventDataModel);
		}

		public void TestEventDataModelWithIStmAlog()
		{
			var log = new Mock<IStmALog>();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var triggerEventLog = new TriggerEventLog(log.Object, trigger, dummy);
			AssertNotNull(triggerEventLog.EventDataModel);
		}

		public void TestEventDataModelWithIQueuedLog()
		{
			var log = new Mock<IQueuedLog>();
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var triggerEventLog = new TriggerEventLog(Factory, log.Object, trigger, dummy);
			AssertNotNull(triggerEventLog.EventDataModel);
		}
	}
}
