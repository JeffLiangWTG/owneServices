using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	class ProcessTaskConcurrencyTest : TemplateApplicationTestCase
	{
		#region P9_CardNote

		public void TestCardNoteConcurrency_Milestone()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var milestone = dummy.WorkflowItems.Milestones.AddNew();
			milestone.P9_Description = "Albert has nice hair";
			AssertEquals(ZString.Empty, milestone.P9_CardNote);
			Factory.Save();

			var f1 = new BusinessObjectFactory { RefreshEnabled = false };
			var m1 = f1.Load<ProcessTask>(milestone.PK);
			m1.TemplateConditions.TemplateCondition2 = "UDF";
			m1.P9_CardNote = "Dan has nice hair";

			var f2 = new BusinessObjectFactory { RefreshEnabled = false };
			var m2 = f2.Load<ProcessTask>(milestone.PK);
			m2.TemplateConditions.TemplateCondition2 = "UDF";
			m2.P9_CardNote = "Nice hair is tha best";

			AssertNotEquals(ZString.Empty, m2.P9_Condition2ValueHash);
			AssertNotEquals(ZString.Empty, m2.P9_CardNote);
			AssertNotEquals(m1.P9_CardNote, m2.P9_CardNote);
			f1.Save();
			AssertNoExceptionThrown(f2.Save);
		}

		public void TestCardNoteConcurrency_Task()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var task = dummy.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Michael has no hair";
			AssertEquals(ZString.Empty, task.P9_CardNote);
			Factory.Save();

			var f1 = new BusinessObjectFactory { RefreshEnabled = false };
			var m1 = f1.Load<ProcessTask>(task.PK);
			m1.P9_CardNote = "Ben has some hair";

			var f2 = new BusinessObjectFactory { RefreshEnabled = false };
			var m2 = f2.Load<ProcessTask>(task.PK);
			m2.P9_CardNote = "Bret has a lot of hair";

			AssertNotEquals(m1.P9_CardNote, m2.P9_CardNote);
			f1.Save();
			AssertExceptionThrown<ZSaveConcurrencyException>(f2.Save);
		}

		#endregion

		#region P9_TriggerFiredCountdown

		void TriggerCountdownConcurrencyHandling(ZShort startingTriggerCount, int instance1FireCount, int instance2FireCount, int expectedFinalTriggerCount, int expectedFinalWTECount)
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "T1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.TriggerConditions.TriggerFiredCountdown = startingTriggerCount;

			Factory.Save();

			var secondFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var anotherVersionOfDummy = secondFactory.Load<DummyWithWorkflow>(dummy.PK);
			var anotherVersionOfTrigger = anotherVersionOfDummy.WorkflowItems.Triggers[0];

			for (int i = 0; i < instance1FireCount; i++)
			{
				dummy.Logs.AddNew(Events.CustomisableEvent00);
			}

			Factory.Save();

			for (int i = 0; i < instance2FireCount; i++)
			{
				anotherVersionOfDummy.Logs.AddNew(Events.CustomisableEvent00);
			}

			AssertNoExceptionThrown(() => ZExceptionReporting.ProcessWithConcurrencyHandling(() =>
			{
				secondFactory.Save();
			}, () => { }));

			anotherVersionOfTrigger.Reload();
			AssertEquals("Expected Trigger Count", expectedFinalTriggerCount, anotherVersionOfTrigger.P9_TriggerFiredCountdown);
			AssertEquals("Expected WTE Count", expectedFinalWTECount, anotherVersionOfTrigger.Logs.Find(log => log.SL_SE_NKEvent == "WTE").Count());
		}

		public void TestTriggerCountDownIgnoreConcurrency()
		{
			CombineAssertions(() =>
			{
				TriggerCountdownConcurrencyHandling(100, 1, 1, 99, 2);
				TriggerCountdownConcurrencyHandling(100, 2, 5, 95, 7);
				TriggerCountdownConcurrencyHandling(100, 5, 2, 98, 7);
			});
		}

		public void TestTriggerCountDownStrictConcurrencyWhenCountdownIsLessThan10()
		{
			CombineAssertions(() =>
			{
				TriggerCountdownConcurrencyHandling(5, 1, 1, 3, 2);
				TriggerCountdownConcurrencyHandling(5, 2, 1, 2, 3);
				TriggerCountdownConcurrencyHandling(5, 1, 2, 2, 3);
				TriggerCountdownConcurrencyHandling(1, 1, 1, 0, 1);
				TriggerCountdownConcurrencyHandling(2, 2, 2, 0, 2);
			});
		}

		public void TestTriggerCountdownIfProcessTaskIsDeleted()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "T1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.TriggerConditions.TriggerFiredCountdown = 5;

			Factory.Save();

			var secondFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var anotherVersionOfDummy = secondFactory.Load<DummyWithWorkflow>(dummy.PK);
			var anotherVersionOfTrigger = anotherVersionOfDummy.WorkflowItems.Triggers[0];

			dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			anotherVersionOfDummy.Logs.AddNew(Events.CustomisableEvent00);
			anotherVersionOfTrigger.Delete();
			var row = ((INeedRow)anotherVersionOfTrigger).Row;
			AssertEquals(DataRowState.Deleted, row.RowState);
			AssertNoExceptionThrown(() =>
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(() =>
				{
					secondFactory.Save();
				}, () => { });
			});
		}

		public void TestMergeWithDifferentTypesSucceeds()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "T1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.TriggerConditions.TriggerFiredCountdown = 3;
			Factory.Save();

			var eventArgs = new ConcurrencyValueChangedEventArgs(new ZByte(4), 3L, trigger.P9_TriggerFiredCountdownInfo, 5.0, "");
			trigger.P9_TriggerFiredCountdownConcurrencyMergeHandler(this, eventArgs);
			AssertEquals(eventArgs.FinalValue, (short)2);

			eventArgs = new ConcurrencyValueChangedEventArgs(new ZString("1"), 2, trigger.P9_TriggerFiredCountdownInfo, 3.0M, "");
			trigger.P9_TriggerFiredCountdownConcurrencyMergeHandler(this, eventArgs);
			AssertEquals(eventArgs.FinalValue, (short)0);
		}

		public void TestMergeWithDbNullSucceeds()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "T1";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.TriggerConditions.TriggerFiredCountdown = 3;
			Factory.Save();

			var eventArgs = new ConcurrencyValueChangedEventArgs(new ZByte(4), DBNull.Value, trigger.P9_TriggerFiredCountdownInfo, DBNull.Value, "");
			trigger.P9_TriggerFiredCountdownConcurrencyMergeHandler(this, eventArgs);
			AssertEquals(eventArgs.FinalValue, (short)4);
		}

		#endregion

		#region On Template Modified

		public void TestTemplateApplication()
		{
			TemplateApplicationRaceConditionHandler.TryHookupFactory(Factory, true);
			var template = MakeTemplate();
			var trigger = MakeTrigger(template, Events.CustomisableEvent00Code);
			Factory.Save();

			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.ApplyWorkflowTemplates();
			AssertEquals(1, dummy.WorkflowItems.Triggers.Count);
			dummy.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			trigger.TemplateConditions.TemplateCondition2 = "UDF";
			trigger.TemplateConditions.TemplateCondition2Value = "\"1\"==\"1\"";
			MakeNotification(trigger);
			Factory.Save();

			AssertEquals("Making sure that nobody changed DummyWithWorkflow", 1, dummy.WorkflowItems.Triggers.Count);
			dummy.ApplyWorkflowTemplates();

			AssertEquals("No longer duplicate triggers when UDF condition is added for the first time.", 1, dummy.WorkflowItems.Triggers.Count);

			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion
	}
}
