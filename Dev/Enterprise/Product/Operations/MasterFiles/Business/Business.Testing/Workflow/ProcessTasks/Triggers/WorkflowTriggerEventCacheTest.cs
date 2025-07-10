using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	sealed class WorkflowTriggerEventCacheTest : TestCaseWithFactory
	{
		public void TestHasWTELog()
		{
			// Arrange
			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();
			var wteLog = trigger.Logs.AddNew(Events.WorkflowTriggerEvent);
			var eventSource = new EventSource(wteLog);
			WorkflowTriggerEventCache.GetCache(Factory).Add(trigger, eventSource, wteLog);

			// Act
			var result = WorkflowTriggerEventCache.HasWTELog(trigger, eventSource);

			// Assert
			AssertEquals(true, result);
		}

		public void TestDeleteWTELog()
		{
			// Arrange
			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();
			var wteLog = trigger.Logs.AddNew(Events.WorkflowTriggerEvent);
			var eventSource = new EventSource(wteLog);
			WorkflowTriggerEventCache.GetCache(Factory).Add(trigger, eventSource, wteLog);

			// Act
			var results = WorkflowTriggerEventCache.DeleteWTELog(trigger, eventSource);

			// Assert
			AssertEquals(wteLog, results.deletedLog);
			AssertNull(results.nextLog);

			AssertEquals(false, WorkflowTriggerEventCache.GetCache(Factory).TryGet(trigger, eventSource, out StmALog _));
		}

		public void TestTryClearWTELog_AlreadyInDatabase()
		{
			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();
			var wteLog = trigger.Logs.AddNew(Events.WorkflowTriggerEvent);
			Dummy.Factory.Save();
			var eventSource = new EventSource(wteLog);
			WorkflowTriggerEventCache.GetCache(Factory).Add(trigger, eventSource, wteLog);

			AssertNoExceptionThrown(() => WorkflowTriggerEventCache.ClearExistingWTELogs(trigger));
		}

		public void TestTryAddWTELog()
		{
			// Arrange
			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();
			var wteLog = trigger.Logs.AddNew(Events.WorkflowTriggerEvent);
			var eventSource = new EventSource(wteLog);

			// Act
			WorkflowTriggerEventCache.TryAddWTELog(trigger, eventSource, wteLog);

			// Assert
			WorkflowTriggerEventCache.GetCache(Factory).TryGet(trigger, eventSource, out StmALog result);
			AssertEquals(wteLog, result);
		}

		public void TestTryAddWTELog_DuplicateAdd()
		{
			// Arrange
			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();
			var wteLog = trigger.Logs.AddNew(Events.WorkflowTriggerEvent);
			var eventSource = new EventSource(wteLog);

			// Act
			var first = WorkflowTriggerEventCache.TryAddWTELog(trigger, eventSource, wteLog);
			var second = WorkflowTriggerEventCache.TryAddWTELog(trigger, eventSource, wteLog);

			// Assert
			AssertEquals(true, first);
			AssertEquals(false, second);
		}

		public void TestClearExistingWTELogs()
		{
			// Arrange
			ProcessTask trigger = Dummy.WorkflowItems.Triggers.AddNew();
			var wteLog = trigger.Logs.AddNew(Events.WorkflowTriggerEvent);
			var eventSource = new EventSource(wteLog);
			WorkflowTriggerEventCache.GetCache(Factory).Add(trigger, eventSource, wteLog);

			// Act
			WorkflowTriggerEventCache.ClearExistingWTELogs(trigger);

			// Assert
			AssertEquals(false, WorkflowTriggerEventCache.GetCache(Factory).TryGet(trigger, eventSource, out StmALog _));
		}

		public void TestGetCache()
		{
			// Arrange
			// First
			BusinessObjectFactory factory1 = new BusinessObjectFactory();
			var dummy1 = factory1.New<DummyWithWorkflow>();
			ProcessTask trigger = dummy1.WorkflowItems.Triggers.AddNew();
			var wteLog = trigger.Logs.AddNew(Events.WorkflowTriggerEvent);
			var eventSource = new EventSource(wteLog);
			// Second
			BusinessObjectFactory factory2 = new BusinessObjectFactory();

			//Act
			// First
			var wteCache1 = WorkflowTriggerEventCache.GetCache(factory1);
			wteCache1.Add(trigger, eventSource, wteLog);
			// Second
			var wteCache2 = WorkflowTriggerEventCache.GetCache(factory2);

			// Assert
			AssertNotEquals(wteCache1, wteCache2);
			wteCache1.TryGet(trigger, eventSource, out StmALog result1);
			AssertEquals(wteLog, result1);
			wteCache2.TryGet(trigger, eventSource, out StmALog result2);
			AssertNull(result2);
		}

		DummyWithWorkflow Dummy
		{
			get
			{
				if (dummy == null)
				{
					dummy = Factory.New<DummyWithWorkflow>();
				}
				return dummy;
			}
		}
		DummyWithWorkflow dummy;
	}
}
