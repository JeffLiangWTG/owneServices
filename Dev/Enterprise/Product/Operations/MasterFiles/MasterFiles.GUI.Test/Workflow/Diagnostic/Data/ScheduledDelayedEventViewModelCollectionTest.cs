using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI.Workflow;
using Enterprise.MasterFiles.Integration;
using Enterprise.TimeEngineScheduler.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Test
{
	[TestedType(typeof(ScheduledDelayedEventViewModelCollection))]
	class ScheduledDelayedEventViewModelCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ScheduledDelayedEventViewModelCollection>
	{
		[TestDate(2022, 6, 19)]
		public void TestShouldContainSchedulesRelatedToTrigger()
		{
			var bizo = Factory.New<DummyWithWorkflow>();
			var trigger = MasterFilesTestHelper.CreateTrigger(bizo, Events.CustomisableEvent00);

			var bizo2 = Factory.New<DummyWithWorkflow>();

			Factory.Save();

			AssertEquals("Precondition", "Z0", bizo.TablePrefix);

			var scheduleProvider = ObjectFactory.Get<IActionScheduleProvider>();

			// related to bizo
			var schedule1 = scheduleProvider.ScheduleAction("DLY", new ZDateTime(2022, 06, 19), bizo.PK, "Z0", "Ref1");
			TestDateAttribute.AddDays(5);
			var schedule2 = scheduleProvider.ScheduleAction("DLY", new ZDateTime(2022, 06, 20), bizo.PK, "Z0", "Ref2");
			var schedule3 = scheduleProvider.ScheduleAction("DLY", new ZDateTime(2022, 06, 18), bizo.PK, "Z0", "Ref3");
			schedule3.ExecutionStatus = Constants.TimeActionScheduleStatus.Closed;
			scheduleProvider.ChangeState(schedule3);

			// wrong TargetCodePk
			scheduleProvider.ScheduleAction("DLY", new ZDateTime(2022, 06, 19), bizo.PK, "YYY", "Ref1");

			// related to bizo2
			scheduleProvider.ScheduleAction("DLY", new ZDateTime(2022, 06, 19), bizo2.PK, "Z0", "Ref3");

			// other event types
			scheduleProvider.ScheduleAction("ARR", new ZDateTime(2022, 06, 19), bizo.PK, "Z0", "Ref1");
			scheduleProvider.ScheduleAction("DEP", new ZDateTime(2022, 06, 19), bizo.PK, "Z0", "Ref1");

			AssertCollection("Should contain all the scheduled DLY actions related to the trigger's job ordered by TAS_ExecutionDateTimeUtc",
				new IActionSchedule[] { schedule2, schedule1, schedule3 }, trigger);
		}

		[TestDate(2022, 6, 19)]
		public void TestShouldContainSchedulesRelatedToProcessJobTriggerLink()
		{
			var bizo = Factory.New<DummyWithWorkflow>();
			var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, DummyWorkflowDescriptor.Instance.Code);
			template.P0_Name = "Universal";
			var templateTrigger = (IUniversalTemplateTrigger)template.TemplateTriggers.AddNew();
			templateTrigger.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00Code;
			templateTrigger.Description = "Trig";

			Factory.Save();

			bizo.ApplyWorkflowTemplates();
			var bizo2 = Factory.New<DummyWithWorkflow>();
			Factory.Save();

			var trigger = bizo.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Single(t => t.P9_ParentTemplateID == templateTrigger.Identifier);
			AssertNotNull(trigger);
			AssertEquals("Precondition", "Z0", bizo.TablePrefix);

			var scheduleProvider = ObjectFactory.Get<IActionScheduleProvider>();

			// related to bizo
			var schedule1 = scheduleProvider.ScheduleAction("DLY", new ZDateTime(2022, 06, 19), bizo.PK, "Z0", "Ref1");
			TestDateAttribute.AddDays(5);
			var schedule2 = scheduleProvider.ScheduleAction("DLY", new ZDateTime(2022, 06, 20), bizo.PK, "Z0", "Ref2");
			var schedule3 = scheduleProvider.ScheduleAction("DLY", new ZDateTime(2022, 06, 18), bizo.PK, "Z0", "Ref3");
			schedule3.ExecutionStatus = Constants.TimeActionScheduleStatus.Closed;
			scheduleProvider.ChangeState(schedule3);

			// wrong TargetCodePk
			scheduleProvider.ScheduleAction("DLY", new ZDateTime(2022, 06, 19), bizo.PK, "YYY", "Ref1");

			// related to bizo2
			scheduleProvider.ScheduleAction("DLY", new ZDateTime(2022, 06, 19), bizo2.PK, "Z0", "Ref3");

			// other event types
			scheduleProvider.ScheduleAction("ARR", new ZDateTime(2022, 06, 19), bizo.PK, "Z0", "Ref1");
			scheduleProvider.ScheduleAction("DEP", new ZDateTime(2022, 06, 19), bizo.PK, "Z0", "Ref1");

			AssertCollection("Should contain all the scheduled DLY actions related to the trigger's job ordered by TAS_ExecutionDateTimeUtc",
				new IActionSchedule[] { schedule2, schedule1, schedule3 }, trigger);
		}

		void AssertCollection(string message, IEnumerable<IActionSchedule> expectedSchedules, ProcessTask trigger)
		{
			var collection = new ScheduledDelayedEventViewModelCollection(trigger);
			collection.Load();
			AssertContainsExactElementsInExactOrder(message,
				expectedSchedules.Select(s => Tuple.Create(
					s.ExecutionDateTimeUtc, s.JsonParameter, s.ExecutionStatus, s.ExecutionResult, s.SystemCreateTimeUtc)),
				collection.Cast<ScheduledDelayedEventViewModel>().Select(m => Tuple.Create(
					m.ExecutionDateTimeUtc, m.EventReference, m.ExecutionStatus, m.ExecutionResult, m.SystemCreateTimeUtc)));
		}

		#region NonPersistentBusinessObjectCollectionTestCase Overrides

		protected override ScheduledDelayedEventViewModelCollection GetCollectionToTest()
		{
			return new ScheduledDelayedEventViewModelCollection(null);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var helper = ObjectFactory.Get<ITimeEngineSchedulerTestHelper>();
			return new ScheduledDelayedEventViewModel(helper.ScheduleAction(new BusinessObjectFactory()));
		}

		#endregion
	}
}
