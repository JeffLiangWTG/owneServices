using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	#region Setting EventDateProperty Values Test Cases

	class SettingEventDateProperty_ActualDate_MilestoneAndEventSynchronicityTest : MilestoneAndEventSynchronicityBaseTest
	{
		protected override StmALog CauseMilestoneToHaveActualDateSet(DummyWithWorkflowAndEventDateProperty job)
		{
			job.Z0_AnotherDate = ZDateTime.Now;

			return GetMostRecentNonCancelledTagLog(job);
		}

		protected override void CauseMilestoneToNoLongerHaveActualDateSet(DummyWithWorkflowAndEventDateProperty job, StmALog @event)
		{
			job.Z0_AnotherDate = ZDateTime.Empty;
		}

		protected override bool IsEstimate => false;

		/// <summary>
		/// Setting 'event date property' value on a job should keep relevant milestone in sync, even if it isn't re-fired.
		/// So we call Logs.CreateRecreateOrUpdateEventLog which first cancels the event and creates a new one, causing the milestone to re-fire.
		/// </summary>
		protected override bool IsRecreatingEventUponDateChange => true;
	}

	class SettingEventDateProperty_ScheduledDate_MilestoneAndEventSynchronicityTest : MilestoneAndEventSynchronicityBaseTest
	{
		protected override StmALog CauseMilestoneToHaveActualDateSet(DummyWithWorkflowAndEventDateProperty job)
		{
			job.Z0_SmallDateTime = ZDateTime.Now;

			return GetMostRecentNonCancelledTagLog(job);
		}

		protected override void CauseMilestoneToNoLongerHaveActualDateSet(DummyWithWorkflowAndEventDateProperty job, StmALog @event)
		{
			job.Z0_SmallDateTime = ZDateTime.Empty;
		}

		protected override bool IsEstimate => true;
	}

	#endregion

	#region Manually Adding Events Test Cases

	class ManuallyAddingEvent_ActualDate_MilestoneAndEventSynchronicityTest : MilestoneAndEventSynchronicityBaseTest
	{
		protected override StmALog CauseMilestoneToHaveActualDateSet(DummyWithWorkflowAndEventDateProperty job)
		{
			return job.Logs.AddNew(Events.TagWasAddedOrRemoved);
		}

		protected override void CauseMilestoneToNoLongerHaveActualDateSet(DummyWithWorkflowAndEventDateProperty job, StmALog @event)
		{
			@event.Cancel();
		}

		protected override bool IsEstimate => false;
	}

	class ManuallyAddingEvent_ScheduledDate_MilestoneAndEventSynchronicityTest : MilestoneAndEventSynchronicityBaseTest
	{
		protected override StmALog CauseMilestoneToHaveActualDateSet(DummyWithWorkflowAndEventDateProperty job)
		{
			return job.Logs.AddNew(new EventValue(Events.TagWasAddedOrRemoved, isEstimate: true));
		}

		protected override void CauseMilestoneToNoLongerHaveActualDateSet(DummyWithWorkflowAndEventDateProperty job, StmALog @event)
		{
			@event.Cancel();
		}

		protected override bool IsEstimate => true;
	}

	#endregion

	abstract class MilestoneAndEventSynchronicityBaseTest : TestCaseWithFactory
	{
		[TestTimeZoneUNLOCO("AUBNE")]
		[TestDate(2015, 7, 14)]
		public void TestSetRemoveAndReSetMilestoneDate()
		{
			TestDateAttribute.UseUNLOCO = true;
			var job = Factory.NewWithValidTestData<DummyWithWorkflowAndEventDateProperty>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
			var triggerAction = milestone.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;

			Factory.Save();

			AssertEquals(ZDateTime.Empty, GetEventDatePropertyValue(job));

			var @event = CauseMilestoneToHaveActualDateSet(job);
			Factory.Save();

			AssertEquals(ZDateTimeOffset.Now, GetRelevantMilestoneDate(milestone));
			AssertEquals(ZDateTime.Now, GetEventDatePropertyValue(job));
			AssertEquals(IsEstimate, @event.SL_IsEstimate);

			StmALog workflowTriggerEvent = null;

			if (!IsEstimate)
			{
				workflowTriggerEvent = milestone.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).SingleOrDefault();

				AssertNotNull("Raising a matching event should fire workflow", workflowTriggerEvent);
			}

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(10);

			CauseMilestoneToNoLongerHaveActualDateSet(job, @event);
			if (GetRelevantMilestoneDate(milestone).IsValid)
			{
				milestone.P9_ActualDateForBinding = ZDateTimeOffset.Empty;
			}
			Factory.Save();

			AssertEquals("Cancelling an event should remove the date from the milestone", ZDateTimeOffset.Empty, GetRelevantMilestoneDate(milestone));
			AssertEquals(true, @event.SL_IsCancelled);
			AssertEquals(ZDateTime.Empty, GetEventDatePropertyValue(job));

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(10);

			var newEvent = CauseMilestoneToHaveActualDateSet(job);
			Factory.Save();

			AssertEquals(ZDateTimeOffset.Now, GetRelevantMilestoneDate(milestone));
			AssertEquals(ZDateTime.Now, GetEventDatePropertyValue(job));
			AssertNotEquals(@event, newEvent);
			AssertEquals(false, newEvent.SL_IsCancelled);
			AssertEquals(IsEstimate, newEvent.SL_IsEstimate);

			if (!IsEstimate)
			{
				workflowTriggerEvent = milestone.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode && l != workflowTriggerEvent).SingleOrDefault();

				AssertNotNull("Raising another event after the milestone was previously cancelled should fire the milestone again", workflowTriggerEvent);
			}
		}

		[TestTimeZoneUNLOCO("AUBNE")]
		[TestDate(2015, 7, 14)]
		public void TestUpdateMilestoneDateMultipleTimes()
		{
			TestDateAttribute.UseUNLOCO = true;

			var job = Factory.NewWithValidTestData<DummyWithWorkflowAndEventDateProperty>();
			var milestone = job.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.TagWasAddedOrRemovedCode;
			var triggerAction = milestone.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;

			Factory.Save();

			AssertEquals(ZDateTime.Empty, GetEventDatePropertyValue(job));

			var @event = CauseMilestoneToHaveActualDateSet(job);
			Factory.Save();

			AssertEquals(ZDateTimeOffset.Now, GetRelevantMilestoneDate(milestone));
			AssertEquals(ZDateTime.Now, GetEventDatePropertyValue(job));
			AssertEquals(IsEstimate, @event.SL_IsEstimate);

			StmALog workflowTriggerEvent = null;

			if (!IsEstimate)
			{
				workflowTriggerEvent = milestone.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).SingleOrDefault();

				AssertNotNull("Raising a matching event should fire workflow", workflowTriggerEvent);
			}

			TestDateAttribute.Date = TestDateAttribute.Date.AddMinutes(10);

			var newEvent = CauseMilestoneToHaveActualDateSet(job);
			Factory.Save();

			if (IsEstimate)
			{
				AssertEquals("Relevant milestone date should be kept in sync", ZDateTimeOffset.Now, GetRelevantMilestoneDate(milestone));
			}
			else
			{
				AssertEquals("Relevant milestone date should not update as subsequent events are added", ZDateTimeOffset.Now.AddMinutes(-10), GetRelevantMilestoneDate(milestone));
			}

			AssertEquals(ZDateTime.Now, GetEventDatePropertyValue(job));
			AssertNotEquals(@event, newEvent);
			AssertEquals(false, newEvent.SL_IsCancelled);
			AssertEquals(IsEstimate, newEvent.SL_IsEstimate);
			AssertEquals(@event.SL_EventTime.AddMinutes(10), newEvent.SL_EventTime);

			workflowTriggerEvent = milestone.Logs.Find(l => l.SL_SE_NKEvent == Events.WorkflowTriggerEventCode && l != workflowTriggerEvent).SingleOrDefault();

			AssertNull("Raising another event after the milestone was previously fired should not fire the milestone again", workflowTriggerEvent);
		}

		#region Implementation

		ZDateTime GetEventDatePropertyValue(DummyWithWorkflowAndEventDateProperty job)
		{
			return IsEstimate ? job.Z0_SmallDateTime : job.Z0_AnotherDate;
		}

		ZDateTimeOffset GetRelevantMilestoneDate(ProcessTask milestone)
		{
			return (IsEstimate ? milestone.P9_ScheduledDateForBinding : milestone.P9_ActualDateForBinding);
		}

		protected static StmALog GetMostRecentNonCancelledTagLog(DummyWithWorkflowAndEventDateProperty job)
		{
			return job.GetLogs().Find(l => l.SL_SE_NKEvent == Events.TagWasAddedOrRemovedCode && !l.SL_IsCancelled).OrderByDescending(l => l.SL_EventTime).SingleOrDefault();
		}

		protected abstract StmALog CauseMilestoneToHaveActualDateSet(DummyWithWorkflowAndEventDateProperty job);

		protected abstract void CauseMilestoneToNoLongerHaveActualDateSet(DummyWithWorkflowAndEventDateProperty job, StmALog @event);

		protected abstract bool IsEstimate { get; }

		protected virtual bool IsRecreatingEventUponDateChange => false;

		protected override void SetUp()
		{
			base.SetUp();

			disposables = new DisposableList(new IDisposable[]
			{
				new DisposableAction(() => DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithWorkflowAndEventDateProperty), () => DummyBusinessObject.TypeDecider.TypeForLoadOverride = null),
			});
		}

		protected override void TearDown()
		{
			base.TearDown();

			disposables.Dispose();
		}

		DisposableList disposables;

		#endregion
	}
}
